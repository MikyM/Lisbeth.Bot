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

namespace Lisbeth.Bot.Application.Discord.Extensions;

public static class DiscordChannelExtensions
{
    public static async Task<Result> CreateMuteOverwriteAsync(this DiscordChannel channel, ulong mutedRoleId)
    {
        try
        {
            if (!channel.Guild.RoleExists(mutedRoleId, out var mutedRole)) return new NotFoundError($"Role with Id: {mutedRoleId} does not exist in this channels guild.");
            await channel.AddOverwriteAsync(mutedRole,
                deny: Permissions.SendMessages | Permissions.SendMessagesInThreads |
                      Permissions.AddReactions | Permissions.CreatePrivateThreads |
                      Permissions.CreatePublicThreads);
        }
        catch (Exception ex)
        {
            return ex;
        }

        return Result.FromSuccess();
    }

    public static async Task<Result> CreateMuteOverwriteAsync(this DiscordChannel channel, DiscordRole mutedRole)
    {
        try
        {
            if (!channel.Guild.RoleExists(mutedRole.Id, out _)) return new NotFoundError($"Role with Id: {mutedRole.Id} does not exist in this channels guild.");
            await channel.AddOverwriteAsync(mutedRole,
                deny: Permissions.SendMessages | Permissions.SendMessagesInThreads |
                      Permissions.AddReactions | Permissions.CreatePrivateThreads |
                      Permissions.CreatePublicThreads);
        }
        catch (Exception ex)
        {
            return ex;
        }

        return Result.FromSuccess();
    }
}
