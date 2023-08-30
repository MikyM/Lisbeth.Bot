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

using FluentValidation;
using Lisbeth.Bot.Application.Discord.Commands.RoleMenu;
using Lisbeth.Bot.Application.Discord.SlashCommands.Base;
using Lisbeth.Bot.Application.Validation.RoleMenu;
using Lisbeth.Bot.Domain.DTOs.Request.RoleMenu;

namespace Lisbeth.Bot.Application.Discord.SlashCommands;

[UsedImplicitly]
[SlashModuleLifespan(SlashModuleLifespan.Scoped)]
public class RoleMenuSlashCommands : ExtendedApplicationCommandModule
{
    private readonly IAsyncCommandHandler<SendRoleMenuCommand> _sendRoleMenuHandler;
    private readonly IAsyncCommandHandler<GetRoleMenuCommand, DiscordMessageBuilder> _getRoleMenuHandler;
    private readonly IAsyncCommandHandler<CreateRoleMenuCommand, DiscordMessageBuilder> _createRoleMenuHandler;
    private readonly IDiscordEmbedConfiguratorService<RoleMenu> _discordEmbedConfiguratorService;

    public RoleMenuSlashCommands(IAsyncCommandHandler<SendRoleMenuCommand> sendRoleMenuHandler,
        IAsyncCommandHandler<GetRoleMenuCommand, DiscordMessageBuilder> getRoleMenuHandler,
        IAsyncCommandHandler<CreateRoleMenuCommand, DiscordMessageBuilder> createRoleMenuHandler,
        IDiscordEmbedConfiguratorService<RoleMenu> discordEmbedConfiguratorService)
    {
        _sendRoleMenuHandler = sendRoleMenuHandler;
        _getRoleMenuHandler = getRoleMenuHandler;
        _createRoleMenuHandler = createRoleMenuHandler;
        _discordEmbedConfiguratorService = discordEmbedConfiguratorService;
    }

    [UsedImplicitly]
    [SlashCommand("role-menu", "Allows working with role menus.", false)]
    public async Task RoleMenuCommand(InteractionContext ctx,
        [Option("action", "Type of action to perform")]
        RoleMenuActionType action,
        [Option("channel", "Channel to send the role menu to")]
        DiscordChannel? channel = null,
        [Option("name", "Name of the role menu")]
        string name = "",
        [Option("text", "Base text for the role menu")]
        string text = "")
    {
        await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);

        switch (action)
        {
            case RoleMenuActionType.Get:
                if (string.IsNullOrWhiteSpace(name))
                    throw new ArgumentException("You must supply a name");

                var getReq = new RoleMenuGetReqDto
                {
                    GuildId = ctx.Guild.Id,
                    Name = name,
                    RequestedOnBehalfOfId = ctx.User.Id
                };

                var getValidator = new RoleMenuGetReqValidator(ctx.Client);
                await getValidator.ValidateAndThrowAsync(getReq);

                var getRes = await _getRoleMenuHandler.HandleAsync(new GetRoleMenuCommand(getReq, ctx));
                if (getRes.IsDefined(out var builder))
                    await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbeds(builder.Embeds)
                        .AddComponents(builder.Components)
                        .WithContent(builder.Content));
                else
                    await ctx.EditResponseAsync(
                        new DiscordWebhookBuilder().AddEmbed(GetUnsuccessfulResultEmbed(getRes, ctx.Client)));

                break;
            case RoleMenuActionType.Create:
                if (string.IsNullOrWhiteSpace(name))
                    throw new ArgumentException("You must supply name.");

                var addReq = new RoleMenuAddReqDto
                {
                    GuildId = ctx.Guild.Id,
                    Name = name,
                    RequestedOnBehalfOfId = ctx.User.Id,
                    Text = text
                };

                var addValidator = new RoleMenuDiscordAddReqValidator(ctx.Client);
                await addValidator.ValidateAndThrowAsync(addReq);

                var createRes = await _createRoleMenuHandler.HandleAsync(new CreateRoleMenuCommand(addReq, ctx));
                if (createRes.IsDefined(out var createBuilder))
                    await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbeds(createBuilder.Embeds)
                        .AddComponents(createBuilder.Components)
                        .WithContent(createBuilder.Content));
                else
                    await ctx.EditResponseAsync(
                        new DiscordWebhookBuilder().AddEmbed(GetUnsuccessfulResultEmbed(createRes, ctx.Client)));
                break;
            case RoleMenuActionType.Edit:
                if (string.IsNullOrWhiteSpace(name))
                    throw new ArgumentException("You must supply a name");

                var editReq = new RoleMenuEditReqDto
                {
                    GuildId = ctx.Guild.Id,
                    Name = name,
                    RequestedOnBehalfOfId = ctx.User.Id,
                    Text = text
                };

                var editValidator = new RoleMenuEditReqValidator(ctx.Client);
                await editValidator.ValidateAndThrowAsync(editReq);

                //result.Embed = await _discordRoleMenuService.EditAsync(ctx, editReq);
                break;
            case RoleMenuActionType.Remove:
                if (string.IsNullOrWhiteSpace(name))
                    throw new ArgumentException("You must supply a name");

                var removeReq = new RoleMenuDisableReqDto
                {
                    GuildId = ctx.Guild.Id,
                    Name = name,
                    RequestedOnBehalfOfId = ctx.User.Id
                };

                var disableValidator = new RoleMenuDisableReqValidator(ctx.Client);
                await disableValidator.ValidateAndThrowAsync(removeReq);

                //partial = await _discordRoleMenuService.DisableAsync(ctx, removeReq);
                break;
            case RoleMenuActionType.ConfigureEmbed:
                var configRes = await _discordEmbedConfiguratorService.ConfigureAsync(ctx, x => x.EmbedConfig,
                    x => x.EmbedConfigId, name);
                if (configRes.IsDefined(out var configEmbed))
                    await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(configEmbed));
                else
                    await ctx.EditResponseAsync(
                        new DiscordWebhookBuilder().AddEmbed(GetUnsuccessfulResultEmbed(configRes, ctx.Client)));
                break;
            case RoleMenuActionType.Send:
                if (string.IsNullOrWhiteSpace(name))
                    throw new ArgumentException("You must supply a name");
                if (channel is null) throw new ArgumentException("You must supply a channel to send a role menu");

                var sendReq = new RoleMenuSendReqDto
                {
                    GuildId = ctx.Guild.Id,
                    Name = name,
                    RequestedOnBehalfOfId = ctx.User.Id,
                    ChannelId = channel.Id
                };

                var sendValidator = new RoleMenuSendReqValidator(ctx.Client);
                await sendValidator.ValidateAndThrowAsync(sendReq);

                var sendRes = await _sendRoleMenuHandler.HandleAsync(new SendRoleMenuCommand(sendReq, ctx));
                if (sendRes.IsSuccess)
                    await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(GetSuccessfulActionEmbed(ctx.Client)));
                else
                    await ctx.EditResponseAsync(
                        new DiscordWebhookBuilder().AddEmbed(GetUnsuccessfulResultEmbed(sendRes, ctx.Client)));
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(action), action, null);
        }
    }
}
