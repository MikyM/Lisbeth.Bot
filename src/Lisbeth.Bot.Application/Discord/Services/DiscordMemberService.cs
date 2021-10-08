﻿// This file is part of Lisbeth.Bot project
//
// Copyright (C) 2021 MikyM
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
using Lisbeth.Bot.Application.Discord.Extensions;
using Lisbeth.Bot.Application.Discord.Services.Interfaces;
using Lisbeth.Bot.Application.Services.Interfaces;
using Lisbeth.Bot.Domain.Entities;
using MikyM.Common.DataAccessLayer.Specifications;
using MikyM.Discord.Interfaces;
using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Lisbeth.Bot.DataAccessLayer.Specifications.MuteSpecifications;

namespace Lisbeth.Bot.Application.Discord.Services
{
    [UsedImplicitly]
    public class DiscordMemberService : IDiscordMemberService
    {
        private readonly IGuildService _guildService;
        private readonly IMuteService _muteService;
        private readonly IDiscordService _discord;

        public DiscordMemberService(IDiscordService discord, IGuildService guildService, IMuteService muteService)
        {
            _guildService = guildService;
            _muteService = muteService;
            _discord = discord;
        }

        public async Task LogMemberRemovedEventAsync(GuildMemberRemoveEventArgs args)
        {
            if (args is null) throw new ArgumentNullException(nameof(args));

            var res = await _guildService.GetBySpecificationsAsync<Guild>(
                new Specifications<Guild>(x => x.GuildId == args.Guild.Id && !x.IsDisabled));

            var guild = res.FirstOrDefault();

            if (guild?.ModerationConfig?.MemberEventsLogChannelId is null) return;

            DiscordChannel logChannel =
                args.Guild.Channels.FirstOrDefault(x => x.Key == guild.ModerationConfig.MemberEventsLogChannelId.Value).Value;

            if (logChannel is null) return;

            string reasonLeft = "No reason found";

            var auditLogsBans = await args.Guild.GetAuditLogsAsync(1, null, AuditLogActionType.Ban);
            await Task.Delay(500);
            var auditLogsKicks = await args.Guild.GetAuditLogsAsync(1, null, AuditLogActionType.Kick);
            var filtered = auditLogsBans.Concat(auditLogsKicks).Where(m => m.CreationTimestamp.LocalDateTime > DateTime.Now.Subtract(new TimeSpan(0, 0, 4))).ToList();

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
            embed.AddField("Joined guild", $"{args.Member.JoinedAt}");
            embed.AddField("Account created", $"{args.Member.CreationTimestamp}");
            embed.WithColor(new DiscordColor(guild.EmbedHexColor));
            embed.WithFooter($"Member ID: {args.Member.Id}");

            if (reasonLeft != "No reason found")
            {
                embed.AddField("Reason for leaving", reasonLeft);
            }

            try
            {
                await _discord.Client.SendMessageAsync(logChannel, embed.Build());
            }
            catch (Exception ex)
            {
                // probably should tell idiots to fix channel id in config but idk how so return for now, mebe msg members with admin privs
                return;
            }
        }

        public async Task SendWelcomeMessageAsync(GuildMemberAddEventArgs args)
        {
            if (args is null) throw new ArgumentNullException(nameof(args));

            var res = await _guildService.GetBySpecificationsAsync<Guild>(
                new Specifications<Guild>(x => x.GuildId == args.Guild.Id && !x.IsDisabled));

            var guild = res.FirstOrDefault();

            if (guild?.ModerationConfig?.MemberWelcomeMessage is null) return;

            var embed = new DiscordEmbedBuilder();
            embed.WithColor(new DiscordColor(guild.EmbedHexColor));
            if (guild.ModerationConfig.MemberWelcomeMessageTitle is not null) embed.WithTitle(guild.ModerationConfig.MemberWelcomeMessageTitle);

            var matches = Regex.Matches(guild.ModerationConfig.MemberWelcomeMessage, @"@field@(.+?)@endField@");
            int count = matches.Count > 25 ? 25 : matches.Count;


            for (int i = 0; i < count; i++)
            {
                var match = Regex.Match(matches[i].Groups[1].Value, @"@title@(.+?)@endTitle@");
                embed.AddField(match.Groups[1].Value, matches[i].Groups[1].Value);
            }

            try
            {
                await args.Member.SendMessageAsync(embed.Build());
            }
            catch (Exception ex)
            {
                // probably should tell idiots to fix channel id in config but idk how so return for now, mebe msg members with admin privs
                return;
            }
        }

        public async Task MemberMuteCheckAsync(GuildMemberAddEventArgs args)
        {
            if (args is null) throw new ArgumentNullException(nameof(args));

            var res = await _muteService.GetBySpecificationsAsync<Mute>(new ActiveMutesByGuildAndUserSpecifications(args.Guild.Id, args.Member.Id));

            var mute = res.FirstOrDefault();

            if (mute?.Guild.ModerationConfig is null) return; // no mod config enabled so we don't care

            var role = args.Guild.Roles.FirstOrDefault(x => x.Key == mute.Guild.ModerationConfig.MuteRoleId).Value;

            if (role is null) return;

            await args.Member.GrantRoleAsync(role);
        }
    }
}
