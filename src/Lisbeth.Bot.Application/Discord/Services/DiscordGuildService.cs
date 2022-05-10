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
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.SlashCommands;
using Lisbeth.Bot.Application.Discord.Extensions;
using Lisbeth.Bot.Application.Discord.Helpers;
using Lisbeth.Bot.DataAccessLayer.Specifications.Guild;
using Lisbeth.Bot.Domain;
using Lisbeth.Bot.Domain.DTOs.Request.Guild;
using Lisbeth.Bot.Domain.DTOs.Request.ModerationConfig;
using Lisbeth.Bot.Domain.DTOs.Request.ReminderConfig;
using Lisbeth.Bot.Domain.DTOs.Request.TicketingConfig;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MikyM.Common.Utilities.Extensions;
using MikyM.Common.Utilities.Results;
using MikyM.Common.Utilities.Results.Errors;
using MikyM.Discord.Enums;
using MikyM.Discord.Extensions.BaseExtensions;
using MikyM.Discord.Interfaces;

namespace Lisbeth.Bot.Application.Discord.Services;

[UsedImplicitly]
[Service]
[RegisterAs(typeof(IDiscordGuildService))]
[Lifetime(Lifetime.InstancePerLifetimeScope)]
public class DiscordGuildService : IDiscordGuildService
{
    private readonly IDiscordService _discord;
    private readonly IEmbedConfigDataService _embedConfigDataService;
    private readonly IDiscordEmbedProvider _embedProvider;
    private readonly IGuildDataService _guildDataService;
    private readonly ILogger<DiscordGuildService> _logger;
    private readonly ITicketQueueService _ticketQueueService;
    private readonly IOptions<BotConfiguration> _options;

    public DiscordGuildService(IEmbedConfigDataService embedConfigDataService, IDiscordEmbedProvider embedProvider,
        IGuildDataService guildDataService, IDiscordService discord, ILogger<DiscordGuildService> logger,
        ITicketQueueService ticketQueueService, IOptions<BotConfiguration> options)
    {
        _embedConfigDataService = embedConfigDataService;
        _embedProvider = embedProvider;
        _guildDataService = guildDataService;
        _discord = discord;
        _logger = logger;
        _ticketQueueService = ticketQueueService;
        _options = options;
    }

    public async Task<Result> HandleGuildCreateAsync(GuildCreateEventArgs args)
    {
        _logger.LogInformation($"New guild spotted: {args.Guild.Id}");

        var result = await _guildDataService.GetSingleBySpecAsync(new GuildByIdSpec(args.Guild.Id));

        if (!result.IsDefined())
        {
            await _guildDataService.AddAsync(new Guild { GuildId = args.Guild.Id, UserId = args.Guild.OwnerId }, true);
            var embedResult = await _embedConfigDataService.GetAsync((long)1);
            if (embedResult.IsDefined())
                await args.Guild.Owner.SendMessageAsync(_embedProvider.GetEmbedFromConfig(embedResult.Entity).Build());
        }
        else
        {
            _guildDataService.BeginUpdate(result.Entity);
            result.Entity.IsDisabled = false;
            await _guildDataService.CommitAsync();

            var embedResult = await _embedConfigDataService.GetAsync((long)2);
            if (embedResult.IsDefined())
                await args.Guild.Owner.SendMessageAsync(_embedProvider.GetEmbedFromConfig(embedResult.Entity).Build());
        }
        _ = await PrepareSlashPermissionsAsync(args.Guild);

        _logger.LogInformation($"Add process for guild with Id: {args.Guild.Id} finished");
        return Result.FromSuccess();
    }

    public async Task<Result> HandleGuildDeleteAsync(GuildDeleteEventArgs args)
    {
        var result = await _guildDataService.GetSingleBySpecAsync(new GuildByIdSpec(args.Guild.Id));

        if (result.IsDefined()) await _guildDataService.DisableAsync(result.Entity, true);

        return Result.FromSuccess();
    }


    public async Task<Result<DiscordEmbed>> CreateModuleAsync(TicketingConfigReqDto req)
    {
        if (req is null) throw new ArgumentNullException(nameof(req));

        DiscordGuild guild = await _discord.Client.GetGuildAsync(req.GuildId);

        return await CreateTicketingModuleAsync(guild, await guild.GetMemberAsync(req.RequestedOnBehalfOfId), req);
    }

    public async Task<Result<DiscordEmbed>> CreateModuleAsync(InteractionContext ctx, TicketingConfigReqDto req)
    {
        if (ctx is null) throw new ArgumentNullException(nameof(ctx));
        if (req is null) throw new ArgumentNullException(nameof(req));

        return await CreateTicketingModuleAsync(ctx.Guild, ctx.Member, req);
    }

