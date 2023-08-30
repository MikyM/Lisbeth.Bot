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

using DSharpPlus.SlashCommands.Attributes;
using Lisbeth.Bot.Application.Discord.SlashCommands.Base;
using Lisbeth.Bot.Domain.DTOs.Request.Prune;

// ReSharper disable once CheckNamespace
namespace Lisbeth.Bot.Application.Discord.ApplicationCommands;

[SlashModuleLifespan(SlashModuleLifespan.Scoped)]
[UsedImplicitly]
public partial class PruneApplicationCommands : ExtendedApplicationCommandModule
{
    public PruneApplicationCommands(IDiscordMessageService discordMessageService)
    {
        _discordMessageService = discordMessageService;
    }

    private readonly IDiscordMessageService _discordMessageService;

    [UsedImplicitly]
    [SlashRequireUserPermissions(Permissions.ManageMessages)]
    [SlashCommand("prune", "Command that allows pruning messages", false)]
    public async Task PruneCommand(InteractionContext ctx,
        [Option("count", "Number of messages to prune")]
        long count,
        [Option("user", "User to target prune at")]
        DiscordUser? user = null,
        [Option("id", "Message id to prune to")]
        long id = 0,
        [Option("reason", "Reason for ban")] string reason = "No reason provided")
    {
        await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource,
            new DiscordInteractionResponseBuilder().AsEphemeral());

        if (count > 99) count = 99;

        Result<DiscordEmbed>? result = null;
        switch (user)
        {
            case null:
                switch (id)
                {
                    case 0:
                        var reqNoUsNoMsgId = new PruneReqDto(ctx.Guild.Id, ctx.Channel.Id, ctx.Member.Id, (int)count);
                        result = await _discordMessageService.PruneAsync(ctx, reqNoUsNoMsgId);
                        break;
                    default:
                        var reqNoUsWithMsgId =
                            new PruneReqDto(ctx.Guild.Id, ctx.Channel.Id, ctx.Member.Id, null, (ulong)id);
                        result = await _discordMessageService.PruneAsync(ctx, reqNoUsWithMsgId);
                        break;
                }

                break;
            case not null:
                switch (id)
                {
                    case 0:
                        var reqWithUsNoMsgId = new PruneReqDto(ctx.Guild.Id, ctx.Channel.Id, ctx.Member.Id, null, null,
                            user.Id);
                        result = await _discordMessageService.PruneAsync(ctx, reqWithUsNoMsgId);
                        break;
                    default:
                        throw new ArgumentException(nameof(id));
                }
                break;
        }


        if (!result.Value.IsDefined(out var embed))
            await ctx.Interaction.CreateFollowupMessageAsync(new DiscordFollowupMessageBuilder().AddEmbed(GetUnsuccessfulResultEmbed(result, ctx.Client))
                .AsEphemeral());
        else
            await ctx.Interaction.CreateFollowupMessageAsync(new DiscordFollowupMessageBuilder().AddEmbed(embed)
                .AsEphemeral());
    }
}
