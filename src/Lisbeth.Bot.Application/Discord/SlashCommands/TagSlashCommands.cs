// This file is part of Lisbeth.Bot project
//
// Copyright (C) 2021 Krzysztof Kupisz - MikyM
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU Affero General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Affero General Public License for more details.
//
// You should have received a copy of the GNU Affero General Public License
// along with this program.  If not, see <https://www.gnu.org/licenses/>.

using System.Threading;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity.Enums;
using DSharpPlus.Interactivity.EventHandling;
using DSharpPlus.Interactivity.Extensions;
using DSharpPlus.SlashCommands;
using FluentValidation;
using Lisbeth.Bot.Application.Discord.CommandHandlers.Tag;
using Lisbeth.Bot.Application.Discord.Commands.Tag;
using Lisbeth.Bot.Application.Discord.Extensions;
using Lisbeth.Bot.Application.Discord.SlashCommands.Base;
using Lisbeth.Bot.Application.Validation.Tag;
using Lisbeth.Bot.Domain.DTOs.Request.Tag;
using MikyM.Common.Application.CommandHandlers;

namespace Lisbeth.Bot.Application.Discord.SlashCommands;

[UsedImplicitly]
[SlashModuleLifespan(SlashModuleLifespan.Scoped)]
public class TagSlashCommands : ExtendedApplicationCommandModule
{
    public TagSlashCommands(IDiscordEmbedConfiguratorService<Tag> discordEmbedTagConfiguratorService,
        ICommandHandlerUnitOfWorkManager commandHandlerUnitOfWorkManager)
    {
        _discordEmbedTagConfiguratorService = discordEmbedTagConfiguratorService;
        _commandHandlerUnitOfWorkManager = commandHandlerUnitOfWorkManager;
    }

    private readonly IDiscordEmbedConfiguratorService<Tag> _discordEmbedTagConfiguratorService;
    private readonly ICommandHandlerUnitOfWorkManager _commandHandlerUnitOfWorkManager;

