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

using System.IO;
using System.Net;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using MikyM.Common.Utilities.Extensions;

namespace Lisbeth.Bot.API.ExceptionMiddleware;

public class CustomExceptionMiddleware
{
    private readonly ILogger<CustomExceptionMiddleware> _logger;
    private readonly RequestDelegate _next;

    public CustomExceptionMiddleware(RequestDelegate next, ILogger<CustomExceptionMiddleware> logger)
    {
        _logger = logger;
        _next = next;
    }

    public async Task InvokeAsync(HttpContext httpContext)
    {
        try
        {
            httpContext.Request.EnableBuffering();
            await _next(httpContext);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(httpContext, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        _logger.LogError(exception,
            $"Headers: {string.Join(" ", context.Request.Headers)}\nRoute values: {string.Join(" ", context.Request.RouteValues)}\nQuery params: {string.Join(" ", context.Request.Query)}\nBody: {await GetRequestBody(context)} , Exception details: {exception.ToFormattedString()}");

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int) HttpStatusCode.InternalServerError;

        await context.Response.WriteAsync(new ErrorDetails
        {
            StatusCode = context.Response.StatusCode, Message = "Internal Server Error"
        }.ToString());
    }

    private async Task<string> GetRequestBody(HttpContext context)
    {
        if (context.Request.Body.Length == 0) return "";

        context.Request.Body.Seek(0, SeekOrigin.Begin);
        return await new StreamReader(context.Request.Body).ReadToEndAsync();
    }
}
