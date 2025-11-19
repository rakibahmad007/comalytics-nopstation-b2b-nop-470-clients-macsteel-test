using System.Text.RegularExpressions;
using Nop.Core;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Directory;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Payments;
using Nop.Core.Domain.Shipping;
using Nop.Core.Domain.Stores;
using Nop.Data;
using Nop.Services.Common;
using Nop.Services.Directory;
using Nop.Services.Orders;
using NopStation.Plugin.B2B.B2BB2CFeatures;
using NopStation.Plugin.B2B.ErpDataScheduler.Services.SyncLogServices;
using NopStation.Plugin.B2B.ErpDataScheduler.Services.SyncWorkflowMessage;
using NopStation.Plugin.B2B.ERPIntegrationCore.Domain;
using NopStation.Plugin.B2B.ERPIntegrationCore.Enums;
using NopStation.Plugin.B2B.ERPIntegrationCore.Model;
using NopStation.Plugin.B2B.ERPIntegrationCore.Services;

namespace NopStation.Plugin.B2B.ErpDataScheduler.Services.SyncTaskServices;

public class ErpOrderSyncService : IErpOrderSyncService
{
    #region Fields

    private readonly IOrderService _orderService;
    private readonly IStoreContext _storeContext;
    private readonly ICountryService _countryService;
    private readonly IAddressService _addressService;
    private readonly ICurrencyService _currencyService;
    private readonly CurrencySettings _currencySettings;
    private readonly IStateProvinceService _stateProvinceService;
    private readonly ISyncLogService _erpSyncLogService;
    private readonly IErpAccountService _erpAccountService;
    private readonly IErpSalesOrgService _erpSalesOrgService;
    private readonly B2BB2CFeaturesSettings _b2BB2CFeaturesSettings;
    private readonly ErpDataSchedulerSettings _erpDataSchedulerSettings;
    private readonly IErpShipToAddressService _erpShipToAddressService;
    private readonly IErpIntegrationPluginManager _erpIntegrationPluginService;
    private readonly IErpOrderAdditionalDataService _erpOrderAdditionalDataService;
    private readonly IErpOrderItemAdditionalDataService _erpOrderItemAdditionalDataService;
    private readonly ISyncWorkflowMessageService _syncWorkflowMessageService;
    private readonly OrderSettings _orderSettings;
    private readonly IRepository<Product> _productRepository;
    private readonly IRepository<OrderItem> _orderItemRepository;
    private static Regex _lineBreakReplacer = new Regex(@"\r?\n");
    private string NopOrderPrefix { get; set; } = "";
    private static string ExternalOrderNumberPrefix => "E-";
    private bool _isManualTriggerForCurrentRun;

    private static Dictionary<string, int> _orderStatusMap = new(StringComparer.InvariantCultureIgnoreCase)
    {
        { "Open", (int) OrderStatus.Processing },
        { "Released", (int) OrderStatus.Processing },
        { "Pending_Approval", (int) OrderStatus.Processing },
        { "Pending_Prepayment", (int) OrderStatus.Processing },
    };

    private static int? ParseOrderStatus(string? input)
    {
        input = input?.Trim();
        if (input != null && _orderStatusMap.TryGetValue(input, out var value))
        {
            return value;
        }
        return null;
    }

    #endregion

    #region Ctor

    public ErpOrderSyncService(IOrderService orderService,
        IStoreContext storeContext,
        ICountryService countryService,
        IAddressService addressService,
        ICurrencyService currencyService,
        CurrencySettings currencySettings,
        IStateProvinceService stateProvinceService,
        ISyncLogService erpSyncLogService,
        IErpAccountService erpAccountService,
        IErpSalesOrgService erpSalesOrgService,
        B2BB2CFeaturesSettings b2BB2CFeaturesSettings,
        ErpDataSchedulerSettings erpDataSchedulerSettings,
        IErpShipToAddressService erpSShipToAddressService,
        IErpIntegrationPluginManager erpIntegrationPluginService,
        IErpOrderAdditionalDataService erpOrderAdditionalDataService,
        IErpOrderItemAdditionalDataService erpOrderItemAdditionalDataService,
        ISyncWorkflowMessageService syncWorkflowMessageService,
        OrderSettings orderSettings,
        IRepository<Product> productRepository,
        IRepository<OrderItem> orderItemRepository)
    {
        _orderService = orderService;
        _storeContext = storeContext;
        _countryService = countryService;
        _addressService = addressService;
        _currencyService = currencyService;
        _currencySettings = currencySettings;
        _stateProvinceService = stateProvinceService;
        _erpSyncLogService = erpSyncLogService;
        _erpAccountService = erpAccountService;
        _erpSalesOrgService = erpSalesOrgService;
        _b2BB2CFeaturesSettings = b2BB2CFeaturesSettings;
        _erpDataSchedulerSettings = erpDataSchedulerSettings;
        _erpShipToAddressService = erpSShipToAddressService;
        _erpIntegrationPluginService = erpIntegrationPluginService;
        _erpOrderAdditionalDataService = erpOrderAdditionalDataService;
        _erpOrderItemAdditionalDataService = erpOrderItemAdditionalDataService;
        _syncWorkflowMessageService = syncWorkflowMessageService;
        _orderSettings = orderSettings;
        _productRepository = productRepository;
        _orderItemRepository = orderItemRepository;
    }

    #endregion

    #region Utilities

    public int? GetCountryId(string twoOrThreeLetterIsoCode, List<Country> allCountries)
    {
        var country = allCountries.Find(c =>
                c.TwoLetterIsoCode == twoOrThreeLetterIsoCode ||
                c.ThreeLetterIsoCode == twoOrThreeLetterIsoCode);
        if (country == null)
        {
            return null;
        }
        return country.Id;
    }

    public int? GetStateProvinceId(int countryId, string abbreviation, List<StateProvince> allStateProvinces)
    {
        var stateProvince = allStateProvinces.Find(s =>
            s.CountryId == countryId &&
            s.Abbreviation == abbreviation);
        if (stateProvince == null)
        {
            return null;
        }
        return stateProvince.Id;
    }

    public static string NormalizeProductDescription(string description)
    {
        if (!string.IsNullOrEmpty(description) && description.Length > 400)
        {
            description = description.Substring(0, 400);
        }
        return description;
    }

    public static string NormalizeProductFullDescription(string fullDescription)
    {
        if (!string.IsNullOrEmpty(fullDescription))
        {
            if (fullDescription.Length > 400)
            {
                fullDescription = fullDescription.Substring(0, 400);
            }

            fullDescription = _lineBreakReplacer.Replace(fullDescription, "<br/>");
        }
        return fullDescription;
    }

    private ErpOrderType ParseOrderType(string orderType, bool isB2COrder)
    {
        orderType = orderType?.Trim().ToLower() ?? string.Empty;

        if (orderType.Contains("quote") || orderType.Contains("q"))
        {
            if (orderType.Contains("b2c"))
            {
                return ErpOrderType.B2CQuote;
            }
            return ErpOrderType.B2BQuote;
        }
        else
        {
            if (isB2COrder)
            {
                return ErpOrderType.B2CSalesOrder;
            }
            return ErpOrderType.B2BSalesOrder;
        }        
    }

