using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Directory;
using Nop.Core.Domain.Localization;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Payments;
using Nop.Core.Domain.Shipping;
using Nop.Core.Domain.Tax;
using Nop.Core.Events;
using Nop.Services.Affiliates;
using Nop.Services.Catalog;
using Nop.Services.Common;
using Nop.Services.Customers;
using Nop.Services.Directory;
using Nop.Services.Discounts;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Messages;
using Nop.Services.Orders;
using Nop.Services.Payments;
using Nop.Services.Security;
using Nop.Services.Shipping;
using Nop.Services.Stores;
using Nop.Services.Tax;
using Nop.Services.Vendors;
using NopStation.Plugin.B2B.B2BB2CFeatures.Services.ErpCustomerFunctionality;
using NopStation.Plugin.B2B.B2BB2CFeatures.Services.ERPSAPErrorMsgTranslations;
using NopStation.Plugin.B2B.B2BB2CFeatures.Services.ErpSpecificationAttributeService;
using NopStation.Plugin.B2B.B2BB2CFeatures.Services.ErpWorkflowMessage;
using NopStation.Plugin.B2B.ERPIntegrationCore.Domain;
using NopStation.Plugin.B2B.ERPIntegrationCore.Enums;
using NopStation.Plugin.B2B.ERPIntegrationCore.Model;
using NopStation.Plugin.B2B.ERPIntegrationCore.Services;

namespace NopStation.Plugin.B2B.B2BB2CFeatures.Services.Overriden;

public class OverriddenOrderProcessingService : OrderProcessingService, IOverriddenOrderProcessingService
{
    #region Fields

    private readonly IStoreContext _storeContext;
    private readonly IErpCustomerFunctionalityService _erpCustomerFunctionalityService;
    private readonly IErpSalesOrgService _erpSalesOrgService;
    private readonly IErpOrderAdditionalDataService _erpOrderAdditionalDataService;
    private readonly IErpShipToAddressService _erpShipToAddressService;
    private readonly IErpOrderItemAdditionalDataService _erpOrderItemAdditionalDataService;
    private readonly IErpAccountService _erpAccountService;
    private readonly IErpNopUserService _erpNopUserService;
    private readonly IErpLogsService _erpLogsService;
    private readonly IErpSpecificationAttributeService _erpSpecificationAttributeService;
    private readonly IErpWorkflowMessageService _erpWorkflowMessageService;
    private readonly B2BB2CFeaturesSettings _b2BB2CFeaturesSettings;
    private readonly IErpSpecialPriceService _erpSpecialPriceService;
    private readonly IErpIntegrationPluginManager _erpIntegrationPluginManager;
    private readonly IERPSAPErrorMsgTranslationService _erpSapErrorMsgTranslationService;
    private readonly IB2CShoppingCartItemService _b2CShoppingCartItemService;
    private readonly IB2CUserStockRestrictionService _b2CUserStockRestrictionService;
    private const string DELIVERY_METHOD_COLLECT = "COLLECT";
    private const string DELIVERY_METHOD_DELIVERY = "DELIVERY";

    #endregion Fields

    #region Ctor

    public OverriddenOrderProcessingService(
        CurrencySettings currencySettings,
        IAddressService addressService,
        IAffiliateService affiliateService,
        ICheckoutAttributeFormatter checkoutAttributeFormatter,
        ICountryService countryService,
        ICurrencyService currencyService,
        ICustomerActivityService customerActivityService,
        ICustomerService customerService,
        ICustomNumberFormatter customNumberFormatter,
        IDiscountService discountService,
        IEncryptionService encryptionService,
        IEventPublisher eventPublisher,
        IGenericAttributeService genericAttributeService,
        IGiftCardService giftCardService,
        ILanguageService languageService,
        ILocalizationService localizationService,
        ILogger logger,
        IOrderService orderService,
        IOrderTotalCalculationService orderTotalCalculationService,
        IPaymentPluginManager paymentPluginManager,
        IPaymentService paymentService,
        IPdfService pdfService,
        IPriceCalculationService priceCalculationService,
        IPriceFormatter priceFormatter,
        IProductAttributeFormatter productAttributeFormatter,
        IProductAttributeParser productAttributeParser,
        IProductService productService,
        IRewardPointService rewardPointService,
        IShipmentService shipmentService,
        IShippingService shippingService,
        IShoppingCartService shoppingCartService,
        IStateProvinceService stateProvinceService,
        IStoreContext storeContext,
        ITaxService taxService,
        IVendorService vendorService,
        IWebHelper webHelper,
        IWorkContext workContext,
        IWorkflowMessageService workflowMessageService,
        LocalizationSettings localizationSettings,
        OrderSettings orderSettings,
        PaymentSettings paymentSettings,
        RewardPointsSettings rewardPointsSettings,
        ShippingSettings shippingSettings,
        TaxSettings taxSettings,
        IReturnRequestService returnRequestService,
        IStoreService storeService,
        IErpCustomerFunctionalityService erpCustomerFunctionalityService,
        IErpSalesOrgService erpSalesOrgService,
        IErpOrderAdditionalDataService erpOrderAdditionalDataService,
        IErpShipToAddressService erpShipToAddressService,
        IErpOrderItemAdditionalDataService erpOrderItemAdditionalDataService,
        IErpAccountService erpAccountService,
        IErpNopUserService erpNopUserService,
        IErpLogsService erpLogsService,
        IErpIntegrationPluginManager erpIntegrationPluginManager,
        IErpSpecificationAttributeService erpSpecificationAttributeService,
        IErpWorkflowMessageService erpWorkflowMessageService,
        IStoreMappingService storeMappingService,
        B2BB2CFeaturesSettings b2BB2CFeaturesSettings,
        IErpSpecialPriceService erpSpecialPriceService,
        IB2CShoppingCartItemService b2CShoppingCartItemService,
        IB2CUserStockRestrictionService b2CUserStockRestrictionService,
        IERPSAPErrorMsgTranslationService erpSapErrorMsgTranslationService) : base(currencySettings,
            addressService,
            affiliateService,
            checkoutAttributeFormatter,
            countryService,
            currencyService,
            customerActivityService,
            customerService,
            customNumberFormatter,
            discountService,
            encryptionService,
            eventPublisher,
            genericAttributeService,
            giftCardService,
            languageService,
            localizationService,
            logger,
            orderService,
            orderTotalCalculationService,
            paymentPluginManager,
            paymentService,
            pdfService,
            priceCalculationService,
            priceFormatter,
            productAttributeFormatter,
            productAttributeParser,
            productService,
            returnRequestService,
            rewardPointService,
            shipmentService,
            shippingService,
            shoppingCartService,
            stateProvinceService,
            storeMappingService,
            storeService,
            taxService,
            vendorService,
            webHelper,
            workContext,
            workflowMessageService,
            localizationSettings,
            orderSettings,
            paymentSettings,
            rewardPointsSettings,
            shippingSettings,
            taxSettings)
    {
        _storeContext = storeContext;
        _erpCustomerFunctionalityService = erpCustomerFunctionalityService;
        _erpSalesOrgService = erpSalesOrgService;
        _erpOrderAdditionalDataService = erpOrderAdditionalDataService;
        _erpShipToAddressService = erpShipToAddressService;
        _erpOrderItemAdditionalDataService = erpOrderItemAdditionalDataService;
        _erpAccountService = erpAccountService;
        _erpNopUserService = erpNopUserService;
        _erpLogsService = erpLogsService;
        _erpIntegrationPluginManager = erpIntegrationPluginManager;
        _erpSpecificationAttributeService = erpSpecificationAttributeService;
        _erpWorkflowMessageService = erpWorkflowMessageService;
        _b2BB2CFeaturesSettings = b2BB2CFeaturesSettings;
        _erpSpecialPriceService = erpSpecialPriceService;
        _b2CShoppingCartItemService = b2CShoppingCartItemService;
        _b2CUserStockRestrictionService = b2CUserStockRestrictionService;
        _erpSapErrorMsgTranslationService = erpSapErrorMsgTranslationService;
    }

    #endregion Ctor

    #region Methods

    private static ErpShipToAddress CloneErpShipToAddress(ErpShipToAddress source)
    {
        if (source == null)
            return null;

        var clone = new ErpShipToAddress();
        var properties = typeof(ErpShipToAddress).GetProperties()
            .Where(p => p.CanRead && p.CanWrite && p.Name != "Id");

        foreach (var prop in properties)
        {
            prop.SetValue(clone, prop.GetValue(source));
        }

        clone.CreatedOnUtc = DateTime.UtcNow;
        return clone;
    }

    private KeyValuePair<string, string> GetMasterProductAndBatchCodeFromProduct(Product product)
    {
        if (_b2BB2CFeaturesSettings.UseManufacturerPartNumberAsItemNo)
        {
            return new KeyValuePair<string, string>(
                            string.IsNullOrEmpty(product.ManufacturerPartNumber) ? product.Sku : product.ManufacturerPartNumber,
                            string.IsNullOrEmpty(product.ManufacturerPartNumber) ? "" : product.Sku.Replace(product.ManufacturerPartNumber, ""));
        }

        return new KeyValuePair<string, string>(product.Sku, "");
    }

    protected virtual async Task AddOrderNoteForCustomerAsync(Order order, string note)
    {
        await _orderService.InsertOrderNoteAsync(new OrderNote
        {
            OrderId = order.Id,
            Note = note,
            DisplayToCustomer = true,
            CreatedOnUtc = DateTime.UtcNow
        });
    }


    #region Common

    private async Task<IList<ErpPlaceOrderItemDataModel>> GetDetailLinesAsync(Order nopOrder)
    {
        if (nopOrder is null)
        {
            return new List<ErpPlaceOrderItemDataModel>();
        }

        var detailLines = new List<ErpPlaceOrderItemDataModel>();
        var orderItems = await _orderService.GetOrderItemsAsync(nopOrder.Id);
        foreach (var nopOrderItem in orderItems)
        {
            var unitPriceWithDiscount = nopOrderItem.UnitPriceExclTax;
            if (nopOrderItem.DiscountAmountExclTax > 0)
            {
                unitPriceWithDiscount =
                    unitPriceWithDiscount
                    + (nopOrderItem.DiscountAmountExclTax / nopOrderItem.Quantity);
            }

            try
            {
                var uom =
                    await _erpSpecificationAttributeService.GetProductUOMByProductIdAndSpecificationAttributeId(
                        nopOrderItem.ProductId,
                        _b2BB2CFeaturesSettings.UnitOfMeasureSpecificationAttributeId
                    ) ?? string.Empty;

                var totalQuantityDiscount = _currencyService.ConvertCurrency(
                    nopOrderItem.DiscountAmountExclTax,
                    nopOrder.CurrencyRate
                );
                var unitDiscount = Math.Round(totalQuantityDiscount / nopOrderItem.Quantity, 2);

                var product = await _productService.GetProductByIdAsync(nopOrderItem.ProductId);

                detailLines.Add(
                    new ErpPlaceOrderItemDataModel
                    {
                        Sku = GetMasterProductAndBatchCodeFromProduct(product).Key,
                        BatchCode = GetMasterProductAndBatchCodeFromProduct(product).Value,
                        Description = product.Name,
                        Quantity = nopOrderItem.Quantity,
                        UnitOfMeasure = uom,
                        SpecialInstruction = "",
                        UnitPriceExclTax = Math.Round(
                            _currencyService.ConvertCurrency(
                                unitPriceWithDiscount,
                                nopOrder.CurrencyRate
                            ),
                            2
                        ),
                        Discount = unitDiscount,
                        TotalQuantityDiscount = totalQuantityDiscount,
                        PriceExclTax = Math.Round(
                            _currencyService.ConvertCurrency(
                                nopOrderItem.PriceExclTax,
                                nopOrder.CurrencyRate
                            ),
                            2
                        ),
                        PriceInclTax = Math.Round(
                            _currencyService.ConvertCurrency(
                                nopOrderItem.PriceInclTax,
                                nopOrder.CurrencyRate
                            ),
                            2
                        ),
                    }
                );
            }
            catch (Exception ex)
            {
                await _erpLogsService.ErrorAsync(
                    $"B2B ErpOrderPlaceModel DetailLine Prepare error (while retry erp place) for Nop Order Id: {nopOrder.Id}",
                    ErpSyncLevel.Order,
                    ex
                );
            }
        }

        return detailLines;
    }

    private bool IsQuoteOrder(ErpOrderType erpOrderType)
    {
        if (erpOrderType == ErpOrderType.B2BQuote || erpOrderType == ErpOrderType.B2CQuote)
            return true;
        return false;
    }

