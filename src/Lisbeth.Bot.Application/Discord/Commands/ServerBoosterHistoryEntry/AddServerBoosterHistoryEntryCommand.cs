using DSharpPlus.Entities;
using MikyM.CommandHandlers;

namespace Lisbeth.Bot.Application.Discord.Commands.ServerBoosterHistoryEntry;

public class AddServerBoosterHistoryEntryCommand : CommandBase
{
    public DiscordMember Member { get; }
    public DiscordGuild Guild { get; }
    
    public AddServerBoosterHistoryEntryCommand(DiscordGuild guild, DiscordMember member)
    {
        Guild = guild;
        Member = member;
    }
}
