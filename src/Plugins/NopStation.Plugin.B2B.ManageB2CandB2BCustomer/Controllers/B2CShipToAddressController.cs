using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Core.Domain.Common;
using Nop.Services.Common;
using Nop.Services.Configuration;
using Nop.Services.Customers;
using Nop.Services.Directory;
using Nop.Services.Localization;
using Nop.Services.Messages;
using Nop.Services.Shipping;
using Nop.Web.Controllers;
using Nop.Web.Factories;
using Nop.Web.Framework;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Mvc.Filters;
using NopStation.Plugin.B2B.B2BB2CFeatures;
using NopStation.Plugin.B2B.B2BB2CFeatures.Services.ErpCustomerFunctionality;
using NopStation.Plugin.B2B.B2BB2CFeatures.Services.SoltrackIntegration;
using NopStation.Plugin.B2B.ERPIntegrationCore.Domain;
using NopStation.Plugin.B2B.ERPIntegrationCore.Enums;
using NopStation.Plugin.B2B.ERPIntegrationCore.Services;
using NopStation.Plugin.B2B.ManageB2CandB2BCustomer.Factories;
using NopStation.Plugin.B2B.ManageB2CandB2BCustomer.Models;

namespace NopStation.Plugin.B2B.ManageB2CandB2BCustomer.Controllers;

public class B2CShipToAddressController : BasePublicController
{
    #region Fields

    private readonly ICustomerService _customerService;
    private readonly IWorkContext _workContext;
    private readonly IErpNopUserService _erpNopUserService;
    private readonly IErpShipToAddressService _erpShipToAddressService;
    private readonly IErpAccountService _erpAccountService;
    private readonly IErpSalesOrgService _erpSalesOrgService;
    private readonly INotificationService _notificationService;
    private readonly ILocalizationService _localizationService;
    private readonly B2BB2CFeaturesSettings _b2BB2CFeaturesSettings;
    private readonly ICustomerModelFactory _customerModelFactory;
    private readonly IB2CRegisterModelFactory _b2CRegisterModelFactory;
    private readonly IShippingService _shippingService;
    private readonly IStoreContext _storeContext;
    private readonly ISettingService _settingService;
    private readonly IAddressService _addressService;
    private readonly IErpCustomerFunctionalityService _erpCustomerFunctionalityService;
    private readonly ICountryService _countryService;
    private readonly IStateProvinceService _stateProvinceService;
    private readonly IErpLogsService _erpLogsService;
    private readonly ISoltrackIntegrationService _soltrackIntegrationService;
    private readonly IErpWarehouseSalesOrgMapService _erpWarehouseSalesOrgMapService;
    private readonly IB2CMacsteelExpressShopService _b2CMacsteelExpressShopService;

    #endregion Fields

    #region Ctor

    public B2CShipToAddressController(
        ICustomerService customerService,
        IWorkContext workContext,
        IErpNopUserService erpNopUserService,
        IErpShipToAddressService erpShipToAddressService,
        IErpAccountService erpAccountService,
        IErpSalesOrgService erpSalesOrgService,
        INotificationService notificationService,
        ILocalizationService localizationService,
        B2BB2CFeaturesSettings b2BB2CFeaturesSettings,
        ICustomerModelFactory customerModelFactory,
        IShippingService shippingService,
        IB2CRegisterModelFactory b2CRegisterModelFactory,
        IStoreContext storeContext,
        ISettingService settingService,
        IAddressService addressService,
        IErpCustomerFunctionalityService erpCustomerFunctionalityService,
        ICountryService countryService,
        IStateProvinceService stateProvinceService,
        IErpLogsService erpLogsService,
        ISoltrackIntegrationService soltrackIntegrationService,
        IErpWarehouseSalesOrgMapService erpWarehouseSalesOrgMapService,
        IB2CMacsteelExpressShopService b2CMacsteelExpressShopService)
    {
        _customerService = customerService;
        _workContext = workContext;
        _erpNopUserService = erpNopUserService;
        _erpShipToAddressService = erpShipToAddressService;
        _erpAccountService = erpAccountService;
        _erpSalesOrgService = erpSalesOrgService;
        _notificationService = notificationService;
        _localizationService = localizationService;
        _b2BB2CFeaturesSettings = b2BB2CFeaturesSettings;
        _customerModelFactory = customerModelFactory;
        _shippingService = shippingService;
        _b2CRegisterModelFactory = b2CRegisterModelFactory;
        _storeContext = storeContext;
        _settingService = settingService;
        _addressService = addressService;
        _erpCustomerFunctionalityService = erpCustomerFunctionalityService;
        _countryService = countryService;
        _stateProvinceService = stateProvinceService;
        _erpLogsService = erpLogsService;
        _soltrackIntegrationService = soltrackIntegrationService;
        _erpWarehouseSalesOrgMapService = erpWarehouseSalesOrgMapService;
        _b2CMacsteelExpressShopService = b2CMacsteelExpressShopService;
    }

    #endregion Ctor

    #region Methods

