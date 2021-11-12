// This file is part of Lisbeth.Bot project
//
// Copyright (C) 2017 Jarl Gullberg
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

namespace MikyM.Common.Application.Results;

/// <summary>
/// Represents the public API of an interface.
/// </summary>
[PublicAPI]
public interface IResult
{
    /// <summary>
    /// Gets a value indicating whether the result was successful.
    /// </summary>
    bool IsSuccess { get; }

    /// <summary>
    /// Gets the error, if any.
    /// </summary>
    IResultError? Error { get; }

    /// <summary>
    /// Gets the inner result, if any.
    /// </summary>
    IResult? Inner { get; }

    /// <summary>
    /// Tries to get the underlying inner result.
    /// </summary>
    /// <returns>true if the result contains an entity; otherwise, false.</returns>
    bool TryGetInner(out IResult? inner);

    /// <summary>
    /// Tries to get the underlying error result.
    /// </summary>
    /// <returns>true if the result contains an entity; otherwise, false.</returns>
    bool TryGetError(out IResultError? error);
}

/// <summary>
/// Represents the public API of an interface.
/// </summary>
[PublicAPI]
public interface IResult<TEntity> : IResult
{
    /// <summary>
    /// Tries to get the underlying entity returned by the result.
    /// </summary>
    /// <returns>true if the result contains an entity; otherwise, false.</returns>
    bool TryGetEntity(out TEntity? entity);

    /// <summary>
    /// Gets the entity returned by the result.
    /// </summary>
    TEntity? Entity { get; }
}