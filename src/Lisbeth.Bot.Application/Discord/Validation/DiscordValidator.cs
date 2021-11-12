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
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;

namespace Lisbeth.Bot.Application.Discord.Validation;

public class DiscordValidator<TDiscordType> : IDiscordValidator<TDiscordType> where TDiscordType : class
{
    private readonly DiscordClient _client;

    public DiscordValidator(DiscordClient client, ulong objectId)
    {
        ObjectId = objectId;
        _client = client;
    }

    public ulong ObjectId { get; }
    public TDiscordType? RetrievedObject { get; private set; }
    public Exception? Exception { get; private set; }

    public async Task<bool> IsValidAsync()
    {
        try
        {
            RetrievedObject = RetrievedObject switch
            {
                DiscordGuild => await _client.GetGuildAsync(ObjectId) as TDiscordType,
                DiscordChannel => await _client.GetChannelAsync(ObjectId) as TDiscordType,
                DiscordUser => await _client.GetUserAsync(ObjectId) as TDiscordType,
                _ => throw new ArgumentOutOfRangeException()
            };
        }
        catch (Exception ex)
        {
            Exception = ex;
            return false;
        }

        return true;
    }
}