using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Nop.Core;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Payments;
using Nop.Core.Domain.Shipping;
using Nop.Core.Domain.Tax;
using Nop.Data;
using Nop.Plugin.Misc.ErpWebhook.Domain.ParallelTables;
using Nop.Plugin.Misc.ErpWebhook.Models.ErpOrder;
using Nop.Plugin.Misc.ErpWebhook.Services.Interfaces;
using Nop.Services.Common;
using Nop.Services.Logging;
using Nop.Services.Orders;
using NopStation.Plugin.B2B.ERPIntegrationCore.Domain;
using NopStation.Plugin.B2B.ERPIntegrationCore.Enums;
using NopStation.Plugin.B2B.ERPIntegrationCore.Services;

namespace Nop.Plugin.Misc.ErpWebhook.Services
{
    public class WebhookErpOrderService : IWebhookErpOrderService
    {
        #region fields

        private ErpWebhookConfig _erpWebhookConfig = null;
        private readonly ILogger _logger;
        private readonly ErpWebhookSettings _webhookSettings;
        private readonly IErpWebhookService _erpWebhookService;
        private readonly IRepository<ErpOrderAdditionalData> _erpOrderPerAccountRepo;
        private readonly IRepository<ErpOrderAdditionalData> _erpOrderAdditionalDataRepo;
        private readonly IRepository<ErpShipToAddress> _erpShipToAddressRepo;
        private readonly IRepository<ErpShiptoAddressErpAccountMap> _erpShiptoAddressErpAccountMap;
        private readonly IRepository<ErpNopUserAccountMap> _erpNopUserAccountMap;
        private readonly IRepository<OrderItem> _orderItemRepository;
        private readonly IRepository<Customer> _customerRepo;
        private readonly IRepository<Order> _orderRepo;
        private readonly IRepository<ErpNopUser> _erpNopUserRepo;
        private readonly IOrderService _orderService;
        private readonly IAddressService _addressService;
        private readonly IStoreContext _storeContext;
        private readonly IErpOrderItemAdditionalDataService _erpOrderItemAdditionalDataService;
        private readonly IWorkContext _workContext;
        private readonly IRepository<Parallel_ErpOrder> _erpOrderRepo;

        #endregion

        #region ctor

        public WebhookErpOrderService(ILogger logger,
            ErpWebhookSettings webhookSettings,
           IErpWebhookService erpWebhookService,
           IRepository<ErpOrderAdditionalData> erpOrderPerAccountRepo,
           IRepository<ErpOrderAdditionalData> erpOrderPerUserRepo,
           IRepository<Customer> customerRepo,
           IRepository<Order> orderRepo,
           IOrderService orderService,
           IAddressService addressService,
           IStoreContext storeContext,
           IErpOrderItemAdditionalDataService erpOrderItemAdditionalDataService,
           IWorkContext workContext,
           IRepository<Parallel_ErpOrder> erpOrderRepo,
           IRepository<ErpNopUser> erpNopUserRepo,
           IRepository<ErpShipToAddress> erpShipToAddressRepo,
           IRepository<ErpShiptoAddressErpAccountMap> erpShiptoAddressErpAccountMap,
           IRepository<ErpNopUserAccountMap> erpNopUserAccountMap,
           IRepository<OrderItem> orderItemRepository)
        {
            _logger = logger;
            _webhookSettings = webhookSettings;
            _erpWebhookService = erpWebhookService;
            _erpOrderPerAccountRepo = erpOrderPerAccountRepo;
            _erpOrderAdditionalDataRepo = erpOrderPerUserRepo;
            _customerRepo = customerRepo;
            _orderRepo = orderRepo;
            _orderService = orderService;
            _addressService = addressService;
            _storeContext = storeContext;
            _erpOrderItemAdditionalDataService = erpOrderItemAdditionalDataService;
            _workContext = workContext;
            _erpOrderRepo = erpOrderRepo;
            _erpNopUserRepo = erpNopUserRepo;
            _erpShipToAddressRepo = erpShipToAddressRepo;
            _erpShiptoAddressErpAccountMap = erpShiptoAddressErpAccountMap;
            _erpNopUserAccountMap = erpNopUserAccountMap;
            _orderItemRepository = orderItemRepository;
        }

        #endregion

        #region utils

