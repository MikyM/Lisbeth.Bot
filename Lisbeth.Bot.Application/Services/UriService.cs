using Lisbeth.Bot.Application.Interfaces;
using Microsoft.AspNetCore.WebUtilities;
using MikyM.Common.DataAccessLayer.Filters;
using System;
using System.Linq;
using Microsoft.AspNetCore.Http;

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
                foreach (var param in query)
                {
                    foreach (var multiParam in param.Value)
                    {
                        endpointUri = QueryHelpers.AddQueryString(endpointUri, param.Key, multiParam);
                    }
                }
            }

            endpointUri = QueryHelpers.AddQueryString(endpointUri, "pageNumber", filter.PageNumber.ToString());
            endpointUri = QueryHelpers.AddQueryString(endpointUri, "pageSize", filter.PageSize.ToString());

            return new Uri(endpointUri);
        }
    }
}
