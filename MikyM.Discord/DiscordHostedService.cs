using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MikyM.Discord.Interfaces;
using MikyM.Discord.Services;
using OpenTracing;

namespace MikyM.Discord
{
    /// <summary>
    ///     Brings a <see cref="IDiscordService" /> online.
    /// </summary>
    [UsedImplicitly]
    public class DiscordHostedService : IHostedService
    {
        private readonly IDiscordService _discordClient;

        private readonly ILogger<DiscordHostedService> _logger;

        private readonly ITracer _tracer;

        public DiscordHostedService(
            IDiscordService discordClient,
            ITracer tracer,
            ILogger<DiscordHostedService> logger)
        {
            _discordClient = discordClient;
            _tracer = tracer;
            _logger = logger;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            ((DiscordService)_discordClient).Initialize();

            using (_tracer.BuildSpan(nameof(_discordClient.Client.ConnectAsync)).StartActive(true))
            {
                _logger.LogInformation("Connecting to Discord API...");
                await _discordClient.Client.ConnectAsync();
                _logger.LogInformation("Connected");
            }
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            await _discordClient.Client.DisconnectAsync();
        }
    }
}