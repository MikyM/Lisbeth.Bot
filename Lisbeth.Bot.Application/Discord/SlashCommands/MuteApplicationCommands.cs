using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using DSharpPlus.SlashCommands.Attributes;
using JetBrains.Annotations;
using Lisbeth.Bot.Application.Discord.Services.Interfaces;
using Lisbeth.Bot.Application.Discord.SlashCommands;
using Lisbeth.Bot.Application.Extensions;
using Lisbeth.Bot.Domain.DTOs.Request;
using System;
using System.Threading.Tasks;

// ReSharper disable once CheckNamespace
namespace Lisbeth.Bot.Application.Discord.ApplicationCommands
{
    [SlashModuleLifespan(SlashModuleLifespan.Transient)]
    [UsedImplicitly]
    public partial class MuteApplicationCommands : ApplicationCommandModule
    {
        [UsedImplicitly]
        // ReSharper disable once InconsistentNaming
        public IDiscordMuteService _discordMuteService { private get; set; }

        [SlashRequireUserPermissions(Permissions.BanMembers)]
        [SlashCommand("mute", "A command that allows mute actions.")]
        [UsedImplicitly]
        public async Task MuteCommand(InteractionContext ctx, [Option("action", "Action type")] MuteActionType actionType,
            [Option("user", "User to mute")] DiscordUser user,
            [Option("length", "For how long should the user be muted")] string length = "",
            [Option("reason", "Reason for mute")] string reason = "No reason provided")
        {
            await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);

            DiscordEmbed embed;

            switch (actionType)
            {
                case MuteActionType.Add:
                    DateTime? liftsOn = length.ToDateTimeDuration().FinalDateFromToday;
                    if (liftsOn is null)
                        throw new ArgumentException($"Parameter {nameof(length)} can't be parsed to a known duration.");
                    if (length is "") throw new ArgumentException($"Parameter {nameof(length)} can't be empty.");
                    var newReq = new MuteReqDto(user.Id, ctx.Guild.Id, ctx.Member.Id, liftsOn, reason);
                    embed = await _discordMuteService.MuteAsync(newReq, 0, ctx);
                    break;
                case MuteActionType.Remove:
                    var disReq = new MuteDisableReqDto(user.Id, ctx.Guild.Id, ctx.Member.Id);
                    embed = await _discordMuteService.UnmuteAsync(disReq, 0, ctx);
                    break;
                case MuteActionType.Get:
                    var getReq = new MuteGetReqDto(null, user.Id, ctx.Guild.Id);
                    embed = await _discordMuteService.GetAsync(getReq, 0, ctx);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(actionType), actionType, null);
            }

            await ctx.Interaction.CreateFollowupMessageAsync(new DiscordFollowupMessageBuilder().AddEmbed(embed)
                .AsEphemeral(true));
        }
    }
}
