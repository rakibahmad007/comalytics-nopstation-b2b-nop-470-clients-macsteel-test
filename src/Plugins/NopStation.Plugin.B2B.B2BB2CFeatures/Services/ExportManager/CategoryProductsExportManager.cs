using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Nop.Core;
using Nop.Core.Caching;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Directory;
using Nop.Core.Domain.Discounts;
using Nop.Core.Domain.Localization;
using Nop.Core.Domain.Seo;
using Nop.Core.Domain.Shipping;
using Nop.Data;
using Nop.Services.Catalog;
using Nop.Services.Common;
using Nop.Services.Customers;
using Nop.Services.Directory;
using Nop.Services.ExportImport;
using Nop.Services.ExportImport.Help;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Security;
using Nop.Services.Seo;
using Nop.Services.Stores;
using Nop.Web.Framework;
using NopStation.Plugin.B2B.B2BB2CFeatures.Services.ErpCustomerFunctionality;
using NopStation.Plugin.B2B.B2BB2CFeatures.Services.ErpSpecificationAttributeService;
using NopStation.Plugin.B2B.ERPIntegrationCore.Services;
using QuestPDF.Fluent;
using QuestPDF.Helpers;

namespace NopStation.Plugin.B2B.B2BB2CFeatures.Services.ExportManager;

public class CategoryProductsExportManager : ICategoryProductsExportManager
{
    #region Fields

    protected readonly IAclService _aclService;
    protected readonly ICustomerService _customerService;
    protected readonly IProductAttributeParser _productAttributeParser;
    protected readonly IRepository<Category> _categoryRepository;
    protected readonly IRepository<CrossSellProduct> _crossSellProductRepository;
    protected readonly IRepository<DiscountProductMapping> _discountProductMappingRepository;
    protected readonly IRepository<LocalizedProperty> _localizedPropertyRepository;
    protected readonly IRepository<Manufacturer> _manufacturerRepository;
    protected readonly IRepository<Product> _productRepository;
    protected readonly IRepository<ProductAttributeCombination> _productAttributeCombinationRepository;
    protected readonly IRepository<ProductAttributeMapping> _productAttributeMappingRepository;
    protected readonly IRepository<ProductCategory> _productCategoryRepository;
    protected readonly IRepository<ProductManufacturer> _productManufacturerRepository;
    protected readonly IRepository<ProductPicture> _productPictureRepository;
    protected readonly IRepository<ProductProductTagMapping> _productTagMappingRepository;
    protected readonly IRepository<ProductReview> _productReviewRepository;
    protected readonly IRepository<ProductReviewHelpfulness> _productReviewHelpfulnessRepository;
    protected readonly IRepository<ProductSpecificationAttribute> _productSpecificationAttributeRepository;
    protected readonly IRepository<ProductTag> _productTagRepository;
    protected readonly IRepository<ProductVideo> _productVideoRepository;
    protected readonly IRepository<ProductWarehouseInventory> _productWarehouseInventoryRepository;
    protected readonly IRepository<RelatedProduct> _relatedProductRepository;
    protected readonly IRepository<Shipment> _shipmentRepository;
    protected readonly IRepository<StockQuantityHistory> _stockQuantityHistoryRepository;
    protected readonly IRepository<TierPrice> _tierPriceRepository;
    protected readonly ISearchPluginManager _searchPluginManager;
    protected readonly IStaticCacheManager _staticCacheManager;
    protected readonly LocalizationSettings _localizationSettings;

    private readonly ICategoryService _categoryService;
    private readonly IProductService _productService;
    private readonly ILocalizationService _localizationService;
    private readonly ICustomerActivityService _customerActivityService;
    private readonly ILanguageService _languageService;
    private readonly IWorkContext _workContext;
    private readonly IGenericAttributeService _genericAttributeService;
    private readonly ProductEditorSettings _productEditorSettings;
    private readonly ILocalizedEntityService _localizedEntityService;
    private readonly IUrlRecordService _urlRecordService;
    private readonly ISpecificationAttributeService _specificationAttributeService;
    private readonly IManufacturerService _manufacturerService;
    private readonly CatalogSettings _catalogSettings;
    private readonly IMeasureService _measureService;
    private readonly IStoreService _storeService;

    private readonly IActionContextAccessor _actionContextAccessor;
    private readonly IErpAccountService _erpAccountService;
    private readonly B2BB2CFeaturesSettings _b2BB2CFeaturesSettings;
    private readonly IErpSpecificationAttributeService _erpSpecificationAttributeService;
    private readonly IStoreContext _storeContext;
    private readonly IErpCustomerFunctionalityService _erpCustomerFunctionalityService;
    private readonly MeasureSettings _measureSettings;
    private readonly IPriceCalculationService _priceCalculationService;
    private readonly IPriceFormatter _priceFormatter;
    private readonly ICurrencyService _currencyService;
    private readonly PdfSettings _pdfSettings;
    private readonly IStoreMappingService _storeMappingService;

    #endregion Fields

    #region Ctor

