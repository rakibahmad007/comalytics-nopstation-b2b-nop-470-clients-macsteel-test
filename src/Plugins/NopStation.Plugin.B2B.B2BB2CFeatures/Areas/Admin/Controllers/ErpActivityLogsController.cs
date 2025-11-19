using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Services.Security;
using Nop.Web.Framework.Mvc;
using NopStation.Plugin.B2B.B2BB2CFeatures.Areas.Admin.Factories;
using NopStation.Plugin.B2B.B2BB2CFeatures.Areas.Admin.Models.ErpActivityLogs;
using NopStation.Plugin.B2B.ERPIntegrationCore.Services;
using NopStation.Plugin.Misc.Core.Controllers;

namespace NopStation.Plugin.B2B.B2BB2CFeatures.Areas.Admin.Controllers;

public partial class ErpActivityLogsController : NopStationAdminController
{
    #region Fields

    private readonly IErpActivityLogsModelFactory _erpActivityLogsModelFactory;
    private readonly IErpActivityLogsService _erpActivityLogsService;
    private readonly IPermissionService _permissionService;

    #endregion

    #region Ctor

    public ErpActivityLogsController(IErpActivityLogsModelFactory erpActivityLogsModelFactory,
        IErpActivityLogsService erpActivityLogsService,
        IPermissionService permissionService)
    {
        _erpActivityLogsModelFactory = erpActivityLogsModelFactory;
        _erpActivityLogsService = erpActivityLogsService;
        _permissionService = permissionService;
    }

    #endregion

    #region Methods

    public virtual async Task<IActionResult> ERPActivityLogs()
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageActivityLog))
            return AccessDeniedView();

        //prepare model
        var model = await _erpActivityLogsModelFactory.PrepareErpActivityLogsSearchModelAsync(new ErpActivityLogsSearchModel());

        return View(model);
    }

    [HttpPost]
    public virtual async Task<IActionResult> ListLogs(ErpActivityLogsSearchModel searchModel)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageActivityLog))
            return await AccessDeniedDataTablesJson();

        //prepare model
        var model = await _erpActivityLogsModelFactory.PrepareErpActivityLogsListModelAsync(searchModel);

        return Json(model);
    }

    [HttpPost]
    public virtual async Task<IActionResult> ActivityLogDelete(int id)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageActivityLog))
            return AccessDeniedView();

        //try to get a log item with the specified id
        var logItem = await _erpActivityLogsService.GetErpActivityByIdAsync(id)
            ?? throw new ArgumentException("No erp activity log found with the specified id", nameof(id));

        await _erpActivityLogsService.DeleteErpActivityAsync(logItem);

        return new NullJsonResult();
    }

    [HttpPost]
    public virtual async Task<IActionResult> ClearAll()
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageActivityLog))
            return AccessDeniedView();

        await _erpActivityLogsService.ClearAllErpActivitiesAsync();

        return RedirectToAction("ERPActivityLogs");
    }

    #endregion
}