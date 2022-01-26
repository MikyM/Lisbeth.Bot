// This file is part of Lisbeth.Bot project
//
// Copyright (C) 2021 VTPDevelopment - @VelvetThePanda
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
using Lisbeth.Bot.Application.Discord.EventHandlers.Base;
using MikyM.Common.Utilities;
using MikyM.Discord.Events;

namespace Lisbeth.Bot.Application.Discord.EventHandlers;

[UsedImplicitly]
public class PhishingEventHandler : BaseEventHandler, IDiscordMessageEventsSubscriber
{
    private readonly IDiscordPhishingDetectionService _phishingDetection;

    public PhishingEventHandler(IDiscordPhishingDetectionService phishingDetection, IAsyncExecutor asyncExecutor) : base(asyncExecutor)
    {
        _phishingDetection = phishingDetection;
    }

    public async Task DiscordOnMessageCreated(DiscordClient sender, MessageCreateEventArgs args)
    {
        await _phishingDetection.DetectPhishingAsync(args.Message);
    }

    public Task DiscordOnMessageAcknowledged(DiscordClient sender, MessageAcknowledgeEventArgs args)
    {
        return Task.CompletedTask;
    }

    public async Task DiscordOnMessageUpdated(DiscordClient sender, MessageUpdateEventArgs args)
    {
        await _phishingDetection.DetectPhishingAsync(args.Message);
    }

    public Task DiscordOnMessageDeleted(DiscordClient sender, MessageDeleteEventArgs args)
    {
        return Task.CompletedTask;
    }

    public Task DiscordOnMessagesBulkDeleted(DiscordClient sender, MessageBulkDeleteEventArgs args)
    {
        return Task.CompletedTask;
    }
}