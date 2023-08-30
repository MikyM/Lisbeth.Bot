// This file is part of Lisbeth.Bot project
//
// Copyright (C) 2021-2022 Krzysztof Kupisz - MikyM
// 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU Affero General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Affero General Public License for more details.
// 
// You should have received a copy of the GNU Affero General Public License
// along with this program.  If not, see <https://www.gnu.org/licenses/>.

using Lisbeth.Bot.Application.Discord.Commands.Mute;
using Lisbeth.Bot.Application.Discord.Commands.Timeout;
using Lisbeth.Bot.Domain.DTOs.Request.Mute;

namespace Lisbeth.Bot.Application.Discord.CommandHandlers.Timeout;

[UsedImplicitly]
public class LogTimeoutCommandHandler : IAsyncCommandHandler<LogTimeoutCommand>
{
    private readonly IDiscordService _discord;
    private readonly IAsyncCommandHandler<ApplyMuteCommand, DiscordEmbed> _applyMuteHandler;
    private readonly IAsyncCommandHandler<RevokeMuteCommand, DiscordEmbed> _revokeMuteHandler;

    public LogTimeoutCommandHandler(IAsyncCommandHandler<ApplyMuteCommand, DiscordEmbed> applyMuteHandler,
        IAsyncCommandHandler<RevokeMuteCommand, DiscordEmbed> revokeMuteHandler, IDiscordService discord)
    {
        _applyMuteHandler = applyMuteHandler;
        _revokeMuteHandler = revokeMuteHandler;
        _discord = discord;
    }

    public async Task<Result> HandleAsync(LogTimeoutCommand command, CancellationToken cancellationToken = default)
    {
        if (command is null) throw new ArgumentNullException(nameof(command));

        if (!command.CommunicationDisabledUntilAfter.HasValue && !command.CommunicationDisabledUntilBefore.HasValue ||
            command.CommunicationDisabledUntilAfter.HasValue && command.CommunicationDisabledUntilBefore.HasValue)
            return Result.FromSuccess();

        var wasTimeoutApplied = !command.CommunicationDisabledUntilBefore.HasValue && command.CommunicationDisabledUntilAfter.HasValue;
        var wasTimeoutRevoked = command.CommunicationDisabledUntilBefore.HasValue && !command.CommunicationDisabledUntilAfter.HasValue;

        var auditLogs = await command.Member.Guild.GetAuditLogsAsync(1, null, AuditLogActionType.MemberUpdate);
        await Task.Delay(500);
        var filtered = auditLogs.Where(m =>
            m.CreationTimestamp.UtcDateTime > DateTime.UtcNow.Subtract(new TimeSpan(0, 0, 4)))
            .OrderByDescending(x => x.CreationTimestamp).ToList();
        var requestingUser = filtered.Count == 0
            ? _discord.Client.CurrentUser
            : filtered.First().UserResponsible ?? _discord.Client.CurrentUser;
        var reason = filtered.Count == 0
            ? null
            : filtered.First().Reason ?? null;

        if (wasTimeoutApplied)
        {
            if (!command.CommunicationDisabledUntilAfter.HasValue) return Result.FromSuccess();
            var applyDto =
                new MuteApplyReqDto
                {
                    GuildId = command.Member.Guild.Id,
                    RequestedOnBehalfOfId = requestingUser.Id,
                    TargetUserId = command.Member.Id,
                    Reason = reason is "" or " " ? null : reason,
                    AppliedUntil = command.CommunicationDisabledUntilAfter.Value.UtcDateTime
                };
            await _applyMuteHandler.HandleAsync(new ApplyMuteCommand(applyDto));
        }
        else if (wasTimeoutRevoked)
        {
            var revokeDto =
                new MuteRevokeReqDto
                {
                    GuildId = command.Member.Guild.Id,
                    RequestedOnBehalfOfId = requestingUser.Id,
                    TargetUserId = command.Member.Id
                };
            await _revokeMuteHandler.HandleAsync(new RevokeMuteCommand(revokeDto));
        }

        return Result.FromSuccess();
    }
}
