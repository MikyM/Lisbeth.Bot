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

using Lisbeth.Bot.Application.Enums;

namespace Lisbeth.Bot.Application.Results;

/// <summary>
///     Represents a failure to find something that was searched for from Discord services.
/// </summary>
/// <param name="message">The custom message to provide.</param>
public record DiscordNotFoundError : ResultError
{
    /// <summary>
    ///     Represents a failure to find something that was searched for from Discord services.
    /// </summary>
    /// <param name="message">The custom message to provide.</param>
    public DiscordNotFoundError(string message = "The searched-for Discord entity was not found.") : base(message)
    {
    }

    /// <summary>
    ///     Represents a failure to find something that was searched for from Discord services.
    /// </summary>
    /// <param name="type">The type of Discord entity that was not found.</param>
    public DiscordNotFoundError(DiscordEntityType type) : base($"The searched-for Discord {type} was not found.")
    {
    }
}