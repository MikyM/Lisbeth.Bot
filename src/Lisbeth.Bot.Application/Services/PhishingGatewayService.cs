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


using System.Collections.Generic;
using System.Net.Http;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Toolkit.HighPerformance.Buffers;

namespace Lisbeth.Bot.Application.Services;

public sealed class PhishingGatewayService : IHostedService
{
    private const string HeaderName = "X-Identity";
    private const string ApiUrl = "https://phish.sinking.yachts/v2/all";
    private const string WebSocketUrl = "wss://phish.sinking.yachts/feed";

    private const int WebSocketBufferSize = 16 * 1024;

    private readonly HttpClient _client;
    private readonly ClientWebSocket _ws = new();
    private readonly HashSet<string> _domains = new();
    private readonly CancellationTokenSource _cts = new();
    private readonly ILogger<PhishingGatewayService> _logger;

    public PhishingGatewayService(ILogger<PhishingGatewayService> logger, HttpClient client)
    {
        _logger = logger;
        _client = client;
        _ws.Options.SetRequestHeader(HeaderName, "Lisbeth.Bot");
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        if (!await GetDomainsAsync())
        {
            _logger.LogCritical("Failed to retrieve domains. API - Unavailable");
            return;
        }

        try
        {
            await _ws.ConnectAsync(new(WebSocketUrl), CancellationToken.None);
            _logger.LogInformation("Phishing gateway is up and running");
        }
        catch (WebSocketException)
        {
            _logger.LogCritical("Failed to establish a websocket connection. API - Unavailable");
            return;
        }

        _ = Task.Run(ReceiveLoopAsync);
    }

#pragma warning disable CS1998
    public async Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Cancellation requested. Stopping service");

        _cts.Cancel();
    }
#pragma warning restore CS1998

    public bool IsBlacklisted(string link) => _domains.Contains(link);

    private async Task ReceiveLoopAsync()
    {
        try
        {
            CancellationToken stoppingToken = _cts.Token;

            // 16KB cache; should be more than sufficient for the foreseeable future. //
            using var buffer = new ArrayPoolBufferWriter<byte>(WebSocketBufferSize);

            while (!stoppingToken.IsCancellationRequested)
            {
                // See https://github.com/discord-net/Discord.Net/commit/ac389f5f6823e3a720aedd81b7805adbdd78b66d 
                // for explanation on the cancellation token
                // TL;DR passing cancellation token to websocket kills the socket //

                ValueWebSocketReceiveResult result;
                do
                {
                    Memory<byte> mem = buffer.GetMemory(WebSocketBufferSize);
                    result = await _ws.ReceiveAsync(mem, CancellationToken.None);

                    if (result.MessageType is WebSocketMessageType.Close) break; // Damn it, CloudFlare. //

                    buffer.Advance(result.Count);
                } while (!result.EndOfMessage);

                if (result.MessageType is WebSocketMessageType.Close)
                {
                    if (await RestartWebsocketAsync()) continue;

                    return;
                }

                string? json = Encoding.UTF8.GetString(buffer.WrittenSpan);

                //JObject? payload = JObject.Parse(json);
                var payload = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(json);

                var command = payload?["type"].GetString(); // "add" or "delete"
                var domains = payload?["domains"].Deserialize<string[]>(); // An array of domains. 

                if (domains is not null)
                    HandleWebsocketCommand(command, domains);

                buffer.Clear(); // Clear, or you'll get JSON exceptions //
            }
        }
        catch (Exception e)
        {
            _logger.LogWarning(e, "Websocket threw an exception. API - Unavailable");
        }
        finally
        {
            _logger.LogInformation("Closing websocket");
            
            await _ws.CloseAsync(WebSocketCloseStatus.NormalClosure, "Ready to shut down.",
                CancellationToken.None);
        }
    }

    private async Task<bool> RestartWebsocketAsync()
    {
        if (_ws.State is not (WebSocketState.Aborted or WebSocketState.Closed))
            await _ws.CloseAsync(WebSocketCloseStatus.NormalClosure, "Close requested. I'll be back soon.",
                CancellationToken.None);

        try
        {
            await _ws.ConnectAsync(new(WebSocketUrl), CancellationToken.None);
            return true;
        }
        catch
        {
            _logger.LogWarning("Could not connect to phishing. API - Unavailable");
            return false;
        }
    }

    private async Task<bool> GetDomainsAsync()
    {
        _logger.LogTrace("Getting domains...");
        using var req = new HttpRequestMessage(HttpMethod.Get, ApiUrl)
        {
            Headers = { { HeaderName, "Lisbeth.Bot" } } // X-Identifier MUST be set or we get 403'd //
        };

        using HttpResponseMessage? res = await _client.SendAsync(req);

        if (!res.IsSuccessStatusCode)
        {
            _logger.LogDebug("Unable to get domains. ({Status}, {Reason})", res.StatusCode, res.ReasonPhrase);
            return false;
        }

        string? json = await res.Content.ReadAsStringAsync();
        string[]? payload = JsonSerializer.Deserialize<string[]>(json)!;

        foreach (var domain in payload) _domains.Add(domain);

        _logger.LogInformation("Retrieved {Count} phishing domains via REST", payload.Length);

        return true;
    }

    private void HandleWebsocketCommand(string? command, IReadOnlyCollection<string> domains)
    {
        switch (command)
        {
            case "add":
                _logger.LogDebug("Adding {Count} new domains.", domains.Count);

                foreach (var domain in domains) _domains.Add(domain);
                break;

            case "delete":
                _logger.LogDebug("Removing {Count} domains.", domains.Count);
                foreach (var domain in domains) _domains.Remove(domain);
                break;

            default:
                _logger.LogDebug("Unknown command from websocket ({Command}); skipping.", command);
                break;
        }
    }
}
