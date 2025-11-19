using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Messages;
using Nop.Web.Areas.Admin.Infrastructure.Mapper.Extensions;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Mvc.Filters;
using NopStation.Plugin.B2B.B2BB2CFeatures.Areas.Admin.Factories;
using NopStation.Plugin.B2B.B2BB2CFeatures.Areas.Admin.Models.B2CMacsteelExpressShops;
using NopStation.Plugin.B2B.B2BB2CFeatures.Services.ErpCustomerFunctionality;
using NopStation.Plugin.B2B.ERPIntegrationCore.Domain;
using NopStation.Plugin.B2B.ERPIntegrationCore.Services;
using NopStation.Plugin.Misc.Core.Controllers;

namespace NopStation.Plugin.B2B.B2BB2CFeatures.Areas.Admin.Controllers;
public class B2CMacsteelExpressShopController : NopStationAdminController
{
    #region Fields

    private readonly IB2CMacsteelExpressShopFactory _b2CMacsteelExpressShopFactory;
    private readonly IWorkContext _workContext;
    private readonly INotificationService _notificationService;
    private readonly ILocalizationService _localizationService;
    private readonly ICustomerActivityService _customerActivityService;
    private readonly IB2CMacsteelExpressShopService _b2CMacsteelExpressShopService;
    private readonly IErpCustomerFunctionalityService _erpCustomerFunctionalityService;

    #endregion

    #region Ctor

    public B2CMacsteelExpressShopController(
        IB2CMacsteelExpressShopFactory b2CMacsteelExpressShopFactory,
        IWorkContext workContext,
        INotificationService notificationService,
        ILocalizationService localizationService,
        ICustomerActivityService customerActivityService,
        IB2CMacsteelExpressShopService b2CMacsteelExpressShopService,
        IErpCustomerFunctionalityService erpCustomerFunctionalityService)
    {
        _b2CMacsteelExpressShopFactory = b2CMacsteelExpressShopFactory;
        _workContext = workContext;
        _notificationService = notificationService;
        _localizationService = localizationService;
        _customerActivityService = customerActivityService;
        _b2CMacsteelExpressShopService = b2CMacsteelExpressShopService;
        _erpCustomerFunctionalityService = erpCustomerFunctionalityService;
    }

    #endregion

    #region Methods

    public virtual async Task<IActionResult> List()
    {
        if (!await _erpCustomerFunctionalityService.IsCurrentCustomerInAdministratorRoleAsync())
            return AccessDeniedView();

        var model = await _b2CMacsteelExpressShopFactory.PrepareB2CMacsteelExpressShopSearchModelAsync(new B2CMacsteelExpressShopSearchModel());

        return View(model);
    }

    [HttpPost]
    public virtual async Task<IActionResult> MacsteelExpressShopList(B2CMacsteelExpressShopSearchModel searchModel)
    {
        if (!await _erpCustomerFunctionalityService.IsCurrentCustomerInAdministratorRoleAsync())
            return await AccessDeniedDataTablesJson();

        var model = await _b2CMacsteelExpressShopFactory.PrepareB2CMacsteelExpressShopListModelAsync(searchModel);

        return Json(model);
    }

    public virtual async Task<IActionResult> Create()
    {
        if (!await _erpCustomerFunctionalityService.IsCurrentCustomerInAdministratorRoleAsync())
            return AccessDeniedView();

        var model = await _b2CMacsteelExpressShopFactory.PrepareB2CMacsteelExpressShopModelAsync(new B2CMacsteelExpressShopModel(), null);

        return View(model);
    }

