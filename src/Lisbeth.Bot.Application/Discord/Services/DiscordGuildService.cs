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

using DSharpPlus.EventArgs;
using JetBrains.Annotations;
using Lisbeth.Bot.Application.Discord.Helpers;
using Lisbeth.Bot.Application.Discord.Services.Interfaces;
using Lisbeth.Bot.Application.Services.Interfaces.Database;
using Lisbeth.Bot.DataAccessLayer.Specifications.Guild;
using Lisbeth.Bot.Domain.Entities;
using System.Threading.Tasks;

namespace Lisbeth.Bot.Application.Discord.Services
{
    [UsedImplicitly]
    public class DiscordGuildService : IDiscordGuildService
    {
        private readonly IGuildService _guildService;
        private readonly IDiscordEmbedProvider _embedProvider;
        private readonly IEmbedConfigService _embedConfigService;

        public DiscordGuildService(IGuildService guildService, IDiscordEmbedProvider embedProvider, IEmbedConfigService embedConfigService)
        {
            _guildService = guildService;
            _embedProvider = embedProvider;
            _embedConfigService = embedConfigService;
        }

        public async Task HandleGuildCreateAsync(GuildCreateEventArgs args)
        {
            var guild = await _guildService.GetSingleBySpecAsync<Guild>(new GuildByIdSpec(args.Guild.Id));

            if (guild is null)
            {
                await _guildService.AddAsync(new Guild {GuildId = args.Guild.Id, UserId = args.Guild.OwnerId}, true);
                var embed = await _embedConfigService.GetAsync<EmbedConfig>(1);
                await args.Guild.Owner.SendMessageAsync(_embedProvider.ConfigureEmbed(embed).Build());
            }
            else
            {
                _guildService.BeginUpdate(guild);
                guild.IsDisabled = false;
                await _guildService.CommitAsync();

                var embed = await _embedConfigService.GetAsync<EmbedConfig>(2);
                await args.Guild.Owner.SendMessageAsync(_embedProvider.ConfigureEmbed(embed).Build());
            }
        }

        public async Task HandleGuildDeleteAsync(GuildDeleteEventArgs args)
        {
            var guild = await _guildService.GetSingleBySpecAsync<Guild>(new GuildByIdSpec(args.Guild.Id));

            if (guild is not null) await _guildService.DisableAsync(guild, true);
        }
    }
}
