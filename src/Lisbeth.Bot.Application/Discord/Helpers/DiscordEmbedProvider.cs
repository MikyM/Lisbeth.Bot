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
using Lisbeth.Bot.Domain.DTOs.Request.Base;
using Lisbeth.Bot.Domain.Entities.Base;
using MikyM.Discord.EmbedBuilders.Enums;
using MikyM.Discord.Extensions.BaseExtensions;
using MikyM.Discord.Interfaces;
using System.Globalization;
using MikyM.Discord.Enums;

namespace Lisbeth.Bot.Application.Discord.Helpers;

public interface IDiscordEmbedProvider
{
    DiscordEmbedBuilder GetEmbedFromConfig(EmbedConfig config);
    DiscordEmbedBuilder GetUnsuccessfulActionEmbed(IResult result);
    DiscordEmbedBuilder GetUnsuccessfulActionEmbed(IResultError error);
    DiscordEmbedBuilder GetUnsuccessfulActionEmbed(string error);
    DiscordEmbedBuilder GetActionTimedOutEmbed();
    DiscordEmbedBuilder GetEmbedResponseFrom(IApplyInfractionReq req, DiscordMember target, long? caseId = null,
        string hexColor = "#26296e", IModEntity? previous = null);
    DiscordEmbedBuilder GetEmbedResponseFrom(IModEntity entity, DiscordMember target,
        long? caseId = null, string hexColor = "#26296e");
    DiscordEmbedBuilder GetEmbedResponseFrom(IRevokeInfractionReq req, DiscordMember target,
        long? caseId = null, string hexColor = "#26296e");
    DiscordEmbedBuilder GetModerationEmbedLogFrom(IGetInfractionReq req, DiscordMember moderator,
        long? caseId = null, string hexColor = "#26296e");
    DiscordEmbedBuilder GetModerationEmbedLogFrom(IApplyInfractionReq req, DiscordMember moderator,
        long? caseId = null, string hexColor = "#26296e");
    DiscordEmbedBuilder GetModerationEmbedLogFrom(IRevokeInfractionReq req, DiscordMember moderator,
        long? caseId = null, string hexColor = "#26296e");
}

[UsedImplicitly]
public class DiscordEmbedProvider : IDiscordEmbedProvider
{
    private readonly IDiscordService _discord;

    public DiscordEmbedProvider(IDiscordService discord)
    {
        _discord = discord;
    }

    public DiscordEmbedBuilder GetEmbedFromConfig(EmbedConfig config)
    {
        if (config is null) throw new ArgumentNullException(nameof(config));

        var builder = new DiscordEmbedBuilder();

        if (!string.IsNullOrWhiteSpace(config.Author))
            builder.WithAuthor(config.Author, null, config.AuthorImageUrl);

        if (!string.IsNullOrWhiteSpace(config.Footer))
            builder.WithFooter(config.Footer, config.FooterImageUrl);

        if (!string.IsNullOrWhiteSpace(config.Description))
            builder.WithDescription(config.Description);

        if (!string.IsNullOrWhiteSpace(config.ImageUrl)) 
            builder.WithImageUrl(config.ImageUrl);

        if (!string.IsNullOrWhiteSpace(config.HexColor))
            builder.WithColor(new DiscordColor(config.HexColor));

        if (config.Timestamp is not null)
            builder.WithTimestamp(config.Timestamp);

        if (!string.IsNullOrWhiteSpace(config.Title)) 
            builder.WithTitle(config.Title);

        if (!string.IsNullOrWhiteSpace(config.Thumbnail))
            builder.WithThumbnail(config.Thumbnail, config.ThumbnailHeight ?? 0,
                config.ThumbnailWidth ?? 0);

        if (config.Fields is null || config.Fields.Count == 0) return builder;

        foreach (var field in config.Fields.Where(field =>
                     !string.IsNullOrWhiteSpace(field.Text) && !string.IsNullOrWhiteSpace(field.Title)))
            builder.AddField(field.Title, field.Text);

        return builder;
    }

    public DiscordEmbedBuilder GetUnsuccessfulActionEmbed(IResult result)
    {
        return this.GetUnsuccessfulActionEmbed(result.Error ??
                                          throw new InvalidOperationException(
                                              "Given result does not contain an error"));
    }

    public DiscordEmbedBuilder GetUnsuccessfulActionEmbed(IResultError error)
    {
        return this.GetUnsuccessfulActionEmbed(error.Message);
    }

