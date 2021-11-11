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
using System.Collections.Generic;
using System.Linq.Expressions;
using AutoMapper;
using MikyM.Common.DataAccessLayer.Specifications.Builders;
using MikyM.Common.DataAccessLayer.Specifications.Evaluators;
using MikyM.Common.DataAccessLayer.Specifications.Helpers;

namespace MikyM.Common.DataAccessLayer.Specifications
{
    /// <inheritdoc cref="ISpecification{T,TResult}" />
    public
        class Specification<T, TResult> : Specification<T>, ISpecification<T, TResult>
        where T : class where TResult : class
    {
        protected Specification() : this(InMemorySpecificationEvaluator.Default)
        {
        }

        public Specification(Expression<Func<T, bool>> criteria) : this(InMemorySpecificationEvaluator.Default)
        {
            Where(criteria);
        }

        protected Specification(IInMemorySpecificationEvaluator inMemorySpecificationEvaluator) : base(
            inMemorySpecificationEvaluator)
        {
            Query = new SpecificationBuilder<T, TResult>(this);
        }

        protected new virtual ISpecificationBuilder<T, TResult> Query { get; }

        public new virtual IEnumerable<T> Evaluate(IEnumerable<T> entities)
        {
            return Evaluator.Evaluate(entities, this);
        }

        public MapperConfiguration? MapperConfiguration { get; }
        public IEnumerable<Expression<Func<TResult, object>>>? MembersToExpand { get; }
        public IEnumerable<string>? StringMembersToExpand { get; }
        public new Func<IEnumerable<TResult>, IEnumerable<TResult>>? PostProcessingAction { get; internal set; }
    }

    /// <inheritdoc cref="ISpecification{T}" />
    public class Specification<T> : ISpecification<T> where T : class
    {
        protected Specification() : this(InMemorySpecificationEvaluator.Default)
        {
        }

        public Specification(Expression<Func<T, bool>> criteria) : this(InMemorySpecificationEvaluator.Default)
        {
            Where(criteria);
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

        public IEnumerable<Expression<Func<T, bool>>>? WhereExpressions { get; internal set; }

        public IEnumerable<(Expression<Func<T, object>> KeySelector, OrderTypeEnum OrderType)>? OrderExpressions
        {
            get;
            internal set;
        }

        public IEnumerable<IncludeExpressionInfo>? IncludeExpressions { get; internal set; }

        public Expression<Func<T, object>>? GroupByExpression { get; internal set; }

        public IEnumerable<string>? IncludeStrings { get; internal set; }

        public IEnumerable<(Expression<Func<T, string>> Selector, string SearchTerm, int SearchGroup)>? SearchCriterias
        {
            get;
            internal set;
        }

        public int? Take { get; internal set; }

        public int? Skip { get; internal set; }

        public Func<IEnumerable<T>, IEnumerable<T>>? PostProcessingAction { get; internal set; }
        public string? CacheKey { get; internal set; }
        public bool CacheEnabled { get; internal set; } = true;
        public bool IsPagingEnabled { get; internal set; }
        public bool AsNoTracking { get; internal set; } = true;
        public bool AsSplitQuery { get; internal set; }
        public bool AsNoTrackingWithIdentityResolution { get; internal set; }

        protected Specification<T> Include<TProperty>(Expression<Func<T, TProperty>> includeExpression)
        {
            Query.Include(includeExpression);
            return this;
        }

        protected IIncludableSpecificationBuilder<T, TProperty> IncludeWithChildren<TProperty>(
            Expression<Func<T, TProperty>> includeExpression)
        {
            return Query.Include(includeExpression);
        }

        protected Specification<T> Include(string includeExpression)
        {
            Query.Include(includeExpression);
            return this;
        }

        protected Specification<T> OrderBy(Expression<Func<T, object?>> orderByExpression)
        {
            Query.OrderBy(orderByExpression);
            return this;
        }

        protected Specification<T> OrderByDescending(Expression<Func<T, object?>> orderByDescendingExpression)
        {
            Query.OrderByDescending(orderByDescendingExpression);
            return this;
        }

        protected Specification<T> Where(Expression<Func<T, bool>> criteria)
        {
            Query.Where(criteria);
            return this;
        }

        protected Specification<T> GroupBy(Expression<Func<T, object>> criteria)
        {
            Query.GroupBy(criteria);
            return this;
        }

        protected Specification<T> Search(Expression<Func<T, string>> selector, string searchTerm, int searchGroup = 1)
        {
            Query.Search(selector, searchTerm, searchGroup);
            return this;
        }

        protected Specification<T> ApplyTake(int limit)
        {
            Take = limit;
            return this;
        }

        protected Specification<T> ApplySkip(int skip)
        {
            Skip = skip;
            return this;
        }

        protected Specification<T> DisableCache(int limit)
        {
            CacheEnabled = false;
            return this;
        }

        protected Specification<T> WithTracking(bool withTracking = true)
        {
            AsNoTracking = withTracking;
            return this;
        }

        protected Specification<T> TreatAsSplitQuery(bool asSplitQuery = true)
        {
            AsSplitQuery = true;
            return this;
        }

        protected Specification<T> TreatAsNoTrackingWithIdentityResolution()
        {
            AsNoTrackingWithIdentityResolution = true;
            return this;
        }
    }
}