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
using DSharpPlus.Entities;
using Lisbeth.Bot.Application.Discord.Commands.RoleMenu;
using Lisbeth.Bot.Application.Discord.Helpers;
using Lisbeth.Bot.DataAccessLayer.Specifications.Guild;
using Lisbeth.Bot.DataAccessLayer.Specifications.RoleMenu;
using Microsoft.Extensions.Logging;
using MikyM.CommandHandlers;
using MikyM.Common.Utilities.Results;
using MikyM.Discord.Enums;
using MikyM.Discord.Extensions.BaseExtensions;
using MikyM.Discord.Interfaces;

namespace Lisbeth.Bot.Application.Discord.CommandHandlers.RoleMenu;

[UsedImplicitly]
public class GetRoleMenuCommandHandler : ICommandHandler<GetRoleMenuCommand, DiscordMessageBuilder>
{
    private readonly IGuildDataService _guildDataService;
    private readonly IRoleMenuDataService _roleMenuDataService;
    private readonly ILogger<CreateRoleMenuCommandHandler> _logger;
    private readonly IDiscordService _discord;
    private readonly IDiscordEmbedProvider _discordEmbedProvider;

    public GetRoleMenuCommandHandler(IGuildDataService guildDataService, IRoleMenuDataService roleMenuDataService,
        ILogger<CreateRoleMenuCommandHandler> logger, IDiscordService discord,
        IDiscordEmbedProvider discordEmbedProvider)
    {
        _guildDataService = guildDataService;
        _roleMenuDataService = roleMenuDataService;
        _logger = logger;
        _discord = discord;
        _discordEmbedProvider = discordEmbedProvider;
    }

    public async Task<Result<DiscordMessageBuilder>> HandleAsync(GetRoleMenuCommand command)
    {
        if (command is null) throw new ArgumentNullException(nameof(command));

        Result<Domain.Entities.RoleMenu> partial;

        // data req
        DiscordGuild guild = command.Ctx?.Guild ?? await _discord.Client.GetGuildAsync(command.Dto.GuildId);
        DiscordMember requestingUser = command.Ctx?.User as DiscordMember ??
                                       await guild.GetMemberAsync(command.Dto.RequestedOnBehalfOfId);

        if (guild is null)
            return new DiscordNotFoundError(DiscordEntity.Guild);
        if (requestingUser is null)
            return new DiscordNotFoundError(DiscordEntity.Member);

        if (requestingUser.IsBotOwner(_discord.Client))
        {
            partial = await _roleMenuDataService.GetSingleBySpecAsync<Domain.Entities.RoleMenu>(
                new RoleMenuByNameWithOptionsSpec(command.Dto.Name!));
        }
        else
        {
            var guildResult =
                await _guildDataService.GetSingleBySpecAsync(
                    new ActiveGuildByIdSpec(guild.Id));
            if (!guildResult.IsDefined())
                return Result<DiscordMessageBuilder>.FromError(guildResult);

            if (requestingUser.Guild.Id != guild.Id)
                return new DiscordNotAuthorizedError();

            partial = await _roleMenuDataService.GetSingleBySpecAsync<Domain.Entities.RoleMenu>(
                new RoleMenuByNameAndGuildWithOptionsSpec(command.Dto.Name!, guild.Id));
        }

        if (!partial.IsDefined())
            return Result<DiscordMessageBuilder>.FromError(partial);

        if (partial.Entity.IsDisabled && !requestingUser.IsBotOwner(_discord.Client))
            return new DisabledEntityError(nameof(partial.Entity));

        var builder = new DiscordMessageBuilder();

        //builder.AddComponents(this.GetRoleMenuSelect(partial.Entity).Entity);
        var button = new DiscordButtonComponent(ButtonStyle.Primary, partial.Entity.CustomButtonId,
            "Lets manage my roles!"/*, false, new DiscordComponentEmoji(DiscordEmoji.FromName("::"))*/);

        builder.AddComponents(button);

        if (partial.Entity.EmbedConfig is null)
            return builder.WithContent(partial.Entity.Text + "\n\nClick on the button below to manage your roles!" ??
                                       throw new ArgumentNullException(nameof(partial.Entity.Text)));

        var embed = _discordEmbedProvider.GetEmbedFromConfig(partial.Entity.EmbedConfig)
            .WithFooter("Click on the button below to manage your roles!")
            .Build();

        return builder.AddEmbed(embed);
    }
}
