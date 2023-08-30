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

using DSharpPlus.EventArgs;
using Lisbeth.Bot.Application.Discord.EmbedEnrichers.Log.Moderation;
using Lisbeth.Bot.Domain.DTOs.Request.Base;
using Lisbeth.Bot.Domain.DTOs.Request.Prune;
using MikyM.Discord.EmbedBuilders.Enrichers;
using MikyM.Discord.EmbedBuilders.Enums;

namespace Lisbeth.Bot.Application.Discord.Services;

[UsedImplicitly]
[ServiceImplementation<IDiscordGuildLoggerService>(ServiceLifetime.InstancePerLifetimeScope)]
public class DiscordGuildLoggerService : IDiscordGuildLoggerService
{
    private readonly IDiscordGuildLogSenderService _logSender;
    private readonly IDiscordService _discord;
    private readonly IGuildDataService _guildDataService;
    private readonly ILogDiscordEmbedBuilder _embedBuilder;

    public DiscordGuildLoggerService(ILogDiscordEmbedBuilder embedBuilder, IDiscordGuildLogSenderService logSender,
        IDiscordService discord, IGuildDataService guildDataService)
    {
        _embedBuilder = embedBuilder;
        _logSender = logSender;
        _discord = discord;
        _guildDataService = guildDataService;
    }

    public async Task<Result> LogToDiscordAsync<TRequest>(DiscordGuild discordGuild, TRequest req, DiscordModeration moderation, DiscordUser? moderator = null, SnowflakeObject? target = null, string hexColor = "#26296e", long? caseId = null) where TRequest : class, IBaseModAuthReq
    {
        if (moderator is null)
        {
            try
            {
                moderator = await _discord.Client.GetUserAsync(req.RequestedOnBehalfOfId);
            }
            catch (Exception)
            {
                // ignore
            }
        }

        IEmbedEnricher enricher = req switch
        {
            IApplyInfractionReq addReq => new MemberModAddReqLogEnricher(addReq),
            IRevokeInfractionReq disableReq => new MemberModDisableReqLogEnricher(disableReq),
            IGetInfractionReq getReq => new MemberModGetReqLogEnricher(getReq),
            PruneReqDto pruneReq => new PruneModAddReqLogEnricher(pruneReq),
            _ => throw new NotSupportedException("Given request to log is not supported")
        };

        var embed = _embedBuilder
            .EnrichFrom(enricher)
            .WithType(DiscordLog.Moderation)
            .WithModerationType(moderation)
            .WithCase(caseId)
            .WithEmbedColor(new DiscordColor(hexColor))
            .WithAuthorSnowflakeInfo(moderator)
            .WithFooterSnowflakeInfo(target)
            .Build();

        return await _logSender.SendAsync(discordGuild, DiscordLog.Moderation, embed);
    }


    public async Task<Result> LogToDiscordAsync<TRequest>(ulong discordGuildId, TRequest req, DiscordModeration moderation, DiscordUser? moderator = null, SnowflakeObject? target = null, string hexColor = "#26296e", long? caseId = null) where TRequest : class, IBaseModAuthReq
    {
        if (_discord.Client.Guilds.TryGetValue(discordGuildId, out var guild)) return new DiscordNotFoundError(DiscordEntity.Guild);

        return await LogToDiscordAsync(guild ?? throw new InvalidOperationException("Guild was null."), req, moderation, moderator, target, hexColor, caseId);
    }

    public async Task<Result> LogToDiscordAsync<TRequest>(Guild guild, TRequest req, DiscordModeration moderation, DiscordUser? moderator = null, SnowflakeObject? target = null, string hexColor = "#26296e", long? caseId = null) where TRequest : class, IBaseModAuthReq
    {
        return await LogToDiscordAsync(guild.GuildId, req, moderation, moderator, target, hexColor, caseId);
    }

    public async Task<Result> LogToDiscordAsync<TRequest>(long guildId, TRequest req, DiscordModeration moderation, DiscordUser? moderator = null, SnowflakeObject? target = null, string hexColor = "#26296e", long? caseId = null) where TRequest : class, IBaseModAuthReq
    {
        var guildRes =
            await _guildDataService.GetAsync(guildId);

        if (!guildRes.IsDefined()) return new NotFoundError();

        return await LogToDiscordAsync(guildRes.Entity.GuildId, req, moderation, moderator, target, hexColor, caseId);
    }

    public Task<Result> LogToDiscordAsync<TEvent>(DiscordGuild discordGuild, TEvent discordEvent, DiscordLog log, string hexColor = "#26296e") where TEvent : DiscordEventArgs
    {
        throw new NotImplementedException();
    }


    public async Task<Result> LogToDiscordAsync<TEvent>(ulong discordGuildId, TEvent discordEvent, DiscordLog log, string hexColor = "#26296e") where TEvent : DiscordEventArgs
    {
        if (_discord.Client.Guilds.TryGetValue(discordGuildId, out var guild)) return new DiscordNotFoundError(DiscordEntity.Guild);

        return await LogToDiscordAsync(guild ?? throw new InvalidOperationException("Guild was null."), discordEvent, log, hexColor);
    }

    public async Task<Result> LogToDiscordAsync<TEvent>(Guild guild, TEvent discordEvent, DiscordLog log, string hexColor = "#26296e") where TEvent : DiscordEventArgs
    {
        return await LogToDiscordAsync(guild.GuildId, discordEvent, log, hexColor);
    }

    public async Task<Result> LogToDiscordAsync<TEvent>(long guildId, TEvent discordEvent, DiscordLog log, string hexColor = "#26296e") where TEvent : DiscordEventArgs
    {
        var guildRes =
            await _guildDataService.GetAsync(guildId);

        if (!guildRes.IsDefined()) return new NotFoundError();

        return await LogToDiscordAsync(guildRes.Entity.GuildId, discordEvent, log, hexColor);
    }
}