    [AuthorizeAdmin]
    [Area(AreaNames.ADMIN)]
    public async Task<IActionResult> Configure()
    {
        var model = new ConfigurationModel();
        await _b2CRegisterModelFactory.PrepareModelAsync(model);

        return View("~/Plugins/NopStation.Plugin.B2B.ManageB2CandB2BCustomer/Views/Shared/Configure.cshtml", model);
    }

    [AuthorizeAdmin]
    [Area(AreaNames.ADMIN)]
    [HttpPost, ActionName("Configure")]
    [FormValueRequired("save")]
    public async Task<IActionResult> Configure(ConfigurationModel model)
    {
        if (!ModelState.IsValid)
            return await Configure();

        var storeId = await _storeContext.GetActiveStoreScopeConfigurationAsync();
        _b2BB2CFeaturesSettings.SoltrackBaseUrl = model.ServiceUrl;
        _b2BB2CFeaturesSettings.SoltrackPassword = model.AuthToken;
        await _settingService.SaveSettingAsync(_b2BB2CFeaturesSettings, settings => settings.SoltrackBaseUrl, storeId, clearCache: false);
        await _settingService.SaveSettingAsync(_b2BB2CFeaturesSettings, settings => settings.SoltrackPassword, storeId, clearCache: false);
        await _settingService.ClearCacheAsync();

        _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.Plugins.Saved"));

        return await Configure();
    }

    public async Task<IActionResult> Create(string returnUrl)
    {
        var customer = await _workContext.GetCurrentCustomerAsync();

        if (customer == null || !await _customerService.IsRegisteredAsync(customer))
            return Challenge();

        var erpNopUser = await _erpNopUserService.GetErpNopUserByCustomerIdAsync(customer?.Id ?? 0, showHidden: false);

        if (erpNopUser is null || erpNopUser.ErpUserType != ErpUserType.B2CUser)
            return AccessDeniedView();

        var erpShipToAddress = await _erpShipToAddressService.GetErpShipToAddressByIdAsync(erpNopUser.ErpShipToAddressId);

        var defaultAddressChanged = "0";
        if (erpShipToAddress is not null && erpShipToAddress.DeliveryOptionId == (int)DeliveryOption.NoShop)
        {
            defaultAddressChanged = "1";
        }

        ViewBag.ReturnUrl = returnUrl;
        ViewBag.DefaultAddressChanged = defaultAddressChanged;

        return View(new B2CShipToAddressModel());
    }

