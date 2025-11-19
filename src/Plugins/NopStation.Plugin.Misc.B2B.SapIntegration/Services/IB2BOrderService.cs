using NopStation.Plugin.B2B.ERPIntegrationCore.Model;

namespace NopStation.Plugin.Misc.B2B.SapIntegration.Services;
public interface IB2BOrderService
{
    Task<ErpResponseModel> CreateOrderOnErpAsync(ErpPlaceOrderDataModel erpPlaceOrderDataModel);
    Task<ErpResponseData<IList<ErpPlaceOrderDataModel>>> GetOrderByAccountFromErpAsync(ErpGetRequestModel erpRequest);
}
