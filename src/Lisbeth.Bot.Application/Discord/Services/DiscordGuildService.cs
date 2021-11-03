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

using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.SlashCommands;
using JetBrains.Annotations;
using Lisbeth.Bot.Application.Discord.Exceptions;
using Lisbeth.Bot.Application.Discord.Extensions;
using Lisbeth.Bot.Application.Discord.Helpers;
using Lisbeth.Bot.Application.Discord.Services.Interfaces;
using Lisbeth.Bot.Application.Enums;
using Lisbeth.Bot.Application.Exceptions;
using Lisbeth.Bot.Application.Services.Database.Interfaces;
using Lisbeth.Bot.DataAccessLayer.Specifications.Guild;
using Lisbeth.Bot.Domain.DTOs.Request.ModerationConfig;
using Lisbeth.Bot.Domain.DTOs.Request.TicketingConfig;
using Lisbeth.Bot.Domain.Entities;
using MikyM.Discord.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Lisbeth.Bot.Domain.DTOs.Request;
using MikyM.Common.DataAccessLayer.Specifications;

namespace Lisbeth.Bot.Application.Discord.Services
{
    [UsedImplicitly]
    public class DiscordGuildService : IDiscordGuildService
    {
        private readonly IEmbedConfigService _embedConfigService;
        private readonly IDiscordEmbedProvider _embedProvider;
        private readonly IGuildService _guildService;
        private readonly IDiscordService _discord;
        private readonly IMapper _mapper;

        public DiscordGuildService(IEmbedConfigService embedConfigService, IDiscordEmbedProvider embedProvider,
            IGuildService guildService, IDiscordService discord, IMapper mapper)
        {
            _embedConfigService = embedConfigService;
            _embedProvider = embedProvider;
            _guildService = guildService;
            _discord = discord;
            _mapper = mapper;
        }
        
        public async Task HandleGuildCreateAsync(GuildCreateEventArgs args)
        {
            var guild = await _guildService.GetSingleBySpecAsync<Guild>(new GuildByIdSpec(args.Guild.Id));

            if (guild is null)
            {
                await _guildService.AddAsync(new Guild {GuildId = args.Guild.Id, UserId = args.Guild.OwnerId}, true);
                var embed = await _embedConfigService.GetAsync<EmbedConfig>(1);
                await args.Guild.Owner.SendMessageAsync(_embedProvider.ConfigureEmbed(embed).Build());
            }
            else
            {
                _guildService.BeginUpdate(guild);
                guild.IsDisabled = false;
                await _guildService.CommitAsync();

                var embed = await _embedConfigService.GetAsync<EmbedConfig>(2);
                await args.Guild.Owner.SendMessageAsync(_embedProvider.ConfigureEmbed(embed).Build());
            }
        }

        public async Task HandleGuildDeleteAsync(GuildDeleteEventArgs args)
        {
            var guild = await _guildService.GetSingleBySpecAsync<Guild>(new GuildByIdSpec(args.Guild.Id));

            if (guild is not null) await _guildService.DisableAsync(guild, true);
        }


        public async Task<DiscordEmbed> CreateModuleAsync(TicketingConfigReqDto req)
        {
            if (req is null) throw new ArgumentNullException(nameof(req));

            DiscordGuild guild = await _discord.Client.GetGuildAsync(req.GuildId);

            return await this.CreateTicketingModuleAsync(guild, await guild.GetMemberAsync(req.RequestedOnBehalfOfId), req);
        }

        public async Task<DiscordEmbed> CreateModuleAsync(InteractionContext ctx, TicketingConfigReqDto req)
        {
            if (ctx is null) throw new ArgumentNullException(nameof(ctx));
            if (req is null) throw new ArgumentNullException(nameof(req));

            return await this.CreateTicketingModuleAsync(ctx.Guild, ctx.Member, req);
        }

