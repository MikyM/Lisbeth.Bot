// This file is part of Lisbeth.Bot project
//
// Copyright (C) 2021 MikyM
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

using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using Lisbeth.Bot.Application.Discord.Extensions;
using Lisbeth.Bot.Application.Discord.Services.Interfaces;
using Lisbeth.Bot.DataAccessLayer.Specifications.MuteSpecifications;
using Lisbeth.Bot.Domain.DTOs.Request;
using Lisbeth.Bot.Domain.Entities;
using MikyM.Discord.Interfaces;
using System;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Lisbeth.Bot.Application.Services.Interfaces;
using MikyM.Common.DataAccessLayer.Specifications;

namespace Lisbeth.Bot.Application.Discord.Services
{
    [UsedImplicitly]
    public class DiscordMuteService : IDiscordMuteService
    {
        private readonly IDiscordService _discord;
        private readonly IMuteService _muteService;
        private readonly IGuildService _guildService;

        public DiscordMuteService(IDiscordService discord, IMuteService muteService, IGuildService guildService)
        {
            _muteService = muteService;
            _discord = discord;
            _guildService = guildService;
        }

        public async Task<DiscordEmbed> MuteAsync(MuteReqDto req)
        {
            if (req is null) throw new ArgumentNullException(nameof(req));

            DiscordMember target;
            DiscordMember moderator;
            DiscordGuild guild;

            try
            {
                guild = await _discord.Client.GetGuildAsync(req.GuildId);
            }
            catch (Exception)
            {
                throw new ArgumentException($"Guild with Id: {req.GuildId} doesn't exist.");
            }

            try
            {
                target = await guild.GetMemberAsync(req.TargetUserId);
            }
            catch (Exception)
            {
                throw new ArgumentException($"User with Id: {req.TargetUserId} doesn't exist or isn't this guild's target.");
            }

            try
            {
                moderator = await guild.GetMemberAsync(req.RequestedOnBehalfOfId);
            }
            catch (Exception)
            {
                throw new ArgumentException($"User with Id: {req.RequestedOnBehalfOfId} doesn't exist or isn't this guild's target.");
            }

            return await MuteAsync(guild, target, moderator, req.AppliedUntil, req.Reason, req);
        }

        public async Task<DiscordEmbed> MuteAsync(InteractionContext ctx, DateTime appliedUntil, string reason = "")
        {
            if (ctx is null) throw new ArgumentNullException(nameof(ctx));

            return await MuteAsync(ctx.Guild, (DiscordMember)ctx.ResolvedUserMentions[0], ctx.Member, appliedUntil, reason);
        }

        public async Task<DiscordEmbed> MuteAsync(ContextMenuContext ctx, DateTime appliedUntil, string reason = "")
        {
            if (ctx is null) throw new ArgumentNullException(nameof(ctx));

            return await MuteAsync(ctx.Guild, ctx.TargetMember, ctx.Member, appliedUntil, reason);
        }

