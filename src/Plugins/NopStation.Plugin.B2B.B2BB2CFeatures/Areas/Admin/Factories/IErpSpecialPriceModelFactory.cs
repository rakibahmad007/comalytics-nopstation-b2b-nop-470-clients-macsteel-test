using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Nop.Web.Areas.Admin.Models.Catalog;
using NopStation.Plugin.B2B.B2BB2CFeatures.Areas.Admin.Models;
using NopStation.Plugin.B2B.ERPIntegrationCore.Domain;

namespace NopStation.Plugin.B2B.B2BB2CFeatures.Areas.Admin.Factories;

public interface IErpSpecialPriceModelFactory
{
    Task<ErpSpecialPriceSearchModel> PrepareErpProductSpecialPriceSearchModel(ErpSpecialPriceSearchModel searchModel, int productId);

    Task<ErpSpecialPriceListModel> PrepareErpProductSpecialPriceListModel(ErpSpecialPriceSearchModel searchModel);

    Task<ErpSpecialPriceModel> PrepareErpProductSpecialPriceModel(ErpSpecialPriceModel model, ErpSpecialPrice erpProductPricing);
    Task<byte[]> ExportSpecialPriceToXlsx(List<int> list);
    Task<byte[]> ExportSpecialPriceToXlsxAll(ProductSearchModel searchModel);
    void ImportSpecialPriceFromXlsx(Stream stream);
}