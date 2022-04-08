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

using Lisbeth.Bot.Application.Discord.Helpers;
using Lisbeth.Bot.Domain.DTOs.Request.Prune;
using MikyM.Discord.EmbedBuilders.Wrappers;
using MikyM.Discord.Enums;

namespace Lisbeth.Bot.Application.Discord.EmbedEnrichers.Log.Moderation;

public class PruneModAddReqLogEnricher : EmbedEnricher<PruneReqDto>
{
    public PruneModAddReqLogEnricher(PruneReqDto request) : base(request)
    {
    }

    public override void Enrich(IDiscordEmbedBuilderWrapper embedBuilder)
    {
        var (name, pastTense) = GetUnderlyingNameAndPastTense();

        embedBuilder.AddField("Moderator",
            ExtendedFormatter.Mention(PrimaryEnricher.RequestedOnBehalfOfId, DiscordEntity.Member), true);

        embedBuilder.AddField("Channel",
            ExtendedFormatter.Mention(PrimaryEnricher.ChannelId, DiscordEntity.Channel), true);

        if (PrimaryEnricher.Count.HasValue)
            embedBuilder.AddField("Message count",
                PrimaryEnricher.Count.Value.ToString(), true);

        if (PrimaryEnricher.TargetAuthorId.HasValue)
            embedBuilder.AddField("Target author",
                ExtendedFormatter.Mention(PrimaryEnricher.TargetAuthorId.Value, DiscordEntity.Member), true);

        if (PrimaryEnricher.MessageId.HasValue)
            embedBuilder.AddField("Target message Id",
                PrimaryEnricher.MessageId.Value.ToString(), true);

        if (PrimaryEnricher.IsTargetedMessageDelete.HasValue)
            embedBuilder.AddField("Is targeted message delete",
            PrimaryEnricher.IsTargetedMessageDelete.Value.ToString(), true);

        embedBuilder.WithFooter();
    }
}
