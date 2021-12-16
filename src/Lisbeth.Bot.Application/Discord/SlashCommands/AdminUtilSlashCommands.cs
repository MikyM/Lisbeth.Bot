using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using DSharpPlus.SlashCommands.Attributes;
using FluentValidation;
using Lisbeth.Bot.Application.Discord.SlashCommands.Base;
using Lisbeth.Bot.Application.Validation.ModerationConfig;
using Lisbeth.Bot.Application.Validation.TicketingConfig;
using Lisbeth.Bot.DataAccessLayer.Specifications.Guild;
using Lisbeth.Bot.Domain.DTOs.Request.ModerationConfig;
using Lisbeth.Bot.Domain.DTOs.Request.TicketingConfig;

namespace Lisbeth.Bot.Application.Discord.SlashCommands;

[UsedImplicitly]
[SlashCommandGroup("admin-util", "Admin utility commands", false)]
[SlashModuleLifespan(SlashModuleLifespan.Scoped)]
public class AdminUtilSlashCommands : ExtendedApplicationCommandModule
{
    private readonly IGuildDataService _guildDataService;
    private readonly IDiscordGuildService _discordGuildService;
    private readonly IDiscordTicketService _discordTicketService;
    private readonly IDiscordEmbedConfiguratorService<TicketingConfig> _embedConfiguratorService;

    public AdminUtilSlashCommands(IGuildDataService guildDataService, IDiscordGuildService discordGuildService,
        IDiscordTicketService discordTicketService, IDiscordEmbedConfiguratorService<TicketingConfig> embedConfiguratorService)
    {
        _guildDataService = guildDataService;
        _discordGuildService = discordGuildService;
        _discordTicketService = discordTicketService;
        _embedConfiguratorService = embedConfiguratorService;
    }

    [UsedImplicitly]
    [SlashRequireUserPermissions(Permissions.Administrator)]
    [SlashCommand("ticket-center", "A command that allows working with a ticket center message", false)]
    public async Task TicketCenterCommand(InteractionContext ctx, [Option("action", "Action to perform")] TicketCenterActionType action, [Option("channel", "Channel to send the message to")] DiscordChannel? channel = null)
    {
        await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource,
            new DiscordInteractionResponseBuilder().AsEphemeral(true));

