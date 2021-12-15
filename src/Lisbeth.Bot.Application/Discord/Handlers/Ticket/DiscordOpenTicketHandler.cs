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
using Lisbeth.Bot.Application.Discord.Handlers.Ticket.Interfaces;
using Lisbeth.Bot.Application.Discord.Helpers;
using Lisbeth.Bot.Application.Discord.Requests.Ticket;
using Lisbeth.Bot.DataAccessLayer.Specifications.Guild;
using Microsoft.Extensions.Logging;
using MikyM.Common.Utilities.Extensions;
using MikyM.Discord.Extensions.BaseExtensions;
using MikyM.Discord.Interfaces;
using System.Collections.Generic;

namespace Lisbeth.Bot.Application.Discord.Handlers.Ticket;

[UsedImplicitly]
public class DiscordOpenTicketHandler : IDiscordOpenTicketHandler
{
    private readonly IDiscordService _discord;
    private readonly IGuildDataService _guildDataService;
    private readonly ITicketDataService _ticketDataService;
    private readonly ILogger<DiscordOpenTicketHandler> _logger;
    private readonly IDiscordGetWelcomeEmbedTicketHandler _welcomeEmbedHandler;

    public DiscordOpenTicketHandler(IGuildDataService guildDataService, ITicketDataService ticketDataService,
        IDiscordService discord, ILogger<DiscordOpenTicketHandler> logger,
        IDiscordGetWelcomeEmbedTicketHandler welcomeEmbedHandler)
    {
        _guildDataService = guildDataService;
        _ticketDataService = ticketDataService;
        _discord = discord;
        _logger = logger;
        _welcomeEmbedHandler = welcomeEmbedHandler;
    }

    public async Task<Result<DiscordMessageBuilder>> HandleAsync(OpenTicketRequest request)
    {
        if (request is null) throw new ArgumentNullException(nameof(request));

        // data req
        DiscordGuild guild = request.Interaction?.Guild ?? await _discord.Client.GetGuildAsync(request.Dto.GuildId);
        DiscordMember owner = request.Interaction?.User as DiscordMember ??
                              await guild.GetMemberAsync(request.Dto.RequestedOnBehalfOfId);

        if (owner.Guild.Id != guild.Id) return new DiscordNotAuthorizedError(nameof(owner));

        var guildRes =
            await _guildDataService.GetSingleBySpecAsync(
                new ActiveGuildByDiscordIdWithTicketingSpecifications(guild.Id));

        if (!guildRes.IsDefined(out var guildCfg)) return Result<DiscordMessageBuilder>.FromError(guildRes);

        if (guildCfg.TicketingConfig is null)
            return new DisabledEntityError($"Guild with Id:{guild.Id} doesn't have ticketing enabled.");

        var ticketRes = await _ticketDataService.OpenAsync(request.Dto);
        if (!ticketRes.IsDefined(out var ticket))
        {
            var failEmbed = new DiscordEmbedBuilder();
            failEmbed.WithColor(new DiscordColor(guildCfg.EmbedHexColor));
            failEmbed.WithDescription("You already have an opened ticket in this guild.");
            if (request.Interaction is not null)
                await request.Interaction.CreateFollowupMessageAsync(new DiscordFollowupMessageBuilder()
                    .AddEmbed(failEmbed.Build())
                    .AsEphemeral(true));
            return new InvalidOperationError("Member already has an opened ticket in this guild.");
        }

        request.Dto.GuildSpecificId = guildCfg.TicketingConfig.LastTicketId + 1;
        _guildDataService.BeginUpdate(guildCfg);
        guildCfg.TicketingConfig.LastTicketId++;
        await _guildDataService.CommitAsync();

        var msgRes =
            await _welcomeEmbedHandler.HandleAsync(new TicketWelcomeEmbedRequest(guild.Id,
                request.Dto.GuildSpecificId.Value, owner));

        if (!msgRes.IsDefined(out var message))
        {
            message = new DiscordMessageBuilder();
            message.WithContent("to do");
        }

        var modRoles = guild.Roles.Where(x => x.Value.Permissions.HasPermission(Permissions.BanMembers));

        List<DiscordOverwriteBuilder> overwrites = modRoles.Select(role =>
            new DiscordOverwriteBuilder(role.Value).Allow(Permissions.AccessChannels)).ToList();
        overwrites.Add(new DiscordOverwriteBuilder(guild.EveryoneRole).Deny(Permissions.AccessChannels));
        overwrites.Add(new DiscordOverwriteBuilder(owner).Allow(Permissions.AccessChannels));

        string topic = $"Support ticket opened by user {owner.GetFullUsername()} at {DateTime.UtcNow}";

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

        try
        {
            DiscordChannel newTicketChannel = await guild.CreateChannelAsync(
                $"{guildCfg.TicketingConfig.OpenedNamePrefix}-{request.Dto.GuildSpecificId:D4}", ChannelType.Text,
                openedCat, topic, null, null, overwrites);
            DiscordMessage msg =
                await newTicketChannel.SendMessageAsync(message);
            //Program.cachedMsgs.Add(msg.Id, msg);

            if (request.Interaction is not null)
            {
                var succEmbed = new DiscordEmbedBuilder();
                succEmbed.WithColor(new DiscordColor(guildCfg.EmbedHexColor));
                succEmbed.WithDescription($"Ticket created successfully! Channel: {newTicketChannel.Mention}");
                await request.Interaction.CreateFollowupMessageAsync(new DiscordFollowupMessageBuilder()
                    .AddEmbed(succEmbed.Build())
                    .AsEphemeral(true));
            }

            _ticketDataService.BeginUpdate(ticket);
            ticket.ChannelId = newTicketChannel.Id;
            ticket.MessageOpenId = msg.Id;
            await _ticketDataService.SetAddedUsersAsync(ticket, newTicketChannel.Users.Select(x => x.Id));

            List<ulong> roleIds = new();
            foreach (var overwrite in newTicketChannel.PermissionOverwrites)
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
            await _ticketDataService.CommitAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError($"Errored while opening new ticket: {ex.GetFullMessage()}");
            return ex;
        }

        return message;
    }
}
