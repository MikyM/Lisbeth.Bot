using System;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;
using Serilog;

namespace Lisbeth.Bot.Application.Discord.Extensions
{
    public static class DiscordUserExtensions
    {
        public static string GetFullUsername(this DiscordUser user)
        {
            return user.Username + "#" + user.Discriminator;
        }
    }
}
