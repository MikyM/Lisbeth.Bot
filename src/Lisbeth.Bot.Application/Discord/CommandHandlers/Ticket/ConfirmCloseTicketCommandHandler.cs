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
using Lisbeth.Bot.Application.Discord.Commands.Ticket;
using Lisbeth.Bot.Application.Discord.Exceptions;
using Lisbeth.Bot.Application.Discord.Extensions;
using Lisbeth.Bot.Application.Discord.Helpers.InteractionIdEnums.Selects;
using Lisbeth.Bot.Application.Discord.Helpers.InteractionIdEnums.SelectValues;
using Lisbeth.Bot.DataAccessLayer.Specifications.Guild;
using Lisbeth.Bot.DataAccessLayer.Specifications.Ticket;
using Microsoft.Extensions.Logging;
using MikyM.Common.Application.CommandHandlers;
using MikyM.Common.Utilities;
using MikyM.Discord.Extensions.BaseExtensions;
using System.Collections.Generic;
using MikyM.Discord.Interfaces;

namespace Lisbeth.Bot.Application.Discord.CommandHandlers.Ticket;

[UsedImplicitly]
public class ConfirmCloseTicketCommandHandler : ICommandHandler<ConfirmCloseTicketCommand>
{
    private readonly IDiscordGuildRequestDataProvider _requestDataProvider;
    private readonly IGuildDataService _guildDataService;
    private readonly ITicketDataService _ticketDataService;
    private readonly ILogger<ConfirmCloseTicketCommandHandler> _logger;
    private readonly IAsyncExecutor _asyncExecutor;
    private readonly IDiscordService _discord;

    public ConfirmCloseTicketCommandHandler(IDiscordGuildRequestDataProvider guildDataProvider, IGuildDataService guildDataService,
        ITicketDataService ticketDataService, ILogger<ConfirmCloseTicketCommandHandler> logger,
        IAsyncExecutor asyncExecutor, IDiscordService discord)
    {
        _requestDataProvider = guildDataProvider;
        _guildDataService = guildDataService;
        _ticketDataService = ticketDataService;
        _logger = logger;
        _asyncExecutor = asyncExecutor;
        _discord = discord;
    }

    public async Task<Result> HandleAsync(ConfirmCloseTicketCommand command)
    {
        if (command is null) throw new ArgumentNullException(nameof(command));

        var guildRes =
            await _guildDataService.GetSingleBySpecAsync(
                new ActiveGuildByDiscordIdWithTicketingSpecifications(command.Dto.GuildId));

        if (!guildRes.IsDefined(out var guildCfg)) return Result.FromError(guildRes);

        if (guildCfg.TicketingConfig is null)
            return new DisabledEntityError($"Guild with Id:{command.Dto.GuildId} doesn't have ticketing enabled.");

        var res = await _ticketDataService.GetSingleBySpecAsync(
            new TicketByChannelIdOrGuildAndOwnerIdSpec(command.Dto.ChannelId, command.Dto.GuildId, command.Dto.OwnerId));

        if (!res.IsDefined(out var ticket)) return new NotFoundError($"Ticket with given params doesn't exist.");

        if (ticket.IsDisabled)
            return new DisabledEntityError(
                $"Ticket with Id: {ticket.GuildSpecificId}, TargetUserId: {ticket.UserId}, GuildId: {ticket.GuildId}, ChannelId: {ticket.ChannelId} is already closed.");

        // data req
        var initRes = await _requestDataProvider.InitializeAsync(command.Dto, command.Interaction);
        if (!initRes.IsSuccess)
            return initRes;

        DiscordGuild guild = _requestDataProvider.DiscordGuild;
        DiscordMember requestingMember = _requestDataProvider.RequestingMember;

        var channelRes = await _requestDataProvider.GetChannelAsync(ticket.ChannelId);
        if (!channelRes.IsDefined(out var target))
            return Result.FromError(channelRes);

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
                false, new DiscordComponentEmoji(DiscordEmoji.FromName(_discord.Client, ":blue_book:"))),
            new("Delete", nameof(TicketSelectValue.TicketDeleteValue),
                "Delete this ticket",
                false, new DiscordComponentEmoji(DiscordEmoji.FromName(_discord.Client, ":no_entry:")))
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

            if (ticket.AddedUserIds is not null)
                foreach (var userId in ticket.AddedUserIds)
                {
                    try
                    {
                        var member = await guild.GetMemberAsync(userId);
                        await Task.Delay(150);
                        if (member is null || member.IsModerator()) continue;

                        await target.AddOverwriteAsync(member, deny: Permissions.AccessChannels);
                        await Task.Delay(250);
                    }
                    catch
                    {
                        // ignored
                    }
                }
            if (ticket.AddedRoleIds is not null)
                foreach (var roleId in ticket.AddedRoleIds)
                {
                    try
                    {
                        var role = guild.GetRole(roleId);
                        if (role is null || role.IsModeratorRole()) continue;

                        await target.AddOverwriteAsync(role, deny: Permissions.AccessChannels);
                        await Task.Delay(250);
                    }
                    catch
                    {
                        // ignored
                    }
                }
        }
        catch
        {
            owner = null;
            // ignored
        }

        await Task.Delay(500);

        await target.ModifyAsync(x =>
            x.Name = $"{guildCfg.TicketingConfig.ClosedNamePrefix}-{ticket.GuildSpecificId:D4}");


        var closedCatRes = await _requestDataProvider.GetChannelAsync(guildCfg.TicketingConfig.ClosedCategoryId);
        if (!closedCatRes.IsDefined(out var closedCat))
            return new DiscordNotFoundError(
                $"Closed category channel with Id {guildCfg.TicketingConfig.ClosedCategoryId} doesn't exist");

        await target.ModifyAsync(x => x.Parent = closedCat);

        command.Dto.ClosedMessageId = closeMsg.Id;
        await _ticketDataService.CloseAsync(command.Dto, ticket);

        if (ticket.IsPrivate) return Result.FromSuccess();

        _ = _asyncExecutor.ExecuteAsync<IDiscordChatExportService>(async x => await x.ExportToHtmlAsync(guild,
            target, requestingMember,
            owner ?? await _discord.Client.GetUserAsync(ticket.UserId), ticket));

        return Result.FromSuccess();
    }
}
