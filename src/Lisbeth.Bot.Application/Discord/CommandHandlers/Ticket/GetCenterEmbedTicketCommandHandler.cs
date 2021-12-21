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
using Lisbeth.Bot.Application.Discord.Helpers;
using Lisbeth.Bot.Application.Discord.Helpers.InteractionIdEnums.Buttons;
using Lisbeth.Bot.DataAccessLayer.Specifications.Guild;
using MikyM.Common.Application.CommandHandlers;

namespace Lisbeth.Bot.Application.Discord.CommandHandlers.Ticket;

[UsedImplicitly]
public class GetCenterEmbedTicketCommandHandler : ICommandHandler<GetTicketCenterEmbedCommand, DiscordMessageBuilder>
{
    private readonly IGuildDataService _guildDataService;
    private readonly IDiscordEmbedProvider _embedProvider;

    public GetCenterEmbedTicketCommandHandler(IGuildDataService guildDataService, IDiscordEmbedProvider embedProvider)
    {
        _guildDataService = guildDataService;
        _embedProvider = embedProvider;
    }

    public async Task<Result<DiscordMessageBuilder>> HandleAsync(GetTicketCenterEmbedCommand command)
    {
        var res = await _guildDataService.GetSingleBySpecAsync<Guild>(
            new ActiveGuildByDiscordIdWithTicketingSpecifications(command.InteractionContext.Guild.Id));

        if (!res.IsDefined(out var guild)) return Result<DiscordMessageBuilder>.FromError(res);
        if (res.Entity.TicketingConfig is null)
            return new DisabledEntityError("Guild doesn't have ticketing configured");

        var envelopeEmoji = DiscordEmoji.FromName(command.InteractionContext.Client, ":envelope:");
        var embed = new DiscordEmbedBuilder();

        if (res.Entity.TicketingConfig.CenterEmbedConfig is not null)
        {
            embed = _embedProvider.GetEmbedFromConfig(res.Entity.TicketingConfig.CenterEmbedConfig);
        }
        else
        {
            embed.WithTitle($"__{command.InteractionContext.Guild.Name}'s Support Ticket Center__");
            embed.WithDescription(res.Entity.TicketingConfig.BaseCenterMessage);
            embed.WithColor(new DiscordColor(guild.EmbedHexColor));
        }

        embed.WithFooter("Click on the button below to open a ticket");

        var btn = new DiscordButtonComponent(ButtonStyle.Primary, nameof(TicketButton.TicketOpen), "Open a ticket", false,
            new DiscordComponentEmoji(envelopeEmoji));
        var builder = new DiscordMessageBuilder();
        builder.AddEmbed(embed.Build());
        builder.AddComponents(btn);

        return builder;
    }
}
