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

using DSharpPlus.Entities;
using Emzi0767.Utilities;
using Lisbeth.Bot.Application.Discord.Helpers;
using Lisbeth.Bot.Domain.DTOs.Request.Base;
using MikyM.Discord.EmbedBuilders.Wrappers;
using MikyM.Discord.Enums;

namespace Lisbeth.Bot.Application.Discord.EmbedEnrichers.Log;

public class MemberModGetReqLogEnricher : EmbedEnricher<IGetModReq>
{
    public DiscordMember? Moderator { get; }

    public MemberModGetReqLogEnricher(IGetModReq request, DiscordMember? moderator) : base(request)
        => this.Moderator = moderator;

    public override void Enrich(IDiscordEmbedBuilderWrapper embedBuilder)
    {
        embedBuilder.AddField("Moderator", ExtendedFormatter.Mention(this.Entity.RequestedOnBehalfOfId, DiscordEntity.Member),
            true);

        var args = this.Entity.ToDictionary();

        foreach (var (argumentName, argumentValue) in args.Where(x => x.Value is not null))
        {
            var value = argumentValue?.ToString();

            if (value is not null) embedBuilder.AddField(argumentName, value);
        }
    }
}
