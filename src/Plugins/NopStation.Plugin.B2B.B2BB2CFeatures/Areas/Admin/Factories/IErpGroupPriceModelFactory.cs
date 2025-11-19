using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Nop.Web.Areas.Admin.Models.Catalog;
using NopStation.Plugin.B2B.B2BB2CFeatures.Areas.Admin.Models;
using NopStation.Plugin.B2B.ERPIntegrationCore.Domain;

namespace NopStation.Plugin.B2B.B2BB2CFeatures.Areas.Admin.Factories;

public interface IErpGroupPriceModelFactory
{
    Task<ErpPriceGroupProductPricingSearchModel> PrepareErpProductPricingSearchModel(ErpPriceGroupProductPricingSearchModel searchModel, int productId);

    Task<ErpPriceGroupProductPricingListModel> PrepareErpProductPricingListModel(ErpPriceGroupProductPricingSearchModel searchModel);

    Task<ErpPriceGroupProductPricingModel> PrepareErpProductPricingModel(ErpPriceGroupProductPricingModel model, ErpGroupPrice erpProductPricing);
    Task<byte[]> ExportPriceGroupProductPricingToXlsx(List<int> list);
    Task<byte[]> ExportPriceGroupProductPricingToXlsxAll(ProductSearchModel searchModel);
    void ImportPriceGroupProductPricingFromXlsx(Stream stream);
}