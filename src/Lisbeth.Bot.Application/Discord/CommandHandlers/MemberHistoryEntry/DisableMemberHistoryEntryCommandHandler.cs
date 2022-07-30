using DSharpPlus.Entities;
using Lisbeth.Bot.Application.Discord.Commands.MemberHistoryEntry;
using Lisbeth.Bot.Application.Discord.Commands.ServerBoosterHistoryEntry;
using Lisbeth.Bot.DataAccessLayer.Specifications.Guild;
using MikyM.CommandHandlers;
using MikyM.Common.Utilities.Results;

namespace Lisbeth.Bot.Application.Discord.CommandHandlers.MemberHistoryEntry;

[UsedImplicitly]
public class DisableMemberHistoryEntryCommandHandler : ICommandHandler<DisableMemberHistoryEntryCommand>
{
    private readonly IGuildDataService _guildDataService;

    public DisableMemberHistoryEntryCommandHandler(IGuildDataService guildDataService)
    {
        _guildDataService = guildDataService;
    }
    
    public async Task<Result> HandleAsync(DisableMemberHistoryEntryCommand historyEntryCommand)
    {
        var guildRes =
            await _guildDataService.GetSingleBySpecAsync(
                new ActiveGuildByDiscordIdWithMembersEntriesSpec(historyEntryCommand.Guild.Id, historyEntryCommand.Member.Id));

        if (!guildRes.IsDefined(out var guildCfg))
            return Result.FromError(guildRes);
        
        _ = _guildDataService.BeginUpdate(guildCfg);
        guildCfg.DisableMemberHistoryEntry(historyEntryCommand.Member.Id);
        _ = await _guildDataService.CommitAsync();

        return Result.FromSuccess();
    }
}
