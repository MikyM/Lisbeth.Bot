using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace MikyM.Common.DataAccessLayer.Specifications
{
    public static class SpecificationEvaluator<TEntity> where TEntity : class
    {
        public static IQueryable<TEntity> GetQuery(IQueryable<TEntity> query, ISpecifications<TEntity> specifications)
        {
            if (specifications == null)
            {
                return query;
            }

            if (specifications.FilterCondition != null)
            {
                query = query.Where(specifications.FilterCondition);
            }

            query = specifications.Includes.Aggregate(query, (current, include) => current.Include(include));

            if (specifications.OrderBy != null)
            {
                query = query.OrderBy(specifications.OrderBy);
            }
            else if (specifications.OrderByDescending != null)
            {
                query = query.OrderByDescending(specifications.OrderByDescending);
            }

            if (specifications.GroupBy != null)
            {
                query = query.GroupBy(specifications.GroupBy).SelectMany(x => x);
            }

            return query;
        }
    }
}
