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

using System.Threading.Tasks;
using Lisbeth.Bot.DataAccessLayer;
using Lisbeth.Bot.Domain.DTOs.Request;
using Lisbeth.Bot.Domain.Entities;
using MikyM.Common.Application.Interfaces;

namespace Lisbeth.Bot.Application.Services.Interfaces.Database
{
    public interface IMuteService : ICrudService<Mute, LisbethBotDbContext>
    {
        Task<(long Id, Mute FoundEntity)> AddOrExtendAsync(MuteReqDto req, bool shouldSave = false);
        Task<Mute> DisableAsync(MuteDisableReqDto entry, bool shouldSave = false);
    }
}