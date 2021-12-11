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
using Lisbeth.Bot.Application.Discord.SlashCommands.Base;
using Lisbeth.Bot.Application.Validation.Tag;
using Lisbeth.Bot.Domain.DTOs.Request.Tag;

namespace Lisbeth.Bot.Application.Discord.SlashCommands;

[UsedImplicitly]
[SlashModuleLifespan(SlashModuleLifespan.Scoped)]
public class TagSlashCommands : ExtendedApplicationCommandModule
{
    public TagSlashCommands(IDiscordTagService discordTagService,
        IDiscordEmbedConfiguratorService<Tag> discordEmbedTagConfiguratorService)
    {
        _discordTagService = discordTagService;
        _discordEmbedTagConfiguratorService = discordEmbedTagConfiguratorService;
    }

    private readonly IDiscordTagService _discordTagService;
    private readonly IDiscordEmbedConfiguratorService<Tag> _discordEmbedTagConfiguratorService;

    [UsedImplicitly]
    [SlashCommand("tag", "Allows working with tags.")]
    public async Task TagCommand(InteractionContext ctx,
        [Option("action", "Type of action to perform")]
        TagActionType action,
        [Option("channel", "Channel to send the tag to.")]
        DiscordChannel? channel = null,
        [Option("id", "Id or name of the tag")]
        string idOrName = "",
        [Option("text", "Base text for the tag.")]
        string text = "")
    {
        await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);

        bool isId = long.TryParse(idOrName, out long id);

        Result<(DiscordEmbed? Embed, string Text)>? result = null;
        Result<DiscordEmbed>? partial = null;

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

                result = await this._discordTagService!.GetAsync(ctx, getReq);
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

                partial = await this._discordTagService!.AddAsync(ctx, addReq);

                break;
            case TagActionType.Edit:
                if (!isId && string.IsNullOrWhiteSpace(idOrName))
                    throw new ArgumentException("You must supply a valid Id or name");

                var editReq = new TagEditReqDto
                {
                    GuildId = ctx.Guild.Id,
                    Id = isId ? id : null,
                    Name = isId ? null : idOrName,
                    RequestedOnBehalfOfId = ctx.User.Id,
                    Text = text
                };

                var editValidator = new TagEditReqValidator(ctx.Client);
                await editValidator.ValidateAndThrowAsync(editReq);

                partial = await this._discordTagService!.EditAsync(ctx, editReq);
                break;
            case TagActionType.Remove:
                if (!isId && string.IsNullOrWhiteSpace(idOrName))
                    throw new ArgumentException("You must supply a valid Id or name");

                var removeReq = new TagDisableReqDto
                {
                    GuildId = ctx.Guild.Id,
                    Id = isId ? id : null,
                    Name = isId ? null : idOrName,
                    RequestedOnBehalfOfId = ctx.User.Id
                };

                var disableValidator = new TagDisableReqValidator(ctx.Client);
                await disableValidator.ValidateAndThrowAsync(removeReq);

                partial = await this._discordTagService!.DisableAsync(ctx, removeReq);
                break;
            case TagActionType.ConfigureEmbed:
                partial = await this._discordEmbedTagConfiguratorService!.ConfigureAsync(ctx, x => x.EmbedConfig, x => x.EmbedConfigId, idOrName);
                break;
            case TagActionType.Send:
                if (!isId && string.IsNullOrWhiteSpace(idOrName))
                    throw new ArgumentException("You must supply a valid Id or name");
                if (channel is null)
                    throw new ArgumentException("You must supply a channel to send a tag");

                var sendReq = new TagSendReqDto
                {
                    GuildId = ctx.Guild.Id,
                    Id = isId ? id : null,
                    Name = isId ? null : idOrName,
                    RequestedOnBehalfOfId = ctx.User.Id,
                    ChannelId = channel.Id
                };

                var sendValidator = new TagSendReqValidator(ctx.Client);
                await sendValidator.ValidateAndThrowAsync(sendReq);

                result = await this._discordTagService!.SendAsync(ctx, sendReq);
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
                    new DiscordWebhookBuilder().AddEmbed(base.GetUnsuccessfulResultEmbed(partial, ctx.Client)));
        }
        else if (result.HasValue)
        {
            if (!result.Value.IsDefined())
            {
                await ctx.EditResponseAsync(
                    new DiscordWebhookBuilder().AddEmbed(base.GetUnsuccessfulResultEmbed(result, ctx.Client)));
            }
            else
            {
                if (result.Value.Entity.Embed is not null)
                    await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(result.Value.Entity.Embed));
                else await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent(result.Value.Entity.Text));
            }
        }
    }
}
