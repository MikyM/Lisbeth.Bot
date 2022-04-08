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

using DSharpPlus.Entities;
using MikyM.Discord.EmbedBuilders.Enrichers;
using MikyM.Discord.EmbedBuilders.Wrappers;
using MikyM.Discord.Extensions.BaseExtensions;

namespace Lisbeth.Bot.Application.Discord.EmbedEnrichers.DirectMessage;

public class MessageFormatComplianceEmbedEnricher : EmbedEnricherBase<ChannelMessageFormat, DiscordMessage>
{

    public override void Enrich(IDiscordEmbedBuilderWrapper embedBuilder)
    {
        embedBuilder.WithAuthor($"Channel message format warning | {SecondaryEnricher.Author.GetFullUsername()}",
            null, SecondaryEnricher.Author.AvatarUrl);

        embedBuilder.WithDescription(
            $"Your message in {SecondaryEnricher.Channel.Mention} channel was deleted, because it was not compliant with the message format defined for this channel.");

        embedBuilder.AddField("Channel", SecondaryEnricher.Channel.Mention);
        embedBuilder.AddField("Required format", PrimaryEnricher.MessageFormat ?? "Unknown");
        embedBuilder.AddField("Your message", SecondaryEnricher.Content);

        embedBuilder.WithFooter(
            $"Guild: {SecondaryEnricher.Channel.Guild.Name} | Channel Id: {SecondaryEnricher.Channel.Id} | Message Id: {SecondaryEnricher.Id}");
    }

    public MessageFormatComplianceEmbedEnricher(ChannelMessageFormat primaryEnricher, DiscordMessage secondaryEnricher)
        : base(primaryEnricher, secondaryEnricher)
    {
    }
}
