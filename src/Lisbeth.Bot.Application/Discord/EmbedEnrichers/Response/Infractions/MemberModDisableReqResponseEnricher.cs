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

using DSharpPlus.Entities;
using Lisbeth.Bot.Application.Discord.Helpers;
using Lisbeth.Bot.Domain.DTOs.Request.Base;
using MikyM.Discord.EmbedBuilders.Wrappers;
using MikyM.Discord.Enums;

namespace Lisbeth.Bot.Application.Discord.EmbedEnrichers.Response.Infractions;

public class MemberModDisableReqResponseEnricher : EmbedEnricher<IRevokeInfractionReq>
{
    public DiscordUser? Target { get; }

    public MemberModDisableReqResponseEnricher(IRevokeInfractionReq request, DiscordUser? target) : base(request) =>
        Target = target;

    public override void Enrich(IDiscordEmbedBuilderWrapper embedBuilder)
    {
        embedBuilder.WithDescription($"Successfully {GetModerationTypeAndPastTense().PastTense.ToLower()}");
        
        embedBuilder.AddField("Target user mention", Target?.Mention ?? "Unknown", true);
        embedBuilder.AddInvisibleField();
        embedBuilder.AddField("Target user ID and profile", $"{(Target?.Mention is not null ? $"[{Target.Id}](https://discordapp.com/users/{Target.Id})" : "Unknown")}", true);
        
        embedBuilder.AddField("Moderator",
            ExtendedFormatter.Mention(PrimaryEnricher.RequestedOnBehalfOfId, DiscordEntity.Member), true);
        embedBuilder.AddInvisibleField();
        embedBuilder.AddField("Moderator user ID and profile", $"[{PrimaryEnricher.RequestedOnBehalfOfId}](https://discordapp.com/users/{PrimaryEnricher.RequestedOnBehalfOfId})", true);
    }
}
