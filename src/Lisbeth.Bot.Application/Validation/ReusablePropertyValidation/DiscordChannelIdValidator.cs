﻿// This file is part of Lisbeth.Bot project
//
// Copyright (C) 2021-2022 Krzysztof Kupisz - MikyM
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

using Fasterflect;
using FluentValidation;
using FluentValidation.Validators;

namespace Lisbeth.Bot.Application.Validation.ReusablePropertyValidation;

public sealed class DiscordChannelIdValidator<T> : IAsyncPropertyValidator<T, ulong>
{
    private readonly DiscordClient _discord;
    private readonly bool _suppressGuildCheck;
    private bool _doesGuildExist = true;
    private object? _guildId;

    public DiscordChannelIdValidator(DiscordClient discord, bool suppressGuildCheck = false)
    {
        _discord = discord;
        _suppressGuildCheck = suppressGuildCheck;
    }

    public async Task<bool> IsValidAsync(ValidationContext<T> context, ulong value, CancellationToken cancellation)
    {
        try
        {
            _guildId = context.InstanceToValidate.GetPropertyValue("GuildId");
            
            DiscordGuild guild;
            try
            {
                guild = await _discord.GetGuildAsync((ulong)_guildId);
                if (guild is null)
                {
                    _doesGuildExist = false;
                    return false;
                }
            }
            catch (Exception)
            {
                _doesGuildExist = false;
                return false;
            }

            try
            {
                var channel = await _discord.GetChannelAsync(value);
                if (channel is null) return false;
                if (!_suppressGuildCheck && channel.Guild.Id != guild.Id) return false;
            }
            catch (Exception)
            {
                return false;
            }

            return true;
        }
        catch
        {
            // ignored
        }

        try
        {
            var channel = _discord.GetChannelAsync(value);
            if (channel is null) return false;
        }
        catch (Exception)
        {
            return false;
        }

        return true;
    }

    public string GetDefaultMessageTemplate(string errorCode)
    {
        return _doesGuildExist
            ? "'{PropertyName}' is not a valid Discord Id or a discord channel with given Id doesn't exist."
            : "'{PropertyName}' is not a valid Discord Id or a discord guild with given Id doesn't exist.";
    }

    public string Name => "DiscordChannelIdValidator";
}
