using NopStation.Plugin.B2B.ERPIntegrationCore.Model;

namespace NopStation.Plugin.Misc.B2B.SapIntegration.Services;
public interface IB2BSalesOrgService
{
    Task<ErpResponseData<IList<ErpAreaCodeResponseModel>>> GetAreaCodesFromErpAsync(ErpGetRequestModel erpRequest);
}
