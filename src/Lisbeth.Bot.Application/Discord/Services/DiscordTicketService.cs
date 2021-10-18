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
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using JetBrains.Annotations;
using Lisbeth.Bot.Application.Discord.ChatExport;
using Lisbeth.Bot.Application.Discord.Exceptions;
using Lisbeth.Bot.Application.Discord.Extensions;
using Lisbeth.Bot.Application.Discord.Services.Interfaces;
using Lisbeth.Bot.Application.Services.Interfaces;
using Lisbeth.Bot.DataAccessLayer.Specifications.GuildSpecifications;
using Lisbeth.Bot.DataAccessLayer.Specifications.TicketSpecifications;
using Lisbeth.Bot.Domain.DTOs.Request;
using Lisbeth.Bot.Domain.Entities;
using Microsoft.Extensions.Logging;
using MikyM.Discord.Interfaces;

namespace Lisbeth.Bot.Application.Discord.Services
{
    [UsedImplicitly]
    public class DiscordTicketService : IDiscordTicketService
    {
        private readonly IDiscordChatExportService _chatExportService;
        private readonly IDiscordService _discord;
        private readonly IGuildService _guildService;
        private readonly ILogger<DiscordTicketService> _logger;
        private readonly ITicketService _ticketService;

        public DiscordTicketService(IDiscordService discord, ITicketService ticketService, IGuildService guildService,
            IDiscordChatExportService chatExportService, ILogger<DiscordTicketService> logger)
        {
            _discord = discord;
            _ticketService = ticketService;
            _chatExportService = chatExportService;
            _guildService = guildService;
            _logger = logger;
        }

        public async Task Test()
        {
            await Task.Delay(5000);
        }

        public async Task<DiscordMessageBuilder> CloseTicketAsync(TicketCloseReqDto req)
        {
            if (req is null) throw new ArgumentNullException(nameof(req));

            DiscordChannel target = null;
            DiscordMember requestingMember;
            Ticket ticket;

            if (req.Id is null && req.ChannelId is null && (req.OwnerId is null || req.GuildId is null) &&
                (req.GuildSpecificId is null || req.GuildId is null))
                throw new ArgumentException(
                    "You must supply either a ticket Id or a channel Id or a user Id and a guild Id or a guild specific ticket Id and guild Id.");

            if (req.Id is not null)
            {
                ticket = await _ticketService.GetAsync<Ticket>(req.Id.Value);
                if (ticket is null)
                    throw new ArgumentException(
                        $"Ticket with Id: {req.Id} doesn't exist in the database.");

                req.ChannelId = ticket.ChannelId;
                req.GuildId = ticket.GuildId;
            }
            else if (req.OwnerId is not null && req.GuildId is not null)
            {
                var res = await _ticketService.GetBySpecificationsAsync<Ticket>(
                    new TicketBaseGetSpecifications(null, req.OwnerId, req.GuildId, null, null, false, 1));
                ticket = res.FirstOrDefault();
                if (ticket is null)
                    throw new ArgumentException(
                        $"Opened ticket in guild with Id: {req.GuildId} and owner with Id: {req.OwnerId} doesn't exist in the database.");

                req.ChannelId = ticket.ChannelId;
                req.GuildId = ticket.GuildId;
            }
            else if (req.GuildSpecificId is not null && req.GuildId is not null)
            {
                var res = await _ticketService.GetBySpecificationsAsync<Ticket>(
                    new TicketBaseGetSpecifications(null, null, req.GuildId, null, req.GuildSpecificId, false, 1));
                ticket = res.FirstOrDefault();
                if (ticket is null)
                    throw new ArgumentException(
                        $"Opened ticket in guild with Id: {req.GuildId} and owner with Id: {req.OwnerId} doesn't exist in the database.");

                req.ChannelId = ticket.ChannelId;
                req.GuildId = ticket.GuildId;
            }
            else
            {
                try
                {
                    target = await _discord.Client.GetChannelAsync(req.ChannelId.Value);
                }
                catch (Exception ex)
                {
                    throw new DiscordNotFoundException(
                        $"User with Id: {req.ChannelId} doesn't exist or isn't this guild's member.", ex);
                }
            }

            target ??= await _discord.Client.GetChannelAsync(req.ChannelId.Value);

            var guild = target.Guild;

            try
            {
                requestingMember = await guild.GetMemberAsync(req.RequestedById);
            }
            catch (Exception ex)
            {
                throw new DiscordNotFoundException(
                    $"User with Id: {req.RequestedById} doesn't exist or isn't this guild's member.", ex);
            }

            return await CloseTicketAsync(guild, target, requestingMember, req);
        }

        public async Task<DiscordMessageBuilder> CloseTicketAsync(DiscordInteraction intr)
        {
            if (intr is null) throw new ArgumentNullException(nameof(intr));

            return await CloseTicketAsync(intr.Guild, intr.Channel, (DiscordMember) intr.User);
        }

        public async Task<DiscordMessageBuilder> OpenTicketAsync(TicketOpenReqDto req)
        {
            if (req is null) throw new ArgumentNullException(nameof(req));

            DiscordMember owner;
            DiscordGuild guild;

            try
            {
                guild = await _discord.Client.GetGuildAsync(req.GuildId);
            }
            catch (Exception ex)
            {
                throw new DiscordNotFoundException($"Guild with Id: {req.GuildId} doesn't exist", ex);
            }

            try
            {
                owner = await guild.GetMemberAsync(req.OwnerId);
            }
            catch (Exception ex)
            {
                throw new DiscordNotFoundException(
                    $"Member with Id: {req.OwnerId} doesn't exist or isn't the guilds member.", ex);
            }

            return await OpenTicketAsync(guild, owner, req);
        }

