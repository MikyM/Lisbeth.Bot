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
using Lisbeth.Bot.Application.Discord.Commands.RoleMenu;
using MikyM.Common.Application.CommandHandlers;
using MikyM.Discord.Interfaces;
using System.Collections.Generic;
using MikyM.Common.Utilities.Results;

namespace Lisbeth.Bot.Application.Discord.CommandHandlers.RoleMenu;

[UsedImplicitly]
public class GetRoleMenuSelectCommandHandler : ICommandHandler<GetRoleMenuSelectCommand, DiscordSelectComponent>
{
    private readonly IDiscordService _discord;

    public GetRoleMenuSelectCommandHandler(IDiscordService discord)
    {
        _discord = discord;
    }

    public Task<Result<DiscordSelectComponent>> HandleAsync(GetRoleMenuSelectCommand command)
    {
        List<DiscordSelectComponentOption> options = new();

        foreach (var option in command.RoleMenu.RoleMenuOptions ?? throw new InvalidOperationException())
        {
            DiscordEmoji? emoji = null;

            if (!string.IsNullOrWhiteSpace(option.Emoji))
                DiscordEmoji.TryFromName(_discord.Client, option.Emoji, true, out emoji);
            if (!string.IsNullOrWhiteSpace(option.Emoji))
                DiscordEmoji.TryFromUnicode(_discord.Client, option.Emoji, out emoji);

            options.Add(new DiscordSelectComponentOption(option.Name, option.CustomSelectOptionValueId,
                option.Description, command.MemberRoles.Any(x => x.Id == option.RoleId), emoji is null ? null : new DiscordComponentEmoji(emoji)));
        }

        var select = new DiscordSelectComponent(command.RoleMenu.CustomSelectComponentId, "Choose a role!", options, false, 0,
            command.RoleMenu.RoleMenuOptions.Count);

        return Task.FromResult(Result<DiscordSelectComponent>.FromSuccess(select));
    }
}
