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

using System.Collections.Generic;
using DSharpPlus.Entities;
using Hangfire;
using Lisbeth.Bot.Application.Discord.Helpers;
using Lisbeth.Bot.DataAccessLayer.Specifications.RecurringReminder;
using Lisbeth.Bot.DataAccessLayer.Specifications.Reminder;
using MikyM.Common.Utilities.Results;
using MikyM.Common.Utilities.Results.Errors;
using MikyM.Common.Utilities.Results.Errors.Bases;
using MikyM.Discord.Enums;
using MikyM.Discord.Interfaces;

namespace Lisbeth.Bot.Application.Discord.Services;

[UsedImplicitly]
[Service]
[RegisterAs(typeof(IDiscordSendReminderService))]
[Lifetime(Lifetime.InstancePerLifetimeScope)]
public class DiscordSendReminderService : IDiscordSendReminderService
{
    private readonly IDiscordService _discord;
    private readonly IDiscordEmbedProvider _embedProvider;
    private readonly IReminderDataService _reminderDataDataService;

    public DiscordSendReminderService(IReminderDataService reminderDataDataService, IDiscordService discord,
        IDiscordEmbedProvider embedProvider)
    {
        _reminderDataDataService = reminderDataDataService;
        _discord = discord;
        _embedProvider = embedProvider;
    }

    [Queue("reminder")]
    [PreserveOriginalQueue]
    public async Task<Result> SendReminderAsync(long reminderId, ReminderType type)
    {
        Guild guild;
        EmbedConfig? embedConfig;
        string? text;
        List<string>? mentions;
        DiscordChannel channel;
        ulong? channelId;
        DiscordGuild discordGuild;
        ulong guildId;

        switch (type)
        {
            case ReminderType.Single:
                var rem = await _reminderDataDataService.GetSingleBySpecAsync(new ActiveReminderByIdWithEmbedSpec(reminderId));
                if (!rem.IsDefined() || rem.Entity.Guild?.ReminderChannelId is null)
                    return Result.FromError(new NotFoundError());
                guild = rem.Entity.Guild;
                embedConfig = rem.Entity.EmbedConfig;
                text = rem.Entity.Text;
                mentions = rem.Entity.Mentions;
                channelId = rem.Entity.ChannelId;
                guildId = rem.Entity.GuildId;
                break;
            case ReminderType.Recurring:
                var recRem = await _reminderDataDataService.GetSingleBySpecAsync(
                    new ActiveRecurringReminderByIdWithEmbedSpec(reminderId));
                if (!recRem.IsDefined() || recRem.Entity.Guild?.ReminderChannelId is null)
                    return Result.FromError(new NotFoundError());
                guild = recRem.Entity.Guild;
                embedConfig = recRem.Entity.EmbedConfig;
                text = recRem.Entity.Text;
                mentions = recRem.Entity.Mentions;
                channelId = recRem.Entity.ChannelId;
                guildId = recRem.Entity.GuildId;
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(type), type, null);
        }

        try
        {
            discordGuild = _discord.Client.Guilds[guildId];
        }
        catch (Exception)
        {
            return Result.FromError(new DiscordNotFoundError(DiscordEntity.Guild));
        }

        try
        {
            if (channelId.HasValue)
                channel = discordGuild.GetChannel(channelId.Value);
            else if (!guild.ReminderChannelId.HasValue)
                return new ArgumentError(nameof(guild.ReminderChannelId),"Guild doesn't have a set reminder channel");
            else 
                channel = discordGuild.GetChannel(guild.ReminderChannelId.Value);

            if (channel is null)
                return Result.FromError(new DiscordNotFoundError(DiscordEntity.Channel));
        }
        catch (Exception)
        {
            return Result.FromError(new DiscordNotFoundError(DiscordEntity.Channel));
        }

        if (embedConfig is not null)
            await channel.SendMessageAsync(string.Join(' ', mentions ?? throw new InvalidOperationException()),
                _embedProvider.GetEmbedFromConfig(embedConfig).Build());
        else
            await channel.SendMessageAsync(string.Join(' ', mentions ?? throw new InvalidOperationException()) + "\n\n" + text);

        return Result.FromSuccess();
    }
}