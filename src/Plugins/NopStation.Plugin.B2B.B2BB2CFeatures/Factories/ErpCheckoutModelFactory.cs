using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Core;
using Nop.Core.Caching;
using Nop.Core.Domain;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Security;
using Nop.Core.Domain.Shipping;
using Nop.Core.Infrastructure;
using Nop.Services.Catalog;
using Nop.Services.Common;
using Nop.Services.Configuration;
using Nop.Services.Customers;
using Nop.Services.Directory;
using Nop.Services.Localization;
using Nop.Services.Orders;
using Nop.Services.Shipping;
using Nop.Services.Shipping.Pickup;
using Nop.Services.Tax;
using Nop.Web.Factories;
using Nop.Web.Models.Checkout;
using Nop.Web.Models.ShoppingCart;
using NopStation.Plugin.B2B.B2BB2CFeatures.Model.Checkout;
using NopStation.Plugin.B2B.B2BB2CFeatures.Services.ErpCustomerFunctionality;
using NopStation.Plugin.B2B.B2BB2CFeatures.Services.ErpDeliveyDates;
using NopStation.Plugin.B2B.B2BB2CFeatures.Services.ErpShippingService;
using NopStation.Plugin.B2B.B2BB2CFeatures.Services.ErpWorkflowMessage;
using NopStation.Plugin.B2B.ERPIntegrationCore.Domain;
using NopStation.Plugin.B2B.ERPIntegrationCore.Enums;
using NopStation.Plugin.B2B.ERPIntegrationCore.Model;
using NopStation.Plugin.B2B.ERPIntegrationCore.Services;

namespace NopStation.Plugin.B2B.B2BB2CFeatures.Factories;

public class ErpCheckoutModelFactory : IErpCheckoutModelFactory
{
    #region Fields

    private readonly IWorkContext _workContext;
    private readonly OrderSettings _orderSettings;
    private readonly IAddressService _addressService;
    private readonly IPriceFormatter _priceFormatter;
    private readonly ICustomerService _customerService;
    private readonly ICurrencyService _currencyService;
    private readonly ICountryService _countryService;
    private readonly IErpLogsService _erpLogsService;
    private readonly IStateProvinceService _stateProvinceService;
    private readonly AddressSettings _addressSettings;
    private readonly ILocalizationService _localizationService;
    private readonly IStoreContext _storeContext;
    private readonly IGenericAttributeService _genericAttributeService;
    private readonly IErpAccountService _erpAccountService;
    private readonly IErpSalesOrgService _erpSalesOrgService;
    private readonly ISettingService _settingService;
    private readonly IShippingService _shippingService;
    private readonly IErpCustomerFunctionalityService _erpCustomerFunctionalityService;
    private readonly IShoppingCartService _shoppingCartService;
    private readonly IErpShipToAddressService _erpShipToAddressService;
    private readonly ShippingSettings _shippingSettings;
    private readonly IPickupPluginManager _pickupPluginManager;
    private readonly ITaxService _taxService;
    private readonly IShippingPluginManager _shippingPluginManager;
    private readonly CaptchaSettings _captchaSettings;
    private readonly B2BB2CFeaturesSettings _b2BB2CFeaturesSettings;
    private readonly IErpWorkflowMessageService _erpWorkflowMessageService;
    private readonly IErpDeliveryDatesService _erpDeliveryDatesService;
    private readonly IErpShippingService _erpShippingService;
    private readonly IShoppingCartModelFactory _shoppingCartModelFactory;
    private readonly IProductService _productService;
    private readonly IErpWarehouseSalesOrgMapService _erpWarehouseSalesOrgMapService;
    private readonly IB2CShoppingCartItemService _b2cShoppingCartItemService;
    private readonly IStaticCacheManager _staticCacheManager;
    private readonly IErpIntegrationPluginManager _erpIntegrationPluginManager;
    private readonly IErpNopUserService _erpNopUserService;

    #endregion Fields

    #region Ctor

    public ErpCheckoutModelFactory(IWorkContext workContext,
        ILocalizationService localizationService,
        OrderSettings orderSettings,
        IAddressService addressService,
        IPriceFormatter priceFormatter,
        ICustomerService customerService,
        ICurrencyService currencyService,
        ICountryService countryService,
        IErpLogsService erpLogsService,
        IStateProvinceService stateProvinceService,
        AddressSettings addressSettings,
        IStoreContext storeContext,
        IGenericAttributeService genericAttributeService,
        IErpAccountService erpAccountService,
        IErpSalesOrgService erpSalesOrgService,
        ISettingService settingService,
        IShippingService shippingService,
        IErpCustomerFunctionalityService erpCustomerFunctionalityService,
        IShoppingCartService shoppingCartService,
        IErpShipToAddressService erpShipToAddressService,
        ShippingSettings shippingSettings,
        IPickupPluginManager pickupPluginManager,
        ITaxService taxService,
        IShippingPluginManager shippingPluginManager,
        CaptchaSettings captchaSettings,
        B2BB2CFeaturesSettings b2BB2CFeaturesSettings,
        IErpWorkflowMessageService erpWorkflowMessageService,
        IErpDeliveryDatesService erpDeliveryDatesService,
        IErpShippingService erpShippingService,
        IShoppingCartModelFactory shoppingCartModelFactory,
        IProductService productService,
        IErpWarehouseSalesOrgMapService erpWarehouseSalesOrgMapService,
        IB2CShoppingCartItemService b2cShoppingCartItemService,
        IStaticCacheManager staticCacheManager,
        IErpIntegrationPluginManager erpIntegrationPluginManager,
        IErpNopUserService erpNopUserService)
    {
        _workContext = workContext;
        _localizationService = localizationService;
        _orderSettings = orderSettings;
        _addressService = addressService;
        _priceFormatter = priceFormatter;
        _customerService = customerService;
        _currencyService = currencyService;
        _countryService = countryService;
        _erpLogsService = erpLogsService;
        _stateProvinceService = stateProvinceService;
        _addressSettings = addressSettings;
        _storeContext = storeContext;
        _genericAttributeService = genericAttributeService;
        _erpAccountService = erpAccountService;
        _erpSalesOrgService = erpSalesOrgService;
        _settingService = settingService;
        _shippingService = shippingService;
        _erpCustomerFunctionalityService = erpCustomerFunctionalityService;
        _shoppingCartService = shoppingCartService;
        _erpShipToAddressService = erpShipToAddressService;
        _shippingSettings = shippingSettings;
        _pickupPluginManager = pickupPluginManager;
        _taxService = taxService;
        _shippingPluginManager = shippingPluginManager;
        _captchaSettings = captchaSettings;
        _b2BB2CFeaturesSettings = b2BB2CFeaturesSettings;
        _erpWorkflowMessageService = erpWorkflowMessageService;
        _erpDeliveryDatesService = erpDeliveryDatesService;
        _erpShippingService = erpShippingService;
        _shoppingCartModelFactory = shoppingCartModelFactory;
        _productService = productService;
        _erpWarehouseSalesOrgMapService = erpWarehouseSalesOrgMapService;
        _b2cShoppingCartItemService = b2cShoppingCartItemService;
        _staticCacheManager = staticCacheManager;
        _erpIntegrationPluginManager = erpIntegrationPluginManager;
        _erpNopUserService = erpNopUserService;
    }

    #endregion Ctor

    #region Method

