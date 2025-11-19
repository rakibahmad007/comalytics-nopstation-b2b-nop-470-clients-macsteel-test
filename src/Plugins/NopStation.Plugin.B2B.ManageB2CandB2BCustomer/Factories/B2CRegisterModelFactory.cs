using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Core;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Gdpr;
using Nop.Core.Domain.Security;
using Nop.Core.Domain.Tax;
using Nop.Core.Infrastructure;
using Nop.Services.Attributes;
using Nop.Services.Common;
using Nop.Services.Configuration;
using Nop.Services.Customers;
using Nop.Services.Directory;
using Nop.Services.Gdpr;
using Nop.Services.Helpers;
using Nop.Services.Localization;
using Nop.Services.Shipping;
using Nop.Web.Models.Customer;
using NopStation.Plugin.B2B.B2BB2CFeatures;
using NopStation.Plugin.B2B.B2BB2CFeatures.Helpers;
using NopStation.Plugin.B2B.B2BB2CFeatures.Services.ErpCustomerFunctionality;
using NopStation.Plugin.B2B.ERPIntegrationCore;
using NopStation.Plugin.B2B.ERPIntegrationCore.Enums;
using NopStation.Plugin.B2B.ERPIntegrationCore.Services;
using NopStation.Plugin.B2B.ManageB2CandB2BCustomer.Models;


namespace NopStation.Plugin.B2B.ManageB2CandB2BCustomer.Factories;
public class B2CRegisterModelFactory : IB2CRegisterModelFactory
{
    #region Fields

    private readonly CaptchaSettings _captchaSettings;
    private readonly CommonSettings _commonSettings;
    private readonly CustomerSettings _customerSettings;
    private readonly DateTimeSettings _dateTimeSettings;
    private readonly GdprSettings _gdprSettings;
    private readonly ICountryService _countryService;
    private readonly IAttributeParser<CustomerAttribute, CustomerAttributeValue> _customerAttributeParser;
    private readonly IAttributeService<CustomerAttribute, CustomerAttributeValue> _customerAttributeService;
    private readonly IDateTimeHelper _dateTimeHelper;
    private readonly IGdprService _gdprService;
    private readonly ILocalizationService _localizationService;
    private readonly IStateProvinceService _stateProvinceService;
    private readonly IWorkContext _workContext;
    private readonly SecuritySettings _securitySettings;
    private readonly TaxSettings _taxSettings;
    private readonly ICommonHelper _commonHelper;
    private readonly IErpSalesOrgService _erpSalesOrgService;
    private readonly ISettingService _settingService;
    private readonly IStoreContext _storeContext;
    private readonly IErpAccountService _erpAccountService;
    private readonly ICustomerService _customerService;
    private readonly IB2CMacsteelExpressShopService _b2CMacsteelExpressShopService;
    private readonly IAddressService _addressService;
    private readonly IShippingService _shippingService;
    private readonly IErpShipToAddressService _erpShipToAddressService;
    private readonly IErpCustomerFunctionalityService _erpCustomerFunctionalityService;

    #endregion

    #region Ctor

