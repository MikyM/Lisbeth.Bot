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
using Lisbeth.Bot.Application.Discord.Commands.Tag;
using Lisbeth.Bot.Application.Discord.Extensions;
using Lisbeth.Bot.Application.Discord.Helpers;
using Lisbeth.Bot.DataAccessLayer.Specifications.Guild;
using Lisbeth.Bot.DataAccessLayer.Specifications.Tag;
using MikyM.Common.Application.CommandHandlers;
using MikyM.Common.Utilities.Results;
using MikyM.Common.Utilities.Results.Errors;
using MikyM.Discord.Enums;
using MikyM.Discord.Extensions.BaseExtensions;
using MikyM.Discord.Interfaces;

namespace Lisbeth.Bot.Application.Discord.CommandHandlers.Tag;

[UsedImplicitly]
public class GetTagCommandHandler : ICommandHandler<GetTagCommand, DiscordMessageBuilder>
{
    private readonly IDiscordService _discord;
    private readonly IGuildDataService _guildDataService;
    private readonly ITagDataService _tagDataService;
    private readonly IDiscordEmbedProvider _embedProvider;

    public GetTagCommandHandler(IDiscordService discord, IGuildDataService guildDataService,
        ITagDataService tagDataService, IDiscordEmbedProvider embedProvider)
    {
        _discord = discord;
        _guildDataService = guildDataService;
        _tagDataService = tagDataService;
        _embedProvider = embedProvider;
    }

    public async Task<Result<DiscordMessageBuilder>> HandleAsync(GetTagCommand command)
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

        Domain.Entities.Tag tag;
        if (requestingUser.IsBotOwner(_discord.Client))
        {
            Result<Domain.Entities.Tag> partial;
            if (command.Dto.Id.HasValue) partial = await _tagDataService.GetAsync(command.Dto.Id.Value);
            else
                partial = await _tagDataService.GetSingleBySpecAsync<Domain.Entities.Tag>(new TagByNameSpec(command.Dto.Name!));

            if (!partial.IsDefined()) return Result<DiscordMessageBuilder>.FromError(partial);

            tag = partial.Entity;
        }
        else
        {
            var guildCfg =
                await _guildDataService.GetSingleBySpecAsync(
                    new ActiveGuildByIdSpec(guild.Id));
            if (!guildCfg.IsDefined())
                return Result<DiscordMessageBuilder>.FromError(guildCfg);

            if (requestingUser.Guild.Id != guild.Id) return new DiscordNotAuthorizedError();

            var partial = await _tagDataService.GetSingleBySpecAsync<Domain.Entities.Tag>(new ActiveTagByGuildAndNameSpec(command.Dto.Name!, command.Dto.GuildId));

            if (!partial.IsDefined()) return Result<DiscordMessageBuilder>.FromError(partial);

            tag = partial.Entity;
        }

        if (tag is null) return new NotFoundError("Tag not found");
        if (tag.IsDisabled && !requestingUser.IsBotOwner(_discord.Client)) return new DisabledEntityError(nameof(tag));
        if (!requestingUser.CanAccessTag(tag) && !requestingUser.IsBotOwner(_discord.Client))
            return new DiscordNotAuthorizedError();

        return tag.EmbedConfig is null
            ? new DiscordMessageBuilder().WithContent(tag.Text)
            : new DiscordMessageBuilder().AddEmbed(_embedProvider.GetEmbedFromConfig(tag.EmbedConfig).Build());
    }
}
