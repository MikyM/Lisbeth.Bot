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
using System.Linq;

namespace MikyM.Common.DataAccessLayer.Specifications.Evaluators
{
    public class InMemorySpecificationEvaluator : IInMemorySpecificationEvaluator
    {
        private readonly List<IInMemoryEvaluator> _evaluators = new();

        public InMemorySpecificationEvaluator()
        {
            _evaluators.AddRange(new IInMemoryEvaluator[]
            {
                WhereEvaluator.Instance,
                OrderEvaluator.Instance,
                PaginationEvaluator.Instance,
                GroupByEvaluator.Instance
            });
        }

        public InMemorySpecificationEvaluator(IEnumerable<IInMemoryEvaluator> evaluators)
        {
            _evaluators.AddRange(evaluators);
        }

        // Will use singleton for default configuration. Yet, it can be instantiated if necessary, with default or provided evaluators.
        public static InMemorySpecificationEvaluator Default { get; } = new();

        public virtual IEnumerable<T> Evaluate<T>(IEnumerable<T> source, ISpecification<T> specification)
            where T : class
        {
            if ((specification.SearchCriterias ?? throw new InvalidOperationException()).Any())
                throw new NotSupportedException(
                    "The specification contains Search expressions and can't be evaluated with in-memory evaluator.");

            source = _evaluators.Aggregate(source, (current, evaluator) => evaluator.Evaluate(current, specification));

            return specification.PostProcessingAction is null
                ? source
                : specification.PostProcessingAction(source);
        }
    }
}