using DSharpPlus.Entities;
using Lisbeth.Bot.Application.Discord.Commands.Reminder;
using Lisbeth.Bot.DataAccessLayer.Specifications.Guild;
using MikyM.Common.Application.CommandHandlers;
using MikyM.Discord.Enums;
using MikyM.Discord.Extensions.BaseExtensions;
using MikyM.Discord.Interfaces;

namespace Lisbeth.Bot.Application.Discord.CommandHandlers.Reminder;

[UsedImplicitly]
public class DisableReminderCommandHandler : ICommandHandler<DisableReminderCommand, DiscordEmbed>
{
    private readonly IGuildDataService _guildDataService;
    private readonly IDiscordService _discord;
    private readonly IMainReminderService _reminderService;

    public DisableReminderCommandHandler(IGuildDataService guildDataService, IDiscordService discord,
        IMainReminderService reminderService)
    {
        _guildDataService = guildDataService;
        _discord = discord;
        _reminderService = reminderService;
    }

    public async Task<Result<DiscordEmbed>> HandleAsync(DisableReminderCommand command)
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

        if (command.Dto.Type is ReminderType.Recurring && !requestingUser.IsModerator())
            return new DiscordNotAuthorizedError();

        var result = await _guildDataService.GetSingleBySpecAsync<Guild>(new ActiveGuildByIdSpec(command.Dto.GuildId));

        if (!result.IsDefined()) return Result<DiscordEmbed>.FromError(result);

        var res = await _reminderService.DisableReminderAsync(command.Dto);

        if (!res.IsDefined()) return Result<DiscordEmbed>.FromError(res);

        var embed = new DiscordEmbedBuilder().WithColor(new DiscordColor(result.Entity.EmbedHexColor))
            .WithAuthor("Lisbeth reminder service")
            .WithDescription("Reminder disabled successfully")
            .AddField("Reminder's id", res.Entity.Id.ToString())
            .AddField("Reminder's name", res.Entity.Name);

        return embed.Build();
    }
}
