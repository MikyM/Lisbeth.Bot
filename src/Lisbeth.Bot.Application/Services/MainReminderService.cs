// This file is part of Lisbeth.Bot project
//
// Copyright (C) 2021-2022 Krzysztof Kupisz - MikyM
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
using Lisbeth.Bot.DataAccessLayer.Specifications.RecurringReminder;
using Lisbeth.Bot.DataAccessLayer.Specifications.Reminder;
using Lisbeth.Bot.Domain.DTOs.Request.Reminder;
using NCrontab;

namespace Lisbeth.Bot.Application.Services;

[UsedImplicitly]
[ServiceImplementation<IMainReminderService>(ServiceLifetime.InstancePerLifetimeScope)]
public class MainReminderService : IMainReminderService
{
    private readonly IBackgroundJobClient _backgroundJobClient;
    private readonly IReminderDataService _reminderDataDataService;

    public MainReminderService(IReminderDataService reminderDataDataService, IBackgroundJobClient backgroundJobClient)
    {
        _reminderDataDataService = reminderDataDataService;
        _backgroundJobClient = backgroundJobClient;
    }

    public async Task<Result<ReminderResDto>> SetNewReminderAsync(SetReminderReqDto req)
    {
        if (req is null) throw new ArgumentNullException(nameof(req));

        var reUser =
            await _reminderDataDataService.LongCountAsync(new ActiveRemindersPerUserSpec(req.RequestedOnBehalfOfId));
        var recUser = await _reminderDataDataService.LongCountAsync(
            new ActiveRecurringRemindersPerUserSpec(req.RequestedOnBehalfOfId));
        var remindersPerUser = reUser.Entity + recUser.Entity;

        var reGuild = await _reminderDataDataService.LongCountAsync(
            new ActiveRecurringRemindersPerGuildSpec(req.RequestedOnBehalfOfId));
        var recGuild =
            await _reminderDataDataService.LongCountAsync(new ActiveRemindersPerGuildSpec(req.RequestedOnBehalfOfId));
        var remindersPerGuild = recGuild.Entity + reGuild.Entity;

        if (recGuild.Entity >= 20)
            return new DiscordArgumentInvalidError(nameof(recGuild.Entity),
                "A guild can have up to 20 active recurring reminders");
        if (remindersPerUser >= 10)
            return new DiscordArgumentInvalidError(nameof(recGuild.Entity), "A user can have up to 10 active reminders");
        if (remindersPerGuild >= 200)
            return new DiscordArgumentInvalidError(nameof(recGuild.Entity), "A guild can have up to 200 active reminders");

        if (req.SetFor.HasValue ||
            !string.IsNullOrWhiteSpace(req.TimeSpanExpression)) // handle single reminder as first option
        {
            DateTime setFor;
            if (!string.IsNullOrWhiteSpace(req.TimeSpanExpression))
            {
                if (!req.TimeSpanExpression.TryParseToDurationAndNextOccurrence(out var occurrence, out _)) 
                    return new ArgumentInvalidError(nameof(req.TimeSpanExpression), "Timespan expression couldn't be parsed.");
                setFor = occurrence.Value;
                req.SetFor = occurrence;
            }
            else
            {
                setFor = req.SetFor ?? throw new InvalidOperationException();
            }

            req.CronExpression = null; // clean cron expression just in case, single takes precedence

            if (string.IsNullOrWhiteSpace(req.Name))
                req.Name = $"{req.GuildId}_{req.RequestedOnBehalfOfId}_{Guid.NewGuid().ToString()}";

            var partial = await _reminderDataDataService.AddAsync(req, true);
            var hangfireId = _backgroundJobClient.Schedule<IDiscordSendReminderService>(
                x => x.SendReminderAsync(partial.Entity, ReminderType.Single), setFor.ToUniversalTime(),
                "reminder");
            await _reminderDataDataService.SetHangfireIdAsync(partial.Entity, hangfireId, true);

            return new ReminderResDto(partial.Entity, req.Name, setFor, req.Mentions, req.Text ?? "Text not set");
        }

        if (string.IsNullOrWhiteSpace(req.CronExpression)) 
            throw new InvalidOperationException();

        var parsed = CrontabSchedule.TryParse(req.CronExpression);
        var parsedWithSeconds = CrontabSchedule.TryParse(req.CronExpression,
            new CrontabSchedule.ParseOptions { IncludingSeconds = true });
        if (parsed is null && parsedWithSeconds is null)
            return Result<ReminderResDto>.FromError(new DiscordArgumentInvalidError(nameof(recGuild.Entity),
                "Invalid cron expression"));
        if (parsed is not null &&
            parsed.GetNextOccurrences(DateTime.UtcNow, DateTime.UtcNow.AddHours(1)).Count() > 12 ||
            parsedWithSeconds is not null && parsedWithSeconds
                .GetNextOccurrences(DateTime.UtcNow, DateTime.UtcNow.AddHours(1))
                .Count() > 12)
            return new DiscordArgumentInvalidError(nameof(recGuild.Entity),
                "Cron expressions with more than 12 occurrences per hour (more frequent than every 5 minutes) are not allowed");

        var count = await _reminderDataDataService.LongCountAsync(
            new ActiveRecurringRemindersPerGuildByNameSpec(req.GuildId, req.Name ?? string.Empty));
        if (count.Entity != 0)
            return new DiscordArgumentInvalidError(nameof(recGuild.Entity),
                $"This guild already has a recurring reminder with name: {req.Name}");

        var jobName = $"{req.GuildId}_{req.Name}";

        req.SetFor = null;
        req.TimeSpanExpression = null; // clean these just in case

        var partialRec = await _reminderDataDataService.AddAsync(req, true);
        RecurringJob.AddOrUpdate<IDiscordSendReminderService>(jobName,
            x => x.SendReminderAsync(partialRec.Entity, ReminderType.Recurring), req.CronExpression,
            TimeZoneInfo.Utc, "reminder");
        await _reminderDataDataService.SetHangfireIdAsync(partialRec.Entity, jobName, true);

        return new ReminderResDto(partialRec.Entity, req.Name,
            parsed?.GetNextOccurrence(DateTime.UtcNow).ToUniversalTime() ??
            parsedWithSeconds?.GetNextOccurrence(DateTime.UtcNow).ToUniversalTime() ??
            throw new InvalidOperationException(), req.Mentions, req.Text ?? "Text not set", true);
    }

