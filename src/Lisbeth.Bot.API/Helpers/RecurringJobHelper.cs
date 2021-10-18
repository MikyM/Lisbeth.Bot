using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Hangfire;
using Lisbeth.Bot.Application.Discord.Services.Interfaces;
using Serilog;

namespace Lisbeth.Bot.API.Helpers
{
    public static class RecurringJobHelper
    {
        public static List<string> JobIds { get; private set; } = new();

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

        public static async Task ScheduleAllDefinedAfterDelayAsync()
        {
            await Task.Delay(5000);
            ScheduleAutomaticUnban();
            ScheduleAutomaticUnmute();
            Log.Logger.Information("Recurring jobs scheduled.");
        }
    }
}