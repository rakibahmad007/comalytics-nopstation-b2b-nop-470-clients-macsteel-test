using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Payments;
using Nop.Core.Domain.Security;
using Nop.Core.Domain.Shipping;
using Nop.Core.Domain.Tax;
using Nop.Core.Http.Extensions;
using Nop.Services.Attributes;
using Nop.Services.Catalog;
using Nop.Services.Common;
using Nop.Services.Customers;
using Nop.Services.Directory;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Messages;
using Nop.Services.Orders;
using Nop.Services.Payments;
using Nop.Services.Security;
using Nop.Services.Shipping;
using Nop.Services.Tax;
using Nop.Web.Components;
using Nop.Web.Controllers;
using Nop.Web.Factories;
using Nop.Web.Framework;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Mvc.Filters;
using Nop.Web.Models.Checkout;
using NopStation.Plugin.B2B.B2BB2CFeatures.Factories;
using NopStation.Plugin.B2B.B2BB2CFeatures.Model.Checkout;
using NopStation.Plugin.B2B.B2BB2CFeatures.Services.ErpAccountCreditSyncFunctionality;
using NopStation.Plugin.B2B.B2BB2CFeatures.Services.ErpCustomerFunctionality;
using NopStation.Plugin.B2B.B2BB2CFeatures.Services.ErpShippingService;
using NopStation.Plugin.B2B.B2BB2CFeatures.Services.Overriden;
using NopStation.Plugin.B2B.B2BB2CFeatures.Services.SoltrackIntegration;
using NopStation.Plugin.B2B.ERPIntegrationCore.Domain;
using NopStation.Plugin.B2B.ERPIntegrationCore.Enums;
using NopStation.Plugin.B2B.ERPIntegrationCore.Infrastructure;
using NopStation.Plugin.B2B.ERPIntegrationCore.Services;

namespace NopStation.Plugin.B2B.B2BB2CFeatures.Controllers;

[AutoValidateAntiforgeryToken]
public class ErpCheckoutController : CheckoutController
{
    #region Fields

    private readonly IPermissionService _permissionService;
    private readonly IErpCustomerFunctionalityService _erpCustomerFunctionalityService;
    private readonly IErpLogsService _erpLogsService;
    private readonly IErpOrderAdditionalDataService _erpOrderAdditionalDataService;
    private readonly IOrderModelFactory _orderModelFactory;
    private readonly IErpShipToAddressService _erpShipToAddressService;
    private readonly IErpCheckoutModelFactory _erpCheckoutModelFactory;
    private readonly IErpOrderItemModelFactory _erpOrderItemModelFactory;
    private readonly IErpNopUserService _erpNopUserService;
    private readonly IOverriddenOrderProcessingService _overriddenOrderProcessingService;
    private readonly INotificationService _notificationService;
    private readonly IErpAccountService _erpAccountService;
    private readonly ICustomerActivityService _customerActivityService;
    private readonly B2BB2CFeaturesSettings _b2BB2CFeaturesSettings;
    private readonly IErpShippingService _erpShippingService;
    private readonly IB2CShoppingCartItemService _b2CShoppingCartItemService;
    private readonly ISoltrackIntegrationService _soltrackIntegrationService;
    private readonly IErpAccountCreditSyncFunctionality _erpAccountCreditSyncFunctionality;
    private readonly IOrderTotalCalculationService _orderTotalCalculationService;
    private readonly ICurrencyService _currencyService;
    private readonly IErpSalesOrgService _erpSalesOrgService;
    private static readonly string[] _separator = ["___"];

    #endregion

    #region Ctor

    public ErpCheckoutController(
        AddressSettings addressSettings,
        CaptchaSettings captchaSettings,
        CustomerSettings customerSettings,
        IAttributeParser<AddressAttribute, AddressAttributeValue> addressAttributeParser,
        IAddressModelFactory addressModelFactory,
        IAddressService addressService,
        ICheckoutModelFactory checkoutModelFactory,
        ICountryService countryService,
        ICustomerService customerService,
        IGenericAttributeService genericAttributeService,
        ILocalizationService localizationService,
        ILogger logger,
        IOrderProcessingService orderProcessingService,
        IOrderService orderService,
        IPaymentPluginManager paymentPluginManager,
        IPaymentService paymentService,
        IProductService productService,
        IShippingService shippingService,
        IShoppingCartService shoppingCartService,
        IStoreContext storeContext,
        ITaxService taxService,
        IWebHelper webHelper,
        IWorkContext workContext,
        OrderSettings orderSettings,
        PaymentSettings paymentSettings,
        RewardPointsSettings rewardPointsSettings,
        ShippingSettings shippingSettings,
        TaxSettings taxSettings,
        IPermissionService permissionService,
        IOverriddenOrderProcessingService overriddenOrderProcessingService,
        INotificationService notificationService,
        IErpAccountService erpAccountService,
        IErpCustomerFunctionalityService erpCustomerFunctionalityService,
        IErpLogsService erpLogsService,
        IErpOrderAdditionalDataService erpOrderAdditionalDataService,
        IOrderModelFactory orderModelFactory,
        IErpShipToAddressService erpShipToAddressService,
        IErpCheckoutModelFactory erpCheckoutModelFactory,
        IErpOrderItemModelFactory erpOrderItemModelFactory,
        IErpNopUserService erpNopUserService,
        B2BB2CFeaturesSettings b2BB2CFeaturesSettings,
        ICustomerActivityService customerActivityService,
        IB2CShoppingCartItemService b2CShoppingCartItemService,
        IErpShippingService erpShippingService,
        ISoltrackIntegrationService soltrackIntegrationService,
        IErpAccountCreditSyncFunctionality erpAccountCreditSyncFunctionality,
        IOrderTotalCalculationService orderTotalCalculationService,
        ICurrencyService currencyService,
        IErpSalesOrgService erpSalesOrgService) : base(addressSettings,
            captchaSettings,
            customerSettings,
            addressModelFactory,
            addressService,
            addressAttributeParser,
            checkoutModelFactory,
            countryService,
            customerService,
            genericAttributeService,
            localizationService,
            logger,
            orderProcessingService,
            orderService,
            paymentPluginManager,
            paymentService,
            productService,
            shippingService,
            shoppingCartService,
            storeContext,
            taxService,
            webHelper,
            workContext,
            orderSettings,
            paymentSettings,
            rewardPointsSettings,
            shippingSettings,
            taxSettings
        )
    {
        _permissionService = permissionService;
        _overriddenOrderProcessingService = overriddenOrderProcessingService;
        _notificationService = notificationService;
        _erpAccountService = erpAccountService;
        _erpCustomerFunctionalityService = erpCustomerFunctionalityService;
        _erpLogsService = erpLogsService;
        _erpOrderAdditionalDataService = erpOrderAdditionalDataService;
        _orderModelFactory = orderModelFactory;
        _erpShipToAddressService = erpShipToAddressService;
        _erpCheckoutModelFactory = erpCheckoutModelFactory;
        _erpOrderItemModelFactory = erpOrderItemModelFactory;
        _erpNopUserService = erpNopUserService;
        _b2BB2CFeaturesSettings = b2BB2CFeaturesSettings;
        _customerActivityService = customerActivityService;
        _b2CShoppingCartItemService = b2CShoppingCartItemService;
        _erpShippingService = erpShippingService;
        _soltrackIntegrationService = soltrackIntegrationService;
        _erpAccountCreditSyncFunctionality = erpAccountCreditSyncFunctionality;
        _orderTotalCalculationService = orderTotalCalculationService;
        _currencyService = currencyService;
        _erpSalesOrgService = erpSalesOrgService;
    }

    #endregion

    #region Utilities

    private async Task ClearGenericAttributeForQuoteOrderAsync()
    {
        (var b2BAccount, var b2BUser, var b2CUser) =
            await GetB2BAccountAndUserOfCurrentCustomerAsync();
        var customer = await _workContext.GetCurrentCustomerAsync();
        var store = await _storeContext.GetCurrentStoreAsync();

        if (b2BAccount != null && b2BUser != null)
            await _genericAttributeService.SaveAttributeAsync(
                customer,
                B2BB2CFeaturesDefaults.B2BQouteOrderAttribute,
                false,
                store.Id
            );
        else if (b2BAccount != null && b2CUser != null)
            await _genericAttributeService.SaveAttributeAsync(
                customer,
                B2BB2CFeaturesDefaults.B2CQouteOrderAttribute,
                false,
                store.Id
            );
    }

    private async Task<bool> IsQuoteOrderAsync()
    {
        var currCustomer = await _workContext.GetCurrentCustomerAsync();
        var currStore = await _storeContext.GetCurrentStoreAsync();
        (var b2BAccount, var b2BUser, var b2CUser) =
            await GetB2BAccountAndUserOfCurrentCustomerAsync();
        var isQuoteOrder = false;

        if (b2BAccount != null && b2BUser != null)
            isQuoteOrder = await _genericAttributeService.GetAttributeAsync<bool>(
                currCustomer,
                B2BB2CFeaturesDefaults.B2BQouteOrderAttribute,
                currStore.Id
            );
        else if (b2BAccount != null && b2CUser != null)
            isQuoteOrder = await _genericAttributeService.GetAttributeAsync<bool>(
                currCustomer,
                B2BB2CFeaturesDefaults.B2CQouteOrderAttribute,
                currStore.Id
            );

        return isQuoteOrder;
    }

    private async Task<(ErpAccount b2BAccount, ErpNopUser b2BUser, ErpNopUser b2CUser)> GetB2BAccountAndUserOfCurrentCustomerAsync()
    {
        var currCustomer = await _workContext.GetCurrentCustomerAsync();
        var b2BAccount = await _erpAccountService.GetActiveErpAccountByCustomerIdAsync(
            currCustomer.Id
        );
        var erpUser = await _erpCustomerFunctionalityService.GetActiveErpNopUserByCustomerAsync(
            currCustomer
        );

        var b2BUser = erpUser?.ErpUserType == ErpUserType.B2BUser ? erpUser : null;
        var b2CUser = erpUser?.ErpUserType == ErpUserType.B2CUser ? erpUser : null;

        return (b2BAccount, b2BUser, b2CUser);
    }

    private async Task<(ErpAccount erpAccount, ErpNopUser erpNopUser)> GetErpAccountAndUserOfCurrentCustomerAsync(int customerId)
    {
        var erpNopUser = await _erpNopUserService.GetErpNopUserByCustomerIdAsync(customerId, showHidden: false);
        var erpAccount = await _erpAccountService.GetErpAccountByIdAsync(erpNopUser?.ErpAccountId ?? 0);

        return (erpAccount, erpNopUser);
    }

    private async Task<bool> IsUserPermittedToPlaceOrderOrQuote()
    {
        if (!await _permissionService.AuthorizeAsync(ErpPermissionProvider.PlaceB2BOrder) &&
            !await _permissionService.AuthorizeAsync(ErpPermissionProvider.PlaceB2BQuote))
            return false;
        return true;
    }

    private async Task<bool> IsUserValid(ErpAccount erpAccount)
    {
        if (erpAccount == null ||
            !await IsUserPermittedToPlaceOrderOrQuote() ||
            await _erpCustomerFunctionalityService.IsSalesOrderInvalidForCurrentCustomerAsync() ||
            _orderSettings.CheckoutDisabled)
            return false;

        return true;
    }

    #endregion

    #region Methods (Common)

    public override async Task<IActionResult> Index()
    {
        if (await _erpCustomerFunctionalityService.IsCurrentCustomerInB2BSalesRepRoleAsync())
            return RedirectToAction("List", "SalesRepUsers", new { area = AreaNames.ADMIN });

        if (await _erpCustomerFunctionalityService.IsCurrentCustomerInB2BQuoteAssistantRoleAsync())
            return RedirectToRoute("QuoteOrder");

        var customer = await _workContext.GetCurrentCustomerAsync();
        var b2BAccount = await _erpAccountService.GetActiveErpAccountByCustomerIdAsync(customer.Id);

        if (!await IsUserValid(b2BAccount))
            return RedirectToRoute("ShoppingCart");

        if (await _erpCustomerFunctionalityService.IsSalesOrderInvalidForCurrentCustomerAsync())
            return RedirectToRoute("ShoppingCart");

        await ClearGenericAttributeForQuoteOrderAsync();

        if (_orderSettings.CheckoutDisabled)
            return RedirectToRoute("ShoppingCart");

        var store = await _storeContext.GetCurrentStoreAsync();
        var cart = await _shoppingCartService.GetShoppingCartAsync(
            customer,
            ShoppingCartType.ShoppingCart,
            store.Id
        );

        if (!cart.Any())
            return RedirectToRoute("ShoppingCart");

        //await _erpAccountCreditSyncFunctionality.LiveErpAccountCreditCheckAsync(b2BAccount, customer);

        var cartProductIds = cart.Select(ci => ci.ProductId).ToArray();
        var downloadableProductsRequireRegistration =
            _customerSettings.RequireRegistrationForDownloadableProducts
            && await _productService.HasAnyDownloadableProductAsync(cartProductIds);

        if (await _customerService.IsGuestAsync(customer) &&
            (!_orderSettings.AnonymousCheckoutAllowed || downloadableProductsRequireRegistration))
            return Challenge();

        var paymentMethods = await
            (await _paymentPluginManager.LoadActivePluginsAsync(customer, store.Id))
            .WhereAwait(async pm => !await pm.HidePaymentMethodAsync(cart))
            .ToListAsync();

        var nonButtonPaymentMethods = paymentMethods
            .Where(pm => pm.PaymentMethodType != PaymentMethodType.Button)
            .ToList();

        var buttonPaymentMethods = paymentMethods
            .Where(pm => pm.PaymentMethodType == PaymentMethodType.Button)
            .ToList();
        if (nonButtonPaymentMethods.Count == 0 && buttonPaymentMethods.Count != 0)
            return RedirectToRoute("ShoppingCart");

        await _customerService.ResetCheckoutDataAsync(customer, store.Id);

        var checkoutAttributesXml = await _genericAttributeService.GetAttributeAsync<string>(
            customer,
            NopCustomerDefaults.CheckoutAttributes,
            store.Id
        );
        var scWarnings = await _shoppingCartService.GetShoppingCartWarningsAsync(
            cart,
            checkoutAttributesXml,
            true
        );
        if (scWarnings.Any())
            return RedirectToRoute("ShoppingCart");
        foreach (var sci in cart)
        {
            var product = await _productService.GetProductByIdAsync(sci.ProductId);

            var sciWarnings = await _shoppingCartService.GetShoppingCartItemWarningsAsync(
                customer,
                sci.ShoppingCartType,
                product,
                sci.StoreId,
                sci.AttributesXml,
                sci.CustomerEnteredPrice,
                sci.RentalStartDateUtc,
                sci.RentalEndDateUtc,
                sci.Quantity,
                false,
                sci.Id
            );
            if (sciWarnings.Any())
                return RedirectToRoute("ShoppingCart");
        }

        if (_orderSettings.OnePageCheckoutEnabled)
            return RedirectToRoute("CheckoutOnePage");

        return RedirectToRoute("CheckoutBillingAddress");
    }

    public async Task<IActionResult> IsCartItemsLivePriceSyncProcessing()
    {
        var isCartItemsLivePriceSyncProcessing = await _genericAttributeService.GetAttributeAsync<bool>
            (await _workContext.GetCurrentCustomerAsync(),
            B2BB2CFeaturesDefaults.CartItemsLivePriceSyncProcessing,
            (await _storeContext.GetCurrentStoreAsync()).Id);
        return Json(new
        {
            success = true,
            data = isCartItemsLivePriceSyncProcessing
        });
    }

    public override async Task<IActionResult> Completed(int? orderId)
    {
        var customer = await _workContext.GetCurrentCustomerAsync();
        if (await _customerService.IsGuestAsync(customer) && !_orderSettings.AnonymousCheckoutAllowed)
            return Challenge();

        var order = await _orderService.GetOrderByIdAsync(orderId ?? 0);

        if (order == null)
        {
            var store = await _storeContext.GetCurrentStoreAsync();
            order = (await _orderService.SearchOrdersAsync(storeId: store.Id,
                    customerId: customer.Id, pageSize: 1))
                .FirstOrDefault();
        }
        if (order == null || order.Deleted || customer.Id != order.CustomerId)
            return Challenge();


        #region B2B

        var currCustomer = await _workContext.GetCurrentCustomerAsync();
        (var erpAccount, var erpNopUser) = await GetErpAccountAndUserOfCurrentCustomerAsync(
            currCustomer.Id
        );

        var erpOrder =
                await _erpOrderAdditionalDataService.GetErpOrderAdditionalDataByNopOrderIdAsync(
                    order.Id
                );

        if (erpAccount != null && erpNopUser != null)
        {
            var erpOrderPerAccount = await _erpOrderAdditionalDataService.GetErpOrderAdditionalDataByNopOrderIdAsync(order.Id);
            if (erpOrderPerAccount == null || erpOrderPerAccount.ErpAccountId != erpAccount.Id)
                return Challenge();
        }

        #endregion

        if (_orderSettings.DisableOrderCompletedPage)
        {
            return RedirectToRoute("OrderDetails", new { orderId = order.Id });
        }

        var model = await _orderModelFactory.PrepareOrderDetailsModelAsync(order);
        return View("~/Plugins/NopStation.Plugin.B2B.B2BB2CFeatures/Views/Checkout/Completed.cshtml", model);
    }

    #endregion

    #region Methods (Multistep Checkout)