        private async Task<DiscordEmbed> CreateTicketingModuleAsync(DiscordGuild guild, DiscordMember requestingMember, TicketingConfigReqDto req)
        {
            if (guild is null) throw new ArgumentNullException(nameof(guild));
            if (requestingMember is null) throw new ArgumentNullException(nameof(requestingMember));
            if (req is null) throw new ArgumentNullException(nameof(req));

            if (!requestingMember.IsAdmin()) throw new DiscordNotAuthorizedException();

            var everyoneDeny = new[] {new DiscordOverwriteBuilder(guild.EveryoneRole).Deny(Permissions.AccessChannels)};

            var openedCat = await guild.CreateChannelAsync("TICKETS", ChannelType.Category, null,
                "Category with opened tickets", null, null, everyoneDeny);
            var closedCat = await guild.CreateChannelAsync("TICKETS-ARCHIVE", ChannelType.Category, null,
                "Category with closed tickets", null, null, everyoneDeny);
            var ticketLogs = await guild.CreateTextChannelAsync("ticket-logs", closedCat, "Channel with ticket logs and transcripts",
                everyoneDeny);

            req.OpenedCategoryId = openedCat.Id;
            req.ClosedCategoryId = closedCat.Id;
            req.LogChannelId = ticketLogs.Id;
            var res = await _guildService.AddConfigAsync(req, true);
            if (res is null) throw new InvalidOperationException("Guild already has an enabled ticketing configuration");

            var embed = new DiscordEmbedBuilder();
            embed.WithColor(new DiscordColor(res.EmbedHexColor));
            embed.WithAuthor("Ticketing configuration");
            embed.WithDescription("Process completed successfully");
            embed.AddField("Opened ticket category", openedCat.Mention);
            embed.AddField("Closed ticket category", closedCat.Mention);
            embed.AddField("Ticket log channel", ticketLogs.Mention);
            embed.WithFooter($"Lisbeth configuration requested by {requestingMember.GetFullDisplayName()} | Id: {requestingMember.Id}");

            return embed.Build();
        }

        public async Task<DiscordEmbed> CreateModuleAsync(ModerationConfigReqDto req)
        {
            if (req is null) throw new ArgumentNullException(nameof(req));

            DiscordGuild guild = await _discord.Client.GetGuildAsync(req.GuildId);

            return await this.CreateModerationModuleAsync(guild, await guild.GetMemberAsync(req.RequestedOnBehalfOfId), req);
        }

        public async Task<DiscordEmbed> CreateModuleAsync(InteractionContext ctx, ModerationConfigReqDto req)
        {
            if (ctx is null) throw new ArgumentNullException(nameof(ctx));
            if (req is null) throw new ArgumentNullException(nameof(req));

            return await this.CreateModerationModuleAsync(ctx.Guild, ctx.Member, req);
        }

        private async Task<DiscordEmbed> CreateModerationModuleAsync(DiscordGuild guild, DiscordMember requestingMember, ModerationConfigReqDto req)
        {
            if (guild is null) throw new ArgumentNullException(nameof(guild));
            if (requestingMember is null) throw new ArgumentNullException(nameof(requestingMember));
            if (req is null) throw new ArgumentNullException(nameof(req));

            if (!requestingMember.IsAdmin()) throw new DiscordNotAuthorizedException();

            var everyoneDeny = new[] {new DiscordOverwriteBuilder(guild.EveryoneRole).Deny(Permissions.AccessChannels)};
            var moderationCat = await guild.CreateChannelAsync("MODERATION", ChannelType.Category, null,
                "Moderation category", null, null, everyoneDeny);
            var moderationChannelLog = await guild.CreateTextChannelAsync("moderation-logs", moderationCat, 
                "Category with moderation logs", everyoneDeny);
            var memberEventsLogChannel = await guild.CreateTextChannelAsync("member-logs", moderationCat,
                "Category with member logs", everyoneDeny);
            var messageEditLogChannel = await guild.CreateTextChannelAsync("edit-logs", moderationCat,
                "Category with message edit logs", everyoneDeny);
            var messageDeleteLogChannel = await guild.CreateTextChannelAsync("delete-logs", moderationCat,
                "Category with message delete logs", everyoneDeny);
            var mutedRole = await guild.CreateRoleAsync("Muted");

            await Task.Delay(300); // give discord a break

            _ = await this.CreateOverwritesForMutedRoleAsync(guild, mutedRole, requestingMember);

            req.MemberEventsLogChannelId = memberEventsLogChannel.Id;
            req.MessageDeletedEventsLogChannelId = messageDeleteLogChannel.Id;
            req.MessageUpdatedEventsLogChannelId = messageEditLogChannel.Id;
            req.ModerationLogChannelId = moderationChannelLog.Id;
            req.MuteRoleId = mutedRole.Id;
            var res = await _guildService.AddConfigAsync(req, true);
            if (res is null) throw new InvalidOperationException("Guild already has an enabled ticketing configuration");

            var embed = new DiscordEmbedBuilder();
            embed.WithColor(new DiscordColor(res.EmbedHexColor));
            embed.WithAuthor("Moderation configuration");
            embed.WithDescription("Process completed successfully");
            embed.AddField("Moderation category", moderationCat.Mention);
            embed.AddField("Moderation log channel", moderationChannelLog.Mention);
            embed.AddField("Member log channel", memberEventsLogChannel.Mention);
            embed.AddField("Message edited log channel", messageEditLogChannel.Mention);
            embed.AddField("Message deleted channel", messageDeleteLogChannel.Mention);
            embed.AddField("Muted role", mutedRole.Mention);
            embed.WithFooter($"Lisbeth configuration requested by {requestingMember.GetFullDisplayName()} | Id: {requestingMember.Id}");

            return embed.Build();
        }

