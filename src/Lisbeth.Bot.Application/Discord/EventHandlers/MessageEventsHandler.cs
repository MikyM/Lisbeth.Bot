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

using DSharpPlus;
using DSharpPlus.EventArgs;
using Lisbeth.Bot.Application.Discord.Commands.ChannelMessageFormat;
using Lisbeth.Bot.Application.Discord.Commands.Modules.Suggestions;
using Lisbeth.Bot.Domain.DTOs.Request.ChannelMessageFormat;
using MikyM.Discord.Events;

namespace Lisbeth.Bot.Application.Discord.EventHandlers;

[UsedImplicitly]
public class MessageEventsHandler : IDiscordMessageEventsSubscriber
{
    private readonly IDiscordService _discord;
    private readonly ICommandHandler<VerifyMessageFormatCommand, VerifyMessageFormatResDto> _verifyCommandHandler;
    private readonly ICommandHandler<HandlePossibleSuggestionCommand> _suggestionCommandHandler;

    public MessageEventsHandler(IDiscordService discord, ICommandHandler<VerifyMessageFormatCommand, VerifyMessageFormatResDto> verifyCommandHandler, ICommandHandler<HandlePossibleSuggestionCommand> suggestionCommandHandler)
    {
        _discord = discord;
        _verifyCommandHandler = verifyCommandHandler;
        _suggestionCommandHandler = suggestionCommandHandler;
    }

    public async Task DiscordOnMessageCreated(DiscordClient sender, MessageCreateEventArgs args)
    {
        if (args.Channel is null || args.Guild is null)
            return;

        var result = await _verifyCommandHandler.HandleAsync(new VerifyMessageFormatCommand(
                new VerifyMessageFormatReqDto(args.Channel.Id, args.Message.Id, args.Guild.Id,
                    _discord.Client.CurrentUser.Id), args));

        if (result.IsDefined(out var verifyResult) && verifyResult.IsDeleted.HasValue && verifyResult.IsDeleted.Value)
            return;
        
        await _suggestionCommandHandler.HandleAsync(new HandlePossibleSuggestionCommand(args));
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
