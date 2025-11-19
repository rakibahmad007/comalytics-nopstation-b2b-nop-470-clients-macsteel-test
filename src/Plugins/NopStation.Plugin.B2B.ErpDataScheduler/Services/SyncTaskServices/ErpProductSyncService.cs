using System.Reflection;
using System.Text.RegularExpressions;
using FluentValidation;
using LinqToDB.Data;
using Newtonsoft.Json.Linq;
using Nop.Core;
using Nop.Core.Caching;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Stores;
using Nop.Core.Domain.Tax;
using Nop.Core.Domain.Vendors;
using Nop.Core.Infrastructure;
using Nop.Data;
using Nop.Services.Catalog;
using Nop.Services.Seo;
using Nop.Services.Shipping;
using Nop.Services.Tax;
using Nop.Services.Vendors;
using NopStation.Plugin.B2B.B2BB2CFeatures;
using NopStation.Plugin.B2B.ErpDataScheduler.Services.SyncLogServices;
using NopStation.Plugin.B2B.ErpDataScheduler.Services.SyncWorkflowMessage;
using NopStation.Plugin.B2B.ERPIntegrationCore.Domain;
using NopStation.Plugin.B2B.ERPIntegrationCore.Enums;
using NopStation.Plugin.B2B.ERPIntegrationCore.Model;
using NopStation.Plugin.B2B.ERPIntegrationCore.Model.ErpIntegration;
using NopStation.Plugin.B2B.ERPIntegrationCore.Services;
using NopStation.Plugin.B2B.ERPIntegrationCore.Validators.Helpers;

namespace NopStation.Plugin.B2B.ErpDataScheduler.Services.SyncTaskServices;

public class ErpProductSyncService : IErpProductSyncService
{
    #region Fields

    private readonly IVendorService _vendorService;
    private readonly IProductService _productService;
    private readonly ICategoryService _categoryService;
    private readonly IShippingService _shippingService;
    private readonly IUrlRecordService _urlRecordService;
    private readonly ITaxCategoryService _taxCategoryService;
    private readonly IManufacturerService _manufacturerService;
    private readonly IProductTemplateService _productTemplateService;
    private readonly ICategoryTemplateService _categoryTemplateService;
    private readonly IManufacturerTemplateService _manufacturerTemplateService;
    private readonly ISpecificationAttributeService _specificationAttributeService;
    private readonly ISyncLogService _erpSyncLogService;
    private readonly IErpProductService _erpProductService;
    private readonly IErpIntegrationPluginManager _erpIntegrationPluginManager;
    private readonly IErpSalesOrgService _erpSalesOrgService;
    private readonly B2BB2CFeaturesSettings _b2BB2CFeaturesSettings;
    private readonly IStaticCacheManager _staticCacheManager;
    private readonly ISyncWorkflowMessageService _syncWorkflowMessageService;
    private readonly INopDataProvider _nopDataProvider;
    private const int CATEGORY_PAGE_SIZE = 5;
    private const string MANUFACTURER_TEMPLATE_VIEWPATH = "ManufacturerTemplate.ProductsInGridOrLines";
    private const string PRODUCT_TEMPLATE_VIEWPATH = "ProductTemplate.Simple";
    private const string CATEGORY_TEMPLATE_VIEWPATH = "CategoryTemplate.ProductsInGridOrLines";
    private const string SP_PRODUCT_RUN_FINISHED = "sp_erpproduct_run_finished";
    private const int MANUFACTURER_PAGE_SIZE = 6;
    private const string MANUFACTURER_PAGE_SIZE_OPTIONS = "6, 3, 9";
    private readonly IValidator<Product> _productValidator;
    private readonly IValidator<Manufacturer> _manufacturerValidator;
    private readonly IValidator<Category> _categoryValidator;
    private readonly IRepository<ERPProductCategoryMap> _erpProductCategoryMapRepository;
    private readonly IRepository<ProductWarehouseInventory> _productWarehouseInventoryRepository;
    private readonly IRepository<ErpWarehouseSalesOrgMap> _erpWarehouseSalesOrgMapRepository;
    private readonly INopFileProvider _nopFileProvider;
    private readonly IRepository<ProductSpecificationAttribute> _productSpecificationAttributeRepository;
    private readonly IRepository<SpecificationAttributeOption> _specificationAttributeOptionRepository;
    private readonly IRepository<SpecificationAttribute> _specificationAttributeRepository;
    private readonly IStoreContext _storeContext;
    private readonly ErpDataSchedulerSettings _erpDataSchedulerSettings;
    private readonly TaxSettings _taxSettings;

    #endregion Fields

    #region Ctor

    public ErpProductSyncService(IVendorService vendorService,
        IProductService productService,
        ICategoryService categoryService,
        IShippingService shippingService,
        IUrlRecordService urlRecordService,
        ITaxCategoryService taxCategoryService,
        IManufacturerService manufacturerService,
        IProductTemplateService productTemplateService,
        ICategoryTemplateService categoryTemplateService,
        IManufacturerTemplateService manufacturerTemplateService,
        ISpecificationAttributeService specificationAttributeService,
        ISyncLogService erpSyncLogService,
        IErpProductService erpProductService,
        IErpIntegrationPluginManager erpIntegrationPluginManager,
        IErpSalesOrgService erpSalesOrgService,
        B2BB2CFeaturesSettings b2BB2CFeaturesSettings,
        IValidator<Product> productValidator,
        IValidator<Manufacturer> manufacturerValidator,
        IValidator<Category> categoryValidatory,
        IStaticCacheManager staticCacheManager,
        ISyncWorkflowMessageService syncWorkflowMessageService,
        INopDataProvider nopDataProvider,
        IRepository<ERPProductCategoryMap> erpProductCategoryMapRepository,
        IRepository<ProductWarehouseInventory> productWarehouseInventoryRepository,
        IRepository<ErpWarehouseSalesOrgMap> erpWarehouseSalesOrgMapRepository,
        INopFileProvider nopFileProvider,
        IRepository<ProductSpecificationAttribute> productSpecificationAttributeRepository,
        IRepository<SpecificationAttributeOption> specificationAttributeOptionRepository,
        IRepository<SpecificationAttribute> specificationAttributeRepository,
        IStoreContext storeContext,
        ErpDataSchedulerSettings erpDataSchedulerSettings,
        TaxSettings taxSettings)
    {
        _vendorService = vendorService;
        _productService = productService;
        _categoryService = categoryService;
        _shippingService = shippingService;
        _urlRecordService = urlRecordService;
        _taxCategoryService = taxCategoryService;
        _manufacturerService = manufacturerService;
        _productTemplateService = productTemplateService;
        _categoryTemplateService = categoryTemplateService;
        _manufacturerTemplateService = manufacturerTemplateService;
        _specificationAttributeService = specificationAttributeService;
        _erpSyncLogService = erpSyncLogService;
        _erpProductService = erpProductService;
        _erpSalesOrgService = erpSalesOrgService;
        _b2BB2CFeaturesSettings = b2BB2CFeaturesSettings;
        _productValidator = productValidator;
        _manufacturerValidator = manufacturerValidator;
        _categoryValidator = categoryValidatory;
        _staticCacheManager = staticCacheManager;
        _syncWorkflowMessageService = syncWorkflowMessageService;
        _erpIntegrationPluginManager = erpIntegrationPluginManager;
        _nopDataProvider = nopDataProvider;
        _erpProductCategoryMapRepository = erpProductCategoryMapRepository;
        _productWarehouseInventoryRepository = productWarehouseInventoryRepository;
        _erpWarehouseSalesOrgMapRepository = erpWarehouseSalesOrgMapRepository;
        _nopFileProvider = nopFileProvider;
        _productSpecificationAttributeRepository = productSpecificationAttributeRepository;
        _specificationAttributeOptionRepository = specificationAttributeOptionRepository;
        _specificationAttributeRepository = specificationAttributeRepository;
        _storeContext = storeContext;
        _erpDataSchedulerSettings = erpDataSchedulerSettings;
        _taxSettings = taxSettings;
    }

