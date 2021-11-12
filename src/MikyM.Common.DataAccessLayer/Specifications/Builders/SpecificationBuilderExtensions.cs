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

using AutoMapper;
using MikyM.Common.DataAccessLayer.Filters;
using MikyM.Common.DataAccessLayer.Specifications.Exceptions;
using MikyM.Common.DataAccessLayer.Specifications.Helpers;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace MikyM.Common.DataAccessLayer.Specifications.Builders;

public static class SpecificationBuilderExtensions
{
    public static ISpecificationBuilder<T, TResult> Expand<T, TResult>(
        this ISpecificationBuilder<T, TResult> specificationBuilder, Expression<Func<TResult, object>> expression)
        where T : class where TResult : class
    {
        specificationBuilder.Specification.MembersToExpand ??= new List<Expression<Func<TResult, object>>>();
        ((List<Expression<Func<TResult, object>>>)specificationBuilder.Specification.MembersToExpand).Add(expression);

        return specificationBuilder;
    }

    public static ISpecificationBuilder<T, TResult> Expand<T, TResult>(
        this ISpecificationBuilder<T, TResult> specificationBuilder, string member) where T : class where TResult : class
    {
        specificationBuilder.Specification.StringMembersToExpand ??= new List<string>();
        ((List<string>)specificationBuilder.Specification.StringMembersToExpand).Add(member);

        return specificationBuilder;
    }
    public static ISpecificationBuilder<T, TResult> WithMapperConfiguration<T, TResult>(
        this ISpecificationBuilder<T, TResult> specificationBuilder,
        MapperConfiguration mapperConfiguration) where T : class where TResult : class
    {
        specificationBuilder.Specification.MapperConfiguration = mapperConfiguration;

        return specificationBuilder;
    }

    public static ISpecificationBuilder<T> Where<T>(
        this ISpecificationBuilder<T> specificationBuilder,
        Expression<Func<T, bool>> criteria) where T : class
    {
        specificationBuilder.Specification.WhereExpressions ??= new List<Expression<Func<T, bool>>>();
        ((List<Expression<Func<T, bool>>>)specificationBuilder.Specification.WhereExpressions).Add(criteria);

        return specificationBuilder;
    }

    public static ISpecificationBuilder<T> GroupBy<T>(
        this ISpecificationBuilder<T> specificationBuilder,
        Expression<Func<T, object>> criteria) where T : class
    {
        specificationBuilder.Specification.GroupByExpression = criteria;

        return specificationBuilder;
    }

    public static IOrderedSpecificationBuilder<T> OrderBy<T>(
        this ISpecificationBuilder<T> specificationBuilder,
        Expression<Func<T, object?>> orderExpression) where T : class
    {
        specificationBuilder.Specification.OrderExpressions ??=
            new List<(Expression<Func<T, object>> KeySelector, OrderTypeEnum OrderType)>();
        ((List<(Expression<Func<T, object?>> OrderExpression, OrderTypeEnum OrderType)>) specificationBuilder
                .Specification.OrderExpressions)
            .Add((orderExpression, OrderTypeEnum.OrderBy));

        return new OrderedSpecificationBuilder<T>(specificationBuilder.Specification);
    }

    public static IOrderedSpecificationBuilder<T> OrderByDescending<T>(
        this ISpecificationBuilder<T> specificationBuilder,
        Expression<Func<T, object?>> orderExpression) where T : class
    {
        specificationBuilder.Specification.OrderExpressions ??=
            new List<(Expression<Func<T, object>> KeySelector, OrderTypeEnum OrderType)>();
        ((List<(Expression<Func<T, object?>> OrderExpression, OrderTypeEnum OrderType)>) specificationBuilder
                .Specification.OrderExpressions)
            .Add((orderExpression, OrderTypeEnum.OrderByDescending));

        return new OrderedSpecificationBuilder<T>(specificationBuilder.Specification);
    }

    public static IIncludableSpecificationBuilder<T, TProperty> Include<T, TProperty>(
        this ISpecificationBuilder<T> specificationBuilder,
        Expression<Func<T, TProperty>> includeExpression) where T : class
    {
        var info = new IncludeExpressionInfo(includeExpression, typeof(T), typeof(TProperty));

        specificationBuilder.Specification.IncludeExpressions ??= new List<IncludeExpressionInfo>();
        ((List<IncludeExpressionInfo>) specificationBuilder.Specification.IncludeExpressions).Add(info);

        return new IncludableSpecificationBuilder<T, TProperty>(specificationBuilder.Specification);
    }