    private async Task MapBillingAddressAsync(ErpPlaceOrderDataModel erpOrder,
        Address address,
        List<Country> allCountries,
        List<StateProvince> allStateProvinces)
    {
        if (address.Id < 0)
        {
            address.CreatedOnUtc = DateTime.UtcNow;
        }
        address.FirstName = erpOrder.BillingAddress?.Name;
        address.Email = erpOrder.BillingAddress?.Email;
        address.PhoneNumber = erpOrder.BillingAddress?.PhoneNumber;
        address.Address1 = erpOrder.BillingAddress?.Address1 ?? erpOrder.Address1;
        address.Address2 = erpOrder.BillingAddress?.Address2 ?? erpOrder.Address2;
        address.City = erpOrder.BillingAddress?.City ?? erpOrder.Address3;
        address.ZipPostalCode = erpOrder.BillingAddress?.ZipPostalCode ?? erpOrder.PostalCode;
        address.CountryId = GetCountryId(
            erpOrder.BillingAddress?.Country ?? erpOrder.AddressCountryCode,
            allCountries);
        if (address.CountryId.HasValue)
        {
            address.StateProvinceId = GetStateProvinceId(address.CountryId.Value,
                erpOrder.AddressProvince,
                allStateProvinces);
        }
    }

    private async Task MapShippingAddressAsync(ErpPlaceOrderDataModel erpOrder,
        Address address,
        List<Country> allCountries,
        List<StateProvince> allStateProvinces)
    {
        if (address.Id < 0)
        {
            address.CreatedOnUtc = DateTime.UtcNow;
        }
        address.FirstName = erpOrder.ShippingAddress?.Name;
        address.Email = erpOrder.ShippingAddress?.Email;
        address.PhoneNumber = erpOrder.ShippingAddress?.PhoneNumber;
        address.Address1 = erpOrder.ShippingAddress?.Address1 ?? erpOrder.DelAddress1;
        address.Address2 = erpOrder.ShippingAddress?.Address2 ?? erpOrder.DelAddress2;
        address.City = erpOrder.ShippingAddress?.City ?? erpOrder.DelAddress3;
        address.ZipPostalCode = erpOrder.ShippingAddress?.ZipPostalCode ?? erpOrder.DelPostalCode;
        address.CountryId = GetCountryId(
            erpOrder.ShippingAddress?.Country ?? erpOrder.DelCountryCode,
            allCountries);
        if (address.CountryId.HasValue)
        {
            address.StateProvinceId = GetStateProvinceId(address.CountryId.Value,
                erpOrder.ShippingAddress?.StateProvince ?? erpOrder.DelProvince,
                allStateProvinces);
        }
    }

    private async Task<Dictionary<string, int>> GetOrCreateProductsAsync(List<string> productSkus, Action<Product> fieldMapper)
    {
        if (productSkus == null || productSkus.Count == 0)
        {
            return new Dictionary<string, int>(0);
        }

        var pos = 0;
        var chunk = 1000;
        var products = new Dictionary<string, int>();

        while (productSkus.Skip(pos).Any())
        {
            var splitedProductSkus = productSkus.Skip(pos).Take(chunk);
            pos += chunk;

            var existing = _productRepository.Table
                .Where(p => splitedProductSkus.Contains(p.Sku))
                .Select(p => new { p.Id, p.Sku, p.Published, p.Deleted })
                .ToLookup(p => p.Sku)
                .Select(g => g.OrderByDescending(x => x.Published)
                    .ThenBy(x => x.Deleted)
                    .ThenBy(x => x.Id).First())
                .ToDictionary(p => p.Sku, p => p.Id);

            var missing = splitedProductSkus.Except(existing.Keys).ToList();
            if (missing.Count > 0)
            {
                var newProducts = new List<Product>();
                foreach (var sku in missing)
                {
                    var now = DateTime.UtcNow;
                    var newProduct = new Product
                    {
                        Sku = sku,
                        Published = false,
                        CreatedOnUtc = now,
                        UpdatedOnUtc = now,
                    };
                    fieldMapper(newProduct);
                    newProducts.Add(newProduct);
                }

                await _productRepository.InsertAsync(newProducts);

                newProducts.ForEach(p => existing.Add(p.Sku, p.Id));
            }

            existing.ToList().ForEach(x => products.Add(x.Key, x.Value));
        }

        return products;
    }

    private async Task MapOrderFieldsAsync(ErpPlaceOrderDataModel erpOrder, Order oldNopOrder)
    {
        var now = DateTime.UtcNow;
        var orderStatusId = ParseOrderStatus(erpOrder.Status);
        if (orderStatusId != null)
        {
            oldNopOrder.OrderStatusId = orderStatusId.Value;
        }
        oldNopOrder.OrderGuid = Guid.NewGuid();
        oldNopOrder.VatNumber = erpOrder.VatNumber;
        oldNopOrder.PaymentStatusId = (int)PaymentStatus.Paid;
        oldNopOrder.ShippingStatusId = (int)ShippingStatus.Delivered;
        oldNopOrder.OrderShippingExclTax = 0;
        oldNopOrder.OrderShippingInclTax = 0;
        oldNopOrder.OrderSubtotalExclTax = erpOrder.OrderSubtotalExclTax ?? decimal.Zero;
        oldNopOrder.OrderSubtotalInclTax = erpOrder.OrderSubtotalExclTax ?? decimal.Zero;
        oldNopOrder.OrderTotal = erpOrder.OrderSubtotalInclTax ?? decimal.Zero;
        oldNopOrder.OrderTax = erpOrder.OrderTax ?? decimal.Zero;
        oldNopOrder.PaidDateUtc = now;
        oldNopOrder.CreatedOnUtc = erpOrder.OrderDate ?? now;
        oldNopOrder.OrderShippingExclTax = erpOrder.ShippingFees;
    }

    private async Task<OrderItem> MapOrderItemFields(ErpPlaceOrderItemDataModel erpLine, OrderItem oi, Dictionary<string, int> productIds)
    {
        if (!productIds.TryGetValue(erpLine.Sku, out var productId))
        {
            return null;
        }

        oi.Quantity = (int)(erpLine.Quantity ?? 0);
        oi.ProductId = productId;
        oi.OriginalProductCost = 0;
        oi.ItemWeight = erpLine.Weight;
        oi.OrderItemGuid = Guid.NewGuid();
        oi.ProductId = productId;
        oi.PriceInclTax = erpLine.PriceInclTax ?? 0;
        oi.PriceExclTax = erpLine.PriceExclTax ?? 0;
        oi.UnitPriceInclTax = erpLine.UnitPriceInclTax ?? 0;
        oi.UnitPriceExclTax = erpLine.UnitPriceExclTax ?? 0;
        oi.DiscountAmountInclTax = erpLine.DiscountAmountInclTax.HasValue && erpLine.DiscountAmountInclTax.Value < 0 ?
            erpLine.DiscountAmountInclTax.Value * (-1) :
            erpLine.DiscountAmountInclTax ?? 0;
        oi.DiscountAmountExclTax = erpLine.DiscountAmountExclTax.HasValue && erpLine.DiscountAmountExclTax.Value < 0 ?
            erpLine.DiscountAmountExclTax.Value * (-1) :
            erpLine.DiscountAmountExclTax ?? 0;

        return oi;
    }

