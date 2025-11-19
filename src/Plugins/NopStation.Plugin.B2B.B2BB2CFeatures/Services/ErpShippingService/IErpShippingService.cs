using System.Collections.Generic;
using System.Threading.Tasks;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Orders;
using NopStation.Plugin.B2B.ERPIntegrationCore.Domain;

namespace NopStation.Plugin.B2B.B2BB2CFeatures.Services.ErpShippingService
{
    public interface IErpShippingService
    {
        Task<decimal?> GetB2CShippingCostAsync(IList<ShoppingCartItem> cart, Customer customer, ErpShipToAddress erpShipToAddress);
    }
}