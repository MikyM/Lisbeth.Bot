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
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using JetBrains.Annotations;
using Lisbeth.Bot.Application.Discord.Extensions;
using Lisbeth.Bot.Application.Discord.Helpers;
using Lisbeth.Bot.Application.Discord.Services.Interfaces;
using Lisbeth.Bot.Application.Services.Database.Interfaces;
using Lisbeth.Bot.DataAccessLayer.Specifications.Guild;
using Lisbeth.Bot.DataAccessLayer.Specifications.Mute;
using Lisbeth.Bot.Domain.Entities;
using MikyM.Common.DataAccessLayer.Specifications;
using MikyM.Discord.Interfaces;

namespace Lisbeth.Bot.Application.Discord.Services
{
    [UsedImplicitly]
    public class DiscordMemberService : IDiscordMemberService
    {
        private readonly IDiscordService _discord;
        private readonly IDiscordEmbedProvider _embedProvider;
        private readonly IGuildService _guildService;
        private readonly IMuteService _muteService;

        public DiscordMemberService(IDiscordService discord, IGuildService guildService, IMuteService muteService,
            IDiscordEmbedProvider embedProvider)
        {
            _guildService = guildService;
            _muteService = muteService;
            _discord = discord;
            _embedProvider = embedProvider;
        }

        public async Task LogMemberRemovedEventAsync(GuildMemberRemoveEventArgs args)
        {
            if (args is null) throw new ArgumentNullException(nameof(args));

            var res = await _guildService.GetBySpecAsync<Guild>(
                new Specification<Guild>(x => x.GuildId == args.Guild.Id && !x.IsDisabled));

            var guild = res.FirstOrDefault();

            if (guild?.ModerationConfig?.MemberEventsLogChannelId is null) return;

            DiscordChannel logChannel =
                args.Guild.Channels.FirstOrDefault(x => x.Key == guild.ModerationConfig.MemberEventsLogChannelId)
                    .Value;

            if (logChannel is null) return;

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
            embed.AddField("Joined guild", $"{args.Member.JoinedAt}");
            embed.AddField("Account created", $"{args.Member.CreationTimestamp}");
            embed.WithColor(new DiscordColor(guild.EmbedHexColor));
            embed.WithFooter($"Member ID: {args.Member.Id}");

            if (reasonLeft != "No reason found") embed.AddField("Reason for leaving", reasonLeft);

            try
            {
                await _discord.Client.SendMessageAsync(logChannel, embed.Build());
            }
            catch (Exception ex)
            {
                // probably should tell idiots to fix channel id in config but idk how so return for now, mebe msg members with admin privs
            }
        }

        public async Task SendWelcomeMessageAsync(GuildMemberAddEventArgs args)
        {
            if (args is null) throw new ArgumentNullException(nameof(args));

            var guild = await _guildService.GetSingleBySpecAsync<Guild>(
                new ActiveGuildByDiscordIdWithModerationSpecifications(args.Guild.Id));

            if (guild?.ModerationConfig?.BaseMemberWelcomeMessage is null) return;

            var embed = new DiscordEmbedBuilder();
            if (guild.ModerationConfig.MemberWelcomeEmbedConfig is not null)
            {
                embed = _embedProvider.ConfigureEmbed(guild.ModerationConfig.MemberWelcomeEmbedConfig);
            }
            else
            {
                embed.WithColor(new DiscordColor(guild.EmbedHexColor));
                embed.WithDescription(guild.ModerationConfig.BaseMemberWelcomeMessage);
            }

            try
            {
                await args.Member.SendMessageAsync(embed.Build());
            }
            catch (Exception ex)
            {
                // probably should tell idiots to fix channel id in config but idk how so return for now, mebe msg members with admin privs
            }
        }

        public async Task MemberMuteCheckAsync(GuildMemberAddEventArgs args)
        {
            if (args is null) throw new ArgumentNullException(nameof(args));

            var res = await _muteService.GetBySpecAsync<Mute>(
                new ActiveMutesByGuildAndUserSpecifications(args.Guild.Id, args.Member.Id));

            var mute = res.FirstOrDefault();

            if (mute?.Guild.ModerationConfig is null) return; // no mod config enabled so we don't care

            var role = args.Guild.Roles.FirstOrDefault(x => x.Key == mute.Guild.ModerationConfig.MuteRoleId).Value;

            if (role is null) return;

            await args.Member.GrantRoleAsync(role);
        }
    }
}