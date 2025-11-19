using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Orders;
using NopStation.Plugin.B2B.ERPIntegrationCore.Domain;

namespace Nop.Plugin.Misc.ErpWebhook.Services.Interfaces;

public interface IErpWebhookService
{
    Task<int> GetSalesOrganisationIdAsync(string location);

    Task<ErpAccount> GetErpAccountAsync(string location, string accountNo);

    Task<Dictionary<string, int>> GetERPAccountIdsAsync(List<string> accountNos, string location);

    Dictionary<string, int> GetProductIds(List<string> skus);

    Task<Address> GetBillingAddressByNopOrderIdAsync(int nopOrderId);

    Task<Address> GetShippingAddressByNopOrderIdAsync(int nopOrderId);

    Task<int?> GetCountryIdByTwoOrThreeLetterIsoCodeAsync(string twoOrThreeLetterIsoCode);

    Task<int?> GetStateProvinceIdByCountryIdAndAbbreviationAsync(int countryId, string abbreviation);

    Task<Dictionary<string, int>> GetOrCreateProductsAsync(List<string> productSkus, Action<Product> fieldMapper);

    Task<int?> GetDefaultShipToAsync(int accountId);

    Task<List<OrderItem>> GetOrderLinesByNopOrderIdAsync(int orderId);

    Task<ErpOrderItemAdditionalData> GetERPOrderItemByERPOrderLineNumberAndNopOrderIdAndProductIdAsync(string erpOrderLineNumber, int nopOrderId, int productId);

    Task<OrderItem> GetNopOrderItemByOrderIdAndProductIdAsync(int nopOrderId, int productId);

    Task DeleteOrderLinesByListAsync(List<int> orderItemIds);

    Task<ErpWebhookConfig> LoadErpWebhookConfigsFromJsonAsync();

    Task<IList<string>> GetWareHouseCodesBySalesOrgCodeAsync(string salesOrgCode, bool isB2CWarehouse = false);

    Task<ErpOrderItemAdditionalData> GetErpOrderItemByERPOrderLineNumberAndNopOrderIdAndProductIdAsync(string lineNumber, int nopOrderId, int productId);

    bool StringToBool(string value);
}
