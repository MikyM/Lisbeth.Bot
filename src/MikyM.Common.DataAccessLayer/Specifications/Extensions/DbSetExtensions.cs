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

using System.Collections.Generic;
using System.Threading;

namespace MikyM.Common.DataAccessLayer.Specifications.Extensions;

public static class DbSetExtensions
{
    /*public static async Task<List<TSource>> ToListAsync<TSource>(this DbSet<TSource> source,
        ISpecification<TSource> specification, CancellationToken cancellationToken = default)
        where TSource : class
    {
        var result = await SpecificationEvaluator.Default.GetQuery(source, specification)
            .ToListAsync(cancellationToken);

        return specification.PostProcessingAction is null
            ? result
            : specification.PostProcessingAction(result).ToList();
    }

    public static async Task<IEnumerable<TSource>> ToEnumerableAsync<TSource>(this DbSet<TSource> source,
        ISpecification<TSource> specification, CancellationToken cancellationToken = default)
        where TSource : class
    {
        var result = await SpecificationEvaluator.Default.GetQuery(source, specification)
            .ToListAsync(cancellationToken);

        return specification.PostProcessingAction is null
            ? result
            : specification.PostProcessingAction(result);
    }

    public static IQueryable<TSource> WithSpecification<TSource>(this IQueryable<TSource> source,
        ISpecification<TSource> specification, ISpecificationEvaluator? evaluator = null) where TSource : class
    {
        evaluator ??= SpecificationEvaluator.Default;
        return evaluator.GetQuery(source, specification);
    }*/
}