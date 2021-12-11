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
using Lisbeth.Bot.Application.Discord.ChatExport;
using Lisbeth.Bot.Application.Discord.Exceptions;
using Lisbeth.Bot.Application.Discord.Helpers;
using Lisbeth.Bot.Application.Discord.Helpers.InteractionIdEnums.Buttons;
using Lisbeth.Bot.Application.Discord.Helpers.InteractionIdEnums.Selects;
using Lisbeth.Bot.Application.Discord.Helpers.InteractionIdEnums.SelectValues;
using Lisbeth.Bot.Application.Helpers;
using Lisbeth.Bot.Application.Validation.Ticket;
using Lisbeth.Bot.DataAccessLayer.Specifications.Guild;
using Lisbeth.Bot.DataAccessLayer.Specifications.Ticket;
using Lisbeth.Bot.Domain.DTOs.Request.Ticket;
using Lisbeth.Bot.Domain.DTOs.Request.Ticket.Base;
using Microsoft.Extensions.Logging;
using MikyM.Discord.Enums;
using MikyM.Discord.Extensions.BaseExtensions;
using MikyM.Discord.Interfaces;
using System.Collections.Generic;

namespace Lisbeth.Bot.Application.Discord.Services;

[UsedImplicitly]
public class DiscordTicketService : IDiscordTicketService
{
    private readonly IAsyncExecutor _asyncExecutor;
    private readonly IDiscordService _discord;
    private readonly IDiscordEmbedProvider _embedProvider;
    private readonly IGuildService _guildService;
    private readonly ILogger<DiscordTicketService> _logger;
    private readonly ITicketService _ticketService;

    public DiscordTicketService(IDiscordService discord, ITicketService ticketService, IGuildService guildService,
        ILogger<DiscordTicketService> logger, IAsyncExecutor asyncExecutor, IDiscordEmbedProvider embedProvider)
    {
        _discord = discord;
        _ticketService = ticketService;
        _guildService = guildService;
        _logger = logger;
        _asyncExecutor = asyncExecutor;
        _embedProvider = embedProvider;
    }

    public async Task<Result<DiscordMessageBuilder>> OpenTicketAsync(TicketOpenReqDto req)
    {
        if (req is null) throw new ArgumentNullException(nameof(req));

        DiscordGuild guild = await _discord.Client.GetGuildAsync(req.GuildId);
        DiscordMember owner = await guild.GetMemberAsync(req.RequestedOnBehalfOfId);

        return await OpenTicketAsync(guild, owner, req);
    }

    public async Task<Result<DiscordMessageBuilder>> OpenTicketAsync(DiscordInteraction intr, TicketOpenReqDto req)
    {
        if (intr is null) throw new ArgumentNullException(nameof(intr));
        if (req is null) throw new ArgumentNullException(nameof(req));

        return await OpenTicketAsync(intr.Guild, (DiscordMember)intr.User, req, intr);
    }

    public async Task<Result<DiscordMessageBuilder>> CloseTicketAsync(TicketCloseReqDto req)
    {
        if (req is null) throw new ArgumentNullException(nameof(req));

        var res = await GetTicketByBaseRequestAsync(req);

        if (!res.IsDefined()) return Result<DiscordMessageBuilder>.FromError(res);

        return await CloseTicketAsync(res.Entity.Guild, res.Entity.Channel, res.Entity.RequestingMember, req);
    }

    public async Task<Result<DiscordMessageBuilder>> CloseTicketAsync(DiscordInteraction intr,
        TicketCloseReqDto req)
    {
        if (intr is null) throw new ArgumentNullException(nameof(intr));
        if (req is null) throw new ArgumentNullException(nameof(req));

        return await CloseTicketAsync(intr.Guild, intr.Channel, (DiscordMember)intr.User, req);
    }

    public async Task<Result<DiscordMessageBuilder>> ReopenTicketAsync(TicketReopenReqDto req)
    {
        if (req is null) throw new ArgumentNullException(nameof(req));

        var res = await GetTicketByBaseRequestAsync(req);

        if (!res.IsDefined()) return Result<DiscordMessageBuilder>.FromError(res);

        return await ReopenTicketAsync(res.Entity.Guild, res.Entity.Channel, res.Entity.RequestingMember, req);
    }

    public async Task<Result<DiscordMessageBuilder>> ReopenTicketAsync(DiscordInteraction intr,
        TicketReopenReqDto req)
    {
        if (intr is null) throw new ArgumentNullException(nameof(intr));
        if (req is null) throw new ArgumentNullException(nameof(req));

        return await ReopenTicketAsync(intr.Guild, intr.Channel, (DiscordMember)intr.User, req);
    }

