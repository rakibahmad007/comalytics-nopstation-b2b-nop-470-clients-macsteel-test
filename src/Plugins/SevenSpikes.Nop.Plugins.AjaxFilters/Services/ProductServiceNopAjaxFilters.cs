using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using LinqToDB;
using LinqToDB.Data;
using Nop.Core;
using Nop.Core.Caching;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Vendors;
using Nop.Data;
using Nop.Services.Customers;
using Nop.Services.Localization;
using NopStation.Plugin.B2B.B2BB2CFeatures.Services.ErpCustomerFunctionality;
using NopStation.Plugin.B2B.ERPIntegrationCore.Domain;
using NopStation.Plugin.B2B.ERPIntegrationCore.Services;
using SevenSpikes.Nop.Plugins.AjaxFilters.Domain;
using SevenSpikes.Nop.Plugins.AjaxFilters.Helpers;
using SevenSpikes.Nop.Services.Catalog;
using SevenSpikes.Nop.Services.Catalog.DTO;
using SevenSpikes.Nop.Services.Helpers;

namespace SevenSpikes.Nop.Plugins.AjaxFilters.Services;

public class ProductServiceNopAjaxFilters : IProductServiceNopAjaxFilters
{
    private readonly ILanguageService _languageService;

    private readonly INopDataProvider _dataProvider;

    private readonly IWorkContext _workContext;

    private readonly IStoreContext _storeContext;

    private readonly ICategoryService7Spikes _categoryService7Spikes;

    private readonly IRepository<Product> _productRepository;

    private readonly IRepository<ProductCategory> _productCategoryRepository;

    private readonly IRepository<ProductManufacturer> _productManufacturerRepository;

    private readonly IRepository<Manufacturer> _manufacturerRepository;

    private readonly IRepository<Vendor> _vendorRepository;

    private readonly IRepository<ProductWarehouseInventory> _productWarehouseInventory;

    private readonly CommonSettings _commonSettings;

    private readonly CatalogSettings _catalogSettings;

    private readonly NopAjaxFiltersSettings _nopAjaxFiltersSettings;

    private IStaticCacheManager _cacheManager;

    private IAclHelper _aclHelper;

    private readonly ICustomerService _customerService;

    private readonly ICustomAclHelper _customAclHelper;

    private readonly IErpCustomerFunctionalityService _erpCustomerFunctionalityService;

    private readonly IRepository<ErpWarehouseSalesOrgMap> _erpWarehouseSalesOrgMapRepository;
    private readonly IRepository<ProductWarehouseInventory> _productWarehouseInventoryRepository;

    private CacheKey GROUPPRODUCTIDS_BY_CATEGORYIDS =>
        new CacheKey("nop.groupproductids.categoryids.{0}", Array.Empty<string>());

    private CacheKey GROUPPRODUCTIDS_BY_MANUFACTURERID =>
        new CacheKey("nop.groupproductids.manufacturerid.{0}", Array.Empty<string>());

    private CacheKey GROUPPRODUCTIDS_BY_VENDORID =>
        new CacheKey("nop.groupproductids.vendorid.{0}", Array.Empty<string>());

    private CacheKey ONSALESTATE_BY_PRODUCTIDS_KEY =>
        new CacheKey("Nop.onsalestate.productids-{0}-{1}-{2}", Array.Empty<string>());

    private CacheKey INSTOCKSTATE_BY_PRODUCTIDS_KEY =>
        new CacheKey("nop.instock.productids-{0}-{1}-{2}", Array.Empty<string>());

    public ProductServiceNopAjaxFilters(
        ILanguageService languageService,
        INopDataProvider dataProvider,
        ICategoryService7Spikes categoryService7Spikes,
        IRepository<Product> productRepository,
        IRepository<ProductManufacturer> productManufacturer,
        IRepository<Manufacturer> manufacturerRepository,
        IRepository<Vendor> vendorRepository,
        IRepository<ProductCategory> productCategoryRepository,
        IRepository<ProductWarehouseInventory> productWarehouseInventory,
        IWorkContext workContext,
        IStoreContext storeContext,
        CommonSettings commonSettings,
        CatalogSettings catalogSettings,
        NopAjaxFiltersSettings nopAjaxFiltersSettings,
        IStaticCacheManager cacheManager,
        IAclHelper aclHelper,
        ICustomerService customerService,
        ICustomAclHelper customAclHelper,
        IErpCustomerFunctionalityService erpCustomerFunctionalityService,
        IRepository<ErpWarehouseSalesOrgMap> erpWarehouseSalesOrgMapRepository,
        IRepository<ProductWarehouseInventory> productWarehouseInventoryRepository
    )
    {
        _languageService = languageService;
        _dataProvider = dataProvider;
        _categoryService7Spikes = categoryService7Spikes;
        _productRepository = productRepository;
        _productCategoryRepository = productCategoryRepository;
        _productManufacturerRepository = productManufacturer;
        _manufacturerRepository = manufacturerRepository;
        _vendorRepository = vendorRepository;
        _productWarehouseInventory = productWarehouseInventory;
        _workContext = workContext;
        _storeContext = storeContext;
        _commonSettings = commonSettings;
        _cacheManager = cacheManager;
        _aclHelper = aclHelper;
        _nopAjaxFiltersSettings = nopAjaxFiltersSettings;
        _catalogSettings = catalogSettings;
        _customerService = customerService;
        _customAclHelper = customAclHelper;
        _erpCustomerFunctionalityService = erpCustomerFunctionalityService;
        _erpWarehouseSalesOrgMapRepository = erpWarehouseSalesOrgMapRepository;
        _productWarehouseInventoryRepository = productWarehouseInventoryRepository;
    }

    public virtual async Task<ProductsResultDataDTO> SearchProductsAsync(
        IList<int> categoryIds,
        SpecificationFilterModelDTO specifiationFilterModelDTO,
        AttributeFilterModelDTO attributeFilterModelDTO,
        ManufacturerFilterModelDTO manufacturerFilterModelDTO,
        VendorFilterModelDTO vendorFilterModelDTO,
        int pageIndex = 0,
        int pageSize = int.MaxValue,
        int manufacturerId = 0,
        int vendorId = 0,
        int storeId = 0,
        bool? featuredProducts = null,
        decimal? priceMin = null,
        decimal? priceMax = null,
        int productTagId = 0,
        string keywords = null,
        bool searchDescriptions = false,
        bool searchSku = false,
        bool searchProductTags = false,
        int languageId = 0,
        ProductSortingEnum orderBy = 0,
        bool showHidden = false,
        bool onSale = false,
        bool inStock = false
    )
    {
        //IL_00be: Unknown result type (might be due to invalid IL or missing references)
        //IL_00c0: Unknown result type (might be due to invalid IL or missing references)
        return await SearchProductsInternalAsync(
            categoryIds,
            specifiationFilterModelDTO,
            attributeFilterModelDTO,
            manufacturerFilterModelDTO,
            vendorFilterModelDTO,
            pageIndex,
            pageSize,
            manufacturerId,
            vendorId,
            storeId,
            featuredProducts,
            priceMin,
            priceMax,
            productTagId,
            keywords,
            searchDescriptions,
            searchSku,
            searchProductTags,
            languageId,
            orderBy,
            showHidden,
            onSale,
            inStock
        );
    }