    public override async Task<PlaceOrderResult> PlaceOrderAsync(
        ProcessPaymentRequest processPaymentRequest
    )
    {
        ArgumentNullException.ThrowIfNull(processPaymentRequest);

        var result = new PlaceOrderResult();
        try
        {
            if (processPaymentRequest.OrderGuid == Guid.Empty)
                throw new Exception("Order GUID is not generated");

            var details = await PreparePlaceOrderDetailsAsync(processPaymentRequest);

            var processPaymentResult = await GetProcessPaymentResultAsync(
                processPaymentRequest,
                details
            ) ?? throw new NopException("processPaymentResult is not available");

            if (processPaymentResult.Success)
            {
                var order = await SaveOrderDetailsAsync(
                    processPaymentRequest,
                    processPaymentResult,
                    details
                );
                result.PlacedOrder = order;

                var nopUser = await _erpCustomerFunctionalityService.GetActiveErpNopUserByCustomerAsync(details?.Customer);

                if (nopUser != null && nopUser.ErpUserType == ErpUserType.B2CUser)
                {
                    var isQouteOrder = await _genericAttributeService.GetAttributeAsync<bool>(await _workContext.GetCurrentCustomerAsync(), B2BB2CFeaturesDefaults.B2CQouteOrderAttribute, (await _storeContext.GetCurrentStoreAsync()).Id);

                    await PlaceErpOrderAtNopAsync(order, isQouteOrder ? ErpOrderType.B2CQuote : ErpOrderType.B2CSalesOrder);
                }

                await MoveShoppingCartItemsToOrderItemsAsync(details, order);

                await SaveDiscountUsageHistoryAsync(details, order);

                await SaveGiftCardUsageHistoryAsync(details, order);

                if (details.IsRecurringShoppingCart)
                {
                    await CreateFirstRecurringPaymentAsync(processPaymentRequest, order);
                }

                await SendNotificationsAndSaveNotesAsync(order);

                await _customerService.ResetCheckoutDataAsync(
                    details.Customer,
                    processPaymentRequest.StoreId,
                    clearCouponCodes: true,
                    clearCheckoutAttributes: true
                );
                await _customerActivityService.InsertActivityAsync(
                    "PublicStore.PlaceOrder",
                    string.Format(
                        await _localizationService.GetResourceAsync(
                            "ActivityLog.PublicStore.PlaceOrder"
                        ),
                        order.Id
                    ),
                    order
                );

                await CheckOrderStatusAsync(order);

                await _eventPublisher.PublishAsync(new OrderPlacedEvent(order));

                if (order.PaymentStatus == PaymentStatus.Paid)
                    await ProcessOrderPaidAsync(order);

                if (nopUser != null && nopUser.ErpUserType == ErpUserType.B2CUser)
                {
                    var erpOrderAdditionalData = await _erpOrderAdditionalDataService.GetErpOrderAdditionalDataByNopOrderIdAsync(order.Id);

                    // we will place order at ERP only if order Integration Status is Queued
                    if (erpOrderAdditionalData != null && erpOrderAdditionalData.IntegrationStatusType == IntegrationStatusType.Queued)
                    {
                        await PlaceERPOrderAtERPAsync(order, erpOrderAdditionalData, nopUser, await GetDetailLinesAsync(order));
                    }
                }
            }
            else
                foreach (var paymentError in processPaymentResult.Errors)
                    result.AddError(
                        string.Format(
                            await _localizationService.GetResourceAsync("Checkout.PaymentError"),
                            paymentError
                        )
                    );
        }
        catch (Exception exc)
        {
            await _erpLogsService.ErrorAsync(exc.Message, ErpSyncLevel.Order, exc);
            result.AddError(exc.Message);
        }

        if (result.Success)
        {
            return result;
        }

        var logError = result.Errors.Aggregate(
            "Error while placing order. ",
            (current, next) => $"{current}Error {result.Errors.IndexOf(next) + 1}: {next}. "
        );
        var customer = await _customerService.GetCustomerByIdAsync(
            processPaymentRequest.CustomerId
        );

        await _erpLogsService.ErrorAsync(logError, ErpSyncLevel.Order, customer: customer);
        return result;
    }

    public async Task<PlaceOrderResult> PlaceQuoteOrderAsync(
        ProcessPaymentRequest processPaymentRequest
    )
    {
        ArgumentNullException.ThrowIfNull(processPaymentRequest);

        (var b2BAccount, var b2BUser, var b2CUser) =
            await GetB2BAccountAndUserOfCurrentCustomerAsync();

        if (b2BAccount == null)
        {
            throw new ArgumentNullException(nameof(b2BAccount));
        }

        if (b2BUser != null)
        {
            if (!await _erpCustomerFunctionalityService.IsConsideredAsB2BOrderByB2BUser(b2BUser))
                return await base.PlaceOrderAsync(processPaymentRequest);
        }
        else if (b2CUser != null)
        {
            if (!await _erpCustomerFunctionalityService.IsConsideredAsB2COrderByB2CUser(b2CUser))
                return await base.PlaceOrderAsync(processPaymentRequest);
        }
        else
            return await base.PlaceOrderAsync(processPaymentRequest);

        var result = new PlaceOrderResult();
        try
        {
            if (processPaymentRequest.OrderGuid == Guid.Empty)
                throw new Exception("Order GUID is not generated");

            var details = await PreparePlaceOrderDetailsAsync(processPaymentRequest);

            var processPaymentResult = new ProcessPaymentResult();
            if (processPaymentResult.Success)
            {
                var order = await SaveOrderDetailsAsync(
                    processPaymentRequest,
                    processPaymentResult,
                    details
                );
                result.PlacedOrder = order;

                if (b2CUser != null)
                {
                    var isQouteOrder = await _genericAttributeService.GetAttributeAsync<bool>(await _workContext.GetCurrentCustomerAsync(), B2BB2CFeaturesDefaults.B2CQouteOrderAttribute, (await _storeContext.GetCurrentStoreAsync()).Id);

                    await PlaceErpOrderAtNopAsync(order, isQouteOrder ? ErpOrderType.B2CQuote : ErpOrderType.B2CSalesOrder);
                }

                await MoveShoppingCartItemsToOrderItemsAsync(details, order);

                await SaveDiscountUsageHistoryAsync(details, order);

                await SaveGiftCardUsageHistoryAsync(details, order);

                if (details.IsRecurringShoppingCart)
                    await CreateFirstRecurringPaymentAsync(processPaymentRequest, order);

                await SendNotificationsAndSaveNotesAsync(order);

                await _customerService.ResetCheckoutDataAsync(
                    details.Customer,
                    processPaymentRequest.StoreId,
                    clearCouponCodes: true,
                    clearCheckoutAttributes: true
                );
                await _customerActivityService.InsertActivityAsync(
                    "PublicStore.PlaceOrder",
                    string.Format(
                        await _localizationService.GetResourceAsync(
                            "ActivityLog.PublicStore.PlaceOrder"
                        ),
                        order.Id
                    ),
                    order
                );

                order.OrderStatus = OrderStatus.Complete;
                await _orderService.UpdateOrderAsync(order);

                if (order.PaymentStatus == PaymentStatus.Paid)
                    await ProcessOrderPaidAsync(order);

                //clear generic Attribute for Qoute Order
                if (b2BUser != null)
                {
                    await _genericAttributeService.SaveAttributeAsync(await _workContext.GetCurrentCustomerAsync(), B2BB2CFeaturesDefaults.B2BQouteOrderAttribute, false, (await _storeContext.GetCurrentStoreAsync()).Id);
                }
                else if (b2CUser != null)
                {
                    await _genericAttributeService.SaveAttributeAsync(await _workContext.GetCurrentCustomerAsync(), B2BB2CFeaturesDefaults.B2CQouteOrderAttribute, false, (await _storeContext.GetCurrentStoreAsync()).Id);

                    var erpOrderAdditionalData = await _erpOrderAdditionalDataService.GetErpOrderAdditionalDataByNopOrderIdAsync(order.Id);

                    // we will place order at ERP only if order Integration Status is Queued
                    if (erpOrderAdditionalData != null && erpOrderAdditionalData.IntegrationStatusType == IntegrationStatusType.Queued)
                    {
                        await PlaceERPOrderAtERPAsync(order, erpOrderAdditionalData, b2CUser, await GetDetailLinesAsync(order));
                    }
                }
            }
            else
                foreach (var paymentError in processPaymentResult.Errors)
                    result.AddError(
                        string.Format(
                            await _localizationService.GetResourceAsync("Checkout.PaymentError"),
                            paymentError
                        )
                    );
        }
        catch (Exception exc)
        {
            await _erpLogsService.ErrorAsync(exc.Message, ErpSyncLevel.Order, exc);
            result.AddError(exc.Message);
        }

        if (result.Success)
        {
            return result;
        }

        var logError = result.Errors.Aggregate(
            "Error while placing order. ",
            (current, next) => $"{current}Error {result.Errors.IndexOf(next) + 1}: {next}. "
        );
        var customer = await _customerService.GetCustomerByIdAsync(
            processPaymentRequest.CustomerId
        );

        await _erpLogsService.ErrorAsync(logError, ErpSyncLevel.Order, customer: customer);
        return result;
    }

    private async Task PlaceOrderOrQuoteOnERPAsync(
        ErpPlaceOrderDataModel erpPlaceOrderDataModel,
        ErpOrderAdditionalData erpOrderAdditionalData,
        ErpNopUser erpNopUser
    )
    {
        var erpIntegrationPlugin =
            await _erpIntegrationPluginManager.LoadActiveERPIntegrationPlugin();

        if (erpIntegrationPlugin == null)
        {
            await _erpLogsService.InsertErpLogAsync(
                ErpLogLevel.Error,
                ErpSyncLevel.Order,
                $"Integration method not found. Unable to place Nop Order (ID: {erpOrderAdditionalData.NopOrderId}) at Erp."
            );
            return;
        }

        var response = await erpIntegrationPlugin.CreateOrderOnErpAsync(erpPlaceOrderDataModel);

        var message = "";

        var integrationError = string.Empty;
        DateTime? quoteExpiryDate = null;
        var orderTypeString = GetOrderTypeStringAsync(erpOrderAdditionalData, erpNopUser) ?? string.Empty;

        if (IsQuoteOrder(erpOrderAdditionalData.ErpOrderType) && response.QuoteExpiryDate.HasValue)
        {
            quoteExpiryDate = response.QuoteExpiryDate.Value;
        }

        if (string.IsNullOrWhiteSpace(response.OrderNumber) || response.OrderNumber.Trim().ToUpper() == "ERROR")
        {
            integrationError = $"Returned Erp number is '{response.OrderNumber}'";
        }
        else
        {
            integrationError = string.Empty;
        }

        if (!response.IsError && !string.IsNullOrWhiteSpace(response.OrderNumber))
        {
            UpdateErpOrderStatus(
                erpOrderAdditionalData,
                IntegrationStatusType.Confirmed,
                orderTypeString,
                null,
                response.OrderNumber,
                quoteExpiryDate);

            await _erpOrderAdditionalDataService.UpdateErpOrderAdditionalDataAsync(
                erpOrderAdditionalData
            );

            message = $"Erp {erpPlaceOrderDataModel.OrderType} placed successfully " +
                $"with Erp {erpPlaceOrderDataModel.OrderType} number: {erpOrderAdditionalData.ErpOrderNumber}";
        }
        else
        {
            integrationError = response.ErrorShortMessage;

            UpdateErpOrderStatus(
                erpOrderAdditionalData,
                IntegrationStatusType.Processing,
                orderTypeString,
                integrationError,
                null);

            await _erpOrderAdditionalDataService.UpdateErpOrderAdditionalDataAsync(
                erpOrderAdditionalData
            );

            message = $"Erp {erpPlaceOrderDataModel.OrderType} placement failed.";

            if (!response.IsError && string.IsNullOrWhiteSpace(response.OrderNumber))
            {
                message += " SAP response has no error, but we are getting the order number as null or empty. Check the payload.";
            }
        }

        erpOrderAdditionalData.LastERPUpdateUtc = DateTime.UtcNow;
        erpOrderAdditionalData.ChangedOnUtc = DateTime.UtcNow;
        await _erpOrderAdditionalDataService.UpdateErpOrderAdditionalDataAsync(
            erpOrderAdditionalData
        );

        await _erpLogsService.InsertErpLogAsync(
            ErpLogLevel.Information,
            ErpSyncLevel.Order,
            $"{message}. {response.ErrorShortMessage}",
            response.ErrorFullMessage
        );
    }

    private void UpdateErpOrderStatus(ErpOrderAdditionalData erpOrderAdditionalData, 
        IntegrationStatusType integrationStatusType, 
        string orderTypeString, 
        string integrationError, 
        string returnedOrderNo, 
        DateTime? quoteExpiryDate = null)
    {
        if (integrationStatusType == IntegrationStatusType.Confirmed)
        {
            erpOrderAdditionalData.ErpOrderNumber = returnedOrderNo;
            erpOrderAdditionalData.ERPOrderStatus = B2BB2CFeaturesDefaults.ErpOrderStatusApproved;
            erpOrderAdditionalData.IntegrationStatusType = IntegrationStatusType.Confirmed;

            if (quoteExpiryDate.HasValue)
            {
                erpOrderAdditionalData.QuoteExpiryDate = quoteExpiryDate.Value;
            }
        }
        else if (integrationStatusType == IntegrationStatusType.Processing)
        {
            if (string.IsNullOrEmpty(integrationError))
                integrationError = "unknown issue";

            erpOrderAdditionalData.IntegrationError = integrationError;
            erpOrderAdditionalData.IntegrationErrorDateTimeUtc = DateTime.UtcNow;
            erpOrderAdditionalData.ERPOrderStatus = B2BB2CFeaturesDefaults.ErpOrderStatusProcessing;
            erpOrderAdditionalData.IntegrationStatusType = IntegrationStatusType.Processing;
        }

        erpOrderAdditionalData.IntegrationRetries = erpOrderAdditionalData.IntegrationRetries == null ? 
            1 : erpOrderAdditionalData.IntegrationRetries.Value + 1;
        erpOrderAdditionalData.LastERPUpdateUtc = DateTime.UtcNow;
        erpOrderAdditionalData.ChangedOnUtc = DateTime.UtcNow;
    }

    #endregion

    #endregion

    #region B2B

    public override async Task<IList<string>> ReOrderAsync(Order order)
    {
        ArgumentNullException.ThrowIfNull(order);

        var customer = await _workContext.GetCurrentCustomerAsync();

        var warnings = new List<string>();

        //move shopping cart items (if possible)
        foreach (var orderItem in await _orderService.GetOrderItemsAsync(order.Id))
        {
            var product = await _productService.GetProductByIdAsync(orderItem.ProductId);

            warnings.AddRange(
                await _shoppingCartService.AddToCartAsync(
                    customer,
                    product,
                    ShoppingCartType.ShoppingCart,
                    order.StoreId,
                    orderItem.AttributesXml,
                    orderItem.UnitPriceExclTax,
                    orderItem.RentalStartDateUtc,
                    orderItem.RentalEndDateUtc,
                    orderItem.Quantity,
                    false
                )
            );
        }

        //set checkout attributes
        //comment the code below if you want to disable this functionality
        await _genericAttributeService.SaveAttributeAsync(
            customer,
            NopCustomerDefaults.CheckoutAttributes,
            order.CheckoutAttributesXml,
            order.StoreId
        );

        return warnings;
    }

