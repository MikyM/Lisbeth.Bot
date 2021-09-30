using System;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;
using Serilog;

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
