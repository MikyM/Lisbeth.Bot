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
using DSharpPlus.EventArgs;
using Lisbeth.Bot.Application.Discord.Commands.ChannelMessageFormat;
using Lisbeth.Bot.Domain.DTOs.Request.ChannelMessageFormat;
using MikyM.Common.Application.CommandHandlers;
using MikyM.Discord.Events;
using MikyM.Discord.Interfaces;

namespace Lisbeth.Bot.Application.Discord.EventHandlers;

[UsedImplicitly]
public class ChannelMessageFormatEventHandler : IDiscordMessageEventsSubscriber
{
    private readonly IDiscordService _discord;
    private readonly ICommandHandler<VerifyMessageFormatCommand, VerifyMessageFormatResDto> _commandHandler;

    public ChannelMessageFormatEventHandler(IDiscordService discord, ICommandHandler<VerifyMessageFormatCommand, VerifyMessageFormatResDto> commandHandler)
    {
        _discord = discord;
        _commandHandler = commandHandler;
    }

    public async Task DiscordOnMessageCreated(DiscordClient sender, MessageCreateEventArgs args)
    {
        if (args.Channel is null || args.Guild is null)
            return;

        await _commandHandler.HandleAsync(new VerifyMessageFormatCommand(
                new VerifyMessageFormatReqDto(args.Channel.Id, args.Message.Id, args.Guild.Id,
                    _discord.Client.CurrentUser.Id), args));
    }

    public Task DiscordOnMessageAcknowledged(DiscordClient sender, MessageAcknowledgeEventArgs args)
    {
        return Task.CompletedTask;
    }

    public Task DiscordOnMessageUpdated(DiscordClient sender, MessageUpdateEventArgs args)
    {
        return Task.CompletedTask;
    }

    public Task DiscordOnMessageDeleted(DiscordClient sender, MessageDeleteEventArgs args)
    {
        return Task.CompletedTask;
    }

    public Task DiscordOnMessagesBulkDeleted(DiscordClient sender, MessageBulkDeleteEventArgs args)
    {
        return Task.CompletedTask;
    }
}