    public async Task<Result<DiscordEmbed>> CreateModuleAsync(ModerationConfigReqDto req)
    {
        if (req is null) throw new ArgumentNullException(nameof(req));

        DiscordGuild guild = await _discord.Client.GetGuildAsync(req.GuildId);

        return await CreateModerationModuleAsync(guild, await guild.GetMemberAsync(req.RequestedOnBehalfOfId), req);
    }

    public async Task<Result<DiscordEmbed>> CreateModuleAsync(InteractionContext ctx, ModerationConfigReqDto req)
    {
        if (ctx is null) throw new ArgumentNullException(nameof(ctx));
        if (req is null) throw new ArgumentNullException(nameof(req));

        return await CreateModerationModuleAsync(ctx.Guild, ctx.Member, req);
    }

    public async Task<Result<DiscordEmbed>> CreateModuleAsync(ReminderConfigReqDto req)
    {
        if (req is null) throw new ArgumentNullException(nameof(req));

        DiscordGuild guild = await _discord.Client.GetGuildAsync(req.GuildId);

        return await CreateReminderModuleAsync(guild, await guild.GetMemberAsync(req.RequestedOnBehalfOfId), req);
    }

    public async Task<Result<DiscordEmbed>> CreateModuleAsync(InteractionContext ctx, ReminderConfigReqDto req)
    {
        if (ctx is null) throw new ArgumentNullException(nameof(ctx));
        if (req is null) throw new ArgumentNullException(nameof(req));

        return await CreateReminderModuleAsync(ctx.Guild, ctx.Member, req);
    }

    public async Task<Result<DiscordEmbed>> RepairConfigAsync(InteractionContext ctx,
        ModerationConfigRepairReqDto req)
    {
        if (req is null) throw new ArgumentNullException(nameof(req));
        if (ctx is null) throw new ArgumentNullException(nameof(ctx));

        return await RepairConfigAsync(ctx.Guild, GuildModule.Moderation, ctx.Member);
    }

    public async Task<Result<DiscordEmbed>> RepairConfigAsync(TicketingConfigRepairReqDto req)
    {
        if (req is null) throw new ArgumentNullException(nameof(req));

        DiscordGuild guild = await _discord.Client.GetGuildAsync(req.GuildId);

        return await RepairConfigAsync(guild, GuildModule.Ticketing,
            await guild.GetMemberAsync(req.RequestedOnBehalfOfId));
    }

    public async Task<Result<DiscordEmbed>> RepairConfigAsync(ModerationConfigRepairReqDto req)
    {
        if (req is null) throw new ArgumentNullException(nameof(req));

        DiscordGuild guild = await _discord.Client.GetGuildAsync(req.GuildId);

        return await RepairConfigAsync(guild, GuildModule.Moderation,
            await guild.GetMemberAsync(req.RequestedOnBehalfOfId));
    }

    public async Task<Result<DiscordEmbed>> RepairConfigAsync(InteractionContext ctx,
        TicketingConfigRepairReqDto req)
    {
        if (req is null) throw new ArgumentNullException(nameof(req));
        if (ctx is null) throw new ArgumentNullException(nameof(ctx));

        return await RepairConfigAsync(ctx.Guild, GuildModule.Ticketing, ctx.Member);
    }

    public async Task<Result<DiscordEmbed>> RepairConfigAsync(ReminderConfigRepairReqDto req)
    {
        if (req is null) throw new ArgumentNullException(nameof(req));

        DiscordGuild guild = await _discord.Client.GetGuildAsync(req.GuildId);

        return await RepairConfigAsync(guild, GuildModule.Reminders,
            await guild.GetMemberAsync(req.RequestedOnBehalfOfId), req);
    }

    public async Task<Result<DiscordEmbed>> RepairConfigAsync(InteractionContext ctx,
        ReminderConfigRepairReqDto req)
    {
        if (req is null) throw new ArgumentNullException(nameof(req));
        if (ctx is null) throw new ArgumentNullException(nameof(ctx));

        return await RepairConfigAsync(ctx.Guild, GuildModule.Reminders, ctx.Member, req);
    }

    public async Task<Result<DiscordEmbed>> DisableModuleAsync(ModerationConfigDisableReqDto req)
    {
        if (req is null) throw new ArgumentNullException(nameof(req));

        DiscordGuild guild = await _discord.Client.GetGuildAsync(req.GuildId);

        return await DisableModuleAsync(guild, await guild.GetMemberAsync(req.RequestedOnBehalfOfId),
            GuildModule.Moderation);
    }

    public async Task<Result<DiscordEmbed>> DisableModuleAsync(TicketingConfigDisableReqDto req)
    {
        if (req is null) throw new ArgumentNullException(nameof(req));

        DiscordGuild guild = await _discord.Client.GetGuildAsync(req.GuildId);

        return await DisableModuleAsync(guild, await guild.GetMemberAsync(req.RequestedOnBehalfOfId),
            GuildModule.Ticketing);
    }

