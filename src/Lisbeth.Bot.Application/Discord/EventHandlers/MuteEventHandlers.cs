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

using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.EventArgs;
using JetBrains.Annotations;
using Lisbeth.Bot.Application.Helpers;
using Lisbeth.Bot.Application.Services.Interfaces;
using MikyM.Discord.Events;

namespace Lisbeth.Bot.Application.Discord.EventHandlers
{
    [UsedImplicitly]
    public class MuteEventHandlers : IDiscordGuildMemberEventsSubscriber
    {
        private readonly IAsyncExecutor _asyncExecutor;

        public MuteEventHandlers(IAsyncExecutor asyncExecutor)
        {
            _asyncExecutor = asyncExecutor;
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
            _ = _asyncExecutor.ExecuteAsync<IMuteCheckService>(x =>
                x.CheckForNonBotMuteActionAsync(args.Member.Id, args.Guild.Id, sender.CurrentUser.Id, args.RolesBefore,
                    args.RolesAfter));
            return Task.CompletedTask;
        }

        public Task DiscordOnGuildMembersChunked(DiscordClient sender, GuildMembersChunkEventArgs args)
        {
            return Task.CompletedTask;
        }
    }
}