    public override async Task<IActionResult> BillingAddress(IFormCollection form)
    {
        var (b2BAccount, b2BUser, b2CUser) = await GetB2BAccountAndUserOfCurrentCustomerAsync();

        if (!await IsUserValid(b2BAccount))
            return RedirectToRoute("ShoppingCart");

        if (await _erpCustomerFunctionalityService.IsSalesOrderInvalidForCurrentCustomerAsync())
            return RedirectToRoute("ShoppingCart");

        if (_orderSettings.CheckoutDisabled)
            return RedirectToRoute("ShoppingCart");

        var customer = await _workContext.GetCurrentCustomerAsync();
        var store = await _storeContext.GetCurrentStoreAsync();
        var cart = await _shoppingCartService.GetShoppingCartAsync(
            customer,
            ShoppingCartType.ShoppingCart,
            store.Id
        );

        if (!cart.Any())
            return RedirectToRoute("ShoppingCart");

        if (_orderSettings.OnePageCheckoutEnabled)
            return RedirectToRoute("CheckoutOnePage");

        if (
            await _customerService.IsGuestAsync(customer)
            && !_orderSettings.AnonymousCheckoutAllowed
        )
            return Challenge();

        #region B2B

        if (b2BAccount != null)
        {
            if (_orderSettings.DisableBillingAddressCheckoutStep)
            {
                var currentCustomer = await _workContext.GetCurrentCustomerAsync();
                if (b2BUser != null && b2BAccount.BillingAddressId != null)
                {
                    var b2bBillingAddress = await _addressService.GetAddressByIdAsync(b2BAccount.BillingAddressId.Value);
                    var billingAddress = await _addressService.GetAddressByIdAsync(currentCustomer.BillingAddressId ?? 0);

                    if (b2bBillingAddress == null)
                    {
                        await _erpLogsService.ErrorAsync(
                            $"No valid billing address found during checkout for Erp Account {b2BAccount.AccountName} ({b2BAccount.AccountNumber}, Id: {b2BAccount.Id})" +
                            $"ErpAccount Billing Address Id: {b2BAccount.BillingAddressId.Value}, " +
                            $"CustomerId: {currentCustomer.Id}, Email: {currentCustomer.Email}",
                            ErpSyncLevel.ShipToAddress
                        );
                        _notificationService.ErrorNotification("Plugin.Misc.NopStation.B2BB2CFeaturesPlugins.ErpCheckout.BillingAddress.NotFound.");

                        return RedirectToRoute("ShoppingCart");
                    }

                    if (billingAddress == null || !await _addressService.IsAddressValidAsync(billingAddress))
                    {
                        var newBillingAddress = _addressService.CloneAddress(b2bBillingAddress);
                        newBillingAddress.FirstName = currentCustomer.FirstName;
                        newBillingAddress.LastName = currentCustomer.LastName;
                        newBillingAddress.Email = currentCustomer.Email;

                        await _addressService.InsertAddressAsync(newBillingAddress);
                        await _customerService.InsertCustomerAddressAsync(
                            customer,
                            newBillingAddress
                        );

                        currentCustomer.BillingAddressId = newBillingAddress.Id;
                        await _customerService.UpdateCustomerAsync(currentCustomer);
                    }
                    else
                    {
                        billingAddress.FirstName = currentCustomer.FirstName;
                        billingAddress.LastName = currentCustomer.LastName;
                        billingAddress.Email = currentCustomer.Email;
                        billingAddress.Company = b2bBillingAddress.Company;
                        billingAddress.CountryId = b2bBillingAddress.CountryId;
                        billingAddress.StateProvinceId = b2bBillingAddress.StateProvinceId;
                        billingAddress.County = b2bBillingAddress.County;
                        billingAddress.City = b2bBillingAddress.City;
                        billingAddress.Address1 = b2bBillingAddress.Address1;
                        billingAddress.Address2 = b2bBillingAddress.Address2;
                        billingAddress.ZipPostalCode = b2bBillingAddress.ZipPostalCode;
                        billingAddress.PhoneNumber = b2bBillingAddress.PhoneNumber;
                        billingAddress.FaxNumber = b2bBillingAddress.FaxNumber;
                        billingAddress.CustomAttributes = b2bBillingAddress.CustomAttributes;

                        await _addressService.UpdateAddressAsync(billingAddress);
                    }

                    return RedirectToRoute("CheckoutShippingAddress");
                }
                else
                {
                    var billingAddress = await _addressService.GetAddressByIdAsync(
                        currentCustomer.BillingAddressId ?? 0
                    );

                    if (b2CUser != null)
                    {
                        var b2CShipToAddress =
                            await _erpShipToAddressService.GetErpShipToAddressByIdWithActiveAsync(
                                b2CUser.ErpShipToAddressId
                            );
                        var address = await _addressService.GetAddressByIdAsync(
                            b2CShipToAddress != null
                                ? b2CShipToAddress.AddressId
                                : currentCustomer.ShippingAddressId ?? 0
                        );

                        if (address == null)
                        {
                            await _erpLogsService.ErrorAsync(
                                $"No valid shipping address found during checkout. " +
                                $"Tried Address Ids:" +
                                $"Erp Ship-To-Address Address Id: {(b2CShipToAddress?.AddressId.ToString() ?? "null")}, " +
                                $"Customer Shipping Address Id: {(currentCustomer.ShippingAddressId?.ToString() ?? "null")}. " +
                                $"Customer Id: {currentCustomer.Id}, Email: {currentCustomer.Email}",
                                ErpSyncLevel.ShipToAddress
                            );
                            _notificationService.ErrorNotification("Plugin.Misc.NopStation.B2BB2CFeaturesPlugins.ErpCheckout.BillingAddress.NotFound.");

                            return RedirectToRoute("ShoppingCart");
                        }

                        if (billingAddress == null || !await _addressService.IsAddressValidAsync(billingAddress))
                        {
                            var newBillingAddress = _addressService.CloneAddress(address);
                            newBillingAddress.FirstName = currentCustomer.FirstName;
                            newBillingAddress.LastName = currentCustomer.LastName;
                            newBillingAddress.Email = currentCustomer.Email;
                            newBillingAddress.Company = currentCustomer.Company;
                            newBillingAddress.PhoneNumber = currentCustomer.Phone;

                            newBillingAddress.CountryId = address.CountryId;
                            newBillingAddress.StateProvinceId = address.StateProvinceId;
                            newBillingAddress.County = address.County;
                            newBillingAddress.City = address.City;
                            newBillingAddress.Address1 = address.Address1;
                            newBillingAddress.Address2 = address.Address2;
                            newBillingAddress.ZipPostalCode = address.ZipPostalCode;
                            newBillingAddress.FaxNumber = address.FaxNumber;
                            newBillingAddress.CustomAttributes = address.CustomAttributes;

                            await _addressService.InsertAddressAsync(newBillingAddress);
                            await _customerService.InsertCustomerAddressAsync(
                                customer,
                                newBillingAddress
                            );

                            currentCustomer.BillingAddressId = newBillingAddress.Id;
                            await _customerService.UpdateCustomerAsync(currentCustomer);
                        }
                        else
                        {
                            billingAddress.FirstName = currentCustomer.FirstName;
                            billingAddress.LastName = currentCustomer.LastName;
                            billingAddress.Email = currentCustomer.Email;
                            billingAddress.Company = currentCustomer.Company;
                            billingAddress.PhoneNumber = currentCustomer.Phone;

                            billingAddress.CountryId = address.CountryId;
                            billingAddress.StateProvinceId = address.StateProvinceId;
                            billingAddress.County = address.County;
                            billingAddress.City = address.City;
                            billingAddress.Address1 = address.Address1;
                            billingAddress.Address2 = address.Address2;
                            billingAddress.ZipPostalCode = address.ZipPostalCode;
                            billingAddress.FaxNumber = address.FaxNumber;
                            billingAddress.CustomAttributes = address.CustomAttributes;

                            await _addressService.UpdateAddressAsync(billingAddress);
                        }

                        return RedirectToRoute("CheckoutShippingAddress");
                    }
                }
            }

            var b2BBillingAddressModel =
                await _erpCheckoutModelFactory.PrepareCheckoutErpBillingAddressModelAsync(
                    cart,
                    b2BAccount
                );

            return View(
                "~/Plugins/NopStation.Plugin.B2B.B2BB2CFeatures/Views/ErpCheckout/BillingAddress.cshtml",
                b2BBillingAddressModel
            );
        }

        #endregion

        var model = await _checkoutModelFactory.PrepareBillingAddressModelAsync(cart, prePopulateNewAddressWithCustomerFields: true);

        if (_orderSettings.DisableBillingAddressCheckoutStep && model.ExistingAddresses.Any())
        {
            if (model.ExistingAddresses.Any())
            {
                return await SelectBillingAddress(model.ExistingAddresses[0].Id);
            }

            TryValidateModel(model);
            TryValidateModel(model.BillingNewAddress);
            return await NewBillingAddress(model, form);
        }

        return View(model);
    }

    public override async Task<IActionResult> SelectBillingAddress(
        int addressId,
        bool shipToSameAddress = false
    )
    {
        var customer = await _workContext.GetCurrentCustomerAsync();
        var b2BAccount = await _erpAccountService.GetActiveErpAccountByCustomerIdAsync(customer.Id);

        if (!await IsUserValid(b2BAccount))
            return RedirectToRoute("ShoppingCart");

        var address = await _customerService.GetCustomerAddressAsync(customer.Id, addressId);

        if (address == null)
            return RedirectToRoute("CheckoutBillingAddress");

        customer.BillingAddressId = address.Id;
        await _customerService.UpdateCustomerAsync(customer);

        var store = await _storeContext.GetCurrentStoreAsync();
        var cart = await _shoppingCartService.GetShoppingCartAsync(
            customer,
            ShoppingCartType.ShoppingCart,
            store.Id
        );

        //ship to the same address?
        //by default Shipping is available if the country is not specified
        var shippingAllowed =
            !_addressSettings.CountryEnabled
            || ((await _countryService.GetCountryByAddressAsync(address))?.AllowsShipping ?? false);
        if (
            _shippingSettings.ShipToSameAddress
            && shipToSameAddress
            && await _shoppingCartService.ShoppingCartRequiresShippingAsync(cart)
            && shippingAllowed
        )
        {
            customer.ShippingAddressId = customer.BillingAddressId;
            await _customerService.UpdateCustomerAsync(customer);
            //reset selected shipping method (in case if "pick up in store" was selected)
            await _genericAttributeService.SaveAttributeAsync<ShippingOption>(
                customer,
                NopCustomerDefaults.SelectedShippingOptionAttribute,
                null,
                store.Id
            );
            await _genericAttributeService.SaveAttributeAsync<PickupPoint>(
                customer,
                NopCustomerDefaults.SelectedPickupPointAttribute,
                null,
                store.Id
            );
            //limitation - "Ship to the same address" doesn't properly work in "pick up in store only" case (when no shipping plugins are available)
            return RedirectToRoute("CheckoutShippingMethod");
        }

        return RedirectToRoute("CheckoutShippingAddress");
    }

    [HttpPost, ActionName("ErpBillingAddress")]
    [FormValueRequired("nextstep")]
    public async Task<IActionResult> NewBillingAddress(
        CheckoutErpBillingAddressModel model,
        IFormCollection form
    )
    {
        var customer = await _workContext.GetCurrentCustomerAsync();
        var b2BAccount = await _erpAccountService.GetActiveErpAccountByCustomerIdAsync(customer.Id);

        if (!await IsUserValid(b2BAccount))
            return RedirectToRoute("ShoppingCart");

        if (await _erpCustomerFunctionalityService.IsSalesOrderInvalidForCurrentCustomerAsync())
            return RedirectToRoute("ShoppingCart");

        //validation
        if (_orderSettings.CheckoutDisabled)
            return RedirectToRoute("ShoppingCart");

        var store = await _storeContext.GetCurrentStoreAsync();
        var cart = await _shoppingCartService.GetShoppingCartAsync(
            customer,
            ShoppingCartType.ShoppingCart,
            store.Id
        );

        if (!cart.Any())
            return RedirectToRoute("ShoppingCart");

        if (_orderSettings.OnePageCheckoutEnabled)
            return RedirectToRoute("CheckoutOnePage");

        if (
            await _customerService.IsGuestAsync(customer)
            && !_orderSettings.AnonymousCheckoutAllowed
        )
            return Challenge();

        //custom address attributes
        var customAttributes = await _addressAttributeParser.ParseCustomAttributesAsync(
            form,
            NopCommonDefaults.AddressAttributeControlName
        );
        var customAttributeWarnings = await _addressAttributeParser.GetAttributeWarningsAsync(
            customAttributes
        );
        foreach (var error in customAttributeWarnings)
        {
            ModelState.AddModelError("", error);
        }

        var newAddress = model.ErpBillingAddress;

        if (
            ModelState.IsValid
            && b2BAccount != null
            && b2BAccount.BillingAddressId.HasValue
            && b2BAccount.BillingAddressId.Value > 0
        )
        {
            //check if billing address id is matching
            if (newAddress.Id != b2BAccount.BillingAddressId.Value)
                throw new Exception("Billing Address can't be loaded");

            // Billing address is not editable at all for B2BUser
            var billingAddress = await _addressService.GetAddressByIdAsync(
                b2BAccount.BillingAddressId.Value
            );
            customer.BillingAddressId = billingAddress?.Id;
            await _customerService.UpdateCustomerAsync(customer);
            await _customerService.InsertCustomerAddressAsync(customer, billingAddress);

            //ship to the same address?
            if (
                _shippingSettings.ShipToSameAddress
                && model.ShipToSameAddress
                && await _shoppingCartService.ShoppingCartRequiresShippingAsync(cart)
            )
            {
                customer.ShippingAddressId = customer.BillingAddressId;
                await _customerService.UpdateCustomerAsync(customer);
                await _customerService.InsertCustomerAddressAsync(customer, billingAddress);

                //reset selected shipping method (in case if "pick up in store" was selected)
                await _genericAttributeService.SaveAttributeAsync<ShippingOption>(
                    customer,
                    NopCustomerDefaults.SelectedShippingOptionAttribute,
                    null,
                    store.Id
                );
                await _genericAttributeService.SaveAttributeAsync<PickupPoint>(
                    customer,
                    NopCustomerDefaults.SelectedPickupPointAttribute,
                    null,
                    store.Id
                );

                //limitation - "Ship to the same address" doesn't properly work in "pick up in store only" case (when no shipping plugins are available)
                return RedirectToRoute("CheckoutShippingMethod");
            }

            return RedirectToRoute("CheckoutShippingAddress");
        }

        //If we got this far, something failed, redisplay form
        model = await _erpCheckoutModelFactory.PrepareCheckoutErpBillingAddressModelAsync(
            cart,
            b2BAccount
        );
        return View(
            "~/Plugins/NopStation.Plugin.B2B.B2BB2CFeatures/Views/ErpCheckout/BillingAddress.cshtml",
            model
        );
    }

    public override async Task<IActionResult> ShippingAddress()
    {
        #region Check settings

        var (b2BAccount, b2BUser, b2CUser) = await GetB2BAccountAndUserOfCurrentCustomerAsync();

        if (!await IsUserValid(b2BAccount))
            return RedirectToRoute("ShoppingCart");

        if (await _erpCustomerFunctionalityService.IsSalesOrderInvalidForCurrentCustomerAsync())
            return RedirectToRoute("ShoppingCart");

        var customer = await _workContext.GetCurrentCustomerAsync();
        var store = await _storeContext.GetCurrentStoreAsync();
        var cart = await _shoppingCartService.GetShoppingCartAsync(
            customer,
            ShoppingCartType.ShoppingCart,
            store.Id
        );

        if (!cart.Any())
            return RedirectToRoute("ShoppingCart");

        if (_orderSettings.OnePageCheckoutEnabled)
            return RedirectToRoute("CheckoutOnePage");

        if (
            await _customerService.IsGuestAsync(customer)
            && !_orderSettings.AnonymousCheckoutAllowed
        )
            return Challenge();

        if (!await _shoppingCartService.ShoppingCartRequiresShippingAsync(cart))
            return RedirectToRoute("CheckoutShippingMethod");

        #endregion

        //await _erpAccountCreditSyncFunctionality.LiveErpAccountCreditCheckAsync(b2BAccount, customer);

        #region B2B User

        if (b2BUser != null && b2BAccount != null && b2BUser.ErpShipToAddressId > 0)
        {
            var b2BShipToAddressModel =
                await _erpCheckoutModelFactory.PrepareCheckoutB2BShippingAddressModelAsync(
                    cart,
                    b2BUser,
                    b2BAccount
                );
            return View(
                "~/Plugins/NopStation.Plugin.B2B.B2BB2CFeatures/Views/ErpCheckout/ShippingAddress.cshtml",
                b2BShipToAddressModel
            );
        }

        #endregion

        #region B2C User

        if (b2CUser != null && b2BAccount != null)
        {
            var b2CShipToAddress =
                await _erpShipToAddressService.GetErpShipToAddressByIdWithActiveAsync(
                    b2CUser.ErpShipToAddressId
                );
            if (b2CShipToAddress == null || b2CShipToAddress.Id < 0)
            {
                _notificationService.ErrorNotification(
                    await _localizationService.GetResourceAsync(
                        "Plugin.Misc.NopStation.B2BB2CFeaturesPlugins.ErpCheckout.B2CShipToAddress.NotFound"
                    )
                );
                return RedirectToRoute("ShoppingCart");
            }

            if (!Enum.IsDefined(typeof(DeliveryOption), b2CShipToAddress.DeliveryOptionId))
            {
                _notificationService.ErrorNotification(await _localizationService.GetResourceAsync("Plugins.Payment.B2BCustomerAccount.B2BCheckout.B2CShipToAddress.DeliveryOption.NotFound"));
                return RedirectToRoute("ShoppingCart");
            }

            var (isCustomerInExpressShopZone, _, _) = await _soltrackIntegrationService.GetSoltrackResponseAsync(customer, b2CShipToAddress.Latitude, b2CShipToAddress.Longitude);

            if (isCustomerInExpressShopZone)
            {
                b2CShipToAddress.IsActive = false;
                b2CShipToAddress.DeliveryOptionId = (int)DeliveryOption.NoShop;
                await _erpShipToAddressService.UpdateErpShipToAddressAsync(b2CShipToAddress);
            }

            var b2CShipToAddressModel =
                await _erpCheckoutModelFactory.PrepareCheckoutB2CShippingAddressModelAsync(
                    cart,
                    b2CUser,
                    b2BAccount
                );
            if (isCustomerInExpressShopZone)
            {
                b2CShipToAddressModel.HasAddressChanged = 1;
            }

            return View(
                "~/Plugins/NopStation.Plugin.B2B.B2BB2CFeatures/Views/ErpCheckout/ShippingAddress.cshtml",
                b2CShipToAddressModel
            );
        }

        #endregion

        //model
        var model = await _checkoutModelFactory.PrepareShippingAddressModelAsync(
            cart,
            prePopulateNewAddressWithCustomerFields: true
        );
        return View(model);
    }

    public override async Task<IActionResult> SelectShippingAddress(int addressId)
    {
        var customer = await _workContext.GetCurrentCustomerAsync();
        var b2BAccount = await _erpAccountService.GetActiveErpAccountByCustomerIdAsync(customer.Id);

        if (!await IsUserValid(b2BAccount))
            return RedirectToRoute("ShoppingCart");

        var address = await _customerService.GetCustomerAddressAsync(customer.Id, addressId);

        if (address == null)
            return RedirectToRoute("CheckoutShippingAddress");

        customer.ShippingAddressId = address.Id;
        await _customerService.UpdateCustomerAsync(customer);

        if (_shippingSettings.AllowPickupInStore)
        {
            var store = await _storeContext.GetCurrentStoreAsync();
            //set value indicating that "pick up in store" option has not been chosen
            await _genericAttributeService.SaveAttributeAsync<PickupPoint>(
                customer,
                NopCustomerDefaults.SelectedPickupPointAttribute,
                null,
                store.Id
            );
        }

        return RedirectToRoute("CheckoutShippingMethod");
    }

