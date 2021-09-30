using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using DSharpPlus.SlashCommands.Attributes;
using JetBrains.Annotations;

namespace Lisbeth.Bot.Application.Discord.SlashCommands
{
    [SlashCommandGroup("ticket", "Ticket commands")]
    [SlashModuleLifespan(SlashModuleLifespan.Transient)]
    [UsedImplicitly]
    public class TicketSlashCommands : ApplicationCommandModule
    {
        //public ITicketService _service { private get; set; }

        [SlashRequireUserPermissions(Permissions.BanMembers)]
        [SlashCommand("add", "A command that allows a certain user or a role to see the ticket")]
        public async Task AddCommand(InteractionContext ctx, [Option("target", "A user or a role to add")] SnowflakeObject target)
        {
        }

        [SlashRequireUserPermissions(Permissions.BanMembers)]
        [SlashCommand("remove", "A command that removes a certain user or a role from seeing the ticket")]
        public async Task RemoveCommand(InteractionContext ctx, [Option("target", "A user or a role to remove")] SnowflakeObject target)
        {
        }
    }
}