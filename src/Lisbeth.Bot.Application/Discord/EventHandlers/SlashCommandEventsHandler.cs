// This file is part of Lisbeth.Bot project
//
// Copyright (C) 2021 MikyM
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
using DSharpPlus.SlashCommands;
using DSharpPlus.SlashCommands.EventArgs;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;
using MikyM.Discord.Extensions.SlashCommands.Events;
using System.Threading.Tasks;

namespace Lisbeth.Bot.Application.Discord.EventHandlers
{
    [UsedImplicitly]
    public class SlashCommandEventsHandler : IDiscordSlashCommandsEventsSubscriber
    {
        private readonly ILogger<SlashCommandEventsHandler> _logger;

        public SlashCommandEventsHandler(ILogger<SlashCommandEventsHandler> logger)
        {
            _logger = logger;
        }

        public Task SlashCommandsOnContextMenuErrored(SlashCommandsExtension sender, ContextMenuErrorEventArgs args)
        {
            _logger.LogError(args.Exception.ToString());
            var noEntryEmoji = DiscordEmoji.FromName(sender.Client, ":x:");
            var embed = new DiscordEmbedBuilder();
            embed.WithColor(new DiscordColor(170, 1, 20));
            embed.WithAuthor($"{noEntryEmoji} Context menu errored");
            embed.AddField("Type", args.Exception.GetType().ToString());
            embed.AddField("Message", args.Exception.Message);
            args.Context.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(embed.Build()));
            return Task.CompletedTask;
        }

        public Task SlashCommandsOnContextMenuExecuted(SlashCommandsExtension sender, ContextMenuExecutedEventArgs args)
        {
            return Task.CompletedTask;
        }

        public Task SlashCommandsOnSlashCommandErrored(SlashCommandsExtension sender, SlashCommandErrorEventArgs args)
        {
            _logger.LogError(args.Exception.ToString());
            var noEntryEmoji = DiscordEmoji.FromName(sender.Client, ":x:");
            var embed = new DiscordEmbedBuilder();
            embed.WithColor(new DiscordColor(170, 1, 20));
            embed.WithAuthor($"{noEntryEmoji} Command errored");
            embed.AddField("Type", args.Exception.GetType().ToString());
            embed.AddField("Message", args.Exception.Message);
            args.Context.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(embed.Build()));
            return Task.CompletedTask;
        }

        public Task SlashCommandsOnSlashCommandExecuted(SlashCommandsExtension sender, SlashCommandExecutedEventArgs args)
        {
            return Task.CompletedTask;
        }
    }
}
