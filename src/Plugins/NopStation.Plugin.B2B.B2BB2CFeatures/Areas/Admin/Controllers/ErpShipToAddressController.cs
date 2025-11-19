using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Core.Domain.Common;
using Nop.Services.Common;
using Nop.Services.Localization;
using Nop.Services.Messages;
using Nop.Services.Security;
using Nop.Web.Areas.Admin.Infrastructure.Mapper.Extensions;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Mvc.Filters;
using NopStation.Plugin.B2B.B2BB2CFeatures.Areas.Admin.Factories;
using NopStation.Plugin.B2B.B2BB2CFeatures.Areas.Admin.Models.ErpShipToAddress;
using NopStation.Plugin.B2B.ERPIntegrationCore.Domain;
using NopStation.Plugin.B2B.ERPIntegrationCore.Enums;
using NopStation.Plugin.B2B.ERPIntegrationCore.Services;
using NopStation.Plugin.Misc.Core.Controllers;
using NopStation.Plugin.B2B.B2BB2CFeatures;

namespace NopStation.Plugin.B2B.B2BB2CFeatures.Areas.Admin.Controllers;

public class ErpShipToAddressController : NopStationAdminController
{
    #region Fields

    private readonly ILocalizationService _localizationService;
    private readonly INotificationService _notificationService;
    private readonly IPermissionService _permissionService;
    private readonly IErpShipToAddressModelFactory _erpShipToAddressModelFactory;
    private readonly IErpShipToAddressService _erpShipToAddressService;
    private readonly IAddressService _addressService;
    private readonly IErpLogsService _erpLogsService;
    private readonly IErpAccountService _accountService;
    private readonly IWorkContext _workContext;
    private readonly IErpActivityLogsService _erpActivityLogsService;
    private readonly B2BB2CFeaturesSettings _b2BB2CFeaturesSettings;

    #endregion

    #region Ctor

    public ErpShipToAddressController(ILocalizationService localizationService,
        INotificationService notificationService,
        IPermissionService permissionService,
        IErpShipToAddressModelFactory erpShipToAddressModelFactory,
        IErpShipToAddressService erpShipToAddressService,
        IAddressService addressService,
        IErpActivityLogsService erpActivityLogsService,
        IErpLogsService erpLogsService,
        IErpAccountService erpAccountService,
        IWorkContext workContext,
        B2BB2CFeaturesSettings b2BB2CFeaturesSettings)
    {
        _localizationService = localizationService;
        _notificationService = notificationService;
        _permissionService = permissionService;
        _erpShipToAddressModelFactory = erpShipToAddressModelFactory;
        _erpShipToAddressService = erpShipToAddressService;
        _addressService = addressService;
        _erpLogsService = erpLogsService;
        _erpActivityLogsService = erpActivityLogsService;
        _accountService = erpAccountService;
        _workContext = workContext;
        _b2BB2CFeaturesSettings = b2BB2CFeaturesSettings;
    }

    #endregion

    #region Methods

    public async Task<IActionResult> Index()
    {
        return RedirectToAction("List");
    }

