using NopStation.Plugin.B2B.ERPIntegrationCore.Model;

namespace NopStation.Plugin.Misc.B2B.SapIntegration.Services;

public interface IB2BShipToAddressService
{
    Task<ErpResponseData<IList<ErpShipToAddressDataModel>>> GetShipToAddressFromErpAsync(ErpGetRequestModel erpRequest);
}
