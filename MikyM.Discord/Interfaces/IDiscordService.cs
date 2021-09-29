using DSharpPlus;

namespace MikyM.Discord.Interfaces
{
    public interface IDiscordService
    {
        /// <summary>
        ///     The underlying <see cref="DiscordClient"/>.
        /// </summary>
        public DiscordClient Client { get; }
    }
}
