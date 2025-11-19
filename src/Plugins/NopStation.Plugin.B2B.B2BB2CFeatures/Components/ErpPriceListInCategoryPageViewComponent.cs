using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Services.Catalog;
using Nop.Web.Framework.Components;
using Nop.Web.Models.Catalog;
using NopStation.Plugin.B2B.B2BB2CFeatures.Model;
using NopStation.Plugin.B2B.B2BB2CFeatures.Services.ErpCustomerFunctionality;

namespace NopStation.Plugin.B2B.B2BB2CFeatures.Components;

public class ErpPriceListInCategoryPageViewComponent : NopViewComponent
{
    private readonly IErpCustomerFunctionalityService _erpCustomerFunctionalityService;
    private readonly ICategoryService _categoryService;
    private readonly IWorkContext _workContext;

    public ErpPriceListInCategoryPageViewComponent(
        IErpCustomerFunctionalityService erpCustomerFunctionalityService,
        ICategoryService categoryService,
        IWorkContext workContext
    )
    {
        _erpCustomerFunctionalityService = erpCustomerFunctionalityService;
        _categoryService = categoryService;
        _workContext = workContext;
    }

    public async Task<IViewComponentResult> InvokeAsync(string widgetZone, object additionalData)
    {

        var categoryModel = additionalData as CategoryModel;
        if (categoryModel == null)
        {
            return Content(string.Empty);  
        }

        var erpAccount = await _erpCustomerFunctionalityService.GetActiveErpAccountByCustomerAsync(
            await _workContext.GetCurrentCustomerAsync()
        );

        if (erpAccount == null)
        {
            return Content(string.Empty);
        }

        var category = await _categoryService.GetCategoryByIdAsync(categoryModel.Id);

        if (category == null)
        {
            return Content(string.Empty);
        }

        var model = new ErpPriceListInCategoryPageModel { CategoryId = categoryModel.Id };

        return View(model);
    }
}