    public async Task<Result<DiscordEmbed>> DisableModuleAsync(InteractionContext ctx,
        ModerationConfigDisableReqDto req)
    {
        if (req is null) throw new ArgumentNullException(nameof(req));
        if (ctx is null) throw new ArgumentNullException(nameof(ctx));

        return await DisableModuleAsync(ctx.Guild, ctx.Member, GuildModule.Moderation);
    }

    public async Task<Result<DiscordEmbed>> DisableModuleAsync(InteractionContext ctx,
        TicketingConfigDisableReqDto req)
    {
        if (req is null) throw new ArgumentNullException(nameof(req));
        if (ctx is null) throw new ArgumentNullException(nameof(ctx));

        return await DisableModuleAsync(ctx.Guild, ctx.Member, GuildModule.Ticketing);
    }

    public async Task<Result<DiscordEmbed>> DisableModuleAsync(InteractionContext ctx,
        ReminderConfigDisableReqDto req)
    {
        if (req is null) throw new ArgumentNullException(nameof(req));
        if (ctx is null) throw new ArgumentNullException(nameof(ctx));

        return await DisableModuleAsync(ctx.Guild, ctx.Member, GuildModule.Reminders);
    }

    public async Task<Result<DiscordEmbed>> DisableModuleAsync(ReminderConfigDisableReqDto req)
    {
        if (req is null) throw new ArgumentNullException(nameof(req));

        DiscordGuild guild = await _discord.Client.GetGuildAsync(req.GuildId);

        return await DisableModuleAsync(guild, await guild.GetMemberAsync(req.RequestedOnBehalfOfId),
            GuildModule.Reminders);
    }

    public async Task<Result<int>> CreateOverwritesForMutedRoleAsync(CreateMuteOverwritesReqDto req)
    {
        if (req is null) throw new ArgumentNullException(nameof(req));

        var guildResult = await _guildDataService.GetSingleBySpecAsync<Guild>(
            new ActiveGuildByDiscordIdWithModerationSpec(req.GuildId));

        if (!guildResult.IsDefined() || guildResult.Entity.ModerationConfig is null)
            return new NotFoundError();
        if (guildResult.Entity.ModerationConfig.IsDisabled)
            return new DisabledEntityError(nameof(guildResult.Entity.ModerationConfig));

        var discordGuild = await _discord.Client.GetGuildAsync(req.GuildId);

        return await CreateOverwritesForMutedRoleAsync(discordGuild,
            discordGuild.Roles[guildResult.Entity.ModerationConfig.MuteRoleId],
            await discordGuild.GetMemberAsync(req.RequestedOnBehalfOfId));
    }

    public async Task<Result<int>> CreateOverwritesForMutedRoleAsync(InteractionContext ctx,
        CreateMuteOverwritesReqDto req)
    {
        if (ctx is null) throw new ArgumentNullException(nameof(ctx));
        if (req is null) throw new ArgumentNullException(nameof(req));

        var guildResult = await _guildDataService.GetSingleBySpecAsync<Guild>(
            new ActiveGuildByDiscordIdWithModerationSpec(req.GuildId));

        if (!guildResult.IsDefined() || guildResult.Entity.ModerationConfig is null)
            return new NotFoundError();
        if (guildResult.Entity.ModerationConfig.IsDisabled)
            return new DisabledEntityError(nameof(guildResult.Entity.ModerationConfig));

        return await CreateOverwritesForMutedRoleAsync(ctx.Guild,
            ctx.Guild.Roles[guildResult.Entity.ModerationConfig.MuteRoleId], ctx.Member);
    }

    public async Task<Result> PrepareSlashPermissionsAsync(DiscordGuild guild)
    {
        if (guild is null) throw new ArgumentNullException(nameof(guild));

        return await PrepareSlashPermissionsAsync(new[] { guild });
    }

    public async Task<Result> PrepareBotAsync(IEnumerable<ulong> guildIds)
    {
        await _discord.Client.UpdateStatusAsync(new DiscordActivity("you closely.", ActivityType.Watching));
        return Result.FromSuccess();
    }

    public async Task<Result> SetPhishingDetectionAsync(SetPhishingReqDto req)
    {
        var res =  await _guildDataService.GetSingleBySpecAsync(new ActiveGuildByIdSpec(req.GuildId));
        if (!res.IsDefined(out var cfg))
            return Result.FromError(res);

        _guildDataService.BeginUpdate(cfg);
        cfg.PhishingDetection = req.PhishingDetection;
        await _guildDataService.CommitAsync(req.RequestedOnBehalfOfId.ToString());

        return Result.FromSuccess();
    }

