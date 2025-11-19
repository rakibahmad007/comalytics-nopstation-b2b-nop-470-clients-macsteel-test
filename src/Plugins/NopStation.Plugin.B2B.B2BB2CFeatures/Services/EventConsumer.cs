using System;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Core.Caching;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Messages;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Payments;
using Nop.Core.Domain.Tax;
using Nop.Core.Events;
using Nop.Services.Catalog;
using Nop.Services.Customers;
using Nop.Services.Events;
using Nop.Services.Localization;
using Nop.Services.Messages;
using Nop.Services.Orders;
using NopStation.Plugin.B2B.B2BB2CFeatures.Services.ErpCustomerFunctionality;
using NopStation.Plugin.B2B.B2BB2CFeatures.Services.ErpOrderTotalCalculationService;
using NopStation.Plugin.B2B.B2BB2CFeatures.Services.ErpShoppingCartItemService;
using NopStation.Plugin.B2B.B2BB2CFeatures.Services.Overriden;
using NopStation.Plugin.B2B.ERPIntegrationCore;
using NopStation.Plugin.B2B.ERPIntegrationCore.Domain;
using NopStation.Plugin.B2B.ERPIntegrationCore.Enums;
using NopStation.Plugin.B2B.ERPIntegrationCore.Services;

namespace NopStation.Plugin.B2B.B2BB2CFeatures.Services;

