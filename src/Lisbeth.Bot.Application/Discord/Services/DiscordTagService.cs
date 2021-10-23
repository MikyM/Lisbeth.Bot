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
using Lisbeth.Bot.Application.Discord.Exceptions;
using Lisbeth.Bot.Application.Discord.Services.Interfaces;
using Lisbeth.Bot.Application.Services.Interfaces;
using Lisbeth.Bot.DataAccessLayer.Specifications.GuildSpecifications;
using Lisbeth.Bot.Domain.DTOs.Request;
using Lisbeth.Bot.Domain.Entities;
using MikyM.Common.DataAccessLayer.Specifications;
using MikyM.Discord.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace Lisbeth.Bot.Application.Discord.Services
{
    public class DiscordTagService : IDiscordTagService
    {
        private readonly IDiscordService _discord;
        private readonly IGuildService _guildService;
        private readonly ITagService _tagService;

        public DiscordTagService(IDiscordService discord, IGuildService guildService, ITagService tagService)
        {
            _discord = discord;
            _guildService = guildService;
            _tagService = tagService;
        }

        public async Task<DiscordEmbed> AddAsync(TagAddReqDto req)
        {
            if (req is null) throw new ArgumentNullException(nameof(req));

            DiscordGuild guild = await _discord.Client.GetGuildAsync(req.GuildId);
            DiscordMember creator = await guild.GetMemberAsync(req.UserId);

            return await AddAsync(guild, creator, req);
        }

        public async Task<DiscordEmbed> AddAsync(InteractionContext ctx, TagAddReqDto req)
        {
            if (ctx is null) throw new ArgumentNullException(nameof(ctx));
            if (req is null) throw new ArgumentNullException(nameof(req));

            return await AddAsync(ctx.Guild, ctx.Member, req);
        }

        private async Task<DiscordEmbed> AddAsync(DiscordGuild guild, DiscordMember creator, TagAddReqDto req)
        {
            if (guild is null) throw new ArgumentNullException(nameof(guild));
            if (creator is null) throw new ArgumentNullException(nameof(creator));
            if (req is null) throw new ArgumentNullException(nameof(req));

            var guildRes =
                await _guildService.GetBySpecificationsAsync<Guild>(
                    new Specifications<Guild>(x => x.GuildId == guild.Id && !x.IsDisabled));
            var guildCfg = guildRes.FirstOrDefault();
            if (guildCfg is null)
                throw new ArgumentException($"Guild with Id: {guild.Id} doesn't exist in the database.");

            if (!creator.Roles.Any(x => x.Permissions.HasPermission(Permissions.BanMembers)))
                throw new DiscordNotAuthorizedException("You are not authorized to create tags");

            await _tagService.AddAsync(req, true);

            var embed = new DiscordEmbedBuilder();
            embed.WithColor(new DiscordColor(guildCfg.EmbedHexColor));
            embed.WithDescription("Tag added successfully");

            return embed.Build();
        }

        public async Task<DiscordEmbed> EditAsync(TagEditReqDto req)
        {
            if (req is null) throw new ArgumentNullException(nameof(req));

            DiscordGuild guild = await _discord.Client.GetGuildAsync(req.GuildId);
            DiscordMember requestingUser = await guild.GetMemberAsync(req.RequestedOnBehalfOfId);

            return await EditAsync(guild, requestingUser, req);
        }

        public async Task<DiscordEmbed> EditAsync(InteractionContext ctx, TagEditReqDto req)
        {
            if (ctx is null) throw new ArgumentNullException(nameof(ctx));
            if (req is null) throw new ArgumentNullException(nameof(req));

            return await EditAsync(ctx.Guild, ctx.Member, req);
        }

        private async Task<DiscordEmbed> EditAsync(DiscordGuild guild, DiscordMember requestingUser, TagEditReqDto req)
        {
            if (guild is null) throw new ArgumentNullException(nameof(guild));
            if (requestingUser is null) throw new ArgumentNullException(nameof(requestingUser));
            if (req is null) throw new ArgumentNullException(nameof(req));

            var guildRes =
                await _guildService.GetBySpecificationsAsync<Guild>(new ActiveGuildByDiscordIdWithTagsSpecifications(guild.Id));
            var guildCfg = guildRes.FirstOrDefault();
            if (guildCfg is null)
                throw new ArgumentException($"Guild with Id: {guild.Id} doesn't exist in the database.");

            if (!requestingUser.Roles.Any(x => x.Permissions.HasPermission(Permissions.BanMembers)))
                throw new DiscordNotAuthorizedException("You are not authorized to edit tags");

            await _tagService.UpdateTagEmbedConfigAsync(req, true);

            var embed = new DiscordEmbedBuilder();
            embed.WithColor(new DiscordColor(guildCfg.EmbedHexColor));
            embed.WithDescription("Tag edited successfully");

            return embed.Build();
        }

        public async Task<DiscordEmbed> GetAsync(TagGetReqDto req)
        {
            if (req is null) throw new ArgumentNullException(nameof(req));

            DiscordGuild guild = await _discord.Client.GetGuildAsync(req.GuildId);
            DiscordMember requestingUser = await guild.GetMemberAsync(req.RequestedOnBehalfOfId);

            return await GetAsync(guild, requestingUser, req);
        }

        public async Task<DiscordEmbed> GetAsync(InteractionContext ctx, TagGetReqDto req)
        {
            if (ctx is null) throw new ArgumentNullException(nameof(ctx));
            if (req is null) throw new ArgumentNullException(nameof(req));

            return await GetAsync(ctx.Guild, ctx.Member, req);
        }

        private async Task<DiscordEmbed> GetAsync(DiscordGuild guild, DiscordMember requestingUser, TagGetReqDto req)
        {
            if (guild is null) throw new ArgumentNullException(nameof(guild));
            if (requestingUser is null) throw new ArgumentNullException(nameof(requestingUser));
            if (req is null) throw new ArgumentNullException(nameof(req));

            var guildRes =
                await _guildService.GetBySpecificationsAsync<Guild>(new ActiveGuildByDiscordIdWithTagsSpecifications(guild.Id));
            var guildCfg = guildRes.FirstOrDefault();
            if (guildCfg is null)
                throw new ArgumentException($"Guild with Id: {guild.Id} doesn't exist in the database.");

            if (requestingUser.Guild.Id != guild.Id) throw new DiscordNotAuthorizedException();

            var tag = req.Id.HasValue ? guildCfg.Tags.FirstOrDefault(x => x.Id == req.Id) : guildCfg.Tags.FirstOrDefault(x => x.Name == req.Name);

            if (tag is null) throw new ArgumentException("Tag not found");
            if (tag.IsDisabled) throw new ArgumentException("Found tag is disabled");
            if (tag.EmbedConfig is null) throw new ArgumentNullException(nameof(tag.EmbedConfig));

            var embed = new DiscordEmbedBuilder();
            embed.WithColor(new DiscordColor(guildCfg.EmbedHexColor));
            if (tag.EmbedConfig.Description is not null && tag.EmbedConfig.Description != "") embed.WithDescription(tag.EmbedConfig.Description);
            if (tag.EmbedConfig.Author is not null && tag.EmbedConfig.Author != "") embed.WithAuthor(tag.EmbedConfig.Author, null, tag.EmbedConfig.AuthorImageUrl);
            if (tag.EmbedConfig.Footer is not null && tag.EmbedConfig.Footer != "") embed.WithFooter(tag.EmbedConfig.Footer, tag.EmbedConfig.FooterImageUrl);
            if (tag.EmbedConfig.ImageUrl is not null && tag.EmbedConfig.ImageUrl != "") embed.WithImageUrl(tag.EmbedConfig.ImageUrl);

            var fields = JsonSerializer.Deserialize<Dictionary<string, string>>(tag.EmbedConfig.Fields);

            if (fields is null || fields.Count == 0) return embed.Build();
            
            int i = 1;
            foreach (var (title, field) in fields)
            {
                if (i >= 25) break;
                embed.AddField(title, field);
                i++;
            }

            return embed.Build();
        }

        public async Task<DiscordEmbed> DisableAsync(TagDisableReqDto req)
        {
            if (req is null) throw new ArgumentNullException(nameof(req));

            DiscordGuild guild = await _discord.Client.GetGuildAsync(req.GuildId);
            DiscordMember requestingUser = await guild.GetMemberAsync(req.RequestedOnBehalfOfId);

            return await DisableAsync(guild, requestingUser, req);
        }

        public async Task<DiscordEmbed> DisableAsync(InteractionContext ctx, TagDisableReqDto req)
        {
            if (ctx is null) throw new ArgumentNullException(nameof(ctx));
            if (req is null) throw new ArgumentNullException(nameof(req));

            return await DisableAsync(ctx.Guild, ctx.Member, req);
        }

        private async Task<DiscordEmbed> DisableAsync(DiscordGuild guild, DiscordMember requestingUser, TagDisableReqDto req)
        {
            if (guild is null) throw new ArgumentNullException(nameof(guild));
            if (requestingUser is null) throw new ArgumentNullException(nameof(requestingUser));
            if (req is null) throw new ArgumentNullException(nameof(req));

            var guildRes =
                await _guildService.GetBySpecificationsAsync<Guild>(new ActiveGuildByDiscordIdWithTagsSpecifications(guild.Id));
            var guildCfg = guildRes.FirstOrDefault();
            if (guildCfg is null)
                throw new ArgumentException($"Guild with Id: {guild.Id} doesn't exist in the database.");

            if (!requestingUser.Roles.Any(x => x.Permissions.HasPermission(Permissions.BanMembers)))
                throw new DiscordNotAuthorizedException("You are not authorized to edit tags");

            await _tagService.DisableAsync(req, true);

            var embed = new DiscordEmbedBuilder();
            embed.WithColor(new DiscordColor(guildCfg.EmbedHexColor));
            embed.WithDescription("Tag disabled successfully");

            return embed.Build();
        }
    }
}
