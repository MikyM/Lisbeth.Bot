namespace Lisbeth.Bot.Application.Discord.Extensions;

public static class DiscordEmbedBuilderExtensions
{
    public static DiscordEmbedBuilder AddInvisibleField(this DiscordEmbedBuilder builder, bool inline = true)
    {
        builder.AddField("\u200b", "\u200b", inline);
        return builder;
    }
}