    public async Task<bool> HasProductsOnSaleAsync(int categoryId, int manufacturerId, int vendorId)
    {
        return await HasProductsOnSaleInternalAsync(categoryId, manufacturerId, vendorId);
    }

    public async Task<bool> HasProductsInStockAsync(
        int categoryId,
        int manufacturerId,
        int vendorId
    )
    {
        return await HasProductsInStockInternalAsync(categoryId, manufacturerId, vendorId);
    }

    private async Task<ProductsResultDataDTO> SearchProductsInternalAsync(
        IList<int> categoryIds,
        SpecificationFilterModelDTO specifiationFilterModelDTO,
        AttributeFilterModelDTO attributeFilterModelDTO,
        ManufacturerFilterModelDTO manufacturerFilterModelDTO,
        VendorFilterModelDTO vendorFilterModelDTO,
        int pageIndex = 0,
        int pageSize = int.MaxValue,
        int manufacturerId = 0,
        int vendorId = 0,
        int storeId = 0,
        bool? featuredProducts = null,
        decimal? priceMin = null,
        decimal? priceMax = null,
        int productTagId = 0,
        string keywords = null,
        bool searchDescriptions = false,
        bool searchSku = false,
        bool searchProductTags = false,
        int languageId = 0,
        ProductSortingEnum orderBy = 0,
        bool showHidden = false,
        bool onSale = false,
        bool inStock = false
    )
    {
        //IL_00be: Unknown result type (might be due to invalid IL or missing references)
        //IL_00c0: Unknown result type (might be due to invalid IL or missing references)

        var model = await _erpCustomerFunctionalityService.GetErpFilterInfoModel();
        bool searchLocalizedValue = false;
        if (languageId > 0)
        {
            if (showHidden)
            {
                searchLocalizedValue = true;
            }
            else
            {
                ILanguageService languageService = _languageService;
                int count = (
                    await languageService.GetAllLanguagesAsync(
                        false,
                        ((BaseEntity)(await _storeContext.GetCurrentStoreAsync())).Id
                    )
                ).Count;
                searchLocalizedValue = count >= 2;
            }
        }
        if (categoryIds != null && categoryIds.Contains(0))
        {
            categoryIds.Remove(0);
        }
        ICustomerService customerService = _customerService;
        int[] values = await customerService.GetCustomerRoleIdsAsync(
            await _workContext.GetCurrentCustomerAsync(),
            false
        );
        string text = (_catalogSettings.IgnoreAcl ? string.Empty : string.Join(",", values));
        string parameterValue = (
            (categoryIds == null) ? string.Empty : string.Join(",", categoryIds)
        );
        string text2 = "";
        if (
            specifiationFilterModelDTO != null
            && specifiationFilterModelDTO.SpecificationFilterDTOs.Any()
        )
        {
            List<int> list = specifiationFilterModelDTO
                .SpecificationFilterDTOs.SelectMany(
                    (SpecificationFilterDTO x) => x.SelectedFilterIds
                )
                .ToList();
            list.Sort();
            for (int i = 0; i < list.Count; i++)
            {
                text2 += list[i];
                if (i != list.Count - 1)
                {
                    text2 += ",";
                }
            }
        }
        string text3 = "";
        if (attributeFilterModelDTO != null && attributeFilterModelDTO.AttributeFilterDTOs.Any())
        {
            List<int> list2 = attributeFilterModelDTO
                .AttributeFilterDTOs.SelectMany(
                    (AttributeFilterDTO x) => x.SelectedProductVariantIds
                )
                .ToList();
            list2.Sort();
            for (int j = 0; j < list2.Count; j++)
            {
                text3 += list2[j];
                if (j != list2.Count - 1)
                {
                    text3 += ",";
                }
            }
        }
        string text4 = "";
        if (
            manufacturerFilterModelDTO != null
            && manufacturerFilterModelDTO.SelectedFilterIds.Any()
        )
        {
            List<int> list3 = manufacturerFilterModelDTO.SelectedFilterIds.ToList();
            list3.Sort();
            for (int k = 0; k < list3.Count; k++)
            {
                text4 += list3[k];
                if (k != list3.Count - 1)
                {
                    text4 += ",";
                }
            }
        }
        string text5 = "";
        if (vendorFilterModelDTO != null && vendorFilterModelDTO.SelectedFilterIds.Any())
        {
            List<int> list4 = vendorFilterModelDTO.SelectedFilterIds.ToList();
            list4.Sort();
            for (int l = 0; l < list4.Count; l++)
            {
                text5 += list4[l];
                if (l != list4.Count - 1)
                {
                    text5 += ",";
                }
            }
        }
        if (pageSize == int.MaxValue)
        {
            pageSize = 2147483646;
        }

        #region B2B Account

        var pIsB2BAccount = SqlParameterHelper.GetBooleanParameter(
            "IsB2BAccount",
            model.IsErpAccount
        );

        var pB2BAccountId = SqlParameterHelper.GetInt32Parameter(
            "B2BAccountId",
            model.ErpAccountId
        );

        var pB2BSalesOrganisationId = SqlParameterHelper.GetInt32Parameter(
            "B2BSalesOrganisationId",
            model.ErpSalesOrganisationId
        );

        var pUsePriceGroupPricing = SqlParameterHelper.GetBooleanParameter(
            "UsePriceGroupPricing",
            model.UsePriceGroupPricing
        );

        var pPriceGroupCodeId = SqlParameterHelper.GetInt32Parameter(
            "PriceGroupCodeId",
            model.PriceGroupCodeId
        );

        //pass specification identifiers as comma-delimited string
        var commaSeparatedPreFilterFacetSpecIds = string.Empty;
        if (model.PreFilterFacetSpecIds != null && model.PreFilterFacetSpecIds.Count > 0)
        {
            ((List<int>)model.PreFilterFacetSpecIds).Sort();
            commaSeparatedPreFilterFacetSpecIds = string.Join(",", model.PreFilterFacetSpecIds);
        }

        var pPreFilterFacetSpecIds = SqlParameterHelper.GetStringParameter(
            "PreFilterFacetSpecIds",
            commaSeparatedPreFilterFacetSpecIds
        );

        //pass special exclusion specification identifiers as comma-delimited string
        var commaSeparatedSpecialExcludeSpecIds = string.Empty;
        if (model.SpecialExcludeSpecIds != null && model.SpecialExcludeSpecIds.Count > 0)
        {
            ((List<int>)model.SpecialExcludeSpecIds).Sort();
            commaSeparatedSpecialExcludeSpecIds = string.Join(",", model.SpecialExcludeSpecIds);
        }

        var pSpecialExcludeSpecIds = SqlParameterHelper.GetStringParameter(
            "SpecialExcludeSpecIds",
            commaSeparatedSpecialExcludeSpecIds
        );

        #endregion B2B Account

        DataParameter stringParameter = SqlParameterHelper.GetStringParameter(
            "CategoryIds",
            parameterValue
        );
        DataParameter int32Parameter = SqlParameterHelper.GetInt32Parameter(
            "ManufacturerId",
            manufacturerId
        );
        DataParameter int32Parameter2 = SqlParameterHelper.GetInt32Parameter(
            "StoreId",
            (!_catalogSettings.IgnoreStoreLimitations) ? storeId : 0
        );
        DataParameter int32Parameter3 = SqlParameterHelper.GetInt32Parameter("VendorId", vendorId);
        DataParameter int32Parameter4 = SqlParameterHelper.GetInt32Parameter(
            "ParentGroupedProductId",
            0
        );
        DataParameter int32Parameter5 = SqlParameterHelper.GetInt32Parameter("ProductTypeId", null);
        DataParameter booleanParameter = SqlParameterHelper.GetBooleanParameter(
            "VisibleIndividuallyOnly",
            true
        );
        DataParameter int32Parameter6 = SqlParameterHelper.GetInt32Parameter(
            "ProductTagId",
            productTagId
        );
        DataParameter booleanParameter2 = SqlParameterHelper.GetBooleanParameter(
            "FeaturedProducts",
            featuredProducts
        );
        DataParameter decimalParameter = SqlParameterHelper.GetDecimalParameter(
            "PriceMin",
            priceMin
        );
        DataParameter decimalParameter2 = SqlParameterHelper.GetDecimalParameter(
            "PriceMax",
            priceMax
        );
        DataParameter stringParameter2 = SqlParameterHelper.GetStringParameter(
            "Keywords",
            keywords
        );
        DataParameter booleanParameter3 = SqlParameterHelper.GetBooleanParameter(
            "SearchDescriptions",
            searchDescriptions
        );
        DataParameter booleanParameter4 = SqlParameterHelper.GetBooleanParameter(
            "SearchManufacturerPartNumber",
            true
        );
        DataParameter booleanParameter5 = SqlParameterHelper.GetBooleanParameter(
            "SearchSku",
            searchSku
        );
        DataParameter booleanParameter6 = SqlParameterHelper.GetBooleanParameter(
            "SearchProductTags",
            searchProductTags
        );
        DataParameter booleanParameter7 = SqlParameterHelper.GetBooleanParameter(
            "UseFullTextSearch",
            false
        );
        DataParameter int32Parameter7 = SqlParameterHelper.GetInt32Parameter("FullTextMode", 0);
        DataParameter stringParameter3 = SqlParameterHelper.GetStringParameter(
            "FilteredSpecs",
            text2
        );
        DataParameter stringParameter4 = SqlParameterHelper.GetStringParameter(
            "FilteredProductVariantAttributes",
            text3
        );
        DataParameter stringParameter5 = SqlParameterHelper.GetStringParameter(
            "FilteredManufacturers",
            text4
        );
        DataParameter stringParameter6 = SqlParameterHelper.GetStringParameter(
            "FilteredVendors",
            text5
        );
        DataParameter booleanParameter8 = SqlParameterHelper.GetBooleanParameter("OnSale", onSale);
        DataParameter booleanParameter9 = SqlParameterHelper.GetBooleanParameter(
            "InStock",
            inStock
        );
        DataParameter int32Parameter8 = SqlParameterHelper.GetInt32Parameter(
            "LanguageId",
            searchLocalizedValue ? languageId : 0
        );
        DataParameter int32Parameter9 = SqlParameterHelper.GetInt32Parameter(
            "OrderBy",
            (int)orderBy
        );
        DataParameter stringParameter7 = SqlParameterHelper.GetStringParameter(
            "AllowedCustomerRoleIds",
            (!_catalogSettings.IgnoreAcl) ? text : string.Empty
        );
        DataParameter int32Parameter10 = SqlParameterHelper.GetInt32Parameter(
            "PageIndex",
            pageIndex
        );
        DataParameter int32Parameter11 = SqlParameterHelper.GetInt32Parameter("PageSize", pageSize);
        DataParameter booleanParameter10 = SqlParameterHelper.GetBooleanParameter(
            "ShowHidden",
            showHidden
        );
        DataParameter booleanParameter11 = SqlParameterHelper.GetBooleanParameter(
            "LoadAvailableFilters",
            true
        );
        DataParameter pFilterableSpecificationAttributeOptionIds =
            SqlParameterHelper.GetOutputStringParameter(
                "FilterableSpecificationAttributeOptionIds"
            );
        pFilterableSpecificationAttributeOptionIds.Size = 2147483646;
        DataParameter pFilterableProductVariantAttributeIds =
            SqlParameterHelper.GetOutputStringParameter("FilterableProductVariantAttributeIds");
        pFilterableProductVariantAttributeIds.Size = 2147483646;
        DataParameter pFilterableManufacturerIds = SqlParameterHelper.GetOutputStringParameter(
            "FilterableManufacturerIds"
        );
        pFilterableManufacturerIds.Size = 2147483646;
        DataParameter pFilterableVendorIds = SqlParameterHelper.GetOutputStringParameter(
            "FilterableVendorIds"
        );
        pFilterableVendorIds.Size = 2147483646;
        DataParameter booleanParameter12 = SqlParameterHelper.GetBooleanParameter(
            "IsOnSaleFilterEnabled",
            _nopAjaxFiltersSettings.EnableOnSaleFilter
        );
        DataParameter booleanParameter13 = SqlParameterHelper.GetBooleanParameter(
            "IsInStockFilterEnabled",
            _nopAjaxFiltersSettings.EnableInStockFilter
        );
        DataParameter pHasProductsOnSale = new DataParameter
        {
            Name = "HasProductsOnSale",
            DataType = (DataType)11,
            Direction = ParameterDirection.Output,
        };
        DataParameter pHasProductsInStock = new DataParameter
        {
            Name = "HasProductsInStock",
            DataType = (DataType)11,
            Direction = ParameterDirection.Output,
        };
        DataParameter pTotalRecords = SqlParameterHelper.GetOutputInt32Parameter("TotalRecords");
        List<Product> list5 = (
            await _dataProvider.QueryProcAsync<Product>(
                "ErpProductLoadAllPagedNopAjaxFilters",
                (DataParameter[])
                    (object)
                        new DataParameter[47]
                        {
                            stringParameter,
                            int32Parameter,
                            int32Parameter2,
                            int32Parameter3,
                            int32Parameter4,
                            int32Parameter5,
                            booleanParameter,
                            int32Parameter6,
                            booleanParameter2,
                            decimalParameter,
                            decimalParameter2,
                            stringParameter2,
                            booleanParameter3,
                            booleanParameter4,
                            booleanParameter5,
                            booleanParameter6,
                            booleanParameter7,
                            int32Parameter7,
                            stringParameter3,
                            stringParameter4,
                            stringParameter5,
                            stringParameter6,
                            booleanParameter8,
                            booleanParameter9,
                            int32Parameter8,
                            int32Parameter9,
                            stringParameter7,
                            int32Parameter10,
                            int32Parameter11,
                            booleanParameter10,
                            booleanParameter11,
                            pIsB2BAccount,
                            pB2BAccountId,
                            pB2BSalesOrganisationId,
                            pUsePriceGroupPricing,
                            pPriceGroupCodeId,
                            pPreFilterFacetSpecIds,
                            pSpecialExcludeSpecIds,
                            pFilterableSpecificationAttributeOptionIds,
                            pFilterableProductVariantAttributeIds,
                            pFilterableManufacturerIds,
                            pFilterableVendorIds,
                            booleanParameter12,
                            booleanParameter13,
                            pHasProductsOnSale,
                            pHasProductsInStock,
                            pTotalRecords,
                        }
            )
        ).ToList();
        List<int> specificationOptionIds = new List<int>();
        string text6 = (
            (pFilterableSpecificationAttributeOptionIds.Value != DBNull.Value)
                ? ((string)pFilterableSpecificationAttributeOptionIds.Value)
                : ""
        );
        if (!string.IsNullOrWhiteSpace(text6))
        {
            specificationOptionIds = (
                from x in text6.Split(new char[1] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                select Convert.ToInt32(x.Trim())
            ).ToList();
        }
        List<int> productVariantIds = new List<int>();
        string text7 = (
            (pFilterableProductVariantAttributeIds.Value != DBNull.Value)
                ? ((string)pFilterableProductVariantAttributeIds.Value)
                : ""
        );
        if (!string.IsNullOrWhiteSpace(text7))
        {
            productVariantIds = (
                from x in text7.Split(new char[1] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                select Convert.ToInt32(x.Trim())
            ).ToList();
        }
        List<int> manufacturerIds = new List<int>();
        string text8 = (
            (pFilterableManufacturerIds.Value != DBNull.Value)
                ? ((string)pFilterableManufacturerIds.Value)
                : ""
        );
        if (!string.IsNullOrWhiteSpace(text8))
        {
            manufacturerIds = (
                from x in text8.Split(new char[1] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                select Convert.ToInt32(x.Trim())
            ).ToList();
        }
        List<int> vendorIds = new List<int>();
        string text9 = (
            (pFilterableVendorIds.Value != DBNull.Value) ? ((string)pFilterableVendorIds.Value) : ""
        );
        if (!string.IsNullOrWhiteSpace(text9))
        {
            vendorIds = (
                from x in text9.Split(new char[1] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                select Convert.ToInt32(x.Trim())
            ).ToList();
        }
        int value = (
            (pTotalRecords.Value != DBNull.Value) ? Convert.ToInt32(pTotalRecords.Value) : 0
        );
        bool hasProductsOnSale =
            pHasProductsOnSale.Value != DBNull.Value && Convert.ToBoolean(pHasProductsOnSale.Value);
        bool hasProductsInStock =
            pHasProductsInStock.Value != DBNull.Value
            && Convert.ToBoolean(pHasProductsInStock.Value);
        PagedList<Product> productsPagedList = new PagedList<Product>(
            (IList<Product>)list5,
            pageIndex,
            pageSize,
            (int?)value
        );
        return new ProductsResultDataDTO
        {
            ProductsPagedList = (IPagedList<Product>)(object)productsPagedList,
            SpecificationOptionIds = specificationOptionIds,
            ProductVariantIds = productVariantIds,
            ManufacturerIds = manufacturerIds,
            VendorIds = vendorIds,
            HasProductsOnSale = hasProductsOnSale,
            HasProductsInStock = hasProductsInStock,
        };
    }

    public async Task<IList<int>> GetAllGroupProductIdsInCategoriesAsync(List<int> categoriesIds)
    {
        return (await GetAllGroupProductIdsInCategoriesInternalAsync(categoriesIds)).ToList();
    }

    public async Task<IList<int>> GetAllGroupProductIdsInCategoryAsync(int categoryId)
    {
        return (
            await GetAllGroupProductIdsInCategoriesInternalAsync(new List<int> { categoryId })
        ).ToList();
    }

    public async Task<IList<int>> GetAllGroupProductIdsInManufacturerAsync(int manufacturerId)
    {
        return (await GetAllGroupProductIdsInManufacturerInternalAsync(manufacturerId)).ToList();
    }

    public async Task<IList<int>> GetAllGroupProductIdsInVendorAsync(int vendorId)
    {
        return (await GetAllGroupProductIdsInVendorInternalAsync(vendorId)).ToList();
    }

    public async Task<bool> HasProductsOnSaleInternalAsync(
        int categoryId,
        int manufacturerId,
        int vendorId
    )
    {
        CacheKey val = ((ICacheKeyService)_cacheManager).PrepareKeyForDefaultCache(
            ONSALESTATE_BY_PRODUCTIDS_KEY,
            new object[3] { categoryId, manufacturerId, vendorId }
        );
        return await _cacheManager.GetAsync<bool>(
            val,
            (Func<Task<bool>>)
                async delegate
                {
                    bool hasProducts = false;
                    IQueryable<Product> availableProducts =
                        await _customAclHelper.GetAvailableProductsForCurrentErpCustomerAsync();
                    bool showProductsFromSubcategories =
                        _catalogSettings.ShowProductsFromSubcategories;
                    bool includeFeaturedProducts =
                        _catalogSettings.IncludeFeaturedProductsInNormalLists;
                    if (categoryId > 0)
                    {
                        List<int> categoryIds = new List<int> { categoryId };
                        if (showProductsFromSubcategories)
                        {
                            IEnumerable<int> collection = (
                                await _categoryService7Spikes.GetCategoriesByParentCategoryIdAsync(
                                    categoryId,
                                    includeSubCategoriesFromAllLevels: true
                                )
                            ).Select((Category x) => ((BaseEntity)x).Id);
                            categoryIds.AddRange(collection);
                        }
                        IList<int> groupProductIds =
                            await GetAllGroupProductIdsInCategoriesInternalAsync(categoryIds);
                        hasProducts = await HasAvailableProductsOnSaleInCategoryAsync(
                            availableProducts,
                            groupProductIds,
                            categoryIds,
                            includeFeaturedProducts
                        );
                    }
                    else if (manufacturerId > 0)
                    {
                        IList<int> groupProductIds2 =
                            await GetAllGroupProductIdsInManufacturerInternalAsync(manufacturerId);
                        hasProducts = await HasAvailableProductsOnSaleInManufacturerAsync(
                            availableProducts,
                            groupProductIds2,
                            manufacturerId,
                            includeFeaturedProducts
                        );
                    }
                    else if (vendorId > 0)
                    {
                        IList<int> groupProductIds3 =
                            await GetAllGroupProductIdsInVendorInternalAsync(vendorId);
                        hasProducts = await HasAvailableProductsOnSaleInVendorAsync(
                            availableProducts,
                            groupProductIds3,
                            vendorId,
                            includeFeaturedProducts
                        );
                    }
                    return hasProducts;
                }
        );
    }

    public async Task<bool> HasProductsInStockInternalAsync(
        int categoryId,
        int manufacturerId,
        int vendorId
    )
    {
        CacheKey key = _cacheManager.PrepareKeyForDefaultCache(
            INSTOCKSTATE_BY_PRODUCTIDS_KEY,
            categoryId,
            manufacturerId,
            vendorId
        );
        return await _cacheManager.GetAsync(
            key,
            async delegate
            {
                bool hasProducts = false;
                IQueryable<Product> availableProducts =
                    await _customAclHelper.GetAvailableProductsForCurrentErpCustomerAsync();
                bool showProductsFromSubcategories = _catalogSettings.ShowProductsFromSubcategories;
                bool includeFeaturedProducts =
                    _catalogSettings.IncludeFeaturedProductsInNormalLists;
                if (categoryId > 0)
                {
                    List<int> categoryIds = new List<int> { categoryId };
                    if (showProductsFromSubcategories)
                    {
                        IEnumerable<int> collection = (
                            await _categoryService7Spikes.GetCategoriesByParentCategoryIdAsync(
                                categoryId,
                                includeSubCategoriesFromAllLevels: true
                            )
                        ).Select((Category x) => x.Id);
                        categoryIds.AddRange(collection);
                    }
                    IList<int> groupProductIds =
                        await GetAllGroupProductIdsInCategoriesInternalAsync(categoryIds);
                    hasProducts = await HasAvailableProductsInStockInCategoryAsync(
                        availableProducts,
                        groupProductIds,
                        categoryIds,
                        includeFeaturedProducts
                    );
                }
                else if (manufacturerId > 0)
                {
                    IList<int> groupProductIds2 =
                        await GetAllGroupProductIdsInManufacturerInternalAsync(manufacturerId);
                    hasProducts = await HasAvailableProductsInStockInManufacturerAsync(
                        availableProducts,
                        groupProductIds2,
                        manufacturerId,
                        includeFeaturedProducts
                    );
                }
                else if (vendorId > 0)
                {
                    IList<int> groupProductIds3 = await GetAllGroupProductIdsInVendorInternalAsync(
                        vendorId
                    );
                    hasProducts = await HasAvailableProductsInStockInVendorAsync(
                        availableProducts,
                        groupProductIds3,
                        vendorId,
                        includeFeaturedProducts
                    );
                }
                return hasProducts;
            }
        );
    }

    private async Task<bool> HasAvailableProductsInStockInVendorAsync(
        IQueryable<Product> availableProducts,
        IList<int> groupProductIds,
        int vendorId,
        bool includeFeaturedProducts
    )
    {
        return true;
    }

    private async Task<bool> HasAvailableProductsInStockInManufacturerAsync(
        IQueryable<Product> availableProducts,
        IList<int> groupProductIds,
        int manufacturerId,
        bool includeFeaturedProducts
    )
    {
        return true;
    }

    private IQueryable<Product> GetProductsFilteredByInventory(
        IQueryable<Product> availableProducts,
        IList<int> groupProductIds,
        List<int> categoryIds,
        bool includeFeaturedProducts,
        int salesOrgId
    )
    {
        var warehouseIds = _erpWarehouseSalesOrgMapRepository
            .Table.Where(wsm => wsm.ErpSalesOrgId == salesOrgId)
            .Select(wsm => wsm.NopWarehouseId);

        var productIdsWithAvailableStock = _productWarehouseInventoryRepository
            .Table.Where(pwi =>
                warehouseIds.Contains(pwi.WarehouseId)
                && pwi.StockQuantity > 0
                && pwi.StockQuantity > pwi.ReservedQuantity
            )
            .Select(pwi => pwi.ProductId);

        return from pc in _productCategoryRepository.Table
            join p in availableProducts on pc.ProductId equals p.Id
            where
                pc != null
                    && categoryIds.Contains(pc.CategoryId)
                    && p.ProductTypeId != 10
                    && p.VisibleIndividually
                    && (pc.IsFeaturedProduct == includeFeaturedProducts || !pc.IsFeaturedProduct)
                || groupProductIds.Contains(p.ParentGroupedProductId)
                    && p.Published
                    && !p.Deleted
                    && (
                        p.ManageInventoryMethodId == 0
                        || (
                            p.ManageInventoryMethodId == 1
                            && (
                                (p.StockQuantity > 0 && !p.UseMultipleWarehouses)
                                || (
                                    productIdsWithAvailableStock.Contains(p.Id)
                                    && p.UseMultipleWarehouses
                                )
                            )
                        )
                    )
            select p;
    }

    private async Task<bool> HasAvailableProductsInStockInCategoryAsync(
        IQueryable<Product> availableProducts,
        IList<int> groupProductIds,
        List<int> categoryIds,
        bool includeFeaturedProducts
    )
    {
        #region B2B

        var currCustomer = await _workContext.GetCurrentCustomerAsync();
        var erpAccount = await _erpCustomerFunctionalityService.GetActiveErpAccountByCustomerAsync(
            currCustomer
        );
        var erpNopUser = await _erpCustomerFunctionalityService.GetActiveErpNopUserByCustomerAsync(
            currCustomer
        );
        if (erpAccount != null && erpNopUser != null)
        {
            return GetProductsFilteredByInventory(
                    availableProducts,
                    groupProductIds,
                    categoryIds,
                    includeFeaturedProducts,
                    erpAccount.ErpSalesOrgId
                )
                .Any();
        }

        #endregion B2B

        return await AsyncIQueryableExtensions.AnyAsync<Product>(
            from _003C_003Eh__TransparentIdentifier1 in Queryable.SelectMany(
                availableProducts.GroupJoin(
                    (IEnumerable<ProductCategory>)_productCategoryRepository.Table,
                    (Expression<Func<Product, int>>)((Product p) => ((BaseEntity)p).Id),
                    (Expression<Func<ProductCategory, int>>)((ProductCategory pc) => pc.ProductId),
                    (Product p, IEnumerable<ProductCategory> p_pc) => new { p, p_pc }
                ),
                _003C_003Eh__TransparentIdentifier0 =>
                    _003C_003Eh__TransparentIdentifier0.p_pc.DefaultIfEmpty(),
                (_003C_003Eh__TransparentIdentifier0, pc) =>
                    new { _003C_003Eh__TransparentIdentifier0, pc }
            )
            where
                (
                    (
                        _003C_003Eh__TransparentIdentifier1.pc != null
                        && categoryIds.Contains(_003C_003Eh__TransparentIdentifier1.pc.CategoryId)
                        && _003C_003Eh__TransparentIdentifier1
                            ._003C_003Eh__TransparentIdentifier0
                            .p
                            .ProductTypeId != 10
                        && _003C_003Eh__TransparentIdentifier1
                            ._003C_003Eh__TransparentIdentifier0
                            .p
                            .VisibleIndividually
                        && (
                            _003C_003Eh__TransparentIdentifier1.pc.IsFeaturedProduct
                                == includeFeaturedProducts
                            || !_003C_003Eh__TransparentIdentifier1.pc.IsFeaturedProduct
                        )
                    )
                    || groupProductIds.Contains(
                        _003C_003Eh__TransparentIdentifier1
                            ._003C_003Eh__TransparentIdentifier0
                            .p
                            .ParentGroupedProductId
                    )
                )
                && _003C_003Eh__TransparentIdentifier1
                    ._003C_003Eh__TransparentIdentifier0
                    .p
                    .Published
                && !_003C_003Eh__TransparentIdentifier1
                    ._003C_003Eh__TransparentIdentifier0
                    .p
                    .Deleted
                && (
                    _003C_003Eh__TransparentIdentifier1
                        ._003C_003Eh__TransparentIdentifier0
                        .p
                        .ManageInventoryMethodId == 0
                    || (
                        _003C_003Eh__TransparentIdentifier1
                            ._003C_003Eh__TransparentIdentifier0
                            .p
                            .ManageInventoryMethodId == 1
                        && (
                            (
                                _003C_003Eh__TransparentIdentifier1
                                    ._003C_003Eh__TransparentIdentifier0
                                    .p
                                    .StockQuantity > 0
                                && !_003C_003Eh__TransparentIdentifier1
                                    ._003C_003Eh__TransparentIdentifier0
                                    .p
                                    .UseMultipleWarehouses
                            )
                            || (
                                _003C_003Eh__TransparentIdentifier1
                                    ._003C_003Eh__TransparentIdentifier0
                                    .p
                                    .UseMultipleWarehouses
                                && _productWarehouseInventory
                                    .Table.Where(
                                        (Expression<Func<ProductWarehouseInventory, bool>>)(
                                            (ProductWarehouseInventory pwi) =>
                                                pwi.StockQuantity > 0
                                                && pwi.StockQuantity > pwi.ReservedQuantity
                                        )
                                    )
                                    .Any(
                                        (Expression<Func<ProductWarehouseInventory, bool>>)(
                                            (ProductWarehouseInventory pwi) =>
                                                pwi.ProductId
                                                == (
                                                    (BaseEntity)
                                                        _003C_003Eh__TransparentIdentifier1
                                                            ._003C_003Eh__TransparentIdentifier0
                                                            .p
                                                ).Id
                                        )
                                    )
                            )
                        )
                    )
                )
            select _003C_003Eh__TransparentIdentifier1._003C_003Eh__TransparentIdentifier0.p,
            (Expression<Func<Product, bool>>)null
        );
    }

    private async Task<IList<int>> GetAllGroupProductIdsInCategoriesInternalAsync(
        List<int> categoryIds
    )
    {
        CacheKey val = ((ICacheKeyService)_cacheManager).PrepareKeyForDefaultCache(
            GROUPPRODUCTIDS_BY_CATEGORYIDS,
            new object[1] { categoryIds }
        );
        return await _cacheManager.GetAsync<List<int>>(
            val,
            (Func<Task<List<int>>>)
                async delegate
                {
                    IQueryable<Product> source = (
                        await _customAclHelper.GetAvailableProductsForCurrentErpCustomerAsync()
                    ).Where((Expression<Func<Product, bool>>)((Product p) => !p.Deleted));
                    source = source.Where(
                        (Expression<Func<Product, bool>>)((Product p) => p.Published)
                    );
                    source = source.Where(
                        (Expression<Func<Product, bool>>)((Product p) => p.ProductTypeId == 10)
                    );
                    if (categoryIds != null && categoryIds.Count > 0)
                    {
                        source =
                            from _003C_003Eh__TransparentIdentifier0 in source.SelectMany(
                                (Expression<Func<Product, IEnumerable<ProductCategory>>>)(
                                    (Product p) =>
                                        _productCategoryRepository.Table.Where(
                                            (Expression<Func<ProductCategory, bool>>)(
                                                (ProductCategory pc) =>
                                                    categoryIds.Contains(pc.CategoryId)
                                            )
                                        )
                                ),
                                (Product p, ProductCategory pc) => new { p, pc }
                            )
                            where
                                _003C_003Eh__TransparentIdentifier0.pc.ProductId
                                == ((BaseEntity)_003C_003Eh__TransparentIdentifier0.p).Id
                            select _003C_003Eh__TransparentIdentifier0.p;
                    }
                    return await AsyncIQueryableExtensions.ToListAsync<int>(
                        source.Select(
                            (Expression<Func<Product, int>>)((Product x) => ((BaseEntity)x).Id)
                        )
                    );
                }
        );
    }

    private async Task<IList<int>> GetAllGroupProductIdsInManufacturerInternalAsync(
        int manufacturerId
    )
    {
        CacheKey val = ((ICacheKeyService)_cacheManager).PrepareKeyForDefaultCache(
            GROUPPRODUCTIDS_BY_MANUFACTURERID,
            new object[1] { manufacturerId }
        );
        return await _cacheManager.GetAsync<List<int>>(
            val,
            (Func<Task<List<int>>>)(
                async () =>
                    await AsyncIQueryableExtensions.ToListAsync<int>(
                        (
                            from _003C_003Eh__TransparentIdentifier0 in (
                                await _aclHelper.GetAvailableProductsForCurrentCustomerAsync()
                            )
                                .Where((Expression<Func<Product, bool>>)((Product p) => !p.Deleted))
                                .Where(
                                    (Expression<Func<Product, bool>>)((Product p) => p.Published)
                                )
                                .Where(
                                    (Expression<Func<Product, bool>>)(
                                        (Product p) => p.ProductTypeId == 10
                                    )
                                )
                                .SelectMany(
                                    (Expression<Func<Product, IEnumerable<ProductManufacturer>>>)(
                                        (Product p) => _productManufacturerRepository.Table
                                    ),
                                    (Product p, ProductManufacturer pm) => new { p, pm }
                                )
                            where
                                ((BaseEntity)_003C_003Eh__TransparentIdentifier0.p).Id
                                    == _003C_003Eh__TransparentIdentifier0.pm.ProductId
                                && _003C_003Eh__TransparentIdentifier0.pm.ManufacturerId
                                    == manufacturerId
                            select _003C_003Eh__TransparentIdentifier0.p
                        ).Select(
                            (Expression<Func<Product, int>>)((Product x) => ((BaseEntity)x).Id)
                        )
                    )
            )
        );
    }

    private async Task<IList<int>> GetAllGroupProductIdsInVendorInternalAsync(int vendorId)
    {
        CacheKey val = ((ICacheKeyService)_cacheManager).PrepareKeyForDefaultCache(
            GROUPPRODUCTIDS_BY_VENDORID,
            new object[1] { vendorId }
        );
        return await _cacheManager.GetAsync<List<int>>(
            val,
            (Func<Task<List<int>>>)(
                async () =>
                    await AsyncIQueryableExtensions.ToListAsync<int>(
                        (await _aclHelper.GetAvailableProductsForCurrentCustomerAsync())
                            .Where((Expression<Func<Product, bool>>)((Product p) => !p.Deleted))
                            .Where((Expression<Func<Product, bool>>)((Product p) => p.Published))
                            .Where(
                                (Expression<Func<Product, bool>>)(
                                    (Product p) => p.ProductTypeId == 10
                                )
                            )
                            .Where(
                                (Expression<Func<Product, bool>>)(
                                    (Product p) => p.VendorId == vendorId
                                )
                            )
                            .Select(
                                (Expression<Func<Product, int>>)((Product x) => ((BaseEntity)x).Id)
                            )
                    )
            )
        );
    }

    private async Task<bool> HasAvailableProductsOnSaleInCategoryAsync(
        IQueryable<Product> availableProducts,
        IList<int> groupProductIds,
        IList<int> categoryIds,
        bool includeFeaturedProducts
    )
    {
        DateTime nowUtc = DateTime.UtcNow;
        return await AsyncIQueryableExtensions.AnyAsync<Product>(
            from _003C_003Eh__TransparentIdentifier1 in Queryable.SelectMany(
                availableProducts.GroupJoin(
                    (IEnumerable<ProductCategory>)_productCategoryRepository.Table,
                    (Expression<Func<Product, int>>)((Product p) => ((BaseEntity)p).Id),
                    (Expression<Func<ProductCategory, int>>)((ProductCategory pc) => pc.ProductId),
                    (Product p, IEnumerable<ProductCategory> p_pc) => new { p, p_pc }
                ),
                _003C_003Eh__TransparentIdentifier0 =>
                    _003C_003Eh__TransparentIdentifier0.p_pc.DefaultIfEmpty(),
                (_003C_003Eh__TransparentIdentifier0, pc) =>
                    new { _003C_003Eh__TransparentIdentifier0, pc }
            )
            where
                (
                    (
                        _003C_003Eh__TransparentIdentifier1.pc != null
                        && categoryIds.Contains(_003C_003Eh__TransparentIdentifier1.pc.CategoryId)
                        && _003C_003Eh__TransparentIdentifier1
                            ._003C_003Eh__TransparentIdentifier0
                            .p
                            .ProductTypeId != 10
                        && _003C_003Eh__TransparentIdentifier1
                            ._003C_003Eh__TransparentIdentifier0
                            .p
                            .VisibleIndividually
                        && (
                            _003C_003Eh__TransparentIdentifier1.pc.IsFeaturedProduct
                                == includeFeaturedProducts
                            || !_003C_003Eh__TransparentIdentifier1.pc.IsFeaturedProduct
                        )
                    )
                    || groupProductIds.Contains(
                        _003C_003Eh__TransparentIdentifier1
                            ._003C_003Eh__TransparentIdentifier0
                            .p
                            .ParentGroupedProductId
                    )
                )
                && _003C_003Eh__TransparentIdentifier1
                    ._003C_003Eh__TransparentIdentifier0
                    .p
                    .Published
                && !_003C_003Eh__TransparentIdentifier1
                    ._003C_003Eh__TransparentIdentifier0
                    .p
                    .Deleted
                && (
                    !_003C_003Eh__TransparentIdentifier1
                        ._003C_003Eh__TransparentIdentifier0
                        .p
                        .AvailableStartDateTimeUtc
                        .HasValue
                    || _003C_003Eh__TransparentIdentifier1
                        ._003C_003Eh__TransparentIdentifier0
                        .p
                        .AvailableStartDateTimeUtc <= nowUtc
                )
                && (
                    !_003C_003Eh__TransparentIdentifier1
                        ._003C_003Eh__TransparentIdentifier0
                        .p
                        .AvailableEndDateTimeUtc
                        .HasValue
                    || _003C_003Eh__TransparentIdentifier1
                        ._003C_003Eh__TransparentIdentifier0
                        .p
                        .AvailableEndDateTimeUtc >= nowUtc
                )
                && _003C_003Eh__TransparentIdentifier1
                    ._003C_003Eh__TransparentIdentifier0
                    .p
                    .OldPrice > 0m
                && _003C_003Eh__TransparentIdentifier1
                    ._003C_003Eh__TransparentIdentifier0
                    .p
                    .OldPrice
                    != _003C_003Eh__TransparentIdentifier1
                        ._003C_003Eh__TransparentIdentifier0
                        .p
                        .Price
            select _003C_003Eh__TransparentIdentifier1._003C_003Eh__TransparentIdentifier0.p,
            (Expression<Func<Product, bool>>)null
        );
    }

    private async Task<bool> HasAvailableProductsOnSaleInVendorAsync(
        IQueryable<Product> availableProducts,
        IList<int> groupProductIds,
        int vendorId,
        bool includeFeaturedProducts
    )
    {
        DateTime nowUtc = DateTime.UtcNow;
        return await AsyncIQueryableExtensions.AnyAsync<Product>(
            from _003C_003Eh__TransparentIdentifier1 in Queryable.SelectMany(
                availableProducts.GroupJoin(
                    (IEnumerable<Vendor>)
                        _vendorRepository.Table.Where(
                            (Expression<Func<Vendor, bool>>)((Vendor v) => v.Active && !v.Deleted)
                        ),
                    (Expression<Func<Product, int>>)((Product p) => p.VendorId),
                    (Expression<Func<Vendor, int>>)((Vendor v) => ((BaseEntity)v).Id),
                    (Product p, IEnumerable<Vendor> p_pv) => new { p, p_pv }
                ),
                _003C_003Eh__TransparentIdentifier0 =>
                    _003C_003Eh__TransparentIdentifier0.p_pv.DefaultIfEmpty(),
                (_003C_003Eh__TransparentIdentifier0, v) =>
                    new { _003C_003Eh__TransparentIdentifier0, v }
            )
            where
                (
                    (
                        _003C_003Eh__TransparentIdentifier1.v != null
                        && ((BaseEntity)_003C_003Eh__TransparentIdentifier1.v).Id == vendorId
                        && _003C_003Eh__TransparentIdentifier1
                            ._003C_003Eh__TransparentIdentifier0
                            .p
                            .ProductTypeId != 10
                        && _003C_003Eh__TransparentIdentifier1
                            ._003C_003Eh__TransparentIdentifier0
                            .p
                            .VisibleIndividually
                    )
                    || groupProductIds.Contains(
                        _003C_003Eh__TransparentIdentifier1
                            ._003C_003Eh__TransparentIdentifier0
                            .p
                            .ParentGroupedProductId
                    )
                )
                && _003C_003Eh__TransparentIdentifier1
                    ._003C_003Eh__TransparentIdentifier0
                    .p
                    .Published
                && !_003C_003Eh__TransparentIdentifier1
                    ._003C_003Eh__TransparentIdentifier0
                    .p
                    .Deleted
                && (
                    !_003C_003Eh__TransparentIdentifier1
                        ._003C_003Eh__TransparentIdentifier0
                        .p
                        .AvailableStartDateTimeUtc
                        .HasValue
                    || _003C_003Eh__TransparentIdentifier1
                        ._003C_003Eh__TransparentIdentifier0
                        .p
                        .AvailableStartDateTimeUtc <= nowUtc
                )
                && (
                    !_003C_003Eh__TransparentIdentifier1
                        ._003C_003Eh__TransparentIdentifier0
                        .p
                        .AvailableEndDateTimeUtc
                        .HasValue
                    || _003C_003Eh__TransparentIdentifier1
                        ._003C_003Eh__TransparentIdentifier0
                        .p
                        .AvailableEndDateTimeUtc >= nowUtc
                )
                && _003C_003Eh__TransparentIdentifier1
                    ._003C_003Eh__TransparentIdentifier0
                    .p
                    .OldPrice > 0m
                && _003C_003Eh__TransparentIdentifier1
                    ._003C_003Eh__TransparentIdentifier0
                    .p
                    .OldPrice
                    != _003C_003Eh__TransparentIdentifier1
                        ._003C_003Eh__TransparentIdentifier0
                        .p
                        .Price
            select _003C_003Eh__TransparentIdentifier1._003C_003Eh__TransparentIdentifier0.p,
            (Expression<Func<Product, bool>>)null
        );
    }

    private async Task<bool> HasAvailableProductsOnSaleInManufacturerAsync(
        IQueryable<Product> availableProducts,
        IList<int> groupProductIds,
        int manufacturerId,
        bool includeFeaturedProducts
    )
    {
        DateTime nowUtc = DateTime.UtcNow;
        IQueryable<ProductManufacturer> inner =
            from _003C_003Eh__TransparentIdentifier0 in _productManufacturerRepository.Table.Join(
                (IEnumerable<Manufacturer>)_manufacturerRepository.Table,
                (Expression<Func<ProductManufacturer, int>>)(
                    (ProductManufacturer pm) => pm.ManufacturerId
                ),
                (Expression<Func<Manufacturer, int>>)((Manufacturer m) => ((BaseEntity)m).Id),
                (ProductManufacturer pm, Manufacturer m) => new { pm, m }
            )
            where
                _003C_003Eh__TransparentIdentifier0.m.Published
                && !_003C_003Eh__TransparentIdentifier0.m.Deleted
            select _003C_003Eh__TransparentIdentifier0.pm;
        return await AsyncIQueryableExtensions.AnyAsync<Product>(
            from _003C_003Eh__TransparentIdentifier1 in Queryable.SelectMany(
                availableProducts.GroupJoin(
                    (IEnumerable<ProductManufacturer>)inner,
                    (Expression<Func<Product, int>>)((Product p) => ((BaseEntity)p).Id),
                    (Expression<Func<ProductManufacturer, int>>)(
                        (ProductManufacturer pm) => pm.ProductId
                    ),
                    (Product p, IEnumerable<ProductManufacturer> p_pm) => new { p, p_pm }
                ),
                _003C_003Eh__TransparentIdentifier0 =>
                    _003C_003Eh__TransparentIdentifier0.p_pm.DefaultIfEmpty(),
                (_003C_003Eh__TransparentIdentifier0, pm) =>
                    new { _003C_003Eh__TransparentIdentifier0, pm }
            )
            where
                (
                    (
                        _003C_003Eh__TransparentIdentifier1.pm != null
                        && _003C_003Eh__TransparentIdentifier1.pm.ManufacturerId == manufacturerId
                        && _003C_003Eh__TransparentIdentifier1
                            ._003C_003Eh__TransparentIdentifier0
                            .p
                            .ProductTypeId != 10
                        && _003C_003Eh__TransparentIdentifier1
                            ._003C_003Eh__TransparentIdentifier0
                            .p
                            .VisibleIndividually
                        && (
                            _003C_003Eh__TransparentIdentifier1.pm.IsFeaturedProduct
                                == includeFeaturedProducts
                            || !_003C_003Eh__TransparentIdentifier1.pm.IsFeaturedProduct
                        )
                    )
                    || groupProductIds.Contains(
                        _003C_003Eh__TransparentIdentifier1
                            ._003C_003Eh__TransparentIdentifier0
                            .p
                            .ParentGroupedProductId
                    )
                )
                && _003C_003Eh__TransparentIdentifier1
                    ._003C_003Eh__TransparentIdentifier0
                    .p
                    .Published
                && !_003C_003Eh__TransparentIdentifier1
                    ._003C_003Eh__TransparentIdentifier0
                    .p
                    .Deleted
                && (
                    !_003C_003Eh__TransparentIdentifier1
                        ._003C_003Eh__TransparentIdentifier0
                        .p
                        .AvailableStartDateTimeUtc
                        .HasValue
                    || _003C_003Eh__TransparentIdentifier1
                        ._003C_003Eh__TransparentIdentifier0
                        .p
                        .AvailableStartDateTimeUtc <= nowUtc
                )
                && (
                    !_003C_003Eh__TransparentIdentifier1
                        ._003C_003Eh__TransparentIdentifier0
                        .p
                        .AvailableEndDateTimeUtc
                        .HasValue
                    || _003C_003Eh__TransparentIdentifier1
                        ._003C_003Eh__TransparentIdentifier0
                        .p
                        .AvailableEndDateTimeUtc >= nowUtc
                )
                && _003C_003Eh__TransparentIdentifier1
                    ._003C_003Eh__TransparentIdentifier0
                    .p
                    .OldPrice > 0m
                && _003C_003Eh__TransparentIdentifier1
                    ._003C_003Eh__TransparentIdentifier0
                    .p
                    .OldPrice
                    != _003C_003Eh__TransparentIdentifier1
                        ._003C_003Eh__TransparentIdentifier0
                        .p
                        .Price
            select _003C_003Eh__TransparentIdentifier1._003C_003Eh__TransparentIdentifier0.p,
            (Expression<Func<Product, bool>>)null
        );
    }
}
