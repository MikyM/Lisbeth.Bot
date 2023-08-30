using System.Globalization;
using Lisbeth.Bot.Application.Discord.Commands.MemberHistoryEntry;
using Lisbeth.Bot.DataAccessLayer.Specifications.Guild;
using MikyM.Discord.Extensions.BaseExtensions;

namespace Lisbeth.Bot.Application.Discord.CommandHandlers.MemberHistoryEntry;

[UsedImplicitly]
public class DisableMemberHistoryEntryCommandHandler : IAsyncCommandHandler<DisableMemberHistoryEntryCommand, Guild>
{
    private readonly IGuildDataService _guildDataService;
    private readonly IDiscordService _discord;

    public DisableMemberHistoryEntryCommandHandler(IGuildDataService guildDataService, IDiscordService discord)
    {
        _guildDataService = guildDataService;
        _discord = discord;
    }
    
    public async Task<Result<Guild>> HandleAsync(DisableMemberHistoryEntryCommand historyEntryCommand, CancellationToken cancellationToken = default)
    {
        var guildRes =
            await _guildDataService.GetSingleBySpecAsync(
                new ActiveGuildByDiscordIdWithMembersAndBoostsSpec(historyEntryCommand.Guild.Id, historyEntryCommand.Member.Id));

        if (!guildRes.IsDefined(out var guildCfg))
            return Result<Guild>.FromError(guildRes);

        DiscordAuditLogEntry? auditLogEntry = null;

        if (guildCfg.IsModerationModuleEnabled && historyEntryCommand.Guild.Channels.TryGetValue(guildCfg.ModerationConfig.MemberEventsLogChannelId,
                out var logChannel))
        {
            var auditLogsBans = await historyEntryCommand.Guild.GetAuditLogsAsync(1, null, AuditLogActionType.Ban);
            await Task.Delay(500);
            var auditLogsKicks = await historyEntryCommand.Guild.GetAuditLogsAsync(1, null, AuditLogActionType.Kick);
            var filtered = auditLogsBans.Concat(auditLogsKicks).Where(m =>
                m.CreationTimestamp.UtcDateTime > DateTime.UtcNow.Subtract(new TimeSpan(0, 0, 4))).ToList();

            var embed = new DiscordEmbedBuilder();

            string? reasonLeft = null;

            if (filtered.Count != 0)
            {
                var auditLog = filtered[0];
                var logType = auditLog.ActionType;
                var userResponsible = auditLog.UserResponsible.Mention;
                reasonLeft = logType switch
                {
                    AuditLogActionType.Ban =>
                        $"Banned by {userResponsible} {(string.IsNullOrEmpty(auditLog.Reason) ? "" : $"with reason: {auditLog.Reason}")}",
                    AuditLogActionType.Kick =>
                        $"Kicked by {userResponsible} {(string.IsNullOrEmpty(auditLog.Reason) ? "" : $"with reason: {auditLog.Reason}")}",
                    _ => reasonLeft
                };

                if (logType is AuditLogActionType.Ban or AuditLogActionType.Kick)
                    auditLogEntry = auditLog;
            }

            embed.WithThumbnail(historyEntryCommand.Member.AvatarUrl);
            embed.WithTitle("Member has left the guild");
            embed.AddField("Member's identity", $"{historyEntryCommand.Member.GetFullUsername()}", true);
            embed.AddField("Member's mention", $"{historyEntryCommand.Member.Mention}", true);
            embed.AddField("Member's ID and profile",
                $"[{historyEntryCommand.Member.Id}](https://discordapp.com/users/{historyEntryCommand.Member.Id})", true);
            embed.AddField("Joined guild", $"{historyEntryCommand.Member.JoinedAt.ToString(CultureInfo.CurrentCulture)}");
            embed.AddField("Account created", $"{historyEntryCommand.Member.CreationTimestamp.ToString(CultureInfo.CurrentCulture)}");
            embed.WithColor(new DiscordColor(guildCfg.EmbedHexColor));
            embed.WithFooter($"Member's User ID: {historyEntryCommand.Member.Id}");

            if (reasonLeft is not null) 
                embed.AddField("Reason for leaving", reasonLeft);

            try
            {
                await _discord.Client.SendMessageAsync(logChannel, embed.Build());
            }
            catch (Exception)
            {
                // probably should tell idiots to fix channel id in config but idk how so return for now, mebe msg members with admin privs
            }
        }

        _ = _guildDataService.BeginUpdate(guildCfg);
        guildCfg.DisableMemberHistoryEntry(historyEntryCommand.Member.Id, auditLogEntry);
        _ = await _guildDataService.CommitAsync();

        return guildCfg;
    }
}
