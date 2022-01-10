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

using DSharpPlus;
using DSharpPlus.Entities;
using Lisbeth.Bot.Application.Discord.Commands.Tag;
using Lisbeth.Bot.DataAccessLayer.Specifications.Guild;
using Lisbeth.Bot.DataAccessLayer.Specifications.Tag;
using Microsoft.Extensions.Logging;
using MikyM.Common.Application.CommandHandlers;
using MikyM.Discord.Extensions.BaseExtensions;
using MikyM.Discord.Interfaces;

namespace Lisbeth.Bot.Application.Discord.CommandHandlers.Tag;

[UsedImplicitly]
public class RevokeSnowflakePermissionTagCommandHandler : ICommandHandler<RevokeSnowflakePermissionTagCommand>
{
    private readonly IGuildDataService _guildDataService;
    private readonly ITagDataService _tagDataService;
    private readonly ILogger<RevokeSnowflakePermissionTagCommand> _logger;
    private readonly IDiscordService _discord;

    public RevokeSnowflakePermissionTagCommandHandler(IGuildDataService guildDataService,
        ILogger<RevokeSnowflakePermissionTagCommand> logger, IDiscordService discord,
        ITagDataService tagDataService)
    {
        _guildDataService = guildDataService;
        _logger = logger;
        _discord = discord;
        _tagDataService = tagDataService;
    }

    public async Task<Result> HandleAsync(RevokeSnowflakePermissionTagCommand command)
    {
        if (command is null) throw new ArgumentNullException(nameof(command));

        var guildRes =
            await _guildDataService.GetSingleBySpecAsync(
                new ActiveGuildByIdSpec(command.Dto.GuildId));

        if (!guildRes.IsDefined()) return Result.FromError(guildRes);

        var tagRes =
            await _tagDataService.GetSingleBySpecAsync(new ActiveTagByGuildAndNameSpec(command.Dto.Name,
                command.Dto.GuildId));

        if (!tagRes.IsDefined(out var tag)) return Result.FromError(tagRes);

        if (tag.IsDisabled)
            return new DisabledEntityError(
                $"Found tag is disabled.");

        // data req
        DiscordGuild guild = command.Ctx?.Guild ?? await _discord.Client.GetGuildAsync(command.Dto.GuildId);
        DiscordMember requestingMember = command.Ctx?.Member ?? await guild.GetMemberAsync(command.Dto.RequestedOnBehalfOfId);
        if (!requestingMember.IsModerator())
            return new DiscordNotAuthorizedError("Requesting member doesn't have moderator rights.");

        DiscordRole? targetRole = command.Ctx?.ResolvedRoleMentions?[0] ?? guild.GetRole(command.Dto.SnowflakeId!.Value);
        DiscordMember? targetMember = command.Ctx?.ResolvedUserMentions?[0] as DiscordMember ?? await guild.GetMemberAsync(command.Dto.SnowflakeId!.Value);

        if (targetMember is null && targetRole is null)
            return new DiscordNotFoundError("Didn't find any roles or members with given snowflake Id");

        Result result;
        if (targetRole is null && targetMember is not null)
        {
            result = await _tagDataService.RemoveAllowedUserAsync(command.Dto, true);
        }
        else if (targetRole is not null)
        {
            result = await _tagDataService.RemoveAllowedRoleAsync(command.Dto, true);
        }
        else
        {
            return new InvalidOperationError();
        }

        return result;
    }
}
