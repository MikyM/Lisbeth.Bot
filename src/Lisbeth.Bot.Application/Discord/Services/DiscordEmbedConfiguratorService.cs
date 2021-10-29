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
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using AutoMapper;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.Extensions;
using DSharpPlus.SlashCommands;
using JetBrains.Annotations;
using Lisbeth.Bot.Application.Discord.Exceptions;
using Lisbeth.Bot.Application.Discord.Extensions;
using Lisbeth.Bot.Application.Discord.Helpers;
using Lisbeth.Bot.Application.Discord.Helpers.InteractionIdEnums.Buttons;
using Lisbeth.Bot.Application.Discord.Helpers.InteractionIdEnums.Selects;
using Lisbeth.Bot.Application.Discord.Helpers.InteractionIdEnums.SelectValues;
using Lisbeth.Bot.Application.Discord.Services.Interfaces;
using Lisbeth.Bot.Application.Enums;
using Lisbeth.Bot.Application.Extensions;
using Lisbeth.Bot.Application.Services.Interfaces.Database;
using Lisbeth.Bot.DataAccessLayer;
using Lisbeth.Bot.DataAccessLayer.Specifications.EmbedConfig;
using Lisbeth.Bot.Domain.Entities;
using Lisbeth.Bot.Domain.Entities.Base;
using MikyM.Common.Application.Interfaces;

namespace Lisbeth.Bot.Application.Discord.Services
{
    [UsedImplicitly]
    public class DiscordEmbedConfiguratorService<T> : IDiscordEmbedConfiguratorService<T> where T : EmbedConfigEntity
    {
        private readonly IEmbedConfigService _embedConfigService;
        private readonly IDiscordEmbedProvider _embedProvider;
        private readonly IMapper _mapper;
        private readonly ICrudService<T, LisbethBotDbContext> _service;

        public DiscordEmbedConfiguratorService(ICrudService<T, LisbethBotDbContext> service, IMapper mapper,
            IDiscordEmbedProvider embedProvider, IEmbedConfigService embedConfigService)
        {
            _service = service;
            _mapper = mapper;
            _embedProvider = embedProvider;
            _embedConfigService = embedConfigService;
        }

