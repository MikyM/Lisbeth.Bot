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

using Lisbeth.Bot.Application.Discord.Commands.ChannelMessageFormat;
using Lisbeth.Bot.Application.Discord.SlashCommands;
using Lisbeth.Bot.DataAccessLayer.Specifications.Guild;
using MikyM.Discord.Extensions.BaseExtensions;

namespace Lisbeth.Bot.Application.Discord.CommandHandlers.ChannelMessageFormat;

[UsedImplicitly]
public class DisableMessageFormatCommandHandler : IAsyncCommandHandler<DisableMessageFormatCommand, DiscordEmbed>
{
    private readonly IDiscordService _discord;
    private readonly IGuildDataService _guildDataService;
    private readonly IResponseDiscordEmbedBuilder<RegularUserInteraction> _embedBuilder;

    public DisableMessageFormatCommandHandler(IDiscordService discord, IGuildDataService guildDataService,
        IResponseDiscordEmbedBuilder<RegularUserInteraction> embedBuilder)
    {
        _discord = discord;
        _guildDataService = guildDataService;
        _embedBuilder = embedBuilder;
    }

    public async Task<Result<DiscordEmbed>> HandleAsync(DisableMessageFormatCommand command, CancellationToken cancellationToken = default)
    {
        if (command is null) throw new ArgumentNullException(nameof(command));

        // data req
        DiscordMember? requestingUser;
        DiscordChannel? channel;

        try
        {
            var guild = command.Ctx?.Guild ?? await _discord.Client.GetGuildAsync(command.Dto.GuildId);

            if (guild is null)
                return new DiscordNotFoundError(DiscordEntity.Guild);

            requestingUser = command.Ctx?.User as DiscordMember ?? await guild.GetMemberAsync(command.Dto.RequestedOnBehalfOfId);
            channel = command.Ctx?.ResolvedChannelMentions[0] ?? guild.GetChannel(command.Dto.ChannelId);
        }
        catch (Exception ex)
        {
            return ex;
        }

        if (requestingUser is null)
            return new DiscordNotFoundError(DiscordEntity.Member);
        if (channel is null)
            return new DiscordNotFoundError(DiscordEntity.Channel);

        if (!requestingUser.IsModerator())
            return new DiscordNotAuthorizedError();

        var guildRes =
            await _guildDataService.GetSingleBySpecAsync(
                new ActiveGuildByDiscordIdWithChannelMessageFormatSpec(command.Dto.GuildId, channel.Id));

        if (!guildRes.IsDefined(out var guildCfg))
            return Result<DiscordEmbed>.FromError(guildRes);

        var format = guildCfg.ChannelMessageFormats?.FirstOrDefault(x => x.ChannelId == command.Dto.ChannelId);
        if (format is null)
            return new ArgumentInvalidError(nameof(command.Dto.ChannelId),
                "There's no message format registered for this channel");
        if (format.IsDisabled == command.Dto.IsDisabled)
            return new ArgumentInvalidError(nameof(format.IsDisabled),
                $"Entity is already {(format.IsDisabled ? "disabled" : "enabled")}");

        _guildDataService.BeginUpdate(guildCfg);
        format.IsDisabled = command.Dto.IsDisabled;
        format.LastEditById = command.Dto.RequestedOnBehalfOfId;
        await _guildDataService.CommitAsync(requestingUser.Id.ToString());

        return _embedBuilder
            .WithType(RegularUserInteraction.ChannelMessageFormat)
            .EnrichFrom(new ChannelMessageFormatEmbedEnricher(format, ChannelMessageFormatActionType.Disable))
            .WithEmbedColor(new DiscordColor(guildCfg.EmbedHexColor))
            .WithAuthorSnowflakeInfo(requestingUser)
            .Build();
    }
}
