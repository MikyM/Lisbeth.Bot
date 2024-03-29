﻿// This file is part of Lisbeth.Bot project
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

using Lisbeth.Bot.Application.Discord.Commands.Ticket;
using Lisbeth.Bot.DataAccessLayer.Specifications.Guild;
using Lisbeth.Bot.DataAccessLayer.Specifications.Ticket;
using Microsoft.Extensions.Logging;
using MikyM.Discord.Extensions.BaseExtensions;

namespace Lisbeth.Bot.Application.Discord.CommandHandlers.Ticket;

[UsedImplicitly]
public class ReopenTicketCommandHandler : IAsyncCommandHandler<ReopenTicketCommand, DiscordMessageBuilder>
{
    private readonly IDiscordGuildRequestDataProvider _requestDataProvider;
    private readonly IGuildDataService _guildDataService;
    private readonly ITicketDataService _ticketDataService;
    private readonly ILogger<ReopenTicketCommandHandler> _logger;

    public ReopenTicketCommandHandler(IDiscordGuildRequestDataProvider requestDataProvider, IGuildDataService guildDataService,
        ITicketDataService ticketDataService, ILogger<ReopenTicketCommandHandler> logger)
    {
        _requestDataProvider = requestDataProvider;
        _guildDataService = guildDataService;
        _ticketDataService = ticketDataService;
        _logger = logger;
    }

    public async Task<Result<DiscordMessageBuilder>> HandleAsync(ReopenTicketCommand command, CancellationToken cancellationToken = default)
    {
        if (command is null) throw new ArgumentNullException(nameof(command));

        var guildRes =
            await _guildDataService.GetSingleBySpecAsync(
                new ActiveGuildByDiscordIdWithTicketingSpecifications(command.Dto.GuildId), cancellationToken);

        if (!guildRes.IsDefined(out var guildCfg)) return Result<DiscordMessageBuilder>.FromError(guildRes);

        if (guildCfg.TicketingConfig is null)
            return new DisabledEntityError($"Guild with Id:{command.Dto.GuildId} doesn't have ticketing enabled.");

        var res = await _ticketDataService.GetSingleBySpecAsync(
            new TicketByChannelIdOrGuildAndOwnerIdSpec(command.Dto.ChannelId, command.Dto.GuildId, command.Dto.OwnerId), cancellationToken);

        if (!res.IsDefined(out var ticket)) return new NotFoundError("Ticket with given params doesn't exist.");

        if (!ticket.IsDisabled)
            return new DisabledEntityError(
                $"Ticket with Id: {ticket.GuildSpecificId}, TargetUserId: {ticket.UserId}, GuildId: {ticket.GuildId}, ChannelId: {ticket.ChannelId} is not closed.");

        // data req
        var initRes = await _requestDataProvider.InitializeAsync(command.Dto, command.Interaction);
        if (!initRes.IsSuccess)
            return Result<DiscordMessageBuilder>.FromError(initRes);

        var guild = _requestDataProvider.DiscordGuild;
        var requestingMember = _requestDataProvider.RequestingMember;

        var channelRes = await _requestDataProvider.GetChannelAsync(ticket.ChannelId);
        if (!channelRes.IsDefined(out var target))
            return Result<DiscordMessageBuilder>.FromError(channelRes);

        if (!requestingMember.IsModerator())
            return new DiscordNotAuthorizedError("Requesting member doesn't have moderator rights.");

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
            return new DiscordError($"Couldn't send ticket close message in channel with Id: {target.Id}");
        }

        try
        {
            var owner = requestingMember.Id == ticket.UserId
                ? requestingMember // means requested by owner so we don't need to grab the owner again
                : await guild.GetMemberAsync(ticket.UserId);

            await target.AddOverwriteAsync(owner, Permissions.AccessChannels);
        }
        catch
        {
            _logger.LogDebug($"User left the guild before reopening the ticket with Id: {ticket.Id}");
        }

        await Task.Delay(500);

        await target.ModifyAsync(x =>
            x.Name = $"{guildCfg.TicketingConfig.OpenedNamePrefix}-{ticket.GuildSpecificId:D4}");

        var openedCatRes = await _requestDataProvider.GetChannelAsync(guildCfg.TicketingConfig.OpenedCategoryId);
        if (!openedCatRes.IsDefined(out var openedCat))
            return new DiscordNotFoundError(
                $"Opened category channel with Id {guildCfg.TicketingConfig.OpenedCategoryId} doesn't exist");

        await target.ModifyAsync(x => x.Parent = openedCat);

        if (ticket.MessageCloseId.HasValue)
            await target.DeleteMessageAsync(await target.GetMessageAsync(ticket.MessageCloseId.Value));

        command.Dto.ReopenMessageId = reopenMsg.Id;
        await _ticketDataService.ReopenAsync(command.Dto, ticket);

        return msgBuilder;
    }
}