    public async Task<Result<DiscordEmbed>> AddToTicketAsync(TicketAddReqDto req)
    {
        if (req is null) throw new ArgumentNullException(nameof(req));

        DiscordMember? targetMember = null;
        DiscordRole? targetRole = null;

        var res = await GetTicketByBaseRequestAsync(req);

        if (!res.IsDefined()) return Result<DiscordEmbed>.FromError(res);

        try
        {
            targetMember = await res.Entity.Guild.GetMemberAsync(req.SnowflakeId);
        }
        catch (Exception)
        {
            // means user doesn't exist in the guild, at all, or we're targeting a role
            try
            {
                targetRole = res.Entity.Guild.GetRole(req.SnowflakeId);
            }
            catch (Exception)
            {
                return new DiscordNotFoundError($"Member or role with Id: {req.SnowflakeId} not found");
            }
        }

        if (targetMember is null && targetRole is null) return new DiscordNotFoundError();

        return targetRole is null
            ? await AddToTicketAsync(res.Entity.Guild, res.Entity.RequestingMember, res.Entity.Channel, req,
                targetMember)
            : await AddToTicketAsync(res.Entity.Guild, res.Entity.RequestingMember, res.Entity.Channel, req, null,
                targetRole);
    }

    public async Task<Result<DiscordEmbed>> AddToTicketAsync(InteractionContext ctx, TicketAddReqDto req)
    {
        if (ctx is null) throw new ArgumentNullException(nameof(ctx));

        return await AddToTicketAsync(ctx.Guild, ctx.Member, ctx.Channel, req,
            ctx.ResolvedUserMentions is not null
                ? await ctx.Guild.GetMemberAsync(ctx.ResolvedUserMentions[0].Id)
                : null, ctx.ResolvedRoleMentions?[0]);
    }

    public async Task<Result<DiscordEmbed>> RemoveFromTicketAsync(InteractionContext ctx, TicketRemoveReqDto req)
    {
        if (ctx is null) throw new ArgumentNullException(nameof(ctx));
        if (req is null) throw new ArgumentNullException(nameof(req));

        return await RemoveFromTicketAsync(ctx.Guild, ctx.Member, ctx.Channel, req,
            ctx.ResolvedUserMentions is not null
                ? await ctx.Guild.GetMemberAsync(ctx.ResolvedUserMentions[0].Id)
                : null, ctx.ResolvedRoleMentions?[0]);
    }

    public async Task<Result<DiscordEmbed>> RemoveFromTicketAsync(TicketRemoveReqDto req)
    {
        if (req is null) throw new ArgumentNullException(nameof(req));

        DiscordMember? targetMember = null;
        DiscordRole? targetRole = null;

        var res = await GetTicketByBaseRequestAsync(req);

        if (!res.IsDefined()) return Result<DiscordEmbed>.FromError(res);

        try
        {
            targetMember = await res.Entity.Guild.GetMemberAsync(req.SnowflakeId);
        }
        catch (Exception)
        {
            // means user doesn't exist in the guild, at all, or we're targeting a role
            try
            {
                targetRole = res.Entity.Guild.GetRole(req.SnowflakeId);
            }
            catch (Exception ex)
            {
                throw new DiscordNotFoundException(
                    $"Role or member with Id: {req.SnowflakeId} doesn't exist or isn't in this guild.", ex);
            }
        }

        return targetRole is null
            ? await RemoveFromTicketAsync(res.Entity.Guild, res.Entity.RequestingMember, res.Entity.Channel, req,
                targetMember)
            : await RemoveFromTicketAsync(res.Entity.Guild, res.Entity.RequestingMember, res.Entity.Channel, req,
                null, targetRole);
    }