        public async Task<ErpOrderAdditionalData> GetERPOrderPerAccountAsync(int accountId, int nopOrderIdFromErp, string orderNo)
        {
            if (accountId < 0)
                throw new ArgumentOutOfRangeException(nameof(accountId));
            if (nopOrderIdFromErp < 0)
                throw new ArgumentNullException(nameof(nopOrderIdFromErp));
            if (orderNo == null)
                throw new ArgumentNullException(nameof(orderNo));

            try
            {
                return await _erpOrderPerAccountRepo.Table
                    .FirstOrDefaultAsync(o => (orderNo == o.ErpOrderNumber && o.ErpAccountId == accountId) || o.Id == nopOrderIdFromErp);
            }
            catch (Exception ex)
            {
                var errmsg = $"Exception getting erporder for account id = {accountId}, orderNo = {orderNo}";
                _logger.Error(errmsg, ex);
                throw new Exception(errmsg, ex);
            }
        }
        public async Task<ErpOrderAdditionalData> GetERPOrderPerUserAsync(string billingEmail, int nopOrderIdFromErp, string orderNumber)
        {
            if (string.IsNullOrEmpty(billingEmail))
                throw new ArgumentOutOfRangeException(nameof(billingEmail));
            if (nopOrderIdFromErp < 0)
                throw new ArgumentNullException(nameof(nopOrderIdFromErp));
            if (orderNumber == null)
                throw new ArgumentNullException(nameof(orderNumber));

            var query = (from erpOrderAdditionalData in _erpOrderAdditionalDataRepo.Table
                         join _erpNopUser in _erpNopUserRepo.Table on erpOrderAdditionalData.ErpAccountId equals _erpNopUser.ErpAccountId
                         join c in _customerRepo.Table on _erpNopUser.NopCustomerId equals c.Id
                         join nopOrder in _orderRepo.Table on erpOrderAdditionalData.NopOrderId equals nopOrder.Id into nopOrdrGroup
                         from nopOrder in nopOrdrGroup.DefaultIfEmpty()
                         where c.Email == billingEmail
                            && !c.Deleted
                            && erpOrderAdditionalData.NopOrderId == nopOrderIdFromErp
                            && erpOrderAdditionalData.ErpOrderNumber == orderNumber
                         select erpOrderAdditionalData).Take(1);

            return await query.FirstOrDefaultAsync();
        }
        public async Task<int?> GetDefaultERPShipToAddressAsync(string customerEmail)
        {
            var shipTo = await (from addr in _erpShipToAddressRepo.Table
                                join shipMapAcc in _erpShiptoAddressErpAccountMap.Table on addr.Id equals shipMapAcc.ErpShiptoAddressId
                                join accMapUser in _erpNopUserAccountMap.Table on shipMapAcc.ErpAccountId equals accMapUser.ErpAccountId
                                join usr in _erpNopUserRepo.Table on accMapUser.Id equals usr.Id
                                join c in _customerRepo.Table on usr.NopCustomerId equals c.Id
                                where c.Email == customerEmail && !c.Deleted && usr.ErpShipToAddressId == addr.Id
                                select addr)
                               .FirstOrDefaultAsync();

            return shipTo?.Id;
        }
        private async Task MapBillingAddressAsync(Parallel_ErpOrder erpOrder, Address billingAddress)
        {
            if (billingAddress.Id < 0)
            {
                billingAddress.CreatedOnUtc = DateTime.UtcNow;
            }
            billingAddress.FirstName = erpOrder.BillingName ?? string.Empty;
            billingAddress.Email = erpOrder.BillingEmail ?? string.Empty;
            billingAddress.PhoneNumber = erpOrder.BillingPhone ?? string.Empty;
            billingAddress.Address1 = erpOrder.BillingAddress1 ?? string.Empty;
            billingAddress.Address2 = erpOrder.BillingAddress2 ?? string.Empty;
            billingAddress.City = erpOrder.BillingCity ?? string.Empty;
            billingAddress.ZipPostalCode = erpOrder.BillingPostalCode ?? string.Empty;

            billingAddress.CountryId = await _erpWebhookService.GetCountryIdByTwoOrThreeLetterIsoCodeAsync(erpOrder.BillingCountryCode);
            if (billingAddress.CountryId.HasValue)
            {
                billingAddress.StateProvinceId = await _erpWebhookService.GetStateProvinceIdByCountryIdAndAbbreviationAsync(billingAddress.CountryId.Value, erpOrder.BillingProvince);
            }
        }

        private async Task MapShippingAddressAsync(Parallel_ErpOrder erpOrder, Address shippingAddress)
        {
            if (shippingAddress.Id < 0)
            {
                shippingAddress.CreatedOnUtc = DateTime.UtcNow;
            }
            shippingAddress.FirstName = erpOrder.ShippingName ?? string.Empty;
            shippingAddress.Email = erpOrder.ShippingEmail ?? string.Empty;
            shippingAddress.PhoneNumber = erpOrder.ShippingPhone ?? string.Empty;
            shippingAddress.Address1 = erpOrder.ShippingAddress1 ?? string.Empty;
            shippingAddress.Address2 = erpOrder.ShippingAddress2 ?? string.Empty;
            shippingAddress.City = erpOrder.ShippingCity ?? string.Empty;
            shippingAddress.ZipPostalCode = erpOrder.ShippingPostalCode ?? string.Empty;
            shippingAddress.CountryId = await _erpWebhookService.GetCountryIdByTwoOrThreeLetterIsoCodeAsync(erpOrder.ShippingCountryCode);
            if (shippingAddress.CountryId.HasValue)
            {
                shippingAddress.StateProvinceId = await _erpWebhookService.GetStateProvinceIdByCountryIdAndAbbreviationAsync(shippingAddress.CountryId.Value, erpOrder.BillingProvince);
            }
        }

        static Dictionary<string, int> _orderStatusMap = new Dictionary<string, int>(StringComparer.InvariantCultureIgnoreCase)
        {
			// TODO Load from mappings.json
			// Yes, all mapped to Processing
			{ "Open", (int) OrderStatus.Processing },
            { "Released", (int) OrderStatus.Processing },
            { "Pending_Approval", (int) OrderStatus.Processing },
            { "Pending_Prepayment", (int) OrderStatus.Processing },
        };

        private static int? ParseOrderStatus(string input)
        {
            input = input?.Trim();
            if (input != null && _orderStatusMap.TryGetValue(input, out int value))
            {
                return value;
            }
            return null;
        }

        private Order MapOrderFields(Parallel_ErpOrder erpOrder, Order order)
        {
            int? orderStatusId = ParseOrderStatus(erpOrder.ERPOrderStatus);
            if (orderStatusId != null)
            {
                order.OrderStatusId = orderStatusId.Value;
            }
            order.ShippingStatusId = (int)ShippingStatus.Delivered; // TODO
            order.PaymentStatusId = (int)PaymentStatus.Paid; // TODO
            order.CustomerCurrencyCode = _erpWebhookConfig.DefaultCurrencyCode;
            order.CustomerTaxDisplayTypeId = _erpWebhookConfig.DefaultCustomerTaxDisplayTypeId ?? (int)TaxDisplayType.ExcludingTax;
            order.VatNumber = string.Empty;
            order.OrderSubtotalExclTax = erpOrder.TotalExcl; // ?
            order.OrderSubtotalInclTax = erpOrder.TotalExcl; // ? we do not have the subtotal incl tax...
            order.OrderShippingExclTax = 0;
            order.OrderShippingInclTax = 0;
            order.OrderTax = erpOrder.VAT;
            order.OrderTotal = erpOrder.TotalIncl;
            return order;
        }

