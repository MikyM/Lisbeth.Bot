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

using System.Threading;
using DSharpPlus;
using FluentValidation;
using FluentValidation.Validators;

namespace Lisbeth.Bot.Application.Validation.ReusablePropertyValidation;

public sealed class DiscordGuildIdValidator<T> : IAsyncPropertyValidator<T, ulong>
{
    private readonly DiscordClient _discord;
    private readonly bool _suppressCacheCheck;

    public DiscordGuildIdValidator(DiscordClient discord, bool suppressCacheCheck = false)
    {
        _discord = discord;
        _suppressCacheCheck = suppressCacheCheck;
    }

    public async Task<bool> IsValidAsync(ValidationContext<T> context, ulong value, CancellationToken cancellation)
    {
        try
        {
            var result = await _discord.GetGuildAsync(value);
            if (result is null) return false;
            if (!_suppressCacheCheck && !_discord.Guilds.TryGetValue(value, out _)) return false;
        }
        catch (Exception)
        {
            return false;
        }

        return true;
    }

    public string GetDefaultMessageTemplate(string errorCode)
    {
        return "'{PropertyName}' is not a valid Discord Id or a discord guild with given Id doesn't exist.";
    }

    public string Name => "DiscordGuildIdValidator";
}