    public CategoryProductsExportManager(
        ICategoryService categoryService,
        IProductService productService,
        ICustomerActivityService customerActivityService,
        ILocalizationService localizationService,
        ILanguageService languageService,
        IWorkContext workContext,
        IGenericAttributeService genericAttributeService,
        ProductEditorSettings productEditorSettings,
        ILocalizedEntityService localizedEntityService,
        IUrlRecordService urlRecordService,
        ISpecificationAttributeService specificationAttributeService,
        IProductAttributeService productAttributeService,
        IManufacturerService manufacturerService,
        CatalogSettings catalogSettings,
        IMeasureService measureService,
        IStoreService storeService,
        IAclService aclService,
        ICustomerService customerService,
        IProductAttributeParser productAttributeParser,
        IRepository<Category> categoryRepository,
        IRepository<CrossSellProduct> crossSellProductRepository,
        IRepository<DiscountProductMapping> discountProductMappingRepository,
        IRepository<LocalizedProperty> localizedPropertyRepository,
        IRepository<Manufacturer> manufacturerRepository,
        IRepository<Product> productRepository,
        IRepository<ProductAttributeCombination> productAttributeCombinationRepository,
        IRepository<ProductAttributeMapping> productAttributeMappingRepository,
        IRepository<ProductCategory> productCategoryRepository,
        IRepository<ProductManufacturer> productManufacturerRepository,
        IRepository<ProductPicture> productPictureRepository,
        IRepository<ProductProductTagMapping> productTagMappingRepository,
        IRepository<ProductReview> productReviewRepository,
        IRepository<ProductReviewHelpfulness> productReviewHelpfulnessRepository,
        IRepository<ProductSpecificationAttribute> productSpecificationAttributeRepository,
        IRepository<ProductTag> productTagRepository,
        IRepository<ProductVideo> productVideoRepository,
        IRepository<ProductWarehouseInventory> productWarehouseInventoryRepository,
        IRepository<RelatedProduct> relatedProductRepository,
        IRepository<Shipment> shipmentRepository,
        IRepository<StockQuantityHistory> stockQuantityHistoryRepository,
        IRepository<TierPrice> tierPriceRepository,
        ISearchPluginManager searchPluginManager,
        IStaticCacheManager staticCacheManager,
        LocalizationSettings localizationSettings,
        IActionContextAccessor actionContextAccessor,
        IErpAccountService erpAccountService,
        B2BB2CFeaturesSettings b2BB2CFeaturesSettings,
        IErpSpecificationAttributeService erpSpecificationAttributeService,
        IStoreContext storeContext,
        IErpCustomerFunctionalityService erpCustomerFunctionalityService,
        MeasureSettings measureSettings,
        IPriceCalculationService priceCalculationService,
        IPriceFormatter priceFormatter,
        ICurrencyService currencyService,
        PdfSettings pdfSettings,
        IStoreMappingService storeMappingService)
    {
        _categoryService = categoryService;
        _productService = productService;
        _customerActivityService = customerActivityService;
        _localizationService = localizationService;
        _languageService = languageService;
        _workContext = workContext;
        _genericAttributeService = genericAttributeService;
        _productEditorSettings = productEditorSettings;
        _localizedEntityService = localizedEntityService;
        _urlRecordService = urlRecordService;
        _specificationAttributeService = specificationAttributeService;
        _manufacturerService = manufacturerService;
        _catalogSettings = catalogSettings;
        _measureService = measureService;
        _storeService = storeService;
        _aclService = aclService;
        _customerService = customerService;
        _productAttributeParser = productAttributeParser;
        _categoryRepository = categoryRepository;
        _crossSellProductRepository = crossSellProductRepository;
        _discountProductMappingRepository = discountProductMappingRepository;
        _localizedPropertyRepository = localizedPropertyRepository;
        _manufacturerRepository = manufacturerRepository;
        _productRepository = productRepository;
        _productAttributeCombinationRepository = productAttributeCombinationRepository;
        _productAttributeMappingRepository = productAttributeMappingRepository;
        _productCategoryRepository = productCategoryRepository;
        _productManufacturerRepository = productManufacturerRepository;
        _productPictureRepository = productPictureRepository;
        _productTagMappingRepository = productTagMappingRepository;
        _productReviewRepository = productReviewRepository;
        _productReviewHelpfulnessRepository = productReviewHelpfulnessRepository;
        _productSpecificationAttributeRepository = productSpecificationAttributeRepository;
        _productTagRepository = productTagRepository;
        _productVideoRepository = productVideoRepository;
        _productWarehouseInventoryRepository = productWarehouseInventoryRepository;
        _relatedProductRepository = relatedProductRepository;
        _shipmentRepository = shipmentRepository;
        _stockQuantityHistoryRepository = stockQuantityHistoryRepository;
        _tierPriceRepository = tierPriceRepository;
        _searchPluginManager = searchPluginManager;
        _staticCacheManager = staticCacheManager;
        _localizationSettings = localizationSettings;
        _actionContextAccessor = actionContextAccessor;
        _erpAccountService = erpAccountService;
        _b2BB2CFeaturesSettings = b2BB2CFeaturesSettings;
        _erpSpecificationAttributeService = erpSpecificationAttributeService;
        _storeContext = storeContext;
        _erpCustomerFunctionalityService = erpCustomerFunctionalityService;
        _measureSettings = measureSettings;
        _priceCalculationService = priceCalculationService;
        _priceFormatter = priceFormatter;
        _currencyService = currencyService;
        _pdfSettings = pdfSettings;
        _storeMappingService = storeMappingService;
    }

    #endregion Ctor

    #region Utilites

    private bool IsAdminRoute()
    {
        var haveArea = _actionContextAccessor.ActionContext.RouteData.Values.TryGetValue(
            "area",
            out var data
        );
        if (haveArea && data.ToString().Equals(AreaNames.ADMIN))
            return true;

        return false;
    }

