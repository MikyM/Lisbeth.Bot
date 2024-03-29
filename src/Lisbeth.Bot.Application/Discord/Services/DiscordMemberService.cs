﻿// This file is part of Lisbeth.Bot project
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
using Lisbeth.Bot.Application.Discord.Helpers;
using Lisbeth.Bot.DataAccessLayer.Specifications.Guild;
using Lisbeth.Bot.DataAccessLayer.Specifications.Mute;

namespace Lisbeth.Bot.Application.Discord.Services;

[UsedImplicitly]
[ServiceImplementation<IDiscordMemberService>(ServiceLifetime.InstancePerLifetimeScope)]
public class DiscordMemberService : IDiscordMemberService
{
    private readonly IDiscordService _discord;
    private readonly IDiscordEmbedProvider _embedProvider;
    private readonly IGuildDataService _guildDataService;
    private readonly IMuteDataService _muteDataService;

    public DiscordMemberService(IDiscordService discord, IGuildDataService guildDataService, IMuteDataService muteDataService,
        IDiscordEmbedProvider embedProvider)
    {
        _guildDataService = guildDataService;
        _muteDataService = muteDataService;
        _discord = discord;
        _embedProvider = embedProvider;
    }

    public async Task<Result> SendWelcomeMessageAsync(GuildMemberAddEventArgs args)
    {
        if (args is null) throw new ArgumentNullException(nameof(args));

        var result = await _guildDataService.GetSingleBySpecAsync<Guild>(
            new ActiveGuildByDiscordIdWithModerationSpec(args.Guild.Id));


        if (!result.IsDefined() || result.Entity.ModerationConfig?.BaseMemberWelcomeMessage is null)
            return Result.FromSuccess();

        var embed = new DiscordEmbedBuilder();
        if (result.Entity.ModerationConfig.MemberWelcomeEmbedConfig is not null)
        {
            embed = _embedProvider.GetEmbedFromConfig(result.Entity.ModerationConfig.MemberWelcomeEmbedConfig);
        }
        else
        {
            embed.WithColor(new DiscordColor(result.Entity.EmbedHexColor));
            embed.WithDescription(result.Entity.ModerationConfig.BaseMemberWelcomeMessage);
        }

        try
        {
            await args.Member.SendMessageAsync(embed.Build());
        }
        catch (Exception)
        {
            // probably should tell idiots to fix channel id in config but idk how so return for now, mebe msg members with admin privs
        }

        return Result.FromSuccess();
    }

    public async Task<Result> MemberMuteCheckAsync(GuildMemberAddEventArgs args)
    {
        if (args is null) throw new ArgumentNullException(nameof(args));

        var res = await _muteDataService.GetSingleBySpecAsync<Mute>(
            new ActiveMutesByGuildAndUserSpecifications(args.Guild.Id, args.Member.Id));

        if (!res.IsDefined() || res.Entity.Guild?.ModerationConfig is null)
            return Result.FromSuccess(); // no mod config enabled so we don't care

        if (!args.Guild.Roles.TryGetValue(res.Entity.Guild.ModerationConfig.MuteRoleId, out var role))
            return Result.FromSuccess();

        await args.Member.GrantRoleAsync(role);

        return Result.FromSuccess();
    }
}
