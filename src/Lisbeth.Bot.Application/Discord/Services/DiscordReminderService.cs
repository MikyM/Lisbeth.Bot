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

using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using JetBrains.Annotations;
using Lisbeth.Bot.Application.Discord.Exceptions;
using Lisbeth.Bot.Application.Discord.Extensions;
using Lisbeth.Bot.Application.Discord.Services.Interfaces;
using Lisbeth.Bot.Application.Exceptions;
using Lisbeth.Bot.Application.Services.Database.Interfaces;
using Lisbeth.Bot.Application.Services.Interfaces;
using Lisbeth.Bot.DataAccessLayer.Specifications.Guild;
using Lisbeth.Bot.Domain.DTOs.Request.Reminder;
using Lisbeth.Bot.Domain.Entities;
using Lisbeth.Bot.Domain.Enums;
using MikyM.Discord.Interfaces;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using Lisbeth.Bot.Application.Results;
using MikyM.Common.Application.Results;

namespace Lisbeth.Bot.Application.Discord.Services
{
    public class DiscordReminderService : IDiscordReminderService
    {
        private readonly IDiscordService _discord;
        private readonly IMainReminderService _reminderService;
        private readonly IGuildService _guildService;

        public DiscordReminderService(IDiscordService discord, IMainReminderService reminderService,
            IGuildService guildService)
        {
            _discord = discord;
            _reminderService = reminderService;
            _guildService = guildService;
        }

        public async Task<Result<DiscordEmbed>> SetNewReminderAsync(SetReminderReqDto req)
        {
            if (req is null) throw new ArgumentNullException(nameof(req));

            DiscordGuild guild = await _discord.Client.GetGuildAsync(req.GuildId);

            return await this.SetNewReminderAsync(guild, await guild.GetMemberAsync(req.RequestedOnBehalfOfId), req);
        }

        public async Task<Result<DiscordEmbed>> SetNewReminderAsync(InteractionContext ctx,
            SetReminderReqDto req)
        {
            if (ctx is null) throw new ArgumentNullException(nameof(ctx));
            if (req is null) throw new ArgumentNullException(nameof(req));

            return await this.SetNewReminderAsync(ctx.Guild, ctx.Member, req);
        }

        private async Task<Result<DiscordEmbed>> SetNewReminderAsync(DiscordGuild discordGuild, DiscordMember requestingMember,
            SetReminderReqDto req)
        {
            if (discordGuild is null) throw new ArgumentNullException(nameof(discordGuild));
            if (requestingMember is null) throw new ArgumentNullException(nameof(requestingMember));
            if (req is null) throw new ArgumentNullException(nameof(req));

            if (!string.IsNullOrWhiteSpace(req.CronExpression) && !requestingMember.IsModerator())
                return Result<DiscordEmbed>.FromError(new DiscordNotAuthorizedError());

            var result = await _guildService.GetSingleBySpecAsync<Guild>(new ActiveGuildByIdSpec(req.GuildId));

            if (!result.IsSuccess) return Result<DiscordEmbed>.FromError(result);

            var res = await _reminderService.SetNewReminderAsync(req);

            var embed = new DiscordEmbedBuilder().WithColor(new DiscordColor(result.Entity.EmbedHexColor))
                .WithAuthor("Lisbeth reminder service")
                .WithDescription("Reminder set successfully")
                .AddField("Reminder's id", res.Entity.Id.ToString())
                .AddField("Reminder's name", res.Entity.Name)
                .AddField("Next occurrence", res.Entity.NextOccurrence.ToString(CultureInfo.InvariantCulture))
                .AddField("Mentions", string.Join(", ", res.Entity.Mentions ?? throw new InvalidOperationException()));

            return embed.Build();
        }

        public async Task<Result<DiscordEmbed>> DisableReminderAsync(DisableReminderReqDto req)
        {
            if (req is null) throw new ArgumentNullException(nameof(req));

            DiscordGuild guild = await _discord.Client.GetGuildAsync(req.GuildId);

            return await this.DisableReminderAsync(guild, await guild.GetMemberAsync(req.RequestedOnBehalfOfId), req);
        }