    #endregion Ctor

    #region Utilities

    private async Task SaveOrUpdateEntitySeNameAsync<T>(T entity) where T : BaseEntity
    {
        if (entity is Category category)
        {
            var seName = await _urlRecordService.GetSeNameAsync(category);

            if (string.IsNullOrWhiteSpace(seName))
            {
                seName = await _urlRecordService.ValidateSeNameAsync(category, string.Empty, category.Name, true);
                await _urlRecordService.SaveSlugAsync(category, seName, 0);
            }
        }
        else if (entity is Product product)
        {
            var seName = await _urlRecordService.GetSeNameAsync(product);

            if (string.IsNullOrWhiteSpace(seName))
            {
                seName = await _urlRecordService.ValidateSeNameAsync(product, string.Empty, product.Name, true);
                await _urlRecordService.SaveSlugAsync(product, seName, 0);
            }
        }
        else if (entity is Manufacturer manufacturer)
        {
            var seName = await _urlRecordService.GetSeNameAsync(manufacturer);

            if (string.IsNullOrWhiteSpace(seName))
            {
                seName = await _urlRecordService.ValidateSeNameAsync(manufacturer, string.Empty, manufacturer.Name, true);
                await _urlRecordService.SaveSlugAsync(manufacturer, seName, 0);
            }
        }
    }

    private async Task<bool> IsthisProductIsValidAsync(Product product, string syncTaskName)
    {
        if (product is null)
            return false;

        var validationResult = await _productValidator.ValidateAsync(product);

        if (!validationResult.IsValid)
        {
            var errorMessages = ErpDataValidationHelper.PrepareValidationLog(validationResult);

            await _erpSyncLogService.SyncLogSaveOnFileAsync(
                syncTaskName,
                ErpSyncLevel.Product,
                $"Data mapping skipped for {nameof(Product)}, {nameof(Product.Sku)}: {product.Sku}. \r\n {errorMessages}");
        }

        return validationResult.IsValid;
    }

    private async Task<bool> IsValidCategoryAsync(Category category, string syncTaskName)
    {
        if (category is null)
            return false;

        var validationResult = await _categoryValidator.ValidateAsync(category);

        if (!validationResult.IsValid)
        {
            var errorMessages = ErpDataValidationHelper.PrepareValidationLog(validationResult);

            await _erpSyncLogService.SyncLogSaveOnFileAsync(
                syncTaskName,
                ErpSyncLevel.Product,
                $"Data mapping skipped for {nameof(Category)}, {nameof(Category.Name)}: {category.Name}. \r\n {errorMessages}");
        }

        return validationResult.IsValid;
    }

    private async Task<bool> IsValidManufacturerAsync(Manufacturer manufacturer, string syncTaskName)
    {
        if (manufacturer is null)
            return false;

        var validationResult = await _manufacturerValidator.ValidateAsync(manufacturer);

        if (!validationResult.IsValid)
        {
            var errorMessages = ErpDataValidationHelper.PrepareValidationLog(validationResult);

            await _erpSyncLogService.SyncLogSaveOnFileAsync(
                syncTaskName,
                ErpSyncLevel.Product,
                $"Data mapping skipped for {nameof(Manufacturer)}, {nameof(Manufacturer.Name)}: {manufacturer.Name}. \r\n {errorMessages}");
        }

        return validationResult.IsValid;
    }

    /// <summary>
    /// Creates a temporary table #DeletedData to track product-category mappings that will be deleted.
    /// Deletes records from the Product_Category_Mapping table that meet specific criteria:
    /// - They match the current product ID
    /// - Their category IDs are not in a special list (combines categories from ErpSalesOrg.SpecialsCategoryId and 
    ///   any additional categories passed in the categoryIds parameter)
    /// - They exist in the ERP_Product_Category_Map table (meaning they were previously created by the ERP sync)
    ///
    /// The deleted records are captured in the #DeletedData temporary table using the OUTPUT clause.
    /// Removes corresponding records from the ERP_Product_Category_Map tracking table based on the mappings that were just deleted.
    /// Cleans up by dropping the temporary table.
    /// </summary>
    /// <param name="productId"></param>
    /// <param name="categoryIds"></param>
    /// <returns></returns>
    private async Task RemoveProductCategoriesExceptGivenAndSpecialsAsync(int productId, List<int> categoryIds, string syncTaskName)
    {
        if (categoryIds == null || categoryIds.Count == 0)
            return;

        try
        {
            var valuesSql = string.Join(", ", categoryIds.Select((id, index) => $"(@p{index})").ToList());

            var sql = $@"
                CREATE TABLE #DeletedData (ProductId INT, CategoryId INT);

                DELETE FROM Product_Category_Mapping
                OUTPUT deleted.ProductId, deleted.CategoryId INTO #DeletedData
                WHERE ProductId = @ProductId AND CategoryId NOT IN (
                    SELECT SpecialsCategoryId as CatId FROM Erp_Sales_Org
                    {(categoryIds.Count > 0 ? $"UNION SELECT CatId FROM (VALUES {valuesSql}) as v(CatId)" : string.Empty)}
                )
                AND EXISTS (
                    SELECT 1 FROM ERP_Product_Category_Map as EPCM2
                    WHERE EPCM2.ProductId = Product_Category_Mapping.ProductId
                    AND EPCM2.CategoryId = Product_Category_Mapping.CategoryId
                );

                DELETE FROM ERP_Product_Category_Map
                WHERE EXISTS (
                    SELECT 1 FROM #DeletedData as dd
                    WHERE dd.ProductId = ERP_Product_Category_Map.ProductId
                    AND dd.CategoryId = ERP_Product_Category_Map.CategoryId
                );

                IF OBJECT_ID('tempdb..#DeletedData') IS NOT NULL
                BEGIN
                    DROP TABLE #DeletedData;
                END;
            ";

            // Create all parameters: productId + categoryIds
            var parameters = new List<DataParameter>
            {
                new("ProductId", productId)
            };

            for (var paramIndex = 0; paramIndex < categoryIds.Count; paramIndex++)
            {
                parameters.Add(new DataParameter($"p{paramIndex}", categoryIds[paramIndex]));
            }

            await _nopDataProvider.ExecuteNonQueryAsync(sql, parameters.ToArray());
        }
        catch (Exception ex)
        {
            await _erpSyncLogService.SyncLogSaveOnFileAsync(
                syncTaskName,
                ErpSyncLevel.Product,
                ex.Message,
                ex.StackTrace ?? string.Empty);
        }
    }