    public async Task PlaceErpOrderAtNopAsync(Order order, ErpOrderType erpOrderType)
    {
        var currentStore = await _storeContext.GetCurrentStoreAsync();
        var currentCustomer = await _workContext.GetCurrentCustomerAsync();

        var erpNopUser = await _erpCustomerFunctionalityService.GetActiveErpNopUserByCustomerAsync(
            await _customerService.GetCustomerByIdAsync(order.CustomerId)
        );

        if (erpNopUser == null)
        {
            await _erpLogsService.ErrorAsync(
                $"Erp Nop User not found when trying to place Erp Order at Nop for {erpOrderType} Order (Nop Order Id: {order.Id})",
                ErpSyncLevel.Order);
            return;
        }

        var erpAccount = await _erpAccountService.GetErpAccountByIdAsync(
            erpNopUser.ErpAccountId
        );

        if (erpAccount == null)
        {
            await _erpLogsService.ErrorAsync(
                $"Erp Account not found when trying to place Erp Order at Nop for {erpOrderType} Order (Nop Order Id: {order.Id})",
                ErpSyncLevel.Order);
            return;
        }

        var b2BOrderPlaceByCustomerType = erpNopUser.ErpUserType;

        ErpOrderAdditionalData originalQuoteOrder = null;
        if (erpOrderType == ErpOrderType.B2BSalesOrder || erpOrderType == ErpOrderType.B2CSalesOrder)
        {
            var originalQuoteNopOrderIdReference = await _genericAttributeService.GetAttributeAsync<int>(currentCustomer,
                erpOrderType == ErpOrderType.B2BSalesOrder ? B2BB2CFeaturesDefaults.B2BOriginalB2BQuoteOrderIdReference : B2BB2CFeaturesDefaults.B2COriginalB2CQuoteOrderIdReference,
                currentStore.Id);

            if (originalQuoteNopOrderIdReference > 0)
            {
                originalQuoteOrder = await _erpOrderAdditionalDataService.GetErpOrderAdditionalDataByIdAsync(originalQuoteNopOrderIdReference);

                if (originalQuoteOrder != null)
                {
                   var ordreNote=new OrderNote
                    {
                        OrderId=order.Id,
                        Note = await _localizationService.GetResourceAsync("B2B.B2BOrder.QuoteReferenceNote.QuoteReferenceNumber") + " " + originalQuoteOrder.ErpOrderNumber,
                        DisplayToCustomer = true,
                        CreatedOnUtc = DateTime.UtcNow
                    };
                   await _orderService.InsertOrderNoteAsync(ordreNote);
                }
            }
        }
        var cashRoundingValue = decimal.Zero;
        if (erpNopUser.ErpUserType == ErpUserType.B2CUser)
        {
            cashRoundingValue = await _genericAttributeService.GetAttributeAsync<decimal>(
                order,
                B2BB2CFeaturesDefaults.B2CCashRounding,
                currentStore.Id
            );
        }

        var orderItems = await _orderService.GetOrderItemsAsync(order.Id);

        var specialInstructions = "";
        var customerReference = "";
        if (erpNopUser.ErpUserType == ErpUserType.B2BUser)
        {
            specialInstructions = await _genericAttributeService.GetAttributeAsync<string>(
                currentCustomer,
                B2BB2CFeaturesDefaults.B2BSpecialInstructions,
                currentStore.Id
            );
            customerReference = await _genericAttributeService.GetAttributeAsync<string>(
                currentCustomer,
                B2BB2CFeaturesDefaults.B2BCustomerReferenceAsPO,
                currentStore.Id
            );
        }
        else
        {
            specialInstructions = await _genericAttributeService.GetAttributeAsync<String>(
                currentCustomer,
                B2BB2CFeaturesDefaults.B2CSpecialInstructions,
                currentStore.Id
            );
            customerReference = await _genericAttributeService.GetAttributeAsync<string>(
                currentCustomer,
                B2BB2CFeaturesDefaults.B2CCustomerReferenceAsPO,
                currentStore.Id
            );
            if (string.IsNullOrWhiteSpace(customerReference))
            {
                customerReference = $"{order.Id}";
            }
        }

        if (orderItems.Any(a => a.PriceExclTax == _b2BB2CFeaturesSettings.ProductQuotePrice))
        {
            specialInstructions = await _localizationService.GetResourceAsync("Products.ProductForQuote.SpecialInstructions") + " " + specialInstructions;
        }

        var erpOrderAdditionalData = new ErpOrderAdditionalData
        {
            NopOrderId = order.Id,
            ErpOrderOriginType = ErpOrderOriginType.OnlineOrder,
            ErpAccountId = erpAccount.Id,
            ErpOrderType = erpOrderType,
            QuoteSalesOrderId = originalQuoteOrder?.Id ?? 0,
            IsOrderPlaceNotificationSent = false,
            SpecialInstructions = specialInstructions,
            CustomerReference = customerReference,
            ERPOrderStatus = $"{order.OrderStatus}",
            IntegrationStatusTypeId = (int)(
                order.PaymentStatus == PaymentStatus.Paid
                    ? IntegrationStatusType.Queued
                    : IntegrationStatusType.WaitingForPayment
            ),
            IntegrationError = "",
            IsShippingAddressModified = false,
            ErpShipToAddressId = erpNopUser?.ShippingErpShipToAddressId,
            ErpOrderPlaceByCustomerTypeId = (int)b2BOrderPlaceByCustomerType,
            QuoteExpiryDate =
                erpOrderType == ErpOrderType.B2BQuote || erpOrderType == ErpOrderType.B2CQuote
                    ? DateTime.Now.AddDays(1)
                    : null,
            ChangedById = currentCustomer.Id,
            ChangedOnUtc = DateTime.UtcNow,
            OrderPlacedByNopCustomerId = _workContext.OriginalCustomerIfImpersonated != null ?
                _workContext.OriginalCustomerIfImpersonated.Id : currentCustomer.Id,
            ErpOrderNumber = string.Empty,
            CashRounding = cashRoundingValue,
        };
        order.CustomValuesXml = null;

        var deliveryDate = DateTime.Now.AddDays(1);

        if (b2BOrderPlaceByCustomerType == ErpUserType.B2BUser)
        {
            deliveryDate = await _genericAttributeService.GetAttributeAsync<DateTime>(
                currentCustomer,
                B2BB2CFeaturesDefaults.SelectedB2BDeliveryDateAttribute,
                currentStore.Id
            );
        }

        var isShippingAddressModifiedInCheckout = await _genericAttributeService.GetAttributeAsync<bool>(currentCustomer, B2BB2CFeaturesDefaults.IsShippingAddressModifiedInCheckoutAttribute, currentStore.Id);
        var modifiedShipToAddressIdOnCheckout = await _genericAttributeService.GetAttributeAsync<int>(currentCustomer, B2BB2CFeaturesDefaults.ShippingAddressModifiedIdInCheckoutAttribute, currentStore.Id);

        erpOrderAdditionalData.CustomerReference = $"{customerReference}";
        erpOrderAdditionalData.DeliveryDate = deliveryDate;
        erpOrderAdditionalData.IsShippingAddressModified = isShippingAddressModifiedInCheckout;

        if (order.PickupInStore)
        {
            erpOrderAdditionalData.ErpShipToAddressId = null;
            erpOrderAdditionalData.SpecialInstructions = specialInstructions;
        }
        else
        {
            if(erpNopUser.ErpUserType == ErpUserType.B2BUser)
            {
                ErpShipToAddress shippingB2BShipToAddress = null;
                if (modifiedShipToAddressIdOnCheckout > 0)
                {
                    shippingB2BShipToAddress = await _erpShipToAddressService.GetErpShipToAddressByIdAsync(modifiedShipToAddressIdOnCheckout);
                    if (shippingB2BShipToAddress == null)
                    {
                        shippingB2BShipToAddress = await _erpShipToAddressService.GetErpShipToAddressByNopOrderIdAsync(order.Id);
                    }
                    order.ShippingAddressId = shippingB2BShipToAddress.AddressId;
                }
                else
                    shippingB2BShipToAddress = await _erpShipToAddressService.GetErpShipToAddressByNopOrderIdAsync(order.Id);

                erpOrderAdditionalData.ErpShipToAddressId = shippingB2BShipToAddress?.Id;
                erpOrderAdditionalData.SpecialInstructions = shippingB2BShipToAddress?.DeliveryNotes + specialInstructions;
            }
            else
            {
                var b2cShipToAddress = await _erpShipToAddressService.GetErpShipToAddressByIdAsync(erpNopUser?.ErpShipToAddressId ?? 0);
                erpOrderAdditionalData.ErpShipToAddressId = b2cShipToAddress?.Id;
                erpOrderAdditionalData.SpecialInstructions = b2cShipToAddress?.DeliveryNotes + specialInstructions;
            }
        }

        if (!string.IsNullOrEmpty(erpOrderAdditionalData.SpecialInstructions))
        {
            await AddOrderNoteForCustomerAsync(
                order,
                $"Special Instructions: {erpOrderAdditionalData.SpecialInstructions}"
            );
        }

        if (!string.IsNullOrEmpty(customerReference))
        {
            if (
                erpOrderAdditionalData.ErpOrderType == ErpOrderType.B2BSalesOrder
                || erpOrderAdditionalData.ErpOrderType == ErpOrderType.B2CSalesOrder
            )
            {
                await AddOrderNoteForCustomerAsync(
                    order,
                    $"{await _localizationService.GetResourceAsync("Plugin.Misc.NopStation.B2BB2CFeaturesPlugins.CustomerReference.PurchaseOrderReference")}: {customerReference}"
                );
            }
            else if (
                erpOrderAdditionalData.ErpOrderType == ErpOrderType.B2BQuote
                || erpOrderAdditionalData.ErpOrderType == ErpOrderType.B2CQuote
            )
            {
                await AddOrderNoteForCustomerAsync(
                    order,
                    $"{await _localizationService.GetResourceAsync("Plugin.Misc.NopStation.B2BB2CFeaturesPlugins.CustomerReference.QuoteOrderReference")}: {customerReference}"
                );                 
            }
        }

        await _orderService.UpdateOrderAsync(order);

        if (erpOrderAdditionalData.ErpOrderType == ErpOrderType.B2BQuote ||
            erpOrderAdditionalData.ErpOrderType == ErpOrderType.B2CQuote ||
            order.PaymentStatus == PaymentStatus.Paid)
        {
            erpOrderAdditionalData.IntegrationStatusTypeId = (int)IntegrationStatusType.Queued;
        }
        else
        {
            erpOrderAdditionalData.IntegrationStatusTypeId = (int)
                IntegrationStatusType.WaitingForPayment;
        }

        await _erpOrderAdditionalDataService.InsertErpOrderAdditionalDataAsync(
            erpOrderAdditionalData
        );

        if (erpNopUser.ErpUserType == ErpUserType.B2CUser)
        {
            //2427 clear generic Attribute for cash rounding
            await _genericAttributeService.SaveAttributeAsync<decimal?>(order, B2BB2CFeaturesDefaults.B2CCashRounding, null, currentStore.Id);
        }

        if (originalQuoteOrder != null)
        {
            originalQuoteOrder.QuoteSalesOrderId = erpOrderAdditionalData.Id;
            await _erpOrderAdditionalDataService.UpdateErpOrderAdditionalDataAsync(
                originalQuoteOrder
            );

            //clear generic Attribute for original Quote Order Id as reference
            await _genericAttributeService.SaveAttributeAsync<int?>(currentCustomer,
                erpOrderType == ErpOrderType.B2BSalesOrder
                ? B2BB2CFeaturesDefaults.B2BOriginalB2BQuoteOrderIdReference :
                B2BB2CFeaturesDefaults.B2COriginalB2CQuoteOrderIdReference,
                null,
                currentStore.Id);
        }

        if (b2BOrderPlaceByCustomerType == ErpUserType.B2BUser)
        {
            DateTime? date = null;
            await _genericAttributeService.SaveAttributeAsync(
                currentCustomer,
                B2BB2CFeaturesDefaults.SelectedB2BDeliveryDateAttribute,
                date,
                currentStore.Id
            );
            await _genericAttributeService.SaveAttributeAsync(
                currentCustomer,
                B2BB2CFeaturesDefaults.IsShippingAddressModifiedInCheckoutAttribute,
                false,
                currentStore.Id
            );
            await _genericAttributeService.SaveAttributeAsync<int?>(
                currentCustomer,
                B2BB2CFeaturesDefaults.ShippingAddressModifiedIdInCheckoutAttribute,
                0,
                currentStore.Id
            );
            await _genericAttributeService.SaveAttributeAsync<string>(
                currentCustomer,
                B2BB2CFeaturesDefaults.B2BSpecialInstructions,
                null,
                currentStore.Id
            );
            await _genericAttributeService.SaveAttributeAsync<string>(
                currentCustomer,
                B2BB2CFeaturesDefaults.B2BCustomerReferenceAsPO,
                null,
                currentStore.Id
            );

            #region Prepare ErpOrderItemAdditionalData

            // Insert ErpOrderItemAdditionalData and prepare items to place b2b order at ERP
            var erpPlaceOrderItemList = new List<ErpPlaceOrderItemDataModel>();
            var orderItemCount = 0;

            foreach (var nopOrderItem in orderItems)
            {
                orderItemCount++;

                var uom = await _erpSpecificationAttributeService.GetProductUOMByProductIdAndSpecificationAttributeId(nopOrderItem.ProductId, _b2BB2CFeaturesSettings.UnitOfMeasureSpecificationAttributeId) ?? string.Empty;

                var erpSpecialPrice =
                    await _erpSpecialPriceService.GetErpSpecialPricesByErpAccountIdAndNopProductIdAsync(
                        erpAccount.Id,
                        nopOrderItem.ProductId
                    );
                var discountPercentage = erpSpecialPrice?.DiscountPerc ?? 0;
                var listPrice = erpSpecialPrice?.ListPrice ?? decimal.Zero;
                var priceWithoutDiscount = listPrice * nopOrderItem.Quantity;

                var orderItemAdditionalData = new ErpOrderItemAdditionalData
                {
                    NopOrderItemId = nopOrderItem.Id,
                    ErpOrderId = erpOrderAdditionalData.Id,
                    ErpOrderLineNumber = $"{orderItemCount * 10: 000000}",
                    ErpSalesUoM = uom,
                    ErpOrderLineStatus = "",
                    ErpDeliveryMethod = "",
                    ErpInvoiceNumber = "",
                    ErpOrderLineNotes = "",
                    ChangedBy = currentCustomer.Id,
                    ErpDateRequired = deliveryDate,
                    ErpDateExpected = deliveryDate,
                };

                await _erpOrderItemAdditionalDataService.InsertErpOrderItemAdditionalDataAsync(
                    orderItemAdditionalData
                );
                var unitPriceWithDiscount = nopOrderItem.UnitPriceExclTax;
                if (nopOrderItem.DiscountAmountExclTax > 0)
                {
                    unitPriceWithDiscount =
                        unitPriceWithDiscount
                        + (nopOrderItem.DiscountAmountExclTax / nopOrderItem.Quantity);
                }

                try
                {
                    var product = await _productService.GetProductByIdAsync(nopOrderItem.ProductId);
                    var priceInclTax = Math.Round(
                        _currencyService.ConvertCurrency(nopOrderItem.PriceInclTax, order.CurrencyRate),
                        2
                    );
                    var priceExclTax = Math.Round(
                        _currencyService.ConvertCurrency(nopOrderItem.PriceExclTax, order.CurrencyRate),
                        2
                    );
                    var unitPriceExclTax = Math.Round(
                        _currencyService.ConvertCurrency(unitPriceWithDiscount, order.CurrencyRate),
                        2
                    );

                    var convertedTotalPrice = _currencyService.ConvertCurrency(
                        priceWithoutDiscount,
                        order.CurrencyRate
                    );
                    if (discountPercentage > 0)
                    {
                        priceExclTax = convertedTotalPrice;
                    }

                    var totalQuantityDiscount = _currencyService.ConvertCurrency(nopOrderItem.DiscountAmountExclTax, order.CurrencyRate);
                    var unitDiscount = Math.Round(totalQuantityDiscount / (decimal)nopOrderItem.Quantity, 2);

                    erpPlaceOrderItemList.Add(
                        new ErpPlaceOrderItemDataModel
                        {
                            Sku = GetMasterProductAndBatchCodeFromProduct(product).Key,
                            BatchCode = GetMasterProductAndBatchCodeFromProduct(product).Value,
                            Description = product.Name,
                            Quantity = nopOrderItem.Quantity,
                            UnitOfMeasure = uom,
                            SpecialInstruction = erpOrderAdditionalData.SpecialInstructions,
                            UnitPriceExclTax = unitPriceExclTax,
                            Discount = unitDiscount,
                            TotalQuantityDiscount = totalQuantityDiscount,
                            PriceExclTax = priceExclTax,
                            PriceInclTax = priceInclTax,
                        }
                    );
                }
                catch (Exception ex)
                {
                    await _erpLogsService.ErrorAsync(
                        $"Erp Order Additional Item data prepare error for Account Number: {erpAccount.AccountNumber}, Nop Order Id: {order.Id}",
                        ErpSyncLevel.Order,
                        ex);
                }
            }

            #endregion Prepare ErpOrderItemAdditionalData

            if (_b2BB2CFeaturesSettings.UseERPIntegration &&
                erpOrderAdditionalData.IntegrationStatusType == IntegrationStatusType.Queued)
            {
                await PlaceERPOrderAtERPAsync(order, erpOrderAdditionalData, erpNopUser, erpPlaceOrderItemList, _b2BB2CFeaturesSettings.MaxErpIntegrationOrderPlaceRetries);
            }
        }
        else if (b2BOrderPlaceByCustomerType == ErpUserType.B2CUser)
        {
            await _genericAttributeService.SaveAttributeAsync<string>(
                currentCustomer,
                B2BB2CFeaturesDefaults.B2CSpecialInstructions,
                null,
                currentStore.Id
            );
        }
    }

