﻿// This file is part of Lisbeth.Bot project
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

using AutoMapper;
using JetBrains.Annotations;
using Lisbeth.Bot.Application.Exceptions;
using Lisbeth.Bot.Application.Services.Database.Interfaces;
using Lisbeth.Bot.DataAccessLayer;
using Lisbeth.Bot.DataAccessLayer.Specifications.RecurringReminder;
using Lisbeth.Bot.Domain.DTOs.Request.Reminder;
using Lisbeth.Bot.Domain.Entities;
using MikyM.Common.Application.Services;
using MikyM.Common.DataAccessLayer.UnitOfWork;
using System.Threading.Tasks;

namespace Lisbeth.Bot.Application.Services.Database
{
    [UsedImplicitly]
    public class RecurringReminderService : CrudService<RecurringReminder, LisbethBotDbContext>,
        IRecurringReminderService
    {
        public RecurringReminderService(IMapper mapper, IUnitOfWork<LisbethBotDbContext> uof) : base(mapper, uof)
        {
        }

        public async Task SetHangfireIdAsync(long reminderId, string hangfireId, bool shouldSave = false)
        {
            var reminder = await base.GetAsync<RecurringReminder>(reminderId);
            reminder.HangfireId = long.Parse(hangfireId);

            if (shouldSave) await base.CommitAsync();
        }

        public async Task RescheduleAsync(RescheduleReminderReqDto req, bool shouldSave = false)
        {
            var reminder = await base.GetSingleBySpecAsync<RecurringReminder>(
                new ActiveRecurringReminderByNameOrIdAndGuildSpec(req.Name, req.GuildId, req.ReminderId));
            if (reminder is null) throw new NotFoundException();

            base.BeginUpdate(reminder);
            reminder.CronExpression = req.CronExpression;
            reminder.LastEditById = req.RequestedOnBehalfOfId;

            if (shouldSave) await base.CommitAsync();
        }
    }
}