/// <summary>
/// Represents event consumer
/// </summary>
public class EventConsumer :
    IConsumer<EntityDeletedEvent<Order>>,
    IConsumer<OrderPaidEvent>,
    IConsumer<OrderPlacedEvent>,
    IConsumer<OrderStatusChangedEvent>,
    IConsumer<EntityTokensAddedEvent<Customer, Token>>,
    IConsumer<EntityTokensAddedEvent<Order, Token>>,
    IConsumer<EntityInsertedEvent<ErpNopUser>>,
    IConsumer<EntityDeletedEvent<ShoppingCartItem>>, 
    IConsumer<EntityInsertedEvent<ShoppingCartItem>>
{
    #region Fields

    private readonly IErpCustomerFunctionalityService _erpCustomerFunctionalityService;
    private readonly IErpOrderAdditionalDataService _erpOrderAdditionalDataService;
    private readonly IOverriddenOrderProcessingService _overriddenOrderProcessingService;
    private readonly IStaticCacheManager _staticCacheManager;
    private readonly IOrderService _orderService;
    private readonly IProductService _productService;
    private readonly IErpProductService _erpProductService;
    private readonly IErpSpecialPriceService _erpSpecialPriceService;
    private readonly ILocalizationService _localizationService;
    private readonly IErpLogsService _erpLogsService;
    private readonly IB2CUserStockRestrictionService _b2CUserStockRestrictionService;
    private readonly IErpOrderTotalCalculationService _erpOrderTotalCalculationService;
    private readonly B2BB2CFeaturesSettings _b2BB2CFeaturesSettings;
    private readonly IWorkContext _workContext;
    private readonly TaxSettings _taxSettings;
    private readonly IShoppingCartService _shoppingCartService;
    private readonly IOrderTotalCalculationService _orderTotalCalculationService;
    private readonly IErpShoppingCartItemService _erpShoppingCartItemService;
    private readonly ICustomerService _customerService;
    private readonly IErpAccountService _erpAccountService;
    private readonly IErpNopUserService _erpNopUserService;

    #endregion Fields

    #region Ctor

    public EventConsumer(IErpCustomerFunctionalityService erpCustomerFunctionalityService,
        IErpOrderAdditionalDataService erpOrderAdditionalDataService,
        IOverriddenOrderProcessingService overriddenOrderProcessingService,
        IErpShoppingCartItemService erpShoppingCartItemService,
        ICustomerService customerService,
        IWorkContext workContext,
        TaxSettings taxSettings,
        IShoppingCartService shoppingCartService,
        IOrderTotalCalculationService orderTotalCalculationService,
        IErpAccountService erpAccountService,
        IErpNopUserService erpNopUserService,
        IStaticCacheManager staticCacheManager,
        IOrderService orderService,
        IProductService productService,
        IErpProductService erpProductService,
        IErpSpecialPriceService erpSpecialPriceService,
        ILocalizationService localizationService,
        IErpLogsService erpLogsService,
        IB2CUserStockRestrictionService b2CUserStockRestrictionService,
        IErpOrderTotalCalculationService erpOrderTotalCalculationService,
        B2BB2CFeaturesSettings b2BB2CFeaturesSettings)
    {
        _erpCustomerFunctionalityService = erpCustomerFunctionalityService;
        _erpOrderAdditionalDataService = erpOrderAdditionalDataService;
        _overriddenOrderProcessingService = overriddenOrderProcessingService;
        _customerService = customerService;
        _workContext = workContext;
        _taxSettings = taxSettings;
        _shoppingCartService = shoppingCartService;
        _orderTotalCalculationService = orderTotalCalculationService;
        _erpAccountService = erpAccountService;
        _erpNopUserService = erpNopUserService;
        _staticCacheManager = staticCacheManager;
        _orderService = orderService;
        _productService = productService;
        _erpProductService = erpProductService;
        _erpSpecialPriceService = erpSpecialPriceService;
        _localizationService = localizationService;
        _erpLogsService = erpLogsService;
        _b2CUserStockRestrictionService = b2CUserStockRestrictionService;
        _erpOrderTotalCalculationService = erpOrderTotalCalculationService;
        _b2BB2CFeaturesSettings = b2BB2CFeaturesSettings;
        _erpShoppingCartItemService = erpShoppingCartItemService;
        _customerService = customerService;
    }

    #endregion Ctor

    #region Methods

    public async Task HandleEventAsync(OrderPaidEvent eventMessage)
    {
        //handle event
        if (eventMessage.Order == null)
            return;

        var erpOrderAdditionalData = await _erpOrderAdditionalDataService.GetErpOrderAdditionalDataByNopOrderIdAsync(eventMessage.Order.Id);

        // if order per account is not null && IntegrationStatusType is WaitingForPayment the we will send from here
        if (erpOrderAdditionalData != null)
        {
            if (erpOrderAdditionalData.IntegrationStatusType == IntegrationStatusType.WaitingForPayment)
            {
                erpOrderAdditionalData.IntegrationStatusTypeId = (int)IntegrationStatusType.Queued;
                await _erpOrderAdditionalDataService.UpdateErpOrderAdditionalDataAsync(erpOrderAdditionalData);
            }

            if (erpOrderAdditionalData.IntegrationStatusType == IntegrationStatusType.Queued)
            {
                await _overriddenOrderProcessingService.RetryPlaceErpOrderAtErpAsync(erpOrderAdditionalData);

                #region Apply 48Hrs stock restriction for B2C sales order, which has pending payment before

                var customer = await _customerService.GetCustomerByIdAsync(eventMessage.Order.CustomerId);
                var erpNopUser = await _erpNopUserService.GetErpNopUserByCustomerIdAsync(customer?.Id ?? 0);
                if (erpNopUser != null && erpNopUser.ErpUserType == ErpUserType.B2CUser)
                {
                    try
                    {
                        var order = await _orderService.GetOrderByIdAsync(erpOrderAdditionalData.NopOrderId);
                        var isB2CSales = erpOrderAdditionalData.ErpOrderType == ErpOrderType.B2CSalesOrder;
                        var b2BAccount = await _erpCustomerFunctionalityService.GetActiveErpAccountByCustomerAsync(customer);

                        var orderItems = await _orderService.GetOrderItemsAsync(order.Id);
                        if (b2BAccount != null && isB2CSales && order.PaymentStatus == PaymentStatus.Paid)
                        {
                            foreach (var orderItem in orderItems)
                            {
                                var product = await _erpProductService.GetProductByIdAsync(orderItem.ProductId);

                                if (b2BAccount.PercentageOfStockAllowed < 100)
                                {
                                    var beforePurchasePercentageOfStockAllowed_a = decimal.Zero;
                                    var productPricing = await _erpSpecialPriceService.GetErpSpecialPricesByErpAccountIdAndNopProductIdAsync(
                                                b2BAccount.Id,
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

                                        // For B2C user, try to get percentageOfAllocatedStockResetTimeUtc from B2CUserRestriction table
                                        var existingB2CUserStockRestriction = await _b2CUserStockRestrictionService
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

                                        if (
                                            !percentageOfAllocatedStockResetTimeUtc.HasValue ||
                                            percentageOfAllocatedStockResetTimeUtc.Value < DateTime.UtcNow
                                        )
                                        {
                                            // if PercentageOfAllocatedStockResetTimeUtc doesn't have any value
                                            // or PercentageOfAllocatedStockResetTimeUtc has passed already
                                            // then take value from account
                                            beforePurchasePercentageOfStockAllowed_a = b2BAccount.PercentageOfStockAllowed ?? 0;
                                        }
                                        else
                                        {
                                            beforePurchasePercentageOfStockAllowed_a = percentageOfAllocatedStock;
                                        }

                                        var beforePurchaseAllowedQuantity_c = await _erpOrderTotalCalculationService.GetTotalStockQuantityForAdminOPEventAsync(product, b2BAccount, erpNopUser);

                                        if (beforePurchaseAllowedQuantity_c > 0 && beforePurchasePercentageOfStockAllowed_a > 0)
                                        {
                                            var usedByPurchase_f = (decimal)orderItem.Quantity / (decimal)beforePurchaseAllowedQuantity_c;
                                            var beforePurchaseStockAllowed_a = beforePurchasePercentageOfStockAllowed_a / 100m;
                                            var afterPurchasePercentageOfStockAllowed = (beforePurchaseStockAllowed_a - (usedByPurchase_f * beforePurchaseStockAllowed_a)) * 100m;

                                            // save new percentage and reset time

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

                                await _productService.AdjustInventoryAsync(product, -orderItem.Quantity, orderItem.AttributesXml,
                                    string.Format(await _localizationService.GetResourceAsync("Admin.StockQuantityHistory.Messages.PlaceOrder"),
                                    order.Id));
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        await _erpLogsService.ErrorAsync(ex.Message, ErpSyncLevel.Order, ex);
                    }
                }

                #endregion Apply 48Hrs stock restriction for B2C sales order, which has pending payment before
            }
        }
    }

    public async Task HandleEventAsync(OrderPlacedEvent eventMessage)
    {
        //handle event
        await _erpCustomerFunctionalityService.ClearCurrentCustomerYearlySavingsCacheAsync(eventMessage.Order.CustomerId);
        await _erpCustomerFunctionalityService.ClearCurrentCustomerAllTimeSavingsCacheAsync(eventMessage.Order.CustomerId);
    }

    public async Task HandleEventAsync(OrderStatusChangedEvent eventMessage)
    {
        var order = eventMessage.Order;

        var erpOrder = await _erpOrderAdditionalDataService.GetErpOrderAdditionalDataByNopOrderIdAsync(order.Id);

        if (order.OrderStatusId == (int)OrderStatus.Cancelled && erpOrder != null)
        {
            erpOrder.IntegrationStatusTypeId = (int)IntegrationStatusType.Cancelled;
            await _erpOrderAdditionalDataService.UpdateErpOrderAdditionalDataAsync(erpOrder);
        }
    }

    public async Task HandleEventAsync(EntityDeletedEvent<Order> eventMessage)
    {
        if (eventMessage.Entity == null)
            return;

        var order = eventMessage.Entity;

        var erpOrder = await _erpOrderAdditionalDataService.GetErpOrderAdditionalDataByNopOrderIdAsync(order.Id);

        if (erpOrder != null)
            await _erpOrderAdditionalDataService.DeleteErpOrderAdditionalDataByIdAsync(erpOrder.Id);
    }

    public async Task HandleEventAsync(EntityTokensAddedEvent<Customer, Token> eventMessage)
    {
        var customer = eventMessage.Entity;
        var customerCompany = customer.Company;
        eventMessage.Tokens.Add(new Token("Customer.Company", customerCompany));
    }

    public async Task HandleEventAsync(EntityTokensAddedEvent<Order, Token> eventMessage)
    {
        var order = eventMessage.Entity;

        if (order is null)
        {
            return;
        }

        var customer = await _customerService.GetCustomerByIdAsync(order.CustomerId);
        var erpNopUser = await _erpCustomerFunctionalityService.GetActiveErpNopUserByCustomerAsync(customer);

        if (erpNopUser is not null)
        {
            var erpOrderAdditionalData = await _erpOrderAdditionalDataService.GetErpOrderAdditionalDataByNopOrderIdAsync(order.Id);

            if (erpOrderAdditionalData is null || string.IsNullOrEmpty(erpOrderAdditionalData.ErpOrderNumber))
            {
                eventMessage.Tokens.Add(new Token("ErpOrderAdditionalData.ErpOrderNumber", "(Null order)"));
                eventMessage.Tokens.Add(new Token("ErpOrderAdditionalData.NopOrderNumber", "(Null)"));
                eventMessage.Tokens.Add(new Token("ErpOrderAdditionalData.ErpOrderOriginType", "(Null)"));
                eventMessage.Tokens.Add(new Token("ErpOrderAdditionalData.ErpOrderType", "(Null)"));
                eventMessage.Tokens.Add(new Token("ErpOrderAdditionalData.SpecialInstructions", "(Null)"));
                eventMessage.Tokens.Add(new Token("ErpOrderAdditionalData.CustomerReference", "(Null)"));
                eventMessage.Tokens.Add(new Token("ErpOrderAdditionalData.ErpOrderStatus", "(Null)"));
                eventMessage.Tokens.Add(new Token("ErpOrderAdditionalData.DeliveryDate", "(Null)"));
                eventMessage.Tokens.Add(new Token("ErpOrderAdditionalData.IntegrationStatusType", "(Null)"));
                eventMessage.Tokens.Add(new Token("ErpOrderAdditionalData.IntegrationError", "(Null)"));
                eventMessage.Tokens.Add(new Token("ErpOrderAdditionalData.LastErpUpdateUtc", "(Null)"));
                eventMessage.Tokens.Add(new Token("ErpOrderAdditionalData.ErpOrderPlaceByCustomerType", "(Null)"));
                eventMessage.Tokens.Add(new Token("ErpOrderAdditionalData.ChangedOnUtc", "(Null)"));
                eventMessage.Tokens.Add(new Token("ErpOrderAdditionalData.ChangedById", "(Null)"));

                return;
            }

            eventMessage.Tokens.Add(new Token("ErpOrderAdditionalData.ErpOrderNumber", string.IsNullOrEmpty(erpOrderAdditionalData.ErpOrderNumber) ? order.CustomOrderNumber : erpOrderAdditionalData.ErpOrderNumber));
            eventMessage.Tokens.Add(new Token("ErpOrderAdditionalData.NopOrderNumber", !string.IsNullOrEmpty(order.CustomOrderNumber) ? order.CustomOrderNumber : erpOrderAdditionalData.NopOrderId.ToString()));
            eventMessage.Tokens.Add(new Token("ErpOrderAdditionalData.ErpOrderOriginType", erpOrderAdditionalData.ErpOrderOriginType.ToString()));
            eventMessage.Tokens.Add(new Token("ErpOrderAdditionalData.ErpOrderType", erpOrderAdditionalData.ErpOrderType.ToString()));
            eventMessage.Tokens.Add(new Token("ErpOrderAdditionalData.SpecialInstructions", erpOrderAdditionalData.SpecialInstructions));
            eventMessage.Tokens.Add(new Token("ErpOrderAdditionalData.CustomerReference", erpOrderAdditionalData.CustomerReference));
            eventMessage.Tokens.Add(new Token("ErpOrderAdditionalData.ErpOrderStatus", erpOrderAdditionalData.ERPOrderStatus));
            eventMessage.Tokens.Add(new Token("ErpOrderAdditionalData.DeliveryDate", erpOrderAdditionalData.DeliveryDate.HasValue ? erpOrderAdditionalData.DeliveryDate.Value.ToString("d") : ""));
            eventMessage.Tokens.Add(new Token("ErpOrderAdditionalData.IntegrationStatusType", erpOrderAdditionalData.IntegrationStatusType.ToString()));
            eventMessage.Tokens.Add(new Token("ErpOrderAdditionalData.IntegrationError", erpOrderAdditionalData.IntegrationError));
            eventMessage.Tokens.Add(new Token("ErpOrderAdditionalData.LastErpUpdateUtc", erpOrderAdditionalData.LastERPUpdateUtc.HasValue ? erpOrderAdditionalData.LastERPUpdateUtc.Value.ToString("d") : ""));
            eventMessage.Tokens.Add(new Token("ErpOrderAdditionalData.ErpOrderPlaceByCustomerType", Enum.GetName(typeof(ErpUserType), erpOrderAdditionalData.ErpOrderPlaceByCustomerTypeId)));
            eventMessage.Tokens.Add(new Token("ErpOrderAdditionalData.ChangedOnUtc", erpOrderAdditionalData.ChangedOnUtc.HasValue ? erpOrderAdditionalData.ChangedOnUtc.Value.ToString("d") : ""));
            eventMessage.Tokens.Add(new Token("ErpOrderAdditionalData.ChangedById", erpOrderAdditionalData.ChangedById.ToString()));
        }
    }

    public async Task HandleEventAsync(EntityInsertedEvent<ErpNopUser> eventMessage)
    {
        await _staticCacheManager.RemoveAsync(_staticCacheManager.PrepareKeyForDefaultCache(ERPIntegrationCoreDefaults.ErpNopUserByCustomerCacheKey,
            eventMessage.Entity.NopCustomerId));
    }

    public async Task HandleEventAsync(EntityDeletedEvent<ShoppingCartItem> eventMessage)
    {
        if (eventMessage.Entity == null)
            return;

        await _erpShoppingCartItemService.ShoppingCartEventCheckerAsync();

        var customer = await _customerService.GetCustomerByIdAsync(eventMessage.Entity.CustomerId);
        var nopUser = await _erpCustomerFunctionalityService.GetActiveErpNopUserByCustomerAsync(customer);

        if (nopUser != null && nopUser.ErpUserType == ErpUserType.B2CUser)
        {
            await _erpShoppingCartItemService.RemoveItemFromB2CShoppingCartByNopSciId(eventMessage.Entity.Id);
        }
    }

    public async Task HandleEventAsync(EntityInsertedEvent<ShoppingCartItem> eventMessage)
    {
        if (eventMessage.Entity == null)
            return;

        var customer = await _customerService.GetCustomerByIdAsync(eventMessage.Entity.CustomerId);
        var nopUser = await _erpCustomerFunctionalityService.GetActiveErpNopUserByCustomerAsync(customer);

        if (nopUser != null && nopUser.ErpUserType == ErpUserType.B2CUser)
        {
            await _erpShoppingCartItemService.AddB2CShoppingCartItem(eventMessage.Entity.Id);
        }
    }

    #endregion
}