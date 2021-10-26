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
using JetBrains.Annotations;
using Lisbeth.Bot.Application.Helpers;
using Lisbeth.Bot.Application.Services.Interfaces;
using MikyM.Discord.Events;
using System.Threading.Tasks;

namespace Lisbeth.Bot.Application.Discord.EventHandlers
{
    [UsedImplicitly]
    public class BanEventsHandler : IDiscordGuildBanEventsSubscriber
    {
        private readonly IAsyncExecutor _asyncExecutor;

        public BanEventsHandler(IAsyncExecutor asyncExecutor)
        {
            _asyncExecutor = asyncExecutor;
        }

        public Task DiscordOnGuildBanAdded(DiscordClient sender, GuildBanAddEventArgs args)
        {
            _ = _asyncExecutor.ExecuteAsync<IBanCheckService>(x =>
                x.CheckForNonBotBanAsync(args.Member.Id, args.Guild.Id, sender.CurrentUser.Id));
            return Task.CompletedTask;
        }

        public Task DiscordOnGuildBanRemoved(DiscordClient sender, GuildBanRemoveEventArgs args)
        {
            _ = _asyncExecutor.ExecuteAsync<IBanCheckService>(x =>
                x.CheckForNonBotUnbanAsync(args.Member.Id, args.Guild.Id, sender.CurrentUser.Id));
            return Task.CompletedTask;
        }
    }
}
