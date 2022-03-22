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
using DSharpPlus.Entities;
using Lisbeth.Bot.Application.Discord.Commands.RoleMenu;
using Lisbeth.Bot.Application.Discord.Extensions;
using Lisbeth.Bot.DataAccessLayer.Specifications.RoleMenu;
using Microsoft.Extensions.Logging;
using MikyM.Common.Application.CommandHandlers;
using MikyM.Common.Utilities.Extensions;
using System.Collections.Generic;
using Lisbeth.Bot.Application.Discord.Exceptions;
using MikyM.Common.Utilities.Results;
using MikyM.Common.Utilities.Results.Errors;

namespace Lisbeth.Bot.Application.Discord.CommandHandlers.RoleMenu;

[UsedImplicitly]
public class RoleMenuOptionSelectedCommandHandler  : ICommandHandler<RoleMenuOptionSelectedCommand>
{
    private readonly IRoleMenuDataService _roleMenuDataService;
    private readonly ILogger<RoleMenuOptionSelectedCommandHandler> _logger;
    private readonly ICommandHandler<GetRoleMenuSelectCommand, DiscordSelectComponent> _getSelectHandler;

    public RoleMenuOptionSelectedCommandHandler(IRoleMenuDataService roleMenuDataService,
        ILogger<RoleMenuOptionSelectedCommandHandler> logger,
        ICommandHandler<GetRoleMenuSelectCommand, DiscordSelectComponent> getSelectHandler)
    {
        _roleMenuDataService = roleMenuDataService;
        _logger = logger;
        _getSelectHandler = getSelectHandler;
    }

    public async Task<Result> HandleAsync(RoleMenuOptionSelectedCommand command)
    {
        if (command is null) throw new ArgumentNullException(nameof(command));

        if (!long.TryParse(command.Interaction.Id.Split('_', StringSplitOptions.RemoveEmptyEntries).Last().Trim(),
                        out var parsedRoleMenuId)) return Result.FromError(new DiscordError());

        var res = await _roleMenuDataService.GetSingleBySpecAsync<Domain.Entities.RoleMenu>(
            new RoleMenuByIdAndGuildWithOptionsSpec(parsedRoleMenuId, command.Interaction.Guild.Id));

        if (!res.IsDefined(out var roleMenu)) return Result.FromError(new NotFoundError());

        if (!command.Interaction.Guild.HasSelfPermissions(Permissions.ManageRoles)) return new DiscordError(
            "Bot doesn't have Manage Roles permission");

        try
        {
            var member = (DiscordMember)command.Interaction.User;
            var selectedMenuIds = command.Interaction.Values
                .Select(x => ulong.Parse(x.Split('_', StringSplitOptions.RemoveEmptyEntries).Last().Trim()))
                .ToList();
            var userRoleIds = member.Roles.Select(r => r.Id).ToList();

            var grantedRoles = new List<string?>();
            var revokedRoles = new List<string?>();
            var roleLists = new List<List<string?>> { grantedRoles, revokedRoles };
            var memberRoles = member.Roles.ToList();

            foreach (var option in roleMenu.RoleMenuOptions ?? throw new InvalidOperationException("Role menu options were null"))
            {
                if (!command.Interaction.Guild.RoleExists(option.RoleId, out var role)) continue;
                if (!command.Interaction.Guild.IsRoleHierarchyValid(role)) continue;

                if (selectedMenuIds.Contains(option.RoleId) && !userRoleIds.Contains(role.Id))
                {
                    await member.GrantRoleAsync(role);
                    grantedRoles.Add(role.Name);
                    memberRoles.Add(role);
                }
                else if (!selectedMenuIds.Contains(option.RoleId) && userRoleIds.Contains(role.Id))
                {
                    await member.RevokeRoleAsync(role);
                    revokedRoles.Add(role.Name);
                    memberRoles.RemoveAll(x => x.Id == role.Id);
                }
            }

            var embed = new DiscordEmbedBuilder().WithAuthor("Lisbeth Role Menu")
                .WithFooter($"Member Id: {member.Id}")
                .WithColor(new DiscordColor(roleMenu.Guild?.EmbedHexColor));

            for (int i = 0; i < roleLists.Count; i++)
            {
                if (roleLists[i].Count == 0) continue;

                string joined = "";
                for (int j = 0; j < roleLists[i].Count; j++)
                {
                    var toAdd = (j is 0 ? "" : "\n") + roleLists[i][j];

                    if (joined.Length + toAdd.Length <= 256)
                    {
                        joined += toAdd;
                        continue;
                    }

                    if (256 - joined.Length >= 5) joined += "\n...";
                    break;
                }

                string fieldName = i switch
                {
                    0 => "Granted roles",
                    1 => "Revoked roles",
                    2 => "Available roles",
                    _ => "Default"
                };
                embed.AddField(fieldName, joined);
            }

            var selectRes =
                await _getSelectHandler.HandleAsync(new GetRoleMenuSelectCommand(roleMenu,
                    memberRoles));

            await command.Interaction.Interaction.EditFollowupMessageAsync(command.Interaction.Message.Id,
                new DiscordWebhookBuilder()
                    .AddComponents(selectRes.IsDefined(out var select) ? select : throw new DiscordNotFoundException())
                    .AddEmbed(embed.Build()));
        }
        catch (Exception ex)
        {
            _logger.LogError($"Failed to grant/revoke role: {ex.GetFullMessage()}");
        }

        return Result.FromSuccess();
    }
}
