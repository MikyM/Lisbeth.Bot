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
using MikyM.Discord.EmbedBuilders.Wrappers;
using MikyM.Discord.Enums;

namespace Lisbeth.Bot.Application.Discord.EmbedEnrichers.Response;

public class MemberModDisableReqResponseEnricher : EmbedEnricher<IDisableModReq>
{
    public DiscordMember Target { get; }

    public MemberModDisableReqResponseEnricher(IDisableModReq request, DiscordMember target,
        IModEntity? previous = null) : base(request) =>
        this.Target = target;

    public override void Enrich(IDiscordEmbedBuilderWrapper embedBuilder)
    {
        var (name, pastTense) = base.GetUnderlyingNameAndPastTense();

        embedBuilder.AddField("User mention", this.Target.Mention, true);
        embedBuilder.AddField("Moderator",
            ExtendedFormatter.Mention(this.Entity.RequestedOnBehalfOfId, DiscordEntity.Member), true);
    }
}
