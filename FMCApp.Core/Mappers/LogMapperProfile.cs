using AutoMapper;
using FMCApp.Core.Dtos.Common;
using FMCApp.Core.Dtos.Log;
using FMCApp.Entity.Entities;

namespace FMCApp.Core.Mappers
{
    public class LogMapperProfile : Profile
    {
        public LogMapperProfile()
        {
            CreateMap<Log, LogDto>(MemberList.Destination)
                .ReverseMap();
            
            CreateMap<PagedList<Log>, LogsDto>(MemberList.Destination)
                .ForMember(x => x.Logs, opt => opt.MapFrom(src => src.Data));
        }
    }
}
