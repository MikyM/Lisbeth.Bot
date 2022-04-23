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
using MikyM.Discord.Events;

namespace Lisbeth.Bot.Application.Discord.EventHandlers;

[UsedImplicitly]
public class ModerationEventsHandler : IDiscordMessageEventsSubscriber, IDiscordGuildMemberEventsSubscriber
{
    private readonly IDiscordMemberService _discordMemberService;
    private readonly IDiscordMessageService _discordMessageService;

    public ModerationEventsHandler(IDiscordMemberService discordMemberService,
        IDiscordMessageService discordMessageService)
    {
        _discordMemberService = discordMemberService;
        _discordMessageService = discordMessageService;
    }

    public async Task DiscordOnGuildMemberAdded(DiscordClient sender, GuildMemberAddEventArgs args)
    {
        await _discordMemberService.SendWelcomeMessageAsync(args);
    }

    public async Task DiscordOnGuildMemberRemoved(DiscordClient sender, GuildMemberRemoveEventArgs args)
    {
        await _discordMemberService.LogMemberRemovedEventAsync(args);
    }

    public Task DiscordOnGuildMemberUpdated(DiscordClient sender, GuildMemberUpdateEventArgs args)
    {
        return Task.CompletedTask;
    }

    public Task DiscordOnGuildMembersChunked(DiscordClient sender, GuildMembersChunkEventArgs args)
    {
        return Task.CompletedTask;
    }

    public Task DiscordOnMessageCreated(DiscordClient sender, MessageCreateEventArgs args)
    {
        return Task.CompletedTask;
    }

    public Task DiscordOnMessageAcknowledged(DiscordClient sender, MessageAcknowledgeEventArgs args)
    {
        return Task.CompletedTask;
    }

    public async Task DiscordOnMessageUpdated(DiscordClient sender, MessageUpdateEventArgs args)
    {
        await _discordMessageService.LogMessageUpdatedEventAsync(args);
    }

    public async Task DiscordOnMessageDeleted(DiscordClient sender, MessageDeleteEventArgs args)
    {
        await _discordMessageService.LogMessageDeletedEventAsync(args);
    }

    public async Task DiscordOnMessagesBulkDeleted(DiscordClient sender, MessageBulkDeleteEventArgs args)
    {
        await _discordMessageService.LogMessageBulkDeletedEventAsync(args);
    }
}