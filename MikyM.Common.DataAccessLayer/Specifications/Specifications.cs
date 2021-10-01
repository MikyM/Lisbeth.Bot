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

        public List<Expression<Func<T, bool>>> FilterConditions { get; private set; } = new();
        public Expression<Func<T, object>> OrderBy { get; private set; }
        public Expression<Func<T, object>> OrderByDescending { get; private set; }
        public List<Expression<Func<T, object>>> Includes { get; } = new();
        public Expression<Func<T, object>> GroupBy { get; private set; }
        public int Limit { get; private set; } = 0;

        protected void AddInclude(Expression<Func<T, object>> includeExpression)
        {
            Includes.Add(includeExpression);
        }

        protected void ApplyOrderBy(Expression<Func<T, object>> orderByExpression)
        {
            OrderBy = orderByExpression;
        }

        protected void ApplyOrderByDescending(Expression<Func<T, object>> orderByDescendingExpression)
        {
            OrderByDescending = orderByDescendingExpression;
        }
        protected void ApplyFilterCondition(Expression<Func<T, bool>> filterExpression)
        {
            FilterConditions.Add(filterExpression);
        }

        protected void ApplyGroupBy(Expression<Func<T, object>> groupByExpression)
        {
            GroupBy = groupByExpression;
        }

        protected void ApplyLimit(int limit)
        {
            Limit = limit;
        }
    }
}