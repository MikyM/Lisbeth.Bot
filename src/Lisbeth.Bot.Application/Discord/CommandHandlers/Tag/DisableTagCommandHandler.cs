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

using DSharpPlus.Entities;
using Lisbeth.Bot.Application.Discord.Commands.Tag;
using Lisbeth.Bot.DataAccessLayer.Specifications.Guild;
using MikyM.Common.Application.CommandHandlers;
using MikyM.Discord.Enums;
using MikyM.Discord.Extensions.BaseExtensions;
using MikyM.Discord.Interfaces;

namespace Lisbeth.Bot.Application.Discord.CommandHandlers.Tag;

[UsedImplicitly]
public class DisableTagCommandHandler : ICommandHandler<DisableTagCommand>
{
    private readonly IDiscordService _discord;
    private readonly IGuildService _guildService;
    private readonly ITagService _tagService;

    public DisableTagCommandHandler(IDiscordService discord, IGuildService guildService,
        ITagService tagService)
    {
        _discord = discord;
        _guildService = guildService;
        _tagService = tagService;
    }

    public async Task<Result> HandleAsync(DisableTagCommand command)
    {
        if (command is null) throw new ArgumentNullException(nameof(command));

        // data req
        DiscordGuild guild = command.Ctx?.Guild ?? await _discord.Client.GetGuildAsync(command.Dto.GuildId);
        DiscordMember requestingUser = command.Ctx?.User as DiscordMember ??
                                       await guild.GetMemberAsync(command.Dto.RequestedOnBehalfOfId);

        if (guild is null)
            return new DiscordNotFoundError(DiscordEntity.Guild);
        if (requestingUser is null)
            return new DiscordNotFoundError(DiscordEntity.User);

        var guildCfg =
            await _guildService.GetSingleBySpecAsync(
                new ActiveGuildByDiscordIdWithTagsSpecifications(guild.Id));
        if (!guildCfg.IsDefined())
            return Result.FromError(guildCfg);

        if (!requestingUser.IsModerator())
            return new DiscordNotAuthorizedError("You are not authorized to disable tags");

        return await _tagService.DisableAsync(command.Dto, true);
    }
}
