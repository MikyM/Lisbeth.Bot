// This file is part of Lisbeth.Bot project
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

using DSharpPlus;
using DSharpPlus.Entities;
using JetBrains.Annotations;
using Lisbeth.Bot.Application.Discord.ChatExport;
using Lisbeth.Bot.Application.Discord.Services.Interfaces;
using Lisbeth.Bot.Application.Services.Interfaces;
using Lisbeth.Bot.Domain.DTOs.Request;
using Lisbeth.Bot.Domain.Entities;
using MikyM.Common.DataAccessLayer.Specifications;
using MikyM.Discord.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using DSharpPlus.SlashCommands;
using Lisbeth.Bot.Application.Discord.Extensions;
using Lisbeth.Bot.DataAccessLayer.Specifications.GuildSpecifications;
using Lisbeth.Bot.DataAccessLayer.Specifications.TicketSpecifications;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Serilog;

namespace Lisbeth.Bot.Application.Discord.Services
{
    [UsedImplicitly]
    public class DiscordTicketService : IDiscordTicketService
    {
        private readonly IDiscordService _discord;
        private readonly ITicketService _ticketService;
        private readonly IDiscordChatExportService _chatExportService;
        private readonly IGuildService _guildService;

        public DiscordTicketService(IDiscordService discord, ITicketService ticketService, IGuildService guildService,
            IDiscordChatExportService chatExportService)
        {
            _discord = discord;
            _ticketService = ticketService;
            _chatExportService = chatExportService;
            _guildService = guildService;
        }

        public async Task<DiscordMessageBuilder> CloseTicketAsync(TicketCloseReqDto req)
        {
            if (req is null) throw new ArgumentNullException(nameof(req));

            DiscordChannel target = null;
            DiscordMember requestingMemeber;
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
                catch (Exception)
                {
                    throw new ArgumentException(
                        $"User with Id: {req.ChannelId} doesn't exist or isn't this guild's target.");
                }
            }

            target ??= await _discord.Client.GetChannelAsync(req.ChannelId.Value);

            var guild = target.Guild;

            try
            {
                requestingMemeber = await guild.GetMemberAsync(req.RequestedById);
            }
            catch (Exception)
            {
                throw new ArgumentException(
                    $"User with Id: {req.RequestedById} doesn't exist or isn't this guild's target.");
            }

            return await CloseTicketAsync(guild, target, requestingMemeber, req);
        }

