using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Core.Domain.Catalog;
using Nop.Services.Catalog;
using Nop.Services.Localization;
using Nop.Services.Messages;
using Nop.Services.Security;
using Nop.Services.Seo;
using Nop.Web.Controllers;
using Nop.Web.Framework.Mvc.Routing;
using NopStation.Plugin.B2B.B2BB2CFeatures.Factories;
using NopStation.Plugin.B2B.B2BB2CFeatures.Services.ErpPriceListDownloadTrack;
using NopStation.Plugin.B2B.B2BB2CFeatures.Services.ExportManager;
using NopStation.Plugin.B2B.ERPIntegrationCore.Domain;
using NopStation.Plugin.B2B.ERPIntegrationCore.Enums;
using NopStation.Plugin.B2B.ERPIntegrationCore.Infrastructure;
using NopStation.Plugin.B2B.ERPIntegrationCore.Services;

namespace NopStation.Plugin.B2B.B2BB2CFeatures.Controllers;

public class ExportController : BasePublicController
{
    #region Fields

    private readonly ICategoryService _categoryService;
    private readonly CatalogSettings _catalogSettings;
    private readonly IProductService _productService;
    private readonly IWorkContext _workContext;
    private readonly IStoreContext _storeContext;
    private readonly INotificationService _notificationService;
    private readonly ICategoryProductsExportManager _categoryProductsExportManager;
    private readonly IUrlRecordService _urlRecordService;
    private readonly ILocalizationService _localizationService;
    private readonly IPermissionService _permissionService;
    private readonly IErpAccountService _erpAccountService;
    private readonly IERPPriceListDownloadTrackService _erpPriceListDownloadTrackService;
    private readonly IErpAccountPublicModelFactory _erpAccountPublicModelFactory;
    private readonly IErpLogsService _erpLogsService;

    #endregion

    #region Ctor

    public ExportController(ICategoryService categoryService,
        CatalogSettings catalogSettings,
        IProductService productService,
        IWorkContext workContext,
        IStoreContext storeContext,
        INotificationService notificationService,
        ICategoryProductsExportManager categoryExportManager,
        IUrlRecordService urlRecordService,
        ILocalizationService localizationService,
        IPermissionService permissionService,
        IErpAccountService erpAccountService,
        IERPPriceListDownloadTrackService erpPriceListDownloadTrackService,
        IErpAccountPublicModelFactory erpAccountPublicModelFactory,
        IErpLogsService erpLogsService)
    {
        _categoryService = categoryService;
        _catalogSettings = catalogSettings;
        _productService = productService;
        _workContext = workContext;
        _storeContext = storeContext;
        _notificationService = notificationService;
        _categoryProductsExportManager = categoryExportManager;
        _urlRecordService = urlRecordService;
        _localizationService = localizationService;
        _permissionService = permissionService;
        _erpAccountService = erpAccountService;
        _erpPriceListDownloadTrackService = erpPriceListDownloadTrackService;
        _erpAccountPublicModelFactory = erpAccountPublicModelFactory;
        _erpLogsService = erpLogsService;
    }

    #endregion

    #region Methods

