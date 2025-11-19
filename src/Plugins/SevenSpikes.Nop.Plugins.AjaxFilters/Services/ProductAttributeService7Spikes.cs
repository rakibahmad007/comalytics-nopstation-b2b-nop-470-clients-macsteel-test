using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Core.Caching;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Localization;
using Nop.Core.Domain.Stores;
using Nop.Core.Domain.Vendors;
using Nop.Data;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using SevenSpikes.Nop.Services.Catalog;
using SevenSpikes.Nop.Services.Catalog.DTO;
using SevenSpikes.Nop.Services.Helpers;

namespace SevenSpikes.Nop.Plugins.AjaxFilters.Services;

public class ProductAttributeService7Spikes : IProductAttributeService7Spikes
{
	private readonly IRepository<ProductManufacturer> _productManufacturerRepository;

	private readonly IRepository<Vendor> _vendorRepository;

	private readonly IAclHelper _aclHelper;

	private readonly IStoreContext _storeContext;

	private readonly IWorkContext _workContext;

	private readonly IProductServiceNopAjaxFilters _productServiceNopAjaxFilters;

	private readonly ILanguageService _languageService;

	private IStoreHelper _storeHelper;

	private ICustomAclHelper _customAclHelper;

	private CacheKey PRODUCTATTRIBUTES_BY_CATEGORYID_KEY => new CacheKey("nop.pres.nop.ajax.filters.productattribute.categoryid-{0}.includeproductsinsubcategories-{1}.showhidden-{2}.store.id-{3}", new string[1] { "nop.pres.nop.ajax.filters" });

	private CacheKey PRODUCTATTRIBUTES_BY_MANUFACTURERID_KEY => new CacheKey("nop.pres.nop.ajax.filters.productattribute.manufacturerid-{0}.showhidden-{1}.store.id-{2}", new string[1] { "nop.pres.nop.ajax.filters" });

	private CacheKey PRODUCTATTRIBUTES_BY_VENDORID_KEY => new CacheKey("nop.pres.nop.ajax.filters.productattribute.vendorid-{0}.showhidden-{1}.store.id-{2}", new string[1] { "nop.pres.nop.ajax.filters" });

	private CacheKey PRODUCTATTRIBUTES_BY_PRODUCTVARIANTIDS_KEY => new CacheKey("nop.pres.nop.ajax.filters..productattribute.productvariantids-{0}.showhidden-{1}.store.id-{2}", new string[1] { "nop.pres.nop.ajax.filters" });

	private CacheKey PRODUCTATTRIBUTEVALUES_BY_PRODUCTATTRIBUTEID_AND_CATEGORYID_KEY => new CacheKey("nop.pres.nop.ajax.filters.productattributevalues.productattributeid-{0}.categoryid-{1}.includeproductsinsubcategories-{2}.showhidden-{3}.store.id-{4}.language.id-{5}", new string[1] { "nop.pres.nop.ajax.filters" });

	private CacheKey PRODUCTATTRIBUTEVALUES_BY_PRODUCTATTRIBUTEID_AND_MANUFACTURERID_KEY => new CacheKey("nop.pres.nop.ajax.filters.productattributevalues.productattributeid-{0}.manufacturerid-{1}.showhidden-{2}.store.id-{3}.language.id-{4}", new string[1] { "nop.pres.nop.ajax.filters" });

	private CacheKey PRODUCTATTRIBUTEVALUES_BY_PRODUCTATTRIBUTEID_AND_VENDORID_KEY => new CacheKey("nop.pres.nop.ajax.filters.productattributevalues.productattributeid-{0}.vendorid-{1}.showhidden-{2}.store.id-{3}.language.id-{4}", new string[1] { "nop.pres.nop.ajax.filters" });

	private CacheKey PRODUCTATTRIBUTEVALUES_AND_PRODUCT_ATTRIBUTES_BY_PRODUCTATTRIBUTEMAPPINGS_KEY => new CacheKey("nop.pres.nop.ajax.filters.productattributevalues.productattributemappingids-{0}.store.id-{1}.language.id-{2}", new string[1] { "nop.pres.nop.ajax.filters" });

	private CacheKey PRODUCTVARIANTATTRIBUTES_BY_PRODUCTID_KEY => new CacheKey("nop.pres.nop.ajax.filters.productvariantattribute.productid-{0}.store.id-{1}", new string[1] { "nop.pres.nop.ajax.filters" });

	private CacheKey PRODUCTVARIANTATTRIBUTES_WHICH_HAVE_VALUES_BY_PRODUCTID_KEY => new CacheKey("nop.pres.nop.ajax.filters.productvariantattributeswhichhavevalues.productid-{0}.store.id-{1}", new string[1] { "nop.pres.nop.ajax.filters" });

	private CacheKey PRODUCTVARIANTATTRIBUTES_WHICH_HAVE_VALUES_BY_PRODUCTIDS_KEY => new CacheKey("nop.pres.nop.ajax.filters.productvariantattributeswhichhavevalues.productids-{0}.store.id-{1}", new string[1] { "nop.pres.nop.ajax.filters" });

	private IRepository<Product> ProductRepository { get; set; }

	private IRepository<Category> CategoryRepository { get; set; }

	private IRepository<ProductAttribute> ProductAttributeRepository { get; set; }

	private IRepository<ProductAttributeMapping> ProductAttributeMappingRepository { get; set; }

	private IRepository<ProductAttributeValue> ProductAttributeValueRepository { get; set; }

	private IRepository<ProductCategory> ProductCategoryRepository { get; set; }

	private IRepository<LocalizedProperty> LocalizedPropertyRepository { get; set; }

	private ICategoryService7Spikes CategoryService7Spikes { get; set; }

	private ISettingService SettingService { get; set; }

	private IStaticCacheManager CacheManager { get; set; }

    public ProductAttributeService7Spikes(IRepository<Product> productRepository, IRepository<ProductManufacturer> productManufacturerRepository, IRepository<Vendor> vendorRepository, IRepository<Category> categoryRepository, IRepository<ProductAttribute> productAttributeRepository, IRepository<ProductAttributeMapping> productVariantAttributeRepository, IRepository<ProductAttributeValue> productVariantAttributeValueRepository, IRepository<ProductCategory> productCategoryRepository, ICategoryService7Spikes categoryService7Spikes, ISettingService settingService, IStaticCacheManager cacheManager, IAclHelper aclHelper, IStoreContext storeContext, IProductServiceNopAjaxFilters productServiceNopAjaxFilters, IStoreHelper storeHelper, IRepository<LocalizedProperty> localizedProperty, ILanguageService languageService, IWorkContext workContext, ICustomAclHelper customAclHelper)
    {
        _productManufacturerRepository = productManufacturerRepository;
        _vendorRepository = vendorRepository;
        _aclHelper = aclHelper;
        _storeContext = storeContext;
        _workContext = workContext;
        _productServiceNopAjaxFilters = productServiceNopAjaxFilters;
        _storeHelper = storeHelper;
        _languageService = languageService;
        ProductRepository = productRepository;
        CategoryRepository = categoryRepository;
        ProductAttributeRepository = productAttributeRepository;
        ProductAttributeMappingRepository = productVariantAttributeRepository;
        ProductAttributeValueRepository = productVariantAttributeValueRepository;
        ProductCategoryRepository = productCategoryRepository;
        LocalizedPropertyRepository = localizedProperty;
        CategoryService7Spikes = categoryService7Spikes;
        SettingService = settingService;
        CacheManager = cacheManager;
        _customAclHelper = customAclHelper;
    }

    public virtual async Task<IList<ProductAttribute>> GetAllProductAttributesByCategoryIdAsync(int categoryId, bool includeProductsInSubcategories = false, bool showHiddenProducts = false)
	{
		return await GetAllProductAttributesByCategoryIdInternalAsync(categoryId, includeProductsInSubcategories, showHiddenProducts);
	}

	public async Task<IList<ProductAttribute>> GetAllProductAttributesByManufacturerIdAsync(int manufacturerId, bool showHiddenProducts = false)
	{
		return await GetAllProductAttributesByManufacturerIdInternalAsync(manufacturerId, showHiddenProducts);
	}

	public async Task<IList<ProductAttribute>> GetAllProductAttributesByVendorIdAsync(int vendorId, bool showHiddenProducts = false)
	{
		return await GetAllProductAttributesByVendorIdInternalAsync(vendorId, showHiddenProducts);
	}

	public async Task<IList<ProductAttribute>> GetAllProductAttributesByProductAttributeMappingIdsAsync(IList<int> productAttributeMappingIds, bool showHiddenProducts = false)
	{
		return await GetAllProductAttributesByProductAttributeMappingIdsInternalAsync(productAttributeMappingIds, showHiddenProducts);
	}

	public virtual async Task<IList<ProductAttributeValue>> GetAllProductVariantAttributeValuesByProductAttributeIdAndCategoryIdAsync(int productAttributeId, int categoryId, bool includeProductsInSubcategories = false, bool showHiddenProducts = false)
	{
		return await GetAllProductVariantAttributeValuesByProductAttributeIdAndCategoryIdInternalAsync(productAttributeId, categoryId, includeProductsInSubcategories, showHiddenProducts);
	}

