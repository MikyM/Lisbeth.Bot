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

namespace MikyM.Discord.EmbedBuilders.Builders;

public sealed class EnhancedDiscordEmbedBuilder : IEnhancedDiscordEmbedBuilder
{
    protected DiscordEmbedBuilder Base { get; }

    internal DiscordEmbedBuilder Current { get; }

    public string? Action { get; private set; }
    public string? ActionType { get; private set; }
    public long? CaseId { get; private set; }
    public DiscordMember? AuthorMember { get; private set; }
    public SnowflakeObject? FooterSnowflake { get; private set; }
    public string AuthorTemplate { get; private set; } = @"@action@ @type@@info@"; // 0 - action , 1 - type, 2 - target/caller
    public string TitleTemplate { get; private set; } = @"@action@ @type@@info@"; // 0 - action , 1 - type, 2 - target/caller
    public string FooterTemplate { get; private set; } = @"@caseId@@info@"; // 0 - caseId , 1 - snowflake info


    internal EnhancedDiscordEmbedBuilder(DiscordEmbedBuilder builder)
    {
        this.Base = new DiscordEmbedBuilder(builder) ?? throw new ArgumentNullException(nameof(builder));
        this.Current = builder;
    }


    public IEnhancedDiscordEmbedBuilder WithCase(long caseId)
    {
        this.CaseId = caseId;
        return this;
    }

    public IEnhancedDiscordEmbedBuilder WithAuthorSnowflakeInfo(DiscordMember member)
    {
        this.AuthorMember = member;
        return this;
    }

    public IEnhancedDiscordEmbedBuilder SetAuthorTemplate(string template)
    {
        if (string.IsNullOrWhiteSpace(template)) throw new ArgumentException("Invalid template", nameof(template));
        this.FooterTemplate = template;
        return this;
    }

    public IEnhancedDiscordEmbedBuilder SetFooterTemplate(string template)
    {
        if (string.IsNullOrWhiteSpace(template)) throw new ArgumentException("Invalid template", nameof(template));
        this.AuthorTemplate = template;
        return this;
    }

    public IEnhancedDiscordEmbedBuilder SetTitleTemplate(string template)
    {
        if (string.IsNullOrWhiteSpace(template)) throw new ArgumentException("Invalid template", nameof(template));
        this.TitleTemplate = template;
        return this;
    }

    public IEnhancedDiscordEmbedBuilder WithFooterSnowflakeInfo(SnowflakeObject snowflake)
    {
        this.FooterSnowflake = snowflake;
        return this;
    }

    internal EnhancedDiscordEmbedBuilder WithAction<TEnum>(TEnum action) where TEnum : Enum
    {
        this.Action = action.ToString();
        return this;
    }

    internal EnhancedDiscordEmbedBuilder WithActionType<TEnum>(TEnum actionType) where TEnum : Enum
    {
        this.ActionType = actionType.ToString();
        return this;
    }

    public void Evaluate()
    {
        string author = this.AuthorTemplate
            .Replace("@action@", this.Action is null ? "" : this.Action.SplitByCapitalAndConcat())
            .Replace("@type@",
                this.ActionType is null
                    ? ""
                    : this.ActionType.SplitByCapitalAndConcat());

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