        private ErpOrderType ParseOrderType(string orderType)
        {
            switch (orderType?.ToLower() ?? string.Empty)
            {
                case "B2BQuote":
                case "q":
                    return ErpOrderType.B2BQuote;
                default:
                    return ErpOrderType.B2BSalesOrder;
            }
        }
        private ErpOrderType ParseB2COrderType(string orderType)
        {
            switch (orderType?.ToLower() ?? string.Empty)
            {
                case "B2CQuote":
                case "b2cq":
                    return ErpOrderType.B2CQuote;
                default:
                    return ErpOrderType.B2CSalesOrder;
            }
        }
        private async Task<ErpOrderAdditionalData> MapERPOrderPerAccountFieldsAsync(Parallel_ErpOrder erpOrder, ErpOrderAdditionalData erpOrderAdditionalData)
        {
            DateTime now = DateTime.UtcNow;

            if (erpOrderAdditionalData.ErpOrderOriginTypeId < 0)
            {
                erpOrderAdditionalData.ErpOrderOriginTypeId = (int)ErpOrderOriginType.StandardOrder;
            }

            ErpOrderType orderType = ParseOrderType(erpOrder.OrderType);
            _logger.Information($"erpOrder.OrderType: {erpOrder.OrderType}, parsed {orderType}");

            erpOrderAdditionalData.ErpOrderTypeId = (int)orderType;
            erpOrderAdditionalData.QuoteExpiryDate = erpOrder.QuoteExpiryDate;
            _logger.Information($"erpOrder.QuoteExpiryDate: {erpOrder.QuoteExpiryDate}");

            erpOrderAdditionalData.CustomerReference = erpOrder.CustomerReference;
            erpOrderAdditionalData.ERPOrderStatus = erpOrder.ERPOrderStatus;
            erpOrderAdditionalData.DeliveryDate = erpOrder.DeliveryDate;
            _logger.Information($"erpOrder.DeliveryDate: {erpOrder.DeliveryDate}");

            erpOrderAdditionalData.IntegrationStatusTypeId = (int)IntegrationStatusType.Confirmed;
            erpOrderAdditionalData.IntegrationError = string.Empty;
            erpOrderAdditionalData.LastERPUpdateUtc = now;

            if (erpOrderAdditionalData.Id < 0)
            {
                var currentCustomerId = (await _workContext.GetCurrentCustomerAsync()).Id;
                erpOrderAdditionalData.ChangedOnUtc = now;
                erpOrderAdditionalData.ChangedById = currentCustomerId;
                erpOrderAdditionalData.OrderPlacedByNopCustomerId = currentCustomerId;
            }

            _logger.Information($"erpOrder.ChangedOnUtc: {erpOrderAdditionalData.ChangedOnUtc}");
            return erpOrderAdditionalData;
        }

        private async Task<ErpOrderAdditionalData> MapERPOrderPerAccountFieldsAsync(Parallel_ErpOrder erpOrder, ErpOrderAdditionalData erpOrderAdditionalData, Order nopOrder)
        {
            DateTime now = DateTime.UtcNow;
            erpOrderAdditionalData.ErpOrderNumber = string.IsNullOrEmpty(erpOrderAdditionalData.ErpOrderNumber) ? erpOrder.OrderNumber : erpOrderAdditionalData.ErpOrderNumber;

            if (erpOrderAdditionalData.ErpOrderOriginTypeId < 0)
            {
                erpOrderAdditionalData.ErpOrderOriginTypeId = (int)ErpOrderOriginType.StandardOrder;
            }

            ErpOrderType orderType = ParseB2COrderType(erpOrder.OrderType);
            erpOrderAdditionalData.ErpOrderTypeId = (int)orderType;
            erpOrderAdditionalData.QuoteExpiryDate = erpOrder.QuoteExpiryDate;
            erpOrderAdditionalData.CustomerReference = erpOrder.CustomerReference;

            // ERPOrderNumber
            erpOrderAdditionalData.ERPOrderStatus = erpOrder.ERPOrderStatus;
            erpOrderAdditionalData.DeliveryDate = erpOrder.DeliveryDate;
            erpOrderAdditionalData.IntegrationStatusTypeId = (int)IntegrationStatusType.Confirmed;
            erpOrderAdditionalData.IntegrationError = "";
            erpOrderAdditionalData.LastERPUpdateUtc = now;

            if (erpOrderAdditionalData.Id < 0)
            {
                erpOrderAdditionalData.ChangedOnUtc = now;
                erpOrderAdditionalData.ChangedById = (await _workContext.GetCurrentCustomerAsync()).Id;
                erpOrderAdditionalData.OrderPlacedByNopCustomerId = (await _workContext.GetCurrentCustomerAsync()).Id;
            }

            return erpOrderAdditionalData;
        }

        private OrderItem MapOrderItemFields(ErpOrderLineModel erpLine, OrderItem oi, Dictionary<string, int> productIds)
        {
            if (!productIds.TryGetValue(erpLine.Sku, out int productId))
            {
                _logger?.Error($"Id of product with SKU = '{erpLine.Sku}' not found");
                return null;
            }
            oi.Quantity = erpLine.Quantity;
            oi.ProductId = productId;
            oi.UnitPriceInclTax = erpLine.UnitPriceIncl ?? 0;
            oi.UnitPriceExclTax = erpLine.UnitPriceExcl ?? 0;
            if (erpLine.LineTotalIncl != null)
            {
                oi.PriceInclTax = erpLine.LineTotalIncl.Value;
            }
            if (erpLine.LineTotalExcl != null)
            {
                oi.PriceExclTax = erpLine.LineTotalExcl.Value;
            }
            oi.DiscountAmountInclTax = 0;
            oi.DiscountAmountExclTax = 0;
            oi.OriginalProductCost = 0;
            //oi.AttributeDescription = erpLine.Description; // turns out this is supposed to be blank instead
            oi.ItemWeight = erpLine.Weight;
            return oi;
        }