    [HttpPost]
    public async Task<IActionResult> Create(B2CShipToAddressModel model, string returnUrl, IFormCollection form)
    {
        var customer = await _workContext.GetCurrentCustomerAsync();

        if (customer == null || !await _customerService.IsRegisteredAsync(customer))
            return Challenge();

        var erpNopUser = await _erpNopUserService.GetErpNopUserByCustomerIdAsync(customer.Id, showHidden: false);
        var erpAccount = await _erpCustomerFunctionalityService.GetActiveErpAccountByCustomerAsync(customer);

        if (erpAccount is null || erpNopUser is null || erpNopUser.ErpUserType != ErpUserType.B2CUser)
        {
            await _erpLogsService.ErrorAsync(
                $"Failed to create B2C Ship To Address for customer: {customer.Email} ({customer.Id}). Erp Nop User or Erp Account not found.", ErpSyncLevel.ShipToAddress, 
                customer: customer);

            _notificationService.ErrorNotification(await _localizationService.GetResourceAsync("Plugins.Payments.B2BCustomerAccount.B2CShipToAddress.Create.Failed.NopUserOrErpAccountNotFound"));

            return string.IsNullOrEmpty(returnUrl) ? RedirectToRoute("Homepage") : Redirect(returnUrl);
        }

        if (ModelState.IsValid)
        {
            try
            {
                ViewBag.ReturnUrl = returnUrl;
                var nearestTradingWarehouseCode = model.NearestWarehouseCode ?? "";
                var distanceToNearestTradingWarehouse = model.DistanceToNearestWarehouse;

                var isCustomerOnDeliveryRoute = model.IsCustomerOnDeliveryRoute;
                var deliveryOption = isCustomerOnDeliveryRoute ? (int)DeliveryOption.DeliverOrCollect : (int)DeliveryOption.Collect;
                var salesOrgWarehouseMap = await _erpWarehouseSalesOrgMapService.GetErpWarehouseSalesOrgMapByWarehouseCodeAsync(nearestTradingWarehouseCode, true);
                var nopWarehouse = await _shippingService.GetWarehouseByIdAsync(salesOrgWarehouseMap?.NopWarehouseId ?? 0);

                if (nopWarehouse == null || nopWarehouse.Id <= 0)
                {
                    ModelState.AddModelError("", await _localizationService.GetResourceAsync("NopStation.Plugin.B2B.ManageB2CandB2BCustomer.B2CShipToAddress.Create.WarehouseNotFound"));
                    await _erpLogsService.ErrorAsync(
                        $"Create B2C Ship To Address: Error, warehouse {nearestTradingWarehouseCode} not found", 
                        ErpSyncLevel.ShipToAddress);

                    return View(model);
                }

                var salesOrg = await _erpSalesOrgService.GetErpSalesOrgByTradingWarehouseIdAsync(nopWarehouse.Id);
                var erpAccountForB2C = await _erpAccountService.GetErpAccountByIdAsync(salesOrg?.ErpAccountIdForB2C ?? 0);

                if (salesOrg == null ||
                    salesOrg.Id <= 0 ||
                    salesOrg.ErpAccountIdForB2C <= 0 ||
                    erpAccountForB2C == null ||
                    erpAccountForB2C.Id < 0)
                {
                    ModelState.AddModelError("", await _localizationService.GetResourceAsync("NopStation.Plugin.B2B.ManageB2CandB2BCustomer.B2CShipToAddress.Create.SalesOrgNotFound"));

                    await _erpLogsService.ErrorAsync(
                        $"Create B2C Ship To Address: Error, Sales Organisation or B2B account not found for Warehouse {nopWarehouse.Name} (Id: {nopWarehouse.Id})", 
                        ErpSyncLevel.ShipToAddress);

                    return View(model);
                }

                var countryId = (await _countryService.GetCountryByTwoLetterIsoCodeAsync(model.CountryCode))?.Id ?? 0;
                if (countryId <= 0)
                {
                    ModelState.AddModelError("", await _localizationService.GetResourceAsync("NopStation.Plugin.B2B.ManageB2CandB2BCustomer.B2CShipToAddress.Create.CountryOrStateProvinceNotFound"));
                    await _erpLogsService.ErrorAsync(
                        $"Create B2C Ship To Address: Error, Country not found for customer: {customer.Email}, " +
                        $"given Country: {model.Country}, Country Code: {model.CountryCode}", 
                        ErpSyncLevel.ShipToAddress, 
                        customer: customer);

                    return View(model);
                }

                var stateProvinceId = (await _stateProvinceService.GetStateProvinceByAbbreviationAsync(model.StateProvinceCode, countryId))?.Id ??
                    (await _stateProvinceService.GetStateProvincesByCountryIdAsync(countryId)).FirstOrDefault(sp => sp.Name == model.StateProvince)?.Id ?? 0;
                if (stateProvinceId <= 0)
                {
                    ModelState.AddModelError("", await _localizationService.GetResourceAsync("NopStation.Plugin.B2B.ManageB2CandB2BCustomer.B2CShipToAddress.Create.CountryOrStateProvinceNotFound"));
                    await _erpLogsService.ErrorAsync(
                        $"Create B2C Ship To Address: Error, State/Province not found for customer: {customer.Email}, " +
                        $"Country Name: {model.Country}, given State/Province: {model.StateProvince}, State/Province code: {model.StateProvinceCode}", ErpSyncLevel.ShipToAddress, 
                        customer: customer);

                    return View(model);
                }

                var address = new Address
                {
                    Email = customer.Email,
                    FirstName = customer.FirstName,
                    LastName = customer.LastName,
                    Company = model.Company,
                    Address1 = model.HouseNumber,
                    Address2 = model.Street,
                    City = model.City,
                    StateProvinceId = stateProvinceId,
                    CountryId = countryId,
                    ZipPostalCode = model.PostalCode,
                    PhoneNumber = model.Phone,
                    County = customer.County,
                    FaxNumber = customer.Fax,
                    CreatedOnUtc = customer.CreatedOnUtc
                };

                await _addressService.InsertAddressAsync(address);
                await _customerService.InsertCustomerAddressAsync(customer, address);

                var erpShipToAddress = new ErpShipToAddress
                {
                    NearestWareHouseId = nopWarehouse?.Id ?? 0,
                    AddressId = address?.Id ?? 0,
                    Suburb = model.Suburb,
                    ProvinceCode = (await _stateProvinceService.GetStateProvinceByIdAsync(address.StateProvinceId ?? 0))?.Abbreviation ?? string.Empty,
                    DeliveryOptionId = deliveryOption,
                    DistanceToNearestWareHouse = distanceToNearestTradingWarehouse,
                    Latitude = model.Latitude,
                    Longitude = model.Longitude,
                    IsActive = true,
                    CreatedOnUtc = DateTime.UtcNow,
                    CreatedById = customer.Id,
                    UpdatedOnUtc = DateTime.UtcNow,
                    UpdatedById = customer.Id,
                    ShipToCode = _erpShipToAddressService.GenerateUniqueShipToCode(),
                    RepNumber = string.Empty,
                    ShipToName = $"{customer.FirstName ?? ""} {customer.LastName ?? ""}",
                };

                await _erpShipToAddressService.InsertErpShipToAddressAsync(erpShipToAddress);
                /// 
                /// compare old and new salesOrgId
                /// salesOrg is found by nearest warehouse, which is found by warehouse code, and this is for the new address
                /// salesOrg?.Id -> from new address
                /// erpAccount?.ErpSalesOrgId -> from prev address [because its from current erpAccount]
                if (erpAccount?.ErpSalesOrgId != salesOrg?.Id)
                {
                    await _erpCustomerFunctionalityService.AddOrUpdateB2CUserSpecialRolesAsync(customer, oldSalesOrgId: erpAccount?.ErpSalesOrgId ?? 0, newSalesOrgId: salesOrg?.Id ?? 0);
                }

                await _erpShipToAddressService.InsertErpShipToAddressErpAccountMapAsync(erpAccountForB2C, erpShipToAddress, ErpShipToAddressCreatedByType.User);

                erpNopUser.ErpShipToAddressId = erpShipToAddress.Id;
                erpNopUser.ErpAccountId = salesOrg.ErpAccountIdForB2C;
                await _erpNopUserService.UpdateErpNopUserAsync(erpNopUser);

                var b2CShipToAddresses =
                    await _erpShipToAddressService.GetErpShipToAddressesByCustomerAddressesAsync(
                            customerId: customer.Id,
                            erpAccountId: erpAccount.Id,
                            erpShipToAddressCreatedByTypeId: (int)ErpShipToAddressCreatedByType.User
                        );

                foreach (var addres in b2CShipToAddresses)
                {
                    if (addres.DeliveryOptionId == (int)DeliveryOption.NoShop)
                    {
                        addres.IsActive = false;
                        await _erpShipToAddressService.UpdateErpShipToAddressAsync(addres);
                    }
                }

                if (!isCustomerOnDeliveryRoute)
                {
                    TempData["SuccessMessage"] = "Address created successfully but not in delivery route";
                    return RedirectToAction(nameof(Create), new { returnUrl = returnUrl });
                }

                _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("NopStation.Plugin.B2B.ManageB2CandB2BCustomer.B2CShipToAddress.Create.Successful"));
                if (string.IsNullOrEmpty(returnUrl) || !Url.IsLocalUrl(returnUrl))
                    return RedirectToRoute("Homepage");

                return Redirect(returnUrl);
            }
            catch (Exception ex)
            {
                await _erpLogsService.InsertErpLogAsync(
                    ErpLogLevel.Error, 
                    ErpSyncLevel.ShipToAddress,
                    $"Create B2C Ship To Address: Error, Failed for customer: {customer?.Email} (Id: {customer?.Id}). Due to - {ex.Message}", 
                    ex.StackTrace);
            }
        }