    public async Task PrepareB2BShipToAddressModelAsync(
        ErpShipToAddressModelForCheckout b2BShipToAddressModel,
        ErpShipToAddress b2BShipToAddress,
        ErpAccount b2BAccount,
        bool loadAvailableSuburbs = false,
        bool loadCountriesAndStates = false
    )
    {
        if (b2BShipToAddress != null)
        {
            var currentCustomer = await _workContext.GetCurrentCustomerAsync();
            var currentStore = await _storeContext.GetCurrentStoreAsync();

            var shipToAddressAccountMap =
                await _erpShipToAddressService.GetErpShipToAddressErpAccountMapByErpShipToAddressIdAsync(
                    b2BShipToAddress.Id
                );

            if (shipToAddressAccountMap == null)
                return;

            var erpAccount = await _erpAccountService.GetErpAccountByIdAsync(
                shipToAddressAccountMap.ErpAccountId
            );
            b2BShipToAddressModel = b2BShipToAddressModel ?? new ErpShipToAddressModelForCheckout();
            b2BShipToAddressModel.Id = b2BShipToAddress.Id;
            b2BShipToAddressModel.ShipToCode = b2BShipToAddress.ShipToCode;
            b2BShipToAddressModel.ShipToName = b2BShipToAddress.ShipToName;
            b2BShipToAddressModel.AddressId = b2BShipToAddress.AddressId;
            b2BShipToAddressModel.Suburb = b2BShipToAddress.Suburb;
            b2BShipToAddressModel.DeliveryNotes = b2BShipToAddress.DeliveryNotes;
            b2BShipToAddressModel.Email = b2BShipToAddress.EmailAddresses;
            b2BShipToAddressModel.IsActive = b2BShipToAddress.IsActive;
            b2BShipToAddressModel.ErpAccountId = erpAccount.Id;
            b2BShipToAddressModel.ErpAccountNumber = erpAccount.AccountNumber;
            b2BShipToAddressModel.ErpSalesOrganizationId = erpAccount.ErpSalesOrgId;
            b2BShipToAddressModel.SalesOrganisationCode = (
                await _erpSalesOrgService.GetErpSalesOrgByIdAsync(erpAccount.ErpSalesOrgId)
            )?.Code;

            if (b2BShipToAddress.AddressId > 0)
            {
                var address = await _addressService.GetAddressByIdAsync(b2BShipToAddress.AddressId);
                var country = await _countryService.GetCountryByIdAsync(address.CountryId ?? 0);
                var stateProvince = await _stateProvinceService.GetStateProvinceByIdAsync(
                    address.StateProvinceId ?? 0
                );
                b2BShipToAddressModel.Company = address.Company;
                b2BShipToAddressModel.CountryId = address.CountryId;
                b2BShipToAddressModel.CountryName = country?.Name;
                b2BShipToAddressModel.CountryCode = country?.ThreeLetterIsoCode;
                b2BShipToAddressModel.StateProvinceId = address.StateProvinceId;
                b2BShipToAddressModel.StateProvinceName = stateProvince?.Name;
                b2BShipToAddressModel.Address1 = address.Address1;
                b2BShipToAddressModel.City = address.City;
                b2BShipToAddressModel.Address2 = address.Address2;
                b2BShipToAddressModel.ZipPostalCode = address.ZipPostalCode;
                b2BShipToAddressModel.PhoneNumber = address.PhoneNumber;
            }

            b2BShipToAddressModel.AllowEdit =
                await _erpCustomerFunctionalityService.CheckAllowAddressEdit(b2BAccount);
            b2BShipToAddressModel.IsQuoteOrder =
                await _genericAttributeService.GetAttributeAsync<bool>(
                    currentCustomer,
                    B2BB2CFeaturesDefaults.B2BQouteOrderAttribute,
                    currentStore.Id
                );
            b2BShipToAddressModel.OnePageCheckoutEnabled = _orderSettings.OnePageCheckoutEnabled;
            b2BShipToAddressModel.SpecialInstructions =
                await _genericAttributeService.GetAttributeAsync<String>(
                    currentCustomer,
                    B2BB2CFeaturesDefaults.B2BSpecialInstructions,
                    currentStore.Id
                );

            // we have to load this data as well even if ERPToDetermineDate is enabled (if erp call to determine date failed, we will use this)
            (var minDeliveryDate, var maxDeliveryDate) =
                await _erpCustomerFunctionalityService.GetMinimumAndMaximumDeliveryDateForShippingAddress();

            b2BShipToAddressModel.DeliveryDate = minDeliveryDate;
            b2BShipToAddressModel.FormatedDeliveryDate = minDeliveryDate.ToString("dd/MM/yyyy");
            b2BShipToAddressModel.MinDeliveryDate = minDeliveryDate.ToString("yyyy-MM-dd"); // html only support this format
            b2BShipToAddressModel.MaxDeliveryDate = maxDeliveryDate.ToString("yyyy-MM-dd"); // html only support this format

            if (b2BShipToAddressModel.AllowEdit)
            {
                if (loadAvailableSuburbs)
                {
                    // we will load area codes only if ERPToDetermineDate is enabled
                    b2BShipToAddressModel.ErpToDetermineDate =
                        _b2BB2CFeaturesSettings.ERPToDetermineDate;

                    if (_b2BB2CFeaturesSettings.ERPToDetermineDate)
                    {
                        var erpIntegrationPlugin =
                            await _erpIntegrationPluginManager.LoadActiveERPIntegrationPlugin();
                        if (erpIntegrationPlugin == null)
                        {
                            await _erpLogsService.InsertErpLogAsync(
                                ErpLogLevel.Error,
                                ErpSyncLevel.Account,
                                $"Integration method not found. " +
                                $"Could not get Area codes for Sales Org {b2BShipToAddressModel.SalesOrganisationCode} " +
                                $"(Id: {erpAccount.ErpSalesOrgId}) as Location during B2B Shipping Address model preparation."
                            );
                            return;
                        }
                        var areaCodes = await erpIntegrationPlugin
                            .GetAreaCodesForSalesOrgAsLocationAsync(new ErpGetRequestModel());
                        if (areaCodes.Data != null && areaCodes.Data.Any())
                        {
                            b2BShipToAddressModel.AvailableAreas = areaCodes.Data
                                .OrderBy(areaCode => areaCode.Area)
                                .Select(areaCode => new SelectListItem(
                                    areaCode.Area,
                                    areaCode.Area
                                ))
                                .ToList();
                        }
                        else
                        {
                            b2BShipToAddressModel.ErpToDetermineDate = false;
                        }
                    }

                    if (b2BShipToAddressModel.ErpToDetermineDate)
                    {
                        b2BShipToAddressModel.AvailableDeliveryDates.Insert(
                            0,
                            new SelectListItem
                            {
                                Text = await _localizationService.GetResourceAsync(
                                    "NopStation.Plugin.B2B.B2BB2CFeatures.DeliveryDate.SelectDeliveryDate"
                                ),
                                Value = string.Empty,
                                Selected = true,
                            }
                        );
                    }
                }

                if (loadCountriesAndStates)
                {
                    //countries and states
                    var countries = await _countryService.GetAllCountriesForShippingAsync();

                    if (_addressSettings.PreselectCountryIfOnlyOne && countries.Count == 1)
                    {
                        b2BShipToAddressModel.CountryId = countries[0].Id;
                    }
                    else
                    {
                        b2BShipToAddressModel.AvailableCountries.Add(
                            new SelectListItem
                            {
                                Text = await _localizationService.GetResourceAsync(
                                    "Address.SelectCountry"
                                ),
                                Value = string.Empty,
                            }
                        );
                    }

                    foreach (var c in countries)
                    {
                        b2BShipToAddressModel.AvailableCountries.Add(
                            new SelectListItem
                            {
                                Text = await _localizationService.GetLocalizedAsync(c, x => x.Name),
                                Value = $"{c.Id}",
                                Selected = c.Id == b2BShipToAddressModel.CountryId,
                            }
                        );
                    }

                    if (_addressSettings.StateProvinceEnabled)
                    {
                        var languageId = EngineContext
                            .Current.Resolve<IWorkContext>()
                            .GetWorkingLanguageAsync()
                            .Id;
                        var states = (
                            await _stateProvinceService.GetStateProvincesByCountryIdAsync(
                                b2BShipToAddressModel.CountryId ?? 0,
                                languageId
                            )
                        ).ToList();

                        if (states.Count != 0)
                        {
                            b2BShipToAddressModel.AvailableStates.Add(
                                new SelectListItem
                                {
                                    Text = await _localizationService.GetResourceAsync(
                                        "Address.SelectState"
                                    ),
                                    Value = string.Empty,
                                }
                            );

                            foreach (var s in states)
                            {
                                b2BShipToAddressModel.AvailableStates.Add(
                                    new SelectListItem
                                    {
                                        Text = await _localizationService.GetLocalizedAsync(
                                            s,
                                            x => x.Name
                                        ),
                                        Value = $"{s.Id}",
                                        Selected = s.Id == b2BShipToAddressModel.StateProvinceId,
                                    }
                                );
                            }
                        }
                        else
                        {
                            var anyCountrySelected = b2BShipToAddressModel.AvailableCountries.Any(
                                x => x.Selected
                            );
                            b2BShipToAddressModel.AvailableStates.Add(
                                new SelectListItem
                                {
                                    Text = await _localizationService.GetResourceAsync(
                                        anyCountrySelected
                                            ? "Address.OtherNonUS"
                                            : "Address.SelectState"
                                    ),
                                    Value = "0",
                                }
                            );
                        }
                    }
                }
            }
        }
    }

