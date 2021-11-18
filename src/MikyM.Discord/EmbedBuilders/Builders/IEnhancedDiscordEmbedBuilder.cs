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
using System;

namespace MikyM.Discord.EmbedBuilders.Builders;

public interface IEnhancedDiscordEmbedBuilder<TEnhancement> : IBaseEmbedBuilder where TEnhancement : Enum
{
    TEnhancement? EnhancementType { get; }
    string EnhancementAction { get; }
    string AuthorTemplate { get; }
    string TitleTemplate { get; }
    string FooterTemplate { get; }
    long? CaseId { get; }
    DiscordMember? AuthorMember { get; }
    SnowflakeObject? FooterSnowflake { get; }
    //DiscordEmbedBuilder Current { get; }

    IEnhancedDiscordEmbedBuilder<TEnhancement> WithCase(long caseId);
    IEnhancedDiscordEmbedBuilder<TEnhancement> WithFooterSnowflakeInfo(SnowflakeObject snowflake);
    IEnhancedDiscordEmbedBuilder<TEnhancement> WithAuthorSnowflakeInfo(DiscordMember member);
    IEnhancedDiscordEmbedBuilder<TEnhancement> SetAuthorTemplate(string template);
    IEnhancedDiscordEmbedBuilder<TEnhancement> SetFooterTemplate(string template);
    IEnhancedDiscordEmbedBuilder<TEnhancement> SetTitleTemplate(string template);
    IEnhancedDiscordEmbedBuilder<TEnhancement> AsType(TEnhancement enhancementType);
}