using System.Threading.Tasks;
using DSharpPlus.SlashCommands;
using DSharpPlus.SlashCommands.EventArgs;
using MikyM.Discord.Extensions.SlashCommands.Events;
using Serilog;

namespace Lisbeth.Bot.Application.Discord.EventHandlers
{
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
            return Task.CompletedTask;
        }

        public Task SlashCommandsOnSlashCommandExecuted(SlashCommandsExtension sender, SlashCommandExecutedEventArgs args)
        {
            return Task.CompletedTask;
        }
    }
}
