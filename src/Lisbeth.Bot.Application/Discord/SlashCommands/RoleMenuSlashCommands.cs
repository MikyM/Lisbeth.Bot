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

using System;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using FluentValidation;
using JetBrains.Annotations;
using Lisbeth.Bot.Application.Discord.Services.Interfaces;
using Lisbeth.Bot.Application.Validation.RoleMenu;
using Lisbeth.Bot.Application.Validation.Tag;
using Lisbeth.Bot.Domain.DTOs.Request;
using Lisbeth.Bot.Domain.DTOs.Request.RoleMenu;
using Lisbeth.Bot.Domain.DTOs.Request.Tag;
using Lisbeth.Bot.Domain.Entities;

namespace Lisbeth.Bot.Application.Discord.SlashCommands
{
    [UsedImplicitly]
    [SlashModuleLifespan(SlashModuleLifespan.Transient)]
    public class RoleMenuSlashCommands
    {
        public IDiscordRoleMenuService _discordRoleMenuService { private get; set; }
        public IDiscordEmbedConfiguratorService<RoleMenu> _discordEmbedConfiguratorService { private get; set; }

        [SlashCommand("tag", "Allows working with tags.")]
        public async Task RoleMenuCommand(InteractionContext ctx,
            [Option("action", "Type of action to perform")]
            RoleMenuActionType action,
            [Option("channel", "Channel to send the tag to.")]
            DiscordChannel channel = null,
            [Option("id", "Type of action to perform")]
            string idOrName = "",
            [Option("text", "Base text for the tag.")]
            string text = "")
        {
            await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);

            bool isId = long.TryParse(idOrName, out long id);
            bool isSuccess = true;
            bool isEmbedConfig = false;
            DiscordWebhookBuilder wbhk = null;

            (DiscordEmbed Embed, string Text) result = new(null, "");

            switch (action)
            {
                case RoleMenuActionType.Get:
                    if (!isId && string.IsNullOrWhiteSpace(idOrName))
                        throw new ArgumentException("You must supply a valid Id or name");

                    var getReq = new RoleMenuGetReqDto()
                    {
                        GuildId = ctx.Guild.Id,
                        Id = isId ? id : null,
                        Name = isId ? null : idOrName,
                        RequestedOnBehalfOfId = ctx.User.Id
                    };

                    var getValidator = new RoleMenuGetReqValidator(ctx.Client);
                    await getValidator.ValidateAndThrowAsync(getReq);

                    var getResult = await _discordRoleMenuService.GetAsync(ctx, getReq);
                    wbhk = getResult.Builder;
                    result.Text = getResult.Text;
                    break;
                case RoleMenuActionType.Add:
                    if (string.IsNullOrWhiteSpace(idOrName))
                        throw new ArgumentException("You must supply name.");

                    var addReq = new RoleMenuAddReqDto()
                    {
                        GuildId = ctx.Guild.Id,
                        Name = idOrName,
                        RequestedOnBehalfOfId = ctx.User.Id,
                        Text = text
                    };

                    var addValidator = new RoleMenuAddReqValidator(ctx.Client);
                    await addValidator.ValidateAndThrowAsync(addReq);

                    var partialResult = await _discordRoleMenuService.CreateRoleMenuAsync(ctx, addReq);

                    result.Embed = partialResult.Embed;

                    break;
                case RoleMenuActionType.Edit:
                    if (!isId && string.IsNullOrWhiteSpace(idOrName))
                        throw new ArgumentException("You must supply a valid Id or name");

                    var editReq = new RoleMenuEditReqDto()
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

                    var removeReq = new RoleMenuDisableReqDto()
                    {
                        GuildId = ctx.Guild.Id,
                        Id = isId ? id : null,
                        Name = isId ? null : idOrName,
                        RequestedOnBehalfOfId = ctx.User.Id
                    };

                    var disableValidator = new RoleMenuDisableReqValidator(ctx.Client);
                    await disableValidator.ValidateAndThrowAsync(removeReq);

                   // result.Embed = await _discordRoleMenuService..DisableAsync(ctx, removeReq);
                    break;
                case RoleMenuActionType.ConfigureEmbed:
                    var cfgResult = await _discordEmbedConfiguratorService.ConfigureAsync(ctx, idOrName);
                    result.Embed = cfgResult.Embed;
                    isSuccess = cfgResult.IsSuccess;
                    isEmbedConfig = true;
                    break;
                case RoleMenuActionType.Send:
                    if (!isId && string.IsNullOrWhiteSpace(idOrName))
                        throw new ArgumentException("You must supply a valid Id or name");
                    if (channel is null)
                        throw new ArgumentException("You must supply a channel to send a tag");

                    var sendReq = new RoleMenuSendReqDto()
                    {
                        GuildId = ctx.Guild.Id,
                        Id = isId ? id : null,
                        Name = isId ? null : idOrName,
                        RequestedOnBehalfOfId = ctx.User.Id,
                        ChannelId = channel.Id
                    };

                    var sendValidator = new RoleMenuSendReqValidator(ctx.Client);
                    await sendValidator.ValidateAndThrowAsync(sendReq);

                    var sendResult = await _discordRoleMenuService.SendAsync(ctx, sendReq);
                    wbhk = sendResult.Builder;
                    result.Text = sendResult.Text;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(action), action, null);
            }

            if (result.Embed is not null)
            {
                if (isSuccess && isEmbedConfig && wbhk is null)
                    await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(result.Embed)
                        .WithContent("Final result:"));
                else await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(result.Embed));
            }
            else
            {
                if (wbhk is null)
                    await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent(result.Text));
                else
                    await ctx.EditResponseAsync(wbhk);
            }
        }
    }
}