    public async Task<IActionResult> ExportProductsByCategory(int categoryId)
    {
        var customer = await _workContext.GetCurrentCustomerAsync();
        var erpAccount = await _erpAccountService.GetActiveErpAccountByCustomerIdAsync(customer.Id);
        if (erpAccount == null)
            return AccessDeniedView();

        var language = await _workContext.GetWorkingLanguageAsync();
        var category = await _categoryService.GetCategoryByIdAsync(categoryId);
        ArgumentNullException.ThrowIfNull(category);

        var sename = await _urlRecordService.GetSeNameAsync(category, language.Id);
        var returnUrl = Url.RouteUrl<Category>(new { SeName = sename });

        var categoryIds = new List<int>();
        var categories = await _categoryService.GetAllCategoriesAsync();
        var currentStore = await _storeContext.GetCurrentStoreAsync();

        foreach (var item in categories)
        {
            categoryIds.Add(item.Id);

            var childCategoryIds = await _categoryService.GetChildCategoryIdsAsync(parentCategoryId: item.Id, showHidden: true);
            categoryIds.AddRange(childCategoryIds);
            if (_catalogSettings.ShowProductsFromSubcategories)
            {
                categoryIds.AddRange(await _categoryService.GetChildCategoryIdsAsync(item.Id, currentStore.Id));
            }
        }

        if (categoryIds.Count <= 0)
        {
            _notificationService
                .ErrorNotification(await _localizationService.GetResourceAsync("B2BB2CFeatures.ExportCategoryProducts.Error.NoCategoryFound"));
            return Redirect(returnUrl);
        }

        var products = await _productService.SearchProductsAsync(categoryIds: categoryIds, storeId: currentStore.Id);
        try
        {
            await _erpPriceListDownloadTrackService.InsertB2BPriceListDownloadTrackAsync(
                new ERPPriceListDownloadTrack
                {
                    NopCustomerId = customer.Id,
                    B2BAccountId = erpAccount.Id,
                    B2BSalesOrganisationId = erpAccount.ErpSalesOrgId,
                    DownloadedOnUtc = DateTime.UtcNow,
                    PriceListDownloadType = PriceListDownloadType.Excel,
                }
            );
            var bytes = await _categoryProductsExportManager.ExportProductsToXlsxAsync(products);
            var file = File(bytes, MimeTypes.TextXlsx, $"{category.Name.Replace(" ", "").Trim()}-Products.xlsx");

            return Json(new
            {
                file = file,
                fileName = file.FileDownloadName
            });
        }
        catch (Exception exc)
        {
            await _notificationService.ErrorNotificationAsync(exc);
            await _erpLogsService.InsertErpLogAsync(
                ErpLogLevel.Error, 
                ErpSyncLevel.Product, 
                $"Export Products By Category (Excel) Error for " +
                $"Customer: {customer.Email} (Id: {customer.Id})", 
                exc.Message);
        }

        return Redirect(returnUrl);
    }

    public async Task<IActionResult> ExportProductsByCategoryPDF(int categoryId)
    {
        var customer = await _workContext.GetCurrentCustomerAsync();
        var erpAccount = await _erpAccountService.GetActiveErpAccountByCustomerIdAsync(customer.Id);
        if (erpAccount == null)
            return AccessDeniedView();

        var category = await _categoryService.GetCategoryByIdAsync(categoryId);
        ArgumentNullException.ThrowIfNull(category);

        var language = await _workContext.GetWorkingLanguageAsync();
        var sename = await _urlRecordService.GetSeNameAsync(category, language.Id);
        var returnUrl = Url.RouteUrl<Category>(new { SeName = sename });

        var categoryIds = new List<int>();
        var categories = await _categoryService.GetAllCategoriesAsync();
        var currentStore = await _storeContext.GetCurrentStoreAsync();

        foreach (var item in categories)
        {
            categoryIds.Add(item.Id);

            var childCategoryIds = await _categoryService.GetChildCategoryIdsAsync(parentCategoryId: item.Id, showHidden: true);
            categoryIds.AddRange(childCategoryIds);
            if (_catalogSettings.ShowProductsFromSubcategories)
            {
                categoryIds.AddRange(await _categoryService.GetChildCategoryIdsAsync(item.Id, currentStore.Id));
            }
        }

        if (categoryIds.Count <= 0)
        {
            _notificationService
                .ErrorNotification(await _localizationService.GetResourceAsync("B2BB2CFeatures.ExportCategoryProducts.Error.NoCategoryFound"));
            return Redirect(returnUrl);
        }

        var products = await _productService.SearchProductsAsync(categoryIds: categoryIds, storeId: currentStore.Id);
        try
        {
            await _erpPriceListDownloadTrackService.InsertB2BPriceListDownloadTrackAsync(
                new ERPPriceListDownloadTrack
                {
                    NopCustomerId = customer.Id,
                    B2BAccountId = erpAccount.Id,
                    B2BSalesOrganisationId = erpAccount.ErpSalesOrgId,
                    DownloadedOnUtc = DateTime.UtcNow,
                    PriceListDownloadType = PriceListDownloadType.PDF,
                }
            );
            byte[] bytes;
            await using (var stream = new MemoryStream())
            {
                await _categoryProductsExportManager.ExportProductsToPdfAsync(stream, products);
                bytes = stream.ToArray();
            }

            var file =  File(bytes, MimeTypes.ApplicationPdf, $"{category.Name.Replace(" ", "").Trim()}-Products.pdf");

            return Json(new
            {
                file = file,
                fileName = file.FileDownloadName
            });
        }
        catch (Exception exc)
        {
            await _notificationService.ErrorNotificationAsync(exc);
            await _erpLogsService.InsertErpLogAsync(
                ErpLogLevel.Error,
                ErpSyncLevel.Product,
                $"Export Products By Category (PDF) Error for " +
                $"Customer: {customer.Email} (Id: {customer.Id})",
                exc.Message);
        }

        return Redirect(returnUrl);
    }

