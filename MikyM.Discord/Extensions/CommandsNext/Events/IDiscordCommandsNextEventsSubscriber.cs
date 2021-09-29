using System.Threading.Tasks;
using DSharpPlus.CommandsNext;

namespace MikyM.Discord.Extensions.CommandsNext.Events
{
    public interface IDiscordCommandsNextEventsSubscriber
    {
        public Task CommandsOnCommandExecuted(CommandsNextExtension sender, CommandExecutionEventArgs args);

        public Task CommandsOnCommandErrored(CommandsNextExtension sender, CommandErrorEventArgs args);
    }
}
