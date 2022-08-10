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
using Lisbeth.Bot.Application.Discord.Commands.MemberHistoryEntry;
using Lisbeth.Bot.Application.Discord.Commands.ServerBoosterHistoryEntry;
using MikyM.Discord.Events;

namespace Lisbeth.Bot.Application.Discord.EventHandlers;

[UsedImplicitly]
public class MemberEventsHandler : IDiscordGuildMemberEventsSubscriber
{
    private readonly ICommandHandlerFactory _commandHandlerFactory;

    public MemberEventsHandler(ICommandHandlerFactory commandHandlerFactory)
    {
        _commandHandlerFactory = commandHandlerFactory;
    }

    public async Task DiscordOnGuildMemberAdded(DiscordClient sender, GuildMemberAddEventArgs args)
    {
        _ = await _commandHandlerFactory.GetHandler<ICommandHandler<AddMemberHistoryEntryCommand>>()
            .HandleAsync(new AddMemberHistoryEntryCommand(args.Guild, args.Member));
    }

    public async Task DiscordOnGuildMemberRemoved(DiscordClient sender, GuildMemberRemoveEventArgs args)
    {
        var guildRes = await _commandHandlerFactory.GetHandler<ICommandHandler<DisableMemberHistoryEntryCommand, Guild>>()
            .HandleAsync(new DisableMemberHistoryEntryCommand(args.Guild, args.Member));
        
        if (!args.Member.Roles.Any(x => x.Tags.IsPremiumSubscriber))
            return;
            
        _ = await _commandHandlerFactory.GetHandler<ICommandHandler<DisableServerBoosterHistoryEntryCommand>>()
            .HandleAsync(new DisableServerBoosterHistoryEntryCommand(args.Guild, args.Member, guildRes.Entity));
    }

    public async Task DiscordOnGuildMemberUpdated(DiscordClient sender, GuildMemberUpdateEventArgs args)
    {

        var hasBoostNow = args.RolesAfter.Any(x => x.Tags.IsPremiumSubscriber);
        var hadBoostBefore = args.RolesBefore.Any(x => x.Tags.IsPremiumSubscriber);

        _ = hadBoostBefore switch
        {
            true when !hasBoostNow => await _commandHandlerFactory
                .GetHandler<ICommandHandler<DisableServerBoosterHistoryEntryCommand>>()
                .HandleAsync(new DisableServerBoosterHistoryEntryCommand(args.Guild, args.Member)),
            false when hasBoostNow => await _commandHandlerFactory
                .GetHandler<ICommandHandler<AddServerBoosterHistoryEntryCommand>>()
                .HandleAsync(new AddServerBoosterHistoryEntryCommand(args.Guild, args.Member)),
            _ => Result.FromSuccess()
        };
    }

    public Task DiscordOnGuildMembersChunked(DiscordClient sender, GuildMembersChunkEventArgs args)
        => Task.CompletedTask;
}
