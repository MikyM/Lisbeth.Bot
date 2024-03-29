﻿// This file is part of Lisbeth.Bot project
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

using Lisbeth.Bot.Application.Discord.Helpers;
using Lisbeth.Bot.Domain.DTOs.Request.Prune;
using MikyM.Discord.EmbedBuilders.Wrappers;

namespace Lisbeth.Bot.Application.Discord.EmbedEnrichers.Response.Prune;

public class PruneReqResponseEnricher : EmbedEnricher<PruneReqDto>
{
    private readonly int _count;

    public PruneReqResponseEnricher(PruneReqDto entity, int count) : base(entity)
        => _count = count;

    public override void Enrich(IDiscordEmbedBuilderWrapper embedBuilder)
    {
        embedBuilder.AddField("Moderator",
            ExtendedFormatter.Mention(PrimaryEnricher.RequestedOnBehalfOfId, DiscordEntity.User), true);
        embedBuilder.AddField("Number of deleted messages", _count.ToString(), true);
        embedBuilder.AddField("Channel",
            ExtendedFormatter.Mention(PrimaryEnricher.ChannelId, DiscordEntity.Channel), true);

        if (PrimaryEnricher.TargetAuthorId is null)
            return;

        embedBuilder.AddField("Target author mention",
            ExtendedFormatter.Mention(PrimaryEnricher.TargetAuthorId.Value, DiscordEntity.User), true);
        embedBuilder.AddInvisibleField();
        embedBuilder.AddField("Target author ID and profile",
            $"[{PrimaryEnricher.TargetAuthorId.Value}](https://discordapp.com/users/{PrimaryEnricher.TargetAuthorId.Value})",
            true);
    }
}
