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

using System;
using Lisbeth.Bot.Domain.DTOs.Request.Base;

namespace Lisbeth.Bot.Domain.DTOs.Request.Reminder;

public class RescheduleReminderReqDto : BaseAuthWithGuildReqDto
{
    public RescheduleReminderReqDto()
    {
    }

    public RescheduleReminderReqDto(string name, string? cronExpression, DateTime? setFor,
        string? timeSpanExpression, ulong guildId, ulong requestedOnBehalfOfId, long? reminderId = null,
        long? newHangfireId = null) : base(guildId, requestedOnBehalfOfId)
    {
        Name = name;
        CronExpression = cronExpression;
        SetFor = setFor;
        TimeSpanExpression = timeSpanExpression;
        ReminderId = reminderId;
        NewHangfireId = newHangfireId;
    }

    public string? Name { get; set; }
    public string? CronExpression { get; set; }
    public DateTime? SetFor { get; set; }
    public string? TimeSpanExpression { get; set; }
    public long? ReminderId { get; set; }
    public long? NewHangfireId { get; set; }
}