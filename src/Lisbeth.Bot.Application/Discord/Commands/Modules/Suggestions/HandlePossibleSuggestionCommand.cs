using DSharpPlus.EventArgs;
using MikyM.CommandHandlers;

namespace Lisbeth.Bot.Application.Discord.Commands.Modules.Suggestions;

public class HandlePossibleSuggestionCommand : CommandBase
{
    public HandlePossibleSuggestionCommand(MessageCreateEventArgs eventData)
    {
        EventData = eventData;
    }

    public MessageCreateEventArgs EventData { get; }
}
