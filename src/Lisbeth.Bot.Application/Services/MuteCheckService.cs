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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus.Entities;
using JetBrains.Annotations;
using Lisbeth.Bot.Application.Services.Interfaces;
using Lisbeth.Bot.Application.Services.Interfaces.Database;
using Lisbeth.Bot.DataAccessLayer.Specifications.GuildSpecifications;
using Lisbeth.Bot.Domain.DTOs.Request;
using Lisbeth.Bot.Domain.Entities;

namespace Lisbeth.Bot.Application.Services
{
    [UsedImplicitly]
    public class MuteCheckService : IMuteCheckService
    {
        private readonly IMuteService _muteService;
        private readonly IGuildService _guildService;

        public MuteCheckService(IMuteService muteService, IGuildService guildService)
        {
            _muteService = muteService;
            _guildService = guildService;
        }

        public async Task CheckForNonBotMuteActionAsync(ulong targetId, ulong guildId, ulong requestedOnBehalfOfId, IReadOnlyList<DiscordRole> rolesBefore, IReadOnlyList<DiscordRole> rolesAfter)
        {
            await Task.Delay(1000);

            var res = await _guildService.GetBySpecAsync<Guild>(
                new ActiveGuildByDiscordIdWithModerationSpecifications(guildId));
            var guild = res.FirstOrDefault();

            if (guild  is null) return;

            bool wasMuted = rolesBefore.Any(x => x.Id == guild.ModerationConfig.MuteRoleId);
            bool isMuted = rolesAfter.Any(x => x.Id == guild.ModerationConfig.MuteRoleId);

            switch (wasMuted)
            {
                case true when !isMuted:
                    await _muteService.DisableAsync(new MuteDisableReqDto(targetId, guildId, requestedOnBehalfOfId));
                    break;
                case false when isMuted:
                    await _muteService.AddOrExtendAsync(new MuteReqDto(targetId, guildId, requestedOnBehalfOfId, DateTime.MaxValue));
                    break;
            }
        }
    }
}
