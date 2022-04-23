// This file is part of Lisbeth.Bot project
//
// Copyright (C) 2021-2022 Krzysztof Kupisz - MikyM
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

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.WebUtilities;
using MikyM.Common.DataAccessLayer.Filters;

namespace Lisbeth.Bot.Application.Services;

public class UriService : IUriService
{
    private readonly string _baseUri;

    public UriService(string baseUri)
    {
        _baseUri = baseUri;
    }

    public Uri GetPageUri(PaginationFilter filter, string route, IQueryCollection? queryParams = null)
    {
        var endpointUri = string.Concat(_baseUri, route);

        if (queryParams is not null)
        {
            var query = queryParams.Where(x =>
                !string.Equals(x.Key.ToLower(), "pagesize", StringComparison.InvariantCultureIgnoreCase) &&
                !string.Equals(x.Key.ToLower(), "pagenumber", StringComparison.InvariantCultureIgnoreCase));

            endpointUri = query.Aggregate(endpointUri,
                (currentOuter, param) => param.Value.Aggregate(currentOuter,
                    (currentInner, multiParam) =>
                        QueryHelpers.AddQueryString(currentInner, param.Key, multiParam)));
        }

        endpointUri = QueryHelpers.AddQueryString(endpointUri, "pageNumber", filter.PageNumber.ToString());
        endpointUri = QueryHelpers.AddQueryString(endpointUri, "pageSize", filter.PageSize.ToString());

        return new Uri(endpointUri);
    }
}