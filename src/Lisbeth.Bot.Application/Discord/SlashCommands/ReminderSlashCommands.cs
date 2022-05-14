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
using System.Globalization;
using System.Text.RegularExpressions;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using FluentValidation;
using Lisbeth.Bot.Application.Discord.Commands.Reminder;
using Lisbeth.Bot.Application.Discord.SlashCommands.Base;
using Lisbeth.Bot.Application.Validation.Reminder;
using Lisbeth.Bot.Domain.DTOs.Request.Reminder;
using MikyM.CommandHandlers;
using MikyM.Common.Utilities.Results;
using NCrontab;

namespace Lisbeth.Bot.Application.Discord.SlashCommands;

[UsedImplicitly]
[SlashModuleLifespan(SlashModuleLifespan.Scoped)]
public class ReminderSlashCommands : ExtendedApplicationCommandModule
{
    public ReminderSlashCommands(ICommandHandler<SetNewReminderCommand, DiscordEmbed> setNewHandler,
        ICommandHandler<RescheduleReminderCommand, DiscordEmbed> rescheduleHandler,
        ICommandHandler<DisableReminderCommand, DiscordEmbed> disableHandler,
        IDiscordEmbedConfiguratorService<Reminder> reminderEmbedConfiguratorService)
    {
        _setNewHandler = setNewHandler;
        _rescheduleHandler = rescheduleHandler;
        _disableHandler = disableHandler;
        _reminderEmbedConfiguratorService = reminderEmbedConfiguratorService;
    }

    private readonly ICommandHandler<SetNewReminderCommand, DiscordEmbed> _setNewHandler;
    private readonly ICommandHandler<RescheduleReminderCommand, DiscordEmbed> _rescheduleHandler;
    private readonly ICommandHandler<DisableReminderCommand, DiscordEmbed> _disableHandler;
    private readonly IDiscordEmbedConfiguratorService<Reminder> _reminderEmbedConfiguratorService;

    [UsedImplicitly]
    [SlashCommand("reminder", "Command that allows working with reminders.")]
    public async Task SingleReminderCommand(InteractionContext ctx,
        [Option("action", "Action to perform")]
        ReminderActionType actionType,
        [Option("reminder-type", "Type of the reminder")]
        ReminderType reminderType,
        [Option("text", "What to remind about")]
        string text = "default",
        [Option("time", "Datetime, cron expression or a string representation of time (1h, 1d etc)")]
        string time = "",
        [Option("mentions", "Role, channel or user mentions - defaults to creator mention")]
        string mentions = "",
        [Option("name", "Reminder's name")]
        string name = "",
        [Option("channel", "Channel to send the message to")]
        DiscordChannel? channel = null)
    {
        await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource,
            new DiscordInteractionResponseBuilder().AsEphemeral());
        Result<DiscordEmbed> result;

        /*
        bool isValidDateTime = DateTime.TryParseExact(time, "d/M/yyyy hh:mm tt", DateTimeFormatInfo.InvariantInfo,
            DateTimeStyles.None, out DateTime parsedDateTime);*/
        bool isValidDateTime = DateTime.TryParse(time, DateTimeFormatInfo.InvariantInfo,
            DateTimeStyles.None, out DateTime parsedDateTime);
        bool isValidTime = DateTime.TryParse(time, DateTimeFormatInfo.InvariantInfo,
            DateTimeStyles.None, out DateTime parsedTime);
        /*if (!isValidTime) isValidTime = DateTime.TryParse(time, DateTimeFormatInfo.InvariantInfo,
            DateTimeStyles.None, out parsedTime);*/
        bool isValidStringRep = !string.IsNullOrWhiteSpace(time) && text.TryParseToDurationAndNextOccurrence(out _, out _);
        bool isValidCron = true;
        string exMessage = "";

