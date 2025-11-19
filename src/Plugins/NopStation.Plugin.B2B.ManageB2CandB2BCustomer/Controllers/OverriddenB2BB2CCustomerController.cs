using System;
using System.Linq;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using Nop.Core;
using Nop.Core.Caching;
using Nop.Core.Domain;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Directory;
using Nop.Core.Domain.Forums;
using Nop.Core.Domain.Gdpr;
using Nop.Core.Domain.Localization;
using Nop.Core.Domain.Media;
using Nop.Core.Domain.Messages;
using Nop.Core.Domain.Security;
using Nop.Core.Domain.Shipping;
using Nop.Core.Domain.Tax;
using Nop.Core.Events;
using Nop.Core.Infrastructure;
using Nop.Services.Attributes;
using Nop.Services.Authentication;
using Nop.Services.Authentication.External;
using Nop.Services.Authentication.MultiFactor;
using Nop.Services.Catalog;
using Nop.Services.Common;
using Nop.Services.Configuration;
using Nop.Services.Customers;
using Nop.Services.Directory;
using Nop.Services.ExportImport;
using Nop.Services.Gdpr;
using Nop.Services.Helpers;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Media;
using Nop.Services.Messages;
using Nop.Services.Orders;
using Nop.Services.Security;
using Nop.Services.Shipping;
using Nop.Services.Tax;
using Nop.Web.Factories;
using Nop.Web.Framework.Mvc.Filters;
using NopStation.Plugin.B2B.B2BB2CFeatures;
using NopStation.Plugin.B2B.B2BB2CFeatures.Contexts;
using NopStation.Plugin.B2B.B2BB2CFeatures.Controllers;
using NopStation.Plugin.B2B.B2BB2CFeatures.Factories;
using NopStation.Plugin.B2B.B2BB2CFeatures.Model.Registration;
using NopStation.Plugin.B2B.B2BB2CFeatures.Services.Customers;
using NopStation.Plugin.B2B.B2BB2CFeatures.Services.ErpCustomerFunctionality;
using NopStation.Plugin.B2B.B2BB2CFeatures.Services.ErpWorkflowMessage;
using NopStation.Plugin.B2B.B2BB2CFeatures.Services.SoltrackIntegration;
using NopStation.Plugin.B2B.ErpDataScheduler.Services.SyncTaskServices;
using NopStation.Plugin.B2B.ERPIntegrationCore;
using NopStation.Plugin.B2B.ERPIntegrationCore.Domain;
using NopStation.Plugin.B2B.ERPIntegrationCore.Enums;
using NopStation.Plugin.B2B.ERPIntegrationCore.Services;
using NopStation.Plugin.B2B.ERPIntegrationCore.Services.ErpUserRegistrationInfoService;
using NopStation.Plugin.B2B.ManageB2CandB2BCustomer.Factories;
using NopStation.Plugin.B2B.ManageB2CandB2BCustomer.Models;

namespace NopStation.Plugin.B2B.ManageB2CandB2BCustomer.Controllers;

public class OverriddenB2BB2CCustomerController : B2BB2CCustomerController
{
    #region Fields

    private readonly IB2BB2CWorkContext _b2BB2CWorkContext;
    private readonly IB2BRegisterModelFactory _b2BRegisterModelFactory;
    private readonly IErpAccountService _erpAccountService;
    private readonly IErpNopUserService _erpNopUserService;
    private readonly IErpNopUserAccountMapService _erpNopUserAccountMapService;
    private readonly IErpShipToAddressService _erpShipToAddressService;
    private readonly IErpLogsService _erpLogsService;
    private readonly IErpActivityLogsService _erpActivityLogsService;
    private readonly IB2CRegisterModelFactory _b2CRegisterModelFactory;
    private readonly ISoltrackIntegrationService _soltrackIntegrationService;
    private readonly B2BB2CFeaturesSettings _b2BB2CFeaturesSettings;
    private readonly IErpUserRegistrationInfoService _erpUserRegistrationInfoService;
    private readonly IWebHelper _webHelper;
    private readonly IErpCustomerFunctionalityService _erpCustomerFunctionalityService;
    private readonly IErpSalesOrgService _erpSalesOrgService;
    private readonly IErpWarehouseSalesOrgMapService _erpWarehouseSalesOrgMapService;
    private readonly IShippingService _shippingService;
    private readonly IErpWorkflowMessageService _erpWorkflowMessageService;

    #endregion Fields

    #region Ctor

    public OverriddenB2BB2CCustomerController(
        CaptchaSettings captchaSettings,
        CustomerSettings customerSettings,
        DateTimeSettings dateTimeSettings,
        GdprSettings gdprSettings,
        IAddressService addressService,
        IAuthenticationService authenticationService,
        ICountryService countryService,
        IAttributeParser<CustomerAttribute, CustomerAttributeValue> customerAttributeParser,
        IAttributeService<CustomerAttribute, CustomerAttributeValue> customerAttributeService,
        ICustomerRegistrationService customerRegistrationService,
        ICustomerService customerService,
        IEventPublisher eventPublisher,
        IGdprService gdprService,
        IGenericAttributeService genericAttributeService,
        ILocalizationService localizationService,
        ILogger logger,
        INewsLetterSubscriptionService newsLetterSubscriptionService,
        IStateProvinceService stateProvinceService,
        IStoreContext storeContext,
        ITaxService taxService,
        IWorkContext workContext,
        IB2BB2CWorkContext b2BB2CWorkContext,
        IWorkflowMessageService workflowMessageService,
        LocalizationSettings localizationSettings,
        TaxSettings taxSettings,
        IB2BRegisterModelFactory b2BRegisterModelFactory,
        IErpAccountService erpAccountService,
        IErpNopUserService erpNopUserService,
        IErpNopUserAccountMapService erpNopUserAccountMapService,
        IErpShipToAddressService erpShipToAddressService,
        ISettingService settingService,
        INotificationService notificationService,
        ForumSettings forumSettings,
        IAddressModelFactory addressModelFactory,
        ICustomerModelFactory customerModelFactory,
        AddressSettings addressSettings,
        HtmlEncoder htmlEncoder,
        IAttributeParser<AddressAttribute, AddressAttributeValue> addressAttributeParser,
        ICurrencyService currencyService,
        ICustomerActivityService customerActivityService,
        IDownloadService downloadService,
        IExportManager exportManager,
        IExternalAuthenticationService externalAuthenticationService,
        IGiftCardService giftCardService,
        IMultiFactorAuthenticationPluginManager multiFactorAuthenticationPluginManager,
        IOrderService orderService,
        IPermissionService permissionService,
        IPictureService pictureService,
        IPriceFormatter priceFormatter,
        IProductService productService,
        MediaSettings mediaSettings,
        MultiFactorAuthenticationSettings multiFactorAuthenticationSettings,
        StoreInformationSettings storeInformationSettings,
        IErpLogsService erpLogsService,
        IErpIntegrationPluginManager erpIntegrationPluginManager,
        B2BB2CFeaturesSettings b2BB2CFeaturesSettings,
        IStaticCacheManager staticCacheManager,
        IErpActivityLogsService erpActivityLogsService,
        IErpAccountCustomerRegistrationFormService erpAccountCustomerRegistrationFormService,
        IErpAccountCustomerRegistrationBankingDetailsService erpAccountCustomerRegistrationBankingDetailsService,
        IErpAccountCustomerRegistrationPremisesService erpAccountCustomerRegistrationPremisesService,
        IErpAccountCustomerRegistrationPhysicalTradingAddressService erpAccountCustomerRegistrationPhysicalTradingAddressService,
        IErpAccountCustomerRegistrationTradeReferencesService erpAccountCustomerRegistrationTradeReferencesService,
        IErpWorkflowMessageService erpWorkflowMessageService,
        IB2CRegisterModelFactory b2CRegisterModelFactory,
        ICommonHelperService commonHelperService,
        IErpUserRegistrationInfoService erpUserRegistrationInfoService,
        IWebHelper webHelper,
        IErpCustomerFunctionalityService erpCustomerFunctionalityService,
        IErpAccountSyncService erpAccountSyncService,
        IErpShipToAddressSyncService erpShipToAddressSyncService,
        IErpSalesOrgService erpSalesOrgService,
        ISoltrackIntegrationService soltrackIntegrationService,
        IShoppingCartService shoppingCartService,
        IErpWarehouseSalesOrgMapService erpWarehouseSalesOrgMapService,
        IShippingService shippingService
    )
        : base(
            captchaSettings,
            customerSettings,
            dateTimeSettings,
            gdprSettings,
            addressService,
            authenticationService,
            countryService,
            customerAttributeParser,
            customerAttributeService,
            customerRegistrationService,
            customerService,
            eventPublisher,
            gdprService,
            genericAttributeService,
            localizationService,
            logger,
            newsLetterSubscriptionService,
            stateProvinceService,
            storeContext,
            taxService,
            workContext,
            b2BB2CWorkContext,
            workflowMessageService,
            localizationSettings,
            taxSettings,
            b2BRegisterModelFactory,
            erpAccountService,
            erpNopUserService,
            erpNopUserAccountMapService,
            erpShipToAddressService,
            settingService,
            notificationService,
            forumSettings,
            addressModelFactory,
            customerModelFactory,
            addressSettings,
            htmlEncoder,
            addressAttributeParser,
            currencyService,
            customerActivityService,
            downloadService,
            exportManager,
            externalAuthenticationService,
            giftCardService,
            multiFactorAuthenticationPluginManager,
            orderService,
            permissionService,
            pictureService,
            priceFormatter,
            productService,
            mediaSettings,
            multiFactorAuthenticationSettings,
            storeInformationSettings,
            erpLogsService,
            erpIntegrationPluginManager,
            b2BB2CFeaturesSettings,
            staticCacheManager,
            erpActivityLogsService,
            erpAccountCustomerRegistrationFormService,
            erpAccountCustomerRegistrationBankingDetailsService,
            erpAccountCustomerRegistrationPremisesService,
            erpAccountCustomerRegistrationPhysicalTradingAddressService,
            erpAccountCustomerRegistrationTradeReferencesService,
            erpWorkflowMessageService,
            commonHelperService,
            erpCustomerFunctionalityService,
            shoppingCartService,
            soltrackIntegrationService,
            erpSalesOrgService
        )
    {
        _b2BB2CWorkContext = b2BB2CWorkContext;
        _b2BRegisterModelFactory = b2BRegisterModelFactory;
        _erpAccountService = erpAccountService;
        _erpNopUserService = erpNopUserService;
        _erpNopUserAccountMapService = erpNopUserAccountMapService;
        _erpShipToAddressService = erpShipToAddressService;
        _erpLogsService = erpLogsService;
        _erpActivityLogsService = erpActivityLogsService;
        _b2CRegisterModelFactory = b2CRegisterModelFactory;
        _erpUserRegistrationInfoService = erpUserRegistrationInfoService;
        _webHelper = webHelper;
        _erpCustomerFunctionalityService = erpCustomerFunctionalityService;
        _erpSalesOrgService = erpSalesOrgService;
        _b2BB2CFeaturesSettings = b2BB2CFeaturesSettings;
        _soltrackIntegrationService = soltrackIntegrationService;
        _erpWarehouseSalesOrgMapService = erpWarehouseSalesOrgMapService;
        _shippingService = shippingService;
        _erpWorkflowMessageService = erpWorkflowMessageService;
    }

