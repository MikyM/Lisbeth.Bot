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
using Lisbeth.Bot.Domain.Entities.Base;
using MikyM.Discord.EmbedBuilders.Wrappers;

namespace Lisbeth.Bot.Application.Discord.EmbedEnrichers.Response.Infractions;

public class MemberModGetReqResponseEnricher : EmbedEnricher<IModEntity>
{
    public MemberModGetReqResponseEnricher(IModEntity request) : base(request)
    {
    }

    public override void Enrich(IDiscordEmbedBuilderWrapper embedBuilder)
    {
        var (name, pastTense) = GetUnderlyingNameAndPastTense();

        embedBuilder.AddField("User mention", ExtendedFormatter.Mention(PrimaryEnricher.UserId, DiscordEntity.User), true);
        embedBuilder.AddInvisibleField();
        embedBuilder.AddField("User ID and profile", $"[{PrimaryEnricher.UserId}](https://discordapp.com/users/{PrimaryEnricher.UserId})", true);
        
        embedBuilder.AddField("Moderator mention", ExtendedFormatter.Mention(PrimaryEnricher.AppliedById, DiscordEntity.User),
            true);
        embedBuilder.AddInvisibleField();
        embedBuilder.AddField("Moderator ID and profile", $"[{PrimaryEnricher.AppliedById}](https://discordapp.com/users/{PrimaryEnricher.AppliedById})", true);

        var duration = PrimaryEnricher.AppliedUntil.Subtract(DateTime.UtcNow);
        var lengthString = PrimaryEnricher.AppliedUntil == DateTime.MaxValue
            ? "Permanent"
            : $"{duration.Days} days, {duration.Hours} hrs, {duration.Minutes} mins";

        embedBuilder.AddField($"{pastTense} on", PrimaryEnricher.CreatedAt?.ToString() ?? "Error");
        embedBuilder.AddField("Length", lengthString, true);
        embedBuilder.AddField($"{pastTense} until", PrimaryEnricher.AppliedUntil.ToString(CultureInfo.CurrentCulture),
            true);

        if (PrimaryEnricher.LiftedOn is not null)
        {
            embedBuilder.AddField("Lifted on", PrimaryEnricher.LiftedOn.Value.ToString(CultureInfo.CurrentCulture), true);
            embedBuilder.AddField("Lifted by", ExtendedFormatter.Mention(PrimaryEnricher.LiftedById, DiscordEntity.User), true);
            embedBuilder.AddField("Lifted by ID and profile", $"[{PrimaryEnricher.LiftedById}](https://discordapp.com/users/{PrimaryEnricher.LiftedById})", true);
        }

        if (!string.IsNullOrWhiteSpace(PrimaryEnricher.Reason)) embedBuilder.AddField("Reason", PrimaryEnricher.Reason);
    }
}
