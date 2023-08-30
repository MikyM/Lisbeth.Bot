// This file is part of Lisbeth.Bot project
//
// Copyright (C) 2021-2022 Krzysztof Kupisz - MikyM
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

using AttributeBasedRegistration.Autofac;
using Autofac;
using DataExplorer;
using DataExplorer.EfCore;
using DataExplorer.EfCore.Extensions;
using IdGen;
using Lisbeth.Bot.DataAccessLayer;
using MikyM.Common.Utilities;
using ResultCommander.Autofac;

namespace Lisbeth.Bot.API;

public class AutofacContainerModule : Module
{
    protected override void Load(ContainerBuilder builder)
    {
        base.Load(builder);
        // automapper

        var serviceAssemblies = new[] { typeof(IBanDataService).Assembly };
        var entityAssemblies = new[] { typeof(Ban).Assembly };
        
        builder.AddAsyncExecutor();
        builder.AddDataExplorer(opt =>
        {
            opt.AddSnowflakeIdGeneration(1, () =>
            {
                var idStructure = new IdStructure(45, 2, 16);
                var defaultTimeSource =
                    new DefaultTimeSource(new DateTime(2021, 1, 1, 0, 0, 0, DateTimeKind.Utc));
                return new IdGeneratorOptions(idStructure, defaultTimeSource, SequenceOverflowStrategy.SpinWait);
            });

            opt.AddEfCore(serviceAssemblies, entityAssemblies, efOpt =>
            {
                efOpt.EnableIncludeCache = true;
                efOpt.AddDbContext<ILisbethBotDbContext, LisbethBotDbContext>();
                efOpt.DateTimeStrategy = DateTimeStrategy.UtcNow;
            });
        });

        builder.AddAttributeDefinedServices(serviceAssemblies);
        
        builder.AddResultCommander(serviceAssemblies);
    }
}
