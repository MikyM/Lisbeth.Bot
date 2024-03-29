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

using Lisbeth.Bot.Application.Discord.Helpers;
using Lisbeth.Bot.DataAccessLayer.Specifications.Guild;
using MikyM.Discord.Extensions.BaseExtensions;

namespace Lisbeth.Bot.Application.Discord.CommandHandlers.DirectMessage;

[UsedImplicitly]
public class SendDirectMessageCommandHandler : IAsyncCommandHandler<SendDirectMessageCommand>
{
    private readonly IDiscordService _discord;
    private readonly IGuildDataService _guildDataService;
    private readonly IDiscordEmbedProvider _embedProvider;

    public SendDirectMessageCommandHandler(IDiscordService discord, IGuildDataService guildDataService,
        IDiscordEmbedProvider embedProvider)
    {
        _discord = discord;
        _guildDataService = guildDataService;
        _embedProvider = embedProvider;
    }

    public async Task<Result> HandleAsync(SendDirectMessageCommand command, CancellationToken cancellationToken = default)
    {
        if (command is null) throw new ArgumentNullException(nameof(command));

        // data req
        DiscordMember? requestingUser;
        DiscordMember? recipientUser;

        try
        {
            var guild = command.Guild ?? await _discord.Client.GetGuildAsync(command.Dto.GuildId);

            if (guild is null) return new DiscordNotFoundError(DiscordEntity.Guild);

            requestingUser = command.RequestedOnBehalfOf ??
                             await guild.GetMemberAsync(command.Dto.RequestedOnBehalfOfId);
            recipientUser = command.RecipientUser ?? await guild.GetMemberAsync(command.Dto.RecipientUserId);
        }
        catch (Exception ex)
        {
            return ex;
        }

        if (requestingUser is null)
            return new DiscordNotFoundError(DiscordEntity.Member);
        if (recipientUser is null)
            return new DiscordNotFoundError(DiscordEntity.Member);

        if (!requestingUser.IsAdmin())
            return new DiscordNotAuthorizedError();

        var guildRes =
            await _guildDataService.GetSingleBySpecAsync(
                new ActiveGuildByIdSpec(command.Dto.GuildId));

        if (!guildRes.IsDefined(out var guildCfg))
            return Result.FromError(guildRes);

        try
        {
            if (command.Dto.EmbedConfig is not null)
                await recipientUser.SendMessageAsync(_embedProvider.GetEmbedFromConfig(command.Dto.EmbedConfig));
            else
                await recipientUser.SendMessageAsync(command.Dto.Content);
        }
        catch (Exception ex)
        {
            return ex;
        }

        return Result.FromSuccess();
    }
}
