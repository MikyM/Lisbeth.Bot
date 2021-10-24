﻿// This file is part of Lisbeth.Bot project
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
using System.Threading.Tasks;

namespace Lisbeth.Bot.Application.Discord.Services.Interfaces
{
    public interface IDiscordReminderService
    {
        Task<DiscordEmbed> SetNewReminderAsync(InteractionContext ctx);
        Task<DiscordEmbed> DisableReminderAsync(InteractionContext ctx);
        Task<DiscordEmbed> RescheduleReminderAsync(InteractionContext ctx);
    }
}