﻿// This file is part of Lisbeth.Bot project
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
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Lisbeth.Bot.Application.Validation.ReusablePropertyValidation
{
    public class DiscordSnowflakeIdValidator<T> : IAsyncPropertyValidator<T, ulong>
    {
        private readonly DiscordClient _discord;
        private object _guildId;
        private bool _roleExists = true;
        private bool _memberExists = true;
        private bool _guildExists = true;

        public DiscordSnowflakeIdValidator(DiscordClient discord)
        {
            _discord = discord;
        }

        public async Task<bool> IsValidAsync(ValidationContext<T> context, ulong value, CancellationToken cancellation)
        {
            var data = context.InstanceToValidate.ToDictionary();

            if (!data.TryGetValue("GuildId", out _guildId)) return false;

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
                var role = guild.Roles.FirstOrDefault(x => x.Key == value).Value;
                if (role is null) _roleExists = false;
            }
            catch (Exception)
            {
                _roleExists = false;
            }

            return _roleExists || _memberExists;

        }

        public string GetDefaultMessageTemplate(string errorCode)
        {
            return "'{PropertyName}' is not a valid Discord Id or a discord member or a role with given Id doesn't exist / isn't guilds part.";
        }


        public string Name => "DiscordIdPropertyValidator";
    }
}