    [HttpPost, ActionName("ErpShippingAddress")]
    [FormValueRequired("nextstep")]
    public async Task<IActionResult> NewShippingAddress(
        CheckoutErpShippingAddressModel model,
        IFormCollection form
    )
    {
        var (b2BAccount, b2BUser, b2CUser) = await GetB2BAccountAndUserOfCurrentCustomerAsync();

        if (!await IsUserValid(b2BAccount))
            return RedirectToRoute("ShoppingCart");

        var customer = await _workContext.GetCurrentCustomerAsync();
        var store = await _storeContext.GetCurrentStoreAsync();
        var cart = await _shoppingCartService.GetShoppingCartAsync(
            customer,
            ShoppingCartType.ShoppingCart,
            store.Id
        );

        if (!cart.Any())
            return RedirectToRoute("ShoppingCart");

        if (_orderSettings.OnePageCheckoutEnabled)
            return RedirectToRoute("CheckoutOnePage");

        if (await _customerService.IsGuestAsync(customer) && !_orderSettings.AnonymousCheckoutAllowed)
            return Challenge();

        if (!await _shoppingCartService.ShoppingCartRequiresShippingAsync(cart))
        {
            customer.ShippingAddressId = null;
            await _customerService.UpdateCustomerAsync(customer);
            return RedirectToRoute("CheckoutShippingMethod");
        }

        //pickup point
        if (_shippingSettings.AllowPickupInStore)
        {
            if (model.PickupInStore)
            {
                // For Collect the delivery date will be the next day of the Order day.
                var defaultDeliveryDateForPickUpInStore = DateTime.Now.AddDays(1);
                await _genericAttributeService.SaveAttributeAsync(
                    customer,
                    B2BB2CFeaturesDefaults.SelectedB2BDeliveryDateAttribute,
                    defaultDeliveryDateForPickUpInStore,
                    store.Id
                );

                //no shipping address selected
                customer.ShippingAddressId = null;
                await _customerService.UpdateCustomerAsync(customer);

                // Parse and save pickup point - similar to old code but adapted for async
                var pickupOption = await ParsePickupOptionAsync(cart, form);
                await SavePickupOptionAsync(pickupOption);

                if (!string.IsNullOrEmpty(model.SpecialInstructions?.Trim()))
                {
                    await _genericAttributeService.SaveAttributeAsync(
                        customer,
                        B2BB2CFeaturesDefaults.B2BSpecialInstructions,
                        model.SpecialInstructions?.Trim(),
                        store.Id
                    );
                }

                if (!string.IsNullOrEmpty(model.CustomerReference?.Trim()))
                {
                    await _genericAttributeService.SaveAttributeAsync(
                        customer,
                        B2BB2CFeaturesDefaults.B2BCustomerReferenceAsPO,
                        model.CustomerReference?.Trim(),
                        store.Id
                    );
                }

                return RedirectToRoute("CheckoutPaymentMethod");
            }

            //set value indicating that "pick up in store" option has not been chosen
            await _genericAttributeService.SaveAttributeAsync<PickupPoint>(
                customer,
                NopCustomerDefaults.SelectedPickupPointAttribute,
                null,
                store.Id
            );
        }

        var erpUser = b2BUser ?? b2CUser;

        if (erpUser == null)
        {
            _notificationService.ErrorNotification(await _localizationService.GetResourceAsync("NopStation.Plugin.B2B.B2BB2CFeatures.ErpCheckout.ErpUser.NotFound"));
            return RedirectToRoute("ShoppingCart");
        }

        var date = model.DeliveryDate;

        // Check for FullLoadRequired
        if (model.IsFullLoadRequired)
        {
            (var minDeliveryDate, var _) = await _erpCustomerFunctionalityService.GetMinimumAndMaximumDeliveryDateForShippingAddress();
            date = minDeliveryDate.Date;
        }
        else if (!_b2BB2CFeaturesSettings.ERPToDetermineDate)
        {
            if (date == null)
                ModelState.AddModelError("DeliveryDate", "Please provide a valid delivery date");

            (var minDeliveryDate, var maxDeliveryDate) = await _erpCustomerFunctionalityService.GetMinimumAndMaximumDeliveryDateForShippingAddress();

            if (date < minDeliveryDate.Date || date > maxDeliveryDate)
                ModelState.AddModelError("DeliveryDate", "Please provide a valid delivery date");
        }

        if (string.IsNullOrEmpty(model.DeliveryDateString) || !DateTime.TryParse(model.DeliveryDateString, out var selectedDeliveryDate))
            ModelState.AddModelError("DeliveryDate", "Please provide a valid delivery date");

        model.ErpAccountId = b2BAccount.Id;
        var shipToAddressOfErpNopUser = await _erpShipToAddressService.GetErpShipToAddressByIdWithActiveAsync(erpUser.ErpShipToAddressId);

        if (ModelState.IsValid && erpUser.ErpUserType == ErpUserType.B2BUser && shipToAddressOfErpNopUser != null && model.ErpShipToAddressId > 0)
        {
            var shipToAddress = await _erpShipToAddressService.GetErpShipToAddressByIdWithActiveAsync(model.ErpShipToAddressId);
            if (shipToAddress == null ||
                (await _erpAccountService.GetErpAccountByErpShipToAddressAsync(shipToAddress))?.Id != erpUser.ErpAccountId)
            {
                throw new Exception("B2B Checkout: Erp account of the ship to address and the user is not the same");
            }

            (var minDeliveryDate, var _) = await _erpCustomerFunctionalityService.GetMinimumAndMaximumDeliveryDateForShippingAddress();
            model.DeliveryDate = minDeliveryDate.Date;

            //set value of selected Delivery Date
            if (model.ErpToDetermineDate && !string.IsNullOrEmpty(model.DeliveryDateString))
            {
                try
                {
                    date = DateTime.Parse(model.DeliveryDateString);
                    if (date == DateTime.MinValue)
                        date = model.DeliveryDate;

                    await _genericAttributeService.SaveAttributeAsync(
                        customer,
                        B2BB2CFeaturesDefaults.SelectedB2BDeliveryDateAttribute,
                        date,
                        store.Id
                    );
                }
                catch
                {
                    await _genericAttributeService.SaveAttributeAsync(
                        customer,
                        B2BB2CFeaturesDefaults.SelectedB2BDeliveryDateAttribute,
                        date,
                        store.Id
                    );
                }
            }
            else
            {
                await _genericAttributeService.SaveAttributeAsync(
                    customer,
                    B2BB2CFeaturesDefaults.SelectedB2BDeliveryDateAttribute,
                    date,
                    store.Id
                );
            }

            if (!string.IsNullOrEmpty(model.SpecialInstructions?.Trim()))
            {
                await _genericAttributeService.SaveAttributeAsync(
                    customer,
                    B2BB2CFeaturesDefaults.B2BSpecialInstructions,
                    model.SpecialInstructions?.Trim(),
                    store.Id
                );
            }

            // Customer reference/PO
            if (!string.IsNullOrEmpty(model.CustomerReference?.Trim()))
            {
                await _genericAttributeService.SaveAttributeAsync(
                    customer,
                    B2BB2CFeaturesDefaults.B2BCustomerReferenceAsPO,
                    model.CustomerReference?.Trim(),
                    store.Id
                );
            }

            // No need to check allow address edit
            erpUser.ShippingErpShipToAddressId = shipToAddress.Id;
            await _erpNopUserService.UpdateErpNopUserAsync(erpUser);

            var address = await _addressService.GetAddressByIdAsync(shipToAddress.AddressId);

            address.Email = customer.Email;
            address.FirstName = customer.FirstName;
            address.LastName = customer.LastName;
            address.Company = shipToAddress.ShipToName;

            customer.ShippingAddressId = address.Id;
            await _customerService.UpdateCustomerAsync(customer);

            var customerAddress = await _customerService.GetCustomerAddressAsync(customer.Id, address.Id);
            if (customerAddress == null)
            {
                await _customerService.InsertCustomerAddressAsync(customer, address);
            }

            return RedirectToRoute("CheckoutShippingMethod");
        }

        //If we got this far, something failed, redisplay form
        var erpShipToAddressModel = await _erpCheckoutModelFactory.PrepareCheckoutB2BShippingAddressModelAsync(cart, erpUser, b2BAccount);
        return View("~/Plugins/NopStation.Plugin.B2B.B2BB2CFeatures/Views/ErpCheckout/ShippingAddress.cshtml", erpShipToAddressModel);
    }

    [HttpPost, ActionName("ErpShippingAddress")]
    [FormValueRequired("b2cnextstep")]
    public async Task<IActionResult> B2CShippingAddress(CheckoutErpShippingAddressModel model, IFormCollection form)
    {
        #region Check settings

        var (b2BAccount, _, b2CUser) = await GetB2BAccountAndUserOfCurrentCustomerAsync();

        if (b2CUser == null || b2CUser.ErpUserType != ErpUserType.B2CUser)
            return Challenge();

        if (!await IsUserValid(b2BAccount))
            return RedirectToRoute("ShoppingCart");

        var customer = await _workContext.GetCurrentCustomerAsync();
        var store = await _storeContext.GetCurrentStoreAsync();
        var cart = await _shoppingCartService.GetShoppingCartAsync(
            customer,
            ShoppingCartType.ShoppingCart,
            store.Id
        );

        if (!cart.Any())
            return RedirectToRoute("ShoppingCart");

        if (_orderSettings.OnePageCheckoutEnabled)
            return RedirectToRoute("CheckoutOnePage");

        if (await _customerService.IsGuestAsync(customer) && !_orderSettings.AnonymousCheckoutAllowed)
            return Challenge();

        if (!await _shoppingCartService.ShoppingCartRequiresShippingAsync(cart))
        {
            customer.ShippingAddressId = null;
            await _customerService.UpdateCustomerAsync(customer);
            return RedirectToRoute("CheckoutShippingMethod");
        }

        #endregion

        if (b2CUser.LastWarehouseCalculationTimeUtc.HasValue &&
            (DateTime.UtcNow - b2CUser.LastWarehouseCalculationTimeUtc.Value).TotalSeconds >= _b2BB2CFeaturesSettings.WarehouseCalculationTimeout)
        {
            _notificationService.ErrorNotification(
                await _localizationService.GetResourceAsync("Plugin.Misc.NopStation.Plugin.B2B.B2BB2CFeatures.ErpCheckout.WarehouseCalculationTimeout")
            );
            return RedirectToRoute("ShoppingCart");
        }

        model.ErpAccountId = b2BAccount.Id;
        model.B2CUserId = b2CUser.Id;

        var erpShipToAddress = await _erpShipToAddressService.GetErpShipToAddressByIdWithActiveAsync(b2CUser.ErpShipToAddressId);
        if (ModelState.IsValid && erpShipToAddress != null && erpShipToAddress.AddressId != 0)
        {
            if ((await _erpAccountService.GetErpAccountByErpShipToAddressAsync(erpShipToAddress))?.Id != b2CUser.ErpAccountId)
            {
                _notificationService.ErrorNotification(await _localizationService.GetResourceAsync("Plugin.Misc.NopStation.B2BB2CFeaturesPlugins.ErpCheckout.B2CShipToAddress.ErpAccountMismatch"));
                return RedirectToRoute("ShoppingCart");
            }

            // Determine delivery date for collection vs. normal delivery
            var defaultDeliveryDateForCollection = new DateTime();
            var minDeliveryDate = new DateTime();
            var maxDeliveryDate = new DateTime();

            var isCollectionOrder = _shippingSettings.AllowPickupInStore && model.PickupInStore;
            if (isCollectionOrder)
                defaultDeliveryDateForCollection = DateTime.Now.AddDays(1);
            else
                (minDeliveryDate, maxDeliveryDate) = await _erpCustomerFunctionalityService.GetMinimumAndMaximumDeliveryDateForShippingAddress();

            // Process each shopping cart item
            foreach (var sci in cart)
            {
                // Get the B2C shopping cart item model associated with this nop shopping cart item
                var b2CSciModel = model.B2CShoppingCartItemModels?.Find(a => a.ShoppingCartItemModelId == sci.Id);
                if (b2CSciModel != null)
                {
                    if (!isCollectionOrder)
                    {
                        if (b2CSciModel.IsFullLoadRequired)
                            b2CSciModel.DeliveryDate = minDeliveryDate.Date;
                        else if (!model.ErpToDetermineDate)
                        {
                            if (b2CSciModel.DeliveryDate == null)
                            {
                                ModelState.AddModelError("DeliveryDate", "Please provide a valid delivery date");
                                break;
                            }
                            if (b2CSciModel.DeliveryDate < minDeliveryDate.Date || b2CSciModel.DeliveryDate > maxDeliveryDate)
                            {
                                ModelState.AddModelError("DeliveryDate", "Please provide a valid delivery date");
                                break;
                            }
                        }
                    }

                    var b2cShoppingCartItem = await _b2CShoppingCartItemService.GetB2CShoppingCartItemByNopShoppingCartItemIdAsync(sci.Id);
                    if (b2cShoppingCartItem == null)
                    {
                        await _b2CShoppingCartItemService.InsertB2CShoppingCartItemAsync(new B2CShoppingCartItem
                        {
                            ShoppingCartItemId = sci.Id,
                            NopWarehouseId = b2CSciModel.NopWarehouseId,
                            WarehouseCode = b2CSciModel.WarehouseCode,
                            SpecialInstructions = b2CSciModel.SpecialInstructions,
                            DeliveryDate = isCollectionOrder ? defaultDeliveryDateForCollection : b2CSciModel.DeliveryDate
                        });
                    }
                    else
                    {
                        b2cShoppingCartItem.NopWarehouseId = b2CSciModel.NopWarehouseId;
                        b2cShoppingCartItem.WarehouseCode = b2CSciModel.WarehouseCode;
                        b2cShoppingCartItem.SpecialInstructions = b2CSciModel.SpecialInstructions;
                        b2cShoppingCartItem.DeliveryDate = isCollectionOrder ? defaultDeliveryDateForCollection : b2CSciModel.DeliveryDate;

                        await _b2CShoppingCartItemService.UpdateB2CShoppingCartItemAsync(b2cShoppingCartItem);
                    }
                }
            }

            // If validation failed after processing items, return with errors
            if (!ModelState.IsValid)
            {
                var erpShipToAddressModel = await _erpCheckoutModelFactory.PrepareCheckoutB2CShippingAddressModelAsync(cart, b2CUser, b2BAccount);
                return View("~/Plugins/NopStation.Plugin.B2B.B2BB2CFeatures/Views/ErpCheckout/ShippingAddress.cshtml", erpShipToAddressModel);
            }

            // Save special instructions
            if (!string.IsNullOrEmpty(model.SpecialInstructions?.Trim()))
                await _genericAttributeService.SaveAttributeAsync(
                    customer,
                    B2BB2CFeaturesDefaults.B2CSpecialInstructions,
                    model.SpecialInstructions?.Trim(),
                    store.Id
                );

            // Save customer reference
            if (!string.IsNullOrEmpty(model.CustomerReference?.Trim()))
                await _genericAttributeService.SaveAttributeAsync(
                    customer,
                    B2BB2CFeaturesDefaults.B2CCustomerReferenceAsPO,
                    model.CustomerReference?.Trim(),
                    store.Id
                );
            else
                await _genericAttributeService.SaveAttributeAsync(
                    customer,
                    B2BB2CFeaturesDefaults.B2CCustomerReferenceAsPO,
                    string.Empty,
                    store.Id
                );

            // Handle shipping address based on collection/delivery
            if (isCollectionOrder)
            {
                customer.ShippingAddressId = null;
            }
            else
            {
                var address = await _addressService.GetAddressByIdAsync(erpShipToAddress.AddressId);
                address.Email = customer.Email;
                address.FirstName = customer.FirstName;
                address.LastName = customer.LastName;
                address.Company = customer.Company;

                customer.ShippingAddressId = address.Id;
                await _customerService.InsertCustomerAddressAsync(customer, address);
            }

            // Update the user's shipping address ID and save
            b2CUser.ShippingErpShipToAddressId = erpShipToAddress.Id;
            await _erpNopUserService.UpdateErpNopUserAsync(b2CUser);

            await _customerService.UpdateCustomerAsync(customer);

            return RedirectToRoute("CheckoutPaymentMethod");
        }

        //If we got this far, something failed, redisplay form
        var b2CShipToAddressModel = await _erpCheckoutModelFactory.PrepareCheckoutB2CShippingAddressModelAsync(cart, b2CUser, b2BAccount);
        return View("~/Plugins/NopStation.Plugin.B2B.B2BB2CFeatures/Views/ErpCheckout/ShippingAddress.cshtml", b2CShipToAddressModel);
    }

    public override async Task<IActionResult> ShippingMethod()
    {
        var customer = await _workContext.GetCurrentCustomerAsync();
        var b2BAccount = await _erpAccountService.GetActiveErpAccountByCustomerIdAsync(customer.Id);

        if (!await IsUserValid(b2BAccount))
            return RedirectToRoute("ShoppingCart");

        var store = await _storeContext.GetCurrentStoreAsync();
        var cart = await _shoppingCartService.GetShoppingCartAsync(
            customer,
            ShoppingCartType.ShoppingCart,
            store.Id
        );

        if (!cart.Any())
            return RedirectToRoute("ShoppingCart");

        if (_orderSettings.OnePageCheckoutEnabled)
            return RedirectToRoute("CheckoutOnePage");

        if (
            await _customerService.IsGuestAsync(customer)
            && !_orderSettings.AnonymousCheckoutAllowed
        )
            return Challenge();

        if (!await _shoppingCartService.ShoppingCartRequiresShippingAsync(cart))
        {
            await _genericAttributeService.SaveAttributeAsync<ShippingOption>(
                customer,
                NopCustomerDefaults.SelectedShippingOptionAttribute,
                null,
                store.Id
            );
            return RedirectToRoute("CheckoutPaymentMethod");
        }

        //check if pickup point is selected on the shipping address step
        if (!_orderSettings.DisplayPickupInStoreOnShippingMethodPage)
        {
            var selectedPickUpPoint = await _genericAttributeService.GetAttributeAsync<PickupPoint>(
                customer,
                NopCustomerDefaults.SelectedPickupPointAttribute,
                store.Id
            );
            if (selectedPickUpPoint != null)
                return RedirectToRoute("CheckoutPaymentMethod");
        }

        //model
        var model = await _checkoutModelFactory.PrepareShippingMethodModelAsync(
            cart,
            await _customerService.GetCustomerShippingAddressAsync(customer)
        );

        if (
            _shippingSettings.BypassShippingMethodSelectionIfOnlyOne
            && model.ShippingMethods.Count == 1
        )
        {
            //if we have only one shipping method, then a customer doesn't have to choose a shipping method
            if (await _genericAttributeService.GetAttributeAsync<ShippingOption>(customer, NopCustomerDefaults.SelectedShippingOptionAttribute, store.Id) == null)
            {
                await _genericAttributeService.SaveAttributeAsync(customer,
                    NopCustomerDefaults.SelectedShippingOptionAttribute,
                    model.ShippingMethods[0].ShippingOption,
                    store.Id);
            }

            return RedirectToRoute("CheckoutPaymentMethod");
        }

        return View(model);
    }

    [HttpPost, ActionName("ShippingMethod")]
    [FormValueRequired("nextstep")]
    public override async Task<IActionResult> SelectShippingMethod(
        string shippingoption,
        IFormCollection form
    )
    {
        //validation
        if (_orderSettings.CheckoutDisabled)
            return RedirectToRoute("ShoppingCart");

        var customer = await _workContext.GetCurrentCustomerAsync();
        var store = await _storeContext.GetCurrentStoreAsync();
        var cart = await _shoppingCartService.GetShoppingCartAsync(customer, ShoppingCartType.ShoppingCart, store.Id);

        if (!cart.Any())
            return RedirectToRoute("ShoppingCart");

        if (_orderSettings.OnePageCheckoutEnabled)
            return RedirectToRoute("CheckoutOnePage");

        if (
            await _customerService.IsGuestAsync(customer)
            && !_orderSettings.AnonymousCheckoutAllowed
        )
            return Challenge();

        if (!await _shoppingCartService.ShoppingCartRequiresShippingAsync(cart))
        {
            await _genericAttributeService.SaveAttributeAsync<ShippingOption>(
                customer,
                NopCustomerDefaults.SelectedShippingOptionAttribute,
                null,
                store.Id
            );
            return RedirectToRoute("CheckoutPaymentMethod");
        }

        //pickup point
        if (
            _shippingSettings.AllowPickupInStore /*&& _orderSettings.DisplayPickupInStoreOnShippingMethodPage*/
        )
        {
            var pickupInStore = ParsePickupInStore(form);
            if (pickupInStore)
            {
                var pickupOption = await ParsePickupOptionAsync(cart, form);
                await SavePickupOptionAsync(pickupOption);

                return RedirectToRoute("CheckoutPaymentMethod");
            }

            //set value indicating that "pick up in store" option has not been chosen
            await _genericAttributeService.SaveAttributeAsync<PickupPoint>(
                customer,
                NopCustomerDefaults.SelectedPickupPointAttribute,
                null,
                store.Id
            );
        }

        //parse selected method
        if (string.IsNullOrEmpty(shippingoption))
            return await ShippingMethod();
        var splittedOption = shippingoption.Split(
            new[] { "___" },
            StringSplitOptions.RemoveEmptyEntries
        );
        if (splittedOption.Length != 2)
            return await ShippingMethod();
        var selectedName = splittedOption[0];
        var shippingRateComputationMethodSystemName = splittedOption[1];

        //find it
        //performance optimization. try cache first
        var shippingOptions = await _genericAttributeService.GetAttributeAsync<List<ShippingOption>>(customer,
            NopCustomerDefaults.OfferedShippingOptionsAttribute, store.Id);
        if (shippingOptions == null || shippingOptions.Count == 0)
        {
            //not found? let's load them using shipping service
            shippingOptions = (
                await _shippingService.GetShippingOptionsAsync(
                    cart,
                    await _customerService.GetCustomerShippingAddressAsync(customer),
                    customer,
                    shippingRateComputationMethodSystemName,
                    store.Id
                )
            ).ShippingOptions.ToList();
        }
        else
        {
            //loaded cached results. let's filter result by a chosen shipping rate computation method
            shippingOptions = shippingOptions
                .Where(so =>
                    so.ShippingRateComputationMethodSystemName.Equals(
                        shippingRateComputationMethodSystemName,
                        StringComparison.InvariantCultureIgnoreCase
                    )
                )
                .ToList();
        }

        var shippingOption = shippingOptions.Find(so =>
            !string.IsNullOrEmpty(so.Name)
            && so.Name.Equals(selectedName, StringComparison.InvariantCultureIgnoreCase)
        );
        if (shippingOption == null)
            return await ShippingMethod();

        //save
        await _genericAttributeService.SaveAttributeAsync(
            customer,
            NopCustomerDefaults.SelectedShippingOptionAttribute,
            shippingOption,
            store.Id
        );

        return RedirectToRoute("CheckoutPaymentMethod");
    }

