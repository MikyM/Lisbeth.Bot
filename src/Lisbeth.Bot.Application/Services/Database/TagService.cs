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
using AutoMapper;
using JetBrains.Annotations;
using Lisbeth.Bot.Application.Services.Interfaces.Database;
using Lisbeth.Bot.DataAccessLayer;
using Lisbeth.Bot.Domain.DTOs.Request;
using Lisbeth.Bot.Domain.Entities;
using MikyM.Common.Application.Services;
using MikyM.Common.DataAccessLayer.Specifications;
using MikyM.Common.DataAccessLayer.UnitOfWork;

namespace Lisbeth.Bot.Application.Services.Database
{
    [UsedImplicitly]
    public class TagService : CrudService<Tag, LisbethBotDbContext>, ITagService
    {
        public TagService(IMapper mapper, IUnitOfWork<LisbethBotDbContext> uof) : base(mapper, uof)
        {
        }

        public async Task UpdateTagEmbedConfigAsync(TagEditReqDto req, bool shouldSave = false)
        {
            Tag tag;
            if (req.Id.HasValue)
            {
                tag = await GetAsync<Tag>(req.Id.Value);
            }
            else if (req.Name is not null && req.Name != "")
            {
                var res = await GetBySpecAsync<Tag>(new Specification<Tag>(x => x.Name == req.Name && x.GuildId == req.GuildId));
                tag = res.FirstOrDefault();
            }
            else throw new ArgumentException("Invalid tag Id/Name was provided.");

            if (tag  is null) throw new ArgumentException("Tag doesn't exist.");
            if (tag.IsDisabled) throw new ArgumentException("Can't update embed config for a disabled tag, enable the tag first.");

            BeginUpdate(tag);
            tag.EmbedConfig = _mapper.Map<Tag>(req).EmbedConfig;
            
            if(shouldSave) await CommitAsync();
        }

        public async Task DisableAsync(TagDisableReqDto req, bool shouldSave = false)
        {
            Tag tag;
            if (req.Id.HasValue)
            {
                tag = await GetAsync<Tag>(req.Id.Value);
            }
            else if (req.Name is not null && req.Name != "")
            {
                var res = await GetBySpecAsync<Tag>(new Specification<Tag>(x => x.Name == req.Name && x.GuildId == req.GuildId));
                tag = res.FirstOrDefault();
            }
            else throw new ArgumentException("Invalid tag Id/Name was provided.");

            if (tag  is null) throw new ArgumentException("Tag doesn't exist.");
            if (tag.IsDisabled) return;

            BeginUpdate(tag);
            tag.IsDisabled = true;

            if (shouldSave) await CommitAsync();
        }

        // enable to do
    }
}
