using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using JetBrains.Annotations;
using Lisbeth.Bot.Application.Discord.Extensions;
using Lisbeth.Bot.Application.Extensions;
using Lisbeth.Bot.Application.Interfaces;
using Lisbeth.Bot.Domain.DTOs.Request;
using Lisbeth.Bot.Domain.Entities;
using MikyM.Common.DataAccessLayer.Specifications;
using System;
using System.Linq;
using System.Threading.Tasks;
using Serilog;

namespace Lisbeth.Bot.Application.Discord.SlashCommands
{
    [SlashCommandGroup("mute", "Mute commands")]
    [SlashModuleLifespan(SlashModuleLifespan.Transient)]
    [UsedImplicitly]
    public class MuteSlashCommands : ApplicationCommandModule
    {
        public IMuteService _service { private get; set; }

        [SlashCommand("add", "A command that allows muting a user.")]
        public async Task MuteCommand(InteractionContext ctx, 
            [Option("user", "User to mute")] DiscordUser user,
            [Option("length", "For how long should the user be muted")] string length,
            [Option("reason", "Reason for mute")] string reason = "No reason provided")
        {
            await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);

            DateTime? liftsOn = length.ToDateTimeDuration().FinalDateFromToday;
            TimeSpan tmspDuration = length.ToDateTimeDuration().Duration;

            if (liftsOn is null)
                throw new ArgumentException($"Parameter {nameof(length)} can't be parsed to a known duration.");

            var mbr = user as DiscordMember;
            var req = new MuteReqDto(user.Id, ctx.Guild.Id, ctx.Member.Id, liftsOn, reason);

            var res = await _service.GetBySpecificationsAsync<Mute>(
                new Specifications<Mute>(x => x.UserId == req.UserId && x.GuildId == req.GuildId && !x.IsDisabled));

            var embed = new DiscordEmbedBuilder();
            embed.WithColor(0x18315C);
            embed.WithAuthor($"Mute | {mbr?.GetFullDisplayName()}", null, mbr?.AvatarUrl);
            var builder = new DiscordWebhookBuilder();

            string lengthString = "";
            if (length.Contains("perm"))
                lengthString = "Permanent";
            else
                lengthString = $"{tmspDuration.Days} days, {tmspDuration.Hours} hrs, {tmspDuration.Minutes} mins";



            long id = 0;

            if (res is null || !res.Any())
            {
                bool resMute = true;

                if (mbr.Roles.FirstOrDefault(r => r.Name == "Muted") is null)
                    resMute = await mbr.Mute(ctx.Guild);

                if (!resMute)
                {
                    var noEntryEmoji = DiscordEmoji.FromName(ctx.Client, ":no_entry:");
                    embed.WithColor(0x18315C);
                    embed.WithAuthor($"{noEntryEmoji} Mute denied");
                    embed.WithDescription("Can't mute other moderators.");
                    await ctx.EditResponseAsync(builder.AddEmbed(embed.Build()));
                    return;
                }

                id = await _service.AddAsync(req, true);

                embed.AddField("User mention", mbr.Mention, true);
                embed.AddField("Moderator", ctx.Member.Mention, true);
                embed.AddField("Length", lengthString, true);
                embed.AddField("Muted until", liftsOn.ToString(), true);
                embed.AddField("Reason", reason);
                embed.WithFooter($"Case ID: {id} | Member ID: {mbr.Id}");
                await ctx.EditResponseAsync(builder.AddEmbed(embed.Build()));
                return;
            }

            var prevMod = await ctx.Client.GetUserAsync(res[0].MutedById);
            if (res?[0].MutedUntil > liftsOn)
            {
                embed.WithDescription($"This user has already been muted until {res?[0].MutedUntil} by {prevMod.Mention}");
                //Log.Logger.Information($"User {ctx.Member.Username}#{ctx.Member.Discriminator} with ID:{ctx.Member.Id} tried to mute {member.Username}#{member.Discriminator} with ID:{userId} to {mutedUntill} but user was already muted, case ID: {muteCaseKVPair.Key}");
                embed.WithFooter($"Case ID: {res?[0].Id} | Member ID: {res?[0].UserId}");
                await ctx.EditResponseAsync(builder.AddEmbed(embed.Build()));
                return;
            }

            res[0].Extend(user.Id, liftsOn, reason);
            //await _service.UpdateAsync(res[0], true);

