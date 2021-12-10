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

using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using FluentValidation;
using Lisbeth.Bot.Application.Discord.SlashCommands.Base;
using Lisbeth.Bot.Application.Validation.Reminder;
using Lisbeth.Bot.Domain.DTOs.Request.Reminder;
using NCrontab;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;

namespace Lisbeth.Bot.Application.Discord.SlashCommands;

[UsedImplicitly]
[SlashModuleLifespan(SlashModuleLifespan.Transient)]
public class ReminderSlashCommands : ExtendedApplicationCommandModule
{
    public ReminderSlashCommands(IDiscordReminderService reminderService,
        IDiscordEmbedConfiguratorService<Reminder> reminderEmbedConfiguratorService,
        IDiscordEmbedConfiguratorService<RecurringReminder> recurringReminderEmbedConfiguratorService)
    {
        _reminderService = reminderService;
        _reminderEmbedConfiguratorService = reminderEmbedConfiguratorService;
        _recurringReminderEmbedConfiguratorService = recurringReminderEmbedConfiguratorService;
    }

    private readonly IDiscordReminderService _reminderService;
    private readonly IDiscordEmbedConfiguratorService<Reminder> _reminderEmbedConfiguratorService;
    private readonly IDiscordEmbedConfiguratorService<RecurringReminder> _recurringReminderEmbedConfiguratorService;

    [UsedImplicitly]
    [SlashCommand("reminder", "Command that allows working with reminders.", false)]
    public async Task SingleReminderCommand(InteractionContext ctx,
        [Option("action", "Action to perform")]
        ReminderActionType actionType,
        [Option("reminder-type", "Type of the reminder")]
        Domain.Enums.ReminderType reminderType,
        [Option("text", "What to remind about")]
        string text = "default",
        [Option("for-time", "Datetime, cron expression or string a representation")]
        string time = "",
        [Option("mentions", "Role, channel or user mentions - defaults to creator mention")]
        string mentions = "",
        [Option("name-or-id", "Reminder's name")]
        string name = "",
        [Option("channel", "Channel to send the message to")]
        DiscordChannel? channel = null)
    {
        await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);
        Result<DiscordEmbed> result;

        bool isValidDateTime = DateTime.TryParseExact(time, "dd/MM/yyyy hh:mm tt", DateTimeFormatInfo.InvariantInfo,
            DateTimeStyles.None, out DateTime parsedDateTime);
        bool isValidTime = DateTime.TryParseExact(time, "hh:mm tt", DateTimeFormatInfo.InvariantInfo,
            DateTimeStyles.None, out DateTime parsedTime);
        bool isValidStringRep = time.TryParseToDurationAndNextOccurrence(out _, out _);
        bool isValidCron = true;
        string exMessage = "";

        try
        {
            CrontabSchedule.Parse(time);
        }
        catch (Exception ex)
        {
            exMessage = ex.Message;
            try
            {
                CrontabSchedule.Parse(time, new CrontabSchedule.ParseOptions{IncludingSeconds = true});
            }
            catch (Exception inner)
            {
                isValidCron = false;
                exMessage = exMessage + "Error while trying to parse including seconds: " + inner.Message;
            }
        }
            
        if (mentions.Length > 500) throw new ArgumentException(nameof(mentions));

        var mentionList = mentions is ""
            ? new List<string> { ctx.Member.Mention }
            : Regex.Matches(mentions, @"\<[^<>]*\>").Select(m => m.Value).ToList();
        mentionList.RemoveAll(string.IsNullOrWhiteSpace);

