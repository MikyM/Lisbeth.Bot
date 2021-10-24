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
using Lisbeth.Bot.Domain.DTOs;
using Lisbeth.Bot.Domain.DTOs.Request;
using Lisbeth.Bot.Domain.Entities;

namespace Lisbeth.Bot.API
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            CreateMap<MuteReqDto, Mute>();
            CreateMap<MuteDisableReqDto, Mute>();
            CreateMap<PruneReqDto, Prune>();
            CreateMap<GuildGetReqDto, Guild>();
            CreateMap<TicketExportReqDto, Ticket>();
            CreateMap<TicketOpenReqDto, Ticket>()
                .ForMember(dest => dest.UserId, source => source.MapFrom(x => x.OwnerId));
            CreateMap<Ticket, Ticket>();
            CreateMap<TicketingConfig, TicketingConfig>();
            CreateMap<ModerationConfig, ModerationConfig>();
            CreateMap<TagAddReqDto, Tag>();
            CreateMap<EmbedConfigDto, EmbedConfig>();
            CreateMap<TagGetReqDto, Tag>();

            CreateMap<RoleEmojiMappingReqDto, RoleEmojiMapping>();
            CreateMap<RoleMenuReqDto, RoleMenu>();
        }
    }
}