        parsedDateTime = DateTime.SpecifyKind(parsedDateTime, DateTimeKind.Utc);
        parsedTime = DateTime.SpecifyKind(parsedTime, DateTimeKind.Utc);

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
            case ReminderActionType.Set or ReminderActionType.Reschedule when reminderType is ReminderType.Single &&
                                                                              isValidCron && !isValidDateTime && !isValidStringRep && !isValidTime:
                throw new ArgumentException("Single reminders can't take a cron expression as an argument",
                    nameof(time));
            case ReminderActionType.Set or ReminderActionType.Reschedule
                when reminderType is ReminderType.Recurring && !isValidCron && (isValidDateTime || isValidStringRep || isValidTime):
                throw new ArgumentException($"Recurring reminders only accepts a valid cron expression as an argument. \n {exMessage}",
                    nameof(time));
            default:
                switch (actionType)
                {
                    case ReminderActionType.Set:
                        if (reminderType is ReminderType.Single && name is "") name = $"{ctx.Guild.Id}_{ctx.User.Id}_{DateTime.UtcNow.ToString("s")}";

                        var setReq = new SetReminderReqDto(name, isValidCron && !isValidDateTime && !isValidTime ? time : null,
                            isValidDateTime ? parsedDateTime : isValidTime ? DateTime.UtcNow.Date.Add(parsedTime.TimeOfDay) : null, isValidStringRep ? time : null, text,
                            mentionList, ctx.Guild.Id, ctx.Member.Id, channel?.Id);
                        var setReqValidator = new SetReminderReqValidator(ctx.Client);
                        await setReqValidator.ValidateAndThrowAsync(setReq);
                        result = await _setNewHandler.HandleAsync(new SetNewReminderCommand(setReq, ctx));
                        break;
                    case ReminderActionType.Reschedule:
                        var rescheduleReq = new RescheduleReminderReqDto(name, isValidCron ? time : null,
                            isValidDateTime ? parsedDateTime : null, isValidStringRep ? time : null, ctx.Guild.Id,
                            ctx.Member.Id, name.IsDigitsOnly() ? long.Parse(name) : null);
                        var rescheduleReqValidator = new RescheduleReminderReqValidator(ctx.Client);
                        await rescheduleReqValidator.ValidateAndThrowAsync(rescheduleReq);
                        result = await _rescheduleHandler.HandleAsync(new RescheduleReminderCommand(rescheduleReq, ctx));
                        break;
                    case ReminderActionType.ConfigureEmbed:
                        switch (reminderType)
                        {
                            case ReminderType.Single:
                                var res = await _reminderEmbedConfiguratorService.ConfigureAsync(ctx,
                                    x => x.EmbedConfig, x => x.EmbedConfigId, name);
                                if (res.IsDefined())
                                    await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(res.Entity));
                                else
                                    await ctx.EditResponseAsync(
                                        new DiscordWebhookBuilder().AddEmbed(
                                            GetUnsuccessfulResultEmbed(res, ctx.Client)));
                                return;
                            case ReminderType.Recurring:
                                var resRec = await _reminderEmbedConfiguratorService.ConfigureAsync(ctx,
                                    x => x.EmbedConfig, x => x.EmbedConfigId, name);
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
                        result = await _disableHandler.HandleAsync(new DisableReminderCommand(disableReq, ctx));
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(actionType), actionType, null);
                }

                break;
        }

        if (result.IsDefined())
            await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(result.Entity));
        else
            await ctx.FollowUpAsync(new DiscordFollowupMessageBuilder()
                .AddEmbed(GetUnsuccessfulResultEmbed(result, ctx.Client))
                .AsEphemeral());
    }

    [UsedImplicitly]
    [SlashCommand("cron-help", "Command that parses and verifies a given cron expression")]
    public async Task CronHelpCommand(InteractionContext ctx,
        [Option("cron-expression", "Cron to parse")]
        string cron)
    {
        await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource,
            new DiscordInteractionResponseBuilder().AsEphemeral());

        var schedule = CrontabSchedule.TryParse(cron);
        var scheduleOpt = CrontabSchedule.TryParse(cron, new CrontabSchedule.ParseOptions { IncludingSeconds = true });
        var scheduleOptN =
            CrontabSchedule.TryParse(cron, new CrontabSchedule.ParseOptions { IncludingSeconds = false });

        await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent(
            $"Schedule: {(schedule is null ? "null" : $"{schedule.GetNextOccurrence(DateTime.UtcNow)}")} \n\n Schedule: {(scheduleOpt is null ? "null" : $"{scheduleOpt.GetNextOccurrence(DateTime.UtcNow)}")} \n\n Schedule: {(scheduleOptN is null ? "null" : $"{scheduleOptN.GetNextOccurrence(DateTime.UtcNow)}")}"));
    }
}
