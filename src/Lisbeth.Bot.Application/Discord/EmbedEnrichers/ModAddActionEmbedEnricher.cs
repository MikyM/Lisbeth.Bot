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
using Lisbeth.Bot.Application.Discord.Helpers;
using Lisbeth.Bot.Domain.DTOs.Request.Base;
using Lisbeth.Bot.Domain.Entities.Base;
using MikyM.Discord.EmbedBuilders.Builders;
using MikyM.Discord.EmbedBuilders.Enums;
using MikyM.Discord.Extensions.BaseExtensions;
using System.Globalization;

namespace Lisbeth.Bot.Application.Discord.EmbedEnrichers;

public class ModAddActionEmbedEnricher : EmbedEnricherBase<IAddModReq>
{
    public DiscordMember Target { get; }
    public IModEntity? Previous { get; }

    public bool IsOverlapping =>
        this.Previous is not null && this.Previous.AppliedUntil > this.Entity.AppliedUntil &&
        !this.Previous.IsDisabled;

    public ModAddActionEmbedEnricher(IAddModReq request, DiscordMember target, long? caseId = null,
        string hexColor = "#26296e", IModEntity? previous = null) : base(request, caseId, hexColor)
    {
        this.Target = target;
        this.Previous = previous;
    }

    public override void Enrich<TBuilder>(IEnrichedEmbedBuilder<TBuilder> embedBuilder)
    {
        var (name, pastTense) = base.GetUnderlyingNameAndPastTense(this.Entity);

        embedBuilder.Base.WithColor(new DiscordColor(this.HexColor));
        embedBuilder.Base.WithAuthor(
            $" {(this.Previous is not null && !this.IsOverlapping ? "Extend " : "")}{name} {(this.Previous is not null && this.IsOverlapping ? "failed " : "")}| {this.Target.GetFullDisplayName()}",
            null, this.Target.AvatarUrl);

        if (this.Previous is not null)
        {
            if (this.IsOverlapping)
            {
                embedBuilder.Base.WithDescription(
                    $"This user has already been {pastTense.ToLower()} until {this.Previous.AppliedUntil} by {ExtendedFormatter.Mention(this.Entity.RequestedOnBehalfOfId, DiscordEntityType.User)}");
                embedBuilder.Base.WithFooter($"Previous case Id: {this.Previous.Id} | Member Id: {this.Previous.UserId}");
                return;
            }

            embedBuilder.Base.AddField($"Previous {name.ToLower()} until", this.Previous.AppliedUntil.ToString(), true);
            embedBuilder.Base.AddField("Previous moderator",
                $"{ExtendedFormatter.Mention(this.Previous.AppliedById, DiscordEntityType.User)}", true);
            embedBuilder.Base.AddField("Previous reason", this.Previous.Reason, true);
        }

        embedBuilder.Base.AddField("User mention", this.Target.Mention, true);
        embedBuilder.Base.AddField("Moderator", ExtendedFormatter.Mention(this.Entity.RequestedOnBehalfOfId, DiscordEntityType.Member),
            true);

        TimeSpan duration = this.Entity.AppliedUntil.Subtract(DateTime.UtcNow);
        string lengthString = this.Entity.AppliedUntil == DateTime.MaxValue
            ? "Permanent"
            : $"{duration.Days} days, {duration.Hours} hrs, {duration.Minutes} mins";

        embedBuilder.Base.AddField("Length", lengthString, true);
        embedBuilder.Base.AddField($"{pastTense} until", this.Entity.AppliedUntil.ToString(CultureInfo.InvariantCulture), true);
        embedBuilder.Base.AddField("Reason", this.Entity.Reason);
        embedBuilder.Base.WithFooter($"Case Id: {(!this.CaseId.HasValue  ? "Unknown" : this.CaseId)} | Member Id: {this.Entity.TargetUserId}");
    }
}