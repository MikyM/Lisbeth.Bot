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
using MikyM.Discord.Extensions.BaseExtensions;

namespace Lisbeth.Bot.Application.Discord.Extensions;

public static class DiscordMemberExtensions
{
    public static async Task<Result> MuteAsync(this DiscordMember member, ulong roleId)
    {
        if (member.IsModerator()) return new DiscordNotAuthorizedError();

        if (!member.Guild.Roles.TryGetValue(roleId, out var mutedRole)) return new DiscordNotFoundError(nameof(roleId));

        try
        {
            await member.GrantRoleAsync(mutedRole);
        }
        catch (Exception ex)
        {
            return ex;
        }

        return Result.FromSuccess();
    }

    public static async Task<Result> UnmuteAsync(this DiscordMember member, ulong roleId)
    {
        if (member.IsModerator()) return new DiscordNotAuthorizedError();

        if (!member.Guild.Roles.TryGetValue(roleId, out var mutedRole)) return new DiscordNotFoundError(nameof(roleId));

        try
        {
            await member.RevokeRoleAsync(mutedRole);
        }
        catch (Exception ex)
        {
            return ex;
        }

        return Result.FromSuccess();
    }

    public static bool HasRole(this DiscordMember member, ulong roleId, out DiscordRole? role)
        => (role = member.Roles.FirstOrDefault(x => x.Id == roleId)) is not null;

    public static bool CanAccessTag(this DiscordMember member, Tag tag)
        => tag.AllowedRoleIds.Count == 0 && tag.AllowedUserIds.Count == 0 || tag.AllowedRoleIds.Any(x => member.Roles.Select(r => r.Id).Contains(x)) || tag.AllowedUserIds.Contains(member.Id);
}