    public async Task<(bool, string)> RetryPlaceErpOrderAtErpAsync(ErpOrderAdditionalData erpOrderAdditionalData)
    {
        if (!_b2BB2CFeaturesSettings.UseERPIntegration)
            return (false, "Use of Erp Integration is disabled!");

        if (erpOrderAdditionalData == null)
            return (false, "Erp order details is null");

        if (erpOrderAdditionalData.IntegrationStatusType == IntegrationStatusType.Confirmed)
            return (false, "Order already placed at ERP");

        if (erpOrderAdditionalData.ErpOrderType != ErpOrderType.B2BQuote
            && erpOrderAdditionalData.ErpOrderType != ErpOrderType.B2CQuote
            && erpOrderAdditionalData.IntegrationStatusType
                == IntegrationStatusType.WaitingForPayment)
            return (false, "This Order can't be placed at Erp since this order is not paid");

        var nopOrder = await _orderService.GetOrderByIdAsync(erpOrderAdditionalData.NopOrderId);
        if (nopOrder == null)
            return (false, "Nop Order not found");

        var customer = await _customerService.GetCustomerByIdAsync(nopOrder.CustomerId);
        if (customer is null)
            return (false, "Customer not found");

        var erpNopUser = await _erpCustomerFunctionalityService.GetActiveErpNopUserByCustomerAsync(customer);
        if (erpNopUser == null)
            return (false, "Erp Nop User not found");

        var erpAccount = await _erpAccountService.GetErpAccountByIdAsync(
            erpOrderAdditionalData.ErpAccountId
        );
        if (erpAccount is null)
            return (false, "Erp Account not found");

        var erpPlaceOrderItemList = await GetDetailLinesAsync(nopOrder);

        if (
            erpOrderAdditionalData.ErpOrderType == ErpOrderType.B2BQuote
            || erpOrderAdditionalData.ErpOrderType == ErpOrderType.B2CQuote
            || erpOrderAdditionalData.IntegrationStatusType != IntegrationStatusType.WaitingForPayment
        )
        {
            if (erpOrderAdditionalData.IntegrationRetries == null
                || erpOrderAdditionalData.IntegrationRetries == 0)
            {
                erpOrderAdditionalData.IntegrationRetries++;
                await _erpOrderAdditionalDataService.UpdateErpOrderAdditionalDataAsync(erpOrderAdditionalData);
            }

            await PlaceERPOrderAtERPAsync(
                   nopOrder,
                   erpOrderAdditionalData,
                   erpNopUser,
                   erpPlaceOrderItemList,
                   maxRetries: _b2BB2CFeaturesSettings.MaxErpIntegrationOrderPlaceRetries
               );
        }

        return (true, string.Empty);
    }