    public async Task<Result> PrepareSlashPermissionsAsync(IEnumerable<DiscordGuild> guilds)
    {
        if (guilds is null) throw new ArgumentNullException(nameof(guilds));

        await Task.Delay(10000);

        foreach (var guild in guilds)
        {
            try
            {
                var adminRoles = new List<DiscordRole>();
                var manageMessagesRoles = new List<DiscordRole>();
                var userManageRoles = new List<DiscordRole>();
                var ownerPerms = new List<DiscordApplicationCommandPermission>();
                var serverOwnerPerms = new List<DiscordApplicationCommandPermission>();

                foreach (var role in guild.Roles.Values)
                {
                    if (role.Permissions.HasPermission(Permissions.Administrator))
                        adminRoles.Add(role);
                    if (role.Permissions.HasPermission(Permissions.BanMembers))
                        userManageRoles.Add(role);
                    if (role.Permissions.HasPermission(Permissions.ManageMessages))
                        manageMessagesRoles.Add(role);
                }

                DiscordMember? botOwner;
                try
                {
                    botOwner = await guild.GetMemberAsync(_discord.Client.CurrentApplication.Owners.First().Id);
                }
                catch
                {
                    botOwner = null;
                }
                DiscordMember? serverOwner = guild.Owner;
                try
                {
                    serverOwner ??= await guild.GetMemberAsync(guild.OwnerId);
                }
                catch
                {
                    serverOwner = null;
                }
                
                if (botOwner is not null) ownerPerms.Add(new DiscordApplicationCommandPermission(botOwner, true));
                var admPerms = adminRoles
                    .Select(adminRole => new DiscordApplicationCommandPermission(adminRole, true))
                    .ToList();
                var messagePerms = manageMessagesRoles
                    .Select(messageRole => new DiscordApplicationCommandPermission(messageRole, true))
                    .ToList();
                var userPerms = userManageRoles
                    .Select(userRole => new DiscordApplicationCommandPermission(userRole, true))
                    .ToList();
                if (serverOwner is not null)
                {
                    admPerms.Add(new DiscordApplicationCommandPermission(serverOwner, true));
                    messagePerms.Add(new DiscordApplicationCommandPermission(serverOwner, true));
                    userPerms.Add(new DiscordApplicationCommandPermission(serverOwner, true));
                }

                var cmds = _options.Value.GlobalRegister ? await _discord.Client.GetGlobalApplicationCommandsAsync() : await guild.GetApplicationCommandsAsync();
                cmds = cmds.Where(x => x.ApplicationId == _discord.Client.CurrentApplication.Id).ToList();

                var modCmds = cmds.Where(x =>
                        x.Name is "ban" or "identity" ||
                        x.Name.Contains("mute", StringComparison.InvariantCultureIgnoreCase))
                    .ToList();
                var messageCmds = cmds.Where(x =>
                        x.Name.Contains("prune", StringComparison.InvariantCultureIgnoreCase) &&
                        !x.Name.Contains("mute", StringComparison.InvariantCultureIgnoreCase))
                    .ToList();
                var admCmds = cmds.Where(x => x.Name is "role-menu" or "ticket" || x.Name.Contains("admin-util"))
                    .ToList();
                var ownerCmds = cmds.Where(x => x.Name is "owner").ToList();

                var update = modCmds.Select(modCmd => new DiscordGuildApplicationCommandPermissions(modCmd.Id, userPerms))
                    .ToList();
                update.AddRange(admCmds.Select(admCmd => new DiscordGuildApplicationCommandPermissions(admCmd.Id, admPerms)));
                update.AddRange(messageCmds.Select(msgCmd =>
                    new DiscordGuildApplicationCommandPermissions(msgCmd.Id, messagePerms)));

                if (botOwner is not null)
                    update.AddRange(ownerCmds.Select(ownerCmd =>
                        new DiscordGuildApplicationCommandPermissions(ownerCmd.Id, ownerPerms)));

                await guild.BatchEditApplicationCommandPermissionsAsync(update);

                _logger.LogInformation(
                    $"{guild.Id} found roles: Admin roles: {string.Join(" ", adminRoles.Select(x => x.Id))}, Message roles: {string.Join(" ", manageMessagesRoles.Select(x => x.Id))}, Mod roles: {string.Join(" ", userManageRoles.Select(x => x.Id))}{(botOwner is not null ? $", Owner : {botOwner.Id}" : "")}");
                _logger.LogInformation($"Overwriting slashies done for {guild.Id}");

                await Task.Delay(2000);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to bulk overwrite slashies for guild: {guild.Id} because: {ex.GetFullMessage()}");
            }
        }
        
        return Result.FromSuccess();
    }

