using Lisbeth.Bot.Application.Discord.Commands.MemberHistoryEntry;
using Lisbeth.Bot.DataAccessLayer.Specifications.Guild;
using MikyM.Discord.Extensions.BaseExtensions;

namespace Lisbeth.Bot.Application.Discord.CommandHandlers.MemberHistoryEntry;

[UsedImplicitly]
public class AddMemberHistoryEntryCommandHandler : ICommandHandler<AddMemberHistoryEntryCommand>
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

        _ = _guildDataService.BeginUpdate(guildCfg);
        guildCfg.AddMemberHistoryEntry(historyEntryCommand.Member.Id, historyEntryCommand.Member.GetFullUsername(),
            historyEntryCommand.Member.CreationTimestamp.UtcDateTime);
        _ = await _guildDataService.CommitAsync();

        return Result.FromSuccess();
    }
}