    private async Task MapErpOrderAdditionalDataFields(ErpPlaceOrderDataModel erpOrder,
        ErpOrderAdditionalData erpOrderAdditionalData,
        bool isB2BUser)
    {
        var now = DateTime.UtcNow;

        erpOrderAdditionalData.ErpOrderTypeId = (int)ParseOrderType(erpOrder.OrderType, isB2BUser);
        erpOrderAdditionalData.ErpOrderNumber = string.IsNullOrWhiteSpace(erpOrderAdditionalData.ErpOrderNumber) ?
            erpOrder.ErpOrderNumber :
            erpOrderAdditionalData.ErpOrderNumber;
        erpOrderAdditionalData.CustomerReference = erpOrder.CustomerReference;
        erpOrderAdditionalData.SpecialInstructions = erpOrder.DeliveryInstruction;
        erpOrderAdditionalData.CashRounding = erpOrder.CashRounding ?? decimal.Zero;
        erpOrderAdditionalData.ERPOrderStatus = erpOrder.Status;
        erpOrderAdditionalData.DeliveryDate = erpOrder.DeliveryDate;
        erpOrderAdditionalData.IntegrationStatusTypeId = erpOrderAdditionalData.IntegrationStatusTypeId == 0 ?
            (int)IntegrationStatusType.Confirmed :
            erpOrderAdditionalData.IntegrationStatusTypeId;
        erpOrderAdditionalData.IntegrationError = string.IsNullOrWhiteSpace(erpOrderAdditionalData.IntegrationError) ?
            "" :
            erpOrderAdditionalData.IntegrationError;
        erpOrderAdditionalData.IntegrationRetries = erpOrderAdditionalData.IntegrationRetries <= 0 ?
            0 :
            erpOrderAdditionalData.IntegrationRetries;
        erpOrderAdditionalData.IntegrationErrorDateTimeUtc = erpOrderAdditionalData.IntegrationErrorDateTimeUtc != null ?
            erpOrderAdditionalData.IntegrationErrorDateTimeUtc :
            null;
        erpOrderAdditionalData.IsShippingAddressModified = false;
        erpOrderAdditionalData.ErpOrderPlaceByCustomerTypeId = isB2BUser ? (int)ErpUserType.B2BUser : (int)ErpUserType.B2CUser;
        erpOrderAdditionalData.QuoteExpiryDate = erpOrder.QuoteExpiryDate;
        erpOrderAdditionalData.LastERPUpdateUtc = now;

        if (erpOrderAdditionalData.Id < 0)
        {
            erpOrderAdditionalData.ChangedOnUtc = now;
            erpOrderAdditionalData.ChangedById = 1;
        }
    }

    private async Task MapErpOrderAdditionalItemFields(ErpPlaceOrderItemDataModel erpLine, ErpOrderItemAdditionalData erpOrderItem)
    {
        var now = DateTime.UtcNow;
        erpOrderItem.ErpOrderLineNumber = erpLine.ErpOrderLineNumber;
        erpOrderItem.ErpSalesUoM = erpLine.ErpSalesUoM;
        erpOrderItem.ErpOrderLineNotes = string.Empty;
        erpOrderItem.ErpOrderLineStatus = erpLine.ErpOrderLineStatus;
        erpOrderItem.ErpDeliveryMethod = erpLine.DeliveryMethod;
        erpOrderItem.ErpInvoiceNumber = erpLine.InvoiceNumber;
        erpOrderItem.ErpDateRequired = erpLine.DateRequired;
        erpOrderItem.ErpDateExpected = erpLine.DeliveryDate;
        erpOrderItem.LastErpUpdateUtc = now;
        erpOrderItem.ChangedOnUtc = now;
        erpOrderItem.ChangedBy = 1;
    }

    #endregion

    #region Method