    private async Task<Result<DiscordEmbed>> CreateReminderModuleAsync(DiscordGuild guild,
        DiscordMember requestingMember, ReminderConfigReqDto req)
    {
        if (guild is null) throw new ArgumentNullException(nameof(guild));
        if (requestingMember is null) throw new ArgumentNullException(nameof(requestingMember));
        if (req is null) throw new ArgumentNullException(nameof(req));

        if (!requestingMember.IsAdmin()) return new DiscordNotAuthorizedError();

        var guildRes = await _guildDataService.AddConfigAsync(req, true);
        if (!guildRes.IsDefined(out var foundGuild)) return Result<DiscordEmbed>.FromError(guildRes);

        var embed = new DiscordEmbedBuilder();
        embed.WithColor(new DiscordColor(foundGuild.EmbedHexColor));
        embed.WithAuthor("Reminder configuration");
        embed.WithDescription("Process completed successfully");
        embed.AddField("Reminder channel", ExtendedFormatter.Mention(req.ChannelId, DiscordEntity.Channel));
        embed.WithFooter(
            $"Lisbeth configuration requested by {requestingMember.GetFullDisplayName()} | Id: {requestingMember.Id}");

        return embed.Build();
    }

    private async Task<Result<DiscordEmbed>> CreateTicketingModuleAsync(DiscordGuild guild,
        DiscordMember requestingMember, TicketingConfigReqDto req)
    {
        if (guild is null) throw new ArgumentNullException(nameof(guild));
        if (requestingMember is null) throw new ArgumentNullException(nameof(requestingMember));
        if (req is null) throw new ArgumentNullException(nameof(req));

        if (!requestingMember.IsAdmin()) return new DiscordNotAuthorizedError();

        var everyoneDeny = new[]
            { new DiscordOverwriteBuilder(guild.EveryoneRole).Deny(Permissions.AccessChannels) };

        var openedCat = await guild.CreateChannelAsync("TICKETS", ChannelType.Category, null,
            "Category with opened tickets", null, null, everyoneDeny);
        var closedCat = await guild.CreateChannelAsync("TICKETS-ARCHIVE", ChannelType.Category, null,
            "Category with closed tickets", null, null, everyoneDeny);
        var ticketLogs = await guild.CreateTextChannelAsync("ticket-logs", closedCat,
            "Channel with ticket logs and transcripts",
            everyoneDeny);

        req.OpenedCategoryId = openedCat.Id;
        req.ClosedCategoryId = closedCat.Id;
        req.LogChannelId = ticketLogs.Id;
        var res = await _guildDataService.AddConfigAsync(req, true);
        if (!res.IsDefined()) return new InvalidOperationError();

        var embed = new DiscordEmbedBuilder();
        embed.WithColor(new DiscordColor(res.Entity.EmbedHexColor));
        embed.WithAuthor("Ticketing configuration");
        embed.WithDescription("Process completed successfully");
        embed.AddField("Opened ticket category", openedCat.Mention);
        embed.AddField("Closed ticket category", closedCat.Mention);
        embed.AddField("Ticket log channel", ticketLogs.Mention);
        embed.WithFooter(
            $"Lisbeth configuration requested by {requestingMember.GetFullDisplayName()} | Id: {requestingMember.Id}");

        return embed.Build();
    }

    private async Task<Result<DiscordEmbed>> CreateModerationModuleAsync(DiscordGuild guild,
        DiscordMember requestingMember, ModerationConfigReqDto req)
    {
        if (guild is null) throw new ArgumentNullException(nameof(guild));
        if (requestingMember is null) throw new ArgumentNullException(nameof(requestingMember));
        if (req is null) throw new ArgumentNullException(nameof(req));

        if (!requestingMember.IsAdmin()) return new DiscordNotAuthorizedError();

        var everyoneDeny = new[]
            { new DiscordOverwriteBuilder(guild.EveryoneRole).Deny(Permissions.AccessChannels) };
        var moderationCat = await guild.CreateChannelAsync("MODERATION", ChannelType.Category, null,
            "Moderation category", null, null, everyoneDeny);
        var moderationChannelLog = await guild.CreateTextChannelAsync("moderation-logs", moderationCat,
            "Category with moderation logs", everyoneDeny);
        var memberEventsLogChannel = await guild.CreateTextChannelAsync("member-logs", moderationCat,
            "Category with member logs", everyoneDeny);
        var messageEditLogChannel = await guild.CreateTextChannelAsync("edit-logs", moderationCat,
            "Category with message edit logs", everyoneDeny);
        var messageDeleteLogChannel = await guild.CreateTextChannelAsync("delete-logs", moderationCat,
            "Category with message delete logs", everyoneDeny);
        var mutedRole = await guild.CreateRoleAsync("Muted");

        await Task.Delay(300); // give discord a break

        var muteRes = await CreateOverwritesForMutedRoleAsync(guild, mutedRole, requestingMember);

        if (!muteRes.IsSuccess) return Result<DiscordEmbed>.FromError(muteRes);

        req.MemberEventsLogChannelId = memberEventsLogChannel.Id;
        req.MessageDeletedEventsLogChannelId = messageDeleteLogChannel.Id;
        req.MessageUpdatedEventsLogChannelId = messageEditLogChannel.Id;
        req.ModerationLogChannelId = moderationChannelLog.Id;
        req.MuteRoleId = mutedRole.Id;
        var res = await _guildDataService.AddConfigAsync(req, true);
        if (!res.IsDefined()) return new InvalidOperationError();

        var embed = new DiscordEmbedBuilder();
        embed.WithColor(new DiscordColor(res.Entity.EmbedHexColor));
        embed.WithAuthor("Moderation configuration");
        embed.WithDescription("Process completed successfully");
        embed.AddField("Moderation category", moderationCat.Mention);
        embed.AddField("Moderation log channel", moderationChannelLog.Mention);
        embed.AddField("Member log channel", memberEventsLogChannel.Mention);
        embed.AddField("message edited log channel", messageEditLogChannel.Mention);
        embed.AddField("message deleted channel", messageDeleteLogChannel.Mention);
        embed.AddField("Muted role", mutedRole.Mention);
        embed.WithFooter(
            $"Lisbeth configuration requested by {requestingMember.GetFullDisplayName()} | Id: {requestingMember.Id}");

        return embed.Build();
    }

