// This file is part of Lisbeth.Bot project
//
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

            return _discord.Client.Ping > 300 ? HealthCheckResult.Degraded() : HealthCheckResult.Healthy();
        }
    }
}