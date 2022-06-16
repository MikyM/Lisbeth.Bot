using DSharpPlus.Entities;
using Lisbeth.Bot.Application.Discord.Commands.ServerBooster;
using Lisbeth.Bot.DataAccessLayer.Specifications.Guild;
using MikyM.CommandHandlers;
using MikyM.Common.Utilities.Results;

namespace Lisbeth.Bot.Application.Discord.CommandHandlers.ServerBooster;

[UsedImplicitly]
public class AddServerBoosterCommandHandler : ICommandHandler<AddServerBoosterCommand>
{
    private readonly IGuildDataService _guildDataService;

    public AddServerBoosterCommandHandler(IGuildDataService guildDataService)
    {
        _guildDataService = guildDataService;
    }

    public async Task<Result> HandleAsync(AddServerBoosterCommand command)
    {
        var guildRes =
            await _guildDataService.GetSingleBySpecAsync(
                new ActiveGuildByIdSpec(command.Guild.Id));

        if (!guildRes.IsDefined(out var guildCfg))
            return Result.FromError(guildRes);
        
        var embed = new DiscordEmbedBuilder();
        var channel = command.Guild.SystemChannel;
        
        embed.WithTitle("Member has boosted the server");
        embed.WithThumbnail(command.Member.AvatarUrl);
        embed.AddField("Member's ID", $"[{command.Member.Id}](https://discordapp.com/users/{command.Member.Id})", true);
        embed.AddField("Member's mention", $"{command.Member.Mention}", true);
        embed.AddField("Joined guild at", $"{command.Member.JoinedAt}");
        embed.WithColor(new DiscordColor(guildCfg.EmbedHexColor));
        embed.WithFooter($"Member ID: {command.Member.Id}");

        _ = await _guildDataService.AddServerBoosterAsync(command.Guild.Id, command.Member.Id, DateTime.UtcNow, true);
        

        try
        {
            await channel.SendMessageAsync(embed);
        }
        catch
        {
            // ignore
        }

        return Result.FromSuccess();
    }
}
