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

using AutoMapper;
using DSharpPlus.Entities;
using Lisbeth.Bot.Application.Discord.Commands.RoleMenu;
using Lisbeth.Bot.Domain.DTOs.Request.RoleMenu;
using Microsoft.Extensions.Logging;
using MikyM.CommandHandlers;
using MikyM.Common.Utilities.Results;
using MikyM.Discord.Enums;
using MikyM.Discord.Interfaces;

namespace Lisbeth.Bot.Application.Discord.CommandHandlers.RoleMenu;

[UsedImplicitly]
public class SendRoleMenuCommandHandler : ICommandHandler<SendRoleMenuCommand>
{
    private readonly ILogger<SendRoleMenuCommandHandler> _logger;
    private readonly IDiscordService _discord;
    private readonly ICommandHandler<GetRoleMenuCommand, DiscordMessageBuilder>
        _getRoleMenuHandler;
    private readonly IMapper _mapper;

    public SendRoleMenuCommandHandler(ILogger<SendRoleMenuCommandHandler> logger, IDiscordService discord,
        ICommandHandler<GetRoleMenuCommand, DiscordMessageBuilder> getRoleMenuHandler, IMapper mapper)
    {
        _logger = logger;
        _discord = discord;
        _getRoleMenuHandler = getRoleMenuHandler;
        _mapper = mapper;
    }

    public async Task<Result> HandleAsync(SendRoleMenuCommand command)
    {
        if (command is null) throw new ArgumentNullException(nameof(command));

        //data req
        DiscordGuild guild = command.Ctx?.Guild ?? await _discord.Client.GetGuildAsync(command.Dto.GuildId);
        DiscordChannel target =
            command.Ctx?.ResolvedChannelMentions?[0] ?? guild.GetChannel(command.Dto.ChannelId);
        DiscordMember requestingUser = command.Ctx?.User as DiscordMember ??
                                       await guild.GetMemberAsync(command.Dto.RequestedOnBehalfOfId);

        if (guild is null)
            return new DiscordNotFoundError(DiscordEntity.Guild);
        if (target is null)
            return new DiscordNotFoundError(DiscordEntity.Channel);
        if (requestingUser is null)
            return new DiscordNotFoundError(DiscordEntity.Member);

        var partial =
            await _getRoleMenuHandler.HandleAsync(new GetRoleMenuCommand(_mapper.Map<RoleMenuGetReqDto>(command.Dto),
                command.Ctx));

        if (!partial.IsDefined(out var result)) return Result.FromError(partial);

        await target.SendMessageAsync(result);

        return Result.FromSuccess();
    }
}
