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
using Lisbeth.Bot.Application.Discord.EmbedBuilders;
using Lisbeth.Bot.Application.Discord.Helpers;
using Lisbeth.Bot.Domain.DTOs.Request.Base;
using MikyM.Discord.EmbedBuilders;
using MikyM.Discord.EmbedBuilders.Enrichers;
using MikyM.Discord.EmbedBuilders.Enums;
using MikyM.Discord.Enums;
using MikyM.Discord.Extensions.BaseExtensions;
using MikyM.Discord.Interfaces;

namespace Lisbeth.Bot.Application.Discord.Services
{
    [UsedImplicitly]
    public class DiscordGuildLoggerService : IDiscordGuildLoggerService
    {
        private readonly IDiscordEmbedProvider _embedProvider;
        private readonly IDiscordGuildLogSenderService _logSender;
        private readonly IDiscordService _discord;
        private readonly IGuildService _guildService;

        public DiscordGuildLoggerService(IDiscordEmbedProvider embedProvider, IDiscordGuildLogSenderService logSender,
            IDiscordService discord, IGuildService guildService)
        {
            _embedProvider = embedProvider;
            _logSender = logSender;
            _discord = discord;
            _guildService = guildService;
        }

        public async Task<Result> LogToDiscordAsync<TRequest>(DiscordGuild discordGuild, TRequest req, DiscordMember? moderator = null, string hexColor = "#26296e", long? id = null) where TRequest : IBaseModAuthReq, IEmbedEnricher
        {
            if (moderator is null)
            {
                try
                {
                    moderator = await discordGuild.GetMemberAsync(req.RequestedOnBehalfOfId);
                    if (moderator is null) return new DiscordNotFoundError(nameof(moderator));
                }
                catch (Exception)
                {
                    return new DiscordNotFoundError(nameof(moderator));
                }
            }

            if (!moderator.IsModerator()) return new DiscordNotAuthorizedError();

            var embed = new DiscordEmbedBuilder().AsEnhanced()
                .WithCase(id)
                .AsEnriched<LogDiscordEmbedBuilder>()
                .WithType(DiscordLog.Moderation)
                .EnrichFrom(req)
                .Build();

            return await _logSender.SendAsync(discordGuild, DiscordLog.Moderation, embed);
        }


        public async Task<Result> LogToDiscordAsync<TRequest>(ulong discordGuildId, TRequest req, DiscordMember? moderator = null, string hexColor = "#26296e", long? id = null) where TRequest : IBaseModAuthReq, IEmbedEnricher
        {
            if (_discord.Client.Guilds.TryGetValue(discordGuildId, out var guild)) return new DiscordNotFoundError(DiscordEntity.Guild);

            return await this.LogToDiscordAsync(guild ?? throw new InvalidOperationException("Guild was null."), req, moderator, hexColor, id);
        }

        public async Task<Result> LogToDiscordAsync<TRequest>(Guild guild, TRequest req, DiscordMember? moderator = null, string hexColor = "#26296e", long? id = null) where TRequest : IBaseModAuthReq, IEmbedEnricher
        {
            return await this.LogToDiscordAsync(guild.GuildId, req, moderator, hexColor, id);
        }

        public async Task<Result> LogToDiscordAsync<TRequest>(long guildId, TRequest req, DiscordMember? moderator = null, string hexColor = "#26296e", long? id = null) where TRequest : IBaseModAuthReq, IEmbedEnricher
        {
            var guildRes =
                await _guildService.GetAsync(guildId);

            if (!guildRes.IsDefined()) return new NotFoundError();

            return await this.LogToDiscordAsync(guildRes.Entity.GuildId, req, moderator, hexColor, id);
        }
    }
}
