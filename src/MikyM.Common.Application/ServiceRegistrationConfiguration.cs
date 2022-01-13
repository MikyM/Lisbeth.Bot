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


namespace MikyM.Common.Application;

/// <summary>
/// Registration extension configuration
/// </summary>
public sealed class ServiceRegistrationConfiguration
{
    internal ServiceRegistrationConfiguration(RegistrationConfiguration config)
    {
        Config = config;
    }

    internal RegistrationConfiguration Config { get; set; }
    /// <summary>
    /// Gets or sets the default lifetime for base generic data services
    /// </summary>
    public Lifetime BaseGenericDataServiceLifetime { get; set; } = Lifetime.InstancePerLifetimeScope;
    /// <summary>
    /// Gets or sets the default lifetime for custom data services that implement or derive from base data services
    /// </summary>
    public Lifetime DataServiceLifetime { get; set; } = Lifetime.InstancePerLifetimeScope;

    /// <summary>
    /// Gets data interceptor registration delegates
    /// </summary>
    internal Dictionary<Type, DataInterceptorConfiguration> DataInterceptors { get; private set; } = new();

    internal Action<AttributeRegistrationConfiguration>? AttributeOptions { get; private set; }

    /// <summary>
    /// Marks an interceptor of a given type to be used for intercepting base data services.
    /// Please note you must also add this interceptor using <see cref="RegistrationConfiguration.AddInterceptor{T}"/>
    /// </summary>
    /// <param name="interceptor">Type of the interceptor</param>
    /// <param name="configuration">Interceptor configuration</param>
    /// <returns>Current instance of the <see cref="ServiceRegistrationConfiguration"/></returns>
    public ServiceRegistrationConfiguration AddDataServiceInterceptor(Type interceptor, DataInterceptorConfiguration configuration = DataInterceptorConfiguration.CrudAndReadOnly)
    {
        DataInterceptors.TryAdd(interceptor ?? throw new ArgumentNullException(nameof(interceptor)), configuration);
        return this;
    }
    /// <summary>
    /// Marks an interceptor of a given type to be used for intercepting base data services.
    /// Please note you must also add this interceptor using <see cref="RegistrationConfiguration.AddInterceptor{T}"/>
    /// </summary>
    /// <param name="configuration">Interceptor configuration</param>
    /// <returns>Current instance of the <see cref="ServiceRegistrationConfiguration"/></returns>
    public ServiceRegistrationConfiguration AddDataServiceInterceptor<T>(DataInterceptorConfiguration configuration = DataInterceptorConfiguration.CrudAndReadOnly) where T : notnull
    {
        DataInterceptors.TryAdd(typeof(T), configuration);
        return this;
    }

    /// <summary>
    /// Configures attribute services registration options
    /// </summary>
    /// <returns>Current instance of the <see cref="ServiceRegistrationConfiguration"/></returns>
    public ServiceRegistrationConfiguration ConfigureAttributeServices(Action<AttributeRegistrationConfiguration> action)
    {
        AttributeOptions = action;
        return this;
    }
}

/// <summary>
/// Configuration for base data service interceptors
/// </summary>
public enum DataInterceptorConfiguration
{
    CrudAndReadOnly,
    Crud,
    ReadOnly
}