    public DiscordEmbedBuilder GetUnsuccessfulActionEmbed(string error)
    {
        return new DiscordEmbedBuilder().WithColor(new DiscordColor(170, 1, 20))
            .WithTitle(
                $"{DiscordEmoji.FromName(_discord.Client, ":x:")} Action errored")
            .AddField("Message", error);
    }

    public DiscordEmbedBuilder GetActionTimedOutEmbed()
    {
        var embed = new DiscordEmbedBuilder();
        embed.WithTitle("Action timed out");
        embed.WithDescription(
            $"Your interaction timed out, please try again!");
        embed.WithFooter("");
        embed.WithColor(new DiscordColor("#26296e"));

        return embed;
    }

    public DiscordEmbedBuilder GetSuccessfulActionEmbed()
    {
        var embed = new DiscordEmbedBuilder();
        embed.WithTitle("Action succeeded");
        embed.WithDescription(
            $"Your interaction timed out, please try again!");
        embed.WithFooter("");
        embed.WithColor(new DiscordColor("#26296e"));

        return embed;
    }

    public DiscordEmbedBuilder GetEmbedResponseFrom(IApplyInfractionReq req, DiscordMember target, long? caseId = null,
        string hexColor = "#26296e", IModEntity? previous = null)
    {
        var data = GetUnderlyingNameAndPastTense(req);

        bool isOverlapping = previous is not null && previous.AppliedUntil > req.AppliedUntil && !previous.IsDisabled;

        var embed = new DiscordEmbedBuilder();
        embed.WithColor(new DiscordColor(hexColor));
        embed.WithAuthor(
            $" {(previous is not null && !isOverlapping ? "Extend " : "")}{data.Name} {(previous is not null && isOverlapping ? "failed " : "")}| {target.GetFullDisplayName()}",
            null, target.AvatarUrl);

        if (previous is not null)
        {
            if (isOverlapping)
            {
                embed.WithDescription(
                    $"This user has already been {data.PastTense.ToLower()} until {previous.AppliedUntil} by {ExtendedFormatter.Mention(req.RequestedOnBehalfOfId, DiscordEntity.User)}");
                embed.WithFooter($"Previous case Id: {previous.Id} | Member Id: {previous.UserId}");
                return embed;
            }

            embed.AddField($"Previous {data.Name.ToLower()} until", previous.AppliedUntil.ToString(CultureInfo.CurrentCulture), true);
            embed.AddField("Previous moderator",
                $"{ExtendedFormatter.Mention(previous.AppliedById, DiscordEntity.User)}", true);
            embed.AddField("Previous reason", previous.Reason, true);
        }

        embed.AddField("User mention", target.Mention, true);
        embed.AddField("Moderator", ExtendedFormatter.Mention(req.RequestedOnBehalfOfId, DiscordEntity.Member),
            true);

        TimeSpan duration = req.AppliedUntil.Subtract(DateTime.UtcNow);
        string lengthString = req.AppliedUntil == DateTime.MaxValue
            ? "Permanent"
            : $"{duration.Days} days, {duration.Hours} hrs, {duration.Minutes} mins";

        embed.AddField("Length", lengthString, true);
        embed.AddField($"{data.PastTense} until", req.AppliedUntil.ToString(CultureInfo.CurrentCulture), true);
        embed.AddField("Reason", req.Reason);
        embed.WithFooter($"Case Id: {(caseId is null ? "Unknown" : caseId)} | Member Id: {req.TargetUserId}");

        return embed;
    }

    public DiscordEmbedBuilder GetEmbedResponseFrom(IModEntity entity, DiscordMember target,
        long? caseId = null, string hexColor = "#26296e")
    {
        var data = GetUnderlyingNameAndPastTense(entity);

        var embed = new DiscordEmbedBuilder();
        embed.WithColor(new DiscordColor(hexColor));

        embed.WithAuthor($"{data.Name} Info | {target.GetFullDisplayName()}", null, target.AvatarUrl);
        embed.AddField("User mention", target.Mention, true);
        embed.AddField("Moderator", ExtendedFormatter.Mention(entity.AppliedById, DiscordEntity.User), true);
        embed.AddField($"{data.PastTense} until", entity.AppliedUntil.ToString(), true);
        embed.AddField("Reason", entity.Reason);

        return embed;
    }

