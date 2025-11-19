using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Services.Security;
using NopStation.Plugin.B2B.B2BB2CFeatures.Areas.Admin.Factories.ErpDeliveryDates;
using NopStation.Plugin.B2B.B2BB2CFeatures.Areas.Admin.Models.ErpDeliveryDates;
using NopStation.Plugin.B2B.B2BB2CFeatures.Services.ErpDeliveyDates;
using NopStation.Plugin.Misc.Core.Controllers;

namespace NopStation.Plugin.B2B.B2BB2CFeatures.Areas.Admin.Controllers;
public class ErpDeliveryDatesController(IPermissionService _permissionService,
                                      IErpDeliveyDatesModelFactory deliveyDatesModelFactory,
                                      IErpDeliveryDatesService erpDeliveryDatesService
                                     ) : NopStationAdminController
{
    #region Fields

    private readonly IErpDeliveyDatesModelFactory _deliveyDatesModelFactory = deliveyDatesModelFactory;
    private readonly IErpDeliveryDatesService _erpDeliveryDatesService = erpDeliveryDatesService;
    private readonly IPermissionService _permissionService = _permissionService;

    #endregion

    #region Methods

    public virtual async Task<IActionResult> List()
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.AccessAdminPanel))
            return AccessDeniedView();

        //prepare model
        var model = await _deliveyDatesModelFactory.PrepareDeliveryDatesSearchModelAsync(new ErpDeliveryDatesSearchModel());

        return View(model);
    }

    [HttpPost]
    public virtual async Task<IActionResult> DeliveryDatesList(ErpDeliveryDatesSearchModel searchModel)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.AccessAdminPanel))
            return await AccessDeniedDataTablesJson();

        //prepare model
        var model = await _deliveyDatesModelFactory.PrepareDeliveryDatesListsModelAsync(searchModel);

        return Json(model);
    }

    #endregion
}
