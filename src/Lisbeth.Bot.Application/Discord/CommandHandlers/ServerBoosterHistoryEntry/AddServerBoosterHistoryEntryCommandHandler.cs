using DSharpPlus.Entities;
using Lisbeth.Bot.Application.Discord.Commands.ServerBoosterHistoryEntry;
using Lisbeth.Bot.DataAccessLayer.Specifications.Guild;
using MikyM.CommandHandlers;
using MikyM.Common.Utilities.Results;
using MikyM.Common.Utilities.Results.Errors;
using MikyM.Discord.Extensions.BaseExtensions;

namespace Lisbeth.Bot.Application.Discord.CommandHandlers.ServerBoosterHistoryEntry;

[UsedImplicitly]
public class AddServerBoosterHistoryEntryCommandHandler : ICommandHandler<AddServerBoosterHistoryEntryCommand>
{
    private readonly IGuildDataService _guildDataService;

    public AddServerBoosterHistoryEntryCommandHandler(IGuildDataService guildDataService)
    {
        _guildDataService = guildDataService;
    }

    public async Task<Result> HandleAsync(AddServerBoosterHistoryEntryCommand historyEntryCommand)
    {
        var guildRes =
            await _guildDataService.GetSingleBySpecAsync(
                new ActiveGuildByDiscordIdWithMembersEntriesSpec(historyEntryCommand.Guild.Id, historyEntryCommand.Member.Id));

        if (!guildRes.IsDefined(out var guildCfg))
            return Result.FromError(guildRes);

        var entry = guildCfg.MemberHistoryEntries?.Where(x =>
            x.UserId == historyEntryCommand.Member.Id && x.GuildId == historyEntryCommand.Guild.Id && !x.IsDisabled)?.MaxBy(x => x.CreatedAt);
        if (entry is null)
            return new NotFoundError("Couldn't find a corresponding member entry.");
        
        var embed = new DiscordEmbedBuilder();
        var channel = historyEntryCommand.Guild.SystemChannel;
        
        embed.WithTitle("Member has boosted the server");
        embed.WithThumbnail(historyEntryCommand.Member.AvatarUrl);
        embed.AddField("Member's ID", $"[{historyEntryCommand.Member.Id}](https://discordapp.com/users/{historyEntryCommand.Member.Id})", true);
        embed.AddField("Member's mention", $"{historyEntryCommand.Member.Mention}", true);
        embed.AddField("Joined guild at", $"{historyEntryCommand.Member.JoinedAt}");
        embed.WithColor(new DiscordColor(guildCfg.EmbedHexColor));
        embed.WithFooter($"Member ID: {historyEntryCommand.Member.Id}");

        _ = _guildDataService.BeginUpdate(guildCfg);
        guildCfg.AddServerBoosterHistoryEntry(historyEntryCommand.Member.Id, historyEntryCommand.Member.GetFullUsername(), entry.Id);
        _ = await _guildDataService.CommitAsync();

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