    public async Task PlaceERPOrderAtERPAsync(
        Order order,
        ErpOrderAdditionalData erpOrderAdditionalData,
        ErpNopUser erpNopUser,
        IList<ErpPlaceOrderItemDataModel> erpPlaceOrderItemData,
        int maxRetries = 0
    )
    {
        if (order == null)
        {
            await _erpLogsService.ErrorAsync(
                "Order not found when trying to place Erp Order at Erp",
                ErpSyncLevel.Order);
            return;
        }
        if (erpOrderAdditionalData == null)
        {
            await _erpLogsService.ErrorAsync(
                $"Erp Order Additional Data not found when trying to place Erp Order at Erp (Nop Order Id: {order.Id})",
                ErpSyncLevel.Order);
            return;
        }
        if (erpNopUser == null)
        {
            await _erpLogsService.ErrorAsync(
                $"Erp Nop User not found when trying to place Erp Order at Erp (Nop Order Id: {order.Id})",
                ErpSyncLevel.Order);
            return;
        }
        if (erpPlaceOrderItemData == null)
        {
            await _erpLogsService.ErrorAsync(
                $"Erp Place Order Item Data not found when trying to place Erp Order at Erp (Nop Order Id: {order.Id})",
                ErpSyncLevel.Order);
            return;
        }
        try
        {
            var erpAccount = await _erpAccountService.GetErpAccountByIdAsync(
                erpNopUser.ErpAccountId
            );
            if (erpAccount == null)
            {
                await _erpLogsService.ErrorAsync(
                    $"Erp Account not found when trying to place Erp Order at Erp (Nop Order Id: {order.Id})",
                    ErpSyncLevel.Order);
                return;
            }
            var accountSalesOrg = await _erpSalesOrgService.GetErpSalesOrgByIdAsync(
                erpAccount.ErpSalesOrgId
            );

            var orderTypeString = GetOrderTypeStringAsync(erpOrderAdditionalData, erpNopUser) ?? string.Empty;

            if (accountSalesOrg == null)
            {
                UpdateErpOrderStatus(
                    erpOrderAdditionalData,
                    IntegrationStatusType.Processing,
                    orderTypeString,
                    await _localizationService.GetResourceAsync("NopStation.Plugin.B2B.B2BB2CFeatures.ErpOrderAdditionalData.NoIntegrationCallHappen"),
                    null);

                await _erpLogsService.ErrorAsync(
                    $"Erp Account Sales Org not found when trying to place Erp Order at Erp (Nop Order Id: {order.Id})",
                    ErpSyncLevel.Order);
                return;
            }

            var currentStore = await _storeContext.GetCurrentStoreAsync();
            var currentCustomer = await _customerService.GetCustomerByIdAsync(order.CustomerId);
            var shippingAddress = await _addressService.GetAddressByIdAsync(
                order.ShippingAddressId ?? 0
            );
            var billingAddress = await _addressService.GetAddressByIdAsync(order.BillingAddressId);
            var erpShipToAddress = erpOrderAdditionalData.ErpShipToAddressId.HasValue
                ? await _erpShipToAddressService.GetErpShipToAddressByIdAsync(erpOrderAdditionalData.ErpShipToAddressId.Value)
                : null;
            var erpShipToAddressOfErpNopUser =
                await _erpShipToAddressService.GetErpShipToAddressByIdAsync(
                    erpNopUser.ErpShipToAddressId
                );

            var erpPlaceOrderDataModel = new ErpPlaceOrderDataModel()
            {
                AccountNumber = erpAccount.AccountNumber,
                AccountName = erpAccount.AccountName,
                User = currentCustomer.Email ?? string.Empty,
                Reference = order.CustomOrderNumber ?? string.Empty,
                Location = accountSalesOrg.Code,
                CustomOrderNumber = string.IsNullOrWhiteSpace(order.CustomOrderNumber) ? $"{order.Id}" : order.CustomOrderNumber,
                DateRequired =
                    order.PickupInStore || erpOrderAdditionalData.DeliveryDate == null
                        ? DateTime.Now
                        : erpOrderAdditionalData.DeliveryDate.Value,
                RepCode =
                    erpShipToAddress == null
                        ? erpShipToAddressOfErpNopUser?.RepNumber ?? string.Empty
                        : erpShipToAddress?.RepNumber ?? string.Empty,
                AddressCode =
                    erpShipToAddress == null
                        ? erpShipToAddressOfErpNopUser?.ShipToCode ?? string.Empty
                        : erpShipToAddress?.ShipToCode ?? string.Empty,
                ShippingAddress = await GetShippingAddressToPlaceErpOrderAsync(
                    order,
                    erpOrderAdditionalData,
                    erpShipToAddress,
                    shippingAddress,
                    erpNopUser,
                    erpShipToAddressOfErpNopUser,
                    currentCustomer
                ),
                BillingAddress = new ErpAddressModel
                {
                    Name = $"{billingAddress.FirstName ?? string.Empty} {billingAddress.LastName ?? string.Empty}",
                    Address1 = billingAddress.Address1 ?? string.Empty,
                    Address2 = billingAddress.Address2 ?? string.Empty,
                    Address3 = "",
                    City = billingAddress.City ?? string.Empty,
                    StateProvince =
                        (await GetStateProvinceAsync(order.BillingAddressId))?.Name ?? "",
                    Region =
                        (await GetStateProvinceAsync(order.BillingAddressId))?.Abbreviation
                        ?? string.Empty,
                    ZipPostalCode = billingAddress.ZipPostalCode ?? string.Empty,
                    Country = (await GetCountryNameAsync(order.BillingAddressId)) ?? string.Empty,
                    PhoneNumber = billingAddress.PhoneNumber ?? string.Empty,
                },
                CustomerName = erpNopUser.ErpUserType == ErpUserType.B2BUser
                                ? erpAccount.AccountName
                                : (
                                    string.IsNullOrWhiteSpace(currentCustomer.Company)
                                    ? currentCustomer.FirstName + " " + currentCustomer.LastName
                                    : currentCustomer.Company
                                  ),
                CustomerNumber = erpAccount.AccountNumber,
                CustomerReference = $"{erpOrderAdditionalData.CustomerReference}",
                DeliveryInstruction = erpOrderAdditionalData.SpecialInstructions ?? string.Empty,

                OrderCategory = "",
                DeliveryMethod = order.PickupInStore
                    ? DELIVERY_METHOD_COLLECT
                    : DELIVERY_METHOD_DELIVERY,
                CustomerFirstName = currentCustomer.FirstName,
                CustomerLastName = currentCustomer.LastName,
                CustomerPhoneNumber = currentCustomer.Phone ?? string.Empty,
                CustomerMobileNumber = billingAddress.PhoneNumber ?? currentCustomer.Phone ?? string.Empty,
                CustomerEmail = currentCustomer.Email,
                VatNumber = erpAccount.VatNumber ?? string.Empty,
                TaxNumber = erpNopUser.ErpUserType == ErpUserType.B2CUser ?
                    await _genericAttributeService
                        .GetAttributeAsync<string>(currentCustomer, B2BB2CFeaturesDefaults.B2CVatNumberAttribute) ?? string.Empty :
                    erpAccount.VatNumber ?? string.Empty,

                OrderType = orderTypeString.ToUpper(),
                QuoteNumber = await GetQuoteNumberAsync(erpOrderAdditionalData),
                OrderTax = Math.Round(
                    _currencyService.ConvertCurrency(order.OrderTax, order.CurrencyRate),
                    2
                ),
                OrderSubtotalExclTax = Math.Round(
                    _currencyService.ConvertCurrency(
                        order.OrderTotal - order.OrderTax,
                        order.CurrencyRate
                    ),
                    2
                ),
                OrderDate = order.CreatedOnUtc,
                CustomerCurrencyCode = order.CustomerCurrencyCode ?? string.Empty,
                ErpPlaceOrderItemDatas = erpPlaceOrderItemData,
                DeliveryDate =
                    erpOrderAdditionalData.DeliveryDate == null
                        ? DateTime.Now
                        : erpOrderAdditionalData.DeliveryDate.Value,
            };

            if (erpNopUser.ErpUserType == ErpUserType.B2CUser)
                erpPlaceOrderDataModel.ShippingAmount = order.OrderShippingExclTax;

            try
            {
                await PlaceOrderOrQuoteOnERPAsync(
                    erpPlaceOrderDataModel,
                    erpOrderAdditionalData,
                    erpNopUser
                );
            }
            catch (Exception ex)
            {
                await _erpLogsService.ErrorAsync(
                    $"Error when trying to place Erp Order at Erp (Nop Order Id: {order.Id}): {ex.Message}. Click view to see details",
                    ErpSyncLevel.Order,
                    ex,
                    currentCustomer
                );

                UpdateErpOrderStatus(
                    erpOrderAdditionalData,
                    IntegrationStatusType.Processing,
                    orderTypeString,
                    "Exception Occurred",
                    null);
            }

            #region Email Notification

            if (erpOrderAdditionalData.IntegrationRetries != null &&
                erpOrderAdditionalData.IntegrationRetries >= maxRetries &&
                erpOrderAdditionalData.IntegrationStatusType != IntegrationStatusType.Confirmed)
            {
                erpOrderAdditionalData.ERPOrderStatus =
                    await _localizationService.GetLocalizedEnumAsync(IntegrationStatusType.Failed);
                erpOrderAdditionalData.IntegrationStatusType = IntegrationStatusType.Failed;

                if (erpNopUser.ErpUserType == ErpUserType.B2BUser)
                {
                    await HandleStockRestrictionAndSalesRepMailForB2BOrderAtERPAsync(
                        order,
                        erpAccount,
                        erpShipToAddress,
                        orderTypeString
                    );
                }

                await _erpWorkflowMessageService.SendOrderOrDeliveryDatesOrShippingCostBAPIFailedMessageAsync(
                    currentCustomer,
                    (int)ERPFailedTypes.CreateOrderBAPIFails,
                    order.Id
                );
            }

            #endregion Email Notification

            var note =
                $"{orderTypeString} place in ERP call complete. Try no: {erpOrderAdditionalData.IntegrationRetries ?? 0}, Integration status: {await _localizationService.GetLocalizedEnumAsync(erpOrderAdditionalData.IntegrationStatusType)}";
            if (!string.IsNullOrEmpty(erpOrderAdditionalData.IntegrationError))
            {
                note = note + $", Integration error: {erpOrderAdditionalData.IntegrationError}";
            }
            await AddOrderNoteAsync(order, note);

            if (
                erpOrderAdditionalData.IntegrationRetries != null
                && erpOrderAdditionalData.IntegrationRetries == 1
                && erpOrderAdditionalData.IntegrationStatusType != IntegrationStatusType.Confirmed
            )
            {
                // we will keep first error as the Nop order notes and mark as shown to customer
                var errornote = await _erpSapErrorMsgTranslationService.GetTranslatedAndCompleteIntegrationErrorMsgAsync(
                                erpOrderAdditionalData.ErpOrderType,
                                erpOrderAdditionalData.IntegrationError
                            );
                await AddOrderNoteForCustomerAsync(order, errornote);
            }

            if (erpOrderAdditionalData.IntegrationStatusType == IntegrationStatusType.Confirmed)
            {
                // we will keep first error as the Nop order notes and mark as shown to customer
                await _orderService.InsertOrderNoteAsync(
                    new OrderNote
                    {
                        OrderId = order.Id,
                        Note =
                            (erpOrderAdditionalData.ErpOrderType == ErpOrderType.B2BSalesOrder ||
                            erpOrderAdditionalData.ErpOrderType == ErpOrderType.B2CSalesOrder)
                                ? await _localizationService.GetResourceAsync(
                                    "Plugin.Misc.NopStation.B2BB2CFeatures.B2BOrder.SuccessOrderNote.OrderPlacedInERP"
                                )
                                : await _localizationService.GetResourceAsync(
                                    "Plugin.Misc.NopStation.B2BB2CFeatures.B2BOrder.SuccessOrderNote.QuotePlacedInERP"
                                ),
                        DisplayToCustomer = true,
                        CreatedOnUtc = DateTime.UtcNow,
                    }
                );
            }

            await _orderService.UpdateOrderAsync(order);

            if (erpOrderAdditionalData.ErpOrderType == ErpOrderType.B2BSalesOrder ||
                erpOrderAdditionalData.ErpOrderType == ErpOrderType.B2CSalesOrder &&
                erpOrderAdditionalData.IntegrationStatusType == IntegrationStatusType.Confirmed)
            {
                await _genericAttributeService.SaveAttributeAsync<int?>(
                    currentCustomer,
                    B2BB2CFeaturesDefaults.B2BOriginalB2BQuoteOrderIdReference,
                    null,
                    currentStore.Id
                );
                await _genericAttributeService.SaveAttributeAsync<int?>(
                    currentCustomer,
                    B2BB2CFeaturesDefaults.B2BSpecialInstructions,
                    null,
                    currentStore.Id
                );
                await _genericAttributeService.SaveAttributeAsync<int?>(
                    currentCustomer,
                    B2BB2CFeaturesDefaults.B2CSpecialInstructions,
                    null,
                    currentStore.Id
                );

                //2158 warning notification
                if (erpOrderAdditionalData.QuoteSalesOrderId != null)
                {
                    var quoteNopOrder =
                        await _erpOrderAdditionalDataService.GetErpOrderAdditionalDataByByQuoteSalesOrderId(
                            erpOrderAdditionalData.QuoteSalesOrderId.Value
                        );
                    if (quoteNopOrder != null)
                    {
                        await _erpWorkflowMessageService.SendCombinedLineItemWarningMessageAsync(
                            order.Id,
                            quoteNopOrder.NopOrderId,
                            _localizationSettings.DefaultAdminLanguageId
                        );
                    }
                }

                //notifications
                await SendCustomNotificationsAsync(order);
                erpOrderAdditionalData.IsOrderPlaceNotificationSent = true;
            }
            else if (
                erpNopUser.ErpUserType == ErpUserType.B2CUser
                && erpOrderAdditionalData.IntegrationStatusType != IntegrationStatusType.Confirmed
                && erpOrderAdditionalData.IntegrationRetries
                    >= _b2BB2CFeaturesSettings.MaxErpIntegrationOrderPlaceRetries
            )
            {
                if (
                    erpOrderAdditionalData.ErpOrderOriginType == ErpOrderOriginType.OnlineOrder
                    && erpOrderAdditionalData.IsOrderPlaceNotificationSent.HasValue
                    && !erpOrderAdditionalData.IsOrderPlaceNotificationSent.Value

                )
                {
                    //notifications
                    await SendCustomNotificationsAsync(order);
                    erpOrderAdditionalData.IsOrderPlaceNotificationSent = true;
                }
            }

            erpOrderAdditionalData.LastERPUpdateUtc = DateTime.UtcNow;
            erpOrderAdditionalData.ChangedOnUtc = DateTime.UtcNow;
            await _erpOrderAdditionalDataService.UpdateErpOrderAdditionalDataAsync(
                erpOrderAdditionalData
            );
        }
        catch (Exception ex)
        {
            await _erpLogsService.InsertErpLogAsync(
                ErpLogLevel.Error,
                ErpSyncLevel.Order,
                $"Error when trying to place Erp Order at Erp (Nop Order Id: {order.Id}): {ex.Message}. Click view to see details",
                ex.StackTrace
            );
        }
    }

    #endregion

    #region Utilities

