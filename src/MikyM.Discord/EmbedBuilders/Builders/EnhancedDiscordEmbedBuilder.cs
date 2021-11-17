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
using MikyM.Discord.Extensions.BaseExtensions;
using System;

namespace MikyM.Discord.EmbedBuilders.Builders;

// ReSharper disable once ClassWithVirtualMembersNeverInherited.Global
public class EnhancedDiscordEmbedBuilder : IEnhancedDiscordEmbedBuilder
{
    public DiscordEmbedBuilder Base { get; }
    public DiscordEmbedEnhancement EnhancementType { get; private set; }
    public string? EnhancementAction { get; private set; }
    public long? CaseId { get; private set; }
    public DiscordMember? AuthorMember { get; private set; }
    public SnowflakeObject? FooterSnowflake { get; private set; }
    public string AuthorTemplate { get; private set; } = @"{0} {1}{2}"; // 0 - action , 1 - type, 2 - target/caller
    public string TitleTemplate { get; private set; } = @"{0} {1}{2}"; // 0 - action , 1 - type, 2 - target/caller
    public string FooterTemplate { get; private set; } = @"{0}{1}"; // 0 - caseId , 1 - snowflake info

    internal EnhancedDiscordEmbedBuilder(DiscordEmbedBuilder builder, DiscordEmbedEnhancement enhancementType)
    {
        this.Base = builder ?? throw new ArgumentNullException(nameof(builder));
        this.EnhancementType = enhancementType;
    }

    public virtual IResponseEmbedBuilder AsResponse(DiscordResponse response)
    {
        this.EnhancementType = DiscordEmbedEnhancement.Response;
        this.EnhancementAction = response.ToString();
        return new ResponseEmbedBuilder(this, response);
    }

    /*protected virtual IEnhancedDiscordEmbedBuilder<TEnhancement> AsLog(DiscordLog response)
    {
        this.EnhancementType = DiscordEmbedEnhancement.Log;
        return this;
    }*/

    public virtual IEnhancedDiscordEmbedBuilder WithCase(long caseId)
    {
        this.CaseId = caseId;
        return this;
    }

    public virtual IEnhancedDiscordEmbedBuilder WithAuthorSnowflakeInfo(DiscordMember member)
    {
        this.AuthorMember = member;
        return this;
    }

    public IEnhancedDiscordEmbedBuilder SetAuthorTemplate(string template)
    {
        if (!string.IsNullOrWhiteSpace(template)) throw new ArgumentException("Invalid template", nameof(template));
        this.FooterTemplate = template;
        return this;
    }

    public IEnhancedDiscordEmbedBuilder SetFooterTemplate(string template)
    {
        if (!string.IsNullOrWhiteSpace(template)) throw new ArgumentException("Invalid template", nameof(template));
        this.AuthorTemplate = template;
        return this;
    }

    public IEnhancedDiscordEmbedBuilder SetTitleTemplate(string template)
    {
        if (!string.IsNullOrWhiteSpace(template)) throw new ArgumentException("Invalid template", nameof(template));
        this.TitleTemplate = template;
        return this;
    }

    public virtual IEnhancedDiscordEmbedBuilder WithFooterSnowflakeInfo(SnowflakeObject snowflake)
    {
        this.FooterSnowflake = snowflake;
        return this;
    }

    public virtual DiscordEmbedBuilder PartialBuild()
    {
        this.Base.WithAuthor(
            string.Format(this.AuthorTemplate, this.EnhancementAction?.SplitByCapitalAndConcat(),
                this.EnhancementType.ToString().SplitByCapitalAndConcat(),
                this.AuthorMember is null ? null : $" | {this.AuthorMember.GetFullDisplayName()}"),
            null,
            this.AuthorMember?.AvatarUrl);

        if (this.FooterSnowflake is not null || this.CaseId.HasValue)
            this.Base.WithFooter(
                string.Format(this.AuthorTemplate, this.CaseId.HasValue ? $"Case Id: {this.CaseId}" : "",
                    this.FooterSnowflake is null ? "" : $" {this.FooterSnowflake.GetType().Name.SplitByCapitalAndConcat()} Id: {this.FooterSnowflake.Id}"));

        return this.Base;
    }

    public virtual DiscordEmbed Build()
    {
        this.PartialBuild();
        return this.Base.Build();
    }
}
