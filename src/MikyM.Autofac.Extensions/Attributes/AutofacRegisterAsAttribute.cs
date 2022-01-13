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

namespace MikyM.Autofac.Extensions.Attributes;

/// <summary>
/// Defines as what should the service be registered
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public sealed class RegisterAsAttribute : Attribute
{
    public Type? RegisterAsType { get; private set; }

    public RegisterAs? RegisterAsOption { get; private set; }

    public RegisterAsAttribute(Type registerAs)
    {
        RegisterAsType = registerAs ?? throw new ArgumentNullException(nameof(registerAs));
    }

    public RegisterAsAttribute(RegisterAs registerAs)
    {
        RegisterAsOption = registerAs;
    }
}