        public async Task<DiscordMessageBuilder> CloseTicketAsync(DiscordInteraction intr)
        {
            if (intr is null) throw new ArgumentNullException(nameof(intr));

            return await CloseTicketAsync(intr.Guild, intr.Channel, (DiscordMember) intr.User);
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
                throw new ArgumentException($"Unauthorized.");

            var embed = new DiscordEmbedBuilder();

            embed.WithColor(new DiscordColor(0x18315C));
            embed.WithAuthor($"Ticket closed");
            embed.AddField("Moderator", requestingMember.Mention);
            embed.WithFooter($"Ticket Id: {ticket.GuildSpecificId}");

            var reopenOpt = new DiscordSelectComponentOption("Reopen", "Reopen", "Reopen this ticket", true,
                new DiscordComponentEmoji(DiscordEmoji.FromName(_discord.Client, ":unlock:")));
            var saveTransOpt = new DiscordSelectComponentOption("Save transcript", "Save transcript",
                "Saves the transcript of this ticket", false,
                new DiscordComponentEmoji(DiscordEmoji.FromName(_discord.Client, ":bookmark_tabs:")));
            var options = new List<DiscordSelectComponentOption> {reopenOpt, saveTransOpt};

            var selectComponent = new DiscordSelectComponent("options_closed_ticket", "Choose action", options);

            var btn = new DiscordButtonComponent(ButtonStyle.Primary, "closed_ticket_msg_confirm_btn",
                "Confirm action");

            var msgBuilder = new DiscordMessageBuilder();
            msgBuilder.AddEmbed(embed.Build());
            msgBuilder.AddComponents(new List<DiscordComponent> {selectComponent, btn});

            DiscordMessage closeMsg;
            try
            {
                closeMsg = await target.SendMessageAsync(msgBuilder);
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

                await target.AddOverwriteAsync(owner, deny: Permissions.AccessChannels);
            }
            catch
            {
                // to do
            }

            await Task.Delay(500);

            await target.ModifyAsync(x =>
                x.Name = $"{guildCfg.TicketingConfig.ClosedNamePrefix}-{ticket.GuildSpecificId}");

            DiscordChannel closedCat;
            try
            {
                closedCat = await _discord.Client.GetChannelAsync(guildCfg.TicketingConfig.ClosedCategoryId);
            }
            catch
            {
                throw new ArgumentException(
                    $"Closed category channel with Id {guildCfg.TicketingConfig.ClosedCategoryId} doesn't exist");
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
            catch (Exception)
            {
                throw new ArgumentException(
                    $"Channel with Id: {guildCfg.TicketingConfig.LogChannelId} doesn't exist. Couldn't send the log");
            }

            var exportReq = new TicketExportReqDto
            {
                GuildId = ticket.GuildId,
                OwnerId = ticket.UserId,
                ChannelId = ticket.ChannelId,
                TicketId = ticket.Id
            };

            var logEmbed = await _chatExportService.ExportToHtmlAsync(exportReq, requestingMember);

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

        public async Task<DiscordMessageBuilder> OpenTicketAsync(TicketOpenReqDto req)
        {
            if (req is null) throw new ArgumentNullException(nameof(req));

            DiscordMember owner;
            DiscordGuild guild;

            try
            {
                guild = await _discord.Client.GetGuildAsync(req.GuildId);
            }
            catch (Exception)
            {
                throw new ArgumentException($"Guild with Id: {req.GuildId} doesn't exist");
            }

            try
            {
                owner = await guild.GetMemberAsync(req.OwnerId);
            }
            catch (Exception)
            {
                throw new ArgumentException($"Member with Id: {req.OwnerId} doesn't exist or isn't the guilds member.");
            }

            return await OpenTicketAsync(guild, owner, req);
        }

        public async Task<DiscordMessageBuilder> OpenTicketAsync(DiscordInteraction intr)
        {
            if (intr is null) throw new ArgumentNullException(nameof(intr));

            return await OpenTicketAsync(intr.Guild, (DiscordMember) intr.User);
        }

        private async Task<DiscordMessageBuilder> OpenTicketAsync(DiscordGuild guild, DiscordMember owner,
            TicketOpenReqDto req = null)
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

            req ??= new TicketOpenReqDto() {GuildId = guild.Id, OwnerId = owner.Id};

            var (ticket, id) = await _ticketService.OpenAsync(req, guildCfg);

            if (id == 0) throw new ArgumentException($"Member already has an opened ticket in this guild.");

            var embed = new DiscordEmbedBuilder();

            embed.WithColor(new DiscordColor(0x18315C));
            embed.WithDescription(guildCfg.TicketingConfig.WelcomeMessage.Replace("@ownerMention@", owner.Mention));
            embed.WithFooter($"Ticket Id: {guildCfg.TicketingConfig.LastTicketId}");

            var btn = new DiscordButtonComponent(ButtonStyle.Primary, "close_ticket_btn", "Close this ticket");

            var msgBuilder = new DiscordMessageBuilder();
            msgBuilder.AddEmbed(embed.Build());
            msgBuilder.AddComponents(new List<DiscordComponent> {btn});
            msgBuilder.WithContent($"{owner.Mention} Welcome");

            var modRoles = guild.Roles.Where(x => x.Value.Permissions.HasPermission(Permissions.BanMembers));

            List<DiscordOverwriteBuilder> overwrites = modRoles.Select(role =>
                new DiscordOverwriteBuilder().For(role.Value).Allow(Permissions.AccessChannels)).ToList();
            overwrites.Add(new DiscordOverwriteBuilder().For(guild.EveryoneRole).Deny(Permissions.AccessChannels));
            overwrites.Add(new DiscordOverwriteBuilder().For(owner).Allow(Permissions.AccessChannels));

            string topic = $"Support ticket opened by user {owner.GetFullUsername()} at {DateTime.Now}";

            DiscordChannel openedCat;
            try
            {
                openedCat = await _discord.Client.GetChannelAsync(guildCfg.TicketingConfig.OpenedCategoryId);
            }
            catch
            {
                throw new ArgumentException(
                    $"Closed category channel with Id {guildCfg.TicketingConfig.OpenedCategoryId} doesn't exist");
            }

            try
            {
                DiscordChannel newTicketChannel = await guild.CreateChannelAsync(
                    $"{guildCfg.TicketingConfig.OpenedNamePrefix}-{id:D4}", ChannelType.Text, openedCat, topic, null,
                    null, overwrites);
                await Task.Delay(500);
                DiscordMessage msg = await newTicketChannel.SendMessageAsync(msgBuilder);
                //Program.cachedMsgs.Add(msg.Id, msg);

                _ticketService.BeginUpdate(ticket);
                ticket.ChannelId = newTicketChannel.Id;
                ticket.MessageOpenId = msg.Id;
                await _ticketService.CommitAsync();
            }
            catch (Exception)
            {
                //
            }

            return msgBuilder;
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
                catch (Exception)
                {
                    throw new ArgumentException(
                        $"User with Id: {req.ChannelId} doesn't exist or isn't this guild's target.");
                }
            }

            target ??= await _discord.Client.GetChannelAsync(req.ChannelId.Value);

            var guild = target.Guild;

            try
            {
                requestingMember = await guild.GetMemberAsync(req.RequestedById);
            }
            catch (Exception)
            {
                throw new ArgumentException(
                    $"User with Id: {req.RequestedById} doesn't exist or isn't this guild's target.");
            }

            return await ReopenTicketAsync(guild, target, requestingMember, req);
        }

