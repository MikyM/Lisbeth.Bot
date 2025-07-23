using Lisbeth.Bot.Domain;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Lisbeth.Bot.Application.Services;

[UsedImplicitly]
public class InitializationService : IHostedService
{
    private readonly IDiscordService _discordService;
    private readonly IOptions<BotConfiguration> _options;
    private readonly ILogger<InitializationService> _logger;

    public InitializationService(IDiscordService discordService, IOptions<BotConfiguration> options, ILogger<InitializationService> logger)
    {
        _discordService = discordService;
        _options = options;
        _logger = logger;
    }
    
    public Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Currently serving {GuildCount} guilds: {@Guilds}",
            _discordService.Client.Guilds.Count,
            _discordService.Client.Guilds.Select(x => new { x.Value.Id, x.Value.Name }));
        
        return Task.CompletedTask;
        // CLEARS COMMANDS
        /*await _discordService.Client.BulkOverwriteGlobalApplicationCommandsAsync(Array.Empty<DiscordApplicationCommand>());
        await _discordService.Client.BulkOverwriteGuildApplicationCommandsAsync(_options.Value.TestGuildId, Array.Empty<DiscordApplicationCommand>());*/
    }

    public Task StopAsync(CancellationToken cancellationToken)
        => Task.CompletedTask;
}
