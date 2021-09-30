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
using DSharpPlus.SlashCommands.Attributes;
using Lisbeth.Bot.Application.Discord.Services.Interfaces;

namespace Lisbeth.Bot.Application.Discord.SlashCommands
{
    [SlashCommandGroup("ban", "BanAsync commands")]
    [SlashModuleLifespan(SlashModuleLifespan.Transient)]
    [UsedImplicitly]
    public class BanSlashCommands : ApplicationCommandModule
    {
        public IBanService _banService { private get; set; }
        public IDiscordBanService _discordBanService { private get; set; }

        [SlashRequireUserPermissions(Permissions.BanMembers)]
        [SlashCommand("add", "A command that allows banning a user.")]
        public async Task BanCommand(InteractionContext ctx,
            [Option("user", "User to ban")] DiscordUser user,
            [Option("length", "For how long should the user be banned")] string length = "perm",
            [Option("reason", "Reason for ban")] string reason = "No reason provided")
        {
            await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);

            DateTime? liftsOn = length.ToDateTimeDuration().FinalDateFromToday;

            if (liftsOn is null)
                throw new ArgumentException($"Parameter {nameof(length)} can't be parsed to a known duration.");

            var req = new BanReqDto(user.Id, ctx.Guild.Id, ctx.Member.Id, liftsOn, reason);

            var embed = await _discordBanService.BanAsync(req, 0, ctx);

            await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(embed));
        }

        [SlashRequireUserPermissions(Permissions.BanMembers)]
        [SlashCommand("remove", "A command that allows unbanning a user.")]
        public async Task UnbanCommand(InteractionContext ctx,
            [Option("user", "User Id to unban")] ulong userId)
        {
            await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);
            
            var req = new BanDisableReqDto(userId, ctx.Guild.Id, ctx.Member.Id);

            var embed = await _discordBanService.UnbanAsync(req, 0, ctx);

            await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(embed));
        }

        [SlashRequireUserPermissions(Permissions.BanMembers)]
        [SlashCommand("get", "A command that allows checking ban info.")]
        public async Task GetBanCommand(InteractionContext ctx,
            [Option("user", "User Id to fetch info about")] long userId)
        {
            await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);

            ulong parsedId = (ulong) userId;

            var res = await _banService.GetBySpecificationsAsync<Ban>(
                new Specifications<Ban>(x => x.UserId == parsedId && x.GuildId == ctx.Guild.Id));

            var entity = res.FirstOrDefault();

            var builder = new DiscordWebhookBuilder();
            var embed = new DiscordEmbedBuilder();
            embed.WithColor(0x18315C);

            DiscordUser usr = null;
            try
            {
                usr = await ctx.Client.GetUserAsync(parsedId);
            }
            catch (Exception ex)
            {
                throw new ArgumentException($"User with Id: {parsedId} doesn't exist.");
            }

            if (entity is null)
            {
                builder.WithContent($"BanAsync not found.");
            }
            
            else
            {
                var prevMod = await ctx.Client.GetUserAsync(entity.BannedById);

                embed.WithAuthor($"BanAsync | {usr?.GetFullUsername()}", null, usr.AvatarUrl);
                embed.AddField("User mention", usr.Mention, true);
                embed.AddField("Moderator", prevMod.Mention, true);
                embed.AddField("Banned until", entity.BannedUntil.ToString(), true);
                embed.AddField("Reason", entity.Reason);
                if (entity.LiftedById != 0)
                {
                    var liftedBy = await ctx.Client.GetUserAsync(entity.LiftedById);
                    embed.AddField("Was lifted by", liftedBy.Mention);
                }
                embed.WithFooter($"Case ID: {entity.Id} | User ID: {usr.Id}");
            }
            
            await ctx.EditResponseAsync(builder.AddEmbed(embed.Build()));
        }
    }
}
