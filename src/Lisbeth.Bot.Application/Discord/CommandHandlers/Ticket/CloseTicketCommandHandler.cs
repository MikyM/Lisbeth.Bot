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

using DSharpPlus;
using DSharpPlus.Entities;
using Lisbeth.Bot.Application.Discord.Commands.Ticket;
using Lisbeth.Bot.Application.Discord.Helpers.InteractionIdEnums.Buttons;
using Lisbeth.Bot.DataAccessLayer.Specifications.Guild;
using Microsoft.Extensions.Logging;

namespace Lisbeth.Bot.Application.Discord.CommandHandlers.Ticket;

[UsedImplicitly]
public class CloseTicketCommandHandler : ICommandHandler<CloseTicketCommand>
{
    private readonly IGuildDataService _guildDataService;
    private readonly ILogger<CloseTicketCommandHandler> _logger;

    public CloseTicketCommandHandler(IGuildDataService guildDataService,
        ILogger<CloseTicketCommandHandler> logger)
    {
        _guildDataService = guildDataService;
        _logger = logger;
    }

    public async Task<Result> HandleAsync(CloseTicketCommand command, CancellationToken cancellationToken = default)
    {
        var guildRes =
            await _guildDataService.GetSingleBySpecAsync(
                new ActiveGuildByDiscordIdWithTicketingSpecifications(command.Interaction.Guild.Id));

        if (!guildRes.IsDefined(out var guildCfg)) return Result.FromError(guildRes);

        if (guildCfg.TicketingConfig is null)
            return new DisabledEntityError($"Guild with Id:{command.Interaction.Guild.Id} doesn't have ticketing enabled.");

        var confirmButton = new DiscordButtonComponent(ButtonStyle.Success, nameof(TicketButton.TicketCloseConfirm),
            "Yes");

        var embed = new DiscordEmbedBuilder().WithColor(new DiscordColor(guildCfg.EmbedHexColor))
            .WithDescription("Are you sure you want to close this ticket?")
            .Build();

        await command.Interaction.CreateFollowupMessageAsync(new DiscordFollowupMessageBuilder().AddEmbed(embed)
            .AddComponents(confirmButton));
        
        return Result.FromSuccess();
    }
}