        public async Task<DiscordEmbed> RepairConfigAsync([NotNull] InteractionContext ctx, [NotNull] ModerationConfigRepairReqDto req)
        {
            if (req is null) throw new ArgumentNullException(nameof(req));
            if (ctx is null) throw new ArgumentNullException(nameof(ctx));

            return await this.RepairConfigAsync(ctx.Guild, GuildConfigType.Moderation, ctx.Member);
        }

        public async Task<DiscordEmbed> RepairConfigAsync([NotNull] TicketingConfigRepairReqDto req)
        {
            if (req is null) throw new ArgumentNullException(nameof(req));

            DiscordGuild guild = await _discord.Client.GetGuildAsync(req.GuildId);

            return await this.RepairConfigAsync(guild, GuildConfigType.Ticketing, await guild.GetMemberAsync(req.RequestedOnBehalfOfId));
        }

        public async Task<DiscordEmbed> RepairConfigAsync([NotNull] ModerationConfigRepairReqDto req)
        {
            if (req is null) throw new ArgumentNullException(nameof(req));

            DiscordGuild guild = await _discord.Client.GetGuildAsync(req.GuildId);

            return await this.RepairConfigAsync(guild, GuildConfigType.Moderation, await guild.GetMemberAsync(req.RequestedOnBehalfOfId));
        }

        public async Task<DiscordEmbed> RepairConfigAsync([NotNull] InteractionContext ctx, [NotNull] TicketingConfigRepairReqDto req)
        {
            if (req is null) throw new ArgumentNullException(nameof(req));
            if (ctx is null) throw new ArgumentNullException(nameof(ctx));

            return await this.RepairConfigAsync(ctx.Guild, GuildConfigType.Ticketing, ctx.Member);
        }

