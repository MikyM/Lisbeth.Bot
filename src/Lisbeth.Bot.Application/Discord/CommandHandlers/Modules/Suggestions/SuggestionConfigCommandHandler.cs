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

using DSharpPlus.Entities;
using Lisbeth.Bot.Application.Discord.Commands.Modules.Suggestions;
using Lisbeth.Bot.Application.Discord.Helpers;
using Lisbeth.Bot.DataAccessLayer.Specifications.Guild;
using MikyM.CommandHandlers;
using MikyM.Common.Utilities.Results;
using MikyM.Common.Utilities.Results.Errors.Bases;
using MikyM.Discord.Enums;
using MikyM.Discord.Extensions.BaseExtensions;

namespace Lisbeth.Bot.Application.Discord.CommandHandlers.Modules.Suggestions;

[UsedImplicitly]
public class SuggestionConfigCommandHandler : ICommandHandler<SuggestionConfigCommand, DiscordEmbed>
{
    private readonly IGuildDataService _guildDataService;
    private readonly IDiscordGuildRequestDataProvider _dataProvider;

    public SuggestionConfigCommandHandler(IGuildDataService guildDataService, IDiscordGuildRequestDataProvider dataProvider)
    {
        _guildDataService = guildDataService;
        _dataProvider = dataProvider;
    }

    public async Task<Result<DiscordEmbed>> HandleAsync(SuggestionConfigCommand command)
    {
        await _dataProvider.InitializeAsync(command.RequestDto);
        
        var requestingMemberRes = await _dataProvider.GetMemberAsync(command.RequestDto.RequestedOnBehalfOfId);
        if (!requestingMemberRes.IsDefined(out var requestingMember))
            return Result<DiscordEmbed>.FromError(requestingMemberRes);

        if (!requestingMember.IsAdmin())
            return new DiscordNotAuthorizedError();

        var guildRes =
            await _guildDataService.GetSingleBySpecAsync(
                new ActiveGuildByDiscordIdWithSuggestionConfigSpec(command.RequestDto.GuildId));

        if (!guildRes.IsDefined(out var guild))
            return Result<DiscordEmbed>.FromError(guildRes);

        if (guild.IsSuggestionModuleEnabled)
            return new ArgumentError(nameof(guild.SuggestionConfig),"Suggestion module is already enabled, please use the repair action.");

        var addRes = await _guildDataService.AddConfigAsync(command.RequestDto, true);

        var embed = new DiscordEmbedBuilder();
        embed.WithColor(new DiscordColor(guild.EmbedHexColor));
        embed.WithAuthor("Suggestion configuration");
        embed.WithDescription("Process completed successfully");
        embed.AddField("Suggestions channel", ExtendedFormatter.Mention(command.RequestDto.ChannelId, DiscordEntity.Channel));
        embed.AddField("Should create threads", command.RequestDto.ShouldUseThreads.ToString());
        embed.AddField("Should add votes", command.RequestDto.ShouldAddReactionVotes.ToString());
        embed.WithFooter(
            $"Lisbeth configuration requested by {requestingMember.GetFullDisplayName()} | Id: {requestingMember.Id}");

        return embed.Build();
    }
}
