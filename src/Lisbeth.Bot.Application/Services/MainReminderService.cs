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
using Lisbeth.Bot.Application.Exceptions;
using Lisbeth.Bot.Application.Extensions;
using Lisbeth.Bot.Application.Helpers;
using Lisbeth.Bot.Application.Services.Database.Interfaces;
using Lisbeth.Bot.Application.Services.Interfaces;
using Lisbeth.Bot.DataAccessLayer.Specifications.RecurringReminder;
using Lisbeth.Bot.DataAccessLayer.Specifications.Reminder;
using Lisbeth.Bot.Domain.DTOs.Request.Reminder;
using Lisbeth.Bot.Domain.Entities;
using Lisbeth.Bot.Domain.Enums;
using NCrontab;
using System;
using System.Linq;
using System.Threading.Tasks;
using Lisbeth.Bot.Domain.DTOs.Response;

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

        public async Task<ReminderResDto> SetNewReminderAsync([NotNull] SetReminderReqDto req)
        {
            if (req is null) throw new ArgumentNullException(nameof(req));

            long remindersPerUser =
                await _reminderService.LongCountAsync(new ActiveRemindersPerUserSpec(req.RequestedOnBehalfOfId)) +
                await _recurringReminderService.LongCountAsync(
                    new ActiveRecurringRemindersPerUserSpec(req.RequestedOnBehalfOfId));

            long recurringRemindersPerGuild = await _recurringReminderService.LongCountAsync(
                new ActiveRecurringRemindersPerGuildSpec(req.RequestedOnBehalfOfId));
            long remindersPerGuild =
                await _reminderService.LongCountAsync(new ActiveRemindersPerGuildSpec(req.RequestedOnBehalfOfId)) +
                recurringRemindersPerGuild;

            if (recurringRemindersPerGuild >= 20) throw new ArgumentException("A guild can have up to 20 active recurring reminders");
            if (remindersPerUser >= 10) throw new ArgumentException("A user can have up to 10 active reminders");
            if (remindersPerGuild >= 200) throw new ArgumentException("A guild can have up to 200 active reminders");

            if (req.SetFor.HasValue || !string.IsNullOrWhiteSpace(req.TimeSpanExpression)) // handle single reminder as first option
            {
                DateTime setFor;
                if (!string.IsNullOrWhiteSpace(req.TimeSpanExpression))
                {
                    var isValid = req.TimeSpanExpression.TryParseToDurationAndNextOccurrence(out var occurrence, out _);
                    if (!isValid) throw new ArgumentException(nameof(req.TimeSpanExpression));
                    setFor = occurrence;
                }
                else
                {
                    setFor = req.SetFor.Value;
                }

                if (string.IsNullOrWhiteSpace(req.Name)) req.Name = $"{req.GuildId}_{req.RequestedOnBehalfOfId}_{DateTime.UtcNow}";

                long id = await _reminderService.AddAsync(req);
                string hangfireId =
                    _backgroundJobClient.Schedule<IDiscordSendReminderService>(
                        x => x.SendReminderAsync(id, ReminderType.Single), setFor.ToUniversalTime(), "reminder");
                await _reminderService.SetHangfireIdAsync(id, hangfireId, true);

                return new ReminderResDto(id, req.Name, setFor, req.Mentions);
            }
            else // handle recurring reminder, validated req must either fall to first if or have a cron expression
            {
                var parsed = CrontabSchedule.TryParse(req.TimeSpanExpression);
                if (parsed is null) throw new ArgumentException("Invalid cron expression", nameof(req.CronExpression));
                if (parsed.GetNextOccurrences(DateTime.UtcNow, DateTime.UtcNow.AddHours(1)).Count() > 12)
                    throw new ArgumentException(
                        "Cron expressions with more than 12 occurences per hour (more frequent than every 5 minutes) are not allowed");

                if (await _recurringReminderService.LongCountAsync(
                    new ActiveRecurringRemindersPerGuildByNameSpec(req.GuildId, req.Name)) != 0)
                    throw new ArgumentException($"This guild already has a recurring reminder with name: {req.Name}",
                        nameof(req.Name));
                
                string jobName = $"{req.GuildId}_{req.Name}";

                long id = await _recurringReminderService.AddAsync(req);
                RecurringJob.AddOrUpdate<IDiscordSendReminderService>(jobName, x => x.SendReminderAsync(id, ReminderType.Recurring),
                    req.CronExpression, TimeZoneInfo.Utc, "reminder");
                await _recurringReminderService.SetHangfireIdAsync(id, jobName, true);

                return new ReminderResDto(id, req.Name, parsed.GetNextOccurrence(DateTime.UtcNow), req.Mentions);
            }
        }

        public async Task<ReminderResDto> RescheduleReminderAsync([NotNull] RescheduleReminderReqDto req)
        {
            if (req is null) throw new ArgumentNullException(nameof(req));

            if (req.SetFor.HasValue || !string.IsNullOrWhiteSpace(req.TimeSpanExpression)) // handle single reminder
            {
                var reminder = await _reminderService.GetSingleBySpecAsync<Reminder>(
                    new ActiveReminderByNameOrIdAndGuildSpec(req.Name, req.GuildId, req.ReminderId));
                if (reminder is null) throw new NotFoundException();

                DateTime setFor;
                if (!string.IsNullOrWhiteSpace(req.TimeSpanExpression))
                {
                    var isValid = req.TimeSpanExpression.TryParseToDurationAndNextOccurrence(out var occurrence, out _);
                    if (!isValid) throw new ArgumentException(nameof(req.TimeSpanExpression));
                    setFor = occurrence;
                }
                else
                {
                    setFor = req.SetFor.Value;
                }

                bool res = BackgroundJob.Delete(reminder.HangfireId.ToString());

                if (!res) throw new Exception("Hangfire failed to delete a scheduled job");

                string hangfireId =
                    _backgroundJobClient.Schedule<IDiscordSendReminderService>(
                        x => x.SendReminderAsync(reminder.Id, ReminderType.Single), setFor.ToUniversalTime(), "reminder");

                req.NewHangfireId = long.Parse(hangfireId);
                await _reminderService.RescheduleAsync(req, true);

                return new ReminderResDto(reminder.Id, reminder.Name, setFor, reminder.Mentions);
            }
            else // handle recurring reminder, validated req must either fall to first if or have a cron expression
            {
                var parsed = CrontabSchedule.TryParse(req.TimeSpanExpression);
                if (parsed is null) throw new ArgumentException("Invalid cron expression", nameof(req.CronExpression));
                if (parsed.GetNextOccurrences(DateTime.UtcNow, DateTime.UtcNow.AddHours(1)).Count() > 12)
                    throw new ArgumentException(
                        "Cron expressions with more than 12 occurences per hour (more frequent than every 5 minutes) are not allowed");

                var reminder = await _recurringReminderService.GetSingleBySpecAsync<RecurringReminder>(
                    new ActiveRecurringReminderByNameOrIdAndGuildSpec(req.Name, req.GuildId, req.ReminderId));
                if (reminder is null) throw new NotFoundException();

                string jobName = $"{reminder.GuildId}_{reminder.Name}";
                
                RecurringJob.AddOrUpdate<IDiscordSendReminderService>(jobName, x => x.SendReminderAsync(reminder.Id, ReminderType.Recurring),
                    req.CronExpression, TimeZoneInfo.Utc, "reminder");

                await _recurringReminderService.RescheduleAsync(req, true);

                return new ReminderResDto(reminder.Id, reminder.Name, parsed.GetNextOccurrence(DateTime.UtcNow), reminder.Mentions);
            }
        }

        public async Task<ReminderResDto> DisableReminderAsync([NotNull] DisableReminderReqDto req)
        {
            if (req is null) throw new ArgumentNullException(nameof(req));

            switch (req.Type)
            {
                case ReminderType.Single:
                    var reminder = await _reminderService.GetSingleBySpecAsync<Reminder>(
                        new ActiveReminderByNameOrIdAndGuildSpec(req.Name, req.GuildId, req.ReminderId));
                    if (reminder is null) throw new NotFoundException();

                    bool res = BackgroundJob.Delete(reminder.HangfireId.ToString());

                    if (!res) throw new Exception("Hangfire failed to delete a scheduled job");

                    await _reminderService.DisableAsync(req, true);

                    return new ReminderResDto(reminder.Id, reminder.Name, DateTime.MinValue, reminder.Mentions);
                case ReminderType.Recurring:
                    var recurringReminder = await _recurringReminderService.GetSingleBySpecAsync<RecurringReminder>(
                        new ActiveRecurringReminderByNameOrIdAndGuildSpec(req.Name, req.GuildId, req.ReminderId));
                    if (recurringReminder is null) throw new NotFoundException();

                    RecurringJob.RemoveIfExists(recurringReminder.HangfireId.ToString());

                    await _recurringReminderService.DisableAsync(req, true);

                    return new ReminderResDto(recurringReminder.Id, recurringReminder.Name, DateTime.MinValue, recurringReminder.Mentions);
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}