    public async Task PrepareB2CShipToAddressModelAsync(
        ErpShipToAddressModelForCheckout b2BShipToAddressModel,
        ErpShipToAddress b2CShipToAddress,
        ErpAccount b2BAccount,
        bool loadAvailableSuburbs = false,
        bool loadCountriesAndStates = false
    )
    {
        var erpAccount = await _erpAccountService.GetErpAccountByErpShipToAddressAsync(b2CShipToAddress);

        if (erpAccount == null)
        {
            return;
        }

        var currentCustomer = await _workContext.GetCurrentCustomerAsync();
        var currentStore = await _storeContext.GetCurrentStoreAsync();

        b2BShipToAddressModel = b2BShipToAddressModel ?? new ErpShipToAddressModelForCheckout();
        b2BShipToAddressModel.Id = b2CShipToAddress.Id;
        b2BShipToAddressModel.ShipToCode = b2CShipToAddress.ShipToCode;
        b2BShipToAddressModel.ShipToName = b2CShipToAddress.ShipToName;
        b2BShipToAddressModel.AddressId = b2CShipToAddress.AddressId;
        b2BShipToAddressModel.Suburb = b2CShipToAddress.Suburb;
        b2BShipToAddressModel.DeliveryNotes = b2CShipToAddress.DeliveryNotes;
        b2BShipToAddressModel.Email = b2CShipToAddress.EmailAddresses;
        b2BShipToAddressModel.IsActive = b2CShipToAddress.IsActive;
        b2BShipToAddressModel.ErpAccountId = erpAccount.Id;
        b2BShipToAddressModel.ErpAccountNumber = erpAccount.AccountNumber;
        b2BShipToAddressModel.ErpSalesOrganizationId = erpAccount.ErpSalesOrgId;
        b2BShipToAddressModel.SalesOrganisationCode = (await _erpSalesOrgService.GetErpSalesOrgByIdAsync(erpAccount.ErpSalesOrgId))?.Code;

        if (b2CShipToAddress.AddressId > 0)
        {
            var address = await _addressService.GetAddressByIdAsync(b2CShipToAddress.AddressId);
            var country = await _countryService.GetCountryByIdAsync(address.CountryId ?? 0);
            var stateProvince = await _stateProvinceService.GetStateProvinceByIdAsync(
                address.StateProvinceId ?? 0
            );
            b2BShipToAddressModel.Company = address.Company;
            b2BShipToAddressModel.CountryId = address.CountryId;
            b2BShipToAddressModel.CountryName = country?.Name;
            b2BShipToAddressModel.CountryCode = country?.ThreeLetterIsoCode;
            b2BShipToAddressModel.StateProvinceId = address.StateProvinceId;
            b2BShipToAddressModel.StateProvinceName = stateProvince?.Name;
            b2BShipToAddressModel.Address1 = address.Address1;
            b2BShipToAddressModel.City = address.City;
            b2BShipToAddressModel.Address2 = address.Address2;
            b2BShipToAddressModel.ZipPostalCode = address.ZipPostalCode;
            b2BShipToAddressModel.PhoneNumber = address.PhoneNumber;
        }

        b2BShipToAddressModel.AllowEdit =
            await _erpCustomerFunctionalityService.CheckAllowAddressEdit(b2BAccount);
        b2BShipToAddressModel.IsQuoteOrder = await _genericAttributeService.GetAttributeAsync<bool>(
            currentCustomer,
            B2BB2CFeaturesDefaults.B2CQouteOrderAttribute,
            currentStore.Id
        );
        b2BShipToAddressModel.OnePageCheckoutEnabled = _orderSettings.OnePageCheckoutEnabled;
        b2BShipToAddressModel.SpecialInstructions =
            await _genericAttributeService.GetAttributeAsync<string>(
                currentCustomer,
                B2BB2CFeaturesDefaults.B2CSpecialInstructions,
                currentStore.Id
            );

        // we have to load this data as well even if ERPToDetermineDate is enabled (if erp call to determine date failed, we will use this)
        (var minDeliveryDate, var maxDeliveryDate) =
            await _erpCustomerFunctionalityService.GetMinimumAndMaximumDeliveryDateForShippingAddress();

        b2BShipToAddressModel.DeliveryDate = minDeliveryDate;
        b2BShipToAddressModel.FormatedDeliveryDate = minDeliveryDate.ToString("dd/MM/yyyy");
        b2BShipToAddressModel.MinDeliveryDate = minDeliveryDate.ToString("yyyy-MM-dd"); // html only support this format
        b2BShipToAddressModel.MaxDeliveryDate = maxDeliveryDate.ToString("yyyy-MM-dd"); // html only support this format

        if (b2BShipToAddressModel.AllowEdit)
        {
            if (loadAvailableSuburbs)
            {
                // we will load area codes only if ERPToDetermineDate is enabled
                b2BShipToAddressModel.ErpToDetermineDate =
                    _b2BB2CFeaturesSettings.ERPToDetermineDate;

                if (_b2BB2CFeaturesSettings.ERPToDetermineDate)
                {
                    var erpIntegrationPlugin =
                            await _erpIntegrationPluginManager.LoadActiveERPIntegrationPlugin();
                    if (erpIntegrationPlugin == null)
                    {
                        await _erpLogsService.InsertErpLogAsync(
                            ErpLogLevel.Error,
                            ErpSyncLevel.Account,
                            "Integration method not found. " +
                            $"Could not get Area codes for Sales Org {b2BShipToAddressModel.SalesOrganisationCode} " +
                            $"(Id: {erpAccount.ErpSalesOrgId}) as Location during B2C Shipping Address model preparation."
                        );
                        return;
                    }
                    var areaCodes = await erpIntegrationPlugin
                        .GetAreaCodesForSalesOrgAsLocationAsync(new ErpGetRequestModel());
                    if (areaCodes.Data != null && areaCodes.Data.Any())
                    {
                        b2BShipToAddressModel.AvailableAreas = areaCodes.Data
                            .Select(areaCode => new SelectListItem(
                                areaCode.Area,
                                areaCode.Area
                            ))
                            .ToList();
                    }
                    else
                    {
                        b2BShipToAddressModel.ErpToDetermineDate = false;
                    }
                }

                if (b2BShipToAddressModel.ErpToDetermineDate)
                {
                    b2BShipToAddressModel.AvailableDeliveryDates.Insert(
                        0,
                        new SelectListItem
                        {
                            Text = await _localizationService.GetResourceAsync(
                                "NopStation.Plugin.B2B.B2BB2CFeatures.DeliveryDate.SelectDeliveryDate"
                            ),
                            Value = string.Empty,
                            Selected = true,
                        }
                    );
                }
            }

            if (loadCountriesAndStates)
            {
                //countries and states
                var countries = await _countryService.GetAllCountriesForShippingAsync();

                if (_addressSettings.PreselectCountryIfOnlyOne && countries.Count == 1)
                {
                    b2BShipToAddressModel.CountryId = countries[0].Id;
                }
                else
                {
                    b2BShipToAddressModel.AvailableCountries.Add(
                        new SelectListItem
                        {
                            Text = await _localizationService.GetResourceAsync(
                                "Address.SelectCountry"
                            ),
                            Value = string.Empty,
                        }
                    );
                }

                foreach (var c in countries)
                {
                    b2BShipToAddressModel.AvailableCountries.Add(
                        new SelectListItem
                        {
                            Text = await _localizationService.GetLocalizedAsync(c, x => x.Name),
                            Value = $"{c.Id}",
                            Selected = c.Id == b2BShipToAddressModel.CountryId,
                        }
                    );
                }

                if (_addressSettings.StateProvinceEnabled)
                {
                    var languageId = EngineContext
                        .Current.Resolve<IWorkContext>()
                        .GetWorkingLanguageAsync()
                        .Id;
                    var states = (
                        await _stateProvinceService.GetStateProvincesByCountryIdAsync(
                            b2BShipToAddressModel.CountryId ?? 0,
                            languageId
                        )
                    ).ToList();
                    if (states.Count != 0)
                    {
                        b2BShipToAddressModel.AvailableStates.Add(
                            new SelectListItem
                            {
                                Text = await _localizationService.GetResourceAsync(
                                    "Address.SelectState"
                                ),
                                Value = string.Empty,
                            }
                        );

                        foreach (var s in states)
                        {
                            b2BShipToAddressModel.AvailableStates.Add(
                                new SelectListItem
                                {
                                    Text = await _localizationService.GetLocalizedAsync(
                                        s,
                                        x => x.Name
                                    ),
                                    Value = $"{s.Id}",
                                    Selected = s.Id == b2BShipToAddressModel.StateProvinceId,
                                }
                            );
                        }
                    }
                    else
                    {
                        var anyCountrySelected = b2BShipToAddressModel.AvailableCountries.Any(x =>
                            x.Selected
                        );
                        b2BShipToAddressModel.AvailableStates.Add(
                            new SelectListItem
                            {
                                Text = await _localizationService.GetResourceAsync(
                                    anyCountrySelected
                                        ? "Address.OtherNonUS"
                                        : "Address.SelectState"
                                ),
                                Value = "0",
                            }
                        );
                    }
                }
            }
        }
    }