    [SlashCooldown(20, 120, CooldownBucketType.Guild)]
    [UsedImplicitly]
    [SlashCommand("tag", "A command that allows working with tags")]
    public async Task TagCommand(InteractionContext ctx,
        [Option("action", "Type of action to perform")]
        TagActionType action,
        [Option("name", "Name of the tag")]
        string idOrName = "",
        [Option("snowflake", "Channel to send the tag to or a role/member to add/revoke permissions for")]
        SnowflakeObject? snowflake = null,
        [Option("text", "Base text for the tag")]
        string text = "")
    {
        await ctx.DeferAsync(true);

        switch (action)
        {
            case TagActionType.Get:
                var getReq = new TagGetReqDto
                {
                    GuildId = ctx.Guild.Id, Name = idOrName, RequestedOnBehalfOfId = ctx.User.Id
                };

                var getValidator = new TagGetReqValidator(ctx.Client);
                await getValidator.ValidateAndThrowAsync(getReq);

                var getRes = await _commandHandlerUnitOfWorkManager.GetHandler<GetTagCommandHandler>()
                    .HandleAsync(new GetTagCommand(getReq, ctx));
                
                /*
                if (getRes.IsDefined(out var getBuilder))
                    await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent(getBuilder.Content)
                        .AddEmbeds(getBuilder.Embeds));
                else
                    await ctx.FollowUpAsync(new DiscordFollowupMessageBuilder()
                        .AddEmbed(base.GetUnsuccessfulResultEmbed(getRes, ctx.Client))
                        .AsEphemeral(true));*/
                if (getRes.IsDefined(out var getBuilder))
                    await ctx.ExtendedFollowUpAsync(new DiscordFollowupMessageBuilder().WithContent(getBuilder.Content)
                        .AddEmbeds(getBuilder.Embeds));
                else
                    await ctx.ExtendedFollowUpAsync(getRes);

                break;
            case TagActionType.Create:
                var addReq = new TagAddReqDto
                {
                    GuildId = ctx.Guild.Id, Name = idOrName, RequestedOnBehalfOfId = ctx.User.Id, Text = text
                };

                var addValidator = new TagAddReqValidator(ctx.Client);
                await addValidator.ValidateAndThrowAsync(addReq);

                var createRes = await _commandHandlerUnitOfWorkManager.GetHandler<CreateTagCommandHandler>()
                    .HandleAsync(new CreateTagCommand(addReq, ctx));

                if (createRes.IsSuccess)
                    await ctx.EditResponseAsync(
                        new DiscordWebhookBuilder().AddEmbed(base.GetSuccessfulActionEmbed(ctx.Client)));
                else
                    await ctx.EditResponseAsync(
                        new DiscordWebhookBuilder().AddEmbed(base.GetUnsuccessfulResultEmbed(createRes, ctx.Client)));
                break;
            case TagActionType.Edit:
                var editReq = new TagEditReqDto
                {
                    GuildId = ctx.Guild.Id, Name = idOrName, RequestedOnBehalfOfId = ctx.User.Id, Text = text
                };

                var editValidator = new TagEditReqValidator(ctx.Client);
                await editValidator.ValidateAndThrowAsync(editReq);

                var editRes = await _commandHandlerUnitOfWorkManager.GetHandler<EditTagCommandHandler>()
                    .HandleAsync(new EditTagCommand(editReq, ctx));

                if (editRes.IsSuccess)
                    await ctx.EditResponseAsync(
                        new DiscordWebhookBuilder().AddEmbed(base.GetSuccessfulActionEmbed(ctx.Client)));
                else
                    await ctx.EditResponseAsync(
                        new DiscordWebhookBuilder().AddEmbed(base.GetUnsuccessfulResultEmbed(editRes, ctx.Client)));
                break;
            case TagActionType.Disable:
                var removeReq = new TagDisableReqDto
                {
                    GuildId = ctx.Guild.Id, Name = idOrName, RequestedOnBehalfOfId = ctx.User.Id
                };

                var disableValidator = new TagDisableReqValidator(ctx.Client);
                await disableValidator.ValidateAndThrowAsync(removeReq);

                var disableRes = await _commandHandlerUnitOfWorkManager.GetHandler<DisableTagCommandHandler>()
                    .HandleAsync(new DisableTagCommand(removeReq, ctx));

                if (disableRes.IsSuccess)
                    await ctx.EditResponseAsync(
                        new DiscordWebhookBuilder().AddEmbed(base.GetSuccessfulActionEmbed(ctx.Client)));
                else
                    await ctx.EditResponseAsync(
                        new DiscordWebhookBuilder().AddEmbed(base.GetUnsuccessfulResultEmbed(disableRes, ctx.Client)));
                break;
            case TagActionType.ConfigureEmbed:
                var configRes =
                    await _discordEmbedTagConfiguratorService.ConfigureAsync(ctx, x => x.EmbedConfig,
                        x => x.EmbedConfigId, idOrName);

                if (configRes.IsDefined(out var embed))
                    await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(embed));
                else
                    await ctx.EditResponseAsync(
                        new DiscordWebhookBuilder().AddEmbed(base.GetUnsuccessfulResultEmbed(configRes, ctx.Client)));
                break;
            case TagActionType.Send:
                var sendReq = new TagSendReqDto
                {
                    GuildId = ctx.Guild.Id,
                    Name = idOrName,
                    RequestedOnBehalfOfId = ctx.User.Id,
                    ChannelId = snowflake?.Id
                };

                var sendValidator = new TagSendReqValidator(ctx.Client);
                await sendValidator.ValidateAndThrowAsync(sendReq);

                var sendRes = await _commandHandlerUnitOfWorkManager.GetHandler<SendTagCommandHandler>()
                    .HandleAsync(new SendTagCommand(sendReq, ctx));

                if (sendRes.IsSuccess)
                    await ctx.EditResponseAsync(
                        new DiscordWebhookBuilder().AddEmbed(base.GetSuccessfulActionEmbed(ctx.Client)));
                else
                    await ctx.EditResponseAsync(
                        new DiscordWebhookBuilder().AddEmbed(base.GetUnsuccessfulResultEmbed(sendRes, ctx.Client)));
                break;
            case TagActionType.AddPermissionFor:
                var addPermReq = new TagAddSnowflakePermissionReqDto
                {
                    GuildId = ctx.Guild.Id,
                    Name = idOrName,
                    RequestedOnBehalfOfId = ctx.User.Id,
                    SnowflakeId = snowflake?.Id
                };

                var addPermValidator = new TagAddPermissionReqValidator(ctx.Client);
                await addPermValidator.ValidateAndThrowAsync(addPermReq);

                var addPermRes = await _commandHandlerUnitOfWorkManager.GetHandler<AddSnowflakePermissionTagCommandHandler>()
                    .HandleAsync(new AddSnowflakePermissionTagCommand(addPermReq, ctx));

                if (addPermRes.IsSuccess)
                    await ctx.EditResponseAsync(
                        new DiscordWebhookBuilder().AddEmbed(base.GetSuccessfulActionEmbed(ctx.Client)));
                else
                    await ctx.EditResponseAsync(
                        new DiscordWebhookBuilder().AddEmbed(base.GetUnsuccessfulResultEmbed(addPermRes, ctx.Client)));
                break;
            case TagActionType.RevokePermissionFor:
                var revokePermReq = new TagRevokeSnowflakePermissionReqDto
                {
                    GuildId = ctx.Guild.Id,
                    Name = idOrName,
                    RequestedOnBehalfOfId = ctx.User.Id,
                    SnowflakeId = snowflake?.Id
                };

                var revokePermValidator = new TagRevokePermissionReqValidator(ctx.Client);
                await revokePermValidator.ValidateAndThrowAsync(revokePermReq);

                var revokePermRes = await _commandHandlerUnitOfWorkManager
                    .GetHandler<RevokeSnowflakePermissionTagCommandHandler>()
                    .HandleAsync(new RevokeSnowflakePermissionTagCommand(revokePermReq, ctx));

                if (revokePermRes.IsSuccess)
                    await ctx.EditResponseAsync(
                        new DiscordWebhookBuilder().AddEmbed(base.GetSuccessfulActionEmbed(ctx.Client)));
                else
                    await ctx.EditResponseAsync(
                        new DiscordWebhookBuilder().AddEmbed(
                            base.GetUnsuccessfulResultEmbed(revokePermRes, ctx.Client)));
                break;
            case TagActionType.List:
                var getAllReq = new TagGetAllReqDto() { GuildId = ctx.Guild.Id, RequestedOnBehalfOfId = ctx.User.Id };

                var getAllValidator = new TagGetAllReqValidator(ctx.Client);
                await getAllValidator.ValidateAndThrowAsync(getAllReq);

                /*var getAllRes = await _getAllHandler.HandleAsync(new GetAllTagsCommand(getAllReq, ctx));*/
                var getAllRes = await _commandHandlerUnitOfWorkManager
                    .GetHandler<GetAllTagsCommandHandler>()
                    .HandleAsync(new GetAllTagsCommand(getAllReq, ctx));

                var paginationButtons = new PaginationButtons
                {
                    Left = new DiscordButtonComponent(ButtonStyle.Primary, "left_pagination", null, false,
                        new DiscordComponentEmoji(DiscordEmoji.FromName(ctx.Client, ":arrow_left:"))),
                    Right = new DiscordButtonComponent(ButtonStyle.Primary, "right_pagination", null, false,
                        new DiscordComponentEmoji(DiscordEmoji.FromName(ctx.Client, ":arrow_right:"))),
                    SkipRight = new DiscordButtonComponent(ButtonStyle.Primary, "skip_right_pagination",
                        null, false, new DiscordComponentEmoji(DiscordEmoji.FromName(ctx.Client, ":track_next:"))),
                    SkipLeft = new DiscordButtonComponent(ButtonStyle.Primary, "skip_left_pagination", null,
                        false, new DiscordComponentEmoji(DiscordEmoji.FromName(ctx.Client, ":track_previous:"))),
                    Stop = new DiscordButtonComponent(ButtonStyle.Primary, "stop_pagination", null, false,
                        new DiscordComponentEmoji(DiscordEmoji.FromName(ctx.Client, ":stop_button:")))
                };

                var tcs = new CancellationTokenSource();

                if (getAllRes.IsDefined(out var pages))
                {
                    var intr = ctx.Client.GetInteractivity();
                    await intr.SendPaginatedResponseAsync(ctx.Interaction, false, ctx.User, pages, paginationButtons,
                        PaginationBehaviour.WrapAround, ButtonPaginationBehavior.Disable, tcs.Token, true);
                }
                else
                    await ctx.EditResponseAsync(
                        new DiscordWebhookBuilder().AddEmbed(base.GetUnsuccessfulResultEmbed(getAllRes, ctx.Client)));

                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(action), action, null);
        }
    }
}