        private async Task MapERPOrderItemFieldsAsync(ErpOrderLineModel erpLine, ErpOrderItemAdditionalData erpOrderItemAdditionalData)
        {
            DateTime now = DateTime.UtcNow;

            erpOrderItemAdditionalData.ErpSalesUoM = erpLine.UOM;
            erpOrderItemAdditionalData.ErpOrderLineStatus = erpLine.ERPOrderLineStatus;
            erpOrderItemAdditionalData.ErpDateRequired = erpLine.DateRequired ?? DateTime.MinValue;
            erpOrderItemAdditionalData.ErpDateExpected = erpLine.DateExpected ?? DateTime.MinValue;
            erpOrderItemAdditionalData.ErpDeliveryMethod = erpLine.DeliveryMethod;
            erpOrderItemAdditionalData.ErpInvoiceNumber = string.Empty;
            erpOrderItemAdditionalData.ErpOrderLineNotes = string.Empty;
            erpOrderItemAdditionalData.LastErpUpdateUtc = now;
            erpOrderItemAdditionalData.ChangedOnUtc = now;

            erpOrderItemAdditionalData.ChangedBy = (await _workContext.GetCurrentCustomerAsync()).Id;
        }

        private async Task MapErpOrderItemFieldsAsync(ErpOrderLineModel erpLine, ErpOrderItemAdditionalData erpOrderItemAdditionalData)
        {
            DateTime now = DateTime.UtcNow;
            erpOrderItemAdditionalData.ErpSalesUoM = erpLine.UOM;
            erpOrderItemAdditionalData.ErpOrderLineNotes = erpLine.ERPOrderLineStatus;
            erpOrderItemAdditionalData.ErpDateRequired = erpLine.DateRequired ?? DateTime.MinValue;
            erpOrderItemAdditionalData.ErpDateExpected = erpLine.DateExpected ?? DateTime.MinValue;
            erpOrderItemAdditionalData.ErpDeliveryMethod = erpLine.DeliveryMethod;
            erpOrderItemAdditionalData.ErpInvoiceNumber = string.Empty;
            erpOrderItemAdditionalData.ErpOrderLineNotes = string.Empty;
            erpOrderItemAdditionalData.LastErpUpdateUtc = now;
            erpOrderItemAdditionalData.ChangedOnUtc = now;
            erpOrderItemAdditionalData.ChangedBy = _workContext.GetCurrentCustomerAsync().Id;
            //erpOrderItemAdditionalData.SpecialInstruction = erpLine.SpecInstruct;
            erpOrderItemAdditionalData.WareHouse = erpLine.WarehouseCode;
        }
        private void MapErpOrder(Parallel_ErpOrder dbErpOrder, ErpOrderModel updatedErpOrder)
        {
            if (dbErpOrder == null || updatedErpOrder == null)
                return;

            dbErpOrder.IsActive = true;
            dbErpOrder.IsDeleted = false;
            dbErpOrder.IsUpdated = false;
            dbErpOrder.UpdatedOnUtc = DateTime.UtcNow;
            dbErpOrder.AccountNumber = updatedErpOrder.AccountNumber;
            dbErpOrder.SalesOrganisationCode = updatedErpOrder.SalesOrganisationCode;
            dbErpOrder.OrderType = updatedErpOrder.OrderType;
            dbErpOrder.OrderDate = updatedErpOrder.OrderDate;
            dbErpOrder.QuoteExpiryDate = updatedErpOrder.QuoteExpiryDate;
            dbErpOrder.TotalExcl = updatedErpOrder.TotalExcl;
            dbErpOrder.TotalIncl = updatedErpOrder.TotalIncl;
            dbErpOrder.CustomerReference = updatedErpOrder.CustomerReference;
            dbErpOrder.CustomNopOrderNumber = updatedErpOrder.CustomNopOrderNumber;
            dbErpOrder.OrderNumber = updatedErpOrder.OrderNumber;
            dbErpOrder.ERPOrderStatus = updatedErpOrder.ERPOrderStatus;
            dbErpOrder.VAT = updatedErpOrder.VAT;
            dbErpOrder.ShippingFees = updatedErpOrder.ShippingFees;
            dbErpOrder.DeliveryDate = updatedErpOrder.DeliveryDate;
            dbErpOrder.SpecialInstructions = updatedErpOrder.SpecialInstructions;

            // Mapping ShippingAddress
            dbErpOrder.ShippingName = updatedErpOrder.ShippingName;
            dbErpOrder.ShippingPhone = updatedErpOrder.ShippingPhone;
            dbErpOrder.ShippingEmail = updatedErpOrder.ShippingEmail;
            dbErpOrder.ShippingAddress1 = updatedErpOrder.ShippingAddress1;
            dbErpOrder.ShippingAddress2 = updatedErpOrder.ShippingAddress2;
            dbErpOrder.ShippingCity = updatedErpOrder.ShippingCity;
            dbErpOrder.ShippingPostalCode = updatedErpOrder.ShippingPostalCode;
            dbErpOrder.ShippingCountryCode = updatedErpOrder.ShippingCountryCode;
            dbErpOrder.ShippingProvince = updatedErpOrder.ShippingProvince;

            // Mapping BillingAddress
            dbErpOrder.BillingName = updatedErpOrder.BillingName;
            dbErpOrder.BillingPhone = updatedErpOrder.BillingPhone;
            dbErpOrder.BillingEmail = updatedErpOrder.BillingEmail;
            dbErpOrder.BillingAddress1 = updatedErpOrder.BillingAddress1;
            dbErpOrder.BillingAddress2 = updatedErpOrder.BillingAddress2;
            dbErpOrder.BillingCity = updatedErpOrder.BillingCity;
            dbErpOrder.BillingPostalCode = updatedErpOrder.BillingPostalCode;
            dbErpOrder.BillingCountryCode = updatedErpOrder.BillingCountryCode;
            dbErpOrder.BillingProvince = updatedErpOrder.BillingProvince;

            // Mapping DetailLines
            if (updatedErpOrder.DetailLines != null && updatedErpOrder.DetailLines.Any())
            {
                dbErpOrder.DetailLinesJson = JsonConvert.SerializeObject(updatedErpOrder.DetailLines);
            }
        }

