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

using System;
using Lisbeth.Bot.Domain.DTOs.Request.Base;

namespace Lisbeth.Bot.Domain.DTOs.Request.TicketingConfig;

public class TicketingConfigReqDto : BaseAuthWithGuildReqDto
{
    public TimeSpan? CleanAfter { get; set; }
    public TimeSpan? CloseAfter { get; set; }
    public string ClosedNamePrefix { get; set; } = "closed";
    public string OpenedNamePrefix { get; set; } = "ticket";

    public string BaseWelcomeMessage { get; set; } =
        "@ownerMention@ please be patient, support will be with you shortly!";

    public string BaseCenterMessage { get; set; } =
        "\n\nClick on the button below to create a private ticket between the staff members and you. Explain your issue, and a staff member will be here to help you shortly after. Please note it may take up to 48 hours for an answer.";

    public ulong LogChannelId { get; set; }
    public ulong ClosedCategoryId { get; set; }
    public ulong OpenedCategoryId { get; set; }
}