    private async Task RemoveProductCategoryMappingsForNonSpecialProductsAsync(int categoryId, IList<int> productIds, string syncTaskName)
    {
        if (productIds == null || !productIds.Any())
            return;
        try
        {
            var valuesSql = string.Join(", ", productIds.Select((id, index) => $"@p{index}").ToList());

            var sql = $@"
                CREATE TABLE #DeletedSData (CategoryId INT, ProductId INT);

                DELETE FROM Product_Category_Mapping
                OUTPUT deleted.CategoryId, deleted.ProductId INTO #DeletedSData
                WHERE CategoryId = @CategoryId AND ProductId IN ({valuesSql})
                AND EXISTS (
                    SELECT 1 FROM ERP_Product_Category_Map as EPCM
                    WHERE EPCM.CategoryId = [Product_Category_Mapping].CategoryId
                    AND EPCM.ProductId = [Product_Category_Mapping].ProductId
                );

                DELETE FROM ERP_Product_Category_Map
                WHERE EXISTS (
                    SELECT 1 FROM #DeletedSData as dsd
                    WHERE dsd.CategoryId = [ERP_Product_Category_Map].CategoryId
                    AND dsd.ProductId = [ERP_Product_Category_Map].ProductId
                );

                IF OBJECT_ID('tempdb..#DeletedSData') IS NOT NULL
                BEGIN
                    DROP TABLE #DeletedSData;
                END;
            ";

            // Create all parameters: categoryId + productIds
            var parameters = new List<DataParameter>
            {
                new ("CategoryId", categoryId)
            };

            for (var paramIndex = 0; paramIndex < productIds.Count; paramIndex++)
            {
                parameters.Add(new DataParameter($"p{paramIndex}", productIds[paramIndex]));
            }

            await _nopDataProvider.ExecuteNonQueryAsync(sql, parameters.ToArray());
        }
        catch (Exception ex)
        {
            await _erpSyncLogService.SyncLogSaveOnFileAsync(
                syncTaskName,
                ErpSyncLevel.Product,
                ex.Message,
                ex.StackTrace ?? string.Empty);
        }
    }

    private async Task AddProductCategoryMappingAsync(ProductCategory productCategory, 
        IList<ProductCategory> existingCategoryMapping, 
        IList<ProductCategory> allProductCategories)
    {
        if (productCategory == null || existingCategoryMapping == null || allProductCategories == null)
            return;

        if (!existingCategoryMapping.Any(a => a.CategoryId == productCategory.CategoryId && a.ProductId == productCategory.ProductId))
        {
            await _categoryService.InsertProductCategoryAsync(productCategory);

            allProductCategories.Add(productCategory);
            existingCategoryMapping.Add(productCategory);
        }
    }

    private async Task AddErpProductCategoryMappingAsync(int productId, int categoryId)
    {
        if (productId == 0 || categoryId == 0)
            return;

        if (!await _erpProductCategoryMapRepository.Table.AnyAsync(x => x.ProductId == productId && x.CategoryId == categoryId))
        {
            await _erpProductCategoryMapRepository.InsertAsync(new ERPProductCategoryMap
            {
                ProductId = productId,
                CategoryId = categoryId,
            });
        }
    }

    private async Task AddProductMappingWithSpecialCategoryAsync(int specialsCategoryId,
        int productId,
        int salesOrgId,
        IList<ProductCategory> existingCategoryMapping,
        IList<ProductCategory> allProductCategories)
    {
        if (specialsCategoryId == 0 ||
            productId == 0 ||
            salesOrgId == 0 ||
            existingCategoryMapping == null ||
            allProductCategories == null)
            return;

        var hasStock = (from wsm in _erpWarehouseSalesOrgMapRepository.Table
                        where wsm.ErpSalesOrgId == salesOrgId
                        join pwi in _productWarehouseInventoryRepository.Table
                        on wsm.NopWarehouseId equals pwi.WarehouseId
                        where pwi.ProductId == productId && pwi.StockQuantity > 0
                        select pwi).Any();

        //2189 - Macsteel - CR6034 - check stock when add special category
        if (hasStock)
        {
            var productCategory = existingCategoryMapping.FirstOrDefault(map => map.ProductId == productId && map.CategoryId == specialsCategoryId);

            if (productCategory == null)
            {
                var newProductCategory = new ProductCategory
                {
                    ProductId = productId,
                    CategoryId = specialsCategoryId
                };
                await AddProductCategoryMappingAsync(newProductCategory, existingCategoryMapping, allProductCategories);
                await AddErpProductCategoryMappingAsync(newProductCategory.ProductId, newProductCategory.CategoryId);
            }
        }
    }

