using System.Threading.Tasks;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Shipping;
using NopStation.Plugin.B2B.ERPIntegrationCore.Model;

namespace NopStation.Plugin.B2B.ERPIntegrationCore.ErpInterface;

public interface IErpIntegrationShippingService
{
    Task<ShippingOption> GetShippingCostFromERPAsync(
        decimal totalWeightInKgs,
        Customer currentCustomer,
        Address shippingAddress
    );

    Task<ErpResponseModel> GetShippingRateFromERPAsync(ErpGetRequestModel erpRequest);
}
