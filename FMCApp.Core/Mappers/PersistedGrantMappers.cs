using AutoMapper;
using FMCApp.Core.Dtos.Common;
using FMCApp.Core.Dtos.Grant;
using FMCApp.Entity.Entities;

namespace FMCApp.Core.Mappers
{
    public static class PersistedGrantMappers
    {
        static PersistedGrantMappers()
        {
            Mapper = new MapperConfiguration(cfg =>cfg.AddProfile<PersistedGrantMapperProfile>())
                .CreateMapper();
        }

        internal static IMapper Mapper { get; }

        public static PersistedGrantsDto ToModel(this PagedList<PersistedGrantDataView> grant)
        {
            return grant == null ? null : Mapper.Map<PersistedGrantsDto>(grant);
        }

        public static PersistedGrantsDto ToModel(this PagedList<global::IdentityServer4.EntityFramework.Entities.PersistedGrant> grant)
        {
            return grant == null ? null : Mapper.Map<PersistedGrantsDto>(grant);
        }

        public static PersistedGrantDto ToModel(this global::IdentityServer4.EntityFramework.Entities.PersistedGrant grant)
        {
            return grant == null ? null : Mapper.Map<PersistedGrantDto>(grant);
        }
    }
}