    public override async Task<IActionResult> PaymentMethod()
    {
        #region Check settings

        var (b2BAccount, b2BUser, b2CUser) = await GetB2BAccountAndUserOfCurrentCustomerAsync();

        if (!await IsUserValid(b2BAccount))
            return RedirectToRoute("ShoppingCart");

        var customer = await _workContext.GetCurrentCustomerAsync();
        var store = await _storeContext.GetCurrentStoreAsync();
        var cart = await _shoppingCartService.GetShoppingCartAsync(
            customer,
            ShoppingCartType.ShoppingCart,
            store.Id
        );

        if (!cart.Any())
            return RedirectToRoute("ShoppingCart");

        if (_orderSettings.OnePageCheckoutEnabled)
            return RedirectToRoute("CheckoutOnePage");

        if (
            await _customerService.IsGuestAsync(customer)
            && !_orderSettings.AnonymousCheckoutAllowed
        )
            return Challenge();

        if (b2CUser != null &&
            b2CUser.LastWarehouseCalculationTimeUtc.HasValue &&
            (DateTime.UtcNow - b2CUser.LastWarehouseCalculationTimeUtc.Value).TotalSeconds >= _b2BB2CFeaturesSettings.WarehouseCalculationTimeout)
        {
            _notificationService.ErrorNotification(
                await _localizationService.GetResourceAsync("Plugin.Misc.NopStation.Plugin.B2B.B2BB2CFeatures.ErpCheckout.WarehouseCalculationTimeout")
            );
            return RedirectToRoute("ShoppingCart");
        }

        #endregion

        try
        {
            #region Quote place

            // check if it a Quote Order, for Quote Order we will skip payment part and redirect to CheckoutConfirm
            var isQuoteOrder = await IsQuoteOrderAsync();
            if (isQuoteOrder || await _erpCustomerFunctionalityService.IsCurrentCustomerInB2BQuoteAssistantRoleAsync())
            {
                await _genericAttributeService.SaveAttributeAsync<string>(customer, NopCustomerDefaults.SelectedPaymentMethodAttribute, null, store.Id);

                //skip payment info page too
                var paymentInfo = new ProcessPaymentRequest();

                //session save
                await HttpContext.Session.SetAsync("OrderPaymentInfo", paymentInfo);

                // For Quote Order, Place Order From Here

                //prevent 2 orders being placed within an X seconds time frame
                if (!await IsMinimumOrderPlacementIntervalValidAsync(customer))
                    throw new Exception(
                        await _localizationService.GetResourceAsync(
                            "Checkout.MinOrderPlacementInterval"
                        )
                    );

                //place order
                var processPaymentRequest = new ProcessPaymentRequest();

                await _paymentService.GenerateOrderGuidAsync(processPaymentRequest);
                processPaymentRequest.StoreId = store.Id;
                processPaymentRequest.CustomerId = customer.Id;
                processPaymentRequest.PaymentMethodSystemName = await _genericAttributeService.GetAttributeAsync<string>(customer,
                    NopCustomerDefaults.SelectedPaymentMethodAttribute, store.Id);
                await HttpContext.Session.SetAsync("OrderPaymentInfo", processPaymentRequest);

                var placeOrderResult = await _overriddenOrderProcessingService.PlaceQuoteOrderAsync(processPaymentRequest);
                if (placeOrderResult.Success)
                {
                    if (b2BAccount != null && b2BUser != null)
                        await _overriddenOrderProcessingService.PlaceErpOrderAtNopAsync(placeOrderResult.PlacedOrder, ErpOrderType.B2BQuote);

                    await ClearGenericAttributeForQuoteOrderAsync();

                    await _erpLogsService.InformationAsync($"Quote order placed successfully! Order-Id: {placeOrderResult.PlacedOrder.Id}, Erp Order Number: " + placeOrderResult.PlacedOrder.CustomOrderNumber, ErpSyncLevel.Order, customer: customer);

                    await _customerActivityService.InsertActivityAsync(customer, "PublicStore.PlaceOrder",
                            string.Format(await _localizationService.GetResourceAsync("ActivityLog.PublicStore.PlaceOrder"),
                                placeOrderResult.PlacedOrder.CustomOrderNumber), placeOrderResult.PlacedOrder);

                    return RedirectToRoute("CheckoutCompleted", new { orderId = placeOrderResult.PlacedOrder.Id });
                }

                // if place order is not successful
                return RedirectToRoute("CheckoutConfirm");
            }

            #endregion
        }
        catch (Exception ex)
        {
            await _logger.WarningAsync(ex.Message, ex);

            // if place order is not successful
            return RedirectToRoute("CheckoutConfirm");
        }

        //Check whether payment workflow is required
        //we ignore reward points during cart total calculation
        var isPaymentWorkflowRequired =
            await _orderProcessingService.IsPaymentWorkflowRequiredAsync(cart, false);
        if (!isPaymentWorkflowRequired)
        {
            await _genericAttributeService.SaveAttributeAsync<string>(
                customer,
                NopCustomerDefaults.SelectedPaymentMethodAttribute,
                null,
                store.Id
            );
            return RedirectToRoute("CheckoutPaymentInfo");
        }

        var billingAddress = await _addressService.GetAddressByIdAsync(customer.BillingAddressId.Value);
        var country = await _countryService.GetCountryByIdAsync(billingAddress?.CountryId ?? 0);

        //filter by country
        var filterByCountryId = 0;
        if (_addressSettings.CountryEnabled && billingAddress != null && country != null)
        {
            filterByCountryId = country.Id;
        }

        //model
        var paymentMethodModel = await _checkoutModelFactory.PreparePaymentMethodModelAsync(
            cart,
            filterByCountryId
        );

        if (
            _paymentSettings.BypassPaymentMethodSelectionIfOnlyOne
            && paymentMethodModel.PaymentMethods.Count == 1
            && !paymentMethodModel.DisplayRewardPoints
        )
        {
            //if we have only one payment method and reward points are disabled or the current customer doesn't have any reward points
            //so customer doesn't have to choose a payment method

            await _genericAttributeService.SaveAttributeAsync(
                customer,
                NopCustomerDefaults.SelectedPaymentMethodAttribute,
                paymentMethodModel.PaymentMethods[0].PaymentMethodSystemName,
                store.Id
            );
            return RedirectToRoute("CheckoutPaymentInfo");
        }

        return View("~/Plugins/NopStation.Plugin.B2B.B2BB2CFeatures/Views/Checkout/PaymentMethod.cshtml", paymentMethodModel);
    }

    [HttpPost, ActionName("PaymentMethod")]
    [FormValueRequired("nextstep")]
    public override async Task<IActionResult> SelectPaymentMethod(
        string paymentmethod,
        CheckoutPaymentMethodModel model
    )
    {
        var customer = await _workContext.GetCurrentCustomerAsync();
        var b2BAccount = await _erpAccountService.GetActiveErpAccountByCustomerIdAsync(customer.Id);

        if (!await IsUserValid(b2BAccount))
            return RedirectToRoute("ShoppingCart");

        var store = await _storeContext.GetCurrentStoreAsync();
        var cart = await _shoppingCartService.GetShoppingCartAsync(
            customer,
            ShoppingCartType.ShoppingCart,
            store.Id
        );

        if (!cart.Any())
            return RedirectToRoute("ShoppingCart");

        if (_orderSettings.OnePageCheckoutEnabled)
            return RedirectToRoute("CheckoutOnePage");

        if (
            await _customerService.IsGuestAsync(customer)
            && !_orderSettings.AnonymousCheckoutAllowed
        )
            return Challenge();

        //reward points
        if (_rewardPointsSettings.Enabled)
        {
            await _genericAttributeService.SaveAttributeAsync(
                customer,
                NopCustomerDefaults.UseRewardPointsDuringCheckoutAttribute,
                model.UseRewardPoints,
                store.Id
            );
        }

        //Check whether payment workflow is required
        var isPaymentWorkflowRequired =
            await _orderProcessingService.IsPaymentWorkflowRequiredAsync(cart);
        if (!isPaymentWorkflowRequired)
        {
            await _genericAttributeService.SaveAttributeAsync<string>(
                customer,
                NopCustomerDefaults.SelectedPaymentMethodAttribute,
                null,
                store.Id
            );
            return RedirectToRoute("CheckoutPaymentInfo");
        }
        //payment method
        if (string.IsNullOrEmpty(paymentmethod))
            return await PaymentMethod();

        if (!await _paymentPluginManager.IsPluginActiveAsync(paymentmethod, customer, store.Id))
            return await PaymentMethod();

        //save
        await _genericAttributeService.SaveAttributeAsync(
            customer,
            NopCustomerDefaults.SelectedPaymentMethodAttribute,
            paymentmethod,
            store.Id
        );

        return RedirectToRoute("CheckoutPaymentInfo");
    }

    public override async Task<IActionResult> PaymentInfo()
    {
        var customer = await _workContext.GetCurrentCustomerAsync();
        var b2BAccount = await _erpAccountService.GetActiveErpAccountByCustomerIdAsync(customer.Id);

        if (!await IsUserValid(b2BAccount))
            return RedirectToRoute("ShoppingCart");

        var store = await _storeContext.GetCurrentStoreAsync();
        var cart = await _shoppingCartService.GetShoppingCartAsync(
            customer,
            ShoppingCartType.ShoppingCart,
            store.Id
        );

        if (!cart.Any())
            return RedirectToRoute("ShoppingCart");

        if (_orderSettings.OnePageCheckoutEnabled)
            return RedirectToRoute("CheckoutOnePage");

        if (
            await _customerService.IsGuestAsync(customer)
            && !_orderSettings.AnonymousCheckoutAllowed
        )
            return Challenge();

        //await _erpAccountCreditSyncFunctionality.LiveErpAccountCreditCheckAsync(b2BAccount, customer);

        //Check whether payment workflow is required
        var isPaymentWorkflowRequired =
            await _orderProcessingService.IsPaymentWorkflowRequiredAsync(cart);
        if (!isPaymentWorkflowRequired)
        {
            return RedirectToRoute("CheckoutConfirm");
        }

        //load payment method
        var paymentMethodSystemName = await _genericAttributeService.GetAttributeAsync<string>(
            customer,
            NopCustomerDefaults.SelectedPaymentMethodAttribute,
            store.Id
        );
        var paymentMethod = await _paymentPluginManager.LoadPluginBySystemNameAsync(
            paymentMethodSystemName,
            customer,
            store.Id
        );
        if (paymentMethod == null)
            return RedirectToRoute("CheckoutPaymentMethod");

        //Check whether payment info should be skipped
        if (
            paymentMethod.SkipPaymentInfo
            || (
                paymentMethod.PaymentMethodType == PaymentMethodType.Redirection
                && _paymentSettings.SkipPaymentInfoStepForRedirectionPaymentMethods
            )
        )
        {
            //skip payment info page
            var paymentInfo = new ProcessPaymentRequest();

            //session save
            await HttpContext.Session.SetAsync("OrderPaymentInfo", paymentInfo);

            return RedirectToRoute("CheckoutConfirm");
        }

        //model
        var model = await _checkoutModelFactory.PreparePaymentInfoModelAsync(paymentMethod);
        return View("~/Plugins/NopStation.Plugin.B2B.B2BB2CFeatures/Views/Checkout/PaymentInfo.cshtml", model);
    }

    [HttpPost, ActionName("PaymentInfo")]
    [FormValueRequired("nextstep")]
    public override async Task<IActionResult> EnterPaymentInfo(IFormCollection form)
    {
        #region Check settings

        var (b2BAccount, b2BUser, b2CUser) = await GetB2BAccountAndUserOfCurrentCustomerAsync();

        if (!await IsUserValid(b2BAccount))
            return RedirectToRoute("ShoppingCart");

        var customer = await _workContext.GetCurrentCustomerAsync();
        var store = await _storeContext.GetCurrentStoreAsync();
        var cart = await _shoppingCartService.GetShoppingCartAsync(
            customer,
            ShoppingCartType.ShoppingCart,
            store.Id
        );

        if (!cart.Any())
            return RedirectToRoute("ShoppingCart");

        if (_orderSettings.OnePageCheckoutEnabled)
            return RedirectToRoute("CheckoutOnePage");

        if (
            await _customerService.IsGuestAsync(customer)
            && !_orderSettings.AnonymousCheckoutAllowed
        )
            return Challenge();

        if (b2CUser != null &&
            b2CUser.LastWarehouseCalculationTimeUtc.HasValue &&
            (DateTime.UtcNow - b2CUser.LastWarehouseCalculationTimeUtc.Value).TotalSeconds >= _b2BB2CFeaturesSettings.WarehouseCalculationTimeout)
        {
            _notificationService.ErrorNotification(
                await _localizationService.GetResourceAsync("Plugin.Misc.NopStation.Plugin.B2B.B2BB2CFeatures.ErpCheckout.WarehouseCalculationTimeout")
            );
            return RedirectToRoute("ShoppingCart");
        }

        #endregion

        //Check whether payment workflow is required
        var isPaymentWorkflowRequired =
            await _orderProcessingService.IsPaymentWorkflowRequiredAsync(cart);
        if (!isPaymentWorkflowRequired)
        {
            return RedirectToRoute("CheckoutConfirm");
        }

        //load payment method
        var paymentMethodSystemName = await _genericAttributeService.GetAttributeAsync<string>(
            customer,
            NopCustomerDefaults.SelectedPaymentMethodAttribute,
            store.Id
        );
        var paymentMethod = await _paymentPluginManager.LoadPluginBySystemNameAsync(
            paymentMethodSystemName,
            customer,
            store.Id
        );
        if (paymentMethod == null)
            return RedirectToRoute("CheckoutPaymentMethod");

        var warnings = await paymentMethod.ValidatePaymentFormAsync(form);
        foreach (var warning in warnings)
            ModelState.AddModelError("", warning);

        if (ModelState.IsValid)
        {
            //get payment info
            var paymentInfo = await paymentMethod.GetPaymentInfoAsync(form);
            //set previous order GUID (if exists)
            await _paymentService.GenerateOrderGuidAsync(paymentInfo);

            //session save
            await HttpContext.Session.SetAsync("OrderPaymentInfo", paymentInfo);

            // for non b2b account, display confirm form
            if (b2BAccount == null)
            {
                b2BAccount = await _erpCustomerFunctionalityService.GetActiveErpAccountByCustomerAsync(customer);
                if (b2BAccount == null)
                    return RedirectToRoute("CheckoutConfirm");
            }

            if (paymentMethodSystemName.Equals(B2BB2CFeaturesDefaults.B2BCustomerAccountPluginSystemName) &&
                !b2BAccount.AllowOverspend)
            {
                await _erpAccountCreditSyncFunctionality.LiveErpAccountCreditCheckAsync(b2BAccount);
                var availableCredit = b2BAccount.CreditLimitAvailable;

                var shoppingCartTotalBase = await _orderTotalCalculationService.GetShoppingCartTotalAsync(cart);

                var orderTotal = decimal.Zero;

                if (shoppingCartTotalBase.shoppingCartTotal.HasValue)
                {
                    orderTotal = await _currencyService.ConvertFromPrimaryStoreCurrencyAsync(shoppingCartTotalBase.shoppingCartTotal.Value, await _workContext.GetWorkingCurrencyAsync());
                }
                if (orderTotal > availableCredit)
                {
                    var message = await _localizationService.GetResourceAsync("Plugins.Payments.B2BCustomerAccount.B2BQouteOrder.CreditLimitExceed");
                    _notificationService.Notification(NotifyType.Error, message);
                    return RedirectToRoute("ShoppingCart");
                }
            }

            // for b2b account. place order from here (no need to display confirm page)
            try
            {
                var isQouteOrder = await IsQuoteOrderAsync();
                if (isQouteOrder || await _erpCustomerFunctionalityService.IsCurrentCustomerInB2BQuoteAssistantRoleAsync())
                {
                    //prevent 2 orders being placed within an X seconds time frame
                    if (!await IsMinimumOrderPlacementIntervalValidAsync(customer))
                        throw new Exception(
                            await _localizationService.GetResourceAsync(
                                "Checkout.MinOrderPlacementInterval"
                            )
                        );

                    //place order
                    var processPaymentRequest = new ProcessPaymentRequest();

                    await _paymentService.GenerateOrderGuidAsync(processPaymentRequest);
                    processPaymentRequest.StoreId = store.Id;
                    processPaymentRequest.CustomerId = customer.Id;
                    processPaymentRequest.PaymentMethodSystemName =
                        await _genericAttributeService.GetAttributeAsync<string>(
                            customer,
                            NopCustomerDefaults.SelectedPaymentMethodAttribute,
                            store.Id
                        );

                    await HttpContext.Session.SetAsync("OrderPaymentInfo", processPaymentRequest);

                    var placeOrderResult =
                        await _overriddenOrderProcessingService.PlaceQuoteOrderAsync(
                            processPaymentRequest
                        );
                    if (placeOrderResult.Success)
                    {
                        if (b2BAccount != null && b2BUser != null)
                            await _overriddenOrderProcessingService.PlaceErpOrderAtNopAsync(placeOrderResult.PlacedOrder, ErpOrderType.B2BQuote);

                        //activity log
                        await _customerActivityService.InsertActivityAsync(
                            customer,
                            "PublicStore.PlaceOrder",
                            string.Format(
                                await _localizationService.GetResourceAsync(
                                    "ActivityLog.PublicStore.PlaceOrder"
                                ),
                                placeOrderResult.PlacedOrder.CustomOrderNumber
                            ),
                            placeOrderResult.PlacedOrder
                        );

                        await ClearGenericAttributeForQuoteOrderAsync();

                        return RedirectToRoute(
                            "CheckoutCompleted",
                            new { orderId = placeOrderResult.PlacedOrder.Id }
                        );
                    }
                }
                else
                {
                    //prevent 2 orders being placed within an X seconds time frame
                    if (!await IsMinimumOrderPlacementIntervalValidAsync(customer))
                        throw new Exception(
                            await _localizationService.GetResourceAsync(
                                "Checkout.MinOrderPlacementInterval"
                            )
                        );

                    //place order
                    var processPaymentRequest =
                        await HttpContext.Session.GetAsync<ProcessPaymentRequest>(
                            "OrderPaymentInfo"
                        );
                    if (processPaymentRequest == null)
                    {
                        //Check whether payment workflow is required
                        if (await _orderProcessingService.IsPaymentWorkflowRequiredAsync(cart))
                            return RedirectToRoute("CheckoutPaymentInfo");

                        processPaymentRequest = new ProcessPaymentRequest();
                    }
                    await _paymentService.GenerateOrderGuidAsync(processPaymentRequest);

                    processPaymentRequest.StoreId = store.Id;
                    processPaymentRequest.CustomerId = customer.Id;
                    processPaymentRequest.PaymentMethodSystemName =
                        await _genericAttributeService.GetAttributeAsync<string>(
                            customer,
                            NopCustomerDefaults.SelectedPaymentMethodAttribute,
                            store.Id
                        );

                    await HttpContext.Session.SetAsync("OrderPaymentInfo", processPaymentRequest);

                    var placeOrderResult = await _orderProcessingService.PlaceOrderAsync(
                        processPaymentRequest
                    );
                    if (placeOrderResult.Success)
                    {
                        if (b2BAccount != null && b2BUser != null && b2BUser.Id > 0)
                        {
                            await _overriddenOrderProcessingService.PlaceErpOrderAtNopAsync(placeOrderResult.PlacedOrder, ErpOrderType.B2BSalesOrder);
                        }

                        //activity log
                        await _customerActivityService.InsertActivityAsync(
                            customer,
                            "PublicStore.PlaceOrder",
                            string.Format(
                                await _localizationService.GetResourceAsync(
                                    "ActivityLog.PublicStore.PlaceOrder"
                                ),
                                placeOrderResult.PlacedOrder.CustomOrderNumber
                            ),
                            placeOrderResult.PlacedOrder
                        );

                        await HttpContext.Session.SetAsync<ProcessPaymentRequest>(
                            "OrderPaymentInfo",
                            null
                        );

                        var postProcessPaymentRequest = new PostProcessPaymentRequest
                        {
                            Order = placeOrderResult.PlacedOrder,
                        };
                        await _paymentService.PostProcessPaymentAsync(postProcessPaymentRequest);

                        if (_webHelper.IsRequestBeingRedirected || _webHelper.IsPostBeingDone)
                        {
                            //redirection or POST has been done in PostProcessPayment
                            return Content("Redirected");
                        }

                        return RedirectToRoute(
                            "CheckoutCompleted",
                            new { orderId = placeOrderResult.PlacedOrder.Id }
                        );
                    }
                    else
                    {
                        foreach (var error in placeOrderResult.Errors)
                            warnings.Add(error);
                    }
                }
            }
            catch (Exception exc)
            {
                await _logger.WarningAsync(exc.Message, exc);
                warnings.Add(exc.Message);
            }

            if (warnings.Any())
                _notificationService.WarningNotification(warnings.FirstOrDefault());

            //If we got this far, something failed, display form
            return RedirectToRoute("CheckoutPaymentMethod");
        }

        //If we got this far, something failed, redisplay form
        //model
        var model = await _checkoutModelFactory.PreparePaymentInfoModelAsync(paymentMethod);
        return View("~/Plugins/NopStation.Plugin.B2B.B2BB2CFeatures/Views/Checkout/PaymentInfo.cshtml", model);
    }

    public override async Task<IActionResult> Confirm()
    {
        var customer = await _workContext.GetCurrentCustomerAsync();
        var b2BAccount = await _erpAccountService.GetActiveErpAccountByCustomerIdAsync(customer.Id);

        if (!await IsUserValid(b2BAccount))
            return RedirectToRoute("ShoppingCart");

        var store = await _storeContext.GetCurrentStoreAsync();
        var cart = await _shoppingCartService.GetShoppingCartAsync(
            customer,
            ShoppingCartType.ShoppingCart,
            store.Id
        );

        if (!cart.Any())
            return RedirectToRoute("ShoppingCart");

        if (_orderSettings.OnePageCheckoutEnabled)
            return RedirectToRoute("CheckoutOnePage");

        if (
            await _customerService.IsGuestAsync(customer)
            && !_orderSettings.AnonymousCheckoutAllowed
        )
            return Challenge();

        //model
        var model = await _checkoutModelFactory.PrepareConfirmOrderModelAsync(cart);
        return View(model);
    }