    protected virtual async Task<int> WriteCategoriesAsync(
        XmlWriter xmlWriter,
        int parentCategoryId,
        int totalCategories
    )
    {
        var categories = await _categoryService.GetAllCategoriesByParentCategoryIdAsync(
            parentCategoryId,
            true
        );
        if (categories == null || !categories.Any())
            return totalCategories;

        totalCategories += categories.Count;

        var languages = await _languageService.GetAllLanguagesAsync(showHidden: true);

        foreach (var category in categories)
        {
            await xmlWriter.WriteStartElementAsync("Category");

            await xmlWriter.WriteStringAsync("Id", category.Id);

            await WriteLocalizedPropertyXmlAsync(category, c => c.Name, xmlWriter, languages);
            await WriteLocalizedPropertyXmlAsync(
                category,
                c => c.Description,
                xmlWriter,
                languages
            );
            await xmlWriter.WriteStringAsync("CategoryTemplateId", category.CategoryTemplateId);
            await WriteLocalizedPropertyXmlAsync(
                category,
                c => c.MetaKeywords,
                xmlWriter,
                languages,
                await IgnoreExportCategoryPropertyAsync()
            );
            await WriteLocalizedPropertyXmlAsync(
                category,
                c => c.MetaDescription,
                xmlWriter,
                languages,
                await IgnoreExportCategoryPropertyAsync()
            );
            await WriteLocalizedPropertyXmlAsync(
                category,
                c => c.MetaTitle,
                xmlWriter,
                languages,
                await IgnoreExportCategoryPropertyAsync()
            );
            await WriteLocalizedSeNameXmlAsync(
                category,
                xmlWriter,
                languages,
                await IgnoreExportCategoryPropertyAsync()
            );
            await xmlWriter.WriteStringAsync("ParentCategoryId", category.ParentCategoryId);
            await xmlWriter.WriteStringAsync("PictureId", category.PictureId);
            await xmlWriter.WriteStringAsync(
                "PageSize",
                category.PageSize,
                await IgnoreExportCategoryPropertyAsync()
            );
            await xmlWriter.WriteStringAsync(
                "AllowCustomersToSelectPageSize",
                category.AllowCustomersToSelectPageSize,
                await IgnoreExportCategoryPropertyAsync()
            );
            await xmlWriter.WriteStringAsync(
                "PageSizeOptions",
                category.PageSizeOptions,
                await IgnoreExportCategoryPropertyAsync()
            );
            await xmlWriter.WriteStringAsync(
                "PriceRangeFiltering",
                category.PriceRangeFiltering,
                await IgnoreExportCategoryPropertyAsync()
            );
            await xmlWriter.WriteStringAsync(
                "PriceFrom",
                category.PriceFrom,
                await IgnoreExportCategoryPropertyAsync()
            );
            await xmlWriter.WriteStringAsync(
                "PriceTo",
                category.PriceTo,
                await IgnoreExportCategoryPropertyAsync()
            );
            await xmlWriter.WriteStringAsync(
                "ManuallyPriceRange",
                category.ManuallyPriceRange,
                await IgnoreExportCategoryPropertyAsync()
            );
            await xmlWriter.WriteStringAsync(
                "ShowOnHomepage",
                category.ShowOnHomepage,
                await IgnoreExportCategoryPropertyAsync()
            );
            await xmlWriter.WriteStringAsync(
                "IncludeInTopMenu",
                category.IncludeInTopMenu,
                await IgnoreExportCategoryPropertyAsync()
            );
            await xmlWriter.WriteStringAsync(
                "Published",
                category.Published,
                await IgnoreExportCategoryPropertyAsync()
            );
            await xmlWriter.WriteStringAsync("Deleted", category.Deleted, true);
            await xmlWriter.WriteStringAsync("DisplayOrder", category.DisplayOrder);
            await xmlWriter.WriteStringAsync(
                "CreatedOnUtc",
                category.CreatedOnUtc,
                await IgnoreExportCategoryPropertyAsync()
            );
            await xmlWriter.WriteStringAsync(
                "UpdatedOnUtc",
                category.UpdatedOnUtc,
                await IgnoreExportCategoryPropertyAsync()
            );

            await xmlWriter.WriteStartElementAsync("Products");
            var productCategories =
                await _categoryService.GetProductCategoriesByCategoryIdAsync(
                    category.Id,
                    showHidden: true
                );
            foreach (var productCategory in productCategories)
            {
                var product = await _productService.GetProductByIdAsync(
                    productCategory.ProductId
                );
                if (product == null || product.Deleted)
                    continue;

                await xmlWriter.WriteStartElementAsync("ProductCategory");
                await xmlWriter.WriteStringAsync("ProductCategoryId", productCategory.Id);
                await xmlWriter.WriteStringAsync("ProductId", productCategory.ProductId);
                await WriteLocalizedPropertyXmlAsync(
                    product,
                    p => p.Name,
                    xmlWriter,
                    languages,
                    overriddenNodeName: "ProductName"
                );
                await xmlWriter.WriteStringAsync(
                    "IsFeaturedProduct",
                    productCategory.IsFeaturedProduct
                );
                await xmlWriter.WriteStringAsync("DisplayOrder", productCategory.DisplayOrder);
                await xmlWriter.WriteEndElementAsync();
            }

            await xmlWriter.WriteEndElementAsync();

            await xmlWriter.WriteStartElementAsync("SubCategories");
            totalCategories = await WriteCategoriesAsync(
                xmlWriter,
                category.Id,
                totalCategories
            );
            await xmlWriter.WriteEndElementAsync();
            await xmlWriter.WriteEndElementAsync();
        }

        return totalCategories;
    }

    protected virtual async Task<object> GetCategoriesAsync(Product product)
    {
        var categoryNames = new StringBuilder();
        foreach (
            var pc in await _categoryService.GetProductCategoriesByProductIdAsync(
                product.Id,
                true
            )
        )
        {
            if (_catalogSettings.ExportImportRelatedEntitiesByName)
            {
                var category = await _categoryService.GetCategoryByIdAsync(pc.CategoryId);
                categoryNames.Append(_catalogSettings.ExportImportProductCategoryBreadcrumb
                    ? await _categoryService.GetFormattedBreadCrumbAsync(category)
                    : category.Name);
            }
            else
            {
                categoryNames.Append(pc.CategoryId.ToString());
            }
            categoryNames.Append(";");
        }
        return categoryNames.ToString();
    }

    protected virtual async Task<object> GetManufacturersAsync(Product product)
    {
        var manufacturerNames = new StringBuilder();
        foreach (
            var pm in await _manufacturerService.GetProductManufacturersByProductIdAsync(
                product.Id,
                true
            )
        )
        {
            if (_catalogSettings.ExportImportRelatedEntitiesByName)
            {
                var manufacturer = await _manufacturerService.GetManufacturerByIdAsync(
                    pm.ManufacturerId
                );
                manufacturerNames.Append(manufacturer.Name);
            }
            else
            {
                manufacturerNames.Append(pm.ManufacturerId.ToString());
            }
            manufacturerNames.Append(";");
        }
        return manufacturerNames.ToString();
    }

    protected virtual async Task<object> GetLimitedToStoresAsync(Product product)
    {
        var limitedToStores = new StringBuilder();
        foreach (var storeMapping in await _storeMappingService.GetStoreMappingsAsync(product))
        {
            var store = await _storeService.GetStoreByIdAsync(storeMapping.StoreId);
            limitedToStores.Append(_catalogSettings.ExportImportRelatedEntitiesByName
                ? store.Name
                : store.Id.ToString());
            limitedToStores.Append(";");
        }
        return limitedToStores.ToString();
    }

    protected virtual async Task<bool> IgnoreExportProductPropertyAsync(
        Func<ProductEditorSettings, bool> func
    )
    {
        var productAdvancedMode = true;
        try
        {
            productAdvancedMode = await _genericAttributeService.GetAttributeAsync<bool>(
                await _workContext.GetCurrentCustomerAsync(),
                "product-advanced-mode"
            );
        }
        catch (ArgumentNullException) { }

        return !productAdvancedMode && !func(_productEditorSettings);
    }

