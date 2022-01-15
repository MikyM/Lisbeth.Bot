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

using Autofac;
using MikyM.Autofac.Extensions;
using MikyM.Common.DataAccessLayer.Specifications.Validators;

namespace MikyM.Common.DataAccessLayer;

public class DataAccessOptions
{
    public DataAccessOptions(ContainerBuilder builder)
    {
        this._builder = builder;
    }

    private readonly ContainerBuilder _builder;

    public bool EnableIncludeCache { get; set; } = false;

    public DataAccessOptions AddEvaluator<TEvaluator>() where TEvaluator : class, IEvaluator
    {
        _builder.RegisterType(typeof(TEvaluator))
            .As<IEvaluator>()
            .FindConstructorsWith(new AllConstructorsFinder())
            .SingleInstance();

        return this;
    }

    public DataAccessOptions AddEvaluator(Type evaluator)
    {
        if (evaluator.GetInterface(nameof(IEvaluator)) is null)
            throw new NotSupportedException("Registered evaluator did not implement IEvaluator interface");

        _builder.RegisterType(evaluator)
            .As<IEvaluator>()
            .FindConstructorsWith(new AllConstructorsFinder())
            .SingleInstance();

        return this;
    }

    public DataAccessOptions AddInMemoryEvaluators()
    {
        foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
        {
            _builder.RegisterAssemblyTypes(assembly)
                .Where(x => x.GetInterface(nameof(IInMemoryEvaluator)) is not null)
                .As<IInMemoryEvaluator>()
                .FindConstructorsWith(new AllConstructorsFinder())
                .SingleInstance();
        }

        return this;
    }

    public DataAccessOptions AddValidators()
    {
        foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
        {
            _builder.RegisterAssemblyTypes(assembly)
                .Where(x => x.GetInterface(nameof(IValidator)) is not null)
                .As<IValidator>()
                .FindConstructorsWith(new AllConstructorsFinder())
                .SingleInstance();
        }

        return this;
    }

    public DataAccessOptions AddEvaluators()
    {
        foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
        {
            if (assembly == typeof(IncludeEvaluator).Assembly)
                continue;

            _builder.RegisterAssemblyTypes(assembly)
                .Where(x => x.GetInterface(nameof(IEvaluator)) is not null)
                .As<IEvaluator>()
                .FindConstructorsWith(new AllConstructorsFinder())
                .SingleInstance();
        }

        return this;
    }
}