    public B2CRegisterModelFactory(CaptchaSettings captchaSettings,
                                   CommonSettings commonSettings,
                                   CustomerSettings customerSettings,
                                   DateTimeSettings dateTimeSettings,
                                   GdprSettings gdprSettings,
                                   ICountryService countryService,
                                   IAttributeParser<CustomerAttribute, CustomerAttributeValue> customerAttributeParser,
                                   IAttributeService<CustomerAttribute, CustomerAttributeValue> customerAttributeService,
                                   IDateTimeHelper dateTimeHelper,
                                   IGdprService gdprService,
                                   ILocalizationService localizationService,
                                   IStateProvinceService stateProvinceService,
                                   IWorkContext workContext,
                                   SecuritySettings securitySettings,
                                   TaxSettings taxSettings,
                                   ICommonHelper commonHelper,
                                   IErpSalesOrgService erpSalesOrgService,
                                   IStoreContext storeContext,
                                   IErpAccountService erpAccountService,
                                   ISettingService settingService
,
                                   ICustomerService customerService,
                                   IB2CMacsteelExpressShopService b2CMacsteelExpressShopService,
                                   IAddressService addressService,
                                   IShippingService shippingService,
                                   IErpShipToAddressService erpShipToAddressService,
                                   IErpCustomerFunctionalityService erpCustomerFunctionalityService)
    {

        _captchaSettings = captchaSettings;
        _commonSettings = commonSettings;
        _customerSettings = customerSettings;
        _dateTimeSettings = dateTimeSettings;
        _gdprSettings = gdprSettings;
        _countryService = countryService;
        _customerAttributeParser = customerAttributeParser;
        _customerAttributeService = customerAttributeService;
        _dateTimeHelper = dateTimeHelper;
        _gdprService = gdprService;
        _localizationService = localizationService;
        _stateProvinceService = stateProvinceService;
        _workContext = workContext;
        _securitySettings = securitySettings;
        _taxSettings = taxSettings;
        _commonHelper = commonHelper;
        _erpSalesOrgService = erpSalesOrgService;
        _erpAccountService = erpAccountService;
        _settingService = settingService;
        _storeContext = storeContext;
        _customerService = customerService;
        _b2CMacsteelExpressShopService = b2CMacsteelExpressShopService;
        _addressService = addressService;
        _shippingService = shippingService;
        _erpShipToAddressService = erpShipToAddressService;
        _erpCustomerFunctionalityService = erpCustomerFunctionalityService;
    }

    #endregion

    #region Utilities

    /// <returns>A task that represents the asynchronous operation</returns>
    protected virtual async Task<GdprConsentModel> PrepareGdprConsentModelAsync(GdprConsent consent, bool accepted)
    {
        if (consent == null)
            throw new ArgumentNullException(nameof(consent));

        var requiredMessage = await _localizationService.GetLocalizedAsync(consent, x => x.RequiredMessage);
        return new GdprConsentModel
        {
            Id = consent.Id,
            Message = await _localizationService.GetLocalizedAsync(consent, x => x.Message),
            IsRequired = consent.IsRequired,
            RequiredMessage = !string.IsNullOrEmpty(requiredMessage) ? requiredMessage : $"'{consent.Message}' is required",
            Accepted = accepted
        };
    }

    protected virtual async Task PrepareB2BSalesOrganisationsAsync(B2CRegisterModel model)
    {
        //get list of sales org here and prepare the dropdown data
        var salesOrganizations = await _erpSalesOrgService.GetAllErpSalesOrgAsync(showHidden: false);
        if (salesOrganizations.Count == 1)
        {
            model.B2BSalesOrganizationId = salesOrganizations.Select(x => x.Id).FirstOrDefault();
        }
        else
        {
            foreach (var salesOrg in salesOrganizations)
            {
                var items = new SelectListItem
                {
                    Text = salesOrg.Name,
                    Value = salesOrg.Id.ToString()
                };
                model.AvailableB2BSalesOrganizations.Add(items);
            }
            await _commonHelper.PrepareDefaultItemAsync(model.AvailableB2BSalesOrganizations, true);
        }
    }

    protected virtual void SetCustomerAddressFieldsAsRequired(B2CRegisterModel model)
    {
        model.PhoneEnabled = true;
        model.PhoneRequired = true;
        model.StreetAddressRequired = true;
        model.StreetAddressEnabled = true;
        model.StreetAddress2Enabled = true;
        model.StreetAddress2Required = true;
        model.ZipPostalCodeEnabled = true;
        model.ZipPostalCodeRequired = true;
        model.CountryEnabled = true;
        model.CountryRequired = true;
        model.CityEnabled = true;
        model.CityRequired = true;
    }

