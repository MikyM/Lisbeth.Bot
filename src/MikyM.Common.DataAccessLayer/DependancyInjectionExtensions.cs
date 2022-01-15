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
using MikyM.Common.DataAccessLayer.Specifications;
using System.Collections.Generic;
using System.Diagnostics;
using MikyM.Autofac.Extensions.Extensions;
using MikyM.Common.DataAccessLayer.Specifications.Validators;

namespace MikyM.Common.DataAccessLayer;

public static class DependancyInjectionExtensions
{
    public static void AddDataAccessLayer(this ContainerBuilder builder, Action<DataAccessOptions>? options = null)
    {
        var config = new DataAccessOptions(builder);
        options?.Invoke(config);

        var ctorFinder = new AllConstructorsFinder();

        builder.RegisterGeneric(typeof(ReadOnlyRepository<>))
            .As(typeof(IReadOnlyRepository<>))
            .FindConstructorsWith(ctorFinder)
            .InstancePerLifetimeScope();
        builder.RegisterGeneric(typeof(Repository<>))
            .As(typeof(IRepository<>))
            .FindConstructorsWith(ctorFinder)
            .InstancePerLifetimeScope();
        builder.RegisterGeneric(typeof(UnitOfWork<>)).As(typeof(IUnitOfWork<>)).InstancePerLifetimeScope();

        foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
        {
            builder.RegisterAssemblyTypes(assembly)
                .Where(x => x.GetInterface(nameof(IEvaluator)) is not null && x != typeof(IncludeEvaluator))
                .As<IEvaluator>()
                .FindConstructorsWith(ctorFinder)
                .SingleInstance();
        }

        builder.RegisterType<IncludeEvaluator>()
            .As<IEvaluator>()
            .UsingConstructor(typeof(bool))
            .FindConstructorsWith(ctorFinder)
            .WithParameter(new TypedParameter(typeof(bool), config.EnableIncludeCache))
            .SingleInstance();

        builder.RegisterType<ProjectionEvaluator>()
            .As<IProjectionEvaluator>()
            .FindConstructorsWith(ctorFinder)
            .SingleInstance();

        builder.RegisterType<SpecificationEvaluator>()
            .As<ISpecificationEvaluator>()
            .UsingConstructor(typeof(IEnumerable<IEvaluator>), typeof(IProjectionEvaluator))
            .FindConstructorsWith(ctorFinder)
            .SingleInstance();

        builder.RegisterType<SpecificationValidator>()
            .As<ISpecificationValidator>()
            .UsingConstructor(typeof(IEnumerable<IValidator>))
            .FindConstructorsWith(ctorFinder)
            .SingleInstance();

        builder.RegisterType<InMemorySpecificationEvaluator>()
            .As<IInMemorySpecificationEvaluator>()
            .UsingConstructor(typeof(IEnumerable<IInMemoryEvaluator>))
            .FindConstructorsWith(ctorFinder)
            .SingleInstance();
    }
}