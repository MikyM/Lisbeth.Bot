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
using Lisbeth.Bot.Application.Discord.Helpers.InteractionIdEnums.Buttons;
using Lisbeth.Bot.Application.Discord.Requests.Ticket;
using Lisbeth.Bot.DataAccessLayer.Specifications.Guild;
using MikyM.Discord.Interfaces;

namespace Lisbeth.Bot.Application.Discord.Handlers.Ticket;

[UsedImplicitly]
public class DiscordGetWelcomeEmbedTicketHandler : IDiscordGetWelcomeEmbedTicketHandler
{
    private readonly IGuildDataService _guildDataService;
    private readonly IDiscordEmbedProvider _embedProvider;
    private readonly IDiscordService _discord;

    public DiscordGetWelcomeEmbedTicketHandler(IGuildDataService guildDataService, IDiscordEmbedProvider embedProvider,
        IDiscordService discord)
    {
        _guildDataService = guildDataService;
        _embedProvider = embedProvider;
        _discord = discord;
    }

    public async Task<Result<DiscordMessageBuilder>> HandleAsync(TicketWelcomeEmbedRequest request)
    {
        var res = await _guildDataService.GetSingleBySpecAsync<Guild>(
            new ActiveGuildByDiscordIdWithTicketingSpecifications(request.GuildId));

        if (!res.IsDefined(out var guild)) return Result<DiscordMessageBuilder>.FromError(res);
        if (guild.TicketingConfig is null)
            return new DisabledEntityError("Guild doesn't have ticketing configured");

        var envelopeEmoji = DiscordEmoji.FromName(_discord.Client, ":envelope:");
        var embed = new DiscordEmbedBuilder();

        if (guild.TicketingConfig.WelcomeEmbedConfig is not null)
        {
            embed = _embedProvider.GetEmbedFromConfig(guild.TicketingConfig.WelcomeEmbedConfig);
            embed.WithDescription(embed.Description.Replace("@ownerMention@", request.Owner.Mention));
        }
        else
        {
            embed.WithColor(new DiscordColor(guild.EmbedHexColor));
            embed.WithDescription(
                guild.TicketingConfig.BaseWelcomeMessage.Replace("@ownerMention@", request.Owner.Mention));
            embed.WithColor(new DiscordColor(guild.EmbedHexColor));
        }

        embed.WithFooter($"Ticket Id: {request.GuildSpecificId}");

        var btn = new DiscordButtonComponent(ButtonStyle.Primary, nameof(TicketButton.TicketOpen), "Open a ticket", false,
            new DiscordComponentEmoji(envelopeEmoji));
        var builder = new DiscordMessageBuilder();
        builder.AddEmbed(embed.Build());
        builder.AddComponents(btn);
        builder.WithContent($"{request.Owner.Mention} Welcome");

        return builder;
    }
}
