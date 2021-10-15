// This file is part of Lisbeth.Bot project
//
// Copyright (C) 2021 MikyM
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

using System.Linq;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;

namespace Lisbeth.Bot.Application.Discord.Extensions
{
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

        public static async Task<bool> Mute(this DiscordMember member, DiscordGuild guild)
        {
            if (member.Permissions.HasPermission(Permissions.BanMembers))
                return false;

            DiscordRole mutedRole = guild.Roles.FirstOrDefault(x => x.Value.Name == "Muted").Value;
            await member.GrantRoleAsync(mutedRole);

            return true;
        }

        public static async Task<bool> Unmute(this DiscordMember member, DiscordGuild guild)
        {
            if (member.Permissions.HasPermission(Permissions.BanMembers))
                return false;

            DiscordRole mutedRole = guild.Roles.FirstOrDefault(x => x.Value.Name == "Muted").Value;
            await member.RevokeRoleAsync(mutedRole);

            return true;
        }
    }
}