    public async Task<CheckoutErpBillingAddressModel> PrepareCheckoutErpBillingAddressModelAsync(
        IList<ShoppingCartItem> cart,
        ErpAccount b2BAccount
    )
    {
        var currentCustomer = await _workContext.GetCurrentCustomerAsync();
        var model = new CheckoutErpBillingAddressModel
        {
            ShipToSameAddressAllowed =
                _shippingSettings.ShipToSameAddress
                && await _shoppingCartService.ShoppingCartRequiresShippingAsync(cart),
            //allow customers to enter (choose) a shipping address if "Disable Billing address step" setting is enabled
            ShipToSameAddress = !_orderSettings.DisableBillingAddressCheckoutStep,
        };

        model.ErpBillingAddress = model.ErpBillingAddress ?? new ErpBillingAddressModel();
        if (b2BAccount != null)
        {
            model.ErpBillingAddress.ErpAccountId = b2BAccount.Id;
            model.ErpBillingAddress.Suburb = b2BAccount.BillingSuburb;
            if (b2BAccount.BillingAddressId.HasValue && b2BAccount.BillingAddressId.Value > 0)
            {
                var address = await _addressService.GetAddressByIdAsync(
                    b2BAccount.BillingAddressId.Value
                );
                var country = await _countryService.GetCountryByIdAsync(address.CountryId ?? 0);
                var stateProvince = await _stateProvinceService.GetStateProvinceByIdAsync(
                    address.StateProvinceId ?? 0
                );
                model.ErpBillingAddress.Id = address.Id;
                model.ErpBillingAddress.FirstName = currentCustomer.FirstName;
                model.ErpBillingAddress.LastName = currentCustomer.LastName;
                model.ErpBillingAddress.Email = currentCustomer.Email;
                model.ErpBillingAddress.Company = address.Company;
                model.ErpBillingAddress.CountryId = address.CountryId;
                model.ErpBillingAddress.CountryName = country?.Name;
                model.ErpBillingAddress.StateProvinceId = address.StateProvinceId;
                model.ErpBillingAddress.StateProvinceName = stateProvince?.Name;
                model.ErpBillingAddress.Address1 = address.Address1;
                model.ErpBillingAddress.City = address.City;
                model.ErpBillingAddress.Address2 = address.Address2;
                model.ErpBillingAddress.ZipPostalCode = address.ZipPostalCode;
                model.ErpBillingAddress.PhoneNumber = address.PhoneNumber;
            }
        }

        return model;
    }

    public async Task<CheckoutErpShippingAddressModel> PrepareCheckoutB2BShippingAddressModelAsync(
        IList<ShoppingCartItem> cart,
        ErpNopUser b2BUser,
        ErpAccount b2BAccount
    )
    {
        var model = new CheckoutErpShippingAddressModel();
        model.PickupPointsModel = new CheckoutPickupPointsModel
        {
            //allow pickup in store?
            AllowPickupInStore = _shippingSettings.AllowPickupInStore,
        };

        var currentCustomer = await _workContext.GetCurrentCustomerAsync();
        var currentStore = await _storeContext.GetCurrentStoreAsync();
        var b2BSalesOrg = await _erpSalesOrgService.GetErpSalesOrgByIdAsync(
            b2BAccount.ErpSalesOrgId
        );
        model.IsB2BUser = true;
        model.ThemeName = (await _settingService
            .LoadSettingAsync<StoreInformationSettings>(currentStore.Id))
            .DefaultStoreTheme;
        model.SpecialInstructions = await _genericAttributeService.GetAttributeAsync<string>(
            currentCustomer,
            B2BB2CFeaturesDefaults.B2BSpecialInstructions,
            currentStore.Id
        );
        model.DisplayPickupInStore = _orderSettings.DisplayPickupInStoreOnShippingMethodPage;
        var languageId = (await _workContext.GetWorkingLanguageAsync()).Id;

        if (model.PickupPointsModel.AllowPickupInStore)
        {
            model.PickupPointsModel.DisplayPickupPointsOnMap =
                _shippingSettings.DisplayPickupPointsOnMap;
            model.PickupPointsModel.GoogleMapsApiKey = _shippingSettings.GoogleMapsApiKey;
            var pickupPointProviders = await _pickupPluginManager.LoadActivePluginsAsync(
                currentCustomer,
                currentStore.Id
            );
            if (pickupPointProviders.Any())
            {
                var pickupPointsResponse = await _shippingService.GetPickupPointsAsync(
                    cart,
                    address: await _addressService.GetAddressByIdAsync(
                        currentCustomer.BillingAddressId ?? 0
                    ),
                    customer: currentCustomer,
                    storeId: currentStore.Id
                );
                if (pickupPointsResponse.Success)
                    model.PickupPointsModel.PickupPoints = await pickupPointsResponse
                        .PickupPoints.SelectAwait(async point =>
                        {
                            var country = await _countryService.GetCountryByTwoLetterIsoCodeAsync(
                                point.CountryCode
                            );
                            var state =
                                await _stateProvinceService.GetStateProvinceByAbbreviationAsync(
                                    point.StateAbbreviation,
                                    country?.Id
                                );

                            var pickupPointModel = new CheckoutPickupPointModel
                            {
                                Id = point.Id,
                                Name = point.Name,
                                Description = point.Description,
                                ProviderSystemName = point.ProviderSystemName,
                                Address = point.Address,
                                City = point.City,
                                County = point.County,
                                StateName =
                                    state != null
                                        ? await _localizationService.GetLocalizedAsync(
                                            state,
                                            x => x.Name,
                                            languageId
                                        )
                                        : string.Empty,
                                CountryName =
                                    country != null
                                        ? await _localizationService.GetLocalizedAsync(
                                            country,
                                            x => x.Name,
                                            languageId
                                        )
                                        : string.Empty,
                                ZipPostalCode = point.ZipPostalCode,
                                Latitude = point.Latitude,
                                Longitude = point.Longitude,
                                OpeningHours = point.OpeningHours,
                            };
                            if (point.PickupFee > 0)
                            {
                                var amount = await _taxService.GetShippingPriceAsync(
                                    point.PickupFee,
                                    currentCustomer
                                );
                                var priceAmount =
                                    await _currencyService.ConvertFromPrimaryStoreCurrencyAsync(
                                        amount.price,
                                        await _workContext.GetWorkingCurrencyAsync()
                                    );
                                pickupPointModel.PickupFee =
                                    await _priceFormatter.FormatShippingPriceAsync(
                                        priceAmount,
                                        true
                                    );
                            }

                            return pickupPointModel;
                        })
                        .ToListAsync();
                else
                    foreach (var error in pickupPointsResponse.Errors)
                        model.PickupPointsModel.Warnings.Add(error);
            }

            //only available pickup points
            var shippingProviders = await _shippingPluginManager.LoadActivePluginsAsync(
                currentCustomer,
                currentStore.Id
            );
            if (!shippingProviders.Any())
            {
                if (!pickupPointProviders.Any())
                {
                    model.PickupPointsModel.Warnings.Add(
                        await _localizationService.GetResourceAsync("Checkout.ShippingIsNotAllowed")
                    );
                    model.PickupPointsModel.Warnings.Add(
                        await _localizationService.GetResourceAsync(
                            "Checkout.PickupPoints.NotAvailable"
                        )
                    );
                }
                model.PickupPointsModel.PickupInStoreOnly = true;
                model.PickupPointsModel.PickupInStore = true;
                return model;
            }
        }

        // Prepare B2B ShipToAddress Model of B2B User
        model.ErpShipToAddressId = b2BUser.ErpShipToAddressId;
        var b2CShipToAddress = await _erpShipToAddressService.GetErpShipToAddressByIdAsync(
            b2BUser.ErpShipToAddressId
        );
        var modifiedShipToAddressIdOnCheckout =
            await _genericAttributeService.GetAttributeAsync<int>(
                currentCustomer,
                B2BB2CFeaturesDefaults.ShippingAddressModifiedIdInCheckoutAttribute,
                currentStore.Id
            );
        var nopAddress = new Address();
        if (modifiedShipToAddressIdOnCheckout > 0)
        {
            model.ErpShipToAddressId = modifiedShipToAddressIdOnCheckout;
            var modifiedShipToAddress = await _erpShipToAddressService.GetErpShipToAddressByIdAsync(
                modifiedShipToAddressIdOnCheckout
            );
            if (modifiedShipToAddress != null)
            {
                var nopAddressForModifiedShipToAddress = await _addressService.GetAddressByIdAsync(
                    modifiedShipToAddress.AddressId
                );
                var country = await _countryService.GetCountryByIdAsync(
                    nopAddressForModifiedShipToAddress.CountryId ?? 0
                );
                var stateProvince = await _stateProvinceService.GetStateProvinceByIdAsync(
                    nopAddressForModifiedShipToAddress.StateProvinceId ?? 0
                );
                model.ExistingErpShipToAddresses.Add(
                    new ErpShipToAddressModelForCheckout
                    {
                        Id = modifiedShipToAddress.Id,
                        ShipToCode = modifiedShipToAddress.ShipToCode,
                        ShipToName = modifiedShipToAddress.ShipToName,
                        Address1 = nopAddressForModifiedShipToAddress?.Address1 ?? string.Empty,
                        Address2 = nopAddressForModifiedShipToAddress?.Address2 ?? string.Empty,
                        Suburb = modifiedShipToAddress.Suburb,
                        City = nopAddressForModifiedShipToAddress?.City,
                        StateProvinceName = stateProvince?.Name,
                        ZipPostalCode = nopAddressForModifiedShipToAddress?.ZipPostalCode,
                        CountryName = country?.Name,
                    }
                );
            }
        }
        else
        {
            //Prepare B2B Ship To Address
            var shipToAddresses =
                await _erpShipToAddressService.GetErpShipToAddressesByAccountIdAsync(
                    showHidden: false,
                    isActiveOnly: true,
                    accountId: b2BAccount.Id
                );
            foreach (var shipTo in shipToAddresses)
            {
                nopAddress =
                    await _addressService.GetAddressByIdAsync(shipTo.AddressId) ?? new Address();
                var country = await _countryService.GetCountryByIdAsync(nopAddress.CountryId ?? 0);
                var stateProvince = await _stateProvinceService.GetStateProvinceByIdAsync(
                    nopAddress.StateProvinceId ?? 0
                );
                model.ExistingErpShipToAddresses.Add(
                    new ErpShipToAddressModelForCheckout
                    {
                        Id = shipTo.Id,
                        ShipToCode = shipTo.ShipToCode,
                        ShipToName = shipTo.ShipToName,
                        Address1 = nopAddress?.Address1,
                        Address2 = nopAddress?.Address2,
                        Suburb = shipTo.Suburb,
                        City = nopAddress?.City,
                        StateProvinceName = stateProvince?.Name,
                        ZipPostalCode = nopAddress?.ZipPostalCode,
                        CountryName = country?.Name,
                    }
                );
            }
        }

        model.AllowAddressEdit = await _erpCustomerFunctionalityService.CheckAllowAddressEdit(
            b2BAccount
        );
        model.IsQuoteOrder = await _genericAttributeService.GetAttributeAsync<bool>(
            currentCustomer,
            B2BB2CFeaturesDefaults.B2BQouteOrderAttribute,
            currentStore.Id
        );

        //countries and states
        var countries = await _countryService.GetAllCountriesForShippingAsync();
        var erpShippingAddressModel = new ErpShipToAddressModelForCheckout();
        if (_addressSettings.PreselectCountryIfOnlyOne && countries.Count == 1)
        {
            erpShippingAddressModel.CountryId = countries[0].Id;
        }
        else
        {
            erpShippingAddressModel.AvailableCountries.Add(
                new SelectListItem
                {
                    Text = await _localizationService.GetResourceAsync("Address.SelectCountry"),
                    Value = string.Empty,
                }
            );
        }

        foreach (var c in countries)
        {
            erpShippingAddressModel.AvailableCountries.Add(
                new SelectListItem
                {
                    Text = await _localizationService.GetLocalizedAsync(c, x => x.Name),
                    Value = $"{c.Id}",
                    Selected = c.Id == erpShippingAddressModel.CountryId,
                }
            );
        }

        var states = (
            await _stateProvinceService.GetStateProvincesByCountryIdAsync(
                erpShippingAddressModel.CountryId ?? 0,
                languageId
            )
        ).ToList();
        if (states.Count != 0)
        {
            erpShippingAddressModel.AvailableStates.Add(
                new SelectListItem
                {
                    Text = await _localizationService.GetResourceAsync("Address.SelectState"),
                    Value = string.Empty,
                }
            );

            foreach (var s in states)
            {
                erpShippingAddressModel.AvailableStates.Add(
                    new SelectListItem
                    {
                        Text = await _localizationService.GetLocalizedAsync(s, x => x.Name),
                        Value = $"{s.Id}",
                        Selected = s.Id == erpShippingAddressModel.StateProvinceId,
                    }
                );
            }
        }
        else
        {
            var anyCountrySelected = erpShippingAddressModel.AvailableCountries.Any(x =>
                x.Selected
            );
            erpShippingAddressModel.AvailableStates.Add(
                new SelectListItem
                {
                    Text = await _localizationService.GetResourceAsync(
                        anyCountrySelected ? "Address.OtherNonUS" : "Address.SelectState"
                    ),
                    Value = "0",
                }
            );
        }
        model.SelectedShipToAddress = erpShippingAddressModel;

        model.ErpToDetermineDate = _b2BB2CFeaturesSettings.ERPToDetermineDate;
        if (model.ErpToDetermineDate)
        {
            model.AvailableDeliveryDates.Insert(
                0,
                new SelectListItem
                {
                    Text = await _localizationService.GetResourceAsync(
                        "NopStation.Plugin.B2B.B2BB2CFeatures.DeliveryDate.SelectDeliveryDate"
                    ),
                    Value = string.Empty,
                    Selected = true,
                }
            );
        }

        // we have to load this data as well even if ERPToDetermineDate is enabled (if erp call to determine date failed, we will use this)
        (var minDeliveryDate, var maxDeliveryDate) =
            await _erpCustomerFunctionalityService.GetMinimumAndMaximumDeliveryDateForShippingAddress();

        model.DeliveryDate = DateTime.Now.Date.AddDays(1);
        model.CustomDeliveryDateString = DateTime.Now.Date.AddDays(1).ToString("dd/MM/yyyy");
        model.FormatedDeliveryDate = minDeliveryDate.ToString("dd/MM/yyyy");
        model.MinDeliveryDate = minDeliveryDate.ToString("yyyy-MM-dd"); // html only support this format
        model.MaxDeliveryDate = maxDeliveryDate.ToString("yyyy-MM-dd"); // html only support this format
        model.SelectedShipToAddress = new ErpShipToAddressModelForCheckout
        {
            Id = b2CShipToAddress.Id,
            ShipToCode = b2CShipToAddress.ShipToCode,
            ShipToName = b2CShipToAddress.ShipToName,
            Address1 = nopAddress.Address1,
            Address2 = nopAddress.Address2,
            Suburb = b2CShipToAddress.Suburb,
            City = nopAddress.City,
            StateProvinceName = (
                await _stateProvinceService.GetStateProvinceByIdAsync(
                    nopAddress.StateProvinceId ?? 0
                )
            )?.Name,
            ZipPostalCode = nopAddress.ZipPostalCode,
            CountryName = (
                await _countryService.GetCountryByIdAsync(nopAddress.CountryId ?? 0)
            )?.Name,
            ErpSalesOrganizationId = b2BSalesOrg.Id,
            SalesOrganisationCode = b2BSalesOrg.Code,
        };

        return model;
    }

