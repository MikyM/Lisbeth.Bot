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

using System.Globalization;
using DSharpPlus.Entities;
using Lisbeth.Bot.Application.Discord.Helpers;
using Lisbeth.Bot.Domain.DTOs.Request.Base;
using Lisbeth.Bot.Domain.Entities.Base;
using MikyM.Discord.EmbedBuilders.Wrappers;
using MikyM.Discord.Enums;

namespace Lisbeth.Bot.Application.Discord.EmbedEnrichers.Response.Infractions;

public class MemberModAddReqResponseEnricher : EmbedEnricher<IApplyInfractionReq>
{
    public DiscordUser Target { get; }
    public IModEntity? Previous { get; }

    public bool IsOverlapping =>
        Previous is not null && Previous.AppliedUntil > PrimaryEnricher.AppliedUntil && !Previous.IsDisabled;

    public MemberModAddReqResponseEnricher(IApplyInfractionReq request, DiscordUser target, IModEntity? previous = null) :
        base(request)
    {
        Target = target;
        Previous = previous;
    }

    public override void Enrich(IDiscordEmbedBuilderWrapper embedBuilder)
    {
        var (name, pastTense) = GetUnderlyingNameAndPastTense();

        if (Previous is not null)
        {
            if (IsOverlapping)
            {
                embedBuilder.WithDescription(
                    $"This user has already been {pastTense.ToLower()} until {Previous.AppliedUntil} by {ExtendedFormatter.Mention(PrimaryEnricher.RequestedOnBehalfOfId, DiscordEntity.User)}");
                return;
            }

            embedBuilder.AddField($"Previous {name.ToLower()} until", Previous.AppliedUntil.ToString(CultureInfo.CurrentCulture), true);
            embedBuilder.AddField("Previous moderator",
                $"{ExtendedFormatter.Mention(Previous.AppliedById, DiscordEntity.User)}", true);
            if (!string.IsNullOrWhiteSpace(Previous.Reason))
                embedBuilder.AddField("Previous reason", Previous.Reason, true);
            else
                embedBuilder.AddInvisibleField();
        }

        embedBuilder.WithDescription($"Successfully {pastTense.ToLower()}");
        embedBuilder.AddField("Target user mention", Target.Mention, true);
        embedBuilder.AddInvisibleField();
        embedBuilder.AddField("Target user ID and profile", $"[{Target.Id}](https://discordapp.com/users/{Target.Id})", true);
        
        embedBuilder.AddField("Moderator mention",
            ExtendedFormatter.Mention(PrimaryEnricher.RequestedOnBehalfOfId, DiscordEntity.Member), true);
        embedBuilder.AddInvisibleField();
        embedBuilder.AddField("Moderator user ID and profile", $"[{PrimaryEnricher.RequestedOnBehalfOfId}](https://discordapp.com/users/{PrimaryEnricher.RequestedOnBehalfOfId})", true);

        TimeSpan duration = PrimaryEnricher.AppliedUntil.Subtract(DateTime.UtcNow);
        string lengthString = PrimaryEnricher.AppliedUntil == DateTime.MaxValue
            ? "Permanent"
            : $"{duration.Days} days, {duration.Hours} hrs, {duration.Minutes} mins";

        embedBuilder.AddField("Length", lengthString, true);
        embedBuilder.AddField($"{pastTense} until", PrimaryEnricher.AppliedUntil.ToString(CultureInfo.CurrentCulture),
            true);
        if (!string.IsNullOrWhiteSpace(PrimaryEnricher.Reason)) embedBuilder.AddField("Reason", PrimaryEnricher.Reason);
    }
}