    [ValidateCaptcha]
    [HttpPost, ActionName("Confirm")]
    public override async Task<IActionResult> ConfirmOrder(bool captchaValid)
    {

        var (b2BAccount, b2BUser, b2CUser) = await GetB2BAccountAndUserOfCurrentCustomerAsync();

        if (!await IsUserValid(b2BAccount))
            return RedirectToRoute("ShoppingCart");

        var customer = await _workContext.GetCurrentCustomerAsync();
        var store = await _storeContext.GetCurrentStoreAsync();

        var cart = await _shoppingCartService.GetShoppingCartAsync(
            customer,
            ShoppingCartType.ShoppingCart,
            store.Id
        );

        if (!cart.Any())
            return RedirectToRoute("ShoppingCart");

        if (_orderSettings.OnePageCheckoutEnabled)
            return RedirectToRoute("CheckoutOnePage");

        if (
            await _customerService.IsGuestAsync(customer)
            && !_orderSettings.AnonymousCheckoutAllowed
        )
            return Challenge();

        if (b2CUser != null &&
            b2CUser.LastWarehouseCalculationTimeUtc.HasValue &&
            (DateTime.UtcNow - b2CUser.LastWarehouseCalculationTimeUtc.Value).TotalSeconds >= _b2BB2CFeaturesSettings.WarehouseCalculationTimeout)
        {
            _notificationService.ErrorNotification(
                await _localizationService.GetResourceAsync("Plugin.Misc.NopStation.Plugin.B2B.B2BB2CFeatures.ErpCheckout.WarehouseCalculationTimeout")
            );
            return RedirectToRoute("ShoppingCart");
        }

        //model
        var model = await _checkoutModelFactory.PrepareConfirmOrderModelAsync(cart);

        var isCaptchaSettingEnabled =
            await _customerService.IsGuestAsync(customer)
            && _captchaSettings.Enabled
            && _captchaSettings.ShowOnCheckoutPageForGuests;

        //captcha validation for guest customers
        if (isCaptchaSettingEnabled && !captchaValid)
        {
            model.Warnings.Add(
                await _localizationService.GetResourceAsync("Common.WrongCaptchaMessage")
            );
            return View(model);
        }

        // for b2c user, we need to check if warehouse calculation is required and if it has timed out
        if (b2BAccount != null && b2CUser != null)
        {
            if (b2CUser.LastWarehouseCalculationTimeUtc.HasValue && (DateTime.UtcNow - b2CUser.LastWarehouseCalculationTimeUtc.Value).TotalSeconds >= _b2BB2CFeaturesSettings.WarehouseCalculationTimeout)
            {
                _notificationService.ErrorNotification(await _localizationService.GetResourceAsync("Plugins.Payment.B2BCustomerAccount.B2BCheckout.B2Corder.WarehouseCalculationTimeout"));
                return RedirectToRoute("ShoppingCart");
            }
        }

        try
        {
            //prevent 2 orders being placed within an X seconds time frame
            if (!await IsMinimumOrderPlacementIntervalValidAsync(customer))
                throw new Exception(
                    await _localizationService.GetResourceAsync(
                        "Checkout.MinOrderPlacementInterval"
                    )
                );

            var isQuoteOrder = await IsQuoteOrderAsync();

            if (isQuoteOrder || await _erpCustomerFunctionalityService.IsCurrentCustomerInB2BQuoteAssistantRoleAsync())
            {
                //place order
                var processPaymentRequest = new ProcessPaymentRequest();
                if (processPaymentRequest == null)
                {
                    //Check whether payment workflow is required
                    if (await _orderProcessingService.IsPaymentWorkflowRequiredAsync(cart))
                        return RedirectToRoute("CheckoutPaymentInfo");

                    processPaymentRequest = new ProcessPaymentRequest();
                }
                await _paymentService.GenerateOrderGuidAsync(processPaymentRequest);
                processPaymentRequest.StoreId = store.Id;
                processPaymentRequest.CustomerId = customer.Id;
                processPaymentRequest.PaymentMethodSystemName = await _genericAttributeService.GetAttributeAsync<string>(customer,
                    NopCustomerDefaults.SelectedPaymentMethodAttribute, store.Id);
                await HttpContext.Session.SetAsync("OrderPaymentInfo", processPaymentRequest);
                var placeOrderResult = await _overriddenOrderProcessingService.PlaceQuoteOrderAsync(processPaymentRequest);

                if (placeOrderResult.Success)
                {
                    if (b2BAccount != null && b2BUser != null)
                        await _overriddenOrderProcessingService.PlaceErpOrderAtNopAsync(
                            placeOrderResult.PlacedOrder,
                            ErpOrderType.B2BQuote
                        );

                    await ClearGenericAttributeForQuoteOrderAsync();

                    await _erpLogsService.InformationAsync($"Quote order placed successfully! Order-Id: {placeOrderResult.PlacedOrder.Id}, Erp Order Number: {placeOrderResult.PlacedOrder.CustomOrderNumber}", ErpSyncLevel.Order, customer: await _workContext.GetCurrentCustomerAsync());

                    //activity log
                    await _customerActivityService.InsertActivityAsync(
                        customer,
                        "PublicStore.PlaceOrder",
                        string.Format(
                            await _localizationService.GetResourceAsync(
                                "ActivityLog.PublicStore.PlaceOrder"
                            ),
                            placeOrderResult.PlacedOrder.CustomOrderNumber
                        ),
                        placeOrderResult.PlacedOrder
                    );

                    return RedirectToRoute(
                        "CheckoutCompleted",
                        new { orderId = placeOrderResult.PlacedOrder.Id }
                    );
                }

                foreach (var error in placeOrderResult.Errors)
                    model.Warnings.Add(error);
            }
            else
            {
                var processPaymentRequest =
                    await HttpContext.Session.GetAsync<ProcessPaymentRequest>("OrderPaymentInfo");
                if (processPaymentRequest == null)
                {
                    if (await _orderProcessingService.IsPaymentWorkflowRequiredAsync(cart))
                    {
                        throw new Exception("Payment information is not entered");
                    }

                    processPaymentRequest = new ProcessPaymentRequest();
                }
                await _paymentService.GenerateOrderGuidAsync(processPaymentRequest);
                processPaymentRequest.StoreId = store.Id;
                processPaymentRequest.CustomerId = customer.Id;
                processPaymentRequest.PaymentMethodSystemName = await _genericAttributeService.GetAttributeAsync<string>(customer,
                    NopCustomerDefaults.SelectedPaymentMethodAttribute, store.Id);
                await HttpContext.Session.SetAsync("OrderPaymentInfo", processPaymentRequest);

                if (string.IsNullOrWhiteSpace(processPaymentRequest.PaymentMethodSystemName))
                {
                    //payment method could be null if order total is 0
                    //success
                    model.Warnings.Add(await _localizationService.GetResourceAsync("Checkout.NoPaymentMethods"));
                    return RedirectToRoute("CheckoutPaymentMethod");
                }

                var placeOrderResult = await _orderProcessingService.PlaceOrderAsync(processPaymentRequest);
                if (placeOrderResult.Success)
                {
                    if (b2BAccount != null && b2BUser != null && b2BUser.Id > 0)
                    {
                        await _overriddenOrderProcessingService.PlaceErpOrderAtNopAsync(placeOrderResult.PlacedOrder, ErpOrderType.B2BSalesOrder);
                    }

                    //activity log
                    await _customerActivityService.InsertActivityAsync(
                        customer,
                        "PublicStore.PlaceOrder",
                        string.Format(
                            await _localizationService.GetResourceAsync(
                                "ActivityLog.PublicStore.PlaceOrder"
                            ),
                            placeOrderResult.PlacedOrder.CustomOrderNumber
                        ),
                        placeOrderResult.PlacedOrder
                    );

                    await HttpContext.Session.SetAsync<ProcessPaymentRequest>(
                        "OrderPaymentInfo",
                        null
                    );
                    var postProcessPaymentRequest = new PostProcessPaymentRequest
                    {
                        Order = placeOrderResult.PlacedOrder,
                    };

                    var paymentMethod = await _paymentPluginManager.LoadPluginBySystemNameAsync(
                        placeOrderResult.PlacedOrder.PaymentMethodSystemName,
                        customer,
                        store.Id
                    );

                    if (paymentMethod is not null && paymentMethod.PaymentMethodType == PaymentMethodType.Redirection)
                    {
                        //Redirection will not work because it's AJAX request.
                        //That's why we don't process it here (we redirect a user to another page where he'll be redirected)

                        //redirect
                        return Json(
                            new
                            {
                                redirect = $"{_webHelper.GetStoreLocation()}checkout/OpcCompleteRedirectionPayment",
                            }
                        );
                    }

                    await _paymentService.PostProcessPaymentAsync(postProcessPaymentRequest);

                    if (_webHelper.IsRequestBeingRedirected || _webHelper.IsPostBeingDone)
                    {
                        //redirection or POST has been done in PostProcessPayment
                        return Content(
                            await _localizationService.GetResourceAsync("Checkout.RedirectMessage")
                        );
                    }
                    return RedirectToRoute(
                        "CheckoutCompleted",
                        new { orderId = placeOrderResult.PlacedOrder.Id }
                    );
                }

                //error
                foreach (var error in placeOrderResult.Errors)
                    model.Warnings.Add(error);
            }
        }
        catch (Exception ex)
        {
            await _logger.WarningAsync(ex.Message, ex);
            model.Warnings.Add(ex.Message);
        }

        //If we got this far, something failed, redisplay form
        return View(model);
    }

    [HttpPost, ActionName("ErpShipToAddressPopUp")]
    [FormValueRequired("nextstep")]
    public async Task<IActionResult> ErpShipToAddressPopUp(
        ErpShipToAddressModelForCheckout modelShipToAddress,
        IFormCollection form
    )
    {
        var (erpAccount, b2BUser, b2CUser) = await GetB2BAccountAndUserOfCurrentCustomerAsync();
        if (!await IsUserValid(erpAccount))
            return RedirectToRoute("ShoppingCart");

        var user = b2BUser ?? b2CUser;
        var currentCustomer = await _workContext.GetCurrentCustomerAsync();
        var currentStore = await _storeContext.GetCurrentStoreAsync();
        var cart = await _shoppingCartService.GetShoppingCartAsync(
            currentCustomer,
            ShoppingCartType.ShoppingCart,
            currentStore.Id
        );

        if (!cart.Any())
            return RedirectToRoute("ShoppingCart");

        if (_orderSettings.OnePageCheckoutEnabled)
            return RedirectToRoute("CheckoutOnePage");

        if (
            await _customerService.IsGuestAsync(currentCustomer)
            && !_orderSettings.AnonymousCheckoutAllowed
        )
            return Challenge();

        if (!await _shoppingCartService.ShoppingCartRequiresShippingAsync(cart))
        {
            currentCustomer.ShippingAddressId = null;
            await _customerService.UpdateCustomerAsync(currentCustomer);
            return RedirectToRoute("CheckoutShippingMethod");
        }

        if (_shippingSettings.AllowPickupInStore)
        {
            //set value indicating that "pick up in store" option has not been chosen
            await _genericAttributeService.SaveAttributeAsync<PickupPoint>(
                currentCustomer,
                NopCustomerDefaults.SelectedPickupPointAttribute,
                null,
                currentStore.Id
            );
        }

        var erpShipToAddress = await _erpShipToAddressService.GetErpShipToAddressByIdWithActiveAsync(user.ErpShipToAddressId);

        if (modelShipToAddress.IsFullLoadRequired)
        {
            (var minDeliveryDate, var _) = await _erpCustomerFunctionalityService.GetMinimumAndMaximumDeliveryDateForShippingAddress();
            modelShipToAddress.DeliveryDate = minDeliveryDate.Date;
        }
        // validate DeliveryDate
        else if (!modelShipToAddress.ErpToDetermineDate)
        {
            (var minDeliveryDate, var maxDeliveryDate) =
                await _erpCustomerFunctionalityService.GetMinimumAndMaximumDeliveryDateForShippingAddress();

            if (modelShipToAddress.DeliveryDate < minDeliveryDate.Date || modelShipToAddress.DeliveryDate > maxDeliveryDate)
                ModelState.AddModelError("DeliveryDate", "Please provide a valid delivery date");

        }

        if (ModelState.IsValid && erpShipToAddress != null && modelShipToAddress.Id > 0)
        {
            //check if ship To address related b2b account id is matching
            if (modelShipToAddress.ErpAccountId != erpAccount.Id)
                throw new Exception("Ship to Address can't be loaded");

            // 1593 || customer ref set at generic attribute
            if (!string.IsNullOrEmpty(modelShipToAddress.CustomerReference?.Trim()))
            {
                await _genericAttributeService.SaveAttributeAsync(
                    currentCustomer,
                    B2BB2CFeaturesDefaults.B2BCustomerReferenceAsPO,
                    modelShipToAddress.CustomerReference?.Trim(),
                    currentStore.Id
                );
            }

            if (modelShipToAddress.AllowEdit)
            {
                var lastShippingB2BShipToAddress =
                    await _erpShipToAddressService.GetErpShipToAddressByIdWithActiveAsync(
                        user.ShippingErpShipToAddressId
                    );
                var erpAccShipToAddressMap = await _erpShipToAddressService.GetErpShipToAddressErpAccountMapByErpShipToAddressIdAsync(user.ShippingErpShipToAddressId);

                // if current b2b user don't have b2b ship to address or
                // ship to address used in the last order or
                // ship to address created by admin
                // then we will add new ship to address,
                // otherwise edit existing one
                if (lastShippingB2BShipToAddress == null ||
                    lastShippingB2BShipToAddress.OrderId > 0 ||
                    erpAccShipToAddressMap == null ||
                    erpAccShipToAddressMap.ErpShipToAddressCreatedByType == ErpShipToAddressCreatedByType.Admin)
                {
                    var currentShiptoAddress = await _erpShipToAddressService.GetErpShipToAddressByIdWithActiveAsync(modelShipToAddress.Id) ?? throw new Exception("Ship to Address can't be loaded");

                    var address = new Address
                    {
                        Email = currentCustomer.Email,
                        FirstName = currentCustomer.FirstName,
                        LastName = currentCustomer.LastName,
                        Company = modelShipToAddress.ShipToName,
                        CountryId = modelShipToAddress.CountryId,
                        StateProvinceId = modelShipToAddress.StateProvinceId,
                        City = modelShipToAddress.City,
                        Address1 = modelShipToAddress.Address1,
                        Address2 = modelShipToAddress.Address2,
                        ZipPostalCode = modelShipToAddress.ZipPostalCode,
                        PhoneNumber = modelShipToAddress.PhoneNumber,
                    };
                    await _addressService.InsertAddressAsync(address);

                    var b2BShipToAddress = new ErpShipToAddress
                    {
                        ShipToCode = currentShiptoAddress.ShipToCode,
                        ProvinceCode = currentShiptoAddress.ProvinceCode,
                        DeliveryNotes = currentShiptoAddress.DeliveryNotes,
                        EmailAddresses = currentShiptoAddress.EmailAddresses,
                        RepNumber = currentShiptoAddress.RepNumber,
                        RepFullName = currentShiptoAddress.RepFullName,
                        RepPhoneNumber = currentShiptoAddress.RepPhoneNumber,
                        RepEmail = currentShiptoAddress.RepEmail,
                        LastShipToAddressSyncDate = currentShiptoAddress.LastShipToAddressSyncDate,
                        DistanceToNearestWareHouse = currentShiptoAddress.DistanceToNearestWareHouse,
                        Comment = currentShiptoAddress.Comment,
                        Latitude = currentShiptoAddress.Latitude,
                        Longitude = currentShiptoAddress.Longitude,
                        NearestWareHouseId = currentShiptoAddress.NearestWareHouseId,
                        DeliveryOptionId = currentShiptoAddress.DeliveryOptionId,
                        IsActive = currentShiptoAddress.IsActive,
                        IsDeleted = currentShiptoAddress.IsDeleted,
                    };

                    b2BShipToAddress.AddressId = address.Id;
                    b2BShipToAddress.ShipToName = modelShipToAddress.ShipToName;
                    b2BShipToAddress.Suburb = modelShipToAddress.Suburb?.ToUpper();
                    b2BShipToAddress.CreatedOnUtc = DateTime.UtcNow;
                    b2BShipToAddress.CreatedById = currentCustomer.Id;
                    b2BShipToAddress.UpdatedOnUtc = DateTime.UtcNow;
                    b2BShipToAddress.UpdatedById = currentCustomer.Id;

                    await _erpShipToAddressService.InsertErpShipToAddressAsync(b2BShipToAddress);
                    await _erpShipToAddressService.InsertErpShipToAddressErpAccountMapAsync
                        (erpAccount, b2BShipToAddress, ErpShipToAddressCreatedByType.User);

                    await _genericAttributeService.SaveAttributeAsync<int?>(currentCustomer, B2BB2CFeaturesDefaults.ShippingAddressModifiedIdInCheckoutAttribute, b2BShipToAddress.Id, currentStore.Id);
                    user.ShippingErpShipToAddressId = b2BShipToAddress.Id;
                    await _erpNopUserService.UpdateErpNopUserAsync(user);

                    currentCustomer.ShippingAddressId = address.Id;
                    await _customerService.UpdateCustomerAsync(currentCustomer);
                }
                else
                {
                    var address = await _addressService.GetAddressByIdAsync(
                        lastShippingB2BShipToAddress.AddressId
                    );
                    address.Email = currentCustomer.Email;
                    address.FirstName = currentCustomer.FirstName;
                    address.LastName = currentCustomer.LastName;
                    address.Company = modelShipToAddress.ShipToName;
                    address.CountryId = modelShipToAddress.CountryId;
                    address.StateProvinceId = modelShipToAddress.StateProvinceId;
                    address.City = modelShipToAddress.City;
                    address.Address1 = modelShipToAddress.Address1;
                    address.Address2 = modelShipToAddress.Address2;
                    address.ZipPostalCode = modelShipToAddress.ZipPostalCode;
                    address.PhoneNumber = modelShipToAddress.PhoneNumber;

                    await _addressService.UpdateAddressAsync(address);

                    lastShippingB2BShipToAddress.Suburb = modelShipToAddress.Suburb;
                    lastShippingB2BShipToAddress.ShipToName = modelShipToAddress.ShipToName;
                    lastShippingB2BShipToAddress.UpdatedOnUtc = DateTime.UtcNow;
                    lastShippingB2BShipToAddress.UpdatedById = currentCustomer.Id;

                    await _erpShipToAddressService.UpdateErpShipToAddressAsync(
                        lastShippingB2BShipToAddress
                    );

                    await _genericAttributeService.SaveAttributeAsync<int?>(
                        currentCustomer,
                        B2BB2CFeaturesDefaults.ShippingAddressModifiedIdInCheckoutAttribute,
                        lastShippingB2BShipToAddress.Id,
                        currentStore.Id
                    );
                    currentCustomer.ShippingAddressId = address.Id;
                    await _customerService.UpdateCustomerAsync(currentCustomer);
                }

                await _genericAttributeService.SaveAttributeAsync(
                    currentCustomer,
                    B2BB2CFeaturesDefaults.IsShippingAddressModifiedInCheckoutAttribute,
                    true,
                    currentStore.Id
                );
            }
            else
            {
                user.ShippingErpShipToAddressId = user.ErpShipToAddressId;
                await _erpNopUserService.UpdateErpNopUserAsync(user);

                var address = await _addressService.GetAddressByIdAsync(erpShipToAddress.AddressId);

                address.Email = currentCustomer.Email;
                address.FirstName = currentCustomer.FirstName;
                address.LastName = currentCustomer.LastName;
                address.Company = erpShipToAddress.ShipToName;

                currentCustomer.ShippingAddressId = address.Id;
                await _customerService.UpdateCustomerAsync(currentCustomer);
                await _genericAttributeService.SaveAttributeAsync(
                    currentCustomer,
                    B2BB2CFeaturesDefaults.IsShippingAddressModifiedInCheckoutAttribute,
                    false,
                    currentStore.Id
                );
            }

            return RedirectToRoute("CheckoutShippingAddress");
        }

        if (user.ErpUserType == ErpUserType.B2BUser)
        {
            var b2BShipToAddressModel =
                await _erpCheckoutModelFactory.PrepareCheckoutB2BShippingAddressModelAsync(
                    cart,
                    user,
                    erpAccount
                );
            return View(
                "~/Plugins/NopStation.Plugin.B2B.B2BB2CFeatures/Views/ErpCheckout/ShippingAddress.cshtml",
                b2BShipToAddressModel
            );
        }

        var b2CShipToAddressModel =
            await _erpCheckoutModelFactory.PrepareCheckoutB2CShippingAddressModelAsync(
                cart,
                user,
                erpAccount
            );
        return View(
            "~/Plugins/NopStation.Plugin.B2B.B2BB2CFeatures/Views/ErpCheckout/ShippingAddress.cshtml",
            b2CShipToAddressModel
        );
    }

