using DSharpPlus.Entities;
using MikyM.CommandHandlers.Commands;

namespace Lisbeth.Bot.Application.Discord.Commands.ServerBooster;

public class AddServerBoosterCommand : CommandBase
{
    public DiscordMember Member { get; }
    public DiscordGuild Guild { get; }
    
    public AddServerBoosterCommand(DiscordGuild guild, DiscordMember member)
    {
        Guild = guild;
        Member = member;
    }
}
