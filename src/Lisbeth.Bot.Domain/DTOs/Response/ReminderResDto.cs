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

namespace Lisbeth.Bot.Domain.DTOs.Response;

public class ReminderResDto
{
    public ReminderResDto()
    {
    }

    public ReminderResDto(long id, string? name, DateTime nextOccurrence, List<string>? mentions, string text,
        bool isRecurring = false)
    {
        Id = id;
        Name = name;
        NextOccurrence = nextOccurrence;
        Mentions = mentions;
        Text = text;
        IsRecurring = isRecurring;
    }

    public long Id { get; set; }
    public string? Name { get; set; }
    public DateTime NextOccurrence { get; set; }
    public List<string>? Mentions { get; set; }
    public string? Text { get; set; }
    public bool IsRecurring { get; set; }
}