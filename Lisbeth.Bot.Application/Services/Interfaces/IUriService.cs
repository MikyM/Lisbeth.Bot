using System;
using Microsoft.AspNetCore.Http;
using MikyM.Common.DataAccessLayer.Filters;

namespace Lisbeth.Bot.Application.Services.Interfaces
{
    public interface IUriService
    {
        public Uri GetPageUri(PaginationFilter filter, string route, IQueryCollection queryParams = null);
    }
}
