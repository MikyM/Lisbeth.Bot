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

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using FluentValidation;
using JetBrains.Annotations;
using Lisbeth.Bot.Application.Discord.Services.Interfaces;
using Lisbeth.Bot.Application.Extensions;
using Lisbeth.Bot.Application.Validation.Reminder;
using Lisbeth.Bot.Domain.DTOs.Request.Reminder;
using Lisbeth.Bot.Domain.Entities;
using Lisbeth.Bot.Domain.Enums;
using NCrontab;

namespace Lisbeth.Bot.Application.Discord.SlashCommands
{
    [UsedImplicitly]
    [SlashModuleLifespan(SlashModuleLifespan.Transient)]
    [SlashCommandGroup("reminder", "Reminder commands")]
    public class ReminderSlashCommands : ApplicationCommandModule
    {
        public IDiscordReminderService _reminderService { private get; set; }
        public IDiscordEmbedConfiguratorService<Reminder> _reminderEmbedConfiguratorService { private get; set; }
        public IDiscordEmbedConfiguratorService<RecurringReminder> _recurringReminderEmbedConfiguratorService { private get; set; }

        [UsedImplicitly]
        [SlashCommand("single", "Single reminder command")]
        public async Task SingleReminderCommand(InteractionContext ctx,
            [Option("action", "Action to perform")]
            ReminderActionType actionType,
            [Option("reminder-type", "Type of the reminder")]
            ReminderType reminderType,
            [Option("for-time", "Datetime, cron expression or string representation")]
            string time,
            [Option("text", "What to remind about")]
            string text,
            [Option("mentions", "Role, channel or user mentions - defaults to creator mention")]
            string mentions = "",
            [Option("name-or-id", "Reminder's name")]
            string name = "")
        {
            await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);
            DiscordEmbed embed;

            bool isValidDateTime = DateTime.TryParseExact(time, "dd/MM/yyyy hh:mm tt", DateTimeFormatInfo.InvariantInfo,
                DateTimeStyles.None, out DateTime parsedDateTime);
            bool isValidStringRep = time.TryParseToDurationAndNextOccurrence(out _, out _);
            bool isValidCron = true;
            try
            {
                CrontabSchedule.TryParse(time);
            }
            catch
            {
                isValidCron = false;
            }

            if (mentions.Length > 500) throw new ArgumentException(nameof(mentions));

            var mentionList = mentions is ""
                ? new List<string> {ctx.Member.Mention}
                : Regex.Matches(mentions, @"\<[^<>]*\>").Select(m => m.Value).ToList();
            mentionList.RemoveAll(string.IsNullOrWhiteSpace);

            switch (actionType)
            {
                case ReminderActionType.Set or ReminderActionType.Reschedule
                    when !isValidCron && !isValidDateTime && !isValidStringRep:
                    throw new ArgumentException("Couldn't parse given time representation", nameof(time));
                case ReminderActionType.Set or ReminderActionType.Reschedule when reminderType is ReminderType.Single &&
                    isValidCron && !isValidDateTime && !isValidStringRep:
                    throw new ArgumentException("Single reminders can't take a cron expression as an argument",
                        nameof(time));
                case ReminderActionType.Set or ReminderActionType.Reschedule
                    when reminderType is ReminderType.Recurring && !isValidCron && isValidDateTime && isValidStringRep:
                    throw new ArgumentException("Recurring reminders only accept a cron expression as an argument",
                        nameof(time));
                default:
                    switch (actionType)
                    {
                        case ReminderActionType.Set:
                            var setReq = new SetReminderReqDto(name, isValidCron ? time : null,
                                isValidDateTime ? parsedDateTime : null, isValidStringRep ? time : null, text,
                                mentionList, ctx.Guild.Id, ctx.Member.Id);
                            var setReqValidator = new SetReminderReqValidator(ctx.Client);
                            await setReqValidator.ValidateAndThrowAsync(setReq);
                            embed = await _reminderService.SetNewReminderAsync(ctx, setReq);
                            break;
                        case ReminderActionType.Reschedule:
                            var rescheduleReq = new RescheduleReminderReqDto(name, isValidCron ? time : null,
                                isValidDateTime ? parsedDateTime : null, isValidStringRep ? time : null, ctx.Guild.Id,
                                ctx.Member.Id, name.IsDigitsOnly() ? long.Parse(name) : null);
                            var rescheduleReqValidator = new RescheduleReminderReqValidator(ctx.Client);
                            await rescheduleReqValidator.ValidateAndThrowAsync(rescheduleReq);
                            embed = await _reminderService.RescheduleReminderAsync(ctx, rescheduleReq);
                            break;
                        case ReminderActionType.ConfigureEmbed:
                            switch (reminderType)
                            {
                                case ReminderType.Single:
                                    var res = await _reminderEmbedConfiguratorService.ConfigureAsync(ctx, name);
                                    await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(res.Embed));
                                    return;
                                case ReminderType.Recurring:
                                    var resRec =
                                        await _recurringReminderEmbedConfiguratorService.ConfigureAsync(ctx, name);
                                    await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(resRec.Embed));
                                    return;
                                default:
                                    throw new ArgumentOutOfRangeException(nameof(reminderType), reminderType, null);
                            }
                        case ReminderActionType.Disable:
                            var disableReq = new DisableReminderReqDto(reminderType, name, ctx.Guild.Id, ctx.Member.Id,
                                name.IsDigitsOnly() ? long.Parse(name) : null);
                            var disableReqValidator = new DisableReminderReqValidator(ctx.Client);
                            await disableReqValidator.ValidateAndThrowAsync(disableReq);
                            embed = await _reminderService.DisableReminderAsync(ctx, disableReq);
                            break;
                        default:
                            throw new ArgumentOutOfRangeException(nameof(actionType), actionType, null);
                    }

                    break;
            }

            await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(embed));
        }
    }
}