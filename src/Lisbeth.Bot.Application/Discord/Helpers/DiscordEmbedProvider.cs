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

using DSharpPlus.Entities;
using MikyM.Discord.Interfaces;

namespace Lisbeth.Bot.Application.Discord.Helpers;

public interface IDiscordEmbedProvider
{
    DiscordEmbedBuilder ConfigureEmbed(EmbedConfig config);
    DiscordEmbed GetUnsuccessfulResultEmbed(IResult result);
    DiscordEmbed GetUnsuccessfulResultEmbed(IResultError error);
    DiscordEmbed GetUnsuccessfulResultEmbed(string error);
}

[UsedImplicitly]
public class DiscordEmbedProvider : IDiscordEmbedProvider
{
    private readonly IDiscordService _discord;

    public DiscordEmbedProvider(IDiscordService discord)
    {
        _discord = discord;
    }

    public DiscordEmbedBuilder ConfigureEmbed(EmbedConfig config)
    {
        if (config is null) throw new ArgumentNullException(nameof(config));

        var builder = new DiscordEmbedBuilder();

        if (!string.IsNullOrWhiteSpace(config.Author)) builder.WithAuthor(config.Author);
        else if (!string.IsNullOrWhiteSpace(config.Author) && !string.IsNullOrWhiteSpace(config.AuthorImageUrl))
            builder.WithAuthor(config.Author, null, config.AuthorImageUrl);

        if (!string.IsNullOrWhiteSpace(config.Footer)) builder.WithFooter(config.Footer);
        else if (!string.IsNullOrWhiteSpace(config.Footer) && !string.IsNullOrWhiteSpace(config.FooterImageUrl))
            builder.WithFooter(config.Footer, config.FooterImageUrl);

        if (!string.IsNullOrWhiteSpace(config.Description)) builder.WithDescription(config.Description);

        if (!string.IsNullOrWhiteSpace(config.ImageUrl)) builder.WithImageUrl(config.ImageUrl);

        if (!string.IsNullOrWhiteSpace(config.HexColor)) builder.WithColor(new DiscordColor(config.HexColor));

        if (config.Timestamp is not null) builder.WithTimestamp(config.Timestamp);

        if (!string.IsNullOrWhiteSpace(config.Title)) builder.WithTitle(config.Title);

        if (!string.IsNullOrWhiteSpace(config.Thumbnail))
            builder.WithThumbnail(config.Thumbnail, config.ThumbnailHeight ?? throw new InvalidOperationException(),
                config.ThumbnailWidth ?? throw new InvalidOperationException());

        if (config.Fields is null || config.Fields.Count == 0) return builder;

        foreach (var field in config.Fields.Where(field =>
                     !string.IsNullOrWhiteSpace(field.Text) && !string.IsNullOrWhiteSpace(field.Title)))
            builder.AddField(field.Title, field.Text);

        return builder;
    }

    public DiscordEmbed GetUnsuccessfulResultEmbed(IResult result)
    {
        return GetUnsuccessfulResultEmbed(result.Error ??
                                          throw new InvalidOperationException(
                                              "Given result does not contain an error"));
    }

    public DiscordEmbed GetUnsuccessfulResultEmbed(IResultError error)
    {
        return GetUnsuccessfulResultEmbed(error.Message);
    }

    public DiscordEmbed GetUnsuccessfulResultEmbed(string error)
    {
        return new DiscordEmbedBuilder().WithColor(new DiscordColor(170, 1, 20))
            .WithAuthor($"{DiscordEmoji.FromName(_discord.Client, ":x:")} Operation errored")
            .AddField("Message", error)
            .Build();
    }
}