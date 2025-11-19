using System.Collections.Generic;
using System.Threading.Tasks;
using Nop.Plugin.Misc.ErpWebhook.Models.ErpPricing;

namespace Nop.Plugin.Misc.ErpWebhook.Services.Interfaces
{
    public interface IWebhookERPPerAccountPricingService
    {
        Task ProcessERPPerAccountPricingToParallelTableAsync(List<ErpAccountPricingModel> erpAccountPricings);
        //void ProcessB2BPerAccountPricing(List<ErpAccountPricingModel> erpAccountPricings);
    }
}