    public virtual async Task<ErpCheckoutShippingAddressModel> PrepareShippingAddressModelAsync(
        ErpNopUser b2BUser,
        ErpAccount b2BAccount,
        int? selectedCountryId = null,
        bool prePopulateNewAddressWithCustomerFields = false,
        string overrideAttributesXml = ""
    )
    {
        var currentCustomer = await _workContext.GetCurrentCustomerAsync();
        var currentStore = await _storeContext.GetCurrentStoreAsync();
        var model = new ErpCheckoutShippingAddressModel
        {
            AllowPickupInStore = _shippingSettings.AllowPickupInStore,
        };
        if (model.AllowPickupInStore)
        {
            model.DisplayPickupPointsOnMap = _shippingSettings.DisplayPickupPointsOnMap;
            model.GoogleMapsApiKey = _shippingSettings.GoogleMapsApiKey;
            var pickupPointProviders = await _pickupPluginManager.LoadActivePluginsAsync(
                currentCustomer,
                currentStore.Id
            );
            if (pickupPointProviders.Any())
            {
                var cart = await _shoppingCartService.GetShoppingCartAsync(
                    currentCustomer,
                    ShoppingCartType.ShoppingCart,
                    currentStore.Id
                );

                var languageId = (await _workContext.GetWorkingLanguageAsync()).Id;
                var pickupPointsResponse = await _shippingService.GetPickupPointsAsync(
                    cart,
                    await _addressService.GetAddressByIdAsync(
                        currentCustomer.BillingAddressId ?? 0
                    ),
                    currentCustomer,
                    storeId: currentStore.Id
                );
                if (pickupPointsResponse.Success)
                    model.PickupPoints = await pickupPointsResponse
                        .PickupPoints.SelectAwait(async point =>
                        {
                            var country = await _countryService.GetCountryByTwoLetterIsoCodeAsync(
                                point.CountryCode
                            );
                            var state =
                                await _stateProvinceService.GetStateProvinceByAbbreviationAsync(
                                    point.StateAbbreviation,
                                    country?.Id
                                );

                            var pickupPointModel = new CheckoutPickupPointModel
                            {
                                Id = point.Id,
                                Name = point.Name,
                                Description = point.Description,
                                ProviderSystemName = point.ProviderSystemName,
                                Address = point.Address,
                                City = point.City,
                                County = point.County,
                                StateName =
                                    state != null
                                        ? await _localizationService.GetLocalizedAsync(
                                            state,
                                            x => x.Name,
                                            languageId
                                        )
                                        : string.Empty,
                                CountryName =
                                    country != null
                                        ? await _localizationService.GetLocalizedAsync(
                                            country,
                                            x => x.Name,
                                            languageId
                                        )
                                        : string.Empty,
                                ZipPostalCode = point.ZipPostalCode,
                                Latitude = point.Latitude,
                                Longitude = point.Longitude,
                                OpeningHours = point.OpeningHours,
                            };
                            if (point.PickupFee > 0)
                            {
                                var amount = await _taxService.GetShippingPriceAsync(
                                    point.PickupFee,
                                    currentCustomer
                                );
                                var priceAmount =
                                    await _currencyService.ConvertFromPrimaryStoreCurrencyAsync(
                                        amount.price,
                                        await _workContext.GetWorkingCurrencyAsync()
                                    );
                                pickupPointModel.PickupFee =
                                    await _priceFormatter.FormatShippingPriceAsync(
                                        priceAmount,
                                        true
                                    );
                            }

                            return pickupPointModel;
                        })
                        .ToListAsync();
                else
                    foreach (var error in pickupPointsResponse.Errors)
                        model.Warnings.Add(error);
            }

            //only available pickup points
            var shippingProviders = await _shippingPluginManager.LoadActivePluginsAsync(
                currentCustomer,
                currentStore.Id
            );
            if (!shippingProviders.Any())
            {
                if (!pickupPointProviders.Any())
                {
                    model.Warnings.Add(
                        await _localizationService.GetResourceAsync("Checkout.ShippingIsNotAllowed")
                    );
                    model.Warnings.Add(
                        await _localizationService.GetResourceAsync(
                            "Checkout.PickupPoints.NotAvailable"
                        )
                    );
                }
                model.PickupInStoreOnly = true;
                model.PickupInStore = true;
                return model;
            }
        }

        //Prepare B2B Ship To Addresses
        var shipToAddresses = await _erpShipToAddressService.GetErpShipToAddressesByAccountIdAsync(
            showHidden: false,
            isActiveOnly: true,
            accountId: b2BAccount.Id
        );
        foreach (var shipto in shipToAddresses)
        {
            model.ExistingErpShipToAddresses.Add(
                new ErpShipToAddressModelForCheckout
                {
                    Id = shipto.Id,
                    ShipToCode = shipto.ShipToCode,
                    ShipToName = shipto.ShipToName,
                }
            );
        }

        // Prepare B2B ShipToAddress Model of B2B User
        if (b2BUser != null)
            model.ErpShipToAddress.Id = b2BUser.ErpShipToAddressId;

        model.AllowAddressEdit = await _erpCustomerFunctionalityService.CheckAllowAddressEdit(
            b2BAccount
        );
        return model;
    }

