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

using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using DSharpPlus.SlashCommands.Attributes;
using JetBrains.Annotations;

namespace Lisbeth.Bot.Application.Discord.SlashCommands
{
    [SlashCommandGroup("ticket", "Ticket commands")]
    [SlashModuleLifespan(SlashModuleLifespan.Transient)]
    [UsedImplicitly]
    public class TicketSlashCommands : ApplicationCommandModule
    {
        //public ITicketService _service { private get; set; }

        [SlashRequireUserPermissions(Permissions.BanMembers)]
        [SlashCommand("add", "A command that allows a certain user or a role to see the ticket")]
        public async Task AddCommand(InteractionContext ctx, [Option("target", "A user or a role to add")] SnowflakeObject target)
        {
        }

        [SlashRequireUserPermissions(Permissions.BanMembers)]
        [SlashCommand("remove", "A command that removes a certain user or a role from seeing the ticket")]
        public async Task RemoveCommand(InteractionContext ctx, [Option("target", "A user or a role to remove")] SnowflakeObject target)
        {
        }
    }
}