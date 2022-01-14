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
using MikyM.Common.DataAccessLayer.Specifications.Exceptions;

namespace MikyM.Common.DataAccessLayer.Specifications.Evaluators;

public class InMemorySpecificationEvaluator : IInMemorySpecificationEvaluator
{
    // Will use singleton for default configuration. Yet, it can be instantiated if necessary, with default or provided evaluators.
    public static InMemorySpecificationEvaluator Default { get; } = new();

    private readonly List<IInMemoryEvaluator> _evaluators = new();

    private InMemorySpecificationEvaluator()
    {
        this._evaluators.AddRange(new IInMemoryEvaluator[]
        {
            WhereEvaluator.Instance,
            SearchEvaluator.Instance,
            OrderEvaluator.Instance,
            PaginationEvaluator.Instance
        });
    }

    private InMemorySpecificationEvaluator(IEnumerable<IInMemoryEvaluator> evaluators)
    {
        this._evaluators.AddRange(evaluators);
    }

    public virtual IEnumerable<TResult> Evaluate<T, TResult>(IEnumerable<T> source,
        ISpecification<T, TResult> specification) where T : class
    {
        _ = specification.Selector ?? throw new SelectorNotFoundException();

        var baseQuery = Evaluate(source, (ISpecification<T>)specification);

        var resultQuery = baseQuery.Select(specification.Selector.Compile());

        return specification.PostProcessingAction == null
            ? resultQuery
            : specification.PostProcessingAction(resultQuery);
    }

    public virtual IEnumerable<T> Evaluate<T>(IEnumerable<T> source, ISpecification<T> specification) where T : class
    {
        {
            foreach (var evaluator in _evaluators)
            {
                source = evaluator.Evaluate(source, specification);
            }

            return specification.PostProcessingAction == null
                ? source
                : specification.PostProcessingAction(source);
        }
    }
}