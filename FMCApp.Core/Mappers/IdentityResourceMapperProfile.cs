// Based on the IdentityServer4.EntityFramework - authors - Brock Allen & Dominick Baier.
// https://github.com/IdentityServer/IdentityServer4.EntityFramework

// Modified by Jan Škoruba

using System.Linq;
using AutoMapper;
using IdentityServer4.EntityFramework.Entities;
using FMCApp.Core.Dtos.Common;
using FMCApp.Core.Dtos.Configuration;

namespace FMCApp.Core.Mappers
{
    public class IdentityResourceMapperProfile : Profile
    {
        public IdentityResourceMapperProfile()
        {
            // entity to model
            CreateMap<IdentityResource, IdentityResourceDto>(MemberList.Destination)
                .ForMember(x => x.UserClaims, opt => opt.MapFrom(src => src.UserClaims.Select(x => x.Type)));

            CreateMap<PagedList<IdentityResource>, IdentityResourcesDto>(MemberList.Destination)
                .ForMember(x => x.IdentityResources,
                    opt => opt.MapFrom(src => src.Data));

            // model to entity
            CreateMap<IdentityResourceDto, IdentityResource>(MemberList.Source)
                .ForMember(x => x.UserClaims, opts => opts.MapFrom(src => src.UserClaims.Select(x => new IdentityClaim { Type = x })));
        }
    }
}