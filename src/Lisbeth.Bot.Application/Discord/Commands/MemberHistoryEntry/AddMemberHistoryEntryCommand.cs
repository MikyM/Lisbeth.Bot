using DSharpPlus.Entities;

namespace Lisbeth.Bot.Application.Discord.Commands.MemberHistoryEntry;

public class AddMemberHistoryEntryCommand : CommandBase
{
    public DiscordMember Member { get; }
    public DiscordGuild Guild { get; }
    
    public AddMemberHistoryEntryCommand(DiscordGuild guild, DiscordMember member)
    {
        Guild = guild;
        Member = member;
    }
}