    #endregion

    #region Methods (One page checkout)

    protected override async Task<JsonResult> OpcLoadStepAfterShippingAddress(
        IList<ShoppingCartItem> cart
    )
    {
        var customer = await _workContext.GetCurrentCustomerAsync();
        var shippingMethodModel = await _checkoutModelFactory.PrepareShippingMethodModelAsync(cart, await _customerService.GetCustomerShippingAddressAsync(customer));

        if (_shippingSettings.BypassShippingMethodSelectionIfOnlyOne &&
            shippingMethodModel.ShippingMethods.Count == 1)
        {
            var store = await _storeContext.GetCurrentStoreAsync();
            await _genericAttributeService.SaveAttributeAsync(
                customer,
                NopCustomerDefaults.SelectedShippingOptionAttribute,
                shippingMethodModel.ShippingMethods[0].ShippingOption,
                store.Id);

            return await OpcLoadStepAfterShippingMethod(cart);
        }

        return Json(
            new
            {
                update_section = new UpdateSectionJsonModel
                {
                    name = "shipping-method",
                    html = await RenderPartialViewToStringAsync(
                        "~/Views/Checkout/OpcShippingMethods.cshtml",
                        shippingMethodModel
                    ),
                },
                goto_section = "shipping_method",
            }
        );
    }

    protected override async Task<JsonResult> OpcLoadStepAfterPaymentMethod(
        IPaymentMethod paymentMethod,
        IList<ShoppingCartItem> cart
    )
    {
        var customer = await _workContext.GetCurrentCustomerAsync();
        var b2BAccount = await _erpAccountService.GetActiveErpAccountByCustomerIdAsync(customer.Id);

        await _erpAccountCreditSyncFunctionality.LiveErpAccountCreditCheckAsync(b2BAccount);

        if (paymentMethod.SkipPaymentInfo ||
            (paymentMethod.PaymentMethodType == PaymentMethodType.Redirection &&
            _paymentSettings.SkipPaymentInfoStepForRedirectionPaymentMethods))
        {
            var paymentInfo = new ProcessPaymentRequest();

            await HttpContext.Session.SetAsync("OrderPaymentInfo", paymentInfo);

            var confirmOrderModel = await _checkoutModelFactory.PrepareConfirmOrderModelAsync(cart);
            return Json(
                new
                {
                    update_section = new UpdateSectionJsonModel
                    {
                        name = "confirm-order",
                        html = await RenderPartialViewToStringAsync(
                            "~/Views/Checkout/OpcConfirmOrder.cshtml",
                            confirmOrderModel
                        ),
                    },
                    goto_section = "confirm_order",
                }
            );
        }

        var paymenInfoModel = await _checkoutModelFactory.PreparePaymentInfoModelAsync(
            paymentMethod
        );
        return Json(
            new
            {
                update_section = new UpdateSectionJsonModel
                {
                    name = "payment-info",
                    html = await RenderPartialViewToStringAsync(
                        "~/Views/Checkout/OpcPaymentInfo.cshtml",
                        paymenInfoModel
                    ),
                },
                goto_section = "payment_info",
            }
        );
    }

    protected override async Task<JsonResult> OpcLoadStepAfterShippingMethod(
        IList<ShoppingCartItem> cart
    )
    {
        var isQuoteOrder = await IsQuoteOrderAsync();
        var store = await _storeContext.GetCurrentStoreAsync();
        var isPaymentWorkflowRequired = await _orderProcessingService.IsPaymentWorkflowRequiredAsync(cart, false);
        var customer = await _workContext.GetCurrentCustomerAsync();
        var b2BAccount = await _erpAccountService.GetActiveErpAccountByCustomerIdAsync(customer.Id);

        await _erpAccountCreditSyncFunctionality.LiveErpAccountCreditCheckAsync(b2BAccount);

        if (isPaymentWorkflowRequired && !isQuoteOrder && !await _erpCustomerFunctionalityService.IsCurrentCustomerInB2BQuoteAssistantRoleAsync())
        {
            var filterByCountryId = 0;
            if (_addressSettings.CountryEnabled)
            {
                filterByCountryId =
                    (await _customerService.GetCustomerBillingAddressAsync(customer))?.CountryId
                    ?? 0;
            }

            var paymentMethodModel = await _checkoutModelFactory.PreparePaymentMethodModelAsync(
                cart,
                filterByCountryId
            );

            if (_paymentSettings.BypassPaymentMethodSelectionIfOnlyOne &&
                paymentMethodModel.PaymentMethods.Count == 1 &&
                !paymentMethodModel.DisplayRewardPoints)
            {
                var selectedPaymentMethodSystemName = paymentMethodModel.PaymentMethods[0].PaymentMethodSystemName;
                await _genericAttributeService.SaveAttributeAsync(customer,
                    NopCustomerDefaults.SelectedPaymentMethodAttribute,
                    selectedPaymentMethodSystemName,
                    store.Id
                );

                var paymentMethodInst = await _paymentPluginManager.LoadPluginBySystemNameAsync(
                    selectedPaymentMethodSystemName,
                    customer,
                    store.Id
                );
                if (!_paymentPluginManager.IsPluginActive(paymentMethodInst))
                    throw new Exception("Selected payment method can't be parsed");

                return await OpcLoadStepAfterPaymentMethod(paymentMethodInst, cart);
            }

            return Json(
                new
                {
                    update_section = new UpdateSectionJsonModel
                    {
                        name = "payment-method",
                        html = await RenderPartialViewToStringAsync(
                            "~/Views/Checkout/OpcPaymentMethods.cshtml",
                            paymentMethodModel
                        ),
                    },
                    goto_section = "payment_method",
                }
            );
        }

        await _genericAttributeService.SaveAttributeAsync<string>(
            customer,
            NopCustomerDefaults.SelectedPaymentMethodAttribute,
            null,
            store.Id
        );

        var confirmOrderModel = await _checkoutModelFactory.PrepareConfirmOrderModelAsync(cart);
        return Json(
            new
            {
                update_section = new UpdateSectionJsonModel
                {
                    name = "confirm-order",
                    html = await RenderPartialViewToStringAsync(
                        "~/Views/Checkout/OpcConfirmOrder.cshtml",
                        confirmOrderModel
                    ),
                },
                goto_section = "confirm_order",
            }
        );
    }

    public override async Task<IActionResult> OnePageCheckout()
    {
        var customer = await _workContext.GetCurrentCustomerAsync();
        var store = await _storeContext.GetCurrentStoreAsync();
        var cart = await _shoppingCartService.GetShoppingCartAsync(
            customer,
            ShoppingCartType.ShoppingCart,
            store.Id
        );

        var (erpAccount, b2BUser, b2CUser) = await GetB2BAccountAndUserOfCurrentCustomerAsync();
        if (!await IsUserValid(erpAccount))
            return RedirectToRoute("ShoppingCart");

        if (!cart.Any())
            return RedirectToRoute("ShoppingCart");

        if (!_orderSettings.OnePageCheckoutEnabled)
            return RedirectToRoute("Checkout");

        if (
            await _customerService.IsGuestAsync(customer)
            && !_orderSettings.AnonymousCheckoutAllowed
        )
            return Challenge();

        if (erpAccount != null && b2CUser != null)
        {
            var erpShipToAddress = await _erpShipToAddressService.GetErpShipToAddressByIdWithActiveAsync(b2CUser.ErpShipToAddressId);
            if (erpShipToAddress != null)
            {
                var erpOnePageCheckoutModel =
                    await _erpCheckoutModelFactory.PrepareB2COnePageCheckoutModelAsync(
                        cart,
                        b2CUser
                    );
                return View(
                    "~/Plugins/NopStation.Plugin.B2B.B2BB2CFeatures/Views/ErpCheckout/OnePageCheckout.cshtml",
                    erpOnePageCheckoutModel
                );
            }
        }
        else if (erpAccount != null && b2BUser != null)
        {
            var erpShipToAddress = await _erpShipToAddressService.GetErpShipToAddressByIdWithActiveAsync(b2BUser.ErpShipToAddressId);
            if (erpShipToAddress != null)
            {
                var erpOnePageCheckoutModel =
                    await _erpCheckoutModelFactory.PrepareB2BOnePageCheckoutModelAsync(
                        cart,
                        b2BUser
                    );
                return View(
                    "~/Plugins/NopStation.Plugin.B2B.B2BB2CFeatures/Views/ErpCheckout/OnePageCheckout.cshtml",
                    erpOnePageCheckoutModel
                );
            }
        }

        var model = await _checkoutModelFactory.PrepareOnePageCheckoutModelAsync(cart);
        return View(model);
    }

    [HttpPost]
    public virtual async Task<IActionResult> OpcSaveErpBilling(
        CheckoutErpBillingAddressModel model,
        IFormCollection form
    )
    {
        try
        {
            var customer = await _workContext.GetCurrentCustomerAsync();
            var store = await _storeContext.GetCurrentStoreAsync();
            var cart = await _shoppingCartService.GetShoppingCartAsync(
                customer,
                ShoppingCartType.ShoppingCart,
                store.Id
            );

            var (erpAccount, b2BUser, b2CUser) = await GetB2BAccountAndUserOfCurrentCustomerAsync();
            if (!await IsUserValid(erpAccount))
                return RedirectToRoute("ShoppingCart");

            if (!cart.Any())
                throw new Exception("Your cart is empty");

            if (!_orderSettings.OnePageCheckoutEnabled)
                throw new Exception("One page checkout is disabled");

            if (
                await _customerService.IsGuestAsync(customer)
                && !_orderSettings.AnonymousCheckoutAllowed
            )
                throw new Exception("Anonymous checkout is not allowed");

            _ = int.TryParse(form["billing_address_id"], out var billingAddressId);

            var modelBillingAddress = model.ErpBillingAddress;

            if (
                erpAccount != null
                && erpAccount.BillingAddressId.HasValue
                && erpAccount.BillingAddressId.Value > 0
            )
            {
                if (modelBillingAddress.Id != erpAccount.BillingAddressId.Value)
                    throw new Exception("Billing Address can't be loaded");

                var b2bBillingAddress =
                    await _addressService.GetAddressByIdAsync(erpAccount.BillingAddressId.Value)
                    ?? new Address();

                var customerBillingAddress =
                    await _addressService.GetAddressByIdAsync(customer.BillingAddressId ?? 0)
                    ?? new Address();
                if (
                    customerBillingAddress.Id == 0
                    || !await _addressService.IsAddressValidAsync(customerBillingAddress)
                )
                {
                    var newBillingAddress = b2bBillingAddress;
                    newBillingAddress.FirstName = customer.FirstName;
                    newBillingAddress.LastName = customer.LastName;
                    newBillingAddress.Email = customer.Email;

                    await _addressService.InsertAddressAsync(newBillingAddress);
                    await _customerService.InsertCustomerAddressAsync(customer, newBillingAddress);

                    customer.BillingAddressId = newBillingAddress.Id;
                    await _customerService.UpdateCustomerAsync(customer);
                }
                else
                {
                    customerBillingAddress.FirstName = customer.FirstName;
                    customerBillingAddress.LastName = customer.LastName;
                    customerBillingAddress.Email = customer.Email;
                    customerBillingAddress.Company = b2bBillingAddress.Company;
                    customerBillingAddress.CountryId = b2bBillingAddress.CountryId;
                    customerBillingAddress.StateProvinceId = b2bBillingAddress.StateProvinceId;
                    customerBillingAddress.County = b2bBillingAddress.County;
                    customerBillingAddress.City = b2bBillingAddress.City;
                    customerBillingAddress.Address1 = b2bBillingAddress.Address1;
                    customerBillingAddress.Address2 = b2bBillingAddress.Address2;
                    customerBillingAddress.ZipPostalCode = b2bBillingAddress.ZipPostalCode;
                    customerBillingAddress.PhoneNumber = b2bBillingAddress.PhoneNumber;
                    customerBillingAddress.FaxNumber = b2bBillingAddress.FaxNumber;
                    customerBillingAddress.CustomAttributes = b2bBillingAddress.CustomAttributes;

                    await _addressService.UpdateAddressAsync(customerBillingAddress);
                }

                if (await _shoppingCartService.ShoppingCartRequiresShippingAsync(cart))
                {
                    var address = await _addressService.GetAddressByIdAsync(
                        customer.BillingAddressId ?? 0
                    );

                    var country = await _countryService.GetCountryByAddressAsync(address);
                    var shippingAllowed =
                        !_addressSettings.CountryEnabled || (country?.AllowsShipping ?? false);
                    if (
                        _shippingSettings.ShipToSameAddress
                        && model.ShipToSameAddress
                        && shippingAllowed
                    )
                    {
                        customer.ShippingAddressId = address.Id;
                        await _customerService.UpdateCustomerAsync(customer);
                        await _customerService.InsertCustomerAddressAsync(customer, address);
                        await _genericAttributeService.SaveAttributeAsync<ShippingOption>(
                            customer,
                            NopCustomerDefaults.SelectedShippingOptionAttribute,
                            null,
                            store.Id
                        );
                        await _genericAttributeService.SaveAttributeAsync<PickupPoint>(
                            customer,
                            NopCustomerDefaults.SelectedPickupPointAttribute,
                            null,
                            store.Id
                        );
                        return await OpcLoadStepAfterShippingAddress(cart);
                    }

                    var shippingAddressModel = await _erpCheckoutModelFactory.PrepareShippingAddressModelAsync(b2BUser ?? b2CUser, erpAccount, prePopulateNewAddressWithCustomerFields: true);
                    if (shippingAddressModel.Warnings.Any())
                    {
                        _notificationService.ErrorNotification(
                            shippingAddressModel.Warnings.FirstOrDefault()
                        );
                        return Json(new { redirect = Url.RouteUrl("ShoppingCart") });
                    }

                    if (
                        shippingAddressModel.ExistingErpShipToAddresses == null
                        || shippingAddressModel.ExistingErpShipToAddresses.Count == 0
                    )
                    {
                        await _erpLogsService.ErrorAsync(
                            await _localizationService.GetResourceAsync(
                                "Plugin.Misc.NopStation.B2BB2CFeaturesPlugins.ErpCheckout.ShipToAddress.NotFound"
                            ),
                            ErpSyncLevel.Order,
                            null,
                            customer
                        );
                        _notificationService.ErrorNotification(
                            await _localizationService.GetResourceAsync(
                                "Plugin.Misc.NopStation.B2BB2CFeaturesPlugins.ErpCheckout.ShipToAddress.NotFound"
                            )
                        );
                        return Json(new { redirect = Url.RouteUrl("ShoppingCart") });
                    }

                    return Json(
                        new
                        {
                            update_section = new UpdateSectionJsonModel
                            {
                                name = "shipping",
                                html = await RenderPartialViewToStringAsync(
                                    "~/Plugins/NopStation.Plugin.B2B.B2BB2CFeatures/Views/ErpCheckout/OpcShippingAddress.cshtml",
                                    shippingAddressModel
                                ),
                            },
                            goto_section = "shipping",
                        }
                    );
                }

                await _genericAttributeService.SaveAttributeAsync<ShippingOption>(
                    customer,
                    NopCustomerDefaults.SelectedShippingOptionAttribute,
                    null,
                    store.Id
                );

                return await OpcLoadStepAfterShippingMethod(cart);
            }
            else
            {
                var billingAddressModel =
                    await _checkoutModelFactory.PrepareBillingAddressModelAsync(cart);
                return Json(
                    new
                    {
                        update_section = new UpdateSectionJsonModel
                        {
                            name = "billing",
                            html = await RenderPartialViewToStringAsync(
                                "~/Views/Checkout/OpcBillingAddress.cshtml",
                                billingAddressModel
                            ),
                        },
                    }
                );
            }
        }
        catch (Exception exc)
        {
            await _logger.WarningAsync(
                exc.Message,
                exc,
                await _workContext.GetCurrentCustomerAsync()
            );
            return Json(new { error = 1, message = exc.Message });
        }
    }