        private async Task<DiscordEmbed> RepairConfigAsync([NotNull] DiscordGuild discordGuild, GuildConfigType type, DiscordMember requestingMember)
        {
            if (discordGuild is null) throw new ArgumentNullException(nameof(discordGuild));
            if (requestingMember is null) throw new ArgumentNullException(nameof(requestingMember));
            if (!requestingMember.IsAdmin()) throw new DiscordNotAuthorizedException();

            Guild guild;
            DiscordOverwriteBuilder[] everyoneDeny;

            var embed = new DiscordEmbedBuilder();
             
            switch (type)
            {
                case GuildConfigType.Ticketing:
                    guild = await _guildService.GetSingleBySpecAsync<Guild>(
                        new ActiveGuildByDiscordIdWithTicketingSpecifications(discordGuild.Id));
                    if (guild?.TicketingConfig is null)
                        throw new NotFoundException("Guild or ticketing config not found");
                    if (guild.TicketingConfig.IsDisabled)
                        throw new DisabledEntityException("First enable ticketing module.");

                    everyoneDeny = new[]
                    {
                        new DiscordOverwriteBuilder(discordGuild.EveryoneRole).Deny(Permissions.AccessChannels)
                    };

                    DiscordChannel newOpenedCat = null;
                    DiscordChannel newClosedCat = null;
                    DiscordChannel newTicketLogChannel = null;
                    DiscordChannel closedCat = null;

                    TicketingConfigRepairReqDto ticketingReq = new();

                    try
                    {
                        var openedCat = await _discord.Client.GetChannelAsync(guild.TicketingConfig.OpenedCategoryId);
                        if (openedCat is null) throw new DiscordNotFoundException();
                    }
                    catch
                    {
                        newOpenedCat = await discordGuild.CreateChannelAsync("TICKETS", ChannelType.Category, null,
                            "Category with opened tickets", null, null, everyoneDeny);
                        ticketingReq.OpenedCategoryId = newOpenedCat.Id;
                    }

                    try
                    {
                        closedCat = await _discord.Client.GetChannelAsync(guild.TicketingConfig.ClosedCategoryId);
                        if (closedCat is null) throw new DiscordNotFoundException();
                    }
                    catch
                    {
                        newClosedCat = await discordGuild.CreateChannelAsync("TICKETS-ARCHIVE", ChannelType.Category,
                            null, "Category with closed tickets", null, null, everyoneDeny);
                        ticketingReq.ClosedCategoryId = newClosedCat.Id;
                    }

                    try
                    {
                        var ticketLogs = await _discord.Client.GetChannelAsync(guild.TicketingConfig.LogChannelId);
                        if (ticketLogs is null) throw new DiscordNotFoundException();
                    }
                    catch
                    {
                        newTicketLogChannel = await discordGuild.CreateTextChannelAsync("ticket-logs",
                            closedCat ?? newClosedCat, "Channel with ticket logs and transcripts", everyoneDeny);
                        ticketingReq.LogChannelId = newTicketLogChannel.Id;
                    }

                    if (newOpenedCat is not null || newClosedCat is not null || newTicketLogChannel is not null)
                    {
                        ticketingReq.GuildId = guild.GuildId;
                        ticketingReq.RequestedOnBehalfOfId = requestingMember.Id;
                        await _guildService.RepairModuleConfigAsync(ticketingReq, true);

                        if (newOpenedCat is not null) embed.AddField("Opened ticket category", newOpenedCat.Mention);
                        if (newClosedCat is not null) embed.AddField("Closed ticket category", newClosedCat.Mention);
                        if (newTicketLogChannel is not null) embed.AddField("Ticket log channel", newTicketLogChannel.Mention);
                    }
                    else
                    {
                        embed.AddField("Result", "Nothing to repair");
                    }


                    break;
                case GuildConfigType.Moderation:
                    guild = await _guildService.GetSingleBySpecAsync<Guild>(
                        new ActiveGuildByDiscordIdWithModerationSpecifications(discordGuild.Id));

                    if (guild?.ModerationConfig is null)
                        throw new NotFoundException("Guild or ticketing config not found");
                    if (guild.ModerationConfig.IsDisabled)
                        throw new DisabledEntityException("First enable ticketing module.");

                    everyoneDeny = new[]
                    {
                        new DiscordOverwriteBuilder(discordGuild.EveryoneRole).Deny(Permissions.AccessChannels)
                    };

                    DiscordChannel newMemberEventsLogChannel = null;
                    DiscordChannel newMessageDeletedEventsLogChannel = null;
                    DiscordChannel newMessageUpdatedEventsLogChannel = null;
                    DiscordChannel newModerationLogChannel = null;
                    DiscordRole newMuteRole = null;

                    ModerationConfigRepairReqDto moderationReq = new();

                    var newModerationCat = discordGuild.Channels.FirstOrDefault(x =>
                            string.Equals(x.Value.Name.ToLower(), "moderation",
                                StringComparison.InvariantCultureIgnoreCase))
                        .Value ?? await discordGuild.CreateChannelAsync("MODERATION", ChannelType.Category, null,
                        "Moderation category", null, null, everyoneDeny);
                    try
                    {
                        var moderationChannelLog =
                            await _discord.Client.GetChannelAsync(guild.ModerationConfig.ModerationLogChannelId);
                        if (moderationChannelLog is null) throw new DiscordNotFoundException();
                    }
                    catch
                    {
                        newModerationLogChannel = await discordGuild.CreateTextChannelAsync("moderation-logs",
                            newModerationCat, "Channel with moderation logs", everyoneDeny);
                        moderationReq.ModerationLogChannelId = newModerationLogChannel.Id;
                    }

                    try
                    {
                        var memberEventsLogChannel =
                            await _discord.Client.GetChannelAsync(guild.ModerationConfig.MemberEventsLogChannelId);
                        if (memberEventsLogChannel is null) throw new DiscordNotFoundException();
                    }
                    catch
                    {
                        newMemberEventsLogChannel = await discordGuild.CreateTextChannelAsync("member-logs",
                            newModerationCat, "Channel with member logs", everyoneDeny);
                        moderationReq.MemberEventsLogChannelId = newMemberEventsLogChannel.Id;
                    }

                    try
                    {
                        var messageEditLogChannel =
                            await _discord.Client.GetChannelAsync(guild.ModerationConfig
                                .MessageUpdatedEventsLogChannelId);
                        if (messageEditLogChannel is null) throw new DiscordNotFoundException();
                    }
                    catch
                    {
                        newMessageUpdatedEventsLogChannel = await discordGuild.CreateTextChannelAsync("edit-logs",
                            newModerationCat, "Channel with edited message logs", everyoneDeny);
                        moderationReq.MessageUpdatedEventsLogChannelId = newMessageUpdatedEventsLogChannel.Id;
                    }

                    try
                    {
                        var messageDeleteLogChannel =
                            await _discord.Client.GetChannelAsync(guild.ModerationConfig
                                .MessageDeletedEventsLogChannelId);
                        if (messageDeleteLogChannel is null) throw new DiscordNotFoundException();
                    }
                    catch
                    {
                        newMessageDeletedEventsLogChannel = await discordGuild.CreateTextChannelAsync("delete-logs",
                            newModerationCat, "Channel with deleted message logs", everyoneDeny);
                        moderationReq.MessageDeletedEventsLogChannelId = newMessageDeletedEventsLogChannel.Id;
                    }

                    try
                    {
                        var mutedRole = discordGuild.GetRole(guild.ModerationConfig.MuteRoleId);
                        if (mutedRole is null) throw new DiscordNotFoundException();
                    }
                    catch
                    {
                        newMuteRole = await discordGuild.CreateRoleAsync("Muted");
                        moderationReq.MuteRoleId = newMuteRole.Id;

                        await Task.Delay(300); // give discord a break

                        _ = await this.CreateOverwritesForMutedRoleAsync(discordGuild, newMuteRole, requestingMember);
                    }

                    if (newMemberEventsLogChannel is not null || newMessageUpdatedEventsLogChannel is not null ||
                        newMessageDeletedEventsLogChannel is not null || newModerationLogChannel is not null ||
                        newMuteRole is not null)
                    {
                        moderationReq.GuildId = guild.GuildId;
                        moderationReq.RequestedOnBehalfOfId = requestingMember.Id;
                        await _guildService.RepairModuleConfigAsync(moderationReq, true);

                        embed.AddField("Moderation category", newModerationCat.Mention);
                        if (newModerationLogChannel is not null) embed.AddField("Moderation log channel", newModerationLogChannel.Mention);
                        if (newMemberEventsLogChannel is not null) embed.AddField("Member log channel", newMemberEventsLogChannel.Mention);
                        if (newMessageUpdatedEventsLogChannel is not null) embed.AddField("Message edited log channel", newMessageUpdatedEventsLogChannel.Mention);
                        if (newMessageDeletedEventsLogChannel is not null) embed.AddField("Message deleted channel", newMessageDeletedEventsLogChannel.Mention);
                        if (newMuteRole is not null) embed.AddField("Muted role", newMuteRole.Mention);
                    }
                    else
                    {
                        embed.AddField("Result", "Nothing to repair");
                    }

                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }

            embed.WithDescription("Process completed successfully");
            embed.WithColor(new DiscordColor(guild.EmbedHexColor));
            embed.WithAuthor($"{type} configuration repair");
            embed.WithFooter($"Lisbeth configuration requested by {requestingMember.GetFullDisplayName()} | Id: {requestingMember.Id}");

            return embed.Build();
        }