        private async Task<DiscordEmbed> MuteAsync(DiscordGuild guild, DiscordMember target, DiscordMember moderator, DateTime appliedUntil, string reason = "", MuteReqDto req = null)
        {
            if (guild is null) throw new ArgumentNullException(nameof(guild));
            if (target is null) throw new ArgumentNullException(nameof(target));
            if (moderator is null) throw new ArgumentNullException(nameof(moderator));

            DiscordChannel channel = null;

            var guildRes =
                await _guildService.GetBySpecificationsAsync<Guild>(
                    new Specifications<Guild>(x => x.GuildId == guild.Id && !x.IsDisabled));
            var guildCfg = guildRes.FirstOrDefault();

            if (guildCfg is null) throw new ArgumentException($"Guild with Id: {guild.Id} doesn't exist in the database.");

            if(!guildCfg.IsModerationEnabled) throw new ArgumentException($"Guild with Id: {guild.Id} doesn't have moderation module enabled.");

            if (guildCfg.LogChannelId is not null)
            {
                try
                {
                    channel = await _discord.Client.GetChannelAsync(guildCfg.LogChannelId.Value);
                }
                catch (Exception)
                {
                    throw new ArgumentException($"Log channel with Id: {guildCfg.LogChannelId} doesn't exist.");
                }
            }

            if (appliedUntil < DateTime.UtcNow) throw new ArgumentException("Mute until date must be in the future.");

            TimeSpan tmspDuration = appliedUntil.Subtract(DateTime.UtcNow);

            string lengthString = appliedUntil == DateTime.MaxValue ? "Permanent" : $"{tmspDuration.Days} days, {tmspDuration.Hours} hrs, {tmspDuration.Minutes} mins";

            req ??= new MuteReqDto(target.Id, guild.Id, moderator.Id, appliedUntil, reason);

            var (id, foundEntity) = await _muteService.AddOrExtendAsync(req, true);

            var embed = new DiscordEmbedBuilder();
            embed.WithColor(0x18315C);
            embed.WithAuthor($"Mute | {target.GetFullDisplayName()}", null, target.AvatarUrl);

            bool isMuted = target.Roles.FirstOrDefault(r => r.Name == "Muted") is not null;
            bool resMute = true;

            if (foundEntity is null)
            {
                if (!isMuted)
                    resMute = await target.Mute(guild);

                if (!resMute)
                {
                    var noEntryEmoji = DiscordEmoji.FromName(_discord.Client, ":no_entry:");
                    embed.WithColor(0x18315C);
                    embed.WithAuthor($"{noEntryEmoji} Mute denied");
                    embed.WithDescription("Can't mute other moderators.");
                }
                else
                {
                    embed.AddField("User mention", target.Mention, true);
                    embed.AddField("Moderator", moderator.Mention, true);
                    embed.AddField("Length", lengthString, true);
                    embed.AddField("Muted until", req.AppliedUntil.ToString(CultureInfo.InvariantCulture), true);
                    embed.AddField("Reason", req.Reason);
                    embed.WithFooter($"Case ID: {id} | Member ID: {target.Id}");
                }
            }
            else
            {
                DiscordUser previousMod;
                try
                {
                    previousMod = await _discord.Client.GetUserAsync(foundEntity.AppliedById);
                }
                catch (Exception)
                {
                    previousMod = null;
                }

                if (foundEntity.AppliedUntil > req.AppliedUntil)
                {
                    if (!isMuted)
                        resMute = await target.Mute(guild);

                    if (!resMute)
                    {
                        var noEntryEmoji = DiscordEmoji.FromName(_discord.Client, ":no_entry:");
                        embed.WithColor(0x18315C);
                        embed.WithAuthor($"{noEntryEmoji} MuteAsync denied");
                        embed.WithDescription("Can't mute other moderators.");
                    }
                    else
                    {
                        embed.WithDescription($"This user has already been muted until {foundEntity.AppliedUntil} by {(previousMod is not null ? previousMod.Mention : "a deleted user")}");
                        embed.WithFooter($"Case ID: {id} | Member ID: {foundEntity.UserId}");
                    }
                }
                else
                {
                    if (!isMuted)
                        resMute = await target.Mute(guild);

                    if (!resMute)
                    {
                        var noEntryEmoji = DiscordEmoji.FromName(_discord.Client, ":no_entry:");
                        embed.WithColor(0x18315C);
                        embed.WithAuthor($"{noEntryEmoji} MuteAsync denied");
                        embed.WithDescription("Can't mute other moderators.");
                    }
                    else
                    {
                        embed.WithAuthor($"Extend Mute | {target.GetFullUsername()}", null, target.AvatarUrl);
                        embed.AddField("Previous mute until", foundEntity.AppliedUntil.ToString(), true);
                        embed.AddField("Previous moderator", $"{(previousMod is not null ? previousMod.Mention : "Deleted user")}", true);
                        embed.AddField("Previous reason", foundEntity.Reason, true);
                        embed.AddField("User mention", target.Mention, true);
                        embed.AddField("Moderator", moderator.Mention, true);
                        embed.AddField("Length", lengthString, true);
                        embed.AddField("Muted until", req.AppliedUntil.ToString(CultureInfo.InvariantCulture), true);
                        embed.AddField("Reason", req.Reason);
                        embed.WithFooter($"Case ID: {id} | Member ID: {target.Id}");
                    }
                }
            }

            if (guildCfg.LogChannelId is null) return embed; // means we're not sending to log channel

            // means we're logging to log channel and returning an embed for interaction or other purposes

            try
            {
                if (channel is not null) await channel.SendMessageAsync(embed.Build());
            }
            catch (Exception)
            {
                throw new ArgumentException($"Can't send messages in channel with Id: {channel?.Id}.");
            }

            return embed;
        }

