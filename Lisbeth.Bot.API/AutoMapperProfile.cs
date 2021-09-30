using AutoMapper;
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
        }
    }
}