        public async Task<DiscordMessageBuilder> ReopenTicketAsync(DiscordInteraction intr)
        {
            if (intr is null) throw new ArgumentNullException(nameof(intr));

            return await ReopenTicketAsync(intr.Guild, intr.Channel, (DiscordMember) intr.User);
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

            req ??= new TicketReopenReqDto()
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
                throw new ArgumentException($"Unauthorized.");

            var embed = new DiscordEmbedBuilder();

            embed.WithColor(new DiscordColor(0x18315C));
            embed.WithAuthor($"Ticket reopened");
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
            catch
            {
                throw new ArgumentException(
                    $"Closed category channel with Id {guildCfg.TicketingConfig.ClosedCategoryId} doesn't exist");
            }

            await target.ModifyAsync(x => x.Parent = openedCat);

            req.ReopenMessageId = reopenMsg.Id;
            await _ticketService.ReopenAsync(req, ticket);

            return msgBuilder;
        }

        public async Task<DiscordEmbed> AddToTicketAsync(TicketAddReqDto req)
        {
            if (req is null) throw new ArgumentNullException(nameof(req));

            DiscordMember targetMember = null;
            DiscordRole targetRole = null;
            DiscordMember requestingMemeber;
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
                catch (Exception)
                {
                    throw new ArgumentException(
                        $"User with Id: {req.ChannelId} doesn't exist or isn't this guild's target.");
                }
            }

            targetTicketChannel ??=  await _discord.Client.GetChannelAsync(req.ChannelId.Value);

            var guild = targetTicketChannel.Guild;

            try
            {
                requestingMemeber = await guild.GetMemberAsync(req.RequestedById);
            }
            catch (Exception)
            {
                throw new ArgumentException(
                    $"User with Id: {req.RequestedById} doesn't exist or isn't this guild's target.");
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
                catch (Exception)
                {
                    throw new ArgumentException(
                        $"Role or member with Id: {req.SnowflakeId} doesn't exist or isn't in this guild.");
                }
            }

