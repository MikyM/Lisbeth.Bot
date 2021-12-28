using System.Collections.Generic;
using System.Globalization;
using DSharpPlus.Entities;
using Lisbeth.Bot.Application.Discord.Commands.Reminder;
using Lisbeth.Bot.Application.Discord.EmbedBuilders;
using Lisbeth.Bot.DataAccessLayer.Specifications.Guild;
using MikyM.Common.Application.CommandHandlers;
using MikyM.Discord.EmbedBuilders.Builders;
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
    private readonly IResponseDiscordEmbedBuilder<UserInteraction> _embedBuilder;

    public RescheduleReminderCommandHandler(IGuildDataService guildDataService, IDiscordService discord,
        IMainReminderService reminderService, IResponseDiscordEmbedBuilder<UserInteraction> embedBuilder)
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

        var result = await _guildDataService.GetSingleBySpecAsync<Guild>(new ActiveGuildByIdSpec(command.Dto.GuildId));

        if (!result.IsDefined()) return Result<DiscordEmbed>.FromError(result);

        var res = await _reminderService.RescheduleReminderAsync(command.Dto);

        if (!res.IsDefined()) return Result<DiscordEmbed>.FromError(res);

        return new DiscordEmbedBuilder().Build();
    }
}
