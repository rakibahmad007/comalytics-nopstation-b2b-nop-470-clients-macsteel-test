using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Services.Logging;
using Nop.Web.Areas.Admin.Controllers;
using NopStation.Plugin.B2B.B2BB2CFeatures.Areas.Admin.Factories;

namespace NopStation.Plugin.B2B.B2BB2CFeatures.Areas.Admin.Controllers;

public class CategoryImageMappingController : BaseAdminController
{
    #region Fields

    private readonly IErpCategoryImageMappingModelFactory _erpCategoryImageMappingModelFactory;
    private readonly ILogger _logger;

    #endregion

    #region Ctor

    public CategoryImageMappingController(ILogger logger,
        IErpCategoryImageMappingModelFactory erpCategoryImageMappingModelFactory)
    {
        _logger = logger;
        _erpCategoryImageMappingModelFactory = erpCategoryImageMappingModelFactory;
    }

    #endregion

    #region Methods

    [HttpPost]
    public async Task<IActionResult> CategoryDetails(int categoryId, bool isShowImage)
    {
        try
        {
            await _erpCategoryImageMappingModelFactory.SetCategoryImageVisibilityAsync(categoryId, isShowImage);
            return Json(new { success = true, message = "Category image visibility updated successfully." });
        }
        catch (Exception ex)
        {
            _logger.Error("Error updating category image visibility", ex);
            return Json(new { success = false, message = "An error occurred while updating category image visibility." });
        }
    }

    #endregion
}
