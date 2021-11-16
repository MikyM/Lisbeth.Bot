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
using System.Text.Json;
using System.Text.Json.Serialization;
using Lisbeth.Bot.Domain.DTOs.Request.Base;

namespace Lisbeth.Bot.Domain.DTOs.Request.Mute;

public class MuteGetReqDto : BaseAuthWithGuildReqDto, IGetModReq
{
    public MuteGetReqDto(ulong requestedOnBehalfId, ulong guildId, long? id = null, ulong? targetUserId = null,
        ulong? appliedById = null, DateTime? liftedOn = null, DateTime? appliedOn = null, ulong? liftedById = null)
    {
        Id = id;
        TargetUserId = targetUserId;
        GuildId = guildId;
        AppliedById = appliedById;
        LiftedOn = liftedOn;
        AppliedOn = appliedOn;
        LiftedById = liftedById;
        RequestedOnBehalfOfId = requestedOnBehalfId;
    }

    public long? Id { get; set; }
    public ulong? TargetUserId { get; set; }
    public ulong? AppliedById { get; set; }
    public DateTime? LiftedOn { get; set; }
    public DateTime? AppliedOn { get; set; }
    public ulong? LiftedById { get; set; }

    public override string ToString()
    {
        return JsonSerializer.Serialize(this, new JsonSerializerOptions { DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault, WriteIndented = true});
    }
}