    private async Task UpdateProductSpecificationAttributesAsync(int productId, 
        IList<KeyValuePair<string, string>> attributeMappings, 
        string syncTaskName)
    {
        if (productId == 0 || attributeMappings == null)
            return;
        try
        {
            var mappingsFilePath = _nopFileProvider.MapPath("~/Plugins/Misc.B2B.SapIntegration/mappings.json");
            var config = new Dictionary<string, SpecificationAttributeMapping>(StringComparer.InvariantCultureIgnoreCase);

            if (File.Exists(mappingsFilePath))
            {
                var json = File.ReadAllText(mappingsFilePath);
                var token = JObject.Parse(json);
                var mappings = token["specificationAttributes"]?.ToObject<List<SpecificationAttributeMapping>>();
                if (mappings != null)
                {
                    config = mappings.ToDictionary(m => m.ErpAttribute);
                }
            }

            foreach (var mapping in attributeMappings)
            {
                var attributeName = mapping.Key;
                var attributeValue = mapping.Value;

                if (string.IsNullOrEmpty(attributeName) || string.IsNullOrEmpty(attributeValue))
                {
                    continue;
                }

                #region Load specification attribute metadata (from file)

                if (!config.TryGetValue(attributeName, out var metadata))
                {
                    // If no mapping exists, we create a new one which maps from erp name to same nop name
                    metadata = new SpecificationAttributeMapping
                    {
                        ErpAttribute = attributeName,
                        NopAttribute = attributeName,
                        AllowFiltering = false,
                        ShowOnProductPage = false,
                        AttributeType = SpecificationAttributeType.CustomText,
                    };
                    config.Add(attributeName, metadata);
                }
                else
                {
                    attributeName = metadata.NopAttribute;
                }
                //System.Diagnostics.Debug.Assert(metadata != null);

                #endregion Load specification attribute metadata (from file)

                #region Determine id of SpecificationAttribute row

                if (metadata.SpecAttributeId == null)
                {
                    // If mapping exists but id hasn't been established yet, we first try to load from the database
                    var specAttr = _specificationAttributeRepository.Table.FirstOrDefault(sa => sa.Name == attributeName);
                    if (specAttr == null)
                    {
                        // If it doesn't exist yet, we create it
                        specAttr = new SpecificationAttribute
                        {
                            DisplayOrder = 0,
                            Name = attributeName,
                        };
                        await _specificationAttributeService.InsertSpecificationAttributeAsync(specAttr);
                    }
                    metadata.SpecAttributeId = specAttr.Id;
                }
                // System.Diagnostics.Debug.Assert(metadata.NopId != null);

                #endregion Determine id of SpecificationAttribute row

                #region Determine id of SpecificationAttributeOption

                var specificationAttributeOptions = _specificationAttributeOptionRepository.Table
                    .Where(sa => sa.SpecificationAttributeId == metadata.SpecAttributeId);

                int? specAttrOptionId = null;
                if (metadata.AttributeType == SpecificationAttributeType.Option)
                {
                    #region If spec. attr. is of type Option, we need the id of the option with the given name

                    specAttrOptionId = specificationAttributeOptions
                        .FirstOrDefault(a => a.Name == attributeValue)?.Id;
                    if (specAttrOptionId == null)
                    {
                        // if no such option exists we create it
                        var specAttrOption = new SpecificationAttributeOption();
                        specAttrOption.Name = attributeValue;
                        specAttrOption.SpecificationAttributeId = metadata.SpecAttributeId.Value;
                        await _specificationAttributeService.InsertSpecificationAttributeOptionAsync(specAttrOption);

                        specAttrOptionId = specAttrOption.Id;
                    }

                    #endregion If spec. attr. is of type Option, we need the id of the option with the given name
                }
                else
                {
                    #region If spec. attr is NOT of type Option, we need the id of the "default" option

                    // (attribute value is stored in the mapping itself, updated below)
                    specAttrOptionId = specificationAttributeOptions.FirstOrDefault()?.Id;
                    if (specAttrOptionId == null)
                    {
                        // if no such option exists we create it
                        var specAttrOption = new SpecificationAttributeOption();
                        specAttrOption.Name = "default";
                        specAttrOption.SpecificationAttributeId = metadata.SpecAttributeId.Value;
                        await _specificationAttributeService.InsertSpecificationAttributeOptionAsync(specAttrOption);

                        specAttrOptionId = specAttrOption.Id;
                    }

                    #endregion If spec. attr is NOT of type Option, we need the id of the "default" option
                }

                //System.Diagnostics.Debug.Assert(specAttrOptionId != null);

                #endregion Determine id of SpecificationAttributeOption

                #region Finally, update product<->attribute mapping

                //check from here
                var customValue = (metadata.AttributeType == SpecificationAttributeType.Option) ? string.Empty : attributeValue;

                var psaMapping =
                    (from m in _productSpecificationAttributeRepository.Table
                     join sao in specificationAttributeOptions on m.SpecificationAttributeOptionId equals sao.Id
                     where m.ProductId == productId
                     select m)
                    .FirstOrDefault();

                metadata.AttributeTypeId = (int?)metadata.AttributeType;

                if (psaMapping != null)
                {
                    // update existing mapping...
                    psaMapping.SpecificationAttributeOptionId = specAttrOptionId.Value;
                    psaMapping.AttributeTypeId = metadata.AttributeTypeId ?? 0;
                    psaMapping.CustomValue = customValue;
                    psaMapping.AllowFiltering = metadata.AllowFiltering;
                    psaMapping.ShowOnProductPage = metadata.ShowOnProductPage;

                    await _productSpecificationAttributeRepository.UpdateAsync(psaMapping);
                }
                else
                {
                    // ...or create new one
                    psaMapping = new ProductSpecificationAttribute()
                    {
                        ProductId = productId,
                        SpecificationAttributeOptionId = specAttrOptionId.Value,
                        AttributeTypeId = metadata.AttributeTypeId ?? 0,
                        AllowFiltering = metadata.AllowFiltering,
                        CustomValue = customValue,
                        ShowOnProductPage = metadata.ShowOnProductPage,
                    };
                    await _productSpecificationAttributeRepository.InsertAsync(psaMapping);
                }

                #endregion Finally, update product<->attribute mapping
            }
        }
        catch (Exception ex)
        {
            await _erpSyncLogService.SyncLogSaveOnFileAsync(
                syncTaskName,
                ErpSyncLevel.Product,
                $"Something went wrong while updating specification attributes of prodcutid - {productId} due to this error: {ex.Message}",
                ex.StackTrace ?? string.Empty);

        }
    }

    private async Task DeleteStockForLocationAsync(string location, DateTime threshold, string syncTaskName)
    {
        if (string.IsNullOrEmpty(location))
            return;
        try
        {
            var sql = @"
                DELETE PWI
                FROM ProductWarehouseInventory AS PWI
                INNER JOIN Product AS P ON P.Id = PWI.ProductId
                INNER JOIN Warehouse AS W ON W.Id = PWI.WarehouseId
                INNER JOIN Erp_Warehouse_Sales_Org_Map AS SOW ON SOW.NopWarehouseId = W.Id
                INNER JOIN Erp_Sales_Org AS SO ON SO.Id = SOW.ErpSalesOrgId
                WHERE SO.Code = @Location
                    AND P.UpdatedOnUtc < @Threshold;
            ";

            var parameters = new[]
            {
                new DataParameter("Location", location),
                new DataParameter("Threshold", threshold)
            };

            await _nopDataProvider.ExecuteNonQueryAsync(sql, parameters);
        }
        catch (Exception ex)
        {
            await _erpSyncLogService.SyncLogSaveOnFileAsync(
                syncTaskName,
                ErpSyncLevel.Product,
                ex.Message,
                ex.StackTrace ?? string.Empty);
        }
    }

    private async Task ExecuteStoredProcedureAsync(string spName, string syncTaskName)
    {
        try
        {
            var sql = $"IF object_id('{spName}') IS NOT NULL EXEC {spName}";
            await _nopDataProvider.ExecuteNonQueryAsync(sql);
        }
        catch (Exception ex)
        {
            await _erpSyncLogService.SyncLogSaveOnFileAsync(
                syncTaskName,
                ErpSyncLevel.Product,
                ex.Message,
                ex.StackTrace ?? string.Empty);
        }
    }

    public async Task UpdateSkipPriceStockProductAsync(DateTime dateTime, string syncTaskName)
    {
        try
        {
            var sql = @"
                IF OBJECT_ID('UpdateSkipPriceStockProduct', 'P') IS NOT NULL
                BEGIN
                    EXEC UpdateSkipPriceStockProduct @dateTime
                END
            ";

            await _nopDataProvider.ExecuteNonQueryAsync(sql, new[]
            {
                new DataParameter("dateTime", dateTime)
            });
        }
        catch (Exception ex)
        {
            await _erpSyncLogService.SyncLogSaveOnFileAsync(
                syncTaskName,
                ErpSyncLevel.Product,
                ex.Message,
                ex.StackTrace ?? string.Empty);
        }
    }

