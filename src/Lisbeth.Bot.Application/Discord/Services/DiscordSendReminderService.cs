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
    private readonly IReminderDataService _reminderDataService;

    public DiscordSendReminderService(IReminderDataService reminderDataService, IDiscordService discord,
        IDiscordEmbedProvider embedProvider)
    {
        _reminderDataService = reminderDataService;
        _discord = discord;
        _embedProvider = embedProvider;
    }

    [Queue("reminder")]
    [PreserveOriginalQueue]
    public async Task<Result> SendReminderAsync(long reminderId, ReminderType type)
    {
        Reminder? reminder;
        DiscordGuild discordGuild;
        DiscordChannel channel;

        switch (type)
        {
            case ReminderType.Single:
                var rem = await _reminderDataService.GetSingleBySpecAsync(new ActiveReminderByIdWithEmbedSpec(reminderId));
                if (!rem.IsDefined(out reminder) || reminder.Guild?.ReminderChannelId is null)
                    return Result.FromError(new NotFoundError());
                break;
            case ReminderType.Recurring:
                var recRem = await _reminderDataService.GetSingleBySpecAsync(
                    new ActiveRecurringReminderByIdWithEmbedSpec(reminderId));
                if (!recRem.IsDefined(out reminder) || reminder.Guild?.ReminderChannelId is null)
                    return Result.FromError(new NotFoundError());
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(type), type, null);
        }

        try
        {
            discordGuild = _discord.Client.Guilds[reminder.GuildId];
        }
        catch (Exception)
        {
            return Result.FromError(new DiscordNotFoundError(DiscordEntity.Guild));
        }

        try
        {
            if (reminder.ChannelId.HasValue)
                channel = discordGuild.GetChannel(reminder.ChannelId.Value);
            else if (!reminder.Guild.ReminderChannelId.HasValue)
                return new ArgumentError(nameof(reminder.Guild.ReminderChannelId),"Guild doesn't have a set reminder channel");
            else 
                channel = discordGuild.GetChannel(reminder.Guild.ReminderChannelId.Value);

            if (channel is null)
                return Result.FromError(new DiscordNotFoundError(DiscordEntity.Channel));
        }
        catch (Exception)
        {
            return Result.FromError(new DiscordNotFoundError(DiscordEntity.Channel));
        }

        var mentions = reminder.Mentions ?? new List<string> { ExtendedFormatter.Mention(reminder.CreatorId, DiscordEntity.Member) };
        var prefixText = reminder.ShouldAddCreationInfo
            ? $"Oi {string.Join(' ', mentions)}, you asked to be reminded about:"
            : string.Join(' ', mentions);
        var suffixText = reminder.ShouldAddCreationInfo
            ? $"This reminder was created at {reminder.CreatedAt?.ToString("dd/MM/yyyy hh:mm tt")} UTC"
            : string.Empty;

        if (reminder.EmbedConfig is not null)
            await channel.SendMessageAsync(prefixText + $"{(reminder.ShouldAddCreationInfo ? "\n" : string.Empty)}" + suffixText,
                _embedProvider.GetEmbedFromConfig(reminder.EmbedConfig).Build());
        else
            await channel.SendMessageAsync(
                prefixText + "\n\n" + reminder.Text + $"{(reminder.ShouldAddCreationInfo ? "\n\n" : string.Empty)}" + suffixText);

        if (type is ReminderType.Single)
            await _reminderDataService.DisableAsync(reminder, true);

        return Result.FromSuccess();
    }
}