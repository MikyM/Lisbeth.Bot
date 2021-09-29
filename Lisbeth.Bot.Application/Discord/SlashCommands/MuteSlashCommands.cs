using System;
using System.Linq;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using Lisbeth.Bot.DataAccessLayer;
using Lisbeth.Bot.Domain.Entities;
using MikyM.Common.Application.Interfaces;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Lisbeth.Bot.Application.Extensions;
using Lisbeth.Bot.Application.Interfaces;
using Lisbeth.Bot.Domain.DTOs.Request;
using MikyM.Common.DataAccessLayer.Specifications;

namespace Lisbeth.Bot.Application.Discord.SlashCommands
{
    [SlashCommandGroup("mute", "Mute commands")]
    [SlashModuleLifespan(SlashModuleLifespan.Transient)]
    [UsedImplicitly]
    public class MuteSlashCommands : ApplicationCommandModule
    {
        public IMuteService _service { private get; set; }
        [SlashCommand("add", "A command that allows muting users.")]
        public async Task MuteCommand(InteractionContext ctx, 
            [Option("user", "User to mute")] DiscordUser user,
            [Option("length", "For how long should the user be muted")] string length,
            [Option("reason", "Reason for mute")] string reason = "No reason provided")
        {
            await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);
            DateTime? liftsOn = length.ToDateTimeDuration();
            if (liftsOn is null)
                return;

            var mute = new MuteReqDto(user.Id, ctx.Member.Id, liftsOn, reason);

            var res = await _service.GetBySpecificationsAsync<Mute>(
                new Specifications<Mute>(x => x.UserId == user.Id && !x.IsDisabled));

            long id = 0;
            if (res is null || !res.Any())
            {
                id = await _service.AddAsync(mute, true);
            }
            else
            {
                res[0].Extend(user.Id, liftsOn, reason);
                await _service.UpdateAsync(res[0], true);
            }

            var builder = new DiscordWebhookBuilder();
            builder.WithContent("Done");
            await ctx.EditResponseAsync(builder);
        }

        [SlashCommand("get", "A command that allows muting users.")]
        public async Task GetMuteCommand(InteractionContext ctx,
            [Option("user", "User to mute")] DiscordUser user)
        {
            await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);
            var res = await _service.GetBySpecificationsAsync<Mute>(
                new Specifications<Mute>(x => x.UserId == user.Id && !x.IsDisabled));

            long id = 0;
            var builder = new DiscordWebhookBuilder();

            if (res is null || !res.Any())
            {

            }
            else
            {
                builder.WithContent($"Found: {res[0].UserId}");
            }
            
            await ctx.EditResponseAsync(builder);
        }

        [SlashCommand("count", "A command that allows muting users.")]
        public async Task GetMuteCountCommand(InteractionContext ctx)
        {
            await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);
            var res = await _service.LongCountAsync();

            var builder = new DiscordWebhookBuilder();

            builder.WithContent($"Found: {res}");
            await ctx.EditResponseAsync(builder);
        }
    }
}
