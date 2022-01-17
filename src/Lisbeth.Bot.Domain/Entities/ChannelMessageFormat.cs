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

using DSharpPlus;
using Lisbeth.Bot.Domain.Entities.Base;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Lisbeth.Bot.Domain.Entities;

public class ChannelMessageFormat : SnowflakeDiscordEntity
{
    public ulong ChannelId { get; set; }
    public ulong CreatorId { get; set; }
    public ulong LastEditById { get; set; }
    public string? MessageFormat { get; set; }

    public Guild? Guild { get; set; }

    private IEnumerable<string> FormatParts =>
        MessageFormat is null
            ? Enumerable.Empty<string>()
            : MessageFormat.Split('\n', StringSplitOptions.RemoveEmptyEntries).Select(x => x.Trim());

    public bool IsTextCompliant(string messageContent)
    {
        if (string.IsNullOrWhiteSpace(messageContent))
            return false;

        messageContent = Formatter.Strip(messageContent);
        var parts = messageContent.Split('\n', StringSplitOptions.RemoveEmptyEntries).Select(x => x.Trim()).ToList();

        if (parts.Count == 0)
            return false;

        if (!parts.First().Contains(FormatParts.First()))
            return false;

        foreach (var formatPart in FormatParts)
        {
            if (!messageContent.Contains(formatPart))
                return false;

            foreach (var part in parts.Select((text, index) => new { text, index }))
            {
                if (part.text.Contains(formatPart) && string.IsNullOrWhiteSpace(part.text.Replace(formatPart, "")))
                    if (parts.Count - 1 == part.index)
                        return false;
                    else if (FormatParts.Any(x => parts[part.index + 1].Contains(x))) 
                        return false;

                if (part.text.Contains(formatPart) && !part.text.StartsWith(formatPart)) return false;
            }
        }

        return true;
    }
}