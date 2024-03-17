using Lisbeth.Bot.Application.Discord.Commands.MemberHistoryEntry;
using Lisbeth.Bot.DataAccessLayer.Specifications.Guild;
using MikyM.Discord.Extensions.BaseExtensions;

namespace Lisbeth.Bot.Application.Discord.CommandHandlers.MemberHistoryEntry;

[UsedImplicitly]
public class AddMemberHistoryEntryCommandHandler : IAsyncCommandHandler<AddMemberHistoryEntryCommand>
{
    private readonly IGuildDataService _guildDataService;

    public AddMemberHistoryEntryCommandHandler(IGuildDataService guildDataService)
    {
        _guildDataService = guildDataService;
    }

    public async Task<Result> HandleAsync(AddMemberHistoryEntryCommand historyEntryCommand, CancellationToken cancellationToken = default)
    {
        var guildRes =
            await _guildDataService.GetSingleBySpecAsync(
                new ActiveGuildByIdSpec(historyEntryCommand.Guild.Id));

        if (!guildRes.IsDefined(out var guildCfg))
            return Result.FromError(guildRes);

        var dt = historyEntryCommand.Member.CreationTimestamp.UtcDateTime;
        var dtLocal = DateTime.SpecifyKind(dt, DateTimeKind.Local);
        
        _ = _guildDataService.BeginUpdate(guildCfg);
        guildCfg.AddMemberHistoryEntry(historyEntryCommand.Member.Id, historyEntryCommand.Member.GetFullUsername(),
            dtLocal);
        
        _ = await _guildDataService.CommitAsync();

        return Result.FromSuccess();
    }
}
