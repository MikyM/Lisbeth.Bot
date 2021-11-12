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

using MikyM.Common.Application.Results;

namespace Lisbeth.Bot.Application.Results;

/// <summary>
///     Represents a failure to find something that was searched for from Discord services.
/// </summary>
public record ArgumentError : ResultError
{
    /// <summary>
    ///     Represents a failure to find something that was searched for from Discord services.
    /// </summary>
    /// <param name="name">Argument name.</param>
    /// <param name="message">Custom message.</param>
    public ArgumentError(string? name = null, string? message = null) : base(message ??
                                                                             $"Given {(name is null ? "argument" : $"{name}")} is not valid.")
    {
    }
}