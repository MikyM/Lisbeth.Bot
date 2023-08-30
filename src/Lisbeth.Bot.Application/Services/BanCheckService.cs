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

using Lisbeth.Bot.DataAccessLayer.Specifications.Ban;
using Lisbeth.Bot.Domain.DTOs.Request.Ban;

namespace Lisbeth.Bot.Application.Services;

[UsedImplicitly]
[ServiceImplementation<IBanCheckService>(ServiceLifetime.InstancePerLifetimeScope)]
public class BanCheckService : IBanCheckService
{
    private readonly IBanDataService _banDataService;

    public BanCheckService(IBanDataService banDataService)
    {
        _banDataService = banDataService;
    }

    public async Task CheckForNonBotBanAsync(ulong targetId, ulong guildId, ulong requestedOnBehalfOfId)
    {
        await Task.Delay(1500);

        var banRes = await _banDataService.GetSingleBySpecAsync(new NonBotBanSpec(targetId, guildId));

        if (banRes.IsDefined(out var ban) && !ban.IsDisabled)
            return;

        await _banDataService.AddOrExtendAsync(new BanApplyReqDto(targetId, guildId, requestedOnBehalfOfId,
            DateTime.MaxValue));
    }

    public async Task CheckForNonBotUnbanAsync(ulong targetId, ulong guildId, ulong requestedOnBehalfOfId)
    {
        await Task.Delay(1500);

        var banRes = await _banDataService.GetSingleBySpecAsync(new NonBotBanSpec(targetId, guildId));

        if (!banRes.IsDefined(out var ban) || ban.IsDisabled)
            return;

        await _banDataService.DisableAsync(new BanRevokeReqDto(targetId, guildId, requestedOnBehalfOfId));
    }
}
