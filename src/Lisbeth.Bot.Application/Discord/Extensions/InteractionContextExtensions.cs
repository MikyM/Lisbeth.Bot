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

using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;

namespace Lisbeth.Bot.Application.Discord.Extensions;

public static class InteractionContextExtensions
{
    public static async Task ExtendedFollowUpAsync(this InteractionContext ctx, DiscordFollowupMessageBuilder builder)
    {
        await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(new DiscordEmbedBuilder()
            .WithTitle($"{DiscordEmoji.FromName(ctx.Client, ":ok_hand:")} Action successful")
            .WithColor(new DiscordColor("#2ECC71"))));

        await Task.Delay(300);

        await ctx.FollowUpAsync(builder);
    }

    public static async Task ExtendedFollowUpAsync(this InteractionContext ctx)
    {
        await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(new DiscordEmbedBuilder()
            .WithTitle($"{DiscordEmoji.FromName(ctx.Client, ":ok_hand:")} Action successful")
            .WithColor(new DiscordColor("#2ECC71"))));
    }

    public static async Task ExtendedFollowUpAsync(this InteractionContext ctx, IResult unsuccessfulResult)
    {
        await ctx.FollowUpAsync(
            new DiscordFollowupMessageBuilder().AddEmbed(GetUnsuccessfulResultEmbed(unsuccessfulResult, ctx.Client)));
    }

    public static async Task ExtendedFollowUpAsync(this InteractionContext ctx, string errorMessage)
    {
        await ctx.FollowUpAsync(
            new DiscordFollowupMessageBuilder().AddEmbed(GetUnsuccessfulResultEmbed(errorMessage, ctx.Client)));
    }

    public static async Task ExtendedFollowUpAsync(this InteractionContext ctx, IResultError error)
    {
        await ctx.FollowUpAsync(
            new DiscordFollowupMessageBuilder().AddEmbed(GetUnsuccessfulResultEmbed(error, ctx.Client)));
    }

    public static async Task RespondAsync(this InteractionContext ctx, DiscordInteractionResponseBuilder builder, bool asEphemeral = false)
        => await ctx.CreateResponseAsync(builder.AsEphemeral(asEphemeral));

    private static DiscordEmbed GetUnsuccessfulResultEmbed(IResult result, DiscordClient discord)
    {
        return GetUnsuccessfulResultEmbed(
            result.Error ?? throw new InvalidOperationException("Given result does not contain an error"), discord);
    }

    private static DiscordEmbed GetUnsuccessfulResultEmbed(IResultError error, DiscordClient discord)
    {
        return GetUnsuccessfulResultEmbed(error.Message, discord);
    }

    private static DiscordEmbed GetUnsuccessfulResultEmbed(string error, DiscordClient discord)
    {
        return new DiscordEmbedBuilder().WithColor(new DiscordColor(170, 1, 20))
            .WithAuthor($"{DiscordEmoji.FromName(discord, ":x:")} Operation errored")
            .AddField("Message", error)
            .Build();
    }
}
