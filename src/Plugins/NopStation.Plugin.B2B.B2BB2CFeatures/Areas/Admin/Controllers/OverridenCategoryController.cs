using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Core;
using Nop.Core.Caching;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Discounts;
using Nop.Services.Catalog;
using Nop.Services.Common;
using Nop.Services.Customers;
using Nop.Services.Discounts;
using Nop.Services.ExportImport;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Media;
using Nop.Services.Messages;
using Nop.Services.Security;
using Nop.Services.Seo;
using Nop.Services.Stores;
using Nop.Web.Areas.Admin.Controllers;
using Nop.Web.Areas.Admin.Factories;
using Nop.Web.Areas.Admin.Infrastructure.Mapper.Extensions;
using Nop.Web.Areas.Admin.Models.Catalog;
using Nop.Web.Framework.Mvc.Filters;

namespace NopStation.Plugin.B2B.B2BB2CFeatures.Areas.Admin.Controllers;

public class OverridenCategoryController : CategoryController
{
    private readonly CatalogSettings _catalogSettings;
    private readonly IGenericAttributeService _genericAttributeService;
    private readonly IStoreContext _storeContext;

    public OverridenCategoryController(IAclService aclService,
        ICategoryModelFactory categoryModelFactory,
        ICategoryService categoryService,
        ICustomerActivityService customerActivityService,
        ICustomerService customerService,
        IDiscountService discountService,
        IExportManager exportManager,
        IImportManager importManager,
        ILocalizationService localizationService,
        ILocalizedEntityService localizedEntityService,
        INotificationService notificationService,
        IPermissionService permissionService,
        IPictureService pictureService,
        IProductService productService,
        IStaticCacheManager staticCacheManager,
        IStoreMappingService storeMappingService,
        IStoreService storeService,
        IUrlRecordService urlRecordService,
        IWorkContext workContext,
        CatalogSettings catalogSettings,
        IGenericAttributeService genericAttributeService,
        IStoreContext storeContext) : base(aclService,
            categoryModelFactory,
            categoryService,
            customerActivityService,
            customerService,
            discountService,
            exportManager,
            importManager,
            localizationService,
            localizedEntityService,
            notificationService,
            permissionService,
            pictureService,
            productService,
            staticCacheManager,
            storeMappingService,
            storeService,
            urlRecordService,
            workContext)
    {
        _catalogSettings = catalogSettings;
        _genericAttributeService = genericAttributeService;
        _storeContext = storeContext;
    }

    #region Create / Edit / Delete

