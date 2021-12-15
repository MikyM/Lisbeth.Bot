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

using Microsoft.Extensions.Logging;
using MikyM.Common.Application.Results;

namespace MikyM.Common.Application.HandlerServices;

[UsedImplicitly]
public class LoggingHandlerProxy<TRequest> : IHandlerService<TRequest> where TRequest : IHandlerRequest
{
    private readonly ILogger<LoggingHandlerProxy<TRequest>> _logger;
    private readonly IHandlerService<TRequest> _handlerService;

    public LoggingHandlerProxy(ILogger<LoggingHandlerProxy<TRequest>> logger, IHandlerService<TRequest> handlerService)
    {
        _logger = logger;
        _handlerService = handlerService;
    }

    public async Task<Result> HandleAsync(TRequest request)
    {
        _logger.LogDebug("Processing request: {0}", request);
        return await _handlerService.HandleAsync(request);
    }
}

[UsedImplicitly]
public class LoggingHandler<TRequest, TResult> : IHandlerService<TRequest, TResult> where TRequest : IHandlerRequest
{
    private readonly ILogger<LoggingHandler<TRequest, TResult>> _logger;
    private readonly IHandlerService<TRequest, TResult> _handlerService;

    public LoggingHandler(ILogger<LoggingHandler<TRequest, TResult>> logger, IHandlerService<TRequest, TResult> handlerService)
    {
        _logger = logger;
        _handlerService = handlerService;
    }

    public async Task<Result<TResult>> HandleAsync(TRequest request)
    {
        _logger.LogDebug("Processing request: {0}", request);
        return await _handlerService.HandleAsync(request);
    }
}
