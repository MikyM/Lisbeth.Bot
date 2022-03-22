// This file is part of Lisbeth.Bot project
//
// Copyright (C) 2021 Krzysztof Kupisz - MikyM
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

using DSharpPlus.Entities;
using Lisbeth.Bot.DataAccessLayer.Specifications.Guild;
using MikyM.Common.Utilities.Results;
using MikyM.Common.Utilities.Results.Errors;
using MikyM.Discord.EmbedBuilders.Enums;
using MikyM.Discord.Enums;
using MikyM.Discord.Interfaces;

namespace Lisbeth.Bot.Application.Discord.Services;

[UsedImplicitly]
[Service]
[RegisterAs(typeof(IDiscordGuildLogSenderService))]
[Lifetime(Lifetime.InstancePerLifetimeScope)]
public class DiscordGuildLogSenderService : IDiscordGuildLogSenderService
{
    private readonly IGuildDataService _guildDataService;
    private readonly IDiscordService _discordService;

    public DiscordGuildLogSenderService(IGuildDataService guildDataService, IDiscordService discordService)
    {
        _guildDataService = guildDataService;
        _discordService = discordService;
    }

    public async Task<Result> SendAsync(Guild guild, DiscordLog type, DiscordEmbed embed)
    {
        return await this.SendAsync(guild.GuildId, type, embed);
    }

    public async Task<Result> SendAsync(long guildId, DiscordLog type, DiscordEmbed embed)
    {
        var guildRes =
            await _guildDataService.GetAsync(guildId);

        if (!guildRes.IsDefined()) return new NotFoundError();

        return await this.SendAsync(guildRes.Entity.GuildId, type, embed);
    }

    public async Task<Result> SendAsync(DiscordGuild discordGuild, DiscordLog type, DiscordEmbed embed)
    {
        var guildRes =
            await _guildDataService.GetSingleBySpecAsync(new ActiveGuildByDiscordIdWithModerationSpec(discordGuild.Id));

        if (!guildRes.IsDefined()) return Result.FromError(guildRes);
        if (guildRes.Entity.ModerationConfig is null || guildRes.Entity.ModerationConfig.IsDisabled)
            return new DisabledEntityError(nameof(guildRes.Entity.ModerationConfig));

        DiscordChannel? target;

        try
        {
            target = type switch
            {
                DiscordLog.MemberAdded => discordGuild.Channels[guildRes.Entity.ModerationConfig.MemberEventsLogChannelId],
                DiscordLog.MemberRemoved => discordGuild.Channels[guildRes.Entity.ModerationConfig.MemberEventsLogChannelId],
                DiscordLog.MessageDeleted => discordGuild.Channels[
                    guildRes.Entity.ModerationConfig.MessageDeletedEventsLogChannelId],
                DiscordLog.MessageUpdated => discordGuild.Channels[
                    guildRes.Entity.ModerationConfig.MessageUpdatedEventsLogChannelId],
                DiscordLog.Moderation => discordGuild.Channels[guildRes.Entity.ModerationConfig.ModerationLogChannelId],
                _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
            };
        }
        catch (Exception)
        {
            return new DiscordError($"Couldn't find the proper log channel.");
        }

        if (target is null) return new DiscordNotFoundError();

        try
        {
            await target.SendMessageAsync(embed);
        }
        catch (Exception)
        {
            return new DiscordError($"Couldn't send the log to the proper log channel.");
        }

        return Result.FromSuccess();
    }

    public async Task<Result> SendAsync(ulong discordGuildId, DiscordLog type, DiscordEmbed embed)
    {
        if (_discordService.Client.Guilds.TryGetValue(discordGuildId, out var guild)) return new DiscordNotFoundError(DiscordEntity.Guild);

        return await this.SendAsync(guild ?? throw new InvalidOperationException("Guild was null."), type, embed);
    }
}