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
using System.Collections.Generic;

namespace Lisbeth.Bot.Domain.DTOs;

public class EmbedConfigDto
{
    public string? Author { get; set; }
    public string? AuthorUrl { get; set; }
    public string? Footer { get; set; }
    public string? ImageUrl { get; set; }
    public string? FooterImageUrl { get; set; }
    public string? AuthorImageUrl { get; set; }
    public string? Description { get; set; }
    public string? Thumbnail { get; set; }
    public int? ThumbnailHeight { get; set; }
    public int? ThumbnailWidth { get; set; }
    public string? Title { get; set; }
    public ulong CreatorId { get; set; }
    public ulong LastEditById { get; set; }
    public DateTime? Timestamp { get; set; }
    public string HexColor { get; set; } = "#7d7d7d";
    public List<DiscordFieldDto>? Fields { get; set; }
}