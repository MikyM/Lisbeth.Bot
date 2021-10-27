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

namespace MikyM.Common.DataAccessLayer.Specifications
{
    public class Specifications<T> : ISpecifications<T>
    {
        public Specifications()
        {
        }

        public Specifications(Expression<Func<T, bool>> filterCondition, int limit = 0)
        {
            FilterConditions.Add(filterCondition);
            Limit = limit;
        }

        public Specifications(List<Expression<Func<T, bool>>> filterConditions, int limit = 0)
        {
            FilterConditions = filterConditions;
            Limit = limit;
        }

        public List<Expression<Func<T, bool>>> FilterConditions { get; } = new();
        public Expression<Func<T, object>> OrderBy { get; private set; }
        public Expression<Func<T, object>> OrderByDescending { get; private set; }
        public List<Expression<Func<T, object>>> Includes { get; } = new();
        public List<string> StringIncludes { get; } = new();
        public Expression<Func<T, object>> GroupBy { get; private set; }
        public int Limit { get; private set; }

        protected Specifications<T> AddInclude(Expression<Func<T, object>> includeExpression)
        {
            Includes.Add(includeExpression);
            return this;
        }

        protected Specifications<T> AddInclude(string include)
        {
            StringIncludes.Add(include);
            return this;
        }

        protected Specifications<T> ApplyOrderBy(Expression<Func<T, object>> orderByExpression)
        {
            OrderBy = orderByExpression;
            return this;
        }

        protected Specifications<T> ApplyOrderByDescending(Expression<Func<T, object>> orderByDescendingExpression)
        {
            OrderByDescending = orderByDescendingExpression;
            return this;
        }

        protected Specifications<T> AddFilterCondition(Expression<Func<T, bool>> filterExpression)
        {
            FilterConditions.Add(filterExpression);
            return this;
        }

        protected Specifications<T> ApplyGroupBy(Expression<Func<T, object>> groupByExpression)
        {
            GroupBy = groupByExpression;
            return this;
        }

        protected Specifications<T> ApplyLimit(int limit)
        {
            Limit = limit;
            return this;
        }
    }
}