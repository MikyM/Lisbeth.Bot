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

using System.Diagnostics.CodeAnalysis;
using System.Net.Http;
using Autofac;
using MikyM.Discord.Interfaces;

namespace Lisbeth.Bot.Application.Extensions;

// ReSharper disable once InconsistentNaming
public static class ILifetimeScopeExtensions
{
    public static bool TryGetDiscordService(this ILifetimeScope scope, [NotNullWhen(true)] out IDiscordService? discordService)
        => scope.TryResolve(out discordService);

    public static bool TryGetHttpClientFactory(this ILifetimeScope scope, [NotNullWhen(true)] out IHttpClientFactory? httpClientFactory)
        => scope.TryResolve(out httpClientFactory);
}