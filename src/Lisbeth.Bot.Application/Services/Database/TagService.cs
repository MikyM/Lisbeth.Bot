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

using AutoMapper;
using JetBrains.Annotations;
using Lisbeth.Bot.Application.Results;
using Lisbeth.Bot.Application.Services.Database.Interfaces;
using Lisbeth.Bot.DataAccessLayer;
using Lisbeth.Bot.DataAccessLayer.Specifications.Tag;
using Lisbeth.Bot.Domain.DTOs.Request.Tag;
using Lisbeth.Bot.Domain.Entities;
using MikyM.Common.Application.Results;
using MikyM.Common.Application.Services;
using MikyM.Common.DataAccessLayer.Specifications;
using MikyM.Common.DataAccessLayer.UnitOfWork;
using System;
using System.Threading.Tasks;

namespace Lisbeth.Bot.Application.Services.Database
{
    [UsedImplicitly]
    public class TagService : CrudService<Tag, LisbethBotDbContext>, ITagService
    {
        public TagService(IMapper mapper, IUnitOfWork<LisbethBotDbContext> uof) : base(mapper, uof)
        {
        }

        public async Task<Result> AddAsync(TagAddReqDto req, bool shouldSave = false)
        {
            var res = await base.LongCountAsync(new TagByGuildAndNameSpec(req.Name, req.GuildId));
            if (res.Entity != 0) return new ArgumentError(nameof(req.Name), $"Guild already has a tag named {req.Name}");

            await base.AddAsync(req, shouldSave);
            return Result.FromSuccess();
        }

        public async Task<Result> UpdateTagEmbedConfigAsync(TagEditReqDto req, bool shouldSave = false)
        {
            Result<Tag> tag;
            if (req.Id.HasValue)
                tag = await base.GetAsync<Tag>(req.Id.Value);
            else if (req.Name is not null && req.Name != "")
                tag = await base.GetSingleBySpecAsync<Tag>(new Specification<Tag>(x =>
                    x.Name == req.Name && x.GuildId == req.GuildId));
            else throw new ArgumentException("Invalid tag Id/Name was provided.");

            if (!tag.IsSuccess) return Result.FromError(tag);
            if (tag.Entity.IsDisabled)
                return new ArgumentError(nameof(tag.Entity),"Can't update embed config for a disabled tag, enable the tag first.");

            base.BeginUpdate(tag.Entity);
            if (req.EmbedConfig is not null) tag.Entity.EmbedConfig = _mapper.Map<EmbedConfig>(req.EmbedConfig);
            tag.Entity.LastEditById = req.RequestedOnBehalfOfId;
            if (!string.IsNullOrWhiteSpace(req.Name)) tag.Entity.Name = req.Name;
            if (!string.IsNullOrWhiteSpace(req.Text)) tag.Entity.Text = req.Text;

            if (shouldSave) await base.CommitAsync();

            return Result.FromSuccess();
        }

        public async Task<Result> DisableAsync(TagDisableReqDto req, bool shouldSave = false)
        {
            Result<Tag> tag;
            if (req.Id.HasValue)
                tag = await base.GetAsync<Tag>(req.Id.Value);
            else if (req.Name is not null && req.Name != "")
                tag = await base.GetSingleBySpecAsync<Tag>(new Specification<Tag>(x =>
                    x.Name == req.Name && x.GuildId == req.GuildId));
            else throw new ArgumentException("Invalid tag Id/Name was provided.");

            if (!tag.IsSuccess) return Result.FromError(tag);
            if (tag.Entity.IsDisabled) return Result.FromError(new DisabledEntityError(nameof(tag.Entity)));

            base.BeginUpdate(tag.Entity);
            tag.Entity.IsDisabled = true;

            if (shouldSave) await base.CommitAsync();

            return Result.FromSuccess();
        }

        // enable to do
    }
}