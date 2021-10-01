using DSharpPlus.SlashCommands;

namespace Lisbeth.Bot.Application.Discord.SlashCommands
{
    public enum BanActionType
    {
        [ChoiceName("add")]
        Add,
        [ChoiceName("remove")]
        Remove,
        [ChoiceName("get")]
        Get
    }

    public enum MuteActionType
    {
        [ChoiceName("add")]
        Add,
        [ChoiceName("remove")]
        Remove,
        [ChoiceName("get")]
        Get
    }

    public enum PruneActionType
    {
        [ChoiceName("add")]
        Add,
        [ChoiceName("remove")]
        Remove,
        [ChoiceName("get")]
        Get
    }

    public enum TicketActionType
    {
        [ChoiceName("open")]
        Open,
        [ChoiceName("close")]
        Close,
        [ChoiceName("add")]
        Add,
        [ChoiceName("remove")]
        Remove
    }
}
