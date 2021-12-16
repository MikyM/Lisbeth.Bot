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
using Lisbeth.Bot.Application.Discord.EventHandlers.Base;
using MikyM.Common.Utilities;
using MikyM.Discord.Events;

namespace Lisbeth.Bot.Application.Discord.EventHandlers;

[UsedImplicitly]
public class ModerationEventsHandler : BaseEventHandler, IDiscordMessageEventsSubscriber, IDiscordGuildMemberEventsSubscriber
{
    public ModerationEventsHandler(IAsyncExecutor asyncExecutor) : base(asyncExecutor)
    {
    }

    public Task DiscordOnGuildMemberAdded(DiscordClient sender, GuildMemberAddEventArgs args)
    {
        _ = AsyncExecutor.ExecuteAsync<IDiscordMemberService>(async x => await x.SendWelcomeMessageAsync(args));
        _ = AsyncExecutor.ExecuteAsync<IDiscordMemberService>(async x => await x.MemberMuteCheckAsync(args));
        return Task.CompletedTask;
    }

    public Task DiscordOnGuildMemberRemoved(DiscordClient sender, GuildMemberRemoveEventArgs args)
    {
        _ = AsyncExecutor.ExecuteAsync<IDiscordMemberService>(async x => await x.LogMemberRemovedEventAsync(args));
        return Task.CompletedTask;
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

    public Task DiscordOnMessageUpdated(DiscordClient sender, MessageUpdateEventArgs args)
    {
        _ = AsyncExecutor.ExecuteAsync<IDiscordMessageService>(
            async x => await x.LogMessageUpdatedEventAsync(args));
        return Task.CompletedTask;
    }

    public Task DiscordOnMessageDeleted(DiscordClient sender, MessageDeleteEventArgs args)
    {
        _ = AsyncExecutor.ExecuteAsync<IDiscordMessageService>(
            async x => await x.LogMessageDeletedEventAsync(args));
        return Task.CompletedTask;
    }

    public Task DiscordOnMessagesBulkDeleted(DiscordClient sender, MessageBulkDeleteEventArgs args)
    {
        _ = AsyncExecutor.ExecuteAsync<IDiscordMessageService>(async x =>
            await x.LogMessageBulkDeletedEventAsync(args));
        return Task.CompletedTask;
    }
}