    public virtual async Task<bool> IsErpOrderSyncSuccessfulAsync(string? erpAccountNumber = null,
        string? orderNumber = null,
        string? salesOrgCode = null,
        bool isManualTrigger = false,
        bool isIncrementalSync = true,
        CancellationToken cancellationToken = default)
    {
        // This parameter and its usage is for testing purpose only.
        //_isManualTriggerForCurrentRun = isManualTrigger;
        _isManualTriggerForCurrentRun = true;

        var erpIntegrationPlugin = await _erpIntegrationPluginService.LoadActiveERPIntegrationPlugin();

        var syncTaskName = isIncrementalSync ? 
            ErpDataSchedulerDefaults.ErpOrderIncrementalSyncTaskName : 
            ErpDataSchedulerDefaults.ErpOrderSyncTaskName;

        if (erpIntegrationPlugin is null)
        {
            await _erpSyncLogService.SyncLogSaveOnFileAsync(
                syncTaskName,
                ErpSyncLevel.Order,
                $"No integration method found. Unable to run {syncTaskName}.");

            return false;
        }

        try
        {
            #region Data collection

            var salesOrgs = new List<ErpSalesOrg>();

            if (string.IsNullOrWhiteSpace(salesOrgCode))
            {
                salesOrgs = (await _erpSalesOrgService.GetErpSalesOrgsAsync(isActive: true, filterOutDeleted: true)).ToList();
            }
            else
            {
                var salesOrg = await _erpSalesOrgService.GetErpSalesOrgByCodeAsync(salesOrgCode);
                if (salesOrg != null)
                {
                    salesOrgs.Add(salesOrg);
                }
            }

            if (salesOrgs.Count == 0)
            {
                await _erpSyncLogService.SyncLogSaveOnFileAsync(
                    syncTaskName,
                    ErpSyncLevel.Order,
                    $"No Sales org found. Unable to run {syncTaskName}.");

                return false;
            }

            IList<ErpAccount> specificErpAccounts = null;
            if (!string.IsNullOrWhiteSpace(salesOrgCode))
            {
                if (!string.IsNullOrWhiteSpace(erpAccountNumber))
                {
                    specificErpAccounts = (await _erpAccountService.GetErpAccountsOfOnlyActiveErpNopUsersAsync
                            (salesOrgId: salesOrgs.FirstOrDefault()?.Id ?? 0, accountNumber: erpAccountNumber)).ToList();

                    if (specificErpAccounts == null || specificErpAccounts != null && specificErpAccounts.Count == 0)
                    {
                        await _erpSyncLogService.SyncLogSaveOnFileAsync(
                            syncTaskName,
                            ErpSyncLevel.Order,
                            $"No Active Erp Account found with Active Erp Nop User with Account Number: {erpAccountNumber} " +
                            $"and Sales Org: {salesOrgs.FirstOrDefault()?.Code}. " +
                            $"Unable to run {syncTaskName}.");

                        return false;
                    }
                    else
                    {
                        var accList = string.Join("|", specificErpAccounts.Select(x => x.AccountNumber));
                        await _erpSyncLogService.SyncLogSaveOnFileAsync(
                            syncTaskName,
                            ErpSyncLevel.Order,
                            $"Before Order Sync run: Accounts discovered, Sales Org: {salesOrgCode}, Account Numbers: [{accList}]");
                    }
                }
                else
                {
                    await _erpSyncLogService.SyncLogSaveOnFileAsync(
                        syncTaskName,
                        ErpSyncLevel.Order,
                        $"Order Sync will run for Sales Org: {salesOrgCode} related Accounts.");
                }
            }

            var currentStore = await _storeContext.GetCurrentStoreAsync();
            var currency = await _currencyService.GetCurrencyByIdAsync(_currencySettings.PrimaryStoreCurrencyId);
            var allCountries = (await _countryService.GetAllCountriesAsync()).ToList();
            var allStateProvinces = (await _stateProvinceService.GetStateProvincesAsync()).ToList();

            NopOrderPrefix = _orderSettings.CustomOrderNumberMask;

            if (!string.IsNullOrWhiteSpace(NopOrderPrefix))
            {
                NopOrderPrefix = NopOrderPrefix.Split('-')[0];
            }

            #endregion

            await _erpSyncLogService.SyncLogSaveOnFileAsync(
                syncTaskName,
                ErpSyncLevel.Order,
                $"Erp Order Sync started. [Current Nop order prefix: {NopOrderPrefix}]");

            foreach (var salesOrg in salesOrgs)
            {
                IList<ErpAccount> oldErpAccounts;

                if (_isManualTriggerForCurrentRun)
                {
                    await _erpSyncLogService.SyncLogSaveOnFileAsync(
                        syncTaskName,
                        ErpSyncLevel.Order,
                        $"\n\n==========================================================================\n\n" +
                        $"Order sync run started for Sales org: Id: {salesOrg.Id}, Code: {salesOrg.Code}, Name: {salesOrg.Name}\n\n");
                }

                if (specificErpAccounts != null)
                {
                    if (specificErpAccounts.FirstOrDefault(x => x.ErpSalesOrgId == salesOrg.Id) != null)
                    {
                        oldErpAccounts = specificErpAccounts;
                    }
                    else
                    {
                        continue;
                    }
                }
                else
                {
                    oldErpAccounts = (await _erpAccountService.GetErpAccountsOfOnlyActiveErpNopUsersAsync
                        (salesOrgId: salesOrg.Id)).ToList();
                }

                if (oldErpAccounts.Count == 0)
                {
                    await _erpSyncLogService.SyncLogSaveOnFileAsync(
                        syncTaskName,
                        ErpSyncLevel.Order,
                        $"No Erp Accounts found with Active Nop Users for Sales org: {salesOrg.Code}");

                    if (specificErpAccounts != null)
                        return false;

                    continue;
                }

                var isError = false;
                var totalSyncedSoFar = 0;
                var totalNotSyncedSoFar = 0;
                var lastErrorMessage = "";
                var lastErpOrderSynced = "";
                var lastErpOrderSyncedOfErpAccount = "";

                var erpAccountNumbersWithOrders = await _erpOrderAdditionalDataService
                    .CheckAccountHasOrders(salesOrg.Code, oldErpAccounts.Select(x => x.AccountNumber).ToArray());

                var defaultShipToAddressesByErpAccountId = await _erpShipToAddressService
                    .GetErpAccountIdDefaultShipToAddressMappingAsync(oldErpAccounts.Select(x => x.Id).ToArray());

                if (_isManualTriggerForCurrentRun)
                {
                    await _erpSyncLogService.SyncLogSaveOnFileAsync(
                        syncTaskName,
                        ErpSyncLevel.Order,
                        $"Data ready for order sync. Sales org Code: {salesOrg.Code}, \n" +
                        $"Total Accounts: {oldErpAccounts.Count}, \n" +
                        $"Total has-orders map: {erpAccountNumbersWithOrders.Count}, \n" +
                        $"Total default Shipto map: {defaultShipToAddressesByErpAccountId.Count}");
                }

                var nextOrders = new HashSet<string>();

                foreach (var erpAccount in oldErpAccounts)
                {
                    var start = "0";
                    DateTime? dateFrom = DateTime.Today.AddMonths(-4);

                    if (erpAccountNumbersWithOrders[erpAccount.AccountNumber])
                    {
                        if (erpAccount.LastTimeOrderSyncOnUtc.HasValue)
                        {
                            dateFrom = isIncrementalSync ? erpAccount.LastTimeOrderSyncOnUtc.Value.AddHours(-2) : dateFrom;
                        }
                        else
                        {
                            dateFrom = isIncrementalSync ? DateTime.Today.AddDays(-1) : dateFrom;
                        }
                    }

                    if (_isManualTriggerForCurrentRun)
                    {
                        await _erpSyncLogService.SyncLogSaveOnFileAsync(
                            syncTaskName,
                            ErpSyncLevel.Order,
                            $"-------------------------------------New Account-------------------------------------\n\n\n" +
                            $"Current Account orders to be synced: Sales org code: {salesOrg.Code}, " +
                            $"Account Id: {erpAccount.Id}, Number: {erpAccount.AccountNumber}, \n" +
                            $"Specific order: {(string.IsNullOrWhiteSpace(orderNumber) ? "" : orderNumber)}");
                    }

                    while (true)
                    {
                        var erpGetRequestModel = new ErpGetRequestModel
                        {
                            Start = start,
                            AccountNumber = erpAccount.AccountNumber,
                            Location = salesOrg.Code,
                            DateFrom = dateFrom,
                            OrderNumber = orderNumber
                        };

                        await _erpSyncLogService.SyncLogSaveOnFileAsync(
                            syncTaskName,
                            ErpSyncLevel.Order,
                            $"-------------------------------------New Request in while loop-------------------------------------\n\n\n" +
                            $"Request details: \n" +
                            $"Sales org code: {salesOrg.Code}, \n" +
                            $"Account Id: {erpAccount.Id}, Number: {erpAccount.AccountNumber}, \n" +
                            $"Specific order: {(string.IsNullOrWhiteSpace(orderNumber) ? "" : orderNumber)}, \n" +
                            $"Start: {erpGetRequestModel.Start}, \n" +
                            $"Date from: {(erpGetRequestModel.DateFrom.HasValue ? erpGetRequestModel.DateFrom.Value.ToString("yyyy-MM-dd HH:mm:ss") : "null")}");

                        var response = await erpIntegrationPlugin.GetOrderByAccountFromErpAsync(erpGetRequestModel);

                        await _erpSyncLogService.SyncLogSaveOnFileAsync(
                            syncTaskName,
                            ErpSyncLevel.Order,
                            "Response details: \n" +
                            $"IsError: {response.ErpResponseModel.IsError}, \n" +
                            $"ErrorShortMessage: {response.ErpResponseModel.ErrorShortMessage}, \n" +
                            $"ErrorFullMessage: {response.ErpResponseModel.ErrorFullMessage}, \n" +
                            $"Next: {response.ErpResponseModel.Next}, \n" +
                            $"Data count: {(response.Data != null ? response.Data.Count.ToString() : "0")}");

                        if (response.ErpResponseModel.IsError)
                        {
                            isError = true;
                            lastErrorMessage = $"The last error: {response.ErpResponseModel.ErrorShortMessage}";

                            await _syncWorkflowMessageService.SendSyncFailNotificationAsync(
                                DateTime.UtcNow,
                                syncTaskName,
                                response.ErpResponseModel.ErrorShortMessage + "\n\n" + response.ErpResponseModel.ErrorFullMessage);

                            if (_isManualTriggerForCurrentRun)
                            {
                                await _erpSyncLogService.SyncLogSaveOnFileAsync(
                                syncTaskName,
                                ErpSyncLevel.Order,
                                $"Response error while orders to be synced: Sales org code: {salesOrg.Code}, \n" +
                                $"Account Id: {erpAccount.Id}, Number: {erpAccount.AccountNumber}, \n");
                            }

                            break;
                        }
                        else if (response.Data is null)
                        {
                            if (_isManualTriggerForCurrentRun)
                            {
                                await _erpSyncLogService.SyncLogSaveOnFileAsync(
                                syncTaskName,
                                ErpSyncLevel.Order,
                                $"Response came with no data while orders to be synced: Sales org code: {salesOrg.Code}, \n" +
                                $"Account Id: {erpAccount.Id}, Number: {erpAccount.AccountNumber}, \n");
                            }
                            isError = false;
                            break;
                        }

                        var erpOrders = await response.Data.ToListAsync();

                        if (!string.IsNullOrWhiteSpace(orderNumber))
                        {
                            erpOrders = erpOrders.Where(x => x.CustomOrderNumber == orderNumber).ToList();
                        }

                        (lastErpOrderSynced, lastErpOrderSyncedOfErpAccount, totalSyncedSoFar, totalNotSyncedSoFar) = await MapOrderData(
                            erpOrders,
                            salesOrg,
                            erpAccount,
                            defaultShipToAddressesByErpAccountId,
                            lastErpOrderSynced,
                            lastErpOrderSyncedOfErpAccount,
                            totalSyncedSoFar,
                            totalNotSyncedSoFar,
                            allStateProvinces,
                            allCountries,
                            currency,
                            currentStore,
                            syncTaskName);

                        // If specific order was found and synced, break the loop
                        if (!string.IsNullOrWhiteSpace(orderNumber) && erpOrders.Count != 0)
                        {
                            break;
                        }

                        if (cancellationToken.IsCancellationRequested)
                        {
                            await _erpSyncLogService.SyncLogSaveOnFileAsync(
                                syncTaskName,
                                ErpSyncLevel.Order,
                                "The Erp Order Sync run is cancelled. " +
                                (!string.IsNullOrWhiteSpace(lastErpOrderSynced) ?
                                $"The last synced Erp Order: {lastErpOrderSynced}, of Erp Account: {lastErpOrderSyncedOfErpAccount} " +
                                $"for Sales Org: ({salesOrg.Code}) {salesOrg.Name}. " : string.Empty) +
                                $"Total orders synced in this session: {totalSyncedSoFar} and " +
                                $"Total orders not synced due to invalid data in this session: {totalNotSyncedSoFar}");

                            return false;
                        }

                        if (string.IsNullOrWhiteSpace(response.ErpResponseModel.Next))
                        {
                            isError = false;
                            break;
                        }
                        else if (nextOrders.Contains(response.ErpResponseModel.Next))
                        {
                            await _erpSyncLogService.SyncLogSaveOnFileAsync(
                                syncTaskName,
                                ErpSyncLevel.Order,
                                $"Response came with same next data {response.ErpResponseModel.Next} as " +
                                $"one of the previous request next data, " +
                                $"breaking the loop to avoid infinite loop. \n" +
                                $"Request details: \n" +
                                $"Sales org code: {salesOrg.Code}, \n" +
                                $"Account Id: {erpAccount.Id}, Account Number: {erpAccount.AccountNumber}, \n" +
                                $"Specific order: {(string.IsNullOrWhiteSpace(orderNumber) ? "" : orderNumber)}, \n" +
                                $"Start/Next: {erpGetRequestModel.Start}");
                            isError = false;
                            break;
                        }

                        start = response.ErpResponseModel.Next;
                        nextOrders.Add(start);
                    }

                    // Skip quote order call if specific order number is provided
                    if (string.IsNullOrWhiteSpace(orderNumber) && _erpDataSchedulerSettings.NeedQuoteOrderCall)
                    {
                        start = "0";
                        while (true)
                        {
                            var erpGetRequestModel = new ErpGetRequestModel
                            {
                                Start = start,
                                AccountNumber = erpAccount.AccountNumber,
                                Location = salesOrg.Code,
                                DateFrom = isIncrementalSync ? erpAccount.LastTimeOrderSyncOnUtc : null
                            };

                            var response = await erpIntegrationPlugin.GetQuoteByAccountFromErpAsync(erpGetRequestModel);

                            if (response.ErpResponseModel.IsError)
                            {
                                isError = true;
                                lastErrorMessage = $"The last error: {response.ErpResponseModel.ErrorShortMessage}";
                                break;
                            }
                            else if (response.Data is null)
                            {
                                isError = false;
                                break;
                            }

                            var erpOrders = await response.Data.Where(x => !string.IsNullOrWhiteSpace(x.OrderType)).ToListAsync();

                            (lastErpOrderSynced, lastErpOrderSyncedOfErpAccount, totalSyncedSoFar, totalNotSyncedSoFar) = await MapOrderData(
                                erpOrders,
                                salesOrg,
                                erpAccount,
                                defaultShipToAddressesByErpAccountId,
                                lastErpOrderSynced,
                                lastErpOrderSyncedOfErpAccount,
                                totalSyncedSoFar,
                                totalNotSyncedSoFar,
                                allStateProvinces,
                                allCountries,
                                currency,
                                currentStore,
                                syncTaskName);

                            if (cancellationToken.IsCancellationRequested)
                            {
                                await _erpSyncLogService.SyncLogSaveOnFileAsync(
                                    syncTaskName,
                                    ErpSyncLevel.Order,
                                    "The Erp Order Sync run is cancelled. " +
                                    (!string.IsNullOrWhiteSpace(lastErpOrderSynced) ?
                                    $"The last synced Erp Order: {lastErpOrderSynced}, of Erp Account: {lastErpOrderSyncedOfErpAccount} for Sales Org: ({salesOrg.Code}) {salesOrg.Name}. " : string.Empty) +
                                    $"Total orders synced in this session: {totalSyncedSoFar} and " +
                                    $"Total orders not synced due to invalid data in this session: {totalNotSyncedSoFar}");

                                return false;
                            }

                            if (response.ErpResponseModel.Next == null)
                            {
                                isError = false;
                                break;
                            }
                            else if (nextOrders.Contains(response.ErpResponseModel.Next))
                            {
                                await _erpSyncLogService.SyncLogSaveOnFileAsync(
                                    syncTaskName,
                                    ErpSyncLevel.Order,
                                    $"Response came with same next data {response.ErpResponseModel.Next} as request start data {start}, " +
                                    $"breaking the loop to avoid infinite loop. \n" +
                                    $"Request details: \n" +
                                    $"Sales org code: {salesOrg.Code}, \n" +
                                    $"Account Id: {erpAccount.Id}, Number: {erpAccount.AccountNumber}, \n" +
                                    $"Specific order: {(string.IsNullOrWhiteSpace(orderNumber) ? "" : orderNumber)}, \n" +
                                    $"Start/Next: {erpGetRequestModel.Start}");
                                isError = false;
                                break;
                            }

                            start = response.ErpResponseModel.Next;
                            nextOrders.Add(start);
                        }
                    }

                    erpAccount.LastTimeOrderSyncOnUtc = DateTime.UtcNow;
                    await _erpAccountService.UpdateErpAccountAsync(erpAccount);
                }

                if (!isError)
                {
                    await _erpSyncLogService.SyncLogSaveOnFileAsync(
                        syncTaskName,
                        ErpSyncLevel.Order,
                        $"Erp Order sync successful for Sales Org: ({salesOrg.Code}) {salesOrg.Name}");
                }
                else
                {
                    await _erpSyncLogService.SyncLogSaveOnFileAsync(
                        syncTaskName,
                        ErpSyncLevel.Order,
                        $"Erp Order sync is partially or not successful for Sales Org: ({salesOrg.Code}) {salesOrg.Name}",
                        lastErrorMessage);
                }

                await _erpSyncLogService.SyncLogSaveOnFileAsync(
                    syncTaskName,
                    ErpSyncLevel.Order,
                    (!string.IsNullOrWhiteSpace(lastErpOrderSynced) ?
                    $"The last synced Erp Order: {lastErpOrderSynced}, of Erp Account: {lastErpOrderSyncedOfErpAccount} for Sales Org: ({salesOrg.Code}) {salesOrg.Name}. " : string.Empty) +
                    $"Total synced in this session: {totalSyncedSoFar} and " +
                    $"Total orders not synced due to invalid data in this session: {totalNotSyncedSoFar}");

                nextOrders.Clear();
            }

            await _erpSyncLogService.SyncLogSaveOnFileAsync(
                syncTaskName,
                ErpSyncLevel.Order,
                "Erp Order Sync ended.");

            return true;
        }
        catch (Exception ex)
        {
            await _erpSyncLogService.SyncLogSaveOnFileAsync(
                syncTaskName,
                ErpSyncLevel.Order,
                ex.Message,
                ex.StackTrace ?? string.Empty);

            await _syncWorkflowMessageService.SendSyncFailNotificationAsync(
                DateTime.UtcNow,
                syncTaskName,
                ex.Message + "\n\n" + ex.StackTrace);

            await _erpSyncLogService.SyncLogSaveOnFileAsync(
                syncTaskName,
                ErpSyncLevel.Order,
                "Erp Order Sync ended.");

            return false;
        }
    }

