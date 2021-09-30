using DSharpPlus.SlashCommands;

namespace Lisbeth.Bot.Application.Discord.SlashCommands
{
    public enum BanChoice
    {
        [ChoiceName("add")]
        Add,
        [ChoiceName("remove")]
        Remove,
        [ChoiceName("get")]
        Get,
        [ChoiceName("id")]
        Id
    }

    public enum MuteChoice
    {
        [ChoiceName("add")]
        Add,
        [ChoiceName("remove")]
        Remove,
        [ChoiceName("get")]
        Get
    }

    public enum TicketChoice
    {
        [ChoiceName("public")]
        Add,
        [ChoiceName("private")]
        Remove
    }
}