    public virtual async Task<IActionResult> List()
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManagePlugins))
            return AccessDeniedView();

        //prepare model
        var model = await _erpShipToAddressModelFactory.PrepareErpShipToAddressSearchModelAsync(new ErpShipToAddressSearchModel());

        return View(model);
    }

    [HttpPost]
    public virtual async Task<IActionResult> List(ErpShipToAddressSearchModel searchModel)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManagePlugins))
            return await AccessDeniedDataTablesJson();

        //prepare model
        var model = await _erpShipToAddressModelFactory.PrepareErpShipToAddressListModelAsync(searchModel);

        return Json(model);
    }

    public virtual async Task<IActionResult> Create()
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManagePlugins))
            return AccessDeniedView();

        //prepare model
        var model = await _erpShipToAddressModelFactory.PrepareErpShipToAddressModelAsync(new ErpShipToAddressModel(), null);

        return View(model);
    }

    [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
    [FormValueRequired("save", "save-continue")]
    public virtual async Task<IActionResult> Create(ErpShipToAddressModel model, bool continueEditing, IFormCollection form)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManagePlugins))
            return AccessDeniedView();

        if (ModelState.IsValid)
        {
            var currentCustomer = await _workContext.GetCurrentCustomerAsync();

            var address = model.AddressModel.ToEntity<Address>();
            //some validation
            if (address.CountryId == 0)
                address.CountryId = _b2BB2CFeaturesSettings.DefaultCountryId;
            if (address.StateProvinceId == 0)
                address.StateProvinceId = null;
            await _addressService.InsertAddressAsync(address);
            var erpShipToAddress = model.ToEntity<ErpShipToAddress>();
            erpShipToAddress.AddressId = address.Id;
            erpShipToAddress.CreatedOnUtc = DateTime.UtcNow;
            erpShipToAddress.CreatedById = currentCustomer.Id;

            var erpAccount = await _accountService.GetErpAccountByIdAsync(model.ErpAccountId);
            if (erpAccount != null)
            {
                var insertionResult = await _erpShipToAddressService.CreateErpShipToAddressWithMappingAsync(erpShipToAddress, erpAccount, ErpShipToAddressCreatedByType.Admin);
                if (insertionResult.ShipToAddress == null)
                {
                    _notificationService.ErrorNotification(insertionResult.ErrorMessage);
                    model = await _erpShipToAddressModelFactory.PrepareErpShipToAddressModelAsync(model, null, true);
                    return View(model);
                }

                var successMsg = await _localizationService.GetResourceAsync("Admin.ErpShipToAddresss.Added");
                _notificationService.SuccessNotification(successMsg);

                await _erpLogsService.InformationAsync($"{successMsg}. Erp Ship to Address Id: {erpShipToAddress.Id}. Erp Account Id: {erpAccount.Id}", ErpSyncLevel.ShipToAddress, customer: currentCustomer);

            }

            _notificationService.ErrorNotification(await _localizationService.GetResourceAsync("NopStation.Plugin.B2B.B2BB2CFeatures.Admin.ErpAccount.ErpAccountNotAvailable"));

            if (!continueEditing)
                return RedirectToAction("List");

            return RedirectToAction("Edit", new { id = erpShipToAddress.Id });
        }

        //prepare model
        model = await _erpShipToAddressModelFactory.PrepareErpShipToAddressModelAsync(model, null, true);

        //if we got this far, something failed, redisplay form
        return View(model);
    }

    public virtual async Task<IActionResult> Edit(int id)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManagePlugins))
            return AccessDeniedView();

        //try to get a erpShipToAddress with the specified id
        var erpShipToAddress = await _erpShipToAddressService.GetErpShipToAddressByIdAsync(id);
        if (erpShipToAddress == null)
            return RedirectToAction("List");

        //prepare model
        var model = await _erpShipToAddressModelFactory.PrepareErpShipToAddressModelAsync(null, erpShipToAddress);

        return View(model);
    }

    [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
    public virtual async Task<IActionResult> Edit(ErpShipToAddressModel model, bool continueEditing, IFormCollection form)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManagePlugins))
            return AccessDeniedView();

        //try to get a erpShipToAddress with the specified id
        var erpShipToAddress = await _erpShipToAddressService.GetErpShipToAddressByIdAsync(model.Id);
        if (erpShipToAddress == null)
            return RedirectToAction("List");

        if (ModelState.IsValid)
        {
            var currentCustomer = await _workContext.GetCurrentCustomerAsync();
            var address = model.AddressModel.ToEntity<Address>();
            await _addressService.UpdateAddressAsync(address);

            erpShipToAddress = model.ToEntity(erpShipToAddress);
            erpShipToAddress.AddressId = address.Id;
            erpShipToAddress.UpdatedById = currentCustomer.Id;
            erpShipToAddress.UpdatedOnUtc = DateTime.UtcNow;

            var erpAccount = await _accountService.GetErpAccountByIdAsync(model.ErpAccountId);
            if (erpAccount == null)
            {
                _notificationService.ErrorNotification(await _localizationService.GetResourceAsync("Admin.ErpShipToAddresss.Edit.Error.ErpAccountNotFound"));
                return RedirectToAction("Edit", new { id = erpShipToAddress.Id });
            }

            var countDuplicates = await _erpShipToAddressService.CountErpShipToAddressOfSameShipToCodeAndErpAccountIdAsync(model.ShipToCode,
                model.ErpAccountId,
                ErpShipToAddressCreatedByType.Admin);

            if (countDuplicates > 1)
            {
                _notificationService.ErrorNotification(await _localizationService.GetResourceAsync("Admin.ErpShipToAddresss.Edit.Error.DuplicateShipToAddressExistWithSameCodeAndAccountId"));
                return RedirectToAction("Edit", new { id = erpShipToAddress.Id });
            }

            await _erpShipToAddressService.UpdateErpShipToAddressAsync(erpShipToAddress);

            if (await _erpShipToAddressService.GetErpShipToAddressErpAccountMapByErpShipToAddressIdAsync(erpShipToAddress.Id) == null)
            {
                await _erpShipToAddressService.InsertErpShipToAddressErpAccountMapAsync(erpAccount, erpShipToAddress, ErpShipToAddressCreatedByType.Admin);
            }

            var shipToAddressErpAccountMap = await _erpShipToAddressService.GetErpShipToAddressErpAccountMapByErpShipToAddressIdAsync(erpShipToAddress.Id);

            if (shipToAddressErpAccountMap != null)
            {
                var successMsg = await _localizationService.GetResourceAsync("Admin.ErpShipToAddresss.Updated");
                _notificationService.SuccessNotification(successMsg);

                await _erpLogsService.InformationAsync($"{successMsg}. Erp Ship to Address Id: {erpShipToAddress.Id}. Erp Account Id: {shipToAddressErpAccountMap.ErpAccountId}", ErpSyncLevel.ShipToAddress, customer: currentCustomer);

            }
            else
            {
                var errorMsg = await _localizationService.GetResourceAsync("NopStation.Plugin.B2B.B2BB2CFeatures.Admin.ErpAccount.ErpAccountNotAvailable");
                _notificationService.ErrorNotification(errorMsg);

                await _erpLogsService.InformationAsync($"{errorMsg}. Erp Ship to Address Id: {erpShipToAddress.Id} could not be mapped with an erp-account.", ErpSyncLevel.ShipToAddress, customer: currentCustomer);
            }

            if (!continueEditing)
                return RedirectToAction("List");

            return RedirectToAction("Edit", new { id = erpShipToAddress.Id });
        }

        //prepare model
        model = await _erpShipToAddressModelFactory.PrepareErpShipToAddressModelAsync(model, erpShipToAddress, true);

        //if we got this far, something failed, redisplay form
        return View(model);
    }

    [HttpPost]
    public virtual async Task<IActionResult> Delete(int id)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManagePlugins))
            return AccessDeniedView();

        //try to get a erpShipToAddress with the specified id
        var erpShipToAddress = await _erpShipToAddressService.GetErpShipToAddressByIdAsync(id);
        if (erpShipToAddress == null)
            return RedirectToAction("List");

        var shipToAddressErpAccountMap = await _erpShipToAddressService.GetErpShipToAddressErpAccountMapByErpShipToAddressIdAsync(erpShipToAddress.Id);

        //delete a erpShipToAddress
        await _erpShipToAddressService.DeleteErpShipToAddressAsync(erpShipToAddress);

        var successMsg = await _localizationService.GetResourceAsync("Admin.ErpShipToAddresss.Deleted");
        _notificationService.SuccessNotification(successMsg);

        await _erpLogsService.InformationAsync($"{successMsg}. Erp Ship to Address Id: {erpShipToAddress.Id}. Erp Account Id: {shipToAddressErpAccountMap.ErpAccountId}", ErpSyncLevel.ShipToAddress, customer: await _workContext.GetCurrentCustomerAsync());

        return RedirectToAction("List");
    }

    #endregion

    #region exprot/excel

    [HttpPost, ActionName("List")]
    [FormValueRequired("exportexcel-all")]
    public virtual async Task<IActionResult> ExportExcelAll(ErpShipToAddressSearchModel searchModel)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.AccessAdminPanel))
            return AccessDeniedView();


        try
        {
            var bytes = await _erpShipToAddressModelFactory.ExportAllErpShipToAddressesToXlsxAsync(searchModel);
            return File(bytes, MimeTypes.TextXlsx, "Erp_ShipToAddresses.xlsx");
        }
        catch (Exception exc)
        {
            _notificationService.ErrorNotification(exc.Message);
            return RedirectToAction("List");
        }
    }

    [HttpPost]
    public virtual async Task<IActionResult> ExportExcelSelected(string selectedIds)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.AccessAdminPanel))
            return AccessDeniedView();

        try
        {
            if (string.IsNullOrEmpty(selectedIds))
            {
                _notificationService.ErrorNotification("No accounts selected.");
                return RedirectToAction("List");
            }

            var bytes = await _erpShipToAddressModelFactory.ExportSelectedErpShipToAddressesToXlsxAsync(selectedIds);
            return File(bytes, MimeTypes.TextXlsx, "Erp_ShipToAddress_Selected.xlsx");
        }
        catch (Exception exc)
        {
            _notificationService.ErrorNotification(exc.Message);
            return RedirectToAction("List");
        }
    }

    #endregion

    #region import/excel
    [HttpPost]
    public virtual async Task<IActionResult> ImportExcel(IFormFile importexcelfile)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.AccessAdminPanel))
            return AccessDeniedView();

        try
        {
            if (importexcelfile != null && importexcelfile.Length > 0)
            {
                await _erpShipToAddressModelFactory.ImportErpShipToAddressFromXlsxAsync(importexcelfile.OpenReadStream());
            }
            else
            {
                _notificationService.ErrorNotification(await _localizationService.GetResourceAsync("Admin.Common.UploadFile"));
                return RedirectToAction("List");
            }

            _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("NopStation.Plugin.B2B.B2BB2CFeatures.ErpShipToAddress.Imported"));
            return RedirectToAction("List");
        }
        catch (Exception exc)
        {
            _notificationService.ErrorNotification(exc.Message);
            return RedirectToAction("List");
        }
    }
    #endregion
}