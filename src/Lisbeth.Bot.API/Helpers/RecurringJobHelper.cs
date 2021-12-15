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

using System.Collections.Generic;
using Hangfire;
using Lisbeth.Bot.Application.Discord.Handlers.Ticket.Interfaces;
using Lisbeth.Bot.Application.Discord.Requests.Ticket;
using Serilog;

namespace Lisbeth.Bot.API.Helpers;

public static class RecurringJobHelper
{
    public static List<string> JobIds { get; } = new();

    public static void ScheduleAutomaticUnmute()
    {
        RecurringJob.AddOrUpdate<IDiscordMuteService>("unmute", x => x.UnmuteCheckAsync(), Cron.Minutely,
            TimeZoneInfo.Utc, "moderation");
        JobIds.Add("unmute");
    }

    public static void ScheduleAutomaticUnban()
    {
        RecurringJob.AddOrUpdate<IDiscordBanService>("unban", x => x.UnbanCheckAsync(), Cron.Minutely,
            TimeZoneInfo.Utc, "moderation");
        JobIds.Add("unban");
    }

    public static void ScheduleAutomaticTicketClean()
    {
        RecurringJob.AddOrUpdate<IDiscordCleanClosedTicketsHandler>("unmute", x => x.HandleAsync(new CleanClosedTicketsRequest()), Cron.Hourly,
            TimeZoneInfo.Utc, "ticketing");
        JobIds.Add("ticketClean");
    }

    public static void ScheduleAutomaticTicketClose()
    {
        RecurringJob.AddOrUpdate<IDiscordCloseInactiveTicketsHandler>("unmute", x => x.HandleAsync(new CloseInactiveTicketsRequest()), Cron.Hourly,
            TimeZoneInfo.Utc, "ticketing");
        JobIds.Add("ticketClose");
    }

    public static async Task ScheduleAllDefinedAfterDelayAsync()
    {
        await Task.Delay(3000);
        ScheduleAutomaticUnban();
        ScheduleAutomaticUnmute();
        ScheduleAutomaticTicketClean();
        ScheduleAutomaticTicketClose();
        Log.Logger.Information("Recurring jobs scheduled.");
    }
}