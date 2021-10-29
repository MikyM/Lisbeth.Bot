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

using System.Linq;
using AutoMapper;
using DSharpPlus.Entities;
using Lisbeth.Bot.Domain.DTOs;
using Lisbeth.Bot.Domain.DTOs.Request;
using Lisbeth.Bot.Domain.Entities;

namespace Lisbeth.Bot.API
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            CreateMap<MuteReqDto, Mute>()
                .ForMember(dest => dest.UserId, source => source.MapFrom(x => x.TargetUserId))
                .ForMember(dest => dest.AppliedById, source => source.MapFrom(x => x.RequestedOnBehalfOfId));
            CreateMap<MuteDisableReqDto, Mute>()
                .ForMember(dest => dest.UserId, source => source.MapFrom(x => x.TargetUserId))
                .ForMember(dest => dest.AppliedById, source => source.MapFrom(x => x.RequestedOnBehalfOfId))
                .ForMember(dest => dest.GuildId, source => source.MapFrom(x => x.GuildId));
            CreateMap<BanReqDto, Ban>()
                .ForMember(dest => dest.UserId, source => source.MapFrom(x => x.TargetUserId))
                .ForMember(dest => dest.AppliedById, source => source.MapFrom(x => x.RequestedOnBehalfOfId));
            CreateMap<BanDisableReqDto, Ban>()
                .ForMember(dest => dest.UserId, source => source.MapFrom(x => x.TargetUserId))
                .ForMember(dest => dest.AppliedById, source => source.MapFrom(x => x.RequestedOnBehalfOfId));
            CreateMap<PruneReqDto, Prune>()
                .ForMember(dest => dest.UserId, source => source.MapFrom(x => x.TargetAuthorId))
                .ForMember(dest => dest.ModeratorId, source => source.MapFrom(x => x.RequestedOnBehalfOfId));
            CreateMap<GuildGetReqDto, Guild>();
            CreateMap<TicketExportReqDto, Ticket>();
            CreateMap<TicketOpenReqDto, Ticket>()
                .ForMember(dest => dest.UserId, source => source.MapFrom(x => x.OwnerId));
            CreateMap<TicketingConfig, TicketingConfig>();
            CreateMap<ModerationConfig, ModerationConfig>();
            CreateMap<TagAddReqDto, Tag>()
                .ForMember(dest => dest.CreatorId, source => source.MapFrom(x => x.RequestedOnBehalfOfId))
                .ForMember(dest => dest.LastEditById, source => source.MapFrom(x => x.RequestedOnBehalfOfId));
            CreateMap<EmbedConfigDto, EmbedConfig>();
            CreateMap<TagEditReqDto, Tag>()
                .ForMember(dest => dest.LastEditById, source => source.MapFrom(x => x.RequestedOnBehalfOfId));
            CreateMap<TagDisableReqDto, Tag>()
                .ForMember(dest => dest.LastEditById, source => source.MapFrom(x => x.RequestedOnBehalfOfId));

            CreateMap<RoleEmojiMappingReqDto, RoleEmojiMapping>();
            CreateMap<RoleMenuReqDto, RoleMenu>();
            CreateMap<DiscordFieldDto, DiscordField>();
            CreateMap<EmbedConfigDto, EmbedConfig>();

            CreateMap<DiscordEmbed, EmbedConfig>()
                .ForMember(dest => dest.Author, source => source.MapFrom(x => x.Author.Name))
                .ForMember(dest => dest.AuthorImageUrl, source => source.MapFrom(x => x.Author.IconUrl))
                .ForMember(dest => dest.AuthorUrl, source => source.MapFrom(x => x.Author.Url))
                .ForMember(dest => dest.Footer, source => source.MapFrom(x => x.Footer.Text))
                .ForMember(dest => dest.FooterImageUrl, source => source.MapFrom(x => x.Footer.IconUrl))
                .ForMember(dest => dest.Description, source => source.MapFrom(x => x.Description))
                .ForMember(dest => dest.ImageUrl, source => source.MapFrom(x => x.Image.Url))
                .ForMember(dest => dest.Fields,
                    source => source.MapFrom(x =>
                        x.Fields.Select(y => new DiscordField {Text = y.Value, Title = y.Name})))
                .ForMember(dest => dest.Title, source => source.MapFrom(x => x.Title))
                .ForMember(dest => dest.Timestamp, source => source.PreCondition(x => x.Timestamp.HasValue))
                .ForMember(dest => dest.Timestamp, source => source.MapFrom(x => x.Timestamp.Value.DateTime))
                .ForMember(dest => dest.HexColor, source => source.PreCondition(x => x.Color.HasValue))
                .ForMember(dest => dest.HexColor, source => source.MapFrom(x => x.Color.Value.ToString()))
                .ForMember(dest => dest.Thumbnail, source => source.PreCondition(x => x.Thumbnail is not null))
                .ForMember(dest => dest.Thumbnail, source => source.MapFrom(x => x.Thumbnail.Url.ToString()))
                .ForMember(dest => dest.ThumbnailHeight, source => source.PreCondition(x => x.Thumbnail is not null))
                .ForMember(dest => dest.ThumbnailHeight, source => source.MapFrom(x => x.Thumbnail.Height))
                .ForMember(dest => dest.ThumbnailWidth, source => source.PreCondition(x => x.Thumbnail is not null))
                .ForMember(dest => dest.ThumbnailWidth, source => source.MapFrom(x => x.Thumbnail.Width))
                .ForMember(dest => dest.Id, source => source.Ignore());
        }
    }
}