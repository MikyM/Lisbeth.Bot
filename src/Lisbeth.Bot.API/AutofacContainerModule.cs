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

using Autofac;
using IdGen;
using MikyM.Common.ApplicationLayer;
using MikyM.Common.DataAccessLayer;
using MikyM.Common.EfCore.ApplicationLayer;
using MikyM.Common.EfCore.DataAccessLayer;

namespace Lisbeth.Bot.API;

public class AutofacContainerModule : Module
{
    protected override void Load(ContainerBuilder builder)
    {
        base.Load(builder);
        // automapper

        builder.AddDataAccessLayer(options =>
        {
            options.AddEfCoreDataAccessLayer(efCoreOptions =>
            {
                efCoreOptions.EnableIncludeCache = true;
                efCoreOptions.AddInMemoryEvaluators();
                efCoreOptions.AddEvaluators();
                efCoreOptions.AddValidators();
            });
            options.AddSnowflakeIdGenerator(generatorOptions =>
            {
                generatorOptions.GeneratorId = 1;
                generatorOptions.IdStructure = new IdStructure(45, 2, 16);
                generatorOptions.DefaultTimeSource =
                    new DefaultTimeSource(new DateTime(2021, 1, 1, 0, 0, 0, DateTimeKind.Utc));
                generatorOptions.SequenceOverflowStrategy = SequenceOverflowStrategy.SpinWait;
            });
        }); 

        builder.AddApplicationLayer(options =>
        {
            options.AddAttributeDefinedServices();
            options.AddCommandHandlers();
            options.AddEfCoreDataServices();
            options.AddAsyncExecutor();
        });
    }
}