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
using JetBrains.Annotations;
using Lisbeth.Bot.Application.Discord.Services.Interfaces;
using Lisbeth.Bot.Domain.DTOs.Request;
using System;
using System.Threading.Tasks;
using FluentValidation;
using Lisbeth.Bot.Application.Validation.Tag;
using VimeoDotNet.Models;
using Tag = Lisbeth.Bot.Domain.Entities.Tag;

namespace Lisbeth.Bot.Application.Discord.SlashCommands
{
    [UsedImplicitly]
    [SlashModuleLifespan(SlashModuleLifespan.Transient)]
    public class TagSlashCommands : ApplicationCommandModule
    {
        public IDiscordTagService _discordTagService { private get; set; }
        public IDiscordEmbedConfiguratorService<Tag> _discordEmbedTagConfiguratorService { private get; set; }

        [SlashCommand("tag", "Allows working with tags.")]
        public async Task TagCommand(InteractionContext ctx, 
            [Option("action", "Type of action to perform")] TagActionType action,
            [Option("name-or-id", "Type of action to perform")] string idOrName = "",
            [Option("text", "Base text for the tag.")] string text = "")
        {
            await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);

            bool isId = long.TryParse(idOrName, out long id);
            bool isSuccess = true;
            bool isEmbedConfig = false;

            (DiscordEmbed Embed, string Text) result = new (null, "");

            switch (action)
            {
                case TagActionType.Get:
                    if (!isId && string.IsNullOrWhiteSpace(idOrName))
                        throw new ArgumentException("You must supply a valid Id or name");

                    var getReq = new TagGetReqDto
                    {
                        GuildId = ctx.Guild.Id,
                        Id = isId ? id : null,
                        Name = isId ? null : idOrName,
                        RequestedOnBehalfOfId = ctx.User.Id
                    };

                    var getValidator = new TagGetReqValidator(ctx.Client);
                    await getValidator.ValidateAndThrowAsync(getReq);

                    result = await _discordTagService.GetAsync(ctx, getReq);
                    break;
                case TagActionType.Add:
                    if (string.IsNullOrWhiteSpace(idOrName))
                        throw new ArgumentException("You must supply name.");

                    var addReq = new TagAddReqDto()
                    {
                        GuildId = ctx.Guild.Id,
                        Name = idOrName,
                        RequestedOnBehalfOfId = ctx.User.Id,
                        Text = text
                    };

                    var addValidator = new TagAddReqValidator(ctx.Client);
                    await addValidator.ValidateAndThrowAsync(addReq);

                    result.Embed = await _discordTagService.AddAsync(ctx, addReq);
                    break;
                case TagActionType.Edit:
                    if (!isId && string.IsNullOrWhiteSpace(idOrName))
                        throw new ArgumentException("You must supply a valid Id or name");

                    var editReq = new TagEditReqDto()
                    {
                        GuildId = ctx.Guild.Id,
                        Id = isId ? id : null,
                        Name = isId ? null : idOrName,
                        RequestedOnBehalfOfId = ctx.User.Id,
                        Text = text
                    };

                    var editValidator = new TagEditReqValidator(ctx.Client);
                    await editValidator.ValidateAndThrowAsync(editReq);

                    result.Embed = await _discordTagService.EditAsync(ctx, editReq);
                    break;
                case TagActionType.Remove:
                    if (!isId && string.IsNullOrWhiteSpace(idOrName))
                        throw new ArgumentException("You must supply a valid Id or name");

                    var removeReq = new TagDisableReqDto()
                    {
                        GuildId = ctx.Guild.Id,
                        Id = isId ? id : null,
                        Name = isId ? null : idOrName,
                        RequestedOnBehalfOfId = ctx.User.Id
                    };

                    var disableValidator = new TagDisableReqValidator(ctx.Client);
                    await disableValidator.ValidateAndThrowAsync(removeReq);

                    result.Embed = await _discordTagService.DisableAsync(ctx, removeReq);
                    break;
                case TagActionType.ConfigureEmbed:
                    var cfgResult = await _discordEmbedTagConfiguratorService.ConfigureAsync(ctx, idOrName);
                    result.Embed = cfgResult.Embed;
                    isSuccess = cfgResult.IsSuccess;
                    isEmbedConfig = true;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(action), action, null);
            }

            if (result.Embed is not null)
            {
                if (isSuccess && isEmbedConfig) await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(result.Embed).WithContent("Final result:"));
                else await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(result.Embed));
            }
            else
            {
                await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent(result.Text));
            }
        }
    }
}
