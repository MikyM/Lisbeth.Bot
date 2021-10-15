// This file is part of Lisbeth.Bot project
//
// Copyright (C) 2021 MikyM
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
using System.Linq;
using Lisbeth.Bot.Application.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.WebUtilities;
using MikyM.Common.DataAccessLayer.Filters;

namespace Lisbeth.Bot.Application.Services
{
    public class UriService : IUriService
    {
        private readonly string _baseUri;

        public UriService(string baseUri)
        {
            _baseUri = baseUri;
        }

        public Uri GetPageUri(PaginationFilter filter, string route, IQueryCollection queryParams = null)
        {
            var endpointUri = string.Concat(_baseUri, route);

            if (queryParams != null)
            {
                var query = queryParams.Where(x => x.Key.ToLower() != "pagesize" && x.Key.ToLower() != "pagenumber");

                endpointUri = query.Aggregate(endpointUri, (currentOuter, param) =>
                    param.Value.Aggregate(currentOuter, (currentInner, multiParam) =>
                        QueryHelpers.AddQueryString(currentInner, param.Key, multiParam)));
            }

            endpointUri = QueryHelpers.AddQueryString(endpointUri, "pageNumber", filter.PageNumber.ToString());
            endpointUri = QueryHelpers.AddQueryString(endpointUri, "pageSize", filter.PageSize.ToString());

            return new Uri(endpointUri);
        }
    }
}