    protected override async Task<Order> SaveOrderDetailsAsync(
        ProcessPaymentRequest processPaymentRequest,
        ProcessPaymentResult processPaymentResult,
        PlaceOrderContainer details
    )
    {
        var currCustomer = await _workContext.GetCurrentCustomerAsync();
        var currStore = await _storeContext.GetCurrentStoreAsync();
        var erpNopUser = await _erpCustomerFunctionalityService.GetActiveErpNopUserByCustomerAsync(
            currCustomer
        );

        var cashRounding = decimal.Zero;
        if (erpNopUser != null && erpNopUser.ErpUserType == ErpUserType.B2CUser)
        {
            var targetTotal = Math.Floor(details.OrderTotal * 10) / 10;
            cashRounding = details.OrderTotal - targetTotal;

            details.OrderSubTotalInclTax = details.OrderSubTotalInclTax - cashRounding;
            details.OrderTotal = details.OrderTotal - cashRounding;
        }

        var order = new Order
        {
            StoreId = processPaymentRequest.StoreId,
            OrderGuid = processPaymentRequest.OrderGuid,
            CustomerId = details.Customer.Id,
            CustomerLanguageId = details.CustomerLanguage.Id,
            CustomerTaxDisplayType = details.CustomerTaxDisplayType,
            CustomerIp = _webHelper.GetCurrentIpAddress(),
            OrderSubtotalInclTax = details.OrderSubTotalInclTax,
            OrderSubtotalExclTax = details.OrderSubTotalExclTax,
            OrderSubTotalDiscountInclTax = details.OrderSubTotalDiscountInclTax,
            OrderSubTotalDiscountExclTax = details.OrderSubTotalDiscountExclTax,
            OrderShippingInclTax = details.OrderShippingTotalInclTax,
            OrderShippingExclTax = details.OrderShippingTotalExclTax,
            PaymentMethodAdditionalFeeInclTax = details.PaymentAdditionalFeeInclTax,
            PaymentMethodAdditionalFeeExclTax = details.PaymentAdditionalFeeExclTax,
            TaxRates = details.TaxRates,
            OrderTax = details.OrderTaxTotal,
            OrderTotal = details.OrderTotal,
            RefundedAmount = decimal.Zero,
            OrderDiscount = details.OrderDiscountAmount,
            CheckoutAttributeDescription = details.CheckoutAttributeDescription,
            CheckoutAttributesXml = details.CheckoutAttributesXml,
            CustomerCurrencyCode = details.CustomerCurrencyCode,
            CurrencyRate = details.CustomerCurrencyRate,
            AffiliateId = details.AffiliateId,
            OrderStatus = OrderStatus.Pending,
            AllowStoringCreditCardNumber = processPaymentResult.AllowStoringCreditCardNumber,
            CardType = processPaymentResult.AllowStoringCreditCardNumber
                ? _encryptionService.EncryptText(processPaymentRequest.CreditCardType)
                : string.Empty,
            CardName = processPaymentResult.AllowStoringCreditCardNumber
                ? _encryptionService.EncryptText(processPaymentRequest.CreditCardName)
                : string.Empty,
            CardNumber = processPaymentResult.AllowStoringCreditCardNumber
                ? _encryptionService.EncryptText(processPaymentRequest.CreditCardNumber)
                : string.Empty,
            MaskedCreditCardNumber = _encryptionService.EncryptText(
                _paymentService.GetMaskedCreditCardNumber(processPaymentRequest.CreditCardNumber)
            ),
            CardCvv2 = processPaymentResult.AllowStoringCreditCardNumber
                ? _encryptionService.EncryptText(processPaymentRequest.CreditCardCvv2)
                : string.Empty,
            CardExpirationMonth = processPaymentResult.AllowStoringCreditCardNumber
                ? _encryptionService.EncryptText(
                    processPaymentRequest.CreditCardExpireMonth.ToString()
                )
                : string.Empty,
            CardExpirationYear = processPaymentResult.AllowStoringCreditCardNumber
                ? _encryptionService.EncryptText(
                    processPaymentRequest.CreditCardExpireYear.ToString()
                )
                : string.Empty,
            PaymentMethodSystemName = processPaymentRequest.PaymentMethodSystemName,
            AuthorizationTransactionId = processPaymentResult.AuthorizationTransactionId,
            AuthorizationTransactionCode = processPaymentResult.AuthorizationTransactionCode,
            AuthorizationTransactionResult = processPaymentResult.AuthorizationTransactionResult,
            CaptureTransactionId = processPaymentResult.CaptureTransactionId,
            CaptureTransactionResult = processPaymentResult.CaptureTransactionResult,
            SubscriptionTransactionId = processPaymentResult.SubscriptionTransactionId,
            PaymentStatus = processPaymentResult.NewPaymentStatus,
            PaidDateUtc = null,
            PickupInStore = details.PickupInStore,
            ShippingStatus = details.ShippingStatus,
            ShippingMethod = details.ShippingMethodName,
            ShippingRateComputationMethodSystemName =
                details.ShippingRateComputationMethodSystemName,
            CustomValuesXml = _paymentService.SerializeCustomValues(processPaymentRequest),
            VatNumber = details.VatNumber,
            CreatedOnUtc = DateTime.UtcNow,
            CustomOrderNumber = string.Empty,
        };

        if (details.BillingAddress is null)
            throw new NopException("Billing address is not provided");

        await _addressService.InsertAddressAsync(details.BillingAddress);
        order.BillingAddressId = details.BillingAddress.Id;

        if (details.PickupAddress != null)
        {
            await _addressService.InsertAddressAsync(details.PickupAddress);
            order.PickupAddressId = details.PickupAddress.Id;
        }

        if (details.ShippingAddress != null)
        {
            await _addressService.InsertAddressAsync(details.ShippingAddress);
            order.ShippingAddressId = details.ShippingAddress.Id;
        }

        await _orderService.InsertOrderAsync(order);

        //generate and set custom order number
        order.CustomOrderNumber = _customNumberFormatter.GenerateOrderCustomNumber(order);
        await _orderService.UpdateOrderAsync(order);

        #region B2B/B2C Customer

        //2427 saving cash rounding for order
        await _genericAttributeService.SaveAttributeAsync(
            order,
            B2BB2CFeaturesDefaults.B2CCashRounding,
            cashRounding,
            order.StoreId
        );


        var erpAccount = await _erpAccountService.GetErpAccountByIdAsync(erpNopUser?.ErpAccountId ?? 0);

        if (erpNopUser != null &&
            erpNopUser.ErpUserType == ErpUserType.B2BUser &&
            erpNopUser.ShippingErpShipToAddressId > 0 &&
            erpAccount != null)
        {
            var checkoutShipToAddress = await _erpShipToAddressService.GetErpShipToAddressByIdAsync(
                erpNopUser.ShippingErpShipToAddressId
            );

            if (checkoutShipToAddress != null)
            {
                var shiptoAddressErpAccountMap = await _erpShipToAddressService.GetErpShipToAddressErpAccountMapByErpShipToAddressIdAsync(
                    checkoutShipToAddress.Id
                );

                if (shiptoAddressErpAccountMap != null &&
                    shiptoAddressErpAccountMap.ErpShipToAddressCreatedByType == ErpShipToAddressCreatedByType.Admin)
                {
                    // if user was not allowed to edit on checkout, then we will clone B2BShipToAddress
                    var newB2BShipToAddress = CloneErpShipToAddress(checkoutShipToAddress);

                    newB2BShipToAddress.OrderId = order.Id;
                    newB2BShipToAddress.AddressId = details.ShippingAddress?.Id ?? newB2BShipToAddress.AddressId;
                    newB2BShipToAddress.CreatedById = details.Customer.Id;
                    newB2BShipToAddress.CreatedOnUtc = DateTime.UtcNow;
                    newB2BShipToAddress.UpdatedById = details.Customer.Id;
                    newB2BShipToAddress.UpdatedOnUtc = DateTime.UtcNow;
                    await _erpShipToAddressService.InsertErpShipToAddressAsync(newB2BShipToAddress);
                    await _erpShipToAddressService.InsertErpShipToAddressErpAccountMapAsync(erpAccount, newB2BShipToAddress, ErpShipToAddressCreatedByType.User);
                }

                else
                {
                    checkoutShipToAddress.OrderId = order.Id;
                    checkoutShipToAddress.AddressId = details.ShippingAddress?.Id ?? checkoutShipToAddress.AddressId;
                    checkoutShipToAddress.UpdatedById = details.Customer.Id;
                    checkoutShipToAddress.UpdatedOnUtc = DateTime.UtcNow;
                    await _erpShipToAddressService.UpdateErpShipToAddressAsync(checkoutShipToAddress);
                }
            }
        }

        //Quote reference Id
        var isQuoteOrder = false;

        if (erpNopUser != null && erpNopUser.ErpUserType == ErpUserType.B2BUser)
        {
            // Update allocated stock at B2BPerAccountProductPricing for each item only for sales order
            isQuoteOrder = await _genericAttributeService.GetAttributeAsync<bool>(
                currCustomer,
                B2BB2CFeaturesDefaults.B2BQouteOrderAttribute,
                currStore.Id
            );
            if (!isQuoteOrder)
            {
                var b2BConvertedQuoteOrderId =
                    await _genericAttributeService.GetAttributeAsync<int>(
                        currCustomer,
                        B2BB2CFeaturesDefaults.B2BConvertedQuoteB2BOrderId,
                        currStore.Id
                    );
                if (b2BConvertedQuoteOrderId > 0)
                {
                    var b2BOrderPerAccount =
                        await _erpOrderAdditionalDataService.GetErpOrderAdditionalDataByIdAsync(
                            b2BConvertedQuoteOrderId
                        );
                    if (
                        await _erpCustomerFunctionalityService.CheckQuoteOrderStatusAsync(
                            b2BOrderPerAccount
                        )
                    )
                    {
                        //save generic Attribute for original B2B Quote Order Id as reference
                        await _genericAttributeService.SaveAttributeAsync(
                            currCustomer,
                            B2BB2CFeaturesDefaults.B2BOriginalB2BQuoteOrderIdReference,
                            b2BOrderPerAccount.Id,
                            currStore.Id
                        );
                    }
                }
            }
        }
        else if (erpNopUser != null && erpNopUser.ErpUserType == ErpUserType.B2CUser)
        {
            // Update allocated stock at B2BPerAccountProductPricing for each item only for sales order
            isQuoteOrder = await _genericAttributeService.GetAttributeAsync<bool>(
                currCustomer,
                B2BB2CFeaturesDefaults.B2CQouteOrderAttribute,
                currStore.Id
            );
            if (!isQuoteOrder)
            {
                var b2CConvertedQuoteOrderId =
                    await _genericAttributeService.GetAttributeAsync<int>(
                        currCustomer,
                        B2BB2CFeaturesDefaults.B2CConvertedQuoteB2COrderId,
                        currStore.Id
                    );
                if (b2CConvertedQuoteOrderId > 0)
                {
                    var b2COrder =
                        await _erpOrderAdditionalDataService.GetErpOrderAdditionalDataByIdAsync(
                            b2CConvertedQuoteOrderId
                        );
                    if (await _erpCustomerFunctionalityService.CheckQuoteOrderStatusAsync(b2COrder))
                    {
                        //save generic Attribute for original B2C Quote Order Id as reference
                        await _genericAttributeService.SaveAttributeAsync(
                            currCustomer,
                            B2BB2CFeaturesDefaults.B2COriginalB2CQuoteOrderIdReference,
                            b2COrder.Id,
                            currStore.Id
                        );
                    }
                }
            }
        }

        #endregion

        //reward points history
        if (details.RedeemedRewardPointsAmount <= decimal.Zero)
            return order;

        order.RedeemedRewardPointsEntryId =
            await _rewardPointService.AddRewardPointsHistoryEntryAsync(
                details.Customer,
                -details.RedeemedRewardPoints,
                order.StoreId,
                string.Format(
                    await _localizationService.GetResourceAsync(
                        "RewardPoints.Message.RedeemedForOrder",
                        order.CustomerLanguageId
                    ),
                    order.CustomOrderNumber
                ),
                order,
                details.RedeemedRewardPointsAmount
            );
        await _customerService.UpdateCustomerAsync(details.Customer);
        await _orderService.UpdateOrderAsync(order);

        return order;
    }

    protected async Task<string> GetQuoteNumberAsync(ErpOrderAdditionalData erpOrderAdditionalData)
    {
        if (erpOrderAdditionalData.ErpOrderType == ErpOrderType.B2BQuote)
            return string.Empty;

        if (
            !erpOrderAdditionalData.QuoteSalesOrderId.HasValue
            || erpOrderAdditionalData.QuoteSalesOrderId.Value < 1
        )
        {
            return string.Empty;
        }
        return (
                await _erpOrderAdditionalDataService.GetErpOrderAdditionalDataByIdAsync(
                    erpOrderAdditionalData.QuoteSalesOrderId.Value
                )
            )?.ErpOrderNumber ?? string.Empty;
    }

    protected string GetOrderTypeStringAsync(ErpOrderAdditionalData erpOrderAdditionalData, ErpNopUser erpNopUser)
    {
        if (erpOrderAdditionalData == null || erpNopUser == null)
            return string.Empty;

        if (erpNopUser.ErpUserType == ErpUserType.B2BUser)
        {
            if (erpOrderAdditionalData.ErpOrderType == ErpOrderType.B2BSalesOrder &&
                erpOrderAdditionalData.QuoteSalesOrderId.HasValue &&
                erpOrderAdditionalData.QuoteSalesOrderId > 0)
            {
                return "Order from Quote";
            }
            else
            {
                return erpOrderAdditionalData.ErpOrderType == ErpOrderType.B2BSalesOrder
                    ? "Order"
                    : "Quote";
            }
        }
        else if (erpNopUser.ErpUserType == ErpUserType.B2CUser)
        {
            if (erpOrderAdditionalData.ErpOrderType == ErpOrderType.B2CSalesOrder &&
                erpOrderAdditionalData.QuoteSalesOrderId.HasValue &&
                erpOrderAdditionalData.QuoteSalesOrderId > 0)
            {
                return "Order from B2C Quote";
            }
            else
            {
                return erpOrderAdditionalData.ErpOrderType == ErpOrderType.B2CSalesOrder
                    ? "B2COrder"
                    : "B2CQuote";
            }
        }
        return string.Empty;
    }

    protected async Task<StateProvince> GetStateProvinceAsync(int? addressId)
    {
        if (addressId.HasValue)
        {
            var address = await _addressService.GetAddressByIdAsync(addressId.Value);
            if (address != null)
            {
                return await _stateProvinceService.GetStateProvinceByAddressAsync(address);
            }
            return null;
        }
        return null;
    }

    protected async Task<string> GetCountryNameAsync(int? addressId)
    {
        if (addressId.HasValue)
        {
            var address = await _addressService.GetAddressByIdAsync(addressId.Value);
            if (address != null)
            {
                var country = await _countryService.GetCountryByAddressAsync(address);
                return (country == null) ? string.Empty : country.Name;
            }
            return string.Empty;
        }
        return string.Empty;
    }

    private async Task<(
        ErpAccount b2BAccount,
        ErpNopUser b2BUser,
        ErpNopUser b2CUser
    )> GetB2BAccountAndUserOfCurrentCustomerAsync()
    {
        var currCustomer = await _workContext.GetCurrentCustomerAsync();
        var b2BAccount = await _erpAccountService.GetActiveErpAccountByCustomerIdAsync(currCustomer.Id);
        var erpUser = await _erpCustomerFunctionalityService.GetActiveErpNopUserByCustomerAsync(currCustomer);

        var b2BUser = erpUser?.ErpUserType == ErpUserType.B2BUser ? erpUser : null;
        var b2CUser = erpUser?.ErpUserType == ErpUserType.B2CUser ? erpUser : null;

        return (b2BAccount, b2BUser, b2CUser);
    }

    public async Task<string> GetB2CQuoteNumberAsync(ErpOrderAdditionalData orderPerUser)
    {
        if (orderPerUser.ErpOrderType == ErpOrderType.B2CQuote)
            return string.Empty;

        if (!orderPerUser.QuoteSalesOrderId.HasValue || orderPerUser.QuoteSalesOrderId.Value < 1)
        {
            return string.Empty;
        }
        var erpOrderAdditionalData =
            await _erpOrderAdditionalDataService.GetErpOrderAdditionalDataByIdAsync(
                orderPerUser.QuoteSalesOrderId.Value
            );

        return erpOrderAdditionalData?.ErpOrderNumber ?? string.Empty;
    }

    private async Task<ErpAddressModel> GetShippingAddressToPlaceErpOrderAsync(
        Order order,
        ErpOrderAdditionalData erpOrderAdditionalData,
        ErpShipToAddress erpShipToAddress,
        Address shippingAddress,
        ErpNopUser erpNopUser,
        ErpShipToAddress erpShipToAddressOfErpNopUser,
        Customer customer
    )
    {
        if (order == null || erpNopUser == null || customer == null)
            return new ErpAddressModel();

        StateProvince stateProvince = null;

        if (erpNopUser.ErpUserType == ErpUserType.B2BUser)
        {
            if (order.PickupInStore)
            {
                stateProvince = await GetStateProvinceAsync(order.PickupAddressId);
                return new ErpAddressModel
                {
                    Address1 = stateProvince?.Abbreviation ?? string.Empty,
                    Region = stateProvince?.Abbreviation ?? string.Empty,
                    Country = await GetCountryNameAsync(order.PickupAddressId) ?? string.Empty,
                };
            }

            if (
                order.ShippingAddressId == 0
                || (
                    erpOrderAdditionalData is not null
                    && !erpOrderAdditionalData.IsShippingAddressModified
                )
            )
            {
                stateProvince = await GetStateProvinceAsync(order.ShippingAddressId);
                return new ErpAddressModel
                {
                    Address1 = stateProvince?.Abbreviation ?? string.Empty,
                    Region = stateProvince?.Abbreviation ?? string.Empty,
                    Country = await GetCountryNameAsync(order.ShippingAddressId) ?? string.Empty,
                };
            }
            else
            {
                stateProvince = await GetStateProvinceAsync(order.ShippingAddressId);
                return new ErpAddressModel
                {
                    Name = erpShipToAddress?.ShipToName ?? string.Empty,
                    Address1 = shippingAddress?.Address1 ?? string.Empty,
                    Address2 = shippingAddress?.Address2 ?? string.Empty,
                    Address3 = stateProvince?.Abbreviation ?? string.Empty,
                    City = shippingAddress?.City ?? string.Empty,
                    StateProvince = stateProvince?.Name ?? string.Empty,
                    Region = stateProvince?.Abbreviation ?? string.Empty,
                    ZipPostalCode = shippingAddress.ZipPostalCode ?? string.Empty,
                    Country = await GetCountryNameAsync(order.ShippingAddressId) ?? string.Empty,
                    PhoneNumber = shippingAddress.PhoneNumber ?? string.Empty,
                    Suburb = erpShipToAddress?.Suburb.ToUpper() ?? string.Empty,
                };
            }
        }
        else
        {
            if (order.PickupInStore)
            {
                var nopAddressOfB2CUser = await _addressService.GetAddressByIdAsync(
                    erpShipToAddressOfErpNopUser?.AddressId ?? 0
                );
                stateProvince = await GetStateProvinceAsync(nopAddressOfB2CUser.Id);
                return new ErpAddressModel
                {
                    Address1 = nopAddressOfB2CUser.Address1 ?? string.Empty,
                    Address2 = nopAddressOfB2CUser.Address2 ?? string.Empty,
                    Address3 = stateProvince?.Abbreviation ?? string.Empty,
                    City = nopAddressOfB2CUser.City ?? string.Empty,
                    Country =
                        await GetCountryNameAsync(nopAddressOfB2CUser.CountryId) ?? string.Empty,
                    Name = customer.Company ?? string.Empty,
                    PhoneNumber = nopAddressOfB2CUser.PhoneNumber ?? string.Empty,
                    ZipPostalCode = nopAddressOfB2CUser.ZipPostalCode ?? string.Empty,
                    Region = stateProvince?.Abbreviation ?? string.Empty,
                    StateProvince = stateProvince?.Name ?? string.Empty,
                    Suburb = erpShipToAddressOfErpNopUser?.Suburb ?? string.Empty,
                };
            }

            if (shippingAddress == null)
            {
                return new ErpAddressModel();
            }
            else
            {
                stateProvince = await GetStateProvinceAsync(shippingAddress.Id);
                return new ErpAddressModel
                {
                    Name = customer.Company ?? string.Empty,
                    Address1 = shippingAddress.Address1 ?? string.Empty,
                    Address2 = shippingAddress.Address2 ?? string.Empty,
                    Address3 = stateProvince?.Abbreviation ?? string.Empty,
                    City = shippingAddress.City ?? string.Empty,
                    StateProvince = stateProvince?.Name ?? string.Empty,
                    Region = stateProvince?.Abbreviation ?? string.Empty,
                    ZipPostalCode = shippingAddress.ZipPostalCode ?? string.Empty,
                    Country = await GetCountryNameAsync(shippingAddress.CountryId) ?? string.Empty,
                    PhoneNumber = shippingAddress.PhoneNumber ?? string.Empty,
                    Suburb = erpShipToAddressOfErpNopUser?.Suburb ?? string.Empty,
                };
            }
        }
    }