    #endregion Ctor

    #region Utilities

    private async Task<Country> GetCountryByB2CRegisterModelAsync(B2CRegisterModel model)
    {
        if (model == null)
            return null;

        var country =
            await _countryService.GetCountryByIdAsync(model.CountryId)
            ?? await _countryService.GetCountryByTwoLetterIsoCodeAsync(model.CountryCode);

        if (country != null)
        {
            model.CountryId = country.Id;
            return country;
        }

        model.CountryId = _b2BB2CFeaturesSettings.DefaultCountryId;
        return await _countryService.GetCountryByIdAsync(model.CountryId);
    }

    private async Task<StateProvince> GetStateProvinceByB2CRegisterModelAsync(
        B2CRegisterModel model
    )
    {
        if (model == null)
            return null;

        var stateProvince =
            await _stateProvinceService.GetStateProvinceByIdAsync(model.StateProvinceId)
            ?? await _stateProvinceService.GetStateProvinceByAbbreviationAsync(
                model.StateProvinceCode
            );

        if (stateProvince != null)
        {
            model.StateProvinceId = stateProvince.Id;
            return stateProvince;
        }

        stateProvince =
            (
                await _stateProvinceService.GetStateProvincesByCountryIdAsync(model.CountryId)
            )?.FirstOrDefault()
            ?? (
                await _stateProvinceService.GetStateProvincesByCountryIdAsync(
                    _b2BB2CFeaturesSettings.DefaultCountryId
                )
            )?.FirstOrDefault();

        model.StateProvinceId = stateProvince?.Id ?? 0;
        return stateProvince;
    }

    private async Task AddOrRemoveB2CCustomerUnsuccessfulRegisterRole(
        Customer customer,
        bool? removeRole = false
    )
    {
        ArgumentNullException.ThrowIfNull(customer);

        var b2CRegistrationUnsuccessfulRole =
            await _customerService.GetCustomerRoleBySystemNameAsync(
                ManageB2CandB2BCustomerDefaults.B2CRegistrationUnsuccessfulRoleSystemName
            );

        if (b2CRegistrationUnsuccessfulRole == null)
        {
            b2CRegistrationUnsuccessfulRole = new CustomerRole
            {
                Name = ManageB2CandB2BCustomerDefaults.B2CRegistrationUnsuccessfulRoleSystemName,
                Active = true,
                IsSystemRole = false,
                SystemName =
                    ManageB2CandB2BCustomerDefaults.B2CRegistrationUnsuccessfulRoleSystemName,
            };
            await _customerService.InsertCustomerRoleAsync(b2CRegistrationUnsuccessfulRole);
        }

        if (removeRole.HasValue && !removeRole.Value)
        {
            if (
                (await _customerService.GetCustomerRolesAsync(customer))?.Any(role =>
                    role.Id == b2CRegistrationUnsuccessfulRole.Id
                ) ?? false
            )
            {
                return;
            }

            await _customerService.AddCustomerRoleMappingAsync(
                new CustomerCustomerRoleMapping
                {
                    CustomerId = customer.Id,
                    CustomerRoleId = b2CRegistrationUnsuccessfulRole.Id,
                }
            );
        }
        else
        {
            await _customerService.RemoveCustomerRoleMappingAsync(
                customer,
                b2CRegistrationUnsuccessfulRole
            );
        }
    }

