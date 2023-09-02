// This file is part of Lisbeth.Bot project
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

using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using Lisbeth.Bot.Application.Discord.ChatExport.Wrappers;
using Lisbeth.Bot.DataAccessLayer.Specifications.Guild;
using Lisbeth.Bot.DataAccessLayer.Specifications.Ticket;
using Lisbeth.Bot.Domain;
using Lisbeth.Bot.Domain.DTOs.Request.Ticket;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MikyM.Discord.Extensions.BaseExtensions;

namespace Lisbeth.Bot.Application.Discord.ChatExport;

[UsedImplicitly]
[ServiceImplementation<IDiscordChatExportService>(ServiceLifetime.InstancePerLifetimeScope)]
public class DiscordChatExportService : IDiscordChatExportService
{
    private readonly IDiscordService _discord;
    private readonly IGuildDataService _guildDataService;
    private readonly ILogger<DiscordChatExportService> _logger;
    private readonly ITicketDataService _ticketDataService;
    private readonly IOptions<BotConfiguration> _options;

    public DiscordChatExportService(IDiscordService discord, IGuildDataService guildDataService,
        ITicketDataService ticketDataService, ILogger<DiscordChatExportService> logger, IOptions<BotConfiguration> options)
    {
        _discord = discord;
        _guildDataService = guildDataService;
        _ticketDataService = ticketDataService;
        _logger = logger;
        _options = options;
    }

    public async Task<Result<DiscordEmbed>> ExportToHtmlAsync(TicketExportReqDto req)
    {
        if (req is null) throw new ArgumentNullException(nameof(req));

        DiscordChannel? target = null;
        DiscordMember requestingMember;
        DiscordUser owner;
        Ticket ticket;

        if (req.OwnerId.HasValue)
        {
            var res = await _ticketDataService.GetSingleBySpecAsync(
                new TicketBaseGetSpecifications(null, req.OwnerId, req.GuildId, null, null, false, 1));
            if (!res.IsDefined())
                return new NotFoundError("Opened ticket with given params doesn't exist in the database.");

            ticket = res.Entity;
            req.ChannelId = ticket.ChannelId;
            req.GuildId = ticket.GuildId;
            req.OwnerId = ticket.UserId;
        }
        else
        {
            var res = await _ticketDataService.GetSingleBySpecAsync(
                new TicketBaseGetSpecifications(null, null, null, req.ChannelId, null, false, 1));
            if (!res.IsDefined())
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
            if (!_options.Value.ChatExportCss.Any() || !_options.Value.ChatExportJs.Any())
                throw new ArgumentException("CSS or JS file was not loaded");

            var resGuild =
                await _guildDataService.GetSingleBySpecAsync(new ActiveGuildByDiscordIdWithTicketingSpecifications(guild.Id));

            if (!resGuild.IsDefined()) return new NotFoundError("Guild doesn't exist in database.");

            if (resGuild.Entity.TicketingConfig is null)
                return new DisabledEntityError("Guild doesn't have ticketing enabled.");

            var guildCfg = resGuild.Entity;

            if (ticket is null)
            {
                var res = await _ticketDataService.GetSingleBySpecAsync(
                    new TicketByChannelIdOrGuildAndOwnerIdSpec(target.Id, null, null));

                if (!res.IsDefined(out ticket)) return new NotFoundError("Ticket doesn't exist in database.");
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
            catch
            {
                // ignored
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
                if (newMessages.Count == 0) break;

                messages.AddRange(newMessages);

                if (newMessages.Any(x => x.Id == ticket.MessageOpenId))
                    break;
            }

            messages.Reverse();

            foreach (var msg in messages.Where(msg => !users.Contains(msg.Author) && msg.Author is not null)) { users.Add(msg.Author); }

            var htmlChatBuilder = new HtmlChatBuilder();
            htmlChatBuilder.WithChannel(target)
                .WithUsers(users)
                .WithMessages(messages)
                .WithCss(_options.Value.ChatExportCss)
                .WithJs(_options.Value.ChatExportJs)
                .WithOptions(_options.Value)
                .WithGuild(guild);
            var html = await htmlChatBuilder.BuildAsync();

            var parser = new MarkdownParser(html, users, guild, _discord);
            html = await parser.GetParsedContentAsync();

            var usersString = users.Aggregate("", (current, user) => current + $"{user.Mention}\n");

            var embedBuilder = new DiscordEmbedBuilder();

            embedBuilder.WithFooter(
                $"This transcript was saved by {requestingMember.Username}#{requestingMember.Discriminator}");

            embedBuilder.WithAuthor($"Transcript | {owner?.GetFullUsername() ?? "Deleted user"}", null, owner?.AvatarUrl);
            embedBuilder.AddField("Ticket Owner", owner?.Mention ?? "Deleted user", true);
            embedBuilder.AddField("Ticket Name", $"ticket-{Regex.Replace(target.Name, @"[^\d]", "")}", true);
            embedBuilder.AddField("Channel", target.Mention, true);
            embedBuilder.AddField("Users in transcript", usersString);
            embedBuilder.WithColor(new DiscordColor(guildCfg.EmbedHexColor));

            MemoryStream ms = new(Encoding.UTF8.GetBytes(html));
            DiscordMessageBuilder messageBuilder = new();

            messageBuilder.AddFile($"transcript-{target.Name}.html", ms);
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
