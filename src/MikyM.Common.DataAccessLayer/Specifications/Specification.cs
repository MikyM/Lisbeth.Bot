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

using MikyM.Common.DataAccessLayer.Specifications.Builders;
using MikyM.Common.DataAccessLayer.Specifications.Evaluators;
using MikyM.Common.DataAccessLayer.Specifications.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace MikyM.Common.DataAccessLayer.Specifications
{
    /// <inheritdoc cref="ISpecification{T,TResult}" />
    public /*abstract*/ class Specification<T, TResult> : Specification<T>, ISpecification<T, TResult> where T : class where TResult : class
    {
        protected Specification() : this(InMemorySpecificationEvaluator.Default)
        {
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

        public Expression<Func<T, TResult>>? Selector { get; internal set; }

        public new Func<IEnumerable<TResult>, IEnumerable<TResult>>? PostProcessingAction { get; internal set; } = null;
    }

    /// <inheritdoc cref="ISpecification{T}" />
    public /*abstract*/ class Specification<T> : ISpecification<T> where T : class
    {
        protected Specification() : this(InMemorySpecificationEvaluator.Default)
        {
        }

        public Specification(Expression<Func<T, bool>> criteria, int limit = 0) : this(InMemorySpecificationEvaluator.Default)
        {
            AddFilterCondition(criteria);
            ApplyTake(limit);
        }

        public Specification(List<Expression<Func<T, bool>>> criteria, int limit = 0) : this(InMemorySpecificationEvaluator.Default)
        {
            WhereExpressions = criteria ?? throw new ArgumentNullException(nameof(criteria));
            ApplyTake(limit);
        }

        protected Specification(IInMemorySpecificationEvaluator inMemorySpecificationEvaluator)
        {
            Evaluator = inMemorySpecificationEvaluator;
            Query = new SpecificationBuilder<T>(this);
        }

        protected IInMemorySpecificationEvaluator Evaluator { get; }
        protected virtual ISpecificationBuilder<T> Query { get; }

        public virtual IEnumerable<T> Evaluate(IEnumerable<T> entities)
        {
            return Evaluator.Evaluate(entities, this);
        }

        public IEnumerable<Expression<Func<T, bool>>> WhereExpressions { get; } = new List<Expression<Func<T, bool>>>();

        public IEnumerable<(Expression<Func<T, object>> KeySelector, OrderTypeEnum OrderType)>
            OrderExpressions { get; } = new List<(Expression<Func<T, object>> KeySelector, OrderTypeEnum OrderType)>();

        public IEnumerable<IncludeExpressionInfo> IncludeExpressions { get; } = new List<IncludeExpressionInfo>();

        public Expression<Func<T, bool>> GroupByExpression { get; internal set; }

        public IEnumerable<string> IncludeStrings { get; } = new List<string>();

        public IEnumerable<(Expression<Func<T, string>> Selector, string SearchTerm, int SearchGroup)> SearchCriterias
        {
            get;
        } = new List<(Expression<Func<T, string>> Selector, string SearchTerm, int SearchGroup)>();

        public int? Take { get; internal set; } = null;

        public int? Skip { get; internal set; } = null;

        public Func<IEnumerable<T>, IEnumerable<T>>? PostProcessingAction { get; internal set; } = null;
        public string? CacheKey { get; internal set; }
        public bool CacheEnabled { get; internal set; } = false;
        public bool IsPagingEnabled { get; internal set; } = false;
        public bool AsNoTracking { get; internal set; } = true;
        public bool AsSplitQuery { get; internal set; } = false;
        public bool AsNoTrackingWithIdentityResolution { get; internal set; } = false;

        protected Specification<T> AddInclude<TProperty>(Expression<Func<T, TProperty>> includeExpression)
        {
            this.Query.Include(includeExpression);
            return this;
        }

        protected IIncludableSpecificationBuilder<T, TProperty> AddNestedInclude<TProperty>(Expression<Func<T, TProperty>> includeExpression)
        {
            return this.Query.Include(includeExpression);
        }

        protected Specification<T> AddInclude(string includeExpression)
        {
            this.Query.Include(includeExpression);
            return this;
        }

        protected Specification<T> ApplyOrderBy(Expression<Func<T, object>> orderByExpression)
        {
            this.Query.OrderBy(orderByExpression);
            return this;
        }

        protected Specification<T> ApplyOrderByDescending(Expression<Func<T, object>> orderByDescendingExpression)
        {
            this.Query.OrderByDescending(orderByDescendingExpression);
            return this;
        }

        protected Specification<T> AddFilterCondition(Expression<Func<T, bool>> criteria)
        {
            this.Query.Where(criteria);
            return this;
        }

        protected Specification<T> ApplyGroupBy(Expression<Func<T, bool>> criteria)
        {
            this.Query.GroupBy(criteria);
            return this;
        }

        protected Specification<T> ApplySearch(Expression<Func<T, string>> selector, string searchTerm, int searchGroup = 1)
        {
            this.Query.Search(selector, searchTerm, searchGroup);
            return this;
        }

        protected Specification<T> ApplyTake(int limit)
        {
            this.Take = limit;
            return this;
        }

        protected Specification<T> ApplySkip(int skip)
        {
            this.Skip = skip;
            return this;
        }

        protected Specification<T> DisableCache(int limit)
        {
            this.CacheEnabled = false;
            return this;
        }

        protected Specification<T> ApplyTracking()
        {
            this.AsNoTracking = false;
            return this;
        }

        protected Specification<T> ApplySplitQuery()
        {
            this.AsSplitQuery = true;
            return this;
        }

        protected Specification<T> ApplyAsNoTrackingWithIdentityResolution()
        {
            this.AsNoTrackingWithIdentityResolution = true;
            return this;
        }
    }
}