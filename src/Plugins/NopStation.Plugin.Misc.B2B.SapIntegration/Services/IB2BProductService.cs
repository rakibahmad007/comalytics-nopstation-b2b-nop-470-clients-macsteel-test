using NopStation.Plugin.B2B.ERPIntegrationCore.Model;
using NopStation.Plugin.B2B.ERPIntegrationCore.Model.Common;

namespace NopStation.Plugin.Misc.B2B.SapIntegration.Services;

public interface IB2BProductService
{
    Task<ErpResponseData<IList<ErpProductDataModel>>> GetProductsFromErpAsync(
        ErpGetRequestModel erpRequest
    );

    Task<ErpResponseData<ErpProductImageDataModel>> GetProductImageFromErpAsync(
        ErpGetRequestModel erpRequest
    );

    Task<ErpResponseData<IList<ErpProductImageDataModel>>> GetProductImagesFromErpAsync(
        ErpGetRequestModel erpRequest
    );
}