    public virtual async Task<ErpOnePageCheckoutModel> PrepareB2BOnePageCheckoutModelAsync(
        IList<ShoppingCartItem> cart,
        ErpNopUser b2BUser
    )
    {
        ArgumentNullException.ThrowIfNull(cart);

        var b2BAccount = await _erpAccountService.GetErpAccountByIdAsync(b2BUser.ErpAccountId);
        var model = new ErpOnePageCheckoutModel
        {
            ShippingRequired = true,
            DisableBillingAddressCheckoutStep =
                _orderSettings.DisableBillingAddressCheckoutStep
                && b2BAccount?.BillingAddressId != null,
            CheckoutErpBillingAddress = await PrepareCheckoutErpBillingAddressModelAsync(
                cart,
                b2BAccount
            ),
            CheckoutErpShipToAddress = await PrepareCheckoutB2BShippingAddressModelAsync(
                cart,
                b2BUser,
                b2BAccount
            ),
            IsQuoteOrder = await _genericAttributeService.GetAttributeAsync<bool>(
                await _workContext.GetWorkingCurrencyAsync(),
                B2BB2CFeaturesDefaults.B2BQouteOrderAttribute,
                (await _storeContext.GetCurrentStoreAsync()).Id
            ),
            DisplayCaptcha =
                await _customerService.IsGuestAsync(
                    await _customerService.GetShoppingCartCustomerAsync(cart)
                )
                && _captchaSettings.Enabled
                && _captchaSettings.ShowOnCheckoutPageForGuests,
            IsReCaptchaV3 = _captchaSettings.CaptchaType == CaptchaType.ReCaptchaV3,
            ReCaptchaPublicKey = _captchaSettings.ReCaptchaPublicKey,
        };

        return model;
    }

    public virtual async Task<ErpOnePageCheckoutModel> PrepareB2COnePageCheckoutModelAsync(
        IList<ShoppingCartItem> cart,
        ErpNopUser b2CUser
    )
    {
        ArgumentNullException.ThrowIfNull(cart);

        var b2BAccount = await _erpAccountService.GetErpAccountByIdAsync(b2CUser.ErpAccountId);
        var model = new ErpOnePageCheckoutModel
        {
            ShippingRequired = true,
            DisableBillingAddressCheckoutStep =
                _orderSettings.DisableBillingAddressCheckoutStep
                && b2BAccount?.BillingAddressId != null,
            CheckoutErpBillingAddress = await PrepareCheckoutErpBillingAddressModelAsync(
                cart,
                b2BAccount
            ),
            CheckoutErpShipToAddress = await PrepareCheckoutB2CShippingAddressModelAsync(
                cart,
                b2CUser,
                b2BAccount
            ),
            IsQuoteOrder = await _genericAttributeService.GetAttributeAsync<bool>(
                await _workContext.GetCurrentCustomerAsync(),
                B2BB2CFeaturesDefaults.B2CQouteOrderAttribute,
                (await _storeContext.GetCurrentStoreAsync()).Id
            ),
            DisplayCaptcha =
                await _customerService.IsGuestAsync(
                    await _customerService.GetShoppingCartCustomerAsync(cart)
                )
                && _captchaSettings.Enabled
                && _captchaSettings.ShowOnCheckoutPageForGuests,
            IsReCaptchaV3 = _captchaSettings.CaptchaType == CaptchaType.ReCaptchaV3,
            ReCaptchaPublicKey = _captchaSettings.ReCaptchaPublicKey,
        };

        return model;
    }

    public async Task<(IList<SelectListItem>, bool)> GetDeliveryDatesByShipToAddressAsync(int shipToAddressId)
    {
        if (!_b2BB2CFeaturesSettings.ERPToDetermineDate)
            return (null, false);

        var deliveryDateResponse = new ErpDeliveryDateResponseModel();

        deliveryDateResponse.DeliveryDates = new List<ERPIntegrationCore.Model.DeliveryDate>();

        if (deliveryDateResponse.IsFullLoadRequired)
            return (null, true);

        if (deliveryDateResponse.DeliveryDates.Count <= 0)
            return (null, false);

        var availableDeliveryDates = deliveryDateResponse.DeliveryDates
                .Select(deliveryDate => new SelectListItem(deliveryDate.DelDate, deliveryDate.DelDate))
                .ToList();

        return (availableDeliveryDates, true);
    }

    private async Task<(IList<SelectListItem>, bool)> GetDeliveryDatesByAreaAndPlantAsync(string suburb, string city, string warehouseCode)
    {
        if (!_b2BB2CFeaturesSettings.ERPToDetermineDate)
            return (null, false);

        var customer = await _workContext.GetCurrentCustomerAsync();

        var erpAccount = await _erpAccountService.GetActiveErpAccountByCustomerIdAsync(
            customer.Id
        );

        #region Call By Suburb

        var deliveryDateResponse =
            await _erpDeliveryDatesService.GetDeliveryDatesForAreaAndWarehouseAsync(
                erpAccount,
                suburb,
                warehouseCode
            );

        if (deliveryDateResponse != null && deliveryDateResponse.IsFullLoadRequired)
            return (null, true);

        #endregion Call By Suburb

        #region Call By City

        if (deliveryDateResponse == null || deliveryDateResponse.DeliveryDates.Count < 1)
        {
            deliveryDateResponse =
                await _erpDeliveryDatesService.GetDeliveryDatesForAreaAndWarehouseAsync(
                    erpAccount,
                    city,
                    warehouseCode
                );
        }

        if (deliveryDateResponse != null && deliveryDateResponse.IsFullLoadRequired)
            return (null, true);

        #endregion Call By City

        if (deliveryDateResponse == null || deliveryDateResponse.DeliveryDates.Count < 1)
        {
            deliveryDateResponse =
                await _erpDeliveryDatesService.GetDeliveryDatesForAreaAndWarehouseAsync(
                    erpAccount,
                    "OTHER",
                    warehouseCode
                );

            if (deliveryDateResponse == null || deliveryDateResponse.DeliveryDates?.Count < 1)
                await _erpWorkflowMessageService.SendOrderOrDeliveryDatesOrShippingCostBAPIFailedMessageAsync(
                    customer,
                    (int)ERPFailedTypes.DeliveryDateFails,
                    0
                );

            if (deliveryDateResponse == null)
                return (null, false);

            if (deliveryDateResponse.IsFullLoadRequired)
                return (null, true);

            if (deliveryDateResponse.DeliveryDates.Count < 1)
                return (null, false);
        }

        var availableDeliveryDates = deliveryDateResponse.DeliveryDates
            .Select(deliveryDate => new SelectListItem(deliveryDate.DelDate, deliveryDate.DelDate))
            .ToList();

        return (availableDeliveryDates, true);
    }

