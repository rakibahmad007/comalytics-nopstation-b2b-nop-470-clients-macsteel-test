using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Directory;
using Nop.Core.Domain.Orders;
using Nop.Core.Infrastructure;
using Nop.Data;
using Nop.Plugin.Misc.ErpWebhook.Services.Interfaces;
using Nop.Services.Logging;
using NopStation.Plugin.B2B.ERPIntegrationCore.Domain;
using NopStation.Plugin.B2B.ERPIntegrationCore.Enums;
using NopStation.Plugin.B2B.ERPIntegrationCore.Services;

namespace Nop.Plugin.Misc.ErpWebhook.Services;

public class ErpWebhookService : IErpWebhookService
{
    #region Fields

    private readonly IErpSalesOrgService _erpSalesOrgService;
    private readonly IRepository<ErpAccount> _b2BAccountRepo;
    private readonly ILogger _logger;
    private readonly IRepository<Product> _productRepo;
    private readonly IRepository<Address> _addressRepo;
    private readonly IRepository<Order> _orderRepo;
    private readonly IRepository<OrderItem> _orderItemRepo;
    private readonly IRepository<ErpOrderItemAdditionalData> _b2BOrderItemRepo;
    private readonly IRepository<ErpOrderItemAdditionalData> _b2COrderItemRepo;
    private readonly IRepository<ErpWarehouseSalesOrgMap> _b2BSalesOrgWarehouseRepo;
    private readonly IErpLogsService _erpLogsService;
    private readonly IRepository<Country> _countryRepo;
    private readonly IRepository<StateProvince> _stateprovinceRepo;
    private readonly IRepository<ErpShipToAddress> _b2BShipToAddressRepo;
    private readonly IRepository<ErpShiptoAddressErpAccountMap> _b2BShipToAddressErpAccountMapRepo;
    private readonly INopFileProvider _fileProvider;
    private readonly INopDataProvider _nopDataProvider;

    #endregion

    #region Ctor

    public ErpWebhookService(IErpSalesOrgService b2BSalesOrganisationService,
         IRepository<ErpAccount> b2BAccountRepo,
         ILogger logger,
         IRepository<Product> productRepo,
         IRepository<Address> addressRepo,
         IRepository<Order> orderRepo,
         IRepository<Country> countryRepo,
         IRepository<StateProvince> stateprovinceRepo,
         IRepository<ErpShipToAddress> b2BShipToAddressRepo,
         IRepository<OrderItem> orderItemRepo,
         IRepository<ErpOrderItemAdditionalData> b2BOrderItemRepo,
         IRepository<ErpOrderItemAdditionalData> b2COrderItemRepo,
         IRepository<ErpWarehouseSalesOrgMap> b2bSalesOrgWarehouseRepo,
         INopFileProvider fileProvider,
         INopDataProvider nopDataProvider,
         IRepository<ErpShiptoAddressErpAccountMap> b2BShipToAddressErpAccountMapRepo,
         IErpLogsService erpLogsService)
    {
        _erpSalesOrgService = b2BSalesOrganisationService;
        _b2BAccountRepo = b2BAccountRepo;
        _logger = logger;
        _productRepo = productRepo;
        _addressRepo = addressRepo;
        _orderRepo = orderRepo;
        _countryRepo = countryRepo;
        _stateprovinceRepo = stateprovinceRepo;
        _b2BShipToAddressRepo = b2BShipToAddressRepo;
        _orderItemRepo = orderItemRepo;
        _b2BOrderItemRepo = b2BOrderItemRepo;
        _b2COrderItemRepo = b2COrderItemRepo;
        _b2BSalesOrgWarehouseRepo = b2bSalesOrgWarehouseRepo;
        _fileProvider = fileProvider;
        _nopDataProvider = nopDataProvider;
        _b2BShipToAddressErpAccountMapRepo = b2BShipToAddressErpAccountMapRepo;
        _erpLogsService = erpLogsService;
    }

    #endregion

    #region Method