    protected virtual async Task<bool> IgnoreExportCategoryPropertyAsync()
    {
        try
        {
            return !await _genericAttributeService.GetAttributeAsync<bool>(
                await _workContext.GetCurrentCustomerAsync(),
                "category-advanced-mode"
            );
        }
        catch (ArgumentNullException)
        {
            return false;
        }
    }

    protected virtual async Task<bool> IgnoreExportLimitedToStoreAsync()
    {
        return _catalogSettings.IgnoreStoreLimitations
            || !_catalogSettings.ExportImportProductUseLimitedToStores
            || (await _storeService.GetAllStoresAsync()).Count == 1;
    }

    private async Task<TProperty> GetLocalizedAsync<TEntity, TProperty>(
        TEntity entity,
        Expression<Func<TEntity, TProperty>> keySelector,
        Language language
    )
        where TEntity : BaseEntity, ILocalizedEntity
    {
        if (entity == null)
            return default;

        return await _localizationService.GetLocalizedAsync(
            entity,
            keySelector,
            language.Id,
            false
        );
    }

    private async Task WriteLocalizedSeNameXmlAsync<TEntity>(
        TEntity entity,
        XmlWriter xmlWriter,
        IList<Language> languages,
        bool ignore = false,
        string overriddenNodeName = null
    )
        where TEntity : BaseEntity, ISlugSupported
    {
        if (ignore)
            return;

        ArgumentNullException.ThrowIfNull(entity);

        var nodeName = "SEName";
        if (!string.IsNullOrWhiteSpace(overriddenNodeName))
            nodeName = overriddenNodeName;

        await xmlWriter.WriteStartElementAsync(nodeName);
        await xmlWriter.WriteStringAsync(
            "Standard",
            await _urlRecordService.GetSeNameAsync(entity, 0)
        );

        if (languages.Count >= 2)
        {
            await xmlWriter.WriteStartElementAsync("Locales");

            foreach (var language in languages)
                if (
                    await _urlRecordService.GetSeNameAsync(
                        entity,
                        language.Id,
                        returnDefaultValue: false
                    )
                        is string seName
                    && !string.IsNullOrWhiteSpace(seName)
                )
                    await xmlWriter.WriteStringAsync(language.UniqueSeoCode, seName);

            await xmlWriter.WriteEndElementAsync();
        }

        await xmlWriter.WriteEndElementAsync();
    }

    private async Task WriteLocalizedPropertyXmlAsync<TEntity, TPropType>(
        TEntity entity,
        Expression<Func<TEntity, TPropType>> keySelector,
        XmlWriter xmlWriter,
        IList<Language> languages,
        bool ignore = false,
        string overriddenNodeName = null
    )
        where TEntity : BaseEntity, ILocalizedEntity
    {
        if (ignore)
            return;

        ArgumentNullException.ThrowIfNull(entity);

        if (keySelector.Body is not MemberExpression member)
            throw new ArgumentException(
                $"Expression '{keySelector}' refers to a method, not a property."
            );

        if (member.Member is not PropertyInfo propInfo)
            throw new ArgumentException(
                $"Expression '{keySelector}' refers to a field, not a property."
            );

        var localeKeyGroup = entity.GetType().Name;
        var localeKey = propInfo.Name;

        var nodeName = localeKey;
        if (!string.IsNullOrWhiteSpace(overriddenNodeName))
            nodeName = overriddenNodeName;

        await xmlWriter.WriteStartElementAsync(nodeName);
        await xmlWriter.WriteStringAsync("Standard", propInfo.GetValue(entity));

        if (languages.Count >= 2)
        {
            await xmlWriter.WriteStartElementAsync("Locales");

            var properties = await _localizedEntityService.GetEntityLocalizedPropertiesAsync(
                entity.Id,
                localeKeyGroup,
                localeKey
            );
            foreach (var language in languages)
                if (
                    properties.FirstOrDefault(lp => lp.LanguageId == language.Id)
                    is LocalizedProperty localizedProperty
                )
                    await xmlWriter.WriteStringAsync(
                        language.UniqueSeoCode,
                        localizedProperty.LocaleValue
                    );

            await xmlWriter.WriteEndElementAsync();
        }

        await xmlWriter.WriteEndElementAsync();
    }

    #endregion Utilites

    #region Methods

    private async Task<string> GetPriceAsync(Product product)
    {
        var customer = await _workContext.GetCurrentCustomerAsync();
        var currencyId = customer.CurrencyId ?? 0;
        var currencyTmp = await _currencyService.GetCurrencyByIdAsync(currencyId);

        var customerCurrency =
            currencyTmp != null && currencyTmp.Published
                ? currencyTmp
                : await _workContext.GetWorkingCurrencyAsync();
        var customerCurrencyCode = customerCurrency.CurrencyCode;
        var store = await _storeContext.GetCurrentStoreAsync();
        var price = await _priceCalculationService.GetFinalPriceAsync(product, customer, store);

        if (price.finalPrice == _b2BB2CFeaturesSettings.ProductQuotePrice)
            return await _localizationService.GetResourceAsync("Products.ProductForQuote");

        return await _priceFormatter.FormatPriceAsync(
            price.finalPrice,
            true,
            customerCurrencyCode,
            false,
            (await _workContext.GetWorkingLanguageAsync()).Id
        );
    }

