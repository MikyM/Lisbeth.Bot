using DSharpPlus.Entities;
using Lisbeth.Bot.Application.Discord.Commands.Reminder;
using Lisbeth.Bot.Application.Discord.EmbedBuilders;
using Lisbeth.Bot.Application.Discord.EmbedEnrichers.Response.Reminder;
using Lisbeth.Bot.Application.Discord.SlashCommands;
using Lisbeth.Bot.DataAccessLayer.Specifications.Guild;
using MikyM.Common.Application.CommandHandlers;
using MikyM.Discord.Enums;
using MikyM.Discord.Extensions.BaseExtensions;
using MikyM.Discord.Interfaces;

namespace Lisbeth.Bot.Application.Discord.CommandHandlers.Reminder;

[UsedImplicitly]
public class SetNewReminderCommandHandler : ICommandHandler<SetNewReminderCommand, DiscordEmbed>
{
    private readonly IGuildDataService _guildDataService;
    private readonly IDiscordService _discord;
    private readonly IMainReminderService _reminderService;
    private readonly IResponseDiscordEmbedBuilder<RegularUserInteraction> _embedBuilder;

    public SetNewReminderCommandHandler(IGuildDataService guildDataService, IDiscordService discord,
        IMainReminderService reminderService, IResponseDiscordEmbedBuilder<RegularUserInteraction> embedBuilder)
    {
        _guildDataService = guildDataService;
        _discord = discord;
        _reminderService = reminderService;
        _embedBuilder = embedBuilder;
    }

    public async Task<Result<DiscordEmbed>> HandleAsync(SetNewReminderCommand command)
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
            return Result<DiscordEmbed>.FromError(new DiscordNotAuthorizedError());

        var result = await _guildDataService.GetSingleBySpecAsync<Guild>(new ActiveGuildByIdSpec(command.Dto.GuildId));

        if (!result.IsDefined()) return Result<DiscordEmbed>.FromError(result);

        if (command.Dto.ChannelId.HasValue && !requestingUser.IsModerator()) return new DiscordNotAuthorizedError("Only moderators can set channel specific reminders.");

        var res = await _reminderService.SetNewReminderAsync(command.Dto);

        if (!res.IsDefined(out var reminder)) return Result<DiscordEmbed>.FromError(res);

        return _embedBuilder
            .WithType(RegularUserInteraction.Reminder)
            .EnrichFrom(new ReminderEmbedEnricher(reminder, ReminderActionType.Set))
            .WithEmbedColor(new DiscordColor(result.Entity.EmbedHexColor))
            .WithAuthorSnowflakeInfo(requestingUser)
            .Build();
    }
}
