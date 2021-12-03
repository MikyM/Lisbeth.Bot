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
using DSharpPlus.EventArgs;
using Lisbeth.Bot.Application.Discord.EmbedBuilders;
using Lisbeth.Bot.Application.Discord.EmbedEnrichers.Log;
using Lisbeth.Bot.Domain.DTOs.Request.Base;
using MikyM.Discord.EmbedBuilders.Builders;
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
        private readonly IDiscordGuildLogSenderService _logSender;
        private readonly IDiscordService _discord;
        private readonly IGuildService _guildService;
        private readonly IEnhancedDiscordEmbedBuilder _embedBuilder;

        public DiscordGuildLoggerService(IEnhancedDiscordEmbedBuilder embedBuilder, IDiscordGuildLogSenderService logSender,
            IDiscordService discord, IGuildService guildService)
        {
            _embedBuilder = embedBuilder;
            _logSender = logSender;
            _discord = discord;
            _guildService = guildService;
        }

        public async Task<Result> LogToDiscordAsync<TRequest>(DiscordGuild discordGuild, TRequest req, DiscordModeration moderation, DiscordMember? moderator = null, SnowflakeObject? target = null, string hexColor = "#26296e", long? caseId = null) where TRequest : class, IBaseModAuthReq
        {
            if (moderator is null)
            {
                try
                {
                    moderator = await discordGuild.GetMemberAsync(req.RequestedOnBehalfOfId);
                }
                catch (Exception)
                {
                    // ignore
                }
            }

            IEmbedEnricher enricher = req switch
            {
                IAddModReq addReq => new MemberModAddReqLogEnricher(addReq),
                IDisableModReq disableReq => new MemberModDisableReqLogEnricher(disableReq),
                IGetModReq getReq => new MemberModGetReqLogEnricher(getReq),
                _ => throw new NotSupportedException("Given request to log is not supported")
            };

            var embed = _embedBuilder
                .WithCase(caseId)
                .WithEmbedColor(new DiscordColor(hexColor))
                .WithAuthorSnowflakeInfo(moderator)
                .WithFooterSnowflakeInfo(target)
                .AsEnriched<LogDiscordEmbedBuilder>()
                .WithType(DiscordLog.Moderation)
                .WithModerationType(moderation)
                .EnrichFrom(enricher)
                .Build();

            return await _logSender.SendAsync(discordGuild, DiscordLog.Moderation, embed);
        }


        public async Task<Result> LogToDiscordAsync<TRequest>(ulong discordGuildId, TRequest req, DiscordModeration moderation, DiscordMember? moderator = null, SnowflakeObject? target = null, string hexColor = "#26296e", long? caseId = null) where TRequest : class, IBaseModAuthReq
        {
            if (_discord.Client.Guilds.TryGetValue(discordGuildId, out var guild)) return new DiscordNotFoundError(DiscordEntity.Guild);

            return await this.LogToDiscordAsync(guild ?? throw new InvalidOperationException("Guild was null."), req, moderation, moderator, target, hexColor, caseId);
        }

        public async Task<Result> LogToDiscordAsync<TRequest>(Guild guild, TRequest req, DiscordModeration moderation, DiscordMember? moderator = null, SnowflakeObject? target = null, string hexColor = "#26296e", long? caseId = null) where TRequest : class, IBaseModAuthReq
        {
            return await this.LogToDiscordAsync(guild.GuildId, req, moderation, moderator, target, hexColor, caseId);
        }

        public async Task<Result> LogToDiscordAsync<TRequest>(long guildId, TRequest req, DiscordModeration moderation, DiscordMember? moderator = null, SnowflakeObject? target = null, string hexColor = "#26296e", long? caseId = null) where TRequest : class, IBaseModAuthReq
        {
            var guildRes =
                await _guildService.GetAsync(guildId);

            if (!guildRes.IsDefined()) return new NotFoundError();

            return await this.LogToDiscordAsync(guildRes.Entity.GuildId, req, moderation, moderator, target, hexColor, caseId);
        }

        public async Task<Result> LogToDiscordAsync<TEvent>(DiscordGuild discordGuild, TEvent discordEvent, DiscordLog log, string hexColor = "#26296e") where TEvent : DiscordEventArgs
        {
            IEmbedEnricher enricher = null;

            var embed = _embedBuilder
                .WithEmbedColor(new DiscordColor(hexColor))
                .AsEnriched<LogDiscordEmbedBuilder>()
                .WithType(log)
                .EnrichFrom(enricher)
                .Build();

            return await _logSender.SendAsync(discordGuild, log, embed);
        }


        public async Task<Result> LogToDiscordAsync<TEvent>(ulong discordGuildId, TEvent discordEvent, DiscordLog log, string hexColor = "#26296e") where TEvent : DiscordEventArgs
        {
            if (_discord.Client.Guilds.TryGetValue(discordGuildId, out var guild)) return new DiscordNotFoundError(DiscordEntity.Guild);

            return await this.LogToDiscordAsync(guild ?? throw new InvalidOperationException("Guild was null."), discordEvent, log, hexColor);
        }

        public async Task<Result> LogToDiscordAsync<TEvent>(Guild guild, TEvent discordEvent, DiscordLog log, string hexColor = "#26296e") where TEvent : DiscordEventArgs
        {
            return await this.LogToDiscordAsync(guild.GuildId, discordEvent, log, hexColor);
        }

        public async Task<Result> LogToDiscordAsync<TEvent>(long guildId, TEvent discordEvent, DiscordLog log, string hexColor = "#26296e") where TEvent : DiscordEventArgs
        {
            var guildRes =
                await _guildService.GetAsync(guildId);

            if (!guildRes.IsDefined()) return new NotFoundError();

            return await this.LogToDiscordAsync(guildRes.Entity.GuildId, discordEvent, log, hexColor);
        }
    }
}
