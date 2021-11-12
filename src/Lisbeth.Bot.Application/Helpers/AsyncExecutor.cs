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
using System.Threading.Tasks;
using Autofac;
using Lisbeth.Bot.Application.Extensions;
using Microsoft.Extensions.Logging;

#nullable disable
// ReSharper disable UseAwaitUsing

namespace Lisbeth.Bot.Application.Helpers;

public interface IAsyncExecutor
{
    public Task ExecuteAsync<T>(Func<T, Task> func);
    public Task ExecuteAsync(Func<Task> func);
}

public class AsyncExecutor : IAsyncExecutor
{
    private readonly ILifetimeScope _lifetimeScope;
    private readonly ILogger<AsyncExecutor> _logger;

    public AsyncExecutor(ILifetimeScope lifetimeScope, ILogger<AsyncExecutor> logger)
    {
        _lifetimeScope = lifetimeScope;
        _logger = logger;
    }

    public Task ExecuteAsync<T>(Func<T, Task> func)
    {
        return Task.Run(async () =>
            {
                using var scope = _lifetimeScope.BeginLifetimeScope();
                var service = scope.Resolve<T>();
                await func(service);
            })
            .ContinueWith(x => _lifetimeScope.Resolve<ILogger<T>>().LogError(x.Exception.GetFullMessage()),
                TaskContinuationOptions.OnlyOnFaulted);
    }

    public Task ExecuteAsync(Func<Task> func)
    {
        Type t = func.GetType();
        return Task.Run(async () =>
            {
                using var scope = _lifetimeScope.BeginLifetimeScope();
                await func.Invoke();
            })
            .ContinueWith(x => _logger.LogError(x.Exception.GetFullMessage()),
                TaskContinuationOptions.OnlyOnFaulted);
    }
}