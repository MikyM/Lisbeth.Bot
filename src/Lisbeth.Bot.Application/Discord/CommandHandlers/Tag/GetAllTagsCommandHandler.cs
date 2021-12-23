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

using System.Collections.Generic;
using System.Globalization;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity;
using Lisbeth.Bot.Application.Discord.Commands.Tag;
using Lisbeth.Bot.Application.Discord.Extensions;
using Lisbeth.Bot.Application.Discord.Helpers;
using Lisbeth.Bot.DataAccessLayer.Specifications.Guild;
using Lisbeth.Bot.DataAccessLayer.Specifications.Tag;
using MikyM.Common.Application.CommandHandlers;
using MikyM.Common.Utilities.Extensions;
using MikyM.Discord.Enums;
using MikyM.Discord.Extensions.BaseExtensions;
using MikyM.Discord.Interfaces;

namespace Lisbeth.Bot.Application.Discord.CommandHandlers.Tag;

[UsedImplicitly]
public class GetAllTagsCommandHandler : ICommandHandler<GetAllTagsCommand, List<Page>>
{
    private readonly IDiscordService _discord;
    private readonly IGuildDataService _guildDataService;

    public GetAllTagsCommandHandler(IDiscordService discord, IGuildDataService guildDataService)
    {
        _discord = discord;
        _guildDataService = guildDataService;
    }

    public async Task<Result<List<Page>>> HandleAsync(GetAllTagsCommand command)
    {
        if (command is null) throw new ArgumentNullException(nameof(command));

        // data req
        DiscordGuild guild = command.Ctx?.Guild ?? await _discord.Client.GetGuildAsync(command.Dto.GuildId);
        DiscordMember requestingUser = command.Ctx?.User as DiscordMember ??
                                       await guild.GetMemberAsync(command.Dto.RequestedOnBehalfOfId);

        if (guild is null)
            return new DiscordNotFoundError(DiscordEntity.Guild);
        if (requestingUser is null)
            return new DiscordNotFoundError(DiscordEntity.User);

        var guildRes =
            await _guildDataService.GetSingleBySpecAsync(
                new ActiveGuildByDiscordIdWithTagsSpecifications(guild.Id));

        if (!guildRes.IsDefined(out var guildCfg))
            return Result<List<Page>>.FromError(guildRes);

        if (requestingUser.Guild.Id != guild.Id) return new DiscordNotAuthorizedError();

        var tags = guildCfg.Tags?.ToList();

        if (tags is null || !tags.Any()) return new NotFoundError("This guild has no tags created yet!");

        var pages = new List<Page>();
        var chunked = tags.OrderByDescending(x => x.CreatedAt).ToList().ChunkBy(10).OrderByDescending(x => x.Count).ToList();

        int pageNumber = 1;
        foreach (var chunk in chunked)
        {
            var embedBuilder = new DiscordEmbedBuilder()
                .WithColor(new DiscordColor(guildCfg.EmbedHexColor))
                .WithTitle("Available tags")
                .WithFooter($"Current page: {pageNumber} | Total pages: {chunked.Count}");

            foreach (var tag in chunk)
            {
                embedBuilder.AddField(tag.Name,
                    $"Created by {ExtendedFormatter.Mention(tag.CreatorId, DiscordEntity.User)} on {(tag.CreatedAt.HasValue ? tag.CreatedAt.Value.ToString(CultureInfo.CurrentCulture) : "unknown")}");
            }

            pages.Add(new Page("", embedBuilder));
            pageNumber++;
        }

        return pages;
    }
}
