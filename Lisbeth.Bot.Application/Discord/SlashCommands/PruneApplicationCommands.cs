using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using DSharpPlus.SlashCommands.Attributes;
using JetBrains.Annotations;
using Lisbeth.Bot.Application.Discord.Services.Interfaces;
using Lisbeth.Bot.Domain.DTOs.Request;

// ReSharper disable once CheckNamespace
namespace Lisbeth.Bot.Application.Discord.ApplicationCommands
{
    [SlashModuleLifespan(SlashModuleLifespan.Transient)]
    [UsedImplicitly]
    public partial class PruneApplicationCommands : ApplicationCommandModule
    {
        [UsedImplicitly]
        // ReSharper disable once InconsistentNaming
        public IDiscordMessageService _discordMessageService { private get; set; }

        [UsedImplicitly]
        [SlashRequireUserPermissions(Permissions.ManageMessages)]
        [SlashCommand("prune", "A command that allows banning a user.")]
        public async Task PruneCommand(InteractionContext ctx,
            [Option("count", "Number of messages to prune")] long count,
            [Option("user", "User to target prune at")] DiscordUser user = null,
            [Option("id", "Message id to prune to")] long id = 0,
            [Option("reason", "Reason for ban")] string reason = "No reason provided")
        {
            await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);

            if (count > 99) count = 99;

            DiscordEmbed embed;
            switch (user)
            {
                case null:
                    switch (id)
                    {
                        case 0:
                            var reqNoUsNoMsgId = new PruneReqDto((int)count + 1);
                            embed = await _discordMessageService.PruneAsync(reqNoUsNoMsgId, 0, ctx);
                            break;
                        default:
                            var reqNoUsWithMsgId = new PruneReqDto((int)count, (ulong)id);
                            embed = await _discordMessageService.PruneAsync(reqNoUsWithMsgId, 0, ctx);
                            break;
                    }
                    break;
                default:
                    switch (id)
                    {
                        case 0:
                            var reqWithUsNoMsgId = new PruneReqDto((int)count, null, user.Id);
                            embed = await _discordMessageService.PruneAsync(reqWithUsNoMsgId, 0, ctx);
                            break;
                        default:
                            var reqWithUsWithMsgId = new PruneReqDto((int)count, (ulong)id, user.Id);
                            embed = await _discordMessageService.PruneAsync(reqWithUsWithMsgId, 0, ctx);
                            break;
                    }
                    break;
            }

            await ctx.Interaction.CreateFollowupMessageAsync(new DiscordFollowupMessageBuilder().AddEmbed(embed)
                .AsEphemeral(true));
        }

    }
}