	public async Task<IList<ProductAttributeValue>> GetAllProductVariantAttributeValuesByProductAttributeIdAndManufacturerIdAsync(int productAttributeId, int manufacturerId, bool showHiddenProducts = false)
	{
		return await GetAllProductVariantAttributeValuesByProductAttributeIdAndManufacturerIdInternalAsync(productAttributeId, manufacturerId, showHiddenProducts);
	}

	public async Task<IList<ProductAttributeValue>> GetAllProductVariantAttributeValuesByProductAttributeIdAndVendorIdAsync(int productAttributeId, int vendorId, bool showHiddenProducts = false)
	{
		return await GetAllProductVariantAttributeValuesByProductAttributeIdAndVendorIdInternalAsync(productAttributeId, vendorId, showHiddenProducts);
	}

	public virtual async Task<IList<ProductAttributeMapping>> GetAllProductVariantAttributesByProductIdAsync(int productId, bool showHiddenProducts = false)
	{
		return await GetAllProductVariantAttributesByProductIdInternalAsync(productId, showHiddenProducts);
	}

	public virtual async Task<IList<ProductAttributeMapping>> GetAllProductVariantAttributesWhichHaveValuesByProductIdAsync(int productId, bool showHiddenProducts = false)
	{
		return await GetAllProductVariantAttributesWhichHaveValuesByProductIdInternalAsync(productId, showHiddenProducts);
	}

	public virtual async Task<IList<ProductAttributeMapping>> GetAllProductVariantAttributesWhichHaveValuesByProductIdsAsync(IList<int> productIds, bool showHiddenProducts = false)
	{
		return await GetAllProductVariantAttributesWhichHaveValuesByProductIdsInternalAsync(productIds, showHiddenProducts);
	}

	public async Task<IList<ProductAttributeProductAttributeValueDTO>> GetProductAttributeProductAttributeValueDtosByProductAttributeMappingIdsAsync(IList<int> productAttributeMappingIds)
	{
		return await GetProductAttributeProductAttributeValueDtosByProductAttributeMappingIdsInternalAsync(productAttributeMappingIds);
	}

	private async Task<IList<ProductAttribute>> GetAllProductAttributesByCategoryIdInternalAsync(int categoryId, bool includeProductsInSubcategories, bool showHiddenProducts)
	{
		if (categoryId == 0)
		{
			return null;
		}
		DateTime nowUtc = DateTime.UtcNow;
		IStaticCacheManager cacheManager = CacheManager;
		CacheKey pRODUCTATTRIBUTES_BY_CATEGORYID_KEY = PRODUCTATTRIBUTES_BY_CATEGORYID_KEY;
		object obj = categoryId;
		object obj2 = includeProductsInSubcategories;
		object obj3 = showHiddenProducts;
		Store val = await _storeContext.GetCurrentStoreAsync();
		CacheKey val2 = ((ICacheKeyService)cacheManager).PrepareKeyForDefaultCache(pRODUCTATTRIBUTES_BY_CATEGORYID_KEY, new object[4]
		{
			obj,
			obj2,
			obj3,
			((BaseEntity)val).Id
		});
		return new List<ProductAttribute>(await CacheManager.GetAsync<IEnumerable<ProductAttribute>>(val2, (Func<Task<IEnumerable<ProductAttribute>>>)async delegate
		{
			List<int> categoryIds = new List<int> { categoryId };
			if (includeProductsInSubcategories)
			{
				List<int> collection = await CategoryService7Spikes.GetCategoryIdsByParentCategoryAsync(categoryId);
				categoryIds.AddRange(collection);
			}
			IList<int> groupProductIds = await _productServiceNopAjaxFilters.GetAllGroupProductIdsInCategoriesAsync(categoryIds);
			IQueryable<Product> availableProductsQuery = await _customAclHelper.GetAvailableProductsForCurrentErpCustomerAsync();
			availableProductsQuery = await _storeHelper.GetProductsForCurrentStoreAsync(availableProductsQuery);
			bool includeFeaturedProductsInNormalList = await GetIncludeFeaturedProductsInNormalListAsync();
			return await AsyncIQueryableExtensions.ToListAsync<ProductAttribute>((from _003C_003Eh__TransparentIdentifier3 in Queryable.SelectMany(ProductAttributeRepository.Table.Join((IEnumerable<ProductAttributeMapping>)ProductAttributeMappingRepository.Table, (Expression<Func<ProductAttribute, int>>)((ProductAttribute pa) => ((BaseEntity)pa).Id), (Expression<Func<ProductAttributeMapping, int>>)((ProductAttributeMapping pva) => pva.ProductAttributeId), (ProductAttribute pa, ProductAttributeMapping pva) => new { pa, pva }).Join((IEnumerable<Product>)availableProductsQuery, _003C_003Eh__TransparentIdentifier0 => _003C_003Eh__TransparentIdentifier0.pva.ProductId, (Expression<Func<Product, int>>)((Product p) => ((BaseEntity)p).Id), (_003C_003Eh__TransparentIdentifier0,   p) => new { _003C_003Eh__TransparentIdentifier0, p }).GroupJoin((IEnumerable<ProductCategory>)ProductCategoryRepository.Table, _003C_003Eh__TransparentIdentifier1 => ((BaseEntity)_003C_003Eh__TransparentIdentifier1.p).Id, (Expression<Func<ProductCategory, int>>)((ProductCategory pc) => pc.ProductId), (_003C_003Eh__TransparentIdentifier1,   p_pc) => new { _003C_003Eh__TransparentIdentifier1, p_pc }), _003C_003Eh__TransparentIdentifier2 => _003C_003Eh__TransparentIdentifier2.p_pc.DefaultIfEmpty(), (_003C_003Eh__TransparentIdentifier2,   pc) => new { _003C_003Eh__TransparentIdentifier2, pc })
				where ((_003C_003Eh__TransparentIdentifier3.pc != null && categoryIds.Contains(_003C_003Eh__TransparentIdentifier3.pc.CategoryId) && (_003C_003Eh__TransparentIdentifier3._003C_003Eh__TransparentIdentifier2._003C_003Eh__TransparentIdentifier1.p.ParentGroupedProductId == 0 || _003C_003Eh__TransparentIdentifier3._003C_003Eh__TransparentIdentifier2._003C_003Eh__TransparentIdentifier1.p.VisibleIndividually)) || (_003C_003Eh__TransparentIdentifier3.pc == null && groupProductIds.Contains(_003C_003Eh__TransparentIdentifier3._003C_003Eh__TransparentIdentifier2._003C_003Eh__TransparentIdentifier1.p.ParentGroupedProductId))) && (includeFeaturedProductsInNormalList || _003C_003Eh__TransparentIdentifier3.pc == null || !_003C_003Eh__TransparentIdentifier3.pc.IsFeaturedProduct) && (showHiddenProducts || _003C_003Eh__TransparentIdentifier3._003C_003Eh__TransparentIdentifier2._003C_003Eh__TransparentIdentifier1.p.Published) && !_003C_003Eh__TransparentIdentifier3._003C_003Eh__TransparentIdentifier2._003C_003Eh__TransparentIdentifier1.p.Deleted && (!_003C_003Eh__TransparentIdentifier3._003C_003Eh__TransparentIdentifier2._003C_003Eh__TransparentIdentifier1.p.AvailableStartDateTimeUtc.HasValue || _003C_003Eh__TransparentIdentifier3._003C_003Eh__TransparentIdentifier2._003C_003Eh__TransparentIdentifier1.p.AvailableStartDateTimeUtc <= nowUtc) && (!_003C_003Eh__TransparentIdentifier3._003C_003Eh__TransparentIdentifier2._003C_003Eh__TransparentIdentifier1.p.AvailableEndDateTimeUtc.HasValue || _003C_003Eh__TransparentIdentifier3._003C_003Eh__TransparentIdentifier2._003C_003Eh__TransparentIdentifier1.p.AvailableEndDateTimeUtc >= nowUtc)
				select _003C_003Eh__TransparentIdentifier3._003C_003Eh__TransparentIdentifier2._003C_003Eh__TransparentIdentifier1._003C_003Eh__TransparentIdentifier0.pa).Distinct());
		}));
	}

