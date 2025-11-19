using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Web.Areas.Admin.Models.Catalog;
using Nop.Web.Framework.Components;
using NopStation.Plugin.B2B.B2BB2CFeatures.Areas.Admin.Factories;

namespace NopStation.Plugin.B2B.B2BB2CFeatures.Areas.Admin.Components;

public class ErpCategoryImageMappingAdminViewComponent : NopViewComponent
{
    private readonly IErpCategoryImageMappingModelFactory _erpCategoryImageMappingModelFactory;

    public ErpCategoryImageMappingAdminViewComponent(IErpCategoryImageMappingModelFactory erpCategoryImageMappingModelFactory)
    {
        _erpCategoryImageMappingModelFactory = erpCategoryImageMappingModelFactory;
    }

    public async Task<IViewComponentResult> InvokeAsync(string widgetZone, object additionalData)
    {
        if (additionalData == null)
        {
            return Content(string.Empty);
        }
        var categoryModel = (CategoryModel)additionalData;
        var b2BCategoryImageShow = await _erpCategoryImageMappingModelFactory.PrepareB2BCategoryImageMappingModelAsync(categoryModel, categoryModel.Id);
        return View(b2BCategoryImageShow);
    }
}
