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

using System.Globalization;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using Lisbeth.Bot.Application.Discord.Helpers;
using Lisbeth.Bot.DataAccessLayer.Specifications.Guild;
using Lisbeth.Bot.DataAccessLayer.Specifications.Mute;
using MikyM.Common.Utilities.Results;
using MikyM.Discord.Extensions.BaseExtensions;
using MikyM.Discord.Interfaces;

namespace Lisbeth.Bot.Application.Discord.Services;

[UsedImplicitly]
[Service]
[RegisterAs(typeof(IDiscordMemberService))]
[Lifetime(Lifetime.InstancePerLifetimeScope)]
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

    public async Task<Result> LogMemberRemovedEventAsync(GuildMemberRemoveEventArgs args)
    {
        if (args is null) throw new ArgumentNullException(nameof(args));

        var res = await _guildDataService.GetSingleBySpecAsync(new ActiveGuildByDiscordIdWithModerationSpec(args.Guild.Id));

        if (!res.IsDefined(out var guildCfg) || guildCfg.ModerationConfig is null) return Result.FromSuccess();
        if (!args.Guild.Channels.TryGetValue(guildCfg.ModerationConfig.MemberEventsLogChannelId,
                out var logChannel)) return Result.FromSuccess();

        string reasonLeft = "No reason found";

        var auditLogsBans = await args.Guild.GetAuditLogsAsync(1, null, AuditLogActionType.Ban);
        await Task.Delay(500);
        var auditLogsKicks = await args.Guild.GetAuditLogsAsync(1, null, AuditLogActionType.Kick);
        var filtered = auditLogsBans.Concat(auditLogsKicks).Where(m =>
            m.CreationTimestamp.UtcDateTime > DateTime.UtcNow.Subtract(new TimeSpan(0, 0, 4))).ToList();

        var embed = new DiscordEmbedBuilder();

        if (filtered.Count != 0)
        {
            var auditLog = filtered[0];
            var logType = auditLog.ActionType;
            var userResponsible = auditLog.UserResponsible.Mention;
            reasonLeft = logType switch
            {
                AuditLogActionType.Ban =>
                    $"Banned by {userResponsible} {(string.IsNullOrEmpty(auditLog.Reason) ? "" : $"with reason: {auditLog.Reason}")}",
                AuditLogActionType.Kick =>
                    $"Kicked by {userResponsible} {(string.IsNullOrEmpty(auditLog.Reason) ? "" : $"with reason: {auditLog.Reason}")}",
                _ => reasonLeft
            };
        }

        embed.WithThumbnail(args.Member.AvatarUrl);
        embed.WithTitle("Member has left the guild");
        embed.AddField("Member's identity", $"{args.Member.GetFullUsername()}", true);
        embed.AddField("Member's mention", $"{args.Member.Mention}", true);
        embed.AddField("Joined guild", $"{args.Member.JoinedAt.ToString(CultureInfo.CurrentCulture)}");
        embed.AddField("Account created", $"{args.Member.CreationTimestamp.ToString(CultureInfo.CurrentCulture)}");
        embed.WithColor(new DiscordColor(res.Entity.EmbedHexColor));
        embed.WithFooter($"Member ID: {args.Member.Id}");

        if (reasonLeft != "No reason found") embed.AddField("Reason for leaving", reasonLeft);

        try
        {
            await _discord.Client.SendMessageAsync(logChannel, embed.Build());
        }
        catch (Exception)
        {
            // probably should tell idiots to fix channel id in config but idk how so return for now, mebe msg members with admin privs
        }

        return Result.FromSuccess();
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