    public virtual async Task<byte[]> ExportProductsToXlsxAsync(IEnumerable<Product> products)
    {
        if (products == null)
            products = new List<Product>();

        var languages = await _languageService.GetAllLanguagesAsync(showHidden: true);

        var localizedProperties = new[]
        {
            new PropertyByName<Product, Language>(
                "Name",
                async (p, l) =>
                    await _localizationService.GetLocalizedAsync(p, x => x.Name, l.Id, false)
            ),
        };
        var properties = new List<PropertyByName<Product, Language>>
        {
            new (
                await _localizationService.GetResourceAsync("NopStation.Plugin.B2B.B2BB2CFeatures.Services.ExportManager.Excel.Sku"),
                (p, l) => p.Sku
            ),
            new (
                await _localizationService.GetResourceAsync( "NopStation.Plugin.B2B.B2BB2CFeatures.Services.ExportManager.Excel.Name"),
                (p, l) => p.Name
            ),
            new (
                await _localizationService.GetResourceAsync(
                    "NopStation.Plugin.B2B.B2BB2CFeatures.Services.ExportManager.Excel.Price"
                ),
                async (p, l) => await GetPriceAsync(p)
            )
        };

        if (_b2BB2CFeaturesSettings.DisplayUOMColumnInExcelAndPdf)
        {
            properties.Add(
                new(
                    await _localizationService.GetResourceAsync(
                        "NopStation.Plugin.B2B.B2BB2CFeatures.Services.ExportManager.Excel.UnitOfMeasure"
                    ),
                    (p, l) =>
                    {
                        return _erpSpecificationAttributeService
                                .GetProductUOMByProductIdAndSpecificationAttributeId(
                                    p.Id,
                                    _b2BB2CFeaturesSettings.UnitOfMeasureSpecificationAttributeId
                                )
                                ?.Result ?? string.Empty;
                    }
                )
            );
        }

        var erpAccount = await _erpCustomerFunctionalityService.GetActiveErpAccountByCustomerAsync(
            await _workContext.GetCurrentCustomerAsync()
        );

        if (_b2BB2CFeaturesSettings.DisplayPricingNoteColumnInExcelAndPdf)
        {
            properties.Add(
                new(
                    await _localizationService.GetResourceAsync(
                        "NopStation.Plugin.B2B.B2BB2CFeatures.Services.ExportManager.Excel.PricingNotes"
                    ),
                    (p, l) =>
                        _erpCustomerFunctionalityService
                            .GetPricingNoteAsync(erpAccount, p)
                            .Result
                )
            );
        }

        if (_b2BB2CFeaturesSettings.DisplayWeightColumnInExcelAndPdf)
        {
            var baseWeight = (
                await _measureService.GetMeasureWeightByIdAsync(_measureSettings.BaseWeightId)
            )?.Name;

            properties.Add(
                new(
                    await _localizationService.GetResourceAsync(
                        "NopStation.Plugin.B2B.B2BB2CFeatures.Services.ExportManager.Excel.Weight"
                    ),
                    (p, l) =>
                    {
                        return $"{p.Weight:F2} {baseWeight ?? string.Empty}";
                    }
                )
            );
        }

        if (_b2BB2CFeaturesSettings.DisplayQuantityColumnInExcel)
        {
            properties.Add(
                new(
                await _localizationService.GetResourceAsync(
                    "NopStation.Plugin.B2B.B2BB2CFeatures.Services.ExportManager.Excel.Quantity"
                ),
                (p, l) => p.OrderMinimumQuantity)
            );
        }

        var productList = products.ToList();

        if (
            !_catalogSettings.ExportImportProductAttributes
            && !_catalogSettings.ExportImportProductSpecificationAttributes
        )
            return await new PropertyManager<Product, Language>(
                properties,
                _catalogSettings
            ).ExportToXlsxAsync(productList);

        await _customerActivityService.InsertActivityAsync(
            "ExportCategoryProducts",
            string.Format(
                await _localizationService.GetResourceAsync(
                    "ActivityLog.ExportCategoryProducts"
                ),
                productList.Count
            )
        );

        return await new PropertyManager<Product, Language>(
            properties,
            _catalogSettings,
            localizedProperties,
            languages
        ).ExportToXlsxAsync(productList);
    }