    public async Task<ErpAccount> GetErpAccountAsync(string location, string accountNo)
    {
        try
        {
            ArgumentNullException.ThrowIfNull(location);
            ArgumentNullException.ThrowIfNull(accountNo);

            var b2bSalesOrgId = await GetSalesOrganisationIdAsync(location);

            return await _b2BAccountRepo.Table
            .FirstOrDefaultAsync(a => accountNo == a.AccountNumber && a.ErpSalesOrgId == b2bSalesOrgId);

        }
        catch (Exception ex)
        {
            await _erpLogsService.ErrorAsync(
                $"Exception getting Erp Account for location = {location}, accountNo = {accountNo}", 
                ErpSyncLevel.Account,
                ex);
            throw;
        }
    }

    public async Task<int> GetSalesOrganisationIdAsync(string location)
    {
        var erpWebhookConfig = await LoadErpWebhookConfigsFromJsonAsync();
        var b2bSalesOrgId = erpWebhookConfig?.DefaultSalesOrgId ?? 1;

        if (!string.IsNullOrWhiteSpace(location))
        {
            var b2bSalesOrg = await _erpSalesOrgService.GetErpSalesOrgByCodeAsync(location);
            if (b2bSalesOrg != null)
                b2bSalesOrgId = b2bSalesOrg.Id;
        }

        return b2bSalesOrgId;
    }

    public async Task<Dictionary<string, int>> GetERPAccountIdsAsync(List<string> accountNos, string location)
    {
        if (accountNos.Count == 0)
        {
            return new Dictionary<string, int>();
        }

        // Await the asynchronous call to get the sales organization ID
        var salesOrgId = await GetSalesOrganisationIdAsync(location);

        // Use asynchronous execution for the database query
        return await _b2BAccountRepo.Table
            .Where(a => accountNos.Contains(a.AccountNumber) && a.ErpSalesOrgId == salesOrgId)
            .OrderBy(a => a.AccountNumber)
            .ToDictionaryAsync(a => a.AccountNumber, a => a.Id);
    }

    public Dictionary<string, int> GetProductIds(List<string> skus)
    {
        var products = _productRepo.Table
            .Where(p => skus.Contains(p.Sku))
            .Select(p => new { p.Id, p.Sku, p.Published, p.Deleted });

        try
        {
            // Happy path
            return products.ToDictionary(p => p.Sku, p => p.Id);
        }
        catch (ArgumentException)
        {
            var result = new Dictionary<string, int>();
            // If more than one product per sku is loaded, the ToDictionary() call above with fail
            foreach (var grp in products.ToLookup(p => p.Sku))
            {
                if (grp.Count() == 1)
                {
                    var product = grp.First();
                    result.Add(product.Sku, product.Id);
                }
                else
                {
                    var product = grp.OrderByDescending(p => p.Published).ThenBy(p => p.Deleted).ThenBy(p => p.Id).First();
                    result.Add(product.Sku, product.Id);
                }
            }
            return result;
        }
    }

    public async Task<Address> GetBillingAddressByNopOrderIdAsync(int nopOrderId)
    {
        if (nopOrderId < 1)
            return null;

        var query = from address in _addressRepo.Table
                    join order in _orderRepo.Table on address.Id equals order.BillingAddressId
                    where order.Id == nopOrderId
                    select address;

        return await query.FirstOrDefaultAsync();
    }

    public async Task<Address> GetShippingAddressByNopOrderIdAsync(int nopOrderId)
    {
        if (nopOrderId < 1)
            return null;

        var query = from address in _addressRepo.Table
                    join order in _orderRepo.Table on address.Id equals order.ShippingAddressId
                    where order.Id == nopOrderId
                    select address;

        return await query.FirstOrDefaultAsync();
    }


    public async Task<int?> GetCountryIdByTwoOrThreeLetterIsoCodeAsync(string twoOrThreeLetterIsoCode)
    {
        var country = _countryRepo.Table
            .Where(c => c.TwoLetterIsoCode == twoOrThreeLetterIsoCode || c.ThreeLetterIsoCode == twoOrThreeLetterIsoCode)
            .FirstOrDefault();
        if (country == null)
        {
            if (!string.IsNullOrWhiteSpace(twoOrThreeLetterIsoCode))
            {
                await _erpLogsService.ErrorAsync(
                    $"Country with code {twoOrThreeLetterIsoCode} does not exist. Return null instead",
                    ErpSyncLevel.ShipToAddress);
            }
            return null;
        }
        return country.Id;
    }