    public static ISpecificationBuilder<T> Include<T>(
        this ISpecificationBuilder<T> specificationBuilder,
        string includeString) where T : class
    {
        specificationBuilder.Specification.IncludeStrings ??= new List<string>(); 
        ((List<string>) specificationBuilder.Specification.IncludeStrings).Add(includeString);
        return specificationBuilder;
    }


    public static ISpecificationBuilder<T> Search<T>(
        this ISpecificationBuilder<T> specificationBuilder,
        Expression<Func<T, string>> selector,
        string searchTerm,
        int searchGroup = 1) where T : class
    {
        specificationBuilder.Specification.SearchCriterias ??=
            new List<(Expression<Func<T, string>> Selector, string SearchTerm, int SearchGroup)>();
        ((List<(Expression<Func<T, string>> Selector, string SearchTerm, int SearchGroup)>)specificationBuilder
            .Specification.SearchCriterias).Add((selector, searchTerm, searchGroup));

        return specificationBuilder;
    }

    public static ISpecificationBuilder<T> Take<T>(
        this ISpecificationBuilder<T> specificationBuilder,
        int take) where T : class
    {
        if (specificationBuilder.Specification.Take is not null) throw new DuplicateTakeException();

        specificationBuilder.Specification.Take = take;
        specificationBuilder.Specification.IsPagingEnabled = true;
        return specificationBuilder;
    }

    public static ISpecificationBuilder<T> Skip<T>(
        this ISpecificationBuilder<T> specificationBuilder,
        int skip) where T : class
    {
        if (specificationBuilder.Specification.Skip is not null) throw new DuplicateSkipException();

        specificationBuilder.Specification.Skip = skip;
        specificationBuilder.Specification.IsPagingEnabled = true;
        return specificationBuilder;
    }

    public static ISpecificationBuilder<T> WithPaginationFilter<T>(
        this ISpecificationBuilder<T> specificationBuilder,
        PaginationFilter paginationFilter) where T : class
    {
        if (specificationBuilder.Specification.Skip is not null) throw new DuplicateSkipException();
        if (specificationBuilder.Specification.Take is not null) throw new DuplicateTakeException();
        if (specificationBuilder.Specification.PaginationFilter is not null) throw new DuplicatePaginationException();
        if (paginationFilter is null) throw new ArgumentNullException(nameof(paginationFilter));

        specificationBuilder.Specification.PaginationFilter = paginationFilter;

        return specificationBuilder;
    }

    public static ISpecificationBuilder<T> WithPostProcessingAction<T>(
        this ISpecificationBuilder<T> specificationBuilder,
        Func<IEnumerable<T>, IEnumerable<T>> predicate) where T : class
    {
        specificationBuilder.Specification.PostProcessingAction = predicate;

        return specificationBuilder;
    }

    /*public static ISpecificationBuilder<T, TResult> Select<T, TResult>(
        this ISpecificationBuilder<T, TResult> specificationBuilder,
        Expression<Func<T, TResult>> selector) where T : class where TResult : class
    {
        specificationBuilder.Specification.Selector = selector;

        return specificationBuilder;
    }*/

    public static ISpecificationBuilder<T, TResult> WithPostProcessingAction<T, TResult>(
        this ISpecificationBuilder<T, TResult> specificationBuilder,
        Func<IEnumerable<TResult>, IEnumerable<TResult>> predicate) where T : class where TResult : class
    {
        specificationBuilder.Specification.PostProcessingAction = predicate;

        return specificationBuilder;
    }


    /// <summary>
    ///     Explicitly enables or disables caching for this query.
    /// </summary>
    public static ICacheSpecificationBuilder<T> WithCaching<T>(
        this ISpecificationBuilder<T> specificationBuilder, bool withCaching = true) where T : class
    {
        specificationBuilder.Specification.IsCacheEnabled = withCaching;

        return new CacheSpecificationBuilder<T>(specificationBuilder.Specification);
    }

    public static ISpecificationBuilder<T> AsTracking<T>(
        this ISpecificationBuilder<T> specificationBuilder) where T : class
    {
        specificationBuilder.Specification.IsAsNoTracking = false;

        return specificationBuilder;
    }

    public static ISpecificationBuilder<T> AsSplitQuery<T>(
        this ISpecificationBuilder<T> specificationBuilder) where T : class
    {
        specificationBuilder.Specification.IsAsSplitQuery = true;

        return specificationBuilder;
    }

    public static ISpecificationBuilder<T> AsNoTrackingWithIdentityResolution<T>(
        this ISpecificationBuilder<T> specificationBuilder) where T : class
    {
        specificationBuilder.Specification.IsAsNoTrackingWithIdentityResolution = true;

        return specificationBuilder;
    }
}