        public async Task<(DiscordEmbed Embed, bool IsSuccess)> ConfigureAsync(InteractionContext ctx, string idOrName)
        {
            if (ctx is null) throw new ArgumentNullException(nameof(ctx));

            bool isValidId = long.TryParse(idOrName, out long id);

            var entity = await _service.GetSingleBySpecAsync<T>(
                new ActiveWithEmbedCfgByIdOrNameSpecifications<T>(isValidId ? id : null, idOrName, ctx.Guild.Id));

            if (entity is null) throw new ArgumentException("Target object not found.");
            if (entity.GuildId != ctx.Guild.Id) throw new DiscordNotAuthorizedException();

            var member = await ctx.Guild.GetMemberAsync(ctx.User.Id);

            if (entity.CreatorId != ctx.User.Id && !member.IsModerator())
                throw new DiscordNotAuthorizedException();

            var intr = ctx.Client.GetInteractivity();
            int loopCount = 0;

            var mainMenu = new DiscordEmbedBuilder();
            mainMenu.WithAuthor($"Embed configurator menu for Id: {idOrName}");
            mainMenu.WithDescription("Please select an option below to edit your embed!");
            mainMenu.WithFooter($"Parent Id: {idOrName}");
            mainMenu.WithColor(new DiscordColor("#26296e"));

            var resultEmbed = new DiscordEmbedBuilder();
            var wbhk = new DiscordWebhookBuilder();

            if (entity.EmbedConfig is not null)
            {
                resultEmbed = _embedProvider.ConfigureEmbed(entity.EmbedConfig);
                wbhk.AddEmbed(resultEmbed.Build()).WithContent("Current embed:");
            }

            var mainMenuSelectOptions = new List<DiscordSelectComponentOption>
            {
                new("Set author", nameof(EmbedConfigSelectValue.EmbedConfigSetAuthorValue),
                    "Sets embeds author values", false,
                    new DiscordComponentEmoji(DiscordEmoji.FromName(ctx.Client, ":man:"))),
                new("Set footer", nameof(EmbedConfigSelectValue.EmbedConfigSetFooterValue),
                    "Sets embeds footer values", false,
                    new DiscordComponentEmoji(DiscordEmoji.FromName(ctx.Client, ":footprints:"))),
                new("Set description", nameof(EmbedConfigSelectValue.EmbedConfigSetDescValue),
                    "Sets embeds description", false,
                    new DiscordComponentEmoji(DiscordEmoji.FromName(ctx.Client, ":page_facing_up:"))),
                new("Set image", nameof(EmbedConfigSelectValue.EmbedConfigSetImageValue), "Sets embeds image", false
                    , new DiscordComponentEmoji(DiscordEmoji.FromName(ctx.Client, ":frame_photo:"))),
                new("Set field", nameof(EmbedConfigSelectValue.EmbedConfigSetFieldValue), "Sets field values", false
                    , new DiscordComponentEmoji(DiscordEmoji.FromName(ctx.Client, ":page_with_curl:"))),
                new("Remove field", nameof(EmbedConfigSelectValue.EmbedConfigDeleteFieldValue),
                    "Removes a field from the embed", false,
                    new DiscordComponentEmoji(DiscordEmoji.FromName(ctx.Client, ":heavy_multiplication_x:"))),
                new("Set embed color", nameof(EmbedConfigSelectValue.EmbedConfigSetColorValue), "Sets embeds color",
                    false, new DiscordComponentEmoji(DiscordEmoji.FromName(ctx.Client, ":rainbow:"))),
                new("Set embed thumbnail", nameof(EmbedConfigSelectValue.EmbedConfigSetThumbnailValue),
                    "Sets embeds thumbnail", false,
                    new DiscordComponentEmoji(DiscordEmoji.FromName(ctx.Client, ":sunrise:"))),
                new("Set embed title", nameof(EmbedConfigSelectValue.EmbedConfigSetTitleValue), "Sets embeds title",
                    false, new DiscordComponentEmoji(DiscordEmoji.FromName(ctx.Client, ":newspaper:"))),
                new("Set embed timestamp", nameof(EmbedConfigSelectValue.EmbedConfigSetTimestampValue),
                    "Sets embeds timestamp", false, new DiscordComponentEmoji(
                        DiscordEmoji.FromName(ctx.Client, ":clock1:")))
            };
            var mainMenuSelect = new DiscordSelectComponent(nameof(EmbedConfigSelect.EmbedConfigMainSelect),
                "Choose an action", mainMenuSelectOptions);

            var mainMsg = await ctx.EditResponseAsync(wbhk.AddEmbed(mainMenu.Build()).AddComponents(mainMenuSelect));

            var waitResult = await intr.WaitForSelectAsync(mainMsg, ctx.User,
                nameof(EmbedConfigSelect.EmbedConfigMainSelect), TimeSpan.FromMinutes(1));

            if (waitResult.TimedOut)
            {
                await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(GetTimedOutEmbed(idOrName, true)));
                return (null, false);
            }

            string choice = waitResult.Result.Values[0];

            while (true)
            {
                if (loopCount > 30) return (resultEmbed, true);

                (DiscordEmbedBuilder Result, bool IsSuccess) result = new(resultEmbed, false);
                switch (choice)
                {
                    case nameof(EmbedConfigSelectValue.EmbedConfigSetAuthorValue):
                        result = await SetModuleAsync(EmbedConfigModuleType.Author, ctx, intr, resultEmbed, entity);
                        break;
                    case nameof(EmbedConfigSelectValue.EmbedConfigSetFooterValue):
                        result = await SetModuleAsync(EmbedConfigModuleType.Footer, ctx, intr, resultEmbed, entity);
                        break;
                    case nameof(EmbedConfigSelectValue.EmbedConfigSetDescValue):
                        result = await SetModuleAsync(EmbedConfigModuleType.Description, ctx, intr, resultEmbed,
                            entity);
                        break;
                    case nameof(EmbedConfigSelectValue.EmbedConfigSetFieldValue):
                        result = await SetModuleAsync(EmbedConfigModuleType.Field, ctx, intr, resultEmbed, entity);
                        break;
                    case nameof(EmbedConfigSelectValue.EmbedConfigDeleteFieldValue):
                        result = await SetModuleAsync(EmbedConfigModuleType.RemoveField, ctx, intr, resultEmbed,
                            entity);
                        break;
                    case nameof(EmbedConfigSelectValue.EmbedConfigSetImageValue):
                        result = await SetModuleAsync(EmbedConfigModuleType.Image, ctx, intr, resultEmbed, entity);
                        break;
                    case nameof(EmbedConfigSelectValue.EmbedConfigSetColorValue):
                        result = await SetModuleAsync(EmbedConfigModuleType.Color, ctx, intr, resultEmbed, entity);
                        break;
                    case nameof(EmbedConfigSelectValue.EmbedConfigSetTitleValue):
                        result = await SetModuleAsync(EmbedConfigModuleType.Title, ctx, intr, resultEmbed, entity);
                        break;
                    case nameof(EmbedConfigSelectValue.EmbedConfigSetThumbnailValue):
                        result = await SetModuleAsync(EmbedConfigModuleType.Thumbnail, ctx, intr, resultEmbed, entity);
                        break;
                    case nameof(EmbedConfigSelectValue.EmbedConfigSetTimestampValue):
                        result = await SetModuleAsync(EmbedConfigModuleType.Timestamp, ctx, intr, resultEmbed, entity);
                        break;
                }

                if (result.IsSuccess) resultEmbed = result.Result;

                var finalizeMsg = await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(resultEmbed.Build())
                    .AddEmbed(mainMenu.Build())
                    .AddComponents(mainMenuSelect)
                    .AddComponents(GetMainFinalizeButton(ctx.Client)));