    private async Task<Result<DiscordEmbed>> RepairConfigAsync(DiscordGuild discordGuild,
        GuildModule type, DiscordMember requestingMember, ReminderConfigRepairReqDto? reminderDto = null)
    {
        if (discordGuild is null) throw new ArgumentNullException(nameof(discordGuild));
        if (requestingMember is null) throw new ArgumentNullException(nameof(requestingMember));
        if (!requestingMember.IsAdmin()) return new DiscordNotAuthorizedError();

        Result<Guild> guildResult;
        Guild guild;
        DiscordOverwriteBuilder[] everyoneDeny;

        var embed = new DiscordEmbedBuilder();

        switch (type)
        {
            case GuildModule.Ticketing:
                guildResult = await _guildDataService.GetSingleBySpecAsync<Guild>(
                    new ActiveGuildByDiscordIdWithTicketingSpecifications(discordGuild.Id));
                if (!guildResult.IsDefined() || guildResult.Entity.TicketingConfig is null)
                    return new NotFoundError();
                if (guildResult.Entity.TicketingConfig.IsDisabled)
                    return new DisabledEntityError(nameof(guildResult.Entity.TicketingConfig));

                guild = guildResult.Entity;

                everyoneDeny = new[]
                {
                    new DiscordOverwriteBuilder(discordGuild.EveryoneRole).Deny(Permissions.AccessChannels)
                };

                DiscordChannel? newOpenedCat = null;
                DiscordChannel? newClosedCat = null;
                DiscordChannel? newTicketLogChannel = null;
                DiscordChannel? closedCat = null;

                TicketingConfigRepairReqDto ticketingReq = new();

                try
                {
                    var openedCat = await _discord.Client.GetChannelAsync(guild.TicketingConfig.OpenedCategoryId);
                    if (openedCat is null) return new DiscordNotFoundError();
                }
                catch
                {
                    newOpenedCat = await discordGuild.CreateChannelAsync("TICKETS", ChannelType.Category, null,
                        "Category with opened tickets", null, null, everyoneDeny);
                    ticketingReq.OpenedCategoryId = newOpenedCat.Id;
                }

                try
                {
                    closedCat = await _discord.Client.GetChannelAsync(guild.TicketingConfig.ClosedCategoryId);
                    if (closedCat is null) return new DiscordNotFoundError();
                }
                catch
                {
                    newClosedCat = await discordGuild.CreateChannelAsync("TICKETS-ARCHIVE", ChannelType.Category,
                        null, "Category with closed tickets", null, null, everyoneDeny);
                    ticketingReq.ClosedCategoryId = newClosedCat.Id;
                }

                try
                {
                    var ticketLogs = await _discord.Client.GetChannelAsync(guild.TicketingConfig.LogChannelId);
                    if (ticketLogs is null) return new DiscordNotFoundError();
                }
                catch
                {
                    newTicketLogChannel = await discordGuild.CreateTextChannelAsync("ticket-logs",
                        closedCat ?? newClosedCat, "Channel with ticket logs and transcripts", everyoneDeny);
                    ticketingReq.LogChannelId = newTicketLogChannel.Id;
                }

                if (newOpenedCat is not null || newClosedCat is not null || newTicketLogChannel is not null)
                {
                    ticketingReq.GuildId = guild.GuildId;
                    ticketingReq.RequestedOnBehalfOfId = requestingMember.Id;
                    await _guildDataService.RepairModuleConfigAsync(ticketingReq, true);

                    if (newOpenedCat is not null) embed.AddField("Opened ticket category", newOpenedCat.Mention);
                    if (newClosedCat is not null) embed.AddField("Closed ticket category", newClosedCat.Mention);
                    if (newTicketLogChannel is not null)
                        embed.AddField("Ticket log channel", newTicketLogChannel.Mention);
                }
                else
                {
                    embed.AddField("Result", "Nothing to repair");
                }


                break;
            case GuildModule.Moderation:
                guildResult = await _guildDataService.GetSingleBySpecAsync<Guild>(
                    new ActiveGuildByDiscordIdWithModerationSpec(discordGuild.Id));

                if (!guildResult.IsDefined() || guildResult.Entity.ModerationConfig is null)
                    return new NotFoundError();
                if (guildResult.Entity.ModerationConfig.IsDisabled)
                    return new DisabledEntityError(nameof(guildResult.Entity.ModerationConfig));

                guild = guildResult.Entity;

                everyoneDeny = new[]
                {
                    new DiscordOverwriteBuilder(discordGuild.EveryoneRole).Deny(Permissions.AccessChannels)
                };

                DiscordChannel? newMemberEventsLogChannel = null;
                DiscordChannel? newMessageDeletedEventsLogChannel = null;
                DiscordChannel? newMessageUpdatedEventsLogChannel = null;
                DiscordChannel? newModerationLogChannel = null;
                DiscordRole? newMuteRole = null;

                ModerationConfigRepairReqDto moderationReq = new();

                var newModerationCat = discordGuild.Channels.FirstOrDefault(x =>
                        string.Equals(x.Value.Name.ToLower(), "moderation",
                            StringComparison.InvariantCultureIgnoreCase))
                    .Value ?? await discordGuild.CreateChannelAsync("MODERATION", ChannelType.Category, null,
                    "Moderation category", null, null, everyoneDeny);
                try
                {
                    var moderationChannelLog =
                        await _discord.Client.GetChannelAsync(guild.ModerationConfig.ModerationLogChannelId);
                    if (moderationChannelLog is null)
                        return new DiscordNotFoundError();
                }
                catch
                {
                    newModerationLogChannel = await discordGuild.CreateTextChannelAsync("moderation-logs",
                        newModerationCat, "Channel with moderation logs", everyoneDeny);
                    moderationReq.ModerationLogChannelId = newModerationLogChannel.Id;
                }

                try
                {
                    var memberEventsLogChannel =
                        await _discord.Client.GetChannelAsync(guild.ModerationConfig.MemberEventsLogChannelId);
                    if (memberEventsLogChannel is null)
                        return new DiscordNotFoundError();
                }
                catch
                {
                    newMemberEventsLogChannel = await discordGuild.CreateTextChannelAsync("member-logs",
                        newModerationCat, "Channel with member logs", everyoneDeny);
                    moderationReq.MemberEventsLogChannelId = newMemberEventsLogChannel.Id;
                }

                try
                {
                    var messageEditLogChannel =
                        await _discord.Client.GetChannelAsync(guild.ModerationConfig
                            .MessageUpdatedEventsLogChannelId);
                    if (messageEditLogChannel is null)
                        return new DiscordNotFoundError();
                }
                catch
                {
                    newMessageUpdatedEventsLogChannel = await discordGuild.CreateTextChannelAsync("edit-logs",
                        newModerationCat, "Channel with edited message logs", everyoneDeny);
                    moderationReq.MessageUpdatedEventsLogChannelId = newMessageUpdatedEventsLogChannel.Id;
                }

                try
                {
                    var messageDeleteLogChannel =
                        await _discord.Client.GetChannelAsync(guild.ModerationConfig
                            .MessageDeletedEventsLogChannelId);
                    if (messageDeleteLogChannel is null)
                        return new DiscordNotFoundError();
                }
                catch
                {
                    newMessageDeletedEventsLogChannel = await discordGuild.CreateTextChannelAsync("delete-logs",
                        newModerationCat, "Channel with deleted message logs", everyoneDeny);
                    moderationReq.MessageDeletedEventsLogChannelId = newMessageDeletedEventsLogChannel.Id;
                }

                try
                {
                    var mutedRole = discordGuild.GetRole(guild.ModerationConfig.MuteRoleId);
                    if (mutedRole is null) return new DiscordNotFoundError();
                }
                catch
                {
                    newMuteRole = await discordGuild.CreateRoleAsync("Muted");
                    moderationReq.MuteRoleId = newMuteRole.Id;

                    await Task.Delay(300); // give discord a break

                    _ = await CreateOverwritesForMutedRoleAsync(discordGuild, newMuteRole, requestingMember);
                }

                if (newMemberEventsLogChannel is not null || newMessageUpdatedEventsLogChannel is not null ||
                    newMessageDeletedEventsLogChannel is not null || newModerationLogChannel is not null ||
                    newMuteRole is not null)
                {
                    moderationReq.GuildId = guild.GuildId;
                    moderationReq.RequestedOnBehalfOfId = requestingMember.Id;
                    await _guildDataService.RepairModuleConfigAsync(moderationReq, true);

                    embed.AddField("Moderation category", newModerationCat.Mention);
                    if (newModerationLogChannel is not null)
                        embed.AddField("Moderation log channel", newModerationLogChannel.Mention);
                    if (newMemberEventsLogChannel is not null)
                        embed.AddField("Member log channel", newMemberEventsLogChannel.Mention);
                    if (newMessageUpdatedEventsLogChannel is not null)
                        embed.AddField("message edited log channel", newMessageUpdatedEventsLogChannel.Mention);
                    if (newMessageDeletedEventsLogChannel is not null)
                        embed.AddField("message deleted channel", newMessageDeletedEventsLogChannel.Mention);
                    if (newMuteRole is not null) embed.AddField("Muted role", newMuteRole.Mention);
                }
                else
                {
                    embed.AddField("Result", "Nothing to repair");
                }

                break;
            case GuildModule.Reminders:
                if (reminderDto is null)
                    throw new ArgumentNullException(nameof(reminderDto));

                guildResult = await _guildDataService.GetSingleBySpecAsync(
                    new ActiveGuildByIdSpec(discordGuild.Id));

                if (!guildResult.IsDefined() || guildResult.Entity.ModerationConfig is null)
                    return Result<DiscordEmbed>.FromError(guildResult);
                if (!guildResult.Entity.IsReminderModuleEnabled)
                    return new DisabledEntityError(nameof(guildResult.Entity.ReminderChannelId));

                guild = guildResult.Entity;

                var partial = await _guildDataService.RepairModuleConfigAsync(reminderDto, true);

                if (!partial.IsSuccess)
                    return Result<DiscordEmbed>.FromError(partial);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(type), type, null);
        }

