using MikyM.Common.DataAccessLayer.Filters;
using System;
using Microsoft.AspNetCore.Http;

namespace Lisbeth.Bot.Application.Interfaces
{
    public interface IUriService
    {
        public Uri GetPageUri(PaginationFilter filter, string route, IQueryCollection queryParams = null);
    }
}
