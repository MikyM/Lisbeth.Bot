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
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using DSharpPlus.Entities;
using JetBrains.Annotations;
using Lisbeth.Bot.Application.Discord.ChatExport.Builders;
using Lisbeth.Bot.Application.Discord.ChatExport.Models;
using Lisbeth.Bot.Application.Results;
using Lisbeth.Bot.Application.Services.Database.Interfaces;
using Lisbeth.Bot.DataAccessLayer.Specifications.Ticket;
using Lisbeth.Bot.Domain.DTOs.Request.Ticket;
using Lisbeth.Bot.Domain.Entities;
using Microsoft.Extensions.Logging;
using MikyM.Common.Application.Results;
using MikyM.Common.Application.Results.Errors;
using MikyM.Common.DataAccessLayer.Specifications;
using MikyM.Discord.Interfaces;

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

        public async Task<Result<DiscordEmbed>> ExportToHtmlAsync(TicketExportReqDto req)
        {
            if (req is null) throw new ArgumentNullException(nameof(req));

            DiscordChannel? target = null;
            DiscordMember requestingMember;
            DiscordUser owner;
            Ticket ticket;

            if (req.Id.HasValue)
            {
                var ticketResult = await _ticketService.GetAsync<Ticket>(req.Id.Value);
                if (!ticketResult.IsSuccess)
                    return new NotFoundError("Opened ticket with given params doesn't exist in the database.");

                ticket = ticketResult.Entity;
                req.ChannelId = ticket.ChannelId;
                req.GuildId = ticket.GuildId;
                req.OwnerId = ticket.UserId;
            }
            else if (req.OwnerId.HasValue && req.GuildId.HasValue)
            {
                var res = await _ticketService.GetSingleBySpecAsync<Ticket>(
                    new TicketBaseGetSpecifications(null, req.OwnerId, req.GuildId, null, null, false, 1));
                if (!res.IsSuccess)
                    return new NotFoundError("Opened ticket with given params doesn't exist in the database.");

                ticket = res.Entity;
                req.ChannelId = ticket.ChannelId;
                req.GuildId = ticket.GuildId;
                req.OwnerId = ticket.UserId;
            }
            else if (req.GuildSpecificId.HasValue && req.GuildId.HasValue)
            {
                var res = await _ticketService.GetSingleBySpecAsync<Ticket>(
                    new TicketBaseGetSpecifications(null, null, req.GuildId, null, req.GuildSpecificId.Value, false,
                        1));
                if (!res.IsSuccess)
                    return new NotFoundError("Opened ticket with given params doesn't exist in the database.");

                ticket = res.Entity;
                req.ChannelId = ticket.ChannelId;
                req.GuildId = ticket.GuildId;
                req.OwnerId = ticket.UserId;
            }
            else
            {
                var res = await _ticketService.GetSingleBySpecAsync<Ticket>(
                    new TicketBaseGetSpecifications(null, null, null, req.ChannelId, null, false, 1));
                if (!res.IsSuccess)
                    return new NotFoundError("Opened ticket with given params doesn't exist in the database.");

                ticket = res.Entity;
                req.OwnerId = ticket.UserId;
                try
                {
                    target = await _discord.Client.GetChannelAsync(req.ChannelId ??
                                                                   throw new InvalidOperationException());
                }
                catch (Exception)
                {
                    return new DiscordNotFoundError(
                        $"User with Id: {req.ChannelId} doesn't exist or isn't this guild's member.");
                }
            }

            target ??= await _discord.Client.GetChannelAsync(req.ChannelId.Value);

            var guild = target.Guild;

            if (guild.Id != ticket.GuildId)
                return new DiscordNotAuthorizedError("Requested ticket doesn't belong to this guild");

            try
            {
                requestingMember = await guild.GetMemberAsync(req.RequestedOnBehalfOfId);
            }
            catch (Exception)
            {
                return new DiscordNotFoundError(
                    $"User with Id: {req.RequestedOnBehalfOfId} doesn't exist or isn't this guild's member.");
            }

            try
            {
                owner = await _discord.Client.GetUserAsync(req.OwnerId.Value);
            }
            catch (Exception)
            {
                return new DiscordNotFoundError(
                    $"User with Id: {req.OwnerId} doesn't exist or isn't this guild's member.");
            }

            return await ExportToHtmlAsync(guild, target, requestingMember, owner, ticket);
        }

        public async Task<Result<DiscordEmbed>> ExportToHtmlAsync(DiscordInteraction intr)
        {
            if (intr is null) throw new ArgumentNullException(nameof(intr));

            return await ExportToHtmlAsync(intr.Guild, intr.Channel, (DiscordMember)intr.User);
        }

        public async Task<Result<DiscordEmbed>> ExportToHtmlAsync(DiscordGuild guild, DiscordChannel target,
            DiscordMember requestingMember, DiscordUser? owner = null, Ticket? ticket = null)
        {
            try
            {
                if (guild is null) throw new ArgumentNullException(nameof(guild));
                if (target is null) throw new ArgumentNullException(nameof(target));
                if (requestingMember is null) throw new ArgumentNullException(nameof(requestingMember));

                var resGuild =
                    await _guildService.GetSingleBySpecAsync<Guild>(
                        new Specification<Guild>(x => x.GuildId == guild.Id && !x.IsDisabled));

                if (!resGuild.IsSuccess) return new NotFoundError("Guild doesn't exist in database.");

                if (resGuild.Entity.TicketingConfig is null)
                    return new DisabledEntityError("Guild doesn't have ticketing enabled.");

                var guildCfg = resGuild.Entity;

                if (ticket is null)
                {
                    var res = await _ticketService.GetSingleBySpecAsync<Ticket>(
                        new TicketBaseGetSpecifications(null, null, guild.Id, target.Id, null, false, 1, true));

                    if (!res.IsSuccess) return new NotFoundError("Ticket doesn't exist in database.");

                    ticket = res.Entity;
                }

                DiscordChannel ticketLogChannel;

                try
                {
                    ticketLogChannel =
                        await _discord.Client.GetChannelAsync(guildCfg.TicketingConfig.LogChannelId);
                }
                catch (Exception)
                {
                    return new DiscordNotFoundError(
                        $"Channel with Id: {guildCfg.TicketingConfig.LogChannelId} doesn't exist.");
                }

                try
                {
                    owner ??= await _discord.Client.GetUserAsync(ticket.UserId);
                }
                catch (Exception)
                {
                    return new DiscordNotFoundError($"User with Id: {ticket.UserId} doesn't exist.");
                }

                if (guild.Id != target.GuildId)
                    return new DiscordNotAuthorizedError("Channel doesn't belong to this guild");
                if (guild.Id != ticketLogChannel.GuildId)
                    return new DiscordNotAuthorizedError("Ticket log channel doesn't belong to this guild");
                if (guild.Id != requestingMember.Guild.Id)
                    return new DiscordNotAuthorizedError("Requesting member isn't in this guild");

                await Task.Delay(500);
                List<DiscordUser> users = new();
                List<DiscordMessage> messages = new();

                messages.AddRange(await target.GetMessagesAsync());

                var embed = new DiscordEmbedBuilder();

                while (true)
                {
                    await Task.Delay(1000);
                    var newMessages = await target.GetMessagesBeforeAsync(messages.Last().Id);
                    if (newMessages.Count == 0 || newMessages.Any(x => x.Id == ticket.MessageOpenId)) break;

                    messages.AddRange(newMessages);
                }

                messages.Reverse();

                foreach (var msg in messages.Where(msg => !users.Contains(msg.Author))) users.Add(msg.Author);

                string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Discord", "ChatExport",
                    "ChatExport.css");
                string css;
                if (File.Exists(path))
                {
                    using StreamReader streamReader = new(path, Encoding.UTF8);
                    css = await streamReader.ReadToEndAsync();
                    css = css.Trim().Replace("\r", string.Empty);
                    css = css.Trim().Replace("\n", string.Empty);
                }
                else
                {
                    throw new IOException($"CSS file was not found at {path}");
                }

                path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Discord", "ChatExport", "ChatExport.js");
                string js;
                if (File.Exists(path))
                {
                    using StreamReader streamReader = new(path, Encoding.UTF8);
                    js = await streamReader.ReadToEndAsync();
                    js = js.Trim().Replace("\r", string.Empty);
                    js = js.Trim().Replace("\n", string.Empty);
                }
                else
                {
                    throw new IOException($"JS file was not found at {path}");
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

                MemoryStream ms = new(Encoding.UTF8.GetBytes(html));
                DiscordMessageBuilder messageBuilder = new();

                messageBuilder.WithFile($"transcript-{target.Name}.html", ms);
                messageBuilder.WithEmbed(embedBuilder.Build());

                await ticketLogChannel.SendMessageAsync(messageBuilder);

                return embed.Build();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Exporting to HTML failed with: {ex}");
                return Result<DiscordEmbed>.FromError(ex);
            }
        }
    }
}