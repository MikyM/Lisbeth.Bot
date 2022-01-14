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
using EFCoreSecondLevelCacheInterceptor;
using MikyM.Common.DataAccessLayer.Filters;
using MikyM.Common.DataAccessLayer.Specifications.Builders;
using MikyM.Common.DataAccessLayer.Specifications.Expressions;
using MikyM.Common.DataAccessLayer.Specifications.Validators;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace MikyM.Common.DataAccessLayer.Specifications;

/// <inheritdoc cref="ISpecification{T,TResult}" />
public
    class Specification<T, TResult> : Specification<T>, ISpecification<T, TResult>
    where T : class where TResult : class
{
    protected internal Specification() : this(InMemorySpecificationEvaluator.Default)
    {
    }

    public Specification(PaginationFilter paginationFilter) : this(InMemorySpecificationEvaluator.Default, paginationFilter)
    {
    }

    protected Specification(IInMemorySpecificationEvaluator inMemorySpecificationEvaluator, PaginationFilter paginationFilter) : base(
        inMemorySpecificationEvaluator, paginationFilter)
    {
        Query = new SpecificationBuilder<T, TResult>(this);
    }

    protected Specification(IInMemorySpecificationEvaluator inMemorySpecificationEvaluator) : base(
        inMemorySpecificationEvaluator)
    {
        Query = new SpecificationBuilder<T, TResult>(this);
    }

    protected new virtual ISpecificationBuilder<T, TResult> Query { get; }

    public new virtual IEnumerable<TResult> Evaluate(IEnumerable<T> entities)
    {
        return Evaluator.Evaluate(entities, this);
    }

    /// <inheritdoc/>
    public Expression<Func<T, TResult>>? Selector { get; internal set; }

    public MapperConfiguration? MapperConfiguration { get; internal set; }
    public IEnumerable<Expression<Func<TResult, object>>>? MembersToExpand { get; internal set; }
    public IEnumerable<string>? StringMembersToExpand { get; internal set; }
    public new Func<IEnumerable<TResult>, IEnumerable<TResult>>? PostProcessingAction { get; internal set; }

    protected Specification<T> WithPostProcessingAction(Func<IEnumerable<TResult>, IEnumerable<TResult>> postProcessingAction)
    {
        this.Query.WithPostProcessingAction(postProcessingAction);
        return this;
    }

    protected Specification<T> WithMapperConfiguration(MapperConfiguration mapperConfiguration)
    {
        this.Query.WithMapperConfiguration(mapperConfiguration);
        return this;
    }

    protected Specification<T> Expand(Expression<Func<TResult, object>> expression)
    {
        this.Query.Expand(expression);
        return this;
    }

    protected Specification<T> Expand(string member)
    {
        this.Query.Expand(member);
        return this;
    }
}

/// <inheritdoc cref="ISpecification{T}" />
public class Specification<T> : ISpecification<T> where T : class
{
    protected Specification(PaginationFilter paginationFilter)
        : this(InMemorySpecificationEvaluator.Default, SpecificationValidator.Default, paginationFilter)
    {
    }

    protected Specification()
        : this(InMemorySpecificationEvaluator.Default, SpecificationValidator.Default)
    {
    }

    protected Specification(IInMemorySpecificationEvaluator inMemorySpecificationEvaluator)
        : this(inMemorySpecificationEvaluator, SpecificationValidator.Default)
    {
    }

    protected Specification(IInMemorySpecificationEvaluator inMemorySpecificationEvaluator, PaginationFilter paginationFilter)
        : this(inMemorySpecificationEvaluator, SpecificationValidator.Default, paginationFilter)
    {
    }

    protected Specification(ISpecificationValidator specificationValidator, PaginationFilter paginationFilter)
        : this(InMemorySpecificationEvaluator.Default, specificationValidator, paginationFilter)
    {
    }

    protected Specification(ISpecificationValidator specificationValidator)
        : this(InMemorySpecificationEvaluator.Default, specificationValidator)
    {
    }

    protected Specification(IInMemorySpecificationEvaluator inMemorySpecificationEvaluator, ISpecificationValidator specificationValidator)
    {
        this.Evaluator = inMemorySpecificationEvaluator;
        this.Validator = specificationValidator;
        this.Query = new SpecificationBuilder<T>(this);
    }

    protected Specification(IInMemorySpecificationEvaluator inMemorySpecificationEvaluator, ISpecificationValidator specificationValidator, PaginationFilter paginationFilter)
    {
        this.Evaluator = inMemorySpecificationEvaluator;
        this.Validator = specificationValidator;
        this.Query = new SpecificationBuilder<T>(this);
        this.PaginationFilter = paginationFilter;
    }

    protected IInMemorySpecificationEvaluator Evaluator { get; }
    protected ISpecificationValidator Validator { get; }
    protected virtual ISpecificationBuilder<T> Query { get; }

