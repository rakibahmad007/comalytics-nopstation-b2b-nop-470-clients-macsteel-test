using System.Collections.Generic;
using System.Threading.Tasks;
using NopStation.Plugin.B2B.ERPIntegrationCore.Domain;
using NopStation.Plugin.B2B.ERPIntegrationCore.Model;

namespace NopStation.Plugin.B2B.ERPIntegrationCore.ErpInterface;

public interface IErpIntegrationSalesOrgService
{
    Task<ErpResponseModel> GetSalesOrgsFromErpAsync(ErpGetRequestModel erpRequest);

    Task<ErpResponseModel> GetSalesWarehouseFromErpAsync(ErpGetRequestModel erpRequest);
    Task<ErpResponseData<IList<ErpAreaCodeResponseModel>>> GetAreaCodesForSalesOrgAsLocationAsync(ErpGetRequestModel erpRequest);
}
