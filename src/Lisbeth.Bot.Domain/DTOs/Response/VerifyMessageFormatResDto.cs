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

using System.Text.Json.Serialization;
using DSharpPlus.Entities;

namespace Lisbeth.Bot.Domain.DTOs.Response;

public class VerifyMessageFormatResDto
{
    [JsonIgnore]
    public DiscordEmbed? Embed { get; set; }

    public bool IsCompliant { get; set; }
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public bool? WasAuthorInformed { get; set; }
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public bool? IsDeleted { get; set; }

    public VerifyMessageFormatResDto(bool isCompliant, bool isDeleted = false, bool wasAuthorInformed = false, DiscordEmbed? embed = null)
    {
        IsCompliant = isCompliant;
        IsDeleted = isDeleted;
        Embed = embed;
        WasAuthorInformed = wasAuthorInformed;
    }

    public VerifyMessageFormatResDto(bool isCompliant, bool isDeleted, bool wasAuthorInformed)
    {
        IsCompliant = isCompliant;
        WasAuthorInformed = wasAuthorInformed;
        IsDeleted = isDeleted;
    }

    public VerifyMessageFormatResDto(bool isCompliant)
    {
        IsCompliant = isCompliant;
    }

    public VerifyMessageFormatResDto()
    {
    }
}