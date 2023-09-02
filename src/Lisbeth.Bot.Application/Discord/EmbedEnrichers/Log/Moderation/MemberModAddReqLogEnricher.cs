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

using System.Globalization;
using Lisbeth.Bot.Application.Discord.Helpers;
using Lisbeth.Bot.Domain.DTOs.Request.Base;
using MikyM.Discord.EmbedBuilders.Wrappers;

namespace Lisbeth.Bot.Application.Discord.EmbedEnrichers.Log.Moderation;

public class MemberModAddReqLogEnricher : EmbedEnricher<IApplyInfractionReq>
{
    public MemberModAddReqLogEnricher(IApplyInfractionReq request) : base(request)
    {
    }

    public override void Enrich(IDiscordEmbedBuilderWrapper embedBuilder)
    {
        var (name, pastTense) = GetUnderlyingNameAndPastTense();

        embedBuilder.AddField("Moderator mention",
            ExtendedFormatter.Mention(PrimaryEnricher.RequestedOnBehalfOfId, DiscordEntity.Member), true);
        embedBuilder.AddInvisibleField();
        embedBuilder.AddField($"Moderator ID and profile",
            $"[{PrimaryEnricher.RequestedOnBehalfOfId}](https://discordapp.com/users/{PrimaryEnricher.RequestedOnBehalfOfId})", true);

        embedBuilder.AddField("Target user mention",
            ExtendedFormatter.Mention(PrimaryEnricher.TargetUserId, DiscordEntity.Member),
            true);
        embedBuilder.AddInvisibleField();
        embedBuilder.AddField($"Target user ID and profile",
            $"[{PrimaryEnricher.TargetUserId}](https://discordapp.com/users/{PrimaryEnricher.TargetUserId})", true);

        var duration = PrimaryEnricher.AppliedUntil.Subtract(DateTime.UtcNow);
        var lengthString = PrimaryEnricher.AppliedUntil == DateTime.MaxValue
            ? "Permanent"
            : $"{duration.Days} days, {duration.Hours} hrs, {duration.Minutes} mins";

        embedBuilder.AddField("Length", lengthString, true);
        embedBuilder.AddField($"{pastTense} until", PrimaryEnricher.AppliedUntil.ToString(CultureInfo.CurrentCulture),
            true);

        if (!string.IsNullOrWhiteSpace(PrimaryEnricher.Reason)) 
            embedBuilder.AddField("Reason", PrimaryEnricher.Reason);
        else
            embedBuilder.AddInvisibleField();
    }
}
