using Lisbeth.Bot.Application.Discord.Exceptions;
using Lisbeth.Bot.Application.Discord.SlashCommands.Base;
using Lisbeth.Bot.DataAccessLayer.Specifications.Guild;
using MikyM.Discord.Extensions.BaseExtensions;

namespace Lisbeth.Bot.Application.Discord.SlashCommands;

[SlashModuleLifespan(SlashModuleLifespan.Scoped)]
[UsedImplicitly]
public class UtilitySlashCommands : ExtendedApplicationCommandModule
{
    private readonly IGuildDataService _guildDataService;

    public UtilitySlashCommands(IGuildDataService guildDataService)
    {
        _guildDataService = guildDataService;
    }

    [UsedImplicitly]
    [SlashCommand("avatar", "Command that allows retrieving given user avatar, or your own.")]
    public async Task BanCommand(InteractionContext ctx,
        [Option("user", "User to retrieve avatar for if not for yourself")]
        DiscordUser? user = null)
    {
        await ctx.DeferAsync();

        var guildRes = await _guildDataService.GetSingleBySpecAsync(new ActiveGuildByIdSpec(ctx.Guild.Id));
        if (!guildRes.IsDefined(out var guild))
            throw new DiscordNotFoundException();
        
        user ??= ctx.Member;

        if (user is not DiscordMember member)
            throw new InvalidOperationException("Couldn't retrieve discord member");

        var embed = new DiscordEmbedBuilder();
        embed.WithColor(new DiscordColor(guild.EmbedHexColor));
        embed.WithImageUrl(member.GetGuildAvatarUrl(ImageFormat.Auto));
        embed.AddField("Default avatar URL", member.GetAvatarUrl(ImageFormat.Auto));
        embed.AddField("Guild avatar URL", member.GetGuildAvatarUrl(ImageFormat.Auto));
        embed.WithTitle($"Avatar response | {member.GetFullUsername()}");

        await ctx.EditResponseAsync(
            new DiscordWebhookBuilder().AddEmbed(embed));
    }
}
