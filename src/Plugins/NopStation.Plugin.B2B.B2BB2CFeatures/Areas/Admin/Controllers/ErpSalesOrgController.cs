using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Core.Domain.Common;
using Nop.Services.Common;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Messages;
using Nop.Services.Security;
using Nop.Web.Areas.Admin.Infrastructure.Mapper.Extensions;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Mvc;
using Nop.Web.Framework.Mvc.Filters;
using Nop.Web.Framework.Mvc.ModelBinding;
using Nop.Web.Framework.Validators;
using NopStation.Plugin.B2B.B2BB2CFeatures.Areas.Admin.Factories;
using NopStation.Plugin.B2B.B2BB2CFeatures.Areas.Admin.Models;
using NopStation.Plugin.B2B.B2BB2CFeatures.Services.ErpCustomerFunctionality;
using NopStation.Plugin.B2B.ERPIntegrationCore.Domain;
using NopStation.Plugin.B2B.ERPIntegrationCore.Enums;
using NopStation.Plugin.B2B.ERPIntegrationCore.Services;
using NopStation.Plugin.Misc.Core.Controllers;

namespace NopStation.Plugin.B2B.B2BB2CFeatures.Areas.Admin.Controllers;

public class ErpSalesOrgController : NopStationAdminController
{
    #region Fields

    private readonly ILocalizationService _localizationService;
    private readonly INotificationService _notificationService;
    private readonly IPermissionService _permissionService;
    private readonly IErpSalesOrgModelFactory _erpSalesOrgModelFactory;
    private readonly IErpSalesOrgService _erpSalesOrgService;
    private readonly IAddressService _addressService;
    private readonly IWorkContext _workContext;
    private readonly IErpLogsService _erpLogsService;
    private readonly IErpCustomerFunctionalityService _erpCustomerFunctionalityService;
    private readonly IErpWarehouseSalesOrgMapService _erpWarehouseSalesOrgMapService;
    private readonly B2BB2CFeaturesSettings _b2BB2CFeaturesSettings;
    private readonly IErpAccountService _erpAccountService;
    private readonly ICustomerActivityService _customerActivityService;

    public const string CREATE = "Create";
    public const string UPDATE = "Update";
    public const string DELETE = "Delete";

    #endregion

    #region Ctor

    public ErpSalesOrgController(
        ILocalizationService localizationService,
        INotificationService notificationService,
        IPermissionService permissionService,
        IErpSalesOrgModelFactory erpSalesOrgModelFactory,
        IErpSalesOrgService erpSalesOrgService,
        IAddressService addressService,
        IWorkContext workContext,
        IErpLogsService erpLogsService,
        IErpCustomerFunctionalityService erpCustomerFunctionalityService,
        IErpWarehouseSalesOrgMapService erpWarehouseSalesOrgMapService,
        B2BB2CFeaturesSettings b2BB2CFeaturesSettings,
        IErpAccountService erpAccountService,
        ICustomerActivityService customerActivityService
    )
    {
        _localizationService = localizationService;
        _notificationService = notificationService;
        _permissionService = permissionService;
        _erpSalesOrgModelFactory = erpSalesOrgModelFactory;
        _erpSalesOrgService = erpSalesOrgService;
        _addressService = addressService;
        _workContext = workContext;
        _erpLogsService = erpLogsService;
        _erpCustomerFunctionalityService = erpCustomerFunctionalityService;
        _erpWarehouseSalesOrgMapService = erpWarehouseSalesOrgMapService;
        _b2BB2CFeaturesSettings = b2BB2CFeaturesSettings;
        _erpAccountService = erpAccountService;
        _customerActivityService = customerActivityService;
    }

    #endregion

    #region Utilities

