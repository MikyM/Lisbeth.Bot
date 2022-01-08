using System.Globalization;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using DSharpPlus.SlashCommands.Attributes;
using Lisbeth.Bot.Application.Discord.SlashCommands.Base;
using Lisbeth.Bot.DataAccessLayer.Specifications.Guild;
using MikyM.Discord.Extensions.BaseExtensions;

namespace Lisbeth.Bot.Application.Discord.SlashCommands;


[UsedImplicitly]
[SlashModuleLifespan(SlashModuleLifespan.Scoped)]
public class ModUtilSlashCommands : ExtendedApplicationCommandModule
{
    private readonly IGuildDataDataService _guildDataDataService;

    public ModUtilSlashCommands(IGuildDataDataService guildDataDataService)
    {
        _guildDataDataService = guildDataDataService;

    }
    [SlashRequireUserPermissions(Permissions.BanMembers)]
    [SlashCommand("identity", "A command that allows checking information about a member.", false)]
    [UsedImplicitly]
    public async Task IdentityCommand(InteractionContext ctx,
        [Option("user", "User to identify")] DiscordUser user)
    {
        if (user is null) throw new ArgumentNullException(nameof(user));
        await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource,
            new DiscordInteractionResponseBuilder().AsEphemeral(true));

        var res = await this._guildDataDataService!.GetSingleBySpecAsync<Guild>(
            new ActiveGuildByDiscordIdWithTicketingSpecifications(ctx.Guild.Id));

        if (!res.IsDefined()) throw new ArgumentException("Guild not found in database");

        var member = (DiscordMember)user;

        var embed = new DiscordEmbedBuilder();
        embed.WithThumbnail(member.AvatarUrl);
        embed.WithTitle("Member information");
        embed.AddField("Member's identity", $"{user.GetFullUsername()}", true);
        embed.AddField("Joined guild", $"{member.JoinedAt.ToString(CultureInfo.CurrentCulture)}");
        embed.AddField("Account created", $"{member.CreationTimestamp.ToString(CultureInfo.CurrentCulture)}");
        embed.WithColor(new DiscordColor(res.Entity.EmbedHexColor));
        embed.WithFooter($"Member Id: {member.Id}");

        await ctx.Interaction.CreateFollowupMessageAsync(new DiscordFollowupMessageBuilder().AddEmbed(embed.Build())
            .AsEphemeral(true));
    }
}