    #endregion

    #region Methods

    public virtual async Task<B2CRegisterModel> PrepareB2CRegisterModelAsync(B2CRegisterModel model, bool excludeProperties,
    string overrideCustomCustomerAttributesXml = "", bool setDefaultValues = false)
    {
        if (model == null)
            throw new ArgumentNullException(nameof(model));

        var customer = await _workContext.GetCurrentCustomerAsync();

        model.AllowCustomersToSetTimeZone = _dateTimeSettings.AllowCustomersToSetTimeZone;
        foreach (var tzi in _dateTimeHelper.GetSystemTimeZones())
            model.AvailableTimeZones.Add(new SelectListItem { Text = tzi.DisplayName, Value = tzi.Id, Selected = (excludeProperties ? tzi.Id == model.TimeZoneId : tzi.Id == (await _dateTimeHelper.GetCurrentTimeZoneAsync()).Id) });

        //VAT
        model.DisplayVatNumber = _taxSettings.EuVatEnabled;
        if (_taxSettings.EuVatEnabled && _taxSettings.EuVatEnabledForGuests)
            model.VatNumber = customer.VatNumber;

        //form fields
        model.FirstNameEnabled = _customerSettings.FirstNameEnabled;
        model.LastNameEnabled = _customerSettings.LastNameEnabled;
        model.FirstNameRequired = _customerSettings.FirstNameRequired;
        model.LastNameRequired = _customerSettings.LastNameRequired;
        model.GenderEnabled = _customerSettings.GenderEnabled;
        model.DateOfBirthEnabled = _customerSettings.DateOfBirthEnabled;
        model.DateOfBirthRequired = _customerSettings.DateOfBirthRequired;
        model.CompanyEnabled = _customerSettings.CompanyEnabled;
        model.CompanyRequired = _customerSettings.CompanyRequired;
        model.StreetAddressEnabled = _customerSettings.StreetAddressEnabled;
        model.StreetAddressRequired = _customerSettings.StreetAddressRequired;
        model.StreetAddress2Enabled = _customerSettings.StreetAddress2Enabled;
        model.StreetAddress2Required = _customerSettings.StreetAddress2Required;
        model.ZipPostalCodeEnabled = _customerSettings.ZipPostalCodeEnabled;
        model.ZipPostalCodeRequired = _customerSettings.ZipPostalCodeRequired;
        model.CityEnabled = _customerSettings.CityEnabled;
        model.CityRequired = _customerSettings.CityRequired;
        model.CountyEnabled = _customerSettings.CountyEnabled;
        model.CountyRequired = _customerSettings.CountyRequired;
        model.CountryEnabled = _customerSettings.CountryEnabled;
        model.CountryRequired = _customerSettings.CountryRequired;
        model.StateProvinceEnabled = _customerSettings.StateProvinceEnabled;
        model.StateProvinceRequired = _customerSettings.StateProvinceRequired;
        model.PhoneEnabled = _customerSettings.PhoneEnabled;
        model.PhoneRequired = _customerSettings.PhoneRequired;
        model.FaxEnabled = _customerSettings.FaxEnabled;
        model.FaxRequired = _customerSettings.FaxRequired;
        model.NewsletterEnabled = _customerSettings.NewsletterEnabled;
        model.AcceptPrivacyPolicyEnabled = _customerSettings.AcceptPrivacyPolicyEnabled;
        model.AcceptPrivacyPolicyPopup = _commonSettings.PopupForTermsOfServiceLinks;
        model.UsernamesEnabled = _customerSettings.UsernamesEnabled;
        model.CheckUsernameAvailabilityEnabled = _customerSettings.CheckUsernameAvailabilityEnabled;
        model.HoneypotEnabled = _securitySettings.HoneypotEnabled;
        model.DisplayCaptcha = _captchaSettings.Enabled && _captchaSettings.ShowOnRegistrationPage;
        model.EnteringEmailTwice = _customerSettings.EnteringEmailTwice;

        if (!model.IsB2BUser)
        {
            var storeScope = await _storeContext.GetActiveStoreScopeConfigurationAsync();
            var b2BB2CFeaturesSettings = await _settingService.LoadSettingAsync<B2BB2CFeaturesSettings>(storeScope);
            if (b2BB2CFeaturesSettings.DefaultB2CErpAccountId > 0)
            {
                var erpAccountInfo = await _erpAccountService.GetErpAccountByIdAsync(b2BB2CFeaturesSettings.DefaultB2CErpAccountId);
                model.B2CIdentificationNumber = erpAccountInfo.AccountNumber;
                model.AccountName = erpAccountInfo.AccountName;
            }
        }

        if (setDefaultValues)
        {
            //enable newsletter by default
            model.Newsletter = _customerSettings.NewsletterTickedByDefault;
        }

        //countries and states
        if (_customerSettings.CountryEnabled || !model.IsB2BUser)
        {
            model.AvailableCountries.Add(new SelectListItem { Text = await _localizationService.GetResourceAsync("Address.SelectCountry"), Value = "0" });
            var currentLanguage = await _workContext.GetWorkingLanguageAsync();
            foreach (var c in await _countryService.GetAllCountriesAsync(currentLanguage.Id))
            {
                model.AvailableCountries.Add(new SelectListItem
                {
                    Text = await _localizationService.GetLocalizedAsync(c, x => x.Name),
                    Value = c.Id.ToString(),
                    Selected = c.Id == model.CountryId
                });
            }

            if (_customerSettings.StateProvinceEnabled)
            {
                //states
                var states = (await _stateProvinceService.GetStateProvincesByCountryIdAsync(model.CountryId, currentLanguage.Id)).ToList();
                if (states.Any())
                {
                    model.AvailableStates.Add(new SelectListItem { Text = await _localizationService.GetResourceAsync("Address.SelectState"), Value = "0" });

                    foreach (var s in states)
                    {
                        model.AvailableStates.Add(new SelectListItem { Text = await _localizationService.GetLocalizedAsync(s, x => x.Name), Value = s.Id.ToString(), Selected = (s.Id == model.StateProvinceId) });
                    }
                }
                else
                {
                    var anyCountrySelected = model.AvailableCountries.Any(x => x.Selected);

                    model.AvailableStates.Add(new SelectListItem
                    {
                        Text = await _localizationService.GetResourceAsync(anyCountrySelected ? "Address.Other" : "Address.SelectState"),
                        Value = "0"
                    });
                }

            }
        }

        //custom customer attributes
        var customAttributes = await PrepareCustomCustomerAttributesAsync(customer, overrideCustomCustomerAttributesXml);
        foreach (var attribute in customAttributes)
            model.CustomerAttributes.Add(attribute);

        //GDPR
        if (_gdprSettings.GdprEnabled)
        {
            var consents = (await _gdprService.GetAllConsentsAsync()).Where(consent => consent.DisplayDuringRegistration).ToList();
            foreach (var consent in consents)
            {
                model.GdprConsents.Add(await PrepareGdprConsentModelAsync(consent, false));
            }
        }

        // B2B - Prepare available sales orgs
        await PrepareB2BSalesOrganisationsAsync(model);

        if (!model.IsB2BUser)
            SetCustomerAddressFieldsAsRequired(model);

        return model;
    }

