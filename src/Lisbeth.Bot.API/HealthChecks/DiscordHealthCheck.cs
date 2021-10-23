using System;
using System.Threading;
using System.Threading.Tasks;
using DSharpPlus.Net;
using JetBrains.Annotations;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using MikyM.Discord.Interfaces;

namespace Lisbeth.Bot.API.HealthChecks
{
    [UsedImplicitly]
    public class DiscordHealthCheck : IHealthCheck
    {
        private readonly IDiscordService _discord;

        public DiscordHealthCheck(IDiscordService discord)
        {
            _discord = discord;
        }

        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context,
            CancellationToken cancellationToken = new())
        {
            GatewayInfo info;

            try
            {
                info = await _discord.Client.GetGatewayInfoAsync();
            }
            catch (Exception)
            {
                return HealthCheckResult.Unhealthy();
            }

            if (info is null) return HealthCheckResult.Unhealthy();

            return _discord.Client.Ping > 200 ? HealthCheckResult.Degraded() : HealthCheckResult.Healthy();
        }
    }
}