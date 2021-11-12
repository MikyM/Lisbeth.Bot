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

using DSharpPlus.Entities;

namespace Lisbeth.Bot.Application.Discord.Validation;

public class DiscordGuildValidator<TDiscordType> : IDiscordGuildValidator<TDiscordType> where TDiscordType : class
{
    private readonly DiscordGuild _guild;

    public DiscordGuildValidator(DiscordGuild guild, ulong objectId)
    {
        _guild = guild;
        ObjectId = objectId;
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
                DiscordMember => await _guild.GetMemberAsync(ObjectId) as TDiscordType,
                DiscordChannel => _guild.Channels.FirstOrDefault(x => x.Key == ObjectId) as TDiscordType,
                DiscordRole => _guild.Roles.FirstOrDefault(x => x.Key == ObjectId) as TDiscordType,
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