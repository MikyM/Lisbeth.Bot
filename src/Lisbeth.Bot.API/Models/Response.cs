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

using System.Collections.Generic;

namespace Lisbeth.Bot.API.Models;

public class Response
{
    public Response(IEnumerable<string>? errors) : this(null, errors)
    {
    }

    public Response(string? message = null, IEnumerable<string>? errors = null)
    {
        Message = message ?? string.Empty;
        Errors = errors?.ToArray();
    }

    public bool IsSuccess => Errors is null;
    public string[]? Errors { get; set; }
    public string? Message { get; set; }
}

public class Response<T> : Response
{
    public Response(T? data, IEnumerable<string>? errors) : this(data, null, errors)
    {
    }

    public Response(T? data, string? message = null, IEnumerable<string>? errors = null) : base(message, errors)
    {
        Data = data;
    }

    public T? Data { get; set; }
}