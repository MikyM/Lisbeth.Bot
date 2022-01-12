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
using MikyM.Common.Utilities.Autofac;

namespace MikyM.Common.Application;

public sealed class RegistrationConfiguration
{
    public Lifetime BaseGenericDataServiceLifetime { get; set; } = Lifetime.InstancePerLifetimeScope;
    public Lifetime DataServiceLifetime { get; set; } = Lifetime.InstancePerLifetimeScope;
    internal Dictionary<Type, object> InterceptorDelegates { get; private set; } = new();
    internal Dictionary<Type, DataInterceptorConfiguration> DataInterceptors { get; private set; } = new();

    public RegistrationConfiguration AddInterceptor<T>(Func<IComponentContext, T> factoryMethod) where T : notnull
    {
        InterceptorDelegates.TryAdd(typeof(T), factoryMethod);
        return this;
    }

    public RegistrationConfiguration AddDataServiceInterceptor(Type interceptor, DataInterceptorConfiguration configuration = DataInterceptorConfiguration.CrudAndReadOnly)
    {
        DataInterceptors.TryAdd(interceptor, configuration);
        return this;
    }

    public RegistrationConfiguration AddDataServiceInterceptor<T>(DataInterceptorConfiguration configuration = DataInterceptorConfiguration.CrudAndReadOnly) where T : notnull
    {
        DataInterceptors.TryAdd(typeof(T), configuration);
        return this;
    }
}

public enum DataInterceptorConfiguration
{
    CrudAndReadOnly,
    Crud,
    ReadOnly
}