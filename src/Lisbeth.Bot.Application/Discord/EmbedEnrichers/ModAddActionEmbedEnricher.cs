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
using MikyM.Discord.EmbedBuilders;
using MikyM.Discord.EmbedBuilders.Enums;
using MikyM.Discord.Extensions.BaseExtensions;
using System.Globalization;
using MikyM.Discord.EmbedBuilders.Wrappers;

namespace Lisbeth.Bot.Application.Discord.EmbedEnrichers;

public class ModAddActionEmbedEnricher : EmbedEnricher<IAddModReq>
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

    public override void Enrich(IDiscordEmbedBuilderWrapper embedBuilder)
    {
        var (name, pastTense) = base.GetUnderlyingNameAndPastTense(this.Entity);
        embedBuilder.WithColor(new DiscordColor(this.HexColor));
        embedBuilder.WithAuthor(
            $" {(this.Previous is not null && !this.IsOverlapping ? "Extend " : "")}{name} {(this.Previous is not null && this.IsOverlapping ? "failed " : "")}| {this.Target.GetFullDisplayName()}",
            null, this.Target.AvatarUrl);

        if (this.Previous is not null)
        {
            if (this.IsOverlapping)
            {
                embedBuilder.WithDescription(
                    $"This user has already been {pastTense.ToLower()} until {this.Previous.AppliedUntil} by {ExtendedFormatter.Mention(this.Entity.RequestedOnBehalfOfId, DiscordEntity.User)}");
                embedBuilder.WithFooter($"Previous case Id: {this.Previous.Id} | Member Id: {this.Previous.UserId}");
                return;
            }

            embedBuilder.AddField($"Previous {name.ToLower()} until", this.Previous.AppliedUntil.ToString(), true);
            embedBuilder.AddField("Previous moderator",
                $"{ExtendedFormatter.Mention(this.Previous.AppliedById, DiscordEntity.User)}", true);
            if (!string.IsNullOrWhiteSpace(this.Previous.Reason)) embedBuilder.AddField("Previous reason", this.Previous.Reason, true);
        }

        embedBuilder.AddField("User mention", this.Target.Mention, true);
        embedBuilder.AddField("Moderator", ExtendedFormatter.Mention(this.Entity.RequestedOnBehalfOfId, DiscordEntity.Member),
            true);

        TimeSpan duration = this.Entity.AppliedUntil.Subtract(DateTime.UtcNow);
        string lengthString = this.Entity.AppliedUntil == DateTime.MaxValue
            ? "Permanent"
            : $"{duration.Days} days, {duration.Hours} hrs, {duration.Minutes} mins";

        embedBuilder.AddField("Length", lengthString, true);
        embedBuilder.AddField($"{pastTense} until", this.Entity.AppliedUntil.ToString(CultureInfo.InvariantCulture), true);
        if (!string.IsNullOrWhiteSpace(this.Entity.Reason)) embedBuilder.AddField("Reason", this.Entity.Reason);
        embedBuilder.WithFooter($"Case Id: {(!this.CaseId.HasValue ? "Unknown" : this.CaseId)} | Member Id: {this.Entity.TargetUserId}");
    }
}