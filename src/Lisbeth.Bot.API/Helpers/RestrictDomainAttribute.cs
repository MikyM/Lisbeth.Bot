using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Lisbeth.Bot.API.Helpers;

[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
public class RestrictDomainAttribute : Attribute, IAuthorizationFilter
{
    public RestrictDomainAttribute(params string[] allowedHosts)
    {
        AllowedHosts = allowedHosts;
    }

    public IEnumerable<string> AllowedHosts { get; }

    public void OnAuthorization(AuthorizationFilterContext context)
    {
        string host = context.HttpContext.Request.Host.Host;
        if (!AllowedHosts.Contains(host, StringComparer.OrdinalIgnoreCase))
            context.Result = new UnauthorizedResult();
    }
}