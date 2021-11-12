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

using System.Collections.Generic;
using DSharpPlus.Entities;
using Hangfire;
using Lisbeth.Bot.Application.Discord.Helpers;
using Lisbeth.Bot.Application.Enums;
using Lisbeth.Bot.DataAccessLayer.Specifications.RecurringReminder;
using Lisbeth.Bot.DataAccessLayer.Specifications.Reminder;
using MikyM.Discord.Interfaces;

namespace Lisbeth.Bot.Application.Discord.Services;

[UsedImplicitly]
public class DiscordSendReminderService : IDiscordSendReminderService
{
    private readonly IDiscordService _discord;
    private readonly IDiscordEmbedProvider _embedProvider;
    private readonly IRecurringReminderService _recurringReminderService;
    private readonly IReminderService _reminderService;

    public DiscordSendReminderService(IReminderService reminderService,
        IRecurringReminderService recurringReminderService, IDiscordService discord,
        IDiscordEmbedProvider embedProvider)
    {
        _reminderService = reminderService;
        _recurringReminderService = recurringReminderService;
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

        switch (type)
        {
            case ReminderType.Single:
                var rem = await _reminderService.GetSingleBySpecAsync<Reminder>(
                    new ActiveReminderByIdWithEmbedSpec(reminderId));
                if (!rem.IsDefined() || rem.Entity.Guild?.ReminderChannelId is null)
                    return Result.FromError(new NotFoundError());
                guild = rem.Entity.Guild;
                embedConfig = rem.Entity.EmbedConfig;
                text = rem.Entity.Text;
                mentions = rem.Entity.Mentions;
                channelId = rem.Entity.ChannelId;
                break;
            case ReminderType.Recurring:
                var recRem = await _recurringReminderService.GetSingleBySpecAsync<RecurringReminder>(
                    new ActiveRecurringReminderByIdWithEmbedSpec(reminderId));
                if (!recRem.IsDefined() || recRem.Entity.Guild?.ReminderChannelId is null)
                    return Result.FromError(new NotFoundError());
                guild = recRem.Entity.Guild;
                embedConfig = recRem.Entity.EmbedConfig;
                text = recRem.Entity.Text;
                mentions = recRem.Entity.Mentions;
                channelId = recRem.Entity.ChannelId;
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(type), type, null);
        }

        try
        {
            channel = channelId.HasValue ? await _discord.Client.GetChannelAsync(channelId.Value) : await _discord.Client.GetChannelAsync(guild.ReminderChannelId.Value);
        }
        catch (Exception)
        {
            // ignore
            return Result.FromError(new DiscordNotFoundError(DiscordEntityType.Channel));
        }

        if (embedConfig is not null)
            await channel.SendMessageAsync(string.Join(' ', mentions ?? throw new InvalidOperationException()),
                _embedProvider.ConfigureEmbed(embedConfig).Build());
        else
            await channel.SendMessageAsync(string.Join(' ', mentions ?? throw new InvalidOperationException()) + "\n\n" + text);

        return Result.FromSuccess();
    }
}