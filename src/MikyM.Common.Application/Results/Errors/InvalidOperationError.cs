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

using System;

namespace MikyM.Common.Application.Results.Errors
{
    /// <summary>
    /// Represents an error raised when an attempt to perform an invalid operation is made.
    /// </summary>
    /// <param name="message">The error message.</param>
    /// <remarks>Used in place of <see cref="InvalidOperationException"/>.</remarks>
    public record InvalidOperationError(string message = "The requested operation is invalid.")
        : ResultError(message);
}