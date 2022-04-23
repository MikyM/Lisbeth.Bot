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
using Lisbeth.Bot.Application.Discord.SlashCommands;
using MikyM.Discord.EmbedBuilders.Enrichers;
using MikyM.Discord.EmbedBuilders.Wrappers;
using MikyM.Discord.Enums;

namespace Lisbeth.Bot.Application.Discord.EmbedEnrichers.Response.ChannelMessageFormat;

public class ChannelMessageFormatEmbedEnricher : EmbedEnricherBase<Domain.Entities.ChannelMessageFormat, ChannelMessageFormatActionType>
{
    private readonly VerifyMessageFormatResDto? _resDto;
    private readonly string? _content;

    public ChannelMessageFormatEmbedEnricher(Domain.Entities.ChannelMessageFormat entity,
        ChannelMessageFormatActionType actionType, VerifyMessageFormatResDto? verifyRes = null, string? messageContent = null) : base(entity, actionType)
    {
        _resDto = verifyRes;
        _content = messageContent;
    }

    public override void Enrich(IDiscordEmbedBuilderWrapper embedBuilder)
    {
        string action = SecondaryEnricher switch
        {
            ChannelMessageFormatActionType.Create => "created",
            ChannelMessageFormatActionType.Get => "retrieved",
            ChannelMessageFormatActionType.Edit => "edited",
            ChannelMessageFormatActionType.Disable => PrimaryEnricher.IsDisabled ? "disabled" : "enabled",
            ChannelMessageFormatActionType.Verify => "verified",
            _ => throw new ArgumentOutOfRangeException()
        };

        embedBuilder.WithAuthor("Lisbeth channel message format service")
            .WithDescription($"Channel message format {action} successfully");

        embedBuilder.AddField("Channel",
            ExtendedFormatter.Mention(PrimaryEnricher.ChannelId, DiscordEntity.Channel), true);

        embedBuilder.AddField("Created by",
            ExtendedFormatter.Mention(PrimaryEnricher.CreatorId, DiscordEntity.User), true);

        embedBuilder.AddField("Created on",
            PrimaryEnricher.CreatedAt.HasValue
                ? PrimaryEnricher.CreatedAt.Value.ToString(CultureInfo.CurrentCulture)
                : "unknown", true);

        embedBuilder.AddField("Format",
            PrimaryEnricher.MessageFormat ?? "Unknown");

        if (SecondaryEnricher is not ChannelMessageFormatActionType.Verify || _resDto is null) return;

        embedBuilder.AddField("Verification result", _resDto.IsCompliant ? "Compliant" : "Not compliant", true);

        if (_resDto.IsDeleted.HasValue)
            embedBuilder.AddField("Message status", _resDto.IsDeleted.Value ? "Deleted" : "Not deleted", true);

        if (_resDto.WasAuthorInformed.HasValue)
            embedBuilder.AddField("Info status", _resDto.WasAuthorInformed.Value ? "User informed" : "User not informed", true);

        if (_content is not null)
            embedBuilder.AddField("Message content", _content);
    }
}
