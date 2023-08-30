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

using DSharpPlus.SlashCommands.Attributes;
using FluentValidation;
using Lisbeth.Bot.Application.Discord.Commands.ChannelMessageFormat;
using Lisbeth.Bot.Application.Discord.Commands.Modules.Suggestions;
using Lisbeth.Bot.Application.Discord.Commands.Ticket;
using Lisbeth.Bot.Application.Discord.Extensions;
using Lisbeth.Bot.Application.Discord.SlashCommands.Base;
using Lisbeth.Bot.Application.Validation.ChannelMessageFormat;
using Lisbeth.Bot.Application.Validation.ModerationConfig;
using Lisbeth.Bot.Application.Validation.ReminderConfig;
using Lisbeth.Bot.Application.Validation.SuggestionConfig;
using Lisbeth.Bot.Application.Validation.TicketingConfig;
using Lisbeth.Bot.Domain.DTOs.Request.ChannelMessageFormat;
using Lisbeth.Bot.Domain.DTOs.Request.Guild;
using Lisbeth.Bot.Domain.DTOs.Request.ModerationConfig;
using Lisbeth.Bot.Domain.DTOs.Request.ReminderConfig;
using Lisbeth.Bot.Domain.DTOs.Request.SuggestionConfig;
using Lisbeth.Bot.Domain.DTOs.Request.TicketingConfig;

namespace Lisbeth.Bot.Application.Discord.SlashCommands;

[UsedImplicitly]
[SlashCommandGroup("admin-util", "Admin utility commands", false)]
[SlashModuleLifespan(SlashModuleLifespan.Scoped)]
public class AdminUtilSlashCommands : ExtendedApplicationCommandModule
{
    private readonly IDiscordGuildService _discordGuildService;
    private readonly IAsyncCommandHandler<GetTicketCenterEmbedCommand, DiscordMessageBuilder> _discordTicketService;
    private readonly IDiscordEmbedConfiguratorService<TicketingConfig> _embedConfiguratorService;
    private readonly ICommandHandlerResolver _commandHandlerFactory;

    public AdminUtilSlashCommands(IDiscordGuildService discordGuildService,
        IAsyncCommandHandler<GetTicketCenterEmbedCommand, DiscordMessageBuilder> discordTicketService,
        IDiscordEmbedConfiguratorService<TicketingConfig> embedConfiguratorService,
        ICommandHandlerResolver commandHandlerFactory)
    {
        _discordGuildService = discordGuildService;
        _discordTicketService = discordTicketService;
        _embedConfiguratorService = embedConfiguratorService;
        _commandHandlerFactory = commandHandlerFactory;
    }

    [UsedImplicitly]
    [SlashRequireUserPermissions(Permissions.Administrator)]
    [SlashCommand("ticket-center", "Command that allows working with a ticket center message", false)]
    public async Task TicketCenterCommand(InteractionContext ctx, [Option("action", "Action to perform")] TicketCenterActionType action, [Option("channel", "Channel to send the message to")] DiscordChannel? channel = null)
    {
        await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource,
            new DiscordInteractionResponseBuilder().AsEphemeral());