    public string GetSpecSheetURL(ErpProductDataModel erpProduct, Store currStore)
    {
        if (erpProduct == null || string.IsNullOrEmpty(_erpDataSchedulerSettings.SpecSheetLocation))
        {
            return string.Empty;
        }

        var specSheetFileName = $"SpecData_{erpProduct.Sku}.pdf";
        var specSheetRelativePath = $"wwwroot/{_erpDataSchedulerSettings.SpecSheetLocation.TrimEnd('/')}/{specSheetFileName}";

        if (!_nopFileProvider.FileExists(_nopFileProvider.MapPath(specSheetRelativePath)))
            return string.Empty;

        var specSheetUrl = $"{currStore.Url.TrimEnd('/')}/{_erpDataSchedulerSettings.SpecSheetLocation.TrimEnd('/')}/{specSheetFileName}";
        return $"<a target='_blank' href='{specSheetUrl}'>Click Here To Open Spec Sheet</a>";
    }

    private void MapProductFields(Product product,
        ErpProductDataModel erpProduct,
        IList<Vendor> allVendors,
        IList<TaxCategory> allTaxCategories,
        string programName,
        Regex lineBreakReplacer,
        int productTemplateId)
    {
        if (product == null || erpProduct == null)
            return;

        product.Sku = erpProduct.Sku;
        product.ManufacturerPartNumber = erpProduct.ManufacturerPartNumber;
        product.Name = !string.IsNullOrEmpty(erpProduct.Name)
            ? (erpProduct.Name.Length > 400 ? erpProduct.Name.Substring(0, 400) : erpProduct.Name)
            : erpProduct.Sku;

        //product.ShortDescription = erpProduct.ShortDescription.Length > 400
        //                    ? erpProduct.ShortDescription.Substring(0, 400)
        //                    : erpProduct.ShortDescription;

        var processedDescription = lineBreakReplacer.Replace(product.FullDescription, "<br/>");
        product.FullDescription = (processedDescription.Length > 400)
            ? processedDescription.Substring(0, 400)
            : processedDescription;

        product.MetaTitle = product.Name;
        product.Height = erpProduct.Height ?? product.Height;
        product.Width = erpProduct.Width ?? product.Width;
        product.Length = erpProduct.Length ?? product.Length;
        product.Price = erpProduct.Price ?? product.Price;
        product.OldPrice = erpProduct.Price ?? product.OldPrice;
        product.StockQuantity = Convert.ToInt32(Math.Min(Math.Max(Math.Round(erpProduct.StockQuantity ?? 0), int.MinValue), int.MaxValue));

        product.ProductType = ProductType.SimpleProduct;
        product.OrderMaximumQuantity = int.MaxValue;
        if (allVendors != null)
        {
            product.VendorId = allVendors
                                    .FirstOrDefault(x => x.Name.Equals(erpProduct.VendorCode) || x.Name.Equals(erpProduct.VendorName))
                                    ?.Id ?? 0;
        }
        product.UseMultipleWarehouses = true;
        product.VisibleIndividually = true;

        product.ManageInventoryMethodId = _b2BB2CFeaturesSettings.TrackInventoryMethodId;
        product.LowStockActivityId = _b2BB2CFeaturesSettings.LowStockActivityId_DefaultValue;
        product.BackorderModeId = _b2BB2CFeaturesSettings.BackorderModeId_DefaultValue;
        product.AllowBackInStockSubscriptions = _b2BB2CFeaturesSettings.AllowBackInStockSubscriptions_DefaultValue;
        product.ProductAvailabilityRangeId = _b2BB2CFeaturesSettings.ProductAvailabilityRangeId_DefaultValue;
        product.AvailableForPreOrder = _b2BB2CFeaturesSettings.AvailableForPreOrder_DefaultValue;
        product.DisplayStockAvailability = _b2BB2CFeaturesSettings.DisplayStockAvailability_DefaultValue;
        product.DisplayStockQuantity = _b2BB2CFeaturesSettings.DisplayStockQuantity_DefaultValue;

        product.IsShipEnabled = true;
        if (!string.IsNullOrWhiteSpace(erpProduct.TaxCategoryName) && allTaxCategories != null)
        {
            product.TaxCategoryId = allTaxCategories.FirstOrDefault(x => x.Name.Equals(erpProduct.TaxCategoryName))?.Id ?? product.TaxCategoryId;
            if (product.TaxCategoryId == 0 && _taxSettings.DefaultTaxCategoryId > 0)
                product.TaxCategoryId = _taxSettings.DefaultTaxCategoryId;
        }
        if ( _taxSettings.DefaultTaxCategoryId > 0)
            product.TaxCategoryId = _taxSettings.DefaultTaxCategoryId;

        if (product.Id == 0)
        {
            product.CreatedOnUtc = DateTime.UtcNow;
        }
        product.UpdatedOnUtc = DateTime.UtcNow;
        product.AdminComment = $"Updated by {programName} (B2B) on {DateTime.UtcNow:u}";
        product.Published = true;
        product.Deleted = false;

        product.ProductTemplateId = productTemplateId;
        product.PreOrderAvailabilityStartDateTimeUtc = DateTime.UtcNow;
        product.AvailableStartDateTimeUtc = DateTime.UtcNow;
        product.DisplayOrder = 1;
    }

    #endregion Utilities

    #region Method

