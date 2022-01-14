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

using System.Linq.Expressions;
using MikyM.Common.DataAccessLayer.Specifications.Helpers;

namespace MikyM.Common.DataAccessLayer.Specifications.Expressions;

/// <summary>
/// Encapsulates data needed to build Include/ThenInclude query.
/// </summary>
public class IncludeExpressionInfo
{
    /// <summary>
    /// If <see cref="Type" /> is <see cref="IncludeTypeEnum.Include" />, represents a related entity that should be included.<para />
    /// If <see cref="Type" /> is <see cref="IncludeTypeEnum.ThenInclude" />, represents a related entity that should be included as part of the previously included entity.
    /// </summary>
    public LambdaExpression LambdaExpression { get; }

    /// <summary>
    /// The type of the source entity.
    /// </summary>
    public Type EntityType { get; }

    /// <summary>
    /// The type of the included entity.
    /// </summary>
    public Type PropertyType { get; }

    /// <summary>
    /// The type of the previously included entity.
    /// </summary>
    public Type? PreviousPropertyType { get; }

    /// <summary>
    /// The include type.
    /// </summary>
    public IncludeTypeEnum Type { get; }

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
        {
            _ = previousPropertyType ?? throw new ArgumentNullException(nameof(previousPropertyType));
        }

        this.LambdaExpression = expression;
        this.EntityType = entityType;
        this.PropertyType = propertyType;
        this.PreviousPropertyType = previousPropertyType;
        this.Type = includeType;
    }

    /// <summary>
    /// Creates instance of <see cref="IncludeExpressionInfo" /> which describes 'Include' query part.<para />
    /// Source (entityType) -> Include (propertyType).
    /// </summary>
    /// <param name="expression">The expression represents a related entity that should be included.</param>
    /// <param name="entityType">The type of the source entity.</param>
    /// <param name="propertyType">The type of the included entity.</param>
    public IncludeExpressionInfo(LambdaExpression expression,
                                 Type entityType,
                                 Type propertyType)
        : this(expression, entityType, propertyType, null, IncludeTypeEnum.Include)
    {
    }

    /// <summary>
    /// Creates instance of <see cref="IncludeExpressionInfo" /> which describes 'ThenInclude' query part.<para />
    /// Source (entityType) -> Include (previousPropertyType) -> ThenInclude (propertyType).
    /// </summary>
    /// <param name="expression">The expression represents a related entity that should be included as part of the previously included entity.</param>
    /// <param name="entityType">The type of the source entity.</param>
    /// <param name="propertyType">The type of the included entity.</param>
    /// <param name="previousPropertyType">The type of the previously included entity.</param>
    public IncludeExpressionInfo(LambdaExpression expression,
                                 Type entityType,
                                 Type propertyType,
                                 Type previousPropertyType)
        : this(expression, entityType, propertyType, previousPropertyType, IncludeTypeEnum.ThenInclude)
    {
    }
}