    public async Task<Result<ReminderResDto>> RescheduleReminderAsync(RescheduleReminderReqDto req)
    {
        if (req is null) throw new ArgumentNullException(nameof(req));

        if (req.SetFor.HasValue || !string.IsNullOrWhiteSpace(req.TimeSpanExpression)) // handle single reminder
        {
            var result = await _reminderDataDataService.GetSingleBySpecAsync(
                new ActiveReminderByNameOrIdAndGuildSpec(req.Name ?? string.Empty, req.GuildId, req.ReminderId));
            if (!result.IsDefined(out var reminder)) return Result<ReminderResDto>.FromError(result);

            if (req.RequestedOnBehalfOfId != reminder.CreatorId)
                return new DiscordNotAuthorizedError("Only the creator of a reminder can make changes to it.");

            DateTime setFor;
            if (!string.IsNullOrWhiteSpace(req.TimeSpanExpression))
            {
                if (!req.TimeSpanExpression.TryParseToDurationAndNextOccurrence(out var occurrence, out _)) 
                    return new ArgumentInvalidError(nameof(req.TimeSpanExpression), "Timespan expression couldn't be parsed.");
                setFor = occurrence.Value;
                req.SetFor = occurrence;
            }
            else
            {
                setFor = req.SetFor ?? throw new InvalidOperationException();
            }

            var res = BackgroundJob.Delete(reminder.HangfireId);

            if (!res) return new HangfireError("Hangfire failed to delete the job");

            var hangfireId = _backgroundJobClient.Schedule<IDiscordSendReminderService>(
                 x => x.SendReminderAsync(reminder.Id, ReminderType.Single), setFor.ToUniversalTime(),
                "reminder");

            req.NewHangfireId = long.Parse(hangfireId);
            await _reminderDataDataService.RescheduleAsync(req, true);

            return new ReminderResDto(reminder.Id, reminder.Name, setFor, reminder.Mentions, reminder.Text ?? "Text not set");
        }

        if (string.IsNullOrWhiteSpace(req.CronExpression)) throw new InvalidOperationException();

        var parsed = CrontabSchedule.TryParse(req.TimeSpanExpression);
        if (parsed is null) return new DiscordArgumentInvalidError(nameof(req.CronExpression), "Invalid cron expression");
        if (parsed.GetNextOccurrences(DateTime.UtcNow, DateTime.UtcNow.AddHours(1)).Count() > 12)
            return new DiscordArgumentInvalidError(nameof(req.CronExpression),
                "Cron expressions with more than 12 occurrences per hour (more frequent than every 5 minutes) are not allowed");

        var partial = await _reminderDataDataService.GetSingleBySpecAsync(
            new ActiveRecurringReminderByNameOrIdAndGuildSpec(req.Name ?? string.Empty, req.GuildId, req.ReminderId));
        if (!partial.IsDefined(out var recurringReminder)) return Result<ReminderResDto>.FromError(partial);

        if (req.RequestedOnBehalfOfId != recurringReminder.CreatorId)
            return new DiscordNotAuthorizedError("Only the creator of a reminder can make changes to it.");

        var jobName = $"{recurringReminder.GuildId}_{recurringReminder.Name}";

        RecurringJob.AddOrUpdate<IDiscordSendReminderService>(jobName,
            x => x.SendReminderAsync(recurringReminder.Id, ReminderType.Recurring), req.CronExpression,
            TimeZoneInfo.Utc, "reminder");

        await _reminderDataDataService.RescheduleAsync(req, true);

        return new ReminderResDto(recurringReminder.Id, recurringReminder.Name, parsed.GetNextOccurrence(DateTime.UtcNow),
            recurringReminder.Mentions, recurringReminder.Text ?? "Text not set", true);
    }

