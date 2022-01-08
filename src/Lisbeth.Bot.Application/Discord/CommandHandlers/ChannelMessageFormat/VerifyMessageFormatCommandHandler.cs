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
using Lisbeth.Bot.Application.Discord.Commands.ChannelMessageFormat;
using Lisbeth.Bot.DataAccessLayer.Specifications.Guild;
using MikyM.Common.Application.CommandHandlers;
using MikyM.Discord.Enums;
using MikyM.Discord.Extensions.BaseExtensions;
using MikyM.Discord.Interfaces;

namespace Lisbeth.Bot.Application.Discord.CommandHandlers.ChannelMessageFormat;

[UsedImplicitly]
public class VerifyMessageFormatCommandHandler : ICommandHandler<VerifyMessageFormatCommand, VerifyMessageFormatResDto>
{
    private readonly IDiscordService _discord;
    private readonly IGuildDataService _guildDataService;

    public VerifyMessageFormatCommandHandler(IDiscordService discord, IGuildDataService guildDataService)
    {
        _discord = discord;
        _guildDataService = guildDataService;
    }

    public async Task<Result<VerifyMessageFormatResDto>> HandleAsync(VerifyMessageFormatCommand command)
    {
        if (command is null) throw new ArgumentNullException(nameof(command));

        // data req
        DiscordMember? requestingUser;
        DiscordChannel? channel;
        DiscordMessage? target;

        try
        {
            var guild = command.EventArgs?.Guild ?? await _discord.Client.GetGuildAsync(command.Dto.GuildId);
            requestingUser = await guild.GetMemberAsync(command.Dto.RequestedOnBehalfOfId);
            channel = command.EventArgs?.Channel ?? guild.GetChannel(command.Dto.ChannelId);
            target = command.EventArgs?.Message ?? await channel.GetMessageAsync(command.Dto.MessageId);
        }
        catch (Exception ex)
        {
            return ex;
        }

        if (requestingUser is null)
            return new DiscordNotFoundError(DiscordEntity.Member);
        if (channel is null)
            return new DiscordNotFoundError(DiscordEntity.Channel);
        if (target is null)
            return new DiscordNotFoundError(DiscordEntity.Message);

        if (!requestingUser.IsModerator())
            return new DiscordNotAuthorizedError();

        var guildRes =
            await _guildDataService.GetSingleBySpecAsync(
                new ActiveGuildByDiscordIdWithChannelMessageFormatsSpec(command.Dto.GuildId));

        if (!guildRes.IsDefined(out var guildCfg))
            return Result<VerifyMessageFormatResDto>.FromError(guildRes);

        var format = guildCfg.ChannelMessageFormats?.FirstOrDefault(x => x.ChannelId == command.Dto.ChannelId);
        if (format is null)
            return new ArgumentError(nameof(command.Dto.ChannelId),
                "There's no message format registered for this channel");

        var isCompliant = format.IsTextCompliant(target.Content ?? string.Empty);

        if (isCompliant)
            return new VerifyMessageFormatResDto(true);

        bool isDeleted = false;
        try
        {
            await channel.DeleteMessageAsync(target, "Message not compliant with channel message format");
            isDeleted = true;
        }
        catch
        {
            // ignored
        }

        return new VerifyMessageFormatResDto(false, isDeleted);
    }
}