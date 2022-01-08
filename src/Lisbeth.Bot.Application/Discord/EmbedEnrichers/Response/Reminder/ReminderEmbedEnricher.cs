using Lisbeth.Bot.Application.Discord.SlashCommands;
using MikyM.Discord.EmbedBuilders.Enrichers;
using MikyM.Discord.EmbedBuilders.Wrappers;

namespace Lisbeth.Bot.Application.Discord.EmbedEnrichers.Response.Reminder;

public class ReminderEmbedEnricher : EmbedEnricherBase<ReminderResDto, ReminderActionType>
{
    public ReminderEmbedEnricher(ReminderResDto entity, ReminderActionType actionType) : base(entity, actionType)
    {
    }

    public override void Enrich(IDiscordEmbedBuilderWrapper embedBuilder)
    {
        string action = this.SecondaryEnricher switch
        {
            ReminderActionType.Set => "set",
            ReminderActionType.Reschedule => "rescheduled",
            ReminderActionType.ConfigureEmbed => "configured",
            ReminderActionType.Disable => "disabled",
            _ => throw new ArgumentOutOfRangeException()
        };

        embedBuilder.WithAuthor("Lisbeth reminder service")
            .WithDescription($"Reminder {action} successfully");

        if (this.PrimaryEnricher.IsRecurring)
            embedBuilder.AddField("Name", this.PrimaryEnricher.Name ?? "Unknown");

        if (this.SecondaryEnricher is not ReminderActionType.Disable)
        {
            embedBuilder.AddField("Next occurrence",
                    this.PrimaryEnricher.NextOccurrence.ToUniversalTime().ToString("dd/MM/yyyy hh:mm tt").ToUpper() +
                    " UTC", true)
                .AddField("Mentions",
                    string.Join(", ", this.PrimaryEnricher.Mentions ?? throw new InvalidOperationException()), true)
                .AddField("Text", this.PrimaryEnricher.Text);
        }
    }
}
