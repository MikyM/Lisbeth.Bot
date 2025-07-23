using Lisbeth.Bot.Domain;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace Lisbeth.Bot.Application.Services;

[UsedImplicitly]
public class InitializationService : IHostedService
{
    private readonly IDiscordService _discordService;
    private readonly IOptions<BotConfiguration> _options;

    public InitializationService(IDiscordService discordService, IOptions<BotConfiguration> options)
    {
        _discordService = discordService;
        _options = options;
    }
    
    public Task StartAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
        // CLEARS COMMANDS
        /*await _discordService.Client.BulkOverwriteGlobalApplicationCommandsAsync(Array.Empty<DiscordApplicationCommand>());
        await _discordService.Client.BulkOverwriteGuildApplicationCommandsAsync(_options.Value.TestGuildId, Array.Empty<DiscordApplicationCommand>());*/
    }

    public Task StopAsync(CancellationToken cancellationToken)
        => Task.CompletedTask;
}
