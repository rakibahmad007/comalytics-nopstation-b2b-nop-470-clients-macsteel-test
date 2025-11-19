using AutoMapper;
using Nop.Core.Infrastructure.Mapper;
using Nop.Plugin.Misc.ErpWebhook.Areas.Admin.Models.AllowedWebhookManagerIpAddress;
using Nop.Plugin.Misc.ErpWebhook.Areas.Admin.Models.Configuration;
using Nop.Plugin.Misc.ErpWebhook.Domain;

namespace Nop.Plugin.Misc.ErpWebhook.Areas.Admin.Infrastructure;

public class MapperConfiguration : Profile, IOrderedMapperProfile
{
    public MapperConfiguration()
    {
        CreateMap<ErpWebhookSettings, ConfigurationModel>()
            .ForMember(model => model.ActiveStoreScopeConfiguration, options => options.Ignore());
        CreateMap<ConfigurationModel, ErpWebhookSettings>();
        CreateMap<AllowedWebhookManagerIpAddresses, AllowedWebhookManagerIpAddressModel>();
    }

    public int Order => 100;
}