    public DiscordEmbedBuilder GetEmbedResponseFrom(IRevokeInfractionReq req, DiscordMember target,
        long? caseId = null, string hexColor = "#26296e")
    {
        var data = GetUnderlyingNameAndPastTense(req);

        var embed = new DiscordEmbedBuilder();
        embed.WithColor(new DiscordColor(hexColor));

        embed.WithAuthor($"Un{data.Name.ToLower()} | {target.GetFullDisplayName()}", null, target.AvatarUrl);
        embed.AddField("Moderator", ExtendedFormatter.Mention(req.RequestedOnBehalfOfId, DiscordEntity.User), true);
        embed.AddField("User mention", target.Mention, true);
        embed.WithDescription($"Successfully un{data.PastTense.ToLower()}");
        embed.WithFooter($"Case Id: {(caseId is null ? "Unknown" : caseId)} | Member ID: {target.Id}");

        return embed;
    }

    public DiscordEmbedBuilder GetModerationEmbedLogFrom(IRevokeInfractionReq req, DiscordMember moderator,
        long? caseId = null, string hexColor = "#26296e")
    {
        var data = GetUnderlyingNameAndPastTense(req);
        var target = ExtendedFormatter.Mention(req.TargetUserId, DiscordEntity.User);

        var embed = new DiscordEmbedBuilder();
        embed.WithColor(new DiscordColor(hexColor));

        embed.WithAuthor("Moderation log", null, moderator.AvatarUrl);
        embed.AddField("Moderator", ExtendedFormatter.Mention(req.RequestedOnBehalfOfId, DiscordEntity.User), true);
        embed.AddField("Action", $"Un{data.Name}", true);
        embed.AddField("Target", target, true);
        embed.WithFooter($"Case Id: {(caseId is null ? "Unknown" : caseId)}");

        return embed;
    }

    public DiscordEmbedBuilder GetModerationEmbedLogFrom(IApplyInfractionReq req, DiscordMember moderator,
        long? caseId = null, string hexColor = "#26296e")
    {
        var data = GetUnderlyingNameAndPastTense(req);

        var embed = new DiscordEmbedBuilder();
        embed.WithColor(new DiscordColor(hexColor));

        TimeSpan duration = req.AppliedUntil.Subtract(DateTime.UtcNow);
        string lengthString = req.AppliedUntil == DateTime.MaxValue
            ? "Permanent"
            : $"{duration.Days} days, {duration.Hours} hrs, {duration.Minutes} mins";

        embed.WithAuthor("Moderation log", null, moderator.AvatarUrl);
        embed.AddField("Moderator", ExtendedFormatter.Mention(req.RequestedOnBehalfOfId, DiscordEntity.User), true);
        embed.AddField("Action", data.Name, true);
        embed.AddField("Target", ExtendedFormatter.Mention(req.TargetUserId, DiscordEntity.Member), true);
        embed.AddField("Length", lengthString, true);
        embed.AddField($"{data.PastTense} until", req.AppliedUntil.ToString(CultureInfo.CurrentCulture), true);
        embed.AddField("Reason", req.Reason, true);
        embed.WithFooter($"Case Id: {(caseId is null ? "Unknown" : caseId)}");

        return embed;
    }

    public DiscordEmbedBuilder GetModerationEmbedLogFrom(IGetInfractionReq req, DiscordMember moderator,
        long? caseId = null, string hexColor = "#26296e")
    {
        var data = GetUnderlyingNameAndPastTense(req);

        var embed = new DiscordEmbedBuilder();
        embed.WithColor(new DiscordColor(hexColor));

        embed.WithAuthor("Moderation log", null, moderator.AvatarUrl);
        embed.AddField("Moderator", ExtendedFormatter.Mention(req.RequestedOnBehalfOfId, DiscordEntity.User), true);
        embed.AddField("Action", $"Get {data.Name.ToLower()}", true);

        string reqParams = req.ToString();
        if (reqParams.Length <= 4096) embed.WithDescription(reqParams);

        embed.WithFooter($"Case Id: {(caseId is null ? "Unknown" : caseId)}");

        return embed;
    }

    private static (string Name, string PastTense) GetUnderlyingNameAndPastTense(object req)
    {
        string type = req.GetType().Name;
        if (type.Contains("Ban")) return ("Ban", "Banned");
        if (type.Contains("Mute")) return ("Mute", "Muted");

        throw new NotSupportedException();
    }
}