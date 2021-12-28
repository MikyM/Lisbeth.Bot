using DSharpPlus.Entities;
using Lisbeth.Bot.Application.Discord.Commands.Reminder;
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

    public SetNewReminderCommandHandler(IGuildDataService guildDataService, IDiscordService discord,
        IMainReminderService reminderService)
    {
        _guildDataService = guildDataService;
        _discord = discord;
        _reminderService = reminderService;
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

        if (command.Dto.ChannelId.HasValue && !requestingUser.IsModerator()) return new DiscordNotAuthorizedError("Only moderators can set specific channel reminders.");

        var res = await _reminderService.SetNewReminderAsync(command.Dto);

        if (!res.IsDefined(out var reminder)) return Result<DiscordEmbed>.FromError(res);

        var embed = new DiscordEmbedBuilder().WithColor(new DiscordColor(result.Entity.EmbedHexColor))
            .WithAuthor("Lisbeth reminder service")
            .WithDescription("Reminder set successfully")
            .AddField("Reminder's id", res.Entity.Id.ToString())
            .AddField("Reminder's name", res.Entity.Name)
            .AddField("Next occurrence", res.Entity.NextOccurrence.ToUniversalTime().ToString("dd/MM/yyyy hh:mm tt") + " UTC")
            .AddField("Mentions", string.Join(", ", res.Entity.Mentions ?? throw new InvalidOperationException()));

        return embed.Build();
    }
}
