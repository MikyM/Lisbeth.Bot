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

using Lisbeth.Bot.Application.Discord.Commands.Ticket;
using Lisbeth.Bot.DataAccessLayer.Specifications.Guild;
using MikyM.Discord.Extensions.BaseExtensions;

namespace Lisbeth.Bot.Application.Discord.CommandHandlers.Ticket;

[UsedImplicitly]
public class DeleteTicketCommandHandler : IAsyncCommandHandler<DeleteTicketCommand>
{
    private readonly IGuildDataService _guildDataService;
    private readonly IDiscordGuildRequestDataProvider _requestDataProvider;

    public DeleteTicketCommandHandler(IGuildDataService guildDataService, IDiscordGuildRequestDataProvider requestDataProvider)
    {
        _guildDataService = guildDataService;
        _requestDataProvider = requestDataProvider;
    }

    public async Task<Result> HandleAsync(DeleteTicketCommand command, CancellationToken cancellationToken = default)
    {
        var guildRes = await _guildDataService.GetSingleBySpecAsync(new ActiveGuildByIdSpec(command.Interaction.Guild.Id), cancellationToken);

        if (!guildRes.IsDefined(out var guild))
            return Result.FromError(guildRes);
        
        // data req
        var initRes = await _requestDataProvider.InitializeAsync(command.Dto, command.Interaction);
        if (!initRes.IsSuccess)
            return initRes;
        
        var requestingMember = _requestDataProvider.RequestingMember;
        
        if (!requestingMember.IsModerator())
            return new DiscordNotAuthorizedError();

        await command.Interaction.CreateFollowupMessageAsync(
            new DiscordFollowupMessageBuilder().AddEmbed(
                new DiscordEmbedBuilder().WithColor(new DiscordColor(guild.EmbedHexColor)).WithDescription("This ticket will be deleted in 5 seconds")));

        await Task.Delay(millisecondsDelay: 5000, cancellationToken: cancellationToken);

        await command.Interaction.Channel.DeleteAsync();

        return Result.FromSuccess();
    }
}
