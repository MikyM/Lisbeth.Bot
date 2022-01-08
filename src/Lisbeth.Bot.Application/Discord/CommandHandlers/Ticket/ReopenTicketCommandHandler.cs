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
using Lisbeth.Bot.Application.Discord.Commands.Ticket;
using Lisbeth.Bot.DataAccessLayer.Specifications.Guild;
using Lisbeth.Bot.DataAccessLayer.Specifications.Ticket;
using Microsoft.Extensions.Logging;
using MikyM.Common.Application.CommandHandlers;
using MikyM.Discord.Extensions.BaseExtensions;
using MikyM.Discord.Interfaces;

namespace Lisbeth.Bot.Application.Discord.CommandHandlers.Ticket;

[UsedImplicitly]
public class ReopenTicketCommandHandler : ICommandHandler<ReopenTicketCommand, DiscordMessageBuilder>
{
    private readonly IDiscordService _discord;
    private readonly IGuildDataDataService _guildDataDataService;
    private readonly ITicketDataService _ticketDataService;
    private readonly ILogger<ConfirmCloseTicketCommandHandler> _logger;

    public ReopenTicketCommandHandler(IDiscordService discord, IGuildDataDataService guildDataDataService,
        ITicketDataService ticketDataService, ILogger<ConfirmCloseTicketCommandHandler> logger)
    {
        _discord = discord;
        _guildDataDataService = guildDataDataService;
        _ticketDataService = ticketDataService;
        _logger = logger;
    }

    public async Task<Result<DiscordMessageBuilder>> HandleAsync(ReopenTicketCommand command)
    {
        if (command is null) throw new ArgumentNullException(nameof(command));

        var guildRes =
            await _guildDataDataService.GetSingleBySpecAsync<Guild>(
                new ActiveGuildByDiscordIdWithTicketingSpecifications(command.Dto.GuildId));

        if (!guildRes.IsDefined(out var guildCfg)) return Result<DiscordMessageBuilder>.FromError(guildRes);

        if (guildCfg.TicketingConfig is null)
            return new DisabledEntityError($"Guild with Id:{command.Dto.GuildId} doesn't have ticketing enabled.");

        var res = await _ticketDataService.GetSingleBySpecAsync(
            new TicketByChannelIdOrGuildAndOwnerIdSpec(command.Dto.ChannelId, command.Dto.GuildId, command.Dto.OwnerId));

        if (!res.IsDefined(out var ticket)) return new NotFoundError($"Ticket with given params doesn't exist.");

        if (!ticket.IsDisabled)
            return new DisabledEntityError(
                $"Ticket with Id: {ticket.GuildSpecificId}, TargetUserId: {ticket.UserId}, GuildId: {ticket.GuildId}, ChannelId: {ticket.ChannelId} is not closed.");

        // data req
        DiscordGuild guild = command.Interaction?.Guild ?? await _discord.Client.GetGuildAsync(command.Dto.GuildId);
        DiscordMember requestingMember = command.Interaction?.User as DiscordMember ?? await guild.GetMemberAsync(command.Dto.RequestedOnBehalfOfId);
        DiscordChannel target = command.Interaction?.Channel ?? guild.GetChannel(ticket.ChannelId);


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
            DiscordMember owner = requestingMember.Id == ticket.UserId
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

        DiscordChannel openedCat;
        try
        {
            openedCat = await _discord.Client.GetChannelAsync(guildCfg.TicketingConfig.OpenedCategoryId);
        }
        catch (Exception)
        {
            return new DiscordNotFoundError(
                $"Closed category channel with Id {guildCfg.TicketingConfig.OpenedCategoryId} doesn't exist");
        }

        await target.ModifyAsync(x => x.Parent = openedCat);

        if (ticket.MessageCloseId.HasValue)
            await target.DeleteMessageAsync(await target.GetMessageAsync(ticket.MessageCloseId.Value));

        command.Dto.ReopenMessageId = reopenMsg.Id;
        await _ticketDataService.ReopenAsync(command.Dto, ticket);

        return msgBuilder;
    }
}
