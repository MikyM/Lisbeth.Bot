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

using Autofac;
using Autofac.Builder;
using Autofac.Core;
using Autofac.Features.Scanning;

namespace MikyM.Autofac.Extensions.Extensions;

// ReSharper disable once InconsistentNaming
public static class IRegistrationBuilderExtensions
{
    /// <summary>
    /// Specifies that a type from a scanned assembly is registered if it implements an interface
    /// that closes the provided open generic interface type.
    /// </summary>
    /// <typeparam name="TLimit">Registration limit type.</typeparam>
    /// <typeparam name="TScanningActivatorData">Activator data type.</typeparam>
    /// <typeparam name="TRegistrationStyle">Registration style.</typeparam>
    /// <param name="registration">Registration to set service mapping on.</param>
    /// <param name="openGenericServiceType">The open generic interface or base class type for which implementations will be found.</param>
    /// <returns>Registration builder allowing the registration to be configured.</returns>
    public static IRegistrationBuilder<TLimit, TScanningActivatorData, TRegistrationStyle>
        AsClosedInterfacesOf<TLimit, TScanningActivatorData, TRegistrationStyle>(
            this IRegistrationBuilder<TLimit, TScanningActivatorData, TRegistrationStyle> registration,
            Type openGenericServiceType) where TScanningActivatorData : ScanningActivatorData
    {
        if ((object)openGenericServiceType == null) throw new ArgumentNullException(nameof(openGenericServiceType));
        if (!openGenericServiceType.IsInterface)
            throw new ArgumentException("Generic type must be an interface", nameof(openGenericServiceType));

        return registration.Where(candidateType => candidateType.IsClosedTypeOf(openGenericServiceType))
            .As(candidateType => candidateType.GetInterfaces()
                .Where(i => i.IsClosedTypeOf(openGenericServiceType))
                .Select(t => (Service)new TypedService(t)));
    }

    /// <summary>
    /// Specifies that a type from a scanned assembly is registered if it implements an interface
    /// that closes the provided open generic interface type.
    /// </summary>
    /// <typeparam name="TLimit">Registration limit type.</typeparam>
    /// <typeparam name="TScanningActivatorData">Activator data type.</typeparam>
    /// <typeparam name="TRegistrationStyle">Registration style.</typeparam>
    /// <param name="registration">Registration to set service mapping on.</param>
    /// <param name="openGenericServiceType">The open generic interface or base class type for which implementations will be found.</param>
    /// <returns>Registration builder allowing the registration to be configured.</returns>
    public static IRegistrationBuilder<TLimit, TScanningActivatorData, TRegistrationStyle>
        AsClosedClassesOf<TLimit, TScanningActivatorData, TRegistrationStyle>(
            this IRegistrationBuilder<TLimit, TScanningActivatorData, TRegistrationStyle> registration,
            Type openGenericServiceType) where TScanningActivatorData : ScanningActivatorData
    {
        if ((object)openGenericServiceType == null) throw new ArgumentNullException(nameof(openGenericServiceType));
        if (!openGenericServiceType.IsInterface)
            throw new ArgumentException("Generic type must be an interface", nameof(openGenericServiceType));

        return registration.Where(candidateType => candidateType.IsClosedTypeOf(openGenericServiceType))
            .As(x => x);
    }
}