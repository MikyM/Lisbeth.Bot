//  This file is part of Lisbeth.Bot project
//
//  Copyright (C) 2021 VTPDevelopment - @VelvetThePanda
//  Copyright (C) 2021-2022 Krzysztof Kupisz - MikyM
//
//  Licensed under the Apache License, Version 2.0 (the "License");
//  you may not use this file except in compliance with the License.
//  You may obtain a copy of the License at
//
//  http://www.apache.org/licenses/LICENSE-2.0
//
//  Unless required by applicable law or agreed to in writing, software
//  distributed under the License is distributed on an "AS IS" BASIS,
//  WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//  See the License for the specific language governing permissions and
//  limitations under the License.
//  
//  Original license can be found in ./licenses directory.
//  This file has been edited after obtaining it's copy.

using System.Net.Http;
using System.Text.RegularExpressions;
using Lisbeth.Bot.Application.Discord.Commands.Mute;
using Lisbeth.Bot.Application.Services;
using Lisbeth.Bot.DataAccessLayer.Specifications.Guild;
using Lisbeth.Bot.Domain;
using Lisbeth.Bot.Domain.DTOs.Request.Ban;
using Lisbeth.Bot.Domain.DTOs.Request.Mute;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Lisbeth.Bot.Application.Discord.Services;

/// <summary>
///     Handles potential phishing links.
/// </summary>
[UsedImplicitly]
[ServiceImplementation<IDiscordPhishingDetectionService>(ServiceLifetime.InstancePerLifetimeScope)]
public sealed class DiscordPhishingDetectionService : IDiscordPhishingDetectionService
{
    private const string PhishingReason = "Message contained a phishing link.";
    private const string PhishingSuspiciousReason = "Message contained suspicious content and needs verification:\n\n";

    private static readonly Regex LinkRegex =
        new(
            @"[.]*(?:https?:\/\/(www\.)?)?(?<link>[-a-zA-Z0-9@:%._\+~#=]{1,256}\.[a-zA-Z0-9()]{1,6})\b([-a-zA-Z0-9()@:%_\+.~#?&\/\/=]*)");

    private readonly PhishingGatewayService _phishGateway;
    private readonly ILogger<DiscordPhishingDetectionService> _logger;
    private readonly IGuildDataService _guildDataService;
    private readonly IDiscordService _discord;
    private readonly IDiscordBanService _banService;
    private readonly ICommandHandlerResolver _commandHandlerFactory;
    private readonly IOptions<BotConfiguration> _options;
    private readonly HttpClient _httpClient;

    public DiscordPhishingDetectionService(PhishingGatewayService phishGateway,
        ILogger<DiscordPhishingDetectionService> logger,
        IGuildDataService guildDataService, IDiscordService discord, IDiscordBanService banService,
        ICommandHandlerResolver commandHandlerFactory, IOptions<BotConfiguration> options, IHttpClientFactory httpClientFactory)
    {
        _phishGateway = phishGateway;
        _logger = logger;
        _guildDataService = guildDataService;
        _discord = discord;
        _banService = banService;
        _commandHandlerFactory = commandHandlerFactory;
        _options = options;
        _httpClient = httpClientFactory.CreateClient();
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
        if (!res.IsDefined(out var config) || config.IsDisabled)
            return Result.FromSuccess();

        if (config.PhishingDetection is PhishingDetection.Disabled)
            return Result.FromSuccess(); // Phishing detection is disabled.

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

            if (_phishGateway.IsBlacklisted(link))
            {
                _logger.LogInformation("Detected phishing link");
                return await HandleDetectedPhishingAsync(message);
            }

            if (_options.Value.Shorteners.Contains(link)) // follow shortened link
            {
                using var request = new HttpRequestMessage(HttpMethod.Get, link);
                using var response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseContentRead);
                
                if (response.IsSuccessStatusCode)
                {
                    var location = response.Headers.Location;
                    if (location is not null)
                    {
                        var shortLink = location.ToString();
                        if (_phishGateway.IsBlacklisted(shortLink))
                        {
                            _logger.LogInformation("Detected phishing link that was shortened");
                            return await HandleDetectedPhishingAsync(message);
                        }
                    }
                }
            }

            if ((message.Content.Contains("pls") || message.Content.Contains("please") || message.Content.Contains("plz")) 
                && message.Content.Contains("first game") 
                && message.Content.Contains("test") 
                && (message.Content.Contains("pw") || message.Content.Contains("pass") || message.Content.Contains("password")))
            {
                _logger.LogInformation("Detected suspicious first game message");
                return await HandleDetectedPhishingAsync(message, true);
            }
        }

        return Result.FromSuccess();
    }

    /// <summary>
    ///     Handles a detected phishing link.
    /// </summary>
    /// <param name="message">Message to handle.</param>
    /// <param name="isOnlySuspicious">Whether the content of the message isn't a 100% hit</param>
    private async Task<Result> HandleDetectedPhishingAsync(DiscordMessage message, bool isOnlySuspicious = false)
    {
        var res = await _guildDataService.GetSingleBySpecAsync(new ActiveGuildByIdSpec(message.Channel.Guild.Id));
        if (!res.IsDefined(out var config) || config.IsDisabled) return Result.FromSuccess();
        if (config.PhishingDetection is PhishingDetection.Disabled)
            return Result.FromSuccess(); // Phishing detection is disabled.
        
        await message.Channel.DeleteMessageAsync(message);

        var self = _discord.Client.CurrentUser.Id;
        
        if (isOnlySuspicious)
        {
            var mr = await _commandHandlerFactory
                .GetHandler<IAsyncCommandHandler<ApplyMuteCommand, DiscordEmbed>>()
                .HandleAsync(new ApplyMuteCommand(new MuteApplyReqDto(message.Author.Id, message.Channel.Guild.Id,
                    self,
                    DateTime.UtcNow.AddDays(7), $"{PhishingSuspiciousReason}{message.Content}")));
            return mr.IsSuccess ? Result.FromSuccess() : Result.FromError(mr);
        }

        switch (config.PhishingDetection)
        {
            case PhishingDetection.Disabled:
                return Result.FromSuccess();
            case PhishingDetection.Mute:
                var mr = await _commandHandlerFactory
                    .GetHandler<IAsyncCommandHandler<ApplyMuteCommand, DiscordEmbed>>()
                    .HandleAsync(new ApplyMuteCommand(new MuteApplyReqDto(message.Author.Id, message.Channel.Guild.Id,
                        self,
                        DateTime.UtcNow.AddDays(7), PhishingReason)));
                return mr.IsSuccess ? Result.FromSuccess() : Result.FromError(mr);
            case PhishingDetection.Kick:
                try
                {
                    await ((DiscordMember)message.Author).RemoveAsync(PhishingReason);
                }
                catch
                {
                    return new DiscordError();
                }
                return Result.FromSuccess();
            case PhishingDetection.Ban:
                var br = await _banService.BanAsync(new BanApplyReqDto(message.Author.Id,
                    message.Channel.Guild.Id, self, DateTime.MaxValue, PhishingReason));
                return br.IsSuccess ? Result.FromSuccess() : Result.FromError(br);
            default:
                throw new ArgumentOutOfRangeException();
        }
    }
}
