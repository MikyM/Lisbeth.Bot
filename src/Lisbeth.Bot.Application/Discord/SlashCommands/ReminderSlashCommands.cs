using System.Threading.Tasks;
using DSharpPlus.SlashCommands;
using JetBrains.Annotations;

namespace Lisbeth.Bot.Application.Discord.SlashCommands
{
    [UsedImplicitly]
    [SlashModuleLifespan(SlashModuleLifespan.Transient)]
    [SlashCommandGroup("reminder", "Reminder commands")]
    public class ReminderSlashCommands : ApplicationCommandModule
    {
        [UsedImplicitly]
        [SlashCommand("single", "Single reminder command")]
        public async Task SingleReminderCommand(InteractionContext ctx,
            [Option("action", "Action to perform")]
            ReminderActionType actionType,
            [Option("time", "This can be either time or a datetime")]
            string time,
            [Option("text", "Text for a single reminder")]
            string text)
        {
        }

        [UsedImplicitly]
        [SlashCommand("recurring", "Recurring reminder command")]
        public async Task RecurringReminderCommand(InteractionContext ctx,
            [Option("action", "Action to perform")]
            ReminderActionType actionType,
            [Option("cron", "Cron expression")] string cron,
            [Option("description", "Text for a single reminder")]
            string description,
            [Option("author", "Author of the embed")]
            string author,
            [Option("footer", "Footer of the embed")]
            string footer,
            [Option("authorUrl", "Url for author image for the embed")]
            string authorUrl,
            [Option("footerUrl", "Url for footer image for the embed")]
            string footerUrl,
            [Option("fields", "Fields to parse for the embed")]
            string fields,
            [Option("imageUrl", "Base image url for the embed")]
            string imageUrl,
            [Option("id", "Id of a created reminder")]
            string id = "")
        {
        }
    }
}