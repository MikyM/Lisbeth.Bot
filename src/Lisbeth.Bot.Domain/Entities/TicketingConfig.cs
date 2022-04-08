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

using System;
using System.ComponentModel.DataAnnotations.Schema;
using Lisbeth.Bot.Domain.Entities.Base;

namespace Lisbeth.Bot.Domain.Entities;

public sealed class TicketingConfig : SnowflakeDiscordEntity
{
    public ulong LogChannelId { get; set; }
    public long LastTicketId { get; set; }
    public ulong ClosedCategoryId { get; set; }
    public ulong OpenedCategoryId { get; set; }
    public TimeSpan? CleanAfter { get; set; }
    public TimeSpan? CloseAfter { get; set; }
    public string OpenedNamePrefix { get; set; } = "ticket";
    public string ClosedNamePrefix { get; set; } = "closed";

    public string BaseWelcomeMessage { get; set; } =
        "@ownerMention@ please be patient, support will be with you shortly!";

    public EmbedConfig? WelcomeEmbedConfig { get; set; }

    public long? WelcomeEmbedConfigId { get; set; }

    public EmbedConfig? CenterEmbedConfig { get; set; }
    public long? CenterEmbedConfigId { get; set; }

    public string BaseCenterMessage { get; set; } =
        "\n\nClick on the button below to create a private ticket between the staff members and you.\n\nExplain your issue, and a staff member will be here to help you shortly after.";

    public Guild? Guild { get; set; }

    [NotMapped] public bool ShouldAutoClean => CleanAfter.HasValue;

    [NotMapped] public bool ShouldAutoClose => CloseAfter.HasValue;
}