    private async Task HandleStockRestrictionAndSalesRepMailForB2BOrderAtERPAsync(
        Order order,
        ErpAccount erpAccount,
        ErpShipToAddress erpShipToAddress,
        string orderTypeString
    )
    {
        if (order == null || erpAccount == null)
            return;

        //If the order fails to be created in SAP, we must reset the stock % to what it was originally

        #region 2581 failed/cancelled order rule

        //If the order is marked as cancelled on e-commerce,
        //the 48 hour stock restriction should not be applied, (means revert the percentage)
        var erpOrder =
            await _erpOrderAdditionalDataService.GetErpOrderAdditionalDataByNopOrderIdAsync(
                order.Id
            );

        var orderItemList = await _orderService.GetOrderItemsAsync(order.Id);

        //Adjust inventory and PercentageOfStockAllowed
        foreach (var orderItem in orderItemList)
        {
            var product = await _productService.GetProductByIdAsync(orderItem.ProductId);
            if (product is null)
                continue;

            await _productService.AdjustInventoryAsync(
                product,
                orderItem.Quantity,
                orderItem.AttributesXml,
                string.Format(
                    await _localizationService.GetResourceAsync(
                        "Admin.StockQuantityHistory.Messages.CancelOrder"
                    ),
                    order.Id
                )
            );

            if (
                (erpOrder != null && erpOrder.ErpOrderTypeId != (int)ErpOrderType.B2BQuote)
                || (
                    erpOrder != null
                    && erpOrder.ErpOrderTypeId != (int)ErpOrderType.B2CQuote
                    && order.PaymentStatus == PaymentStatus.Pending
                )
            )
            {
                if (erpAccount.PercentageOfStockAllowed < 100)
                {
                    var erpSpecialPrice =
                        await _erpSpecialPriceService.GetErpSpecialPricesByErpAccountIdAndNopProductIdAsync(
                            erpAccount.Id,
                            product.Id
                        );

                    if (erpSpecialPrice != null && erpSpecialPrice.Id > 0)
                    {
                        decimal currPercentageOfAllocatedStock;
                        if (
                            !erpSpecialPrice.PercentageOfAllocatedStockResetTimeUtc.HasValue
                            || erpSpecialPrice.PercentageOfAllocatedStockResetTimeUtc.Value
                                < DateTime.UtcNow
                        )
                        {
                            // if PercentageOfAllocatedStockResetTimeUtc doesn't have any value
                            // or PercentageOfAllocatedStockResetTimeUtc has passed already
                            // then take value from account
                            currPercentageOfAllocatedStock = erpAccount.PercentageOfStockAllowed ?? 0;
                        }
                        else
                        {
                            currPercentageOfAllocatedStock =
                                erpSpecialPrice.PercentageOfAllocatedStock;
                        }

                        var currentStock = await _productService.GetTotalStockQuantityAsync(
                            product
                        );
                        var beforePurchaseAllowedQuantity_c = currentStock + orderItem.Quantity;
                        var afterCancelPercentageOfStockAllowed = decimal.Zero;
                        try
                        {
                            afterCancelPercentageOfStockAllowed =
                                currPercentageOfAllocatedStock
                                * beforePurchaseAllowedQuantity_c
                                / currentStock;
                        }
                        catch (Exception ex)
                        {
                            await _erpLogsService.ErrorAsync(ex.Message, ErpSyncLevel.Order, ex);
                        }

                        if (
                            beforePurchaseAllowedQuantity_c > 0
                            && afterCancelPercentageOfStockAllowed > 0
                        )
                        {
                            // save new percentage and reset time
                            erpSpecialPrice.PercentageOfAllocatedStock =
                                afterCancelPercentageOfStockAllowed;

                            if (
                                !erpSpecialPrice.PercentageOfAllocatedStockResetTimeUtc.HasValue
                                || erpSpecialPrice.PercentageOfAllocatedStockResetTimeUtc.Value
                                    < DateTime.UtcNow
                            )
                            {
                                var exactResetTime = DateTime.UtcNow.AddDays(
                                    _b2BB2CFeaturesSettings.PercentageOfAllocatedStockResetTimeUtc
                                        - 1
                                );
                                erpSpecialPrice.PercentageOfAllocatedStockResetTimeUtc =
                                    new DateTime(
                                        exactResetTime.Year,
                                        exactResetTime.Month,
                                        exactResetTime.Day,
                                        23,
                                        59,
                                        59
                                    );
                            }

                            await _erpSpecialPriceService.UpdateErpSpecialPriceAsync(
                                erpSpecialPrice
                            );
                        }
                    }
                }
            }
        }

        #endregion 2581 failed/cancelled order rule

        if (!string.IsNullOrEmpty(erpShipToAddress?.RepEmail))
        {
            //sent email notification
            var queuedEmailId =
                await _erpWorkflowMessageService.SendERPOrderPlaceFailedSalesRepNotificationAsync(
                    order,
                    order.CustomerLanguageId,
                    erpShipToAddress
                );
            if (queuedEmailId > 0)
                await AddOrderNoteAsync(
                    order,
                    $"\"erp {orderTypeString ?? string.Empty} place failed\" email (to sales rep) has been queued. queued email identifier: {queuedEmailId}."
                );
        }
    }

    protected async Task SendCustomNotificationsAsync(Order order)
    {
        if (order == null)
            return;

        //send email notifications
        var orderPlacedStoreOwnerNotificationQueuedEmailIds =
            await _workflowMessageService.SendOrderPlacedStoreOwnerNotificationAsync(
                order,
                _localizationSettings.DefaultAdminLanguageId
            );
        if (orderPlacedStoreOwnerNotificationQueuedEmailIds.Any())
            await AddOrderNoteAsync(
                order,
                $"\"Order placed\" email (to store owner) has been queued. Queued email identifiers: {string.Join(", ", orderPlacedStoreOwnerNotificationQueuedEmailIds)}."
            );

        var orderPlacedAttachmentFilePath = _orderSettings.AttachPdfInvoiceToOrderPlacedEmail
            ? await _pdfService.SaveOrderPdfToDiskAsync(order)
            : null;
        var orderPlacedAttachmentFileName = _orderSettings.AttachPdfInvoiceToOrderPlacedEmail
            ? "order.pdf"
            : null;
        var orderPlacedCustomerNotificationQueuedEmailIds =
            await _workflowMessageService.SendOrderPlacedCustomerNotificationAsync(
                order,
                order.CustomerLanguageId,
                orderPlacedAttachmentFilePath,
                orderPlacedAttachmentFileName
            );
        if (orderPlacedCustomerNotificationQueuedEmailIds.Any())
            await AddOrderNoteAsync(
                order,
                $"\"Order placed\" email (to customer) has been queued. Queued email identifiers: {string.Join(", ", orderPlacedCustomerNotificationQueuedEmailIds)}."
            );

        var vendors = await GetVendorsInOrderAsync(order);
        foreach (var vendor in vendors)
        {
            var orderPlacedVendorNotificationQueuedEmailIds =
                await _workflowMessageService.SendOrderPlacedVendorNotificationAsync(
                    order,
                    vendor,
                    _localizationSettings.DefaultAdminLanguageId
                );
            if (orderPlacedVendorNotificationQueuedEmailIds.Any())
                await AddOrderNoteAsync(
                    order,
                    $"\"Order placed\" email (to vendor) has been queued. Queued email identifiers: {string.Join(", ", orderPlacedVendorNotificationQueuedEmailIds)}."
                );
        }

        if (order.AffiliateId == 0)
            return;

        var orderPlacedAffiliateNotificationQueuedEmailIds =
            await _workflowMessageService.SendOrderPlacedAffiliateNotificationAsync(
                order,
                _localizationSettings.DefaultAdminLanguageId
            );
        if (orderPlacedAffiliateNotificationQueuedEmailIds.Any())
            await AddOrderNoteAsync(
                order,
                $"\"Order placed\" email (to affiliate) has been queued. Queued email identifiers: {string.Join(", ", orderPlacedAffiliateNotificationQueuedEmailIds)}."
            );
    }

    protected async Task SaveNotesAsync(Order order)
    {
        if (order == null)
            return;
        //notes, messages
        if (_workContext.OriginalCustomerIfImpersonated != null)
        {
            await _orderService.InsertOrderNoteAsync(new OrderNote
            {
                OrderId = order.Id,
                Note = $"Order placed by ({_workContext.OriginalCustomerIfImpersonated.Email}), representing the customer.",
                DisplayToCustomer = true,
                CreatedOnUtc = DateTime.UtcNow
            });
        }

        await AddOrderNoteAsync(order, _workContext.OriginalCustomerIfImpersonated != null
            ? $"Order placed by a store owner ('{_workContext.OriginalCustomerIfImpersonated.Email}'. ID = {_workContext.OriginalCustomerIfImpersonated.Id}) impersonating the customer."
            : "Order placed");
    }

    /// <summary>
    /// Send "order placed" notifications and save order notes
    /// </summary>
    /// <param name="order">Order</param>
    protected async Task SendNotificationsAsync(Order order)
    {
        if (order == null)
            return;

        //send email notifications
        var orderPlacedStoreOwnerNotificationQueuedEmailIds = await _workflowMessageService.SendOrderPlacedStoreOwnerNotificationAsync(order, _localizationSettings.DefaultAdminLanguageId);
        if (orderPlacedStoreOwnerNotificationQueuedEmailIds.Any())
            await AddOrderNoteAsync(order, $"\"Order placed\" email (to store owner) has been queued. Queued email identifiers: {string.Join(", ", orderPlacedStoreOwnerNotificationQueuedEmailIds)}.");

        var orderPlacedAttachmentFilePath = _orderSettings.AttachPdfInvoiceToOrderPlacedEmail ?
          await _pdfService.SaveOrderPdfToDiskAsync(order) : null;
        var orderPlacedAttachmentFileName = _orderSettings.AttachPdfInvoiceToOrderPlacedEmail ?
            "order.pdf" : null;
        var orderPlacedCustomerNotificationQueuedEmailIds = await _workflowMessageService
            .SendOrderPlacedCustomerNotificationAsync(order, order.CustomerLanguageId, orderPlacedAttachmentFilePath, orderPlacedAttachmentFileName);
        if (orderPlacedCustomerNotificationQueuedEmailIds.Any())
            await AddOrderNoteAsync(order, $"\"Order placed\" email (to customer) has been queued. Queued email identifiers: {string.Join(", ", orderPlacedCustomerNotificationQueuedEmailIds)}.");

        var vendors = await GetVendorsInOrderAsync(order);
        foreach (var vendor in vendors)
        {
            var orderPlacedVendorNotificationQueuedEmailIds = await _workflowMessageService.SendOrderPlacedVendorNotificationAsync(order, vendor, _localizationSettings.DefaultAdminLanguageId);
            if (orderPlacedVendorNotificationQueuedEmailIds.Any())
                await AddOrderNoteAsync(order, $"\"Order placed\" email (to vendor) has been queued. Queued email identifiers: {string.Join(", ", orderPlacedVendorNotificationQueuedEmailIds)}.");
        }

        if (order.AffiliateId == 0)
            return;

        var orderPlacedAffiliateNotificationQueuedEmailIds = await _workflowMessageService.SendOrderPlacedAffiliateNotificationAsync(order, _localizationSettings.DefaultAdminLanguageId);
        if (orderPlacedAffiliateNotificationQueuedEmailIds.Any())
            await AddOrderNoteAsync(order, $"\"Order placed\" email (to affiliate) has been queued. Queued email identifiers: {string.Join(", ", orderPlacedAffiliateNotificationQueuedEmailIds)}.");
    }

    protected override async Task SendNotificationsAndSaveNotesAsync(Order order)
    {
        await SaveNotesAsync(order);

        await SendNotificationsAsync(order);
    }

