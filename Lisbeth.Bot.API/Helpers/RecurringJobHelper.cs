using Hangfire;
using Lisbeth.Bot.Application.Discord.Services.Interfaces;

namespace Lisbeth.Bot.API.Helpers
{
    public static class RecurringJobHelper
    {
        public static void ScheduleAutomaticUnmute() =>
            RecurringJob.AddOrUpdate<IDiscordMuteService>("unmute", x => x.UnmuteCheckAsync(), Cron.Minutely);
        public static void ScheduleAutomaticUnban() =>
            RecurringJob.AddOrUpdate<IDiscordBanService>("unban", x => x.UnbanCheckAsync(), Cron.Minutely);

        public static void ScheduleAllDefined()
        {
            ScheduleAutomaticUnban();
            ScheduleAutomaticUnmute();
        }
    }
}
