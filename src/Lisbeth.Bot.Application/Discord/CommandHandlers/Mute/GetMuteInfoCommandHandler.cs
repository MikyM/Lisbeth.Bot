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
using Lisbeth.Bot.Application.Discord.Commands.Mute;
using Lisbeth.Bot.Application.Discord.EmbedBuilders;
using Lisbeth.Bot.Application.Discord.EmbedEnrichers.Response.Infractions;
using Lisbeth.Bot.DataAccessLayer.Specifications.Guild;
using Lisbeth.Bot.DataAccessLayer.Specifications.Mute;
using Microsoft.Extensions.Logging;
using MikyM.Common.Application.CommandHandlers;
using MikyM.Discord.EmbedBuilders.Enums;
using MikyM.Discord.Enums;
using MikyM.Discord.Extensions.BaseExtensions;
using MikyM.Discord.Interfaces;

namespace Lisbeth.Bot.Application.Discord.CommandHandlers.Mute;

[UsedImplicitly]
public class GetMuteInfoCommandHandler : ICommandHandler<GetMuteInfoCommand, DiscordEmbed>
{
    private readonly IDiscordService _discord;
    private readonly IGuildDataDataService _guildDataDataService;
    private readonly ILogger<GetMuteInfoCommandHandler> _logger;
    private readonly IMuteDataService _muteDataService;
    private readonly IDiscordGuildLoggerService _guildLogger;
    private readonly IResponseDiscordEmbedBuilder<DiscordModeration> _embedBuilder;

    public GetMuteInfoCommandHandler(IDiscordService discord, IGuildDataDataService guildDataDataService,
        ILogger<GetMuteInfoCommandHandler> logger, IMuteDataService muteDataService,
        IDiscordGuildLoggerService guildLogger, IResponseDiscordEmbedBuilder<DiscordModeration> embedBuilder)
    {
        _discord = discord;
        _guildDataDataService = guildDataDataService;
        _logger = logger;
        _muteDataService = muteDataService;
        _guildLogger = guildLogger;
        _embedBuilder = embedBuilder;
    }

    public async Task<Result<DiscordEmbed>> HandleAsync(GetMuteInfoCommand command)
    {
        if (command is null) throw new ArgumentNullException(nameof(command));

        // data req
        DiscordGuild guild = command.Ctx?.Guild ??
                             command.MenuCtx?.Guild ?? await _discord.Client.GetGuildAsync(command.Dto.GuildId);
        DiscordMember requestingUser = command.Ctx?.User as DiscordMember ?? command.MenuCtx?.User as DiscordMember ??
            await guild.GetMemberAsync(command.Dto.RequestedOnBehalfOfId);

        if (!requestingUser.IsModerator())
            return new DiscordNotAuthorizedError();

        var result =
            await _guildDataDataService.GetSingleBySpecAsync(new ActiveGuildByDiscordIdWithModerationSpec(guild.Id));

        if (!result.IsDefined(out var guildEntity))
            return new DiscordNotFoundError(DiscordEntity.Guild);

        if (!guildEntity.IsModerationModuleEnabled)
            return new DisabledGuildModuleError(GuildModule.Moderation);

        var res = await _muteDataService.GetSingleBySpecAsync(new MuteBaseGetSpecifications(command.Dto.Id,
            command.Dto.TargetUserId, command.Dto.GuildId, command.Dto.AppliedById, command.Dto.LiftedOn,
            command.Dto.AppliedOn, command.Dto.LiftedById));

        if (!res.IsDefined(out var foundMute)) return new NotFoundError();

        DiscordMember target = command.Ctx?.ResolvedUserMentions[0] as DiscordMember ?? command.MenuCtx?.TargetMember ??
            command.MenuCtx?.TargetMessage.Author as DiscordMember ??
            await guild.GetMemberAsync(foundMute.UserId);

        await _guildLogger.LogToDiscordAsync(guild, command.Dto, DiscordModeration.MuteGet, requestingUser, target, guildEntity.EmbedHexColor);

        return _embedBuilder
            .WithType(DiscordModeration.MuteGet)
            .EnrichFrom(new MemberModGetReqResponseEnricher(foundMute))
            .WithCase(foundMute.Id)
            .WithEmbedColor(new DiscordColor(guildEntity.EmbedHexColor))
            .WithAuthorSnowflakeInfo(target)
            .WithFooterSnowflakeInfo(target)
            .Build();
    }
}
