﻿// This file is part of Lisbeth.Bot project
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

using DSharpPlus.EventArgs;
using Lisbeth.Bot.Application.Discord.Helpers.InteractionIdEnums.Buttons;
using Lisbeth.Bot.Application.Discord.Helpers.InteractionIdEnums.Selects;
using MikyM.Discord.Events;

namespace Lisbeth.Bot.Application.Discord.EventHandlers;

[UsedImplicitly]
public class EmbedConfigEventsHandler : IDiscordMiscEventsSubscriber
{
    public async Task DiscordOnComponentInteractionCreated(DiscordClient sender,
        ComponentInteractionCreateEventArgs args)
    {
        switch (args.Id)
        {
            case nameof(EmbedConfigSelect.EmbedConfigMainSelect):
            case nameof(EmbedConfigButton.EmbedConfigConfirm):
            case nameof(EmbedConfigButton.EmbedConfigAbort):
            case nameof(EmbedConfigButton.EmbedConfigFinal):
            case nameof(EmbedConfigButton.EmbedConfigContinue):
                await args.Interaction.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate);
                break;
        }
    }

    public Task DiscordOnClientErrored(DiscordClient sender, ClientErrorEventArgs args)
    {
        return Task.CompletedTask;
    }
}
