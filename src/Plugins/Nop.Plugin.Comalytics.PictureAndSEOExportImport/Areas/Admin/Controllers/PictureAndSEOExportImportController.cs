using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Plugin.Comalytics.PictureAndSEOExportImport.Areas.Admin.Models;
using Nop.Plugin.Comalytics.PictureAndSEOExportImport.Services;
using Nop.Services.Catalog;
using Nop.Services.Common;
using Nop.Services.Configuration;
using Nop.Services.ExportImport;
using Nop.Services.Localization;
using Nop.Services.Messages;
using Nop.Services.Security;
using Nop.Web.Areas.Admin.Controllers;
using Nop.Web.Framework.Controllers;

namespace Nop.Plugin.Comalytics.PictureAndSEOExportImport.Areas.Admin.Controllers
{
    public class PictureAndSEOExportImportController : BaseAdminController
    {
        private readonly IPermissionService _permissionService;
        private readonly IWorkContext _workContext;
        private readonly IProductService _productService;
        private readonly IExportManager _exportManager;
        private readonly INotificationService _notificationService;
        private readonly IPictureAndSEOExportImportService _pictureAndSEOExportImportService;
        private readonly ILocalizationService _localizationService;
        private readonly ISettingService _settingService;
        private readonly IStoreContext _storeContext;
        private readonly IGenericAttributeService _genericAttributeService;

        public PictureAndSEOExportImportController(IPermissionService permissionService,
                                                   IWorkContext workContext,
                                                   IProductService productService,
                                                   IExportManager exportManager,
                                                   INotificationService notificationService,
                                                   IPictureAndSEOExportImportService pictureAndSEOExportImportService,
                                                   ILocalizationService localizationService,
                                                   ISettingService settingService,
                                                   IStoreContext storeContext,
                                                   IGenericAttributeService genericAttributeService)
        {
            _permissionService = permissionService;
            _workContext = workContext;
            _productService = productService;
            _exportManager = exportManager;
            _notificationService = notificationService;
            _pictureAndSEOExportImportService = pictureAndSEOExportImportService;
            _localizationService = localizationService;
            _settingService = settingService;
            _storeContext = storeContext;
            _genericAttributeService = genericAttributeService;
        }

        public async Task<IActionResult> Configure(PictureAndSEOExportImportSearchModel searchModel)
        {
            var customer = await _workContext.GetCurrentCustomerAsync();
            var result = await _genericAttributeService.GetAttributeAsync<string>(customer, "excelImportResult");

            searchModel.IsUploaded = 0;
            searchModel.SetGridPageSize();

            if (int.TryParse(result, out var logId))
            {
                searchModel.LogId = logId;
                searchModel.IsUploaded = 1;
                if (logId == 0)
                {
                    searchModel.Message = await _localizationService.GetResourceAsync("Plugins.Comalytics.PictureAndSEOExportImport.ImportExcel.Success");
                }
                else if (logId == -1)
                {
                    searchModel.Message = await _localizationService.GetResourceAsync("Plugins.Comalytics.PictureAndSEOExportImport.ImportExcel.Failed");
                }
                else
                {
                    searchModel.Message = await _localizationService.GetResourceAsync("Plugins.Comalytics.PictureAndSEOExportImport.ImportExcel.ErrorOccured");
                }
            }
            result = null;
            await _genericAttributeService.SaveAttributeAsync(customer, "excelImportResult", result);

            return View(searchModel);
        }

        [HttpPost, ActionName("Configure")]
        [FormValueRequired("export-excel-file")]
        public async Task<IActionResult> ExportToExcel(PictureAndSEOExportImportSearchModel model)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageProducts))
                return AccessDeniedView();

            var products = await _productService.SearchProductsAsync();

            try
            {
                var bytes = await _pictureAndSEOExportImportService.ExportToExcelAsync(products);

                return File(bytes, MimeTypes.TextXlsx, "products.xlsx");
            }
            catch (Exception exc)
            {
                await _notificationService.ErrorNotificationAsync(exc, true);
                return RedirectToAction("Configure");
            }
        }

        [HttpPost]
        public async Task<IActionResult> ImportExcel(IFormFile importexcelfile)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageProducts))
                return AccessDeniedView();

            //load settings for a chosen store scope
            var storeScope = await _storeContext.GetActiveStoreScopeConfigurationAsync();
            var setting = await _settingService.LoadSettingAsync<PictureAndSEOExportImportSettings>(storeScope);

            try
            {
                if (importexcelfile != null && importexcelfile.Length > 0)
                {
                    if (importexcelfile.ContentType != "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet")
                    {
                        _notificationService.ErrorNotification("Please upload a valid Excel (.xlsx) file.");
                        return RedirectToAction("Configure");
                    }

                    if (setting.IsOccupied)
                    {
                        _notificationService.ErrorNotification(await _localizationService.GetResourceAsync("Plugins.Comalytics.PictureAndSEOExportImport.ImportExcel.IsOccupied"));
                        return RedirectToAction("Configure");
                    }
                    setting.IsOccupied = true;
                    await _settingService.SaveSettingAsync(setting);
                    var result = await _pictureAndSEOExportImportService.ImportExcelAsync(importexcelfile.OpenReadStream());

                    var customer = await _workContext.GetCurrentCustomerAsync();
                    await _genericAttributeService.SaveAttributeAsync(customer, "excelImportResult", result);
                }
                else
                {
                    _notificationService.ErrorNotification(await _localizationService.GetResourceAsync("Admin.Common.UploadFile"));
                }
            }
            catch (Exception ex)
            {
                await _notificationService.ErrorNotificationAsync(ex, true);
            }

            setting.IsOccupied = false;
            await _settingService.SaveSettingAsync(setting);

            return RedirectToAction("Configure");
        }
    }
}