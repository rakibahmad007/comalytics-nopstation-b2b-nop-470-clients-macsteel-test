using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Plugin.Widgets.AdditionalCategoryInfo.Areas.Admin.Models;
using Nop.Plugin.Widgets.AdditionalCategoryInfo.Domain;
using Nop.Plugin.Widgets.AdditionalCategoryInfo.Services;
using Nop.Services.Localization;
using Nop.Services.Security;
using Nop.Web.Areas.Admin.Controllers;

namespace Nop.Plugin.Widgets.AdditionalCategoryInfo.Areas.Admin.Controllers;

public class AdditionalCategoryInfoController : BaseAdminController
{
    private readonly ILocalizationService _localizationService;

    private readonly IPermissionService _permissionService;

    private readonly IAdditionalCategoryInfoDataService _customService;

    public AdditionalCategoryInfoController(ILocalizationService localizationService, IPermissionService permissionService, IAdditionalCategoryInfoDataService customService)
    {
        _localizationService = localizationService;
        _permissionService = permissionService;
        _customService = customService;
    }

    public async Task<IActionResult> AdditionalCategoryInfoAction(AdditionalCategoryInfoModel model)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageWidgets))
        {
            return AccessDeniedView();
        }
        var additionalCategoryInfo = await _customService.GetAdditionalCategoryInfoByCategoryIdAsync(model.CategoryId);
        if (additionalCategoryInfo == null)
        {
            additionalCategoryInfo = new AdditionalCategoryInfoData
            {
                CategoryId = model.CategoryId,
                Active = model.Active,
                AdditionalInfoField = model.AdditionalInfoField
            };
            await _customService.CreateAsync(additionalCategoryInfo);
        }
        else
        {
            additionalCategoryInfo.Active = model.Active;
            additionalCategoryInfo.AdditionalInfoField = model.AdditionalInfoField;
            await _customService.UpdateAsync(additionalCategoryInfo);
        }
        return Json(new
        {
            Result = true,
            Msg = await _localizationService.GetResourceAsync("Plugins.Widgets.AdditionalCategoryInfo.UpdatedSuccess")
        });
    }
}