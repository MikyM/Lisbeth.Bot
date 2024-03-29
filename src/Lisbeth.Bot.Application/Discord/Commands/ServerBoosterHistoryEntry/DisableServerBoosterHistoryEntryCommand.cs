﻿namespace Lisbeth.Bot.Application.Discord.Commands.ServerBoosterHistoryEntry;

public class DisableServerBoosterHistoryEntryCommand : ICommand
{
    public DiscordMember Member { get; }
    public DiscordGuild DiscordGuild { get; }
    public Guild? Guild { get; }
    
    public DisableServerBoosterHistoryEntryCommand(DiscordGuild discordGuild, DiscordMember member, Guild? guild = null)
    {
        DiscordGuild = discordGuild;
        Member = member;
        Guild = guild;
    }
}
