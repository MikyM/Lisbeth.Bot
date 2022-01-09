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
using System.Linq;
using Lisbeth.Bot.Domain.Entities.Base;

namespace Lisbeth.Bot.Domain.Entities;

public class ChannelMessageFormat : SnowflakeDiscordEntity
{
    public ulong ChannelId { get; set; }
    public ulong CreatorId { get; set; }
    public ulong LastEditById { get; set; }
    public string? MessageFormat { get; set; }

    private string[] FormatParts =>
        MessageFormat is null
            ? Array.Empty<string>()
            : MessageFormat.Split('\n', StringSplitOptions.RemoveEmptyEntries);

    public Guild? Guild { get; set; }

    public bool IsTextCompliant(string messageContent)
    {
        if (FormatParts.Any(formatPart =>
                !messageContent.Contains(formatPart, StringComparison.InvariantCultureIgnoreCase)))
            return false;

        var parts = messageContent.Split('\n', StringSplitOptions.RemoveEmptyEntries);

        return parts.All(part => !FormatParts.Any(formatPart =>
            part.Contains(formatPart) && string.IsNullOrWhiteSpace(part.Replace(formatPart, ""))));
    }
}