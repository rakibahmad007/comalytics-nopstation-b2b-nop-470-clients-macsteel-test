using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Messages;
using Nop.Web.Framework;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Mvc;
using Nop.Web.Framework.Mvc.Filters;
using NopStation.Plugin.B2B.B2BB2CFeatures.Areas.Admin.Factories;
using NopStation.Plugin.B2B.B2BB2CFeatures.Areas.Admin.Models.SpecialIncludeExcludes;
using NopStation.Plugin.B2B.B2BB2CFeatures.Services.ErpCustomerFunctionality;
using NopStation.Plugin.B2B.B2BB2CFeatures.Services.SpecialIncludeExcludeService;
using NopStation.Plugin.B2B.ERPIntegrationCore.Domain;
using NopStation.Plugin.B2B.ERPIntegrationCore.Enums;

namespace NopStation.Plugin.B2B.B2BB2CFeatures.Areas.Admin.Controllers;

[Area(AreaNames.ADMIN)]
public class B2BCustomerRestrictionController : BasePluginController
{
    #region Fields

    private readonly IErpCustomerFunctionalityService _erpCustomerFunctionalityService;
    private readonly INotificationService _notificationService;
    private readonly ILocalizationService _localizationService;
    private readonly ISpecialIncludeExcludeModelFactory _specialIncludeExcludeModelFactory;
    private readonly IB2BSpecialIncludeExcludeService _b2BSpecialIncludeExcludeService;
    private readonly ILogger _logger;

    #endregion

    #region Ctor

    public B2BCustomerRestrictionController(
        INotificationService notificationService,
        ILocalizationService localizationService,
        ISpecialIncludeExcludeModelFactory specialIncludeExcludeModelFactory,
        IB2BSpecialIncludeExcludeService b2BSpecialIncludeExcludeService,
        ILogger logger,
        IErpCustomerFunctionalityService erpCustomerFunctionalityService)
    {
        _notificationService = notificationService;
        _localizationService = localizationService;
        _specialIncludeExcludeModelFactory = specialIncludeExcludeModelFactory;
        _b2BSpecialIncludeExcludeService = b2BSpecialIncludeExcludeService;
        _logger = logger;
        _erpCustomerFunctionalityService = erpCustomerFunctionalityService;
    }

    #endregion

    #region List/ Create/ Update/ Delete

    public virtual async Task<IActionResult> ShownList()
    {
        if (!await _erpCustomerFunctionalityService.IsCurrentCustomerInAdministratorRoleAsync())
            return AccessDeniedView();

        var model =
            await _specialIncludeExcludeModelFactory.PrepareSpecialIncludeExcludeSearchModelAsync(
                new SpecialIncludeExcludeSearchModel()
            );
        return View(model);
    }

    [HttpPost]
    public virtual async Task<IActionResult> ShownList(
        SpecialIncludeExcludeSearchModel searchModel
    )
    {
        if (!await _erpCustomerFunctionalityService.IsCurrentCustomerInAdministratorRoleAsync())
            return await AccessDeniedDataTablesJson();
        searchModel.Type = (int)SpecialType.Show;
        var model =
            await _specialIncludeExcludeModelFactory.PrepareSpecialIncludeExcludeListModelAsync(
                searchModel
            );
        return Json(model);
    }

    public virtual async Task<IActionResult> HideList()
    {
        if (!await _erpCustomerFunctionalityService.IsCurrentCustomerInAdministratorRoleAsync())
            return AccessDeniedView();

        var model =
            await _specialIncludeExcludeModelFactory.PrepareSpecialIncludeExcludeSearchModelAsync(
                new SpecialIncludeExcludeSearchModel()
            );
        return View(model);
    }

    [HttpPost]
    public virtual async Task<IActionResult> HideList(
        SpecialIncludeExcludeSearchModel searchModel
    )
    {
        if (!await _erpCustomerFunctionalityService.IsCurrentCustomerInAdministratorRoleAsync())
            return await AccessDeniedDataTablesJson();
        searchModel.Type = (int)SpecialType.Hide;
        var model =
            await _specialIncludeExcludeModelFactory.PrepareSpecialIncludeExcludeListModelAsync(
                searchModel
            );
        return Json(model);
    }

