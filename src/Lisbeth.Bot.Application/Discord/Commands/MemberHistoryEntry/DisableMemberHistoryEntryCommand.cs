using DSharpPlus.Entities;
using MikyM.CommandHandlers;

namespace Lisbeth.Bot.Application.Discord.Commands.MemberHistoryEntry;

public class DisableMemberHistoryEntryCommand : CommandBase
{
    public DiscordMember Member { get; }
    public DiscordGuild Guild { get; }
    
    public DisableMemberHistoryEntryCommand(DiscordGuild guild, DiscordMember member)
    {
        Guild = guild;
        Member = member;
    }
}
