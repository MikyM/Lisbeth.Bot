// This file is part of Lisbeth.Bot project
//
// Copyright (C) 2021-2022 Krzysztof Kupisz - MikyM
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

using System.Collections.Generic;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.Extensions;
using DSharpPlus.SlashCommands;
using FluentValidation;
using Lisbeth.Bot.Application.Discord.Commands.Tag;
using Lisbeth.Bot.Application.Discord.SlashCommands.Base;
using Lisbeth.Bot.Application.Validation.Tag;
using Lisbeth.Bot.Domain.DTOs.Request.Tag;
using MikyM.CommandHandlers;

namespace Lisbeth.Bot.Application.Discord.SlashCommands;

[UsedImplicitly]
[SlashModuleLifespan(SlashModuleLifespan.Scoped)]
public class TagSlashCommands : ExtendedApplicationCommandModule
{
    public TagSlashCommands(IDiscordEmbedConfiguratorService<Tag> discordEmbedTagConfiguratorService,
        ICommandHandlerFactory commandHandlerFactory)
    {
        _discordEmbedTagConfiguratorService = discordEmbedTagConfiguratorService;
        _commandHandlerFactory = commandHandlerFactory;
    }

    private readonly IDiscordEmbedConfiguratorService<Tag> _discordEmbedTagConfiguratorService;
    private readonly ICommandHandlerFactory _commandHandlerFactory;

    [SlashCooldown(20, 120, CooldownBucketType.Guild)]
    [UsedImplicitly]
    [SlashCommand("tag", "Command that allows working with tags")]
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
        await ctx.DeferAsync();

        switch (action)
        {
            case TagActionType.Get:
                var getReq = new TagGetReqDto
                {
                    GuildId = ctx.Guild.Id, Name = idOrName, RequestedOnBehalfOfId = ctx.User.Id
                };

                var getValidator = new TagGetReqValidator(ctx.Client);
                await getValidator.ValidateAndThrowAsync(getReq);

                var getRes = await _commandHandlerFactory.GetHandler<ICommandHandler<GetTagCommand, DiscordMessageBuilder>>()
                    .HandleAsync(new GetTagCommand(getReq, ctx));

                if (getRes.IsDefined(out var getBuilder))
                    await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent(getBuilder.Content)
                        .AddEmbeds(getBuilder.Embeds));
                else
                    await ctx.FollowUpAsync(new DiscordFollowupMessageBuilder()
                        .AddEmbed(GetUnsuccessfulResultEmbed(getRes, ctx.Client))
                        .AsEphemeral());
                break;
            case TagActionType.Create:
                var addReq = new TagAddReqDto
                {
                    GuildId = ctx.Guild.Id, Name = idOrName, RequestedOnBehalfOfId = ctx.User.Id, Text = text
                };

                var addValidator = new TagAddReqValidator(ctx.Client);
                await addValidator.ValidateAndThrowAsync(addReq);

                var createRes = await _commandHandlerFactory.GetHandler<ICommandHandler<CreateTagCommand>>()
                    .HandleAsync(new CreateTagCommand(addReq, ctx));

                if (createRes.IsSuccess)
                    await ctx.EditResponseAsync(
                        new DiscordWebhookBuilder().AddEmbed(GetSuccessfulActionEmbed(ctx.Client)));
                else
                    await ctx.EditResponseAsync(
                        new DiscordWebhookBuilder().AddEmbed(GetUnsuccessfulResultEmbed(createRes, ctx.Client)));
                break;
            case TagActionType.Edit:
                var editReq = new TagEditReqDto
                {
                    GuildId = ctx.Guild.Id, Name = idOrName, RequestedOnBehalfOfId = ctx.User.Id, Text = text
                };

                var editValidator = new TagEditReqValidator(ctx.Client);
                await editValidator.ValidateAndThrowAsync(editReq);

                var editRes = await _commandHandlerFactory.GetHandler<ICommandHandler<EditTagCommand>>()
                    .HandleAsync(new EditTagCommand(editReq, ctx));

                if (editRes.IsSuccess)
                    await ctx.EditResponseAsync(
                        new DiscordWebhookBuilder().AddEmbed(GetSuccessfulActionEmbed(ctx.Client)));
                else
                    await ctx.EditResponseAsync(
                        new DiscordWebhookBuilder().AddEmbed(GetUnsuccessfulResultEmbed(editRes, ctx.Client)));
                break;
            case TagActionType.Disable:
                var removeReq = new TagDisableReqDto
                {
                    GuildId = ctx.Guild.Id, Name = idOrName, RequestedOnBehalfOfId = ctx.User.Id
                };

