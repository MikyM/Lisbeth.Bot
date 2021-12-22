﻿// This file is part of Lisbeth.Bot project
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
using EFCoreSecondLevelCacheInterceptor;
using IdGen;
using Lisbeth.Bot.Application.Discord.CommandHandlers.Ticket;
using Lisbeth.Bot.Application.Discord.Helpers;
using Lisbeth.Bot.Application.Discord.Services;
using Lisbeth.Bot.Application.Services;
using Lisbeth.Bot.Application.Services.Database;
using Lisbeth.Bot.DataAccessLayer;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using MikyM.Common.Application;
using MikyM.Common.Application.CommandHandlers;
using MikyM.Common.DataAccessLayer;
using MikyM.Common.DataAccessLayer.Specifications;
using MikyM.Common.DataAccessLayer.Specifications.Evaluators;
using MikyM.Common.Utilities;

namespace Lisbeth.Bot.API;

public class AutofacContainerModule : Module
{
    protected override void Load(ContainerBuilder builder)
    {
        base.Load(builder);
        // automapper

        builder.AddDataAccessLayer();
        builder.AddApplicationLayer();

        // bulk register custom services - follow naming convention
        builder.RegisterAssemblyTypes(typeof(MuteDataService).Assembly)
            .Where(t => t.Name.EndsWith("Service"))
            .AsImplementedInterfaces()
            .InstancePerLifetimeScope();
        builder.RegisterAssemblyTypes(typeof(CloseTicketCommandHandler).Assembly)
            .AsClosedTypesOf(typeof(ICommandHandler<>))
            .InstancePerLifetimeScope();
        builder.RegisterAssemblyTypes(typeof(CloseTicketCommandHandler).Assembly)
            .AsClosedTypesOf(typeof(ICommandHandler<,>))
            .InstancePerLifetimeScope();

        // pagination stuff
        builder.RegisterType<HttpContextAccessor>().As<IHttpContextAccessor>().SingleInstance();
        builder.Register(x =>
        {
            var accessor = x.Resolve<IHttpContextAccessor>();
            var request = accessor?.HttpContext?.Request;
            var uri = string.Concat(request?.Scheme, "://", request?.Host.ToUriComponent());
            return new UriService(uri);
        }).As<IUriService>().SingleInstance();

        builder.RegisterType<AsyncExecutor>().As<IAsyncExecutor>().SingleInstance();
        builder.RegisterType<TicketQueueService>().As<ITicketQueueService>().SingleInstance();

        // Register Entity Framework
        builder.Register(x =>
        {
            var optionsBuilder = new DbContextOptionsBuilder<LisbethBotDbContext>();
            //optionsBuilder.UseInMemoryDatabase("testdb");
            optionsBuilder.AddInterceptors(x.Resolve<SecondLevelCacheInterceptor>());
            //optionsBuilder.EnableSensitiveDataLogging();
            //optionsBuilder.UseLoggerFactory(x.Resolve<ILoggerFactory>());
            optionsBuilder.UseNpgsql(x.Resolve<IConfiguration>().GetConnectionString("MainDb"));
            return new LisbethBotDbContext(optionsBuilder.Options);
        }).AsSelf().InstancePerLifetimeScope();

        builder.Register(_ =>
        {
            var epoch = new DateTime(2021, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            var structure = new IdStructure(45, 2, 16);
            var options = new IdGeneratorOptions(structure, new DefaultTimeSource(epoch),
                SequenceOverflowStrategy.SpinWait);
            return new IdGenerator(0, options);
        }).AsSelf().SingleInstance();

        builder.RegisterType<DiscordEmbedProvider>().As<IDiscordEmbedProvider>().SingleInstance();
        builder.RegisterType<SpecificationEvaluator>().As<ISpecificationEvaluator>().UsingConstructor()
            .SingleInstance();
        builder.RegisterGeneric(typeof(DiscordEmbedConfiguratorService<>))
            .As(typeof(IDiscordEmbedConfiguratorService<>)).InstancePerLifetimeScope();
    }
}