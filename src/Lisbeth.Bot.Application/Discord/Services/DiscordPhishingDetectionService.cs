// This file is part of Lisbeth.Bot project
//
// Copyright (C) 2021 VTPDevelopment - @VelvetThePanda
// Copyright (C) 2021 Krzysztof Kupisz - MikyM
// 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU Affero General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Affero General Public License for more details.
// 
// You should have received a copy of the GNU Affero General Public License
// along with this program.  If not, see <https://www.gnu.org/licenses/>.

using System.Text.RegularExpressions;
using DSharpPlus.Entities;
using Lisbeth.Bot.Application.Discord.Commands.Mute;
using Lisbeth.Bot.Application.Services;
using Lisbeth.Bot.DataAccessLayer.Specifications.Guild;
using Lisbeth.Bot.Domain.DTOs.Request.Ban;
using Lisbeth.Bot.Domain.DTOs.Request.Mute;
using Microsoft.Extensions.Logging;
using MikyM.Common.Application.CommandHandlers;
using MikyM.Discord.Interfaces;

namespace Lisbeth.Bot.Application.Discord.Services;

/// <summary>
///     Handles potential phishing links.
/// </summary>
[Service]
[RegisterAs(typeof(IDiscordPhishingDetectionService))]
[Lifetime(Lifetime.InstancePerLifetimeScope)]
[UsedImplicitly]
public sealed class DiscordPhishingDetectionService : IDiscordPhishingDetectionService
{
    private const string Phishing = "Message contained a phishing link.";

    private static readonly Regex LinkRegex =
        new(
            @"[.]*(?:https?:\/\/(www\.)?)?(?<link>[-a-zA-Z0-9@:%._\+~#=]{1,256}\.[a-zA-Z0-9()]{1,6})\b([-a-zA-Z0-9()@:%_\+.~#?&\/\/=]*)");

    private readonly PhishingGatewayService _phishGateway;
    private readonly ILogger<DiscordPhishingDetectionService> _logger;
    private readonly IGuildDataService _guildDataService;
    private readonly IDiscordService _discord;
    private readonly IDiscordBanService _banService;
    private readonly ICommandHandlerFactory _commandHandlerFactory;

    public DiscordPhishingDetectionService(PhishingGatewayService phishGateway, ILogger<DiscordPhishingDetectionService> logger,
        IGuildDataService guildDataService, IDiscordService discord, IDiscordBanService banService, ICommandHandlerFactory commandHandlerFactory)
    {
        _phishGateway = phishGateway;
        _logger = logger;
        _guildDataService = guildDataService;
        _discord = discord;
        _banService = banService;
        _commandHandlerFactory = commandHandlerFactory;
    }

    /// <summary>
    ///     Detects any phishing links in a given message.
    /// </summary>
    /// <param name="message">The message to scan.</param>
    public async Task<Result> DetectPhishingAsync(DiscordMessage message)
    {
        if (message.Author is null) return Result.FromSuccess(); // In case it's an edit and it's not in cache.

        if (message.Author.IsBot) return Result.FromSuccess(); // Sus.

        if (message.Channel?.Guild is null) return Result.FromSuccess(); // DM channels are exmepted.

        var res = await _guildDataService.GetSingleBySpecAsync(new ActiveGuildByIdSpec(message.Channel.Guild.Id));
        if (!res.IsDefined(out var config))
            return Result.FromSuccess();

        if (config.PhishingDetection == PhishingDetection.Disabled) return Result.FromSuccess(); // Phishing detection is disabled.

        // As to why I don't use Regex.Match() instead:
        // Regex.Match casts its return value to a non-nullable Match.
        // Run(), the method which it invokes returns Match?, which can cause an unexpected null ref.
        // You'd think this would be documented, but I digress.
        // Source: https://source.dot.net/#System.Text.RegularExpressions/System/Text/RegularExpressions/Regex.cs,388
        MatchCollection links = LinkRegex.Matches(message.Content);

        foreach (Match match in links)
        {
            if (match is null || !match.Success) continue;

            var link = match.Groups["link"].Value;

            if (!_phishGateway.IsBlacklisted(link)) continue;

            _logger.LogInformation("Detected phishing link.");
            return await HandleDetectedPhishingAsync(message);
        }

        return Result.FromSuccess();
    }

    /// <summary>
    ///     Handles a detected phishing link.
    /// </summary>
    /// <param name="message">Message to handle.</param>
    private async Task<Result> HandleDetectedPhishingAsync(DiscordMessage message)
    {

        await message.Channel.DeleteMessageAsync(message);

        var res = await _guildDataService.GetSingleBySpecAsync(new ActiveGuildByIdSpec(message.Channel.Guild.Id));
        if (!res.IsDefined(out var config)) return Result.FromSuccess();

        var self = _discord.Client.CurrentUser.Id;

        switch (config.PhishingDetection)
        {
            case PhishingDetection.Disabled:
                _logger.LogError("Wrong enum chosen for phis detection");
                return new ArgumentError(nameof(config.PhishingDetection), "Wrong enum chosen for phis detection");
            case PhishingDetection.Mute:
                var mr = await _commandHandlerFactory
                    .GetHandler<ICommandHandler<ApplyMuteCommand, DiscordEmbed>>()
                    .HandleAsync(new ApplyMuteCommand(new MuteApplyReqDto(message.Author.Id, message.Channel.Guild.Id,
                        self,
                        DateTime.UtcNow.AddDays(7), Phishing)));
                return mr.IsSuccess ? Result.FromSuccess() : Result.FromError(mr);
            case PhishingDetection.Kick:
                try
                {
                    await ((DiscordMember)message.Author).RemoveAsync(Phishing);
                }
                catch
                {
                    return new DiscordError();
                }
                return Result.FromSuccess();
            case PhishingDetection.Ban:
                var br = await _banService.BanAsync(new BanApplyReqDto(message.Author.Id,
                    message.Channel.Guild.Id, self, DateTime.MaxValue, Phishing));
                return br.IsSuccess ? Result.FromSuccess() : Result.FromError(br);
            default:
                throw new ArgumentOutOfRangeException();
        }
    }
}