            embed.WithAuthor($"Extend Mute | {mbr.GetFullUsername()}", null, mbr.AvatarUrl);
            embed.AddField("Previous mute until", res[0].MutedUntil.ToString(), true);
            embed.AddField("Previous moderator", prevMod.Mention, true);
            embed.AddField("Previous reason", res[0].Reason, true);
            embed.AddField("User mention", mbr.Mention, true);
            embed.AddField("Moderator", ctx.Member.Mention, true);
            embed.AddField("Length", lengthString, true);
            embed.AddField("Muted until", liftsOn.ToString(), true);
            embed.AddField("Reason", reason);
            embed.WithFooter($"Case ID: {res[0].Id} | Member ID: {mbr.Id}");
            await ctx.EditResponseAsync(builder.AddEmbed(embed.Build()));
        }

        [SlashCommand("remove", "A command that allows unmuting a user.")]
        public async Task UnmuteCommand(InteractionContext ctx,
            [Option("user", "User to mute")] DiscordUser user)
        {
            await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);
            
            var req = new MuteDisableReqDto(user.Id, ctx.Guild.Id, ctx.Member.Id);

            var res = await _service.DisableAsync(req);

            var mbr = user as DiscordMember;

            var embed = new DiscordEmbedBuilder();
            embed.WithColor(0x18315C);
            var builder = new DiscordWebhookBuilder();

            if (res is null)
            {
                if (mbr?.Roles.FirstOrDefault(r => r.Name == "Muted") is null)
                {
                    embed.WithAuthor($"Unmute failed");
                    embed.WithDescription($"This user isn't currently muted.");
                    await ctx.EditResponseAsync(builder.AddEmbed(embed.Build()));
                    return;
                }

                await mbr.Unmute(ctx.Guild);
                embed.WithAuthor($"Unmute | {mbr.GetFullUsername()}", null, mbr.AvatarUrl);
                embed.AddField("Moderator", ctx.Member.Mention, true);
                embed.AddField("User mention", mbr.Mention, true);
                embed.WithDescription($"Successfully unmuted");
                embed.WithFooter($"Case ID: unknown | Member ID: {mbr.Id}");
                await ctx.EditResponseAsync(builder.AddEmbed(embed.Build()));
                return;
            }

            await mbr.Unmute(ctx.Guild);
            await _service.CommitAsync();

            embed.WithAuthor($"Unmute | {mbr.GetFullUsername()}", null, mbr.AvatarUrl);
            embed.AddField("Moderator", ctx.Member.Mention, true);
            embed.AddField("User mention", mbr.Mention, true);
            embed.WithDescription($"Successfully unmuted");
            embed.WithFooter($"Case ID: {res.Id} | Member ID: {mbr.Id}");
            await ctx.EditResponseAsync(builder.AddEmbed(embed.Build()));
        }

        [SlashCommand("get", "A command that allows checking mute info.")]
        public async Task GetMuteCommand(InteractionContext ctx,
            [Option("user", "Muted user to fetch mute info about")] DiscordUser user)
        {
            await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);

            var res = await _service.GetBySpecificationsAsync<Mute>(
                new Specifications<Mute>(x => x.UserId == user.Id));

            var mbr = user as DiscordMember;

            var builder = new DiscordWebhookBuilder();
            var embed = new DiscordEmbedBuilder();
            embed.WithColor(0x18315C);

            if (res is null || !res.Any())
            {
                builder.WithContent($"Mute not found.");
            }
            else
            {
                var prevMod = await ctx.Client.GetUserAsync(res[0].MutedById);

                embed.WithAuthor($"Mute | {mbr?.GetFullDisplayName()}", null, mbr.AvatarUrl);
                embed.AddField("User mention", mbr.Mention, true);
                embed.AddField("Moderator", prevMod.Mention, true);
                embed.AddField("Muted until", res[0].MutedUntil.ToString(), true);
                embed.AddField("Reason", res[0].Reason);
                if (res[0].LiftedById != 0)
                {
                    var liftedBy = await ctx.Client.GetUserAsync(res[0].LiftedById);
                    embed.AddField("Was lifted by", liftedBy.Mention);
                }
                embed.WithFooter($"Case ID: {res[0].Id} | Member ID: {mbr.Id}");
            }
            
            await ctx.EditResponseAsync(builder.AddEmbed(embed.Build()));
        }
    }
}