	private async Task<IList<ProductAttribute>> GetAllProductAttributesByManufacturerIdInternalAsync(int manufacturerId, bool showHiddenProducts)
	{
		if (manufacturerId == 0)
		{
			return null;
		}
		DateTime nowUtc = DateTime.UtcNow;
		IStaticCacheManager cacheManager = CacheManager;
		CacheKey pRODUCTATTRIBUTES_BY_MANUFACTURERID_KEY = PRODUCTATTRIBUTES_BY_MANUFACTURERID_KEY;
		object obj = manufacturerId;
		object obj2 = showHiddenProducts;
		Store val = await _storeContext.GetCurrentStoreAsync();
		CacheKey val2 = ((ICacheKeyService)cacheManager).PrepareKeyForDefaultCache(pRODUCTATTRIBUTES_BY_MANUFACTURERID_KEY, new object[3]
		{
			obj,
			obj2,
			((BaseEntity)val).Id
		});
		return new List<ProductAttribute>(await CacheManager.GetAsync<IEnumerable<ProductAttribute>>(val2, (Func<Task<IEnumerable<ProductAttribute>>>)async delegate
		{
			IList<int> groupProductIds = await _productServiceNopAjaxFilters.GetAllGroupProductIdsInManufacturerAsync(manufacturerId);
			IQueryable<Product> availableProductsQuery = await _customAclHelper.GetAvailableProductsForCurrentErpCustomerAsync();
			availableProductsQuery = await _storeHelper.GetProductsForCurrentStoreAsync(availableProductsQuery);
			bool includeFeaturedProductsInNormalList = await GetIncludeFeaturedProductsInNormalListAsync();
			return await AsyncIQueryableExtensions.ToListAsync<ProductAttribute>((from _003C_003Eh__TransparentIdentifier3 in Queryable.SelectMany(ProductAttributeRepository.Table.Join((IEnumerable<ProductAttributeMapping>)ProductAttributeMappingRepository.Table, (Expression<Func<ProductAttribute, int>>)((ProductAttribute pa) => ((BaseEntity)pa).Id), (Expression<Func<ProductAttributeMapping, int>>)((ProductAttributeMapping pva) => pva.ProductAttributeId), (ProductAttribute pa, ProductAttributeMapping pva) => new { pa, pva }).Join((IEnumerable<Product>)availableProductsQuery, _003C_003Eh__TransparentIdentifier0 => _003C_003Eh__TransparentIdentifier0.pva.ProductId, (Expression<Func<Product, int>>)((Product p) => ((BaseEntity)p).Id), (_003C_003Eh__TransparentIdentifier0,   p) => new { _003C_003Eh__TransparentIdentifier0, p }).GroupJoin((IEnumerable<ProductManufacturer>)_productManufacturerRepository.Table, _003C_003Eh__TransparentIdentifier1 => ((BaseEntity)_003C_003Eh__TransparentIdentifier1.p).Id, (Expression<Func<ProductManufacturer, int>>)((ProductManufacturer pm) => pm.ProductId), (_003C_003Eh__TransparentIdentifier1,  p_pm) => new { _003C_003Eh__TransparentIdentifier1, p_pm }), _003C_003Eh__TransparentIdentifier2 => _003C_003Eh__TransparentIdentifier2.p_pm.DefaultIfEmpty(), (_003C_003Eh__TransparentIdentifier2,   pm) => new { _003C_003Eh__TransparentIdentifier2, pm })
				where ((_003C_003Eh__TransparentIdentifier3.pm != null && _003C_003Eh__TransparentIdentifier3.pm.ManufacturerId == manufacturerId && (includeFeaturedProductsInNormalList || !_003C_003Eh__TransparentIdentifier3.pm.IsFeaturedProduct) && (_003C_003Eh__TransparentIdentifier3._003C_003Eh__TransparentIdentifier2._003C_003Eh__TransparentIdentifier1.p.ParentGroupedProductId == 0 || _003C_003Eh__TransparentIdentifier3._003C_003Eh__TransparentIdentifier2._003C_003Eh__TransparentIdentifier1.p.VisibleIndividually)) || (_003C_003Eh__TransparentIdentifier3.pm == null && groupProductIds.Contains(_003C_003Eh__TransparentIdentifier3._003C_003Eh__TransparentIdentifier2._003C_003Eh__TransparentIdentifier1.p.ParentGroupedProductId))) && (showHiddenProducts || _003C_003Eh__TransparentIdentifier3._003C_003Eh__TransparentIdentifier2._003C_003Eh__TransparentIdentifier1.p.Published) && !_003C_003Eh__TransparentIdentifier3._003C_003Eh__TransparentIdentifier2._003C_003Eh__TransparentIdentifier1.p.Deleted && (!_003C_003Eh__TransparentIdentifier3._003C_003Eh__TransparentIdentifier2._003C_003Eh__TransparentIdentifier1.p.AvailableStartDateTimeUtc.HasValue || _003C_003Eh__TransparentIdentifier3._003C_003Eh__TransparentIdentifier2._003C_003Eh__TransparentIdentifier1.p.AvailableStartDateTimeUtc <= nowUtc) && (!_003C_003Eh__TransparentIdentifier3._003C_003Eh__TransparentIdentifier2._003C_003Eh__TransparentIdentifier1.p.AvailableEndDateTimeUtc.HasValue || _003C_003Eh__TransparentIdentifier3._003C_003Eh__TransparentIdentifier2._003C_003Eh__TransparentIdentifier1.p.AvailableEndDateTimeUtc >= nowUtc)
				select _003C_003Eh__TransparentIdentifier3._003C_003Eh__TransparentIdentifier2._003C_003Eh__TransparentIdentifier1._003C_003Eh__TransparentIdentifier0.pa).Distinct());
		}));
	}

	private async Task<IList<ProductAttribute>> GetAllProductAttributesByVendorIdInternalAsync(int vendorId, bool showHiddenProducts)
	{
		if (vendorId == 0)
		{
			return null;
		}
		DateTime nowUtc = DateTime.UtcNow;
		IStaticCacheManager cacheManager = CacheManager;
		CacheKey pRODUCTATTRIBUTES_BY_VENDORID_KEY = PRODUCTATTRIBUTES_BY_VENDORID_KEY;
		object obj = vendorId;
		object obj2 = showHiddenProducts;
		Store val = await _storeContext.GetCurrentStoreAsync();
		CacheKey val2 = ((ICacheKeyService)cacheManager).PrepareKeyForDefaultCache(pRODUCTATTRIBUTES_BY_VENDORID_KEY, new object[3]
		{
			obj,
			obj2,
			((BaseEntity)val).Id
		});
		return new List<ProductAttribute>(await CacheManager.GetAsync<IEnumerable<ProductAttribute>>(val2, (Func<Task<IEnumerable<ProductAttribute>>>)async delegate
		{
			IList<int> groupProductIds = await _productServiceNopAjaxFilters.GetAllGroupProductIdsInVendorAsync(vendorId);
			IQueryable<Product> productsQuery = await _customAclHelper.GetAvailableProductsForCurrentErpCustomerAsync();
			productsQuery = await _storeHelper.GetProductsForCurrentStoreAsync(productsQuery);
			return await AsyncIQueryableExtensions.ToListAsync<ProductAttribute>((from _003C_003Eh__TransparentIdentifier3 in Queryable.SelectMany(ProductAttributeRepository.Table.Join((IEnumerable<ProductAttributeMapping>)ProductAttributeMappingRepository.Table, (Expression<Func<ProductAttribute, int>>)((ProductAttribute pa) => ((BaseEntity)pa).Id), (Expression<Func<ProductAttributeMapping, int>>)((ProductAttributeMapping pva) => pva.ProductAttributeId), (ProductAttribute pa, ProductAttributeMapping pva) => new { pa, pva }).Join((IEnumerable<Product>)productsQuery, _003C_003Eh__TransparentIdentifier0 => _003C_003Eh__TransparentIdentifier0.pva.ProductId, (Expression<Func<Product, int>>)((Product p) => ((BaseEntity)p).Id), (_003C_003Eh__TransparentIdentifier0,   p) => new { _003C_003Eh__TransparentIdentifier0, p }).GroupJoin((IEnumerable<Vendor>)_vendorRepository.Table, _003C_003Eh__TransparentIdentifier1 => _003C_003Eh__TransparentIdentifier1.p.VendorId, (Expression<Func<Vendor, int>>)((Vendor v) => ((BaseEntity)v).Id), (_003C_003Eh__TransparentIdentifier1,   p_pv) => new { _003C_003Eh__TransparentIdentifier1, p_pv }), _003C_003Eh__TransparentIdentifier2 => _003C_003Eh__TransparentIdentifier2.p_pv.DefaultIfEmpty(), (_003C_003Eh__TransparentIdentifier2,   v) => new { _003C_003Eh__TransparentIdentifier2, v })
				where ((_003C_003Eh__TransparentIdentifier3.v != null && ((BaseEntity)_003C_003Eh__TransparentIdentifier3.v).Id == vendorId && (_003C_003Eh__TransparentIdentifier3._003C_003Eh__TransparentIdentifier2._003C_003Eh__TransparentIdentifier1.p.ParentGroupedProductId == 0 || _003C_003Eh__TransparentIdentifier3._003C_003Eh__TransparentIdentifier2._003C_003Eh__TransparentIdentifier1.p.VisibleIndividually)) || (_003C_003Eh__TransparentIdentifier3.v == null && groupProductIds.Contains(_003C_003Eh__TransparentIdentifier3._003C_003Eh__TransparentIdentifier2._003C_003Eh__TransparentIdentifier1.p.ParentGroupedProductId))) && (showHiddenProducts || _003C_003Eh__TransparentIdentifier3._003C_003Eh__TransparentIdentifier2._003C_003Eh__TransparentIdentifier1.p.Published) && !_003C_003Eh__TransparentIdentifier3._003C_003Eh__TransparentIdentifier2._003C_003Eh__TransparentIdentifier1.p.Deleted && (!_003C_003Eh__TransparentIdentifier3._003C_003Eh__TransparentIdentifier2._003C_003Eh__TransparentIdentifier1.p.AvailableStartDateTimeUtc.HasValue || _003C_003Eh__TransparentIdentifier3._003C_003Eh__TransparentIdentifier2._003C_003Eh__TransparentIdentifier1.p.AvailableStartDateTimeUtc <= nowUtc) && (!_003C_003Eh__TransparentIdentifier3._003C_003Eh__TransparentIdentifier2._003C_003Eh__TransparentIdentifier1.p.AvailableEndDateTimeUtc.HasValue || _003C_003Eh__TransparentIdentifier3._003C_003Eh__TransparentIdentifier2._003C_003Eh__TransparentIdentifier1.p.AvailableEndDateTimeUtc >= nowUtc)
				select _003C_003Eh__TransparentIdentifier3._003C_003Eh__TransparentIdentifier2._003C_003Eh__TransparentIdentifier1._003C_003Eh__TransparentIdentifier0.pa).Distinct());
		}));
	}

