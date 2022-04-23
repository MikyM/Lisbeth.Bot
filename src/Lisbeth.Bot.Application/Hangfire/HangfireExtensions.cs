// This file is part of Lisbeth.Bot project
//
// Copyright (C) 2021-2022 Krzysztof Kupisz - MikyM
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

using System.Linq.Expressions;
using Hangfire;

namespace Lisbeth.Bot.Application.Hangfire;

[UsedImplicitly]
public static class HangfireExtensions
{
    public static string Schedule([global::Hangfire.Annotations.NotNull] this IBackgroundJobClient client,
        [global::Hangfire.Annotations.NotNull] [global::Hangfire.Annotations.InstantHandle]
        Expression<Action> methodCall, DateTime enqueueAt, string queue)
    {
        if (client is null) throw new ArgumentNullException(nameof(client));

        return client.Create(methodCall, new ScheduledEnqueuedState(enqueueAt, queue));
    }

    public static string Schedule<T>([global::Hangfire.Annotations.NotNull] this IBackgroundJobClient client,
        [global::Hangfire.Annotations.NotNull] [global::Hangfire.Annotations.InstantHandle]
        Expression<Action<T>> methodCall, DateTime enqueueAt, string queue)
    {
        if (client is null) throw new ArgumentNullException(nameof(client));

        return client.Create(methodCall, new ScheduledEnqueuedState(enqueueAt, queue));
    }
}