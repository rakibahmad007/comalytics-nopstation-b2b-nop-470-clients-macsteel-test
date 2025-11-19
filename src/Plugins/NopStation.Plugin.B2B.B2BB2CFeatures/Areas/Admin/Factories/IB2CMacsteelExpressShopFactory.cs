using System.Threading.Tasks;
using NopStation.Plugin.B2B.B2BB2CFeatures.Areas.Admin.Models.B2CMacsteelExpressShops;
using NopStation.Plugin.B2B.ERPIntegrationCore.Domain;

namespace NopStation.Plugin.B2B.B2BB2CFeatures.Areas.Admin.Factories;
public interface IB2CMacsteelExpressShopFactory
{
    Task<B2CMacsteelExpressShopSearchModel> PrepareB2CMacsteelExpressShopSearchModelAsync(B2CMacsteelExpressShopSearchModel searchModel);
    Task<B2CMacsteelExpressShopListModel> PrepareB2CMacsteelExpressShopListModelAsync(B2CMacsteelExpressShopSearchModel searchModel);
    Task<B2CMacsteelExpressShopModel> PrepareB2CMacsteelExpressShopModelAsync(B2CMacsteelExpressShopModel model, B2CMacsteelExpressShop b2CMacsteelExpressShop);
}