	private async Task<IList<ProductAttribute>> GetAllProductAttributesByProductAttributeMappingIdsInternalAsync(IList<int> productAttributeMappingIds, bool showHiddenProducts)
	{
		string text = string.Join(",", productAttributeMappingIds);
		IStaticCacheManager cacheManager = CacheManager;
		CacheKey pRODUCTATTRIBUTES_BY_PRODUCTVARIANTIDS_KEY = PRODUCTATTRIBUTES_BY_PRODUCTVARIANTIDS_KEY;
		object obj = text;
		object obj2 = showHiddenProducts;
		Store val = await _storeContext.GetCurrentStoreAsync();
		CacheKey val2 = ((ICacheKeyService)cacheManager).PrepareKeyForDefaultCache(pRODUCTATTRIBUTES_BY_PRODUCTVARIANTIDS_KEY, new object[3]
		{
			obj,
			obj2,
			((BaseEntity)val).Id
		});
		return new List<ProductAttribute>(await CacheManager.GetAsync<IEnumerable<ProductAttribute>>(val2, (Func<Task<IEnumerable<ProductAttribute>>>)(async () => await AsyncIQueryableExtensions.ToListAsync<ProductAttribute>((from _003C_003Eh__TransparentIdentifier0 in ProductAttributeRepository.Table.SelectMany((Expression<Func<ProductAttribute, IEnumerable<ProductAttributeMapping>>>)((ProductAttribute pa) => ProductAttributeMappingRepository.Table), (ProductAttribute pa, ProductAttributeMapping pva) => new { pa, pva })
			where productAttributeMappingIds.Contains(((BaseEntity)_003C_003Eh__TransparentIdentifier0.pva).Id) && _003C_003Eh__TransparentIdentifier0.pva.ProductAttributeId == ((BaseEntity)_003C_003Eh__TransparentIdentifier0.pa).Id
			select _003C_003Eh__TransparentIdentifier0.pa).Distinct()))));
	}

	private async Task<IList<ProductAttributeValue>> GetAllProductVariantAttributeValuesByProductAttributeIdAndCategoryIdInternalAsync(int productAttributeId, int categoryId, bool includeProductsInSubcategories, bool showHiddenProducts)
	{
		if (productAttributeId == 0 || categoryId == 0)
		{
			return null;
		}
		DateTime nowUtc = DateTime.UtcNow;
		IStaticCacheManager cacheManager = CacheManager;
		CacheKey pRODUCTATTRIBUTEVALUES_BY_PRODUCTATTRIBUTEID_AND_CATEGORYID_KEY = PRODUCTATTRIBUTEVALUES_BY_PRODUCTATTRIBUTEID_AND_CATEGORYID_KEY;
		object obj = productAttributeId;
		object obj2 = categoryId;
		object obj3 = includeProductsInSubcategories;
		object obj4 = showHiddenProducts;
		object obj5 = ((BaseEntity)(await _storeContext.GetCurrentStoreAsync())).Id;
		Language val = await _workContext.GetWorkingLanguageAsync();
		CacheKey val2 = ((ICacheKeyService)cacheManager).PrepareKeyForDefaultCache(pRODUCTATTRIBUTEVALUES_BY_PRODUCTATTRIBUTEID_AND_CATEGORYID_KEY, new object[6]
		{
			obj,
			obj2,
			obj3,
			obj4,
			obj5,
			((BaseEntity)val).Id
		});
		return await CacheManager.GetAsync<IList<ProductAttributeValue>>(val2, (Func<Task<IList<ProductAttributeValue>>>)async delegate
		{
			List<int> categoryIds = new List<int> { categoryId };
			if (includeProductsInSubcategories)
			{
				List<int> collection = await CategoryService7Spikes.GetCategoryIdsByParentCategoryAsync(categoryId);
				categoryIds.AddRange(collection);
			}
			IList<int> groupProductIds = await _productServiceNopAjaxFilters.GetAllGroupProductIdsInCategoriesAsync(categoryIds);
			IQueryable<Product> availableProductsQuery = await _customAclHelper.GetAvailableProductsForCurrentErpCustomerAsync();
			availableProductsQuery = await _storeHelper.GetProductsForCurrentStoreAsync(availableProductsQuery);
			bool includeFeaturedProductsInNormalList = await GetIncludeFeaturedProductsInNormalListAsync();
			var query2 = from _003C_003Eh__TransparentIdentifier4 in Queryable.SelectMany(Queryable.Join(ProductAttributeValueRepository.Table.Join((IEnumerable<ProductAttributeMapping>)ProductAttributeMappingRepository.Table, (Expression<Func<ProductAttributeValue, int>>)((ProductAttributeValue pvav) => pvav.ProductAttributeMappingId), (Expression<Func<ProductAttributeMapping, int>>)((ProductAttributeMapping pva) => ((BaseEntity)pva).Id), (ProductAttributeValue pvav, ProductAttributeMapping pva) => new { pvav, pva }), (IEnumerable<ProductAttribute>)ProductAttributeRepository.Table, _003C_003Eh__TransparentIdentifier0 => new
				{
					ProductAttributeId1 = _003C_003Eh__TransparentIdentifier0.pva.ProductAttributeId,
					ProductAttributeId2 = _003C_003Eh__TransparentIdentifier0.pva.ProductAttributeId
				}, (ProductAttribute pa) => new
				{
					ProductAttributeId1 = ((BaseEntity)pa).Id,
					ProductAttributeId2 = productAttributeId
				}, (_003C_003Eh__TransparentIdentifier0,   pa) => new { _003C_003Eh__TransparentIdentifier0, pa }).Join((IEnumerable<Product>)availableProductsQuery, _003C_003Eh__TransparentIdentifier1 => _003C_003Eh__TransparentIdentifier1._003C_003Eh__TransparentIdentifier0.pva.ProductId, (Expression<Func<Product, int>>)((Product p) => ((BaseEntity)p).Id), (_003C_003Eh__TransparentIdentifier1,   p) => new { _003C_003Eh__TransparentIdentifier1, p }).GroupJoin((IEnumerable<ProductCategory>)ProductCategoryRepository.Table.Where((Expression<Func<ProductCategory, bool>>)((ProductCategory x) => categoryIds.Contains(x.CategoryId))), _003C_003Eh__TransparentIdentifier2 => ((BaseEntity)_003C_003Eh__TransparentIdentifier2.p).Id, (Expression<Func<ProductCategory, int>>)((ProductCategory pc) => pc.ProductId), (_003C_003Eh__TransparentIdentifier2,   p_pc) => new { _003C_003Eh__TransparentIdentifier2, p_pc }), _003C_003Eh__TransparentIdentifier3 => _003C_003Eh__TransparentIdentifier3.p_pc.DefaultIfEmpty(), (_003C_003Eh__TransparentIdentifier3,   pc) => new { _003C_003Eh__TransparentIdentifier3, pc })
				where (showHiddenProducts || _003C_003Eh__TransparentIdentifier4._003C_003Eh__TransparentIdentifier3._003C_003Eh__TransparentIdentifier2.p.Published) && !_003C_003Eh__TransparentIdentifier4._003C_003Eh__TransparentIdentifier3._003C_003Eh__TransparentIdentifier2.p.Deleted && ((_003C_003Eh__TransparentIdentifier4.pc != null && (includeFeaturedProductsInNormalList || !_003C_003Eh__TransparentIdentifier4.pc.IsFeaturedProduct) && (_003C_003Eh__TransparentIdentifier4._003C_003Eh__TransparentIdentifier3._003C_003Eh__TransparentIdentifier2.p.ParentGroupedProductId == 0 || _003C_003Eh__TransparentIdentifier4._003C_003Eh__TransparentIdentifier3._003C_003Eh__TransparentIdentifier2.p.VisibleIndividually)) || (_003C_003Eh__TransparentIdentifier4.pc == null && groupProductIds.Contains(_003C_003Eh__TransparentIdentifier4._003C_003Eh__TransparentIdentifier3._003C_003Eh__TransparentIdentifier2.p.ParentGroupedProductId))) && (!_003C_003Eh__TransparentIdentifier4._003C_003Eh__TransparentIdentifier3._003C_003Eh__TransparentIdentifier2.p.AvailableStartDateTimeUtc.HasValue || _003C_003Eh__TransparentIdentifier4._003C_003Eh__TransparentIdentifier3._003C_003Eh__TransparentIdentifier2.p.AvailableStartDateTimeUtc <= nowUtc) && (!_003C_003Eh__TransparentIdentifier4._003C_003Eh__TransparentIdentifier3._003C_003Eh__TransparentIdentifier2.p.AvailableEndDateTimeUtc.HasValue || _003C_003Eh__TransparentIdentifier4._003C_003Eh__TransparentIdentifier3._003C_003Eh__TransparentIdentifier2.p.AvailableEndDateTimeUtc >= nowUtc)
				orderby _003C_003Eh__TransparentIdentifier4._003C_003Eh__TransparentIdentifier3._003C_003Eh__TransparentIdentifier2._003C_003Eh__TransparentIdentifier1._003C_003Eh__TransparentIdentifier0.pvav.DisplayOrder
				select new
				{
					ProductAttributeValue = _003C_003Eh__TransparentIdentifier4._003C_003Eh__TransparentIdentifier3._003C_003Eh__TransparentIdentifier2._003C_003Eh__TransparentIdentifier1._003C_003Eh__TransparentIdentifier0.pvav,
					LocalizedName = string.Empty
				};
			new List<ProductAttributeValue>();
			IList<ProductAttributeValue> result;
			if ((await _languageService.GetAllLanguagesAsync(false, 0)).Count >= 2)
			{
				int workingLanguageId = ((BaseEntity)(await _workContext.GetWorkingLanguageAsync())).Id;
				query2 = Queryable.SelectMany(query2.GroupJoin((IEnumerable<LocalizedProperty>)LocalizedPropertyRepository.Table.Where((Expression<Func<LocalizedProperty, bool>>)((LocalizedProperty x) => x.LocaleKeyGroup == "ProductAttributeValue" && x.LocaleKey == "Name" && x.LanguageId == workingLanguageId)), pvav => ((BaseEntity)pvav.ProductAttributeValue).Id, (Expression<Func<LocalizedProperty, int>>)((LocalizedProperty lp) => lp.EntityId), (pvav,  pvav_lp) => new { pvav, pvav_lp }), _003C_003Eh__TransparentIdentifier0 => _003C_003Eh__TransparentIdentifier0.pvav_lp.DefaultIfEmpty(), (_003C_003Eh__TransparentIdentifier0,   lp) => new
				{
					ProductAttributeValue = _003C_003Eh__TransparentIdentifier0.pvav.ProductAttributeValue,
					LocalizedName = lp.LocaleValue
				});
				result = (await AsyncIQueryableExtensions.ToListAsync(query2)).Select(x =>
				{
					if (!string.IsNullOrEmpty(x.LocalizedName))
					{
						x.ProductAttributeValue.Name = x.LocalizedName;
					}
					return x.ProductAttributeValue;
				}).ToList();
			}
			else
			{
				result = await AsyncIQueryableExtensions.ToListAsync<ProductAttributeValue>(query2.Select(x => x.ProductAttributeValue));
			}
			return result;
		});
	}

