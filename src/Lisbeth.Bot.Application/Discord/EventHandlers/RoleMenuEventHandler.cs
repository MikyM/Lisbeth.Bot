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
using DSharpPlus.EventArgs;
using Lisbeth.Bot.Application.Discord.Helpers.InteractionIdEnums.Buttons;
using Lisbeth.Bot.Application.Discord.Services.Interfaces;
using Lisbeth.Bot.Application.Helpers;
using MikyM.Discord.Events;
using System.Threading.Tasks;

namespace Lisbeth.Bot.Application.Discord.EventHandlers
{
    public class RoleMenuEventHandler : IDiscordMiscEventsSubscriber
    {
        private readonly IAsyncExecutor _asyncExecutor;

        public RoleMenuEventHandler(IAsyncExecutor asyncExecutor)
        {
            _asyncExecutor = asyncExecutor;
        }

        public async Task DiscordOnComponentInteractionCreated(DiscordClient sender, ComponentInteractionCreateEventArgs args)
        {
            switch (args.Id)
            {
                case nameof(RoleMenuButton.RoleMenuFinalizeButton):
                case nameof(RoleMenuButton.RoleMenuAddOptionButton):
                    await args.Interaction.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate);
                    break;
            }

            if (args.Id.Contains("role_menu_"))
            {
                _ = _asyncExecutor.ExecuteAsync<IDiscordRoleMenuService>(async x =>
                    await x.HandleOptionSelectionAsync(args));
            }
        }

        public Task DiscordOnClientErrored(DiscordClient sender, ClientErrorEventArgs args)
        {
            return Task.CompletedTask;
        }
    }
}