        public async Task<DiscordMessageBuilder> OpenTicketAsync(DiscordInteraction intr)
        {
            if (intr is null) throw new ArgumentNullException(nameof(intr));

            return await OpenTicketAsync(intr.Guild, (DiscordMember) intr.User, null, intr);
        }

        public async Task<DiscordMessageBuilder> ReopenTicketAsync(TicketReopenReqDto req)
        {
            if (req is null) throw new ArgumentNullException(nameof(req));

            DiscordChannel target = null;
            DiscordMember requestingMember;
            Ticket ticket;

            if (req.Id is null && req.ChannelId is null && (req.OwnerId is null || req.GuildId is null) &&
                (req.GuildSpecificId is null || req.GuildId is null))
                throw new ArgumentException(
                    "You must supply either a ticket Id or a channel Id or a user Id and a guild Id or a guild specific ticket Id and guild Id.");

            if (req.Id is not null)
            {
                ticket = await _ticketService.GetAsync<Ticket>(req.Id.Value);
                if (ticket is null)
                    throw new ArgumentException(
                        $"Ticket with Id: {req.Id} doesn't exist in the database.");

                req.ChannelId = ticket.ChannelId;
                req.GuildId = ticket.GuildId;
            }
            else if (req.OwnerId is not null && req.GuildId is not null)
            {
                var res = await _ticketService.GetBySpecificationsAsync<Ticket>(
                    new TicketBaseGetSpecifications(null, req.OwnerId, req.GuildId, null, null, true, 1));
                ticket = res.FirstOrDefault();
                if (ticket is null)
                    throw new ArgumentException(
                        $"Closed ticket in guild with Id: {req.GuildId} and owner with Id: {req.OwnerId} doesn't exist in the database.");

                req.ChannelId = ticket.ChannelId;
                req.GuildId = ticket.GuildId;
            }
            else if (req.GuildSpecificId is not null && req.GuildId is not null)
            {
                var res = await _ticketService.GetBySpecificationsAsync<Ticket>(
                    new TicketBaseGetSpecifications(null, null, req.GuildId, null, req.GuildSpecificId, true, 1));
                ticket = res.FirstOrDefault();
                if (ticket is null)
                    throw new ArgumentException(
                        $"Closed ticket in guild with Id: {req.GuildId} and owner with Id: {req.OwnerId} doesn't exist in the database.");

                req.ChannelId = ticket.ChannelId;
                req.GuildId = ticket.GuildId;
            }
            else
            {
                try
                {
                    target = await _discord.Client.GetChannelAsync(req.ChannelId.Value);
                }
                catch (Exception ex)
                {
                    throw new DiscordNotFoundException(
                        $"User with Id: {req.ChannelId} doesn't exist or isn't this guild's member.", ex);
                }
            }

            target ??= await _discord.Client.GetChannelAsync(req.ChannelId.Value);

            var guild = target.Guild;

            try
            {
                requestingMember = await guild.GetMemberAsync(req.RequestedById);
            }
            catch (Exception ex)
            {
                throw new DiscordNotFoundException(
                    $"User with Id: {req.RequestedById} doesn't exist or isn't this guild's target.", ex);
            }

            return await ReopenTicketAsync(guild, target, requestingMember, req);
        }

        public async Task<DiscordMessageBuilder> ReopenTicketAsync(DiscordInteraction intr)
        {
            if (intr is null) throw new ArgumentNullException(nameof(intr));

            return await ReopenTicketAsync(intr.Guild, intr.Channel, (DiscordMember) intr.User);
        }

        public async Task<DiscordEmbed> AddToTicketAsync(TicketAddReqDto req)
        {
            if (req is null) throw new ArgumentNullException(nameof(req));

            DiscordMember targetMember = null;
            DiscordRole targetRole = null;
            DiscordMember requestingMember;
            DiscordChannel targetTicketChannel = null;
            Ticket ticket;

            if (req.Id is null && req.ChannelId is null && (req.OwnerId is null || req.GuildId is null) &&
                (req.GuildSpecificId is null || req.GuildId is null))
                throw new ArgumentException(
                    "You must supply either a ticket Id or a channel Id or a user Id and a guild Id or a guild specific ticket Id and guild Id.");

            if (req.Id is not null)
            {
                ticket = await _ticketService.GetAsync<Ticket>(req.Id.Value);
                if (ticket is null)
                    throw new ArgumentException(
                        $"Ticket with Id: {req.Id} doesn't exist in the database.");

                req.ChannelId = ticket.ChannelId;
                req.GuildId = ticket.GuildId;
            }
            else if (req.OwnerId is not null && req.GuildId is not null)
            {
                var res = await _ticketService.GetBySpecificationsAsync<Ticket>(
                    new TicketBaseGetSpecifications(null, req.OwnerId, req.GuildId, null, null, false, 1));
                ticket = res.FirstOrDefault();
                if (ticket is null)
                    throw new ArgumentException(
                        $"Opened ticket in guild with Id: {req.GuildId} and owner with Id: {req.OwnerId} doesn't exist in the database.");

                req.ChannelId = ticket.ChannelId;
                req.GuildId = ticket.GuildId;
            }
            else if (req.GuildSpecificId is not null && req.GuildId is not null)
            {
                var res = await _ticketService.GetBySpecificationsAsync<Ticket>(
                    new TicketBaseGetSpecifications(null, null, req.GuildId, null, req.GuildSpecificId, false, 1));
                ticket = res.FirstOrDefault();
                if (ticket is null)
                    throw new ArgumentException(
                        $"Opened ticket in guild with Id: {req.GuildId} and owner with Id: {req.OwnerId} doesn't exist in the database.");

                req.ChannelId = ticket.ChannelId;
                req.GuildId = ticket.GuildId;
            }
            else
            {
                try
                {
                    targetTicketChannel = await _discord.Client.GetChannelAsync(req.ChannelId.Value);
                }
                catch (Exception ex)
                {
                    throw new DiscordNotFoundException(
                        $"User with Id: {req.ChannelId} doesn't exist or isn't this guild's member.", ex);
                }
            }

            targetTicketChannel ??= await _discord.Client.GetChannelAsync(req.ChannelId.Value);

            var guild = targetTicketChannel.Guild;

            try
            {
                requestingMember = await guild.GetMemberAsync(req.RequestedById);
            }
            catch (Exception ex)
            {
                throw new DiscordNotFoundException(
                    $"User with Id: {req.RequestedById} doesn't exist or isn't this guild's member.", ex);
            }

            try
            {
                targetMember = await guild.GetMemberAsync(req.SnowflakeId);
            }
            catch (Exception)
            {
                // means user doesn't exist in the guild, at all, or we're targeting a role
                try
                {
                    targetRole = guild.GetRole(req.SnowflakeId);
                }
                catch (Exception ex)
                {
                    throw new DiscordNotFoundException(
                        $"Role or member with Id: {req.SnowflakeId} doesn't exist or isn't in this guild.", ex);
                }
            }

            return targetRole is null
                ? await AddToTicketAsync(guild, requestingMember, targetTicketChannel, targetMember, null, req)
                : await AddToTicketAsync(guild, requestingMember, targetTicketChannel, null, targetRole, req);
        }

