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
using Lisbeth.Bot.Domain.Entities;
using System;
using System.Threading.Tasks;

namespace Lisbeth.Bot.Application.Discord.SlashCommands
{
    [UsedImplicitly]
    [SlashModuleLifespan(SlashModuleLifespan.Transient)]
    public class EmbedConfigSlashCommands : ApplicationCommandModule
    {
        public IDiscordEmbedConfiguratorService<Tag> _embedConfigService { private get; set; }

        [SlashCommand("test", "something")]
        public async Task EmbedConfigCommand(InteractionContext ctx, [Option("target", "The id of a reminder, tag or role menu to create embed for,")] string id)
        {
            if (!long.TryParse(id, out long parsedId)) throw new ArgumentException(nameof(id));

            await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);

            var result = await _embedConfigService.ConfigureAsync(ctx, parsedId.ToString());

            await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(result.Embed));
        }
    }
}