    public async Task<IActionResult> ExportProductsByCategoryId(int categoryId)
    {
        var customer = await _workContext.GetCurrentCustomerAsync();
        var erpAccount = await _erpAccountService.GetActiveErpAccountByCustomerIdAsync(customer.Id);
        if (erpAccount == null)
            return AccessDeniedView();

        var category = await _categoryService.GetCategoryByIdAsync(categoryId);
        var currentStore = await _storeContext.GetCurrentStoreAsync();
        var categoryIds = new List<int> { category.Id };

        var childCategoryIds = await _categoryService.GetChildCategoryIdsAsync(parentCategoryId: category.Id, showHidden: true);
        categoryIds.AddRange(childCategoryIds);

        if (_catalogSettings.ShowProductsFromSubcategories)
        {
            categoryIds.AddRange(await _categoryService.GetChildCategoryIdsAsync(category.Id, currentStore.Id));
        }

        var products = await _productService.SearchProductsAsync(categoryIds: categoryIds, storeId: currentStore.Id);
        try
        {
            await _erpPriceListDownloadTrackService.InsertB2BPriceListDownloadTrackAsync(
                new ERPPriceListDownloadTrack
                {
                    NopCustomerId = customer.Id,
                    B2BAccountId = erpAccount.Id,
                    B2BSalesOrganisationId = erpAccount.ErpSalesOrgId,
                    DownloadedOnUtc = DateTime.UtcNow,
                    PriceListDownloadType = PriceListDownloadType.Excel,
                }
            );

            var bytes = await _categoryProductsExportManager.ExportProductsToXlsxAsync(products);

            var file = File(bytes, MimeTypes.TextXlsx, $"{category.Name.Replace(" ", "").Trim()}-Products.xlsx");

            return Json(new
            {
                file = file,
                fileName = file.FileDownloadName
            });
        }
        catch (Exception exc)
        {
            await _notificationService.ErrorNotificationAsync(exc);
            await _erpLogsService.InsertErpLogAsync(
                ErpLogLevel.Error,
                ErpSyncLevel.Product,
                $"Export Products By Category (Excel) Error for " +
                $"Customer: {customer.Email} (Id: {customer.Id})",
                exc.Message);
        }

        var language = await _workContext.GetWorkingLanguageAsync();
        var sename = await _urlRecordService.GetSeNameAsync(category, language.Id);
        var returnUrl = Url.RouteUrl<Category>(new { SeName = sename });

        return Redirect(returnUrl);
    }

    public async Task<IActionResult> ExportProductsByCategoryIdPDF(int categoryId)
    {
        var customer = await _workContext.GetCurrentCustomerAsync();
        var erpAccount = await _erpAccountService.GetActiveErpAccountByCustomerIdAsync(customer.Id);
        if (erpAccount == null)
            return AccessDeniedView();

        var category = await _categoryService.GetCategoryByIdAsync(categoryId);
        ArgumentNullException.ThrowIfNull(category);

        var currentStore = await _storeContext.GetCurrentStoreAsync();
        var categoryIds = new List<int> { category.Id };

        var childCategoryIds = await _categoryService.GetChildCategoryIdsAsync(parentCategoryId: category.Id, showHidden: true);
        categoryIds.AddRange(childCategoryIds);

        if (_catalogSettings.ShowProductsFromSubcategories)
        {
            categoryIds.AddRange(await _categoryService.GetChildCategoryIdsAsync(category.Id, currentStore.Id));
        }

        var products = await _productService.SearchProductsAsync(categoryIds: categoryIds, storeId: currentStore.Id);
        try
        {
            await _erpPriceListDownloadTrackService.InsertB2BPriceListDownloadTrackAsync(
                new ERPPriceListDownloadTrack
                {
                    NopCustomerId = customer.Id,
                    B2BAccountId = erpAccount.Id,
                    B2BSalesOrganisationId = erpAccount.ErpSalesOrgId,
                    DownloadedOnUtc = DateTime.UtcNow,
                    PriceListDownloadType = PriceListDownloadType.PDF,
                }
            );
            byte[] bytes;
            await using (var stream = new MemoryStream())
            {
                await _categoryProductsExportManager.ExportProductsToPdfAsync(stream, products);
                bytes = stream.ToArray();
            }
            var file = File(bytes, MimeTypes.ApplicationPdf, $"{category.Name.Replace(" ", "").Trim()}-Products.pdf");

            return Json(new
            {
                file = file,
                fileName = file.FileDownloadName
            });
        }
        catch (Exception exc)
        {
            await _notificationService.ErrorNotificationAsync(exc);
            await _erpLogsService.InsertErpLogAsync(
                ErpLogLevel.Error, 
                ErpSyncLevel.Product, 
                $"Export Products By Category (PDF) Error for " +
                $"Customer: {customer.Email} (Id: {customer.Id})", 
                exc.Message);
        }

        var language = await _workContext.GetWorkingLanguageAsync();
        var sename = await _urlRecordService.GetSeNameAsync(category, language.Id);
        var returnUrl = Url.RouteUrl<Category>(new { SeName = sename });

        return Redirect(returnUrl);
    }

