using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using Nop.Core;
using Nop.Core.Caching;
using Nop.Core.Domain.Blogs;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Forums;
using Nop.Core.Domain.Media;
using Nop.Core.Domain.Seo;
using Nop.Core.Domain.Vendors;
using Nop.Core.Events;
using Nop.Services.Catalog;
using Nop.Services.Common;
using Nop.Services.Customers;
using Nop.Services.Directory;
using Nop.Services.Localization;
using Nop.Services.Media;
using Nop.Services.Seo;
using Nop.Services.Topics;
using Nop.Services.Vendors;
using Nop.Web.Factories;
using Nop.Web.Framework.Mvc.Routing;
using Nop.Web.Infrastructure.Cache;
using Nop.Web.Models.Catalog;
using Nop.Web.Models.Media;

namespace NopStation.Plugin.B2B.B2BB2CFeatures.Factories;

public class OverridenCatalogModelFactory : CatalogModelFactory
{
    private readonly IGenericAttributeService _genericAttributeService;

    public OverridenCatalogModelFactory(
        BlogSettings blogSettings,
        CatalogSettings catalogSettings,
        DisplayDefaultMenuItemSettings displayDefaultMenuItemSettings,
        ForumSettings forumSettings,
        ICategoryService categoryService,
        ICategoryTemplateService categoryTemplateService,
        ICurrencyService currencyService,
        ICustomerService customerService,
        IEventPublisher eventPublisher,
        IHttpContextAccessor httpContextAccessor,
        IJsonLdModelFactory jsonLdModelFactory,
        ILocalizationService localizationService,
        IManufacturerService manufacturerService,
        IManufacturerTemplateService manufacturerTemplateService,
        INopUrlHelper nopUrlHelper,
        IPictureService pictureService,
        IProductModelFactory productModelFactory,
        IProductService productService,
        IProductTagService productTagService,
        ISearchTermService searchTermService,
        ISpecificationAttributeService specificationAttributeService,
        IStaticCacheManager staticCacheManager,
        IStoreContext storeContext,
        ITopicService topicService,
        IUrlRecordService urlRecordService,
        IVendorService vendorService,
        IWebHelper webHelper,
        IWorkContext workContext,
        MediaSettings mediaSettings,
        SeoSettings seoSettings,
        VendorSettings vendorSettings,
        IGenericAttributeService genericAttributeService) : base(blogSettings,
            catalogSettings,
            displayDefaultMenuItemSettings,
            forumSettings,
            categoryService,
            categoryTemplateService,
            currencyService,
            customerService,
            eventPublisher,
            httpContextAccessor,
            jsonLdModelFactory,
            localizationService,
            manufacturerService,
            manufacturerTemplateService,
            nopUrlHelper,
            pictureService,
            productModelFactory,
            productService,
            productTagService,
            searchTermService,
            specificationAttributeService,
            staticCacheManager,
            storeContext,
            topicService,
            urlRecordService,
            vendorService,
            webHelper,
            workContext,
            mediaSettings,
            seoSettings,
            vendorSettings)
    {
        _genericAttributeService = genericAttributeService;
    }

