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

    private static readonly ulong[] GuildIds = [ 942512020656357396, 1122172811293769828, 596706031954952192, 432630458837106699, 717213120837451816 ]; 

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        var guilds = _discordService.Client.Guilds;
        foreach (var (_, guild) in guilds)
        {
            if (!GuildIds.Contains(guild.Id))
            {
                await guild.LeaveAsync();
            }
        }
        
        //return Task.CompletedTask;
        // CLEARS COMMANDS
        /*await _discordService.Client.BulkOverwriteGlobalApplicationCommandsAsync(Array.Empty<DiscordApplicationCommand>());
        await _discordService.Client.BulkOverwriteGuildApplicationCommandsAsync(_options.Value.TestGuildId, Array.Empty<DiscordApplicationCommand>());*/
    }

    public Task StopAsync(CancellationToken cancellationToken)
        => Task.CompletedTask;
}