	private async Task<IList<ProductAttributeValue>> GetAllProductVariantAttributeValuesByProductAttributeIdAndManufacturerIdInternalAsync(int productAttributeId, int manufacturerId, bool showHiddenProducts)
	{
		if (productAttributeId == 0 || manufacturerId == 0)
		{
			return null;
		}
		DateTime nowUtc = DateTime.UtcNow;
		IStaticCacheManager cacheManager = CacheManager;
		CacheKey pRODUCTATTRIBUTEVALUES_BY_PRODUCTATTRIBUTEID_AND_MANUFACTURERID_KEY = PRODUCTATTRIBUTEVALUES_BY_PRODUCTATTRIBUTEID_AND_MANUFACTURERID_KEY;
		object obj = productAttributeId;
		object obj2 = manufacturerId;
		object obj3 = showHiddenProducts;
		object obj4 = ((BaseEntity)(await _storeContext.GetCurrentStoreAsync())).Id;
		Language val = await _workContext.GetWorkingLanguageAsync();
		CacheKey val2 = ((ICacheKeyService)cacheManager).PrepareKeyForDefaultCache(pRODUCTATTRIBUTEVALUES_BY_PRODUCTATTRIBUTEID_AND_MANUFACTURERID_KEY, new object[5]
		{
			obj,
			obj2,
			obj3,
			obj4,
			((BaseEntity)val).Id
		});
		return await CacheManager.GetAsync<IList<ProductAttributeValue>>(val2, (Func<Task<IList<ProductAttributeValue>>>)async delegate
		{
			IList<int> groupProductIds = await _productServiceNopAjaxFilters.GetAllGroupProductIdsInManufacturerAsync(manufacturerId);
			IQueryable<Product> availableProductsQuery = await _customAclHelper.GetAvailableProductsForCurrentErpCustomerAsync();
			availableProductsQuery = await _storeHelper.GetProductsForCurrentStoreAsync(availableProductsQuery);
			bool includeFeaturedProductsInNormalList = await GetIncludeFeaturedProductsInNormalListAsync();
			var query2 = from _003C_003Eh__TransparentIdentifier4 in Queryable.SelectMany(Queryable.Join(ProductAttributeValueRepository.Table.Join((IEnumerable<ProductAttributeMapping>)ProductAttributeMappingRepository.Table, (Expression<Func<ProductAttributeValue, int>>)((ProductAttributeValue pvav) => pvav.ProductAttributeMappingId), (Expression<Func<ProductAttributeMapping, int>>)((ProductAttributeMapping pva) => ((BaseEntity)pva).Id), (ProductAttributeValue pvav, ProductAttributeMapping pva) => new { pvav, pva }), (IEnumerable<ProductAttribute>)ProductAttributeRepository.Table, _003C_003Eh__TransparentIdentifier0 => new
				{
					ProductAttributeId1 = _003C_003Eh__TransparentIdentifier0.pva.ProductAttributeId,
					ProductAttributeId2 = _003C_003Eh__TransparentIdentifier0.pva.ProductAttributeId
				}, (ProductAttribute pa) => new
				{
					ProductAttributeId1 = ((BaseEntity)pa).Id,
					ProductAttributeId2 = productAttributeId
				}, (_003C_003Eh__TransparentIdentifier0,   pa) => new { _003C_003Eh__TransparentIdentifier0, pa }).Join((IEnumerable<Product>)availableProductsQuery, _003C_003Eh__TransparentIdentifier1 => _003C_003Eh__TransparentIdentifier1._003C_003Eh__TransparentIdentifier0.pva.ProductId, (Expression<Func<Product, int>>)((Product p) => ((BaseEntity)p).Id), (_003C_003Eh__TransparentIdentifier1,   p) => new { _003C_003Eh__TransparentIdentifier1, p }).GroupJoin((IEnumerable<ProductManufacturer>)_productManufacturerRepository.Table.Where((Expression<Func<ProductManufacturer, bool>>)((ProductManufacturer x) => x.ManufacturerId == manufacturerId)), _003C_003Eh__TransparentIdentifier2 => ((BaseEntity)_003C_003Eh__TransparentIdentifier2.p).Id, (Expression<Func<ProductManufacturer, int>>)((ProductManufacturer pm) => pm.ProductId), (_003C_003Eh__TransparentIdentifier2,   p_pm) => new { _003C_003Eh__TransparentIdentifier2, p_pm }), _003C_003Eh__TransparentIdentifier3 => _003C_003Eh__TransparentIdentifier3.p_pm.DefaultIfEmpty(), (_003C_003Eh__TransparentIdentifier3,   pm) => new { _003C_003Eh__TransparentIdentifier3, pm })
				where (showHiddenProducts || _003C_003Eh__TransparentIdentifier4._003C_003Eh__TransparentIdentifier3._003C_003Eh__TransparentIdentifier2.p.Published) && !_003C_003Eh__TransparentIdentifier4._003C_003Eh__TransparentIdentifier3._003C_003Eh__TransparentIdentifier2.p.Deleted && ((_003C_003Eh__TransparentIdentifier4.pm != null && (includeFeaturedProductsInNormalList || !_003C_003Eh__TransparentIdentifier4.pm.IsFeaturedProduct) && (_003C_003Eh__TransparentIdentifier4._003C_003Eh__TransparentIdentifier3._003C_003Eh__TransparentIdentifier2.p.ParentGroupedProductId == 0 || _003C_003Eh__TransparentIdentifier4._003C_003Eh__TransparentIdentifier3._003C_003Eh__TransparentIdentifier2.p.VisibleIndividually)) || (_003C_003Eh__TransparentIdentifier4.pm == null && groupProductIds.Contains(_003C_003Eh__TransparentIdentifier4._003C_003Eh__TransparentIdentifier3._003C_003Eh__TransparentIdentifier2.p.ParentGroupedProductId))) && (!_003C_003Eh__TransparentIdentifier4._003C_003Eh__TransparentIdentifier3._003C_003Eh__TransparentIdentifier2.p.AvailableStartDateTimeUtc.HasValue || _003C_003Eh__TransparentIdentifier4._003C_003Eh__TransparentIdentifier3._003C_003Eh__TransparentIdentifier2.p.AvailableStartDateTimeUtc <= nowUtc) && (!_003C_003Eh__TransparentIdentifier4._003C_003Eh__TransparentIdentifier3._003C_003Eh__TransparentIdentifier2.p.AvailableEndDateTimeUtc.HasValue || _003C_003Eh__TransparentIdentifier4._003C_003Eh__TransparentIdentifier3._003C_003Eh__TransparentIdentifier2.p.AvailableEndDateTimeUtc >= nowUtc)
				orderby _003C_003Eh__TransparentIdentifier4._003C_003Eh__TransparentIdentifier3._003C_003Eh__TransparentIdentifier2._003C_003Eh__TransparentIdentifier1._003C_003Eh__TransparentIdentifier0.pvav.DisplayOrder
				select new
				{
					ProductAttributeValue = _003C_003Eh__TransparentIdentifier4._003C_003Eh__TransparentIdentifier3._003C_003Eh__TransparentIdentifier2._003C_003Eh__TransparentIdentifier1._003C_003Eh__TransparentIdentifier0.pvav,
					LocalizedName = string.Empty
				};
			new List<ProductAttributeValue>();
			IList<ProductAttributeValue> result;
			if ((await _languageService.GetAllLanguagesAsync(false, 0)).Count >= 2)
			{
				int workingLanguageId = ((BaseEntity)(await _workContext.GetWorkingLanguageAsync())).Id;
				query2 = Queryable.SelectMany(query2.GroupJoin((IEnumerable<LocalizedProperty>)LocalizedPropertyRepository.Table.Where((Expression<Func<LocalizedProperty, bool>>)((LocalizedProperty x) => x.LocaleKeyGroup == "ProductAttributeValue" && x.LocaleKey == "Name" && x.LanguageId == workingLanguageId)), pvav => ((BaseEntity)pvav.ProductAttributeValue).Id, (Expression<Func<LocalizedProperty, int>>)((LocalizedProperty lp) => lp.EntityId), (pvav,   pvav_lp) => new { pvav, pvav_lp }), _003C_003Eh__TransparentIdentifier0 => _003C_003Eh__TransparentIdentifier0.pvav_lp.DefaultIfEmpty(), (_003C_003Eh__TransparentIdentifier0,   lp) => new
				{
					ProductAttributeValue = _003C_003Eh__TransparentIdentifier0.pvav.ProductAttributeValue,
					LocalizedName = lp.LocaleValue
				});
				result = (await AsyncIQueryableExtensions.ToListAsync(query2)).Select(x =>
				{
					if (!string.IsNullOrEmpty(x.LocalizedName))
					{
						x.ProductAttributeValue.Name = x.LocalizedName;
					}
					return x.ProductAttributeValue;
				}).ToList();
			}
			else
			{
				result = await AsyncIQueryableExtensions.ToListAsync<ProductAttributeValue>(query2.Select(x => x.ProductAttributeValue));
			}
			return result;
		});
	}