        public async Task<DiscordEmbed> UnmuteAsync(MuteDisableReqDto req)
        {
            if (req is null) throw new ArgumentNullException(nameof(req));

            DiscordMember target;
            DiscordMember moderator;
            DiscordGuild guild;
            Mute mute;

            if (req.Id is null && (req.GuildId is null || req.TargetUserId is null))
                throw new ArgumentException("You must supply either mute Id or guild Id and user Id");

            if (req.Id is not null)
            {
                mute = await _muteService.GetAsync<Mute>(req.Id.Value);
                if (mute is null) throw new ArgumentException("Mute not found");
                req.GuildId = mute.GuildId;
                req.TargetUserId = mute.UserId;
            }

            try
            {
                guild = await _discord.Client.GetGuildAsync(req.GuildId.Value);
            }
            catch (Exception)
            {
                throw new ArgumentException($"Guild with Id: {req.GuildId} doesn't exist.");
            }

            try
            {
                target = await guild.GetMemberAsync(req.TargetUserId.Value);
            }
            catch (Exception)
            {
                throw new ArgumentException($"User with Id: {req.TargetUserId} doesn't exist or isn't this guild's target.");
            }

            try
            {
                moderator = await guild.GetMemberAsync(req.RequestedOnBehalfOfIdId);
            }
            catch (Exception)
            {
                throw new ArgumentException($"User with Id: {req.RequestedOnBehalfOfIdId} doesn't exist or isn't this guild's target.");
            }

            return await UnmuteAsync(guild, target, moderator, req);
        }

        public async Task<DiscordEmbed> UnmuteAsync(InteractionContext ctx)
        {
            if (ctx is null) throw new ArgumentNullException(nameof(ctx));

            return await UnmuteAsync(ctx.Guild, (DiscordMember)ctx.ResolvedUserMentions[0], ctx.Member);
        }

        public async Task<DiscordEmbed> UnmuteAsync(ContextMenuContext ctx)
        {
            if (ctx is null) throw new ArgumentNullException(nameof(ctx));

            return await UnmuteAsync(ctx.Guild, ctx.TargetMember, ctx.Member);
        }

