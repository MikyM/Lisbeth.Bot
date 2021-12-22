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
using Lisbeth.Bot.Application.Discord.Commands.Mute;
using Lisbeth.Bot.Application.Discord.EmbedBuilders;
using Lisbeth.Bot.Application.Discord.EmbedEnrichers.Response.Infractions;
using Lisbeth.Bot.Application.Discord.Extensions;
using Lisbeth.Bot.DataAccessLayer.Specifications.Guild;
using Microsoft.Extensions.Logging;
using MikyM.Common.Application.CommandHandlers;
using MikyM.Discord.EmbedBuilders.Enums;
using MikyM.Discord.Enums;
using MikyM.Discord.Extensions.BaseExtensions;
using MikyM.Discord.Interfaces;

namespace Lisbeth.Bot.Application.Discord.CommandHandlers.Mute;

[UsedImplicitly]
public class RevokeMuteCommandHandler : ICommandHandler<RevokeMuteCommand, DiscordEmbed>
{
    private readonly IDiscordService _discord;
    private readonly IGuildDataService _guildDataService;
    private readonly ILogger<RevokeMuteCommandHandler> _logger;
    private readonly IMuteDataService _muteDataService;
    private readonly IDiscordGuildLoggerService _guildLogger;
    private readonly IResponseDiscordEmbedBuilder _embedBuilder;

    public RevokeMuteCommandHandler(IDiscordService discord, IGuildDataService guildDataService,
        ILogger<RevokeMuteCommandHandler> logger, IMuteDataService muteDataService,
        IDiscordGuildLoggerService guildLogger, IResponseDiscordEmbedBuilder embedBuilder)
    {
        _discord = discord;
        _guildDataService = guildDataService;
        _logger = logger;
        _muteDataService = muteDataService;
        _guildLogger = guildLogger;
        _embedBuilder = embedBuilder;
    }

    public async Task<Result<DiscordEmbed>> HandleAsync(RevokeMuteCommand command)
    {
        if (command is null) throw new ArgumentNullException(nameof(command));

        // data req
        DiscordGuild guild = command.Ctx?.Guild ??
                             command.MenuCtx?.Guild ?? await _discord.Client.GetGuildAsync(command.Dto.GuildId);
        DiscordMember requestingUser = command.Ctx?.User as DiscordMember ?? command.MenuCtx?.User as DiscordMember ??
            await guild.GetMemberAsync(command.Dto.RequestedOnBehalfOfId);
        DiscordMember target = command.Ctx?.ResolvedUserMentions[0] as DiscordMember ?? command.MenuCtx?.TargetMember ??
            command.MenuCtx?.TargetMessage.Author as DiscordMember ??
            await guild.GetMemberAsync(command.Dto.TargetUserId);

        if (!requestingUser.IsModerator())
            return new DiscordNotAuthorizedError();

        var result =
            await _guildDataService.GetSingleBySpecAsync<Guild>(new ActiveGuildByDiscordIdWithModerationSpec(guild.Id));

        if (!result.IsDefined(out var guildEntity))
            return new DiscordNotFoundError(DiscordEntity.Guild);

        if (!guildEntity.IsModerationModuleEnabled)
            return new DisabledGuildModuleError(GuildModule.Moderation);

        if (!guild.RoleExists(guildEntity.ModerationConfig.MuteRoleId, out var mutedRole)) return new DiscordNotFoundError("Mute role not found.");
        if (!guild.IsRoleHierarchyValid(mutedRole)) return new DiscordError("Bots role is below muted role in the role hierarchy.");
        if (!guild.HasSelfPermissions(Permissions.ManageRoles)) return new DiscordError("Bot doesn't have manage roles permission.");

        bool isMuted = target.Roles.Any(r => r.Id == guildEntity.ModerationConfig.MuteRoleId);

        await _guildLogger.LogToDiscordAsync(guild, command.Dto, DiscordModeration.Unmute, requestingUser, target, guildEntity.EmbedHexColor);

        if (isMuted)
        {
            var muteRes = await target.UnmuteAsync(guildEntity.ModerationConfig.MuteRoleId);
            if (!muteRes.IsSuccess) return new DiscordError("Failed to unmute");
        }

        var res = await _muteDataService.DisableAsync(command.Dto, true);

        if (!res.IsDefined(out var foundMute)) return Result<DiscordEmbed>.FromError(res);

        return _embedBuilder
            .WithType(DiscordModeration.Unmute)
            .EnrichFrom(new MemberModDisableReqResponseEnricher(command.Dto, target))
            .WithCase(foundMute.Id)
            .WithEmbedColor(new DiscordColor(guildEntity.EmbedHexColor))
            .WithAuthorSnowflakeInfo(target)
            .WithFooterSnowflakeInfo(target)
            .Build();
    }
}
