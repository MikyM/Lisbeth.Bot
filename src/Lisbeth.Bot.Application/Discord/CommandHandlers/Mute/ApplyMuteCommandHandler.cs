﻿// This file is part of Lisbeth.Bot project
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
using Lisbeth.Bot.Application.Discord.EmbedEnrichers.Response.Infractions;
using Lisbeth.Bot.Application.Discord.Extensions;
using Lisbeth.Bot.DataAccessLayer.Specifications.Guild;
using Microsoft.Extensions.Logging;
using MikyM.Discord.EmbedBuilders.Enums;
using MikyM.Discord.Extensions.BaseExtensions;

namespace Lisbeth.Bot.Application.Discord.CommandHandlers.Mute;

[UsedImplicitly]
public class ApplyMuteCommandHandler : IAsyncCommandHandler<ApplyMuteCommand, DiscordEmbed>
{
    private readonly IDiscordService _discord;
    private readonly IGuildDataService _guildDataService;
    private readonly ILogger<ApplyMuteCommandHandler> _logger;
    private readonly IMuteDataService _muteDataService;
    private readonly IDiscordGuildLoggerService _guildLogger;
    private readonly IResponseDiscordEmbedBuilder<DiscordModeration> _embedBuilder;

    public ApplyMuteCommandHandler(IDiscordService discord, IGuildDataService guildDataService,
        ILogger<ApplyMuteCommandHandler> logger, IMuteDataService muteDataService,
        IDiscordGuildLoggerService guildLogger, IResponseDiscordEmbedBuilder<DiscordModeration> embedBuilder)
    {
        _discord = discord;
        _guildDataService = guildDataService;
        _logger = logger;
        _muteDataService = muteDataService;
        _guildLogger = guildLogger;
        _embedBuilder = embedBuilder;
    }

    public async Task<Result<DiscordEmbed>> HandleAsync(ApplyMuteCommand command, CancellationToken cancellationToken = default)
    {
            if (command is null) throw new ArgumentNullException(nameof(command));

            // data req
            var guild = command.Ctx?.Guild ??
                        command.MenuCtx?.Guild ?? await _discord.Client.GetGuildAsync(command.Dto.GuildId);
            var requestingUser = command.Ctx?.User as DiscordMember ?? command.MenuCtx?.User as DiscordMember ??
                await guild.GetMemberAsync(command.Dto.RequestedOnBehalfOfId);
            var target = command.Ctx?.ResolvedUserMentions[0] as DiscordMember ?? command.MenuCtx?.TargetMember ??
                command.MenuCtx?.TargetMessage.Author as DiscordMember ??
                await guild.GetMemberAsync(command.Dto.TargetUserId);

        if (command.Dto.AppliedUntil < DateTime.UtcNow)
            return new ArgumentOutOfRangeError(nameof(command.Dto.AppliedUntil));

        if (!requestingUser.IsModerator() || target.IsModerator()) return new DiscordNotAuthorizedError();

        if (!guild.HasSelfPermissions(Permissions.ManageRoles))
            return new DiscordError("Bot doesn't have manage roles permission.");

        var result =
            await _guildDataService.GetSingleBySpecAsync(new ActiveGuildByDiscordIdWithModerationSpec(guild.Id));

        if (!result.IsDefined(out var guildEntity)) return new DiscordNotFoundError(DiscordEntity.Guild);

        if (!guildEntity.IsModerationModuleEnabled) return new DisabledGuildModuleError(GuildModule.Moderation);

        if (!guild.RoleExists(guildEntity.ModerationConfig.MuteRoleId, out var mutedRole))
            return new DiscordNotFoundError("Mute role not found.");

        if (!guild.IsRoleHierarchyValid(mutedRole))
            return new DiscordError("Bots role is below muted role in the role hierarchy.");
        
        await _guildLogger.LogToDiscordAsync(guild, command.Dto, DiscordModeration.Mute, requestingUser, target, guildEntity.EmbedHexColor);

        var resMute = await target.MuteAsync(guildEntity.ModerationConfig.MuteRoleId);
        if (!resMute.IsSuccess) return new DiscordError("Failed to mute.");

        var partial = await _muteDataService.AddOrExtendAsync(command.Dto, true);
        if (!partial.IsDefined(out var idEntityPair)) return Result<DiscordEmbed>.FromError(partial);

        return _embedBuilder
            .WithType(DiscordModeration.Mute)
            .EnrichFrom(new MemberModAddReqResponseEnricher(command.Dto, target, idEntityPair.FoundEntity))
            .WithCase(idEntityPair.Id)
            .WithEmbedColor(new DiscordColor(guildEntity.EmbedHexColor))
            .WithAuthorSnowflakeInfo(target)
            .WithFooterSnowflakeInfo(target)
            .Build();
    }
}
