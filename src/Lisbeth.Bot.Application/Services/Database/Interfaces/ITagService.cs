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

using Lisbeth.Bot.DataAccessLayer;
using Lisbeth.Bot.Domain.DTOs.Request.Tag;

namespace Lisbeth.Bot.Application.Services.Database.Interfaces;

public interface ITagDataDataService : ICrudDataService<Tag, LisbethBotDbContext>
{
    Task<Result> AddAsync(TagAddReqDto req, bool shouldSave = false);
    Task<Result> UpdateTagTextAsync(TagEditReqDto req, bool shouldSave = false);
    Task<Result> DisableAsync(TagDisableReqDto req, bool shouldSave = false);
    Task<Result> AddAllowedRoleAsync(TagAddSnowflakePermissionReqDto permissionReq, bool shouldSave = false);
    Task<Result> AddAllowedUserAsync(TagAddSnowflakePermissionReqDto permissionReq, bool shouldSave = false);
    Task<Result> RemoveAllowedRoleAsync(TagRevokeSnowflakePermissionReqDto permissionReq, bool shouldSave = false);
    Task<Result> RemoveAllowedUserAsync(TagRevokeSnowflakePermissionReqDto permissionReq, bool shouldSave = false);
}