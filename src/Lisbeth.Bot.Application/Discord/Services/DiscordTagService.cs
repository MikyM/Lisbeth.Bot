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
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using JetBrains.Annotations;
using Lisbeth.Bot.Application.Discord.Exceptions;
using Lisbeth.Bot.Application.Discord.Extensions;
using Lisbeth.Bot.Application.Discord.Helpers;
using Lisbeth.Bot.Application.Discord.Services.Interfaces;
using Lisbeth.Bot.Application.Exceptions;
using Lisbeth.Bot.Application.Services.Interfaces.Database;
using Lisbeth.Bot.DataAccessLayer.Specifications.Guild;
using Lisbeth.Bot.Domain.DTOs.Request;
using Lisbeth.Bot.Domain.Entities;
using MikyM.Common.DataAccessLayer.Specifications;
using MikyM.Discord.Interfaces;

namespace Lisbeth.Bot.Application.Discord.Services
{
    [UsedImplicitly]
    public class DiscordTagService : IDiscordTagService
    {
        private readonly IDiscordService _discord;
        private readonly IDiscordEmbedProvider _embedProvider;
        private readonly IGuildService _guildService;
        private readonly ITagService _tagService;

        public DiscordTagService(IDiscordService discord, IGuildService guildService, ITagService tagService,
            IDiscordEmbedProvider embedProvider)
        {
            _discord = discord;
            _guildService = guildService;
            _tagService = tagService;
            _embedProvider = embedProvider;
        }

        public async Task<DiscordEmbed> AddAsync(TagAddReqDto req)
        {
            if (req is null) throw new ArgumentNullException(nameof(req));

            DiscordGuild guild = await _discord.Client.GetGuildAsync(req.GuildId);
            DiscordMember creator = await guild.GetMemberAsync(req.RequestedOnBehalfOfId);

            return await AddAsync(guild, creator, req);
        }

        public async Task<DiscordEmbed> AddAsync(InteractionContext ctx, TagAddReqDto req)
        {
            if (ctx is null) throw new ArgumentNullException(nameof(ctx));
            if (req is null) throw new ArgumentNullException(nameof(req));

            return await AddAsync(ctx.Guild, ctx.Member, req);
        }

        public async Task<DiscordEmbed> EditAsync(TagEditReqDto req)
        {
            if (req is null) throw new ArgumentNullException(nameof(req));

            DiscordGuild guild = null;
            if (req.GuildId.HasValue)
            {
                guild = await _discord.Client.GetGuildAsync(req.GuildId.Value);
            }
            else if (req.Id.HasValue)
            {
                var tag = await _tagService.GetAsync<Tag>(req.Id.Value);
                if (tag is null) throw new NotFoundException("Tag with given Id was not found");
                guild = await _discord.Client.GetGuildAsync(tag.GuildId);
            }

            DiscordMember requestingUser = await guild.GetMemberAsync(req.RequestedOnBehalfOfId);

            return await EditAsync(guild, requestingUser, req);
        }

        public async Task<DiscordEmbed> EditAsync(InteractionContext ctx, TagEditReqDto req)
        {
            if (ctx is null) throw new ArgumentNullException(nameof(ctx));
            if (req is null) throw new ArgumentNullException(nameof(req));

            return await EditAsync(ctx.Guild, ctx.Member, req);
        }

        public async Task<(DiscordEmbed Embed, string Text)> GetAsync(TagGetReqDto req)
        {
            if (req is null) throw new ArgumentNullException(nameof(req));

            DiscordGuild guild = null;
            if (req.GuildId.HasValue)
            {
                guild = await _discord.Client.GetGuildAsync(req.GuildId.Value);
            }
            else if (req.Id.HasValue)
            {
                var tag = await _tagService.GetAsync<Tag>(req.Id.Value);
                if (tag is null) throw new NotFoundException("Tag with given Id was not found");
                guild = await _discord.Client.GetGuildAsync(tag.GuildId);
            }

            DiscordMember requestingUser = await guild.GetMemberAsync(req.RequestedOnBehalfOfId);

            return await GetAsync(guild, requestingUser, req);
        }

        public async Task<(DiscordEmbed Embed, string Text)> GetAsync(InteractionContext ctx, TagGetReqDto req)
        {
            if (ctx is null) throw new ArgumentNullException(nameof(ctx));
            if (req is null) throw new ArgumentNullException(nameof(req));

            return await GetAsync(ctx.Guild, ctx.Member, req);
        }

        public async Task<DiscordEmbed> DisableAsync(TagDisableReqDto req)
        {
            if (req is null) throw new ArgumentNullException(nameof(req));

            DiscordGuild guild = null;
            if (req.GuildId.HasValue)
            {
                guild = await _discord.Client.GetGuildAsync(req.GuildId.Value);
            }
            else if (req.Id.HasValue)
            {
                var tag = await _tagService.GetAsync<Tag>(req.Id.Value);
                if (tag is null) throw new NotFoundException("Tag with given Id was not found");
                guild = await _discord.Client.GetGuildAsync(tag.GuildId);
            }

            DiscordMember requestingUser = await guild.GetMemberAsync(req.RequestedOnBehalfOfId);

            return await DisableAsync(guild, requestingUser, req);
        }

