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

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DSharpPlus.Entities;
using Hangfire;
using JetBrains.Annotations;
using Lisbeth.Bot.Application.Discord.Helpers;
using Lisbeth.Bot.Application.Discord.Services.Interfaces;
using Lisbeth.Bot.Application.Enums;
using Lisbeth.Bot.Application.Helpers;
using Lisbeth.Bot.Application.Results;
using Lisbeth.Bot.Application.Services.Database.Interfaces;
using Lisbeth.Bot.DataAccessLayer.Specifications.RecurringReminder;
using Lisbeth.Bot.DataAccessLayer.Specifications.Reminder;
using Lisbeth.Bot.Domain.Entities;
using Lisbeth.Bot.Domain.Enums;
using MikyM.Common.Application.Results;
using MikyM.Common.Application.Results.Errors;
using MikyM.Discord.Interfaces;

namespace Lisbeth.Bot.Application.Discord.Services
{
    [UsedImplicitly]
    public class DiscordSendReminderService : IDiscordSendReminderService
    {
        private readonly IDiscordService _discord;
        private readonly IDiscordEmbedProvider _embedProvider;
        private readonly IRecurringReminderService _recurringReminderService;
        private readonly IReminderService _reminderService;

        public DiscordSendReminderService(IReminderService reminderService,
            IRecurringReminderService recurringReminderService, IDiscordService discord,
            IDiscordEmbedProvider embedProvider)
        {
            _reminderService = reminderService;
            _recurringReminderService = recurringReminderService;
            _discord = discord;
            _embedProvider = embedProvider;
        }

        [Queue("reminder")]
        [PreserveOriginalQueue]
        public async Task<Result> SendReminderAsync(long reminderId, ReminderType type)
        {
            Guild guild;
            EmbedConfig? embedConfig;
            string? text;
            List<string>? mentions;
            DiscordChannel channel;

            switch (type)
            {
                case ReminderType.Single:
                    var rem = await _reminderService.GetSingleBySpecAsync<Reminder>(
                        new ActiveReminderByIdWithEmbedSpec(reminderId));
                    if (!rem.IsSuccess || rem.Entity.Guild?.ReminderChannelId is null)
                        return Result.FromError(new NotFoundError());
                    guild = rem.Entity.Guild;
                    embedConfig = rem.Entity.EmbedConfig;
                    text = rem.Entity.Text;
                    mentions = rem.Entity.Mentions;
                    break;
                case ReminderType.Recurring:
                    var recRem = await _recurringReminderService.GetSingleBySpecAsync<RecurringReminder>(
                        new ActiveRecurringReminderByIdWithEmbedSpec(reminderId));
                    if (!recRem.IsSuccess || recRem.Entity.Guild?.ReminderChannelId is null)
                        return Result.FromError(new NotFoundError());
                    guild = recRem.Entity.Guild;
                    embedConfig = recRem.Entity.EmbedConfig;
                    text = recRem.Entity.Text;
                    mentions = recRem.Entity.Mentions;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }

            try
            {
                channel = await _discord.Client.GetChannelAsync(guild.ReminderChannelId.Value);
            }
            catch (Exception)
            {
                // ignore
                return Result.FromError(new DiscordNotFoundError(DiscordEntityType.Channel));
            }

            if (embedConfig is not null)
                await channel.SendMessageAsync(string.Join(' ', mentions,
                    _embedProvider.ConfigureEmbed(embedConfig).Build()));
            else
                await channel.SendMessageAsync(string.Join(' ', mentions + "\n\n" + text));

            return Result.FromSuccess();
        }
    }
}