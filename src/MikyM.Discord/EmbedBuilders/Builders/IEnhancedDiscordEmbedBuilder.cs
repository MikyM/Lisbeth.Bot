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
using MikyM.Discord.EmbedBuilders.Enums;

namespace MikyM.Discord.EmbedBuilders.Builders;

public interface IEnhancedDiscordEmbedBuilder : IBaseEmbedBuilder
{
    DiscordEmbedEnhancement EnhancementType { get; }
    string EnhancementAction { get; }
    string AuthorTemplate { get; }
    string TitleTemplate { get; }
    string FooterTemplate { get; }
    long? CaseId { get; }
    DiscordMember? AuthorMember { get; }
    SnowflakeObject? FooterSnowflake { get; }

    IResponseEmbedBuilder AsResponse();
    IEnhancedDiscordEmbedBuilder WithCase(long caseId);
    IEnhancedDiscordEmbedBuilder WithFooterSnowflakeInfo(SnowflakeObject snowflake);
    IEnhancedDiscordEmbedBuilder WithAuthorSnowflakeInfo(DiscordMember member);
    IEnhancedDiscordEmbedBuilder SetAuthorTemplate(string template);
    IEnhancedDiscordEmbedBuilder SetFooterTemplate(string template);
    IEnhancedDiscordEmbedBuilder SetTitleTemplate(string template);
    IEnhancedDiscordEmbedBuilder WithEnhancementAction(string action);
}