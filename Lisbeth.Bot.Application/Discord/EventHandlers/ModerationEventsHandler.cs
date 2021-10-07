// This file is part of Lisbeth.Bot project
//
// Copyright (C) 2021 MikyM
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
using JetBrains.Annotations;
using MikyM.Discord.Events;
using System.Threading.Tasks;

namespace Lisbeth.Bot.Application.Discord.EventHandlers
{
    [UsedImplicitly]
    public class ModerationEventsHandler : IDiscordMessageEventsSubscriber, IDiscordGuildMemberEventsSubscriber
    {
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

        public Task DiscordOnGuildMemberAdded(DiscordClient sender, GuildMemberAddEventArgs args)
        {
            return Task.CompletedTask;
        }

        public Task DiscordOnGuildMemberRemoved(DiscordClient sender, GuildMemberRemoveEventArgs args)
        {
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
    }
}
