using System.Collections.Generic;
using System.Globalization;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.Extensions;
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
    private readonly IGuildDataService _guildDataService;

    public ModUtilSlashCommands(IGuildDataService guildDataService)
    {
        _guildDataService = guildDataService;

    }
    [SlashRequireUserPermissions(Permissions.BanMembers)]
    [SlashCommand("identity", "Command that allows checking information about a member.", false)]
    [UsedImplicitly]
    public async Task IdentityCommand(InteractionContext ctx,
        [Option("user", "User to identify")] DiscordUser user)
    {
        if (user is null) throw new ArgumentNullException(nameof(user));
        await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource,
            new DiscordInteractionResponseBuilder().AsEphemeral());

        var res = await _guildDataService!.GetSingleBySpecAsync<Guild>(
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
            .AsEphemeral());
    }

    [SlashRequireUserPermissions(Permissions.BanMembers)]
    [SlashCommand("member", "Command that allows checking information about members.", false)]
    [UsedImplicitly]
    public async Task MemberCommand(InteractionContext ctx,
        [Option("action", "Action type")] MemberActionType actionType,
        [Option("user", "User to check if any")]
        DiscordUser? user = null)
    {
        _ = ctx.DeferAsync();

        var res = await _guildDataService.GetSingleBySpecAsync(
            new ActiveGuildByDiscordIdWithMembersEntriesSpec(ctx.Guild.Id));

        if (!res.IsDefined(out var guild)) 
            throw new ArgumentException("Guild not found in database");
        
        switch (actionType)
        {
            case MemberActionType.Check:
                var member = (DiscordMember?)user;
                if (member is null)
                    throw new ArgumentNullException(nameof(user), "You must supply a user to use this option.");

                var embed = new DiscordEmbedBuilder();
                embed.WithThumbnail(member.AvatarUrl);
                embed.WithTitle("Member information");
                embed.AddField("Member's identity", $"{member.GetFullUsername()}", true);
                embed.AddField("Joined guild", $"{member.JoinedAt.ToString(CultureInfo.CurrentCulture)}");
                embed.AddField("Account created", $"{member.CreationTimestamp.ToString(CultureInfo.CurrentCulture)}");
                embed.WithColor(new DiscordColor(res.Entity.EmbedHexColor));
                embed.WithFooter($"Member Id: {member.Id}");

                await ctx.Interaction.CreateFollowupMessageAsync(new DiscordFollowupMessageBuilder().AddEmbed(embed.Build())
                    .AsEphemeral());
                break;
            case MemberActionType.History:
                var intr = ctx.Client.GetInteractivity();
                var pages = new List<Page>();
                var chunked = (guild.MemberHistoryEntries?.Where(x => x.GuildId == ctx.Guild.Id) ?? throw new ArgumentNullException())
                    .GroupBy(x => new { x.UserId, x.GuildId })
                    .Select(x => x.OrderByDescending(y => y.CreatedAt!.Value).First())
                    .OrderBy(x => x.CreatedAt!.Value)
                    .Take(1000)
                    .Chunk(25)
                    .OrderByDescending(x => x.Length)
                    .ToList();

                int pageNumber = 1;

                foreach (var chunk in chunked)
                {
                    var embedBuilder = new DiscordEmbedBuilder().WithColor(new DiscordColor(guild.EmbedHexColor))
                        .WithTitle("Member history")
                        .WithFooter($"Current page: {pageNumber} | Total pages: {chunked.Count}");

                    foreach (var entry in chunk)
                    {
                        embedBuilder.AddField(entry.Username,
                            $"Joined at: {entry.CreatedAt!.Value.ToString("g")} UTC {(entry.IsDisabled ? $"\nLeft at: {entry.UpdatedAt!.Value.ToString("g")} UTC" : $"Member currently for: {Math.Round(DateTime.UtcNow.Subtract(entry.CreatedAt!.Value.ToUniversalTime()).TotalDays, 2).ToString(CultureInfo.InvariantCulture)} days\nAccount created at: {entry.AccountCreated.ToString("g")} UTC")}");
       
                        await Task.Delay(500);
                    }

                    pages.Add(new Page("", embedBuilder));
                    pageNumber++;
                }
                
                
                if (pages.Any())
                    await intr.SendPaginatedResponseAsync(ctx.Interaction, false, ctx.User, pages, null,
                        null, null, default, true);
                else
                    await ctx.FollowUpAsync(new DiscordFollowupMessageBuilder().AddEmbed(new DiscordEmbedBuilder()
                        .WithTitle("Member history").WithDescription("No history entries available.").WithColor(new DiscordColor(guild.EmbedHexColor))));
                break;
            case MemberActionType.Backtrack:
                var members = await ctx.Guild.GetAllMembersAsync();

                foreach (var memberBacktrack in members)
                {
                    var dateBacktrack = memberBacktrack.JoinedAt != DateTimeOffset.MinValue
                        ? memberBacktrack.JoinedAt.UtcDateTime
                        : DateTime.UtcNow;

                    _ = _guildDataService.BeginUpdate(guild);
                    guild.AddMemberHistoryEntry(memberBacktrack.Id, memberBacktrack.GetFullUsername(), memberBacktrack.CreationTimestamp.UtcDateTime, dateBacktrack);
                }

                _ = await _guildDataService.CommitAsync();

                _ = await ctx.Interaction.CreateFollowupMessageAsync(new DiscordFollowupMessageBuilder().AddEmbed(
                    new DiscordEmbedBuilder().WithDescription("Backtracking members has finished successfully!")
                        .WithColor(new DiscordColor(guild.EmbedHexColor))));
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(actionType), actionType, null);
        }
    }
    

    [SlashRequireUserPermissions(Permissions.BanMembers)]
    [SlashCommand("booster", "Command that allows checking information about boosters.", false)]
    [UsedImplicitly]
    public async Task BoosterCommand(InteractionContext ctx,
        [Option("action", "Action type")] BoosterActionType actionType,
        [Option("user", "User to check if any")] DiscordUser? user = null)
    {
        _ = ctx.DeferAsync();

        var res = await _guildDataService.GetSingleBySpecAsync(
            new ActiveGuildByDiscordIdWithBoostersEntriesSpec(ctx.Guild.Id));

        if (!res.IsDefined(out var guild)) 
            throw new ArgumentException("Guild not found in database");

        var member = (DiscordMember?)user;

        switch (actionType)
        {
            case BoosterActionType.Check:
                if (member is null)
                    throw new ArgumentException("User must be supplied if using Check action");
                var embed = new DiscordEmbedBuilder();

                embed.WithTitle("Booster information");
                embed.WithThumbnail(member.AvatarUrl);
                embed.WithColor(new DiscordColor(guild.EmbedHexColor));
                var isBoosting = member.Roles.Any(x => x.Tags.IsPremiumSubscriber);
                var dbBooster =
                    (guild.ServerBoosterHistoryEntries?.Where(x => x.GuildId == ctx.Guild.Id && x.UserId == member.Id))
                    ?.MaxBy(x => x.CreatedAt!.Value);

                var date = dbBooster?.CreatedAt;
                var discordDate = member.PremiumSince;
                
                var daysBoostedTotallyCheck = guild.ServerBoosterHistoryEntries?.Where(x => x.UserId == member.Id && x.GuildId == ctx.Guild.Id).Sum(x =>
                    x.IsDisabled
                        ? x.UpdatedAt!.Value.Subtract(x.CreatedAt!.Value).TotalDays
                        : DateTime.UtcNow.Subtract(x.CreatedAt!.Value).TotalDays);
                
                embed.AddField("Currently boosting", isBoosting ? "Yes" : "No", true);
                
                switch (isBoosting)
                {
                    case false:
                        embed.AddField("Boosted previously", dbBooster is not null ? "Yes" : "Unknown", true);
                        break;
                    case true:
                        embed.AddField("Boosting since", date.HasValue ? $"{date.Value.ToString("g")} UTC" : "Unknown");
                        embed.AddField("Boosting since according to Discord's data (untrusted)",
                            discordDate.HasValue
                                ? $"{discordDate.Value.ToUniversalTime().DateTime.ToString("g")} UTC"
                                : "Unknown");
                        embed.AddField("Boosting currently for", date.HasValue
                            ? $"{Math.Round(DateTime.UtcNow.Subtract(date.Value).TotalDays, 2).ToString(CultureInfo.InvariantCulture)} days"
                            : "Unknown");
                        embed.AddField("Boosting currently for according to Discord's data (untrusted)", discordDate.HasValue
                            ? $"{Math.Round(DateTime.UtcNow.Subtract(discordDate.Value.ToUniversalTime().DateTime).TotalDays, 2).ToString(CultureInfo.InvariantCulture)} days"
                            : "Unknown");
                        break;
                }

                if (daysBoostedTotallyCheck is not null && dbBooster is not null)
                    embed.AddField("Boosted totally for", $"{Math.Round(daysBoostedTotallyCheck.Value, 2).ToString(CultureInfo.InvariantCulture)} days");
                
                await ctx.Interaction.CreateFollowupMessageAsync(new DiscordFollowupMessageBuilder().AddEmbed(embed.Build()));
                break;
            case BoosterActionType.History:
                var intr = ctx.Client.GetInteractivity();
                var pages = new List<Page>();
                var chunked = (guild.ServerBoosterHistoryEntries?.Where(x => x.GuildId == ctx.Guild.Id) ?? throw new ArgumentNullException())
                    .GroupBy(x => new { x.UserId, x.GuildId })
                    .Select(x => x.OrderByDescending(y => y.CreatedAt!.Value).First())
                    .OrderBy(x => x.CreatedAt!.Value)
                    .Take(1000)
                    .Chunk(25)
                    .OrderByDescending(x => x.Length)
                    .ToList();

                int pageNumber = 1;

                foreach (var chunk in chunked)
                {
                    var embedBuilder = new DiscordEmbedBuilder().WithColor(new DiscordColor(guild.EmbedHexColor))
                        .WithTitle("Booster history")
                        .WithFooter($"Current page: {pageNumber} | Total pages: {chunked.Count}");

                    foreach (var booster in chunk)
                    {
                        var daysBoostedTotally = guild.ServerBoosterHistoryEntries?.Where(x => x.UserId == booster.UserId && x.GuildId == ctx.Guild.Id).Sum(x =>
                            x.IsDisabled
                                ? x.UpdatedAt!.Value.Subtract(x.CreatedAt!.Value).TotalDays
                                : DateTime.UtcNow.Subtract(x.CreatedAt!.Value).TotalDays);
                        
                        embedBuilder.AddField(booster.Username,
                            $"Is currently boosting: {!booster.IsDisabled}\nLast boost date: {booster.CreatedAt!.Value.ToString("g")} UTC{(booster.IsDisabled ? $"\nBoosted until: {booster.UpdatedAt!.Value.ToString("g")} UTC" : $"\nBoosting currently for: {Math.Round(DateTime.UtcNow.Subtract(booster.CreatedAt!.Value.ToUniversalTime()).TotalDays, 2).ToString(CultureInfo.InvariantCulture)} days")}\nBoosted totally for: {Math.Round(daysBoostedTotally ?? 0, 2)} days");
       
                        await Task.Delay(500);
                    }

                    pages.Add(new Page("", embedBuilder));
                    pageNumber++;
                }
                
                
                if (pages.Any())
                    await intr.SendPaginatedResponseAsync(ctx.Interaction, false, ctx.User, pages, null,
                        null, null, default, true);
                else
                    await ctx.FollowUpAsync(new DiscordFollowupMessageBuilder().AddEmbed(new DiscordEmbedBuilder()
                        .WithTitle("Booster history").WithDescription("No history entries available.").WithColor(new DiscordColor(guild.EmbedHexColor))));
                break;
            case BoosterActionType.Active:
                var intrActive = ctx.Client.GetInteractivity();
                var pagesActive = new List<Page>();
                var chunkedActive = (guild.ServerBoosterHistoryEntries?.Where(x => !x.IsDisabled) ?? throw new ArgumentNullException()).Where(x => !x.IsDisabled)
                    .OrderBy(x => x.CreatedAt!.Value)
                    .Take(1000)
                    .Chunk(25)
                    .OrderByDescending(x => x.Length)
                    .ToList();

                int pageNumberActive = 1;
                
                foreach (var chunk in chunkedActive)
                {
                    var embedBuilderActive = new DiscordEmbedBuilder().WithColor(new DiscordColor(guild.EmbedHexColor))
                        .WithTitle("Boosters")
                        .WithFooter($"Current page: {pageNumberActive} | Total pages: {chunkedActive.Count}");

                    foreach (var booster in chunk)
                    {
                        embedBuilderActive.AddField(booster.Username,
                            $"Last boost date: {booster.CreatedAt!.Value.ToString("g")} UTC\nBoosting for: {Math.Round(DateTime.UtcNow.Subtract(booster.CreatedAt!.Value.ToUniversalTime()).TotalDays, 2).ToString(CultureInfo.InvariantCulture)} days");
                        await Task.Delay(500);
                    }

                    pagesActive.Add(new Page("", embedBuilderActive));
                    pageNumberActive++;
                }

                if (pagesActive.Any())
                    await intrActive.SendPaginatedResponseAsync(ctx.Interaction, false, ctx.User, pagesActive, null,
                        null, null, default, true);
                else
                    _ = await ctx.FollowUpAsync(new DiscordFollowupMessageBuilder().AddEmbed(new DiscordEmbedBuilder()
                        .WithTitle("Boosters").WithDescription("No active boosters found.").WithColor(new DiscordColor(guild.EmbedHexColor))));
                break;
            case BoosterActionType.ActiveByDiscord:
                var intrActiveDisc = ctx.Client.GetInteractivity();
                var pagesActiveDisc = new List<Page>();
                var membersActiveDisc = await ctx.Guild.GetAllMembersAsync();
                var chunkedActiveDisc = membersActiveDisc.Where(x => x.Roles.Any(y => y.Tags.IsPremiumSubscriber))
                    .OrderBy(x => x.PremiumSince!.Value)
                    .Take(1000)
                    .Chunk(25)
                    .OrderByDescending(x => x.Length)
                    .ToList();
                
                int pageNumberActiveDisc = 1;
                
                foreach (var chunk in chunkedActiveDisc)
                {
                    var embedBuilderActiveDisc = new DiscordEmbedBuilder().WithColor(new DiscordColor(guild.EmbedHexColor))
                        .WithTitle("Boosters")
                        .WithFooter($"Current page: {pageNumberActiveDisc} | Total pages: {chunkedActiveDisc.Count}");

                    foreach (var booster in chunk)
                    {
                        embedBuilderActiveDisc.AddField(booster.GetFullUsername(),
                            $"Last boost date: {booster.PremiumSince!.Value.UtcDateTime.ToString("g")} UTC\nBoosting for: {Math.Round(DateTime.UtcNow.Subtract(booster.PremiumSince!.Value.UtcDateTime).TotalDays, 2).ToString(CultureInfo.InvariantCulture)} days");
                    }

                    pagesActiveDisc.Add(new Page("", embedBuilderActiveDisc));
                    pageNumberActiveDisc++;
                }
                
                if (pagesActiveDisc.Any())
                    await intrActiveDisc.SendPaginatedResponseAsync(ctx.Interaction, false, ctx.User, pagesActiveDisc, null,
                      null, null, default, true);
                else
                    _ = await ctx.FollowUpAsync(new DiscordFollowupMessageBuilder().AddEmbed(new DiscordEmbedBuilder()
                        .WithTitle("Boosters").WithDescription("No active boosters found.").WithColor(new DiscordColor(guild.EmbedHexColor))));
                break;
            case BoosterActionType.Backtrack:
                var members = await ctx.Guild.GetAllMembersAsync();
                var boosters = members.Where(x => x.Roles.Any(y => y.Tags.IsPremiumSubscriber)).ToList();
                
                foreach (var memberBacktrack in boosters)
                {
                    var dateBacktrack = memberBacktrack.PremiumSince is not null &&
                                        memberBacktrack.PremiumSince != DateTimeOffset.MinValue
                        ? memberBacktrack.PremiumSince.Value.UtcDateTime
                        : DateTime.UtcNow;

                    _ = _guildDataService.BeginUpdate(guild);
                    guild.AddServerBoosterHistoryEntry(memberBacktrack.Id, memberBacktrack.GetFullUsername(), dateBacktrack);
                }

                _ = await _guildDataService.CommitAsync();

                _ = await ctx.Interaction.CreateFollowupMessageAsync(new DiscordFollowupMessageBuilder().AddEmbed(
                    new DiscordEmbedBuilder().WithDescription("Backtracking server boosters has finished successfully!}")
                        .WithColor(new DiscordColor(guild.EmbedHexColor))));
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(actionType), actionType, null);
        }
    }
}