        public async Task<DiscordEmbed> DisableModuleAsync([NotNull] ModerationConfigDisableReqDto req)
        {
            if (req is null) throw new ArgumentNullException(nameof(req));

            DiscordGuild guild = await _discord.Client.GetGuildAsync(req.GuildId);

            return await this.DisableModuleAsync(guild, await guild.GetMemberAsync(req.RequestedOnBehalfOfId), GuildConfigType.Moderation);
        }

        public async Task<DiscordEmbed> DisableModuleAsync([NotNull] TicketingConfigDisableReqDto req)
        {
            if (req is null) throw new ArgumentNullException(nameof(req));

            DiscordGuild guild = await _discord.Client.GetGuildAsync(req.GuildId);

            return await this.DisableModuleAsync(guild, await guild.GetMemberAsync(req.RequestedOnBehalfOfId), GuildConfigType.Ticketing);
        }
        public async Task<DiscordEmbed> DisableModuleAsync([NotNull] InteractionContext ctx, [NotNull] ModerationConfigDisableReqDto req)
        {
            if (req is null) throw new ArgumentNullException(nameof(req));
            if (ctx is null) throw new ArgumentNullException(nameof(ctx));

            return await this.DisableModuleAsync(ctx.Guild, ctx.Member, GuildConfigType.Moderation);
        }
        public async Task<DiscordEmbed> DisableModuleAsync([NotNull] InteractionContext ctx, [NotNull] TicketingConfigDisableReqDto req)
        {
            if (req is null) throw new ArgumentNullException(nameof(req));
            if (ctx is null) throw new ArgumentNullException(nameof(ctx));

            return await this.DisableModuleAsync(ctx.Guild, ctx.Member, GuildConfigType.Ticketing);
        }

