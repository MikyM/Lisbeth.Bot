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

namespace Lisbeth.Bot.Application.Discord.Extensions;

public static class DiscordMemberExtensions
{
    public static string GetFullUsername(this DiscordMember member)
    {
        return member.Username + "#" + member.Discriminator;
    }

    public static string GetFullDisplayName(this DiscordMember member)
    {
        return member.DisplayName + "#" + member.Discriminator;
    }

    public static async Task<Result> MuteAsync(this DiscordMember member, ulong roleId)
    {
        if (member.IsModerator())
            return new DiscordNotAuthorizedError();

        if (!member.Guild.Roles.TryGetValue(roleId, out var mutedRole)) return new DiscordNotFoundError(nameof(roleId));
        await member.GrantRoleAsync(mutedRole);

        return Result.FromSuccess();
    }

    public static async Task<Result> UnmuteAsync(this DiscordMember member, ulong roleId)
    {
        if (member.IsModerator())
            return new DiscordNotAuthorizedError();

        if (!member.Guild.Roles.TryGetValue(roleId, out var mutedRole)) return new DiscordNotFoundError(nameof(roleId));
        await member.RevokeRoleAsync(mutedRole);

        return Result.FromSuccess();
    }

    public static bool IsModerator(this DiscordMember member)
    {
        return member.Roles.Any(x =>
                   x.Permissions.HasPermission(Permissions.BanMembers)) ||
               member.Permissions.HasPermission(Permissions.BanMembers) ||
               member.Permissions.HasPermission(Permissions.All) ||
               member.IsOwner;
    }

    public static bool IsAdmin(this DiscordMember member)
    {
        return member.Roles.Any(x =>
                   x.Permissions.HasPermission(Permissions.Administrator)) ||
               member.Permissions.HasPermission(Permissions.All) ||
               member.IsOwner;
    }

    public static bool IsBotOwner(this DiscordMember member, DiscordClient client)
    {
        return client.CurrentApplication.Owners.Any(x => x.Id == member.Id);
    }
}