    [HttpPost]
    public virtual async Task<IActionResult> OpcSaveShippingAddress(
        CheckoutErpShippingAddressModel model,
        IFormCollection form
    )
    {
        try
        {
            var customer = await _workContext.GetCurrentCustomerAsync();
            var store = await _storeContext.GetCurrentStoreAsync();
            var cart = await _shoppingCartService.GetShoppingCartAsync(
                customer,
                ShoppingCartType.ShoppingCart,
                store.Id
            );

            var (erpAccount, b2BUser, b2CUser) = await GetB2BAccountAndUserOfCurrentCustomerAsync();
            if (!await IsUserValid(erpAccount))
                return RedirectToRoute("ShoppingCart");

            if (!cart.Any())
                throw new Exception("Your cart is empty");

            if (!_orderSettings.OnePageCheckoutEnabled)
                throw new Exception("One page checkout is disabled");

            if (
                await _customerService.IsGuestAsync(customer)
                && !_orderSettings.AnonymousCheckoutAllowed
            )
                throw new Exception("Anonymous checkout is not allowed");

            if (!await _shoppingCartService.ShoppingCartRequiresShippingAsync(cart))
                throw new Exception("Shipping is not required");

            var specialInstruction = form["CheckoutErpShipToAddress.SpecialInstructions"];
            var cusReference = form["CheckoutErpShipToAddress.CustomerReference"];

            if (
                _shippingSettings.AllowPickupInStore /*&& !_orderSettings.DisplayPickupInStoreOnShippingMethodPage*/
            )
            {
                var pickupInStore = ParsePickupInStore(form);

                if (pickupInStore)
                {
                    var pickupOption = await ParsePickupOptionAsync(cart, form);
                    await SavePickupOptionAsync(pickupOption);

                    #region Erp Order Additional Data

                    if (b2BUser != null)
                    {
                        await _genericAttributeService.SaveAttributeAsync(
                            customer,
                            B2BB2CFeaturesDefaults.SelectedB2BDeliveryDateAttribute,
                            model.DeliveryDate.Date <= DateTime.Now.Date
                                ? DateTime.Now.AddDays(1)
                                : model.DeliveryDate.Date,
                            store.Id
                        );

                        if (!string.IsNullOrEmpty(specialInstruction))
                        {
                            await _genericAttributeService.SaveAttributeAsync(
                                customer,
                                B2BB2CFeaturesDefaults.B2BSpecialInstructions,
                                specialInstruction,
                                store.Id
                            );
                        }

                        if (!string.IsNullOrEmpty(cusReference))
                        {
                            await _genericAttributeService.SaveAttributeAsync(
                                customer,
                                B2BB2CFeaturesDefaults.B2BCustomerReferenceAsPO,
                                cusReference,
                                store.Id
                            );
                        }
                    }

                    if (b2CUser != null)
                    {
                        await _genericAttributeService.SaveAttributeAsync(
                            customer,
                            B2BB2CFeaturesDefaults.SelectedB2BDeliveryDateAttribute,
                            model.DeliveryDate.Date <= DateTime.Now.Date
                                ? DateTime.Now.AddDays(1)
                                : model.DeliveryDate.Date,
                            store.Id
                        );

                        if (!string.IsNullOrEmpty(specialInstruction))
                        {
                            await _genericAttributeService.SaveAttributeAsync(
                                customer,
                                B2BB2CFeaturesDefaults.B2CSpecialInstructions,
                                specialInstruction,
                                store.Id
                            );
                        }
                    }

                    #endregion

                    return await OpcLoadStepAfterShippingMethod(cart);
                }

                await _genericAttributeService.SaveAttributeAsync<PickupPoint>(
                    customer,
                    NopCustomerDefaults.SelectedPickupPointAttribute,
                    null,
                    store.Id
                );
            }

            _ = int.TryParse(form["shipping_address_id"], out var shippingAddressId);

            if (model.SelectedShipToAddress.Id <= 0)
            {
                await _erpCheckoutModelFactory.PrepareB2BShipToAddressModelAsync(
                    model.SelectedShipToAddress,
                    await _erpShipToAddressService.GetErpShipToAddressByIdWithActiveAsync(
                        shippingAddressId
                    ),
                    erpAccount
                );
            }

            var modelShipToAddress = model.SelectedShipToAddress;

            modelShipToAddress.DeliveryDate = model.DeliveryDate;
            modelShipToAddress.CustomerReference = model.CustomerReference;
            modelShipToAddress.SpecialInstructions = model.SpecialInstructions;

            if (modelShipToAddress.IsFullLoadRequired)
            {
                (var minDeliveryDate, var _) = await _erpCustomerFunctionalityService.GetMinimumAndMaximumDeliveryDateForShippingAddress();
                modelShipToAddress.DeliveryDate = minDeliveryDate.Date;
            }
            else if (!modelShipToAddress.ErpToDetermineDate)
            {
                if (modelShipToAddress.DeliveryDate == DateTime.MinValue)
                    ModelState.AddModelError(
                        "DeliveryDate",
                        "Please provide a valid delivery date"
                    );
                (var minDeliveryDate, var maxDeliveryDate) =
                    await _erpCustomerFunctionalityService.GetMinimumAndMaximumDeliveryDateForShippingAddress();

                if (
                    modelShipToAddress.DeliveryDate < minDeliveryDate.Date
                    || modelShipToAddress.DeliveryDate > maxDeliveryDate
                )
                    ModelState.AddModelError(
                        "DeliveryDate",
                        "Please provide a valid delivery date"
                    );
            }

            var user = b2BUser ?? b2CUser;
            var erpShipToAddress = await _erpShipToAddressService.GetErpShipToAddressByIdWithActiveAsync(user?.ErpShipToAddressId ?? 0);

            if (erpAccount is not null && erpShipToAddress is not null && modelShipToAddress.Id > 0)
            {
                if (modelShipToAddress.AllowEdit)
                {
                    var lastShippingB2BShipToAddress =
                        await _erpShipToAddressService.GetErpShipToAddressByIdWithActiveAsync(
                            user.ErpShipToAddressId
                        );

                    if (lastShippingB2BShipToAddress == null)
                    {
                        var address = new Address
                        {
                            Email = customer.Email,
                            FirstName = customer.FirstName,
                            LastName = customer.LastName,
                            Company = modelShipToAddress.Company,
                            CountryId = modelShipToAddress.CountryId,
                            StateProvinceId = modelShipToAddress.StateProvinceId,
                            City = modelShipToAddress.City,
                            Address1 = modelShipToAddress.Address1,
                            Address2 = modelShipToAddress.Address2,
                            ZipPostalCode = modelShipToAddress.ZipPostalCode,
                            PhoneNumber = modelShipToAddress.PhoneNumber,
                        };
                        await _addressService.InsertAddressAsync(address);
                        await _customerService.InsertCustomerAddressAsync(customer, address);

                        var b2BShipToAddress = new ErpShipToAddress
                        {
                            ShipToCode = modelShipToAddress.ShipToCode,
                            ShipToName = modelShipToAddress.ShipToName,
                            AddressId = address.Id,
                            Suburb = modelShipToAddress.Suburb,
                            DeliveryNotes = modelShipToAddress.DeliveryNotes,
                            EmailAddresses = modelShipToAddress.Email,
                            IsActive = modelShipToAddress.IsActive,
                            ProvinceCode =
                                lastShippingB2BShipToAddress != null
                                    ? lastShippingB2BShipToAddress.ProvinceCode
                                    : erpShipToAddress.ProvinceCode,
                            RepEmail =
                                lastShippingB2BShipToAddress != null
                                    ? lastShippingB2BShipToAddress.RepEmail
                                    : erpShipToAddress.RepEmail,
                            RepFullName =
                                lastShippingB2BShipToAddress != null
                                    ? lastShippingB2BShipToAddress.RepFullName
                                    : erpShipToAddress.RepFullName,
                            RepNumber =
                                lastShippingB2BShipToAddress != null
                                    ? lastShippingB2BShipToAddress.RepNumber
                                    : erpShipToAddress.RepNumber,
                            RepPhoneNumber =
                                lastShippingB2BShipToAddress != null
                                    ? lastShippingB2BShipToAddress.RepPhoneNumber
                                    : erpShipToAddress.RepPhoneNumber,
                            CreatedOnUtc = DateTime.UtcNow,
                            CreatedById = customer.Id,
                            UpdatedOnUtc = DateTime.UtcNow,
                            UpdatedById = customer.Id,
                        };

                        await _erpShipToAddressService.InsertErpShipToAddressAsync(b2BShipToAddress);
                        await _erpShipToAddressService.InsertErpShipToAddressErpAccountMapAsync(erpAccount, b2BShipToAddress, ErpShipToAddressCreatedByType.User);

                        user.ShippingErpShipToAddressId = b2BShipToAddress.Id;
                        await _erpNopUserService.UpdateErpNopUserAsync(user);

                        customer.ShippingAddressId = address.Id;
                        await _customerService.UpdateCustomerAsync(customer);
                    }
                    else
                    {
                        var address = await _addressService.GetAddressByIdAsync(
                            lastShippingB2BShipToAddress.AddressId
                        );
                        address.Email = customer.Email;
                        address.FirstName = customer.FirstName;
                        address.LastName = customer.LastName;
                        address.Company = modelShipToAddress.Company;
                        address.CountryId = modelShipToAddress.CountryId;
                        address.StateProvinceId = modelShipToAddress.StateProvinceId;
                        address.City = modelShipToAddress.City;
                        address.Address1 = modelShipToAddress.Address1;
                        address.Address2 = modelShipToAddress.Address2;
                        address.ZipPostalCode = modelShipToAddress.ZipPostalCode;
                        address.PhoneNumber = modelShipToAddress.PhoneNumber;

                        await _addressService.UpdateAddressAsync(address);

                        lastShippingB2BShipToAddress.IsActive = modelShipToAddress.IsActive;
                        lastShippingB2BShipToAddress.UpdatedOnUtc = DateTime.UtcNow;
                        lastShippingB2BShipToAddress.UpdatedById = customer.Id;

                        await _erpShipToAddressService.UpdateErpShipToAddressAsync(
                            lastShippingB2BShipToAddress
                        );

                        customer.ShippingAddressId = address.Id;
                        await _customerService.UpdateCustomerAsync(customer);
                        await _customerService.InsertCustomerAddressAsync(customer, address);
                    }
                }
                else
                {
                    user.ShippingErpShipToAddressId = erpShipToAddress.Id;
                    await _erpNopUserService.UpdateErpNopUserAsync(user);

                    var address = await _addressService.GetAddressByIdAsync(erpShipToAddress.AddressId);

                    address.Email = customer.Email;
                    address.FirstName = customer.FirstName;
                    address.LastName = customer.LastName;

                    customer.ShippingAddressId = address.Id;
                    await _customerService.UpdateCustomerAsync(customer);
                    await _customerService.InsertCustomerAddressAsync(customer, address);
                }

                #region Erp Order Additional Data

                var date = model.DeliveryDate;

                if (!model.ErpToDetermineDate)
                {
                    if (model.CustomDeliveryDateString is null)
                    {
                        ModelState.AddModelError("DeliveryDateString", "Please provide a valid delivery date");
                    }
                    else
                    {
                        if (DateTime.TryParseExact(model.CustomDeliveryDateString, "dd/MM/yyyy", new CultureInfo("en-GB"), DateTimeStyles.None, out var dateTimeForDelivery))
                            date = dateTimeForDelivery;

                        if (date < DateTime.Now.Date)
                        {
                            ModelState.AddModelError("DeliveryDate", "Please provide a valid delivery date");
                        }
                    }
                }
                else
                {
                    if (model.DeliveryDateString is null)
                    {
                        ModelState.AddModelError("DeliveryDate", "Please provide a valid delivery date");
                    }
                    else
                    {
                        if (DateTime.TryParseExact(model.DeliveryDateString, "dd/MM/yyyy", new CultureInfo("en-GB"), DateTimeStyles.None, out var dateTimeForDelivery))
                            date = dateTimeForDelivery;

                        if (date < DateTime.Now.Date)
                        {
                            ModelState.AddModelError("DeliveryDate", "Please provide a valid delivery date");
                        }
                    }
                }

                if (date >= DateTime.Now.Date)
                {
                    await _genericAttributeService.SaveAttributeAsync(customer, B2BB2CFeaturesDefaults.SelectedB2BDeliveryDateAttribute, date, store.Id);
                }

                if (b2BUser != null)
                {
                    if (!string.IsNullOrEmpty(specialInstruction))
                    {
                        await _genericAttributeService.SaveAttributeAsync(
                            customer,
                            B2BB2CFeaturesDefaults.B2BSpecialInstructions,
                            specialInstruction,
                            store.Id
                        );
                    }

                    if (!string.IsNullOrEmpty(cusReference))
                    {
                        await _genericAttributeService.SaveAttributeAsync(
                            customer,
                            B2BB2CFeaturesDefaults.B2BCustomerReferenceAsPO,
                            cusReference,
                            store.Id
                        );
                    }
                }

                if (b2CUser != null && !string.IsNullOrEmpty(specialInstruction))
                {
                    await _genericAttributeService.SaveAttributeAsync(customer, B2BB2CFeaturesDefaults.B2CSpecialInstructions, specialInstruction, store.Id);
                }

                #endregion


                return await OpcLoadStepAfterShippingAddress(cart);
            }
            else
            {
                var customAttributes = await _addressAttributeParser.ParseCustomAttributesAsync(form, NopCommonDefaults.AddressAttributeControlName);
                var customAttributeWarnings = await _addressAttributeParser.GetAttributeWarningsAsync(customAttributes);
                foreach (var error in customAttributeWarnings)
                {
                    ModelState.AddModelError("", error);
                }
                var shippingAddressModel =
                    await _erpCheckoutModelFactory.PrepareShippingAddressModelAsync(
                        b2BUser,
                        erpAccount,
                        prePopulateNewAddressWithCustomerFields: true
                    );

                return Json(
                    new
                    {
                        update_section = new UpdateSectionJsonModel
                        {
                            name = "shipping",
                            html = await RenderPartialViewToStringAsync(
                                "~/Plugins/NopStation.Plugin.B2B.B2BB2CFeatures/Views/ErpCheckout/OpcShippingAddress.cshtml",
                                shippingAddressModel
                            ),
                        },
                        goto_section = "shipping",
                    }
                );
            }
        }
        catch (Exception exc)
        {
            await _logger.WarningAsync(
                exc.Message,
                exc,
                await _workContext.GetCurrentCustomerAsync()
            );
            return Json(new { error = 1, message = exc.Message });
        }
    }

    [HttpPost]
    public override async Task<IActionResult> OpcConfirmOrder(bool captchaValid)
    {
        try
        {
            var currCustomer = await _workContext.GetCurrentCustomerAsync();
            var currStore = await _storeContext.GetCurrentStoreAsync();

            var (erpAccount, b2BUser, b2CUser) = await GetB2BAccountAndUserOfCurrentCustomerAsync();
            if (!await IsUserValid(erpAccount))
                return RedirectToRoute("ShoppingCart");

            var isCaptchaSettingEnabled = await _customerService.IsGuestAsync(currCustomer) &&
                _captchaSettings.Enabled && _captchaSettings.ShowOnCheckoutPageForGuests;

            var confirmOrderModel = new CheckoutConfirmModel()
            {
                DisplayCaptcha = isCaptchaSettingEnabled,
            };

            var cart = await _shoppingCartService.GetShoppingCartAsync(
                currCustomer,
                ShoppingCartType.ShoppingCart,
                currStore.Id
            );

            if (!cart.Any())
                throw new Exception("Your cart is empty");

            if (!_orderSettings.OnePageCheckoutEnabled)
                throw new Exception("One page checkout is disabled");

            if (
                await _customerService.IsGuestAsync(currCustomer)
                && !_orderSettings.AnonymousCheckoutAllowed
            )
                throw new Exception("Anonymous checkout is not allowed");

            if (!await IsMinimumOrderPlacementIntervalValidAsync(currCustomer))
                throw new Exception(
                    await _localizationService.GetResourceAsync(
                        "Checkout.MinOrderPlacementInterval"
                    )
                );

            if (b2CUser != null &&
                b2CUser.LastWarehouseCalculationTimeUtc.HasValue &&
                (DateTime.UtcNow - b2CUser.LastWarehouseCalculationTimeUtc.Value).TotalSeconds >= _b2BB2CFeaturesSettings.WarehouseCalculationTimeout)
            {
                _notificationService.ErrorNotification(
                    await _localizationService.GetResourceAsync("Plugin.Misc.NopStation.Plugin.B2B.B2BB2CFeatures.ErpCheckout.WarehouseCalculationTimeout")
                );
                return RedirectToRoute("ShoppingCart");
            }

            var isQuoteOrder = await IsQuoteOrderAsync();
            if (isQuoteOrder || await _erpCustomerFunctionalityService.IsCurrentCustomerInB2BQuoteAssistantRoleAsync())
            {
                var processPaymentRequest = new ProcessPaymentRequest();

                await _paymentService.GenerateOrderGuidAsync(processPaymentRequest);
                processPaymentRequest.StoreId = currStore.Id;
                processPaymentRequest.CustomerId = currCustomer.Id;
                processPaymentRequest.PaymentMethodSystemName =
                    await _genericAttributeService.GetAttributeAsync<string>(
                        currCustomer,
                        NopCustomerDefaults.SelectedPaymentMethodAttribute,
                        currStore.Id
                    );
                await HttpContext.Session.SetAsync<ProcessPaymentRequest>(
                    "OrderPaymentInfo",
                    processPaymentRequest
                );
                var placeOrderResult = await _overriddenOrderProcessingService.PlaceQuoteOrderAsync(
                    processPaymentRequest
                );
                if (placeOrderResult.Success)
                {
                    if (erpAccount != null && b2BUser != null)
                        await _overriddenOrderProcessingService.PlaceErpOrderAtNopAsync(
                            placeOrderResult.PlacedOrder,
                            ErpOrderType.B2BQuote
                        );

                    await _erpLogsService.InformationAsync($"Quote order placed successfully! Order-Id: {placeOrderResult.PlacedOrder.Id}, Erp Order Number: {placeOrderResult.PlacedOrder.CustomOrderNumber}", ErpSyncLevel.Order, customer: currCustomer);

                    //activity log
                    await _customerActivityService.InsertActivityAsync(
                        currCustomer,
                        "PublicStore.PlaceOrder",
                        string.Format(
                            await _localizationService.GetResourceAsync(
                                "ActivityLog.PublicStore.PlaceOrder"
                            ),
                            placeOrderResult.PlacedOrder.CustomOrderNumber
                        ),
                        placeOrderResult.PlacedOrder
                    );

                    return Json(new { success = 1 });
                }

                foreach (var error in placeOrderResult.Errors)
                    confirmOrderModel.Warnings.Add(error);

                return Json(
                    new
                    {
                        update_section = new UpdateSectionJsonModel
                        {
                            name = "confirm-order",
                            html = await RenderPartialViewToStringAsync(
                                "OpcConfirmOrder",
                                confirmOrderModel
                            ),
                        },
                        goto_section = "confirm_order",
                    }
                );
            }
            else
            {
                var processPaymentRequest =
                    await HttpContext.Session.GetAsync<ProcessPaymentRequest>("OrderPaymentInfo");
                if (processPaymentRequest == null)
                {
                    if (await _orderProcessingService.IsPaymentWorkflowRequiredAsync(cart))
                    {
                        throw new Exception("Payment information is not entered");
                    }

                    processPaymentRequest = new ProcessPaymentRequest();
                }
                await _paymentService.GenerateOrderGuidAsync(processPaymentRequest);
                processPaymentRequest.StoreId = currStore.Id;
                processPaymentRequest.CustomerId = currCustomer.Id;
                processPaymentRequest.PaymentMethodSystemName =
                    await _genericAttributeService.GetAttributeAsync<string>(
                        currCustomer,
                        NopCustomerDefaults.SelectedPaymentMethodAttribute,
                        currStore.Id
                    );
                await HttpContext.Session.SetAsync<ProcessPaymentRequest>(
                    "OrderPaymentInfo",
                    processPaymentRequest
                );
                var placeOrderResult = await _orderProcessingService.PlaceOrderAsync(
                    processPaymentRequest
                );
                if (placeOrderResult.Success)
                {
                    if (erpAccount != null && b2BUser != null && b2BUser.Id > 0)
                    {
                        await _overriddenOrderProcessingService.PlaceErpOrderAtNopAsync(
                            placeOrderResult.PlacedOrder,
                            ErpOrderType.B2BSalesOrder
                        );

                        await _erpLogsService.InformationAsync($"B2B Order placed successfully! Order-Id: {placeOrderResult.PlacedOrder.Id}, Erp Order Number: {placeOrderResult.PlacedOrder.CustomOrderNumber}", ErpSyncLevel.Order, customer: currCustomer);
                    }

                    //activity log
                    await _customerActivityService.InsertActivityAsync(
                        currCustomer,
                        "PublicStore.PlaceOrder",
                        string.Format(
                            await _localizationService.GetResourceAsync(
                                "ActivityLog.PublicStore.PlaceOrder"
                            ),
                            placeOrderResult.PlacedOrder.CustomOrderNumber
                        ),
                        placeOrderResult.PlacedOrder
                    );

                    await HttpContext.Session.SetAsync<ProcessPaymentRequest>(
                        "OrderPaymentInfo",
                        null
                    );
                    var postProcessPaymentRequest = new PostProcessPaymentRequest
                    {
                        Order = placeOrderResult.PlacedOrder,
                    };

                    var paymentMethod = await _paymentPluginManager.LoadPluginBySystemNameAsync(
                        placeOrderResult.PlacedOrder.PaymentMethodSystemName,
                        currCustomer,
                        currStore.Id
                    );
                    if (paymentMethod == null)
                        return Json(new { success = 1 });

                    if (paymentMethod.PaymentMethodType == PaymentMethodType.Redirection)
                    {
                        return Json(
                            new
                            {
                                redirect = $"{_webHelper.GetStoreLocation()}checkout/OpcCompleteRedirectionPayment",
                            }
                        );
                    }

                    await _paymentService.PostProcessPaymentAsync(postProcessPaymentRequest);
                    return Json(new { success = 1 });
                }

                foreach (var error in placeOrderResult.Errors)
                    confirmOrderModel.Warnings.Add(error);

                return Json(
                    new
                    {
                        update_section = new UpdateSectionJsonModel
                        {
                            name = "confirm-order",
                            html = await RenderPartialViewToStringAsync(
                                "OpcConfirmOrder",
                                confirmOrderModel
                            ),
                        },
                        goto_section = "confirm_order",
                    }
                );
            }
        }
        catch (Exception exc)
        {
            await _logger.WarningAsync(
                exc.Message,
                exc,
                await _workContext.GetCurrentCustomerAsync()
            );
            return Json(new { error = 1, message = exc.Message });
        }
    }

