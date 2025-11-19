using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Core.Domain.Common;
using Nop.Services.Common;
using Nop.Services.Localization;
using Nop.Services.Media;
using Nop.Services.Messages;
using Nop.Services.Security;
using Nop.Web.Areas.Admin.Infrastructure.Mapper.Extensions;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Mvc.Filters;
using NopStation.Plugin.B2B.B2BB2CFeatures.Areas.Admin.Factories;
using NopStation.Plugin.B2B.B2BB2CFeatures.Areas.Admin.Models;
using NopStation.Plugin.B2B.B2BB2CFeatures.Areas.Admin.Models.ErpShipToAddress;
using NopStation.Plugin.B2B.ERPIntegrationCore.Domain;
using NopStation.Plugin.B2B.ERPIntegrationCore.Enums;
using NopStation.Plugin.B2B.ERPIntegrationCore.Services;
using NopStation.Plugin.Misc.Core.Controllers;
using NopStation.Plugin.B2B.B2BB2CFeatures;

namespace NopStation.Plugin.B2B.B2BB2CFeatures.Areas.Admin.Controllers;

public class ErpAccountController : NopStationAdminController
{
    #region Fields

    private readonly IAddressService _addressService;
    private readonly IPermissionService _permissionService;
    private readonly ILocalizationService _localizationService;
    private readonly INotificationService _notificationService;
    private readonly IErpLogsService _erpLogsService;
    private readonly IErpAccountService _erpAccountService;
    private readonly IErpSalesOrgService _erpSalesOrgService;
    private readonly IErpAccountModelFactory _erpAccountModelFactory;
    private readonly IErpNopUserModelFactory _erpNopUserModelFactory;
    private readonly IErpActivityLogsService _erpActivityLogsService;
    private readonly IErpShipToAddressModelFactory _erpShipToAddressModelFactory;
    private readonly IPictureService _pictureService;
    private readonly IWorkContext _workContext;
    private readonly B2BB2CFeaturesSettings _b2BB2CFeaturesSettings;

    #endregion

    #region Ctor

    public ErpAccountController(
        IAddressService addressService,
        IPermissionService permissionService,
        ILocalizationService localizationService,
        INotificationService notificationService,
        IErpLogsService erpLogsService,
        IErpAccountService erpAccountService,
        IErpSalesOrgService erpSalesOrgService,
        IErpAccountModelFactory erpAccountModelFactory,
        IErpNopUserModelFactory erpNopUserModelFactory,
        IErpActivityLogsService erpActivityLogsService,
        IErpShipToAddressModelFactory erpShipToAddressModelFactory,
        IPictureService pictureService,
        IWorkContext workContext,
        B2BB2CFeaturesSettings b2BB2CFeaturesSettings
    )
    {
        _addressService = addressService;
        _permissionService = permissionService;
        _localizationService = localizationService;
        _notificationService = notificationService;
        _erpLogsService = erpLogsService;
        _erpAccountService = erpAccountService;
        _erpSalesOrgService = erpSalesOrgService;
        _erpAccountModelFactory = erpAccountModelFactory;
        _erpNopUserModelFactory = erpNopUserModelFactory;
        _erpActivityLogsService = erpActivityLogsService;
        _erpShipToAddressModelFactory = erpShipToAddressModelFactory;
        _pictureService = pictureService;
        _workContext = workContext;
        _b2BB2CFeaturesSettings = b2BB2CFeaturesSettings;
    }

    #endregion

    #region Utilities

    private async Task<string> GetErpSalesOrganisationNameAndCodeByIdAsync(int salesOrgId)
    {
        if (salesOrgId == 0)
            return string.Empty;

        var salesOrg = await _erpSalesOrgService.GetErpSalesOrgByIdAsync(salesOrgId);

        if (salesOrg == null)
            return string.Empty;

        return $" - {salesOrg.Name} ({salesOrg.Code})";
    }

    #endregion

    #region Methods

    public async Task<IActionResult> Index()
    {
        return RedirectToAction("List");
    }