    public virtual async Task<bool> IsErpProductSyncSuccessfulAsync(string? stockCode, 
        string? salesOrgCode = null, 
        bool isManualTrigger = false,
        bool isIncrementalSync = true,
        CancellationToken cancellationToken = default)
    {
        var syncTaskName = isIncrementalSync ? 
            ErpDataSchedulerDefaults.ErpProductIncrementalSyncTaskName :
            ErpDataSchedulerDefaults.ErpProductSyncTaskName;
    
        var erpIntegrationPlugin = await _erpIntegrationPluginManager.LoadActiveERPIntegrationPlugin();

        if (erpIntegrationPlugin is null)
        {
            await _erpSyncLogService.SyncLogSaveOnFileAsync(
                syncTaskName,
                ErpSyncLevel.Product,
                $"No integration method found. Unable to run {syncTaskName}.");

            return false;
        }

        var previousStart = "0";
        var lastSyncedErpProduct = string.Empty;
        var syncStartTime = DateTime.UtcNow.AddSeconds(-5);

        try
        {
            #region Data Collections

            var salesOrgs = new List<ErpSalesOrg>();
            if (!string.IsNullOrWhiteSpace(salesOrgCode))
            {
                var salesOrg = await _erpSalesOrgService.GetErpSalesOrgByCodeAsync(salesOrgCode);
                if (salesOrg != null)
                {
                    salesOrgs.Add(salesOrg);
                }
            }
            else
            {
                salesOrgs = (await _erpSalesOrgService.GetErpSalesOrgsAsync(isActive: true, filterOutDeleted: true)).ToList();
            }

            if (salesOrgs.Count == 0)
            {
                salesOrgs = (await _erpSalesOrgService.GetErpSalesOrgsAsync(isActive: true, filterOutDeleted: true)).ToList();
            }

            if (salesOrgs.Count == 0)
            {
                await _erpSyncLogService.SyncLogSaveOnFileAsync(
                    syncTaskName,
                    ErpSyncLevel.Product,
                    $"No Sales Org found. Unable to run {syncTaskName}.");

                return false;
            }

            var lineBreakReplacer = new Regex(@"\r?\n");
            var programName = Assembly.GetExecutingAssembly().GetName().Name ?? "Product Sync";

            var manufacturerTemplate = (await _manufacturerTemplateService.GetAllManufacturerTemplatesAsync())
                .FirstOrDefault(mftemp => mftemp.ViewPath.Equals(MANUFACTURER_TEMPLATE_VIEWPATH));
            var productTemplate = (await _productTemplateService.GetAllProductTemplatesAsync())
                .FirstOrDefault(x => x.ViewPath.Equals(PRODUCT_TEMPLATE_VIEWPATH));
            var categoryTemplate = (await _categoryTemplateService.GetAllCategoryTemplatesAsync())
                .FirstOrDefault(x => x.ViewPath.Equals(CATEGORY_TEMPLATE_VIEWPATH));

            var preFilterSpecificAttribute = await _specificationAttributeService.GetSpecificationAttributeByIdAsync(
                _b2BB2CFeaturesSettings.PreFilterFacetSpecificationAttributeId);
            var uomSpecificAttribute = await _specificationAttributeService.GetSpecificationAttributeByIdAsync(
                _b2BB2CFeaturesSettings.UnitOfMeasureSpecificationAttributeId);

            var allVendors = (await _vendorService.GetAllVendorsAsync(showHidden: true)).ToList();

            var allWarehouses = await _shippingService.GetAllWarehousesAsync();
            var allTaxCategories = await _taxCategoryService.GetAllTaxCategoriesAsync();
            var allManufacturers = (await _manufacturerService.GetAllManufacturersAsync(showHidden: true)).ToList();
            var currStore = await _storeContext.GetCurrentStoreAsync();

            var allCategories = await _categoryService.GetAllCategoriesAsync(showHidden: true) ?? new List<Category>();
            var allProductCategories = new List<ProductCategory>();
            foreach (var category in allCategories)
            {
                allProductCategories.AddRange((await _categoryService.GetProductCategoriesByCategoryIdAsync(category.Id, showHidden: true)).ToList());
            }

            #endregion Data Collections

            await _erpSyncLogService.SyncLogSaveOnFileAsync(
                syncTaskName,
                ErpSyncLevel.Product,
                "Erp Product Sync started.");

            foreach (var salesOrg in salesOrgs)
            {
                var start = "0";
                previousStart = "0";
                var isError = false;
                var totalSyncedSoFar = 0;
                var totalNotSyncedSoFar = 0;
                List<Product> products;

                while (true)
                {
                    var erpGetRequestModel = new ErpGetRequestModel
                    {
                        Start = !string.IsNullOrWhiteSpace(stockCode) ? stockCode : start,
                        Location = salesOrg.Code,
                        DateFrom = isIncrementalSync ? salesOrg.LastErpProductSyncTimeOnUtc : null,
                        Limit = !string.IsNullOrWhiteSpace(stockCode) ? 1 : 100
                    };

                    var response = await erpIntegrationPlugin.GetProductsFromErpAsync(erpGetRequestModel);

                    if (response.ErpResponseModel.IsError)
                    {
                        isError = true;

                        await _erpSyncLogService.SyncLogSaveOnFileAsync(
                            syncTaskName,
                            ErpSyncLevel.Product,
                            response.ErpResponseModel.ErrorShortMessage,
                            response.ErpResponseModel.ErrorFullMessage);

                        await _syncWorkflowMessageService.SendSyncFailNotificationAsync(
                            DateTime.UtcNow,
                            syncTaskName,
                            response.ErpResponseModel.ErrorShortMessage + "\n\n" + response.ErpResponseModel.ErrorFullMessage);

                        break;
                    }
                    else if (response.Data is null)
                    {
                        isError = false;
                        break;
                    }

                    previousStart = start;
                    start = response.ErpResponseModel.Next;

                    var responseData = response.Data
                        .Where(x => !string.IsNullOrWhiteSpace(x.Sku))
                        .GroupBy(x => x.Sku)
                        .Select(g => g.Last());

                    if (responseData == null)
                    {
                        isError = false;
                        break;
                    }

                    totalNotSyncedSoFar += response.Data.Count - responseData.Count();

                    products = (List<Product>)await _erpProductService
                            .GetProductsBySkuAsync(
                                responseData.Select(x => x.Sku.Trim().ToLower()).ToArray(),
                                filterOutDeleted: true,
                                filterOutUnpublished: false);

                    int? specialsCategoryId = null;
                    if (salesOrg != null &&
                        salesOrg.SpecialsCategoryId.HasValue &&
                        salesOrg.SpecialsCategoryId != 0)
                    {
                        specialsCategoryId = allCategories.FirstOrDefault(c => c.Id == salesOrg.SpecialsCategoryId)?.Id ?? 0;
                    }

                    var nonSpecialProductIds = new List<int>();

                    foreach (var erpProduct in responseData)
                    {
                        #region Products

                        var thisProductIsValid = false;
                        var oldErpProduct = products.FirstOrDefault(x => x.Sku.Trim().ToLower() == erpProduct.Sku.Trim().ToLower());

                        if (oldErpProduct is null)
                        {
                            oldErpProduct = new Product();
                            MapProductFields(oldErpProduct,
                               erpProduct,
                               allVendors,
                               allTaxCategories,
                               programName,
                               lineBreakReplacer,
                               productTemplate?.Id ?? 0);

                            if (await IsthisProductIsValidAsync(oldErpProduct, syncTaskName))
                            {
                                thisProductIsValid = true;
                                await _productService.InsertProductAsync(oldErpProduct);
                            }
                        }
                        else
                        {
                            var specSheetUrl = GetSpecSheetURL(erpProduct, currStore);
                            if (!string.IsNullOrEmpty(specSheetUrl))
                            {
                                oldErpProduct.ShortDescription = specSheetUrl;
                                oldErpProduct.FullDescription = specSheetUrl;
                            }

                            MapProductFields(oldErpProduct,
                               erpProduct,
                               allVendors,
                               allTaxCategories,
                               programName,
                               lineBreakReplacer,
                               productTemplate?.Id ?? 0);

                            if (await IsthisProductIsValidAsync(oldErpProduct, syncTaskName))
                            {
                                thisProductIsValid = true;
                                await _productService.UpdateProductAsync(oldErpProduct);
                            }
                        }

                        //search engine name
                        await SaveOrUpdateEntitySeNameAsync(oldErpProduct);

                        if (!erpProduct.IsSpecial)
                        {
                            nonSpecialProductIds.Add(oldErpProduct.Id);
                        }

                        #endregion Products

                        #region Categories

                        if (erpProduct.ProductCategories.Any())
                        {
                            var incommingCategoryIds = new List<int>();
                            var categories = erpProduct.ProductCategories.ToList();
                            var parentCategoryId = 0;

                            for (int i = 0; i < categories.Count; i++)
                            {
                                if (string.IsNullOrWhiteSpace(categories[i].CategoryName))
                                {
                                    continue;
                                }

                                categories[i].CategoryName = categories[i].CategoryName.Trim();
                                var currentCategory = allCategories.FirstOrDefault
                                              (category => category.Name.ToLower().Trim() == categories[i].CategoryName.ToLower() &&
                                              category.ParentCategoryId == parentCategoryId);

                                if (currentCategory is null)
                                {
                                    currentCategory = new Category
                                    {
                                        Name = categories[i].CategoryName,
                                        Description = categories[i].Description,
                                        CategoryTemplateId = categoryTemplate?.Id ?? 0,
                                        MetaKeywords = categories[i].CategoryName,
                                        MetaDescription = categories[i].CategoryName,
                                        MetaTitle = categories[i].CategoryName,
                                        ParentCategoryId = parentCategoryId,
                                        PictureId = 0,
                                        PageSize = CATEGORY_PAGE_SIZE,
                                        AllowCustomersToSelectPageSize = true,
                                        PageSizeOptions = CATEGORY_PAGE_SIZE.ToString(),
                                        PriceRangeFiltering = true,
                                        ShowOnHomepage = false,
                                        IncludeInTopMenu = true,
                                        SubjectToAcl = false,
                                        LimitedToStores = false,
                                        Published = true,
                                        Deleted = false,
                                        DisplayOrder = 0,
                                        CreatedOnUtc = DateTime.UtcNow,
                                        UpdatedOnUtc = DateTime.UtcNow
                                    };


                                    if (await IsValidCategoryAsync(currentCategory, syncTaskName))
                                    {
                                        await _categoryService.InsertCategoryAsync(currentCategory);
                                        allCategories.Add(currentCategory);
                                    }
                                }
                                else
                                {
                                    currentCategory.Deleted = false;
                                    currentCategory.Published = true;
                                    currentCategory.UpdatedOnUtc = DateTime.UtcNow;
                                    currentCategory.IncludeInTopMenu = true;
                                    currentCategory.CategoryTemplateId = categoryTemplate?.Id ?? currentCategory.CategoryTemplateId;
                                    currentCategory.ParentCategoryId = parentCategoryId;

                                    await _categoryService.UpdateCategoryAsync(currentCategory);
                                }

                                if (await IsValidCategoryAsync(currentCategory, syncTaskName))
                                {
                                    //search engine name
                                    await SaveOrUpdateEntitySeNameAsync(currentCategory);

                                    parentCategoryId = currentCategory.Id;
                                    incommingCategoryIds.Add(currentCategory.Id);
                                }
                            }

                            /// 3 categories: A, A-B, A-B-C
                            /// product will be mapped with A-B-C only, the last one (leaf node)
                            if (thisProductIsValid)
                            {
                                var existingCategoryMapping = allProductCategories.Where(pc => pc.ProductId == oldErpProduct.Id).ToList();

                                await RemoveProductCategoriesExceptGivenAndSpecialsAsync(oldErpProduct.Id, incommingCategoryIds, syncTaskName);

                                // map with last category, leaf node
                                if (parentCategoryId > 0)
                                {
                                    var newProductCategory = new ProductCategory
                                    {
                                        ProductId = oldErpProduct.Id,
                                        CategoryId = parentCategoryId, // this is leaf node ID
                                        DisplayOrder = 0
                                    };

                                    await AddProductCategoryMappingAsync(newProductCategory, existingCategoryMapping, allProductCategories);
                                    await AddErpProductCategoryMappingAsync(newProductCategory.ProductId, newProductCategory.CategoryId);
                                }

                                // add mapping with special category
                                if (specialsCategoryId != null && erpProduct.IsSpecial)
                                {
                                    await AddProductMappingWithSpecialCategoryAsync(specialsCategoryId.Value, oldErpProduct.Id, salesOrg.Id, existingCategoryMapping, allProductCategories);
                                }
                            }
                        }

                        #endregion Categories

                        #region Specification Attribute

                        await UpdateProductSpecificationAttributesAsync(oldErpProduct.Id, erpProduct.ProductAttributes, syncTaskName);

                        #endregion Specification Attribute

                        #region Manufacturer

                        if (!string.IsNullOrWhiteSpace(erpProduct.ManufacturerName))
                        {
                            var existingProductManufacturers = await _manufacturerService.GetProductManufacturersByProductIdAsync(oldErpProduct.Id, showHidden: true);

                            var manufacturersWithSameName = allManufacturers
                                .Where(mft => mft.Name.ToLower().Trim().Equals(erpProduct.ManufacturerName.ToLower().Trim())
                                || mft.Name.ToLower().Trim().Equals(erpProduct.ManufacturerCode.ToLower().Trim()));

                            var currentManufacturer = manufacturersWithSameName.FirstOrDefault();

                            #region Delete all manufacturers with same name except the first one

                            foreach (var manufacturer in manufacturersWithSameName.Skip(1).ToList())
                            {
                                var productManufacturersToDelete = existingProductManufacturers
                                    .Where(pm => pm.ManufacturerId == manufacturer.Id)
                                    .ToList();

                                foreach (var productManufacturer in productManufacturersToDelete)
                                {
                                    existingProductManufacturers.Remove(productManufacturer);
                                    await _manufacturerService.DeleteProductManufacturerAsync(productManufacturer);
                                }

                                allManufacturers.Remove(manufacturer);
                                await _manufacturerService.DeleteManufacturerAsync(manufacturer);
                            }

                            #endregion Delete all manufacturers with same name except the first one

                            if (currentManufacturer is null)
                            {
                                currentManufacturer = new Manufacturer();
                                currentManufacturer.Name = erpProduct.ManufacturerName.Trim();
                                currentManufacturer.ManufacturerTemplateId = manufacturerTemplate?.Id ?? 1;
                                currentManufacturer.PageSize = MANUFACTURER_PAGE_SIZE;
                                currentManufacturer.AllowCustomersToSelectPageSize = true;
                                currentManufacturer.PageSizeOptions = MANUFACTURER_PAGE_SIZE_OPTIONS;
                                currentManufacturer.ManuallyPriceRange = true;
                                currentManufacturer.PriceFrom = NopCatalogDefaults.DefaultPriceRangeFrom;
                                currentManufacturer.PriceTo = NopCatalogDefaults.DefaultPriceRangeTo;
                                currentManufacturer.Published = true;
                                currentManufacturer.DisplayOrder = 1;
                                currentManufacturer.CreatedOnUtc = DateTime.UtcNow;
                                currentManufacturer.UpdatedOnUtc = DateTime.UtcNow;
                                currentManufacturer.Description = erpProduct.ManufacturerName.Trim();
                                currentManufacturer.MetaKeywords = erpProduct.ManufacturerName.Trim();
                                currentManufacturer.MetaDescription = erpProduct.ManufacturerName.Trim();
                                currentManufacturer.MetaTitle = erpProduct.ManufacturerName.Trim();

                                if (await IsValidManufacturerAsync(currentManufacturer, syncTaskName))
                                {
                                    await _manufacturerService.InsertManufacturerAsync(currentManufacturer);
                                    allManufacturers.Add(currentManufacturer);
                                }
                            }
                            else
                            {
                                currentManufacturer.Description = erpProduct.ManufacturerName.Trim();
                                currentManufacturer.ManufacturerTemplateId = 1;
                                currentManufacturer.MetaKeywords = erpProduct.ManufacturerName.Trim();
                                currentManufacturer.MetaDescription = erpProduct.ManufacturerName.Trim();
                                currentManufacturer.MetaTitle = erpProduct.ManufacturerName.Trim();
                                currentManufacturer.PictureId = 0;
                                currentManufacturer.PageSize = MANUFACTURER_PAGE_SIZE;
                                currentManufacturer.AllowCustomersToSelectPageSize = true;
                                currentManufacturer.PageSizeOptions = MANUFACTURER_PAGE_SIZE_OPTIONS;
                                currentManufacturer.PriceRangeFiltering = false;
                                currentManufacturer.SubjectToAcl = false;
                                currentManufacturer.LimitedToStores = false;
                                currentManufacturer.Published = true;
                                currentManufacturer.Deleted = false;
                                currentManufacturer.DisplayOrder = 0;
                                currentManufacturer.UpdatedOnUtc = DateTime.UtcNow;
                                await _manufacturerService.UpdateManufacturerAsync(currentManufacturer);
                            }

                            if (await IsValidManufacturerAsync(currentManufacturer, syncTaskName) && thisProductIsValid)
                            {
                                //search engine name
                                await SaveOrUpdateEntitySeNameAsync(currentManufacturer);

                                if (existingProductManufacturers.Any())
                                {
                                    var productManufacturer = existingProductManufacturers.FirstOrDefault();
                                    var productManufacturersToDelete = existingProductManufacturers.Skip(1).ToList();

                                    foreach (var prodMfct in productManufacturersToDelete)
                                    {
                                        existingProductManufacturers.Remove(prodMfct);
                                        await _manufacturerService.DeleteProductManufacturerAsync(prodMfct);
                                    }

                                    if (productManufacturer.ManufacturerId != currentManufacturer.Id)
                                    {
                                        productManufacturer.ManufacturerId = currentManufacturer.Id;
                                        await _manufacturerService.UpdateProductManufacturerAsync(productManufacturer);
                                    }
                                }
                                else
                                {
                                    var newProductManufacturer = new ProductManufacturer
                                    {
                                        ManufacturerId = currentManufacturer.Id,
                                        ProductId = oldErpProduct.Id,
                                        IsFeaturedProduct = false,
                                        DisplayOrder = 1
                                    };

                                    await _manufacturerService.InsertProductManufacturerAsync(newProductManufacturer);
                                }
                            }
                        }

                        #endregion Manufacturer

                        if (thisProductIsValid)
                        {
                            lastSyncedErpProduct = oldErpProduct.Sku;
                            totalSyncedSoFar++;
                        }
                        else
                        {
                            totalNotSyncedSoFar++;
                        }
                    }

                    await RemoveProductCategoryMappingsForNonSpecialProductsAsync(specialsCategoryId ?? 0, nonSpecialProductIds, syncTaskName);

                    if (cancellationToken.IsCancellationRequested)
                    {
                        await _erpSyncLogService.SyncLogSaveOnFileAsync(
                        syncTaskName,
                        ErpSyncLevel.Product,
                        $"The Erp Product Sync run is cancelled for Sales Org: ({salesOrg.Code}) {salesOrg.Name}." +
                        (!string.IsNullOrWhiteSpace(lastSyncedErpProduct) ?
                        $"The last synced Erp Product: {lastSyncedErpProduct} in this batch. " : string.Empty) +
                        $"Total products synced so far: {totalSyncedSoFar} " +
                        $"And total products not sync due to invalid data: {totalNotSyncedSoFar}");

                        return false;
                    }

                    await _erpSyncLogService.SyncLogSaveOnFileAsync(
                        syncTaskName,
                        ErpSyncLevel.Product,
                        (!string.IsNullOrWhiteSpace(lastSyncedErpProduct) ?
                        $"The last synced Erp Product: {lastSyncedErpProduct} in this batch. " : string.Empty) +
                        $"Total product synced so far: {totalSyncedSoFar}");

                    if ((!string.IsNullOrWhiteSpace(stockCode) && previousStart == start) || response.ErpResponseModel.Next == null)
                    {
                        isError = false;
                        break;
                    }
                }

                if (_b2BB2CFeaturesSettings.EnableUpdatingSkippedProductsPriceStockDuringProductSync)
                {
                    await UpdateSkipPriceStockProductAsync(DateTime.UtcNow, syncTaskName);
                }

                if (string.IsNullOrWhiteSpace(stockCode) && !isIncrementalSync)
                    await DeleteStockForLocationAsync(salesOrg.Code, syncStartTime, syncTaskName);

                if (!isError)
                {
                    if (string.IsNullOrWhiteSpace(stockCode) && !isIncrementalSync)
                    {
                        await _erpProductService.UnpublishAllOldProduct(syncStartTime);
                    }

                    await _erpSyncLogService.SyncLogSaveOnFileAsync(
                        syncTaskName,
                        ErpSyncLevel.Product,
                        $"Erp Product sync successful for Sales Org: ({salesOrg.Code}) {salesOrg.Name}");
                }
                else
                {
                    await _erpSyncLogService.SyncLogSaveOnFileAsync(
                        syncTaskName,
                        ErpSyncLevel.Product,
                        $"Erp Product sync is partially or not successful for Sales Org: ({salesOrg.Code}) {salesOrg.Name}");
                }

                await _erpSyncLogService.SyncLogSaveOnFileAsync(
                    syncTaskName,
                    ErpSyncLevel.Product,
                    (!string.IsNullOrWhiteSpace(lastSyncedErpProduct) ?
                    $"The last synced Erp Product: {lastSyncedErpProduct}. " : string.Empty) +
                    $"Total product synced so far: {totalSyncedSoFar} " +
                    $"And total products not sync due to invalid data: {totalNotSyncedSoFar}");

                salesOrg.LastErpProductSyncTimeOnUtc = DateTime.UtcNow;
                await _erpSalesOrgService.UpdateErpSalesOrgAsync(salesOrg);
            }

            await ExecuteStoredProcedureAsync(SP_PRODUCT_RUN_FINISHED, syncTaskName);

            await _staticCacheManager.RemoveByPrefixAsync("nop.pres.jcarousel.");
            await _erpSyncLogService.SyncLogSaveOnFileAsync(
                syncTaskName,
                ErpSyncLevel.Product,
                "Erp Product Sync ended.");

            return true;
        }
        catch (Exception ex)
        {
            await _staticCacheManager.RemoveByPrefixAsync("nop.pres.jcarousel.");

            await _erpSyncLogService.SyncLogSaveOnFileAsync(
                syncTaskName,
                ErpSyncLevel.Product,
                ex.Message,
                ex.StackTrace ?? string.Empty);

            await _syncWorkflowMessageService.SendSyncFailNotificationAsync(
                DateTime.UtcNow,
                syncTaskName,
                ex.Message + "\n\n" + ex.StackTrace);

            await _erpSyncLogService.SyncLogSaveOnFileAsync(
                syncTaskName,
                ErpSyncLevel.Product,
                "Erp Product Sync ended.");

            return false;
        }
    }

    #endregion Method
}