    public async Task<Result> CleanClosedTicketsAsync()
    {
        try
        {
            foreach (var (guildId, _) in _discord.Client.Guilds)
            {
                var res = await _guildService.GetSingleBySpecAsync<Guild>(
                    new ActiveGuildByDiscordIdWithTicketingAndInactiveTicketsSpecifications(guildId));

                if (!res.IsDefined()) continue;

                if (res.Entity.TicketingConfig?.CleanAfter is null) continue;
                if (res.Entity.Tickets?.Count == 0) continue;

                var guildCfg = res.Entity;

                DiscordChannel closedCat;
                try
                {
                    closedCat = await _discord.Client.GetChannelAsync(guildCfg.TicketingConfig.ClosedCategoryId);
                }
                catch (Exception)
                {
                    _logger.LogInformation(
                        $"Guild with Id: {guildId} has non-existing closed ticket category set with Id: {guildCfg.TicketingConfig.ClosedCategoryId}.");
                    continue;
                }

                foreach (var closedTicketChannel in closedCat.Children)
                {
                    if ((guildCfg.Tickets ?? throw new InvalidOperationException()).All(x =>
                            x.ChannelId != closedTicketChannel.Id)) continue;

                    var lastMessage = await closedTicketChannel.GetMessagesAsync(1);
                    if (lastMessage is null || lastMessage.Count == 0) continue;

                    var timeDifference = DateTime.UtcNow.Subtract(lastMessage[0].Timestamp.UtcDateTime);
                    if (timeDifference.TotalHours >= guildCfg.TicketingConfig.CleanAfter.Value.Hours)
                        await closedTicketChannel.DeleteAsync();

                    await Task.Delay(500);
                }

                await Task.Delay(500);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError($"Something went wrong with cleaning closed tickets: {ex}");
            return ex;
        }

        return Result.FromSuccess();
    }

    public async Task<Result> CloseInactiveTicketsAsync()
    {
        try
        {
            foreach (var (guildId, guild) in _discord.Client.Guilds)
            {
                var res = await _guildService.GetSingleBySpecAsync<Guild>(
                    new ActiveGuildByDiscordIdWithTicketingAndTicketsSpecifications(guildId));

                if (!res.IsDefined()) continue;

                if (res.Entity.TicketingConfig?.CloseAfter is null) continue;
                if (res.Entity.Tickets?.Count == 0) continue;

                var guildCfg = res.Entity;

                DiscordChannel openedCat;
                try
                {
                    openedCat = await _discord.Client.GetChannelAsync(guildCfg.TicketingConfig.OpenedCategoryId);
                }
                catch (Exception)
                {
                    _logger.LogInformation(
                        $"Guild with Id: {guildId} has non-existing opened ticket category set with Id: {guildCfg.TicketingConfig.OpenedCategoryId}.");
                    continue;
                }

                foreach (var openedTicketChannel in openedCat.Children)
                {
                    if ((guildCfg.Tickets ?? throw new InvalidOperationException()).All(x =>
                            x.ChannelId != openedTicketChannel.Id)) continue;

                    var lastMessage = await openedTicketChannel.GetMessagesAsync(1);
                    var msg = lastMessage?.FirstOrDefault();
                    if (msg is null) continue;

                    if (!((DiscordMember)msg.Author).Permissions.HasPermission(Permissions.BanMembers)) continue;

                    var timeDifference = DateTime.UtcNow.Subtract(msg.Timestamp.UtcDateTime);

                    var req = new TicketCloseReqDto(null, null, guildId, openedTicketChannel.Id,
                        _discord.Client.CurrentUser.Id);

                    var validator = new TicketCloseReqValidator(_discord);
                    await validator.ValidateAndThrowAsync(req);

                    if (timeDifference.TotalHours >= guildCfg.TicketingConfig.CloseAfter.Value.Hours)
                        await CloseTicketAsync(guild, openedTicketChannel,
                            (DiscordMember)_discord.Client.CurrentUser, req);

                    await Task.Delay(500);
                }

                await Task.Delay(500);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError($"Something went wrong with closing inactive tickets: {ex}");
            return ex;
        }

        return Result.FromSuccess();
    }

    public async Task<Result<DiscordMessageBuilder>> GetTicketCenterEmbedAsync(InteractionContext ctx)
    {
        var res = await _guildService.GetSingleBySpecAsync<Guild>(
            new ActiveGuildByDiscordIdWithTicketingSpecifications(ctx.Guild.Id));

        if (!res.IsDefined()) return Result<DiscordMessageBuilder>.FromError(res);
        if (res.Entity.TicketingConfig is null)
            return new DisabledEntityError("Guild doesn't have ticketing configured");

        var envelopeEmoji = DiscordEmoji.FromName(ctx.Client, ":envelope:");
        var embed = new DiscordEmbedBuilder();

        if (res.Entity.TicketingConfig.CenterEmbedConfig is not null)
        {
            embed = _embedProvider.GetEmbedFromConfig(res.Entity.TicketingConfig.CenterEmbedConfig);
        }
        else
        {
            embed.WithTitle($"__{ctx.Guild.Name}'s Support Ticket Center__");
            embed.WithDescription(res.Entity.TicketingConfig.BaseCenterMessage);
            embed.WithColor(new DiscordColor(res.Entity.EmbedHexColor));
        }

        embed.WithFooter("Click on the button below to create a ticket");

        var btn = new DiscordButtonComponent(ButtonStyle.Primary, nameof(TicketButton.TicketOpenButton), "Open a ticket", false,
            new DiscordComponentEmoji(envelopeEmoji));
        var builder = new DiscordMessageBuilder();
        builder.AddEmbed(embed.Build());
        builder.AddComponents(btn);

        return builder;
    }

    private async Task<Result<DiscordMessageBuilder>> OpenTicketAsync(DiscordGuild guild, DiscordMember owner,
        TicketOpenReqDto req, DiscordInteraction? intr = null)
    {
        if (guild is null) throw new ArgumentNullException(nameof(guild));
        if (owner is null) throw new ArgumentNullException(nameof(owner));
        if (req is null) throw new ArgumentNullException(nameof(req));

        if (owner.Guild.Id != guild.Id) return new DiscordNotAuthorizedError(nameof(owner));

        var guildRes =
            await _guildService.GetSingleBySpecAsync(
                new ActiveGuildByDiscordIdWithTicketingSpecifications(guild.Id));

        if (!guildRes.IsDefined(out var guildCfg)) return Result<DiscordMessageBuilder>.FromError(guildRes);

        if (guildCfg.TicketingConfig is null)
            return new DisabledEntityError($"Guild with Id:{guild.Id} doesn't have ticketing enabled.");

        var ticketRes = await _ticketService.OpenAsync(req);
        if (!ticketRes.IsDefined(out var ticket))
        {
            var failEmbed = new DiscordEmbedBuilder();
            failEmbed.WithColor(new DiscordColor(guildCfg.EmbedHexColor));
            failEmbed.WithDescription("You already have an opened ticket in this guild.");
            if (intr is not null)
                await intr.CreateFollowupMessageAsync(new DiscordFollowupMessageBuilder()
                    .AddEmbed(failEmbed.Build())
                    .AsEphemeral(true));
            return new InvalidOperationError("Member already has an opened ticket in this guild.");
        }


        req.GuildSpecificId = guildCfg.TicketingConfig.LastTicketId + 1;
        _guildService.BeginUpdate(guildCfg);
        guildCfg.TicketingConfig.LastTicketId++;
        await _guildService.CommitAsync();



        var embed = new DiscordEmbedBuilder();

        if (guildCfg.TicketingConfig.WelcomeEmbedConfig is not null)
        {
            embed = _embedProvider.GetEmbedFromConfig(guildCfg.TicketingConfig.WelcomeEmbedConfig);
            embed.WithDescription(embed.Description.Replace("@ownerMention@", owner.Mention));
        }
        else
        {
            embed.WithColor(new DiscordColor(guildCfg.EmbedHexColor));
            embed.WithDescription(
                guildCfg.TicketingConfig.BaseWelcomeMessage.Replace("@ownerMention@", owner.Mention));
            embed.WithColor(new DiscordColor(guildCfg.EmbedHexColor));
        }

        embed.WithFooter($"Ticket Id: {req.GuildSpecificId}");

        var btn = new DiscordButtonComponent(ButtonStyle.Primary, nameof(TicketButton.TicketCloseButton),
            "Close this ticket", false,
            new DiscordComponentEmoji(DiscordEmoji.FromName(_discord.Client, ":lock:")));

        var msgBuilder = new DiscordMessageBuilder();
        msgBuilder.AddEmbed(embed.Build());
        msgBuilder.AddComponents(new List<DiscordComponent> { btn });
        msgBuilder.WithContent($"{owner.Mention} Welcome");

        var modRoles = guild.Roles.Where(x => x.Value.Permissions.HasPermission(Permissions.BanMembers));

        List<DiscordOverwriteBuilder> overwrites = modRoles.Select(role =>
            new DiscordOverwriteBuilder(role.Value).Allow(Permissions.AccessChannels)).ToList();
        overwrites.Add(new DiscordOverwriteBuilder(guild.EveryoneRole).Deny(Permissions.AccessChannels));
        overwrites.Add(new DiscordOverwriteBuilder(owner).Allow(Permissions.AccessChannels));

        string topic = $"Support ticket opened by user {owner.GetFullUsername()} at {DateTime.UtcNow}";

        DiscordChannel openedCat;
        try
        {
            openedCat = await _discord.Client.GetChannelAsync(guildCfg.TicketingConfig.OpenedCategoryId);
        }
        catch (Exception)
        {
            return new DiscordNotFoundError(
                $"Closed category channel with Id {guildCfg.TicketingConfig.OpenedCategoryId} doesn't exist");
        }

        try
        {
            DiscordChannel newTicketChannel = await guild.CreateChannelAsync(
                $"{guildCfg.TicketingConfig.OpenedNamePrefix}-{req.GuildSpecificId:D4}", ChannelType.Text,
                openedCat, topic, null, null, overwrites);
            DiscordMessage msg = await newTicketChannel.SendMessageAsync(msgBuilder);
            //Program.cachedMsgs.Add(msg.Id, msg);

            if (intr is not null)
            {
                var succEmbed = new DiscordEmbedBuilder();
                succEmbed.WithColor(new DiscordColor(guildCfg.EmbedHexColor));
                succEmbed.WithDescription($"Ticket created successfully! Channel: {newTicketChannel.Mention}");
                await intr.CreateFollowupMessageAsync(new DiscordFollowupMessageBuilder()
                    .AddEmbed(succEmbed.Build())
                    .AsEphemeral(true));
            }

            _ticketService.BeginUpdate(ticket);
            ticket.ChannelId = newTicketChannel.Id;
            ticket.MessageOpenId = msg.Id;
            await _ticketService.SetAddedUsersAsync(ticket, newTicketChannel.Users.Select(x => x.Id));

            List<ulong> roleIds = new();
            foreach (var overwrite in newTicketChannel.PermissionOverwrites)
            {
                if (overwrite.CheckPermission(Permissions.AccessChannels) != PermissionLevel.Allowed) continue;

                DiscordRole role;
                try
                {
                    role = await overwrite.GetRoleAsync();
                }
                catch (Exception)
                {
                    continue;
                }

                if (role is null) continue;

                roleIds.Add(role.Id);

                await Task.Delay(500);
            }

            await _ticketService.SetAddedRolesAsync(ticket, roleIds);
            await _ticketService.CommitAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError($"Errored while opening new ticket: {ex.GetFullMessage()}");
            return ex;
        }

        return msgBuilder;
    }

    private async Task<Result<DiscordMessageBuilder>> CloseTicketAsync(DiscordGuild guild, DiscordChannel target,
        DiscordMember requestingMember, TicketCloseReqDto req)
    {
        if (guild is null) throw new ArgumentNullException(nameof(guild));
        if (target is null) throw new ArgumentNullException(nameof(target));
        if (requestingMember is null) throw new ArgumentNullException(nameof(requestingMember));
        if (req is null) throw new ArgumentNullException(nameof(req));

        if (requestingMember.Guild.Id != guild.Id) return new DiscordNotAuthorizedError(nameof(requestingMember));
        if (target.Guild.Id != guild.Id) return new DiscordNotAuthorizedError(nameof(target));

        var guildRes =
            await _guildService.GetSingleBySpecAsync<Guild>(
                new ActiveGuildByDiscordIdWithTicketingSpecifications(guild.Id));

        if (!guildRes.IsDefined()) return Result<DiscordMessageBuilder>.FromError(guildRes);

        if (guildRes.Entity.TicketingConfig is null)
            return new DisabledEntityError($"Guild with Id:{guild.Id} doesn't have ticketing enabled.");

        var guildCfg = guildRes.Entity;

        var res = await _ticketService.GetSingleBySpecAsync(
            new TicketByChannelIdOrGuildAndOwnerIdSpec(req.ChannelId, req.GuildId, req.OwnerId));

        if (!res.IsDefined(out var ticket)) return new NotFoundError($"Ticket with channel Id: {target.Id} doesn't exist.");

        if (ticket.IsDisabled)
            return new DisabledEntityError(
                $"Ticket with Id: {ticket.GuildSpecificId}, TargetUserId: {ticket.UserId}, GuildId: {ticket.GuildId}, ChannelId: {ticket.ChannelId} is already closed.");

        if (ticket.UserId != requestingMember.Id &&
            !requestingMember.Permissions.HasPermission(Permissions.BanMembers))
            return new DiscordNotAuthorizedError(
                "Requesting member doesn't have moderator rights or isn't the ticket's owner.");

        var embed = new DiscordEmbedBuilder();

        embed.WithColor(new DiscordColor(guildCfg.EmbedHexColor));
        embed.WithAuthor("Ticket closed");
        embed.AddField("Requested by", requestingMember.Mention);
        embed.WithFooter($"Ticket Id: {ticket.GuildSpecificId}");

        var options = new List<DiscordSelectComponentOption>
        {
            new("Reopen", nameof(TicketSelectValue.TicketReopenValue), "Reopens this ticket",
                false, new DiscordComponentEmoji(DiscordEmoji.FromName(_discord.Client, ":unlock:"))),
            new("Transcript", nameof(TicketSelectValue.TicketTranscriptValue),
                "Generates HTML transcript for this ticket",
                false, new DiscordComponentEmoji(DiscordEmoji.FromName(_discord.Client, ":blue_book:")))
        };
        var selectDropdown = new DiscordSelectComponent(nameof(TicketSelect.TicketCloseMessageSelect),
            "Choose an action", options);

        var msgBuilder = new DiscordMessageBuilder();
        msgBuilder.AddEmbed(embed.Build());
        msgBuilder.AddComponents(selectDropdown);

        DiscordMessage closeMsg;
        try
        {
            closeMsg = await target.SendMessageAsync(msgBuilder);
        }
        catch (Exception)
        {
            return new DiscordError($"Couldn't send ticket close message in channel with Id: {target.Id}");
        }

        DiscordMember? owner;
        try
        {
            owner = requestingMember.Id == ticket.UserId
                ? requestingMember // means requested by owner so we don't need to grab the owner again
                : await guild.GetMemberAsync(ticket.UserId);

            await target.AddOverwriteAsync(owner, deny: Permissions.AccessChannels);
        }
        catch
        {
            owner = null;
            // ignored
        }

        await Task.Delay(500);

        await target.ModifyAsync(x =>
            x.Name = $"{guildCfg.TicketingConfig.ClosedNamePrefix}-{ticket.GuildSpecificId:D4}");

        DiscordChannel closedCat;
        try
        {
            closedCat = await _discord.Client.GetChannelAsync(guildCfg.TicketingConfig.ClosedCategoryId);
        }
        catch (Exception ex)
        {
            throw new DiscordNotFoundException(
                $"Closed category channel with Id {guildCfg.TicketingConfig.ClosedCategoryId} doesn't exist", ex);
        }

        await target.ModifyAsync(x => x.Parent = closedCat);

        req.ClosedMessageId = closeMsg.Id;
        await _ticketService.CloseAsync(req, ticket);

        if (ticket.IsPrivate) return msgBuilder;

        _ = _asyncExecutor.ExecuteAsync<IDiscordChatExportService>(async x => await x.ExportToHtmlAsync(guild,
            target, requestingMember,
            owner ?? await _discord.Client.GetUserAsync(ticket.UserId), ticket));

        return msgBuilder;
    }

    private async Task<Result<DiscordMessageBuilder>> ReopenTicketAsync(DiscordGuild guild, DiscordChannel target,
        DiscordMember requestingMember, TicketReopenReqDto req)
    {
        if (guild is null) throw new ArgumentNullException(nameof(guild));
        if (target is null) throw new ArgumentNullException(nameof(target));
        if (requestingMember is null) throw new ArgumentNullException(nameof(requestingMember));
        if (req is null) throw new ArgumentNullException(nameof(req));

        if (requestingMember.Guild.Id != guild.Id) throw new ArgumentException(nameof(requestingMember));
        if (target.Guild.Id != guild.Id) throw new ArgumentException(nameof(target));

        var guildRes =
            await _guildService.GetSingleBySpecAsync<Guild>(
                new ActiveGuildByDiscordIdWithTicketingSpecifications(guild.Id));

        if (!guildRes.IsDefined()) return Result<DiscordMessageBuilder>.FromError(guildRes);

        if (guildRes.Entity.TicketingConfig is null)
            return new DisabledEntityError($"Guild with Id:{guild.Id} doesn't have ticketing enabled.");

        var guildCfg = guildRes.Entity;

        var res = await _ticketService.GetSingleBySpecAsync(new TicketByChannelIdOrGuildAndOwnerIdSpec(req.ChannelId, req.GuildId, req.OwnerId));

        if (!res.IsDefined(out var ticket)) return new NotFoundError($"Ticket with channel Id: {target.Id} doesn't exist.");

        if (!ticket.IsDisabled)
            return new DisabledEntityError(
                $"Ticket with Id: {ticket.GuildSpecificId}, TargetUserId: {ticket.UserId}, GuildId: {ticket.GuildId}, ChannelId: {ticket.ChannelId} is already opened.");

        if (!requestingMember.IsModerator())
            return new DiscordNotAuthorizedError("Requesting member doesn't have moderator rights.");

        var embed = new DiscordEmbedBuilder();

        embed.WithColor(new DiscordColor(guildCfg.EmbedHexColor));
        embed.WithAuthor("Ticket reopened");
        embed.AddField("Requested by", requestingMember.Mention);
        embed.WithFooter($"Ticket Id: {ticket.GuildSpecificId}");

        var msgBuilder = new DiscordMessageBuilder();
        msgBuilder.AddEmbed(embed.Build());

        DiscordMessage reopenMsg;
        try
        {
            reopenMsg = await target.SendMessageAsync(msgBuilder);
        }
        catch (Exception)
        {
            return new DiscordError($"Couldn't send ticket close message in channel with Id: {target.Id}");
        }

        try
        {
            DiscordMember owner = requestingMember.Id == ticket.UserId
                ? requestingMember // means requested by owner so we don't need to grab the owner again
                : await guild.GetMemberAsync(ticket.UserId);

            await target.AddOverwriteAsync(owner, Permissions.AccessChannels);
        }
        catch
        {
            _logger.LogDebug($"User left the guild before reopening the ticket with Id: {ticket.Id}");
        }

        await Task.Delay(500);

        await target.ModifyAsync(x =>
            x.Name = $"{guildCfg.TicketingConfig.OpenedNamePrefix}-{ticket.GuildSpecificId:D4}");

        DiscordChannel openedCat;
        try
        {
            openedCat = await _discord.Client.GetChannelAsync(guildCfg.TicketingConfig.OpenedCategoryId);
        }
        catch (Exception)
        {
            return new DiscordNotFoundError(
                $"Closed category channel with Id {guildCfg.TicketingConfig.OpenedCategoryId} doesn't exist");
        }

        await target.ModifyAsync(x => x.Parent = openedCat);

        if (ticket.MessageCloseId.HasValue)
            await target.DeleteMessageAsync(await target.GetMessageAsync(ticket.MessageCloseId.Value));

        req.ReopenMessageId = reopenMsg.Id;
        await _ticketService.ReopenAsync(req, ticket);

        return msgBuilder;
    }

    private async Task<Result<DiscordEmbed>> AddToTicketAsync(DiscordGuild guild, DiscordMember requestingMember,
        DiscordChannel targetTicketChannel, TicketAddReqDto req, DiscordMember? targetMember = null,
        DiscordRole? targetRole = null)
    {
        if (guild is null) throw new ArgumentNullException(nameof(guild));
        if (req is null) throw new ArgumentNullException(nameof(req));
        if (targetTicketChannel is null) throw new ArgumentNullException(nameof(targetTicketChannel));
        if (requestingMember is null) throw new ArgumentNullException(nameof(requestingMember));
        if (targetRole is null && targetMember is null)
            throw new ArgumentException($"Both {nameof(targetRole)} and {nameof(targetMember)} were null.");

        if (requestingMember.Guild.Id != guild.Id) throw new ArgumentException(nameof(requestingMember));
        if (targetTicketChannel.Guild.Id != guild.Id) throw new ArgumentException(nameof(targetTicketChannel));

        var guildRes =
            await _guildService.GetSingleBySpecAsync<Guild>(
                new ActiveGuildByDiscordIdWithTicketingSpecifications(guild.Id));

        if (!guildRes.IsDefined()) return Result<DiscordEmbed>.FromError(guildRes);

        if (guildRes.Entity.TicketingConfig is null)
            return new DisabledEntityError($"Guild with Id:{guild.Id} doesn't have ticketing enabled.");

        var guildCfg = guildRes.Entity;

        var res = await _ticketService.GetSingleBySpecAsync<Ticket>(
            new TicketBaseGetSpecifications(req.Id, req.OwnerId, req.GuildId, req.ChannelId, req.GuildSpecificId));


        if (!res.IsDefined()) return new NotFoundError($"Ticket with channel Id: {req.ChannelId} doesn't exist.");

        var ticket = res.Entity;

        if (ticket.IsDisabled)
            return new DisabledEntityError(
                $"Ticket with Id: {ticket.GuildSpecificId}, TargetUserId: {ticket.UserId}, GuildId: {ticket.GuildId}, ChannelId: {ticket.ChannelId} is already closed.");

        if (!requestingMember.Permissions.HasPermission(Permissions.BanMembers))
            return new DiscordNotAuthorizedError("Requesting member doesn't have moderator rights.");

        if (targetRole is null)
        {
            await targetTicketChannel.AddOverwriteAsync(targetMember, Permissions.AccessChannels);
            await _ticketService.SetAddedUsersAsync(ticket, targetTicketChannel.Users.Select(x => x.Id));
            await _ticketService.CheckAndSetPrivacyAsync(ticket, guild);
        }
        else
        {
            await targetTicketChannel.AddOverwriteAsync(targetRole, Permissions.AccessChannels);
            List<ulong> roleIds = new();
            foreach (var overwrite in targetTicketChannel.PermissionOverwrites)
            {
                if (overwrite.CheckPermission(Permissions.AccessChannels) != PermissionLevel.Allowed) continue;

                DiscordRole role;
                try
                {
                    role = await overwrite.GetRoleAsync();
                }
                catch (Exception)
                {
                    continue;
                }

                if (role is null) continue;

                roleIds.Add(role.Id);

                await Task.Delay(500);
            }

            await _ticketService.SetAddedRolesAsync(ticket, roleIds);
            await _ticketService.CheckAndSetPrivacyAsync(ticket, guild);
        }

        var embed = new DiscordEmbedBuilder();

        embed.WithColor(new DiscordColor(guildCfg.EmbedHexColor));
        embed.WithAuthor($"Ticket moderation | Add {(targetRole is null ? "member" : "role")} action log");
        embed.AddField("Moderator", requestingMember.Mention);
        embed.AddField("Added", $"{targetRole?.Mention ?? targetMember?.Mention}");
        embed.WithFooter($"Ticket Id: {ticket.GuildSpecificId}");

        return embed.Build();
    }

    private async Task<Result<DiscordEmbed>> RemoveFromTicketAsync(DiscordGuild guild,
        DiscordMember requestingMember,
        DiscordChannel targetTicketChannel, TicketRemoveReqDto req, DiscordMember? targetMember = null,
        DiscordRole? targetRole = null)
    {
        if (req is null) throw new ArgumentNullException(nameof(req));
        if (guild is null) throw new ArgumentNullException(nameof(guild));
        if (targetTicketChannel is null) throw new ArgumentNullException(nameof(targetTicketChannel));
        if (requestingMember is null) throw new ArgumentNullException(nameof(requestingMember));
        if (targetRole is null && targetMember is null)
            throw new ArgumentException($"Both {nameof(targetRole)} and {nameof(targetMember)} were null.");

        if (requestingMember.Guild.Id != guild.Id) throw new ArgumentException(nameof(requestingMember));
        if (targetTicketChannel.Guild.Id != guild.Id) throw new ArgumentException(nameof(targetTicketChannel));

        var guildRes =
            await _guildService.GetSingleBySpecAsync<Guild>(
                new ActiveGuildByDiscordIdWithTicketingSpecifications(guild.Id));

        if (!guildRes.IsDefined()) return Result<DiscordEmbed>.FromError(guildRes);

        if (guildRes.Entity.TicketingConfig is null)
            return new DisabledEntityError($"Guild with Id:{guild.Id} doesn't have ticketing enabled.");

        var guildCfg = guildRes.Entity;

        var res = await _ticketService.GetSingleBySpecAsync<Ticket>(
            new TicketBaseGetSpecifications(req.Id, req.OwnerId, req.GuildId, req.ChannelId, req.GuildSpecificId));

        if (!res.IsDefined()) return new NotFoundError($"Ticket with channel Id: {req.ChannelId} doesn't exist.");

        var ticket = res.Entity;

        if (ticket.IsDisabled)
            return new DisabledEntityError(
                $"Ticket with Id: {ticket.GuildSpecificId}, TargetUserId: {ticket.UserId}, GuildId: {ticket.GuildId}, ChannelId: {ticket.ChannelId} is already closed.");

        if (!requestingMember.Permissions.HasPermission(Permissions.BanMembers))
            return new DiscordNotAuthorizedError("Requesting member doesn't have moderator rights.");

        if (targetRole is null)
        {
            await targetTicketChannel.AddOverwriteAsync(targetMember, deny: Permissions.AccessChannels);
            await _ticketService.SetAddedUsersAsync(ticket, targetTicketChannel.Users.Select(x => x.Id));
            await _ticketService.CheckAndSetPrivacyAsync(ticket, guild);
        }
        else
        {
            await targetTicketChannel.AddOverwriteAsync(targetRole, deny: Permissions.AccessChannels);

            List<ulong> roleIds = new();
            foreach (var overwrite in targetTicketChannel.PermissionOverwrites)
            {
                if (overwrite.CheckPermission(Permissions.AccessChannels) != PermissionLevel.Allowed) continue;

                DiscordRole role;
                try
                {
                    role = await overwrite.GetRoleAsync();
                }
                catch (Exception)
                {
                    continue;
                }

                if (role is null) continue;

                roleIds.Add(role.Id);

                await Task.Delay(500);
            }

            await _ticketService.SetAddedRolesAsync(ticket, roleIds);
            await _ticketService.CheckAndSetPrivacyAsync(ticket, guild);
        }

        var embed = new DiscordEmbedBuilder();

        embed.WithColor(new DiscordColor(guildCfg.EmbedHexColor));
        embed.WithAuthor($"Ticket moderation | Remove {(targetRole is null ? "member" : "role")} action log");
        embed.AddField("Moderator", requestingMember.Mention);
        embed.AddField("Removed", $"{targetRole?.Mention ?? targetMember?.Mention}");
        embed.WithFooter($"Ticket Id: {ticket.GuildSpecificId}");

        return embed.Build();
    }

    private async
        Task<Result<(Ticket Ticket, DiscordMember RequestingMember, DiscordGuild Guild, DiscordChannel Channel)>>
        GetTicketByBaseRequestAsync(BaseTicketGetReqDto req)
    {
        if (req is null) throw new ArgumentNullException(nameof(req));

        DiscordMember requestingMember;
        DiscordChannel? targetTicketChannel = null;
        Ticket ticket;

        if (req.Id.HasValue)
        {
            var res = await _ticketService.GetAsync(req.Id.Value);
            if (!res.IsDefined())
                return Result<(Ticket Ticket, DiscordMember RequestingMember, DiscordGuild Guild, DiscordChannel
                    Channel)>.FromError(res);

            ticket = res.Entity;
            req.ChannelId = ticket.ChannelId;
            req.GuildId = ticket.GuildId;
        }
        else if (req.OwnerId.HasValue && req.GuildId.HasValue)
        {
            var res = await _ticketService.GetSingleBySpecAsync<Ticket>(
                new TicketBaseGetSpecifications(null, req.OwnerId, req.GuildId, null, null, false, 1));
            if (!res.IsDefined())
                return Result<(Ticket Ticket, DiscordMember RequestingMember, DiscordGuild Guild, DiscordChannel
                    Channel)>.FromError(res);

            ticket = res.Entity;
            req.ChannelId = ticket.ChannelId;
            req.GuildId = ticket.GuildId;
        }
        else if (req.GuildSpecificId.HasValue && req.GuildId.HasValue)
        {
            var res = await _ticketService.GetSingleBySpecAsync<Ticket>(
                new TicketBaseGetSpecifications(null, null, req.GuildId, null, req.GuildSpecificId, false, 1));
            if (!res.IsDefined())
                return Result<(Ticket Ticket, DiscordMember RequestingMember, DiscordGuild Guild, DiscordChannel
                    Channel)>.FromError(res);

            ticket = res.Entity;
            req.ChannelId = ticket.ChannelId;
            req.GuildId = ticket.GuildId;
        }
        else
        {
            try
            {
                targetTicketChannel =
                    await _discord.Client.GetChannelAsync(req.ChannelId ?? throw new InvalidOperationException());
            }
            catch (Exception)
            {
                return new DiscordNotFoundError(DiscordEntity.Channel);
            }

            var res = await _ticketService.GetSingleBySpecAsync<Ticket>(
                new TicketBaseGetSpecifications(null, null, null, req.ChannelId.Value, null, false, 1));
            if (!res.IsDefined())
                return Result<(Ticket Ticket, DiscordMember RequestingMember, DiscordGuild Guild, DiscordChannel
                    Channel)>.FromError(res);

            ticket = res.Entity;
        }

        targetTicketChannel ??= await _discord.Client.GetChannelAsync(req.ChannelId.Value);

        var guild = targetTicketChannel.Guild;

        try
        {
            requestingMember = await guild.GetMemberAsync(req.RequestedOnBehalfOfId);
        }
        catch (Exception)
        {
            return new DiscordNotFoundError(DiscordEntity.Member);
        }

        return (ticket, requestingMember, guild, targetTicketChannel);
    }
}