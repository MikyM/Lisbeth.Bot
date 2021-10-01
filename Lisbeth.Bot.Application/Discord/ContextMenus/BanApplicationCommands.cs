using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.SlashCommands;
using JetBrains.Annotations;

namespace Lisbeth.Bot.Application.Discord.ContextMenus
{
    // Menus for bans
    [UsedImplicitly]
    public partial class BanApplicationCommands
    {
        //For user commands
        [ContextMenu(ApplicationCommandType.UserContextMenu, "User Menu")]
        public async Task UserMenu(ContextMenuContext ctx) { }

        //For message commands
        [ContextMenu(ApplicationCommandType.MessageContextMenu, "Message Menu")]
        public async Task MessageMenu(ContextMenuContext ctx) { }
    }
}
