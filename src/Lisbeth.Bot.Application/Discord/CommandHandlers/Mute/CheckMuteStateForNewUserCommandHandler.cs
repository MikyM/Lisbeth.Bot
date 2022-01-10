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

using Lisbeth.Bot.Application.Discord.Commands.Mute;
using Lisbeth.Bot.Application.Discord.Extensions;
using Lisbeth.Bot.DataAccessLayer.Specifications.Mute;
using MikyM.Common.Application.CommandHandlers;

namespace Lisbeth.Bot.Application.Discord.CommandHandlers.Mute;

[UsedImplicitly]
public class CheckMuteStateForNewUserCommandHandler : ICommandHandler<CheckMuteStateForNewUserCommand>
{
    private readonly IMuteDataService _muteDataService;

    public CheckMuteStateForNewUserCommandHandler(IMuteDataService muteDataService)
    {
        _muteDataService = muteDataService;
    }

    public async Task<Result> HandleAsync(CheckMuteStateForNewUserCommand command)
    {
        if (command is null) throw new ArgumentNullException(nameof(command));

        var res = await _muteDataService.GetSingleBySpecAsync<Domain.Entities.Mute>(
            new ActiveMutesByGuildAndUserSpecifications(command.Member.Guild.Id, command.Member.Id));

        if (!res.IsDefined() || res.Entity.Guild?.ModerationConfig is null)
            return Result.FromSuccess(); // no mod config enabled so we don't care

        await command.Member.MuteAsync(res.Entity.Guild.ModerationConfig.MuteRoleId);

        return Result.FromSuccess();
    }
}