    public override async Task<CategoryModel> PrepareCategoryModelAsync(Category category, CatalogProductsCommand command)
    {
        ArgumentNullException.ThrowIfNull(category);

        ArgumentNullException.ThrowIfNull(command);

        //For View Mode
        var categoryViewMode = await _genericAttributeService.GetAttributeAsync<string>(category, B2BB2CFeaturesDefaults.CategoryViewModeAttribute, 
            (await _storeContext.GetCurrentStoreAsync()).Id);

        if (string.IsNullOrEmpty(command.ViewMode) && !string.IsNullOrWhiteSpace(categoryViewMode))
            command.ViewMode = categoryViewMode;

        var model = new CategoryModel
        {
            Id = category.Id,
            Name = await _localizationService.GetLocalizedAsync(category, x => x.Name),
            Description = await _localizationService.GetLocalizedAsync(category, x => x.Description),
            MetaKeywords = await _localizationService.GetLocalizedAsync(category, x => x.MetaKeywords),
            MetaDescription = await _localizationService.GetLocalizedAsync(category, x => x.MetaDescription),
            MetaTitle = await _localizationService.GetLocalizedAsync(category, x => x.MetaTitle),
            SeName = await _urlRecordService.GetSeNameAsync(category),
            CatalogProductsModel = await PrepareCategoryProductsModelAsync(category, command)
        };
        //category breadcrumb
        if (_catalogSettings.CategoryBreadcrumbEnabled)
        {
            model.DisplayCategoryBreadcrumb = true;

            model.CategoryBreadcrumb = await (await _categoryService.GetCategoryBreadCrumbAsync(category)).SelectAwait(async catBr =>
                new CategoryModel
                {
                    Id = catBr.Id,
                    Name = await _localizationService.GetLocalizedAsync(catBr, x => x.Name),
                    SeName = await _urlRecordService.GetSeNameAsync(catBr)
                }).ToListAsync();

            if (_seoSettings.MicrodataEnabled)
            {
                var categoryBreadcrumb = model.CategoryBreadcrumb.Select(c => new CategorySimpleModel { Id = c.Id, Name = c.Name, SeName = c.SeName }).ToList();
                var jsonLdModel = await _jsonLdModelFactory.PrepareJsonLdCategoryBreadcrumbAsync(categoryBreadcrumb);
                model.JsonLd = JsonConvert
                    .SerializeObject(jsonLdModel, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
            }
        }

        var currentStore = await _storeContext.GetCurrentStoreAsync();
        var pictureSize = _mediaSettings.CategoryThumbPictureSize;

        //subcategories
        model.SubCategories = await (await _categoryService.GetAllCategoriesByParentCategoryIdAsync(category.Id))
            .SelectAwait(async curCategory =>
            {
                var subCatModel = new CategoryModel.SubCategoryModel
                {
                    Id = curCategory.Id,
                    Name = await _localizationService.GetLocalizedAsync(curCategory, y => y.Name),
                    SeName = await _urlRecordService.GetSeNameAsync(curCategory),
                    Description = await _localizationService.GetLocalizedAsync(curCategory, y => y.Description)
                };

                //prepare picture model
                var categoryPictureCacheKey = _staticCacheManager.PrepareKeyForDefaultCache(NopModelCacheDefaults.CategoryPictureModelKey, curCategory,
                    pictureSize, true, await _workContext.GetWorkingLanguageAsync(), _webHelper.IsCurrentConnectionSecured(),
                    currentStore);

                subCatModel.PictureModel = await _staticCacheManager.GetAsync(categoryPictureCacheKey, async () =>
                {
                    var picture = await _pictureService.GetPictureByIdAsync(curCategory.PictureId);
                    string fullSizeImageUrl, imageUrl;

                    (fullSizeImageUrl, picture) = await _pictureService.GetPictureUrlAsync(picture);
                    (imageUrl, _) = await _pictureService.GetPictureUrlAsync(picture, pictureSize);

                    var pictureModel = new PictureModel
                    {
                        FullSizeImageUrl = fullSizeImageUrl,
                        ImageUrl = imageUrl,
                        Title = string.Format(await _localizationService
                            .GetResourceAsync("Media.Category.ImageLinkTitleFormat"), subCatModel.Name),
                        AlternateText = string.Format(await _localizationService
                            .GetResourceAsync("Media.Category.ImageAlternateTextFormat"), subCatModel.Name)
                    };

                    return pictureModel;
                });

                return subCatModel;
            }).ToListAsync();

        //featured products
        if (!_catalogSettings.IgnoreFeaturedProducts)
        {
            var featuredProducts = await _productService.GetCategoryFeaturedProductsAsync(category.Id, currentStore.Id);
            if (featuredProducts != null)
                model.FeaturedProducts = (await _productModelFactory.PrepareProductOverviewModelsAsync(featuredProducts)).ToList();
        }

        //prepare picture model
        var mainCategoryPictureCacheKey = _staticCacheManager.PrepareKeyForDefaultCache(NopModelCacheDefaults.CategoryPictureModelKey, category,
                pictureSize, true, await _workContext.GetWorkingLanguageAsync(), _webHelper.IsCurrentConnectionSecured(),
                currentStore);

        model.PictureModel = await _staticCacheManager.GetAsync(mainCategoryPictureCacheKey, async () =>
        {
            var picture = await _pictureService.GetPictureByIdAsync(category.PictureId);
            string fullSizeImageUrl, imageUrl;

            (fullSizeImageUrl, picture) = await _pictureService.GetPictureUrlAsync(picture);
            (imageUrl, _) = await _pictureService.GetPictureUrlAsync(picture, pictureSize);

            var pictureModel = new PictureModel
            {
                FullSizeImageUrl = fullSizeImageUrl,
                ImageUrl = imageUrl,
                Title = string.Format(await _localizationService.GetResourceAsync("Media.Category.ImageLinkTitleFormat"), model.Name),
                AlternateText = string.Format(await _localizationService.GetResourceAsync("Media.Category.ImageAlternateTextFormat"), model.Name)
            };

            return pictureModel;
        });

        return model;
    }
}