    [HttpPost]
    public override async Task<IActionResult> OpcSaveShippingMethod(
        string shippingoption,
        IFormCollection form
    )
    {
        try
        {
            var (b2BAccount, _, _) = await GetB2BAccountAndUserOfCurrentCustomerAsync();

            if (!await IsUserValid(b2BAccount))
                return RedirectToRoute("ShoppingCart");

            var customer = await _workContext.GetCurrentCustomerAsync();
            var store = await _storeContext.GetCurrentStoreAsync();
            var cart = await _shoppingCartService.GetShoppingCartAsync(
                customer,
                ShoppingCartType.ShoppingCart,
                store.Id
            );

            if (!cart.Any())
                throw new Exception("Your cart is empty");

            if (!_orderSettings.OnePageCheckoutEnabled)
                throw new Exception("One page checkout is disabled");

            if (
                await _customerService.IsGuestAsync(customer)
                && !_orderSettings.AnonymousCheckoutAllowed
            )
                throw new Exception("Anonymous checkout is not allowed");

            if (!await _shoppingCartService.ShoppingCartRequiresShippingAsync(cart))
                throw new Exception("Shipping is not required");

            //pickup point
            if (
                _shippingSettings.AllowPickupInStore /*&& _orderSettings.DisplayPickupInStoreOnShippingMethodPage*/
            )
            {
                var pickupInStore = ParsePickupInStore(form);
                if (pickupInStore)
                {
                    var pickupOption = await ParsePickupOptionAsync(cart, form);
                    await SavePickupOptionAsync(pickupOption);

                    return await OpcLoadStepAfterShippingMethod(cart);
                }

                //set value indicating that "pick up in store" option has not been chosen
                await _genericAttributeService.SaveAttributeAsync<PickupPoint>(
                    customer,
                    NopCustomerDefaults.SelectedPickupPointAttribute,
                    null,
                    store.Id
                );
            }

            //parse selected method
            if (string.IsNullOrEmpty(shippingoption))
                throw new Exception("Selected shipping method can't be parsed");
            var splittedOption = shippingoption.Split(
                _separator,
                StringSplitOptions.RemoveEmptyEntries
            );
            if (splittedOption.Length != 2)
                throw new Exception("Selected shipping method can't be parsed");
            var selectedName = splittedOption[0];
            var shippingRateComputationMethodSystemName = splittedOption[1];

            //find it
            //performance optimization. try cache first
            var shippingOptions = await _genericAttributeService.GetAttributeAsync<
                List<ShippingOption>
            >(customer, NopCustomerDefaults.OfferedShippingOptionsAttribute, store.Id);
            if (shippingOptions == null || !shippingOptions.Any())
            {
                //not found? let's load them using shipping service
                shippingOptions = (
                    await _shippingService.GetShippingOptionsAsync(
                        cart,
                        await _customerService.GetCustomerShippingAddressAsync(customer),
                        customer,
                        shippingRateComputationMethodSystemName,
                        store.Id
                    )
                ).ShippingOptions.ToList();
            }
            else
            {
                //loaded cached results. let's filter result by a chosen shipping rate computation method
                shippingOptions = shippingOptions
                    .Where(so =>
                        so.ShippingRateComputationMethodSystemName.Equals(
                            shippingRateComputationMethodSystemName,
                            StringComparison.InvariantCultureIgnoreCase
                        )
                    )
                    .ToList();
            }

            var shippingOption =
                shippingOptions.Find(so =>
                    !string.IsNullOrEmpty(so.Name)
                    && so.Name.Equals(selectedName, StringComparison.InvariantCultureIgnoreCase)
                ) ?? throw new Exception("Selected shipping method can't be loaded");

            //save
            await _genericAttributeService.SaveAttributeAsync(
                customer,
                NopCustomerDefaults.SelectedShippingOptionAttribute,
                shippingOption,
                store.Id
            );

            //load next step
            return await OpcLoadStepAfterShippingMethod(cart);
        }
        catch (Exception exc)
        {
            await _logger.WarningAsync(
                exc.Message,
                exc,
                await _workContext.GetCurrentCustomerAsync()
            );
            return Json(new { error = 1, message = exc.Message });
        }
    }

    [HttpPost]
    public override async Task<IActionResult> OpcSavePaymentMethod(
        string paymentmethod,
        CheckoutPaymentMethodModel model
    )
    {
        try
        {
            var (b2BAccount, _, _) = await GetB2BAccountAndUserOfCurrentCustomerAsync();

            if (!await IsUserValid(b2BAccount))
                return RedirectToRoute("ShoppingCart");

            var customer = await _workContext.GetCurrentCustomerAsync();
            var store = await _storeContext.GetCurrentStoreAsync();
            var cart = await _shoppingCartService.GetShoppingCartAsync(
                customer,
                ShoppingCartType.ShoppingCart,
                store.Id
            );

            if (!cart.Any())
                throw new Exception("Your cart is empty");

            if (!_orderSettings.OnePageCheckoutEnabled)
                throw new Exception("One page checkout is disabled");

            if (
                await _customerService.IsGuestAsync(customer)
                && !_orderSettings.AnonymousCheckoutAllowed
            )
                throw new Exception("Anonymous checkout is not allowed");

            //payment method
            if (string.IsNullOrEmpty(paymentmethod))
                throw new Exception("Selected payment method can't be parsed");

            //reward points
            if (_rewardPointsSettings.Enabled)
            {
                await _genericAttributeService.SaveAttributeAsync(
                    customer,
                    NopCustomerDefaults.UseRewardPointsDuringCheckoutAttribute,
                    model.UseRewardPoints,
                    store.Id
                );
            }

            //Check whether payment workflow is required
            var isPaymentWorkflowRequired =
                await _orderProcessingService.IsPaymentWorkflowRequiredAsync(cart);
            if (!isPaymentWorkflowRequired)
            {
                //payment is not required
                await _genericAttributeService.SaveAttributeAsync<string>(
                    customer,
                    NopCustomerDefaults.SelectedPaymentMethodAttribute,
                    null,
                    store.Id
                );

                var confirmOrderModel = await _checkoutModelFactory.PrepareConfirmOrderModelAsync(
                    cart
                );
                return Json(
                    new
                    {
                        update_section = new UpdateSectionJsonModel
                        {
                            name = "confirm-order",
                            html = await RenderPartialViewToStringAsync(
                                "OpcConfirmOrder",
                                confirmOrderModel
                            ),
                        },
                        goto_section = "confirm_order",
                    }
                );
            }

            var paymentMethodInst = await _paymentPluginManager.LoadPluginBySystemNameAsync(
                paymentmethod,
                customer,
                store.Id
            );
            if (!_paymentPluginManager.IsPluginActive(paymentMethodInst))
                throw new Exception("Selected payment method can't be parsed");

            //save
            await _genericAttributeService.SaveAttributeAsync(
                customer,
                NopCustomerDefaults.SelectedPaymentMethodAttribute,
                paymentmethod,
                store.Id
            );

            return await OpcLoadStepAfterPaymentMethod(paymentMethodInst, cart);
        }
        catch (Exception exc)
        {
            await _logger.WarningAsync(
                exc.Message,
                exc,
                await _workContext.GetCurrentCustomerAsync()
            );
            return Json(new { error = 1, message = exc.Message });
        }
    }

    [HttpPost]
    [IgnoreAntiforgeryToken]
    public override async Task<IActionResult> OpcSavePaymentInfo(IFormCollection form)
    {
        try
        {
            var (b2BAccount, _, _) = await GetB2BAccountAndUserOfCurrentCustomerAsync();

            if (!await IsUserValid(b2BAccount))
                return RedirectToRoute("ShoppingCart");

            var customer = await _workContext.GetCurrentCustomerAsync();
            var store = await _storeContext.GetCurrentStoreAsync();
            var cart = await _shoppingCartService.GetShoppingCartAsync(
                customer,
                ShoppingCartType.ShoppingCart,
                store.Id
            );

            if (!cart.Any())
                throw new Exception("Your cart is empty");

            if (!_orderSettings.OnePageCheckoutEnabled)
                throw new Exception("One page checkout is disabled");

            if (
                await _customerService.IsGuestAsync(customer)
                && !_orderSettings.AnonymousCheckoutAllowed
            )
                throw new Exception("Anonymous checkout is not allowed");

            var paymentMethodSystemName = await _genericAttributeService.GetAttributeAsync<string>(
                customer,
                NopCustomerDefaults.SelectedPaymentMethodAttribute,
                store.Id
            );
            var paymentMethod =
                await _paymentPluginManager.LoadPluginBySystemNameAsync(
                    paymentMethodSystemName,
                    customer,
                    store.Id
                ) ?? throw new Exception("Payment method is not selected");

            var warnings = await paymentMethod.ValidatePaymentFormAsync(form);
            foreach (var warning in warnings)
                ModelState.AddModelError("", warning);
            if (ModelState.IsValid)
            {
                //get payment info
                var paymentInfo = await paymentMethod.GetPaymentInfoAsync(form);
                //set previous order GUID (if exists)
                await _paymentService.GenerateOrderGuidAsync(paymentInfo);

                //session save
                await HttpContext.Session.SetAsync("OrderPaymentInfo", paymentInfo);

                var confirmOrderModel = await _checkoutModelFactory.PrepareConfirmOrderModelAsync(
                    cart
                );
                return Json(
                    new
                    {
                        update_section = new UpdateSectionJsonModel
                        {
                            name = "confirm-order",
                            html = await RenderPartialViewToStringAsync(
                                "OpcConfirmOrder",
                                confirmOrderModel
                            ),
                        },
                        goto_section = "confirm_order",
                    }
                );
            }

            //If we got this far, something failed, redisplay form
            var paymenInfoModel = await _checkoutModelFactory.PreparePaymentInfoModelAsync(
                paymentMethod
            );
            return Json(
                new
                {
                    update_section = new UpdateSectionJsonModel
                    {
                        name = "payment-info",
                        html = await RenderPartialViewToStringAsync(
                            "OpcPaymentInfo",
                            paymenInfoModel
                        ),
                    },
                }
            );
        }
        catch (Exception exc)
        {
            await _logger.WarningAsync(
                exc.Message,
                exc,
                await _workContext.GetCurrentCustomerAsync()
            );
            return Json(new { error = 1, message = exc.Message });
        }
    }

    public override async Task<IActionResult> OpcCompleteRedirectionPayment()
    {
        try
        {
            var (b2BAccount, _, _) = await GetB2BAccountAndUserOfCurrentCustomerAsync();

            if (!await IsUserValid(b2BAccount))
                return RedirectToRoute("ShoppingCart");

            //validation
            if (!_orderSettings.OnePageCheckoutEnabled)
                return RedirectToRoute("Homepage");

            var customer = await _workContext.GetCurrentCustomerAsync();
            if (
                await _customerService.IsGuestAsync(customer)
                && !_orderSettings.AnonymousCheckoutAllowed
            )
                return Challenge();

            //get the order
            var store = await _storeContext.GetCurrentStoreAsync();
            var order = (
                await _orderService.SearchOrdersAsync(
                    storeId: store.Id,
                    customerId: customer.Id,
                    pageSize: 1
                )
            ).FirstOrDefault();
            if (order == null)
                return RedirectToRoute("Homepage");

            var paymentMethod = await _paymentPluginManager.LoadPluginBySystemNameAsync(
                order.PaymentMethodSystemName,
                customer,
                store.Id
            );
            if (paymentMethod == null)
                return RedirectToRoute("Homepage");
            if (paymentMethod.PaymentMethodType != PaymentMethodType.Redirection)
                return RedirectToRoute("Homepage");

            //ensure that order has been just placed
            if ((DateTime.UtcNow - order.CreatedOnUtc).TotalMinutes > 3)
                return RedirectToRoute("Homepage");

            //Redirection will not work on one page checkout page because it's AJAX request.
            //That's why we process it here
            var postProcessPaymentRequest = new PostProcessPaymentRequest { Order = order };

            await _paymentService.PostProcessPaymentAsync(postProcessPaymentRequest);

            if (_webHelper.IsRequestBeingRedirected || _webHelper.IsPostBeingDone)
            {
                //redirection or POST has been done in PostProcessPayment
                return Content(
                    await _localizationService.GetResourceAsync("Checkout.RedirectMessage")
                );
            }

            //if no redirection has been done (to a third-party payment page)
            //theoretically it's not possible
            return RedirectToRoute("CheckoutCompleted", new { orderId = order.Id });
        }
        catch (Exception exc)
        {
            await _logger.WarningAsync(
                exc.Message,
                exc,
                await _workContext.GetCurrentCustomerAsync()
            );
            return Content(exc.Message);
        }
    }

    #endregion

    #region B2B Checkout Data

    [HttpPost]
    public async Task<IActionResult> GetERPDeliveryDates(string suburb, string city)
    {
        var (deliveyDates, isSucceed) = await _erpCheckoutModelFactory.GetDeliveryDatesBySuburbOrCityAsync(suburb, city);

        if (isSucceed && (deliveyDates == null || deliveyDates.Count < 1))
        {
            return Json(
                new
                {
                    deliveyDates = deliveyDates,
                    isSucceed = isSucceed,
                    isFullLoadRequired = true,
                }
            );
        }
        return Json(
            new
            {
                deliveyDates = deliveyDates,
                isSucceed = isSucceed,
                isFullLoadRequired = false,
            }
        );
    }

    [HttpPost]
    public async Task<IActionResult> LoadB2BShipToAddressById(int shipToId)
    {
        var currentCustomer = await _workContext.GetCurrentCustomerAsync();
        var b2BAccount = await _erpAccountService.GetActiveErpAccountByCustomerIdAsync(currentCustomer.Id);

        if (b2BAccount == null)
        {
            return Json(new { isSucceed = false });
        }
        else
        {
            var shipToAddress =
                await _erpShipToAddressService.GetErpShipToAddressByIdWithActiveAsync(shipToId);
            if (shipToAddress == null)
            {
                return Json(new { isSucceed = false });
            }

            var b2BShipToAddressModel = new ErpShipToAddressModelForCheckout();
            await _erpCheckoutModelFactory.PrepareB2BShipToAddressModelAsync(
                b2BShipToAddressModel,
                shipToAddress,
                b2BAccount,
                loadAvailableSuburbs: true,
                loadCountriesAndStates: true
            );

            return Json(
                new
                {
                    Data = await RenderPartialViewToStringAsync(
                        "~/Plugins/NopStation.Plugin.B2B.B2BB2CFeatures/Views/ErpCheckout/LoadErpShipToAddress.cshtml",
                        b2BShipToAddressModel
                    ),
                    isSucceed = true,
                }
            );
        }
    }

    public async Task<IActionResult> LoadB2BCheckoutCompletedData(int nopOrderId)
    {
        var model = await _erpOrderItemModelFactory.PrepareB2BCheckoutCompletedModelAsync(
            nopOrderId
        );
        return Json(new { Data = model });
    }

    [HttpPost]
    public async Task<IActionResult> LoadB2BOrderItemData(int nopOrderId, List<string> itemIds)
    {
        var model = await _erpOrderItemModelFactory.PrepareB2BOrderItemDataModelListModelAsync(
            nopOrderId,
            itemIds
        );
        return Json(new { Data = model });
    }

    [HttpGet]
    public async Task<IActionResult> ValidateCustomerReference(
        string customerReferenceAsPO,
        int erpAccountId
    )
    {
        if (
            await _erpOrderAdditionalDataService.IfCustomerReferenceExistWithThisErpAccount(
                customerReferenceAsPO,
                erpAccountId
            )
        )
        {
            return Json(
                new
                {
                    isValid = false,
                    errorMessage = await _localizationService.GetResourceAsync(
                        "Plugins.Payments.NopStation.B2B.Account.CustomerReferenceAsPO.AlreadyExist"
                    ),
                }
            );
        }

        return Json(new { isValid = true });
    }

    public async Task<IActionResult> ClearShippingOptionAndSavePickupPoint()
    {
        if (!_shippingSettings.AllowPickupInStore)
            return RedirectToRoute("CheckoutShippingAddress");

        var currentCustomer = await _workContext.GetCurrentCustomerAsync();
        var store = await _storeContext.GetCurrentStoreAsync();
        (var erpAccount, var erpNopUser) = await GetErpAccountAndUserOfCurrentCustomerAsync(currentCustomer.Id);

        if (erpNopUser == null || erpAccount == null)
            return RedirectToRoute("CheckoutShippingAddress");

        var b2CWarehouseSalesOrg = await _erpSalesOrgService.GetErpSalesOrgByIdAsync(erpAccount.ErpSalesOrgId);
        var b2CWarehouse = await _shippingService.GetWarehouseByIdAsync(b2CWarehouseSalesOrg.TradingWarehouseId ?? 0);
        var nopAddress = await _addressService.GetAddressByIdAsync(b2CWarehouse?.AddressId ?? 0);

        var cart = await _shoppingCartService.GetShoppingCartAsync(currentCustomer, ShoppingCartType.ShoppingCart, store.Id);
        var pickupPoints = 
            (await _shippingService.GetPickupPointsAsync(cart, nopAddress, customer: currentCustomer, storeId: store.Id))?.PickupPoints?.ToList();

        var defaultPoint = pickupPoints?.FirstOrDefault();
        if (defaultPoint == null)
            return RedirectToRoute("CheckoutShippingAddress");

        var pickUpInStoreShippingOption = new ShippingOption
        {
            Name = string.Format(await _localizationService.GetResourceAsync("Checkout.PickupPoints.Name"), defaultPoint.Name),
            Rate = 0,
            Description = defaultPoint.Description,
            ShippingRateComputationMethodSystemName = defaultPoint.ProviderSystemName
        };

        await _genericAttributeService.SaveAttributeAsync(currentCustomer, NopCustomerDefaults.SelectedShippingOptionAttribute, pickUpInStoreShippingOption, store.Id);
        await _genericAttributeService.SaveAttributeAsync(currentCustomer, NopCustomerDefaults.SelectedPickupPointAttribute, defaultPoint, store.Id);

        return Json(await RenderViewComponentToStringAsync(typeof(OrderTotalsViewComponent), new { isEditable = false }));
    }

    public async Task<IActionResult> ClearPickUpPointAndSaveShippingOption()
    {
        var currentCustomer = await _workContext.GetCurrentCustomerAsync();
        await _genericAttributeService.SaveAttributeAsync<PickupPoint>(
            currentCustomer,
            NopCustomerDefaults.SelectedPickupPointAttribute,
            null,
            _storeContext.GetCurrentStoreAsync().Id
        );
        var erpUser = await _erpCustomerFunctionalityService.GetActiveErpNopUserByCustomerAsync(
            currentCustomer
        );

        var isB2CUser = erpUser != null && erpUser.ErpUserType == ErpUserType.B2CUser;

        if (isB2CUser)
        {
            if (
                erpUser.LastWarehouseCalculationTimeUtc.HasValue
                && (DateTime.UtcNow - erpUser.LastWarehouseCalculationTimeUtc.Value).TotalSeconds
                    >= _b2BB2CFeaturesSettings.WarehouseCalculationTimeout
            )
            {
                _notificationService.ErrorNotification(
                    await _localizationService.GetResourceAsync(
                        "Plugin.Misc.NopStation.Plugin.B2B.B2BB2CFeatures.ErpCheckout.WarehouseCalculationTimeout"
                    )
                );
                return Json(new
                {
                    success = false,
                    redirectToShoppingCart = true
                });
            }

            var cart = (
                await _shoppingCartService.GetShoppingCartAsync(
                    currentCustomer,
                    ShoppingCartType.ShoppingCart,
                    (await _storeContext.GetCurrentStoreAsync()).Id
                )
            ).ToList();

            var isAnySciFrom1041 = false;
            decimal? b2CShippingCost = null;

            if (cart.Count != 0)
            {
                foreach (var sci in cart)
                {
                    var b2CSci =
                        await _b2CShoppingCartItemService.GetB2CShoppingCartItemByNopShoppingCartItemIdAsync(
                            sci.Id
                        );
                    if (b2CSci != null)
                    {
                        isAnySciFrom1041 |= b2CSci.WarehouseCode == "1041";
                    }
                }

                var salesOrg = await _erpSalesOrgService.GetErpSalesOrgByErpAccountIdAsync(erpUser.ErpAccountId);
                if (salesOrg.Code == "1040")
                {
                    if (isAnySciFrom1041)
                    {
                        var b2CShipToAddress = await _erpShipToAddressService.GetErpShipToAddressByIdAsync(erpUser.ErpShipToAddressId);


                        b2CShippingCost = await _erpShippingService.GetB2CShippingCostAsync(cart, currentCustomer, b2CShipToAddress);
                    }
                    else
                    {
                        await _erpLogsService.InformationAsync(
                            $"B2B Checkout Clear PickUp Point And Save Shipping Option: " +
                            $"No items from 1041 for customer: {currentCustomer.Email}, shipping cost set to 0.",
                            ErpSyncLevel.Order
                        );
                        b2CShippingCost = 0;
                    }
                }
                else
                {
                    var b2CShipToAddress = await _erpShipToAddressService.GetErpShipToAddressByIdAsync(erpUser.ErpShipToAddressId);
                    b2CShippingCost = await _erpShippingService.GetB2CShippingCostAsync(cart, currentCustomer, b2CShipToAddress);
                }

                if (b2CShippingCost != null)
                {
                    var b2CShippingOption = new ShippingOption
                    {
                        Name = await _localizationService.GetResourceAsync(
                            B2BB2CFeaturesDefaults.B2CShippingOptionName
                        ),
                        Rate = b2CShippingCost.Value,
                        Description = await _localizationService.GetResourceAsync(
                            B2BB2CFeaturesDefaults.B2CShippingOptionDescription
                        ),
                        ShippingRateComputationMethodSystemName =
                            B2BB2CFeaturesDefaults.B2CShippingOptionSystemName,
                    };

                    await _genericAttributeService.SaveAttributeAsync(
                        currentCustomer,
                        NopCustomerDefaults.SelectedShippingOptionAttribute,
                        b2CShippingOption,
                        (await _storeContext.GetCurrentStoreAsync()).Id
                    );

                    return Json(await RenderViewComponentToStringAsync(typeof(OrderTotalsViewComponent), new { isEditable = false }));
                }

                await _erpLogsService.InformationAsync(
                    $"B2B Checkout: Get Shipping Cost From ERP returned null for customer {currentCustomer.Email}",
                    ErpSyncLevel.Order
                );

                return Json(new
                {
                    success = true,
                    isB2CShippingCostERPCallUnSuccessful = true
                });
            }
        }

        return Json(new
        {
            success = false
        });
    }

    #endregion
}
