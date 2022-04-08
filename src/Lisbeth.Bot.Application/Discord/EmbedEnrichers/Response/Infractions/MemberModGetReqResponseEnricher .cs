﻿// This file is part of Lisbeth.Bot project
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
using Lisbeth.Bot.Application.Discord.Helpers;
using Lisbeth.Bot.Domain.Entities.Base;
using MikyM.Discord.EmbedBuilders.Wrappers;
using MikyM.Discord.Enums;

namespace Lisbeth.Bot.Application.Discord.EmbedEnrichers.Response.Infractions;

public class MemberModGetReqResponseEnricher : EmbedEnricher<IModEntity>
{
    public MemberModGetReqResponseEnricher(IModEntity request) : base(request)
    {
    }

    public override void Enrich(IDiscordEmbedBuilderWrapper embedBuilder)
    {
        var (name, pastTense) = GetUnderlyingNameAndPastTense();

        embedBuilder.AddField("User", ExtendedFormatter.Mention(PrimaryEnricher.UserId, DiscordEntity.User), true);
        embedBuilder.AddField("Moderator", ExtendedFormatter.Mention(PrimaryEnricher.AppliedById, DiscordEntity.User),
            true);

        TimeSpan duration = PrimaryEnricher.AppliedUntil.Subtract(DateTime.UtcNow);
        string lengthString = PrimaryEnricher.AppliedUntil == DateTime.MaxValue
            ? "Permanent"
            : $"{duration.Days} days, {duration.Hours} hrs, {duration.Minutes} mins";

        embedBuilder.AddField($"{pastTense} on", PrimaryEnricher.CreatedAt?.ToString() ?? "Error");
        embedBuilder.AddField("Length", lengthString, true);
        embedBuilder.AddField($"{pastTense} until", PrimaryEnricher.AppliedUntil.ToString(CultureInfo.CurrentCulture),
            true);

        if (PrimaryEnricher.LiftedOn is not null)
        {
            embedBuilder.AddField("Lifted on", PrimaryEnricher.LiftedOn.Value.ToString(CultureInfo.CurrentCulture));
            embedBuilder.AddField("Lifted by", ExtendedFormatter.Mention(PrimaryEnricher.LiftedById, DiscordEntity.User));
        }

        if (!string.IsNullOrWhiteSpace(PrimaryEnricher.Reason)) embedBuilder.AddField("Reason", PrimaryEnricher.Reason);
    }
}