        embed.WithDescription("Process completed successfully");
        embed.WithColor(new DiscordColor(guild.EmbedHexColor));
        embed.WithAuthor($"{type} configuration repair");
        embed.WithFooter(
            $"Lisbeth configuration requested by {requestingMember.GetFullDisplayName()} | Id: {requestingMember.Id}");

        return embed.Build();
    }

    private async Task<Result<DiscordEmbed>> DisableModuleAsync(DiscordGuild discordGuild,
        DiscordMember requestingMember, GuildModule type)
    {
        if (!requestingMember.IsAdmin()) return new DiscordNotAuthorizedError();

        var guildResult = await _guildDataService.GetSingleBySpecAsync<Guild>(
            new ActiveGuildByDiscordIdWithTicketingSpecifications(discordGuild.Id));
        switch (type)
        {
            case GuildModule.Ticketing:
                if (!guildResult.IsDefined() || guildResult.Entity.TicketingConfig is null)
                    return new NotFoundError();
                if (guildResult.Entity.TicketingConfig.IsDisabled)
                    return new InvalidOperationError();

                await _guildDataService.DisableConfigAsync(discordGuild.Id, GuildModule.Ticketing, true);
                break;
            case GuildModule.Moderation:
                if (!guildResult.IsDefined() || guildResult.Entity.ModerationConfig is null)
                    return new NotFoundError();
                if (guildResult.Entity.ModerationConfig.IsDisabled)
                    return new InvalidOperationError();

                await _guildDataService.DisableConfigAsync(discordGuild.Id, GuildModule.Moderation, true);
                break;
            case GuildModule.Reminders:
                if (!guildResult.IsDefined())
                    return Result<DiscordEmbed>.FromError(guildResult);
                if (!guildResult.Entity.IsReminderModuleEnabled)
                    return new InvalidOperationError("Reminder module is already disabled");

                await _guildDataService.DisableConfigAsync(discordGuild.Id, GuildModule.Reminders, true);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(type), type, null);
        }

        return new DiscordEmbedBuilder().WithAuthor("Guild configurator")
            .WithDescription("Module disabled successfully")
            .WithColor(new DiscordColor(guildResult.Entity.EmbedHexColor))
            .Build();
    }

    private async Task<Result<int>> CreateOverwritesForMutedRoleAsync(DiscordGuild discordGuild,
        DiscordRole mutedRole, DiscordMember requestingMember)
    {
        if (discordGuild is null) throw new ArgumentNullException(nameof(discordGuild));
        if (mutedRole is null) throw new ArgumentNullException(nameof(mutedRole));
        if (requestingMember is null) throw new ArgumentNullException(nameof(requestingMember));

        if (!requestingMember.IsAdmin()) return new DiscordNotAuthorizedError();

        int count = 0;
        foreach (var channel in discordGuild.Channels.Values.Where(x =>
                     x.Type is ChannelType.Category or ChannelType.Text))
        {
            await channel.CreateMuteOverwriteAsync(mutedRole);
            await Task.Delay(500);

            count++;
        }

        return count;
    }
}