    public virtual async Task<IActionResult> CreateForShow()
    {
        if (!await _erpCustomerFunctionalityService.IsCurrentCustomerInAdministratorRoleAsync())
            return AccessDeniedView();

        var model =
            await _specialIncludeExcludeModelFactory.PrepareSpecialIncludeExcludeModelAsync(
                new SpecialIncludeExcludeModel()
            );
        return View(model);
    }

    public virtual async Task<IActionResult> CreateForHide()
    {
        if (!await _erpCustomerFunctionalityService.IsCurrentCustomerInAdministratorRoleAsync())
            return AccessDeniedView();

        var model =
            await _specialIncludeExcludeModelFactory.PrepareSpecialIncludeExcludeModelAsync(
                new SpecialIncludeExcludeModel()
            );
        return View(model);
    }

    [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
    [FormValueRequired("save", "save-continue")]
    public virtual async Task<IActionResult> Create(
        SpecialIncludeExcludeModel model,
        IFormCollection form
    )
    {
        if (!await _erpCustomerFunctionalityService.IsCurrentCustomerInAdministratorRoleAsync())
            return AccessDeniedView();
        if (ModelState.IsValid)
        {
            if (model != null && model.ErpAccountId > 0 && model.ProductId > 0)
            {
                model =
                    await _specialIncludeExcludeModelFactory.PrepareSpecialIncludeExcludeModelAsync(
                        model
                    );
                var isExisting =
                    await _b2BSpecialIncludeExcludeService.GetUniqueB2BSpecialIncludesAndExcludesAsync(
                        model
                    );
                if (isExisting == null)
                {
                    if (model.ErpSalesOrgId > 0)
                    {
                        var specialIncludeExcludeEntity = new SpecialIncludesAndExcludes
                        {
                            ErpSalesOrgId = model.ErpSalesOrgId,
                            ErpAccountId = model.ErpAccountId,
                            LastUpdate = DateTime.UtcNow,
                            ProductId = model.ProductId,
                            IsActive = model.IsActive,
                            SpecialTypeId = model.SpecialTypeId,
                        };
                        await _b2BSpecialIncludeExcludeService.AddSpecialIncludesAndExcludesAsync(
                            specialIncludeExcludeEntity
                        );
                        if (model.SpecialTypeId == (int)SpecialType.Show)
                            _notificationService.SuccessNotification(
                                await _localizationService.GetResourceAsync(
                                    "Plugin.Misc.NopStation.Plugin.B2B.B2BB2CFeatures.SpecialIncludes.Added"
                                )
                            );
                        else
                            _notificationService.SuccessNotification(
                                await _localizationService.GetResourceAsync(
                                    "Plugin.Misc.NopStation.Plugin.B2B.B2BB2CFeatures.SpecialExcludes.Added"
                                )
                            );
                    }
                }
                else
                {
                    _notificationService.ErrorNotification(
                        await _localizationService.GetResourceAsync(
                            "Plugin.Misc.NopStation.Plugin.B2B.B2BB2CFeatures.SpecialIncludesAndExcludes.AlreadyExist"
                        )
                    );
                }
            }
        }
        if (model.SpecialTypeId == (int)SpecialType.Show)
        {
            return RedirectToAction(nameof(ShownList));
        }
        else
        {
            return RedirectToAction(nameof(HideList));
        }
    }

    [HttpPost]
    public virtual async Task<IActionResult> Edit(SpecialIncludeExcludeModel model)
    {
        if (!await _erpCustomerFunctionalityService.IsCurrentCustomerInAdministratorRoleAsync())
            return AccessDeniedView();

        var specialInExRecord =
            await _b2BSpecialIncludeExcludeService.GetSpecialIncludeExcludeByIdAsync(model.Id);
        if (specialInExRecord == null)
            return new NullJsonResult();

        await _b2BSpecialIncludeExcludeService.UpdateSpecialIncludeExcludeAsync(
            model.Id,
            model.IsActive
        );
        return new NullJsonResult();
    }

    [HttpPost]
    public virtual async Task<IActionResult> Delete(int id)
    {
        if (!await _erpCustomerFunctionalityService.IsCurrentCustomerInAdministratorRoleAsync())
            return AccessDeniedView();

        var specialInExRecord =
            await _b2BSpecialIncludeExcludeService.GetSpecialIncludeExcludeByIdAsync(id);
        if (specialInExRecord == null)
            return new NullJsonResult();
        await _b2BSpecialIncludeExcludeService.DeleteSpecialIncludeExcludeByIdAsync(id);
        return new NullJsonResult();
    }

    public virtual async Task<IActionResult> DeleteSelected(ICollection<int> selectedIds)
    {
        if (!await _erpCustomerFunctionalityService.IsCurrentCustomerInAdministratorRoleAsync())
            return AccessDeniedView();

        if (selectedIds != null)
            await _b2BSpecialIncludeExcludeService.DeleteSpecialIncludeExcludeByIdListAsync(
                selectedIds
            );
        return Json(new { Result = true });
    }

    #endregion

    #region improt/excel

    public virtual async Task<IActionResult> ExportXlsx(int type, int mode)
    {
        if (!await _erpCustomerFunctionalityService.IsCurrentCustomerInAdministratorRoleAsync())
            return AccessDeniedView();

        try
        {
            var bytes =
                await _b2BSpecialIncludeExcludeService.ExportSpecialIncludeExcludeToXlsxAsync(
                    type,
                    mode
                );
            return File(bytes, MimeTypes.TextXlsx, "SpecialIncludeExcludes.xlsx");
        }
        catch (Exception exc)
        {
            await _notificationService.ErrorNotificationAsync(exc);
            return RedirectToAction((type == (int)SpecialType.Show) ? "ShownList" : "HideList");
        }
    }

    [HttpPost]
    public virtual async Task<IActionResult> ExportExcelSelected(ICollection<int> selectedIds)
    {
        if (!await _erpCustomerFunctionalityService.IsCurrentCustomerInAdministratorRoleAsync())
            return AccessDeniedView();

        if (selectedIds != null)
        {
            try
            {
                var bytes =
                    await _b2BSpecialIncludeExcludeService.ExportSpecialIncludeExcludeToXlsxAsync(
                        selectedIds
                    );
                return File(bytes, MimeTypes.TextXlsx, "SpecialIncludeExcludes.xlsx");
            }
            catch (Exception exc)
            {
                return new NullJsonResult();
            }
        }
        return new NullJsonResult();
    }

    [HttpPost]
    public virtual async Task<IActionResult> ImportExcel(int type, IFormFile importexcelfile)
    {
        if (!await _erpCustomerFunctionalityService.IsCurrentCustomerInAdministratorRoleAsync())
            return AccessDeniedView();

        var result = new ImportResult();
        try
        {
            if (
                importexcelfile != null
                && importexcelfile.Length > 0
                && (type == (int)SpecialType.Show || type == (int)SpecialType.Hide)
            )
            {
                result =
                    await _b2BSpecialIncludeExcludeService.ImportSpecialIncludeExcludesFromXlsxAsync(
                        importexcelfile.OpenReadStream(),
                        (SpecialType)type
                    );
            }
            else
            {
                _notificationService.ErrorNotification(
                    await _localizationService.GetResourceAsync("Admin.Common.UploadFile")
                );
                return RedirectToAction(
                    (type == (int)SpecialType.Show) ? "ShownList" : "HideList"
                );
            }

            _notificationService.SuccessNotification(
                $"The import has completed. Given total: {result.GivenTotal}, "
                    + $"successfully imported: {result.GivenTotal - result.FailedImports.Count}, failed: {result.FailedImports.Count}."
            );
            if (result.FailedImports.Count > 0)
            {
                var logText =
                    "B2B special include-exclude import failed for some records. The following records already exist or inconsistent."
                    + Environment.NewLine;
                foreach (var row in result.FailedImports)
                {
                    logText +=
                        $"AccNo: {row.AccountNumber}, SalesOrgCode: {row.SalesOrgCode}, SKU: {row.SKU}{Environment.NewLine}";
                }
                _logger.Information(logText);
            }

            return RedirectToAction((type == (int)SpecialType.Show) ? "ShownList" : "HideList");
        }
        catch (Exception exc)
        {
            await _notificationService.ErrorNotificationAsync(exc);
            return RedirectToAction((type == (int)SpecialType.Show) ? "ShownList" : "HideList");
        }
    }

    #endregion
}