        switch (action)
        {
            case TicketCenterActionType.Get:
                var builderGet = await _discordTicketService.HandleAsync(new GetTicketCenterEmbedCommand(ctx));
                await ctx.Channel.SendMessageAsync(builderGet.Entity);
                await ctx.Interaction.CreateFollowupMessageAsync(new DiscordFollowupMessageBuilder()
                    .AddEmbed(GetSuccessfulActionEmbed(ctx.Client, "Message sent successfully"))
                    .AsEphemeral());
                return;
            case TicketCenterActionType.ConfigureEmbed:
                var partial = await _embedConfiguratorService.ConfigureAsync(ctx, x => x.CenterEmbedConfig,
                    x => x.CenterEmbedConfigId);
                if (!partial.IsDefined(out var embed))
                    await ctx.Interaction.CreateFollowupMessageAsync(new DiscordFollowupMessageBuilder()
                        .AddEmbed(GetUnsuccessfulResultEmbed(partial, ctx.Client))
                        .AsEphemeral());
                else
                    await ctx.Interaction.CreateFollowupMessageAsync(new DiscordFollowupMessageBuilder()
                        .AddEmbed(embed)
                        .AsEphemeral());
                return;
            case TicketCenterActionType.Send:
                if (channel is null)
                {
                    await ctx.Interaction.CreateFollowupMessageAsync(new DiscordFollowupMessageBuilder()
                        .AsEphemeral()
                        .AddEmbed(GetSuccessfulActionEmbed(ctx.Client, "You have to provide a channel")));
                    return;
                }
                var builderSend = await _discordTicketService.HandleAsync(new GetTicketCenterEmbedCommand(ctx));
                await channel.SendMessageAsync(builderSend.Entity);
                await ctx.Interaction.CreateFollowupMessageAsync(new DiscordFollowupMessageBuilder()
                    .AddEmbed(GetSuccessfulActionEmbed(ctx.Client, "Message sent successfully"))
                    .AsEphemeral());
                return;
            default:
                throw new ArgumentOutOfRangeException(nameof(action), action, null);
        }
    }

    [UsedImplicitly]
    [SlashRequireUserPermissions(Permissions.Administrator)]
    [SlashCommand("phishing-detection", "Allows configuring phishing detection", false)]
    public async Task SetPhishingCommand(InteractionContext ctx,
        [Option("phishing-handle", "How to handle detected phishing")]
        PhishingActionType action)
    {
        await ctx.DeferAsync(true);

        var res = action switch
        {
            PhishingActionType.Mute => await _discordGuildService.SetPhishingDetectionAsync(
                new SetPhishingReqDto(PhishingDetection.Mute, ctx.Guild.Id, ctx.User.Id)),
            PhishingActionType.Kick => await _discordGuildService.SetPhishingDetectionAsync(
                new SetPhishingReqDto(PhishingDetection.Kick, ctx.Guild.Id, ctx.User.Id)),
            PhishingActionType.Ban => await _discordGuildService.SetPhishingDetectionAsync(
                new SetPhishingReqDto(PhishingDetection.Ban, ctx.Guild.Id, ctx.User.Id)),
            PhishingActionType.Disable => await _discordGuildService.SetPhishingDetectionAsync(
                new SetPhishingReqDto(PhishingDetection.Disabled, ctx.Guild.Id, ctx.User.Id)),
            _ => throw new ArgumentOutOfRangeException(nameof(action), action, null)
        };

        if (res.IsSuccess)
            await ctx.EditResponseAsync(
                new DiscordWebhookBuilder().AddEmbed(GetSuccessfulActionEmbed(ctx.Client)));
        else
            await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(GetUnsuccessfulResultEmbed(res, ctx.Client)));
    }

    [UsedImplicitly]
    [SlashRequireUserPermissions(Permissions.Administrator)]
    [SlashCommand("message-format", "Command that allows working with channel message formats", false)]
    public async Task ChannelMessageFormatCommand(InteractionContext ctx,
        [Option("action", "Action to perform")] ChannelMessageFormatActionType action,
        [Option("channel", "Channel to configure format for")] DiscordChannel channel,
        [Option("message-id", "Message to verify on demand")] string? messageId = null)
    {
        await ctx.DeferAsync(true);

        switch (action)
        {
            case ChannelMessageFormatActionType.Get:
                var getReq = new GetChannelMessageFormatReqDto
                {
                    ChannelId = channel.Id, GuildId = ctx.Guild.Id, RequestedOnBehalfOfId = ctx.User.Id
                };

                var getReqValidator = new GetMessageFormatReqValidator(ctx.Client);
                await getReqValidator.ValidateAndThrowAsync(getReq);

                var getRes = await _commandHandlerFactory
                    .GetHandler<IAsyncCommandHandler<GetMessageFormatCommand, DiscordEmbed>>()
                    .HandleAsync(new GetMessageFormatCommand(getReq, ctx));

                if (getRes.IsDefined(out var getEmbed))
                    await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(getEmbed));
                else
                    await ctx.EditResponseAsync(
                        new DiscordWebhookBuilder().AddEmbed(GetUnsuccessfulResultEmbed(getRes, ctx.Client)));
                return;
            case ChannelMessageFormatActionType.Create:
                var createReq = new CreateChannelMessageFormatReqDto
                {
                    ChannelId = channel.Id,
                    GuildId = ctx.Guild.Id,
                    RequestedOnBehalfOfId = ctx.User.Id,
                    MessageFormat = "placeholder"
                };

                var createReqValidator = new CreateMessageFormatReqValidator(ctx.Client);
                await createReqValidator.ValidateAndThrowAsync(createReq);

                var createRes = await _commandHandlerFactory
                    .GetHandler<IAsyncCommandHandler<CreateMessageFormatCommand, DiscordEmbed>>()
                    .HandleAsync(new CreateMessageFormatCommand(createReq, ctx));

                if (createRes.IsDefined(out var createEmbed))
                    await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(createEmbed));
                else
                    await ctx.EditResponseAsync(
                        new DiscordWebhookBuilder().AddEmbed(GetUnsuccessfulResultEmbed(createRes, ctx.Client)));
                return;
            case ChannelMessageFormatActionType.Edit:
                var editReq = new EditChannelMessageFormatReqDto
                {
                    ChannelId = channel.Id,
                    GuildId = ctx.Guild.Id,
                    RequestedOnBehalfOfId = ctx.User.Id,
                    MessageFormat = "placeholder"
                };

                var editReqValidator = new EditMessageFormatReqValidator(ctx.Client);
                await editReqValidator.ValidateAndThrowAsync(editReq);

                var editRes = await _commandHandlerFactory
                    .GetHandler<IAsyncCommandHandler<EditMessageFormatCommand, DiscordEmbed>>()
                    .HandleAsync(new EditMessageFormatCommand(editReq, ctx));

                if (editRes.IsDefined(out var editEmbed))
                    await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(editEmbed));
                else
                    await ctx.EditResponseAsync(
                        new DiscordWebhookBuilder().AddEmbed(GetUnsuccessfulResultEmbed(editRes, ctx.Client)));
                return;
            case ChannelMessageFormatActionType.Disable:
                var disableReq = new DisableChannelMessageFormatReqDto
                {
                    ChannelId = channel.Id,
                    GuildId = ctx.Guild.Id,
                    RequestedOnBehalfOfId = ctx.User.Id,
                    IsDisabled = true
                };

                var disableReqValidator = new DisableMessageFormatReqValidator(ctx.Client);
                await disableReqValidator.ValidateAndThrowAsync(disableReq);

                var disableRes = await _commandHandlerFactory
                    .GetHandler<IAsyncCommandHandler<DisableMessageFormatCommand, DiscordEmbed>>()
                    .HandleAsync(new DisableMessageFormatCommand(disableReq, ctx));

                if (disableRes.IsDefined(out var disableEmbed))
                    await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(disableEmbed));
                else
                    await ctx.EditResponseAsync(
                        new DiscordWebhookBuilder().AddEmbed(GetUnsuccessfulResultEmbed(disableRes, ctx.Client)));
                return;
            case ChannelMessageFormatActionType.Verify:

                if (!ulong.TryParse(messageId, out var parsed)) throw new ArgumentException("Message Id is not valid");

                var verifyReq = new VerifyMessageFormatReqDto(channel.Id, parsed, ctx.Guild.Id, ctx.Member.Id);

                var verifyReqValidator = new VerifyMessageFormatReqValidator(ctx.Client);
                await verifyReqValidator.ValidateAndThrowAsync(verifyReq);

                var verifyRes = await _commandHandlerFactory
                    .GetHandler<IAsyncCommandHandler<VerifyMessageFormatCommand, VerifyMessageFormatResDto>>()
                    .HandleAsync(new VerifyMessageFormatCommand(verifyReq, ctx));

                if (verifyRes.IsDefined(out var verifyResDto) && verifyResDto.Embed is not null)
                    await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(verifyResDto.Embed));
                else
                    await ctx.EditResponseAsync(
                        new DiscordWebhookBuilder().AddEmbed(GetUnsuccessfulResultEmbed(verifyRes, ctx.Client)));
                return;
            case ChannelMessageFormatActionType.Enable:
                var enableReq = new DisableChannelMessageFormatReqDto
                {
                    ChannelId = channel.Id,
                    GuildId = ctx.Guild.Id,
                    RequestedOnBehalfOfId = ctx.User.Id,
                    IsDisabled = false
                };

                var enableReqValidator = new DisableMessageFormatReqValidator(ctx.Client);
                await enableReqValidator.ValidateAndThrowAsync(enableReq);

                var enableRes = await _commandHandlerFactory
                    .GetHandler<IAsyncCommandHandler<DisableMessageFormatCommand, DiscordEmbed>>()
                    .HandleAsync(new DisableMessageFormatCommand(enableReq, ctx));

                if (enableRes.IsDefined(out var enableEmbed))
                    await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(enableEmbed));
                else
                    await ctx.EditResponseAsync(
                        new DiscordWebhookBuilder().AddEmbed(GetUnsuccessfulResultEmbed(enableRes, ctx.Client)));
                return;
            default:
                throw new ArgumentOutOfRangeException(nameof(action), action, null);
        }
    }

    [UsedImplicitly]
    [SlashRequireUserPermissions(Permissions.Administrator)]
    [SlashCommand("ticket-welcome-config", "Command that allows configuring ticket welcome message", false)]
    public async Task TicketCenterCommand(InteractionContext ctx)
    {
        await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource,
            new DiscordInteractionResponseBuilder().AsEphemeral());

        var res = await _embedConfiguratorService.ConfigureAsync(ctx, x => x.WelcomeEmbedConfig,
            x => x.WelcomeEmbedConfigId);

        if (!res.IsDefined(out var embed))
            await ctx.EditResponseAsync(
                new DiscordWebhookBuilder().AddEmbed(GetUnsuccessfulResultEmbed(res, ctx.Client)));
        else
            await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(embed));
    }

    [UsedImplicitly]
    [SlashRequireUserPermissions(Permissions.Administrator)]
    [SlashCommand("assign-role", "Command that allows bulk role assignment", false)]
    public async Task AssignRoleCommand(InteractionContext ctx,
        [Option("role-to-assign", "Role to assign")]
        DiscordRole roleToAssign,
        [Option("role-or-user", "User or a role to assign the new role to")]
        SnowflakeObject targetRoleOrUser)
    {
        await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource,
            new DiscordInteractionResponseBuilder().AsEphemeral());

        if (roleToAssign is null || targetRoleOrUser is null)
            throw new ArgumentException("Provide all arguments");

        if (targetRoleOrUser is DiscordRole targetRole)
        {
            var users = await ctx.Guild.GetAllMembersAsync();
            var targets = users.Where(x => x.HasRole(targetRole.Id, out _));

            foreach (var target in targets)
            {
                await Task.Delay(500);
                
                if (target.HasRole(roleToAssign.Id, out _))
                    continue;

                await target.GrantRoleAsync(roleToAssign);
            }
        }
        else if (targetRoleOrUser is DiscordMember member)
        {
            if (!member.HasRole(roleToAssign.Id, out _))
                await member.GrantRoleAsync(roleToAssign);
        }
        
        await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(GetSuccessfulActionEmbed(ctx.Client)));
    }

    [UsedImplicitly]
    [SlashRequireUserPermissions(Permissions.Administrator)]
    [SlashCommand("module", "Command that allows configuring modules", false)]
    public async Task TicketConfigCommand(InteractionContext ctx,
        [Option("action", "Action to perform")]
        ModuleActionType action,
        [Option("module", "Module to perform action on")]
        GuildModule type,
        [Option("clean-after", "After how many hours should inactive closed tickets be cleared")]
        string? cleanAfter = "",
        [Option("close-after", "After how many hours should inactive opened tickets be closed")]
        string? closeAfter = "",
        [Option("reminders-channel", "Channel to send general reminders to")]
        DiscordChannel? reminderChannel = null,
        [Option("suggestions-channel", "Channel to treat as suggestion channel")]
        DiscordChannel? suggestionChannel = null,
        [Option("should-open-threads", "Whether to open a thread for each suggestion")]
        bool shouldOpenThreads = false,
        [Option("should-add-votes", "Whether to add reaction votes to each suggestion")]
        bool shouldAddVotes = false)
    {
        await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource,
            new DiscordInteractionResponseBuilder().AsEphemeral());

        Result<DiscordEmbed>? result = null;

        switch (type)
        {
            case GuildModule.Ticketing:
                switch (action)
                {
                    case ModuleActionType.Enable:
                        double cleanAfterParsed = 0;
                        double closeAfterParsed = 0;

                        if (cleanAfter is not null && cleanAfter != "" &&
                            !double.TryParse(cleanAfter, out cleanAfterParsed))
                            throw new ArgumentException("Time provided was in incorrect format",
                                nameof(cleanAfter));
                        if (closeAfter is not null && closeAfter != "" &&
                            !double.TryParse(closeAfter, out closeAfterParsed))
                            throw new ArgumentException("Time provided was in incorrect format",
                                nameof(closeAfter));

                        if (closeAfterParsed is < 1 or > 744)
                            throw new ArgumentException("Accepted values are in range from 1 to 744",
                                nameof(closeAfter));
                        if (cleanAfterParsed is < 1 or > 744)
                            throw new ArgumentException("Accepted values are in range from 1 to 744",
                                nameof(cleanAfter));

                        var enableTicketingReq = new TicketingConfigReqDto
                        {
                            GuildId = ctx.Guild.Id, RequestedOnBehalfOfId = ctx.Member.Id
                        };
                        if (cleanAfter is not "")
                            enableTicketingReq.CleanAfter = TimeSpan.FromHours(cleanAfterParsed);
                        if (closeAfter is not "")
                            enableTicketingReq.CloseAfter = TimeSpan.FromHours(closeAfterParsed);

                        var enableTicketingValidator = new TicketingConfigReqValidator(ctx.Client);
                        await enableTicketingValidator.ValidateAndThrowAsync(enableTicketingReq);
                        result = await _discordGuildService.CreateModuleAsync(ctx, enableTicketingReq);
                        break;
                    case ModuleActionType.Repair:
                        var repairTicketingReq = new TicketingConfigRepairReqDto
                        {
                            GuildId = ctx.Guild.Id,
                            RequestedOnBehalfOfId = ctx.Member.Id
                        };
                        var repairTicketingValidator = new TicketingConfigRepairReqValidator(ctx.Client);
                        await repairTicketingValidator.ValidateAndThrowAsync(repairTicketingReq);
                        result = await _discordGuildService.RepairConfigAsync(ctx, repairTicketingReq);
                        break;
                    case ModuleActionType.Edit:
                        break;
                    case ModuleActionType.Disable:
                        var disableTicketingReq = new TicketingConfigDisableReqDto
                        {
                            GuildId = ctx.Guild.Id,
                            RequestedOnBehalfOfId = ctx.Member.Id
                        };
                        var disableTicketingValidator = new TicketingConfigDisableReqValidator(ctx.Client);
                        await disableTicketingValidator.ValidateAndThrowAsync(disableTicketingReq);
                        result = await _discordGuildService.DisableModuleAsync(ctx, disableTicketingReq);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(action), action, null);
                }

                break;
            case GuildModule.Moderation:
                switch (action)
                {
                    case ModuleActionType.Enable:
                        var enableModerationReq = new ModerationConfigReqDto
                        {
                            GuildId = ctx.Guild.Id,
                            RequestedOnBehalfOfId = ctx.Member.Id
                        };
                        var enableModerationValidator = new ModerationConfigReqValidator(ctx.Client);
                        await enableModerationValidator.ValidateAndThrowAsync(enableModerationReq);
                        result = await _discordGuildService.CreateModuleAsync(ctx, enableModerationReq);
                        break;
                    case ModuleActionType.Repair:
                        var repairModerationReq = new ModerationConfigRepairReqDto
                        {
                            GuildId = ctx.Guild.Id,
                            RequestedOnBehalfOfId = ctx.Member.Id
                        };
                        var repairModerationValidator = new ModerationConfigRepairReqValidator(ctx.Client);
                        await repairModerationValidator.ValidateAndThrowAsync(repairModerationReq);
                        result = await _discordGuildService.RepairConfigAsync(ctx, repairModerationReq);
                        break;
                    case ModuleActionType.Edit:
                        break;
                    case ModuleActionType.Disable:
                        var disableModerationReq = new ModerationConfigDisableReqDto
                        {
                            GuildId = ctx.Guild.Id,
                            RequestedOnBehalfOfId = ctx.Member.Id
                        };
                        var disableModerationValidator = new ModerationConfigDisableReqValidator(ctx.Client);
                        await disableModerationValidator.ValidateAndThrowAsync(disableModerationReq);
                        result = await _discordGuildService.DisableModuleAsync(ctx, disableModerationReq);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(action), action, null);
                }

                break;
            case GuildModule.Reminders:
                switch (action)
                {
                    case ModuleActionType.Enable:
                        if (reminderChannel is null)
                            throw new ArgumentNullException(nameof(reminderChannel));
                        var enableReminderReq = new ReminderConfigReqDto
                        {
                            GuildId = ctx.Guild.Id,
                            RequestedOnBehalfOfId = ctx.Member.Id,
                            ChannelId = reminderChannel.Id

                        };
                        var enableReminderValidator = new ReminderConfigReqValidator(ctx.Client);
                        await enableReminderValidator.ValidateAndThrowAsync(enableReminderReq);
                        result = await _discordGuildService.CreateModuleAsync(ctx, enableReminderReq);
                        break;
                    case ModuleActionType.Repair:
                        if (reminderChannel is null)
                            throw new ArgumentNullException(nameof(reminderChannel));
                        var repairReminderReq = new ReminderConfigRepairReqDto
                        {
                            GuildId = ctx.Guild.Id,
                            RequestedOnBehalfOfId = ctx.Member.Id,
                            ChannelId = reminderChannel.Id
                        };
                        var repairReminderValidator = new ReminderConfigRepairReqValidator(ctx.Client);
                        await repairReminderValidator.ValidateAndThrowAsync(repairReminderReq);
                        result = await _discordGuildService.RepairConfigAsync(ctx, repairReminderReq);
                        break;
                    case ModuleActionType.Edit:
                        break;
                    case ModuleActionType.Disable:
                        var disableReminderReq = new ReminderConfigDisableReqDto
                        {
                            GuildId = ctx.Guild.Id,
                            RequestedOnBehalfOfId = ctx.Member.Id
                        };
                        var disableReminderValidator = new ReminderConfigDisableReqValidator(ctx.Client);
                        await disableReminderValidator.ValidateAndThrowAsync(disableReminderReq);
                        result = await _discordGuildService.DisableModuleAsync(ctx, disableReminderReq);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(action), action, null);
                }
                break;
            case GuildModule.Suggestions:
                switch (action)
                {
                    case ModuleActionType.Enable:
                        if (suggestionChannel is null)
                            throw new ArgumentNullException(nameof(suggestionChannel));
                        var enableSuggestionReq = new SuggestionConfigReqDto
                        {
                            GuildId = ctx.Guild.Id,
                            RequestedOnBehalfOfId = ctx.Member.Id,
                            ChannelId = suggestionChannel.Id,
                            ShouldUseThreads = shouldOpenThreads,
                            ShouldAddReactionVotes = shouldAddVotes
                        };
                        var enableSuggestionValidator = new SuggestionConfigReqValidator(ctx.Client);
                        await enableSuggestionValidator.ValidateAndThrowAsync(enableSuggestionReq);
                        result = await _commandHandlerFactory
                            .GetHandler<IAsyncCommandHandler<SuggestionConfigCommand, DiscordEmbed>>()
                            .HandleAsync(new SuggestionConfigCommand(enableSuggestionReq));
                        break;
                    case ModuleActionType.Repair:
                        if (suggestionChannel is null)
                            throw new ArgumentNullException(nameof(suggestionChannel));
                        var repairSuggestionReq = new SuggestionConfigRepairReqDto
                        {
                            GuildId = ctx.Guild.Id,
                            RequestedOnBehalfOfId = ctx.Member.Id,
                            ChannelId = suggestionChannel.Id,
                            ShouldUseThreads = shouldOpenThreads,
                            ShouldAddReactionVotes = shouldAddVotes
                        };
                        var repairSuggestionValidator = new SuggestionConfigRepairReqValidator(ctx.Client);
                        await repairSuggestionValidator.ValidateAndThrowAsync(repairSuggestionReq);
                        result = await _commandHandlerFactory
                            .GetHandler<IAsyncCommandHandler<SuggestionConfigRepairCommand, DiscordEmbed>>()
                            .HandleAsync(new SuggestionConfigRepairCommand(repairSuggestionReq));
                        break;
                    case ModuleActionType.Edit:
                        break;
                    case ModuleActionType.Disable:
                        var disableSuggestionReq = new SuggestionConfigDisableReqDto
                        {
                            GuildId = ctx.Guild.Id,
                            RequestedOnBehalfOfId = ctx.Member.Id
                        };
                        var disableSuggestionValidator = new SuggestionConfigDisableReqValidator(ctx.Client);
                        await disableSuggestionValidator.ValidateAndThrowAsync(disableSuggestionReq);
                        result = await _commandHandlerFactory
                            .GetHandler<IAsyncCommandHandler<SuggestionConfigDisableCommand, DiscordEmbed>>()
                            .HandleAsync(new SuggestionConfigDisableCommand(disableSuggestionReq));
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(action), action, null);
                }
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(type), type, null);
        }

        if (!result.HasValue) return;
        if (result.Value.IsDefined( out var embed))
            await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(embed));
        else
            await ctx.EditResponseAsync(
                new DiscordWebhookBuilder().AddEmbed(GetUnsuccessfulResultEmbed(result, ctx.Client)));
    }
}