        private async Task<DiscordEmbed> DisableModuleAsync(DiscordGuild discordGuild, DiscordMember requestingMember, GuildConfigType type)
        {
            if (!requestingMember.IsAdmin()) throw new DiscordNotAuthorizedException();

            var guild = await _guildService.GetSingleBySpecAsync<Guild>(
                new ActiveGuildByDiscordIdWithTicketingSpecifications(discordGuild.Id));
            switch (type)
            {
                case GuildConfigType.Ticketing:
                    if (guild?.TicketingConfig is null)
                        throw new NotFoundException("Guild or ticketing config not found");
                    if (guild.TicketingConfig.IsDisabled)
                        throw new InvalidOperationException("Module already disabled.");

                    await _guildService.DisableConfigAsync(discordGuild.Id, GuildConfigType.Ticketing, true);
                    break;
                case GuildConfigType.Moderation:
                    if (guild?.ModerationConfig is null)
                        throw new NotFoundException("Guild or moderation config not found");
                    if (guild.ModerationConfig.IsDisabled)
                        throw new InvalidOperationException("Module already disabled.");

                    await _guildService.DisableConfigAsync(discordGuild.Id, GuildConfigType.Moderation, true);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }

            return new DiscordEmbedBuilder().WithAuthor("Guild configurator")
                .WithDescription("Module disabled successfully").WithColor(new DiscordColor(guild.EmbedHexColor))
                .Build();
        }

        public async Task<int> CreateOverwritesForMutedRoleAsync([NotNull] CreateMuteOverwritesReqDto req)
        {
            if (req is null) throw new ArgumentNullException(nameof(req));

            var guild = await _guildService.GetSingleBySpecAsync<Guild>(
                new ActiveGuildByDiscordIdWithModerationSpecifications(req.GuildId));

            if (guild?.ModerationConfig is null) throw new NotFoundException();
            if (guild.ModerationConfig.IsDisabled) throw new DisabledEntityException();

            var discordGuild = await _discord.Client.GetGuildAsync(req.GuildId);

            return await this.CreateOverwritesForMutedRoleAsync(discordGuild, discordGuild.Roles[guild.ModerationConfig.MuteRoleId], await discordGuild.GetMemberAsync(req.RequestedOnBehalfOfId));
        }

        public async Task<int> CreateOverwritesForMutedRoleAsync([NotNull] InteractionContext ctx, [NotNull] CreateMuteOverwritesReqDto req)
        {
            if (ctx is null) throw new ArgumentNullException(nameof(ctx));
            if (req is null) throw new ArgumentNullException(nameof(req));

            var guild = await _guildService.GetSingleBySpecAsync<Guild>(
                new ActiveGuildByDiscordIdWithModerationSpecifications(req.GuildId));

            if (guild?.ModerationConfig is null) throw new NotFoundException();
            if (guild.ModerationConfig.IsDisabled) throw new DisabledEntityException();

            return await this.CreateOverwritesForMutedRoleAsync(ctx.Guild, ctx.Guild.Roles[guild.ModerationConfig.MuteRoleId], ctx.Member);
        }

        private async Task<int> CreateOverwritesForMutedRoleAsync([NotNull] DiscordGuild discordGuild,
            [NotNull] DiscordRole mutedRole, [NotNull] DiscordMember requestingMember)
        {
            if (discordGuild is null) throw new ArgumentNullException(nameof(discordGuild));
            if (mutedRole is null) throw new ArgumentNullException(nameof(mutedRole));
            if (requestingMember is null) throw new ArgumentNullException(nameof(requestingMember));

            if (!requestingMember.IsAdmin()) throw new DiscordNotAuthorizedException();

            int count = 0;
            foreach (var channel in discordGuild.Channels.Values.Where(x => x.Type is ChannelType.Category or ChannelType.Text))
            {
                await channel.AddOverwriteAsync(mutedRole,
                    deny: Permissions.SendMessages | Permissions.SendMessagesInThreads |
                          Permissions.AddReactions | Permissions.CreatePrivateThreads |
                          Permissions.CreatePublicThreads);
                await Task.Delay(500);

                count++;
            }

            return count;
        }
    }
}