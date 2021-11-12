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
using JetBrains.Annotations;
using Lisbeth.Bot.Application.Discord.Services.Interfaces;
using Lisbeth.Bot.Application.Discord.SlashCommands.Base;
using Lisbeth.Bot.Application.Validation.RoleMenu;
using Lisbeth.Bot.Domain.DTOs.Request.RoleMenu;
using Lisbeth.Bot.Domain.Entities;
using MikyM.Common.Application.Results;
using System;
using System.Threading.Tasks;

namespace Lisbeth.Bot.Application.Discord.SlashCommands
{
    [UsedImplicitly]
    [SlashModuleLifespan(SlashModuleLifespan.Transient)]
    public class RoleMenuSlashCommands : ExtendedApplicationCommandModule
    {
        [UsedImplicitly] public IDiscordRoleMenuService? DiscordRoleMenuService { private get; set; }
        [UsedImplicitly] public IDiscordEmbedConfiguratorService<RoleMenu>? DiscordEmbedConfiguratorService { private get; set; }

        [SlashCommand("role-menu", "Allows working with role menus.")]
        public async Task RoleMenuCommand(InteractionContext ctx,
            [Option("action", "Type of action to perform")]
            RoleMenuActionType action,
            [Option("channel", "Channel to send the tag to")]
            DiscordChannel? channel = null,
            [Option("id", "Type of action to perform")]
            string idOrName = "",
            [Option("text", "Base text for the tag")]
            string text = "")
        {
            await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);

            bool isId = long.TryParse(idOrName, out long id);

            Result<(DiscordWebhookBuilder? Embed, string Text)>? result = null;
            Result<DiscordEmbed>? partial = null;

            switch (action)
            {
                case RoleMenuActionType.Get:
                    if (!isId && string.IsNullOrWhiteSpace(idOrName))
                        throw new ArgumentException("You must supply a valid Id or name");

                    var getReq = new RoleMenuGetReqDto
                    {
                        GuildId = ctx.Guild.Id,
                        Id = isId ? id : null,
                        Name = isId ? null : idOrName,
                        RequestedOnBehalfOfId = ctx.User.Id
                    };

                    var getValidator = new RoleMenuGetReqValidator(ctx.Client);
                    await getValidator.ValidateAndThrowAsync(getReq);

                    result = await this.DiscordRoleMenuService!.GetAsync(ctx, getReq);
                    break;
                case RoleMenuActionType.Add:
                    if (string.IsNullOrWhiteSpace(idOrName))
                        throw new ArgumentException("You must supply name.");

                    var addReq = new RoleMenuAddReqDto
                    {
                        GuildId = ctx.Guild.Id,
                        Name = idOrName,
                        RequestedOnBehalfOfId = ctx.User.Id,
                        Text = text
                    };

                    var addValidator = new RoleMenuAddReqValidator(ctx.Client);
                    await addValidator.ValidateAndThrowAsync(addReq);

                    partial = await this.DiscordRoleMenuService!.CreateRoleMenuAsync(ctx, addReq);
                    break;
                case RoleMenuActionType.Edit:
                    if (!isId && string.IsNullOrWhiteSpace(idOrName))
                        throw new ArgumentException("You must supply a valid Id or name");

                    var editReq = new RoleMenuEditReqDto
                    {
                        GuildId = ctx.Guild.Id,
                        Id = isId ? id : null,
                        Name = isId ? null : idOrName,
                        RequestedOnBehalfOfId = ctx.User.Id,
                        Text = text
                    };

                    var editValidator = new RoleMenuEditReqValidator(ctx.Client);
                    await editValidator.ValidateAndThrowAsync(editReq);

                    //result.Embed = await _discordRoleMenuService.EditAsync(ctx, editReq);
                    break;
                case RoleMenuActionType.Remove:
                    if (!isId && string.IsNullOrWhiteSpace(idOrName))
                        throw new ArgumentException("You must supply a valid Id or name");

                    var removeReq = new RoleMenuDisableReqDto
                    {
                        GuildId = ctx.Guild.Id,
                        Id = isId ? id : null,
                        Name = isId ? null : idOrName,
                        RequestedOnBehalfOfId = ctx.User.Id
                    };

                    var disableValidator = new RoleMenuDisableReqValidator(ctx.Client);
                    await disableValidator.ValidateAndThrowAsync(removeReq);

                    //partial = await _discordRoleMenuService.DisableAsync(ctx, removeReq);
                    break;
                case RoleMenuActionType.ConfigureEmbed:
                    partial = await this.DiscordEmbedConfiguratorService!.ConfigureAsync(ctx, idOrName);
                    break;
                case RoleMenuActionType.Send:
                    if (!isId && string.IsNullOrWhiteSpace(idOrName))
                        throw new ArgumentException("You must supply a valid Id or name");
                    if (channel is null)
                        throw new ArgumentException("You must supply a channel to send a tag");

                    var sendReq = new RoleMenuSendReqDto
                    {
                        GuildId = ctx.Guild.Id,
                        Id = isId ? id : null,
                        Name = isId ? null : idOrName,
                        RequestedOnBehalfOfId = ctx.User.Id,
                        ChannelId = channel.Id
                    };

                    var sendValidator = new RoleMenuSendReqValidator(ctx.Client);
                    await sendValidator.ValidateAndThrowAsync(sendReq);

                    result = await this.DiscordRoleMenuService!.SendAsync(ctx, sendReq);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(action), action, null);
            }

            if (partial.HasValue)
            {
                if (partial.Value.IsDefined())
                    await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(partial.Value.Entity));
                else
                    await ctx.EditResponseAsync(
                        new DiscordWebhookBuilder().AddEmbed(GetUnsuccessfulResultEmbed(partial, ctx.Client)));
            }
            else if (result.HasValue)
            {
                if (!result.Value.IsDefined())
                {
                    await ctx.EditResponseAsync(
                        new DiscordWebhookBuilder().AddEmbed(GetUnsuccessfulResultEmbed(result, ctx.Client)));
                }
                else
                {
                    if (result.Value.Entity.Embed is not null) await ctx.EditResponseAsync(result.Value.Entity.Embed);
                    else await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent(result.Value.Entity.Text));
                }
            }
        }
    }
}