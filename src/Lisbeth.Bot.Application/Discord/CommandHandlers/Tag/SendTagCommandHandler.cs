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

using AutoMapper;
using DSharpPlus.Entities;
using Lisbeth.Bot.Application.Discord.Commands.Tag;
using Lisbeth.Bot.Domain.DTOs.Request.Tag;
using MikyM.Discord.Extensions.BaseExtensions;

namespace Lisbeth.Bot.Application.Discord.CommandHandlers.Tag;

[UsedImplicitly]
public class SendTagCommandHandler : ICommandHandler<SendTagCommand>
{
    private readonly IDiscordService _discord;
    private readonly ICommandHandler<GetTagCommand, DiscordMessageBuilder> _commandHandler;
    private readonly IMapper _mapper;

    public SendTagCommandHandler(IDiscordService discord,
        ICommandHandler<GetTagCommand, DiscordMessageBuilder> commandHandler, IMapper mapper)
    {
        _discord = discord;
        _commandHandler = commandHandler;
        _mapper = mapper;
    }

    public async Task<Result> HandleAsync(SendTagCommand command, CancellationToken cancellationToken = default)
    {
        if (command is null) throw new ArgumentNullException(nameof(command));

        // data req
        DiscordGuild guild = command.Ctx?.Guild ?? await _discord.Client.GetGuildAsync(command.Dto.GuildId);
        DiscordMember requestingUser = command.Ctx?.User as DiscordMember ??
                                       await guild.GetMemberAsync(command.Dto.RequestedOnBehalfOfId);
        DiscordChannel channel = command.Ctx?.ResolvedChannelMentions?[0] ?? guild.GetChannel(command.Dto.ChannelId!.Value);
        
        if (guild is null)
            return new DiscordNotFoundError(DiscordEntity.Guild);
        if (requestingUser is null)
            return new DiscordNotFoundError(DiscordEntity.User);
        if (channel is null)
            return new DiscordNotFoundError(DiscordEntity.Channel);

        if (!requestingUser.IsModerator())
            return new DiscordNotAuthorizedError();

        var partial =
            await _commandHandler.HandleAsync(new GetTagCommand(_mapper.Map<TagGetReqDto>(command.Dto), command.Ctx));

        if (!partial.IsDefined(out var tag)) return Result.FromError(partial);

        try
        {
            await channel.SendMessageAsync(tag);
        }
        catch (Exception ex)
        {
            return ex;
        }

        return Result.FromSuccess();
    }
}
