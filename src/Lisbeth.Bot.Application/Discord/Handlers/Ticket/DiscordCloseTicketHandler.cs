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
using DSharpPlus.Interactivity.Extensions;
using Lisbeth.Bot.Application.Discord.Handlers.Ticket.Interfaces;
using Lisbeth.Bot.Application.Discord.Helpers.InteractionIdEnums.Buttons;
using Lisbeth.Bot.Application.Discord.Requests.Ticket;
using Lisbeth.Bot.DataAccessLayer.Specifications.Guild;
using Microsoft.Extensions.Logging;
using MikyM.Discord.Interfaces;

namespace Lisbeth.Bot.Application.Discord.Handlers.Ticket;

[UsedImplicitly]
public class DiscordCloseTicketHandler : IDiscordCloseTicketHandler
{
    private readonly IGuildDataService _guildDataService;
    private readonly ILogger<DiscordCloseTicketHandler> _logger;
    private readonly IDiscordService _discord;

    public DiscordCloseTicketHandler(IGuildDataService guildDataService,
        ILogger<DiscordCloseTicketHandler> logger, IDiscordService discord)
    {
        _guildDataService = guildDataService;
        _logger = logger;
        _discord = discord;
    }

    public async Task<Result> HandleAsync(CloseTicketRequest requestBase)
    {
        var guildRes =
            await _guildDataService.GetSingleBySpecAsync(
                new ActiveGuildByDiscordIdWithTicketingSpecifications(requestBase.Interaction.Guild.Id));

        if (!guildRes.IsDefined(out var guildCfg)) return Result.FromError(guildRes);

        if (guildCfg.TicketingConfig is null)
            return new DisabledEntityError($"Guild with Id:{requestBase.Interaction.Guild.Id} doesn't have ticketing enabled.");

        var confirmButton = new DiscordButtonComponent(ButtonStyle.Success, nameof(TicketButton.TicketCloseConfirm),
            "Close this ticket");
        var rejectButton = new DiscordButtonComponent(ButtonStyle.Danger, nameof(TicketButton.TicketCloseReject),
            "Close this ticket");

        var embed = new DiscordEmbedBuilder().WithColor(new DiscordColor(guildCfg.EmbedHexColor))
            .WithDescription("Are you sure you want to close this ticket?")
            .Build();

        var msg = await requestBase.Interaction.CreateFollowupMessageAsync(new DiscordFollowupMessageBuilder().AddEmbed(embed)
            .AddComponents(confirmButton, rejectButton));

        var intr = _discord.Client.GetInteractivity();

        var timeout = await intr.WaitForButtonAsync(msg, msg.Author, TimeSpan.FromSeconds(20));

        if (timeout.TimedOut)
            await requestBase.Interaction.Channel.DeleteMessageAsync(msg);
        
        return Result.FromSuccess();
    }
}
