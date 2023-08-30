// This file is part of Lisbeth.Bot project
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


using Emzi0767.Utilities;
using Fasterflect;
using FluentValidation;
using FluentValidation.Validators;

namespace Lisbeth.Bot.Application.Validation.ReusablePropertyValidation;

public class DiscordSnowflakeIdValidator<T> : IAsyncPropertyValidator<T, ulong>
{
    private readonly DiscordClient _discord;
    private bool _channelExists = true;
    private bool _guildExists = true;
    private object? _guildId;
    private bool _memberExists = true;
    private bool _roleExists = true;

    public DiscordSnowflakeIdValidator(DiscordClient discord)
    {
        _discord = discord;
    }

    public async Task<bool> IsValidAsync(ValidationContext<T> context, ulong value, CancellationToken cancellation)
    {
        try
        {
            _guildId = context.InstanceToValidate.GetPropertyValue("GuildId");
        }
        catch
        {
            return false;
        }

        DiscordGuild guild;
        try
        {
            guild = await _discord.GetGuildAsync((ulong)_guildId);
            if (guild is null)
            {
                _guildExists = false;
                return false;
            }
        }
        catch (Exception)
        {
            _guildExists = false;
            return false;
        }

        try
        {
            var user = await guild.GetMemberAsync(value);
            if (user is null) _memberExists = false;
        }
        catch (Exception)
        {
            _memberExists = false;
        }

        try
        {
            var role = guild.GetRole(value);
            if (role is null) _roleExists = false;
        }
        catch (Exception)
        {
            _roleExists = false;
        }

        try
        {
            var channel = await _discord.GetChannelAsync(value);
            if (channel is null) _channelExists = false;
            if (channel is not null && channel.Guild.Id != guild.Id) _channelExists = false;
        }
        catch (Exception)
        {
            _channelExists = false;
        }

        return _roleExists || _memberExists || _channelExists;
    }

    public string GetDefaultMessageTemplate(string errorCode)
    {
        return _guildExists
            ? "'{PropertyName}' is not a valid Discord Id or a discord member or a role with given Id doesn't exist / isn't guilds part."
            : "'{PropertyName}' is not a valid Discord Id or a discord guild does not exist.";
    }


    public string Name => "DiscordIdPropertyValidator";
}
