using System.Threading.Tasks;
using Nop.Plugin.Misc.ErpWebhook.Areas.Admin.Models.AllowedWebhookManagerIpAddress;

namespace Nop.Plugin.Misc.ErpWebhook.Areas.Admin.Factories
{
    public interface IAllowedWebhookManagerIpAddressModelFactory
    {
        Task<AllowedWebhookManagerIpAddressSearchModel> PrepareAllowedWebhookManagerIpAddressSearchModelAsync(AllowedWebhookManagerIpAddressSearchModel searchModel);
        Task<AllowedWebhookManagerIpAddressListModel> PrepareAllowedWebhookManagerIpAddressListModelAsync(AllowedWebhookManagerIpAddressSearchModel searchModel);
    }
}