        ModelState.AddModelError("", await _localizationService.GetResourceAsync("Plugins.Payments.B2BCustomerAccount.B2CShipToAddress.Create.Failed"));

        return View(model);
    }

    public async Task<IActionResult> DeleteB2CShipToAddress(int b2CShipToAddressId, string returnUrl)
    {
        var customer = await _workContext.GetCurrentCustomerAsync();

        if (customer == null || !await _customerService.IsRegisteredAsync(customer))
            return Challenge();

        var erpUser = await _erpNopUserService.GetErpNopUserByCustomerIdAsync(customer?.Id ?? 0, showHidden: false);

        if (erpUser is null || erpUser.ErpUserType != ErpUserType.B2CUser)
            return AccessDeniedView();

        try
        {
            var erpShipToAddress = await _erpShipToAddressService.GetErpShipToAddressByIdAsync(b2CShipToAddressId);
            await _erpShipToAddressService.DeleteErpShipToAddressAsync(erpShipToAddress);

            var currentErpAccount = await _erpAccountService.GetErpAccountByErpShipToAddressAsync(erpShipToAddress);
            await _erpShipToAddressService.RemoveErpShipToAddressErpAccountMapAsync(currentErpAccount, erpShipToAddress);

            _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("NopStation.Plugin.B2B.ManageB2CandB2BCustomer.B2CShipToAddressSelector.Delete.Successful"));
        }
        catch (Exception ex)
        {
            await _erpLogsService.InsertErpLogAsync(
                ErpLogLevel.Error, 
                ErpSyncLevel.ShipToAddress,
                $"Delete B2C Ship To Address: Error, failed for customer {customer?.Email} (Id: {customer?.Id}), " +
                $"Ship-To-Address Id: {b2CShipToAddressId}. Due to - {ex.Message}", 
                ex.StackTrace);

            _notificationService.ErrorNotification(await _localizationService.GetResourceAsync("Plugins.Payments.B2BCustomerAccount.B2CShipToAddress.Delete.Failed"));
        }

        if (string.IsNullOrEmpty(returnUrl) || !Url.IsLocalUrl(returnUrl))
            return RedirectToRoute("Homepage");

        return Redirect(returnUrl);
    }

    public async Task<IActionResult> DeleteDefaultB2CShipToAddress(string returnUrl)
    {
        var customer = await _workContext.GetCurrentCustomerAsync();

        if (customer == null || !await _customerService.IsRegisteredAsync(customer))
            return Challenge();

        var erpUser = await _erpNopUserService.GetErpNopUserByCustomerIdAsync(customer?.Id ?? 0, showHidden: false);

        if (erpUser is null || erpUser.ErpUserType != ErpUserType.B2CUser)
            return AccessDeniedView();

        try
        {
            var erpShipToAddress = await _erpShipToAddressService.GetErpShipToAddressByIdAsync(erpUser.ErpShipToAddressId);
            await _erpShipToAddressService.DeleteErpShipToAddressAsync(erpShipToAddress);

            var currentErpAccount = await _erpAccountService.GetErpAccountByErpShipToAddressAsync(erpShipToAddress);
            await _erpShipToAddressService.RemoveErpShipToAddressErpAccountMapAsync(currentErpAccount, erpShipToAddress);

            IList<ErpShipToAddress> erpshipToAddressList;
            ErpShipToAddress nextDefaulterpShipToAddress;

            //set the next most recent one as the new default address
            if (_b2BB2CFeaturesSettings.UseDefaultAccountForB2CUser)
            {
                erpshipToAddressList = await _erpShipToAddressService.GetErpShipToAddressesByCustomerAddressesAsync(
                    customerId: customer.Id,
                    erpShipToAddressCreatedByTypeId: (int)ErpShipToAddressCreatedByType.User);
            }
            else
            {
                erpshipToAddressList = await _erpShipToAddressService.GetErpShipToAddressesByErpAccountIdAsync(erpAccountId: erpUser.ErpAccountId);
            }

            nextDefaulterpShipToAddress = erpshipToAddressList.OrderByDescending(x => x.CreatedOnUtc).FirstOrDefault();

            return RedirectToRoute("UpdateDefaultShipToAddressOfB2CUser", 
                new { erpUserId = erpUser.Id, newErpShipToAddressId = nextDefaulterpShipToAddress?.Id ?? 0, returnUrl = returnUrl });
        }
        catch (Exception ex)
        {
            await _erpLogsService.InsertErpLogAsync(
                ErpLogLevel.Error, 
                ErpSyncLevel.ShipToAddress,
                $"Delete default B2C Ship To Address: Error, failed for customer {customer?.Email} (Id: {customer?.Id}), " +
                $"Ship-To-Address Id: {erpUser.ErpShipToAddressId}. Due to - {ex.Message}", 
                ex.StackTrace);
            _notificationService.ErrorNotification(await _localizationService.GetResourceAsync("NopStation.Plugin.B2B.ManageB2CandB2BCustomer.B2CShipToAddressSelector.Delete.Failed"));
        }

        if (string.IsNullOrEmpty(returnUrl) || !Url.IsLocalUrl(returnUrl))
            return RedirectToRoute("Homepage");

        return Redirect(returnUrl);
    }

    public async Task<IActionResult> IsSalesOrgSameForOldAndNewB2CShipToAddress(int erpUserId, int newErpShipToAddressId)
    {
        var customer = await _workContext.GetCurrentCustomerAsync();

        if (customer == null || !await _customerService.IsRegisteredAsync(customer))
            return Challenge();

        var erpUser = await _erpNopUserService.GetErpNopUserByCustomerIdAsync(customer?.Id ?? 0, showHidden: false);

        if (erpUser is null || erpUser.ErpUserType != ErpUserType.B2CUser)
            return AccessDeniedView();

        if (erpUser.Id != erpUserId)
            return Json(new { success = false });

        if (erpUser.ErpShipToAddressId == newErpShipToAddressId)
            return Json(new { success = true });

        var newErpShipToAddress = await _erpShipToAddressService.GetErpShipToAddressByIdAsync(newErpShipToAddressId);

        if (newErpShipToAddress is null)
            return Json(new { success = false });

        var currentErpShipToAddress = await _erpShipToAddressService.GetErpShipToAddressByIdAsync(erpUser.ErpShipToAddressId);

        if (currentErpShipToAddress is null)
            return Json(new { success = false });

        var currentErpAccount = await _erpAccountService.GetErpAccountByErpShipToAddressAsync(currentErpShipToAddress);
        var newErpAccount = await _erpAccountService.GetErpAccountByErpShipToAddressAsync(newErpShipToAddress);

        if (currentErpAccount is null || newErpAccount is null)
            return Json(new { success = false });

        if (currentErpAccount.ErpSalesOrgId == newErpAccount.ErpSalesOrgId)
            return Json(new { success = true });

        return Json(new { Success = false });
    }

    public async Task<IActionResult> IsShipToAddressOnlyOne(int erpUserId)
    {
        var customer = await _workContext.GetCurrentCustomerAsync();

        if (customer == null || !await _customerService.IsRegisteredAsync(customer))
            return Challenge();

        var erpUser = await _erpNopUserService.GetErpNopUserByCustomerIdAsync(customer?.Id ?? 0, showHidden: false);

        if (erpUser is null || erpUser.ErpUserType != ErpUserType.B2CUser)
            return AccessDeniedView();

        IList<ErpShipToAddress> erpshipToAddressList;

        if (_b2BB2CFeaturesSettings.UseDefaultAccountForB2CUser)
        {
            erpshipToAddressList = await _erpShipToAddressService.GetErpShipToAddressesByCustomerAddressesAsync(
                customerId: customer.Id,
                erpShipToAddressCreatedByTypeId: (int)ErpShipToAddressCreatedByType.User);
        }
        else
        {
            erpshipToAddressList = await _erpShipToAddressService.GetErpShipToAddressesByErpAccountIdAsync(erpUser.ErpAccountId);
        }

        return Json(new { success = erpshipToAddressList.Count == 1 });
    }

    public async Task<IActionResult> CheckShiptoAddressIfExist(string latitude, string longitude)
    {
        var customer = await _workContext.GetCurrentCustomerAsync();

        if (string.IsNullOrEmpty(latitude) || string.IsNullOrEmpty(longitude))
        {
            await _erpLogsService.ErrorAsync($"CheckShiptoAddressIfExist: Latitude Or Longitude field is empty. Customer email: {customer.Email}", ErpSyncLevel.ShipToAddress, customer: customer);
            return Json(new
            {
                Success = false,
                IsExist = false
            });
        }

        if (customer == null || !await _customerService.IsRegisteredAsync(customer))
            return Challenge();

        var erpUser = await _erpNopUserService.GetErpNopUserByCustomerIdAsync(customer?.Id ?? 0, showHidden: false);

        if (erpUser is null || erpUser.ErpUserType != ErpUserType.B2CUser)
            return AccessDeniedView();

        IList<ErpShipToAddress> erpshipToAddressList = null;

        if (_b2BB2CFeaturesSettings.UseDefaultAccountForB2CUser)
        {
            erpshipToAddressList = await _erpShipToAddressService.GetErpShipToAddressesByCustomerAddressesAsync(
                customerId: customer.Id,
                erpShipToAddressCreatedByTypeId: (int)ErpShipToAddressCreatedByType.User);
        }
        else
        {
            erpshipToAddressList = await _erpShipToAddressService.GetErpShipToAddressesByErpAccountIdAsync(erpAccountId: erpUser.ErpAccountId);
        }

        if (erpshipToAddressList is null || !erpshipToAddressList.Any())
        {
            return Json(new
            {
                Success = true,
                IsExist = false
            });
        }

        var isExist = erpshipToAddressList
                        .FirstOrDefault(x => !string.IsNullOrEmpty(x.Longitude) && !string.IsNullOrEmpty(longitude) &&
                            !string.IsNullOrEmpty(x.Latitude) && !string.IsNullOrEmpty(latitude) &&
                            (double.Parse(x.Longitude).ToString("#.####") == double.Parse(longitude).ToString("#.####")) &&
                            (double.Parse(x.Latitude).ToString("#.####") == double.Parse(latitude).ToString("#.####"))) != null;


        return Json(new
        {
            Success = true,
            IsExist = isExist
        });
    }

    public async Task<IActionResult> UpdateDefaultShipToAddressOfB2CUser(int erpUserId, int newErpShipToAddressId, string returnUrl)
    {
        var customer = await _workContext.GetCurrentCustomerAsync();

        if (customer == null || !await _customerService.IsRegisteredAsync(customer))
            return Challenge();

        var erpUser = await _erpNopUserService.GetErpNopUserByCustomerIdAsync(customer.Id, showHidden: false);

        if (erpUser is null || erpUser.ErpUserType != ErpUserType.B2CUser)
            return AccessDeniedView();

        try
        {
            var erpShipToAddress = await _erpShipToAddressService.GetErpShipToAddressByIdAsync(newErpShipToAddressId);

            if (erpShipToAddress is null || erpShipToAddress.Id <= 0)
            {
                await _erpLogsService.ErrorAsync(
                    $"Update Ship To Address of B2C Erp Nop User: new Erp Ship-To-Address not found for customer {customer.Email} (Id: {customer.Id}), " +
                    $"Erp Nop User Id: {erpUser.Id}", 
                    ErpSyncLevel.ShipToAddress, 
                    customer: customer);

                _notificationService.ErrorNotification(await _localizationService.GetResourceAsync("NopStation.Plugin.B2B.ManageB2CandB2BCustomer.B2CShipToAddressSelector.Update.Failed"));

                if (string.IsNullOrEmpty(returnUrl) || !Url.IsLocalUrl(returnUrl))
                    return RedirectToRoute("Homepage");

                return Redirect(returnUrl);
            }

            var newErpAccount = await _erpAccountService.GetErpAccountByErpShipToAddressAsync(erpShipToAddress);

            if (newErpAccount is null)
            {
                _notificationService.ErrorNotification(await _localizationService.GetResourceAsync("NopStation.Plugin.B2B.ManageB2CandB2BCustomer.B2CShipToAddressSelector.Update.Failed"));
                await _erpLogsService.ErrorAsync(
                    $"Update Ship To Address of Erp Nop User: Erp Account not found for customer {customer.Email} (Id: {customer.Id}), " +
                    $"Erp Nop User Id: {erpUser.Id}, Erp Ship-To-Address Id: {erpShipToAddress.Id}", 
                    ErpSyncLevel.ShipToAddress, 
                    customer: customer);

                if (string.IsNullOrEmpty(returnUrl) || !Url.IsLocalUrl(returnUrl))
                    return RedirectToRoute("Homepage");

                return Redirect(returnUrl);
            }

            var erpSalesOrg = await _erpSalesOrgService.GetErpSalesOrgByIdAsync(newErpAccount.ErpSalesOrgId);

            if (erpSalesOrg is null || erpSalesOrg.Id <= 0)
            {
                _notificationService.ErrorNotification(await _localizationService.GetResourceAsync("NopStation.Plugin.B2B.ManageB2CandB2BCustomer.B2CShipToAddressSelector.Update.Failed"));
                await _erpLogsService.ErrorAsync($"Update ERP Ship To Address of ERP B2C User: ERP sales org not found for customer {customer.Email}, erp b2c user id: {erpUser.Id}, erp ship to address id: {erpShipToAddress.Id}", ErpSyncLevel.ShipToAddress, customer: customer);

                if (string.IsNullOrEmpty(returnUrl) || !Url.IsLocalUrl(returnUrl))
                    return RedirectToRoute("Homepage");

                return Redirect(returnUrl);
            }

            IList<ErpShipToAddress> erpshipToAddressList;
            if (_b2BB2CFeaturesSettings.UseDefaultAccountForB2CUser)
            {
                erpshipToAddressList = await _erpShipToAddressService.GetErpShipToAddressesByCustomerAddressesAsync(
                    customerId: customer.Id,
                    erpShipToAddressCreatedByTypeId: (int)ErpShipToAddressCreatedByType.User);
            }
            else
            {
                erpshipToAddressList = await _erpShipToAddressService.GetErpShipToAddressesByErpAccountIdAsync(erpAccountId: erpUser.ErpAccountId);
            }

            foreach (var addres in erpshipToAddressList)
            {
                if (addres.DeliveryOptionId == (int)DeliveryOption.NoShop)
                {
                    addres.IsActive = false;
                    await _erpShipToAddressService.UpdateErpShipToAddressAsync(addres);
                }
            }

            /// compare old and new salesOrgId
            /// just compare newErpAccount.SalesOrgId and currentErpAccount.SalesOrgId
            var currentErpAccount = await _erpCustomerFunctionalityService.GetActiveErpAccountByCustomerAsync(customer);
            if (currentErpAccount != null &&
                (currentErpAccount.ErpSalesOrgId != newErpAccount.ErpSalesOrgId))
            {
                await _erpCustomerFunctionalityService.AddOrUpdateB2CUserSpecialRolesAsync(customer, oldSalesOrgId: currentErpAccount?.ErpSalesOrgId ?? 0,
                    newSalesOrgId: newErpAccount?.ErpSalesOrgId ?? 0);
            }

            erpUser.ErpShipToAddressId = newErpShipToAddressId;
            erpUser.ErpAccountId = newErpAccount.Id;
            await _erpNopUserService.UpdateErpNopUserAsync(erpUser);

            var (isCustomerInExpressShopZone, isCustomerOnDeliveryRoute, _) = await _soltrackIntegrationService.GetSoltrackResponseAsync(customer, erpShipToAddress.Latitude, erpShipToAddress.Longitude);

            if (isCustomerInExpressShopZone)
            {
                erpShipToAddress.DeliveryOptionId = (int)DeliveryOption.NoShop;
                await _erpShipToAddressService.UpdateErpShipToAddressAsync(erpShipToAddress);

                _notificationService.ErrorNotification(await _localizationService.GetResourceAsync("Plugins.Payments.B2BCustomerAccount.B2CShipToAddressSelector.NoShopZone.NotUpdatedPreviousOne"));
                await _erpLogsService.ErrorAsync($"Update B2C Ship To Address to no shop zone", ErpSyncLevel.ShipToAddress, customer: customer);
                return Redirect(returnUrl);
            }
            else
            {
                erpShipToAddress.DeliveryOptionId = isCustomerOnDeliveryRoute ? (int)DeliveryOption.DeliverOrCollect : (int)DeliveryOption.Collect;
                await _erpShipToAddressService.UpdateErpShipToAddressAsync(erpShipToAddress);
            }

            _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("NopStation.Plugin.B2B.ManageB2CandB2BCustomer.B2CShipToAddressSelector.Update.Success"));

            if (string.IsNullOrEmpty(returnUrl) || !Url.IsLocalUrl(returnUrl))
                return RedirectToRoute("Homepage");

            return Redirect(returnUrl);
        }
        catch (Exception ex)
        {
            await _erpLogsService.InsertErpLogAsync(
                ErpLogLevel.Error, 
                ErpSyncLevel.ShipToAddress, 
                $"Update Erp Ship To Address: Failed for Customer {customer.Email}, new Erp Ship-To Address Id: {newErpShipToAddressId} " +
                $"Erp Nop User Id: {erpUserId}. " + ex.Message, ex.StackTrace);
        }

        await _erpLogsService.ErrorAsync($"Update B2C Ship To Address: " +
            $"Failed for customer {customer.Email}, new ship to address id: {newErpShipToAddressId}, " +
            $"erp B2C User Id: {erpUserId}", ErpSyncLevel.ShipToAddress, customer: customer);
        _notificationService.ErrorNotification(await _localizationService.GetResourceAsync("NopStation.Plugin.B2B.ManageB2CandB2BCustomer.B2CShipToAddressSelector.Update.Failed"));

        if (string.IsNullOrEmpty(returnUrl) || !Url.IsLocalUrl(returnUrl))
            return RedirectToRoute("Homepage");

        return Redirect(returnUrl);
    }

    public async Task<IActionResult> CheckGeoFencingResponse(string latitude, string longitude)
    {
        var customer = await _workContext.GetCurrentCustomerAsync();

        if (customer == null || !await _customerService.IsRegisteredAsync(customer))
            return Challenge();

        var erpUser = await _erpNopUserService.GetErpNopUserByCustomerIdAsync(customer.Id, showHidden: false);

        if (erpUser is null || erpUser.ErpUserType != ErpUserType.B2CUser)
            return AccessDeniedView();

        if (string.IsNullOrEmpty(latitude) || string.IsNullOrEmpty(longitude))
        {
            await _erpLogsService.ErrorAsync($"CheckSoltrackResponse: Latitude Or Longitude was not found. Customer email: {customer.Email}", ErpSyncLevel.ShipToAddress);

            return Json(new { Success = false });
        }

        var (isCustomerInExpressShopZone, isCustomerOnDeliveryRoute, response) = await _soltrackIntegrationService.GetSoltrackResponseAsync(customer, latitude, longitude);

        if (response == null)
        {
            await _erpLogsService.ErrorAsync($"CheckSoltrackResponse: Soltrack call returned error. Customer email: {customer.Email}", ErpSyncLevel.ShipToAddress);

            return Json(new { Success = false });
        }

        if (isCustomerInExpressShopZone)
        {
            await _erpLogsService.WarningAsync($"CheckSoltrackResponse: Customer {customer.Email} is in express shop zone {response.ExpressStoreAreaEntityName}", ErpSyncLevel.ShipToAddress);

            var message = (await _b2CMacsteelExpressShopService.GetB2CMacsteelExpressShopByCodeAsync(response.BranchAreaEntityName))?.Message;
            return Json(new { Success = true, IsCustomerInExpressShopZone = true, Message = message });
        }

        var currentShipToAddress = await _erpShipToAddressService.GetErpShipToAddressByIdWithActiveAsync(erpUser?.ErpShipToAddressId ?? 0);
        var nearestTradingWarehouseCode = response.BranchAreaEntityName;
        var distanceToNearestnearestTradingWarehouse = response.DistanceFromBranchArea;
        var isSalesOrgSame = true;

        if (currentShipToAddress == null || currentShipToAddress.Id <= 0)
        {
            await _erpLogsService.ErrorAsync($"CheckSoltrackResponse: current ship to address not found. Customer email: {customer.Email}", ErpSyncLevel.ShipToAddress);
            return Json(new
            {
                Success = true,
                IsSalesOrgSame = isSalesOrgSame,
                NearestTradingWarehouseCode = nearestTradingWarehouseCode,
                DistanceToNearestnearestTradingWarehouse = distanceToNearestnearestTradingWarehouse,
                IsCustomerOnDeliveryRoute = isCustomerOnDeliveryRoute
            });
        }

        var nopWarehouse = await _erpSalesOrgService.GetNopWarehousebyB2CWarehouseCodeAsync(nearestTradingWarehouseCode);
        if (nopWarehouse == null || nopWarehouse.Id <= 0)
        {
            await _erpLogsService.ErrorAsync($"CheckSoltrackResponse: Warehouse {nearestTradingWarehouseCode} not found for customer: {customer.Email}", ErpSyncLevel.ShipToAddress);

            return Json(new { Success = false });
        }

        var newSalesOrg = await _erpSalesOrgService.GetErpSalesOrgByTradingWarehouseIdAsync(nopWarehouse.Id);
        var erpAccount = await _erpAccountService.GetErpAccountByErpShipToAddressAsync(currentShipToAddress);
        var oldSalesOrg = await _erpSalesOrgService.GetErpSalesOrgByIdWithActiveAsync(erpAccount?.ErpSalesOrgId ?? 0);

        if (oldSalesOrg == null || newSalesOrg == null)
        {
            await _erpLogsService.ErrorAsync($"CheckSoltrackResponse: old or new sales org not found for customer: {customer.Email}", ErpSyncLevel.ShipToAddress);

            return Json(new { Success = false });
        }

        isSalesOrgSame = oldSalesOrg.Code == newSalesOrg.Code;

        return Json(new
        {
            Success = true,
            IsSalesOrgSame = isSalesOrgSame,
            NearestTradingWarehouseCode = nearestTradingWarehouseCode,
            DistanceToNearestnearestTradingWarehouse = distanceToNearestnearestTradingWarehouse,
            IsCustomerOnDeliveryRoute = isCustomerOnDeliveryRoute
        });
    }

    #endregion Methods
}