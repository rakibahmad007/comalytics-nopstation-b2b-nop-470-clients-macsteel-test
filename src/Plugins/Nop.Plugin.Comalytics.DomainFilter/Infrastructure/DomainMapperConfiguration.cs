using AutoMapper;
using Nop.Core.Infrastructure.Mapper;
using Nop.Plugin.Comalytics.DomainFilter.Domains;
using Nop.Plugin.Comalytics.DomainFilter.Models;

namespace Nop.Plugin.Comalytics.DomainFilter.Infrastructure
{
    public class DomainMapperConfiguration : Profile, IOrderedMapperProfile
    {
        public DomainMapperConfiguration()
        {
            CreateMap<Domain, DomainModel>()
                .ForMember(model => model.AvailableTypeOptions, options => options.Ignore());
            CreateMap<DomainModel, Domain>()
                .ForMember(entity => entity.Type, options => options.Ignore());
        }

        public int Order => 1;
    }
}
