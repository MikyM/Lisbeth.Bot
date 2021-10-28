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

using Lisbeth.Bot.Application.Services.Interfaces;
using Lisbeth.Bot.Application.Services.Interfaces.Database;
using Lisbeth.Bot.DataAccessLayer.Specifications.MuteSpecifications;
using Lisbeth.Bot.Domain.DTOs.Request;
using Lisbeth.Bot.Domain.Entities;
using System;
using System.Linq;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Lisbeth.Bot.DataAccessLayer.Specifications.BanSpecifications;

namespace Lisbeth.Bot.Application.Services
{
    [UsedImplicitly]
    public class BanCheckService : IBanCheckService
    {
        private readonly IBanService _banService;

        public BanCheckService(IBanService banService)
        {
            _banService = banService;
        }

        public async Task CheckForNonBotBanAsync(ulong targetId, ulong guildId, ulong requestedOnBehalfOfId)
        {
            await Task.Delay(1000);

            var res = await _banService.GetBySpecAsync<Ban>(
                new BanBaseGetSpecifications(null, targetId, guildId));

            var ban = res.FirstOrDefault();

            if (ban is not null) return;

            await _banService.AddOrExtendAsync(new BanReqDto(targetId, guildId, requestedOnBehalfOfId,
                DateTime.MaxValue));
        }

        public async Task CheckForNonBotUnbanAsync(ulong targetId, ulong guildId, ulong requestedOnBehalfOfId)
        {
            await Task.Delay(1000);

            var res = await _banService.GetBySpecAsync<Ban>(
                new BanBaseGetSpecifications(null, targetId, guildId));

            var ban = res.FirstOrDefault();

            if (ban  is null) return;

            await _banService.DisableAsync(new BanDisableReqDto(targetId, guildId, requestedOnBehalfOfId));
        }
    }
}