    public virtual async Task<IList<CustomerAttributeModel>> PrepareCustomCustomerAttributesAsync(Customer customer, string overrideAttributesXml = "")
    {
        if (customer == null)
            throw new ArgumentNullException(nameof(customer));

        var result = new List<CustomerAttributeModel>();

        var customerAttributes = await _customerAttributeService.GetAllAttributesAsync();

        foreach (var attribute in customerAttributes)
        {
            var attributeModel = new CustomerAttributeModel
            {
                Id = attribute.Id,
                Name = await _localizationService.GetLocalizedAsync(attribute, x => x.Name),
                IsRequired = attribute.IsRequired,
                AttributeControlType = attribute.AttributeControlType,
            };

            if (attribute.ShouldHaveValues)
            {
                //values

                var attributeValues = await _customerAttributeService.GetAttributeValuesAsync(attribute.Id);

                foreach (var attributeValue in attributeValues)
                {
                    var valueModel = new CustomerAttributeValueModel
                    {
                        Id = attributeValue.Id,
                        Name = await _localizationService.GetLocalizedAsync(attributeValue, x => x.Name),
                        IsPreSelected = attributeValue.IsPreSelected
                    };
                    attributeModel.Values.Add(valueModel);
                }
            }

            //set already selected attributes
            var selectedAttributesXml = !string.IsNullOrEmpty(overrideAttributesXml) ?
                overrideAttributesXml :
                customer.CustomCustomerAttributesXML;
            switch (attribute.AttributeControlType)
            {
                case AttributeControlType.DropdownList:
                case AttributeControlType.RadioList:
                case AttributeControlType.Checkboxes:
                    {
                        if (!string.IsNullOrEmpty(selectedAttributesXml))
                        {
                            if (!_customerAttributeParser.ParseValues(selectedAttributesXml, attribute.Id).Any())
                                break;

                            //clear default selection                                
                            foreach (var item in attributeModel.Values)
                                item.IsPreSelected = false;

                            //select new values
                            var selectedValues = await _customerAttributeParser.ParseAttributeValuesAsync(selectedAttributesXml);
                            foreach (var attributeValue in selectedValues)
                                foreach (var item in attributeModel.Values)
                                    if (attributeValue.Id == item.Id)
                                        item.IsPreSelected = true;
                        }
                    }
                    break;
                case AttributeControlType.ReadonlyCheckboxes:
                    {
                        //do nothing
                        //values are already pre-set
                    }
                    break;
                case AttributeControlType.TextBox:
                case AttributeControlType.MultilineTextbox:
                    {
                        if (!string.IsNullOrEmpty(selectedAttributesXml))
                        {
                            var enteredText = _customerAttributeParser.ParseValues(selectedAttributesXml, attribute.Id);
                            if (enteredText.Any())
                                attributeModel.DefaultValue = enteredText[0];
                        }
                    }
                    break;
                case AttributeControlType.ColorSquares:
                case AttributeControlType.ImageSquares:
                case AttributeControlType.Datepicker:
                case AttributeControlType.FileUpload:
                default:
                    //not supported attribute control types
                    break;
            }

            result.Add(attributeModel);
        }

        return result;
    }

