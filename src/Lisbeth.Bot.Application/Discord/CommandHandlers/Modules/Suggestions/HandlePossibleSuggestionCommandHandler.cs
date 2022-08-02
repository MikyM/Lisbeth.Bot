using DSharpPlus;
using DSharpPlus.Entities;
using Lisbeth.Bot.Application.Discord.Commands.Modules.Suggestions;
using Lisbeth.Bot.DataAccessLayer.Specifications.Guild;
using MikyM.CommandHandlers;
using MikyM.Common.Utilities.Results;
using MikyM.Discord.Extensions.BaseExtensions;
using MikyM.Discord.Interfaces;

namespace Lisbeth.Bot.Application.Discord.CommandHandlers.Modules.Suggestions;

[UsedImplicitly]
public class HandlePossibleSuggestionCommandHandler : ICommandHandler<HandlePossibleSuggestionCommand>
{
    private readonly IGuildDataService _guildDataService;
    private readonly IDiscordService _discord;

    public HandlePossibleSuggestionCommandHandler(IGuildDataService guildDataService, IDiscordService discord)
    {
        _guildDataService = guildDataService;
        _discord = discord;
    }

    public async Task<Result> HandleAsync(HandlePossibleSuggestionCommand command)
    {
        var guildRes =
            await _guildDataService.GetSingleBySpecAsync(
                new ActiveGuildByDiscordIdWithSuggestionConfigSpec(command.EventData.Guild.Id));

        if (!guildRes.IsDefined(out var guild))
            return Result.FromSuccess();
        if (!guild.IsSuggestionModuleEnabled)
            return Result.FromSuccess();
        
        if (command.EventData.Channel.Id != guild.SuggestionConfig.SuggestionChannelId)
            return Result.FromSuccess();

        _ = _guildDataService.BeginUpdate(guild);
        var suggestion = guild.AddSuggestion(command.EventData.Message.Content, command.EventData.Message.Id, command.EventData.Author.Id, command.EventData.Author.GetFullUsername());

        if (guild.SuggestionConfig.ShouldCreateThreads)
        {
            var thread = await command.EventData.Message.CreateThreadAsync($"{suggestion.Id}",
                command.EventData.Guild.PremiumTier is PremiumTier.Tier_2 or PremiumTier.Tier_3
                    ? AutoArchiveDuration.ThreeDays : AutoArchiveDuration.Day, $"Suggestion made by {command.EventData.Author.GetFullUsername()}");

            suggestion.ThreadId = thread.Id;
        }
        
        _ = await _guildDataService.CommitAsync();
        
        await Task.Delay(1000);

        if (guild.SuggestionConfig.ShouldAddVoteReactions)
        {
            await command.EventData.Message.CreateReactionAsync(DiscordEmoji.FromName(_discord.Client, ":arrow_up:"));
            await Task.Delay(500);
            await command.EventData.Message.CreateReactionAsync(DiscordEmoji.FromName(_discord.Client, ":arrow_down:"));
        }
        
        return Result.FromSuccess();
    }
}
