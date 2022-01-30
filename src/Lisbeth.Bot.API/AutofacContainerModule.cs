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
using IdGen;
using Lisbeth.Bot.Application.Services;
using Microsoft.AspNetCore.Http;
using MikyM.Common.Application;
using MikyM.Common.Application.CommandHandlers.Helpers;
using MikyM.Common.DataAccessLayer;
using Module = Autofac.Module;

namespace Lisbeth.Bot.API;

public class AutofacContainerModule : Module
{
    protected override void Load(ContainerBuilder builder)
    {
        base.Load(builder);
        // automapper

        builder.AddDataAccessLayer(options =>
        {
            options.EnableIncludeCache = true;
            options.AddInMemoryEvaluators();
            options.AddEvaluators();
            options.AddValidators();
        }); 

        builder.AddApplicationLayer(options =>
        {
            options.AddCommandHandlers();
            options.AddServices();
            options.AddAsyncExecutor();
        });
        
        // pagination stuff
        builder.RegisterType<HttpContextAccessor>().As<IHttpContextAccessor>().SingleInstance();
        builder.Register(x =>
            {
                var accessor = x.Resolve<IHttpContextAccessor>();
                var request = accessor.HttpContext?.Request;
                var uri = string.Concat(request?.Scheme, "://", request?.Host.ToUriComponent());
                return new UriService(uri);
            })
            .As<IUriService>()
            .SingleInstance();


        builder.Register(_ =>
            {
                var epoch = new DateTime(2021, 1, 1, 0, 0, 0, DateTimeKind.Utc);
                var structure = new IdStructure(45, 2, 16);
                var options = new IdGeneratorOptions(structure, new DefaultTimeSource(epoch),
                    SequenceOverflowStrategy.SpinWait);
                return new IdGenerator(0, options);
            })
            .AsSelf()
            .SingleInstance();

    }
}