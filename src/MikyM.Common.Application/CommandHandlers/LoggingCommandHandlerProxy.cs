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

namespace MikyM.Common.Application.CommandHandlers;

[UsedImplicitly]
public class LoggingCommandHandlerProxy<TRequest> : ICommandHandler<TRequest> where TRequest : ICommand
{
    private readonly ILogger<LoggingCommandHandlerProxy<TRequest>> _logger;
    private readonly ICommandHandler<TRequest> _commandHandler;

    public LoggingCommandHandlerProxy(ILogger<LoggingCommandHandlerProxy<TRequest>> logger, ICommandHandler<TRequest> commandHandler)
    {
        _logger = logger;
        _commandHandler = commandHandler;
    }

    public async Task<Result> HandleAsync(TRequest request)
    {
        _logger.LogDebug("Processing request: {0}", request);
        return await _commandHandler.HandleAsync(request);
    }
}

[UsedImplicitly]
public class LoggingCommandHandler<TRequest, TResult> : ICommandHandler<TRequest, TResult> where TRequest : ICommand
{
    private readonly ILogger<LoggingCommandHandler<TRequest, TResult>> _logger;
    private readonly ICommandHandler<TRequest, TResult> _commandHandler;

    public LoggingCommandHandler(ILogger<LoggingCommandHandler<TRequest, TResult>> logger, ICommandHandler<TRequest, TResult> commandHandler)
    {
        _logger = logger;
        _commandHandler = commandHandler;
    }

    public async Task<Result<TResult>> HandleAsync(TRequest command)
    {
        _logger.LogDebug("Processing request: {0}", command);
        return await _commandHandler.HandleAsync(command);
    }
}
