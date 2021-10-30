using System;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using DSharpPlus.SlashCommands.Attributes;
using JetBrains.Annotations;
using Lisbeth.Bot.Application.Discord.Extensions;
using Lisbeth.Bot.Application.Discord.Services.Interfaces;
using Lisbeth.Bot.Application.Services.Database.Interfaces;
using Lisbeth.Bot.DataAccessLayer.Specifications.Guild;
using Lisbeth.Bot.Domain.Entities;

namespace Lisbeth.Bot.Application.Discord.SlashCommands
{
    [SlashCommandGroup("mod", "Moderation commands")]
    [SlashModuleLifespan(SlashModuleLifespan.Transient)]
    public class ModerationUtilSlashCommands : ApplicationCommandModule
    {
        public IGuildService _guildService { private get; set; }
        public IDiscordTicketService _dicordTicketService { private get; set; }

        [SlashRequireUserPermissions(Permissions.Administrator)]
        [SlashCommand("identity", "A command that allows checking information about a member.")]
        [UsedImplicitly]
        public async Task IdentityCommand(InteractionContext ctx,
            [Option("user", "User to identify")] DiscordUser user)
        {
            if (user is null) throw new ArgumentNullException(nameof(user));
            await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource,
                new DiscordInteractionResponseBuilder().AsEphemeral(true));

            var res = await _guildService.GetBySpecAsync<Guild>(
                new ActiveGuildByDiscordIdWithTicketingSpecifications(ctx.Guild.Id));
            var guild = res.FirstOrDefault();

            if (guild is null) throw new ArgumentException("Guild not found in database");

            var member = (DiscordMember) user;

            var embed = new DiscordEmbedBuilder();
            embed.WithThumbnail(member.AvatarUrl);
            embed.WithTitle("Member information");
            embed.AddField("Member's identity", $"{user.GetFullUsername()}", true);
            embed.AddField("Joined guild", $"{member.JoinedAt}");
            embed.AddField("Account created", $"{member.CreationTimestamp}");
            embed.WithColor(new DiscordColor(guild.EmbedHexColor));
            embed.WithFooter($"Member Id: {member.Id}");

            await ctx.Interaction.CreateFollowupMessageAsync(new DiscordFollowupMessageBuilder().AddEmbed(embed.Build())
                .AsEphemeral(true));
        }


        [UsedImplicitly]
        [SlashRequireUserPermissions(Permissions.Administrator)]
        [SlashCommand("ticket-center", "A command that allows creating a ticket center message")]
        public async Task TicketCenterCommand(InteractionContext ctx)
        {
            await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource,
                new DiscordInteractionResponseBuilder().AsEphemeral(true));

            var builder = await _dicordTicketService.GetTicketCenterEmbedAsync(ctx);

            await ctx.Channel.SendMessageAsync(builder);
            await ctx.Interaction.CreateFollowupMessageAsync(new DiscordFollowupMessageBuilder()
                .WithContent("Message sent successfully").AsEphemeral(true));
        }

        [UsedImplicitly]
        [SlashRequireUserPermissions(Permissions.Administrator)]
        [SlashCommand("ticket-config", "A command that allows setting ticketing module up")]
        public async Task TicketConfigCommand(InteractionContext ctx, [Option("action", "Action to perform")] ModerationActionType action)
        {
            await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource,
                new DiscordInteractionResponseBuilder().AsEphemeral(true));

            DiscordEmbed embed;
            switch (action)
            {
                case ModerationActionType.Enable:
                    embed = await _guildService.AddConfigAsync(addReq, true);
                    break;
                case ModerationActionType.Repair:
                    break;
                case ModerationActionType.Edit:
                    break;
                case ModerationActionType.Disable:
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(action), action, null);
            }


            await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent("Done"));
        }

        [UsedImplicitly]
        [SlashRequireUserPermissions(Permissions.Administrator)]
        [SlashCommand("moderation-config", "A command that allows setting moderation module up")]
        public async Task ModConfigCommand(InteractionContext ctx,
            [Option("deleted", "Channel for message deletion logs")]
            DiscordChannel deletedChannel, [Option("updated", "Channel for message update logs")]
            DiscordChannel updatedChannel, [Option("mute", "Mute role Id")] string muteRoleId)
        {
            await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource,
                new DiscordInteractionResponseBuilder().AsEphemeral(true));

            var res = await _guildService.GetBySpecAsync<Guild>(
                new ActiveGuildByDiscordIdWithTicketingSpecifications(ctx.Guild.Id));
            var guild = res.FirstOrDefault();

            if (guild is null) throw new ArgumentException("Guild not found in database");

            var modConfig = new ModerationConfig
            {
                MuteRoleId = ulong.Parse(muteRoleId), MessageDeletedEventsLogChannelId = deletedChannel.Id,
                MessageUpdatedEventsLogChannelId = updatedChannel.Id
            };

            _guildService.BeginUpdate(guild);
            guild.SetModerationConfig(modConfig);
            await _guildService.CommitAsync();

            await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent("Done"));
        }

        [UsedImplicitly]
        [SlashRequireUserPermissions(Permissions.Administrator)]
        [SlashCommand("guild-add", "A command that adds current guild to bot's database.")]
        public async Task TestGuild(InteractionContext ctx)
        {
            await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource,
                new DiscordInteractionResponseBuilder().AsEphemeral(true));

            var guild = new Guild {GuildId = ctx.Guild.Id, UserId = ctx.User.Id, IsDisabled = false};

            await _guildService.AddAsync(guild, true);

            await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent("Done"));
        }
    }
}