    public virtual IEnumerable<T> Evaluate(IEnumerable<T> entities)
    {
        return Evaluator.Evaluate(entities, this);
    }

    /// <inheritdoc/>
    public virtual bool IsSatisfiedBy(T entity)
    {
        return Validator.IsValid(entity, this);
    }

    public TimeSpan? CacheTimeout { get; internal set; }

    public CacheExpirationMode? CacheExpirationMode { get; internal set; }

    public IEnumerable<WhereExpressionInfo<T>>? WhereExpressions { get; internal set; }

    public IEnumerable<OrderExpressionInfo<T>>? OrderExpressions
    {
        get;
        internal set;
    }

    public IEnumerable<IncludeExpressionInfo>? IncludeExpressions { get; internal set; }

    public Expression<Func<T, object>>? GroupByExpression { get; internal set; }

    public IEnumerable<string>? IncludeStrings { get; internal set; }

    public IEnumerable<SearchExpressionInfo<T>>? SearchCriterias
    {
        get;
        internal set;
    }

    /// <inheritdoc/>
    public bool IgnoreQueryFilters { get; internal set; } = false;

    public int? Take { get; internal set; }

    public int? Skip { get; internal set; }

    private PaginationFilter? _paginationFilter;
    public PaginationFilter? PaginationFilter
    {
        get
        {
            if (this._paginationFilter is not null) return this._paginationFilter;
            if (!this.Take.HasValue || !this.Skip.HasValue) return null;

            this._paginationFilter = new PaginationFilter(this.Skip.Value / this.Take.Value + 1, this.Take.Value);
            if (!this.IsPagingEnabled) this.IsPagingEnabled = true;

            return this._paginationFilter;

        }
        internal set
        {
            if (value is null)
            {
                this._paginationFilter = value;
                return;
            }

            this._paginationFilter = value;
            this.Skip = (value.PageNumber - 1) * value.PageSize;
            this.Take = value.PageSize;

            this.IsPagingEnabled = true;
        }
    }

    public Func<IEnumerable<T>, IEnumerable<T>>? PostProcessingAction { get; internal set; }
    public bool? IsCacheEnabled { get; internal set; }
    public bool IsPagingEnabled { get; internal set; }
    public bool IsAsNoTracking { get; internal set; } = true;
    public bool IsAsSplitQuery { get; internal set; }
    public bool IsAsNoTrackingWithIdentityResolution { get; internal set; }


    protected ISpecificationBuilder<T> WithIgnoreQueryFilters(bool ignore = true)
    {
        return this.Query.IgnoreQueryFilters(ignore);
    }

    protected ISpecificationBuilder<T> WithPostProcessingAction(Func<IEnumerable<T>, IEnumerable<T>> postProcessingAction)
    {
        return this.Query.WithPostProcessingAction(postProcessingAction);
    }

    protected IIncludableSpecificationBuilder<T, TProperty> Include<TProperty>(
        Expression<Func<T, TProperty>> includeExpression)
    {
        return this.Query.Include(includeExpression);
    }

    protected ISpecificationBuilder<T> OrderBy(Expression<Func<T, object?>> orderByExpression)
    {
        return this.Query.OrderBy(orderByExpression);
    }

    protected ISpecificationBuilder<T> OrderByDescending(Expression<Func<T, object?>> orderByDescendingExpression)
    {
        return this.Query.OrderByDescending(orderByDescendingExpression);
    }

    protected ISpecificationBuilder<T> Where(Expression<Func<T, bool>> criteria)
    {
        return this.Query.Where(criteria);
 }

    protected ISpecificationBuilder<T> GroupBy(Expression<Func<T, object>> criteria)
    {
        return this.Query.GroupBy(criteria);
    }

    protected ISpecificationBuilder<T> Search(Expression<Func<T, string>> selector, string searchTerm, int searchGroup = 1)
    {
        return this.Query.Search(selector, searchTerm, searchGroup);
    }
        
    protected ISpecificationBuilder<T> WithPaginationFilter(PaginationFilter paginationFilter)
    {
        return this.Query.WithPaginationFilter(paginationFilter);
    }

    protected ISpecificationBuilder<T> ApplyTake(int take)
    {
        return this.Query.Take(take);
    }

    protected ISpecificationBuilder<T> ApplySkip(int skip)
    {
        return this.Query.Skip(skip);
    }

    protected ICacheSpecificationBuilder<T> WithCaching(bool withCaching = true)
    {
        return this.Query.WithCaching(withCaching); 
    }

    protected ISpecificationBuilder<T> AsTracking()
    {
        return this.Query.AsTracking();
    }

    protected ISpecificationBuilder<T> AsSplitQuery()
    {
        return this.Query.AsSplitQuery();
    }

    protected ISpecificationBuilder<T> AsNoTrackingWithIdentityResolution()
    {
        return this.Query.AsNoTrackingWithIdentityResolution();
    }
}