    public async Task<Result<ReminderResDto>> DisableReminderAsync(DisableReminderReqDto req)
    {
        if (req is null) throw new ArgumentNullException(nameof(req));

        switch (req.Type)
        {
            case ReminderType.Single:
                var singleResult = await _reminderDataDataService.GetSingleBySpecAsync(
                    new ActiveReminderByNameOrIdAndGuildSpec(req.Name ?? string.Empty, req.GuildId, req.ReminderId));
                if (!singleResult.IsDefined(out var reminder)) return Result<ReminderResDto>.FromError(singleResult);

                if (req.RequestedOnBehalfOfId != reminder.CreatorId)
                    return new DiscordNotAuthorizedError("Only the creator of a reminder can make changes to it.");

                var res = BackgroundJob.Delete(reminder.HangfireId);

                if (!res) throw new Exception("Hangfire failed to delete a scheduled job");

                await _reminderDataDataService.DisableAsync(reminder, true);

                return new ReminderResDto(reminder.Id, (string?)reminder.Name, DateTime.MinValue,
                    reminder.Mentions, reminder.Text ?? "Text not set");
            case ReminderType.Recurring:
                var recurringResult = await _reminderDataDataService.GetSingleBySpecAsync(
                    new ActiveRecurringReminderByNameOrIdAndGuildSpec(req.Name ?? string.Empty, req.GuildId, req.ReminderId));
                if (!recurringResult.IsDefined(out var recurringReminder)) return Result<ReminderResDto>.FromError(recurringResult);

                if (req.RequestedOnBehalfOfId != recurringReminder.CreatorId)
                    return new DiscordNotAuthorizedError("Only the creator of a reminder can make changes to it.");

                RecurringJob.RemoveIfExists(recurringReminder.HangfireId);

                await _reminderDataDataService.DisableAsync(recurringReminder, true);

                return new ReminderResDto(recurringReminder.Id, recurringReminder.Name, DateTime.MinValue,
                    recurringReminder.Mentions, recurringReminder.Text ?? "Text not set", true);
            default:
                throw new ArgumentOutOfRangeException(nameof(req.Type));
        }
    }
}