    public virtual async Task<IPagedList<Product>> SearchProductsAsync(
        int pageIndex = 0,
        int pageSize = int.MaxValue,
        IList<int> categoryIds = null,
        IList<int> manufacturerIds = null,
        int storeId = 0,
        int vendorId = 0,
        int warehouseId = 0,
        ProductType? productType = null,
        bool visibleIndividuallyOnly = false,
        bool excludeFeaturedProducts = false,
        decimal? priceMin = null,
        decimal? priceMax = null,
        int productTagId = 0,
        string keywords = null,
        bool searchDescriptions = false,
        bool searchManufacturerPartNumber = true,
        bool searchSku = true,
        bool searchProductTags = false,
        int languageId = 0,
        IList<SpecificationAttributeOption> filteredSpecOptions = null,
        ProductSortingEnum orderBy = ProductSortingEnum.Position,
        bool showHidden = false,
        bool? overridePublished = null,
        bool showProductWithoutAttributes = false
    )
    {
        var customer = await _workContext.GetCurrentCustomerAsync();
        if (pageSize == int.MaxValue)
            pageSize = int.MaxValue - 1;

        var productsQuery = _productRepository.Table;

        if (!showHidden)
            productsQuery = productsQuery.Where(p => p.Published);
        else if (overridePublished.HasValue)
            productsQuery = productsQuery.Where(p => p.Published == overridePublished.Value);

        if (!showHidden)
        {
            productsQuery = await _storeMappingService.ApplyStoreMapping(
                productsQuery,
                storeId
            );
            productsQuery = await _aclService.ApplyAcl(productsQuery, customer);
        }

        productsQuery =
            from p in productsQuery
            where
                !p.Deleted
                && (!visibleIndividuallyOnly || p.VisibleIndividually)
                && (vendorId == 0 || p.VendorId == vendorId)
                && (
                    warehouseId == 0
                    || (
                        !p.UseMultipleWarehouses
                            ? p.WarehouseId == warehouseId
                            : _productWarehouseInventoryRepository.Table.Any(pwi =>
                                pwi.WarehouseId == warehouseId && pwi.ProductId == p.Id
                            )
                    )
                )
                && (productType == null || p.ProductTypeId == (int)productType)
                && (
                    showHidden
                    || DateTime.UtcNow >= (p.AvailableStartDateTimeUtc ?? DateTime.MinValue)
                        && DateTime.UtcNow <= (p.AvailableEndDateTimeUtc ?? DateTime.MaxValue)
                )
                && (priceMin == null || p.Price >= priceMin)
                && (priceMax == null || p.Price <= priceMax)
            select p;

        if (!string.IsNullOrEmpty(keywords))
        {
            var langs = await _languageService.GetAllLanguagesAsync(showHidden: true);

            var searchLocalizedValue =
                languageId > 0
                && langs.Count >= 2
                && (showHidden || langs.Count(l => l.Published) >= 2);
            IQueryable<int> productsByKeywords;

            var activeSearchProvider = await _searchPluginManager.LoadPrimaryPluginAsync(
                customer,
                storeId
            );

            if (activeSearchProvider is not null)
            {
                productsByKeywords = (
                    await activeSearchProvider.SearchProductsAsync(
                        keywords,
                        searchLocalizedValue
                    )
                ).AsQueryable();
            }
            else
            {
                productsByKeywords =
                    from p in _productRepository.Table
                    where
                        p.Name.Contains(keywords)
                        || (
                            searchDescriptions
                            && (
                                p.ShortDescription.Contains(keywords)
                                || p.FullDescription.Contains(keywords)
                            )
                        )
                        || (
                            searchManufacturerPartNumber && p.ManufacturerPartNumber == keywords
                        )
                        || (searchSku && p.Sku == keywords)
                    select p.Id;

                if (searchLocalizedValue)
                {
                    productsByKeywords = productsByKeywords.Union(
                        from lp in _localizedPropertyRepository.Table
                        let checkName = lp.LocaleKey == nameof(Product.Name)
                            && lp.LocaleValue.Contains(keywords)
                        let checkShortDesc = searchDescriptions
                            && lp.LocaleKey == nameof(Product.ShortDescription)
                            && lp.LocaleValue.Contains(keywords)
                        where
                            lp.LocaleKeyGroup == nameof(Product)
                            && lp.LanguageId == languageId
                            && (checkName || checkShortDesc)

                        select lp.EntityId
                    );
                }
            }

            if (searchSku)
            {
                productsByKeywords = productsByKeywords.Union(
                    from pac in _productAttributeCombinationRepository.Table
                    where pac.Sku == keywords
                    select pac.ProductId
                );
            }

            if (_catalogSettings.AllowCustomersToSearchWithCategoryName)
            {
                productsByKeywords = productsByKeywords.Union(
                    from pc in _productCategoryRepository.Table
                    join c in _categoryRepository.Table on pc.CategoryId equals c.Id
                    where c.Name.Contains(keywords)
                    select pc.ProductId
                );

                if (searchLocalizedValue)
                {
                    productsByKeywords = productsByKeywords.Union(
                        from pc in _productCategoryRepository.Table
                        join lp in _localizedPropertyRepository.Table
                            on pc.CategoryId equals lp.EntityId
                        where
                            lp.LocaleKeyGroup == nameof(Category)
                            && lp.LocaleKey == nameof(Category.Name)
                            && lp.LocaleValue.Contains(keywords)
                            && lp.LanguageId == languageId
                        select pc.ProductId
                    );
                }
            }

            if (_catalogSettings.AllowCustomersToSearchWithManufacturerName)
            {
                productsByKeywords = productsByKeywords.Union(
                    from pm in _productManufacturerRepository.Table
                    join m in _manufacturerRepository.Table on pm.ManufacturerId equals m.Id
                    where m.Name.Contains(keywords)
                    select pm.ProductId
                );

                if (searchLocalizedValue)
                {
                    productsByKeywords = productsByKeywords.Union(
                        from pm in _productManufacturerRepository.Table
                        join lp in _localizedPropertyRepository.Table
                            on pm.ManufacturerId equals lp.EntityId
                        where
                            lp.LocaleKeyGroup == nameof(Manufacturer)
                            && lp.LocaleKey == nameof(Manufacturer.Name)
                            && lp.LocaleValue.Contains(keywords)
                            && lp.LanguageId == languageId
                        select pm.ProductId
                    );
                }
            }

            if (searchProductTags)
            {
                productsByKeywords = productsByKeywords.Union(
                    from pptm in _productTagMappingRepository.Table
                    join pt in _productTagRepository.Table on pptm.ProductTagId equals pt.Id
                    where pt.Name.Contains(keywords)
                    select pptm.ProductId
                );

                if (searchLocalizedValue)
                {
                    productsByKeywords = productsByKeywords.Union(
                        from pptm in _productTagMappingRepository.Table
                        join lp in _localizedPropertyRepository.Table
                            on pptm.ProductTagId equals lp.EntityId
                        where
                            lp.LocaleKeyGroup == nameof(ProductTag)
                            && lp.LocaleKey == nameof(ProductTag.Name)
                            && lp.LocaleValue.Contains(keywords)
                            && lp.LanguageId == languageId
                        select pptm.ProductId
                    );
                }
            }

            productsQuery =
                from p in productsQuery
                join pbk in productsByKeywords on p.Id equals pbk
                select p;
        }

        if (categoryIds is not null)
        {
            if (categoryIds.Contains(0))
                categoryIds.Remove(0);

            if (categoryIds.Any())
            {
                var productCategoryQuery =
                    from pc in _productCategoryRepository.Table
                    where
                        (!excludeFeaturedProducts || !pc.IsFeaturedProduct)
                        && categoryIds.Contains(pc.CategoryId)
                    group pc by pc.ProductId into pc
                    select new { ProductId = pc.Key, DisplayOrder = pc.First().DisplayOrder };

                productsQuery =
                    from p in productsQuery
                    join pc in productCategoryQuery on p.Id equals pc.ProductId
                    orderby pc.DisplayOrder, p.Name
                    select p;
            }
        }

        if (manufacturerIds is not null)
        {
            if (manufacturerIds.Contains(0))
                manufacturerIds.Remove(0);

            if (manufacturerIds.Any())
            {
                var productManufacturerQuery =
                    from pm in _productManufacturerRepository.Table
                    where
                        (!excludeFeaturedProducts || !pm.IsFeaturedProduct)
                        && manufacturerIds.Contains(pm.ManufacturerId)
                    group pm by pm.ProductId into pm
                    select new { ProductId = pm.Key, DisplayOrder = pm.First().DisplayOrder };

                productsQuery =
                    from p in productsQuery
                    join pm in productManufacturerQuery on p.Id equals pm.ProductId
                    orderby pm.DisplayOrder, p.Name
                    select p;
            }
        }

        if (productTagId > 0)
        {
            productsQuery =
                from p in productsQuery
                join ptm in _productTagMappingRepository.Table on p.Id equals ptm.ProductId
                where ptm.ProductTagId == productTagId
                select p;
        }

        #region B2B

        var erpAccount = await _erpAccountService.GetActiveErpAccountByCustomerIdAsync(
            customer.Id
        );

        if (!IsAdminRoute() && erpAccount != null)
        {
            if (erpAccount.PreFilterFacets?.Trim() == null)
            {
                var noProducts = new List<Product>();
                return new PagedList<Product>(noProducts, pageIndex, pageSize, 0);
            }
            IList<int> specIds = null;
            if (erpAccount.PreFilterFacets?.Trim() != null)
                specIds =
                    await _erpSpecificationAttributeService.GetSpecificationAttributeOptionIdsByNames(
                        _b2BB2CFeaturesSettings.PreFilterFacetSpecificationAttributeId,
                        erpAccount.PreFilterFacets?.Trim(),
                        erpAccount.Id
                    );

            if (!specIds.Any())
            {
                var noProducts = new List<Product>();
                return new PagedList<Product>(noProducts, pageIndex, pageSize, 0);
            }

            var preFilterFacetSpecIds = new List<int>();

            foreach (var specId in specIds)
            {
                if (!preFilterFacetSpecIds.Contains(specId))
                    preFilterFacetSpecIds.Add(specId);
            }

            if (preFilterFacetSpecIds != null && preFilterFacetSpecIds.Count > 0)
            {
                preFilterFacetSpecIds.Sort();
                var filterableOptions =
                    await _specificationAttributeService.GetSpecificationAttributeOptionsByIdsAsync(
                        preFilterFacetSpecIds.ToArray()
                    );
                if (filteredSpecOptions == null)
                    filteredSpecOptions = new List<SpecificationAttributeOption>();
                foreach (var filterableOption in filterableOptions)
                {
                    filteredSpecOptions.Add(filterableOption);
                }
            }
        }

        #endregion B2B

        if (filteredSpecOptions?.Count > 0)
        {
            var specificationAttributeIds = filteredSpecOptions
                .Select(sao => sao.SpecificationAttributeId)
                .Distinct();

            foreach (var specificationAttributeId in specificationAttributeIds)
            {
                var optionIdsBySpecificationAttribute = filteredSpecOptions
                    .Where(o => o.SpecificationAttributeId == specificationAttributeId)
                    .Select(o => o.Id);

                var productSpecificationQuery =
                    from psa in _productSpecificationAttributeRepository.Table
                    where
                        psa.AllowFiltering
                        && optionIdsBySpecificationAttribute.Contains(
                            psa.SpecificationAttributeOptionId
                        )
                    select psa;

                productsQuery =
                    from p in productsQuery
                    where productSpecificationQuery.Any(pc => pc.ProductId == p.Id)
                    select p;
            }
        }

        return await productsQuery
            .OrderBy(
                _localizedPropertyRepository,
                await _workContext.GetWorkingLanguageAsync(),
                orderBy
            )
            .ToPagedListAsync(pageIndex, pageSize);
    }