        switch (actionType)
        {
            case ReminderActionType.Set or ReminderActionType.Reschedule
                when !isValidCron && !isValidDateTime && !isValidStringRep && !isValidTime:
                throw new ArgumentException("Couldn't parse given time representation", nameof(time));
            case ReminderActionType.Set or ReminderActionType.Reschedule when reminderType is Domain.Enums.ReminderType.Single &&
                                                                              isValidCron && !isValidDateTime && !isValidStringRep && !isValidTime:
                throw new ArgumentException("Single reminders can't take a cron expression as an argument",
                    nameof(time));
            case ReminderActionType.Set or ReminderActionType.Reschedule
                when reminderType is Domain.Enums.ReminderType.Recurring && !isValidCron && (isValidDateTime || isValidStringRep || isValidTime):
                throw new ArgumentException($"Recurring reminders only accepts a valid cron expression as an argument. \n {exMessage}",
                    nameof(time));
            default:
                switch (actionType)
                {
                    case ReminderActionType.Set:
                        if (reminderType is Domain.Enums.ReminderType.Single && name is "") name = $"{ctx.Guild.Id}_{ctx.User.Id}_{DateTime.UtcNow}";

                        var setReq = new SetReminderReqDto(name, isValidCron && !isValidDateTime && !isValidTime ? time : null,
                            isValidDateTime ? parsedDateTime : isValidTime ? global::System.DateTime.UtcNow.Date.Add(parsedTime.TimeOfDay) : null, isValidStringRep ? time : null, text,
                            mentionList, ctx.Guild.Id, ctx.Member.Id, channel?.Id);
                        var setReqValidator = new SetReminderReqValidator(ctx.Client);
                        await setReqValidator.ValidateAndThrowAsync(setReq);
                        result = await this._reminderService!.SetNewReminderAsync(ctx, setReq);
                        break;
                    case ReminderActionType.Reschedule:
                        var rescheduleReq = new RescheduleReminderReqDto(name, isValidCron ? time : null,
                            isValidDateTime ? parsedDateTime : null, isValidStringRep ? time : null, ctx.Guild.Id,
                            ctx.Member.Id, name.IsDigitsOnly() ? long.Parse(name) : null);
                        var rescheduleReqValidator = new RescheduleReminderReqValidator(ctx.Client);
                        await rescheduleReqValidator.ValidateAndThrowAsync(rescheduleReq);
                        result = await this._reminderService!.RescheduleReminderAsync(ctx, rescheduleReq);
                        break;
                    case ReminderActionType.ConfigureEmbed:
                        switch (reminderType)
                        {
                            case Domain.Enums.ReminderType.Single:
                                var res = await this._reminderEmbedConfiguratorService!.ConfigureAsync(ctx, name);
                                if (res.IsDefined())
                                    await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(res.Entity));
                                else
                                    await ctx.EditResponseAsync(
                                        new DiscordWebhookBuilder().AddEmbed(
                                            GetUnsuccessfulResultEmbed(res, ctx.Client)));
                                return;
                            case Domain.Enums.ReminderType.Recurring:
                                var resRec =
                                    await this._recurringReminderEmbedConfiguratorService!.ConfigureAsync(ctx, name);
                                if (resRec.IsDefined())
                                    await ctx.EditResponseAsync(
                                        new DiscordWebhookBuilder().AddEmbed(resRec.Entity));
                                else
                                    await ctx.EditResponseAsync(
                                        new DiscordWebhookBuilder().AddEmbed(
                                            GetUnsuccessfulResultEmbed(resRec, ctx.Client)));
                                return;
                            default:
                                throw new ArgumentOutOfRangeException(nameof(reminderType), reminderType, null);
                        }
                    case ReminderActionType.Disable:
                        var disableReq = new DisableReminderReqDto(reminderType, name, ctx.Guild.Id, ctx.Member.Id,
                            name.IsDigitsOnly() ? long.Parse(name) : null);
                        var disableReqValidator = new DisableReminderReqValidator(ctx.Client);
                        await disableReqValidator.ValidateAndThrowAsync(disableReq);
                        result = await this._reminderService!.DisableReminderAsync(ctx, disableReq);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(actionType), actionType, null);
                }

                break;
        }

        if (result.IsDefined()) await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(result.Entity));
        else
            await ctx.EditResponseAsync(
                new DiscordWebhookBuilder().AddEmbed(base.GetUnsuccessfulResultEmbed(result, ctx.Client)));
    }

    [UsedImplicitly]
    [SlashCommand("cron-help", "Single reminder command", false)]
    public async Task SingleReminderCommand(InteractionContext ctx,
        [Option("cron-expression", "Crone to parse")]
        string cron)
    {
        await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);

        var schedule = CrontabSchedule.TryParse(cron);
        var scheduleOpt = CrontabSchedule.TryParse(cron, new CrontabSchedule.ParseOptions { IncludingSeconds = true });
        var scheduleOptN = CrontabSchedule.TryParse(cron, new CrontabSchedule.ParseOptions { IncludingSeconds = false });

        await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent(
            $"Schedule: {(schedule is null ? "null" : $"{schedule.GetNextOccurrence(DateTime.UtcNow)}")} \n\n Schedule: {(scheduleOpt is null ? "null" : $"{scheduleOpt.GetNextOccurrence(DateTime.UtcNow)}")} \n\n Schedule: {(scheduleOptN is null ? "null" : $"{scheduleOptN.GetNextOccurrence(DateTime.UtcNow)}")}"));
    }
}