    /// <summary>
    /// Prepare ConfigurationModel
    /// </summary>
    /// <param name="model">Model</param>
    /// <returns>A task that represents the asynchronous operation</returns>
    public async Task PrepareModelAsync(ConfigurationModel model)
    {
        //load settings for active store scope
        var storeId = await _storeContext.GetActiveStoreScopeConfigurationAsync();
        var b2BB2CFeaturesSettings = await _settingService.LoadSettingAsync<B2BB2CFeaturesSettings>(storeId);

        model.ServiceUrl = b2BB2CFeaturesSettings.SoltrackBaseUrl;
        model.AuthToken = b2BB2CFeaturesSettings.SoltrackPassword;
    }

    /// <summary>
    /// Prepare the B2CRegister result model
    /// </summary>
    /// <param name="resultId">Value of UserRegistrationType enum</param>
    /// <returns>Register result model</returns>
    public virtual async Task<B2CRegisterResultModel> PrepareB2CRegisterResultModelAsync(int resultId)
    {
        var session = EngineContext.Current.Resolve<IHttpContextAccessor>().HttpContext.Session;
        var model = new B2CRegisterResultModel();
        var message = string.Empty;
        var resultText = string.Empty;

        var customer = (UserRegistrationType)resultId == UserRegistrationType.EmailValidation
            ? await _customerService.GetCustomerByIdAsync(int.TryParse(session.GetString("CustomerId"), out var customerId) ? customerId : 0)
            : await _workContext.GetCurrentCustomerAsync();

        var erpNopUser = await _erpCustomerFunctionalityService.GetActiveErpNopUserByCustomerAsync(customer);

        if (erpNopUser?.ErpUserType == ErpUserType.B2CUser && erpNopUser.ErpShipToAddressId > 0)
        {
            var b2CShipToAddress = await _erpShipToAddressService.GetErpShipToAddressByIdAsync(erpNopUser.ErpShipToAddressId);
            if (b2CShipToAddress != null)
            {
                if (b2CShipToAddress.DeliveryOptionId == (int)DeliveryOption.DeliverOrCollect)
                {
                    model.IsCustomerOnDeliveryRoute = true;
                    message = await _localizationService.GetResourceAsync("Plugins.B2B.ManageB2BCustomer.Account.Register.Result.DeliveryMessage");
                }
                else
                {
                    var warehouse = await _shippingService.GetWarehouseByIdAsync(b2CShipToAddress.NearestWareHouseId ?? 0);

                    model.IsCustomerOnDeliveryRoute = false;
                    model.WarehouseName = warehouse?.Name ?? string.Empty;
                    model.WarehouseAddress = await _addressService.GetAddressByIdAsync(warehouse?.AddressId ?? 0);
                    var adminComment = warehouse?.AdminComment ?? string.Empty;

                    //lat, lng, and plant email is configured in the admin comment field
                    if (!string.IsNullOrEmpty(adminComment))
                    {
                        model.WarehouseAdminComment = adminComment;
                        model.WarehouseEmail = adminComment.Split(';').Where(n => n.ToLower().StartsWith("email:"))?.FirstOrDefault()?.Substring(6);
                        model.WarehouseLatitude = adminComment.Split(';').Where(n => n.ToLower().StartsWith("latitude:"))?.FirstOrDefault()?.Substring(9);
                        model.WarehouseLongitude = adminComment.Split(';').Where(n => n.ToLower().StartsWith("longitude:"))?.FirstOrDefault()?.Substring(10);
                    }

                    message = await _localizationService.GetResourceAsync("Plugins.B2B.ManageB2BCustomer.Account.Register.Result.CollectionMessage");
                }
            }
            model.IsRegistrationSuccessful = true;
            model.Title = await _localizationService.GetResourceAsync("Plugins.B2B.ManageB2BCustomer.B2C.Register.Successful");
        }

        var expressShopCode = session.GetString("ExpressShopCode");
        session.Remove("ExpressShopCode");

        if (!string.IsNullOrEmpty(expressShopCode))
        {
            model.Title = await _localizationService.GetResourceAsync("Plugins.B2B.ManageB2BCustomer.B2C.Register.UnSuccessful");
            message = (await _b2CMacsteelExpressShopService.GetB2CMacsteelExpressShopByCodeAsync(expressShopCode))?.Message ?? string.Empty;
        }

        switch ((UserRegistrationType)resultId)
        {
            case UserRegistrationType.Disabled:
                resultText = await _localizationService.GetResourceAsync("Plugins.B2B.ManageB2BCustomer.Account.B2C.Register.Result.Disabled");
                break;
            case UserRegistrationType.Standard:
                resultText = message;
                break;
            case UserRegistrationType.AdminApproval:
                resultText = await _localizationService.GetResourceAsync("Plugins.B2B.ManageB2BCustomer.Account.B2C.Register.Result.AdminApproval") + message;
                break;
            case UserRegistrationType.EmailValidation:
                resultText = message;
                break;
            default:
                break;
        }

        model.Result = resultText;
        return model;
    }


    #endregion
}