    public async Task<IActionResult> ExportLastNOrdersPerAccountExcel()
    {
        var customer = await _workContext.GetCurrentCustomerAsync();
        var erpAccount = await _erpAccountService.GetActiveErpAccountByCustomerIdAsync(customer.Id);

        if (erpAccount == null || !await _permissionService.AuthorizeAsync(ErpPermissionProvider.DisplayB2BPrices))
        {
            _notificationService.Notification(NotifyType.Error,
                await _localizationService.GetResourceAsync("Plugin.Misc.NopStation.B2BB2CFeatures.PriceListDownLoad.AccessDenied"));
            return AccessDeniedView();
        }

        try
        {
            await _erpPriceListDownloadTrackService.InsertB2BPriceListDownloadTrackAsync(
                new ERPPriceListDownloadTrack
                {
                    NopCustomerId = customer.Id,
                    B2BAccountId = erpAccount.Id,
                    B2BSalesOrganisationId = erpAccount.ErpSalesOrgId,
                    DownloadedOnUtc = DateTime.UtcNow,
                    PriceListDownloadType = PriceListDownloadType.Excel,
                }
            );

            var bytes = await _erpAccountPublicModelFactory.ExportB2BOrderPerAccountProductsToXlsxAsync();
            var file =  File(bytes, MimeTypes.TextXlsx, $"{await _localizationService.GetResourceAsync("B2B.PriceList.Excel.ExcelName")}.xlsx");

            return Json(new
            {
                file = file,
                fileName = file.FileDownloadName
            });
        }
        catch (Exception ex)
        {
            await _erpLogsService.ErrorAsync($"B2B Export All Product Excel: {ex.Message}", ErpSyncLevel.Product, ex);
            _notificationService.Notification(NotifyType.Error,
                await _localizationService.GetResourceAsync("Plugin.Misc.NopStation.B2BB2CFeatures.PriceListDownLoad.DownloadFailed"));

            return RedirectToRoute("ErpAccountInvoices");
        }
    }

    public async Task<IActionResult> ExportLastNOrdersPerAccountPdf()
    {
        var customer = await _workContext.GetCurrentCustomerAsync();
        var erpAccount = await _erpAccountService.GetActiveErpAccountByCustomerIdAsync(customer.Id);

        if (erpAccount == null || !await _permissionService.AuthorizeAsync(ErpPermissionProvider.DisplayB2BPrices))
        {
            _notificationService.Notification(NotifyType.Error,
                await _localizationService.GetResourceAsync("Plugin.Misc.NopStation.B2BB2CFeatures.PriceListDownLoad.AccessDenied"));
            return AccessDeniedView();
        }

        try
        {
            await _erpPriceListDownloadTrackService.InsertB2BPriceListDownloadTrackAsync(
                new ERPPriceListDownloadTrack
                {
                    NopCustomerId = customer.Id,
                    B2BAccountId = erpAccount.Id,
                    B2BSalesOrganisationId = erpAccount.ErpSalesOrgId,
                    DownloadedOnUtc = DateTime.UtcNow,
                    PriceListDownloadType = PriceListDownloadType.PDF,
                }
            );

            var bytes = await _erpAccountPublicModelFactory.PrintB2BOrderPerAccountProductsToPdfAsync(erpAccount);
            var file = File(bytes, MimeTypes.ApplicationPdf, $"{await _localizationService.GetResourceAsync("B2B.PriceList.Pdf.PdfName")}.pdf");

            return Json(new
            {
                file = file,
                fileName = file.FileDownloadName
            });
        }
        catch (Exception ex)
        {
            await _erpLogsService.ErrorAsync($"B2B Export All Product Pdf: {ex.Message}", ErpSyncLevel.Product, ex);
            _notificationService.Notification(NotifyType.Error,
                await _localizationService.GetResourceAsync("Plugin.Misc.NopStation.B2BB2CFeatures.PriceListDownLoad.DownloadFailed"));

            return RedirectToRoute("ErpAccountInvoices");
        }
    }

