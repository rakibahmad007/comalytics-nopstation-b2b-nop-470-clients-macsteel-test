using System.Collections.Generic;
using System.Threading.Tasks;
using Nop.Core.Domain.Catalog;
using NopStation.Plugin.B2B.ERPIntegrationCore.Domain;

namespace NopStation.Plugin.B2B.B2BB2CFeatures.Services.ErpPriceSyncFunctionality
{
    public interface IErpPriceSyncFunctionalityService
    {
        Task<bool> IsB2BPriceSyncRequiredAsync();

        Task<bool> IsCartProductB2BPriceSyncRequiredAsync();

        Task ExecuteAllProductsLivePriceSync();

        Task<(bool success, string message)> ProductListLiveStockSyncAsync(ErpAccount erpAccount, IList<Product> products);
    }
}