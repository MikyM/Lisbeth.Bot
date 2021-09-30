using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using DSharpPlus.SlashCommands.Attributes;
using JetBrains.Annotations;
using Lisbeth.Bot.Application.Discord.Extensions;
using Lisbeth.Bot.Application.Discord.Services.Interfaces;
using Lisbeth.Bot.Application.Extensions;
using Lisbeth.Bot.Domain.DTOs.Request;
using Lisbeth.Bot.Domain.Entities;
using MikyM.Common.DataAccessLayer.Specifications;
using System;
using System.Linq;
using System.Threading.Tasks;
using Lisbeth.Bot.Application.Interfaces;

namespace Lisbeth.Bot.Application.Discord.SlashCommands
{
    [SlashCommandGroup("mute", "Mute commands")]
    [SlashModuleLifespan(SlashModuleLifespan.Transient)]
    [UsedImplicitly]
    public class MuteSlashCommands : ApplicationCommandModule
    {
        [UsedImplicitly]
        public IDiscordMuteService _discordMuteService { private get; set; }
        [UsedImplicitly]
        public IMuteService _muteService { private get; set; }

        [SlashRequireUserPermissions(Permissions.BanMembers)]
        [SlashCommand("add", "A command that allows muting a user.")]
        [UsedImplicitly]
        public async Task MuteCommand(InteractionContext ctx, 
            [Option("user", "User to mute")] DiscordUser user,
            [Option("length", "For how long should the user be muted")] string length,
            [Option("reason", "Reason for mute")] string reason = "No reason provided")
        {
            await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);

            DateTime? liftsOn = length.ToDateTimeDuration().FinalDateFromToday;

            if (liftsOn is null)
                throw new ArgumentException($"Parameter {nameof(length)} can't be parsed to a known duration.");

            var req = new MuteReqDto(user.Id, ctx.Guild.Id, ctx.Member.Id, liftsOn, reason);

            var embed = await _discordMuteService.MuteAsync(req, 0, ctx);

            await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(embed));
        }

        [SlashRequireUserPermissions(Permissions.BanMembers)]
        [SlashCommand("remove", "A command that allows unmuting a user.")]
        [UsedImplicitly]
        public async Task UnmuteCommand(InteractionContext ctx,
            [Option("user", "User to mute")] DiscordUser user)
        {
            await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);
            
            var req = new MuteDisableReqDto(user.Id, ctx.Guild.Id, ctx.Member.Id);

            var embed = await _discordMuteService.UnmuteAsync(req, 0, ctx);

            await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(embed));
        }

        [SlashRequireUserPermissions(Permissions.BanMembers)]
        [SlashCommand("get", "A command that allows checking mute info.")]
        [UsedImplicitly]
        public async Task GetMuteCommand(InteractionContext ctx,
            [Option("user", "Muted user to fetch mute info about")] DiscordUser user)
        {
            await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);

            var res = await _muteService.GetBySpecificationsAsync<Mute>(
                new Specifications<Mute>(x => x.UserId == user.Id && x.GuildId == ctx.Guild.Id));

            var mbr = user as DiscordMember;

            var builder = new DiscordWebhookBuilder();
            var embed = new DiscordEmbedBuilder();
            embed.WithColor(0x18315C);

            if (res is null || !res.Any())
            {
                builder.WithContent($"MuteAsync not found.");
            }
            else
            {
                var prevMod = await ctx.Client.GetUserAsync(res[0].MutedById);

                embed.WithAuthor($"MuteAsync | {mbr?.GetFullDisplayName()}", null, mbr.AvatarUrl);
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