    [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
    [FormValueRequired("save", "save-continue")]
    public virtual async Task<IActionResult> Create(B2CMacsteelExpressShopModel model, bool continueEditing, IFormCollection form)
    {
        if (!await _erpCustomerFunctionalityService.IsCurrentCustomerInAdministratorRoleAsync())
            return AccessDeniedView();

        if (await _b2CMacsteelExpressShopService.CheckAnyB2CMacsteelExpressShopByCodeAsync(model.MacsteelExpressShopCode))
            ModelState.AddModelError("AccountNumber", await _localizationService.GetResourceAsync("Plugin.Misc.NopStation.Plugin.B2B.B2BB2CFeatures.B2CMacsteelExpressShop.AlreadyExists"));

        if (ModelState.IsValid)
        {
            var b2CMacsteelExpressShop = model.ToEntity<B2CMacsteelExpressShop>();
            await _b2CMacsteelExpressShopService.InsertB2CMacsteelExpressShopAsync(b2CMacsteelExpressShop);

            //activity log
            await _customerActivityService.InsertActivityAsync(B2BB2CFeaturesDefaults.B2CMacsteelExpressShopInsert,
               string.Format(await _localizationService.GetResourceAsync("Plugin.Misc.NopStation.Plugin.B2B.B2BB2CFeatures.B2CMacsteelExpressShop.Created"), model.Id, model.MacsteelExpressShopName));
            _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Plugin.Misc.NopStation.Plugin.B2B.B2BB2CFeatures.B2CMacsteelExpressShop.Created"));

            if (!continueEditing)
                return RedirectToAction("List");

            return RedirectToAction("Edit", new { id = b2CMacsteelExpressShop.Id });
        }

        return View(model);
    }

    public virtual async Task<IActionResult> Edit(int id)
    {
        if (!await _erpCustomerFunctionalityService.IsCurrentCustomerInAdministratorRoleAsync())
            return AccessDeniedView();

        var b2CMacsteelExpressShop = await _b2CMacsteelExpressShopService.GetB2CMacsteelExpressShopByIdAsync(id);
        if (b2CMacsteelExpressShop == null)
        {
            _notificationService.WarningNotification(await _localizationService.GetResourceAsync("Plugin.Misc.NopStation.Plugin.B2B.B2BB2CFeatures.B2CMacsteelExpressShop.NotFound"));
            return RedirectToAction("List");
        }

        var model = await _b2CMacsteelExpressShopFactory.PrepareB2CMacsteelExpressShopModelAsync(null, b2CMacsteelExpressShop);

        return View(model);
    }

    [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
    [FormValueRequired("save", "save-continue")]
    public virtual async Task<IActionResult> Edit(B2CMacsteelExpressShopModel model, bool continueEditing, IFormCollection form)
    {
        if (!await _erpCustomerFunctionalityService.IsCurrentCustomerInAdministratorRoleAsync())
            return AccessDeniedView();

        var b2CMacsteelExpressShop = await _b2CMacsteelExpressShopService.GetB2CMacsteelExpressShopByIdWithoutTrackingAsync(model.Id);
        if (b2CMacsteelExpressShop == null)
        {
            _notificationService.WarningNotification(await _localizationService.GetResourceAsync("Plugin.Misc.NopStation.Plugin.B2B.B2BB2CFeatures.B2CMacsteelExpressShop.NotFound"));
            return RedirectToAction("List");
        }

        if (b2CMacsteelExpressShop.MacsteelExpressShopCode != model.MacsteelExpressShopCode)
            if (await _b2CMacsteelExpressShopService.CheckAnyB2CMacsteelExpressShopByCodeAsync(model.MacsteelExpressShopCode))
                ModelState.AddModelError("MacsteelExpressShopCode", await _localizationService.GetResourceAsync("Plugin.Misc.NopStation.Plugin.B2B.B2BB2CFeatures.B2CMacsteelExpressShop.AlreadyExists"));

        if (ModelState.IsValid)
        {
            b2CMacsteelExpressShop = model.ToEntity<B2CMacsteelExpressShop>();
            await _b2CMacsteelExpressShopService.UpdateB2CMacsteelExpressShopAsync(b2CMacsteelExpressShop);

            await _customerActivityService.InsertActivityAsync(B2BB2CFeaturesDefaults.B2CMacsteelExpressShopUpdate,
               string.Format(await _localizationService.GetResourceAsync("Plugin.Misc.NopStation.Plugin.B2B.B2BB2CFeatures.B2CMacsteelExpressShop.Updated"), b2CMacsteelExpressShop.MacsteelExpressShopName, b2CMacsteelExpressShop.Id));

            _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Plugin.Misc.NopStation.Plugin.B2B.B2BB2CFeatures.B2CMacsteelExpressShop.Updated"));

            if (!continueEditing)
                return RedirectToAction("List");

            return RedirectToAction("Edit", new { id = b2CMacsteelExpressShop.Id });
        }

        return View(model);
    }

    [HttpPost]
    public virtual async Task<IActionResult> Delete(int id)
    {
        if (!await _erpCustomerFunctionalityService.IsCurrentCustomerInAdministratorRoleAsync())
            return AccessDeniedView();

        var b2CMacsteelExpressShop = await _b2CMacsteelExpressShopService.GetB2CMacsteelExpressShopByIdAsync(id);

        if (b2CMacsteelExpressShop == null)
        {
            _notificationService.WarningNotification(await _localizationService.GetResourceAsync("Plugin.Misc.NopStation.Plugin.B2B.B2BB2CFeatures.B2CMacsteelExpressShop.NotFound"));
            return RedirectToAction("List");
        }

        await _b2CMacsteelExpressShopService.DeleteB2CMacsteelExpressShopAsync(b2CMacsteelExpressShop);

        await _customerActivityService.InsertActivityAsync(B2BB2CFeaturesDefaults.B2CMacsteelExpressShopDelete,
           string.Format(await _localizationService.GetResourceAsync("Plugins.Plugin.Misc.NopStation.Plugin.B2B.B2BB2CFeatures.B2CMacsteelExpressShop.Deleted"), b2CMacsteelExpressShop.MacsteelExpressShopName, b2CMacsteelExpressShop.MacsteelExpressShopCode));

        _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Plugin.Misc.NopStation.Plugin.B2B.B2BB2CFeatures.B2CMacsteelExpressShop.Deleted"));

        return RedirectToAction("List");
    }

    #endregion
}







