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
using Lisbeth.Bot.Application.Discord.Commands.Reminder;
using Lisbeth.Bot.Application.Discord.EmbedBuilders;
using Lisbeth.Bot.Application.Discord.EmbedEnrichers.Response.Reminder;
using Lisbeth.Bot.Application.Discord.SlashCommands;
using Lisbeth.Bot.DataAccessLayer.Specifications.Guild;
using MikyM.CommandHandlers;
using MikyM.Common.Utilities.Results;
using MikyM.Discord.Enums;
using MikyM.Discord.Extensions.BaseExtensions;
using MikyM.Discord.Interfaces;

namespace Lisbeth.Bot.Application.Discord.CommandHandlers.Reminder;

[UsedImplicitly]
public class RescheduleReminderCommandHandler : ICommandHandler<RescheduleReminderCommand, DiscordEmbed>
{
    private readonly IGuildDataService _guildDataService;
    private readonly IDiscordService _discord;
    private readonly IMainReminderService _reminderService;
    private readonly IResponseDiscordEmbedBuilder<RegularUserInteraction> _embedBuilder;

    public RescheduleReminderCommandHandler(IGuildDataService guildDataService, IDiscordService discord,
        IMainReminderService reminderService, IResponseDiscordEmbedBuilder<RegularUserInteraction> embedBuilder)
    {
        _guildDataService = guildDataService;
        _discord = discord;
        _reminderService = reminderService;
        _embedBuilder = embedBuilder;
    }

    public async Task<Result<DiscordEmbed>> HandleAsync(RescheduleReminderCommand command)
    {
        if (command is null) throw new ArgumentNullException(nameof(command));

        // req data
        DiscordGuild guild = command.Ctx?.Guild ?? await _discord.Client.GetGuildAsync(command.Dto.GuildId);
        DiscordMember requestingUser = command.Ctx?.User as DiscordMember ??
                                       await guild.GetMemberAsync(command.Dto.RequestedOnBehalfOfId);

        if (guild is null)
            return new DiscordNotFoundError(DiscordEntity.Guild);
        if (requestingUser is null)
            return new DiscordNotFoundError(DiscordEntity.User);

        if (!string.IsNullOrWhiteSpace(command.Dto.CronExpression) && !requestingUser.IsModerator())
            return new DiscordNotAuthorizedError();

        var result = await _guildDataService.GetSingleBySpecAsync(new ActiveGuildByIdSpec(command.Dto.GuildId));

        if (!result.IsDefined()) return Result<DiscordEmbed>.FromError(result);
        if (!result.Entity.IsReminderModuleEnabled)
            return new DisabledGuildModuleError(GuildModule.Reminders);

        var res = await _reminderService.RescheduleReminderAsync(command.Dto);

        if (!res.IsDefined()) return Result<DiscordEmbed>.FromError(res);

        return _embedBuilder
            .WithType(RegularUserInteraction.Reminder)
            .EnrichFrom(new ReminderEmbedEnricher(res.Entity, ReminderActionType.Reschedule))
            .WithEmbedColor(new DiscordColor(result.Entity.EmbedHexColor))
            .WithAuthorSnowflakeInfo(requestingUser)
            .Build();
    }
}
