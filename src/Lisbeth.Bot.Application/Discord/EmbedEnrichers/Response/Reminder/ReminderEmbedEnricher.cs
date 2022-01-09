// This file is part of Lisbeth.Bot project
//
// Copyright (C) 2021 Krzysztof Kupisz - MikyM
// 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU Affero General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Affero General Public License for more details.
// 
// You should have received a copy of the GNU Affero General Public License
// along with this program.  If not, see <https://www.gnu.org/licenses/>.

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