        public async Task<Result<DiscordEmbed>> DisableReminderAsync(InteractionContext ctx,
            DisableReminderReqDto req)
        {
            if (ctx is null) throw new ArgumentNullException(nameof(ctx));
            if (req is null) throw new ArgumentNullException(nameof(req));

            return await this.DisableReminderAsync(ctx.Guild, ctx.Member, req);
        }

        private async Task<Result<DiscordEmbed>> DisableReminderAsync(DiscordGuild discordGuild, DiscordMember requestingMember,
            DisableReminderReqDto req)
        {
            if (discordGuild is null) throw new ArgumentNullException(nameof(discordGuild));
            if (requestingMember is null) throw new ArgumentNullException(nameof(requestingMember));
            if (req is null) throw new ArgumentNullException(nameof(req));

            if (req.Type is ReminderType.Recurring && !requestingMember.IsModerator())
                return Result<DiscordEmbed>.FromError(new DiscordNotAuthorizedError());

            var result = await _guildService.GetSingleBySpecAsync<Guild>(new ActiveGuildByIdSpec(req.GuildId));

            if (!result.IsSuccess) return Result<DiscordEmbed>.FromError(result);

            var res = await _reminderService.DisableReminderAsync(req);

            var embed = new DiscordEmbedBuilder().WithColor(new DiscordColor(result.Entity.EmbedHexColor))
                .WithAuthor("Lisbeth reminder service")
                .WithDescription("Reminder disabled successfully")
                .AddField("Reminder's id", res.Entity.Id.ToString())
                .AddField("Reminder's name", res.Entity.Name);

            return embed.Build();
        }

        public async Task<Result<DiscordEmbed>> RescheduleReminderAsync(RescheduleReminderReqDto req)
        {
            if (req is null) throw new ArgumentNullException(nameof(req));

            DiscordGuild guild = await _discord.Client.GetGuildAsync(req.GuildId);

            return await this.RescheduleReminderAsync(guild, await guild.GetMemberAsync(req.RequestedOnBehalfOfId), req);
        }

        public async Task<Result<DiscordEmbed>> RescheduleReminderAsync(InteractionContext ctx,
            RescheduleReminderReqDto req)
        {
            if (ctx is null) throw new ArgumentNullException(nameof(ctx));
            if (req is null) throw new ArgumentNullException(nameof(req));

            return await this.RescheduleReminderAsync(ctx.Guild, ctx.Member, req);
        }

        private async Task<Result<DiscordEmbed>> RescheduleReminderAsync(DiscordGuild discordGuild, DiscordMember requestingMember,
            RescheduleReminderReqDto req)
        {
            if (discordGuild is null) throw new ArgumentNullException(nameof(discordGuild));
            if (requestingMember is null) throw new ArgumentNullException(nameof(requestingMember));
            if (req is null) throw new ArgumentNullException(nameof(req));

            if (!string.IsNullOrWhiteSpace(req.CronExpression) && !requestingMember.IsModerator())
                return Result<DiscordEmbed>.FromError(new DiscordNotAuthorizedError());

            var result = await _guildService.GetSingleBySpecAsync<Guild>(new ActiveGuildByIdSpec(req.GuildId));

            if (!result.IsSuccess) return Result<DiscordEmbed>.FromError(result);

            var res = await _reminderService.RescheduleReminderAsync(req);

            var embed = new DiscordEmbedBuilder().WithColor(new DiscordColor(result.Entity.EmbedHexColor))
                .WithAuthor("Lisbeth reminder service")
                .WithDescription("Reminder rescheduled successfully")
                .AddField("Reminder's id", res.Entity.Id.ToString())
                .AddField("Reminder's name", res.Entity.Name)
                .AddField("Next occurrence", res.Entity.NextOccurrence.ToString(CultureInfo.InvariantCulture))
                .AddField("Mentions", string.Join(", ", res.Entity.Mentions ?? new List<string>()));

            return embed.Build();
        }
    }
}