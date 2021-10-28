﻿// This file is part of Lisbeth.Bot project
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
using JetBrains.Annotations;
using Lisbeth.Bot.Application.Discord.ChatExport.Builders;
using Lisbeth.Bot.Application.Discord.ChatExport.Models;
using Lisbeth.Bot.Application.Discord.Exceptions;
using Lisbeth.Bot.Application.Services.Interfaces;
using Lisbeth.Bot.DataAccessLayer.Specifications.TicketSpecifications;
using Lisbeth.Bot.Domain.DTOs.Request;
using Lisbeth.Bot.Domain.Entities;
using Microsoft.Extensions.Logging;
using MikyM.Common.DataAccessLayer.Specifications;
using MikyM.Discord.Interfaces;
using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Lisbeth.Bot.Application.Services.Interfaces.Database;

namespace Lisbeth.Bot.Application.Discord.ChatExport
{
    [UsedImplicitly]
    public class DiscordChatExportService : IDiscordChatExportService
    {
        private readonly IDiscordService _discord;
        private readonly IGuildService _guildService;
        private readonly ILogger<DiscordChatExportService> _logger;
        private readonly ITicketService _ticketService;

        public DiscordChatExportService(IDiscordService discord, IGuildService guildService,
            ITicketService ticketService, ILogger<DiscordChatExportService> logger)
        {
            _discord = discord;
            _guildService = guildService;
            _ticketService = ticketService;
            _logger = logger;
        }