    private bool IsValidEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return false;

        try
        {
            var pattern = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";
            return Regex.IsMatch(email, pattern, RegexOptions.IgnoreCase);
        }
        catch (Exception)
        {
            return false;
        }
    }

    private async Task<(string lastErpOrderSynced, string lastErpOrderSyncedOfErpAccount, int totalSyncedSoFar, int totalNotSyncedSoFar)> MapOrderData(
        IList<ErpPlaceOrderDataModel> erpOrders,
        ErpSalesOrg salesOrg,
        ErpAccount erpAccount,
        Dictionary<int, ErpShipToAddress> defaultShipToAddresses,
        string lastErpOrderSynced,
        string lastErpOrderSyncedOfErpAccount,
        int totalSyncedSoFar,
        int totalNotSyncedSoFar,
        List<StateProvince> allStateProvinces,
        List<Country> allCountries,
        Currency currency,
        Store store,
        string syncTaskName)
    {
        if (_isManualTriggerForCurrentRun)
        {
            await _erpSyncLogService.SyncLogSaveOnFileAsync(
                syncTaskName,
                ErpSyncLevel.Order,
                $"Total Orders received: {erpOrders.Count} for account: {erpAccount.AccountNumber}, Sales org code: {salesOrg.Code}");
        }

        foreach (var erpOrder in erpOrders)
        {
            var erpOrderNumber = erpOrder.ErpOrderNumber;

            try
            {
                if (_isManualTriggerForCurrentRun)
                {
                    await _erpSyncLogService.SyncLogSaveOnFileAsync(
                        syncTaskName,
                        ErpSyncLevel.Order,
                        $"For Order number {erpOrderNumber}, Sales org code: {salesOrg.Code}, \n" +
                        $"Account number: {erpAccount.AccountNumber}, \n" +
                        $"Order type: {erpOrder.OrderType}, \n" +
                        $"Custom order number: {erpOrder.CustomOrderNumber},  \n" +
                        $"Customer email: {erpOrder.CustomerEmail}, \n" +
                        $"Delivery email: {erpOrder.DelEmail}, \n" +
                        $"Customer Ref : {erpOrder.CustomerReference}, \n" +
                        $"Order date: {erpOrder.OrderDate:O}");
                }

                #region Erp Order Number Validation

                if (string.IsNullOrWhiteSpace(erpOrderNumber))
                {
                    totalNotSyncedSoFar++;
                    await _erpSyncLogService.SyncLogSaveOnFileAsync(
                        syncTaskName,
                        ErpSyncLevel.Order,
                        "Order data mapping skipped due to missing order number.");
                    continue;
                }

                if (string.IsNullOrWhiteSpace(erpOrder.OrderType) ||
                    !Enum.TryParse(typeof(ErpOrderType), erpOrder.OrderType, out var erpOrderType))
                {
                    totalNotSyncedSoFar++;
                    await _erpSyncLogService.SyncLogSaveOnFileAsync(
                        syncTaskName,
                        ErpSyncLevel.Order,
                        $"Order data mapping skipped due to OrderType missing or unknown for Erp Order: {erpOrderNumber}, " +
                        $"unable to determine whether it is B2B or B2C order.");
                    continue;
                }

                var isB2COrder = $"{(ErpOrderType)erpOrderType}".Contains("b2c", StringComparison.CurrentCultureIgnoreCase);
                if (isB2COrder)
                {
                    if (!IsValidEmail(erpOrder.CustomerEmail))
                    {
                        totalNotSyncedSoFar++;
                        await _erpSyncLogService.SyncLogSaveOnFileAsync(
                            syncTaskName,
                            ErpSyncLevel.Order,
                            $"Order data mapping skipped for B2C Order: {erpOrder.ErpOrderNumber}. " +
                            $"The customer email '{erpOrder.CustomerEmail}' is empty or invalid for B2C order.");
                        continue;
                    }
                }
                else
                {
                    if (string.IsNullOrWhiteSpace(erpOrder.AccountNumber))
                    {
                        totalNotSyncedSoFar++;
                        await _erpSyncLogService.SyncLogSaveOnFileAsync(
                            syncTaskName,
                            ErpSyncLevel.Order,
                            $"Order data mapping skipped for B2B Order: {erpOrder.ErpOrderNumber} due to missing Account number from erp.");
                        continue;
                    }

                    if (erpAccount.AccountNumber != erpOrder.AccountNumber || salesOrg.Code != erpOrder.Location)
                    {
                        totalNotSyncedSoFar++;
                        await _erpSyncLogService.SyncLogSaveOnFileAsync(
                            syncTaskName,
                            ErpSyncLevel.Order,
                            $"Order data mapping skipped for B2B Order: {erpOrder.ErpOrderNumber} due to Account {erpOrder.AccountNumber} from Erp not matching with the existing Account {erpAccount.AccountNumber} " +
                            $"or Location (Sales Org) {erpOrder.Location} mismatch with the existing location {salesOrg.Code} or does not exist.");
                        continue;
                    }
                }

                #endregion

                #region Nop Order Id Retrieval

                // Try load existing data first
                var customNopOrderNumberFromErp =
                    string.IsNullOrWhiteSpace(erpOrder.CustomOrderNumber) ?
                    "" :
                    erpOrder.CustomOrderNumber.Trim();
                Order oldNopOrder = null;
                var isErpOrderForCurrentNopSystem = false;

                if (string.IsNullOrWhiteSpace(customNopOrderNumberFromErp))
                {
                    // assuming it's an Erp/Starndard Order that is coming from SAP
                    isErpOrderForCurrentNopSystem = true;
                }
                else if (NopOrderPrefix != null)
                {
                    isErpOrderForCurrentNopSystem = customNopOrderNumberFromErp.StartsWith(NopOrderPrefix);
                }

                // return if the erp order is not for the current nop system
                if (!isErpOrderForCurrentNopSystem)
                {
                    totalNotSyncedSoFar++;
                    await _erpSyncLogService.SyncLogSaveOnFileAsync(
                        syncTaskName,
                        ErpSyncLevel.Order,
                        $"Order data mapping skipped for B2B Order: {erpOrder.ErpOrderNumber} of Account: {erpOrder.AccountNumber} with " +
                        $"Custom Order Number: {customNopOrderNumberFromErp}, does not belong to the current Nop System.");
                    continue;
                }

                // get Nop order number from customNopOrderNumberFromErp
                var nopOrderIdFromSyncedErpOrder = 0;
                if (!string.IsNullOrWhiteSpace(customNopOrderNumberFromErp))
                {
                    try
                    {
                        var index = customNopOrderNumberFromErp.IndexOfAny(['0', '1', '2', '3', '4', '5', '6', '7', '8', '9']);
                        nopOrderIdFromSyncedErpOrder = int.Parse(customNopOrderNumberFromErp[index..]);

                        oldNopOrder = await _orderService.GetOrderByIdAsync(nopOrderIdFromSyncedErpOrder);
                    }
                    catch (Exception ex)
                    {
                        totalNotSyncedSoFar++;
                        await _erpSyncLogService.SyncLogSaveOnFileAsync(
                            syncTaskName,
                            ErpSyncLevel.Order,
                            $"Order data mapping skipped due to exception when trying to get the NopOrderId for the " +
                            $"B2B Order: {erpOrder.ErpOrderNumber} " +
                            $"of Account: {erpOrder.AccountNumber}",
                            ex.Message);
                        continue;
                    }
                }
                else
                {
                    totalNotSyncedSoFar++;
                    await _erpSyncLogService.SyncLogSaveOnFileAsync(
                        syncTaskName,
                        ErpSyncLevel.Order,
                        $"Custom Nop Order Number is empty for  " +
                        $"B2B Order: {erpOrder.ErpOrderNumber} " +
                        $"of Account: {erpOrder.AccountNumber}");
                }

                var oldErpOrder = await _erpOrderAdditionalDataService
                    .GetErpOrderAdditionalDataByErpAccountIdAndErpOrderNumberAsync(accountId: erpAccount.Id, erpOrderNumber: erpOrder.ErpOrderNumber) ??
                    await _erpOrderAdditionalDataService.GetErpOrderAdditionalDataByNopOrderIdAsync(nopOrderIdFromSyncedErpOrder);

                var nopOrderIdFromExistingErpOrder = 0;

                if (oldErpOrder != null)
                {
                    nopOrderIdFromExistingErpOrder = oldErpOrder.NopOrderId;
                }

                if (nopOrderIdFromSyncedErpOrder > 0 && nopOrderIdFromExistingErpOrder > 0 &&
                    nopOrderIdFromSyncedErpOrder != nopOrderIdFromExistingErpOrder)
                {
                    totalNotSyncedSoFar++;
                    await _erpSyncLogService.SyncLogSaveOnFileAsync(
                        syncTaskName,
                        ErpSyncLevel.Order,
                        $"Order data mapping skipped due to mismatch of NopOrderId for the " +
                        $"B2B Order: {erpOrder.ErpOrderNumber} " +
                        $"of Account: {erpOrder.AccountNumber}. " +
                        $"Synced NopOrderId from Erp is {nopOrderIdFromSyncedErpOrder} " +
                        $"but NopOrderId with existing Erp order is {nopOrderIdFromExistingErpOrder}");
                    continue;
                }

                // standard order (which shouldn't have any CustomOrderNumber from SAP), was synced earlier with the ErpOrderAdditional table
                if (oldNopOrder == null && nopOrderIdFromExistingErpOrder > 0)
                {
                    oldNopOrder = await _orderService.GetOrderByIdAsync(nopOrderIdFromExistingErpOrder);
                }

                #endregion

                #region Address

                Address billingAddress = null, shippingAddress = null;

                if (oldNopOrder != null)
                {
                    billingAddress = await _addressService.GetAddressByIdAsync(oldNopOrder.BillingAddressId);
                    shippingAddress = await _addressService.GetAddressByIdAsync(oldNopOrder.ShippingAddressId ?? 0);
                }

                // Create or update addresses
                if (billingAddress == null)
                {
                    billingAddress = new Address();
                    await MapBillingAddressAsync(erpOrder, billingAddress, allCountries, allStateProvinces);
                    await _addressService.InsertAddressAsync(billingAddress);

                    if (oldNopOrder != null)
                        oldNopOrder.BillingAddressId = billingAddress.Id;
                }
                else
                {
                    await MapBillingAddressAsync(erpOrder, billingAddress, allCountries, allStateProvinces);
                    await _addressService.UpdateAddressAsync(billingAddress);
                }

                if (shippingAddress == null)
                {
                    shippingAddress = new Address();
                    await MapShippingAddressAsync(erpOrder, shippingAddress, allCountries, allStateProvinces);
                    await _addressService.InsertAddressAsync(shippingAddress);

                    if (oldNopOrder != null)
                        oldNopOrder.ShippingAddressId = shippingAddress.Id;
                }
                else
                {
                    await MapShippingAddressAsync(erpOrder, shippingAddress, allCountries, allStateProvinces);
                    await _addressService.UpdateAddressAsync(shippingAddress);
                }

                #endregion

                #region Nop Order

                if (oldNopOrder == null)
                {
                    var prefixedOrderNo = $"{ExternalOrderNumberPrefix}{erpOrderNumber}"; // Add prefix "E-" to Erp Orders
                    oldNopOrder = new Order
                    {
                        StoreId = store.Id,
                        CustomerId = 1,
                        CreatedOnUtc = erpOrder.OrderDate ?? DateTime.UtcNow,
                        CustomOrderNumber = prefixedOrderNo,
                        Deleted = false,
                        BillingAddressId = billingAddress.Id,
                        ShippingAddressId = shippingAddress.Id,
                        CustomerCurrencyCode = currency.CurrencyCode,
                        CurrencyRate = currency.Rate
                    };
                    await MapOrderFieldsAsync(erpOrder, oldNopOrder);
                    await _orderService.InsertOrderAsync(oldNopOrder);

                    if (oldErpOrder != null)
                        oldErpOrder.NopOrderId = oldNopOrder.Id;
                }
                else
                {
                    await MapOrderFieldsAsync(erpOrder, oldNopOrder);
                    await _orderService.UpdateOrderAsync(oldNopOrder);
                }

                var products = new Dictionary<string, ErpPlaceOrderItemDataModel>();
                if (erpOrder.ErpPlaceOrderItemDatas != null && erpOrder.ErpPlaceOrderItemDatas.Count > 0)
                {
                    foreach (var line in erpOrder.ErpPlaceOrderItemDatas)
                    {
                        if (string.IsNullOrWhiteSpace(line.Sku) || products.ContainsKey(line.Sku))
                        {
                            continue;
                        }
                        products.Add(line.Sku, line);
                    }
                }

                await _erpSyncLogService.SyncLogSaveOnFileAsync(
                    syncTaskName,
                    ErpSyncLevel.Order,
                    $"Number of products {products.Count} " +
                    $"for ErpOrderNumber {erpOrderNumber}, " +
                    $"Account Number: {erpOrder.AccountNumber}, " +
                    $"Location: {salesOrg.Code}");

                var productSkus = products.Keys.ToList();
                var productIds = new Dictionary<string, int>();

                try
                {
                    productIds = await GetOrCreateProductsAsync(productSkus, newProduct =>
                    {
                        if (!products.TryGetValue(newProduct.Sku, out var erpProduct))
                        {
                            return;
                        }
                        newProduct.Name = NormalizeProductDescription(erpProduct.Description);
                        newProduct.FullDescription = NormalizeProductFullDescription(erpProduct.Description);
                        newProduct.AdminComment = $"Created during Order Sync by Erp Data Scheduler Plugin on {DateTime.UtcNow}";
                        newProduct.ManageInventoryMethodId = _b2BB2CFeaturesSettings.TrackInventoryMethodId;
                        newProduct.ProductAvailabilityRangeId = _b2BB2CFeaturesSettings.ProductAvailabilityRangeId_DefaultValue;
                    });
                }
                catch (OutOfMemoryException ex)
                {
                    totalNotSyncedSoFar++;
                    await _erpSyncLogService.SyncLogSaveOnFileAsync(
                        syncTaskName,
                        ErpSyncLevel.Order,
                        $"Order data mapping skipped due to exception while creating or retrieving order items " +
                        $"for Erp Order: {erpOrder.ErpOrderNumber}, " +
                        $"Custom Order Number: {customNopOrderNumberFromErp} of " +
                        $"Account: {erpOrder.AccountNumber}, " +
                        $"Location: {salesOrg.Code}",
                        ex.Message);
                    continue;
                }
                catch (Exception ex)
                {
                    totalNotSyncedSoFar++;
                    await _erpSyncLogService.SyncLogSaveOnFileAsync(
                        syncTaskName,
                        ErpSyncLevel.Order,
                        $"Order data mapping skipped due to exception while creating or retrieving order items " +
                        $"for Erp Order: {erpOrder.ErpOrderNumber}, " +
                        $"Custom Order Number: {customNopOrderNumberFromErp} of " +
                        $"Account: {erpOrder.AccountNumber}, " +
                        $"Location: {salesOrg.Code}",
                        ex.Message);
                    continue;
                }

                #endregion

                #region Erp Order Additional Data

                if (oldErpOrder == null)
                {
                    oldErpOrder = new ErpOrderAdditionalData()
                    {
                        ErpAccountId = erpAccount.Id,
                        ErpOrderNumber = erpOrder.ErpOrderNumber
                    };

                    oldErpOrder.ErpShipToAddressId = defaultShipToAddresses
                        .FirstOrDefault(x => x.Key == erpAccount.Id).Value?.Id ?? 0;

                    if (!string.IsNullOrWhiteSpace(oldNopOrder.CustomOrderNumber))
                    {
                        oldErpOrder.ErpOrderOriginTypeId =
                            oldNopOrder.CustomOrderNumber.StartsWith(NopOrderPrefix) ?
                            (int)ErpOrderOriginType.OnlineOrder :
                            (int)ErpOrderOriginType.StandardOrder;
                    }

                    await MapErpOrderAdditionalDataFields(erpOrder, oldErpOrder, isB2COrder);
                    oldErpOrder.NopOrderId = oldNopOrder.Id;
                    await _erpOrderAdditionalDataService.InsertErpOrderAdditionalDataAsync(oldErpOrder);
                }
                else
                {
                    await MapErpOrderAdditionalDataFields(erpOrder, oldErpOrder, isB2COrder);
                    await _erpOrderAdditionalDataService.UpdateErpOrderAdditionalDataAsync(oldErpOrder);
                }

                #endregion

                #region Order Items

                var oldErpOrderItems = await _erpOrderItemAdditionalDataService
                    .GetErpOrderAdditionalItemsByNopOrderIdAsync(oldNopOrder.Id);
                var oldOrderItems = await _orderService.GetOrderItemsAsync(orderId: oldNopOrder.Id);
                var toBeDeletedOrderItems = oldOrderItems;
                var lineNo = 1;

                foreach (var item in erpOrder.ErpPlaceOrderItemDatas)
                {
                    if (string.IsNullOrWhiteSpace(item.Sku) || !productIds.TryGetValue(item.Sku, out var productId))
                    {
                        lineNo++;
                        continue;
                    }
                    if (productId == 0)
                    {
                        lineNo++;
                        continue;
                    }

                    var lineNumber = !string.IsNullOrWhiteSpace(item.ERPLineNumber)
                        ? item.ERPLineNumber
                        : $"{item.LineNo}";

                    if (string.IsNullOrWhiteSpace(lineNumber))
                    {
                        lineNumber = $"{lineNo}";
                    }

                    OrderItem nopOrderItem = null;

                    var erpOrderItem = oldErpOrderItems
                        .FirstOrDefault(x => x.erpOrderItem.ErpOrderLineNumber == lineNumber &&
                        x.productId == productId).erpOrderItem;

                    if (erpOrderItem != null)
                    {
                        nopOrderItem = oldOrderItems.FirstOrDefault(x => x.Id == erpOrderItem.NopOrderItemId);
                    }

                    // if nopOrderItem is still null and order origin is Online order then 
                    // for online order we want to retrive by nop order id and product id only
                    if (nopOrderItem == null && (oldErpOrder?.ErpOrderOriginTypeId == (int)ErpOrderOriginType.OnlineOrder))
                    {
                        nopOrderItem = oldOrderItems.FirstOrDefault(x => x.OrderId == oldNopOrder.Id && x.ProductId == productId);
                    }

                    if (nopOrderItem == null)
                    {
                        nopOrderItem = new OrderItem() { OrderId = oldNopOrder.Id, ProductId = productId };
                        nopOrderItem = await MapOrderItemFields(item, nopOrderItem, productIds);
                        nopOrderItem.OrderId = oldNopOrder.Id;
                        await _orderService.InsertOrderItemAsync(nopOrderItem);
                    }
                    else
                    {
                        nopOrderItem = await MapOrderItemFields(item, nopOrderItem, productIds);
                        await _orderService.UpdateOrderItemAsync(nopOrderItem);
                    }

                    toBeDeletedOrderItems.Remove(nopOrderItem);

                    if (erpOrderItem == null)
                    {
                        erpOrderItem = new ErpOrderItemAdditionalData()
                        {
                            ErpOrderId = oldErpOrder.Id,
                            ErpOrderLineNumber = lineNumber,
                            NopOrderItemId = nopOrderItem.Id,
                        };
                        await MapErpOrderAdditionalItemFields(item, erpOrderItem);
                        await _erpOrderItemAdditionalDataService.InsertErpOrderItemAdditionalDataAsync(erpOrderItem);
                    }
                    else
                    {
                        await MapErpOrderAdditionalItemFields(item, erpOrderItem);
                        await _erpOrderItemAdditionalDataService.UpdateErpOrderItemAdditionalDataAsync(erpOrderItem);
                    }

                    lineNo++;
                }

                if (toBeDeletedOrderItems != null && toBeDeletedOrderItems.Count != 0)
                {
                    await _orderItemRepository.DeleteAsync(toBeDeletedOrderItems);
                }

                #endregion

                lastErpOrderSynced = oldErpOrder?.ErpOrderNumber ?? string.Empty;
            }
            catch (Exception ex)
            {
                totalNotSyncedSoFar++;
                await _erpSyncLogService.SyncLogSaveOnFileAsync(
                    syncTaskName,
                    ErpSyncLevel.Order,
                    $"Order data mapping skipped due to exception for Erp Order: {erpOrderNumber}, " +
                    ex.Message);
            }
        }

        lastErpOrderSyncedOfErpAccount = erpAccount.AccountNumber;
        totalSyncedSoFar++;

        return (lastErpOrderSynced, lastErpOrderSyncedOfErpAccount, totalSyncedSoFar, totalNotSyncedSoFar);
    }

    #endregion
}