    public override async Task<IActionResult> Create()
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageCategories))
            return AccessDeniedView();

        //prepare model
        var model = await _categoryModelFactory.PrepareCategoryModelAsync(
            new CategoryModel(),
            null
        );

        #region View mode

        var availableViewModes = new List<SelectListItem>
        {
            new() 
            {
                Text = await _localizationService.GetResourceAsync("Admin.Catalog.ViewMode.Grid"),
                Value = "grid",
            },
            new ()
            {
                Text = await _localizationService.GetResourceAsync("Admin.Catalog.ViewMode.List"),
                Value = "list",
            },
        };

        // Assign the list to ViewBag
        ViewBag.CategoryViewMode = _catalogSettings.DefaultViewMode;
        ViewBag.AvailableViewModes = new SelectList(
            availableViewModes,
            "Value",
            "Text",
            ViewBag.CategoryViewMode
        );

        #endregion

        return View(model);
    }

    [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
    public override async Task<IActionResult> Create(CategoryModel model, bool continueEditing)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageCategories))
            return AccessDeniedView();

        if (ModelState.IsValid)
        {
            var category = model.ToEntity<Category>();
            category.CreatedOnUtc = DateTime.UtcNow;
            category.UpdatedOnUtc = DateTime.UtcNow;
            await _categoryService.InsertCategoryAsync(category);

            // View mode
            var viewmode = Request.Form["CategoryViewMode"].ToString() ?? _catalogSettings.DefaultViewMode;

            var storeId = await _storeContext.GetActiveStoreScopeConfigurationAsync();
            if (storeId == 0)
                storeId = (await _storeContext.GetCurrentStoreAsync()).Id;

            await _genericAttributeService.SaveAttributeAsync(
                category,
                B2BB2CFeaturesDefaults.CategoryViewModeAttribute,
                viewmode,
                storeId
            );

            //search engine name
            model.SeName = await _urlRecordService.ValidateSeNameAsync(
                category,
                model.SeName,
                category.Name,
                true
            );
            await _urlRecordService.SaveSlugAsync(category, model.SeName, 0);

            //locales
            await UpdateLocalesAsync(category, model);

            //discounts
            var allDiscounts = await _discountService.GetAllDiscountsAsync(
                DiscountType.AssignedToCategories,
                showHidden: true,
                isActive: null
            );
            foreach (var discount in allDiscounts)
            {
                if (
                    model.SelectedDiscountIds != null
                    && model.SelectedDiscountIds.Contains(discount.Id)
                )
                    await _categoryService.InsertDiscountCategoryMappingAsync(
                        new DiscountCategoryMapping
                        {
                            DiscountId = discount.Id,
                            EntityId = category.Id,
                        }
                    );
            }

            await _categoryService.UpdateCategoryAsync(category);

            //update picture seo file name
            await UpdatePictureSeoNamesAsync(category);

            //ACL (customer roles)
            await SaveCategoryAclAsync(category, model);

            //stores
            await SaveStoreMappingsAsync(category, model);

            //activity log
            await _customerActivityService.InsertActivityAsync(
                "AddNewCategory",
                string.Format(
                    await _localizationService.GetResourceAsync("ActivityLog.AddNewCategory"),
                    category.Name
                ),
                category
            );

            _notificationService.SuccessNotification(
                await _localizationService.GetResourceAsync("Admin.Catalog.Categories.Added")
            );

            if (!continueEditing)
                return RedirectToAction("List");

            return RedirectToAction("Edit", new { id = category.Id });
        }

        //prepare model
        model = await _categoryModelFactory.PrepareCategoryModelAsync(model, null, true);

        //if we got this far, something failed, redisplay form
        return View(model);
    }

    public override async Task<IActionResult> Edit(int id)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageCategories))
            return AccessDeniedView();

        //try to get a category with the specified id
        var category = await _categoryService.GetCategoryByIdAsync(id);
        if (category == null || category.Deleted)
            return RedirectToAction("List");

        //prepare model
        var model = await _categoryModelFactory.PrepareCategoryModelAsync(null, category);

        #region View mode

        var availableViewModes = new List<SelectListItem>
        {
            new ()
            {
                Text = await _localizationService.GetResourceAsync("Admin.Catalog.ViewMode.Grid"),
                Value = "grid",
            },
            new ()
            {
                Text = await _localizationService.GetResourceAsync("Admin.Catalog.ViewMode.List"),
                Value = "list",
            },
        };

        var storeId = await _storeContext.GetActiveStoreScopeConfigurationAsync();
        if (storeId == 0)
            storeId = (await _storeContext.GetCurrentStoreAsync()).Id;

        // Assign the list to ViewBag
        var viewMode = await _genericAttributeService.GetAttributeAsync<string>(
            category,
            B2BB2CFeaturesDefaults.CategoryViewModeAttribute,
            storeId
        );

        ViewBag.CategoryViewMode = string.IsNullOrWhiteSpace(viewMode)
            ? _catalogSettings.DefaultViewMode
            : viewMode;

        ViewBag.AvailableViewModes = new SelectList(
            availableViewModes,
            "Value",
            "Text",
            ViewBag.CategoryViewMode
        );

        #endregion

        return View(model);
    }

    [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
    public override async Task<IActionResult> Edit(CategoryModel model, bool continueEditing)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageCategories))
            return AccessDeniedView();

        //try to get a category with the specified id
        var category = await _categoryService.GetCategoryByIdAsync(model.Id);
        if (category == null || category.Deleted)
            return RedirectToAction("List");

        if (ModelState.IsValid)
        {
            var prevPictureId = category.PictureId;

            //if parent category changes, we need to clear cache for previous parent category
            if (category.ParentCategoryId != model.ParentCategoryId)
            {
                await _staticCacheManager.RemoveByPrefixAsync(
                    NopCatalogDefaults.CategoriesByParentCategoryPrefix,
                    category.ParentCategoryId
                );
                await _staticCacheManager.RemoveByPrefixAsync(
                    NopCatalogDefaults.CategoriesChildIdsPrefix,
                    category.ParentCategoryId
                );
                await _staticCacheManager.RemoveByPrefixAsync(
                    NopCatalogDefaults.ChildCategoryIdLookupPrefix
                );
            }

            category = model.ToEntity(category);
            category.UpdatedOnUtc = DateTime.UtcNow;
            await _categoryService.UpdateCategoryAsync(category);

            //search engine name
            model.SeName = await _urlRecordService.ValidateSeNameAsync(
                category,
                model.SeName,
                category.Name,
                true
            );
            await _urlRecordService.SaveSlugAsync(category, model.SeName, 0);

            //locales
            await UpdateLocalesAsync(category, model);

            //discounts
            var allDiscounts = await _discountService.GetAllDiscountsAsync(
                DiscountType.AssignedToCategories,
                showHidden: true,
                isActive: null
            );
            foreach (var discount in allDiscounts)
            {
                if (
                    model.SelectedDiscountIds != null
                    && model.SelectedDiscountIds.Contains(discount.Id)
                )
                {
                    //new discount
                    if (
                        await _categoryService.GetDiscountAppliedToCategoryAsync(
                            category.Id,
                            discount.Id
                        )
                        is null
                    )
                        await _categoryService.InsertDiscountCategoryMappingAsync(
                            new DiscountCategoryMapping
                            {
                                DiscountId = discount.Id,
                                EntityId = category.Id,
                            }
                        );
                }
                else
                {
                    //remove discount
                    if (
                        await _categoryService.GetDiscountAppliedToCategoryAsync(
                            category.Id,
                            discount.Id
                        )
                        is DiscountCategoryMapping mapping
                    )
                        await _categoryService.DeleteDiscountCategoryMappingAsync(mapping);
                }
            }

            await _categoryService.UpdateCategoryAsync(category);

            //delete an old picture (if deleted or updated)
            if (prevPictureId > 0 && prevPictureId != category.PictureId)
            {
                var prevPicture = await _pictureService.GetPictureByIdAsync(prevPictureId);
                if (prevPicture != null)
                    await _pictureService.DeletePictureAsync(prevPicture);
            }

            //update picture seo file name
            await UpdatePictureSeoNamesAsync(category);

            //ACL
            await SaveCategoryAclAsync(category, model);

            //stores
            await SaveStoreMappingsAsync(category, model);

            //View mode
            var viewmode = Request.Form["CategoryViewMode"].ToString() ?? _catalogSettings.DefaultViewMode;

            var storeId = await _storeContext.GetActiveStoreScopeConfigurationAsync();
            if (storeId == 0)
                storeId = (await _storeContext.GetCurrentStoreAsync()).Id;

            await _genericAttributeService.SaveAttributeAsync(
                category,
                B2BB2CFeaturesDefaults.CategoryViewModeAttribute,
                viewmode,
                storeId
            );

            //activity log
            await _customerActivityService.InsertActivityAsync(
                "EditCategory",
                string.Format(
                    await _localizationService.GetResourceAsync("ActivityLog.EditCategory"),
                    category.Name
                ),
                category
            );

            _notificationService.SuccessNotification(
                await _localizationService.GetResourceAsync("Admin.Catalog.Categories.Updated")
            );

            if (!continueEditing)
                return RedirectToAction("List");

            return RedirectToAction("Edit", new { id = category.Id });
        }

        //prepare model
        model = await _categoryModelFactory.PrepareCategoryModelAsync(model, category, true);

        //if we got this far, something failed, redisplay form
        return View(model);
    }

    [HttpPost]
    public override async Task<IActionResult> Delete(int id)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageCategories))
            return AccessDeniedView();

        // Try to get a category with the specified id
        var category = await _categoryService.GetCategoryByIdAsync(id);
        if (category == null)
            return RedirectToAction("List");

        await _categoryService.DeleteCategoryAsync(category);

        var storeId = await _storeContext.GetActiveStoreScopeConfigurationAsync();
        if (storeId == 0)
            storeId = (await _storeContext.GetCurrentStoreAsync()).Id;

        // View mode
        await _genericAttributeService.SaveAttributeAsync<string>(
            category,
            B2BB2CFeaturesDefaults.CategoryViewModeAttribute,
            null,
            storeId
        );

        // Activity log
        await _customerActivityService.InsertActivityAsync(
            "DeleteCategory",
            string.Format(
                await _localizationService.GetResourceAsync("ActivityLog.DeleteCategory"),
                category.Name
            ),
            category
        );

        _notificationService.SuccessNotification(
            await _localizationService.GetResourceAsync("Admin.Catalog.Categories.Deleted")
        );

        return RedirectToAction("List");
    }

    [HttpPost]
    public override async Task<IActionResult> DeleteSelected(ICollection<int> selectedIds)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageCategories))
            return await AccessDeniedDataTablesJson();

        if (selectedIds == null || !selectedIds.Any())
            return NoContent();

        var categoriesToDelete = await _categoryService.GetCategoriesByIdsAsync(selectedIds.ToArray());
        var currentVendor = await _workContext.GetCurrentVendorAsync();

        var categoriesToDeleteFiltered = categoriesToDelete.Where(c => currentVendor == null).ToList();
        await _categoryService.DeleteCategoriesAsync(categoriesToDeleteFiltered);

        var storeId = await _storeContext.GetActiveStoreScopeConfigurationAsync();
        if (storeId == 0)
            storeId = (await _storeContext.GetCurrentStoreAsync()).Id;

        foreach (var category in categoriesToDeleteFiltered)
        {
            await _genericAttributeService.SaveAttributeAsync<string>(
                category,
                B2BB2CFeaturesDefaults.CategoryViewModeAttribute,
                null,
                storeId
            );
        }

        return Json(new { Result = true });
    }

    #endregion
}