        public async Task<DiscordEmbed> AddToTicketAsync(InteractionContext ctx)
        {
            if (ctx is null) throw new ArgumentNullException(nameof(ctx));

            return await AddToTicketAsync(ctx.Guild, ctx.Member, ctx.Channel,
                (DiscordMember) ctx.ResolvedUserMentions?[0], ctx.ResolvedRoleMentions?[0]);
        }

        public async Task<DiscordEmbed> RemoveFromTicketAsync(TicketRemoveReqDto req)
        {
            if (req is null) throw new ArgumentNullException(nameof(req));

            DiscordMember targetMember = null;
            DiscordRole targetRole = null;
            DiscordMember requestingMember;
            DiscordChannel targetTicketChannel = null;
            Ticket ticket;

            if (req.Id is null && req.ChannelId is null && (req.OwnerId is null || req.GuildId is null) &&
                (req.GuildSpecificId is null || req.GuildId is null))
                throw new ArgumentException(
                    "You must supply either a ticket Id or a channel Id or a user Id and a guild Id or a guild specific ticket Id and guild Id.");

            if (req.Id is not null)
            {
                ticket = await _ticketService.GetAsync<Ticket>(req.Id.Value);
                if (ticket is null)
                    throw new ArgumentException(
                        $"Ticket with Id: {req.Id} doesn't exist in the database.");

                req.ChannelId = ticket.ChannelId;
                req.GuildId = ticket.GuildId;
            }
            else if (req.OwnerId is not null && req.GuildId is not null)
            {
                var res = await _ticketService.GetBySpecificationsAsync<Ticket>(
                    new TicketBaseGetSpecifications(null, req.OwnerId, req.GuildId, null, null, false, 1));
                ticket = res.FirstOrDefault();
                if (ticket is null)
                    throw new ArgumentException(
                        $"Opened ticket in guild with Id: {req.GuildId} and owner with Id: {req.OwnerId} doesn't exist in the database.");

                req.ChannelId = ticket.ChannelId;
                req.GuildId = ticket.GuildId;
            }
            else if (req.GuildSpecificId is not null && req.GuildId is not null)
            {
                var res = await _ticketService.GetBySpecificationsAsync<Ticket>(
                    new TicketBaseGetSpecifications(null, null, req.GuildId, null, req.GuildSpecificId, false, 1));
                ticket = res.FirstOrDefault();
                if (ticket is null)
                    throw new ArgumentException(
                        $"Opened ticket in guild with Id: {req.GuildId} and guild specific Id: {req.GuildSpecificId} doesn't exist in the database.");

                req.ChannelId = ticket.ChannelId;
                req.GuildId = ticket.GuildId;
            }
            else
            {
                try
                {
                    targetTicketChannel = await _discord.Client.GetChannelAsync(req.ChannelId.Value);
                }
                catch (Exception ex)
                {
                    throw new DiscordNotFoundException(
                        $"User with Id: {req.ChannelId} doesn't exist or isn't this guild's member.", ex);
                }
            }

            targetTicketChannel ??= await _discord.Client.GetChannelAsync(req.ChannelId.Value);

            var guild = targetTicketChannel.Guild;

            try
            {
                requestingMember = await guild.GetMemberAsync(req.RequestedById);
            }
            catch (Exception ex)
            {
                throw new DiscordNotFoundException(
                    $"User with Id: {req.RequestedById} doesn't exist or isn't this guild's member.", ex);
            }

            try
            {
                targetMember = await guild.GetMemberAsync(req.SnowflakeId);
            }
            catch (Exception)
            {
                // means user doesn't exist in the guild, at all, or we're targeting a role
                try
                {
                    targetRole = guild.GetRole(req.SnowflakeId);
                }
                catch (Exception ex)
                {
                    throw new DiscordNotFoundException(
                        $"Role or member with Id: {req.SnowflakeId} doesn't exist or isn't in this guild.", ex);
                }
            }

            return targetRole is null
                ? await RemoveFromTicketAsync(guild, requestingMember, targetTicketChannel, targetMember, null, req)
                : await RemoveFromTicketAsync(guild, requestingMember, targetTicketChannel, null, targetRole, req);
        }

