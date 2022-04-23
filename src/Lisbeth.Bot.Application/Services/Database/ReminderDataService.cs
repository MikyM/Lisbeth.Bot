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

using AutoMapper;
using Lisbeth.Bot.DataAccessLayer;
using Lisbeth.Bot.DataAccessLayer.Specifications.Reminder;
using Lisbeth.Bot.Domain.DTOs.Request.Reminder;
using MikyM.Common.DataAccessLayer.UnitOfWork;
using MikyM.Common.Utilities.Results;
using MikyM.Common.Utilities.Results.Errors.Bases;
using NCrontab;

namespace Lisbeth.Bot.Application.Services.Database;

[UsedImplicitly]
public class ReminderDataService : CrudDataService<Reminder, LisbethBotDbContext>, IReminderDataService
{
    public ReminderDataService(IMapper mapper, IUnitOfWork<LisbethBotDbContext> uof) : base(mapper, uof)
    {
    }

    public async Task<Result> SetHangfireIdAsync(long reminderId, string hangfireId, bool shouldSave = false)
    {
        var result = await base.GetAsync(reminderId);

        if (!result.IsDefined()) return Result.FromError(result);

        result.Entity.HangfireId = hangfireId;

        if (shouldSave) await base.CommitAsync();

        return Result.FromSuccess();
    }

    public async Task<Result> RescheduleAsync(RescheduleReminderReqDto req, bool shouldSave = false)
    {
        var result =
            await base.GetSingleBySpecAsync(
                new ActiveReminderByNameOrIdAndGuildSpec(req.Name ?? string.Empty, req.GuildId, req.ReminderId));
        if (!result.IsDefined(out var reminder)) return Result.FromError(result);

        if (reminder.IsRecurring && string.IsNullOrWhiteSpace(req.CronExpression))
            return new ArgumentError(nameof(req.CronExpression),
                "Reminder is of recurring type but cron expression within the request was empty");
        if (!reminder.IsRecurring && string.IsNullOrWhiteSpace(req.TimeSpanExpression) && !req.SetFor.HasValue)
            return new ArgumentError(nameof(req.TimeSpanExpression) + " " + nameof(req.SetFor),
                "Reminder is of regular type but timespan expression and set for date within the request were empty");


        if (reminder.IsRecurring)
        {
            var parsed = CrontabSchedule.TryParse(req.CronExpression);
            var parsedWithSeconds = CrontabSchedule.TryParse(req.CronExpression,
                new CrontabSchedule.ParseOptions { IncludingSeconds = true });
            if (parsed is null && parsedWithSeconds is null && reminder.IsRecurring)
                return new ArgumentError(nameof(req.CronExpression), "Invalid cron expression");

            base.BeginUpdate(reminder);
            reminder.CronExpression = req.CronExpression;
        }
        else
        {
            var isValidTimeSpanExpression =
                (req.TimeSpanExpression ?? throw new InvalidOperationException()).TryParseToDurationAndNextOccurrence(
                    out var occurrence, out _);

            if (!isValidTimeSpanExpression && !req.SetFor.HasValue)
                return new ArgumentError(nameof(req.TimeSpanExpression),
                    "Invalid timespan expression value");

            base.BeginUpdate(reminder);

            reminder.SetFor = req.SetFor ?? occurrence;
        }

        result.Entity.LastEditById = req.RequestedOnBehalfOfId;

        if (shouldSave) await base.CommitAsync();

        return Result.FromSuccess();
    }
}