        private async Task<DiscordEmbed> UnmuteAsync(DiscordGuild guild, DiscordMember target, DiscordUser moderator, MuteDisableReqDto req = null)
        {
            if (guild is null) throw new ArgumentNullException(nameof(guild));
            if (target is null) throw new ArgumentNullException(nameof(target));
            if (moderator is null) throw new ArgumentNullException(nameof(moderator));

            DiscordChannel channel = null;

            var guildRes =
                await _guildService.GetBySpecificationsAsync<Guild>(
                    new Specifications<Guild>(x => x.GuildId == guild.Id && !x.IsDisabled));
            var guildCfg = guildRes.FirstOrDefault();

            if (guildCfg is null) throw new ArgumentException($"Guild with Id: {guild.Id} doesn't exist in the database.");

            if (!guildCfg.IsModerationEnabled) throw new ArgumentException($"Guild with Id: {guild.Id} doesn't have moderation module enabled.");

            if (guildCfg.LogChannelId is not null)
            {
                try
                {
                    channel = await _discord.Client.GetChannelAsync(guildCfg.LogChannelId.Value);
                }
                catch (Exception)
                {
                    throw new ArgumentException($"Log channel with Id: {guildCfg.LogChannelId} doesn't exist.");
                }
            }

            var embed = new DiscordEmbedBuilder();
            embed.WithColor(0x18315C);

            bool isMuted = target.Roles.FirstOrDefault(r => r.Name == "Muted") is not null;

            req ??= new MuteDisableReqDto(target.Id, guild.Id, moderator.Id);

            var res = await _muteService.DisableAsync(req);

            if (res is null)
            {
                if (isMuted)
                {
                    await target.Unmute(guild);
                    embed.WithAuthor($"Unmute | {target.GetFullUsername()}", null, target.AvatarUrl);
                    embed.AddField("Moderator", moderator.Mention, true);
                    embed.AddField("User mention", target.Mention, true);
                    embed.WithDescription($"Successfully unmuted");
                    embed.WithFooter($"Case ID: unknown | Member ID: {target.Id}");
                }
                else
                {
                    embed.WithAuthor($"Unmute failed | {target.GetFullDisplayName()}", null, target.AvatarUrl);
                    embed.WithDescription($"This user isn't currently muted.");
                    embed.WithFooter($"Case ID: unknown | Member ID: {target.Id}");
                }
            }
            else
            {
                await _muteService.CommitAsync();

                if(isMuted)
                    await target.Unmute(guild);

                embed.WithAuthor($"Unmute | {target.GetFullDisplayName()}", null, target.AvatarUrl);
                embed.AddField("Moderator", moderator.Mention, true);
                embed.AddField("User mention", target.Mention, true);
                embed.WithDescription($"Successfully unmuted");
                embed.WithFooter($"Case ID: {res.Id} | Member ID: {target.Id}");
            }

            if (guildCfg.LogChannelId is null) return embed; // means we're not sending to log channel

            // means we're logging to log channel and returning an embed for interaction or other purposes

            try
            {
                await channel.SendMessageAsync(embed.Build());
            }
            catch (Exception)
            {
                throw new ArgumentException($"Can't send messages in channel with Id: {channel.Id}.");
            }

            return embed;
        }

        public async Task<DiscordEmbed> GetSpecificUserGuildMuteAsync(MuteGetReqDto req)
        {
            if (req is null) throw new ArgumentNullException(nameof(req));

            DiscordMember target = null;
            DiscordMember moderator = null;
            DiscordGuild guild = null;

            if (req.Id is null && (req.GuildId is null || req.TargetUserId is null))
                throw new ArgumentException("You must supply either mute Id or guild Id and user Id");

            if (req.Id is not null)
            {
                var mute = await _muteService.GetAsync<Ban>(req.Id.Value);
                if (mute is null) throw new ArgumentException("Mute not found");
                req.GuildId = mute.GuildId;
                req.TargetUserId = mute.UserId;
                req.AppliedById = mute.AppliedById;
                req.LiftedById = mute.LiftedById;
                req.AppliedOn = mute.AppliedOn;
            }

            try
            {
                guild = await _discord.Client.GetGuildAsync(req.GuildId.Value);
            }
            catch (Exception)
            {
                throw new ArgumentException($"Guild with Id: {req.GuildId} doesn't exist.");
            }

            try
            {
                target = await guild.GetMemberAsync(req.TargetUserId.Value);
            }
            catch (Exception)
            {
                throw new ArgumentException($"User with Id: {req.TargetUserId} doesn't exist or isn't this guild's target.");
            }

            try
            {
                moderator = await guild.GetMemberAsync(req.RequestedOnBehalfOfId);
            }
            catch (Exception)
            {
                throw new ArgumentException($"User with Id: {req.RequestedOnBehalfOfId} doesn't exist or isn't this guild's target.");
            }

            return await GetAsync(guild, target, moderator, req);
        }

