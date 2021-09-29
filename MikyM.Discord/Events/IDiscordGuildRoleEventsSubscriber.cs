using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.EventArgs;

namespace MikyM.Discord.Events
{
    public interface IDiscordGuildRoleEventsSubscriber
    {
        /// <summary>
        ///     Fired when a guild role is created.
        ///     For this Event you need the <see cref="DiscordIntents.Guilds" /> intent specified in
        ///     <seealso cref="DiscordConfiguration.Intents" />
        /// </summary>
        public Task DiscordOnGuildRoleCreated(DiscordClient sender, GuildRoleCreateEventArgs args);

        /// <summary>
        ///     Fired when a guild role is updated.
        ///     For this Event you need the <see cref="DiscordIntents.Guilds" /> intent specified in
        ///     <seealso cref="DiscordConfiguration.Intents" />
        /// </summary>
        public Task DiscordOnGuildRoleUpdated(DiscordClient sender, GuildRoleUpdateEventArgs args);

        /// <summary>
        ///     Fired when a guild role is updated.
        ///     For this Event you need the <see cref="DiscordIntents.Guilds" /> intent specified in
        ///     <seealso cref="DiscordConfiguration.Intents" />
        /// </summary>
        public Task DiscordOnGuildRoleDeleted(DiscordClient sender, GuildRoleDeleteEventArgs args);
    }
}
