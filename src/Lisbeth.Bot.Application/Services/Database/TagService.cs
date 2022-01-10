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
using Lisbeth.Bot.DataAccessLayer;
using Lisbeth.Bot.DataAccessLayer.Specifications.Tag;
using Lisbeth.Bot.Domain.DTOs.Request.Tag;
using MikyM.Common.DataAccessLayer.UnitOfWork;

namespace Lisbeth.Bot.Application.Services.Database;

[UsedImplicitly]
public class TagService : CrudService<Tag, LisbethBotDbContext>, ITagService
{
    public TagService(IMapper mapper, IUnitOfWork<LisbethBotDbContext> uof) : base(mapper, uof)
    {
    }

    public async Task<Result> AddAsync(TagAddReqDto req, bool shouldSave = false)
    {
        var res = await base.LongCountAsync(new ActiveTagByGuildAndNameSpec(req.Name, req.GuildId));
        if (res.Entity != 0)
            return new DiscordArgumentError(nameof(req.Name), $"Guild already has a tag named {req.Name}");

        await base.AddAsync(req, shouldSave);
        return Result.FromSuccess();
    }

    public async Task<Result> UpdateTagTextAsync(TagEditReqDto req, bool shouldSave = false)
    {
        Result<Tag> tagRes;
        if (req.Name is not null && req.Name != "")
            tagRes = await base.GetSingleBySpecAsync(new ActiveTagByGuildAndNameSpec(req.Name,
                req.GuildId));
        else
            return new NotFoundError();

        if (!tagRes.IsDefined(out var tag)) return Result.FromError(tagRes);
        if (tag.IsDisabled)
            return new DiscordArgumentError(nameof(tag),
                "Can't update embed config for a disabled tag, enable the tag first.");

        base.BeginUpdate(tag);
        tag.LastEditById = req.RequestedOnBehalfOfId;
        if (!string.IsNullOrWhiteSpace(req.Text)) tag.Text = req.Text;

        if (shouldSave) await base.CommitAsync();

        return Result.FromSuccess();
    }

    public async Task<Result> AddAllowedUserAsync(TagAddSnowflakePermissionReqDto permissionReq, bool shouldSave = false)
    {
        Result<Tag> tagRes;
        if (permissionReq.Name is not null && permissionReq.Name != "")
            tagRes = await base.GetSingleBySpecAsync(new ActiveTagByGuildAndNameSpec(permissionReq.Name,
                permissionReq.GuildId));
        else
            return new NotFoundError();

        if (!tagRes.IsDefined(out var tag)) return Result.FromError(tagRes);
        if (tag.IsDisabled)
            return new DiscordArgumentError(nameof(tag),
                "Can't update a disabled tag, enable the tag first.");

        base.BeginUpdate(tag, true);
        tag.AllowedUserIds = tag.AllowedUserIds.Select(x => x).Append(permissionReq.SnowflakeId!.Value).ToList();

        if (shouldSave) await base.CommitAsync();

        return Result.FromSuccess();
    }

    public async Task<Result> RemoveAllowedRoleAsync(TagRevokeSnowflakePermissionReqDto permissionReq, bool shouldSave = false)
    {
        Result<Tag> tagRes;
        if (permissionReq.Name is not null && permissionReq.Name != "")
            tagRes = await base.GetSingleBySpecAsync(new ActiveTagByGuildAndNameSpec(permissionReq.Name,
                permissionReq.GuildId));
        else
            return new NotFoundError();

        if (!tagRes.IsDefined(out var tag)) return Result.FromError(tagRes);
        if (tag.IsDisabled)
            return new DiscordArgumentError(nameof(tag),
                "Can't update a disabled tag, enable the tag first.");

        base.BeginUpdate(tag, true);
        tag.AllowedRoleIds = tag.AllowedRoleIds.TakeWhile(x => x != permissionReq.SnowflakeId).ToList();

        if (shouldSave) await base.CommitAsync();

        return Result.FromSuccess();
    }

    public async Task<Result> RemoveAllowedUserAsync(TagRevokeSnowflakePermissionReqDto permissionReq, bool shouldSave = false)
    {
        Result<Tag> tagRes;
        if (permissionReq.Name is not null && permissionReq.Name != "")
            tagRes = await base.GetSingleBySpecAsync(new ActiveTagByGuildAndNameSpec(permissionReq.Name,
                permissionReq.GuildId));
        else
            return new NotFoundError();

        if (!tagRes.IsDefined(out var tag)) return Result.FromError(tagRes);
        if (tag.IsDisabled)
            return new DiscordArgumentError(nameof(tag),
                "Can't update a disabled tag, enable the tag first.");

        base.BeginUpdate(tag, true);
        tag.AllowedUserIds = tag.AllowedUserIds.TakeWhile(x => x != permissionReq.SnowflakeId).ToList();

        if (shouldSave) await base.CommitAsync();

        return Result.FromSuccess();
    }

    public async Task<Result> AddAllowedRoleAsync(TagAddSnowflakePermissionReqDto permissionReq, bool shouldSave = false)
    {
        Result<Tag> tagRes;
        if (permissionReq.Name is not null && permissionReq.Name != "")
            tagRes = await base.GetSingleBySpecAsync(new ActiveTagByGuildAndNameSpec(permissionReq.Name,
                permissionReq.GuildId));
        else
            return new NotFoundError();

        if (!tagRes.IsDefined(out var tag)) return Result.FromError(tagRes);
        if (tag.IsDisabled)
            return new DiscordArgumentError(nameof(tag),
                "Can't update a disabled tag, enable the tag first.");

        base.BeginUpdate(tag, true);
        tag.AllowedRoleIds = tag.AllowedRoleIds.Select(x => x).Append(permissionReq.SnowflakeId!.Value).ToList();

        if (shouldSave) await base.CommitAsync();

        return Result.FromSuccess();
    }

    public async Task<Result> DisableAsync(TagDisableReqDto req, bool shouldSave = false)
    {
        Result<Tag> tag;
        if (req.Name is not null && req.Name != "")
            tag = await base.GetSingleBySpecAsync(new ActiveTagByGuildAndNameSpec(req.Name,
                req.GuildId));
        else
            return new NotFoundError();

        if (!tag.IsDefined()) return Result.FromError(tag);
        if (tag.Entity.IsDisabled) return Result.FromError(new DisabledEntityError(nameof(tag.Entity)));

        base.BeginUpdate(tag.Entity);
        tag.Entity.IsDisabled = true;

        if (shouldSave) await base.CommitAsync();

        return Result.FromSuccess();
    }

    // enable to do
}