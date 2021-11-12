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

using DSharpPlus;
using DSharpPlus.Entities;
using Emzi0767.Utilities;
using FluentValidation;
using FluentValidation.Validators;
using System.Threading;

namespace Lisbeth.Bot.Application.Validation.ReusablePropertyValidation;

public sealed class DiscordUserIdValidator<T> : IAsyncPropertyValidator<T, ulong>
{
    private readonly DiscordClient _discord;
    private bool _suppressMemberCheck;
    private bool _doesGuildExist = true;
    private object? _guildId;

    public DiscordUserIdValidator(DiscordClient discord, bool suppressMemberCheck = false)
    {
        _discord = discord;
        _suppressMemberCheck = suppressMemberCheck;
    }

    public async Task<bool> IsValidAsync(ValidationContext<T> context, ulong value, CancellationToken cancellation)
    {
        if (_discord.CurrentApplication.Owners.Any(x => x.Id == value)) _suppressMemberCheck = true;

        var data = context.InstanceToValidate.ToDictionary();
        if (!_suppressMemberCheck && data.TryGetValue("GuildId", out _guildId))
        {
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
                var user = await guild.GetMemberAsync(value);
                if (user is null) return false;
            }
            catch (Exception)
            {
                return false;
            }

            return true;
        }

        try
        {
            var user = await _discord.GetUserAsync(value);
            if (user is null) return false;
        }
        catch (Exception)
        {
            return false;
        }

        return true;
    }

    public string GetDefaultMessageTemplate(string errorCode)
    {
        if (!_doesGuildExist)
            return "'{PropertyName}' is not a valid Discord Id or a discord guild with given Id doesn't exist.";

        return _guildId is not null
            ? "'{PropertyName}' is not a valid Discord Id or a discord member with given Id doesn't exist / isn't guilds member."
            : "'{PropertyName}' is not a valid Discord Id or a discord user with given Id doesn't exist.";
    }


    public string Name => "DiscordIdPropertyValidator";
}