        public async Task<DiscordEmbed> DisableAsync(InteractionContext ctx, TagDisableReqDto req)
        {
            if (ctx is null) throw new ArgumentNullException(nameof(ctx));
            if (req is null) throw new ArgumentNullException(nameof(req));

            return await DisableAsync(ctx.Guild, ctx.Member, req);
        }

        private async Task<DiscordEmbed> AddAsync(DiscordGuild guild, DiscordMember creator, TagAddReqDto req)
        {
            if (guild is null) throw new ArgumentNullException(nameof(guild));
            if (creator is null) throw new ArgumentNullException(nameof(creator));
            if (req is null) throw new ArgumentNullException(nameof(req));

            var guildCfg =
                await _guildService.GetSingleBySpecAsync<Guild>(
                    new ActiveGuildByDiscordIdWithTagsSpecifications(req.GuildId));
            if (guildCfg is null)
                throw new NotFoundException($"Guild with Id: {guild.Id} doesn't exist in the database.");

            if (!creator.IsModerator())
                throw new DiscordNotAuthorizedException("You are not authorized to create tags");

            var res = await _tagService.AddAsync(req, true);

            if (!res) throw new ArgumentException($"Tag with name {req.Name} already exists.");

            var embed = new DiscordEmbedBuilder();
            embed.WithColor(new DiscordColor(guildCfg.EmbedHexColor));
            embed.WithDescription("Tag added successfully");

            return embed.Build();
        }

        private async Task<DiscordEmbed> EditAsync(DiscordGuild guild, DiscordMember requestingUser, TagEditReqDto req)
        {
            if (guild is null) throw new ArgumentNullException(nameof(guild));
            if (requestingUser is null) throw new ArgumentNullException(nameof(requestingUser));
            if (req is null) throw new ArgumentNullException(nameof(req));

            var guildCfg =
                await _guildService.GetSingleBySpecAsync<Guild>(
                    new ActiveGuildByDiscordIdWithTagsSpecifications(guild.Id));
            if (guildCfg is null)
                throw new NotFoundException($"Guild with Id: {guild.Id} doesn't exist in the database.");

            if (!requestingUser.IsModerator())
                throw new DiscordNotAuthorizedException("You are not authorized to edit tags");

            await _tagService.UpdateTagEmbedConfigAsync(req, true);

            var embed = new DiscordEmbedBuilder();
            embed.WithColor(new DiscordColor(guildCfg.EmbedHexColor));
            embed.WithDescription("Tag edited successfully");

            return embed.Build();
        }

        private async Task<(DiscordEmbed Embed, string Text)> GetAsync(DiscordGuild guild, DiscordMember requestingUser,
            TagGetReqDto req)
        {
            if (guild is null) throw new ArgumentNullException(nameof(guild));
            if (requestingUser is null) throw new ArgumentNullException(nameof(requestingUser));
            if (req is null) throw new ArgumentNullException(nameof(req));

            Tag tag;
            if (requestingUser.IsBotOwner(_discord.Client))
            {
                tag = req.Id.HasValue
                    ? await _tagService.GetAsync<Tag>(req.Id.Value)
                    : await _tagService.GetSingleBySpecAsync<Tag>(new Specification<Tag>(x => x.Name == req.Name));
            }
            else
            {
                var guildCfg =
                    await _guildService.GetSingleBySpecAsync<Guild>(
                        new ActiveGuildByDiscordIdWithTagsSpecifications(guild.Id));
                if (guildCfg is null)
                    throw new NotFoundException($"Guild with Id: {guild.Id} doesn't exist in the database.");

                if (requestingUser.Guild.Id != guild.Id) throw new DiscordNotAuthorizedException();

                tag = req.Id.HasValue
                    ? guildCfg.Tags.FirstOrDefault(x => x.Id == req.Id)
                    : guildCfg.Tags.FirstOrDefault(x => x.Name == req.Name);

                if (requestingUser.Guild.Id != guild.Id) throw new DiscordNotAuthorizedException();
            }

            if (tag is null) throw new NotFoundException("Tag not found");
            if (tag.IsDisabled && !requestingUser.IsBotOwner(_discord.Client))
                throw new DisabledEntityException("Found tag is disabled");

            return tag.EmbedConfig is null
                ? (null, tag.Text)
                : (_embedProvider.ConfigureEmbed(tag.EmbedConfig).Build(), tag.Text);
        }

        private async Task<DiscordEmbed> DisableAsync(DiscordGuild guild, DiscordMember requestingUser,
            TagDisableReqDto req)
        {
            if (guild is null) throw new ArgumentNullException(nameof(guild));
            if (requestingUser is null) throw new ArgumentNullException(nameof(requestingUser));
            if (req is null) throw new ArgumentNullException(nameof(req));

            var guildCfg =
                await _guildService.GetSingleBySpecAsync<Guild>(
                    new ActiveGuildByDiscordIdWithTagsSpecifications(guild.Id));
            if (guildCfg is null)
                throw new NotFoundException($"Guild with Id: {guild.Id} doesn't exist in the database.");

            if (!requestingUser.IsModerator())
                throw new DiscordNotAuthorizedException("You are not authorized to edit tags");

            await _tagService.DisableAsync(req, true);
            var embed = new DiscordEmbedBuilder();
            embed.WithColor(new DiscordColor(guildCfg.EmbedHexColor));
            embed.WithDescription("Tag disabled successfully");

            return embed.Build();
        }
    }
}