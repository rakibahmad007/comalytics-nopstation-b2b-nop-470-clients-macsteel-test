using System.Collections.Generic;
using System.Threading.Tasks;
using Nop.Core.Domain.Catalog;
using Nop.Services.Catalog;
using NopStation.Plugin.B2B.ERPIntegrationCore.Domain;
using NopStation.Plugin.B2B.ERPIntegrationCore.Model;
using NopStation.Plugin.B2B.ERPIntegrationCore.Model.Common;

namespace NopStation.Plugin.B2B.ERPIntegrationCore.ErpInterface;

public interface IErpIntegrationProductService
{
    Task<ErpResponseData<ErpProductDataModel>> GetProductByItemNoFromErpAsync(
        ErpGetRequestModel erpRequest
    );

    Task<ErpResponseData<IList<ErpProductDataModel>>> GetProductsFromErpAsync(
        ErpGetRequestModel erpRequest
    );

    Task<ErpResponseData<ErpStockDataModel>> GetStockByItemNoFromErpAsync(
        ErpGetRequestModel erpRequest
    );

    Task<ErpResponseData<IList<ErpStockDataModel>>> GetStocksFromErpAsync(
        ErpGetRequestModel erpRequest
    );

    Task<ErpResponseData<ErpPriceGroupPricingDataModel>> GetProductGroupPriceFromErpAsync(
        ErpGetRequestModel erpRequest
    );

    Task<
        ErpResponseData<IList<ErpPriceGroupPricingDataModel>>
    > GetProductGroupPricesFromErpAsync(ErpGetRequestModel erpRequest);

    Task<ErpResponseData<ErpPriceSpecialPricingDataModel>> GetProductSpecialPriceFromErpAsync(
        ErpGetRequestModel erpRequest
    );

    Task<
        ErpResponseData<IList<ErpPriceSpecialPricingDataModel>>
    > GetProductSpecialPricesFromErpAsync(ErpGetRequestModel erpRequest);

    Task<ErpResponseData<ErpProductImageDataModel>> GetProductImageFromErpAsync(
        ErpGetRequestModel erpRequest
    );

    Task<ErpResponseData<IList<ErpProductImageDataModel>>> GetProductImagesFromErpAsync(
        ErpGetRequestModel erpRequest
    );

    Task<ErpResponseData<IList<ErpProductImageDataModel>>> GetSpecSheetAsync(
        ErpGetRequestModel erpRequest
    );

    Task<ErpResponseData<IList<ErpPriceSpecialPricingDataModel>>> ProductListLivePriceSync(ErpGetRequestModel erpGetRequest);
}
