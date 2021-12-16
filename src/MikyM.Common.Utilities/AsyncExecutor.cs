﻿// This file is part of Lisbeth.Bot project
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


using Autofac;
using Microsoft.Extensions.Logging;
using MikyM.Common.Utilities.Extensions;

#nullable disable
// ReSharper disable UseAwaitUsing

namespace MikyM.Common.Utilities;

public interface IAsyncExecutor
{
    public Task ExecuteAsync<T>(Func<T, Task> func);
}

public class AsyncExecutor : IAsyncExecutor
{
    private readonly ILifetimeScope _lifetimeScope;

    public AsyncExecutor(ILifetimeScope lifetimeScope)
    {
        _lifetimeScope = lifetimeScope;
    }

    public Task ExecuteAsync<T>(Func<T, Task> func)
    {
        return Task.Run(async () =>
            {
                using var scope = _lifetimeScope.BeginLifetimeScope();
                var service = scope.Resolve<T>();
                await func(service);
            })
            .ContinueWith(x => _lifetimeScope.Resolve<ILogger<T>>().LogError(x.Exception?.GetFullMessage()),
                TaskContinuationOptions.OnlyOnFaulted);
    }
}

public static class AsyncExecutorExtensions
{
    public static void AddAsyncExecutor(this ContainerBuilder services)
        => services.RegisterType<AsyncExecutor>().As<IAsyncExecutor>().SingleInstance();
}