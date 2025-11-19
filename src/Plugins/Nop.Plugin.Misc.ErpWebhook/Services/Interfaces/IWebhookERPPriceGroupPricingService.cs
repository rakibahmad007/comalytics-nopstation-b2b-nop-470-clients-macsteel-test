using System.Collections.Generic;
using System.Threading.Tasks;
using Nop.Plugin.Misc.ErpWebhook.Models.ErpPricing;

namespace Nop.Plugin.Misc.ErpWebhook.Services.Interfaces
{
    public interface IWebhookERPPriceGroupPricingService
    {
        Task ProcessERPPriceGroupPricingAsync(List<ErpPriceGroupPricingModel> erpAccountPricings);
    }
}
