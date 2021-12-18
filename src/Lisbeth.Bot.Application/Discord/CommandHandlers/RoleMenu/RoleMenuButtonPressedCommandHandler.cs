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
using Lisbeth.Bot.Application.Discord.Exceptions;
using Lisbeth.Bot.DataAccessLayer.Specifications.RoleMenu;
using MikyM.Common.Application.CommandHandlers;

namespace Lisbeth.Bot.Application.Discord.CommandHandlers.RoleMenu;

[UsedImplicitly]
public class RoleMenuButtonPressedCommandHandler : ICommandHandler<RoleMenuButtonPressedCommand>
{
    private readonly IRoleMenuDataService _roleMenuDataService;
    private readonly ICommandHandler<GetRoleMenuSelectCommand, DiscordSelectComponent> _getSelectHandler;

    public RoleMenuButtonPressedCommandHandler(IRoleMenuDataService roleMenuDataService,
        ICommandHandler<GetRoleMenuSelectCommand, DiscordSelectComponent> getSelectHandler)
    {
        _roleMenuDataService = roleMenuDataService;
        _getSelectHandler = getSelectHandler;
    }

    public async Task<Result> HandleAsync(RoleMenuButtonPressedCommand command)
    {
        if (command is null) throw new ArgumentNullException(nameof(command));
        if (!long.TryParse(command.Interaction.Id.Split('_', StringSplitOptions.RemoveEmptyEntries).Last().Trim(),
                out var parsedRoleMenuId)) return Result.FromError(new DiscordError());

        var res = await _roleMenuDataService.GetSingleBySpecAsync<Domain.Entities.RoleMenu>(
            new RoleMenuByIdAndGuildWithOptionsSpec(parsedRoleMenuId, command.Interaction.Guild.Id));

        if (!res.IsDefined(out var roleMenu)) return Result.FromError(new NotFoundError());

        var memberRoles = ((DiscordMember)command.Interaction.User).Roles;
        var builder = new DiscordFollowupMessageBuilder().AsEphemeral(true);
        var embed = new DiscordEmbedBuilder().WithColor(new DiscordColor(roleMenu.Guild?.EmbedHexColor));
        embed.WithAuthor("Lisbeth Role Menu");
        embed.WithDescription("Please select roles you'd like to get or deselect them to drop them!");

        var selectRes =
            await _getSelectHandler.HandleAsync(new GetRoleMenuSelectCommand(roleMenu,
                memberRoles));

        builder.AddEmbed(embed.Build())
            .AddComponents(selectRes.IsDefined(out var select) ? select : throw new DiscordNotFoundException());

        await command.Interaction.Interaction.CreateFollowupMessageAsync(builder);

        return Result.FromSuccess();
    }
}