        switch (action)
        {
            case TicketCenterActionType.Get:
                var builderGet = await this._discordTicketService!.GetTicketCenterEmbedAsync(ctx);
                await ctx.Channel.SendMessageAsync(builderGet.Entity);
                await ctx.Interaction.CreateFollowupMessageAsync(new DiscordFollowupMessageBuilder()
                    .AddEmbed(base.GetSuccessfulActionEmbed(ctx.Client, "Message sent successfully"))
                    .AsEphemeral(true));
                return;
            case TicketCenterActionType.ConfigureEmbed:
                var partial = await this._embedConfiguratorService.ConfigureAsync(ctx, x => x.CenterEmbedConfig,
                    x => x.CenterEmbedConfigId);
                if (!partial.IsDefined(out var embed))
                    await ctx.Interaction.CreateFollowupMessageAsync(new DiscordFollowupMessageBuilder()
                        .AddEmbed(base.GetUnsuccessfulResultEmbed(partial, ctx.Client))
                        .AsEphemeral(true));
                else
                    await ctx.Interaction.CreateFollowupMessageAsync(new DiscordFollowupMessageBuilder()
                        .AddEmbed(embed)
                        .AsEphemeral(true));
                return;
            case TicketCenterActionType.Send:
                if (channel is null)
                {
                    await ctx.Interaction.CreateFollowupMessageAsync(new DiscordFollowupMessageBuilder()
                        .AsEphemeral(true)
                        .AddEmbed(GetSuccessfulActionEmbed(ctx.Client, "You have to provide a channel")));
                    return;
                }
                var builderSend = await this._discordTicketService!.GetTicketCenterEmbedAsync(ctx);
                await channel.SendMessageAsync(builderSend.Entity);
                await ctx.Interaction.CreateFollowupMessageAsync(new DiscordFollowupMessageBuilder()
                    .AddEmbed(base.GetSuccessfulActionEmbed(ctx.Client, "Message sent successfully"))
                    .AsEphemeral(true));
                return;
            default:
                throw new ArgumentOutOfRangeException(nameof(action), action, null);
        }
    }

    [UsedImplicitly]
    [SlashRequireUserPermissions(Permissions.Administrator)]
    [SlashCommand("ticket-welcome-config", "A command that allows configuring ticket welcome message", false)]
    public async Task TicketCenterCommand(InteractionContext ctx)
    {
        await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource,
            new DiscordInteractionResponseBuilder().AsEphemeral(true));

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
    [SlashCommand("module", "A command that allows configuring modules", false)]
    public async Task TicketConfigCommand(InteractionContext ctx,
        [Option("action", "Action to perform")]
        ModuleActionType action,
        [Option("module", "Module to perform action on")]
        GuildModule type,
        [Option("clean-after", "After how many hours should inactive closed tickets be cleared")]
        string? cleanAfter = "",
        [Option("close-after", "After how many hours should inactive opened tickets be closed")]
        string? closeAfter = "")
    {
        await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource,
            new DiscordInteractionResponseBuilder().AsEphemeral(true));

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
                        result = await this._discordGuildService!.CreateModuleAsync(ctx, enableTicketingReq);
                        break;
                    case ModuleActionType.Repair:
                        var repairTicketingReq = new TicketingConfigRepairReqDto
                        {
                            GuildId = ctx.Guild.Id,
                            RequestedOnBehalfOfId = ctx.Member.Id
                        };
                        var repairTicketingValidator = new TicketingConfigRepairReqValidator(ctx.Client);
                        await repairTicketingValidator.ValidateAndThrowAsync(repairTicketingReq);
                        result = await this._discordGuildService!.RepairConfigAsync(ctx, repairTicketingReq);
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
                        result = await this._discordGuildService!.DisableModuleAsync(ctx, disableTicketingReq);
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
                        result = await this._discordGuildService!.CreateModuleAsync(ctx, enableModerationReq);
                        break;
                    case ModuleActionType.Repair:
                        var repairModerationReq = new ModerationConfigRepairReqDto
                        {
                            GuildId = ctx.Guild.Id,
                            RequestedOnBehalfOfId = ctx.Member.Id
                        };
                        var repairModerationValidator = new ModerationConfigRepairReqValidator(ctx.Client);
                        await repairModerationValidator.ValidateAndThrowAsync(repairModerationReq);
                        result = await this._discordGuildService!.RepairConfigAsync(ctx, repairModerationReq);
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
                        result = await this._discordGuildService!.DisableModuleAsync(ctx, disableModerationReq);
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

    [UsedImplicitly]
    [SlashRequireUserPermissions(Permissions.Administrator)]
    [SlashCommand("moderation-config", "A command that allows setting moderation module up", false)]
    public async Task ModConfigCommand(InteractionContext ctx,
        [Option("deleted", "Channel for message deletion logs")]
        DiscordChannel deletedChannel,
        [Option("updated", "Channel for message update logs")] DiscordChannel updatedChannel,
        [Option("mute", "Mute role Id")] string muteRoleId)
    {
        await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource,
            new DiscordInteractionResponseBuilder().AsEphemeral(true));

        var res = await this._guildDataService!.GetSingleBySpecAsync<Guild>(
            new ActiveGuildByDiscordIdWithTicketingSpecifications(ctx.Guild.Id));
        if (!res.IsDefined()) throw new ArgumentException("Guild not found in database");

        var guild = res.Entity;

        var modConfig = new ModerationConfig
        {
            MuteRoleId = ulong.Parse(muteRoleId), MessageDeletedEventsLogChannelId = deletedChannel.Id,
            MessageUpdatedEventsLogChannelId = updatedChannel.Id
        };

        this._guildDataService.BeginUpdate(guild);
        guild.SetModerationConfig(modConfig);
        await this._guildDataService.CommitAsync();

        await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent("Done"));
    }

    [UsedImplicitly]
    [SlashRequireOwner]
    [SlashCommand("guild-add", "A command that adds current guild to bot's database.", false)]
    public async Task TestGuild(InteractionContext ctx)
    {
        await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource,
            new DiscordInteractionResponseBuilder().AsEphemeral(true));

        var guild = new Guild { GuildId = ctx.Guild.Id, UserId = ctx.User.Id, IsDisabled = false };

        await this._guildDataService!.AddAsync(guild, true);

        await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent("Done"));
    }
}
