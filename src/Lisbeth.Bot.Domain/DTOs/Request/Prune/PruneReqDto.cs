﻿// This file is part of Lisbeth.Bot project
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

using System.Collections.Generic;
using Lisbeth.Bot.Domain.DTOs.Request.Base;
using Lisbeth.Bot.Domain.Entities;

namespace Lisbeth.Bot.Domain.DTOs.Request.Prune;

public class PruneReqDto : BaseAuthWithGuildReqDto
{
    public PruneReqDto(ulong guildId, ulong channelId, ulong requestedOnBehalfOfId, int? count = null, ulong? messageId = null,
        ulong? targetAuthorId = null) : base(guildId, requestedOnBehalfOfId)
    {
        Count = count;
        MessageId = messageId;
        TargetAuthorId = targetAuthorId;
        ChannelId = channelId;
    }

    public int? Count { get; set; }
    public ulong? TargetAuthorId { get; set; }
    public ulong? MessageId { get; set; }
    public ulong ChannelId { get; set; }
    public bool? IsTargetedMessageDelete { get; set; }
    public List<MessageLog> Messages { get; set; } = new();
}
