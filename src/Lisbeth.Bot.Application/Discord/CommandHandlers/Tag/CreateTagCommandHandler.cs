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

using DSharpPlus.Entities;
using Lisbeth.Bot.Application.Discord.Commands.Tag;
using Lisbeth.Bot.DataAccessLayer.Specifications.Guild;
using Lisbeth.Bot.DataAccessLayer.Specifications.Tag;
using MikyM.CommandHandlers;
using MikyM.Common.Utilities.Results;
using MikyM.Discord.Enums;
using MikyM.Discord.Extensions.BaseExtensions;
using MikyM.Discord.Interfaces;

namespace Lisbeth.Bot.Application.Discord.CommandHandlers.Tag;

[UsedImplicitly]
public class CreateTagCommandHandler : ICommandHandler<CreateTagCommand>
{
    private readonly IDiscordService _discord;
    private readonly IGuildDataService _guildDataService;
    private readonly ITagDataService _tagDataService;

    public CreateTagCommandHandler(IDiscordService discord, IGuildDataService guildDataService,
        ITagDataService tagDataService)
    {
        _discord = discord;
        _guildDataService = guildDataService;
        _tagDataService = tagDataService;
    }

    public async Task<Result> HandleAsync(CreateTagCommand command)
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
            await _guildDataService.GetSingleBySpecAsync(
                new ActiveGuildByDiscordIdWithTagsSpecifications(command.Dto.GuildId));
        if (!guildCfg.IsDefined())
            return Result.FromError(guildCfg);

        if (!requestingUser.IsModerator())
            return new DiscordNotAuthorizedError();

        var check = await _tagDataService.LongCountAsync(new ActiveTagByGuildAndNameSpec(command.Dto.Name ?? string.Empty, command.Dto.GuildId));

        if (check.IsDefined(out var count) && count > 0) return new SameEntityNamePerGuildError("Tag", command.Dto.Name!);

        command.Dto.Name = command.Dto.Name?.ToLower();
        var res = await _tagDataService.AddAsync(command.Dto, true);

        return res;
    }
}