    protected override async Task MoveShoppingCartItemsToOrderItemsAsync(
        PlaceOrderContainer details,
        Order order
    )
    {
        if (details == null || order == null)
            return;

        var customer = await _workContext.GetCurrentCustomerAsync();
        var currentStore = await _storeContext.GetCurrentStoreAsync();

        var isQouteOrder = await _genericAttributeService.GetAttributeAsync<bool>(
            customer,
            B2BB2CFeaturesDefaults.B2BQouteOrderAttribute,
            currentStore.Id
        );
        var erpAccount = await _erpAccountService.GetActiveErpAccountByCustomerIdAsync(
            order.CustomerId
        );
        var erpNopUser = await _erpNopUserService.GetErpNopUserByCustomerIdAsync(order.CustomerId, showHidden: false);
        var erpOrderAdditionalData = new ErpOrderAdditionalData();
        var latestDeliveryDate = new DateTime();
        var orderItemCount = 0;

        if (
            erpAccount != null
            && erpNopUser != null
            && erpNopUser.ErpUserType == ErpUserType.B2CUser
        )
        {
            isQouteOrder = await _genericAttributeService.GetAttributeAsync<bool>(
                customer,
                B2BB2CFeaturesDefaults.B2CQouteOrderAttribute,
                currentStore.Id
            );

            erpOrderAdditionalData =
                await _erpOrderAdditionalDataService.GetErpOrderAdditionalDataByNopOrderIdAsync(
                    order.Id
                );

            if (erpOrderAdditionalData == null)
            {
                await _erpLogsService.ErrorAsync(
                    $"B2C Move Shopping Cart Items To Order Items: B2C Order not found: {erpAccount.AccountNumber}, customer: {customer.Email}", ErpSyncLevel.Order
                );
            }
        }

        var cart = details.Cart.ToList();
        var products = new List<Product>();

        foreach (var sc in details.Cart)
        {
            var product = await _productService.GetProductByIdAsync(sc.ProductId);
            if (product is null)
                continue;

            products.Add(product);

            var scUnitPrice = (await _shoppingCartService.GetUnitPriceAsync(sc, true)).unitPrice;
            var (scSubTotal, discountAmount, scDiscounts, _) =
                await _shoppingCartService.GetSubTotalAsync(sc, true);
            var scUnitPriceInclTax = await _taxService.GetProductPriceAsync(
                product,
                scUnitPrice,
                true,
                details.Customer
            );
            var scUnitPriceExclTax = await _taxService.GetProductPriceAsync(
                product,
                scUnitPrice,
                false,
                details.Customer
            );
            var scSubTotalInclTax = await _taxService.GetProductPriceAsync(
                product,
                scSubTotal,
                true,
                details.Customer
            );
            var scSubTotalExclTax = await _taxService.GetProductPriceAsync(
                product,
                scSubTotal,
                false,
                details.Customer
            );
            var discountAmountInclTax = await _taxService.GetProductPriceAsync(
                product,
                discountAmount,
                true,
                details.Customer
            );
            var discountAmountExclTax = await _taxService.GetProductPriceAsync(
                product,
                discountAmount,
                false,
                details.Customer
            );
            foreach (var disc in scDiscounts)
                if (!_discountService.ContainsDiscount(details.AppliedDiscounts, disc))
                    details.AppliedDiscounts.Add(disc);

            var store = await _storeService.GetStoreByIdAsync(sc.StoreId);
            var attributeDescription = await _productAttributeFormatter.FormatAttributesAsync(
                product,
                sc.AttributesXml,
                details.Customer,
                store
            );
            var itemWeight = await _shippingService.GetShoppingCartItemWeightAsync(sc);
            var orderItem = new OrderItem
            {
                OrderItemGuid = Guid.NewGuid(),
                OrderId = order.Id,
                ProductId = product.Id,
                UnitPriceInclTax = scUnitPriceInclTax.price,
                UnitPriceExclTax = scUnitPriceExclTax.price,
                PriceInclTax = scSubTotalInclTax.price,
                PriceExclTax = scSubTotalExclTax.price,
                OriginalProductCost = await _priceCalculationService.GetProductCostAsync(
                    product,
                    sc.AttributesXml
                ),
                AttributeDescription = attributeDescription,
                AttributesXml = sc.AttributesXml,
                Quantity = sc.Quantity,
                DiscountAmountInclTax = discountAmountInclTax.price,
                DiscountAmountExclTax = discountAmountExclTax.price,
                DownloadCount = 0,
                IsDownloadActivated = false,
                LicenseDownloadId = 0,
                ItemWeight = itemWeight,
                RentalStartDateUtc = sc.RentalStartDateUtc,
                RentalEndDateUtc = sc.RentalEndDateUtc,
            };

            await _orderService.InsertOrderItemAsync(orderItem);
        }

        var orderItems = await _orderService.GetOrderItemsAsync(order.Id);

        foreach (var orderItem in orderItems)
        {
            var product = products.FirstOrDefault(p => p.Id == orderItem.ProductId);
            if (product != null)
            {
                await AddGiftCardsAsync(
                    product,
                    orderItem.AttributesXml,
                    orderItem.Quantity,
                    orderItem,
                    orderItem.UnitPriceExclTax
                );
            }

            #region B2B

            if (
                erpAccount != null &&
                !isQouteOrder &&
                !(
                    erpNopUser != null &&
                    erpNopUser.ErpUserType == ErpUserType.B2CUser &&
                    order.PaymentStatus == PaymentStatus.Pending
                )
            )
            {
                if (erpAccount.PercentageOfStockAllowed < 100)
                {
                    var beforePurchasePercentageOfStockAllowed_a = decimal.Zero;
                    var productPricing = await _erpSpecialPriceService
                        .GetErpSpecialPricesByErpAccountIdAndNopProductIdAsync(
                            erpAccount.Id,
                            orderItem.ProductId
                        );

                    // rules:
                    // for b2b: try to take from specialPrice if time isn't expired
                    //          if not, then take from account
                    // for b2c: try to take existingB2BUserStockRestriction if it exists and if time isn't expired
                    //          if not, then take from specialPrice if time isn't expired
                    //          if not, then take from account
                    if (productPricing != null && productPricing.Id > 0)
                    {
                        var percentageOfAllocatedStockResetTimeUtc = productPricing.PercentageOfAllocatedStockResetTimeUtc;
                        var percentageOfAllocatedStock = productPricing.PercentageOfAllocatedStock;

                        B2CUserStockRestriction existingB2CUserStockRestriction = null;

                        // For B2C user, try to get percentageOfAllocatedStockResetTimeUtc from B2CUserRestriction table
                        if (erpNopUser != null && erpNopUser.ErpUserType == ErpUserType.B2CUser)
                        {
                            existingB2CUserStockRestriction = await _b2CUserStockRestrictionService
                                .GetB2CUserStockRestrictionByUserIdProductIdAsync(
                                    erpNopUser.Id,
                                    orderItem.ProductId
                                );

                            if (existingB2CUserStockRestriction != null &&
                                existingB2CUserStockRestriction.PercentageOfAllocatedStockResetTimeUtc.HasValue &&
                                !(existingB2CUserStockRestriction.PercentageOfAllocatedStockResetTimeUtc.Value < DateTime.UtcNow))
                            {
                                percentageOfAllocatedStockResetTimeUtc = existingB2CUserStockRestriction.PercentageOfAllocatedStockResetTimeUtc;
                                percentageOfAllocatedStock = existingB2CUserStockRestriction.NewPercentageOfAllocatedStock;
                            }
                        }

                        if (
                            !percentageOfAllocatedStockResetTimeUtc.HasValue ||
                            percentageOfAllocatedStockResetTimeUtc.Value < DateTime.UtcNow
                        )
                        {
                            // if PercentageOfAllocatedStockResetTimeUtc doesn't have any value
                            // or PercentageOfAllocatedStockResetTimeUtc has passed already
                            // then take value from account
                            beforePurchasePercentageOfStockAllowed_a = erpAccount.PercentageOfStockAllowed ?? 0;
                        }
                        else
                        {
                            beforePurchasePercentageOfStockAllowed_a = percentageOfAllocatedStock;
                        }

                        var beforePurchaseAllowedQuantity_c = await _productService.GetTotalStockQuantityAsync(product);

                        if (
                            beforePurchaseAllowedQuantity_c > 0 &&
                            beforePurchasePercentageOfStockAllowed_a > 0
                        )
                        {
                            var usedByPurchase_f = (decimal)orderItem.Quantity / (decimal)beforePurchaseAllowedQuantity_c;
                            var beforePurchaseStockAllowed_a = beforePurchasePercentageOfStockAllowed_a / 100m;
                            var afterPurchasePercentageOfStockAllowed = (
                                beforePurchaseStockAllowed_a - (usedByPurchase_f * beforePurchaseStockAllowed_a)
                            ) * 100m;


                            if (
                                !percentageOfAllocatedStockResetTimeUtc.HasValue ||
                                percentageOfAllocatedStockResetTimeUtc.Value < DateTime.UtcNow
                            )
                            {
                                var exactResetTime = DateTime.UtcNow.AddDays(
                                    _b2BB2CFeaturesSettings.PercentageOfAllocatedStockResetTimeUtc - 1
                                );

                                percentageOfAllocatedStockResetTimeUtc = new DateTime(
                                    exactResetTime.Year,
                                    exactResetTime.Month,
                                    exactResetTime.Day,
                                    23, 59, 59
                                );
                            }

                            if (erpNopUser != null && erpNopUser.ErpUserType == ErpUserType.B2BUser)
                            {
                                productPricing.PercentageOfAllocatedStock = afterPurchasePercentageOfStockAllowed;
                                productPricing.PercentageOfAllocatedStockResetTimeUtc = percentageOfAllocatedStockResetTimeUtc;
                                await _erpSpecialPriceService.UpdateErpSpecialPriceAsync(productPricing);
                            }
                            else if (erpNopUser != null && erpNopUser.ErpUserType == ErpUserType.B2CUser)
                            {
                                // For B2C user, insert or update stock restriction
                                if (existingB2CUserStockRestriction == null)
                                {
                                    await _b2CUserStockRestrictionService.InsertB2CUserStockRestrictionAsync(
                                        new B2CUserStockRestriction
                                        {
                                            B2CUserId = erpNopUser.Id,
                                            ProductId = orderItem.ProductId,
                                            NewPercentageOfAllocatedStock = afterPurchasePercentageOfStockAllowed,
                                            PercentageOfAllocatedStockResetTimeUtc = percentageOfAllocatedStockResetTimeUtc
                                        }
                                    );
                                }
                                else
                                {
                                    existingB2CUserStockRestriction.NewPercentageOfAllocatedStock = afterPurchasePercentageOfStockAllowed;
                                    existingB2CUserStockRestriction.PercentageOfAllocatedStockResetTimeUtc = percentageOfAllocatedStockResetTimeUtc;

                                    await _b2CUserStockRestrictionService.UpdateB2CUserStockRestrictionAsync(
                                        existingB2CUserStockRestriction
                                    );
                                }
                            }
                        }
                    }
                }

                await _productService.AdjustInventoryAsync(
                    product,
                    -orderItem.Quantity,
                    orderItem.AttributesXml,
                    string.Format(
                        await _localizationService.GetResourceAsync(
                            "Admin.StockQuantityHistory.Messages.PlaceOrder"
                        ),
                        order.Id
                    )
                );
            }

            #endregion B2B

            #region B2C

            if (
                erpAccount != null
                && erpNopUser != null
                && erpOrderAdditionalData != null
                && erpNopUser.ErpUserType == ErpUserType.B2CUser
            )
            {
                orderItemCount++;
                var sc = cart.Find(sci => sci.ProductId == orderItem.ProductId);
                if (sc != null)
                {
                    var itemDeliveryDate = await MoveErpShoppingCartItemToErpOrderItemAsync(
                        sc.Id,
                        sc.ProductId,
                        orderItem.Id,
                        orderItemCount,
                        erpOrderAdditionalData
                    );
                    if (itemDeliveryDate.HasValue)
                    {
                        latestDeliveryDate =
                        itemDeliveryDate > latestDeliveryDate
                            ? itemDeliveryDate.Value
                            : latestDeliveryDate;
                    }
                }
                else
                    await _erpLogsService.ErrorAsync(
                        $"B2C MoveShoppingCartItemsToOrderItems: cart item or product not found for customer {(await _workContext.GetCurrentCustomerAsync()).Email}", ErpSyncLevel.Order
                    );
            }

            #endregion B2C
        }

        if (
            erpAccount != null
            && erpNopUser != null
            && erpOrderAdditionalData != null
            && erpNopUser.ErpUserType == ErpUserType.B2CUser
        )
        {
            erpOrderAdditionalData.DeliveryDate = latestDeliveryDate;
            await _erpOrderAdditionalDataService.UpdateErpOrderAdditionalDataAsync(
                erpOrderAdditionalData
            );
        }

        details.Customer.HasShoppingCartItems = (
            await _shoppingCartService.GetShoppingCartAsync(
                details.Customer,
                ShoppingCartType.ShoppingCart
            )
        ).Any();

        await _customerService.UpdateCustomerAsync(details.Customer);
        await _shoppingCartService.ClearShoppingCartAsync(details.Customer, order.StoreId);
    }

    private async Task<DateTime?> MoveErpShoppingCartItemToErpOrderItemAsync(
        int sciId,
        int productId,
        int nopOrderItemId,
        int orderItemCount,
        ErpOrderAdditionalData erpOrderAdditionalData
    )
    {
        var b2CSci =
            await _b2CShoppingCartItemService.GetB2CShoppingCartItemByNopShoppingCartItemIdAsync(
                sciId
            );

        if (b2CSci != null)
        {
            var uom =
                await _erpSpecificationAttributeService.GetProductUOMByProductIdAndSpecificationAttributeId(
                    productId,
                    _b2BB2CFeaturesSettings.UnitOfMeasureSpecificationAttributeId
                ) ?? string.Empty;

            var b2COrderItem = new ErpOrderItemAdditionalData
            {
                DeliveryDate = b2CSci.DeliveryDate,
                ErpDateExpected = b2CSci.DeliveryDate,
                ErpDateRequired = b2CSci.DeliveryDate,
                ErpOrderLineNumber = $"{(orderItemCount * 10): 000000}",
                ErpSalesUoM = uom,
                NopOrderItemId = nopOrderItemId,
                ErpOrderId = erpOrderAdditionalData.Id,
                SpecialInstruction = b2CSci.SpecialInstructions,
                WareHouse = b2CSci.WarehouseCode,
                NopWarehouseId = b2CSci.NopWarehouseId,
            };

            await _erpOrderItemAdditionalDataService.InsertErpOrderItemAdditionalDataAsync(
                b2COrderItem
            );
        }

        return b2CSci.DeliveryDate;
    }

    #endregion
}