        public async Task<DiscordEmbed> RemoveFromTicketAsync(InteractionContext ctx)
        {
            if (ctx is null) throw new ArgumentNullException(nameof(ctx));

            return await RemoveFromTicketAsync(ctx.Guild, ctx.Member, ctx.Channel,
                (DiscordMember) ctx.ResolvedUserMentions?[0], ctx.ResolvedRoleMentions?[0]);
        }

        public async Task CleanClosedTicketsAsync()
        {
            try
            {
                foreach (var (guildId, _) in _discord.Client.Guilds)
                {
                    var res = await _guildService.GetBySpecificationsAsync<Guild>(
                        new ActiveGuildByDiscordIdWithTicketingAndInactiveTicketsSpecifications(guildId));
                    var guildCfg = res.FirstOrDefault();

                    if (guildCfg?.TicketingConfig?.CleanAfter is null) continue;
                    if (guildCfg.Tickets.Count == 0) continue;

                    DiscordChannel closedCat;
                    try
                    {
                        closedCat = await _discord.Client.GetChannelAsync(guildCfg.TicketingConfig.ClosedCategoryId);
                    }
                    catch (Exception)
                    {
                        _logger.LogInformation(
                            $"Guild with Id: {guildId} has non-existing closed ticket category set with Id: {guildCfg.TicketingConfig.ClosedCategoryId}.");
                        continue;
                    }

                    foreach (var closedTicketChannel in closedCat.Children)
                    {
                        if (guildCfg.Tickets.All(x => x.ChannelId != closedTicketChannel.Id)) continue;

                        var lastMessage = await closedTicketChannel.GetMessagesAsync(1);
                        if (lastMessage is null || lastMessage.Count == 0) continue;

                        var timeDifference = DateTime.Now.Subtract(lastMessage[0].Timestamp.LocalDateTime);
                        if (timeDifference.TotalHours >= guildCfg.TicketingConfig.CleanAfter.Value.Hours)
                            await closedTicketChannel.DeleteAsync();

                        await Task.Delay(500);
                    }

                    await Task.Delay(500);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Something went wrong with cleaning closed tickets: {ex}");
            }
        }

        public async Task CloseInactiveTicketsAsync()
        {
            try
            {
                foreach (var (guildId, guild) in _discord.Client.Guilds)
                {
                    var res = await _guildService.GetBySpecificationsAsync<Guild>(
                        new ActiveGuildByDiscordIdWithTicketingAndTicketsSpecifications(guildId));
                    var guildCfg = res.FirstOrDefault();

                    if (guildCfg?.TicketingConfig?.CloseAfter is null) continue;
                    if (guildCfg.Tickets.Count == 0) continue;

                    DiscordChannel openedCat;
                    try
                    {
                        openedCat = await _discord.Client.GetChannelAsync(guildCfg.TicketingConfig.OpenedCategoryId);
                    }
                    catch (Exception)
                    {
                        _logger.LogInformation(
                            $"Guild with Id: {guildId} has non-existing opened ticket category set with Id: {guildCfg.TicketingConfig.OpenedCategoryId}.");
                        continue;
                    }

                    foreach (var openedTicketChannel in openedCat.Children)
                    {
                        if (guildCfg.Tickets.All(x => x.ChannelId != openedTicketChannel.Id)) continue;

                        var lastMessage = await openedTicketChannel.GetMessagesAsync(1);
                        var msg = lastMessage?.FirstOrDefault();
                        if (msg is null) continue;

                        if (!((DiscordMember) msg.Author).Permissions.HasPermission(Permissions.BanMembers)) continue;

                        var timeDifference = DateTime.Now.Subtract(msg.Timestamp.LocalDateTime);
                        if (timeDifference.TotalHours >= guildCfg.TicketingConfig.CloseAfter.Value.Hours)
                            await CloseTicketAsync(guild, openedTicketChannel,
                                (DiscordMember) _discord.Client.CurrentUser);

                        await Task.Delay(500);
                    }

                    await Task.Delay(500);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Something went wrong with closing inactive tickets: {ex}");
            }
        }

        private async Task<DiscordMessageBuilder> CloseTicketAsync(DiscordGuild guild, DiscordChannel target,
            DiscordMember requestingMember, TicketCloseReqDto req = null)
        {
            if (guild is null) throw new ArgumentNullException(nameof(guild));
            if (target is null) throw new ArgumentNullException(nameof(target));
            if (requestingMember is null) throw new ArgumentNullException(nameof(requestingMember));

            if (requestingMember.Guild.Id != guild.Id) throw new ArgumentException(nameof(requestingMember));
            if (target.Guild.Id != guild.Id) throw new ArgumentException(nameof(target));

            var guildRes =
                await _guildService.GetBySpecificationsAsync<Guild>(
                    new ActiveGuildByDiscordIdWithTicketingSpecifications(guild.Id));
            var guildCfg = guildRes.FirstOrDefault();

            if (guildCfg is null)
                throw new ArgumentException($"Guild with Id:{guild.Id} doesn't exist in the database.");

            if (guildCfg.TicketingConfig is null)
                throw new ArgumentException($"Guild with Id:{guild.Id} doesn't have ticketing enabled.");

            req ??= new TicketCloseReqDto(null, null, null, target.Id, requestingMember.Id);

            var res = await _ticketService.GetBySpecificationsAsync<Ticket>(
                new TicketBaseGetSpecifications(req.Id, req.OwnerId, req.GuildId, req.ChannelId, req.GuildSpecificId));

            var ticket = res.FirstOrDefault();

            if (ticket is null) throw new ArgumentException($"Ticket with channel Id: {target.Id} doesn't exist.");

            if (ticket.IsDisabled)
                throw new ArgumentException(
                    $"Ticket with Id: {ticket.GuildSpecificId}, TargetUserId: {ticket.UserId}, GuildId: {ticket.GuildId}, ChannelId: {ticket.ChannelId} is already closed.");

            if (ticket.UserId != requestingMember.Id ||
                !requestingMember.Permissions.HasPermission(Permissions.BanMembers))
                throw new DiscordNotAuthorizedException(
                    "Requesting member doesn't have moderator rights or isn't the ticket's owner.");

            var embed = new DiscordEmbedBuilder();

            embed.WithColor(new DiscordColor(guildCfg.EmbedHexColor));
            embed.WithAuthor("Ticket closed");
            embed.AddField("Moderator", requestingMember.Mention);
            embed.WithFooter($"Ticket Id: {ticket.GuildSpecificId}");

            var reopenBtn = new DiscordButtonComponent(ButtonStyle.Primary, "ticket_reopen_btn",
                "Reopen this ticket");
            var saveTransBtn = new DiscordButtonComponent(ButtonStyle.Primary, "ticket_save_trans_btn",
                "Generate transcript");

            var msgBuilder = new DiscordMessageBuilder();
            msgBuilder.AddEmbed(embed.Build());
            msgBuilder.AddComponents(new List<DiscordComponent> {reopenBtn, saveTransBtn});

            DiscordMessage closeMsg;
            try
            {
                closeMsg = await target.SendMessageAsync(msgBuilder);
            }
            catch (Exception)
            {
                throw new ArgumentException($"Couldn't send ticket close message in channel with Id: {target.Id}");
            }

            DiscordMember owner;
            try
            {
                owner = requestingMember.Id == ticket.UserId
                    ? requestingMember // means requested by owner so we don't need to grab the owner again
                    : await guild.GetMemberAsync(ticket.UserId);

                await target.AddOverwriteAsync(owner, deny: Permissions.AccessChannels);
            }
            catch
            {
                owner = null;
                // ignored
            }

            await Task.Delay(500);

            await target.ModifyAsync(x =>
                x.Name = $"{guildCfg.TicketingConfig.ClosedNamePrefix}-{ticket.GuildSpecificId}");

            DiscordChannel closedCat;
            try
            {
                closedCat = await _discord.Client.GetChannelAsync(guildCfg.TicketingConfig.ClosedCategoryId);
            }
            catch (Exception ex)
            {
                throw new DiscordNotFoundException(
                    $"Closed category channel with Id {guildCfg.TicketingConfig.ClosedCategoryId} doesn't exist", ex);
            }

            await target.ModifyAsync(x => x.Parent = closedCat);

            req.ClosedMessageId = closeMsg.Id;
            await _ticketService.CloseAsync(req, ticket);

            if (guildCfg.TicketingConfig.LogChannelId is null || ticket.IsPrivate) return msgBuilder;

            DiscordChannel logChannel;
            try
            {
                logChannel = await _discord.Client.GetChannelAsync(guildCfg.TicketingConfig.LogChannelId.Value);
            }
            catch (Exception ex)
            {
                throw new DiscordNotFoundException(
                    $"Channel with Id: {guildCfg.TicketingConfig.LogChannelId} doesn't exist. Couldn't send the log",
                    ex);
            }

            var logEmbed = await _chatExportService.ExportToHtmlAsync(guild, target, requestingMember,
                owner ?? await _discord.Client.GetUserAsync(ticket.UserId), ticket);

            if (embed is null) throw new DiscordChatExportException("Exporting to HTML failed");

            try
            {
                await logChannel.SendMessageAsync(logEmbed);
            }
            catch (Exception)
            {
                throw new Exception($"Couldn't send the ticket log in channel with Id: {logChannel.Id}");
            }

            return msgBuilder;
        }

        private async Task<DiscordMessageBuilder> OpenTicketAsync(DiscordGuild guild, DiscordMember owner,
            TicketOpenReqDto req = null, DiscordInteraction intr = null)
        {
            if (guild is null) throw new ArgumentNullException(nameof(guild));
            if (owner is null) throw new ArgumentNullException(nameof(owner));

            if (owner.Guild.Id != guild.Id) throw new ArgumentException(nameof(owner));

            var guildRes =
                await _guildService.GetBySpecificationsAsync<Guild>(
                    new ActiveGuildByDiscordIdWithTicketingSpecifications(guild.Id));
            var guildCfg = guildRes.FirstOrDefault();

            if (guildCfg is null)
                throw new ArgumentException($"Guild with Id:{guild.Id} doesn't exist in the database.");

            if (guildCfg.TicketingConfig is null)
                throw new ArgumentException($"Guild with Id:{guild.Id} doesn't have ticketing enabled.");

            req ??= new TicketOpenReqDto {GuildId = guild.Id, OwnerId = owner.Id};
            req.GuildSpecificId = guildCfg.TicketingConfig.LastTicketId + 1;

            var ticket = await _ticketService.OpenAsync(req);

            if (ticket is null)
            {
                var failEmbed = new DiscordEmbedBuilder();
                failEmbed.WithColor(new DiscordColor(guildCfg.EmbedHexColor));
                failEmbed.WithDescription("You already have an opened ticket in this guild.");
                if (intr is not null)
                    await intr.CreateFollowupMessageAsync(new DiscordFollowupMessageBuilder()
                        .AddEmbed(failEmbed.Build())
                        .AsEphemeral(true));
                throw new ArgumentException("Member already has an opened ticket in this guild.");
            }

            var embed = new DiscordEmbedBuilder();
            embed.WithColor(new DiscordColor(guildCfg.EmbedHexColor));
            embed.WithDescription(guildCfg.TicketingConfig.TicketWelcomeMessageDescription.Replace("@ownerMention@", owner.Mention));

            var fields = JsonSerializer.Deserialize<Dictionary<string, string>>(guildCfg.TicketingConfig.TicketWelcomeMessageFields);
            if (fields is not null && fields.Count != 0)
            {
                int i = 1;
                foreach (var (fieldName, fieldValue) in fields)
                {
                    if (i >= 25) break;
                    embed.AddField(fieldName, fieldValue);
                    i++;
                }
            }

            embed.WithFooter($"Ticket Id: {req.GuildSpecificId}");

            var btn = new DiscordButtonComponent(ButtonStyle.Primary, "close_ticket_btn", "Close this ticket");

            var msgBuilder = new DiscordMessageBuilder();
            msgBuilder.AddEmbed(embed.Build());
            msgBuilder.AddComponents(new List<DiscordComponent> {btn});
            msgBuilder.WithContent($"{owner.Mention} Welcome");

            var modRoles = guild.Roles.Where(x => x.Value.Permissions.HasPermission(Permissions.BanMembers));

            List<DiscordOverwriteBuilder> overwrites = modRoles.Select(role =>
                new DiscordOverwriteBuilder(role.Value).Allow(Permissions.AccessChannels)).ToList();
            overwrites.Add(new DiscordOverwriteBuilder(guild.EveryoneRole).Deny(Permissions.AccessChannels));
            overwrites.Add(new DiscordOverwriteBuilder(owner).Allow(Permissions.AccessChannels));

            string topic = $"Support ticket opened by user {owner.GetFullUsername()} at {DateTime.Now}";

            DiscordChannel openedCat;
            try
            {
                openedCat = await _discord.Client.GetChannelAsync(guildCfg.TicketingConfig.OpenedCategoryId);
            }
            catch (Exception ex)
            {
                throw new DiscordNotFoundException(
                    $"Closed category channel with Id {guildCfg.TicketingConfig.OpenedCategoryId} doesn't exist", ex);
            }

            DiscordChannel newTicketChannel;
            try
            {
                newTicketChannel = await guild.CreateChannelAsync(
                    $"{guildCfg.TicketingConfig.OpenedNamePrefix}-{req.GuildSpecificId:D4}", ChannelType.Text,
                    openedCat, topic, null, null, overwrites);
                DiscordMessage msg = await newTicketChannel.SendMessageAsync(msgBuilder);
                //Program.cachedMsgs.Add(msg.Id, msg);

                _ticketService.BeginUpdate(ticket);
                ticket.ChannelId = newTicketChannel.Id;
                ticket.MessageOpenId = msg.Id;
                await _ticketService.SetAddedUsersAsync(ticket, newTicketChannel.Users.Select(x => x.Id));

                List<ulong> roleIds = new();
                foreach (var overwrite in newTicketChannel.PermissionOverwrites)
                {
                    if(overwrite.CheckPermission(Permissions.AccessChannels) != PermissionLevel.Allowed) continue;

                    DiscordRole role;
                    try
                    {
                        role = await overwrite.GetRoleAsync();
                    }
                    catch (Exception)
                    {
                        continue;
                    }

                    if (role is null) continue;

                    roleIds.Add(role.Id);

                    await Task.Delay(500);
                }

                await _ticketService.SetAddedRolesAsync(ticket, roleIds);

                _guildService.BeginUpdate(guildCfg);
                guildCfg.TicketingConfig.LastTicketId++;

                await _guildService.CommitAsync();
                await _ticketService.CommitAsync();
            }
            catch (Exception ex)
            {
                if (intr is null) return msgBuilder;

                var failEmbed = new DiscordEmbedBuilder();
                failEmbed.WithColor(new DiscordColor(guildCfg.EmbedHexColor));
                failEmbed.WithDescription($"Ticket wasn't created. Please message a moderator. {ex}");
                await intr.CreateFollowupMessageAsync(new DiscordFollowupMessageBuilder().AddEmbed(failEmbed.Build())
                    .AsEphemeral(true));
                return msgBuilder;
            }

            if (intr is null) return msgBuilder;

            var succEmbed = new DiscordEmbedBuilder();
            succEmbed.WithColor(new DiscordColor(guildCfg.EmbedHexColor));
            succEmbed.WithDescription($"Ticket created successfully! Channel: {newTicketChannel?.Mention}");
            await intr.CreateFollowupMessageAsync(new DiscordFollowupMessageBuilder().AddEmbed(succEmbed.Build())
                .AsEphemeral(true));

            return msgBuilder;
        }

        private async Task<DiscordMessageBuilder> ReopenTicketAsync(DiscordGuild guild, DiscordChannel target,
            DiscordMember requestingMember,
            TicketReopenReqDto req = null)
        {
            if (guild is null) throw new ArgumentNullException(nameof(guild));
            if (target is null) throw new ArgumentNullException(nameof(target));
            if (requestingMember is null) throw new ArgumentNullException(nameof(requestingMember));

            if (requestingMember.Guild.Id != guild.Id) throw new ArgumentException(nameof(requestingMember));
            if (target.Guild.Id != guild.Id) throw new ArgumentException(nameof(target));

            var guildRes =
                await _guildService.GetBySpecificationsAsync<Guild>(
                    new ActiveGuildByDiscordIdWithTicketingSpecifications(guild.Id));
            var guildCfg = guildRes.FirstOrDefault();

            if (guildCfg is null)
                throw new ArgumentException($"Guild with Id:{guild.Id} doesn't exist in the database.");

            if (guildCfg.TicketingConfig is null)
                throw new ArgumentException($"Guild with Id:{guild.Id} doesn't have ticketing enabled.");

            req ??= new TicketReopenReqDto
                {GuildId = guild.Id, ChannelId = target.Id, RequestedById = requestingMember.Id};

            var res = await _ticketService.GetBySpecificationsAsync<Ticket>(
                new TicketBaseGetSpecifications(req.Id, req.OwnerId, req.GuildId, req.ChannelId, req.GuildSpecificId,
                    true));
            var ticket = res.FirstOrDefault();

            if (ticket is null) throw new ArgumentException($"Ticket with channel Id: {target.Id} doesn't exist.");

            if (!ticket.IsDisabled)
                throw new ArgumentException(
                    $"Ticket with Id: {ticket.GuildSpecificId}, TargetUserId: {ticket.UserId}, GuildId: {ticket.GuildId}, ChannelId: {ticket.ChannelId} is not closed.");

            if (!requestingMember.Permissions.HasPermission(Permissions.BanMembers))
                throw new DiscordNotAuthorizedException("Requesting member doesn't have moderator rights.");

            var embed = new DiscordEmbedBuilder();

            embed.WithColor(new DiscordColor(guildCfg.EmbedHexColor));
            embed.WithAuthor("Ticket reopened");
            embed.AddField("Requested by", requestingMember.Mention);
            embed.WithFooter($"Ticket Id: {ticket.GuildSpecificId}");

            var msgBuilder = new DiscordMessageBuilder();
            msgBuilder.AddEmbed(embed.Build());

            DiscordMessage reopenMsg;
            try
            {
                reopenMsg = await target.SendMessageAsync(msgBuilder);
            }
            catch (Exception)
            {
                throw new ArgumentException($"Couldn't send ticket close message in channel with Id: {target.Id}");
            }

            try
            {
                DiscordMember owner = requestingMember.Id == ticket.UserId
                    ? requestingMember // means requested by owner so we don't need to grab the owner again
                    : await guild.GetMemberAsync(ticket.UserId);

                await target.AddOverwriteAsync(owner, Permissions.AccessChannels);
            }
            catch
            {
                // to do
            }

            await Task.Delay(500);

            await target.ModifyAsync(x =>
                x.Name = $"{guildCfg.TicketingConfig.OpenedNamePrefix}-{ticket.GuildSpecificId}");

            DiscordChannel openedCat;
            try
            {
                openedCat = await _discord.Client.GetChannelAsync(guildCfg.TicketingConfig.ClosedCategoryId);
            }
            catch (Exception ex)
            {
                throw new DiscordNotFoundException(
                    $"Closed category channel with Id {guildCfg.TicketingConfig.ClosedCategoryId} doesn't exist", ex);
            }

            await target.ModifyAsync(x => x.Parent = openedCat);

            req.ReopenMessageId = reopenMsg.Id;
            await _ticketService.ReopenAsync(req, ticket);

            return msgBuilder;
        }

        private async Task<DiscordEmbed> AddToTicketAsync(DiscordGuild guild, DiscordMember requestingMember,
            DiscordChannel targetTicketChannel, DiscordMember targetMember = null, DiscordRole targetRole = null,
            TicketAddReqDto req = null)
        {
            if (guild is null) throw new ArgumentNullException(nameof(guild));
            if (targetTicketChannel is null) throw new ArgumentNullException(nameof(targetTicketChannel));
            if (requestingMember is null) throw new ArgumentNullException(nameof(requestingMember));
            if (targetRole is null && targetMember is null)
                throw new ArgumentException($"Both {nameof(targetRole)} and {nameof(targetMember)} were null.");

            if (requestingMember.Guild.Id != guild.Id) throw new ArgumentException(nameof(requestingMember));
            if (targetTicketChannel.Guild.Id != guild.Id) throw new ArgumentException(nameof(targetTicketChannel));

            var guildRes =
                await _guildService.GetBySpecificationsAsync<Guild>(
                    new ActiveGuildByDiscordIdWithTicketingSpecifications(guild.Id));
            var guildCfg = guildRes.FirstOrDefault();

            if (guildCfg is null)
                throw new ArgumentException($"Guild with Id:{guild.Id} doesn't exist in the database.");

            if (guildCfg.TicketingConfig is null)
                throw new ArgumentException($"Guild with Id:{guild.Id} doesn't have ticketing enabled.");

            req ??= new TicketAddReqDto(null, null, guild.Id, targetTicketChannel.Id, requestingMember.Id,
                targetRole?.Id ?? targetMember.Id);

            var res = await _ticketService.GetBySpecificationsAsync<Ticket>(
                new TicketBaseGetSpecifications(req.Id, req.OwnerId, req.GuildId, req.ChannelId, req.GuildSpecificId));

            var ticket = res.FirstOrDefault();

            if (ticket is null)
                throw new ArgumentException($"Ticket with channel Id: {targetTicketChannel.Id} doesn't exist.");

            if (!requestingMember.Permissions.HasPermission(Permissions.BanMembers))
                throw new DiscordNotAuthorizedException("Requesting member doesn't have moderator rights.");

            if (targetRole is null)
            {
                await targetTicketChannel.AddOverwriteAsync(targetMember, Permissions.AccessChannels);
                await _ticketService.SetAddedUsersAsync(ticket, targetTicketChannel.Users.Select(x => x.Id));
                await _ticketService.CheckAndSetPrivacyAsync(ticket, guild);

            }
            else
            {
                await targetTicketChannel.AddOverwriteAsync(targetRole, Permissions.AccessChannels);
                List<ulong> roleIds = new();
                foreach (var overwrite in targetTicketChannel.PermissionOverwrites)
                {
                    if (overwrite.CheckPermission(Permissions.AccessChannels) != PermissionLevel.Allowed) continue;

                    DiscordRole role;
                    try
                    {
                        role = await overwrite.GetRoleAsync();
                    }
                    catch (Exception)
                    {
                        continue;
                    }

                    if (role is null) continue;

                    roleIds.Add(role.Id);

                    await Task.Delay(500);
                }

                await _ticketService.SetAddedRolesAsync(ticket, roleIds);
                await _ticketService.CheckAndSetPrivacyAsync(ticket, guild);
            }

            var embed = new DiscordEmbedBuilder();

            embed.WithColor(new DiscordColor(guildCfg.EmbedHexColor));
            embed.WithAuthor($"Ticket moderation | Add {(targetRole is null ? "member" : "role")} action log");
            embed.AddField("Moderator", requestingMember.Mention);
            embed.AddField("Added", $"{targetRole?.Mention ?? targetMember?.Mention}");
            embed.WithFooter($"Ticket Id: {ticket.GuildSpecificId}");

            return embed.Build();
        }

        private async Task<DiscordEmbed> RemoveFromTicketAsync(DiscordGuild guild, DiscordMember requestingMember,
            DiscordChannel targetTicketChannel, DiscordMember targetMember = null, DiscordRole targetRole = null,
            TicketRemoveReqDto req = null)
        {
            if (guild is null) throw new ArgumentNullException(nameof(guild));
            if (targetTicketChannel is null) throw new ArgumentNullException(nameof(targetTicketChannel));
            if (requestingMember is null) throw new ArgumentNullException(nameof(requestingMember));
            if (targetRole is null && targetMember is null)
                throw new ArgumentException($"Both {nameof(targetRole)} and {nameof(targetMember)} were null.");

            if (requestingMember.Guild.Id != guild.Id) throw new ArgumentException(nameof(requestingMember));
            if (targetTicketChannel.Guild.Id != guild.Id) throw new ArgumentException(nameof(targetTicketChannel));

            var guildRes =
                await _guildService.GetBySpecificationsAsync<Guild>(
                    new ActiveGuildByDiscordIdWithTicketingSpecifications(guild.Id));
            var guildCfg = guildRes.FirstOrDefault();

            if (guildCfg is null)
                throw new ArgumentException($"Guild with Id:{guild.Id} doesn't exist in the database.");

            if (guildCfg.TicketingConfig is null)
                throw new ArgumentException($"Guild with Id:{guild.Id} doesn't have ticketing enabled.");

            req ??= new TicketRemoveReqDto(null, null, guild.Id, targetTicketChannel.Id, requestingMember.Id,
                targetRole?.Id ?? targetMember.Id);

            var res = await _ticketService.GetBySpecificationsAsync<Ticket>(
                new TicketBaseGetSpecifications(req.Id, req.OwnerId, req.GuildId, req.ChannelId, req.GuildSpecificId));

            var ticket = res.FirstOrDefault();

            if (ticket is null)
                throw new ArgumentException($"Ticket with channel Id: {targetTicketChannel.Id} doesn't exist.");

            if (!requestingMember.Permissions.HasPermission(Permissions.BanMembers))
                throw new DiscordNotAuthorizedException("Requesting member doesn't have moderator rights.");

            if (targetRole is null)
            {
                await targetTicketChannel.AddOverwriteAsync(targetMember, deny: Permissions.AccessChannels);
                await _ticketService.SetAddedUsersAsync(ticket, targetTicketChannel.Users.Select(x => x.Id));
                await _ticketService.CheckAndSetPrivacyAsync(ticket, guild);
            }
            else
            {
                await targetTicketChannel.AddOverwriteAsync(targetRole, deny: Permissions.AccessChannels);

                List<ulong> roleIds = new();
                foreach (var overwrite in targetTicketChannel.PermissionOverwrites)
                {
                    if (overwrite.CheckPermission(Permissions.AccessChannels) != PermissionLevel.Allowed) continue;

                    DiscordRole role;
                    try
                    {
                        role = await overwrite.GetRoleAsync();
                    }
                    catch (Exception)
                    {
                        continue;
                    }

                    if (role is null) continue;

                    roleIds.Add(role.Id);

                    await Task.Delay(500);
                }

                await _ticketService.SetAddedRolesAsync(ticket, roleIds);
                await _ticketService.CheckAndSetPrivacyAsync(ticket, guild);
            }

            var embed = new DiscordEmbedBuilder();

            embed.WithColor(new DiscordColor(guildCfg.EmbedHexColor));
            embed.WithAuthor($"Ticket moderation | Add {(targetRole is null ? "member" : "role")} action log");
            embed.AddField("Moderator", requestingMember.Mention);
            embed.AddField("Added", $"{targetRole?.Mention ?? targetMember?.Mention}");
            embed.WithFooter($"Ticket Id: {ticket.GuildSpecificId}");

            return embed.Build();
        }
    }
}