    public async Task<int?> GetStateProvinceIdByCountryIdAndAbbreviationAsync(int countryId, string abbreviation)
    {
        // Use async to query the database for the state/province
        var stateProvince = await _stateprovinceRepo.Table
            .Where(s => s.Abbreviation.Equals(abbreviation) && s.CountryId == countryId)
            .FirstOrDefaultAsync(); // Async operation to fetch the first match

        if (stateProvince == null)
        {
            _logger.Error($"StateProvince with code {abbreviation} does not exist. Returning null instead");
            return null;
        }

        return stateProvince.Id;
    }

    public async Task<Dictionary<string, int>> GetOrCreateProductsAsync(List<string> productSkus, Action<Product> fieldMapper)
    {
        try
        {
            if (productSkus.Count == 0)
            {
                // Orders without lines are a thing
                return new Dictionary<string, int>(0);
            }

            var existing = _productRepo.Table
                .Where(p => productSkus.Contains(p.Sku))
                .Select(p => new { p.Id, p.Sku, p.Published, p.Deleted }) // no need to load anything else
                .ToLookup(p => p.Sku)
                .Select(g => g.OrderByDescending(x => x.Published).ThenBy(x => x.Deleted).ThenBy(x => x.Id).First())
                .ToDictionary(p => p.Sku, p => p.Id);

            var missing = productSkus.Except(existing.Keys).ToList();

            if (missing.Count > 0)
            {
                var newProducts = new List<Product>();
                foreach (string sku in missing)
                {
                    var now = DateTime.UtcNow;
                    var newProduct = new Product
                    {
                        Sku = sku,
                        Published = false,
                        CreatedOnUtc = now,
                        UpdatedOnUtc = now,
                    };
                    fieldMapper(newProduct); // Apply the fieldMapper action
                    newProducts.Add(newProduct);
                }

                _logger.Information($"Creating products: {string.Join(", ", newProducts.Select(p => p.Sku))}");

                // Insert new products asynchronously
                await _productRepo.InsertAsync(newProducts);

                // Add the newly created products to the existing dictionary
                foreach (var product in newProducts)
                {
                    existing.Add(product.Sku, product.Id);
                }
            }

            return existing;
        }
        catch (OutOfMemoryException ex)
        {
            _logger.Error($"{productSkus.Count} productSkus", ex);
            throw;
        }
    }

    public async Task<int?> GetDefaultShipToAsync(int accountId)
    {
        var shipTo = await (from map in _b2BShipToAddressErpAccountMapRepo.Table
                            join address in _b2BShipToAddressRepo.Table
                                on map.ErpShiptoAddressId equals address.Id
                            where map.ErpAccountId == accountId
                            select address)
                            .FirstOrDefaultAsync();

        if (shipTo == null)
        {
            _logger.Error($"Account {accountId} has no ship-to address. Returning null instead");
            return null;
        }

        return shipTo.Id;
    }

    public async Task<List<OrderItem>> GetOrderLinesByNopOrderIdAsync(int orderId)
    {
        return await _orderItemRepo.Table
            .Where(oi => oi.OrderId == orderId)
            .OrderBy(oi => oi.Id)
            .ToListAsync();  // Use ToListAsync to perform the operation asynchronously
    }

    public async Task<OrderItem> GetNopOrderItemByOrderIdAndProductIdAsync(int nopOrderId, int productId)
    {
        return await _orderItemRepo.Table
            .Where(x => x.OrderId == nopOrderId && x.ProductId == productId)
            .FirstOrDefaultAsync();  // Use FirstOrDefaultAsync to perform the operation asynchronously
    }

