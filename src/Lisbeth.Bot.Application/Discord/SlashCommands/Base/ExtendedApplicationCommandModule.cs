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
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using MikyM.Common.Application.Results;

namespace Lisbeth.Bot.Application.Discord.SlashCommands.Base
{
    public class ExtendedApplicationCommandModule : ApplicationCommandModule
    {
        protected DiscordEmbed GetUnsuccessfulResultEmbed(IResult result, DiscordClient discord)
        {
            return GetUnsuccessfulResultEmbed(
                result.Error ?? throw new InvalidOperationException("Given result does not contain an error"), discord);
        }

        protected DiscordEmbed GetUnsuccessfulResultEmbed(IResultError error, DiscordClient discord)
        {
            return GetUnsuccessfulResultEmbed(error.Message, discord);
        }

        protected DiscordEmbed GetUnsuccessfulResultEmbed(string error, DiscordClient discord)
        {
            return new DiscordEmbedBuilder().WithColor(new DiscordColor(170, 1, 20))
                .WithAuthor($"{DiscordEmoji.FromName(discord, ":x:")} Operation errored")
                .AddField("Message", error)
                .Build();
        }
    }
}