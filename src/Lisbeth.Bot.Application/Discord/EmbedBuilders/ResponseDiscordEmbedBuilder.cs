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
using MikyM.Discord.EmbedBuilders.Builders;
using MikyM.Discord.EmbedBuilders.Enums;

namespace Lisbeth.Bot.Application.Discord.EmbedBuilders;

[UsedImplicitly]
public sealed class ResponseDiscordEmbedBuilder<TType> : EnrichedDiscordEmbedBuilder, IResponseDiscordEmbedBuilder<TType> where TType : Enum
{
    public TType? ResponseType { get; private set; }

    public ResponseDiscordEmbedBuilder() { }

    public ResponseDiscordEmbedBuilder(EnhancedDiscordEmbedBuilder enhanced) : base(enhanced) { }

    public ResponseDiscordEmbedBuilder(TType type)
        => this.ResponseType = type;

    public IResponseDiscordEmbedBuilder<TType> WithType(TType action)
    {
        this.ResponseType = action;
        return this;
    }

    public static implicit operator DiscordEmbed(ResponseDiscordEmbedBuilder<TType> builder)
        => builder.Build();

    protected override void Evaluate()
    {
        if (this.ResponseType is not null or 0) // if not default
            base.WithAction(this.ResponseType);
        base.WithActionType(DiscordBotAction.Response);

        base.Evaluate();
    }

    public override IResponseDiscordEmbedBuilder<TType> EnrichFrom<TEnricher>(TEnricher enricher)
    {
        enricher.Enrich(this.Current);
        return this;
    }
}