    private string GetKeyword(bool isB2CWarehouse, string activityType)
    {
        return (isB2CWarehouse, activityType) switch
        {
            (true, CREATE) => B2BB2CFeaturesDefaults.B2CSalesOrgWarehouseCreate,
            (true, UPDATE) => B2BB2CFeaturesDefaults.B2CSalesOrgWarehouseUpdate,
            (true, DELETE) => B2BB2CFeaturesDefaults.B2CSalesOrgWarehouseDelete,
            (false, CREATE) => B2BB2CFeaturesDefaults.B2BSalesOrgWarehouseCreate,
            (false, UPDATE) => B2BB2CFeaturesDefaults.B2BSalesOrgWarehouseUpdate,
            (false, DELETE) => B2BB2CFeaturesDefaults.B2BSalesOrgWarehouseDelete,
            _ => isB2CWarehouse
                ? B2BB2CFeaturesDefaults.B2CSalesOrgWarehouseUpdate
                : B2BB2CFeaturesDefaults.B2BSalesOrgWarehouseUpdate
        };
    }
    private string GetResourceKey(bool isB2CWarehouse, string activityType)
    {
        return isB2CWarehouse
            ? $"Plugin.Misc.NopStation.B2BB2CFeatures.ActivityLog.B2CSalesOrgWarehouse.{activityType}"
            : $"Plugin.Misc.NopStation.B2BB2CFeatures.ActivityLog.B2BSalesOrgWarehouse.{activityType}";
    }


    private async Task InsertCustomerActivityAsync(
        bool isB2CWarehouse,
        int erpSalesOrgId,
        int nopWarehouseId,
        string activityType)
    {

        var keyword = GetKeyword(isB2CWarehouse, activityType);
        var resourceKey = GetResourceKey(isB2CWarehouse, activityType);
        var message = await _localizationService.GetResourceAsync(resourceKey);
        var comment = string.Format(message, erpSalesOrgId, nopWarehouseId);

        await _customerActivityService.InsertActivityAsync(keyword, comment);
    }

    #endregion

    #region Methods

    #region ErpSalesOrg

    public async Task<IActionResult> Index()
    {
        return RedirectToAction("List");
    }

