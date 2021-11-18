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
using MikyM.Discord.Extensions.BaseExtensions;
using System;
using System.Runtime.CompilerServices;

namespace MikyM.Discord.EmbedBuilders.Builders;

public sealed class EnhancedDiscordEmbedBuilder<TEnhancement> : IEnhancedDiscordEmbedBuilder<TEnhancement>
    where TEnhancement : Enum
{
    internal DiscordEmbedBuilder Base { get; }

    private readonly DiscordEmbedBuilder _current;

    internal DiscordEmbedBuilder Current
    {
        get
        {
            this.Evaluate();
            return _current;
        }
        init => this._current = value;
    }

    public TEnhancement? EnhancementType { get; private set; }
    public string? EnhancementAction { get; private set; }
    public long? CaseId { get; private set; }
    public DiscordMember? AuthorMember { get; private set; }
    public SnowflakeObject? FooterSnowflake { get; private set; }
    public string AuthorTemplate { get; private set; } = @"@action@ @type@@info@"; // 0 - action , 1 - type, 2 - target/caller
    public string TitleTemplate { get; private set; } = @"@action@ @type@@info@"; // 0 - action , 1 - type, 2 - target/caller
    public string FooterTemplate { get; private set; } = @"@caseId@@info@"; // 0 - caseId , 1 - snowflake info


    internal EnhancedDiscordEmbedBuilder(DiscordEmbedBuilder builder, TEnhancement? enhancementType = default)
    {
        this.Base = new DiscordEmbedBuilder(builder) ?? throw new ArgumentNullException(nameof(builder));
        this.EnhancementType = enhancementType;
        this._current = new DiscordEmbedBuilder(builder);
    }

    internal EnhancedDiscordEmbedBuilder<TEnhancement> WithEnhancementAction<TEnum>(TEnum action) where TEnum : Enum
    {
        this.EnhancementAction = action.ToString();
        return this;
    }

    public IEnhancedDiscordEmbedBuilder<TEnhancement> AsType(TEnhancement enhancementType)
    {
        this.EnhancementType = enhancementType;
        return this;
    }

    public IEnhancedDiscordEmbedBuilder<TEnhancement> WithCase(long caseId)
    {
        this.CaseId = caseId;
        return this;
    }

    public IEnhancedDiscordEmbedBuilder<TEnhancement> WithAuthorSnowflakeInfo(DiscordMember member)
    {
        this.AuthorMember = member;
        return this;
    }

    public IEnhancedDiscordEmbedBuilder<TEnhancement> SetAuthorTemplate(string template)
    {
        if (string.IsNullOrWhiteSpace(template)) throw new ArgumentException("Invalid template", nameof(template));
        this.FooterTemplate = template;
        return this;
    }

    public IEnhancedDiscordEmbedBuilder<TEnhancement> SetFooterTemplate(string template)
    {
        if (string.IsNullOrWhiteSpace(template)) throw new ArgumentException("Invalid template", nameof(template));
        this.AuthorTemplate = template;
        return this;
    }

    public IEnhancedDiscordEmbedBuilder<TEnhancement> SetTitleTemplate(string template)
    {
        if (string.IsNullOrWhiteSpace(template)) throw new ArgumentException("Invalid template", nameof(template));
        this.TitleTemplate = template;
        return this;
    }

    public IEnhancedDiscordEmbedBuilder<TEnhancement> WithFooterSnowflakeInfo(SnowflakeObject snowflake)
    {
        this.FooterSnowflake = snowflake;
        return this;
    }

    public void Evaluate()
    {
        string author = this.AuthorTemplate
            .Replace("@action@", this.EnhancementAction is null ? "" : this.EnhancementAction.SplitByCapitalAndConcat())
            .Replace("@type@",
                this.EnhancementType is null
                    ? ""
                    : this.EnhancementType.ToString().SplitByCapitalAndConcat());

        author = author.Replace("@info",
            this.AuthorMember is null ? "" : $" | {this.AuthorMember.GetFullDisplayName()}");

        this.Current.WithAuthor(author, null, this.AuthorMember?.AvatarUrl);

        if (this.FooterSnowflake is null && !this.CaseId.HasValue) return;
        string footer = this.FooterTemplate.Replace("@caseId@", this.CaseId is null ? "" : this.CaseId.ToString())
            .Replace("@info@",
                this.FooterSnowflake is null
                    ? ""
                    : $"{this.FooterSnowflake.GetType().Name.SplitByCapitalAndConcat()} Id: {this.FooterSnowflake.Id}");

        this.Current.WithFooter(footer);
    }

    public DiscordEmbed Build()
        => this.Current.Build();
}