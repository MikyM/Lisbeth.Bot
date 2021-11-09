// This file is part of Lisbeth.Bot project
//
// Copyright (C) 2021 Krzysztof Kupisz - MikyM
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU Affero General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Affero General Public License for more details.
//
// You should have received a copy of the GNU Affero General Public License
// along with this program.  If not, see <https://www.gnu.org/licenses/>.

using Hangfire;
using JetBrains.Annotations;
using Lisbeth.Bot.Application.Discord.Services.Interfaces;
using Lisbeth.Bot.Application.Extensions;
using Lisbeth.Bot.Application.Helpers;
using Lisbeth.Bot.Application.Results;
using Lisbeth.Bot.Application.Services.Database.Interfaces;
using Lisbeth.Bot.Application.Services.Interfaces;
using Lisbeth.Bot.DataAccessLayer.Specifications.RecurringReminder;
using Lisbeth.Bot.DataAccessLayer.Specifications.Reminder;
using Lisbeth.Bot.Domain.DTOs.Request.Reminder;
using Lisbeth.Bot.Domain.DTOs.Response;
using Lisbeth.Bot.Domain.Entities;
using Lisbeth.Bot.Domain.Enums;
using MikyM.Common.Application.Results;
using NCrontab;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Lisbeth.Bot.Application.Services
{
    [UsedImplicitly]
    public class MainReminderService : IMainReminderService
    {
        private readonly IReminderService _reminderService;
        private readonly IRecurringReminderService _recurringReminderService;
        private readonly IBackgroundJobClient _backgroundJobClient;

        public MainReminderService(IReminderService reminderService, IRecurringReminderService recurringReminderService,
            IBackgroundJobClient backgroundJobClient)
        {
            _reminderService = reminderService;
            _recurringReminderService = recurringReminderService;
            _backgroundJobClient = backgroundJobClient;
        }

        public async Task<Result<ReminderResDto>> SetNewReminderAsync(SetReminderReqDto req)
        {
            if (req is null) throw new ArgumentNullException(nameof(req));

            var reUser = await _reminderService.LongCountAsync(new ActiveRemindersPerUserSpec(req.RequestedOnBehalfOfId));
            var recUser = await _recurringReminderService.LongCountAsync(
                new ActiveRecurringRemindersPerUserSpec(req.RequestedOnBehalfOfId));
            long remindersPerUser = reUser.Entity + recUser.Entity;

            var reGuild = await _recurringReminderService.LongCountAsync(
                new ActiveRecurringRemindersPerGuildSpec(req.RequestedOnBehalfOfId));
            var recGuild =
                await _reminderService.LongCountAsync(new ActiveRemindersPerGuildSpec(req.RequestedOnBehalfOfId));
            long remindersPerGuild = recGuild.Entity + reGuild.Entity;

            if (recGuild.Entity >= 20) return new ArgumentError(nameof(recGuild.Entity),"A guild can have up to 20 active recurring reminders");
            if (remindersPerUser >= 10) return new ArgumentError(nameof(recGuild.Entity), "A user can have up to 10 active reminders");
            if (remindersPerGuild >= 200) return new ArgumentError(nameof(recGuild.Entity), "A guild can have up to 200 active reminders");

            if (req.SetFor.HasValue || !string.IsNullOrWhiteSpace(req.TimeSpanExpression)) // handle single reminder as first option
            {
                DateTime setFor;
                if (!string.IsNullOrWhiteSpace(req.TimeSpanExpression))
                {
                    var isValid = req.TimeSpanExpression.TryParseToDurationAndNextOccurrence(out var occurrence, out _);
                    if (!isValid) return new ArgumentError(nameof(req.TimeSpanExpression));
                    setFor = occurrence;
                }
                else
                {
                    setFor = req.SetFor ?? throw new InvalidOperationException();
                }

                if (string.IsNullOrWhiteSpace(req.Name)) req.Name = $"{req.GuildId}_{req.RequestedOnBehalfOfId}_{DateTime.UtcNow}";

                var partial = await _reminderService.AddAsync(req);
                string hangfireId =
                    _backgroundJobClient.Schedule<IDiscordSendReminderService>(
                        x => x.SendReminderAsync(partial.Entity, ReminderType.Single), setFor.ToUniversalTime(), "reminder");
                await _reminderService.SetHangfireIdAsync(partial.Entity, hangfireId, true);

                return new ReminderResDto(partial.Entity, req.Name, setFor, req.Mentions);
            }
            else // handle recurring reminder, validated req must either fall to first if or have a cron expression
            {
                var parsed = CrontabSchedule.TryParse(req.TimeSpanExpression);
                if (parsed is null) return Result<ReminderResDto>.FromError(new ArgumentError(nameof(recGuild.Entity), "Invalid cron expression"));
                if (parsed.GetNextOccurrences(DateTime.UtcNow, DateTime.UtcNow.AddHours(1)).Count() > 12)
                    return new ArgumentError(nameof(recGuild.Entity), "Cron expressions with more than 12 occurences per hour (more frequent than every 5 minutes) are not allowed");

                var count = await _recurringReminderService.LongCountAsync(
                    new ActiveRecurringRemindersPerGuildByNameSpec(req.GuildId, req.Name));
                if (count.Entity != 0)
                    return new ArgumentError(nameof(recGuild.Entity), $"This guild already has a recurring reminder with name: {req.Name}");

                string jobName = $"{req.GuildId}_{req.Name}";

                var partial = await _recurringReminderService.AddAsync(req);
                RecurringJob.AddOrUpdate<IDiscordSendReminderService>(jobName, x => x.SendReminderAsync(partial.Entity, ReminderType.Recurring),
                    req.CronExpression, TimeZoneInfo.Utc, "reminder");
                await _recurringReminderService.SetHangfireIdAsync(partial.Entity, jobName, true);

                return new ReminderResDto(partial.Entity, req.Name, parsed.GetNextOccurrence(DateTime.UtcNow), req.Mentions);
            }
        }

        public async Task<Result<ReminderResDto>> RescheduleReminderAsync(RescheduleReminderReqDto req)
        {
            if (req is null) throw new ArgumentNullException(nameof(req));

            if (req.SetFor.HasValue || !string.IsNullOrWhiteSpace(req.TimeSpanExpression)) // handle single reminder
            {
                var result = await _reminderService.GetSingleBySpecAsync<Reminder>(
                    new ActiveReminderByNameOrIdAndGuildSpec(req.Name, req.GuildId, req.ReminderId));
                if (!result.IsSuccess) return Result<ReminderResDto>.FromError(result);

                DateTime setFor;
                if (!string.IsNullOrWhiteSpace(req.TimeSpanExpression))
                {
                    var isValid = req.TimeSpanExpression.TryParseToDurationAndNextOccurrence(out var occurrence, out _);
                    if (!isValid) throw new ArgumentException(nameof(req.TimeSpanExpression));
                    setFor = occurrence;
                }
                else
                {
                    setFor = req.SetFor ?? throw new InvalidOperationException();
                }

                bool res = BackgroundJob.Delete(result.Entity.HangfireId.ToString());

                if (!res) return new HangfireError("Hangfire failed to delete the job");

                string hangfireId =
                    _backgroundJobClient.Schedule<IDiscordSendReminderService>(
                        x => x.SendReminderAsync(result.Entity.Id, ReminderType.Single), setFor.ToUniversalTime(), "reminder");

                req.NewHangfireId = long.Parse(hangfireId);
                await _reminderService.RescheduleAsync(req, true);

                return new ReminderResDto(result.Entity.Id, result.Entity.Name, setFor, result.Entity.Mentions);
            }
            else // handle recurring reminder, validated req must either fall to first if or have a cron expression
            {
                var parsed = CrontabSchedule.TryParse(req.TimeSpanExpression);
                if (parsed is null) return new ArgumentError(nameof(req.CronExpression), "Invalid cron expression");
                if (parsed.GetNextOccurrences(DateTime.UtcNow, DateTime.UtcNow.AddHours(1)).Count() > 12)
                    return new ArgumentError(nameof(req.CronExpression),
                        "Cron expressions with more than 12 occurrences per hour (more frequent than every 5 minutes) are not allowed");

                var partial = await _recurringReminderService.GetSingleBySpecAsync<RecurringReminder>(
                    new ActiveRecurringReminderByNameOrIdAndGuildSpec(req.Name, req.GuildId, req.ReminderId));
                if (!partial.IsSuccess) return Result<ReminderResDto>.FromError(partial);

                string jobName = $"{partial.Entity.GuildId}_{partial.Entity.Name}";

                RecurringJob.AddOrUpdate<IDiscordSendReminderService>(jobName, x => x.SendReminderAsync(partial.Entity.Id, ReminderType.Recurring),
                    req.CronExpression, TimeZoneInfo.Utc, "reminder");

                await _recurringReminderService.RescheduleAsync(req, true);

                return new ReminderResDto(partial.Entity.Id, partial.Entity.Name, parsed.GetNextOccurrence(DateTime.UtcNow), partial.Entity.Mentions);
            }
        }

        public async Task<Result<ReminderResDto>> DisableReminderAsync(DisableReminderReqDto req)
        {
            if (req is null) throw new ArgumentNullException(nameof(req));

            switch (req.Type)
            {
                case ReminderType.Single:
                    var singleResult = await _reminderService.GetSingleBySpecAsync<Reminder>(
                        new ActiveReminderByNameOrIdAndGuildSpec(req.Name, req.GuildId, req.ReminderId));
                    if (!singleResult.IsSuccess) return Result<ReminderResDto>.FromError(singleResult);

                    bool res = BackgroundJob.Delete(singleResult.Entity.HangfireId.ToString());

                    if (!res) throw new Exception("Hangfire failed to delete a scheduled job");

                    await _reminderService.DisableAsync(req, true);

                    return new ReminderResDto(singleResult.Entity.Id, singleResult.Entity.Name, DateTime.MinValue, singleResult.Entity.Mentions);
                case ReminderType.Recurring:
                    var recurringResult = await _recurringReminderService.GetSingleBySpecAsync<RecurringReminder>(
                        new ActiveRecurringReminderByNameOrIdAndGuildSpec(req.Name, req.GuildId, req.ReminderId));
                    if (!recurringResult.IsSuccess) return Result<ReminderResDto>.FromError(recurringResult);

                    RecurringJob.RemoveIfExists(recurringResult.Entity.HangfireId.ToString());

                    await _recurringReminderService.DisableAsync(req, true);

                    return new ReminderResDto(recurringResult.Entity.Id, recurringResult.Entity.Name, DateTime.MinValue, recurringResult.Entity.Mentions);
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}