    public async Task<IActionResult> ExportAllProductExcel(int categoryId)
    {
        var customer = await _workContext.GetCurrentCustomerAsync();
        var erpAccount = await _erpAccountService.GetActiveErpAccountByCustomerIdAsync(customer.Id);

        if (erpAccount == null || !await _permissionService.AuthorizeAsync(ErpPermissionProvider.DisplayB2BPrices))
        {
            _notificationService.Notification(NotifyType.Error,
                await _localizationService.GetResourceAsync("Plugin.Misc.NopStation.B2BB2CFeatures.PriceListDownLoad.AccessDenied"));
            return AccessDeniedView();
        }

        try
        {
            await _erpPriceListDownloadTrackService.InsertB2BPriceListDownloadTrackAsync(
                new ERPPriceListDownloadTrack
                {
                    NopCustomerId = customer.Id,
                    B2BAccountId = erpAccount.Id,
                    B2BSalesOrganisationId = erpAccount.ErpSalesOrgId,
                    DownloadedOnUtc = DateTime.UtcNow,
                    PriceListDownloadType = PriceListDownloadType.Excel,
                }
            );

            var bytes = await _erpAccountPublicModelFactory.ExportB2BAccountProductsToXlsxAsync(categoryId);
            var file =  File(bytes, MimeTypes.TextXlsx, $"{await _localizationService.GetResourceAsync("B2B.PriceList.Excel.ExcelName")}.xlsx");

            return Json(new
            {
                file = file,
                fileName = file.FileDownloadName
            });
        }
        catch (Exception ex)
        {
            await _erpLogsService.ErrorAsync($"B2B Export All Product Excel: {ex.Message}", ErpSyncLevel.Product, ex);
            _notificationService.Notification(NotifyType.Error,
                await _localizationService.GetResourceAsync("Plugin.Misc.NopStation.B2BB2CFeatures.PriceListDownLoad.DownloadFailed"));

            return RedirectToRoute("ErpAccountInvoices");
        }
    }

    public async Task<IActionResult> ExportAllProductPdf(int categoryId)
    {
        var customer = await _workContext.GetCurrentCustomerAsync();
        var erpAccount = await _erpAccountService.GetActiveErpAccountByCustomerIdAsync(customer.Id);

        if (erpAccount == null || !await _permissionService.AuthorizeAsync(ErpPermissionProvider.DisplayB2BPrices))
        {
            _notificationService.Notification(NotifyType.Error,
                await _localizationService.GetResourceAsync("Plugin.Misc.NopStation.B2BB2CFeatures.PriceListDownLoad.AccessDenied"));
            return AccessDeniedView();
        }

        try
        {
            await _erpPriceListDownloadTrackService.InsertB2BPriceListDownloadTrackAsync(
                new ERPPriceListDownloadTrack
                {
                    NopCustomerId = customer.Id,
                    B2BAccountId = erpAccount.Id,
                    B2BSalesOrganisationId = erpAccount.ErpSalesOrgId,
                    DownloadedOnUtc = DateTime.UtcNow,
                    PriceListDownloadType = PriceListDownloadType.PDF,
                }
            );

            var bytes = await _erpAccountPublicModelFactory.PrintB2BAccountProductsToPdfAsync(erpAccount, categoryId);
            var file =  File(bytes, MimeTypes.ApplicationPdf, $"{await _localizationService.GetResourceAsync("B2B.PriceList.PDF.PdfName")}.pdf");

            return Json(new
            {
                file = file,
                fileName = file.FileDownloadName
            });
        }
        catch (Exception ex)
        {
            await _erpLogsService.ErrorAsync($"B2B Export All Product Excel: {ex.Message}", ErpSyncLevel.Product, ex);
            _notificationService.Notification(NotifyType.Error,
                await _localizationService.GetResourceAsync("Plugin.Misc.NopStation.B2BB2CFeatures.PriceListDownLoad.DownloadFailed"));

            return RedirectToRoute("ErpAccountInvoices");
        }
    }

    #endregion
}