    public async Task<CheckoutErpShippingAddressModel> PrepareCheckoutB2CShippingAddressModelAsync(
        IList<ShoppingCartItem> cart,
        ErpNopUser b2CUser,
        ErpAccount b2BAccount
    )
    {
        ArgumentNullException.ThrowIfNull(cart);
        ArgumentNullException.ThrowIfNull(b2CUser);
        ArgumentNullException.ThrowIfNull(b2BAccount);

        var b2BSalesOrg = await _erpSalesOrgService.GetErpSalesOrgByIdAsync(b2BAccount.ErpSalesOrgId);
        var b2CShipToAddress = await _erpShipToAddressService.GetErpShipToAddressByIdAsync(b2CUser.ErpShipToAddressId);
        var nopAddress = await _addressService.GetAddressByIdAsync(b2CShipToAddress?.AddressId ?? 0);

        var model = new CheckoutErpShippingAddressModel();

        if (b2BSalesOrg == null || b2CShipToAddress == null || nopAddress == null)
            return model;

        var currStore = await _storeContext.GetCurrentStoreAsync();
        var currentCustomer = await _workContext.GetCurrentCustomerAsync();

        var isAnySciFrom1041 = false;
        model.AllowAddressEdit = false;
        model.B2CShipToAddressId = b2CUser.ErpShipToAddressId;
        model.DeliveryOptions = (DeliveryOption)b2CShipToAddress.DeliveryOptionId;
        model.IsQuoteOrder = await _genericAttributeService.GetAttributeAsync<bool>(
            currentCustomer,
            B2BB2CFeaturesDefaults.B2CQouteOrderAttribute,
            currStore.Id
        );
        model.PickupInStore = model.PickupInStoreOnly = model.DeliveryOptions == DeliveryOption.Collect;
        model.SpecialInstructions = await _genericAttributeService.GetAttributeAsync<string>(
            currentCustomer,
            B2BB2CFeaturesDefaults.B2CSpecialInstructions,
            currStore.Id
        );

        model.CustomerReference = await _genericAttributeService.GetAttributeAsync<string>(currentCustomer, B2BB2CFeaturesDefaults.B2CCustomerReferenceAsPO, currStore.Id);
        model.ThemeName = (await _settingService.LoadSettingAsync<StoreInformationSettings>(currStore.Id)).DefaultStoreTheme;
        model.ErpToDetermineDate = _b2BB2CFeaturesSettings.ERPToDetermineDate;

        // we have to load this data as well even if ERPToDetermineDate is enabled (if erp call to determine date failed, we will use this)
        (var minDeliveryDate, var maxDeliveryDate) =
            await _erpCustomerFunctionalityService.GetMinimumAndMaximumDeliveryDateForShippingAddress();
        model.DeliveryDate = minDeliveryDate;
        model.CustomDeliveryDateString = DateTime.Now.Date.AddDays(1).ToString("dd/MM/yyyy");
        model.FormatedDeliveryDate = minDeliveryDate.ToString("dd/MM/yyyy");
        model.MinDeliveryDate = minDeliveryDate.ToString("yyyy-MM-dd"); // html only support this format
        model.MaxDeliveryDate = maxDeliveryDate.ToString("yyyy-MM-dd"); // html only support this format

        model.SelectedShipToAddress = new ErpShipToAddressModelForCheckout
        {
            Id = b2CShipToAddress.Id,
            ShipToCode = b2CShipToAddress.ShipToCode,
            ShipToName = b2CShipToAddress.ShipToName,
            Address1 = nopAddress?.Address1,
            Address2 = nopAddress?.Address2,
            Suburb = b2CShipToAddress.Suburb,
            City = nopAddress?.City,
            StateProvinceName = (await _stateProvinceService.GetStateProvinceByIdAsync(
                           nopAddress.StateProvinceId ?? 0))
                          ?.Name,
            ZipPostalCode = nopAddress?.ZipPostalCode,
            CountryName = (await _countryService.GetCountryByIdAsync(nopAddress.CountryId ?? 0))
                            ?.Name,
            ErpSalesOrganizationId = b2BSalesOrg.Id,
            SalesOrganisationCode = b2BSalesOrg.Code,
            DeliveryOptionId = (int)model.DeliveryOptions,
        };

        model.PickupPointsModel = new CheckoutPickupPointsModel
        {
            AllowPickupInStore = _shippingSettings.AllowPickupInStore,
        };
        model.B2CUserId = b2CUser.Id;
        model.ErpAccountId = b2BAccount.Id;
        model.IsB2BUser = false;

        model.DisplayPickupInStore = _orderSettings.DisplayPickupInStoreOnShippingMethodPage;
        var languageId = (await _workContext.GetWorkingLanguageAsync()).Id;

        var shoppingCartModel = await _shoppingCartModelFactory.PrepareShoppingCartModelAsync(new ShoppingCartModel(), cart);

        var invalidDataMessages = new List<string>();
        foreach (var item in shoppingCartModel.Items)
        {
            var product = await _productService.GetProductByIdAsync(item.ProductId);
            if (product == null)
            {
                invalidDataMessages.Add($"Product (Id: {item.ProductId}) not found for shopping cart item (Id: {item.Id}) of B2C User (Id: {b2CUser.Id})");
                continue;
            }

            var b2CSciModel = model.B2CShoppingCartItemModels
                .Find(x => x.ShoppingCartItemModelId == item.Id) ?? new B2CShoppingCartItemModel();
            b2CSciModel.ShoppingCartItemModel = item;
            b2CSciModel.ShoppingCartItemModelId = item.Id;

            #region Warehouse

            var salesOrgWarehouse = await _erpWarehouseSalesOrgMapService.GetB2CSalesOrgWarehouseMapForProduct(product, b2BAccount.ErpSalesOrgId, item.Quantity);

            if (salesOrgWarehouse == null || salesOrgWarehouse.NopWarehouseId == 0)
            {
                invalidDataMessages.Add($"Sales Org Warehouse for sales org (Id: {b2BAccount.ErpSalesOrgId}) and B2C User (Id: {b2CUser.Id}) not found for product (Sku: {product.Sku})");
                continue;
            }

            var warehouse = await _shippingService.GetWarehouseByIdAsync(salesOrgWarehouse.NopWarehouseId);
            if (warehouse == null)
            {
                invalidDataMessages.Add($"Warehouse for sales org (Id: {b2BAccount.ErpSalesOrgId}) and B2C User (Id: {b2CUser.Id}) not found for product (Sku: {product.Sku})");
                continue;
            }

            b2CSciModel.NopWarehouse = warehouse;
            b2CSciModel.NopWarehouseId = warehouse.Id;
            b2CSciModel.NopWarehouseName = warehouse.Name;
            b2CSciModel.NopWarehouseAddress = await _addressService.GetAddressByIdAsync(warehouse?.AddressId ?? 0);
            b2CSciModel.B2CSalesOrgWarehouse = salesOrgWarehouse;
            b2CSciModel.B2CSalesOrgWarehouseId = salesOrgWarehouse.Id;
            b2CSciModel.WarehouseCode = salesOrgWarehouse.WarehouseCode ?? string.Empty;
            b2CSciModel.SpecialInstructions = b2CSciModel.SpecialInstructions ?? "";

            var b2cShoppingCartItem = await _b2cShoppingCartItemService.GetB2CShoppingCartItemByNopShoppingCartItemIdAsync(item.Id);
            if (b2cShoppingCartItem == null)
            {
                await _b2cShoppingCartItemService.InsertB2CShoppingCartItemAsync(new B2CShoppingCartItem
                {
                    ShoppingCartItemId = item.Id,
                    NopWarehouseId = warehouse.Id,
                    //WarehouseCode = salesOrgWarehouse.WarehouseCode,
                    SpecialInstructions = b2CSciModel.SpecialInstructions
                });
            }
            else
            {
                b2cShoppingCartItem.NopWarehouseId = b2CSciModel.NopWarehouseId;
                b2cShoppingCartItem.WarehouseCode = b2CSciModel.WarehouseCode;
                b2cShoppingCartItem.SpecialInstructions = b2CSciModel.SpecialInstructions;

                await _b2cShoppingCartItemService.UpdateB2CShoppingCartItemAsync(b2cShoppingCartItem);
            }

            // check if shopping cart item is from warehouse 1041
            //isAnySciFrom1041 = isAnySciFrom1041 ? isAnySciFrom1041 : b2CSciModel.WarehouseCode == "1041";
            isAnySciFrom1041 |= b2CSciModel.WarehouseCode == "1041";

            #endregion Warehouse

            #region Delivery dates

            if (model.DeliveryOptions == DeliveryOption.DeliverOrCollect)
            {
                var cacheKey = new CacheKey(string.Format(B2BB2CFeaturesDefaults.ERPDeliveryDatesforPlantByFiltersCacheKey, currentCustomer.Id,
                     b2BSalesOrg.Code, b2CShipToAddress.Suburb, nopAddress.City, b2CSciModel.WarehouseCode))
                {
                    CacheTime = _b2BB2CFeaturesSettings.DeliveryDateCacheTime
                };

                if (model.ErpToDetermineDate)
                {
                    (b2CSciModel.AvailableDeliveryDates, b2CSciModel.IsDeliveryDatesCallSuccessful) =

                    await _staticCacheManager.GetAsync(cacheKey, async () =>
                    {
                        (var deliveryDates, var isSuccessful) = await GetDeliveryDatesByAreaAndPlantAsync(b2CShipToAddress.Suburb, nopAddress.City, b2CSciModel.WarehouseCode);
                        return (deliveryDates, isSuccessful);
                    });

                    if (b2CSciModel.IsDeliveryDatesCallSuccessful && (b2CSciModel.AvailableDeliveryDates == null || b2CSciModel.AvailableDeliveryDates.Count < 1))
                        b2CSciModel.IsFullLoadRequired = true;
                    else
                        b2CSciModel.IsFullLoadRequired = false;
                }
            }

            #endregion Delivery dates

            if (!model.B2CShoppingCartItemModels.Exists(x => x.ShoppingCartItemModelId == item.Id))
                model.B2CShoppingCartItemModels.Add(b2CSciModel);
        }
        if (invalidDataMessages.Count > 0)
        {
            await _erpLogsService.InsertErpLogAsync(
                ErpLogLevel.Warning,
                ErpSyncLevel.Order,
                "B2C Shipping Address Model Preparation: Null/Invalid Data Found. Click view to see details.",
                string.Join("\n", invalidDataMessages)
            );
        }

        if (model.PickupPointsModel.AllowPickupInStore)
        {
            model.PickupPointsModel.DisplayPickupPointsOnMap = _shippingSettings.DisplayPickupPointsOnMap;
            model.PickupPointsModel.GoogleMapsApiKey = _shippingSettings.GoogleMapsApiKey;
            var pickupPointProviders = await _pickupPluginManager.LoadActivePluginsAsync(
                currentCustomer,
                currStore.Id
            );
            if (pickupPointProviders.Any())
            {
                var nopAddressOfTradingWarehouse = await _addressService
                    .GetAddressByIdAsync((await _shippingService.GetWarehouseByIdAsync(b2BSalesOrg.TradingWarehouseId ?? 0))?.AddressId ?? 0);

                var pickupPointsResponse = await _shippingService.GetPickupPointsAsync(
                    cart,
                    address: nopAddressOfTradingWarehouse,
                    customer: currentCustomer,
                    storeId: currStore.Id
                );
                if (pickupPointsResponse.Success)
                    model.PickupPointsModel.PickupPoints = await pickupPointsResponse
                        .PickupPoints.SelectAwait(async point =>
                        {
                            var country = await _countryService.GetCountryByTwoLetterIsoCodeAsync(
                                point.CountryCode
                            );
                            var state =
                                await _stateProvinceService.GetStateProvinceByAbbreviationAsync(
                                    point.StateAbbreviation,
                                    country?.Id
                                );

                            var pickupPointModel = new CheckoutPickupPointModel
                            {
                                Id = point.Id,
                                Name = point.Name,
                                Description = point.Description,
                                ProviderSystemName = point.ProviderSystemName,
                                Address = point.Address,
                                City = point.City,
                                County = point.County,
                                StateName =
                                    state != null
                                        ? await _localizationService.GetLocalizedAsync(
                                            state,
                                            x => x.Name,
                                            languageId
                                        )
                                        : string.Empty,
                                CountryName =
                                    country != null
                                        ? await _localizationService.GetLocalizedAsync(
                                            country,
                                            x => x.Name,
                                            languageId
                                        )
                                        : string.Empty,
                                ZipPostalCode = point.ZipPostalCode,
                                Latitude = point.Latitude,
                                Longitude = point.Longitude,
                                OpeningHours = point.OpeningHours,
                            };
                            if (point.PickupFee > 0)
                            {
                                var amount = await _taxService.GetShippingPriceAsync(
                                    point.PickupFee,
                                    currentCustomer
                                );
                                var priceAmount =
                                    await _currencyService.ConvertFromPrimaryStoreCurrencyAsync(
                                        amount.price,
                                        await _workContext.GetWorkingCurrencyAsync()
                                    );
                                pickupPointModel.PickupFee =
                                    await _priceFormatter.FormatShippingPriceAsync(
                                        priceAmount,
                                        true
                                    );
                            }

                            return pickupPointModel;
                        })
                        .ToListAsync();
                else
                    foreach (var error in pickupPointsResponse.Errors)
                        model.PickupPointsModel.Warnings.Add(error);
            }

            //only available pickup points
            var shippingProviders = await _shippingPluginManager.LoadActivePluginsAsync(
                currentCustomer,
                currStore.Id
            );
            if (!shippingProviders.Any())
            {
                if (!pickupPointProviders.Any())
                {
                    model.PickupPointsModel.Warnings.Add(
                        await _localizationService.GetResourceAsync("Checkout.ShippingIsNotAllowed")
                    );
                    model.PickupPointsModel.Warnings.Add(
                        await _localizationService.GetResourceAsync(
                            "Checkout.PickupPoints.NotAvailable"
                        )
                    );
                }
                model.PickupPointsModel.PickupInStoreOnly = true;
                model.PickupPointsModel.PickupInStore = true;
                return model;
            }
            else
            {
                decimal? b2CShippingCost = null;

                // only make the erp call for shipping if there's any shopping cart item from 1041
                // if there's no item from 1041, it means there are items from 4041 only. set cost to 0
                if (b2BSalesOrg.Code == "1040")
                {
                    if (isAnySciFrom1041)
                    {
                        b2CShippingCost = await _erpShippingService.GetB2CShippingCostAsync(cart, currentCustomer, b2CShipToAddress);
                    }
                    else
                    {
                        await _erpLogsService.InformationAsync(
                            $"B2B Checkout: No items from 1041 for customer: {currentCustomer.Email}, shipping cost set to 0.",
                            ErpSyncLevel.Order
                        );
                        b2CShippingCost = 0;
                    }
                }
                else
                {
                    b2CShippingCost = await _erpShippingService.GetB2CShippingCostAsync(cart, currentCustomer, b2CShipToAddress);
                }

                if (b2CShippingCost == null)
                {
                    model.DeliveryOptions = DeliveryOption.Collect;
                    model.PickupInStore = model.PickupInStoreOnly = true;
                    model.IsB2CShippingCostERPCallUnSuccessful = true;

                    await _erpLogsService.InformationAsync(
                        $"B2B Checkout: Get Shipping Cost From ERP returned null for customer {currentCustomer.Email}",
                        ErpSyncLevel.Order
                    );
                }
                else
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

                    // Save the attribute asynchronously
                    await _genericAttributeService.SaveAttributeAsync(
                        currentCustomer,
                        NopCustomerDefaults.SelectedShippingOptionAttribute,
                        b2CShippingOption,
                        (await _storeContext.GetCurrentStoreAsync()).Id
                    );
                }
            }
        }

