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
using System.Linq.Expressions;

namespace MikyM.Common.DataAccessLayer.Specifications.Helpers;

public class IncludeExpressionInfo
{
    private IncludeExpressionInfo(LambdaExpression expression,
        Type entityType,
        Type propertyType,
        Type? previousPropertyType,
        IncludeTypeEnum includeType)

    {
        _ = expression ?? throw new ArgumentNullException(nameof(expression));
        _ = entityType ?? throw new ArgumentNullException(nameof(entityType));
        _ = propertyType ?? throw new ArgumentNullException(nameof(propertyType));

        if (includeType == IncludeTypeEnum.ThenInclude)
            _ = previousPropertyType ?? throw new ArgumentNullException(nameof(previousPropertyType));

        LambdaExpression = expression;
        EntityType = entityType;
        PropertyType = propertyType;
        PreviousPropertyType = previousPropertyType;
        Type = includeType;
    }

    public IncludeExpressionInfo(LambdaExpression expression,
        Type entityType,
        Type propertyType)
        : this(expression, entityType, propertyType, null, IncludeTypeEnum.Include)
    {
    }

    public IncludeExpressionInfo(LambdaExpression expression,
        Type entityType,
        Type propertyType,
        Type previousPropertyType)
        : this(expression, entityType, propertyType, previousPropertyType, IncludeTypeEnum.ThenInclude)
    {
    }

    public LambdaExpression LambdaExpression { get; }
    public Type EntityType { get; }
    public Type PropertyType { get; }
    public Type? PreviousPropertyType { get; }
    public IncludeTypeEnum Type { get; }
}