    protected virtual async Task<IActionResult> SaveNopCustomerForB2CAsync(
        B2CRegisterModel model,
        string returnUrl,
        bool captchaValid,
        IFormCollection form,
        Customer customer
    )
    {
        try
        {
            if (await _customerService.IsRegisteredAsync(customer))
            {
                await _authenticationService.SignOutAsync();

                await _eventPublisher.PublishAsync(new CustomerLoggedOutEvent(customer));

                await _b2BB2CWorkContext.SetCurrentERPCustomerAsync(
                    await _customerService.InsertGuestCustomerAsync()
                );
            }

            var store = await _storeContext.GetCurrentStoreAsync();
            customer.RegisteredInStoreId = store.Id;

            var customerAttributesXml = await ParseCustomCustomerAttributesAsync(form);

            var customerAttributeWarnings =
                await _customerAttributeParser.GetAttributeWarningsAsync(customerAttributesXml);

            foreach (var error in customerAttributeWarnings)
            {
                ModelState.AddModelError("", error);
            }

            if (
                _captchaSettings.Enabled
                && _captchaSettings.ShowOnRegistrationPage
                && !captchaValid
            )
            {
                ModelState.AddModelError(
                    "",
                    await _localizationService.GetResourceAsync("Common.WrongCaptchaMessage")
                );
            }

            if (_gdprSettings.GdprEnabled)
            {
                var consents = (await _gdprService.GetAllConsentsAsync())
                    .Where(consent => consent.DisplayDuringRegistration && consent.IsRequired)
                    .ToList();

                ValidateRequiredConsentsAsync(consents, form);
            }

            if (ModelState.IsValid)
            {
                var addErpCustomerActivityLogSystemKeyword = "Erp_AddNewB2CCustomer";
                var customerEmail = model.Email?.Trim();
                var deliveryOption = 0;
                var distanceToNearestnearestTradingWarehouse = 0.0;
                var salesOrg = new ErpSalesOrg();
                var nopWarehouse = new Warehouse();
                var errorMessage = string.Empty;

                ErpAccount erpAccountForB2C = null;
                ErpNopUser erpNopUser = null;

                await _genericAttributeService.SaveAttributeAsync<string?>(
                    customer,
                    nameof(ErpUserType),
                    nameof(ErpUserType.B2CUser)
                );

                if (!string.IsNullOrEmpty(model.Latitude) && !string.IsNullOrEmpty(model.Longitude))
                {
                    var (isCustomerInExpressShopZone, isCustomerOnDeliveryRoute, response) =
                        await _soltrackIntegrationService.GetSoltrackResponseAsync(
                            customer,
                            model.Latitude,
                            model.Longitude
                        );

                    if (response is null)
                    {
                        ModelState.AddModelError(
                            "SoltrackResponseError",
                            await _localizationService.GetResourceAsync(
                                "B2BB2C.Account.Registration.AccountNotCreated.SoltrackResponse.Error"
                            )
                        );
                        await _erpLogsService.InsertErpLogAsync(
                            ErpLogLevel.Error,
                            ErpSyncLevel.Account,
                            $"B2C Registration: Soltrack call returned error. Customer email: {model.Email}"
                        );
                    }
                    else
                    {
                        if (_customerSettings.UsernamesEnabled && model.Username != null)
                        {
                            model.Username = model.Username.Trim();
                        }
                        var isApproved =
                            _customerSettings.UserRegistrationType == UserRegistrationType.Standard;

                        await AddOrRemoveB2CCustomerUnsuccessfulRegisterRole(customer: customer);

                        var registrationRequest = new CustomerRegistrationRequest(
                            customer,
                            customerEmail,
                            _customerSettings.UsernamesEnabled ? model.Username : customerEmail,
                            model.Password,
                            _customerSettings.DefaultPasswordFormat,
                            store.Id,
                            isApproved
                        );

                        var registrationResult =
                            await _customerRegistrationService.RegisterCustomerAsync(
                                registrationRequest
                            );

                        if (registrationResult.Success)
                        {
                            //properties
                            if (_dateTimeSettings.AllowCustomersToSetTimeZone)
                                customer.TimeZoneId = model.TimeZoneId;

                            //VAT number
                            if (_taxSettings.EuVatEnabled)
                            {
                                var prevVatNumber = customer.VatNumber;

                                customer.VatNumber = model.VatNumber;

                                if (prevVatNumber != model.VatNumber)
                                {
                                    var vat = await _taxService.GetVatNumberStatusAsync(
                                        model.VatNumber
                                    );
                                    customer.VatNumberStatus = vat.vatNumberStatus;

                                    //send VAT number admin notification
                                    if (
                                        !string.IsNullOrEmpty(model.VatNumber)
                                        && _taxSettings.EuVatEmailAdminWhenNewVatSubmitted
                                    )
                                    {
                                        await _workflowMessageService.SendNewVatSubmittedStoreOwnerNotificationAsync(
                                            customer,
                                            model.VatNumber,
                                            vat.address,
                                            _localizationSettings.DefaultAdminLanguageId
                                        );
                                    }
                                }
                                await _customerService.UpdateCustomerAsync(customer);
                            }

                            await _genericAttributeService.SaveAttributeAsync(
                                customer,
                                B2BB2CFeaturesDefaults.B2CVatNumberAttribute,
                                model.VatNumber
                            );

                            #region Customer Settings check

                            if (_customerSettings.GenderEnabled)
                                customer.Gender = model.Gender;
                            if (_customerSettings.FirstNameEnabled)
                                customer.FirstName = model.FirstName;
                            if (_customerSettings.LastNameEnabled)
                                customer.LastName = model.LastName;
                            if (_customerSettings.DateOfBirthEnabled)
                                customer.DateOfBirth = model.ParseDateOfBirth();
                            if (_customerSettings.CompanyEnabled)
                                customer.Company = model.Company;
                            if (_customerSettings.StreetAddressEnabled)
                                customer.StreetAddress = model.HouseNumber;
                            if (_customerSettings.StreetAddress2Enabled)
                                customer.StreetAddress2 = model.Street;
                            if (_customerSettings.ZipPostalCodeEnabled)
                                customer.ZipPostalCode = model.ZipPostalCode;
                            if (_customerSettings.CityEnabled)
                                customer.City = model.City;
                            if (_customerSettings.CountyEnabled)
                                customer.County = model.County;
                            if (_customerSettings.CountryEnabled)
                                customer.CountryId = model.CountryId;
                            if (
                                _customerSettings.CountryEnabled
                                && _customerSettings.StateProvinceEnabled
                            )
                                customer.StateProvinceId = model.StateProvinceId;
                            if (_customerSettings.PhoneEnabled)
                                customer.Phone = model.Phone;
                            if (_customerSettings.FaxEnabled)
                                customer.Fax = model.Fax;

                            //save customer attributes
                            customer.CustomCustomerAttributesXML = customerAttributesXml;
                            await _customerService.UpdateCustomerAsync(customer);

                            //newsletter
                            if (_customerSettings.NewsletterEnabled)
                            {
                                var isNewsletterActive =
                                    _customerSettings.UserRegistrationType
                                    != UserRegistrationType.EmailValidation;

                                //save newsletter value
                                var newsletter =
                                    await _newsLetterSubscriptionService.GetNewsLetterSubscriptionByEmailAndStoreIdAsync(
                                        customerEmail,
                                        store.Id
                                    );
                                if (newsletter != null)
                                {
                                    if (model.Newsletter)
                                    {
                                        newsletter.Active = isNewsletterActive;
                                        await _newsLetterSubscriptionService.UpdateNewsLetterSubscriptionAsync(
                                            newsletter
                                        );

                                        //GDPR
                                        if (
                                            _gdprSettings.GdprEnabled
                                            && _gdprSettings.LogNewsletterConsent
                                        )
                                        {
                                            await _gdprService.InsertLogAsync(
                                                customer,
                                                0,
                                                GdprRequestType.ConsentAgree,
                                                await _localizationService.GetResourceAsync(
                                                    "Gdpr.Consent.Newsletter"
                                                )
                                            );
                                        }
                                    }
                                    else
                                    {
                                        //When registering, not checking the newsletter check box should not take an existing email address off of the subscription list.
                                        await _newsLetterSubscriptionService.DeleteNewsLetterSubscriptionAsync(
                                            newsletter
                                        );
                                    }
                                }
                                else
                                {
                                    if (model.Newsletter)
                                    {
                                        await _newsLetterSubscriptionService.InsertNewsLetterSubscriptionAsync(
                                            new NewsLetterSubscription
                                            {
                                                NewsLetterSubscriptionGuid = Guid.NewGuid(),
                                                Email = customerEmail,
                                                Active = isNewsletterActive,
                                                StoreId = store.Id,
                                                CreatedOnUtc = DateTime.UtcNow,
                                            }
                                        );

                                        //GDPR
                                        if (
                                            _gdprSettings.GdprEnabled
                                            && _gdprSettings.LogNewsletterConsent
                                        )
                                        {
                                            await _gdprService.InsertLogAsync(
                                                customer,
                                                0,
                                                GdprRequestType.ConsentAgree,
                                                await _localizationService.GetResourceAsync(
                                                    "Gdpr.Consent.Newsletter"
                                                )
                                            );
                                        }
                                    }
                                }
                            }

                            if (_customerSettings.AcceptPrivacyPolicyEnabled)
                            {
                                //privacy policy is required
                                //GDPR
                                if (
                                    _gdprSettings.GdprEnabled
                                    && _gdprSettings.LogPrivacyPolicyConsent
                                )
                                {
                                    await _gdprService.InsertLogAsync(
                                        customer,
                                        0,
                                        GdprRequestType.ConsentAgree,
                                        await _localizationService.GetResourceAsync(
                                            "Gdpr.Consent.PrivacyPolicy"
                                        )
                                    );
                                }
                            }

                            //GDPR
                            if (_gdprSettings.GdprEnabled)
                            {
                                var consents = (await _gdprService.GetAllConsentsAsync())
                                    .Where(consent => consent.DisplayDuringRegistration)
                                    .ToList();
                                foreach (var consent in consents)
                                {
                                    var controlId = $"consent{consent.Id}";
                                    var cbConsent = form[controlId];
                                    if (
                                        !StringValues.IsNullOrEmpty(cbConsent)
                                        && cbConsent.ToString().Equals("on")
                                    )
                                    {
                                        //agree
                                        await _gdprService.InsertLogAsync(
                                            customer,
                                            consent.Id,
                                            GdprRequestType.ConsentAgree,
                                            consent.Message
                                        );
                                    }
                                    else
                                    {
                                        //disagree
                                        await _gdprService.InsertLogAsync(
                                            customer,
                                            consent.Id,
                                            GdprRequestType.ConsentDisagree,
                                            consent.Message
                                        );
                                    }
                                }
                            }

                            #endregion Customer Settings check

                            var country = await GetCountryByB2CRegisterModelAsync(model);
                            var stateProvince = await GetStateProvinceByB2CRegisterModelAsync(
                                model
                            );

                            //insert default address (if possible)
                            var defaultAddress = new Address
                            {
                                Email = customer.Email,
                                FirstName = customer.FirstName,
                                LastName = customer.LastName,
                                Company = model.Company,
                                Address1 = model.HouseNumber,
                                Address2 = model.Street,
                                City = model.CityName,
                                StateProvinceId = stateProvince.Id,
                                CountryId = country.Id,
                                ZipPostalCode = model.PostalCode,
                                PhoneNumber = model.Phone,
                                FaxNumber = model.Fax,
                                CreatedOnUtc = customer.CreatedOnUtc,
                            };

                            if (await _addressService.IsAddressValidAsync(defaultAddress))
                            {
                                //some validation
                                if (defaultAddress.CountryId == 0)
                                    defaultAddress.CountryId = null;
                                if (defaultAddress.StateProvinceId == 0)
                                    defaultAddress.StateProvinceId = null;

                                await _addressService.InsertAddressAsync(defaultAddress);

                                await _customerService.InsertCustomerAddressAsync(
                                    customer,
                                    defaultAddress
                                );

                                customer.BillingAddressId = defaultAddress.Id;
                                customer.ShippingAddressId = defaultAddress.Id;

                                await _customerService.UpdateCustomerAsync(customer);

                                await _erpLogsService.InsertErpLogAsync(
                                    ErpLogLevel.Information,
                                    ErpSyncLevel.Account,
                                    string.Format(
                                        "B2C Registration: Address for customer: {0}, Address Id {1}",
                                        customer.Email,
                                        defaultAddress.Id
                                    )
                                );
                            }

                            //notifications
                            if (_customerSettings.NotifyNewCustomerRegistration)
                                await _workflowMessageService.SendCustomerRegisteredStoreOwnerNotificationMessageAsync(
                                    customer,
                                    _localizationSettings.DefaultAdminLanguageId
                                );

                            //raise event
                            await _eventPublisher.PublishAsync(
                                new CustomerRegisteredEvent(customer)
                            );
                            var shipToAddressIdForB2BB2CUserId = 0;

                            #region B2C

                            if (isCustomerInExpressShopZone)
                            {
                                var customerRole =
                                    await _customerService.GetCustomerRoleBySystemNameAsync(
                                        ManageB2CandB2BCustomerDefaults.B2CNoShopRoleSystemName
                                    );
                                if (customerRole == null)
                                {
                                    customerRole = new CustomerRole
                                    {
                                        Name =
                                            ManageB2CandB2BCustomerDefaults.B2CNoShopRoleSystemName,
                                        Active = true,
                                        IsSystemRole = false,
                                        SystemName =
                                            ManageB2CandB2BCustomerDefaults.B2CNoShopRoleSystemName,
                                    };
                                    await _customerService.InsertCustomerRoleAsync(customerRole);
                                }

                                await _customerService.AddCustomerRoleMappingAsync(
                                    new CustomerCustomerRoleMapping
                                    {
                                        CustomerRoleId = customerRole.Id,
                                        CustomerId = customer.Id,
                                    }
                                );

                                customer.Active = false;
                                await AddOrRemoveB2CCustomerUnsuccessfulRegisterRole(
                                    customer: customer,
                                    removeRole: true
                                );
                                await _customerService.UpdateCustomerAsync(customer);
                                await _authenticationService.SignOutAsync();

                                errorMessage =
                                    $"B2C Registration: Customer {customer.Email} is in no shop zone";
                                await _erpLogsService.InformationAsync(
                                    errorMessage,
                                    ErpSyncLevel.Account,
                                    customer: customer
                                );

                                await _erpUserRegistrationInfoService.InsertErpUserRegistrationInfoAsync(
                                    new ErpUserRegistrationInfo
                                    {
                                        NopCustomerId = customer?.Id ?? 0,
                                        NearestWareHouseId = nopWarehouse?.Id ?? 0,
                                        DistanceToNearestWarehouse =
                                            (decimal)distanceToNearestnearestTradingWarehouse,
                                        DeliveryOptionId = deliveryOption,
                                        Latitude = model.Latitude,
                                        Longitude = model.Longitude,
                                        AddressId = defaultAddress?.Id ?? 0,
                                        HouseNumber = model.HouseNumber,
                                        City = model.CityName,
                                        Suburb = model.Suburb,
                                        Street = model.Street,
                                        PostalCode = model.PostalCode,
                                        Country = model.Country,
                                        ErpSalesOrgId =
                                            erpAccountForB2C?.ErpSalesOrgId ?? 0,
                                        ErpAccountIdForB2C = erpAccountForB2C?.Id ?? 0,
                                        ErpUserId = erpNopUser?.Id ?? 0,
                                        ErrorMessage = errorMessage,
                                        ErpUserTypeId = model.IsB2BUser
                                            ? (int)ErpUserType.B2BUser
                                            : (int)ErpUserType.B2CUser,
                                    }
                                );

                                var redirectUrl = Url.RouteUrl(
                                    "B2CRegisterResult",
                                    new
                                    {
                                        resultId = (int)UserRegistrationType.Standard,
                                        returnUrl,
                                    },
                                    _webHelper.GetCurrentRequestProtocol()
                                );

                                var session = EngineContext
                                    .Current.Resolve<IHttpContextAccessor>()
                                    .HttpContext.Session;
                                session.SetString("ExpressShopCode", response.BranchAreaEntityName);

                                return Redirect(redirectUrl);
                            }
                            else
                            {
                                var nearestTradingWarehouseCode =
                                    response.BranchAreaEntityName ?? "";
                                deliveryOption = isCustomerOnDeliveryRoute
                                    ? (int)DeliveryOption.DeliverOrCollect
                                    : (int)DeliveryOption.Collect;
                                distanceToNearestnearestTradingWarehouse =
                                    response.DistanceFromBranchArea;

                                var salesOrgWarehouse =
                                    await _erpWarehouseSalesOrgMapService.GetErpWarehouseSalesOrgMapByWarehouseCodeAsync(
                                        nearestTradingWarehouseCode,
                                        true
                                    );
                                nopWarehouse = await _shippingService.GetWarehouseByIdAsync(
                                    salesOrgWarehouse?.NopWarehouseId ?? 0
                                );

                                if (nopWarehouse != null && nopWarehouse.Id > 0)
                                {
                                    salesOrg =
                                        await _erpSalesOrgService.GetErpSalesOrgByTradingWarehouseIdAsync(
                                            nopWarehouse.Id
                                        );
                                    erpAccountForB2C =
                                        await _erpAccountService.GetErpAccountByIdAsync(
                                            salesOrg?.ErpAccountIdForB2C ?? 0
                                        );

                                    if (
                                        salesOrg != null
                                        && salesOrg.Id > 0
                                        && salesOrg.ErpAccountIdForB2C > 0
                                        && erpAccountForB2C != null
                                        && erpAccountForB2C.Id > 0
                                    )
                                    {
                                        try
                                        {
                                            if (
                                                country != null
                                                && country.Id > 0
                                                && country.Name?.ToLower() == "south africa"
                                                && stateProvince.Id > 0
                                            )
                                            {
                                                #region Insert ErpShipToAddress

                                                var erpShipToAddress = new ErpShipToAddress
                                                {
                                                    EmailAddresses = customer.Email,
                                                    AddressId = defaultAddress.Id,
                                                    ShipToCode = _erpShipToAddressService.GenerateUniqueShipToCode(),
                                                    ShipToName =
                                                        $"{model.FirstName} {model.LastName}",
                                                    DeliveryNotes = string.Empty,
                                                    RepNumber = model?.RepNumber ?? string.Empty,
                                                    RepFullName =
                                                        model?.RepFullName ?? string.Empty,
                                                    RepPhoneNumber =
                                                        model?.RepPhoneNumber ?? string.Empty,
                                                    RepEmail = model?.RepEmail ?? string.Empty,
                                                    ProvinceCode =
                                                        (
                                                            await _stateProvinceService.GetStateProvinceByIdAsync(
                                                                defaultAddress.StateProvinceId ?? 0
                                                            )
                                                        )?.Abbreviation ?? "",
                                                    IsActive = true,
                                                    CreatedById = customer.Id,
                                                    CreatedOnUtc = DateTime.UtcNow,
                                                    DeliveryOptionId = deliveryOption,
                                                    //RouteCode = routeCode,
                                                    Latitude = model.Latitude,
                                                    Longitude = model.Longitude,
                                                    NearestWareHouseId = nopWarehouse.Id,
                                                    Suburb = model.Suburb,
                                                    DistanceToNearestWareHouse =
                                                        distanceToNearestnearestTradingWarehouse,
                                                    //B2BAccountIdForB2C = salesOrg.B2BAccountIdForB2C ?? 0,
                                                    //ShipToAddressCreatedByTypeId = (int)ShipToAddressCreatedByType.B2CUser,
                                                    UpdatedById = customer.Id,
                                                    UpdatedOnUtc = DateTime.UtcNow,
                                                };

                                                await _erpShipToAddressService.InsertErpShipToAddressAsync(
                                                    erpShipToAddress
                                                );
                                                await _erpShipToAddressService.InsertErpShipToAddressErpAccountMapAsync(
                                                    erpAccountForB2C,
                                                    erpShipToAddress,
                                                    ErpShipToAddressCreatedByType.User
                                                );

                                                await _erpLogsService.InformationAsync(
                                                    $"B2C Registration: {await _localizationService.GetResourceAsync("Admin.ErpShipToAddresss.Added")}, " +
                                                    $"Erp Ship-To-Address Id: {erpShipToAddress.Id}. " +
                                                    $"For registering Customer {customer.Email} (Id: {customer.Id})",
                                                    ErpSyncLevel.Account,
                                                    customer: customer
                                                );

                                                if (shipToAddressIdForB2BB2CUserId == 0)
                                                    shipToAddressIdForB2BB2CUserId =
                                                        erpShipToAddress.Id;

                                                await _erpCustomerFunctionalityService.AddOrUpdateB2CUserSpecialRolesAsync(
                                                    customer,
                                                    oldSalesOrgId: 0,
                                                    newSalesOrgId: salesOrg?.Id ?? 0
                                                );

                                                #endregion Insert ErpShipToAddress

                                                #region Insert ErpNopUser

                                                // if successful erp account created

                                                erpNopUser = new ErpNopUser
                                                {
                                                    NopCustomerId = customer.Id,
                                                    ErpAccountId = erpAccountForB2C.Id,
                                                    ErpShipToAddressId =
                                                        shipToAddressIdForB2BB2CUserId,
                                                    BillingErpShipToAddressId =
                                                        shipToAddressIdForB2BB2CUserId,
                                                    ShippingErpShipToAddressId =
                                                        shipToAddressIdForB2BB2CUserId,
                                                    ErpUserTypeId = model.IsB2BUser
                                                        ? (int)ErpUserType.B2BUser
                                                        : (int)ErpUserType.B2CUser,
                                                    IsActive = true,
                                                    CreatedOnUtc = DateTime.UtcNow,
                                                    CreatedById = customer.Id,
                                                    UpdatedOnUtc = DateTime.UtcNow,
                                                    UpdatedById = customer.Id,
                                                };
                                                await _erpNopUserService.InsertErpNopUserAsync(
                                                    erpNopUser
                                                );

                                                await _erpLogsService.InformationAsync(
                                                    $"B2C Registration: {await _localizationService.GetResourceAsync("Plugin.Misc.NopStation.ERPIntegrationCore.ErpNopUser.Added")}, Erp Nop User Id: {erpNopUser.Id}. For registering Customer {customer.Email} (Id: {customer.Id})",
                                                    ErpSyncLevel.Account,
                                                    customer: customer
                                                );

                                                var erpNopUserAccountMap = new ErpNopUserAccountMap
                                                {
                                                    ErpAccountId = erpAccountForB2C.Id,
                                                    ErpUserId = erpNopUser.Id,
                                                };
                                                await _erpNopUserAccountMapService.InsertErpNopUserAccountMapAsync(
                                                    erpNopUserAccountMap
                                                );

                                                if (erpNopUserAccountMap.Id > 0)
                                                {
                                                    //prepare and save erpNopUser (have to check if erp account created)
                                                    await AddOrRemoveB2CCustomerUnsuccessfulRegisterRole(
                                                        customer,
                                                        true
                                                    );

                                                    var b2CustomerRole =
                                                        await _customerService.GetCustomerRoleBySystemNameAsync(ERPIntegrationCoreDefaults.B2CCustomerRoleSystemName);
                                                    if (b2CustomerRole != null)
                                                    {
                                                        await _customerService.AddCustomerRoleMappingAsync(
                                                            new CustomerCustomerRoleMapping
                                                            {
                                                                CustomerId = customer.Id,
                                                                CustomerRoleId = b2CustomerRole.Id,
                                                            }
                                                        );
                                                    }
                                                    else
                                                    {
                                                        await _erpLogsService.ErrorAsync(
                                                            $"B2C Registration: B2C customer role not found. So b2c customer role hasn't been added to Customer: {customer.Email}",
                                                            ErpSyncLevel.Account,
                                                            customer: customer
                                                        );
                                                    }
                                                }
                                                else
                                                {
                                                    await _erpLogsService.InformationAsync(
                                                        $"B2C Registration: Registration successful! Customer Id: {customer.Id}, but ErpNopUserAccountMap was not created",
                                                        ErpSyncLevel.Account,
                                                        customer: customer
                                                    );
                                                }

                                                #endregion Insert ErpNopUser

                                                // standard registration: everything was successful. log in customer now
                                                if (
                                                    isApproved
                                                    && erpNopUser.Id > 0
                                                    && erpNopUserAccountMap.Id > 0
                                                    && erpShipToAddress.Id > 0
                                                )
                                                {
                                                    await _authenticationService.SignInAsync(
                                                        customer,
                                                        true
                                                    );
                                                }

                                                #region Send email

                                                var currentLanguage =
                                                    await _workContext.GetWorkingLanguageAsync();

                                                // send email message
                                                if (
                                                    !string.IsNullOrEmpty(
                                                        salesOrg.UserRegistrationEmailAdresses
                                                    )
                                                )
                                                {
                                                    var emailAddresses = salesOrg
                                                        .UserRegistrationEmailAdresses.Split(',')
                                                        .ToList();
                                                    foreach (var email in emailAddresses)
                                                    {
                                                        await _erpWorkflowMessageService.SendErpCustomerRegisteredNotificationMessageAsync(
                                                            customer,
                                                            erpAccountForB2C,
                                                            erpNopUser,
                                                            salesOrg,
                                                            erpShipToAddress,
                                                            currentLanguage.Id,
                                                            email,
                                                            null,
                                                            false
                                                        );
                                                    }
                                                }
                                                else
                                                {
                                                    await _erpLogsService.ErrorAsync(
                                                        $"B2C Registration: Email Adresses not found for the Sales Org {salesOrg.Code} (Id: {salesOrg.Id}) for sending mail regarding new Customer {customer.Email} (Id: {customer.Id}) registration.",
                                                        ErpSyncLevel.Account,
                                                        customer: customer
                                                    );
                                                }

                                                #endregion Send email

                                                #region Redirection to B2CRegisterResult confirmation page

                                                if (
                                                    string.IsNullOrEmpty(customer.Email)
                                                    && !string.IsNullOrEmpty(customerEmail)
                                                )
                                                    customer.Email = customerEmail;
                                                await _erpLogsService.InformationAsync(
                                                    $"B2C Registration: Successfully registered new Customer {customer.Email} (Id: {customer.Id})",
                                                    ErpSyncLevel.Account,
                                                    customer: customer
                                                );

                                                switch (_customerSettings.UserRegistrationType)
                                                {
                                                    case UserRegistrationType.EmailValidation:
                                                        //email validation message
                                                        await _genericAttributeService.SaveAttributeAsync(
                                                            customer,
                                                            NopCustomerDefaults.AccountActivationTokenAttribute,
                                                            Guid.NewGuid().ToString()
                                                        );
                                                        await _erpWorkflowMessageService.SendB2CCustomerEmailVerificationMessageAsync(
                                                            customer,
                                                            erpAccountForB2C,
                                                            erpNopUser,
                                                            salesOrg,
                                                            erpShipToAddress,
                                                            currentLanguage.Id
                                                        );
                                                        var session = EngineContext
                                                            .Current.Resolve<IHttpContextAccessor>()
                                                            .HttpContext.Session;
                                                        session.SetString(
                                                            "CustomerId",
                                                            $"{customer.Id}"
                                                        );

                                                        //result
                                                        return RedirectToRoute(
                                                            "B2CRegisterResult",
                                                            new
                                                            {
                                                                resultId = (int)
                                                                    UserRegistrationType.EmailValidation,
                                                                returnUrl,
                                                            }
                                                        );

                                                    case UserRegistrationType.AdminApproval:
                                                        return RedirectToRoute(
                                                            "B2CRegisterResult",
                                                            new
                                                            {
                                                                resultId = (int)
                                                                    UserRegistrationType.AdminApproval,
                                                                returnUrl,
                                                            }
                                                        );

                                                    case UserRegistrationType.Standard:
                                                        //send customer welcome message
                                                        await _erpWorkflowMessageService.SendB2CCustomerWelcomeMessageAsync(
                                                            customer,
                                                            erpAccountForB2C,
                                                            erpNopUser,
                                                            salesOrg,
                                                            erpShipToAddress,
                                                            currentLanguage.Id
                                                        );

                                                        //raise event
                                                        await _eventPublisher.PublishAsync(
                                                            new CustomerActivatedEvent(customer)
                                                        );

                                                        returnUrl = Url.RouteUrl(
                                                            "B2CRegisterResult",
                                                            new
                                                            {
                                                                resultId = (int)
                                                                    UserRegistrationType.Standard,
                                                                returnUrl,
                                                            }
                                                        );
                                                        return await _customerRegistrationService.SignInCustomerAsync(
                                                            customer,
                                                            returnUrl,
                                                            true
                                                        );

                                                    default:
                                                        //If we got this far, something failed, redisplay form
                                                        model =
                                                            await _b2CRegisterModelFactory.PrepareB2CRegisterModelAsync(
                                                                model,
                                                                false,
                                                                setDefaultValues: true
                                                            );
                                                        return View(model);
                                                }

                                                #endregion Redirection to B2CRegisterResult confirmation page
                                            }
                                            else
                                            {
                                                ModelState.AddModelError(
                                                    "",
                                                    await _localizationService.GetResourceAsync(
                                                        "Plugins.B2B.ManageB2BCustomer.Account.Register.Result.CountryOrStateProvinceNotFound"
                                                    )
                                                );
                                                errorMessage =
                                                    $"B2C Registration: Country or State/Province not found for customer: {customer.Email}, Country Name: {model.Country}, "
                                                    + $"Country Code: {model.CountryCode}, state/province: {model.StateProvince}, state/province code: {model.StateProvinceCode}";
                                                await _erpLogsService.InsertErpLogAsync(
                                                    ErpLogLevel.Error,
                                                    ErpSyncLevel.Account,
                                                    errorMessage
                                                );
                                            }
                                        }
                                        catch (Exception ex)
                                        {
                                            ModelState.AddModelError(
                                                "",
                                                await _localizationService.GetResourceAsync(
                                                    "Plugins.B2B.ManageB2BCustomer.B2C.Account.Register.Error"
                                                )
                                            );
                                            errorMessage =
                                                $"B2C Registration: Customer {customer?.Email} could not be registered due to exception {ex.Message}";
                                            await _erpLogsService.InsertErpLogAsync(
                                                ErpLogLevel.Error,
                                                ErpSyncLevel.Account,
                                                errorMessage,
                                                ex.StackTrace,
                                                customer
                                            );
                                        }
                                    }
                                    else
                                    {
                                        ModelState.AddModelError(
                                            "",
                                            await _localizationService.GetResourceAsync(
                                                "B2BB2C.Account.Registration.SalesOrgNotFound"
                                            )
                                        );
                                        errorMessage =
                                            $"B2C Registration: Sales Organisation or B2B account not found for the warehouse {nearestTradingWarehouseCode}";
                                        await _erpLogsService.InsertErpLogAsync(
                                            ErpLogLevel.Error,
                                            ErpSyncLevel.Account,
                                            errorMessage
                                        );
                                    }
                                }
                                else
                                {
                                    ModelState.AddModelError(
                                        "",
                                        await _localizationService.GetResourceAsync(
                                            "B2BB2C.Account.Registration.WarehouseNotFound"
                                        )
                                    );
                                    await _erpLogsService.InsertErpLogAsync(
                                        ErpLogLevel.Error,
                                        ErpSyncLevel.Account,
                                        $"B2C Registration: Warehouse {nearestTradingWarehouseCode} not found for Erp Account: {erpAccountForB2C?.AccountNumber}, Customer Id: {customer?.Id}."
                                    );
                                }
                            }

                            if (shipToAddressIdForB2BB2CUserId == 0)
                            {
                                await _erpLogsService.InsertErpLogAsync(
                                    ErpLogLevel.Error,
                                    ErpSyncLevel.Account,
                                    $"B2C Registration: New ShipToAddress was not created for Erp Account: {erpAccountForB2C?.AccountNumber}, "
                                        + $"Customer Id: {customer?.Id} due to invalid address."
                                );
                            }

                            await AddOrRemoveB2CCustomerUnsuccessfulRegisterRole(customer);
                            customer.Active = false;
                            await _customerService.UpdateCustomerAsync(customer);
                            await _authenticationService.SignOutAsync();

                            await _erpUserRegistrationInfoService.InsertErpUserRegistrationInfoAsync(
                                new ErpUserRegistrationInfo()
                                {
                                    NopCustomerId = customer?.Id ?? 0,
                                    NearestWareHouseId = nopWarehouse?.Id ?? 0,
                                    DistanceToNearestWarehouse =
                                        (decimal)distanceToNearestnearestTradingWarehouse,
                                    DeliveryOptionId = deliveryOption,
                                    Latitude = model.Latitude,
                                    Longitude = model.Longitude,
                                    AddressId = defaultAddress?.Id ?? 0,
                                    HouseNumber = model.HouseNumber,
                                    City = model.CityName,
                                    Suburb = model.Suburb,
                                    Street = model.Street,
                                    PostalCode = model.PostalCode,
                                    Country = model.Country,
                                    ErpSalesOrgId = erpAccountForB2C?.ErpSalesOrgId ?? 0,
                                    ErpAccountIdForB2C = erpAccountForB2C?.Id ?? 0,
                                    ErpUserId = erpNopUser?.Id ?? 0,
                                    ErrorMessage = errorMessage,
                                    ErpUserTypeId = model.IsB2BUser
                                        ? (int)ErpUserType.B2BUser
                                        : (int)ErpUserType.B2CUser,
                                }
                            );

                            #endregion B2C
                        }

                        //errors
                        foreach (var error in registrationResult.Errors)
                            ModelState.AddModelError("", error);
                    }
                }
                else
                {
                    ModelState.AddModelError(
                        "",
                        await _localizationService.GetResourceAsync(
                            "B2BB2C.Account.Registration.AccountNotCreated.LatitudeOrLongitudeNotFound"
                        )
                    );
                    await _erpLogsService.InsertErpLogAsync(
                        ErpLogLevel.Error,
                        ErpSyncLevel.Account,
                        $"B2C Registration: Latitude Or Longitude was not found! Customer: {customer?.Email} (Id - {customer?.Id}). Click view to see details.",
                        customer: customer
                    );
                }
            }
        }
        catch (Exception ex)
        {
            customer.Active = false;
            await _customerService.UpdateCustomerAsync(customer);
            await _authenticationService.SignOutAsync();
            await _erpLogsService.InsertErpLogAsync(
                ErpLogLevel.Error,
                ErpSyncLevel.Account,
                $"B2C Registration: Error while saving the Erp Nop User! Customer: {customer?.FirstName} (Id - {customer?.Id}). Click view to see details.",
                $"Exception Message: {ex.Message}\nStackTrace: {ex.StackTrace}",
                customer: customer
            );

            ModelState.AddModelError(
                "",
                await _localizationService.GetResourceAsync(
                    "Plugins.B2B.ManageB2BCustomer.B2C.Account.Register.Error"
                )
            );
        }

        //If we got this far, something failed, redisplay form
        model = await _b2CRegisterModelFactory.PrepareB2CRegisterModelAsync(
            model,
            false,
            setDefaultValues: true
        );
        return View(
            "~/Plugins/NopStation.Plugin.B2B.ManageB2CandB2BCustomer/Views/B2BB2CCustomer/B2CRegister.cshtml",
            model
        );
    }

