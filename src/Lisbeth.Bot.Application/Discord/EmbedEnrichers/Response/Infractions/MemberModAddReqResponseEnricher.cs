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
        this.Previous is not null && this.Previous.AppliedUntil > this.PrimaryEnricher.AppliedUntil && !this.Previous.IsDisabled;

    public MemberModAddReqResponseEnricher(IApplyInfractionReq request, DiscordUser target, IModEntity? previous = null) :
        base(request)
    {
        this.Target = target;
        this.Previous = previous;
    }

    public override void Enrich(IDiscordEmbedBuilderWrapper embedBuilder)
    {
        var (name, pastTense) = base.GetUnderlyingNameAndPastTense();
        /*embedBuilder.WithAuthor(
            $" {(this.Previous is not null && !this.IsOverlapping ? "Extend " : "")}{name} {(this.Previous is not null && this.IsOverlapping ? "failed " : "")}| {this.Target.GetFullDisplayName()}",
            null, this.Target.AvatarUrl);*/

        if (this.Previous is not null)
        {
            if (this.IsOverlapping)
            {
                embedBuilder.WithDescription(
                    $"This user has already been {pastTense.ToLower()} until {this.Previous.AppliedUntil} by {ExtendedFormatter.Mention(this.PrimaryEnricher.RequestedOnBehalfOfId, DiscordEntity.User)}");
                return;
            }

            embedBuilder.AddField($"Previous {name.ToLower()} until", this.Previous.AppliedUntil.ToString(CultureInfo.InvariantCulture), true);
            embedBuilder.AddField("Previous moderator",
                $"{ExtendedFormatter.Mention(this.Previous.AppliedById, DiscordEntity.User)}", true);
            if (!string.IsNullOrWhiteSpace(this.Previous.Reason))
                embedBuilder.AddField("Previous reason", this.Previous.Reason, true);
        }

        embedBuilder.WithDescription($"Successfully {pastTense.ToLower()}");
        embedBuilder.AddField("User mention", this.Target.Mention, true);
        embedBuilder.AddField("Moderator",
            ExtendedFormatter.Mention(this.PrimaryEnricher.RequestedOnBehalfOfId, DiscordEntity.Member), true);

        TimeSpan duration = this.PrimaryEnricher.AppliedUntil.Subtract(DateTime.UtcNow);
        string lengthString = this.PrimaryEnricher.AppliedUntil == DateTime.MaxValue
            ? "Permanent"
            : $"{duration.Days} days, {duration.Hours} hrs, {duration.Minutes} mins";

        embedBuilder.AddField("Length", lengthString, true);
        embedBuilder.AddField($"{pastTense} until", this.PrimaryEnricher.AppliedUntil.ToString(CultureInfo.InvariantCulture),
            true);
        if (!string.IsNullOrWhiteSpace(this.PrimaryEnricher.Reason)) embedBuilder.AddField("Reason", this.PrimaryEnricher.Reason);
    }
}