                var waitForFinalButtonTask = intr.WaitForButtonAsync(finalizeMsg, ctx.User, TimeSpan.FromMinutes(1));

                var waitForSelectTask = intr.WaitForSelectAsync(finalizeMsg, ctx.User,
                    nameof(EmbedConfigSelect.EmbedConfigMainSelect), TimeSpan.FromMinutes(1));

                var taskAggregate = await Task.WhenAny(new[] {waitForFinalButtonTask, waitForSelectTask});

                if (taskAggregate.Result.TimedOut ||
                    taskAggregate.Result.Result.Id == nameof(EmbedConfigButton.EmbedConfigFinalButton))
                    return (resultEmbed, result.IsSuccess);

                loopCount++;

                choice = waitForSelectTask.Result.Result.Values[0];
            }
        }

        private async Task<(DiscordEmbedBuilder Result, bool IsSuccess)> SetModuleAsync(EmbedConfigModuleType action,
            InteractionContext ctx, InteractivityExtension intr, DiscordEmbedBuilder currentResult, T foundEntity)
        {
            var embed = new DiscordEmbedBuilder().WithFooter($"Parent Id: {foundEntity.Id}");

            string author;
            string desc;
            string req = null;
            string example;

            switch (action)
            {
                case EmbedConfigModuleType.Author:
                    author = "Author";
                    desc =
                        "Please **reply** to this message with embed author configuration as follows: @name@ yourAuthorName @endName@ @url@ yourAuthorUrl @endUrl@ @imageUrl@ yourAuthorImageUrl @endImageUrl@";
                    example =
                        "@name@ Lisbeth @endName@ @url@ https://lisbeth-is-awesome.com@url @endUrl@ @imageUrl@ https://lisbeth-is-awesome.com/my-fancy-image.jpg @endImageUrl@";
                    req = "__You must supply an author name, other values are optional__";
                    break;
                case EmbedConfigModuleType.Footer:
                    author = "Footer";
                    desc =
                        "Please **reply** to this message with embed footer configuration as follows: @text@yourFooterText@endText@ @imageUrl@yourFooterImageUrl@endImageUrl@";
                    example =
                        "@text@Lisbeth@endText@ @imageUrl@ https://lisbeth-is-awesome.com/my-fancy-image.jpg @endImageUrl@";
                    req = "__You must supply footer text, image url is optional__";
                    break;
                case EmbedConfigModuleType.Description:
                    author = "Description";
                    desc = "Please **reply** to this message with embed description.";
                    example = "My super description.";
                    req = "__It can't be an empty string.__";
                    break;
                case EmbedConfigModuleType.Image:
                    author = "Image";
                    desc = "Please **reply** to this message with embed image url";
                    example = "Example:https://lisbeth-is-awesome.com/my-fancy-image.jpg";
                    req = "__It can't be an empty string.__";
                    break;
                case EmbedConfigModuleType.Field:
                    author = "Field";
                    desc =
                        "Please **reply** to this message with embed field configuration as follows: @title@yourFieldTitle@endTitle@ @text@yourFieldText@endText@";
                    example = "@title@Super awesome title@endTitle@ @Super awesome field text@text@";
                    req = "__You must supply both values.__";
                    break;
                case EmbedConfigModuleType.RemoveField:
                    author = "Field Removal";
                    desc = "Please **reply** to this message with embed field title you'd like to remove:";
                    example = "Super awesome field title";
                    break;
                case EmbedConfigModuleType.Color:
                    author = "Color Change";
                    desc = "Please **reply** to this message with color's HEX value:";
                    example = "#26296e";
                    break;
                case EmbedConfigModuleType.Title:
                    author = "Title";
                    desc = "Please **reply** to this message with embed title:";
                    example = "Super awesome embed title";
                    break;
                case EmbedConfigModuleType.Thumbnail:
                    author = "Thumbnail";
                    desc =
                        "Please **reply** to this message with thumbnail url as follows: @url@ yourUrl @endUrl@ @height@ yourHeight @endHeight@ @width@ yourWidth @endWidth@";
                    example =
                        "@url@ https://lisbeth-is-awesome.com/my-fancy-thumbnail.jpg @endUrl @height@ 10 @endHeight@ @width@ 20 @endWidth@";
                    req = "__You must supply both an url, height and width are optional.__";
                    break;
                case EmbedConfigModuleType.Timestamp:
                    author = "Timestamp";
                    desc =
                        "Please **reply** to this message with embed timestamp date in a dd/mm/yy hh/mm format (12H format):";
                    example = "24/12/2021 1:00 PM";
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(action), action, null);
            }

            embed.WithAuthor($"Embed {author} configurator for Id: {foundEntity.Id}");
            embed.AddField("Instructions", desc);
            embed.AddField("Example", example);
            if (req is not null) embed.AddField("Requirements", req);
            if (action is not EmbedConfigModuleType.Field or EmbedConfigModuleType.RemoveField)
                embed.AddField("Removal", "Please reply with **@remove@** to remove this embed part");

            await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(embed.Build()));

            var waitResult = await intr.WaitForMessageAsync(
                x => x.ChannelId == ctx.Channel.Id && x.Author.Id == ctx.User.Id, TimeSpan.FromMinutes(1));

            if (waitResult.TimedOut)
            {
                await ctx.EditResponseAsync(
                    new DiscordWebhookBuilder().AddEmbed(GetTimedOutEmbed(foundEntity.Id.ToString())));
                return (currentResult, false);
            }

            if (string.IsNullOrWhiteSpace(waitResult.Result.Content.Trim()))
                throw new ArgumentException("Response can't be empty or null.");

            if (waitResult.Result.Content.Trim() == "@remove@")
                switch (action)
                {
                    case EmbedConfigModuleType.Author:
                        currentResult.WithAuthor();
                        break;
                    case EmbedConfigModuleType.Footer:
                        currentResult.WithFooter();
                        break;
                    case EmbedConfigModuleType.Description:
                        currentResult.WithDescription(null);
                        break;
                    case EmbedConfigModuleType.Image:
                        currentResult.ImageUrl = null;
                        break;
                    case EmbedConfigModuleType.Color:
                        currentResult.WithColor(new DiscordColor("#7d7d7d"));
                        break;
                    case EmbedConfigModuleType.Title:
                        currentResult.WithTitle(null);
                        break;
                    case EmbedConfigModuleType.Timestamp:
                        currentResult.WithTimestamp(null);
                        break;
                    case EmbedConfigModuleType.Thumbnail:
                        currentResult.Thumbnail = null;
                        break;
                    case EmbedConfigModuleType.Field:
                        break;
                    case EmbedConfigModuleType.RemoveField:
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(action), action, null);
                }
            else
                switch (action)
                {
                    case EmbedConfigModuleType.Author:
                        string authorText = waitResult.Result.Content.GetStringBetween("@name@", "@endName@").Trim();
                        string authorUrl = waitResult.Result.Content.GetStringBetween("@url@", "@endUrl@").Trim();
                        string authorImageUrl = waitResult.Result.Content
                            .GetStringBetween("@imageUrl@", "@endImageUrl@")
                            .Trim();

                        if (string.IsNullOrWhiteSpace(authorText)) throw new ArgumentNullException(nameof(authorText));
                        if (authorText.Length > 256)
                            throw new ArgumentException("Author text has a limit of 256 characters.");

                        currentResult.WithAuthor(authorText, authorUrl == "" ? null : authorUrl,
                            authorImageUrl == "" ? null : authorImageUrl);
                        break;
                    case EmbedConfigModuleType.Footer:
                        string footerText = waitResult.Result.Content.GetStringBetween("@text@", "@endText@").Trim();
                        string footerImageUrl = waitResult.Result.Content
                            .GetStringBetween("@imageUrl@", "@endImageUrl@")
                            .Trim();

                        if (string.IsNullOrWhiteSpace(footerText)) throw new ArgumentNullException(nameof(footerText));
                        if (footerText.Length > 2048)
                            throw new ArgumentException("Footer text has a limit of 2048 characters.");

                        currentResult.WithFooter(footerText, footerImageUrl == "" ? null : footerImageUrl);
                        break;
                    case EmbedConfigModuleType.Description:
                        string newDesc = waitResult.Result.Content;
                        if (newDesc.Length > 2048)
                            throw new ArgumentException("Description text has a limit of 4096 characters.");
                        currentResult.WithDescription(newDesc);
                        break;
                    case EmbedConfigModuleType.Image:
                        string newImage = waitResult.Result.Content;
                        currentResult.WithImageUrl(newImage);
                        break;
                    case EmbedConfigModuleType.Field:
                        if (currentResult.Fields.Count == 25)
                            throw new ArgumentException("This embed already has maximum number of fields (25).");

                        string fieldTitle = waitResult.Result.Content.GetStringBetween("@title@", "@endTitle@").Trim();
                        string fieldText = waitResult.Result.Content.GetStringBetween("@text@", "@endText@").Trim();

                        if (string.IsNullOrWhiteSpace(fieldTitle)) throw new ArgumentNullException(nameof(fieldTitle));
                        if (string.IsNullOrWhiteSpace(fieldText)) throw new ArgumentNullException(nameof(fieldText));

                        if (fieldTitle.Length > 256)
                            throw new ArgumentException("Field title has a limit of 256 characters.");
                        if (fieldText.Length > 256)
                            throw new ArgumentException("Field title has a limit of 1024 characters.");

                        currentResult.AddField(fieldTitle, fieldText);
                        break;
                    case EmbedConfigModuleType.RemoveField:
                        string titleToRemove = waitResult.Result.Content.Trim();
                        int index = currentResult.Fields.ToList().FindIndex(x => x.Name == titleToRemove);

                        if (index == -1) throw new ArgumentException("Field with given title wasn't found");

                        currentResult.RemoveFieldAt(index);
                        break;
                    case EmbedConfigModuleType.Color:
                        string colorToSet = waitResult.Result.Content.Trim();
                        bool isValid = new Regex("^#?[0-9A-F]{6}$").IsMatch(colorToSet);

                        if (!isValid) throw new ArgumentException("Given HEX color value is not valid");

                        currentResult.WithColor(new DiscordColor(colorToSet));
                        break;
                    case EmbedConfigModuleType.Title:
                        string titleToSet = waitResult.Result.Content.Trim();
                        if (titleToSet.Length > 256)
                            throw new ArgumentException("Embed title has a limit of 256 characters.");

                        currentResult.WithTitle(titleToSet);
                        break;
                    case EmbedConfigModuleType.Timestamp:
                        string timeStamp = waitResult.Result.Content.Trim();
                        bool isValidDateTime = DateTime.TryParseExact(timeStamp, "d/MM/yyyy h:mm tt",
                            DateTimeFormatInfo.InvariantInfo, DateTimeStyles.None, out DateTime parsedDate);

                        if (!isValidDateTime) throw new ArgumentException("Given date and time are not valid.");

                        currentResult.WithTimestamp(parsedDate);
                        break;
                    case EmbedConfigModuleType.Thumbnail:
                        string thumbnail = waitResult.Result.Content.Trim();
                        string thumbnailUrl = waitResult.Result.Content.GetStringBetween("@url@", "@endUrl@").Trim();
                        string height = waitResult.Result.Content.GetStringBetween("@height@", "@endHeight@").Trim();
                        string width = waitResult.Result.Content.GetStringBetween("@width@", "@endWidth@").Trim();

                        if (string.IsNullOrWhiteSpace(thumbnailUrl))
                            throw new ArgumentException("Thumbnail URL is required.");

                        bool isHeightValid = false;
                        bool isWidthValid = false;
                        int parsedWidth = 0;
                        int parsedHeight = 0;

                        if (!string.IsNullOrWhiteSpace(height)) isHeightValid = int.TryParse(height, out parsedHeight);
                        if (!string.IsNullOrWhiteSpace(width)) isWidthValid = int.TryParse(height, out parsedWidth);

                        currentResult.WithThumbnail(thumbnail, isHeightValid ? parsedHeight : 0,
                            isWidthValid ? parsedWidth : 0);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(action), action, null);
                }

            if (!currentResult.IsValid())
                throw new ArgumentException("Total count of characters in an embed can't exceed 6000.");

            var msg = await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(currentResult.Build())
                .WithContent("Your current result is:")
                .AddComponents(GetModuleFinalizeButtons(ctx.Client)));

            var btnWaitResult = await intr.WaitForButtonAsync(msg, ctx.User, TimeSpan.FromMinutes(1));

            if (btnWaitResult.TimedOut)
            {
                await ctx.EditResponseAsync(
                    new DiscordWebhookBuilder().AddEmbed(GetTimedOutEmbed(foundEntity.Id.ToString())));
                return (null, false);
            }

            switch (btnWaitResult.Result.Id)
            {
                case nameof(EmbedConfigButton.EmbedConfigConfirmButton):
                    var newEmbed = _mapper.Map<EmbedConfig>(currentResult.Build());
                    if (foundEntity.EmbedConfig is null)
                    {
                        _service.BeginUpdate(foundEntity);
                        foundEntity.EmbedConfig = newEmbed;

                        await _embedConfigService.AddAsync(foundEntity.EmbedConfig, true);
                        foundEntity.EmbedConfigId ??= foundEntity.EmbedConfig.Id;
                        await _service.CommitAsync();
                    }
                    else
                    {
                        var entity = foundEntity.EmbedConfig;

                        // we null cause EF core doesn't detach children objects when detaching parent and we face an ex
                        entity.Tag = null;
                        entity.RecurringReminder = null;
                        entity.Reminder = null;
                        entity.RoleMenu = null;

                        _embedConfigService.BeginUpdate(entity);
                        entity.Fields = newEmbed.Fields;
                        entity.Author = newEmbed.Author;
                        entity.AuthorImageUrl = newEmbed.AuthorImageUrl;
                        entity.AuthorUrl = newEmbed.AuthorUrl;
                        entity.Footer = newEmbed.Footer;
                        entity.FooterImageUrl = newEmbed.FooterImageUrl;
                        entity.ImageUrl = newEmbed.ImageUrl;
                        entity.Description = newEmbed.Description;
                        entity.HexColor = newEmbed.HexColor;
                        entity.Thumbnail = newEmbed.Thumbnail;
                        entity.ThumbnailHeight = newEmbed.ThumbnailHeight;
                        entity.ThumbnailWidth = newEmbed.ThumbnailWidth;
                        entity.Timestamp = newEmbed.Timestamp;
                        entity.Title = newEmbed.Title;
                        await _embedConfigService.CommitAsync();
                    }

                    return (currentResult, true);
                case nameof(EmbedConfigButton.EmbedConfigAbortButton):
                    return (null, false);
            }

            return (null, false);
        }

        private DiscordEmbed GetTimedOutEmbed(string idOrName, bool isFirst = false)
        {
            var timedOut = new DiscordEmbedBuilder();
            timedOut.WithAuthor($"Embed configurator menu for Id: {idOrName}");
            timedOut.WithDescription(
                $"Your interaction timed out, please {(!isFirst ? "decide whether you want to save current version or abort and " : "")} try again!");
            timedOut.WithFooter($"Parent Id: {idOrName}");
            timedOut.WithColor(new DiscordColor("#26296e"));

            return timedOut.Build();
        }

        private static IEnumerable<DiscordButtonComponent> GetModuleFinalizeButtons(DiscordClient client)
        {
            var confirmBtn = new DiscordButtonComponent(ButtonStyle.Primary,
                nameof(EmbedConfigButton.EmbedConfigConfirmButton), "Confirm and save changes", false,
                new DiscordComponentEmoji(DiscordEmoji.FromName(client, ":white_check_mark:")));
            var abortBtn = new DiscordButtonComponent(ButtonStyle.Danger,
                nameof(EmbedConfigButton.EmbedConfigAbortButton), "Abort changes and finish", false,
                new DiscordComponentEmoji(DiscordEmoji.FromName(client, ":x:")));
            return new List<DiscordButtonComponent> {confirmBtn, abortBtn};
        }

        private static DiscordButtonComponent GetMainFinalizeButton(DiscordClient client)
        {
            return new(ButtonStyle.Primary, nameof(EmbedConfigButton.EmbedConfigFinalButton), "Finalize", false,
                new DiscordComponentEmoji(DiscordEmoji.FromName(client, ":white_check_mark:")));
        }
    }
}