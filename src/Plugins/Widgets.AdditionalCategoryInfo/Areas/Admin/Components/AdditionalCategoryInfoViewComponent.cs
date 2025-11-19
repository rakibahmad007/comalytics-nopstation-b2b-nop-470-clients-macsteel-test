using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Plugin.Widgets.AdditionalCategoryInfo.Areas.Admin.Models;
using Nop.Plugin.Widgets.AdditionalCategoryInfo.Domain;
using Nop.Plugin.Widgets.AdditionalCategoryInfo.Services;
using Nop.Services.Catalog;
using Nop.Web.Areas.Admin.Models.Catalog;
using Nop.Web.Framework.Components;
using Nop.Web.Framework.Models;

namespace Nop.Plugin.Widgets.AdditionalCategoryInfo.Areas.Admin.Components;

public class AdditionalCategoryInfoViewComponent : NopViewComponent
{
    private readonly IAdditionalCategoryInfoDataService _customService;

    public AdditionalCategoryInfoViewComponent(
        IAdditionalCategoryInfoDataService customService
    )
    {
        _customService = customService;
    }

    public async Task<IViewComponentResult> InvokeAsync(string widgetZone, object additionalData)
    {
        if (additionalData == null)
        {
            return Content(string.Empty);
        }
        var model = (CategoryModel)additionalData;
        var data = await _customService.GetAdditionalCategoryInfoByCategoryIdAsync(model.Id);
        string richText = "";
        bool active = false;
        if (data != null)
        {
            richText = data.AdditionalInfoField;
            active = data.Active;
        }
        var viewModel = new AdditionalCategoryInfoModel
        {
            CategoryId = model.Id,
            Active = active,
            AdditionalInfoField = richText,
        };
        return View(
            "~/Plugins/Nop.Plugin.Widgets.AdditionalCategoryInfo/Areas/Admin/Views/AdditionalTab.cshtml",
            viewModel
        );
    }
}