    public async Task DeleteOrderLinesByListAsync(List<int> orderItemIds)
    {
        if (orderItemIds == null || !orderItemIds.Any())
        {
            await _logger.WarningAsync("No order item IDs provided for deletion.");
            return;
        }

        var tobeDeletedOrderItemIds = string.Join(",", orderItemIds);
        await _logger.InformationAsync($"Want to delete order item IDs = {tobeDeletedOrderItemIds}");

        var deleteB2BOrderItemSQL = $"DELETE FROM [B2BOrderItem] WHERE [NopOrderItemId] IN ({tobeDeletedOrderItemIds})";
        var deleteNopOrderItemSQL = $"DELETE FROM [OrderItem] WHERE [Id] IN ({tobeDeletedOrderItemIds})";

        try
        {
            await _nopDataProvider.ExecuteNonQueryAsync(deleteB2BOrderItemSQL);
            await _nopDataProvider.ExecuteNonQueryAsync(deleteNopOrderItemSQL);

            await _logger.InformationAsync($"Successfully deleted order item IDs = {tobeDeletedOrderItemIds}");
        }
        catch (Exception ex)
        {
            await _logger.ErrorAsync($"Error while deleting order item IDs = {tobeDeletedOrderItemIds}: {ex.Message}", ex);
            throw;
        }
    }

    public async Task<ErpWebhookConfig> LoadErpWebhookConfigsFromJsonAsync()
    {
        var filePath = _fileProvider.MapPath(ErpWebhookDefaults.PluginsInfoFilePath);

        var text = _fileProvider.FileExists(filePath) ? await _fileProvider.ReadAllTextAsync(filePath, Encoding.UTF8) : string.Empty;
        if (string.IsNullOrEmpty(text))
            return new ErpWebhookConfig();

        try
        {
            var erpWebhookConfig = JsonConvert.DeserializeObject<ErpWebhookConfig>(text);

            return erpWebhookConfig;
        }
        catch (Exception ex)
        {
            await _logger.ErrorAsync($"Error while deserializing erp Webhook Config : {ex.Message}", ex);
            return new ErpWebhookConfig();
        }
    }

    public async Task<IList<string>> GetWareHouseCodesBySalesOrgCodeAsync(string salesOrgCode, bool isB2CWarehouse = false)
    {
        var salesOrgId = await GetSalesOrganisationIdAsync(salesOrgCode);

        var codes = await _b2BSalesOrgWarehouseRepo.Table
            .Where(map => map.ErpSalesOrgId == salesOrgId
                          && map.IsB2CWarehouse == isB2CWarehouse)
            .Select(map => map.WarehouseCode)
            .Distinct()
            .ToListAsync();

        return codes;
    }

    public async Task<ErpOrderItemAdditionalData> GetERPOrderItemByERPOrderLineNumberAndNopOrderIdAndProductIdAsync(string erpOrderLineNumber, int nopOrderId, int productId)
    {
        var b2bOrderItem = await (from bi in _b2BOrderItemRepo.Table
                                  join oi in _orderItemRepo.Table on bi.NopOrderItemId equals oi.Id
                                  where (bi.ErpOrderLineNumber.Equals(erpOrderLineNumber)) && (oi.OrderId == nopOrderId) && (oi.ProductId == productId)
                                  select bi)
                                   .FirstOrDefaultAsync();  // Use FirstOrDefaultAsync for async query execution

        return b2bOrderItem;
    }

    public async Task<ErpOrderItemAdditionalData> GetErpOrderItemByERPOrderLineNumberAndNopOrderIdAndProductIdAsync(string lineNumber, int nopOrderId, int productId)
    {
        var b2cOrderItem = await (from bi in _b2COrderItemRepo.Table
                                  join oi in _orderItemRepo.Table on bi.NopOrderItemId equals oi.Id
                                  where (bi.ErpOrderLineNumber.Equals(lineNumber)) && (oi.OrderId == nopOrderId) && (oi.ProductId == productId)
                                  select bi)
                                   .FirstOrDefaultAsync();

        return b2cOrderItem;
    }

    public bool StringToBool(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        // Convert the string to lower case for case-insensitive comparison
        value = value.Trim().ToLower();

        // Check for different representations of truthy values
        if (value == "1" || value == "true" || value == "y" || value == "yes")
        {
            return true;
        }
        // Check for different representations of falsy values
        else if (value == "0" || value == "false" || value == "n" || value == "no")
        {
            return false;
        }
        else
        {
            return false;
        }
    }

    #endregion
}
