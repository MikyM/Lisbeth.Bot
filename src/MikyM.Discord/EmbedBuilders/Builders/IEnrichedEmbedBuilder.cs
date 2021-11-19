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

using System;
using DSharpPlus.Entities;
using MikyM.Discord.EmbedBuilders.Enrichers;

namespace MikyM.Discord.EmbedBuilders.Builders;

/// <summary>
/// Constructs enriched embeds.
/// </summary>
public interface IEnrichedEmbedBuilder : IBaseEmbedBuilder
{
    /// <summary>
    /// Enriches this embed with an embed enricher.
    /// </summary>
    /// <param name="enricher">Enricher to use.</param>
    IEnrichedEmbedBuilder EnrichFrom<TEnricher>(TEnricher enricher)
        where TEnricher : IEmbedEnricher;

    /// <summary>
    /// Sets the action type.
    /// </summary>
    IEnrichedEmbedBuilder WithActionType<TEnum>(TEnum action) where TEnum : Enum;
    /// <summary>
    /// Sets the action.
    /// </summary>
    IEnrichedEmbedBuilder WithAction<TEnum>(TEnum action) where TEnum : Enum;

    IEnrichedEmbedBuilder WithCase(long caseId);
    IEnrichedEmbedBuilder WithFooterSnowflakeInfo(SnowflakeObject snowflake);
    IEnrichedEmbedBuilder WithAuthorSnowflakeInfo(DiscordMember member);
    IEnrichedEmbedBuilder SetAuthorTemplate(string template);
    IEnrichedEmbedBuilder SetFooterTemplate(string template);
    IEnrichedEmbedBuilder SetTitleTemplate(string template);
}