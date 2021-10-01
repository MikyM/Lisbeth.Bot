using System.Threading.Tasks;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using DSharpPlus.SlashCommands.EventArgs;
using JetBrains.Annotations;
using MikyM.Discord.Extensions.SlashCommands.Events;
using Serilog;

namespace Lisbeth.Bot.Application.Discord.EventHandlers
{
    [UsedImplicitly]
    public class SlashCommandEvents : IDiscordSlashCommandsEventsSubscriber
    {
        public Task SlashCommandsOnContextMenuErrored(SlashCommandsExtension sender, ContextMenuErrorEventArgs args)
        {
            return Task.CompletedTask;
        }

        public Task SlashCommandsOnContextMenuExecuted(SlashCommandsExtension sender, ContextMenuExecutedEventArgs args)
        {
            return Task.CompletedTask;
        }

        public Task SlashCommandsOnSlashCommandErrored(SlashCommandsExtension sender, SlashCommandErrorEventArgs args)
        {
            Log.Logger.Error(args.Exception.ToString());
            var noEntryEmoji = DiscordEmoji.FromName(sender.Client, ":x:");
            var embed = new DiscordEmbedBuilder();
            embed.WithColor(new DiscordColor(170, 1, 20));
            embed.WithAuthor($"{noEntryEmoji} Command errored");
            embed.AddField("Type", args.Exception.GetType().ToString());
            embed.AddField("Message", args.Exception.Message);
            args.Context.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(embed.Build()));
            return Task.CompletedTask;
        }

        public Task SlashCommandsOnSlashCommandExecuted(SlashCommandsExtension sender, SlashCommandExecutedEventArgs args)
        {
            return Task.CompletedTask;
        }
    }
}
