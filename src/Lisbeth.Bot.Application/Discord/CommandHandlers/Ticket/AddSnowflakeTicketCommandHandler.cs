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

using System.Collections.Generic;
using DSharpPlus;
using DSharpPlus.Entities;
using Lisbeth.Bot.Application.Discord.Commands.Ticket;
using Lisbeth.Bot.DataAccessLayer.Specifications.Guild;
using Lisbeth.Bot.DataAccessLayer.Specifications.Ticket;
using Microsoft.Extensions.Logging;
using MikyM.Common.Application.CommandHandlers;
using MikyM.Discord.Interfaces;

namespace Lisbeth.Bot.Application.Discord.CommandHandlers.Ticket;

[UsedImplicitly]
public class AddSnowflakeTicketCommandHandler : ICommandHandler<AddSnowflakeToTicketCommand,  DiscordEmbed>
{
    private readonly IGuildService _guildService;
    private readonly ITicketService _ticketService;
    private readonly ILogger<AddSnowflakeTicketCommandHandler> _logger;
    private readonly IDiscordService _discord;
    private readonly ICommandHandler<PrivacyCheckTicketCommand, bool> _privacyCheckHandler;

    public AddSnowflakeTicketCommandHandler(IGuildService guildService,
        ILogger<AddSnowflakeTicketCommandHandler> logger, IDiscordService discord,
        ITicketService ticketService,
        ICommandHandler<PrivacyCheckTicketCommand, bool> privacyCheckHandler)
    {
        _guildService = guildService;
        _logger = logger;
        _discord = discord;
        _ticketService = ticketService;
        _privacyCheckHandler = privacyCheckHandler;
    }

    public async Task<Result<DiscordEmbed>> HandleAsync(AddSnowflakeToTicketCommand command)
    {
        if (command is null) throw new ArgumentNullException(nameof(command));

        var guildRes =
            await _guildService.GetSingleBySpecAsync<Guild>(
                new ActiveGuildByDiscordIdWithTicketingSpecifications(command.Dto.GuildId));

        if (!guildRes.IsDefined(out var guildCfg)) return Result<DiscordEmbed>.FromError(guildRes);

        if (guildCfg.TicketingConfig is null)
            return new DisabledEntityError($"Guild with Id:{command.Dto.GuildId} doesn't have ticketing enabled.");

        var res = await _ticketService.GetSingleBySpecAsync<Domain.Entities.Ticket>(
            new TicketByChannelIdOrGuildAndOwnerIdSpec(command.Dto.ChannelId, command.Dto.GuildId, command.Dto.OwnerId));

        if (!res.IsDefined(out var ticket)) return new NotFoundError($"Ticket with given params doesn't exist.");

        if (ticket.IsDisabled)
            return new DisabledEntityError(
                $"Ticket with Id: {ticket.GuildSpecificId}, TargetUserId: {ticket.UserId}, GuildId: {ticket.GuildId}, ChannelId: {ticket.ChannelId} is already closed.");

        // data req
        DiscordGuild guild = command.InteractionContext?.Guild ?? await _discord.Client.GetGuildAsync(command.Dto.GuildId);
        DiscordMember requestingMember = command.InteractionContext?.Member ?? await guild.GetMemberAsync(command.Dto.RequestedOnBehalfOfId);
        if (!requestingMember.Permissions.HasPermission(Permissions.BanMembers))
            return new DiscordNotAuthorizedError("Requesting member doesn't have moderator rights.");
        DiscordChannel ticketChannel = guild.GetChannel(ticket.ChannelId);
        DiscordRole? targetRole = command.InteractionContext?.ResolvedRoleMentions?[0] ?? guild.GetRole(command.Dto.SnowflakeId);
        DiscordMember? targetMember = command.InteractionContext?.ResolvedUserMentions?[0] as DiscordMember ?? await guild.GetMemberAsync(command.Dto.SnowflakeId);

        if (targetMember is null && targetRole is null)
            return new DiscordNotFoundError("Didn't find any roles or members with given snowflake Id");

        if (targetRole is null && targetMember is not null)
        {
            await ticketChannel.AddOverwriteAsync(targetMember, Permissions.AccessChannels);

            // give discord half a second and refresh channel
            await Task.Delay(1000);
            ticketChannel = guild.GetChannel(ticket.ChannelId);

            await _ticketService.SetAddedUsersAsync(ticket,
                ticketChannel.Users.Any(x => x.Id == targetMember.Id)
                    ? ticketChannel.Users.Select(x => x.Id)
                    : ticketChannel.Users.Select(x => x.Id).Append(targetMember.Id));

            var privacyRes = await _privacyCheckHandler.HandleAsync(new PrivacyCheckTicketCommand(guild, ticket));

            if (privacyRes.IsDefined(out var isPrivate))

            await _ticketService.SetPrivacyAsync(ticket, isPrivate, true);
        }
        else if (targetRole is not null)
        {
            await ticketChannel.AddOverwriteAsync(targetRole, Permissions.AccessChannels);

            // give discord half a second and refresh channel
            await Task.Delay(1000);
            ticketChannel = guild.GetChannel(ticket.ChannelId);

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

            await _ticketService.SetAddedRolesAsync(ticket, roleIds);

            var privacyRes = await _privacyCheckHandler.HandleAsync(new PrivacyCheckTicketCommand(guild, ticket));

            if (privacyRes.IsDefined(out var isPrivate))

                await _ticketService.SetPrivacyAsync(ticket, isPrivate, true);
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
