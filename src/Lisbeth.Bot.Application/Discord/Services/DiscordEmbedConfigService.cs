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
using DSharpPlus.SlashCommands;
using Lisbeth.Bot.Application.Discord.Services.Interfaces;
using System.Threading.Tasks;

namespace Lisbeth.Bot.Application.Discord.Services
{
    public class DiscordEmbedConfigService : IDiscordEmbedConfigService
    {
        public Task<DiscordEmbed> ConfigureNewEmbedAsync(InteractionContext ctx)
        {
            throw new System.NotImplementedException();
        }

        public Task<DiscordEmbed> EditExistingEmbedAsync(InteractionContext ctx)
        {
            throw new System.NotImplementedException();
        }
    }
}
