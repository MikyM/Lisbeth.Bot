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

using DSharpPlus.Entities;
using Lisbeth.Bot.Application.Discord.Commands.Mute;
using Lisbeth.Bot.DataAccessLayer.Specifications.Mute;
using Lisbeth.Bot.Domain.DTOs.Request.Mute;
using Microsoft.Extensions.Logging;
using MikyM.Common.Application.CommandHandlers;
using MikyM.Common.Utilities.Extensions;
using MikyM.Discord.Interfaces;

namespace Lisbeth.Bot.Application.Discord.CommandHandlers.Mute;

[UsedImplicitly]
public class RevokeExpiredMutesCommandHandler : ICommandHandler<RevokeExpiredMutesCommand>
{
    private readonly IMuteDataService _muteDataService;
    private readonly ILogger<RevokeExpiredMutesCommandHandler> _logger;
    private readonly IDiscordService _discord;
    private readonly ICommandHandler<RevokeMuteCommand, DiscordEmbed> _revokeHandler;

    public RevokeExpiredMutesCommandHandler(IMuteDataService muteDataService,
        ILogger<RevokeExpiredMutesCommandHandler> logger, IDiscordService discord,
        ICommandHandler<RevokeMuteCommand, DiscordEmbed> revokeHandler)
    {
        _muteDataService = muteDataService;
        _logger = logger;
        _discord = discord;
        _revokeHandler = revokeHandler;
    }

    public async Task<Result> HandleAsync(RevokeExpiredMutesCommand command)
    {
        try
        {
            var res = await _muteDataService.GetBySpecAsync(
                new ActiveExpiredMutesInActiveGuildsSpecifications());

            if (!res.IsDefined() || res.Entity.Count == 0) return Result.FromSuccess();

            await Parallel.ForEachAsync(res.Entity, async (x, _) =>
            {
                var req = new MuteRevokeReqDto(x.UserId, x.GuildId, _discord.Client.CurrentUser.Id);
                await _revokeHandler.HandleAsync(new RevokeMuteCommand(req));
            });
        }
        catch (Exception ex)
        {
            _logger.LogError($"Automatic unmute failed with: {ex.GetFullMessage()}");
            return Result.FromError(
                new InvalidOperationError($"Automatic unmute failed with: {ex.GetFullMessage()}"));
        }

        return Result.FromSuccess();
    }
}
