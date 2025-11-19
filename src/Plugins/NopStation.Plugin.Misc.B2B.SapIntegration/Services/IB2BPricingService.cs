using NopStation.Plugin.B2B.ERPIntegrationCore.Model;

namespace NopStation.Plugin.Misc.B2B.SapIntegration.Services;
public interface IB2BPricingService
{
    Task<ErpResponseData<IList<ErpPriceSpecialPricingDataModel>>> GetPerAccountProductPricingFromErpAsync(ErpGetRequestModel erpRequest);
    Task<ErpResponseData<IList<ErpPriceGroupPricingDataModel>>> GetProductGroupPricingFromErpAsync(ErpGetRequestModel erpRequest);
}
