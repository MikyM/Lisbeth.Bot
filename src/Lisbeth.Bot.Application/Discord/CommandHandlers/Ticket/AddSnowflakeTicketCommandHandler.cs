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
using DSharpPlus;
using DSharpPlus.Entities;
using Lisbeth.Bot.Application.Discord.Commands.Ticket;
using Lisbeth.Bot.DataAccessLayer.Specifications.Guild;
using Lisbeth.Bot.DataAccessLayer.Specifications.Ticket;
using Microsoft.Extensions.Logging;
using MikyM.Common.Application.CommandHandlers;
using MikyM.Common.Utilities.Results;
using MikyM.Common.Utilities.Results.Errors;
using MikyM.Discord.Enums;

namespace Lisbeth.Bot.Application.Discord.CommandHandlers.Ticket;

[UsedImplicitly]
public class AddSnowflakeTicketCommandHandler : ICommandHandler<AddSnowflakeToTicketCommand,  DiscordEmbed>
{
    private readonly IGuildDataService _guildDataService;
    private readonly ITicketDataService _ticketDataService;
    private readonly ILogger<AddSnowflakeTicketCommandHandler> _logger;
    private readonly IDiscordGuildRequestDataProvider _requestDataProvider;
    private readonly ICommandHandler<PrivacyCheckTicketCommand, bool> _privacyCheckHandler;

    public AddSnowflakeTicketCommandHandler(IGuildDataService guildDataService,
        ILogger<AddSnowflakeTicketCommandHandler> logger, IDiscordGuildRequestDataProvider requestDataProvider,
        ITicketDataService ticketDataService,
        ICommandHandler<PrivacyCheckTicketCommand, bool> privacyCheckHandler)
    {
        _guildDataService = guildDataService;
        _logger = logger;
        _requestDataProvider = requestDataProvider;
        _ticketDataService = ticketDataService;
        _privacyCheckHandler = privacyCheckHandler;
    }

    public async Task<Result<DiscordEmbed>> HandleAsync(AddSnowflakeToTicketCommand command)
    {
        if (command is null) throw new ArgumentNullException(nameof(command));

        var guildRes =
            await _guildDataService.GetSingleBySpecAsync<Guild>(
                new ActiveGuildByDiscordIdWithTicketingSpecifications(command.Dto.GuildId));

        if (!guildRes.IsDefined(out var guildCfg)) return Result<DiscordEmbed>.FromError(guildRes);

        if (guildCfg.TicketingConfig is null)
            return new DisabledEntityError($"Guild with Id:{command.Dto.GuildId} doesn't have ticketing enabled.");

        var res = await _ticketDataService.GetSingleBySpecAsync<Domain.Entities.Ticket>(
            new TicketByChannelIdOrGuildAndOwnerIdSpec(command.Dto.ChannelId, command.Dto.GuildId, command.Dto.OwnerId));

        if (!res.IsDefined(out var ticket)) return new NotFoundError("Ticket with given params doesn't exist.");

        if (ticket.IsDisabled)
            return new DisabledEntityError(
                $"Ticket with Id: {ticket.GuildSpecificId}, TargetUserId: {ticket.UserId}, GuildId: {ticket.GuildId}, ChannelId: {ticket.ChannelId} is already closed.");

        // data req
        var initRes = await _requestDataProvider.InitializeAsync(command.Dto, command.InteractionContext);
        if (!initRes.IsSuccess)
            return Result<DiscordEmbed>.FromError(initRes);

        DiscordGuild guild = _requestDataProvider.DiscordGuild;
        DiscordMember requestingMember = _requestDataProvider.RequestingMember;

        var channelRes = await _requestDataProvider.GetChannelAsync(ticket.ChannelId);
        if (!channelRes.IsDefined(out var ticketChannel))
            return Result<DiscordEmbed>.FromError(channelRes);

        var snowflakeRes = await _requestDataProvider.GetFirstResolvedRoleOrMemberOrAsync(command.Dto.SnowflakeId);
        if (!snowflakeRes.IsDefined(out var snowflake))
            return Result<DiscordEmbed>.FromError(snowflakeRes);

        if (!requestingMember.Permissions.HasPermission(Permissions.BanMembers))
            return new DiscordNotAuthorizedError("Requesting member doesn't have moderator rights.");

        DiscordRole? targetRole = null;
        DiscordMember? targetMember = null;

        switch (snowflake.Type)
        {
            case DiscordEntity.User:
            case DiscordEntity.Member:
                targetMember = (DiscordMember)snowflake.Snowflake;
                await ticketChannel.AddOverwriteAsync(targetMember, Permissions.AccessChannels);

                // give discord half a second and refresh channel
                await Task.Delay(1000);

                await _ticketDataService.SetAddedUsersAsync(ticket,
                    ticketChannel.Users.Any(x => x.Id == targetMember.Id)
                        ? ticketChannel.Users.Select(x => x.Id)
                        : ticketChannel.Users.Select(x => x.Id).Append(targetMember.Id));

                var privacyMemberRes = await _privacyCheckHandler.HandleAsync(new PrivacyCheckTicketCommand(guild, ticket));

                if (privacyMemberRes.IsDefined(out var isPrivateMember))

                    await _ticketDataService.SetPrivacyAsync(ticket, isPrivateMember, true);
                break;
            case DiscordEntity.Role:
                targetRole = (DiscordRole)snowflake.Snowflake;
                await ticketChannel.AddOverwriteAsync(targetRole, Permissions.AccessChannels);

                // give discord half a second and refresh channel
                await Task.Delay(1000);

                List<ulong> roleIds = new();
                foreach (var overwrite in ticketChannel.PermissionOverwrites)
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

                if (roleIds.All(x => x != targetRole.Id))
                    roleIds.Add(targetRole.Id);

                await _ticketDataService.SetAddedRolesAsync(ticket, roleIds);

                var privacyRes = await _privacyCheckHandler.HandleAsync(new PrivacyCheckTicketCommand(guild, ticket));

                if (privacyRes.IsDefined(out var isPrivate))

                    await _ticketDataService.SetPrivacyAsync(ticket, isPrivate, true);
                break;
            default:
                throw new ArgumentOutOfRangeException();
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