    public virtual async Task ExportProductsToPdfAsync(Stream stream, IList<Product> products)
    {
        ArgumentNullException.ThrowIfNull(stream);
        ArgumentNullException.ThrowIfNull(products);

        var lang = await _workContext.GetWorkingLanguageAsync();
        var customer = await _workContext.GetCurrentCustomerAsync();
        var erpAccount = await _erpCustomerFunctionalityService.GetActiveErpAccountByCustomerAsync(customer);

        var baseWeightUnit = (
            await _measureService.GetMeasureWeightByIdAsync(_measureSettings.BaseWeightId)
        )?.Name;

        var categories = await _categoryService.GetAllCategoriesAsync();
        var productCategoryMappingTasks = products.Select(async product =>
        {
            var productCategories = await _categoryService.GetProductCategoriesByProductIdAsync(product.Id);
            return productCategories.Select(pc => new { Product = product, ProductCategory = pc });
        });

        var productCategoryJoin = (await Task.WhenAll(productCategoryMappingTasks))
            .SelectMany(x => x)
            .ToList();

        var groupedByCategory = productCategoryJoin
            .GroupBy(pc => pc.ProductCategory.CategoryId)
            .ToList();

        var categoryGroups = await Task.WhenAll(
            groupedByCategory.Select(async g =>
            {
                var categoryEntity = categories.FirstOrDefault(c => c.Id == g.Key);
                var groupProducts = g.Select(pc => pc.Product).DistinctBy(p => p.Id).ToList();

                var productsWithDetails = await Task.WhenAll(
                    groupProducts.Select(async product =>
                    {
                        var productName = await _localizationService.GetLocalizedAsync(
                            product,
                            x => x.Name,
                            lang.Id
                        );
                        var pricingNote =
                            erpAccount != null
                            && _b2BB2CFeaturesSettings.DisplayPricingNoteColumnInExcelAndPdf
                                ? await _erpCustomerFunctionalityService.GetPricingNoteAsync(
                                    erpAccount,
                                    product
                                )
                                : string.Empty;
                        var uom = _b2BB2CFeaturesSettings.DisplayUOMColumnInExcelAndPdf
                            ? await _erpSpecificationAttributeService.GetProductUOMByProductIdAndSpecificationAttributeId(
                                product.Id,
                                _b2BB2CFeaturesSettings.UnitOfMeasureSpecificationAttributeId
                            )
                            : string.Empty;
                        var weight =
                            _b2BB2CFeaturesSettings.DisplayWeightColumnInExcelAndPdf
                            && product.Weight > 0
                                ? $"{product.Weight:F2} {baseWeightUnit ?? string.Empty}"
                                : string.Empty;
                        var unitPrice = await GetPriceAsync(product);

                        return new
                        {
                            Product = product,
                            ProductName = productName,
                            PricingNote = pricingNote,
                            UOM = uom,
                            Weight = weight,
                            UnitPrice = unitPrice,
                        };
                    })
                );

                return new
                {
                    CategoryName = categoryEntity?.Name ?? "Other",
                    CategoryEntity = categoryEntity,
                    Products = productsWithDetails,
                };
            })
        );


        var columnCount = 2;
        if (_b2BB2CFeaturesSettings.DisplayPricingNoteColumnInExcelAndPdf)
            columnCount++;
        if (_b2BB2CFeaturesSettings.DisplayUOMColumnInExcelAndPdf)
            columnCount++;
        if (_b2BB2CFeaturesSettings.DisplayWeightColumnInExcelAndPdf)
            columnCount++;
        columnCount++;

        var preparedForText =
            $"" +
            $"{await _localizationService.GetResourceAsync(
                "NopStation.Plugin.B2B.B2BB2CFeatures.Services.ExportManager.Pdf.PreparedFor",
                lang.Id)} {erpAccount?.AccountName ?? ""}";

        var priceListTitle = await _localizationService.GetResourceAsync(
            "NopStation.Plugin.B2B.B2BB2CFeatures.Services.ExportManager.Pdf.PriceListTitle",
            lang.Id
        );
        var customerFullName = await _customerService.GetCustomerFullNameAsync(customer);

        var skuLabel = await _localizationService.GetResourceAsync(
            "NopStation.Plugin.B2B.B2BB2CFeatures.Services.ExportManager.PDF.SKU",
            lang.Id
        );
        var productNameLabel = await _localizationService.GetResourceAsync(
            "NopStation.Plugin.B2B.B2BB2CFeatures.Services.ExportManager.PDF.ProductName",
            lang.Id
        );
        var priceLabel = await _localizationService.GetResourceAsync(
            "NopStation.Plugin.B2B.B2BB2CFeatures.Services.ExportManager.PDF.Price",
            lang.Id
        );
        var uomLabel = _b2BB2CFeaturesSettings.DisplayUOMColumnInExcelAndPdf
            ? await _localizationService.GetResourceAsync("NopStation.Plugin.B2B.B2BB2CFeatures.Services.ExportManager.PDF.UOM", lang.Id)
            : null;
        var pricingNoteLabel = _b2BB2CFeaturesSettings.DisplayPricingNoteColumnInExcelAndPdf
            ? await _localizationService.GetResourceAsync("NopStation.Plugin.B2B.B2BB2CFeatures.Services.ExportManager.PDF.PricingNote", lang.Id)
            : null;
        var weightLabel = _b2BB2CFeaturesSettings.DisplayWeightColumnInExcelAndPdf
            ? await _localizationService.GetResourceAsync("NopStation.Plugin.B2B.B2BB2CFeatures.Services.ExportManager.PDF.Weight", lang.Id)
            : null;

        Document
            .Create(container =>
            {

                container.Page(page =>
                {
                    page.Size(
                        _pdfSettings.LetterPageSizeEnabled ? PageSizes.Letter : PageSizes.A4
                    );
                    page.Margin(35);
                    page.DefaultTextStyle(x => x.FontSize(08).NormalWeight());
                    page.Header()
                        .ShowOnce()
                        .Row(row =>
                        {
                            row.RelativeItem()
                                .AlignLeft()
                                .Column(column =>
                                {
                                    column.Item().Text(preparedForText + ", ").FontSize(10).Bold();
                                    column.Item().Text(customerFullName + ",").FontSize(8.5f);
                                    column.Item().Text(customer.Email).FontSize(8.5f);
                                });

                            row.RelativeItem()
                                .AlignRight()
                                .Column(column =>
                                {
                                    column.Item().AlignRight().Text(priceListTitle).Bold().FontSize(18);
                                    column.Item().AlignRight().Text(DateTime.Now.ToString("MMMM-yyyy")).FontSize(8.5f);
                                });
                        });

                    page.Content().PaddingTop(10)
                        .Element(content =>
                        {
                            content.Table(table =>
                            {
                                table.ColumnsDefinition(columns =>
                                {
                                    columns.RelativeColumn(20);
                                    columns.RelativeColumn(70);
                                    if (
                                        _b2BB2CFeaturesSettings.DisplayPricingNoteColumnInExcelAndPdf
                                    )
                                        columns.RelativeColumn(10);
                                    if (_b2BB2CFeaturesSettings.DisplayUOMColumnInExcelAndPdf)
                                        columns.RelativeColumn(10);
                                    if (
                                        _b2BB2CFeaturesSettings.DisplayWeightColumnInExcelAndPdf
                                    )
                                        columns.RelativeColumn(10);
                                    columns.RelativeColumn(10);
                                });

                                table.Header(header =>
                                {
                                    header.Cell().Border(1).Padding(3).AlignCenter().Text(skuLabel).FontSize(8.5f);
                                    header.Cell().Border(1).Padding(3).AlignCenter().Text(productNameLabel).FontSize(8.5f);

                                    if (pricingNoteLabel is not null)
                                        header.Cell().Border(1).Padding(3).AlignCenter().Text(pricingNoteLabel).FontSize(8.5f);

                                    if (uomLabel is not null)
                                        header.Cell().Border(1).Padding(3).AlignCenter().Text(uomLabel).FontSize(8.5f);

                                    if (weightLabel is not null)
                                        header.Cell().Border(1).Padding(3).AlignCenter().Text(weightLabel).FontSize(8.5f);

                                    header.Cell().Border(1).Padding(3).AlignCenter().Text(priceLabel).FontSize(8.5f);
                                });

                                foreach (var group in categoryGroups)
                                {
                                    table
                                        .Cell()
                                        .ColumnSpan((uint)columnCount)
                                        .Background(Colors.Grey.Lighten2)
                                        .Border(1)
                                        .Padding(3)
                                        .Text(group.CategoryName)
                                        .Bold();

                                    foreach (var productData in group.Products)
                                    {
                                        table
                                            .Cell()
                                            .Border(1)
                                            .Padding(3)
                                            .AlignLeft()
                                            .Text(productData.Product.Sku)
                                            .FontSize(8.5f);

                                        table
                                            .Cell()
                                            .Border(1)
                                            .Padding(3)
                                            .AlignLeft()
                                            .Text(productData.ProductName)
                                            .FontSize(8.5f);

                                        if (_b2BB2CFeaturesSettings.DisplayPricingNoteColumnInExcelAndPdf)
                                            table
                                                .Cell()
                                                .Border(1)
                                                .Padding(3)
                                                .AlignCenter()
                                                .Text(productData.PricingNote)
                                                .FontSize(8.5f);

                                        if (_b2BB2CFeaturesSettings.DisplayUOMColumnInExcelAndPdf)
                                            table
                                                .Cell()
                                                .Border(1)
                                                .Padding(3)
                                                .AlignCenter()
                                                .Text(productData.UOM).FontSize(8.5f);

                                        if (_b2BB2CFeaturesSettings.DisplayWeightColumnInExcelAndPdf)
                                            table
                                                .Cell()
                                                .Border(1)
                                                .Padding(3)
                                                .AlignCenter()
                                                .Text(productData.Weight)
                                                .FontSize(8.5f);

                                        table
                                            .Cell()
                                            .Border(1)
                                            .Padding(3)
                                            .AlignRight()
                                            .Text(productData.UnitPrice)
                                            .FontSize(8.5f);
                                    }
                                }
                            });
                        });

                    page.Footer()
                        .AlignCenter()
                        .Text(text =>
                        {
                            text.CurrentPageNumber().Format(pageNumber => $"- {pageNumber} -");
                        });
                });
            })
            .GeneratePdf(stream);
    }

    #endregion
}