    public async Task<IActionResult> List()
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.AccessAdminPanel))
            return AccessDeniedView();

        var model = new ErpSalesOrgSearchModel();
        model = await _erpSalesOrgModelFactory.PrepareErpSalesOrgSearchModelAsync(
            searchModel: model
        );

        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> ErpSalesOrgList(ErpSalesOrgSearchModel erpSalesOrgSearchModel)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.AccessAdminPanel))
            return await AccessDeniedDataTablesJson();

        var model = await _erpSalesOrgModelFactory.PrepareErpSalesOrgListModelAsync(
            erpSalesOrgSearchModel
        );

        return Json(model);
    }

    public async Task<IActionResult> CreateErpSalesOrg()
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.AccessAdminPanel))
            return AccessDeniedView();

        //prepare model
        var model = await _erpSalesOrgModelFactory.PrepareErpSalesOrgModelAsync(
            new ErpSalesOrgModel(),
            null
        );

        return View(model);
    }

    [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
    [FormValueRequired("save", "save-continue")]
    public async Task<IActionResult> CreateErpSalesOrg(
        ErpSalesOrgModel model,
        bool continueEditing,
        IFormCollection form
    )
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.AccessAdminPanel))
            return AccessDeniedView();

        var customerCustomer = await _workContext.GetCurrentCustomerAsync();

        // Ensure that valid email address is entered if Registered role is checked to avoid registered customers with empty email address
        if (!CommonHelper.IsValidEmail(model.Email))
        {
            var errMsg = await _localizationService.GetResourceAsync(
                "Admin.Customers.Customers.ValidEmailRequiredRegisteredRole"
            );

            ModelState.AddModelError(string.Empty, errMsg);
            _notificationService.ErrorNotification(errMsg);
            await _erpLogsService.ErrorAsync(
                errMsg,
                ErpSyncLevel.SalesOrg,
                customer: customerCustomer
            );
        }

        if (!(await _erpAccountService.ErpAccountExistsById(model.ErpAccountIdForB2C)))
            ModelState.AddModelError(
                "ErpAccountIdForB2C ",
                await _localizationService.GetResourceAsync(
                    "Plugin.Misc.NopStation.B2BB2CFeatures.ErpSalesOrganisations.ErpAccountIdForB2C.DoesNotExist"
                )
            );

        if (ModelState.IsValid)
        {
            //fill entity from model
            var erpSalesOrg = model.ToEntity<ErpSalesOrg>();
            erpSalesOrg.CreatedOnUtc = DateTime.UtcNow;
            erpSalesOrg.CreatedById = customerCustomer.Id;

            await _erpSalesOrgService.InsertErpSalesOrgAsync(erpSalesOrg);

            //address
            var address = model.Address.ToEntity<Address>();
            address.CreatedOnUtc = DateTime.UtcNow;

            //some validation
            if (address.CountryId == 0)
                address.CountryId = _b2BB2CFeaturesSettings.DefaultCountryId;
            if (address.StateProvinceId == 0)
                address.StateProvinceId = null;
            await _addressService.InsertAddressAsync(address);
            erpSalesOrg.AddressId = address.Id;

            await _erpSalesOrgService.UpdateErpSalesOrgAsync(erpSalesOrg);

            // update erpAccount with new SalesOrgId
            var erpAccount = await _erpAccountService.GetErpAccountByIdAsync(
                model.ErpAccountIdForB2C
            );
            erpAccount.ErpSalesOrgId = erpSalesOrg.Id;
            await _erpAccountService.UpdateErpAccountAsync(erpAccount);
            await _customerActivityService.InsertActivityAsync(
                B2BB2CFeaturesDefaults.B2BB2CFeatureErpAccountUpdate,
                $"Erp Account {erpAccount.AccountNumber} ({erpAccount.AccountName}) updated after Sales Org update/create.",
                erpAccount
            );

            var successMsg = await _localizationService.GetResourceAsync(
                "Plugin.Misc.NopStation.ERPIntegrationCore.ErpSalesOrg.Added"
            );
            _notificationService.SuccessNotification(successMsg);

            await _erpLogsService.InformationAsync(
                $"{successMsg}. Erp Sales Org Id: {erpSalesOrg.Id}",
                ErpSyncLevel.SalesOrg,
                customer: customerCustomer
            );

            if (!continueEditing)
                return RedirectToAction("List");

            return RedirectToAction("ErpSalesOrgEdit", new { id = erpSalesOrg.Id });
        }

        //prepare model
        model = await _erpSalesOrgModelFactory.PrepareErpSalesOrgModelAsync(model, null);

        //if we got this far, something failed, redisplay form
        return View(model);
    }

    public async Task<IActionResult> ErpSalesOrgEdit(int id)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.AccessAdminPanel))
            return AccessDeniedView();

        //try to get a customer with the specified id
        var erpSalesOrg = await _erpSalesOrgService.GetErpSalesOrgByIdAsync(id);
        if (erpSalesOrg == null)
            return RedirectToAction("List");

        //prepare model
        var model = await _erpSalesOrgModelFactory.PrepareErpSalesOrgModelAsync(null, erpSalesOrg);

        return View(model);
    }

    [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
    [FormValueRequired("save", "save-continue")]
    public async Task<IActionResult> ErpSalesOrgEdit(
        ErpSalesOrgModel model,
        bool continueEditing,
        IFormCollection form
    )
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.AccessAdminPanel))
            return AccessDeniedView();

        //try to get a erpSalesOrg with the specified id
        var erpSalesOrg = await _erpSalesOrgService.GetErpSalesOrgByIdAsync(model.Id);
        if (erpSalesOrg == null)
            return RedirectToAction("List");

        var customerCustomer = await _workContext.GetCurrentCustomerAsync();

        // Ensure that valid email address is entered if Registered role is checked to avoid registered customers with empty email address
        if (!CommonHelper.IsValidEmail(model.Email))
        {
            var errMsg = await _localizationService.GetResourceAsync(
                "Admin.Customers.Customers.ValidEmailRequiredRegisteredRole"
            );

            ModelState.AddModelError(string.Empty, errMsg);
            _notificationService.ErrorNotification(errMsg);
            await _erpLogsService.ErrorAsync(
                errMsg,
                ErpSyncLevel.SalesOrg,
                customer: customerCustomer
            );
        }

        if (!await _erpAccountService.ErpAccountExistsById(model.ErpAccountIdForB2C))
            ModelState.AddModelError(
                "ErpAccountIdForB2C ",
                await _localizationService.GetResourceAsync(
                    "Plugin.Misc.NopStation.B2BB2CFeatures.ErpSalesOrganisations.ErpAccountIdForB2C.DoesNotExist"
                )
            );

        if (ModelState.IsValid)
        {
            try
            {
                erpSalesOrg.Email = model.Email;
                erpSalesOrg.Code = model.Code;
                erpSalesOrg.IntegrationClientId = model.IntegrationClientId;
                erpSalesOrg.Name = model.Name;
                erpSalesOrg.IsActive = model.IsActive;
                erpSalesOrg.AuthenticationKey = model.AuthenticationKey;
                erpSalesOrg.ErpAccountIdForB2C = model.ErpAccountIdForB2C;
                erpSalesOrg.UpdatedOnUtc = DateTime.UtcNow;
                erpSalesOrg.UpdatedById = customerCustomer.Id;
                erpSalesOrg.SpecialsCategoryId = model.SpecialsCategoryId;
                erpSalesOrg.NoItemsMessage = model.NoItemsMessage;
                await _erpSalesOrgService.UpdateErpSalesOrgAsync(erpSalesOrg);

                //address
                var address = await _addressService.GetAddressByIdAsync(erpSalesOrg.AddressId);
                if (address == null)
                {
                    address = model.Address.ToEntity<Address>();
                    address.CreatedOnUtc = DateTime.UtcNow;

                    //some validation
                    if (address.CountryId == 0)
                        address.CountryId = _b2BB2CFeaturesSettings.DefaultCountryId;
                    if (address.StateProvinceId == 0)
                        address.StateProvinceId = null;

                    await _addressService.InsertAddressAsync(address);

                    erpSalesOrg.AddressId = address.Id;
                    await _erpSalesOrgService.UpdateErpSalesOrgAsync(erpSalesOrg);
                }
                else
                {
                    address = model.Address.ToEntity(address);

                    //some validation
                    if (address.CountryId == 0)
                        address.CountryId = _b2BB2CFeaturesSettings.DefaultCountryId;
                    if (address.StateProvinceId == 0)
                        address.StateProvinceId = null;

                    await _addressService.UpdateAddressAsync(address);
                }

                // update erpAccount with new SalesOrgId
                var erpAccount = await _erpAccountService.GetErpAccountByIdAsync(
                    model.ErpAccountIdForB2C
                );
                erpAccount.ErpSalesOrgId = erpSalesOrg.Id;
                await _erpAccountService.UpdateErpAccountAsync(erpAccount);
                await _customerActivityService.InsertActivityAsync(
                    B2BB2CFeaturesDefaults.B2BB2CFeatureErpAccountUpdate,
                    $"Erp Account {erpAccount.AccountNumber} ({erpAccount.AccountName}) updated after Sales Org update/create.",
                    erpAccount
                );

                var successMsg = await _localizationService.GetResourceAsync(
                    "Plugin.Misc.NopStation.ERPIntegrationCore.ErpSalesOrg.Updated"
                );
                _notificationService.SuccessNotification(successMsg);

                await _erpLogsService.InformationAsync(
                    $"{successMsg}. Erp Sales Org Id: {erpSalesOrg.Id}",
                    ErpSyncLevel.SalesOrg,
                    customer: customerCustomer
                );

                if (!continueEditing)
                    return RedirectToAction("List");

                return RedirectToAction("ErpSalesOrgEdit", new { id = erpSalesOrg.Id });
            }
            catch (Exception exc)
            {
                _notificationService.ErrorNotification(exc.Message);
                await _erpLogsService.ErrorAsync(
                    $"{exc.Message}. Sales Org Id: {erpSalesOrg.Id}",
                    ErpSyncLevel.SalesOrg,
                    exc,
                    customer: customerCustomer
                );
            }
        }

        //prepare model
        model = await _erpSalesOrgModelFactory.PrepareErpSalesOrgModelAsync(model, erpSalesOrg);

        //if we got this far, something failed, redisplay form
        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> Delete(int id)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManagePlugins))
            return AccessDeniedView();

        //try to get a erpSalesOrg with the specified id
        var erpSalesOrg = await _erpSalesOrgService.GetErpSalesOrgByIdAsync(id);
        if (erpSalesOrg == null)
            return RedirectToAction("List");

        //delete a erpSalesOrg
        await _erpSalesOrgService.DeleteErpSalesOrgByIdAsync(erpSalesOrg.Id);

        var successMsg = await _localizationService.GetResourceAsync(
            "Plugin.Misc.NopStation.ERPIntegrationCore.ErpSalesOrg.Deleted"
        );
        _notificationService.SuccessNotification(successMsg);

        await _erpLogsService.InformationAsync(
            $"{successMsg}. Erp Sales Org Id: {erpSalesOrg.Id}",
            ErpSyncLevel.SalesOrg,
            customer: await _workContext.GetCurrentCustomerAsync()
        );

        return Json(new { response = true });
    }

    public async Task<IActionResult> IsMappedWithAnyERPAccount(int erpSalesOrgId)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManagePlugins))
            return AccessDeniedView();

        var isMapped = await _erpSalesOrgService.IsMappedWithAnyERPAccountAsync(erpSalesOrgId);
        return Json(new { isMapped });
    }

    public virtual async Task<IActionResult> ErpSalesOrganisationSearchAutoComplete(string term)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManagePlugins))
            return AccessDeniedView();

        const int searchTermMinimumLength = 3;
        if (string.IsNullOrWhiteSpace(term) || term.Length < searchTermMinimumLength)
            return Content(string.Empty);

        var salesOrganisations = await _erpSalesOrgService.GetAllErpSalesOrgAsync(
            code: term,
            pageSize: 15
        );

        var result = (
            from s in salesOrganisations
            select new { label = s.Code + "(" + s.Name + ")", b2bsalesorgid = s.Id }
        ).ToList();
        return Json(result);
    }

    #endregion

    #region Warehouse

    [HttpPost]
    public virtual async Task<IActionResult> ErpSalesOrgWareHouseList(
        ErpSalesOrgWarehouseSearchModel searchModel
    )
    {
        if (!await _erpCustomerFunctionalityService.IsCurrentCustomerInAdministratorRoleAsync())
            return AccessDeniedView();

        if (!_b2BB2CFeaturesSettings.EnableWarehouse)
        {
            return Json(new ErpSalesOrgWarehouseListModel());
        }

        var model = await _erpSalesOrgModelFactory.PrepareErpSalesOrgWarehouseListModel(
            searchModel
        );
        return Json(model);
    }

    [HttpPost]
    public virtual async Task<IActionResult> CreateErpSalesOrgWareHouse(
        int salesOrgId,
        [Validate] ErpSalesOrgWarehouseModel model
    )
    {
        if (!await _erpCustomerFunctionalityService.IsCurrentCustomerInAdministratorRoleAsync())
            return AccessDeniedView();

        if (!_b2BB2CFeaturesSettings.EnableWarehouse)
        {
            return Json(new { Result = false });
        }

        if (salesOrgId == 0)
        {
            return RedirectToAction("List");
        }

        if (model.WarehouseId > 0 && string.IsNullOrEmpty(model.ErpWarehouseCode))
            return ErrorJson(
                await _localizationService.GetResourceAsync(
                    "Plugin.Misc.NopStation.B2BB2CFeatures.ErpSalesOrgWarehouse.ErpWarehouseCode.Required"
                )
            );

        if (
            await _erpWarehouseSalesOrgMapService.CheckAnyErpSalesOrgWarehouseExistBySalesOrgIdAndNopWarehouseId(
                salesOrgId,
                model.WarehouseId,
                false
            )
        )
            return ErrorJson(
                await _localizationService.GetResourceAsync(
                    "Plugin.Misc.NopStation.B2BB2CFeatures.ErpSalesOrgWarehouse.AlreadyExist"
                )
            );

        if (ModelState.IsValid)
        {
            try
            {
                var salesOrgWarehouseMap = new ErpWarehouseSalesOrgMap
                {
                    NopWarehouseId = model.WarehouseId,
                    ErpSalesOrgId = salesOrgId,
                    WarehouseCode = model.ErpWarehouseCode,
                    IsB2CWarehouse = model.IsB2CWarehouse,
                };

                await _erpWarehouseSalesOrgMapService.InsertErpWarehouseSalesOrgMapAsync(
                    salesOrgWarehouseMap
                );

                await InsertCustomerActivityAsync(
                    model.IsB2CWarehouse,
                    salesOrgWarehouseMap.ErpSalesOrgId,
                    salesOrgWarehouseMap.NopWarehouseId,
                    CREATE
                );
            }
            catch (Exception ex)
            {
                return ErrorJson(ex.Message);
            }
        }
        else
        {
            return ErrorJson(ModelState.SerializeErrors());
        }

        return Json(new { Result = true });
    }

    [HttpPost]
    public virtual async Task<IActionResult> EditErpSalesOrgWareHouse(
        int salesOrgId,
        ErpSalesOrgWarehouseModel model
    )
    {
        if (!await _erpCustomerFunctionalityService.IsCurrentCustomerInAdministratorRoleAsync())
            return AccessDeniedView();

        if (!_b2BB2CFeaturesSettings.EnableWarehouse)
        {
            return new NullJsonResult();
        }

        var erpSalesOrgWarehouseMap =
            await _erpWarehouseSalesOrgMapService.GetErpWarehouseSalesOrgMapByIdAsync(model.Id);

        if (erpSalesOrgWarehouseMap != null)
        {
            if (!ModelState.IsValid)
            {
                return ErrorJson(ModelState.SerializeErrors());
            }

            erpSalesOrgWarehouseMap.WarehouseCode = model.ErpWarehouseCode;

            await _erpWarehouseSalesOrgMapService.UpdateErpWarehouseSalesOrgMapAsync(
                erpSalesOrgWarehouseMap
            );

            await InsertCustomerActivityAsync(
                model.IsB2CWarehouse,
                erpSalesOrgWarehouseMap.ErpSalesOrgId,
                erpSalesOrgWarehouseMap.NopWarehouseId,
                UPDATE
            );

        }

        return new NullJsonResult();
    }

    [HttpPost]
    public virtual async Task<IActionResult> DeleteErpSalesOrgWareHouse(int id)
    {
        if (!await _erpCustomerFunctionalityService.IsCurrentCustomerInAdministratorRoleAsync())
            return AccessDeniedView();

        if (!_b2BB2CFeaturesSettings.EnableWarehouse)
        {
            return new NullJsonResult();
        }

        var erpWarehouseMap =
            await _erpWarehouseSalesOrgMapService.GetErpWarehouseSalesOrgMapByIdAsync(id)
            ?? throw new ArgumentException(
                "No ERP Sales Org Warehouse found with the specified id",
                nameof(id)
            );

        await _erpWarehouseSalesOrgMapService.DeleteErpWarehouseSalesOrgMapByIdAsync(
            erpWarehouseMap.Id
        );

        await InsertCustomerActivityAsync(
            erpWarehouseMap.IsB2CWarehouse,
            erpWarehouseMap.ErpSalesOrgId,
            erpWarehouseMap.NopWarehouseId,
            DELETE
        );

        return new NullJsonResult();
    }

    #region B2CSalesOrgWarehouse

    [HttpPost]
    public virtual async Task<IActionResult> B2CSalesOrgWareHouseList(
        ErpSalesOrgWarehouseSearchModel searchModel
    )
    {
        if (!await _erpCustomerFunctionalityService.IsCurrentCustomerInAdministratorRoleAsync())
            return AccessDeniedView();

        if (!_b2BB2CFeaturesSettings.EnableWarehouse)
        {
            return Json(new ErpSalesOrgWarehouseListModel());
        }

        var model = await _erpSalesOrgModelFactory.PrepareB2CSalesOrgWarehouseListModel(
            searchModel
        );
        return Json(model);
    }

    [HttpPost]
    public virtual async Task<IActionResult> CreateB2CSalesOrgWareHouse(
        int salesOrgId,
        [Validate] B2CSalesOrgWarehouseModel model
    )
    {
        if (!await _erpCustomerFunctionalityService.IsCurrentCustomerInAdministratorRoleAsync())
            return AccessDeniedView();

        if (!_b2BB2CFeaturesSettings.EnableWarehouse)
        {
            return Json(new { Result = false });
        }

        if (salesOrgId == 0)
        {
            return RedirectToAction("List");
        }

        if (
            await _erpWarehouseSalesOrgMapService.CheckAnyErpSalesOrgWarehouseExistBySalesOrgIdAndNopWarehouseId(
                salesOrgId: salesOrgId,
                nopWarehouseId: model.WarehouseId,
                isB2cWarehouse: true
            )
        )
            return ErrorJson(
                await _localizationService.GetResourceAsync(
                    "Plugin.Misc.NopStation.B2BB2CFeatures.ErpSalesOrgWarehouse.AlreadyExist"
                )
            );

        if (model.WarehouseId > 0 && string.IsNullOrEmpty(model.B2CWarehouseCode))
        {
            return ErrorJson(
                await _localizationService.GetResourceAsync(
                    "Plugin.Misc.NopStation.B2BB2CFeatures.ErpSalesOrgWarehouse.ErpWarehouseCode.Required"
                )
            );
        }

        if (ModelState.IsValid)
        {
            try
            {
                var salesOrgWarehouseMap = new ErpWarehouseSalesOrgMap
                {
                    NopWarehouseId = model.WarehouseId,
                    ErpSalesOrgId = salesOrgId,
                    WarehouseCode = model.B2CWarehouseCode,
                    IsB2CWarehouse = model.IsB2CWarehouse,
                };

                await _erpWarehouseSalesOrgMapService.InsertErpWarehouseSalesOrgMapAsync(
                    salesOrgWarehouseMap
                );

                await InsertCustomerActivityAsync(
                    model.IsB2CWarehouse,
                    salesOrgWarehouseMap.ErpSalesOrgId,
                    salesOrgWarehouseMap.NopWarehouseId,
                    CREATE
                );
            }
            catch (Exception ex)
            {
                return ErrorJson(ex.Message);
            }
        }
        else
        {
            return ErrorJson(ModelState.SerializeErrors());
        }

        return Json(new { Result = true });
    }

    [HttpPost]
    public virtual async Task<IActionResult> EditB2CSalesOrgWareHouse(
        int salesOrgId,
        B2CSalesOrgWarehouseModel model
    )
    {
        if (!await _erpCustomerFunctionalityService.IsCurrentCustomerInAdministratorRoleAsync())
            return AccessDeniedView();

        if (!_b2BB2CFeaturesSettings.EnableWarehouse)
        {
            return new NullJsonResult();
        }

        var erpSalesOrgWarehouseMap =
            await _erpWarehouseSalesOrgMapService.GetErpWarehouseSalesOrgMapByIdAsync(model.Id);

        if (erpSalesOrgWarehouseMap != null)
        {
            if (!ModelState.IsValid)
            {
                return ErrorJson(ModelState.SerializeErrors());
            }

            erpSalesOrgWarehouseMap.WarehouseCode = model.B2CWarehouseCode;

            await _erpWarehouseSalesOrgMapService.UpdateErpWarehouseSalesOrgMapAsync(
                erpSalesOrgWarehouseMap
            );

            await InsertCustomerActivityAsync(
                model.IsB2CWarehouse,
                erpSalesOrgWarehouseMap.ErpSalesOrgId,
                erpSalesOrgWarehouseMap.NopWarehouseId,
                UPDATE
            );

        }

        return new NullJsonResult();
    }

    [HttpPost]
    public virtual async Task<IActionResult> DeleteB2CSalesOrgWareHouse(int id)
    {
        if (!await _erpCustomerFunctionalityService.IsCurrentCustomerInAdministratorRoleAsync())
            return AccessDeniedView();

        if (!_b2BB2CFeaturesSettings.EnableWarehouse)
        {
            return new NullJsonResult();
        }

        var erpWarehouseMap = await _erpWarehouseSalesOrgMapService.GetErpWarehouseSalesOrgMapByIdAsync(id)
            ?? throw new ArgumentException("No B2C Sales Org Warehouse found with the specified id", nameof(id));

        var salesOrgsWithTradingWarehouse = await _erpSalesOrgService.GetAllErpSalesOrgByTradingWarehouseId(
            tradingWarehouseId: erpWarehouseMap.NopWarehouseId);

        if (salesOrgsWithTradingWarehouse.Any())
        {
            foreach (var salesOrg in salesOrgsWithTradingWarehouse)
            {
                salesOrg.TradingWarehouseId = null;
            }

            await _erpSalesOrgService.UpdateErpSalesOrgsAsync(salesOrgsWithTradingWarehouse.ToList());
        }

        await _erpWarehouseSalesOrgMapService.DeleteErpWarehouseSalesOrgMapByIdAsync(erpWarehouseMap.Id);

        await InsertCustomerActivityAsync(
            isB2CWarehouse: true,
            erpWarehouseMap.ErpSalesOrgId,
            erpWarehouseMap.NopWarehouseId,
            DELETE
        );

        return new NullJsonResult();
    }

    [HttpPost]
    public virtual async Task<IActionResult> MarkAsTradingWarehouse(int id)
    {
        if (!await _erpCustomerFunctionalityService.IsCurrentCustomerInAdministratorRoleAsync())
            return AccessDeniedView();

        if (!_b2BB2CFeaturesSettings.EnableWarehouse)
        {
            return Json(new ErpSalesOrgWarehouseListModel());
        }

        var erpWarehouseMap = await _erpWarehouseSalesOrgMapService.GetErpWarehouseSalesOrgMapByIdAsync(id);

        if (erpWarehouseMap == null || !erpWarehouseMap.IsB2CWarehouse)
        {
            throw new ArgumentException(
                "No B2C Sales Org Warehouse found with the specified id or it is not a B2C warehouse",
                nameof(id)
            );
        }

        var erpSalesOrg = await _erpSalesOrgService.GetErpSalesOrgByIdAsync(
            erpWarehouseMap.ErpSalesOrgId
        );

        if (erpSalesOrg == null || !erpWarehouseMap.IsB2CWarehouse)
        {
            var message = string.Format(
                await _localizationService.GetResourceAsync("Plugin.Misc.NopStation.B2BB2CFeatures.MarkAsTradingWarehouse.B2CSalesOrgWarehouse.Error"),
                id
            );

            await _erpLogsService.ErrorAsync(
                $"{message}. Erp Sales Org Id: {id}",
                ErpSyncLevel.SalesOrg
            );
            return Json(new { result = false });
        }

        try
        {
            erpSalesOrg.TradingWarehouseId = erpWarehouseMap.NopWarehouseId;
            await _erpSalesOrgService.UpdateErpSalesOrgAsync(erpSalesOrg);

            return Json(new { result = true });
        }
        catch (Exception ex)
        {

            var message = string.Format(
                await _localizationService.GetResourceAsync("Plugin.Misc.NopStation.B2BB2CFeatures.MarkAsTradingWarehouse.B2CSalesOrgWarehouse.Error"),
                id
            );

            await _erpLogsService.ErrorAsync(
                $"{message}. Erp Sales Org Id: {id}",
                ErpSyncLevel.SalesOrg,
                ex
            );

            return Json(new { result = false });
        }
    }

    #endregion

    #endregion

    #endregion
}