        public async Task<DiscordEmbed> ExportToHtmlAsync(TicketExportReqDto req)
        {
            if (req  is null) throw new ArgumentNullException(nameof(req));

            DiscordChannel target = null;
            DiscordMember requestingMember;
            DiscordUser owner;
            Ticket ticket;

            if (req.Id.HasValue)
            {
                ticket = await _ticketService.GetAsync<Ticket>(req.Id.Value);
                if (ticket  is null)
                    throw new ArgumentException("Opened ticket with given params doesn't exist in the database.");

                req.ChannelId = ticket.ChannelId;
                req.GuildId = ticket.GuildId;
                req.OwnerId = ticket.UserId;
            }
            else if (req.OwnerId.HasValue && req.GuildId.HasValue)
            {
                var res = await _ticketService.GetBySpecAsync<Ticket>(
                    new TicketBaseGetSpecifications(null, req.OwnerId, req.GuildId, null, null, false, 1));
                ticket = res.FirstOrDefault();
                if (ticket  is null)
                    throw new ArgumentException("Opened ticket with given params doesn't exist in the database.");

                req.ChannelId = ticket.ChannelId;
                req.GuildId = ticket.GuildId;
                req.OwnerId = ticket.UserId;
            }
            else if (req.GuildSpecificId.HasValue && req.GuildId.HasValue)
            {
                var res = await _ticketService.GetBySpecAsync<Ticket>(
                    new TicketBaseGetSpecifications(null, null, req.GuildId, null, req.GuildSpecificId.Value, false,
                        1));
                ticket = res.FirstOrDefault();
                if (ticket  is null)
                    throw new ArgumentException("Opened ticket with given params doesn't exist in the database.");

                req.ChannelId = ticket.ChannelId;
                req.GuildId = ticket.GuildId;
                req.OwnerId = ticket.UserId;
            }
            else
            {
                var res = await _ticketService.GetBySpecAsync<Ticket>(
                    new TicketBaseGetSpecifications(null, null, null, req.ChannelId, null, false, 1));
                ticket = res.FirstOrDefault();
                if (ticket  is null)
                    throw new ArgumentException("Opened ticket with given params doesn't exist in the database.");
                req.OwnerId = ticket.UserId;
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

            if (guild.Id != ticket.GuildId)
                throw new ArgumentException("Requested ticket doesn't belong to this guild");

            try
            {
                requestingMember = await guild.GetMemberAsync(req.RequestedOnBehalfOfId);
            }
            catch (Exception ex)
            {
                throw new DiscordNotFoundException(
                    $"User with Id: {req.RequestedOnBehalfOfId} doesn't exist or isn't this guild's member.", ex);
            }

            try
            {
                owner = await _discord.Client.GetUserAsync(req.OwnerId.Value);
            }
            catch (Exception ex)
            {
                throw new DiscordNotFoundException(
                    $"User with Id: {req.OwnerId} doesn't exist or isn't this guild's member.", ex);
            }

            return await ExportToHtmlAsync(guild, target, requestingMember, owner, ticket);
        }

        public async Task<DiscordEmbed> ExportToHtmlAsync(DiscordInteraction intr)
        {
            if (intr  is null) throw new ArgumentNullException(nameof(intr));

            return await ExportToHtmlAsync(intr.Guild, intr.Channel, (DiscordMember) intr.User);
        }

        public async Task<DiscordEmbed> ExportToHtmlAsync(DiscordGuild guild, DiscordChannel target,
            DiscordMember requestingMember, DiscordUser owner = null, Ticket ticket = null)
        {
            try
            {
                if (guild  is null) throw new ArgumentNullException(nameof(guild));
                if (target  is null) throw new ArgumentNullException(nameof(target));
                if (requestingMember  is null) throw new ArgumentNullException(nameof(requestingMember));

                var resGuild =
                    await _guildService.GetBySpecAsync<Guild>(
                        new Specification<Guild>(x => x.GuildId == guild.Id && !x.IsDisabled));

                var guildCfg = resGuild.FirstOrDefault();

                if (guildCfg  is null) throw new ArgumentException("Guild doesn't exist in database.");

                if (ticket  is null)
                {
                    var res = await _ticketService.GetBySpecAsync<Ticket>(
                        new TicketBaseGetSpecifications(null, null, guild.Id, target.Id, null, false, 1, true));
                    ticket = res.FirstOrDefault();
                }

                DiscordChannel ticketLogChannel;

                if (ticket  is null) throw new ArgumentException("Ticket doesn't exist in database.");

                if (guildCfg.TicketingConfig.LogChannelId  is null)
                    throw new ArgumentException("Guild doesn't have ticketing log channel set.");

                try
                {
                    ticketLogChannel =
                        await _discord.Client.GetChannelAsync(guildCfg.TicketingConfig.LogChannelId.Value);
                }
                catch (Exception)
                {
                    throw new ArgumentException(
                        $"Channel with Id: {guildCfg.TicketingConfig.LogChannelId} doesn't exist.");
                }

                try
                {
                    owner ??= await _discord.Client.GetUserAsync(ticket.UserId);
                }
                catch (Exception)
                {
                    throw new ArgumentException($"User with Id: {ticket.UserId} doesn't exist.");
                }

                if (guild.Id != target.GuildId) throw new ArgumentException("Channel doesn't belong to this guild");
                if (guild.Id != ticketLogChannel.GuildId)
                    throw new ArgumentException("Ticket log channel doesn't belong to this guild");
                if (guild.Id != requestingMember.Guild.Id)
                    throw new ArgumentException("Requesting member isn't in this guild");

                await Task.Delay(500);
                List<DiscordUser> users = new();
                List<DiscordMessage> messages = new();

                messages.AddRange(await target.GetMessagesAsync());

                var embed = new DiscordEmbedBuilder();

                while (true)
                {
                    await Task.Delay(1000);
                    var newMessages = await target.GetMessagesBeforeAsync(messages.Last().Id);
                    if (newMessages.Count == 0 || newMessages.Any(x=> x.Id == ticket.MessageOpenId)) break;

                    messages.AddRange(newMessages);
                }

                messages.Reverse();

                foreach (var msg in messages.Where(msg => !users.Contains(msg.Author))) users.Add(msg.Author);

                string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Discord", "ChatExport",
                    "ChatExport.css");
                string css;
                if (File.Exists(path))
                {
                    using StreamReader streamReader = new StreamReader(path, Encoding.UTF8);
                    css = await streamReader.ReadToEndAsync();
                    css = css.Trim().Replace("\r", string.Empty);
                    css = css.Trim().Replace("\n", string.Empty);
                }
                else
                {
                    throw new Exception($"CSS file was not found at {path}");
                }

                path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Discord", "ChatExport", "ChatExport.js");
                string js;
                if (File.Exists(path))
                {
                    using StreamReader streamReader = new StreamReader(path, Encoding.UTF8);
                    js = await streamReader.ReadToEndAsync();
                    js = js.Trim().Replace("\r", string.Empty);
                    js = js.Trim().Replace("\n", string.Empty);
                }
                else
                {
                    throw new Exception($"JS file was not found at {path}");
                }

                var htmlChatBuilder = new HtmlChatBuilder();
                htmlChatBuilder.WithChannel(target).WithUsers(users).WithMessages(messages).WithCss(css).WithJs(js);
                string html = await htmlChatBuilder.BuildAsync();

                var parser = new MarkdownParser(html, users, guild, _discord);
                html = await parser.GetParsedContentAsync();

                string usersString = users.Aggregate("", (current, user) => current + $"{user.Mention}\n");

                var embedBuilder = new DiscordEmbedBuilder();

                embedBuilder.WithFooter(
                    $"This transcript was saved by {requestingMember.Username}#{requestingMember.Discriminator}");

                embedBuilder.WithAuthor($"Transcript | {owner.Username}#{owner.Discriminator}", null, owner.AvatarUrl);
                embedBuilder.AddField("Ticket Owner", owner.Mention, true);
                embedBuilder.AddField("Ticket Name", $"ticket-{Regex.Replace(target.Name, @"[^\d]", "")}", true);
                embedBuilder.AddField("Channel", target.Mention, true);
                embedBuilder.AddField("Users in transcript", usersString);
                embedBuilder.WithColor(new DiscordColor(guildCfg.EmbedHexColor));

                MemoryStream ms = new MemoryStream(Encoding.UTF8.GetBytes(html));
                DiscordMessageBuilder messageBuilder = new DiscordMessageBuilder();

                messageBuilder.WithFile($"transcript-{target.Name}.html", ms);
                messageBuilder.WithEmbed(embedBuilder.Build());

                Log.Logger.Information(requestingMember  is null
                    ? $"Automatically saved transcript of {target.Name}"
                    : $"User {requestingMember.Username}#{requestingMember.Discriminator} with ID: {requestingMember.Id} saved transcript of {target.Name}");

                await ticketLogChannel.SendMessageAsync(messageBuilder);

                return embed;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Exporting to HTML failed with: {ex}");
                return null;
            }
        }
    }
}