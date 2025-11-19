using System.Threading.Tasks;
using Nop.Core.Domain.Catalog;
using NopStation.Plugin.B2B.B2BB2CFeatures.Model.Order;
using NopStation.Plugin.B2B.ERPIntegrationCore.Domain;

namespace NopStation.Plugin.B2B.B2BB2CFeatures.Services.ErpOrderTotalCalculationService;

public interface IErpOrderTotalCalculationService
{
    Task<B2BAdditionalOrderTotalsModel> LoadAdditionalOrderTotalsDataOfCartItemsAsync();

    Task<int> GetTotalStockQuantityForAdminOPEventAsync(Product product, ErpAccount b2BAccount, ErpNopUser erpNopUser);
}