    protected virtual async Task<IActionResult> SaveNopCustomerForB2BAsync(
        B2BRegisterModel model,
        string returnUrl,
        bool captchaValid,
        IFormCollection form,
        Customer customer
    )
    {
        try
        {
            if (await _customerService.IsRegisteredAsync(customer))
            {
                //Already registered customer.
                await _authenticationService.SignOutAsync();

                //raise logged out event
                await _eventPublisher.PublishAsync(new CustomerLoggedOutEvent(customer));

                customer = await _customerService.InsertGuestCustomerAsync();

                //Save a new record
                await _workContext.SetCurrentCustomerAsync(customer);
            }

            var store = await _storeContext.GetCurrentStoreAsync();
            customer.RegisteredInStoreId = store.Id;

            //custom customer attributes
            var customerAttributesXml = await ParseCustomCustomerAttributesAsync(form);
            var customerAttributeWarnings =
                await _customerAttributeParser.GetAttributeWarningsAsync(customerAttributesXml);
            foreach (var error in customerAttributeWarnings)
            {
                ModelState.AddModelError("", error);
            }

            //validate CAPTCHA
            if (
                _captchaSettings.Enabled
                && _captchaSettings.ShowOnRegistrationPage
                && !captchaValid
            )
            {
                ModelState.AddModelError(
                    "",
                    await _localizationService.GetResourceAsync("Common.WrongCaptchaMessage")
                );
            }

            //GDPR
            if (_gdprSettings.GdprEnabled)
            {
                var consents = (await _gdprService.GetAllConsentsAsync())
                    .Where(consent => consent.DisplayDuringRegistration && consent.IsRequired)
                    .ToList();

                ValidateRequiredConsents(consents, form);
            }

            if (ModelState.IsValid)
            {
                var customerUserName = model.Username;
                var customerEmail = model.Email;

                var isApproved =
                    _customerSettings.UserRegistrationType == UserRegistrationType.Standard;
                var registrationRequest = new CustomerRegistrationRequest(
                    customer,
                    customerEmail,
                    _customerSettings.UsernamesEnabled ? customerUserName : customerEmail,
                    model.Password,
                    _customerSettings.DefaultPasswordFormat,
                    store.Id,
                    isApproved
                );
                var registrationResult = await _customerRegistrationService.RegisterCustomerAsync(
                    registrationRequest
                );
                if (registrationResult.Success)
                {
                    //properties
                    if (_dateTimeSettings.AllowCustomersToSetTimeZone)
                        customer.TimeZoneId = model.TimeZoneId;

                    //VAT number
                    if (_taxSettings.EuVatEnabled)
                    {
                        customer.VatNumber = model.VatNumber;

                        var (vatNumberStatus, _, vatAddress) =
                            await _taxService.GetVatNumberStatusAsync(model.VatNumber);
                        customer.VatNumberStatusId = (int)vatNumberStatus;
                        //send VAT number admin notification
                        if (
                            !string.IsNullOrEmpty(model.VatNumber)
                            && _taxSettings.EuVatEmailAdminWhenNewVatSubmitted
                        )
                            await _workflowMessageService.SendNewVatSubmittedStoreOwnerNotificationAsync(
                                customer,
                                model.VatNumber,
                                vatAddress,
                                _localizationSettings.DefaultAdminLanguageId
                            );
                    }

                    await _genericAttributeService.SaveAttributeAsync<string>(
                        customer,
                        ErpWorkflowMessageService.JobTitleAttribute,
                        model.JobTitle
                    );

                    #region Customer Settings check

                    if (_customerSettings.GenderEnabled)
                        customer.Gender = model.Gender;
                    if (_customerSettings.FirstNameEnabled)
                        customer.FirstName = model.FirstName;
                    if (_customerSettings.LastNameEnabled)
                        customer.LastName = model.LastName;
                    if (_customerSettings.DateOfBirthEnabled)
                        customer.DateOfBirth = model.ParseDateOfBirth();
                    if (_customerSettings.CompanyEnabled)
                        customer.Company = model.Company;
                    if (_customerSettings.StreetAddressEnabled)
                        customer.StreetAddress = model.StreetAddress;
                    if (_customerSettings.StreetAddress2Enabled)
                        customer.StreetAddress2 = model.StreetAddress2;
                    if (_customerSettings.ZipPostalCodeEnabled)
                        customer.ZipPostalCode = model.ZipPostalCode;
                    if (_customerSettings.CityEnabled)
                        customer.City = model.City;
                    if (_customerSettings.CountyEnabled)
                        customer.County = model.County;
                    if (_customerSettings.CountryEnabled)
                        customer.CountryId = model.CountryId;
                    if (_customerSettings.CountryEnabled && _customerSettings.StateProvinceEnabled)
                        customer.StateProvinceId = model.StateProvinceId;
                    if (_customerSettings.PhoneEnabled)
                        customer.Phone = model.Phone;
                    if (_customerSettings.FaxEnabled)
                        customer.Fax = model.Fax;

                    //save customer attributes
                    customer.CustomCustomerAttributesXML = customerAttributesXml;
                    await _customerService.UpdateCustomerAsync(customer);

                    //newsletter
                    if (_customerSettings.NewsletterEnabled)
                    {
                        var isNewsletterActive =
                            _customerSettings.UserRegistrationType
                            != UserRegistrationType.EmailValidation;

                        //save newsletter value
                        var newsletter =
                            await _newsLetterSubscriptionService.GetNewsLetterSubscriptionByEmailAndStoreIdAsync(
                                customerEmail,
                                store.Id
                            );
                        if (newsletter != null)
                        {
                            if (model.Newsletter)
                            {
                                newsletter.Active = isNewsletterActive;
                                await _newsLetterSubscriptionService.UpdateNewsLetterSubscriptionAsync(
                                    newsletter
                                );

                                //GDPR
                                if (_gdprSettings.GdprEnabled && _gdprSettings.LogNewsletterConsent)
                                {
                                    await _gdprService.InsertLogAsync(
                                        customer,
                                        0,
                                        GdprRequestType.ConsentAgree,
                                        await _localizationService.GetResourceAsync(
                                            "Gdpr.Consent.Newsletter"
                                        )
                                    );
                                }
                            }
                        }
                        else
                        {
                            if (model.Newsletter)
                            {
                                await _newsLetterSubscriptionService.InsertNewsLetterSubscriptionAsync(
                                    new NewsLetterSubscription
                                    {
                                        NewsLetterSubscriptionGuid = Guid.NewGuid(),
                                        Email = customerEmail,
                                        Active = isNewsletterActive,
                                        StoreId = store.Id,
                                        LanguageId = customer.LanguageId ?? store.DefaultLanguageId,
                                        CreatedOnUtc = DateTime.UtcNow,
                                    }
                                );

                                //GDPR
                                if (_gdprSettings.GdprEnabled && _gdprSettings.LogNewsletterConsent)
                                {
                                    await _gdprService.InsertLogAsync(
                                        customer,
                                        0,
                                        GdprRequestType.ConsentAgree,
                                        await _localizationService.GetResourceAsync(
                                            "Gdpr.Consent.Newsletter"
                                        )
                                    );
                                }
                            }
                        }
                    }

                    if (_customerSettings.AcceptPrivacyPolicyEnabled)
                    {
                        //privacy policy is required
                        //GDPR
                        if (_gdprSettings.GdprEnabled && _gdprSettings.LogPrivacyPolicyConsent)
                        {
                            await _gdprService.InsertLogAsync(
                                customer,
                                0,
                                GdprRequestType.ConsentAgree,
                                await _localizationService.GetResourceAsync(
                                    "Gdpr.Consent.PrivacyPolicy"
                                )
                            );
                        }
                    }

                    //GDPR
                    if (_gdprSettings.GdprEnabled)
                    {
                        var consents = (await _gdprService.GetAllConsentsAsync())
                            .Where(consent => consent.DisplayDuringRegistration)
                            .ToList();
                        foreach (var consent in consents)
                        {
                            var controlId = $"consent{consent.Id}";
                            var cbConsent = form[controlId];
                            if (
                                !StringValues.IsNullOrEmpty(cbConsent)
                                && cbConsent.ToString().Equals("on")
                            )
                            {
                                //agree
                                await _gdprService.InsertLogAsync(
                                    customer,
                                    consent.Id,
                                    GdprRequestType.ConsentAgree,
                                    consent.Message
                                );
                            }
                            else
                            {
                                //disagree
                                await _gdprService.InsertLogAsync(
                                    customer,
                                    consent.Id,
                                    GdprRequestType.ConsentDisagree,
                                    consent.Message
                                );
                            }
                        }
                    }

                    #endregion Customer Settings check

                    if (model.CountryId == 0)
                    {
                        model.CountryId = _b2BB2CFeaturesSettings.DefaultCountryId;
                    }
                    if (model.StateProvinceId == 0)
                    {
                        model.StateProvinceId =
                            (
                                await _stateProvinceService.GetStateProvincesByCountryIdAsync(
                                    model.CountryId
                                )
                            )
                                ?[0]
                                .Id ?? 0;
                    }

                    //insert default address (if possible)
                    var defaultAddress = new Address
                    {
                        FirstName = customer.FirstName,
                        LastName = customer.LastName,
                        Email = customer.Email,
                        Company = customer.Company,
                        CountryId = customer.CountryId > 0 ? (int?)customer.CountryId : null,
                        StateProvinceId =
                            customer.StateProvinceId > 0 ? (int?)customer.StateProvinceId : null,
                        County = customer.County,
                        City = customer.City,
                        Address1 = customer.StreetAddress,
                        Address2 = customer.StreetAddress2,
                        ZipPostalCode = customer.ZipPostalCode,
                        PhoneNumber = customer.Phone,
                        FaxNumber = customer.Fax,
                        CreatedOnUtc = customer.CreatedOnUtc,
                    };
                    if (await _addressService.IsAddressValidAsync(defaultAddress))
                    {
                        //some validation
                        if (defaultAddress.CountryId == 0)
                            defaultAddress.CountryId = null;
                        if (defaultAddress.StateProvinceId == 0)
                            defaultAddress.StateProvinceId = null;
                        //set default address
                        //customer.Addresses.Add(defaultAddress);

                        await _addressService.InsertAddressAsync(defaultAddress);

                        await _customerService.InsertCustomerAddressAsync(customer, defaultAddress);

                        customer.BillingAddressId = defaultAddress.Id;
                        customer.ShippingAddressId = defaultAddress.Id;

                        await _customerService.UpdateCustomerAsync(customer);
                    }

                    //notifications
                    if (_customerSettings.NotifyNewCustomerRegistration)
                        await _workflowMessageService.SendCustomerRegisteredStoreOwnerNotificationMessageAsync(
                            customer,
                            _localizationSettings.DefaultAdminLanguageId
                        );

                    //raise event
                    await _eventPublisher.PublishAsync(new CustomerRegisteredEvent(customer));
                    var currentLanguage = await _workContext.GetWorkingLanguageAsync();

                    #region B2B Custom code

                    var salesOrganisations =
                        await _erpSalesOrgService.GetErpSalesOrganisationsByIdsAsync(
                            model.ErpSalesOrganisationIdsArray
                        );

                    if (salesOrganisations != null && salesOrganisations.Any())
                    {
                        var erpUserRegistration = new ErpUserRegistrationInfo
                        {
                            NopCustomerId = customer.Id,
                            //B2BSalesOrganisationId = 0,
                            //1375 || this converted to Ids (string)
                            ErpSalesOrganisationIds = string.Join(
                                ',',
                                salesOrganisations.Select(x => x.Id)
                            ),
                            ErpAccountNumber = model.AccountNumber,
                            SpecialInstructions = model.SpecialInstructions,
                            //1375 || split this column
                            //RegistrationAuthorisedBy = model.RegistrationAuthorisedBy,

                            //1375 || new added db column
                            PersonalAlternateContactNumber = model.PersonalAlternateContactNumber,
                            AuthorisationFullName = model.AuthorisationFullName,
                            AuthorisationContactNumber = model.AuthorisationContactNumber,
                            AuthorisationAlternateContactNumber =
                                model.AuthorisationAlternateContactNumber,
                            AuthorisationJobTitle = model.AuthorisationJobTitle,
                            AuthorisationAdditionalComment = model.AuthorisationAdditionalComment,
                            ErpUserTypeId = model.IsB2BUser
                                ? (int)ErpUserType.B2BUser
                                : (int)ErpUserType.B2CUser,
                        };

                        await _erpUserRegistrationInfoService.InsertErpUserRegistrationInfoAsync(
                            erpUserRegistration
                        );

                        //1375 || This condition set for multiple sales orginsation
                        var emailBody = string.Empty;
                        foreach (var salesOrganisation in salesOrganisations)
                        {
                            var queuedEmailInfo = string.Empty;
                            // send email message
                            if (
                                !string.IsNullOrEmpty(
                                    salesOrganisation.UserRegistrationEmailAdresses
                                )
                            )
                            {
                                queuedEmailInfo =
                                    $"For {salesOrganisation.Name}: Email sent to queue for \n";
                                var emailAddresses = salesOrganisation
                                    .UserRegistrationEmailAdresses.Split(',')
                                    .ToList<string>();
                                foreach (var email in emailAddresses)
                                {
                                    var queuedEmailIdentifiers =
                                        await _erpWorkflowMessageService.SendErpCustomerRegisteredNotificationMessageAsync(
                                            customer,
                                            erpAccount: null,
                                            erpNopUser: null,
                                            salesOrganisation,
                                            erpShipToAddress: null,
                                            currentLanguage.Id,
                                            email,
                                            erpUserRegistration,
                                            true
                                        );

                                    var queuedEmailIdentifiersString = string.Join(
                                        ",",
                                        queuedEmailIdentifiers
                                    );
                                    queuedEmailInfo +=
                                        $"{email} : {queuedEmailIdentifiersString} \n";
                                }
                            }
                            else
                            {
                                queuedEmailInfo =
                                    $"For {salesOrganisation.Name}: Email can't be sent to queue as there is no User Registration Email Adresses for this Sales Org {salesOrganisation.Name}";
                            }
                            emailBody += $"{queuedEmailInfo} \n";
                        }
                        erpUserRegistration.QueuedEmailInfo = emailBody.ToString();
                        await _erpUserRegistrationInfoService.UpdateErpUserRegistrationInfoAsync(
                            erpUserRegistration
                        );
                    }

                    #endregion B2B Custom code

                    switch (_customerSettings.UserRegistrationType)
                    {
                        case UserRegistrationType.EmailValidation:
                            //email validation message
                            await _genericAttributeService.SaveAttributeAsync(
                                customer,
                                NopCustomerDefaults.AccountActivationTokenAttribute,
                                Guid.NewGuid().ToString()
                            );
                            await _workflowMessageService.SendCustomerEmailValidationMessageAsync(
                                customer,
                                currentLanguage.Id
                            );

                            //result
                            return RedirectToRoute(
                                "RegisterResult",
                                new
                                {
                                    resultId = (int)UserRegistrationType.EmailValidation,
                                    returnUrl,
                                }
                            );

                        case UserRegistrationType.AdminApproval:
                            return RedirectToRoute(
                                "RegisterResult",
                                new
                                {
                                    resultId = (int)UserRegistrationType.AdminApproval,
                                    returnUrl,
                                }
                            );

                        case UserRegistrationType.Standard:
                            //send customer welcome message
                            await _workflowMessageService.SendCustomerWelcomeMessageAsync(
                                customer,
                                currentLanguage.Id
                            );

                            //raise event
                            await _eventPublisher.PublishAsync(
                                new CustomerActivatedEvent(customer)
                            );

                            returnUrl = Url.RouteUrl(
                                "RegisterResult",
                                new { resultId = (int)UserRegistrationType.Standard, returnUrl }
                            );
                            return await _customerRegistrationService.SignInCustomerAsync(
                                customer,
                                returnUrl,
                                true
                            );

                        default:
                            //If we got this far, something failed, redisplay form
                            model = await _b2BRegisterModelFactory.PrepareB2BRegisterModelAsync(
                                model,
                                false,
                                setDefaultValues: true
                            );
                            return View(
                                "~/Plugins/NopStation.Plugin.B2B.B2BB2CFeatures/Views/B2BB2CCustomer/B2BRegister.cshtml",
                                model
                            );
                    }
                }

                //errors
                foreach (var error in registrationResult.Errors)
                    ModelState.AddModelError("", error);
            }
        }
        catch (Exception ex)
        {
            await _erpLogsService.InsertErpLogAsync(
                ErpLogLevel.Error,
                ErpSyncLevel.Account,
                "Error while saving the Erp Nop User!"
                    + (
                        customer.Id > 0
                            ? $"Customer: {customer.FirstName} (Id - {customer.Id})"
                            : string.Empty
                    )
                    + $" Exception Message: {ex.Message}",
                ex.StackTrace,
                customer: customer
            );
            ModelState.AddModelError(
                "",
                await _localizationService.GetResourceAsync(
                    "Plugins.B2B.B2BB2CFeatures.B2BUser.Register.Error.SomethingWentWrong"
                )
            );
        }

        //If we got this far, something failed, redisplay form
        model = await _b2BRegisterModelFactory.PrepareB2BRegisterModelAsync(
            model,
            false,
            setDefaultValues: true
        );
        return View(
            "~/Plugins/NopStation.Plugin.B2B.B2BB2CFeatures/Views/B2BB2CCustomer/B2BRegister.cshtml",
            model
        );
    }

