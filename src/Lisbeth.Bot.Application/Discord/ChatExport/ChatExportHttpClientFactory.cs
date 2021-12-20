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

using System.Net.Http;

namespace Lisbeth.Bot.Application.Discord.ChatExport;

public static class ChatExportHttpClientFactory
{
    /// <summary>
    /// The factory used to create an instance of <see cref="HttpClient"/> for ChatExport builders.
    /// </summary>
    private static Func<HttpClient>? _factory;

    /// <summary>
    /// Initializes the specified creation factory.
    /// </summary>
    /// <param name="creationFactory">The creation factory.</param>
    public static void SetFactory(Func<HttpClient>? creationFactory)
        => _factory = creationFactory;

    /// <summary>
    /// Creates a <see cref="HttpClient"/> instance.
    /// </summary>
    /// <returns>Returns an instance of an <see cref="HttpClient"/> </returns>
    public static HttpClient Build()
    {
        if (_factory is null) throw new InvalidOperationException("You can not create an instance without first building the factory.");

        return _factory();
    }
}