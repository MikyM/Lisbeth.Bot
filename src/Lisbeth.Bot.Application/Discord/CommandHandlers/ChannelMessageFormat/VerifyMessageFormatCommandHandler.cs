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

using AutoMapper;
using DSharpPlus.Entities;
using Lisbeth.Bot.Application.Discord.Commands.ChannelMessageFormat;
using Lisbeth.Bot.Application.Discord.Commands.DirectMessage;
using Lisbeth.Bot.Application.Discord.EmbedBuilders;
using Lisbeth.Bot.Application.Discord.EmbedEnrichers.DirectMessage;
using Lisbeth.Bot.Application.Discord.EmbedEnrichers.Response.ChannelMessageFormat;
using Lisbeth.Bot.Application.Discord.SlashCommands;
using Lisbeth.Bot.Application.Validation.DirectMessage;
using Lisbeth.Bot.DataAccessLayer.Specifications.Guild;
using Lisbeth.Bot.Domain.DTOs;
using Lisbeth.Bot.Domain.DTOs.Request.DirectMessage;
using Microsoft.Extensions.Logging;
using MikyM.Common.Application.CommandHandlers;
using MikyM.Common.Utilities.Extensions;
using MikyM.Discord.Enums;
using MikyM.Discord.Extensions.BaseExtensions;
using MikyM.Discord.Interfaces;

namespace Lisbeth.Bot.Application.Discord.CommandHandlers.ChannelMessageFormat;

[UsedImplicitly]
public class VerifyMessageFormatCommandHandler : ICommandHandler<VerifyMessageFormatCommand, VerifyMessageFormatResDto>
{
    private readonly IDiscordService _discord;
    private readonly IGuildDataService _guildDataService;
    private readonly IResponseDiscordEmbedBuilder<RegularUserInteraction> _embedBuilder;
    private readonly ICommandHandler<SendDirectMessageCommand> _sendHandler;
    private readonly IMapper _mapper;
    private readonly ILogger<VerifyMessageFormatCommandHandler> _logger;

    public VerifyMessageFormatCommandHandler(IDiscordService discord, IGuildDataService guildDataService,
        IResponseDiscordEmbedBuilder<RegularUserInteraction> embedBuilder,
        ICommandHandler<SendDirectMessageCommand> sendHandler, IMapper mapper,
        ILogger<VerifyMessageFormatCommandHandler> logger)
    {
        _discord = discord;
        _guildDataService = guildDataService;
        _embedBuilder = embedBuilder;
        _sendHandler = sendHandler;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<Result<VerifyMessageFormatResDto>> HandleAsync(VerifyMessageFormatCommand command)
    {
        if (command is null) throw new ArgumentNullException(nameof(command));

        // data req
        DiscordMember? requestingUser;
        DiscordChannel? channel;
        DiscordMessage? target;
        DiscordGuild? guild;

        try
        {
            guild = command.EventArgs?.Guild ??
                        command.Ctx?.Guild ?? await _discord.Client.GetGuildAsync(command.Dto.GuildId);

            if (guild is null) return new DiscordNotFoundError(DiscordEntity.Guild);

            requestingUser = command.Ctx?.Member ?? await guild.GetMemberAsync(command.Dto.RequestedOnBehalfOfId);

            channel = command.EventArgs?.Channel ??
                      command.Ctx?.ResolvedChannelMentions[0] ?? guild.GetChannel(command.Dto.ChannelId);

            if (channel is null) return new DiscordNotFoundError(DiscordEntity.Channel);

            target = command.EventArgs?.Message ?? await channel.GetMessageAsync(command.Dto.MessageId);
        }
        catch (Exception ex)
        {
            return ex;
        }

        if (requestingUser is null)
            return new DiscordNotFoundError(DiscordEntity.Member);
        if (target is null)
            return new DiscordNotFoundError(DiscordEntity.Message);

        if (!requestingUser.IsModerator())
            return new DiscordNotAuthorizedError();

        var guildRes =
            await _guildDataService.GetSingleBySpecAsync(
                new ActiveGuildByDiscordIdWithChannelMessageFormatSpec(command.Dto.GuildId, channel.Id));

        if (!guildRes.IsDefined(out var guildCfg))
            return Result<VerifyMessageFormatResDto>.FromError(guildRes);

        var format = guildCfg.ChannelMessageFormats?.FirstOrDefault(x => x.ChannelId == command.Dto.ChannelId);
        if (format is null)
            return new ArgumentError(nameof(command.Dto.ChannelId),
                "There's no message format registered for this channel");
        if (format.IsDisabled)
            return new DisabledEntityError("Message format is currently disabled for this channel, enable it first");

        var isCompliant = format.IsTextCompliant(target.Content ?? string.Empty);

        VerifyMessageFormatResDto? res = null;
        bool wasAuthorInformed = false;

        switch (isCompliant)
        {
            case true:
                res = new VerifyMessageFormatResDto(isCompliant);
                break;
            case false:
                try
                {
                    await channel.DeleteMessageAsync(target, "Message not compliant with channel message format");

                    var userInfoEmbed = _embedBuilder
                        .EnrichFrom(new MessageFormatComplianceEmbedEnricher(format, target))
                        .WithEmbedColor(new DiscordColor(guildCfg.EmbedHexColor))
                        .DisableTemplating()
                        .Build();

                    var sendReq = new SendDirectMessageReqDto
                    {
                        EmbedConfig = _mapper.Map<EmbedConfigDto>(userInfoEmbed),
                        GuildId = guild.Id,
                        RecipientUserId = target.Author.Id,
                        RequestedOnBehalfOfId = requestingUser.Id
                    };

                    var validator = new SendDirectMessageReqValidator(_discord.Client);

                    var validationResult = await validator.ValidateAsync(sendReq);

                    if (validationResult.IsValid)
                    {
                        var sendRes = await _sendHandler.HandleAsync(new SendDirectMessageCommand(sendReq,
                            requestingUser, guild, target.Author as DiscordMember));

                        wasAuthorInformed = sendRes.IsSuccess;
                    }
                }
                catch
                {
                    res = new VerifyMessageFormatResDto(isCompliant, false, wasAuthorInformed);
                }

                break;
        }

        res ??= new VerifyMessageFormatResDto(isCompliant, true, wasAuthorInformed);

        var embed = _embedBuilder
            .WithType(RegularUserInteraction.ChannelMessageFormat)
            .EnrichFrom(new ChannelMessageFormatEmbedEnricher(format, ChannelMessageFormatActionType.Verify, res, target.Content ?? ""))
            .WithEmbedColor(new DiscordColor(guildCfg.EmbedHexColor))
            .WithAuthorSnowflakeInfo(requestingUser)
            .Build();

        res.Embed = embed;

        return res;
    }
}