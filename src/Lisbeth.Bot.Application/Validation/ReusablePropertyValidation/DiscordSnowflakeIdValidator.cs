using DSharpPlus.Entities;
using Emzi0767.Utilities;
using FluentValidation;
using FluentValidation.Validators;
using MikyM.Discord.Interfaces;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Lisbeth.Bot.Application.Validation.ReusablePropertyValidation
{
    public class DiscordSnowflakeIdValidator<T> : IAsyncPropertyValidator<T, ulong>
    {
        private readonly IDiscordService _discord;
        private object _guildId;
        private bool _roleExists = true;
        private bool _memberExists = true;
        private bool _guildExists = true;

        public DiscordSnowflakeIdValidator(IDiscordService discord)
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
                guild = await _discord.Client.GetGuildAsync((ulong)_guildId);
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