    #endregion Utilities

    #region Methods

    #region B2BRegister

    //[HttpsRequirement(SslRequirement.Yes)]
    //available even when navigation is not allowed
    [CheckAccessPublicStore(true)]
    public override async Task<IActionResult> B2BRegister()
    {
        //check whether registration is allowed
        if (
            _customerSettings.UserRegistrationType == UserRegistrationType.Disabled
            || !await B2BUserRegisterAllowedAsync()
        )
            return RedirectToRoute(
                "RegisterResult",
                new { resultId = (int)UserRegistrationType.Disabled }
            );

        var model = new B2BRegisterModel();
        model.IsB2BUser = true;
        model = await _b2BRegisterModelFactory.PrepareB2BRegisterModelAsync(
            model,
            false,
            setDefaultValues: true
        );

        return View(
            "~/Plugins/NopStation.Plugin.B2B.B2BB2CFeatures/Views/B2BB2CCustomer/B2BRegister.cshtml",
            model
        );
    }

    [HttpPost]
    [ValidateCaptcha]
    [ValidateHoneypot]
    //[PublicAntiForgery]
    //available even when navigation is not allowed
    [CheckAccessPublicStore(true)]
    public override async Task<IActionResult> B2BRegister(
        B2BRegisterModel model,
        string returnUrl,
        bool captchaValid,
        IFormCollection form
    )
    {
        //check whether registration is allowed
        if (
            _customerSettings.UserRegistrationType == UserRegistrationType.Disabled
            || !await B2BUserRegisterAllowedAsync()
        )
            return RedirectToRoute(
                "RegisterResult",
                new { resultId = (int)UserRegistrationType.Disabled, returnUrl }
            );

        return await SaveNopCustomerForB2BAsync(
            model,
            returnUrl,
            captchaValid,
            form,
            await _workContext.GetCurrentCustomerAsync()
        );
    }

