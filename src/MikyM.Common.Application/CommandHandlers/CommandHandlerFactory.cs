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

using System.Collections.Concurrent;
using Autofac;
using MikyM.Common.Application.CommandHandlers.Commands;

namespace MikyM.Common.Application.CommandHandlers;

public interface ICommandHandlerFactory
{
    TCommandHandler GetHandler<TCommandHandler>() where TCommandHandler : class, ICommandHandler;
    ICommandHandler<TCommand, TResult> GetHandlerFor<TCommand, TResult>() where TCommand : class, ICommand<TResult>;
    ICommandHandler<TCommand> GetHandlerFor<TCommand>() where TCommand : class, ICommand;
}

public class CommandHandlerFactory : ICommandHandlerFactory
{
    private ConcurrentDictionary<string, ICommandHandler>? _commandHandlers;
    private readonly ILifetimeScope _lifetimeScope;

    public CommandHandlerFactory(ILifetimeScope lifetimeScope)
    {
        _lifetimeScope = lifetimeScope;
    }

    public TCommandHandler GetHandler<TCommandHandler>() where TCommandHandler : class, ICommandHandler
    {
        if (!typeof(TCommandHandler).IsInterface)
            throw new ArgumentException("Due to Autofac limitations you must use interfaces");

        _commandHandlers ??= new ConcurrentDictionary<string, ICommandHandler>();

        var type = typeof(TCommandHandler);
        string name = type.FullName ?? throw new InvalidOperationException();

        if (_commandHandlers.TryGetValue(name, out var handler)) 
            return (TCommandHandler) handler;

        var other = _commandHandlers.Values.FirstOrDefault(x => x.GetType().IsAssignableTo(type));
        if (other is not null)
            return (TCommandHandler) other;

        if (_commandHandlers.TryAdd(name, _lifetimeScope.Resolve<TCommandHandler>()))
            return (TCommandHandler)_commandHandlers[name];

        if (_commandHandlers.TryGetValue(name, out handler))
            return (TCommandHandler)handler;

        throw new InvalidOperationException($"Couldn't add nor retrieve handler of type {name}");
    }

    public ICommandHandler<TCommand> GetHandlerFor<TCommand>() where TCommand : class, ICommand
    {
        _commandHandlers ??= new ConcurrentDictionary<string, ICommandHandler>();

        var commandType = typeof(TCommand);
        string name = commandType.FullName ?? throw new InvalidOperationException();

        var generic = typeof(ICommandHandler<,>).MakeGenericType(commandType);
        string genericName = generic.FullName ?? throw new InvalidOperationException();

        if (_commandHandlers.TryGetValue(genericName, out var handler)) 
            return (ICommandHandler<TCommand>) handler;

        if (_commandHandlers.TryAdd(genericName, _lifetimeScope.Resolve<ICommandHandler<TCommand>>()))
            return (ICommandHandler<TCommand>)_commandHandlers[genericName];

        if (_commandHandlers.TryGetValue(name, out handler))
            return (ICommandHandler<TCommand>)handler;

        throw new InvalidOperationException($"Couldn't add nor retrieve handler for type {name}");
    }

    public ICommandHandler<TCommand ,TResult> GetHandlerFor<TCommand, TResult>() where TCommand : class, ICommand<TResult>
    {
        _commandHandlers ??= new ConcurrentDictionary<string, ICommandHandler>();

        var commandType = typeof(TCommand);
        var resultType = typeof(TResult);
        string name = commandType.FullName ?? throw new InvalidOperationException();

        var generic = typeof(ICommandHandler<,>).MakeGenericType(commandType, resultType);
        string genericName = generic.FullName ?? throw new InvalidOperationException();

        if (_commandHandlers.TryGetValue(genericName, out var handler)) 
            return (ICommandHandler<TCommand, TResult>) handler;

        if (_commandHandlers.TryAdd(genericName, _lifetimeScope.Resolve<ICommandHandler<TCommand, TResult>>()))
            return (ICommandHandler<TCommand, TResult>) _commandHandlers[genericName];

        if (_commandHandlers.TryGetValue(name, out handler))
            return (ICommandHandler<TCommand, TResult>)handler;

        throw new InvalidOperationException($"Couldn't add nor retrieve handler for type {name}");
    }
}