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

using System.Diagnostics;
using System.Dynamic;
using System.Text.Json;
using Castle.DynamicProxy;
using Microsoft.Extensions.Logging;

namespace MikyM.Autofac.Extensions;

/// <summary>
/// Base logging interceptor
/// </summary>
public class LoggingInterceptor : AsyncInterceptorBase
{
    private readonly ILogger _logger;

    public LoggingInterceptor(ILogger logger)
    {
        _logger = logger;
    }

    protected override async Task InterceptAsync(IInvocation invocation, IInvocationProceedInfo proceedInfo,
        Func<IInvocation, IInvocationProceedInfo, Task> proceed)
    {
        var sw = new Stopwatch();
        var args = new List<string>();

        foreach (var arg in invocation.Arguments.Select((x, i) => new { i, x }))
        {
            string str = string.Empty;
            dynamic obj = new ExpandoObject();
            obj.Index = arg.i;
            obj.ArgumentType = arg.x.GetType().Name;

            try
            {
                str = JsonSerializer.Serialize(arg);
            }
            catch
            {
                obj.Value = "Couldnt serialize";
                args.Add(JsonSerializer.Serialize(obj));
            }

            obj.Value = str;
            args.Add(JsonSerializer.Serialize(obj));
        }

        args.Insert(0, "{[");
        args.Add("]}");
        var serializedArgs = string.Join(" ", args);

        _logger.LogDebug($"Calling {invocation.Method.DeclaringType?.Name} {invocation.Method.Name} with args: {serializedArgs}");

        try
        {
            sw.Start();
            await proceed(invocation, proceedInfo).ConfigureAwait(false);
            sw.Stop();
        }
        catch (Exception)
        {
            sw.Stop();
            _logger.LogDebug($"Execution of {invocation.Method.DeclaringType?.Name} {invocation.Method.Name} with args: {serializedArgs} errored after {sw.Elapsed.TotalMilliseconds} ms");
            throw;
        }

        _logger.LogDebug($"Finished executing {invocation.Method.DeclaringType?.Name} {invocation.Method.Name} with args: {serializedArgs} after {sw.Elapsed.TotalMilliseconds} ms");
    }

    protected override async Task<TResult> InterceptAsync<TResult>(IInvocation invocation,
        IInvocationProceedInfo proceedInfo, Func<IInvocation, IInvocationProceedInfo, Task<TResult>> proceed)
    {
        var sw = new Stopwatch();
        var args = new List<string>();

        foreach (var arg in invocation.Arguments.Select((x, i) => new { i, x }))
        {
            string str = string.Empty;
            dynamic obj = new ExpandoObject();
            obj.Index = arg.i;
            obj.ArgumentType = arg.x.GetType().Name;

            try
            {
                str = JsonSerializer.Serialize(arg);
            }
            catch
            {
                obj.Value = "Couldnt serialize";
                args.Add(JsonSerializer.Serialize(obj));
            }

            obj.Value = str;
            args.Add(JsonSerializer.Serialize(obj));
        }

        args.Insert(0, "{[");
        args.Add("]}");
        var serializedArgs = string.Join(" ", args);

        _logger.LogDebug($"Calling {invocation.Method.DeclaringType?.Name} {invocation.Method.Name} with args: {serializedArgs}");

        TResult? result;

        try
        {
            sw.Start();
            result = await proceed(invocation, proceedInfo).ConfigureAwait(false);
            sw.Stop();
        }
        catch (Exception)
        {
            sw.Stop();
            _logger.LogDebug($"Execution of {invocation.Method.DeclaringType?.Name} {invocation.Method.Name} with args: {serializedArgs} errored after {sw.Elapsed.TotalMilliseconds} ms");
            throw;
        }

        _logger.LogDebug($"Finished executing {invocation.Method.DeclaringType?.Name} {invocation.Method.Name} with args: {serializedArgs} after {sw.Elapsed.TotalMilliseconds} ms");

        return result;
    }
}