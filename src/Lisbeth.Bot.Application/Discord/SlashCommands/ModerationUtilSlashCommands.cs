using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using DSharpPlus.SlashCommands.Attributes;
using FluentValidation;
using Lisbeth.Bot.Application.Discord.SlashCommands.Base;
using Lisbeth.Bot.Application.Enums;
using Lisbeth.Bot.Application.Validation.ModerationConfig;
using Lisbeth.Bot.Application.Validation.TicketingConfig;
using Lisbeth.Bot.DataAccessLayer.Specifications.Guild;
using Lisbeth.Bot.Domain.DTOs.Request.ModerationConfig;
using Lisbeth.Bot.Domain.DTOs.Request.TicketingConfig;
using MikyM.Discord.Extensions.BaseExtensions;

namespace Lisbeth.Bot.Application.Discord.SlashCommands;

[SlashCommandGroup("mod", "Moderation commands")]
[SlashModuleLifespan(SlashModuleLifespan.Transient)]
public class ModerationUtilSlashCommands : ExtendedApplicationCommandModule
{
    [UsedImplicitly] public IGuildService? GuildService { private get; set; }
    [UsedImplicitly] public IDiscordGuildService? DiscordGuildService { private get; set; }
    [UsedImplicitly] public IDiscordTicketService? DiscordTicketService { private get; set; }

    [SlashRequireUserPermissions(Permissions.Administrator)]
    [SlashCommand("identity", "A command that allows checking information about a member.")]
    [UsedImplicitly]
    public async Task IdentityCommand(InteractionContext ctx,
        [Option("user", "User to identify")] DiscordUser user)
    {
        if (user is null) throw new ArgumentNullException(nameof(user));
        await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource,
            new DiscordInteractionResponseBuilder().AsEphemeral(true));

        var res = await this.GuildService!.GetSingleBySpecAsync<Guild>(
            new ActiveGuildByDiscordIdWithTicketingSpecifications(ctx.Guild.Id));

        if (!res.IsDefined()) throw new ArgumentException("Guild not found in database");

        var member = (DiscordMember)user;

        var embed = new DiscordEmbedBuilder();
        embed.WithThumbnail(member.AvatarUrl);
        embed.WithTitle("Member information");
        embed.AddField("Member's identity", $"{user.GetFullUsername()}", true);
        embed.AddField("Joined guild", $"{member.JoinedAt}");
        embed.AddField("Account created", $"{member.CreationTimestamp}");
        embed.WithColor(new DiscordColor(res.Entity.EmbedHexColor));
        embed.WithFooter($"Member Id: {member.Id}");

        await ctx.Interaction.CreateFollowupMessageAsync(new DiscordFollowupMessageBuilder().AddEmbed(embed.Build())
            .AsEphemeral(true));
    }


    [UsedImplicitly]
    [SlashRequireUserPermissions(Permissions.Administrator)]
    [SlashCommand("ticket-center", "A command that allows creating a ticket center message")]
    public async Task TicketCenterCommand(InteractionContext ctx)
    {
        await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource,
            new DiscordInteractionResponseBuilder().AsEphemeral(true));

        var builder = await this.DiscordTicketService!.GetTicketCenterEmbedAsync(ctx);

        await ctx.Channel.SendMessageAsync(builder.Entity);
        await ctx.Interaction.CreateFollowupMessageAsync(new DiscordFollowupMessageBuilder()
            .WithContent("message sent successfully").AsEphemeral(true));
    }

    [UsedImplicitly]
    [SlashRequireUserPermissions(Permissions.Administrator)]
    [SlashCommand("module", "A command that allows configuring modules")]
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
                        result = await this.DiscordGuildService!.CreateModuleAsync(ctx, enableTicketingReq);
                        break;
                    case ModuleActionType.Repair:
                        var repairTicketingReq = new TicketingConfigRepairReqDto
                        {
                            GuildId = ctx.Guild.Id,
                            RequestedOnBehalfOfId = ctx.Member.Id
                        };
                        var repairTicketingValidator = new TicketingConfigRepairReqValidator(ctx.Client);
                        await repairTicketingValidator.ValidateAndThrowAsync(repairTicketingReq);
                        result = await this.DiscordGuildService!.RepairConfigAsync(ctx, repairTicketingReq);
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
                        result = await this.DiscordGuildService!.DisableModuleAsync(ctx, disableTicketingReq);
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
                        result = await this.DiscordGuildService!.CreateModuleAsync(ctx, enableModerationReq);
                        break;
                    case ModuleActionType.Repair:
                        var repairModerationReq = new ModerationConfigRepairReqDto
                        {
                            GuildId = ctx.Guild.Id,
                            RequestedOnBehalfOfId = ctx.Member.Id
                        };
                        var repairModerationValidator = new ModerationConfigRepairReqValidator(ctx.Client);
                        await repairModerationValidator.ValidateAndThrowAsync(repairModerationReq);
                        result = await this.DiscordGuildService!.RepairConfigAsync(ctx, repairModerationReq);
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
                        result = await this.DiscordGuildService!.DisableModuleAsync(ctx, disableModerationReq);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(action), action, null);
                }

                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(type), type, null);
        }

        if (!result.HasValue) return;
        if (result.Value.IsDefined())
            await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(result.Value.Entity));
        else
            await ctx.EditResponseAsync(
                new DiscordWebhookBuilder().AddEmbed(GetUnsuccessfulResultEmbed(result, ctx.Client)));
    }

    [UsedImplicitly]
    [SlashRequireUserPermissions(Permissions.Administrator)]
    [SlashCommand("moderation-config", "A command that allows setting moderation module up")]
    public async Task ModConfigCommand(InteractionContext ctx,
        [Option("deleted", "Channel for message deletion logs")]
        DiscordChannel deletedChannel,
        [Option("updated", "Channel for message update logs")] DiscordChannel updatedChannel,
        [Option("mute", "Mute role Id")] string muteRoleId)
    {
        await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource,
            new DiscordInteractionResponseBuilder().AsEphemeral(true));

        var res = await this.GuildService!.GetSingleBySpecAsync<Guild>(
            new ActiveGuildByDiscordIdWithTicketingSpecifications(ctx.Guild.Id));
        if (!res.IsDefined()) throw new ArgumentException("Guild not found in database");

        var guild = res.Entity;

        var modConfig = new ModerationConfig
        {
            MuteRoleId = ulong.Parse(muteRoleId), MessageDeletedEventsLogChannelId = deletedChannel.Id,
            MessageUpdatedEventsLogChannelId = updatedChannel.Id
        };

        this.GuildService.BeginUpdate(guild);
        guild.SetModerationConfig(modConfig);
        await this.GuildService.CommitAsync();

        await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent("Done"));
    }

    [UsedImplicitly]
    [SlashRequireUserPermissions(Permissions.Administrator)]
    [SlashCommand("guild-add", "A command that adds current guild to bot's database.")]
    public async Task TestGuild(InteractionContext ctx)
    {
        await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource,
            new DiscordInteractionResponseBuilder().AsEphemeral(true));

        var guild = new Guild { GuildId = ctx.Guild.Id, UserId = ctx.User.Id, IsDisabled = false };

        await this.GuildService!.AddAsync(guild, true);

        await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent("Done"));
    }
}