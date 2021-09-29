using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace MikyM.Common.DataAccessLayer.Specifications
{
    public interface ISpecifications<T>
    {
        Expression<Func<T, bool>> FilterCondition { get; }
        Expression<Func<T, object>> OrderBy { get; }
        Expression<Func<T, object>> OrderByDescending { get; }
        List<Expression<Func<T, object>>> Includes { get; }
        Expression<Func<T, object>> GroupBy { get; }
    }
}
