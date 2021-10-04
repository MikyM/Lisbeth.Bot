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
using System.Threading.Tasks;

namespace Lisbeth.Bot.Application.Discord.Services
{
    [UsedImplicitly]
    public class DiscordTicketService : IDiscordTicketService
    {
        private readonly IDiscordService _discord;
        private readonly ITicketService _ticketService;
        private readonly IDiscordChatExportService _chatExportService;
        private readonly IGuildService _guildService;

        public DiscordTicketService(IDiscordService discord, ITicketService ticketService, IGuildService guildService, IDiscordChatExportService chatExportService)
        {
            _discord = discord;
            _ticketService = ticketService;
            _chatExportService = chatExportService;
            _guildService = guildService;
        }

        public Task<DiscordMessageBuilder> CloseTicketAsync(TicketCloseReqDto req, DiscordInteraction intr = null)
        {
            if (req is null) throw new ArgumentNullException(nameof(req));
            return intr is null
                ? CloseTicketAsync(req, null, null)
                : CloseTicketAsync(req, intr.Channel, intr.User, intr.Guild);
        }

        public async Task<DiscordMessageBuilder> CloseTicketAsync(TicketCloseReqDto req, DiscordChannel channel = null, DiscordUser user = null,
            DiscordGuild guild = null)
        {
            if (req is null) throw new ArgumentNullException(nameof(req));

            var guildRes =
                await _guildService.GetBySpecificationsAsync<Guild>(
                    new Specifications<Guild>(x => x.GuildId == req.GuildId && !x.IsDisabled));
            var guildCfg = guildRes.FirstOrDefault();
            
            if (guildCfg is null)
            {
                throw new ArgumentException($"Guild with Id:{req.GuildId} doesn't exist in the database.");
            }

            var ticket = await _ticketService.CloseAsync(req);

            if (ticket is null)
            {
                throw new ArgumentException(
                    $"Ticket with OwnerId: {req.OwnerId}, GuildId: {req.GuildId} doesn't exist.");
            }

            if (ticket.IsDisabled)
            {
                throw new ArgumentException(
                    $"Ticket with Id: {ticket.Id}, UserId: {ticket.UserId}, GuildId: {ticket.GuildId} is already closed.");
            }

            if (user is null)
            {
                try
                {
                    if (req.RequestedById is not null) user = await _discord.Client.GetUserAsync(req.RequestedById.Value);
                }
                catch (Exception)
                {
                    throw new ArgumentException($"User with Id: {req.GuildId} doesn't exist.");
                }
            }

            if (guild is null)
            {
                try
                {
                    if (req.GuildId is not null) guild = await _discord.Client.GetGuildAsync(req.GuildId.Value);
                }
                catch (Exception)
                {
                    throw new ArgumentException($"Guild with Id: {req.GuildId} doesn't exist.");
                }
            }


            if (channel is null)
            {
                try
                {
                    channel = await _discord.Client.GetChannelAsync(ticket.ChannelId);
                }
                catch (Exception)
                {
                    try
                    {
                        if (req.ChannelId is not null) channel = await _discord.Client.GetChannelAsync(req.ChannelId.Value);
                    }
                    catch(Exception)
                    {
                        throw new ArgumentException($"Channel with Id: {req.ChannelId} doesn't exist.");
                    }
                }
            }
            
            var embed = new DiscordEmbedBuilder();

            embed.WithColor(new DiscordColor(0x18315C));
            embed.WithAuthor($"Ticket closed", null, user?.AvatarUrl);
            embed.AddField("Moderator", user?.Mention);
            embed.WithFooter($"Ticket Id: {ticket.Id}");

            var reopenOpt = new DiscordSelectComponentOption("Reopen", "Reopen", "Reopen this ticket", true,
                new DiscordComponentEmoji(DiscordEmoji.FromName(_discord.Client, ":unlock:")));
            var saveTransOpt = new DiscordSelectComponentOption("Save transcript", "Save transcript", "Saves the transcript of this ticket", false,
                new DiscordComponentEmoji(DiscordEmoji.FromName(_discord.Client, ":bookmark_tabs:")));
            var options = new List<DiscordSelectComponentOption> {reopenOpt, saveTransOpt};

            var selectComponent = new DiscordSelectComponent("options_closed_ticket", "Choose action", options);

            var btn = new DiscordButtonComponent(ButtonStyle.Primary, "closed_ticket_msg_confirm_btn", "Confirm action");

            var msgBuilder = new DiscordMessageBuilder();
            msgBuilder.AddEmbed(embed.Build());
            msgBuilder.AddComponents(new List<DiscordComponent>{selectComponent, btn});

            DiscordMessage closeMsg;
            try
            {
                closeMsg = await channel.SendMessageAsync(msgBuilder);
            }
            catch (Exception)
            {
                throw new ArgumentException($"Couldn't send ticket close message in channel with Id: {channel.Id}");
            }

            ticket.MessageCloseId = closeMsg.Id;
            await _ticketService.CommitAsync();

            if (guildCfg.TicketLogChannelId is null) return msgBuilder;

            DiscordChannel logChannel;
            try
            {
                logChannel = await _discord.Client.GetChannelAsync(guildCfg.TicketLogChannelId.Value);
            }
            catch (Exception)
            {
                throw new ArgumentException(
                    $"Channel with Id: {guildCfg.TicketLogChannelId} doesn't exist. Couldn't send the log");
            }

            var exportReq = new TicketExportReqDto
            {
                GuildId = ticket.GuildId,
                OwnerId = ticket.UserId,
                ChannelId = ticket.ChannelId,
                TicketId = ticket.Id
            };

            var logEmbed = await _chatExportService.ExportToHtmlAsync(exportReq, user);

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

        public Task<DiscordMessageBuilder> OpenTicketAsync(TicketOpenReqDto req, DiscordInteraction intr = null)
        {
            throw new NotImplementedException();
        }

        public Task<DiscordMessageBuilder> OpenTicketAsync(TicketOpenReqDto req, DiscordChannel channel = null, DiscordUser user = null,
            DiscordGuild guild = null)
        {
            throw new NotImplementedException();
        }

        public Task<DiscordMessageBuilder> ReopenTicketAsync(TicketReopenReqDto req, DiscordInteraction intr = null)
        {
            throw new NotImplementedException();
        }

        public Task<DiscordMessageBuilder> ReopenTicketAsync(TicketReopenReqDto req, DiscordChannel channel = null, DiscordUser user = null,
            DiscordGuild guild = null)
        {
            throw new NotImplementedException();
        }
    }
}
