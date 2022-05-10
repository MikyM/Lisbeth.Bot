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

using Lisbeth.Bot.Application.Discord.Commands.Mute;
using Lisbeth.Bot.DataAccessLayer.Specifications.Guild;
using Lisbeth.Bot.Domain.DTOs.Request.Mute;
using MikyM.CommandHandlers;
using MikyM.Common.Utilities.Results;
using MikyM.Common.Utilities.Results.Errors;
using MikyM.Discord.Interfaces;

namespace Lisbeth.Bot.Application.Discord.CommandHandlers.Mute;

[UsedImplicitly]
public class CheckNonBotMuteActionCommandHandler : ICommandHandler<CheckNonBotMuteActionCommand>
{
    private readonly IMuteDataService _muteDataService;
    private readonly IGuildDataService _guildDataService;
    private readonly IDiscordService _discord;

    public CheckNonBotMuteActionCommandHandler(IMuteDataService muteDataService, IGuildDataService guildDataService,
        IDiscordService discord)
    {
        _muteDataService = muteDataService;
        _guildDataService = guildDataService;
        _discord = discord;
    }

    public async Task<Result> HandleAsync(CheckNonBotMuteActionCommand command)
    {
        if (command is null) throw new ArgumentNullException(nameof(command));

        await Task.Delay(1000);

        var result = await _guildDataService.GetSingleBySpecAsync(
            new ActiveGuildByDiscordIdWithModerationSpec(command.Member.Guild.Id));

        if (!result.IsDefined() || result.Entity.ModerationConfig is null)
            return Result.FromError(new NotFoundError());

        bool wasMuted = command.RolesBefore.Any(x => x.Id == result.Entity.ModerationConfig.MuteRoleId);
        bool isMuted = command.RolesAfter.Any(x => x.Id == result.Entity.ModerationConfig.MuteRoleId);

        switch (wasMuted)
        {
            case true when !isMuted:
                await _muteDataService.DisableAsync(new MuteRevokeReqDto(command.Member.Id, command.Member.Guild.Id,
                    _discord.Client.CurrentUser.Id));
                break;
            case false when isMuted:
                await _muteDataService.AddOrExtendAsync(new MuteApplyReqDto(command.Member.Id, command.Member.Guild.Id,
                    _discord.Client.CurrentUser.Id, DateTime.MaxValue));
                break;
        }

        return Result.FromSuccess();
    }
}
