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

using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using FluentValidation;
using Lisbeth.Bot.Application.Discord.Commands.Tag;
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
        ICommandHandler<GetTagCommand, DiscordMessageBuilder> getHandler, ICommandHandler<SendTagCommand> sendHandler,
        ICommandHandler<CreateTagCommand> createHandler, ICommandHandler<EditTagCommand> editHandler,
        ICommandHandler<DisableTagCommand> disableHandler, ICommandHandler<AddSnowflakePermissionTagCommand> addPermissionHandler,
        ICommandHandler<RevokeSnowflakePermissionTagCommand> removePermissionHandler)
    {
        _discordEmbedTagConfiguratorService = discordEmbedTagConfiguratorService;
        _getHandler = getHandler;
        _sendHandler = sendHandler;
        _createHandler = createHandler;
        _editHandler = editHandler;
        _disableHandler = disableHandler;
        _addPermissionHandler = addPermissionHandler;
        _removePermissionHandler = removePermissionHandler;
    }

    private readonly IDiscordEmbedConfiguratorService<Tag> _discordEmbedTagConfiguratorService;
    private readonly ICommandHandler<GetTagCommand, DiscordMessageBuilder> _getHandler;
    private readonly ICommandHandler<SendTagCommand> _sendHandler;
    private readonly ICommandHandler<CreateTagCommand> _createHandler;
    private readonly ICommandHandler<EditTagCommand> _editHandler;
    private readonly ICommandHandler<DisableTagCommand> _disableHandler;
    private readonly ICommandHandler<AddSnowflakePermissionTagCommand> _addPermissionHandler;
    private readonly ICommandHandler<RevokeSnowflakePermissionTagCommand> _removePermissionHandler;

    [SlashCooldown(20, 120, CooldownBucketType.Guild)]
    [UsedImplicitly]
    [SlashCommand("tag", "Allows working with tags")]
    public async Task TagCommand(InteractionContext ctx,
        [Option("action", "Type of action to perform")]
        TagActionType action,
        [Option("name", "Name of the tag")]
        string idOrName,
        [Option("snowflake", "Channel to send the tag to or a role/member to add/revoke permissions for")]
        SnowflakeObject? snowflake = null,
        [Option("text", "Base text for the tag")]
        string text = "")
    {
        await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);

        switch (action)
        {
            case TagActionType.Get:
                if (string.IsNullOrWhiteSpace(idOrName))
                    throw new ArgumentException("You must supply a valid name");

                var getReq = new TagGetReqDto
                {
                    GuildId = ctx.Guild.Id,
                    Name = idOrName,
                    RequestedOnBehalfOfId = ctx.User.Id
                };

                var getValidator = new TagGetReqValidator(ctx.Client);
                await getValidator.ValidateAndThrowAsync(getReq);

                var getRes = await _getHandler.HandleAsync(new GetTagCommand(getReq, ctx));

                if (getRes.IsDefined(out var getBuilder))
                    await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent(getBuilder.Content)
                        .AddEmbeds(getBuilder.Embeds));
                else
                    await ctx.EditResponseAsync(
                        new DiscordWebhookBuilder().AddEmbed(base.GetUnsuccessfulResultEmbed(getRes, ctx.Client)));
                break;
            case TagActionType.Create:
                if (string.IsNullOrWhiteSpace(idOrName))
                    throw new ArgumentException("You must supply name.");

                var addReq = new TagAddReqDto
                {
                    GuildId = ctx.Guild.Id,
                    Name = idOrName,
                    RequestedOnBehalfOfId = ctx.User.Id,
                    Text = text
                };

                var addValidator = new TagAddReqValidator(ctx.Client);
                await addValidator.ValidateAndThrowAsync(addReq);

                var createRes = await _createHandler.HandleAsync(new CreateTagCommand(addReq, ctx));

                if (createRes.IsSuccess)
                    await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(base.GetSuccessfulActionEmbed(ctx.Client)));
                else
                    await ctx.EditResponseAsync(
                        new DiscordWebhookBuilder().AddEmbed(base.GetUnsuccessfulResultEmbed(createRes, ctx.Client)));
                break;
            case TagActionType.Edit:
                if (string.IsNullOrWhiteSpace(idOrName))
                    throw new ArgumentException("You must supply a valid Id or name");

                var editReq = new TagEditReqDto
                {
                    GuildId = ctx.Guild.Id,
                    Name = idOrName,
                    RequestedOnBehalfOfId = ctx.User.Id,
                    Text = text
                };

                var editValidator = new TagEditReqValidator(ctx.Client);
                await editValidator.ValidateAndThrowAsync(editReq);

                var editRes = await _editHandler.HandleAsync(new EditTagCommand(editReq, ctx));

                if (editRes.IsSuccess)
                    await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(base.GetSuccessfulActionEmbed(ctx.Client)));
                else
                    await ctx.EditResponseAsync(
                        new DiscordWebhookBuilder().AddEmbed(base.GetUnsuccessfulResultEmbed(editRes, ctx.Client)));
                break;
            case TagActionType.Disable:
                if (string.IsNullOrWhiteSpace(idOrName))
                    throw new ArgumentException("You must supply a valid Id or name");

                var removeReq = new TagDisableReqDto
                {
                    GuildId = ctx.Guild.Id,
                    Name = idOrName,
                    RequestedOnBehalfOfId = ctx.User.Id
                };

                var disableValidator = new TagDisableReqValidator(ctx.Client);
                await disableValidator.ValidateAndThrowAsync(removeReq);

                var disableRes = await _disableHandler.HandleAsync(new DisableTagCommand(removeReq, ctx));

                if (disableRes.IsSuccess)
                    await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(base.GetSuccessfulActionEmbed(ctx.Client)));
                else
                    await ctx.EditResponseAsync(
                        new DiscordWebhookBuilder().AddEmbed(base.GetUnsuccessfulResultEmbed(disableRes, ctx.Client)));
                break;
            case TagActionType.ConfigureEmbed:
                var configRes = await _discordEmbedTagConfiguratorService.ConfigureAsync(ctx, x => x.EmbedConfig, x => x.EmbedConfigId, idOrName);

                if (configRes.IsDefined(out var embed))
                    await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(embed));
                else
                    await ctx.EditResponseAsync(
                        new DiscordWebhookBuilder().AddEmbed(base.GetUnsuccessfulResultEmbed(configRes, ctx.Client)));
                break;
            case TagActionType.Send:
                if (string.IsNullOrWhiteSpace(idOrName))
                    throw new ArgumentException("You must supply a name");
                if (snowflake is null)
                    throw new ArgumentException("You must supply a channel to send a tag");

                var sendReq = new TagSendReqDto
                {
                    GuildId = ctx.Guild.Id,
                    Name = idOrName,
                    RequestedOnBehalfOfId = ctx.User.Id,
                    ChannelId = snowflake.Id
                };

                var sendValidator = new TagSendReqValidator(ctx.Client);
                await sendValidator.ValidateAndThrowAsync(sendReq);

                var sendRes = await _sendHandler.HandleAsync(new SendTagCommand(sendReq, ctx));

                if (sendRes.IsSuccess)
                    await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(base.GetSuccessfulActionEmbed(ctx.Client)));
                else
                    await ctx.EditResponseAsync(
                        new DiscordWebhookBuilder().AddEmbed(base.GetUnsuccessfulResultEmbed(sendRes, ctx.Client)));
                break;
            case TagActionType.AddPermissionFor:
                if (string.IsNullOrWhiteSpace(idOrName))
                    throw new ArgumentException("You must supply name.");
                if (snowflake is null)
                    throw new ArgumentException("You must supply a member or a role to add permissions for.");

                var addPermReq = new TagAddSnowflakePermissionReqDto
                {
                    GuildId = ctx.Guild.Id,
                    Name = idOrName,
                    RequestedOnBehalfOfId = ctx.User.Id,
                    SnowflakeId = snowflake.Id
                };

                var addPermValidator = new TagAddPermissionReqValidator(ctx.Client);
                await addPermValidator.ValidateAndThrowAsync(addPermReq);

                var addPermRes = await _addPermissionHandler.HandleAsync(new AddSnowflakePermissionTagCommand(addPermReq, ctx));

                if (addPermRes.IsSuccess)
                    await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(base.GetSuccessfulActionEmbed(ctx.Client)));
                else
                    await ctx.EditResponseAsync(
                        new DiscordWebhookBuilder().AddEmbed(base.GetUnsuccessfulResultEmbed(addPermRes, ctx.Client)));
                break;
            case TagActionType.RevokePermissionFor:
                if (string.IsNullOrWhiteSpace(idOrName))
                    throw new ArgumentException("You must supply name.");
                if (snowflake is null)
                    throw new ArgumentException("You must supply a member or a role to add permissions for.");

                var revokePermReq = new TagRevokeSnowflakePermissionReqDto
                {
                    GuildId = ctx.Guild.Id,
                    Name = idOrName,
                    RequestedOnBehalfOfId = ctx.User.Id,
                    SnowflakeId = snowflake.Id
                };

                var revokePermValidator = new TagRevokePermissionReqValidator(ctx.Client);
                await revokePermValidator.ValidateAndThrowAsync(revokePermReq);

                var revokePermRes = await _removePermissionHandler.HandleAsync(new RevokeSnowflakePermissionTagCommand(revokePermReq, ctx));

                if (revokePermRes.IsSuccess)
                    await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(base.GetSuccessfulActionEmbed(ctx.Client)));
                else
                    await ctx.EditResponseAsync(
                        new DiscordWebhookBuilder().AddEmbed(base.GetUnsuccessfulResultEmbed(revokePermRes, ctx.Client)));
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(action), action, null);
        }
    }
}
