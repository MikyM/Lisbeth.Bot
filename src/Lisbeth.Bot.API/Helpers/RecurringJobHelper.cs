using System;
using System.Threading.Tasks;
using Hangfire;
using Lisbeth.Bot.Application.Discord.Services.Interfaces;
using Serilog;

namespace Lisbeth.Bot.API.Helpers
{
    public static class RecurringJobHelper
    {
        public static void ScheduleAutomaticUnmute()
        {
            RecurringJob.AddOrUpdate<IDiscordMuteService>("unmute", x => x.UnmuteCheckAsync(), Cron.Minutely,
                TimeZoneInfo.Utc, "moderation");
        }

        public static void ScheduleAutomaticUnban()
        {
            RecurringJob.AddOrUpdate<IDiscordBanService>("unban", x => x.UnbanCheckAsync(), Cron.Minutely,
                TimeZoneInfo.Utc, "moderation");
        }

        public static async Task ScheduleAllDefinedDelayed()
        {
            await Task.Delay(5000);
            ScheduleAutomaticUnban();
            ScheduleAutomaticUnmute();
            Log.Logger.Information("Recurring jobs scheduled.");
        }
    }
}