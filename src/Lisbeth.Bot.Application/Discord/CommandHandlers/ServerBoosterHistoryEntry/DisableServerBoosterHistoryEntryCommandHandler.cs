using DSharpPlus.Entities;
using Lisbeth.Bot.Application.Discord.Commands.ServerBoosterHistoryEntry;
using Lisbeth.Bot.DataAccessLayer.Specifications.Guild;

namespace Lisbeth.Bot.Application.Discord.CommandHandlers.ServerBoosterHistoryEntry;

[UsedImplicitly]
public class DisableServerBoosterHistoryEntryCommandHandler : ICommandHandler<DisableServerBoosterHistoryEntryCommand>
{
    private readonly IGuildDataService _guildDataService;

    public DisableServerBoosterHistoryEntryCommandHandler(IGuildDataService guildDataService)
    {
        _guildDataService = guildDataService;
    }
    
    public async Task<Result> HandleAsync(DisableServerBoosterHistoryEntryCommand historyEntryCommand, CancellationToken cancellationToken = default)
    {
        var guild = historyEntryCommand.Guild;
        var boosterLeft = historyEntryCommand.Guild is not null;
        
        if (guild is null)
        {
            var guildRes =
                await _guildDataService.GetSingleBySpecAsync(
                    new ActiveGuildByDiscordIdWithBoostersEntriesSpec(historyEntryCommand.DiscordGuild.Id, historyEntryCommand.Member.Id));

            if (!guildRes.IsDefined(out guild))
                return Result.FromError(guildRes);
        }

        var embed = new DiscordEmbedBuilder();
        var channel = historyEntryCommand.DiscordGuild.SystemChannel;
        
        embed.WithTitle("Member has canceled Nitro Boost");
        embed.WithThumbnail(historyEntryCommand.Member.AvatarUrl);
        embed.AddField("Member's ID", $"[{historyEntryCommand.Member.Id}](https://discordapp.com/users/{historyEntryCommand.Member.Id})", true);
        embed.AddField("Member's mention", $"{historyEntryCommand.Member.Mention}", true);
        embed.AddField("Joined guild at", $"{historyEntryCommand.Member.JoinedAt}");
        embed.WithColor(new DiscordColor(guild.EmbedHexColor));
        embed.WithFooter($"Member ID: {historyEntryCommand.Member.Id}");

        if (!boosterLeft)
        {
            _ = _guildDataService.BeginUpdate(guild);
            guild.DisableServerBoosterHistoryEntry(historyEntryCommand.Member.Id);
            _ = await _guildDataService.CommitAsync();
        }

        try
        {
            await channel.SendMessageAsync(embed);
        }
        catch
        {
            // ignore
        }

        return Result.FromSuccess();
    }
}
