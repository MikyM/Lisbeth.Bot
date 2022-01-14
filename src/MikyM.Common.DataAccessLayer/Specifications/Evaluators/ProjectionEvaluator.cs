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

using AutoMapper.QueryableExtensions;

namespace MikyM.Common.DataAccessLayer.Specifications.Evaluators;

public class ProjectionEvaluator : IProjectionEvaluator
{
    public static ProjectionEvaluator Instance { get; } = new();

    private ProjectionEvaluator()
    {
        
    }

    public IQueryable<TResult> GetQuery<T, TResult>(IQueryable<T> query, ISpecification<T, TResult> specification) where T : class where TResult : class
    {
        if (specification.MembersToExpand is not null)
        {
            return specification.MapperConfiguration is null
                ? query.ProjectTo(specification.MembersToExpand.ToArray())
                : query.ProjectTo(specification.MapperConfiguration,
                    specification.MembersToExpand.ToArray());
        }

        if (specification.StringMembersToExpand is not null)
        {
            return specification.MapperConfiguration is null
                ? query.ProjectTo<TResult>(null, specification.StringMembersToExpand.ToArray())
                : query.ProjectTo<TResult>(specification.MapperConfiguration, null,
                    specification.StringMembersToExpand.ToArray());
        }

        return specification.MapperConfiguration is not null
            ? query.ProjectTo<TResult>(specification.MapperConfiguration)
            : query.ProjectTo<TResult>();
    }
}