        #endregion

        #region Methods

        public async Task ProcessErpOrdersAsync()
        {
            _erpWebhookConfig = await _erpWebhookService.LoadErpWebhookConfigsFromJsonAsync();

            List<Parallel_ErpOrder> erpOrders = await _erpOrderRepo.Table.Where(x => !x.IsUpdated).ToListAsync();
            foreach (var erpOrder in erpOrders)
            {
                string orderNo = erpOrder.OrderNumber;
                if (string.IsNullOrWhiteSpace(orderNo))
                {
                    _logger.Error("Skipping order with missing OrderNumber");
                    return;
                }
                _logger.Information($"Looking at order '{orderNo}'");

                if (string.IsNullOrWhiteSpace(erpOrder.OrderType))
                {
                    _logger.Information("OrderType missing, unable to determine whether it is erp order or not");
                    return;
                }

                ErpAccount account = null;
                var isERPOrder = erpOrder.OrderType.ToLower().Contains("b2c");
                if (isERPOrder)
                {
                    _logger.Information($"Erp order loading. Order number " + erpOrder.OrderNumber);
                    if (string.IsNullOrWhiteSpace(erpOrder.BillingEmail) && string.IsNullOrWhiteSpace(erpOrder.ShippingEmail))
                    {
                        _logger.Error("Email address is missing for the erp order");
                        return;
                    }
                }
                else
                {
                    if (string.IsNullOrWhiteSpace(erpOrder.AccountNumber))
                    {
                        _logger.Error($"ERP Order '{erpOrder.OrderNumber}' missing AccNo");
                        return;
                    }
                    account = await _erpWebhookService.GetErpAccountAsync(erpOrder.SalesOrganisationCode, erpOrder.AccountNumber);
                    if (account == null)
                    {
                        _logger.Warning($"Account {erpOrder.AccountNumber} does not exist. Skipping order {erpOrder.AccountNumber}");
                        return;
                    }
                }

                string prefixedOrderNo = (_erpWebhookConfig?.ExternalOrderNumberPrefix ?? string.Empty) + orderNo; // Add prefix "E-" to ERP Orders

                Address billingAddress = null, shippingAddress = null;
                Order nopOrder = null;

                if (!string.IsNullOrEmpty(erpOrder.CustomNopOrderNumber))
                {
                    try
                    {
                        nopOrder = await _orderService.GetOrderByCustomOrderNumberAsync(erpOrder.CustomNopOrderNumber);
                        if (nopOrder == null)
                        {
                            _logger.Error($"Nop order does not exist for the custom nop order number {erpOrder.CustomNopOrderNumber}, So ignoring");
                            return;
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.Error($"Account {erpOrder.AccountNumber}: Got exception when trying to get the NopOrder for the custom nop order number {erpOrder.CustomNopOrderNumber}. Error: ", ex);
                        return;
                    }
                }

                // get Nop order number from customNopOrderNumberFromErp
                int nopOrderIdFromErp = nopOrder?.Id ?? 0;

                // Try load existing data first
                ErpOrderAdditionalData erpOrderAdditionalData = null;
                if (!isERPOrder)
                {
                    erpOrderAdditionalData = await GetERPOrderPerAccountAsync(account.Id, nopOrderIdFromErp, orderNo);
                }
                else
                {
                    // Get from erp table
                    erpOrderAdditionalData = await GetERPOrderPerUserAsync(erpOrder.BillingEmail, nopOrderIdFromErp, erpOrder.OrderNumber);
                }

                int nopOrderIdFromERPOrder = 0;

                if (erpOrderAdditionalData != null)
                {
                    _logger.Information($"Load existing erp order (id = {erpOrderAdditionalData.Id}) for erp order '{erpOrder.OrderNumber}'");

                    if (erpOrderAdditionalData.NopOrderId > 0)
                    {
                        nopOrderIdFromERPOrder = erpOrderAdditionalData.NopOrderId;
                    }
                }
                else if (erpOrderAdditionalData != null)
                {
                    _logger.Information($"Load existing erp order (id = {erpOrderAdditionalData.Id}) for erp order '{erpOrder.OrderNumber}'");

                    if (erpOrderAdditionalData.NopOrderId > 0)
                    {
                        nopOrderIdFromERPOrder = erpOrderAdditionalData.NopOrderId;
                    }
                }

                if (nopOrderIdFromErp > 0 && nopOrderIdFromERPOrder > 0 && nopOrderIdFromErp != nopOrderIdFromERPOrder)
                {
                    _logger.Error($"Account {erpOrder.AccountNumber}: Given NopOrderId from ERP is {nopOrderIdFromErp} " +
                        $"but nop order id with erp order is {nopOrderIdFromERPOrder} for erp order {erpOrder.OrderNumber}.");
                    return;
                }

                // standard order (which shouldn't have any CustomNopOrderNumber from SAP), was synced earlier with the nop ERP Order Per Account table
                if (nopOrder == null && nopOrderIdFromERPOrder > 0)
                {
                    nopOrder = await _orderService.GetOrderByIdAsync(nopOrderIdFromERPOrder);
                }

                if (nopOrder != null)
                {
                    _logger.Information($"Loaded existing nop order (id={nopOrder.Id}) for '{orderNo}'");
                    billingAddress = await _erpWebhookService.GetBillingAddressByNopOrderIdAsync(nopOrder.Id);
                    shippingAddress = await _erpWebhookService.GetShippingAddressByNopOrderIdAsync(nopOrder.Id);
                }

                // Create or update addresses
                if (billingAddress == null)
                {
                    _logger.Information($"Created new billing address for '{orderNo}'");
                    billingAddress = new Address();
                    await MapBillingAddressAsync(erpOrder, billingAddress);
                    await _addressService.InsertAddressAsync(billingAddress);
                    _logger.Information($"Created new billing address for '{orderNo}', id = {billingAddress.Id}");
                    if (nopOrder != null)
                        nopOrder.BillingAddressId = billingAddress.Id;
                }
                else
                {
                    _logger.Information($"Loaded existing billing address (id={billingAddress.Id}) for '{orderNo}'");
                    await MapBillingAddressAsync(erpOrder, billingAddress);
                }

                if (shippingAddress == null)
                {
                    _logger.Information($"Creating new shipping address for '{orderNo}'");
                    shippingAddress = new Address();
                    await MapShippingAddressAsync(erpOrder, shippingAddress);
                    await _addressService.InsertAddressAsync(shippingAddress);
                    _logger.Information($"Created new shipping address for '{orderNo}', id = {shippingAddress.Id}");
                    if (nopOrder != null)
                        nopOrder.ShippingAddressId = shippingAddress.Id;
                }
                else
                {
                    _logger.Information($"Loaded existing shipping address (id={shippingAddress.Id}) for '{orderNo}'");
                    await MapShippingAddressAsync(erpOrder, shippingAddress);
                }

                // Create new nop order if missing
                if (nopOrder == null)
                {
                    _logger.Information($"Creating new nop order for '{orderNo}'");
                    nopOrder = new Order()
                    {
                        StoreId = _storeContext.GetCurrentStoreAsync().Id,
                        CustomerId = (await _workContext.GetCurrentCustomerAsync()).Id,
                        CreatedOnUtc = DateTime.UtcNow,
                        CustomOrderNumber = prefixedOrderNo,
                        Deleted = false,
                    };
                    nopOrder.BillingAddressId = billingAddress.Id;
                    nopOrder.ShippingAddressId = shippingAddress.Id;
                    MapOrderFields(erpOrder, nopOrder);
                    await _orderService.InsertOrderAsync(nopOrder);
                    _logger.Information($"Created new nop order for '{orderNo}', id = {nopOrder.Id}");
                    if (erpOrderAdditionalData != null)
                        erpOrderAdditionalData.NopOrderId = nopOrder.Id;
                }
                else
                {
                    MapOrderFields(erpOrder, nopOrder);
                }

                #region Load or create products mentioned in lines

                var products = new Dictionary<string, ErpOrderLineModel>();
                var detailLines = new List<ErpOrderLineModel>();

                if (string.IsNullOrEmpty(erpOrder.DetailLinesJson))
                {
                    _logger.Warning($"Order {orderNo} has no DetailLines");
                }
                else
                {
                    detailLines = JsonConvert.DeserializeObject<List<ErpOrderLineModel>>(erpOrder.DetailLinesJson);
                    foreach (var line in detailLines)
                    {
                        if (string.IsNullOrWhiteSpace(line.Sku))
                        {
                            continue;
                        }
                        if (products.ContainsKey(line.Sku))
                        {
                            continue;
                        }
                        products.Add(line.Sku, line);
                    }
                }

                List<string> productSkus = products.Keys.ToList();
                Dictionary<string, int> productIds = await _erpWebhookService.GetOrCreateProductsAsync(productSkus, newProduct =>
                {
                    if (!products.TryGetValue(newProduct.Sku, out var erpProduct))
                    {
                        _logger.Error($"Failed to get/create product with sku = '{newProduct.Sku}' for some reason");
                        return;
                    }
                    newProduct.Name = erpProduct.Description;
                    newProduct.FullDescription = erpProduct.Description;
                    newProduct.AdminComment = $"Created by erp webhook (B2B order run) on {DateTime.UtcNow.ToString("u")}";
                    newProduct.ManageInventoryMethodId = _erpWebhookConfig.TrackInventoryMethodId ?? 0;
                    newProduct.ProductAvailabilityRangeId = _erpWebhookConfig.ProductAvailabilityRangeId_DefaultValue ?? 0;
                    if (erpProduct.Weight != null)
                    {
                        newProduct.Weight = erpProduct.Weight.Value;
                    }
                });
                _logger.Information($"Loaded products: {string.Join(", ", productIds.Select(kvp => $"{kvp.Key}=>{kvp.Value}"))}");

                #endregion

                var salesOrgId = await _erpWebhookService.GetSalesOrganisationIdAsync(erpOrder.SalesOrganisationCode);

                if (!isERPOrder)
                {
                    if (erpOrderAdditionalData == null)
                    {
                        _logger.Information($"Will create new erpOrder for '{erpOrderAdditionalData.ErpOrderNumber}'");
                        erpOrderAdditionalData = new ErpOrderAdditionalData()
                        {
                            ErpAccountId = account.Id,
                            ErpShipToAddressId = await _erpWebhookService.GetDefaultShipToAsync(account.Id),
                            ErpOrderNumber = erpOrderAdditionalData.ErpOrderNumber,
                        };

                        if (!string.IsNullOrEmpty(nopOrder.CustomOrderNumber))
                        {
                            erpOrderAdditionalData.ErpOrderOriginTypeId = nopOrder.CustomOrderNumber.StartsWith(_erpWebhookConfig?.ExternalOrderNumberPrefix) ?
                                (int)ErpOrderOriginType.OnlineOrder : (int)ErpOrderOriginType.StandardOrder;
                        }

                        await MapERPOrderPerAccountFieldsAsync(erpOrder, erpOrderAdditionalData);
                        await _erpOrderPerAccountRepo.InsertAsync(erpOrderAdditionalData);
                    }
                    else
                    {
                        await MapERPOrderPerAccountFieldsAsync(erpOrder, erpOrderAdditionalData);
                    }
                }
                else
                {
                    if (erpOrderAdditionalData == null)
                    {
                        _logger.Information($"Will create new erpOrderAdditionalData for '{erpOrderAdditionalData.ErpOrderNumber}'");
                        erpOrderAdditionalData = new ErpOrderAdditionalData()
                        {
                            ErpAccountId = 0,
                            ErpOrderNumber = erpOrderAdditionalData.ErpOrderNumber,
                            ErpShipToAddressId = await GetDefaultERPShipToAddressAsync(erpOrder.BillingEmail),
                        };

                        if (!string.IsNullOrEmpty(nopOrder.CustomOrderNumber))
                        {
                            erpOrderAdditionalData.ErpOrderOriginTypeId = nopOrder.CustomOrderNumber.StartsWith(_erpWebhookConfig?.ExternalOrderNumberPrefix) ?
                                (int)ErpOrderOriginType.OnlineOrder : (int)ErpOrderOriginType.StandardOrder;
                        }

                        await MapERPOrderPerAccountFieldsAsync(erpOrder, erpOrderAdditionalData, nopOrder);
                        await _erpOrderAdditionalDataRepo.InsertAsync(erpOrderAdditionalData);
                    }
                    else
                    {
                        await MapERPOrderPerAccountFieldsAsync(erpOrder, erpOrderAdditionalData, nopOrder);
                    }
                }

                #region From Macsteel branch
                int lineNo = 1;
                var tobeDeletedOrderItems = (await _erpWebhookService.GetOrderLinesByNopOrderIdAsync(nopOrder.Id))?.Select(x => x.Id)?.ToList();
                foreach (ErpOrderLineModel erpLine in detailLines)
                {
                    if (string.IsNullOrEmpty(erpLine.Sku))
                    {
                        _logger.Error($"Failed to insert line {lineNo} of order, because SKU is missing");
                        lineNo++;
                        continue;
                    }

                    if (!productIds.TryGetValue(erpLine.Sku, out int productId))
                    {
                        _logger.Error($"Id of product with SKU = '{erpLine.Sku}' not found");

                        _logger.Error($"Failed to insert line {lineNo} of order, where SKU = '{erpLine.Sku}'");
                        lineNo++;
                        continue;
                    }

                    if (productId == 0)
                    {
                        _logger.Error($"Failed to insert line {lineNo} of order where SKU = '{erpLine.Sku}'");
                        lineNo++;
                        continue;
                    }

                    string lineNumber = !string.IsNullOrEmpty(erpLine.ERPLineNumber)
                            ? erpLine.ERPLineNumber
                            : string.Empty;
                    _logger.Information($"Retrieve product id {productId}, here line number: {lineNumber} erp item no is {erpLine.Sku}, Quantity: {erpLine.Quantity}, for nop Order id = {nopOrder.Id}");

                    if (string.IsNullOrEmpty(lineNumber))
                    {
                        lineNumber = lineNo.ToString();
                    }

                    ErpOrderItemAdditionalData erpOrderItemAdditionalData = null;
                    OrderItem nopOrderItem = null;
                    if (!isERPOrder)
                    {
                        erpOrderItemAdditionalData = await _erpWebhookService.GetErpOrderItemByERPOrderLineNumberAndNopOrderIdAndProductIdAsync(lineNumber, nopOrder.Id, productId);

                        if (erpOrderItemAdditionalData != null)
                        {
                            _logger.Information($"Retrieve erp order item for line number: {lineNumber} Order id = {nopOrder.Id}, product id = {productId}, here erp order item id {erpOrderItemAdditionalData.Id}, nopOrderItemId {erpOrderItemAdditionalData.NopOrderItemId}");
                            nopOrderItem = await _orderService.GetOrderItemByIdAsync(erpOrderItemAdditionalData.NopOrderItemId);
                        }
                    }
                    else
                    {
                        erpOrderItemAdditionalData = await _erpWebhookService.GetErpOrderItemByERPOrderLineNumberAndNopOrderIdAndProductIdAsync(lineNumber, nopOrder.Id, productId);

                        if (erpOrderItemAdditionalData != null)
                        {
                            _logger.Information($"Retrieve erp order item for line number: {lineNumber} Order id = {nopOrder.Id}, product id = {productId}, here erp order item id {erpOrderItemAdditionalData.Id}, nopOrderItemId {erpOrderItemAdditionalData.NopOrderItemId}");
                            nopOrderItem = await _orderService.GetOrderItemByIdAsync(erpOrderItemAdditionalData.NopOrderItemId);
                        }
                    }
                    // if nopOrderItem is still null and order origin is Online order then 
                    // for online order we want to retrive by nop order id and product id only
                    if (nopOrderItem == null && (erpOrderAdditionalData?.ErpOrderOriginTypeId == (int)ErpOrderOriginType.OnlineOrder
                                            || erpOrderAdditionalData?.ErpOrderOriginTypeId == (int)ErpOrderOriginType.OnlineOrder))
                    {
                        nopOrderItem = await _erpWebhookService.GetNopOrderItemByOrderIdAndProductIdAsync(nopOrder.Id, productId);
                    }

                    if (nopOrderItem == null)
                    {
                        nopOrderItem = new OrderItem() { OrderId = nopOrder.Id, ProductId = productId };
                        nopOrderItem = MapOrderItemFields(erpLine, nopOrderItem, productIds);
                        await _orderService.InsertOrderItemAsync(nopOrderItem);
                        await _orderService.UpdateOrderAsync(nopOrder);
                        _logger.Information($"Created new nop order item for Order id = {nopOrder.Id}, item id = {nopOrderItem.Id}");
                    }
                    else
                    {
                        nopOrderItem = MapOrderItemFields(erpLine, nopOrderItem, productIds);
                        _logger.Information($"Update nop order item for Order id = {nopOrder.Id}, item id = {nopOrderItem.Id}");
                    }
                    tobeDeletedOrderItems.Remove(nopOrderItem.Id);
                    if (!isERPOrder)
                    {
                        if (erpOrderItemAdditionalData == null)
                        {
                            erpOrderItemAdditionalData = new ErpOrderItemAdditionalData()
                            {
                                ErpOrderLineNumber = lineNumber,
                                NopOrderItemId = nopOrderItem.Id,
                            };
                            await MapERPOrderItemFieldsAsync(erpLine, erpOrderItemAdditionalData);
                            await _erpOrderItemAdditionalDataService.InsertErpOrderItemAdditionalDataAsync(erpOrderItemAdditionalData);
                            _logger.Information($"Created new erp order item for nop Order item id = {nopOrderItem.Id}, erp item id = {erpOrderItemAdditionalData.Id}");
                        }
                        else
                        {
                            await MapERPOrderItemFieldsAsync(erpLine, erpOrderItemAdditionalData);
                            _logger.Information($"Update erp order item for nop Order item id = {nopOrderItem.Id}, erp item id = {erpOrderItemAdditionalData.Id}");
                        }
                    }
                    else
                    {
                        if (erpOrderItemAdditionalData == null)
                        {
                            erpOrderItemAdditionalData = new ErpOrderItemAdditionalData()
                            {
                                ErpOrderLineNumber = lineNumber,
                                NopOrderItemId = nopOrderItem.Id,
                            };
                            await MapERPOrderItemFieldsAsync(erpLine, erpOrderItemAdditionalData);
                            await _erpOrderItemAdditionalDataService.InsertErpOrderItemAdditionalDataAsync(erpOrderItemAdditionalData);
                            _logger.Information($"Created new erp order item for nop Order item id = {nopOrderItem.Id}, erp item id = {erpOrderItemAdditionalData.Id}");
                        }
                        else
                        {
                            await MapERPOrderItemFieldsAsync(erpLine, erpOrderItemAdditionalData);
                            _logger.Information($"Update erp order item for nop Order item id = {nopOrderItem.Id}, erp item id = {erpOrderItemAdditionalData.Id}");
                        }
                    }

                    lineNo++;
                }

                if (tobeDeletedOrderItems != null && tobeDeletedOrderItems.Any())
                {
                    _logger.Information($"Delete required for nop Order id = {nopOrder.Id}");
                    await _erpWebhookService.DeleteOrderLinesByListAsync(tobeDeletedOrderItems);
                }
                #endregion

            }
        }

        public async Task ProcessErpOrdersToParallelTableAsync(List<ErpOrderModel> erpOrders)
        {
            if (!erpOrders.Any())
                return;

            var currentCustomerId = (await _workContext.GetCurrentCustomerAsync()).Id;
            var ordersToAdd = new List<Parallel_ErpOrder>();

            // Check if similar entities exist in the database
            var existingErpOrders = await (from obj in erpOrders
                                           join dbEntity in _erpOrderRepo.Table
                                           on new { obj.AccountNumber, obj.SalesOrganisationCode, obj.OrderNumber }
                                           equals new { dbEntity.AccountNumber, dbEntity.SalesOrganisationCode, dbEntity.OrderNumber }
                                           select dbEntity).ToListAsync();

            foreach (var dbErpOrder in existingErpOrders)
            {
                var updatedErpOrder = erpOrders.Find(x => x.AccountNumber.Equals(dbErpOrder.AccountNumber) &&
                                                           x.SalesOrganisationCode.Equals(dbErpOrder.SalesOrganisationCode) &&
                                                           x.OrderNumber.Equals(dbErpOrder.OrderNumber));
                if (updatedErpOrder != null)
                {
                    MapErpOrder(dbErpOrder, updatedErpOrder);
                    dbErpOrder.UpdatedById = currentCustomerId;
                }
            }

            if (existingErpOrders.Any())
            {
                await _erpOrderRepo.UpdateAsync(existingErpOrders);
            }

            var newErpOrders = erpOrders.Where(model => !existingErpOrders.Any(existing =>
                                                        model.AccountNumber == existing.AccountNumber &&
                                                        model.SalesOrganisationCode == existing.SalesOrganisationCode &&
                                                        model.OrderNumber == existing.OrderNumber))
                                                        .ToList();

            foreach (var erpOrderModel in newErpOrders)
            {
                var dbErpOrder = new Parallel_ErpOrder();
                MapErpOrder(dbErpOrder, erpOrderModel);

                //common
                dbErpOrder.CreatedById = currentCustomerId;
                dbErpOrder.UpdatedById = currentCustomerId;
                dbErpOrder.CreatedOnUtc = DateTime.UtcNow;

                ordersToAdd.Add(dbErpOrder);
            }

            await _erpOrderRepo.InsertAsync(ordersToAdd);

        }

        public async Task<List<Parallel_ErpOrder>> GetErpOrdersAsync(int skipCount, int batchSize)
        {
            return await _erpOrderRepo.Table
                .Where(x => !x.IsUpdated)
                .OrderByDescending(x => x.Id)
                .Skip(skipCount)
                .Take(batchSize)
                .ToListAsync();
        }


        public async Task UpdateErpOrdersAsync(List<Parallel_ErpOrder> erpOrders)
        {
            if (erpOrders == null)
                return;

            erpOrders.ForEach(x => x.IsUpdated = true);

            await _erpOrderRepo.UpdateAsync(erpOrders);
        }

        #endregion
    }
}
