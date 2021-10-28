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

using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using MikyM.Common.DataAccessLayer.Specifications.Helpers;

namespace MikyM.Common.DataAccessLayer.Specifications.Builders
{
    public static class IncludableBuilderExtensions
    {
        public static IIncludableSpecificationBuilder<TEntity, TProperty> ThenInclude<TEntity, TPreviousProperty,
            TProperty>(
            this IIncludableSpecificationBuilder<TEntity, TPreviousProperty> previousBuilder,
            Expression<Func<TPreviousProperty, TProperty>> thenIncludeExpression)
            where TEntity : class
        {
            var info = new IncludeExpressionInfo(thenIncludeExpression, typeof(TEntity), typeof(TProperty),
                typeof(TPreviousProperty));

            ((List<IncludeExpressionInfo>) previousBuilder.Specification.IncludeExpressions).Add(info);

            var includeBuilder = new IncludableSpecificationBuilder<TEntity, TProperty>(previousBuilder.Specification);

            return includeBuilder;
        }

        public static IIncludableSpecificationBuilder<TEntity, TProperty> ThenInclude<TEntity, TPreviousProperty,
            TProperty>(
            this IIncludableSpecificationBuilder<TEntity, IEnumerable<TPreviousProperty>> previousBuilder,
            Expression<Func<TPreviousProperty, TProperty>> thenIncludeExpression)
            where TEntity : class
        {
            var info = new IncludeExpressionInfo(thenIncludeExpression, typeof(TEntity), typeof(TProperty),
                typeof(TPreviousProperty));

            ((List<IncludeExpressionInfo>) previousBuilder.Specification.IncludeExpressions).Add(info);

            var includeBuilder = new IncludableSpecificationBuilder<TEntity, TProperty>(previousBuilder.Specification);

            return includeBuilder;
        }
    }
}