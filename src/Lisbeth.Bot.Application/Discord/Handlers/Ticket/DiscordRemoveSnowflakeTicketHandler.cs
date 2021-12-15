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

using System.Collections.Generic;
using DSharpPlus;
using DSharpPlus.Entities;
using Lisbeth.Bot.Application.Discord.Handlers.Ticket.Interfaces;
using Lisbeth.Bot.Application.Discord.Requests.Ticket;
using Lisbeth.Bot.DataAccessLayer.Specifications.Guild;
using Lisbeth.Bot.DataAccessLayer.Specifications.Ticket;
using Microsoft.Extensions.Logging;
using MikyM.Discord.Interfaces;

namespace Lisbeth.Bot.Application.Discord.Handlers.Ticket;

[UsedImplicitly]
public class DiscordRemoveSnowflakeTicketHandler : IDiscordRemoveSnowflakeTicketHandler
{
    private readonly IGuildDataService _guildDataService;
    private readonly ITicketDataService _ticketDataService;
    private readonly ILogger<DiscordRemoveSnowflakeTicketHandler> _logger;
    private readonly IDiscordService _discord;

    public DiscordRemoveSnowflakeTicketHandler(IGuildDataService guildDataService,
        ILogger<DiscordRemoveSnowflakeTicketHandler> logger, IDiscordService discord, ITicketDataService ticketDataService)
    {
        _guildDataService = guildDataService;
        _logger = logger;
        _discord = discord;
        _ticketDataService = ticketDataService;
    }

    public async Task<Result<DiscordEmbed>> HandleAsync(RemoveSnowflakeFromTicketRequest request)
    {
        if (request is null) throw new ArgumentNullException(nameof(request));

        var guildRes =
            await _guildDataService.GetSingleBySpecAsync<Guild>(
                new ActiveGuildByDiscordIdWithTicketingSpecifications(request.Dto.GuildId));

        if (!guildRes.IsDefined(out var guildCfg)) return Result<DiscordEmbed>.FromError(guildRes);

        if (guildCfg.TicketingConfig is null)
            return new DisabledEntityError($"Guild with Id:{request.Dto.GuildId} doesn't have ticketing enabled.");

        var res = await _ticketDataService.GetSingleBySpecAsync<Domain.Entities.Ticket>(
            new TicketBaseGetSpecifications(null, request.Dto.OwnerId, request.Dto.GuildId, request.Dto.ChannelId));

        if (!res.IsDefined(out var ticket)) return new NotFoundError($"Ticket with given params doesn't exist.");

        if (ticket.IsDisabled)
            return new DisabledEntityError(
                $"Ticket with Id: {ticket.GuildSpecificId}, TargetUserId: {ticket.UserId}, GuildId: {ticket.GuildId}, ChannelId: {ticket.ChannelId} is already closed.");

        // data req
        DiscordGuild guild = request.InteractionContext?.Guild ?? await _discord.Client.GetGuildAsync(request.Dto.GuildId);
        DiscordMember requestingMember = request.InteractionContext?.Member ?? await guild.GetMemberAsync(request.Dto.RequestedOnBehalfOfId);
        if (!requestingMember.Permissions.HasPermission(Permissions.BanMembers))
            return new DiscordNotAuthorizedError("Requesting member doesn't have moderator rights.");
        DiscordChannel ticketChannel = guild.GetChannel(ticket.ChannelId);
        DiscordRole? targetRole = request.InteractionContext?.ResolvedRoleMentions?[0] ?? guild.GetRole(request.Dto.SnowflakeId);
        DiscordMember? targetMember = request.InteractionContext?.ResolvedUserMentions?[0] as DiscordMember ?? await guild.GetMemberAsync(request.Dto.SnowflakeId);

        if (targetMember is null && targetRole is null)
            return new DiscordNotFoundError("Didn't find any roles or members with given snowflake Id");

        if (targetRole is null)
        {
            await ticketChannel.AddOverwriteAsync(targetMember, deny: Permissions.AccessChannels);
            await _ticketDataService.SetAddedUsersAsync(ticket, ticketChannel.Users.Select(x => x.Id));
            await _ticketDataService.CheckAndSetPrivacyAsync(ticket, guild);
        }
        else
        {
            await ticketChannel.AddOverwriteAsync(targetRole, deny: Permissions.AccessChannels);

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

            await _ticketDataService.SetAddedRolesAsync(ticket, roleIds);
            await _ticketDataService.CheckAndSetPrivacyAsync(ticket, guild);
        }

        var embed = new DiscordEmbedBuilder();

        embed.WithColor(new DiscordColor(guildCfg.EmbedHexColor));
        embed.WithAuthor($"Ticket moderation | Remove {(targetRole is null ? "member" : "role")} action log");
        embed.AddField("Moderator", requestingMember.Mention);
        embed.AddField("Removed", $"{targetRole?.Mention ?? targetMember?.Mention}");
        embed.WithFooter($"Ticket Id: {ticket.GuildSpecificId}");

        return embed.Build();
    }
}