	private async Task<IList<ProductAttributeValue>> GetAllProductVariantAttributeValuesByProductAttributeIdAndVendorIdInternalAsync(int productAttributeId, int vendorId, bool showHiddenProducts)
	{
		if (productAttributeId == 0 || vendorId == 0)
		{
			return null;
		}
		DateTime nowUtc = DateTime.UtcNow;
		IStaticCacheManager cacheManager = CacheManager;
		CacheKey pRODUCTATTRIBUTEVALUES_BY_PRODUCTATTRIBUTEID_AND_VENDORID_KEY = PRODUCTATTRIBUTEVALUES_BY_PRODUCTATTRIBUTEID_AND_VENDORID_KEY;
		object obj = productAttributeId;
		object obj2 = vendorId;
		object obj3 = showHiddenProducts;
		object obj4 = ((BaseEntity)(await _storeContext.GetCurrentStoreAsync())).Id;
		Language val = await _workContext.GetWorkingLanguageAsync();
		CacheKey val2 = ((ICacheKeyService)cacheManager).PrepareKeyForDefaultCache(pRODUCTATTRIBUTEVALUES_BY_PRODUCTATTRIBUTEID_AND_VENDORID_KEY, new object[5]
		{
			obj,
			obj2,
			obj3,
			obj4,
			((BaseEntity)val).Id
		});
		return await CacheManager.GetAsync<IList<ProductAttributeValue>>(val2, (Func<Task<IList<ProductAttributeValue>>>)async delegate
		{
			IList<int> groupProductIds = await _productServiceNopAjaxFilters.GetAllGroupProductIdsInVendorAsync(vendorId);
			IQueryable<Product> productsQuery = await _customAclHelper.GetAvailableProductsForCurrentErpCustomerAsync();
			productsQuery = await _storeHelper.GetProductsForCurrentStoreAsync(productsQuery);
			var query2 = from _003C_003Eh__TransparentIdentifier4 in Queryable.SelectMany(Queryable.Join(ProductAttributeValueRepository.Table.Join((IEnumerable<ProductAttributeMapping>)ProductAttributeMappingRepository.Table, (Expression<Func<ProductAttributeValue, int>>)((ProductAttributeValue pvav) => pvav.ProductAttributeMappingId), (Expression<Func<ProductAttributeMapping, int>>)((ProductAttributeMapping pva) => ((BaseEntity)pva).Id), (ProductAttributeValue pvav, ProductAttributeMapping pva) => new { pvav, pva }), (IEnumerable<ProductAttribute>)ProductAttributeRepository.Table, _003C_003Eh__TransparentIdentifier0 => new
				{
					ProductAttributeId1 = _003C_003Eh__TransparentIdentifier0.pva.ProductAttributeId,
					ProductAttributeId2 = _003C_003Eh__TransparentIdentifier0.pva.ProductAttributeId
				}, (ProductAttribute pa) => new
				{
					ProductAttributeId1 = ((BaseEntity)pa).Id,
					ProductAttributeId2 = productAttributeId
				}, (_003C_003Eh__TransparentIdentifier0,   pa) => new { _003C_003Eh__TransparentIdentifier0, pa }).Join((IEnumerable<Product>)productsQuery, _003C_003Eh__TransparentIdentifier1 => _003C_003Eh__TransparentIdentifier1._003C_003Eh__TransparentIdentifier0.pva.ProductId, (Expression<Func<Product, int>>)((Product p) => ((BaseEntity)p).Id), (_003C_003Eh__TransparentIdentifier1,   p) => new { _003C_003Eh__TransparentIdentifier1, p }).GroupJoin((IEnumerable<Vendor>)_vendorRepository.Table.Where((Expression<Func<Vendor, bool>>)((Vendor x) => ((BaseEntity)x).Id == vendorId)), _003C_003Eh__TransparentIdentifier2 => _003C_003Eh__TransparentIdentifier2.p.VendorId, (Expression<Func<Vendor, int>>)((Vendor v) => ((BaseEntity)v).Id), (_003C_003Eh__TransparentIdentifier2,  p_pv) => new { _003C_003Eh__TransparentIdentifier2, p_pv }), _003C_003Eh__TransparentIdentifier3 => _003C_003Eh__TransparentIdentifier3.p_pv.DefaultIfEmpty(), (_003C_003Eh__TransparentIdentifier3,   v) => new { _003C_003Eh__TransparentIdentifier3, v })
				where (showHiddenProducts || _003C_003Eh__TransparentIdentifier4._003C_003Eh__TransparentIdentifier3._003C_003Eh__TransparentIdentifier2.p.Published) && !_003C_003Eh__TransparentIdentifier4._003C_003Eh__TransparentIdentifier3._003C_003Eh__TransparentIdentifier2.p.Deleted && ((_003C_003Eh__TransparentIdentifier4.v != null && (_003C_003Eh__TransparentIdentifier4._003C_003Eh__TransparentIdentifier3._003C_003Eh__TransparentIdentifier2.p.ParentGroupedProductId == 0 || _003C_003Eh__TransparentIdentifier4._003C_003Eh__TransparentIdentifier3._003C_003Eh__TransparentIdentifier2.p.VisibleIndividually)) || (_003C_003Eh__TransparentIdentifier4.v == null && groupProductIds.Contains(_003C_003Eh__TransparentIdentifier4._003C_003Eh__TransparentIdentifier3._003C_003Eh__TransparentIdentifier2.p.ParentGroupedProductId))) && (!_003C_003Eh__TransparentIdentifier4._003C_003Eh__TransparentIdentifier3._003C_003Eh__TransparentIdentifier2.p.AvailableStartDateTimeUtc.HasValue || _003C_003Eh__TransparentIdentifier4._003C_003Eh__TransparentIdentifier3._003C_003Eh__TransparentIdentifier2.p.AvailableStartDateTimeUtc <= nowUtc) && (!_003C_003Eh__TransparentIdentifier4._003C_003Eh__TransparentIdentifier3._003C_003Eh__TransparentIdentifier2.p.AvailableEndDateTimeUtc.HasValue || _003C_003Eh__TransparentIdentifier4._003C_003Eh__TransparentIdentifier3._003C_003Eh__TransparentIdentifier2.p.AvailableEndDateTimeUtc >= nowUtc)
				orderby _003C_003Eh__TransparentIdentifier4._003C_003Eh__TransparentIdentifier3._003C_003Eh__TransparentIdentifier2._003C_003Eh__TransparentIdentifier1._003C_003Eh__TransparentIdentifier0.pvav.DisplayOrder
				select new
				{
					ProductAttributeValue = _003C_003Eh__TransparentIdentifier4._003C_003Eh__TransparentIdentifier3._003C_003Eh__TransparentIdentifier2._003C_003Eh__TransparentIdentifier1._003C_003Eh__TransparentIdentifier0.pvav,
					LocalizedName = string.Empty
				};
			new List<ProductAttributeValue>();
			IList<ProductAttributeValue> result;
			if ((await _languageService.GetAllLanguagesAsync(false, 0)).Count >= 2)
			{
				int workingLanguageId = ((BaseEntity)(await _workContext.GetWorkingLanguageAsync())).Id;
				query2 = Queryable.SelectMany(query2.GroupJoin((IEnumerable<LocalizedProperty>)LocalizedPropertyRepository.Table.Where((Expression<Func<LocalizedProperty, bool>>)((LocalizedProperty x) => x.LocaleKeyGroup == "ProductAttributeValue" && x.LocaleKey == "Name" && x.LanguageId == workingLanguageId)), pvav => ((BaseEntity)pvav.ProductAttributeValue).Id, (Expression<Func<LocalizedProperty, int>>)((LocalizedProperty lp) => lp.EntityId), (pvav,   pvav_lp) => new { pvav, pvav_lp }), _003C_003Eh__TransparentIdentifier0 => _003C_003Eh__TransparentIdentifier0.pvav_lp.DefaultIfEmpty(), (_003C_003Eh__TransparentIdentifier0,   lp) => new
				{
					ProductAttributeValue = _003C_003Eh__TransparentIdentifier0.pvav.ProductAttributeValue,
					LocalizedName = lp.LocaleValue
				});
				result = (await AsyncIQueryableExtensions.ToListAsync(query2)).Select(x =>
				{
					if (!string.IsNullOrEmpty(x.LocalizedName))
					{
						x.ProductAttributeValue.Name = x.LocalizedName;
					}
					return x.ProductAttributeValue;
				}).ToList();
			}
			else
			{
				result = await AsyncIQueryableExtensions.ToListAsync<ProductAttributeValue>(query2.Select(x => x.ProductAttributeValue));
			}
			return result;
		});
	}

