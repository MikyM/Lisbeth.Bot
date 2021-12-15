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
using Lisbeth.Bot.Application.Discord.ChatExport;
using Lisbeth.Bot.Application.Discord.Exceptions;
using Lisbeth.Bot.Application.Discord.Handlers.Ticket.Interfaces;
using Lisbeth.Bot.Application.Discord.Helpers.InteractionIdEnums.Selects;
using Lisbeth.Bot.Application.Discord.Helpers.InteractionIdEnums.SelectValues;
using Lisbeth.Bot.Application.Discord.Requests.Ticket;
using Lisbeth.Bot.DataAccessLayer.Specifications.Guild;
using Lisbeth.Bot.DataAccessLayer.Specifications.Ticket;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using MikyM.Common.Utilities;
using MikyM.Discord.Interfaces;

namespace Lisbeth.Bot.Application.Discord.Handlers.Ticket;

[UsedImplicitly]
public class DiscordConfirmCloseTicketHandler : IDiscordConfirmCloseTicketHandler
{
    private readonly IDiscordService _discord;
    private readonly IGuildDataService _guildDataService;
    private readonly ITicketDataService _ticketDataService;
    private readonly ILogger<DiscordConfirmCloseTicketHandler> _logger;
    private readonly IAsyncExecutor _asyncExecutor;

    public DiscordConfirmCloseTicketHandler(IDiscordService discord, IGuildDataService guildDataService,
        ITicketDataService ticketDataService, ILogger<DiscordConfirmCloseTicketHandler> logger,
        IAsyncExecutor asyncExecutor)
    {
        _discord = discord;
        _guildDataService = guildDataService;
        _ticketDataService = ticketDataService;
        _logger = logger;
        _asyncExecutor = asyncExecutor;
    }

    public async Task<Result<DiscordMessageBuilder>> HandleAsync(ConfirmCloseTicketRequest request)
    {
        if (request is null) throw new ArgumentNullException(nameof(request));

        var guildRes =
            await _guildDataService.GetSingleBySpecAsync<Guild>(
                new ActiveGuildByDiscordIdWithTicketingSpecifications(request.Dto.GuildId));

        if (!guildRes.IsDefined(out var guildCfg)) return Result<DiscordMessageBuilder>.FromError(guildRes);

        if (guildCfg.TicketingConfig is null)
            return new DisabledEntityError($"Guild with Id:{request.Dto.GuildId} doesn't have ticketing enabled.");

        var res = await _ticketDataService.GetSingleBySpecAsync(
            new TicketByChannelIdOrGuildAndOwnerIdSpec(request.Dto.ChannelId, request.Dto.GuildId, request.Dto.OwnerId));

        if (!res.IsDefined(out var ticket)) return new NotFoundError($"Ticket with given params doesn't exist.");

        if (ticket.IsDisabled)
            return new DisabledEntityError(
                $"Ticket with Id: {ticket.GuildSpecificId}, TargetUserId: {ticket.UserId}, GuildId: {ticket.GuildId}, ChannelId: {ticket.ChannelId} is already closed.");

        // data req
        DiscordGuild guild = request.Interaction?.Guild ?? await _discord.Client.GetGuildAsync(request.Dto.GuildId);
        DiscordMember requestingMember = request.Interaction?.User as DiscordMember ?? await guild.GetMemberAsync(request.Dto.RequestedOnBehalfOfId);
        DiscordChannel target = request.Interaction?.Channel ?? guild.GetChannel(ticket.ChannelId);

        if (ticket.UserId != requestingMember.Id &&
            !requestingMember.Permissions.HasPermission(Permissions.BanMembers))
            return new DiscordNotAuthorizedError(
                "Requesting member doesn't have moderator rights or isn't the ticket's owner.");

        var embed = new DiscordEmbedBuilder();

        embed.WithColor(new DiscordColor(guildCfg.EmbedHexColor));
        embed.WithAuthor("Ticket closed");
        embed.AddField("Requested by", requestingMember.Mention);
        embed.WithFooter($"Ticket Id: {ticket.GuildSpecificId}");

        var options = new List<DiscordSelectComponentOption>
        {
            new("Reopen", nameof(TicketSelectValue.TicketReopenValue), "Reopens this ticket",
                false, new DiscordComponentEmoji(DiscordEmoji.FromName(_discord.Client, ":unlock:"))),
            new("Transcript", nameof(TicketSelectValue.TicketTranscriptValue),
                "Generates HTML transcript for this ticket",
                false, new DiscordComponentEmoji(DiscordEmoji.FromName(_discord.Client, ":blue_book:")))
        };
        var selectDropdown = new DiscordSelectComponent(nameof(TicketSelect.TicketCloseMessageSelect),
            "Choose an action", options);

        var msgBuilder = new DiscordMessageBuilder();
        msgBuilder.AddEmbed(embed.Build());
        msgBuilder.AddComponents(selectDropdown);

        DiscordMessage closeMsg;
        try
        {
            closeMsg = await target.SendMessageAsync(msgBuilder);
        }
        catch (Exception)
        {
            return new DiscordError($"Couldn't send ticket close message in channel with Id: {target.Id}");
        }

        DiscordMember? owner;
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
            x.Name = $"{guildCfg.TicketingConfig.ClosedNamePrefix}-{ticket.GuildSpecificId:D4}");

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

        request.Dto.ClosedMessageId = closeMsg.Id;
        await _ticketDataService.CloseAsync(request.Dto, ticket);

        if (ticket.IsPrivate) return msgBuilder;

        _ = _asyncExecutor.ExecuteAsync<IDiscordChatExportService>(async x => await x.ExportToHtmlAsync(guild,
            target, requestingMember,
            owner ?? await _discord.Client.GetUserAsync(ticket.UserId), ticket));

        return msgBuilder;
    }
}
