using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using DSharpPlus.SlashCommands.Attributes;
using JetBrains.Annotations;
using Lisbeth.Bot.Domain.DTOs.Request;

// ReSharper disable once CheckNamespace
namespace Lisbeth.Bot.Application.Discord.ApplicationCommands
{
    // Menus for prunes
    public partial class ModUtilApplicationCommands
    {
        #region user

        [SlashRequireUserPermissions(Permissions.ManageMessages)]
        [ContextMenu(ApplicationCommandType.UserContextMenu, "Prune last 10 messages")]
        public async Task PruneLastTenUserMenu(ContextMenuContext ctx)
        {
            await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);

            var req = new PruneReqDto(10, null, ctx.TargetUser.Id, ctx.Channel.Id, ctx.Guild.Id, ctx.Member.Id);

            var embed = await _discordMessageService.PruneAsync(req, 0, ctx);

            await ctx.Interaction.CreateFollowupMessageAsync(new DiscordFollowupMessageBuilder().AddEmbed(embed)
                .AsEphemeral(true));
        }

        #endregion

        #region message

        [SlashRequireUserPermissions(Permissions.ManageMessages)]
        [ContextMenu(ApplicationCommandType.MessageContextMenu, "Prune last 10 in channel")]
        public async Task PruneLastTenFromThisMessageMenu(ContextMenuContext ctx)
        {
            await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);

            var req = new PruneReqDto(10, null, null, ctx.Channel.Id, ctx.Guild.Id, ctx.Member.Id);

            var embed = await _discordMessageService.PruneAsync(req, 0, ctx);

            await ctx.Interaction.CreateFollowupMessageAsync(new DiscordFollowupMessageBuilder().AddEmbed(embed)
                .AsEphemeral(true));
        }

        [SlashRequireUserPermissions(Permissions.ManageMessages)]
        [ContextMenu(ApplicationCommandType.MessageContextMenu, "Prune until this")]
        public async Task PruneUntilThisMessageMenu(ContextMenuContext ctx)
        {
            await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);

            var req = new PruneReqDto(0, ctx.TargetMessage.Id, null, ctx.Channel.Id, ctx.Guild.Id, ctx.Member.Id);

            var embed = await _discordMessageService.PruneAsync(req, 0, ctx);

            await ctx.Interaction.CreateFollowupMessageAsync(new DiscordFollowupMessageBuilder().AddEmbed(embed)
                .AsEphemeral(true));
        }

        [SlashRequireUserPermissions(Permissions.ManageMessages)]
        [ContextMenu(ApplicationCommandType.MessageContextMenu, "Author prune until this")]
        public async Task PruneUntilThisByThisAuthorMessageMenu(ContextMenuContext ctx)
        {
            await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);

            var req = new PruneReqDto(0, ctx.TargetMessage.Id, ctx.TargetMessage.Author.Id, ctx.Channel.Id, ctx.Guild.Id, ctx.Member.Id);

            var embed = await _discordMessageService.PruneAsync(req, 0, ctx);

            await ctx.Interaction.CreateFollowupMessageAsync(new DiscordFollowupMessageBuilder().AddEmbed(embed)
                .AsEphemeral(true));
        }

        #endregion
    }
}