	private async Task<IList<ProductAttributeMapping>> GetAllProductVariantAttributesByProductIdInternalAsync(int productId, bool showHiddenProducts)
	{
		if (productId == 0)
		{
			return null;
		}
		IStaticCacheManager cacheManager = CacheManager;
		CacheKey pRODUCTVARIANTATTRIBUTES_BY_PRODUCTID_KEY = PRODUCTVARIANTATTRIBUTES_BY_PRODUCTID_KEY;
		object obj = productId;
		Store val = await _storeContext.GetCurrentStoreAsync();
		CacheKey val2 = ((ICacheKeyService)cacheManager).PrepareKeyForDefaultCache(pRODUCTVARIANTATTRIBUTES_BY_PRODUCTID_KEY, new object[2]
		{
			obj,
			((BaseEntity)val).Id
		});
		return new List<ProductAttributeMapping>(await CacheManager.GetAsync<IEnumerable<ProductAttributeMapping>>(val2, (Func<Task<IEnumerable<ProductAttributeMapping>>>)async delegate
		{
			DateTime nowUtc = DateTime.UtcNow;
			IQueryable<Product> inner = await _customAclHelper.GetAvailableProductsForCurrentErpCustomerAsync();
			return await AsyncIQueryableExtensions.ToListAsync<ProductAttributeMapping>(from _003C_003Eh__TransparentIdentifier1 in ProductAttributeMappingRepository.Table.Join((IEnumerable<ProductAttribute>)ProductAttributeRepository.Table, (Expression<Func<ProductAttributeMapping, int>>)((ProductAttributeMapping pva) => pva.ProductAttributeId), (Expression<Func<ProductAttribute, int>>)((ProductAttribute pa) => ((BaseEntity)pa).Id), (ProductAttributeMapping pva, ProductAttribute pa) => new { pva, pa }).Join((IEnumerable<Product>)inner, _003C_003Eh__TransparentIdentifier0 => _003C_003Eh__TransparentIdentifier0.pva.ProductId, (Expression<Func<Product, int>>)((Product p) => ((BaseEntity)p).Id), (_003C_003Eh__TransparentIdentifier0,   p) => new { _003C_003Eh__TransparentIdentifier0, p })
				where ((BaseEntity)_003C_003Eh__TransparentIdentifier1.p).Id == productId && (showHiddenProducts || _003C_003Eh__TransparentIdentifier1.p.Published) && !_003C_003Eh__TransparentIdentifier1.p.Deleted && (!_003C_003Eh__TransparentIdentifier1.p.AvailableStartDateTimeUtc.HasValue || _003C_003Eh__TransparentIdentifier1.p.AvailableStartDateTimeUtc <= nowUtc) && (!_003C_003Eh__TransparentIdentifier1.p.AvailableEndDateTimeUtc.HasValue || _003C_003Eh__TransparentIdentifier1.p.AvailableEndDateTimeUtc >= nowUtc)
				select _003C_003Eh__TransparentIdentifier1._003C_003Eh__TransparentIdentifier0.pva);
		}));
	}

	private async Task<IList<ProductAttributeMapping>> GetAllProductVariantAttributesWhichHaveValuesByProductIdInternalAsync(int productId, bool showHiddenProducts)
	{
		if (productId == 0)
		{
			return null;
		}
		IStaticCacheManager cacheManager = CacheManager;
		CacheKey pRODUCTVARIANTATTRIBUTES_WHICH_HAVE_VALUES_BY_PRODUCTID_KEY = PRODUCTVARIANTATTRIBUTES_WHICH_HAVE_VALUES_BY_PRODUCTID_KEY;
		object obj = productId;
		Store val = await _storeContext.GetCurrentStoreAsync();
		CacheKey val2 = ((ICacheKeyService)cacheManager).PrepareKeyForDefaultCache(pRODUCTVARIANTATTRIBUTES_WHICH_HAVE_VALUES_BY_PRODUCTID_KEY, new object[2]
		{
			obj,
			((BaseEntity)val).Id
		});
		return new List<ProductAttributeMapping>(await CacheManager.GetAsync<IEnumerable<ProductAttributeMapping>>(val2, (Func<Task<IEnumerable<ProductAttributeMapping>>>)async delegate
		{
			DateTime nowUtc = DateTime.UtcNow;
			IQueryable<Product> inner = await _customAclHelper.GetAvailableProductsForCurrentErpCustomerAsync();
			return await AsyncIQueryableExtensions.ToListAsync<ProductAttributeMapping>(from _003C_003Eh__TransparentIdentifier2 in ProductAttributeMappingRepository.Table.Join((IEnumerable<ProductAttribute>)ProductAttributeRepository.Table, (Expression<Func<ProductAttributeMapping, int>>)((ProductAttributeMapping pva) => pva.ProductAttributeId), (Expression<Func<ProductAttribute, int>>)((ProductAttribute pa) => ((BaseEntity)pa).Id), (ProductAttributeMapping pva, ProductAttribute pa) => new { pva, pa }).Join((IEnumerable<ProductAttributeValue>)ProductAttributeValueRepository.Table, _003C_003Eh__TransparentIdentifier0 => ((BaseEntity)_003C_003Eh__TransparentIdentifier0.pva).Id, (Expression<Func<ProductAttributeValue, int>>)((ProductAttributeValue pvav) => pvav.ProductAttributeMappingId), (_003C_003Eh__TransparentIdentifier0,   pvav) => new { _003C_003Eh__TransparentIdentifier0, pvav }).Join((IEnumerable<Product>)inner, _003C_003Eh__TransparentIdentifier1 => _003C_003Eh__TransparentIdentifier1._003C_003Eh__TransparentIdentifier0.pva.ProductId, (Expression<Func<Product, int>>)((Product p) => ((BaseEntity)p).Id), (_003C_003Eh__TransparentIdentifier1,   p) => new { _003C_003Eh__TransparentIdentifier1, p })
				where ((BaseEntity)_003C_003Eh__TransparentIdentifier2.p).Id == productId && (showHiddenProducts || _003C_003Eh__TransparentIdentifier2.p.Published) && !_003C_003Eh__TransparentIdentifier2.p.Deleted && (!_003C_003Eh__TransparentIdentifier2.p.AvailableStartDateTimeUtc.HasValue || _003C_003Eh__TransparentIdentifier2.p.AvailableStartDateTimeUtc <= nowUtc) && (!_003C_003Eh__TransparentIdentifier2.p.AvailableEndDateTimeUtc.HasValue || _003C_003Eh__TransparentIdentifier2.p.AvailableEndDateTimeUtc >= nowUtc)
				select _003C_003Eh__TransparentIdentifier2._003C_003Eh__TransparentIdentifier1._003C_003Eh__TransparentIdentifier0.pva);
		}));
	}