    #endregion B2BRegister

    #region B2CRegister

    //available even when navigation is not allowed
    [CheckAccessPublicStore(true)]
    public override async Task<IActionResult> B2CRegister()
    {
        //check whether registration is allowed
        if (
            _customerSettings.UserRegistrationType == UserRegistrationType.Disabled
            || !await B2CUserRegisterAllowedAsync()
        )
            return RedirectToRoute(
                "RegisterResult",
                new { resultId = (int)UserRegistrationType.Disabled }
            );

        var model = new B2CRegisterModel();
        model = await _b2CRegisterModelFactory.PrepareB2CRegisterModelAsync(
            model,
            false,
            setDefaultValues: true
        );
        model.IsB2BUser = false;

        return View(
            "~/Plugins/NopStation.Plugin.B2B.ManageB2CandB2BCustomer/Views/B2BB2CCustomer/B2CRegister.cshtml",
            model
        );
    }

    [HttpPost]
    [ValidateCaptcha]
    [ValidateHoneypot]
    //[PublicAntiForgery]
    //available even when navigation is not allowed
    [CheckAccessPublicStore(true)]
    public virtual async Task<IActionResult> B2CRegisterPost(
        B2CRegisterModel model,
        string returnUrl,
        bool captchaValid,
        IFormCollection form
    )
    {
        //check whether registration is allowed
        if (
            _customerSettings.UserRegistrationType == UserRegistrationType.Disabled
            || !await B2CUserRegisterAllowedAsync()
        )
            return RedirectToRoute(
                "RegisterResult",
                new { resultId = (int)UserRegistrationType.Disabled, returnUrl }
            );

        return await SaveNopCustomerForB2CAsync(
            model,
            returnUrl,
            captchaValid,
            form,
            await _workContext.GetCurrentCustomerAsync()
        );
    }

    [CheckAccessPublicStore(true)]
    public async Task<IActionResult> B2CRegisterResult(int resultId)
    {
        var model = await _b2CRegisterModelFactory.PrepareB2CRegisterResultModelAsync(resultId);
        return View(
            "~/Plugins/NopStation.Plugin.B2B.ManageB2CandB2BCustomer/Views/B2BB2CCustomer/B2CRegisterResult.cshtml",
            model
        );
    }

    [CheckAccessPublicStore(true)]
    [HttpPost]
    public async Task<IActionResult> B2CRegisterResult(string returnUrl)
    {
        if (string.IsNullOrEmpty(returnUrl) || !Url.IsLocalUrl(returnUrl))
            return RedirectToRoute("Homepage");

        return Redirect(returnUrl);
    }

    #endregion B2CRegister

    #endregion Methods
}