            return targetRole is null
                ? await AddToTicketAsync(guild, requestingMemeber, targetTicketChannel, targetMember, null, req)
                : await AddToTicketAsync(guild, requestingMemeber, targetTicketChannel, null, targetRole, req);
        }

        public async Task<DiscordEmbed> AddToTicketAsync(InteractionContext ctx)
        {
            if (ctx is null) throw new ArgumentNullException(nameof(ctx));

            return await AddToTicketAsync(ctx.Guild, ctx.Member, ctx.Channel, (DiscordMember) ctx.ResolvedUserMentions?[0], ctx.ResolvedRoleMentions?[0]);
        }

        private async Task<DiscordEmbed> AddToTicketAsync(DiscordGuild guild, DiscordMember requestingMember, 
            DiscordChannel targetTicketChannel, DiscordMember targetMember = null, DiscordRole targetRole = null, TicketAddReqDto req = null)
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

            req ??= new TicketAddReqDto(null, null, guild.Id, targetTicketChannel.Id, requestingMember.Id, targetRole?.Id ?? targetMember.Id);

            var res = await _ticketService.GetBySpecificationsAsync<Ticket>(
                new TicketBaseGetSpecifications(req.Id, req.OwnerId, req.GuildId, req.ChannelId, req.GuildSpecificId));

            var ticket = res.FirstOrDefault();

            if (ticket is null) throw new ArgumentException($"Ticket with channel Id: {targetTicketChannel.Id} doesn't exist.");

            if (!requestingMember.Permissions.HasPermission(Permissions.BanMembers))
                throw new ArgumentException($"Unauthorized.");

            if (targetRole is null)
                await targetTicketChannel.AddOverwriteAsync(targetMember, Permissions.AccessChannels);
            else
                await targetTicketChannel.AddOverwriteAsync(targetRole, Permissions.AccessChannels);

            var embed = new DiscordEmbedBuilder();

            embed.WithColor(new DiscordColor(0x18315C));
            embed.WithAuthor($"Ticket moderation | Add {(targetRole is null ? "member" : "role")} action log");
            embed.AddField("Moderator", requestingMember.Mention);
            embed.AddField("Added", $"{targetRole?.Mention ?? targetMember?.Mention}");
            embed.WithFooter($"Ticket Id: {ticket.GuildSpecificId}");

            return embed.Build();
        }

        public async Task<DiscordEmbed> RemoveFromTicketAsync(TicketRemoveReqDto req)
        {
            if (req is null) throw new ArgumentNullException(nameof(req));

            DiscordMember targetMember = null;
            DiscordRole targetRole = null;
            DiscordMember requestingMemeber;
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
                catch (Exception)
                {
                    throw new ArgumentException(
                        $"User with Id: {req.ChannelId} doesn't exist or isn't this guild's target.");
                }
            }

            targetTicketChannel ??= await _discord.Client.GetChannelAsync(req.ChannelId.Value);

            var guild = targetTicketChannel.Guild;

            try
            {
                requestingMemeber = await guild.GetMemberAsync(req.RequestedById);
            }
            catch (Exception)
            {
                throw new ArgumentException(
                    $"User with Id: {req.RequestedById} doesn't exist or isn't this guild's target.");
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
                catch (Exception)
                {
                    throw new ArgumentException(
                        $"Role or member with Id: {req.SnowflakeId} doesn't exist or isn't in this guild.");
                }
            }

            return targetRole is null
                ? await RemoveFromTicketAsync(guild, requestingMemeber, targetTicketChannel, targetMember, null, req)
                : await RemoveFromTicketAsync(guild, requestingMemeber, targetTicketChannel, null, targetRole, req);
        }

        public async Task<DiscordEmbed> RemoveFromTicketAsync(InteractionContext ctx)
        {
            if (ctx is null) throw new ArgumentNullException(nameof(ctx));

            return await RemoveFromTicketAsync(ctx.Guild, ctx.Member, ctx.Channel, (DiscordMember)ctx.ResolvedUserMentions?[0], ctx.ResolvedRoleMentions?[0]);
        }

        private async Task<DiscordEmbed> RemoveFromTicketAsync(DiscordGuild guild, DiscordMember requestingMember,
            DiscordChannel targetTicketChannel, DiscordMember targetMember = null, DiscordRole targetRole = null, TicketRemoveReqDto req = null)
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

            req ??= new TicketRemoveReqDto(null, null, guild.Id, targetTicketChannel.Id, requestingMember.Id, targetRole?.Id ?? targetMember.Id);

            var res = await _ticketService.GetBySpecificationsAsync<Ticket>(
                new TicketBaseGetSpecifications(req.Id, req.OwnerId, req.GuildId, req.ChannelId, req.GuildSpecificId));

            var ticket = res.FirstOrDefault();

            if (ticket is null) throw new ArgumentException($"Ticket with channel Id: {targetTicketChannel.Id} doesn't exist.");

            if (!requestingMember.Permissions.HasPermission(Permissions.BanMembers))
                throw new ArgumentException($"Unauthorized.");

            if (targetRole is null)
                await targetTicketChannel.AddOverwriteAsync(targetMember, deny: Permissions.AccessChannels);
            else
                await targetTicketChannel.AddOverwriteAsync(targetRole, deny: Permissions.AccessChannels);

            var embed = new DiscordEmbedBuilder();

            embed.WithColor(new DiscordColor(0x18315C));
            embed.WithAuthor($"Ticket moderation | Add {(targetRole is null ? "member" : "role")} action log");
            embed.AddField("Moderator", requestingMember.Mention);
            embed.AddField("Added", $"{targetRole?.Mention ?? targetMember?.Mention}");
            embed.WithFooter($"Ticket Id: {ticket.GuildSpecificId}");

            return embed.Build();
        }
    }
}
