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
using Lisbeth.Bot.Application.Discord.Commands.Mute;
using Lisbeth.Bot.Application.Discord.Commands.Timeout;
using Lisbeth.Bot.Application.Discord.EventHandlers.Base;
using MikyM.Common.Application.CommandHandlers;
using MikyM.Common.Utilities;
using MikyM.Discord.Events;

namespace Lisbeth.Bot.Application.Discord.EventHandlers;

[UsedImplicitly]
public class MuteEventHandlers : BaseEventHandler, IDiscordGuildMemberEventsSubscriber
{
    public MuteEventHandlers(IAsyncExecutor asyncExecutor) : base(asyncExecutor)
    {
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
        _ = AsyncExecutor.ExecuteAsync<ICommandHandler<CheckNonBotMuteActionCommand>>(x =>
            x.HandleAsync(new CheckNonBotMuteActionCommand(args.Member, args.RolesBefore, args.RolesAfter)));

        if (!args.CommunicationDisabledUntilAfter.HasValue && !args.CommunicationDisabledUntilBefore.HasValue)
            return Task.CompletedTask;

        _ = AsyncExecutor.ExecuteAsync<ICommandHandler<LogTimeoutCommand>>(x =>
            x.HandleAsync(new LogTimeoutCommand(args.Member, args.CommunicationDisabledUntilBefore,
                args.CommunicationDisabledUntilAfter)));

        return Task.CompletedTask;
    }

    public Task DiscordOnGuildMembersChunked(DiscordClient sender, GuildMembersChunkEventArgs args)
    {
        return Task.CompletedTask;
    }
}