        public async Task<DiscordEmbed> GetAsync(InteractionContext ctx)
        {
            if (ctx is null) throw new ArgumentNullException(nameof(ctx));

            return await GetAsync(ctx.Guild, (DiscordMember)ctx.ResolvedUserMentions[0], ctx.Member);
        }

        public async Task<DiscordEmbed> GetAsync(ContextMenuContext ctx)
        {
            if (ctx is null) throw new ArgumentNullException(nameof(ctx));

            return await GetAsync(ctx.Guild, ctx.TargetMember, ctx.Member);
        }

        private async Task<DiscordEmbed> GetAsync(DiscordGuild guild, DiscordMember target, DiscordMember moderator, MuteGetReqDto req = null)
        {
            if (guild is null) throw new ArgumentNullException(nameof(guild));
            if (target is null) throw new ArgumentNullException(nameof(target));
            if (moderator is null) throw new ArgumentNullException(nameof(moderator));

            var guildRes =
                await _guildService.GetBySpecificationsAsync<Guild>(
                    new Specifications<Guild>(x => x.GuildId == guild.Id && !x.IsDisabled));
            var guildCfg = guildRes.FirstOrDefault();

            if (guildCfg is null) throw new ArgumentException($"Guild with Id: {guild.Id} doesn't exist in the database.");

            if (!guildCfg.IsModerationEnabled) throw new ArgumentException($"Guild with Id: {guild.Id} doesn't have moderation module enabled.");

            DiscordChannel channel = null;

            if (guildCfg.LogChannelId is not null)
            {
                try
                {
                    channel = await _discord.Client.GetChannelAsync(guildCfg.LogChannelId.Value);
                }
                catch (Exception)
                {
                    throw new ArgumentException($"Log channel with Id: {guildCfg.LogChannelId} doesn't exist.");
                }
            }

            req ??= new MuteGetReqDto(moderator.Id, null, target.Id, guild.Id);

            var res = await _muteService.GetBySpecificationsAsync<Mute>(
                new MuteBaseGetSpecifications(req.Id, req.TargetUserId, req.GuildId, req.AppliedById, req.LiftedOn, req.AppliedOn, req.LiftedById));

            var mute = res.FirstOrDefault();
            
            var embed = new DiscordEmbedBuilder();
            embed.WithColor(0x18315C);

            if (mute is not null)
            {
                DiscordUser mutingMod = null;
                try
                {
                    mutingMod = await _discord.Client.GetUserAsync(mute.AppliedById);
                }
                catch (Exception)
                {
                    // ignore
                }

                embed.WithAuthor($"Mute Info | {target.GetFullDisplayName()}", null, target.AvatarUrl);
                embed.AddField("User mention", target.Mention, true);
                embed.AddField("Moderator", $"{(mutingMod is not null ? mutingMod.Mention : "Deleted user")}", true);
                embed.AddField("Muted until", mute.AppliedUntil.ToString(), true);
                embed.AddField("Reason", mute.Reason);
                if (mute.LiftedById != 0)
                {
                    DiscordUser liftingMod = null;
                    try
                    {
                        liftingMod = await _discord.Client.GetUserAsync(mute.LiftedById);
                    }
                    catch (Exception)
                    {
                        // ignore
                    }

                    embed.AddField("Was lifted by", $"{(liftingMod is not null ? liftingMod.Mention : "Deleted user")}");
                }
                embed.WithFooter($"Case ID: {mute.Id} | User ID: {target.Id}");
            }
            else
            {
                embed.WithDescription("No mute info found.");
                embed.WithFooter($"Case ID: unknown | User ID: {target.Id}");
            }

            if (guildCfg.LogChannelId is null) return embed; // means we're not sending to log channel

            // means we're logging to log channel and returning an embed for interaction or other purposes

            try
            {
                await channel.SendMessageAsync(embed.Build());
            }
            catch (Exception)
            {
                throw new ArgumentException($"Can't send messages in channel with Id: {channel.Id}.");
            }

            return embed;
        }
    }
}