	private async Task<IList<ProductAttributeMapping>> GetAllProductVariantAttributesWhichHaveValuesByProductIdsInternalAsync(IList<int> productIds, bool showHiddenProducts)
	{
		if (productIds == null || productIds.Count == 0)
		{
			return null;
		}
		CacheKey val = ((ICacheKeyService)CacheManager).PrepareKeyForDefaultCache(PRODUCTVARIANTATTRIBUTES_WHICH_HAVE_VALUES_BY_PRODUCTIDS_KEY, new object[2]
		{
			productIds,
			_storeContext.GetCurrentStoreAsync().Id
		});
		return (await CacheManager.GetAsync<IEnumerable<ProductAttributeMapping>>(val, (Func<Task<IEnumerable<ProductAttributeMapping>>>)async delegate
		{
			DateTime nowUtc = DateTime.UtcNow;
			List<int> productIdsArray = productIds.ToList();
			IQueryable<Product> inner = await _customAclHelper.GetAvailableProductsForCurrentErpCustomerAsync();
			return await AsyncIQueryableExtensions.ToListAsync<ProductAttributeMapping>(from _003C_003Eh__TransparentIdentifier2 in ProductAttributeMappingRepository.Table.Join((IEnumerable<ProductAttribute>)ProductAttributeRepository.Table, (Expression<Func<ProductAttributeMapping, int>>)((ProductAttributeMapping pva) => pva.ProductAttributeId), (Expression<Func<ProductAttribute, int>>)((ProductAttribute pa) => ((BaseEntity)pa).Id), (ProductAttributeMapping pva, ProductAttribute pa) => new { pva, pa }).Join((IEnumerable<ProductAttributeValue>)ProductAttributeValueRepository.Table, _003C_003Eh__TransparentIdentifier0 => ((BaseEntity)_003C_003Eh__TransparentIdentifier0.pva).Id, (Expression<Func<ProductAttributeValue, int>>)((ProductAttributeValue pvav) => pvav.ProductAttributeMappingId), (_003C_003Eh__TransparentIdentifier0,   pvav) => new { _003C_003Eh__TransparentIdentifier0, pvav }).Join((IEnumerable<Product>)inner, _003C_003Eh__TransparentIdentifier1 => _003C_003Eh__TransparentIdentifier1._003C_003Eh__TransparentIdentifier0.pva.ProductId, (Expression<Func<Product, int>>)((Product p) => ((BaseEntity)p).Id), (_003C_003Eh__TransparentIdentifier1,   p) => new { _003C_003Eh__TransparentIdentifier1, p })
				where productIdsArray.Contains(((BaseEntity)_003C_003Eh__TransparentIdentifier2.p).Id) && (showHiddenProducts || _003C_003Eh__TransparentIdentifier2.p.Published) && !_003C_003Eh__TransparentIdentifier2.p.Deleted && (!_003C_003Eh__TransparentIdentifier2.p.AvailableStartDateTimeUtc.HasValue || _003C_003Eh__TransparentIdentifier2.p.AvailableStartDateTimeUtc <= nowUtc) && (!_003C_003Eh__TransparentIdentifier2.p.AvailableEndDateTimeUtc.HasValue || _003C_003Eh__TransparentIdentifier2.p.AvailableEndDateTimeUtc >= nowUtc)
				select _003C_003Eh__TransparentIdentifier2._003C_003Eh__TransparentIdentifier1._003C_003Eh__TransparentIdentifier0.pva);
		})).ToList();
	}

	private async Task<IList<ProductAttributeProductAttributeValueDTO>> GetProductAttributeProductAttributeValueDtosByProductAttributeMappingIdsInternalAsync(IList<int> productAttributeMappingIds)
	{
		if (productAttributeMappingIds == null || !productAttributeMappingIds.Any())
		{
			return null;
		}
		IStaticCacheManager cacheManager = CacheManager;
		CacheKey pRODUCTATTRIBUTEVALUES_AND_PRODUCT_ATTRIBUTES_BY_PRODUCTATTRIBUTEMAPPINGS_KEY = PRODUCTATTRIBUTEVALUES_AND_PRODUCT_ATTRIBUTES_BY_PRODUCTATTRIBUTEMAPPINGS_KEY;
		object obj = productAttributeMappingIds;
		object obj2 = ((BaseEntity)(await _storeContext.GetCurrentStoreAsync())).Id;
		Language val = await _workContext.GetWorkingLanguageAsync();
		CacheKey val2 = ((ICacheKeyService)cacheManager).PrepareKeyForDefaultCache(pRODUCTATTRIBUTEVALUES_AND_PRODUCT_ATTRIBUTES_BY_PRODUCTATTRIBUTEMAPPINGS_KEY, new object[3]
		{
			obj,
			obj2,
			((BaseEntity)val).Id
		});
		return await CacheManager.GetAsync<IList<ProductAttributeProductAttributeValueDTO>>(val2, (Func<Task<IList<ProductAttributeProductAttributeValueDTO>>>)async delegate
		{
			new List<ProductAttributeProductAttributeValueDTO>();
			IQueryable<ProductAttributeProductAttributeValueDTO> query = from _003C_003Eh__TransparentIdentifier0 in ProductAttributeValueRepository.Table.Join((IEnumerable<ProductAttributeMapping>)ProductAttributeMappingRepository.Table, (Expression<Func<ProductAttributeValue, int>>)((ProductAttributeValue pvav) => pvav.ProductAttributeMappingId), (Expression<Func<ProductAttributeMapping, int>>)((ProductAttributeMapping pva) => ((BaseEntity)pva).Id), (ProductAttributeValue pvav, ProductAttributeMapping pva) => new { pvav, pva })
				where productAttributeMappingIds.Contains(((BaseEntity)_003C_003Eh__TransparentIdentifier0.pva).Id)
				orderby _003C_003Eh__TransparentIdentifier0.pvav.DisplayOrder
				select new ProductAttributeProductAttributeValueDTO
				{
					ProductAttributeValue = _003C_003Eh__TransparentIdentifier0.pvav,
					ProductAttributeId = _003C_003Eh__TransparentIdentifier0.pva.ProductAttributeId,
					LocalizedName = string.Empty
				};
			IList<ProductAttributeProductAttributeValueDTO> result;
			if ((await _languageService.GetAllLanguagesAsync(false, 0)).Count >= 2)
			{
				int workingLanguageId = ((BaseEntity)(await _workContext.GetWorkingLanguageAsync())).Id;
				query = Queryable.SelectMany(query.GroupJoin((IEnumerable<LocalizedProperty>)LocalizedPropertyRepository.Table.Where((Expression<Func<LocalizedProperty, bool>>)((LocalizedProperty x) => x.LocaleKeyGroup == "ProductAttributeValue" && x.LocaleKey == "Name" && x.LanguageId == workingLanguageId)), (Expression<Func<ProductAttributeProductAttributeValueDTO, int>>)((ProductAttributeProductAttributeValueDTO pvav) => ((BaseEntity)pvav.ProductAttributeValue).Id), (Expression<Func<LocalizedProperty, int>>)((LocalizedProperty lp) => lp.EntityId), (ProductAttributeProductAttributeValueDTO pvav, IEnumerable<LocalizedProperty> pvav_lp) => new { pvav, pvav_lp }), _003C_003Eh__TransparentIdentifier0 => _003C_003Eh__TransparentIdentifier0.pvav_lp.DefaultIfEmpty(), (_003C_003Eh__TransparentIdentifier0,   lp) => new ProductAttributeProductAttributeValueDTO
				{
					ProductAttributeValue = _003C_003Eh__TransparentIdentifier0.pvav.ProductAttributeValue,
					ProductAttributeId = _003C_003Eh__TransparentIdentifier0.pvav.ProductAttributeId,
					LocalizedName = lp.LocaleValue
				});
				result = (await AsyncIQueryableExtensions.ToListAsync<ProductAttributeProductAttributeValueDTO>(query)).Select(delegate(ProductAttributeProductAttributeValueDTO x)
				{
					if (!string.IsNullOrEmpty(x.LocalizedName))
					{
						x.ProductAttributeValue.Name = x.LocalizedName;
					}
					return x;
				}).ToList();
			}
			else
			{
				result = await AsyncIQueryableExtensions.ToListAsync<ProductAttributeProductAttributeValueDTO>(query);
			}
			return result;
		});
	}

	private async Task<bool> GetIncludeFeaturedProductsInNormalListAsync()
	{
		return await SettingService.GetSettingByKeyAsync<bool>("catalogsettings.includefeaturedproductsinnormallists", false, 0, false);
	}
}
