using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using DSharpPlus.SlashCommands.Attributes;
using JetBrains.Annotations;
using Lisbeth.Bot.Domain.DTOs.Request;
using System;
using System.Linq;
using System.Threading.Tasks;

// ReSharper disable once CheckNamespace
namespace Lisbeth.Bot.Application.Discord.ApplicationCommands
{
    // menus for mutes
    public partial class MuteApplicationCommands
    {
        #region user menus

        [UsedImplicitly]
        [SlashRequireUserPermissions(Permissions.BanMembers)]
        [ContextMenu(ApplicationCommandType.UserContextMenu, "Mute user")]
        public async Task MuteUserMenu(ContextMenuContext ctx)
        {
            await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);

            var req = new MuteReqDto(ctx.TargetUser.Id, ctx.Guild.Id, ctx.Member.Id, DateTime.MaxValue, "No reason provided - used through user context menu");

            var embed = await _discordMuteService.MuteAsync(req, 0, ctx);

            await ctx.Interaction.CreateFollowupMessageAsync(new DiscordFollowupMessageBuilder().AddEmbed(embed)
                .AsEphemeral(true));
        }

        [UsedImplicitly]
        [SlashRequireUserPermissions(Permissions.BanMembers)]
        [ContextMenu(ApplicationCommandType.UserContextMenu, "Unmute user")]
        public async Task UnmuteUserMenu(ContextMenuContext ctx)
        {
            await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);

            var req = new MuteDisableReqDto(ctx.TargetUser.Id, ctx.Guild.Id, ctx.Member.Id);

            var embed = await _discordMuteService.UnmuteAsync(req, 0, ctx);

            await ctx.Interaction.CreateFollowupMessageAsync(new DiscordFollowupMessageBuilder().AddEmbed(embed)
                .AsEphemeral(true));
        }

        [UsedImplicitly]
        [SlashRequireUserPermissions(Permissions.BanMembers)]
        [ContextMenu(ApplicationCommandType.UserContextMenu, "Get mute info")]
        public async Task GetMuteUserMenu(ContextMenuContext ctx)
        {
            await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);

            var req = new MuteGetReqDto(null, ctx.TargetUser.Id, ctx.Guild.Id);

            var embed = await _discordMuteService.GetAsync(req, 0, ctx);

            await ctx.Interaction.CreateFollowupMessageAsync(new DiscordFollowupMessageBuilder().AddEmbed(embed)
                .AsEphemeral(true));
        }

        #endregion

        #region message menus

        [UsedImplicitly]
        [SlashRequireUserPermissions(Permissions.BanMembers)]
        [ContextMenu(ApplicationCommandType.MessageContextMenu, "Mute author")]
        public async Task MuteAuthorMessageMenu(ContextMenuContext ctx)
        {
            await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);

            var req = new MuteReqDto(ctx.TargetMessage.Author.Id, ctx.Guild.Id, ctx.Member.Id, DateTime.MaxValue, "No reason provided - used through message context menu");

            var embed = await _discordMuteService.MuteAsync(req, 0, ctx);

            await ctx.Interaction.CreateFollowupMessageAsync(new DiscordFollowupMessageBuilder().AddEmbed(embed)
                .AsEphemeral(true));
        }

        [UsedImplicitly]
        [SlashRequireUserPermissions(Permissions.BanMembers)]
        [ContextMenu(ApplicationCommandType.MessageContextMenu, "Mute author and prune")]
        public async Task MuteAuthorWithWipeMessageMenu(ContextMenuContext ctx)
        {
            await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);

            var req = new MuteReqDto(ctx.TargetMessage.Author.Id, ctx.Guild.Id, ctx.Member.Id, DateTime.MaxValue, "No reason provided - used through message context menu");

            var embed = await _discordMuteService.MuteAsync(req, 0, ctx);

            var msgs = await ctx.Channel.GetMessagesAsync();

            var msgsToDel = msgs.Where(x => x.Author.Id == ctx.TargetMessage.Author.Id).OrderByDescending(x => x.Timestamp).Take(10);

            await ctx.Channel.DeleteMessagesAsync(msgsToDel);

            await ctx.Interaction.CreateFollowupMessageAsync(new DiscordFollowupMessageBuilder().AddEmbed(embed)
                .AsEphemeral(true));
        }

        #endregion
    }
}