    public async Task<IActionResult> List()
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.AccessAdminPanel))
            return AccessDeniedView();

        var model = new ErpAccountSearchModel();
        model = await _erpAccountModelFactory.PrepareErpAccountSearchModelAsync(searchModel: model);

        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> ErpAccountList(ErpAccountSearchModel erpAccountSearchModel)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.AccessAdminPanel))
            return await AccessDeniedDataTablesJson();

        var model = await _erpAccountModelFactory.PrepareErpAccountListModelAsync(
            erpAccountSearchModel
        );

        return Json(model);
    }

    public async Task<IActionResult> CreateErpAccount()
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.AccessAdminPanel))
            return AccessDeniedView();

        //prepare model
        var model = await _erpAccountModelFactory.PrepareErpAccountModelAsync(
            new ErpAccountModel(),
            null
        );

        return View(model);
    }

    [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
    [FormValueRequired("save", "save-continue")]
    public async Task<IActionResult> CreateErpAccount(
        ErpAccountModel model,
        bool continueEditing,
        IFormCollection form
    )
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.AccessAdminPanel))
            return AccessDeniedView();

        if (ModelState.IsValid)
        {
            var currentCustomer = await _workContext.GetCurrentCustomerAsync();

            //fill entity from model
            var erpAccount = model.ToEntity<ErpAccount>();

            erpAccount.CreatedOnUtc = DateTime.UtcNow;
            erpAccount.CreatedById = currentCustomer.Id;
            erpAccount.ErpAccountStatusTypeId = model.ErpAccountStatusTypeId;
            await _erpAccountService.InsertErpAccountAsync(erpAccount);

            //address
            var address = model.BillingAddress.ToEntity<Address>();
            address.CreatedOnUtc = DateTime.UtcNow;
            if (address.CountryId == 0)
                address.CountryId = _b2BB2CFeaturesSettings.DefaultCountryId;
            if (address.StateProvinceId == 0)
                address.StateProvinceId = null;
            await _addressService.InsertAddressAsync(address);

            erpAccount.BillingAddressId = address.Id;
            await _erpAccountService.UpdateErpAccountAsync(erpAccount);

            var successMsg = await _localizationService.GetResourceAsync(
                "Plugin.Misc.NopStation.ERPIntegrationCore.ErpAccount.Added"
            );
            _notificationService.SuccessNotification(successMsg);

            await _erpLogsService.InformationAsync(
                $"{successMsg}. Erp Account Id: {erpAccount.Id}",
                ErpSyncLevel.Account,
                customer: currentCustomer
            );

            if (!continueEditing)
                return RedirectToAction("List");

            return RedirectToAction("ErpAccountEdit", new { id = erpAccount.Id });
        }

        //prepare model
        model = await _erpAccountModelFactory.PrepareErpAccountModelAsync(model, null);

        //if we got this far, something failed, redisplay form
        return View(model);
    }

    public async Task<IActionResult> ErpAccountEdit(int id)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.AccessAdminPanel))
            return AccessDeniedView();

        //try to get a customer with the specified id
        var erpAccount = await _erpAccountService.GetErpAccountByIdAsync(id);
        if (erpAccount == null)
            return RedirectToAction("List");

        //prepare model
        var model = await _erpAccountModelFactory.PrepareErpAccountModelAsync(null, erpAccount);

        return View(model);
    }

    [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
    [FormValueRequired("save", "save-continue")]
    public async Task<IActionResult> ErpAccountEdit(
        ErpAccountModel model,
        bool continueEditing,
        IFormCollection form
    )
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.AccessAdminPanel))
            return AccessDeniedView();

        //try to get a erpAccount with the specified id
        var erpAccount = await _erpAccountService.GetErpAccountByIdAsync(model.Id);
        if (erpAccount == null)
            return RedirectToAction("List");

        if (ModelState.IsValid)
        {
            var currentCustomer = await _workContext.GetCurrentCustomerAsync();
            try
            {
                erpAccount.AccountName = model.AccountName;
                erpAccount.AccountNumber = model.AccountNumber;
                erpAccount.VatNumber = model.VatNumber;
                erpAccount.ErpSalesOrgId = model.ErpSalesOrgId;
                erpAccount.IsActive = model.IsActive;
                erpAccount.BillingSuburb = model.BillingSuburb;
                erpAccount.CreditLimit = model.CreditLimit;
                erpAccount.CreditLimitAvailable = model.CreditLimitAvailable;
                erpAccount.CurrentBalance = model.CurrentBalance;
                erpAccount.LastPaymentAmount = model.LastPaymentAmount;
                erpAccount.LastPaymentDate = model.LastPaymentDate;
                erpAccount.AllowOverspend = model.AllowOverspend;
                erpAccount.PreFilterFacets = model.PreFilterFacets;
                erpAccount.PaymentTypeCode = model.PaymentTypeCode;
                erpAccount.SpecialIncludes = model.SpecialIncludes;
                erpAccount.SpecialExcludes = model.SpecialExcludes;
                erpAccount.OverrideBackOrderingConfigSetting =
                    model.OverrideBackOrderingConfigSetting;
                erpAccount.AllowAccountsBackOrdering = model.AllowAccountsBackOrdering;
                erpAccount.OverrideAddressEditOnCheckoutConfigSetting =
                    model.OverrideAddressEditOnCheckoutConfigSetting;
                erpAccount.AllowAccountsAddressEditOnCheckout =
                    model.AllowAccountsAddressEditOnCheckout;
                erpAccount.OverrideStockDisplayFormatConfigSetting =
                    model.OverrideStockDisplayFormatConfigSetting;
                erpAccount.StockDisplayFormatTypeId = model.StockDisplayFormatTypeId;
                erpAccount.ErpAccountStatusTypeId = model.ErpAccountStatusTypeId;
                erpAccount.B2BPriceGroupCodeId = model.B2BPriceGroupCodeId;
                erpAccount.UpdatedOnUtc = DateTime.UtcNow;
                erpAccount.UpdatedById = currentCustomer.Id;
                erpAccount.PercentageOfStockAllowed = model.PercentageOfStockAllowed;

                await _erpAccountService.UpdateErpAccountAsync(erpAccount);

                //address
                var address = await _addressService.GetAddressByIdAsync(
                    erpAccount.BillingAddressId ?? 0
                );
                if (address == null)
                {
                    address = model.BillingAddress.ToEntity<Address>();
                    address.CreatedOnUtc = DateTime.UtcNow;
                    //some validation
                    if (address.CountryId == 0)
                        address.CountryId = _b2BB2CFeaturesSettings.DefaultCountryId;
                    if (address.StateProvinceId == 0)
                        address.StateProvinceId = null;
                    await _addressService.InsertAddressAsync(address);

                    erpAccount.BillingAddressId = address.Id;
                    await _erpAccountService.UpdateErpAccountAsync(erpAccount);
                }
                else
                {
                    address = model.BillingAddress.ToEntity(address);

                    //some validation
                    if (address.CountryId == 0)
                        address.CountryId = _b2BB2CFeaturesSettings.DefaultCountryId;
                    if (address.StateProvinceId == 0)
                        address.StateProvinceId = null;
                    await _addressService.UpdateAddressAsync(address);
                }

                var successMsg = await _localizationService.GetResourceAsync(
                    "Plugin.Misc.NopStation.ERPIntegrationCore.ErpAccount.Updated"
                );
                _notificationService.SuccessNotification(successMsg);

                await _erpLogsService.InformationAsync(
                    $"{successMsg}. Erp Account Id: {erpAccount.Id}",
                    ErpSyncLevel.Account,
                    customer: currentCustomer
                );

                if (!continueEditing)
                    return RedirectToAction("List");

                return RedirectToAction("ErpAccountEdit", new { id = erpAccount.Id });
            }
            catch (Exception exc)
            {
                _notificationService.ErrorNotification(exc.Message);
            }
        }

        //prepare model
        model = await _erpAccountModelFactory.PrepareErpAccountModelAsync(model, erpAccount);
        //if we got this far, something failed, redisplay form
        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> Delete(int id)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManagePlugins))
            return AccessDeniedView();

        //try to get a erpSalesOrg with the specified id
        var erpAccount = await _erpAccountService.GetErpAccountByIdAsync(id);
        if (erpAccount == null)
            return RedirectToAction("List");

        await _erpAccountService.DeleteErpAccountByIdAsync(erpAccount.Id);

        var successMsg = await _localizationService.GetResourceAsync(
            "Plugin.Misc.NopStation.ERPIntegrationCore.ErpAccount.Deleted"
        );
        _notificationService.SuccessNotification(successMsg);

        await _erpLogsService.InformationAsync(
            $"{successMsg}. Erp Account Id: {erpAccount.Id}",
            ErpSyncLevel.Account,
            customer: await _workContext.GetCurrentCustomerAsync()
        );

        return RedirectToAction("List");
    }

    public async Task<IActionResult> ErpAccountSearchAutoComplete(string term)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManagePlugins))
            return AccessDeniedView();

        const int searchTermMinimumLength = 3;
        if (string.IsNullOrWhiteSpace(term) || term.Length < searchTermMinimumLength)
            return Content(string.Empty);

        //b2b accounts
        var accounts = await _erpAccountService.GetAllErpAccountsAsync(
            erpAccountNo: term,
            pageSize: 15,
            showHidden: false
        );

        var result = await Task.WhenAll(
            accounts.Select(async acc => new
            {
                label = $"{acc.AccountNumber} ({acc.AccountName})"
                    + await GetErpSalesOrganisationNameAndCodeByIdAsync(acc.ErpSalesOrgId),
                erpaccountid = acc.Id,
            })
        );

        return Json(await result.ToListAsync());
    }

    public async Task<IActionResult> GetDefaultB2CErpAccountInfo(int erpAccountId)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManagePlugins))
            return AccessDeniedView();

        var erpAccount = await _erpAccountService.GetErpAccountByIdAsync(erpAccountId);

        var erpAccountDetails = $"{erpAccount.AccountNumber} ({erpAccount.AccountName})";

        return Json(erpAccountDetails);
    }

    #endregion

    #region ErpAccount List of Nop User

    [HttpPost]
    public async Task<IActionResult> ErpAccountNopUsersList(ErpNopUserSearchModel searchModel)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.AccessAdminPanel))
            return await AccessDeniedDataTablesJson();

        var model = await _erpNopUserModelFactory.PrepareErpNopUserListModelAsync(searchModel);

        return Json(model);
    }

    #endregion

    #region ErpAccount List of Erp ShipToAddresses

    [HttpPost]
    public async Task<IActionResult> ErpAccountShipToAddressesList(
        ErpShipToAddressSearchModel searchModel
    )
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.AccessAdminPanel))
            return await AccessDeniedDataTablesJson();

        var model = await _erpShipToAddressModelFactory.PrepareErpShipToAddressListModelAsync(
            searchModel
        );

        return Json(model);
    }

    #endregion

    #region exprot/excel

    [HttpPost, ActionName("List")]
    [FormValueRequired("exportexcel-all")]
    public virtual async Task<IActionResult> ExportExcelAll(ErpAccountSearchModel searchModel)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.AccessAdminPanel))
            return AccessDeniedView();


        try
        {
            var bytes =
               await _erpAccountModelFactory.ExportAllErpAccountsToXlsxAsync(
                   searchModel
               );
            return File(bytes, MimeTypes.TextXlsx, "ErpAccouts.xlsx");
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

            var bytes = await _erpAccountModelFactory.ExportSelectedErpAccountsToXlsxAsync(selectedIds);
            return File(bytes, MimeTypes.TextXlsx, "ErpAccounts_Selected.xlsx");
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
                await _erpAccountModelFactory.ImportErpAccountsFromXlsxAsync(importexcelfile.OpenReadStream());
            }
            else
            {
                _notificationService.ErrorNotification(await _localizationService.GetResourceAsync("Admin.Common.UploadFile"));
                return RedirectToAction("List");
            }

            _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("NopStation.Plugin.B2B.B2BB2CFeatures.ErpAccount.Imported"));
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