                var disableValidator = new TagDisableReqValidator(ctx.Client);
                await disableValidator.ValidateAndThrowAsync(removeReq);

                var disableRes = await _commandHandlerFactory.GetHandler<ICommandHandler<DisableTagCommand>>()
                    .HandleAsync(new DisableTagCommand(removeReq, ctx));

                if (disableRes.IsSuccess)
                    await ctx.EditResponseAsync(
                        new DiscordWebhookBuilder().AddEmbed(GetSuccessfulActionEmbed(ctx.Client)));
                else
                    await ctx.EditResponseAsync(
                        new DiscordWebhookBuilder().AddEmbed(GetUnsuccessfulResultEmbed(disableRes, ctx.Client)));
                break;
            case TagActionType.ConfigureEmbed:
                var configRes =
                    await _discordEmbedTagConfiguratorService.ConfigureAsync(ctx, x => x.EmbedConfig,
                        x => x.EmbedConfigId, idOrName);

                if (configRes.IsDefined(out var embed))
                    await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(embed));
                else
                    await ctx.EditResponseAsync(
                        new DiscordWebhookBuilder().AddEmbed(GetUnsuccessfulResultEmbed(configRes, ctx.Client)));
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

                var sendRes = await _commandHandlerFactory.GetHandler<ICommandHandler<SendTagCommand>>()
                    .HandleAsync(new SendTagCommand(sendReq, ctx));

                if (sendRes.IsSuccess)
                    await ctx.EditResponseAsync(
                        new DiscordWebhookBuilder().AddEmbed(GetSuccessfulActionEmbed(ctx.Client)));
                else
                    await ctx.EditResponseAsync(
                        new DiscordWebhookBuilder().AddEmbed(GetUnsuccessfulResultEmbed(sendRes, ctx.Client)));
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

                var addPermRes = await _commandHandlerFactory.GetHandler<ICommandHandler<AddSnowflakePermissionTagCommand>>()
                    .HandleAsync(new AddSnowflakePermissionTagCommand(addPermReq, ctx));

                if (addPermRes.IsSuccess)
                    await ctx.EditResponseAsync(
                        new DiscordWebhookBuilder().AddEmbed(GetSuccessfulActionEmbed(ctx.Client)));
                else
                    await ctx.EditResponseAsync(
                        new DiscordWebhookBuilder().AddEmbed(GetUnsuccessfulResultEmbed(addPermRes, ctx.Client)));
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

                var revokePermRes = await _commandHandlerFactory
                    .GetHandler<ICommandHandler<RevokeSnowflakePermissionTagCommand>>()
                    .HandleAsync(new RevokeSnowflakePermissionTagCommand(revokePermReq, ctx));

                if (revokePermRes.IsSuccess)
                    await ctx.EditResponseAsync(
                        new DiscordWebhookBuilder().AddEmbed(GetSuccessfulActionEmbed(ctx.Client)));
                else
                    await ctx.EditResponseAsync(
                        new DiscordWebhookBuilder().AddEmbed(
                            GetUnsuccessfulResultEmbed(revokePermRes, ctx.Client)));
                break;
            case TagActionType.List:
                var getAllReq = new TagGetAllReqDto { GuildId = ctx.Guild.Id, RequestedOnBehalfOfId = ctx.User.Id };

                var getAllValidator = new TagGetAllReqValidator(ctx.Client);
                await getAllValidator.ValidateAndThrowAsync(getAllReq);

                var getAllRes = await _commandHandlerFactory
                    .GetHandler<ICommandHandler<GetAllTagsCommand, List<Page>>>()
                    .HandleAsync(new GetAllTagsCommand(getAllReq, ctx));

                if (getAllRes.IsDefined(out var pages))
                {
                    var intr = ctx.Client.GetInteractivity();
                    await intr.SendPaginatedResponseAsync(ctx.Interaction, false, ctx.User, pages, null,
                        null, null, default, true);
                }
                else
                    await ctx.EditResponseAsync(
                        new DiscordWebhookBuilder().AddEmbed(GetUnsuccessfulResultEmbed(getAllRes, ctx.Client)));

                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(action), action, null);
        }
    }
}
