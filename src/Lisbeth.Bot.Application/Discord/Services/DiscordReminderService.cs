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
using System.Globalization;
using System.Threading.Tasks;

namespace Lisbeth.Bot.Application.Discord.Services
{
    public class DiscordReminderService : IDiscordReminderService
    {
        private readonly IDiscordService _discord;
        private readonly IMainReminderService _reminderService;
        private readonly IGuildService _guildService;

        public async Task<DiscordEmbed> SetNewReminderAsync([NotNull] SetReminderReqDto req)
        {
            if (req is null) throw new ArgumentNullException(nameof(req));

            DiscordGuild guild = await _discord.Client.GetGuildAsync(req.GuildId);

            return await this.SetNewReminderAsync(guild, await guild.GetMemberAsync(req.RequestedOnBehalfOfId), req);
        }

        public async Task<DiscordEmbed> SetNewReminderAsync([NotNull] InteractionContext ctx,
            [NotNull] SetReminderReqDto req)
        {
            if (ctx is null) throw new ArgumentNullException(nameof(ctx));
            if (req is null) throw new ArgumentNullException(nameof(req));

            return await this.SetNewReminderAsync(ctx.Guild, ctx.Member, req);
        }

        private async Task<DiscordEmbed> SetNewReminderAsync([NotNull] DiscordGuild discordGuild, [NotNull] DiscordMember requestingMember,
            [NotNull] SetReminderReqDto req)
        {
            if (discordGuild is null) throw new ArgumentNullException(nameof(discordGuild));
            if (requestingMember is null) throw new ArgumentNullException(nameof(requestingMember));
            if (req is null) throw new ArgumentNullException(nameof(req));

            if (!string.IsNullOrWhiteSpace(req.CronExpression) && !requestingMember.IsModerator())
                throw new DiscordNotAuthorizedException();

            var guild = await _guildService.GetSingleBySpecAsync<Guild>(new ActiveGuildByIdSpec(req.GuildId));

            if (guild is null) throw new NotFoundException(nameof(req.GuildId));

            var res = await _reminderService.SetNewReminderAsync(req);

            var embed = new DiscordEmbedBuilder().WithColor(new DiscordColor(guild.EmbedHexColor))
                .WithAuthor("Lisbeth reminder service")
                .WithDescription("Reminder set successfully")
                .AddField("Reminder's id", res.Id.ToString())
                .AddField("Reminder's name", res.Name)
                .AddField("Next occurrence", res.NextOccurrence.ToString(CultureInfo.InvariantCulture))
                .AddField("Mentions", string.Join(", ", res.Mentions));

            return embed.Build();
        }

        public async Task<DiscordEmbed> DisableReminderAsync([NotNull] DisableReminderReqDto req)
        {
            if (req is null) throw new ArgumentNullException(nameof(req));

            DiscordGuild guild = await _discord.Client.GetGuildAsync(req.GuildId);

            return await this.DisableReminderAsync(guild, await guild.GetMemberAsync(req.RequestedOnBehalfOfId), req);
        }

        public async Task<DiscordEmbed> DisableReminderAsync([NotNull] InteractionContext ctx,
            [NotNull] DisableReminderReqDto req)
        {
            if (ctx is null) throw new ArgumentNullException(nameof(ctx));
            if (req is null) throw new ArgumentNullException(nameof(req));

            return await this.DisableReminderAsync(ctx.Guild, ctx.Member, req);
        }

        private async Task<DiscordEmbed> DisableReminderAsync([NotNull] DiscordGuild discordGuild, [NotNull] DiscordMember requestingMember,
            [NotNull] DisableReminderReqDto req)
        {
            if (discordGuild is null) throw new ArgumentNullException(nameof(discordGuild));
            if (requestingMember is null) throw new ArgumentNullException(nameof(requestingMember));
            if (req is null) throw new ArgumentNullException(nameof(req));

            if (req.Type is ReminderType.Recurring && !requestingMember.IsModerator())
                throw new DiscordNotAuthorizedException();

            var guild = await _guildService.GetSingleBySpecAsync<Guild>(new ActiveGuildByIdSpec(req.GuildId));

            if (guild is null) throw new NotFoundException(nameof(req.GuildId));

            var res = await _reminderService.DisableReminderAsync(req);

            var embed = new DiscordEmbedBuilder().WithColor(new DiscordColor(guild.EmbedHexColor))
                .WithAuthor("Lisbeth reminder service")
                .WithDescription("Reminder disabled successfully")
                .AddField("Reminder's id", res.Id.ToString())
                .AddField("Reminder's name", res.Name);

            return embed.Build();
        }

        public async Task<DiscordEmbed> RescheduleReminderAsync([NotNull] RescheduleReminderReqDto req)
        {
            if (req is null) throw new ArgumentNullException(nameof(req));

            DiscordGuild guild = await _discord.Client.GetGuildAsync(req.GuildId);

            return await this.RescheduleReminderAsync(guild, await guild.GetMemberAsync(req.RequestedOnBehalfOfId), req);
        }

        public async Task<DiscordEmbed> RescheduleReminderAsync([NotNull] InteractionContext ctx,
            [NotNull] RescheduleReminderReqDto req)
        {
            if (ctx is null) throw new ArgumentNullException(nameof(ctx));
            if (req is null) throw new ArgumentNullException(nameof(req));

            return await this.RescheduleReminderAsync(ctx.Guild, ctx.Member, req);
        }

        private async Task<DiscordEmbed> RescheduleReminderAsync([NotNull] DiscordGuild discordGuild, [NotNull] DiscordMember requestingMember,
            [NotNull] RescheduleReminderReqDto req)
        {
            if (discordGuild is null) throw new ArgumentNullException(nameof(discordGuild));
            if (requestingMember is null) throw new ArgumentNullException(nameof(requestingMember));
            if (req is null) throw new ArgumentNullException(nameof(req));

            if (!string.IsNullOrWhiteSpace(req.CronExpression) && !requestingMember.IsModerator())
                throw new DiscordNotAuthorizedException();

            var guild = await _guildService.GetSingleBySpecAsync<Guild>(new ActiveGuildByIdSpec(req.GuildId));

            if (guild is null) throw new NotFoundException(nameof(req.GuildId));

            var res = await _reminderService.RescheduleReminderAsync(req);

            var embed = new DiscordEmbedBuilder().WithColor(new DiscordColor(guild.EmbedHexColor))
                .WithAuthor("Lisbeth reminder service")
                .WithDescription("Reminder rescheduled successfully")
                .AddField("Reminder's id", res.Id.ToString())
                .AddField("Reminder's name", res.Name)
                .AddField("Next occurrence", res.NextOccurrence.ToString(CultureInfo.InvariantCulture))
                .AddField("Mentions", string.Join(", ", res.Mentions));

            return embed.Build();
        }
    }
}