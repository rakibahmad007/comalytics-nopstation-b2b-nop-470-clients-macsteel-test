using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Services.Messages;
using Nop.Web.Framework;
using Nop.Web.Framework.Controllers;
using NopStation.Plugin.B2B.B2BB2CFeatures.Areas.Admin.Factories;
using NopStation.Plugin.B2B.B2BB2CFeatures.Areas.Admin.Models.ERPPriceListDownloadTracks;
using NopStation.Plugin.B2B.B2BB2CFeatures.Services.ErpCustomerFunctionality;

namespace NopStation.Plugin.B2B.B2BB2CFeatures.Areas.Admin.Controllers;

[Area(AreaNames.ADMIN)]
public class ERPPriceListDownloadTrackController : BasePluginController
{
    private readonly IERPPriceListDownloadTrackFactory _erpPriceListDownloadTrackFactory;
    private readonly INotificationService _notificationService;
    private readonly IErpCustomerFunctionalityService _erpCustomerFunctionalityService;

    public ERPPriceListDownloadTrackController(IERPPriceListDownloadTrackFactory erpPriceListDownloadTrackFactory, IWorkContext workContext,
       INotificationService notificationService,
       IErpCustomerFunctionalityService erpCustomerFunctionalityService)
    {
        _erpPriceListDownloadTrackFactory = erpPriceListDownloadTrackFactory;
        _notificationService = notificationService;
        _erpCustomerFunctionalityService = erpCustomerFunctionalityService;
    }

    public virtual async Task<IActionResult> List()
    {
        if (!await _erpCustomerFunctionalityService.IsCurrentCustomerInAdministratorRoleAsync())
            return await AccessDeniedDataTablesJson();

        var model = await _erpPriceListDownloadTrackFactory.PrepareERPPriceListSearchModelAsync(new ErpPriceListSearchModel());
        return View("~/Plugins/NopStation.Plugin.B2B.B2BB2CFeatures/Areas/Admin/Views/ERPPriceListDownloadTrack/List.cshtml", model);
    }

    [HttpPost]
    public virtual async Task<IActionResult> ListData(ErpPriceListSearchModel searchModel)
    {
        if (!await _erpCustomerFunctionalityService.IsCurrentCustomerInAdministratorRoleAsync())
            return await AccessDeniedDataTablesJson();

        var model = await _erpPriceListDownloadTrackFactory.PrepareERPPriceListListModelAsync(searchModel);
        return Json(model);
    }

    [HttpPost, ActionName("List")]
    [FormValueRequired("exportexcel-all")]
    public virtual async Task<IActionResult> ExportExcelAll(ErpPriceListSearchModel searchModel)
    {
        if (!await _erpCustomerFunctionalityService.IsCurrentCustomerInAdministratorRoleAsync())
            return AccessDeniedView();

        try
        {
            var bytes = await _erpPriceListDownloadTrackFactory.ExportERPPriceListDownloadToXlsxAsync(searchModel);

            return File(bytes, MimeTypes.TextXlsx, "PriceListDownloadTrack.xlsx");
        }
        catch (Exception exc)
        {
            await _notificationService.ErrorNotificationAsync(exc);
            return RedirectToAction("List");
        }
    }

    [HttpPost]
    public virtual async Task<IActionResult> ExportExcelSelected(string selectedIds)
    {
        if (!await _erpCustomerFunctionalityService.IsCurrentCustomerInAdministratorRoleAsync())
            return AccessDeniedView();

        if (!string.IsNullOrEmpty(selectedIds))
        {
            var ids = selectedIds
                .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(int.Parse)
                .ToList();
            var bytes = await _erpPriceListDownloadTrackFactory.ExportERPPriceListDownloadToXlsxAsync(ids);
            return File(bytes, MimeTypes.TextXlsx, "PriceListDownloadTrack.xlsx");
        }

        return RedirectToAction("List");
    }
}