        model.ErpShipToAddressId = b2CUser.ErpShipToAddressId;

        if (model.ErpToDetermineDate)
        {
            model.AvailableDeliveryDates.Insert(
                0,
                new SelectListItem
                {
                    Text = await _localizationService.GetResourceAsync(
                        "NopStation.Plugin.B2B.B2BB2CFeatures.DeliveryDate.SelectDeliveryDate"
                    ),
                    Value = string.Empty,
                    Selected = true,
                }
            );
        }

        b2CUser.LastWarehouseCalculationTimeUtc = DateTime.UtcNow;
        await _erpNopUserService.UpdateErpNopUserAsync(b2CUser);

        return model;
    }

    public async Task<(IList<SelectListItem>, bool)> GetDeliveryDatesBySuburbOrCityAsync(string suburb, string city)
    {
        if (!_b2BB2CFeaturesSettings.ERPToDetermineDate)
            return (null, false);

        var currentCustomer = await _workContext.GetCurrentCustomerAsync();
        var b2bAccount = await _erpCustomerFunctionalityService.GetActiveErpAccountByCustomerAsync(currentCustomer);

        #region Call By Suburb

        var deliveryDateResponse = await _erpDeliveryDatesService.GetDeliveryDatesForSuburbOrCityAsync(b2bAccount, suburb?.ToUpper());
        if (deliveryDateResponse != null && deliveryDateResponse.IsFullLoadRequired)
            return (null, true);

        #endregion Call By Suburb

        #region Call By City

        if (deliveryDateResponse == null || deliveryDateResponse.DeliveryDates.Count < 1)
            deliveryDateResponse = await _erpDeliveryDatesService.GetDeliveryDatesForSuburbOrCityAsync(b2bAccount, city?.ToUpper());

        if (deliveryDateResponse != null && deliveryDateResponse.IsFullLoadRequired)
            return (null, true);

        #endregion Call By City

        if (deliveryDateResponse == null || deliveryDateResponse.DeliveryDates.Count < 1)
        {
            deliveryDateResponse = await _erpDeliveryDatesService.GetDeliveryDatesForSuburbOrCityAsync(b2bAccount, "OTHER");

            if (deliveryDateResponse == null || deliveryDateResponse.DeliveryDates?.Count < 1) 
                await _erpWorkflowMessageService.SendOrderOrDeliveryDatesOrShippingCostBAPIFailedMessageAsync(currentCustomer, (int)ERPFailedTypes.DeliveryDateFails, 0);

            if (deliveryDateResponse == null)
                return (null, false);

            if (deliveryDateResponse.IsFullLoadRequired)
                return (null, true);

            if (deliveryDateResponse.DeliveryDates.Count < 1)
                return (null, false);
        }

        var deliveryDates = deliveryDateResponse.DeliveryDates;
        var availableDeliveryDates = new List<SelectListItem>();
        availableDeliveryDates = deliveryDates
                .Select(deliveryDate => new SelectListItem(deliveryDate.DelDate, deliveryDate.DelDate))
                .ToList();

        return (availableDeliveryDates, true);
    }

    #endregion Method
}