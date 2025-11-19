using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Core.Caching;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Stores;
using Nop.Data;
using Nop.Services.Configuration;
using SevenSpikes.Nop.Services.Catalog;
using SevenSpikes.Nop.Services.Helpers;

namespace SevenSpikes.Nop.Plugins.AjaxFilters.Services;

public class SpecificationAttributeService7Spikes : ISpecificationAttributeService7Spikes
{
	private readonly IRepository<ProductManufacturer> _productManufacturerRepository;

	private readonly IAclHelper _aclHelper;

	private readonly IStoreContext _storeContext;

	private readonly IStoreHelper _storeHelper;

	private readonly ICustomAclHelper _customAclHelper;

	private CacheKey SPECIFICATIONATTRIBUTEOPTIONS_DICTIONARY_KEY => new CacheKey("Nop.specificationattributeoptions.dictionary.store.id-{0}", Array.Empty<string>());

	private CacheKey SPECIFICATIONATTRIBUTEOPTIONS_BY_CATEGORYID_KEY => new CacheKey("Nop.specificationattributeoptions.categoryid-{0}.includeproductsinsubcategories-{1}.showhidden-{2}.store.id-{3}", Array.Empty<string>());

	private CacheKey SPECIFICATIONATTRIBUTEOPTIONS_BY_MANUFACTURERID_KEY => new CacheKey("Nop.specificationattributeoptions.manufacturerid-{0}.showhidden-{1}.store.id-{2}", Array.Empty<string>());

	private CacheKey SPECIFICATIONATTRIBUTEOPTIONS_BY_VENDORID_KEY => new CacheKey("Nop.specificationattributeoptions.vendorid-{0}.showhidden-{1}.store.id-{2}", Array.Empty<string>());

	private CacheKey SPECIFICATIONATTRIBUTEOPTIONS_BY_IDS_KEY => new CacheKey("Nop.specificationattributeoptions.ids-{0}.store.id-{1}", Array.Empty<string>());

	private CacheKey SPECIFICATIONATTRIBUTEOPTIONS_BY_PRODUCTID_KEY => new CacheKey("Nop.specificationattributeoptions.productid-{0}.store.id-{1}", Array.Empty<string>());

	private CacheKey SPECIFICATIONATTRIBUTEOPTIONS_BY_PRODUCTIDS_KEY => new CacheKey("Nop.specificationattributeoptions.productids-{0}.store.id-{1}", Array.Empty<string>());

	private CacheKey SPECIFICATIONATTRIBUTEOPTIONS_ALL_KEY => new CacheKey("Nop.specificationattributeoptions.all.store.id-{0}", Array.Empty<string>());

	private CacheKey SPECIFICATIONATTRIBUTEOPTIONS_BY_IDS_AND_SPECIFICATIONID_KEY => new CacheKey("Nop.specificationattributeoptions.ids-{0}.specification.id-{1}.store.id-{2}", Array.Empty<string>());

	private IRepository<ProductCategory> ProductCategoryRepository { get; set; }

	private IRepository<ProductSpecificationAttribute> ProductSpecificationAttributeRepository { get; set; }

	private IRepository<SpecificationAttribute> SpecificationAttributeRepository { get; set; }

	private IRepository<SpecificationAttributeOption> SpecificationAttributeOptionRepository { get; set; }

	private ICategoryService7Spikes CategoryService7Spikes { get; set; }

	private ISettingService SettingService { get; set; }

	private IStaticCacheManager CacheManager { get; set; }

    public SpecificationAttributeService7Spikes(IRepository<ProductManufacturer> productManufacturerRepository, IRepository<ProductCategory> productCategoryRepository, IRepository<ProductSpecificationAttribute> productSpecificationAttributeRepository, IRepository<SpecificationAttribute> specificationAttributeRepository, IRepository<SpecificationAttributeOption> specificationAttributeOptionFilterRepository, ISettingService settingService, IStaticCacheManager cacheManager, IAclHelper aclHelper, IStoreContext storeContext, IStoreHelper storeHelper, ICategoryService7Spikes categoryService7Spikes, ICustomAclHelper customAclHelper)
    {
        _productManufacturerRepository = productManufacturerRepository;
        _aclHelper = aclHelper;
        _storeContext = storeContext;
        _storeHelper = storeHelper;
        ProductCategoryRepository = productCategoryRepository;
        ProductSpecificationAttributeRepository = productSpecificationAttributeRepository;
        SpecificationAttributeRepository = specificationAttributeRepository;
        SpecificationAttributeOptionRepository = specificationAttributeOptionFilterRepository;
        CategoryService7Spikes = categoryService7Spikes;
        SettingService = settingService;
        CacheManager = cacheManager;
        _customAclHelper = customAclHelper;
    }

    public virtual async Task<IList<SpecificationAttributeOption>> GetSpecificationAttributeOptionsByIdsAndSpecificationIdAsync(IList<int> specificationAttributeOptionIds, int specificationId)
	{
		return await GetSpecificationAttributeOptionsByIdsAndSpecificationIdInternalAsync(specificationAttributeOptionIds, specificationId);
	}

	public virtual async Task<IList<SpecificationAttributeOption>> GetSpecificationAttributeOptionsByCategoryIdAsync(int categoryId, bool includeProductsInSubcategories = false, bool showHiddenProducts = false)
	{
		return await GetSpecificationAttributeOptionsByCategoryIdInternalAsync(categoryId, includeProductsInSubcategories, showHiddenProducts);
	}

	public async Task<IList<SpecificationAttributeOption>> GetSpecificationAttributeOptionsByManufacturerIdAsync(int manufacturerId, bool showHiddenProducts = false)
	{
		return await GetSpecificationAttributeOptionsByManufacturerIdInternalAsync(manufacturerId, showHiddenProducts);
	}

	public async Task<IList<SpecificationAttributeOption>> GetSpecificationAttributeOptionsByVendorIdAsync(int vendorId, bool showHiddenProducts = false)
	{
		return await GetSpecificationAttributeOptionsByVendorIdInternalAsync(vendorId, showHiddenProducts);
	}

	public virtual async Task<IList<SpecificationAttributeOption>> GetSpecificationAttributeOptionsByIdsAsync(IList<int> specificationAttributeOptionIds)
	{
		return await GetSpecificationAttributeOptionsByIdsInternalAsync(specificationAttributeOptionIds);
	}

	public virtual async Task<IList<SpecificationAttributeOption>> GetAllSpecificationAttributeOptionsAsync(bool includeNotAllowFilteringOptions = false)
	{
		return await GetAllSpecificationAttributeOptionsInternalAsync(includeNotAllowFilteringOptions);
	}

	public virtual async Task<IList<SpecificationAttributeOption>> GetSpecificationAttributeOptionsByProductIdAsync(int productId, bool includeNotAllowFilteringOptions = false)
	{
		return await GetSpecificationAttributeOptionsByProductIdInternalAsync(productId, includeNotAllowFilteringOptions);
	}

	public virtual async Task<IEnumerable<SpecificationAttributeOption>> GetSpecificationAttributeOptionsByProductIdsAsync(IList<int> productIds, bool showHiddenProducts = false)
	{
		return await GetSpecificationAttributeOptionsByProductIdsInternalAsync(productIds, showHiddenProducts);
	}

	public virtual async Task<IDictionary<int, IList<int>>> GetSpecificationAttributeOptionsDictionaryForProductsAsync(IQueryable<Product> products)
	{
		return await GetSpecificationAttributeOptionsDictionaryForProductsInternalAsync(products);
	}

	public virtual async Task<IDictionary<int, IList<int>>> GetSpecificationAttributeOptionsDictionaryAsync()
	{
		return await GetSpecificationAttributeOptionsDictionaryInternalAsync();
	}

	private async Task<IList<SpecificationAttributeOption>> GetSpecificationAttributeOptionsByIdsAndSpecificationIdInternalAsync(IList<int> specificationAttributeOptionIds, int specificationId)
	{
		if (specificationAttributeOptionIds == null || specificationAttributeOptionIds.Count == 0)
		{
			return null;
		}
		IStaticCacheManager cacheManager = CacheManager;
		CacheKey sPECIFICATIONATTRIBUTEOPTIONS_BY_IDS_AND_SPECIFICATIONID_KEY = SPECIFICATIONATTRIBUTEOPTIONS_BY_IDS_AND_SPECIFICATIONID_KEY;
		object obj = specificationAttributeOptionIds;
		object obj2 = specificationId;
		Store val = await _storeContext.GetCurrentStoreAsync();
		CacheKey val2 = ((ICacheKeyService)cacheManager).PrepareKeyForDefaultCache(sPECIFICATIONATTRIBUTEOPTIONS_BY_IDS_AND_SPECIFICATIONID_KEY, new object[3]
		{
			obj,
			obj2,
			((BaseEntity)val).Id
		});
		return new List<SpecificationAttributeOption>(await CacheManager.GetAsync<IEnumerable<SpecificationAttributeOption>>(val2, (Func<Task<IEnumerable<SpecificationAttributeOption>>>)(async () => await AsyncIQueryableExtensions.ToListAsync<SpecificationAttributeOption>(SpecificationAttributeOptionRepository.Table.Where((Expression<Func<SpecificationAttributeOption, bool>>)((SpecificationAttributeOption sao) => specificationAttributeOptionIds.Contains(((BaseEntity)sao).Id) && sao.SpecificationAttributeId == specificationId))))));
	}

	private async Task<IList<SpecificationAttributeOption>> GetSpecificationAttributeOptionsByCategoryIdInternalAsync(int categoryId, bool includeProductsInSubcategories, bool showHiddenProducts)
	{
		if (categoryId == 0)
		{
			return null;
		}
		DateTime nowUtc = DateTime.UtcNow;
		IStaticCacheManager cacheManager = CacheManager;
		CacheKey sPECIFICATIONATTRIBUTEOPTIONS_BY_CATEGORYID_KEY = SPECIFICATIONATTRIBUTEOPTIONS_BY_CATEGORYID_KEY;
		object obj = categoryId;
		object obj2 = includeProductsInSubcategories;
		object obj3 = showHiddenProducts;
		Store val = await _storeContext.GetCurrentStoreAsync();
		CacheKey val2 = ((ICacheKeyService)cacheManager).PrepareKeyForDefaultCache(sPECIFICATIONATTRIBUTEOPTIONS_BY_CATEGORYID_KEY, new object[4]
		{
			obj,
			obj2,
			obj3,
			((BaseEntity)val).Id
		});
		return (await CacheManager.GetAsync<IEnumerable<SpecificationAttributeOption>>(val2, (Func<Task<IEnumerable<SpecificationAttributeOption>>>)async delegate
		{
			List<int> subCategoryIds = new List<int>();
			if (includeProductsInSubcategories)
			{
				subCategoryIds = await CategoryService7Spikes.GetCategoryIdsByParentCategoryAsync(categoryId);
			}
			IQueryable<Product> availableProductsQuery = await _customAclHelper.GetAvailableProductsForCurrentErpCustomerAsync();
			availableProductsQuery = await _storeHelper.GetProductsForCurrentStoreAsync(availableProductsQuery);
			bool includeFeaturedProductsInNormalList = await GetIncludeFeaturedProductsInNormalListAsync();
			return await AsyncIQueryableExtensions.ToListAsync<SpecificationAttributeOption>((from _003C_003Eh__TransparentIdentifier3 in SpecificationAttributeOptionRepository.Table.Join((IEnumerable<ProductSpecificationAttribute>)ProductSpecificationAttributeRepository.Table, (Expression<Func<SpecificationAttributeOption, int>>)((SpecificationAttributeOption saof) => ((BaseEntity)saof).Id), (Expression<Func<ProductSpecificationAttribute, int>>)((ProductSpecificationAttribute psa) => psa.SpecificationAttributeOptionId), (SpecificationAttributeOption saof, ProductSpecificationAttribute psa) => new { saof, psa }).Join((IEnumerable<SpecificationAttribute>)SpecificationAttributeRepository.Table, _003C_003Eh__TransparentIdentifier0 => _003C_003Eh__TransparentIdentifier0.saof.SpecificationAttributeId, (Expression<Func<SpecificationAttribute, int>>)((SpecificationAttribute sa) => ((BaseEntity)sa).Id), (_003C_003Eh__TransparentIdentifier0,   sa) => new { _003C_003Eh__TransparentIdentifier0, sa }).Join((IEnumerable<Product>)availableProductsQuery, _003C_003Eh__TransparentIdentifier1 => _003C_003Eh__TransparentIdentifier1._003C_003Eh__TransparentIdentifier0.psa.ProductId, (Expression<Func<Product, int>>)((Product p) => ((BaseEntity)p).Id), (_003C_003Eh__TransparentIdentifier1,   p) => new { _003C_003Eh__TransparentIdentifier1, p })
					.Join((IEnumerable<ProductCategory>)ProductCategoryRepository.Table, _003C_003Eh__TransparentIdentifier2 => ((BaseEntity)_003C_003Eh__TransparentIdentifier2.p).Id, (Expression<Func<ProductCategory, int>>)((ProductCategory pc) => pc.ProductId), (_003C_003Eh__TransparentIdentifier2,   pc) => new { _003C_003Eh__TransparentIdentifier2, pc })
				where (_003C_003Eh__TransparentIdentifier3.pc.CategoryId == categoryId || (includeProductsInSubcategories && subCategoryIds.Contains(_003C_003Eh__TransparentIdentifier3.pc.CategoryId))) && (includeFeaturedProductsInNormalList || !_003C_003Eh__TransparentIdentifier3.pc.IsFeaturedProduct) && (_003C_003Eh__TransparentIdentifier3._003C_003Eh__TransparentIdentifier2.p.ParentGroupedProductId == 0 || _003C_003Eh__TransparentIdentifier3._003C_003Eh__TransparentIdentifier2.p.VisibleIndividually) && (showHiddenProducts || _003C_003Eh__TransparentIdentifier3._003C_003Eh__TransparentIdentifier2.p.Published) && !_003C_003Eh__TransparentIdentifier3._003C_003Eh__TransparentIdentifier2.p.Deleted && (!_003C_003Eh__TransparentIdentifier3._003C_003Eh__TransparentIdentifier2.p.AvailableStartDateTimeUtc.HasValue || _003C_003Eh__TransparentIdentifier3._003C_003Eh__TransparentIdentifier2.p.AvailableStartDateTimeUtc <= nowUtc) && (!_003C_003Eh__TransparentIdentifier3._003C_003Eh__TransparentIdentifier2.p.AvailableEndDateTimeUtc.HasValue || _003C_003Eh__TransparentIdentifier3._003C_003Eh__TransparentIdentifier2.p.AvailableEndDateTimeUtc >= nowUtc) && _003C_003Eh__TransparentIdentifier3._003C_003Eh__TransparentIdentifier2._003C_003Eh__TransparentIdentifier1._003C_003Eh__TransparentIdentifier0.psa.AllowFiltering
				select _003C_003Eh__TransparentIdentifier3._003C_003Eh__TransparentIdentifier2._003C_003Eh__TransparentIdentifier1._003C_003Eh__TransparentIdentifier0.saof).Distinct());
		})).ToList().ToList();
	}

	private async Task<IList<SpecificationAttributeOption>> GetSpecificationAttributeOptionsByManufacturerIdInternalAsync(int manufacturerId, bool showHiddenProducts)
	{
		if (manufacturerId == 0)
		{
			return null;
		}
		DateTime nowUtc = DateTime.UtcNow;
		IStaticCacheManager cacheManager = CacheManager;
		CacheKey sPECIFICATIONATTRIBUTEOPTIONS_BY_MANUFACTURERID_KEY = SPECIFICATIONATTRIBUTEOPTIONS_BY_MANUFACTURERID_KEY;
		object obj = manufacturerId;
		object obj2 = showHiddenProducts;
		Store val = await _storeContext.GetCurrentStoreAsync();
		CacheKey val2 = ((ICacheKeyService)cacheManager).PrepareKeyForDefaultCache(sPECIFICATIONATTRIBUTEOPTIONS_BY_MANUFACTURERID_KEY, new object[3]
		{
			obj,
			obj2,
			((BaseEntity)val).Id
		});
		return (await CacheManager.GetAsync<IEnumerable<SpecificationAttributeOption>>(val2, (Func<Task<IEnumerable<SpecificationAttributeOption>>>)async delegate
		{
			IQueryable<Product> availableProductsQuery = await _customAclHelper.GetAvailableProductsForCurrentErpCustomerAsync();
			availableProductsQuery = await _storeHelper.GetProductsForCurrentStoreAsync(availableProductsQuery);
			bool includeFeaturedProductsInNormalList = await GetIncludeFeaturedProductsInNormalListAsync();
			return await AsyncIQueryableExtensions.ToListAsync<SpecificationAttributeOption>((from _003C_003Eh__TransparentIdentifier3 in SpecificationAttributeOptionRepository.Table.Join((IEnumerable<ProductSpecificationAttribute>)ProductSpecificationAttributeRepository.Table, (Expression<Func<SpecificationAttributeOption, int>>)((SpecificationAttributeOption saof) => ((BaseEntity)saof).Id), (Expression<Func<ProductSpecificationAttribute, int>>)((ProductSpecificationAttribute psa) => psa.SpecificationAttributeOptionId), (SpecificationAttributeOption saof, ProductSpecificationAttribute psa) => new { saof, psa }).Join((IEnumerable<SpecificationAttribute>)SpecificationAttributeRepository.Table, _003C_003Eh__TransparentIdentifier0 => _003C_003Eh__TransparentIdentifier0.saof.SpecificationAttributeId, (Expression<Func<SpecificationAttribute, int>>)((SpecificationAttribute sa) => ((BaseEntity)sa).Id), (_003C_003Eh__TransparentIdentifier0,  sa) => new { _003C_003Eh__TransparentIdentifier0, sa }).Join((IEnumerable<Product>)availableProductsQuery, _003C_003Eh__TransparentIdentifier1 => _003C_003Eh__TransparentIdentifier1._003C_003Eh__TransparentIdentifier0.psa.ProductId, (Expression<Func<Product, int>>)((Product p) => ((BaseEntity)p).Id), (_003C_003Eh__TransparentIdentifier1,  p) => new { _003C_003Eh__TransparentIdentifier1, p })
					.Join((IEnumerable<ProductManufacturer>)_productManufacturerRepository.Table, _003C_003Eh__TransparentIdentifier2 => ((BaseEntity)_003C_003Eh__TransparentIdentifier2.p).Id, (Expression<Func<ProductManufacturer, int>>)((ProductManufacturer pm) => pm.ProductId), (_003C_003Eh__TransparentIdentifier2,  pm) => new { _003C_003Eh__TransparentIdentifier2, pm })
				where _003C_003Eh__TransparentIdentifier3.pm.ManufacturerId == manufacturerId && (includeFeaturedProductsInNormalList || !_003C_003Eh__TransparentIdentifier3.pm.IsFeaturedProduct) && (_003C_003Eh__TransparentIdentifier3._003C_003Eh__TransparentIdentifier2.p.ParentGroupedProductId == 0 || _003C_003Eh__TransparentIdentifier3._003C_003Eh__TransparentIdentifier2.p.VisibleIndividually) && (showHiddenProducts || _003C_003Eh__TransparentIdentifier3._003C_003Eh__TransparentIdentifier2.p.Published) && !_003C_003Eh__TransparentIdentifier3._003C_003Eh__TransparentIdentifier2.p.Deleted && (!_003C_003Eh__TransparentIdentifier3._003C_003Eh__TransparentIdentifier2.p.AvailableStartDateTimeUtc.HasValue || _003C_003Eh__TransparentIdentifier3._003C_003Eh__TransparentIdentifier2.p.AvailableStartDateTimeUtc <= nowUtc) && (!_003C_003Eh__TransparentIdentifier3._003C_003Eh__TransparentIdentifier2.p.AvailableEndDateTimeUtc.HasValue || _003C_003Eh__TransparentIdentifier3._003C_003Eh__TransparentIdentifier2.p.AvailableEndDateTimeUtc >= nowUtc) && _003C_003Eh__TransparentIdentifier3._003C_003Eh__TransparentIdentifier2._003C_003Eh__TransparentIdentifier1._003C_003Eh__TransparentIdentifier0.psa.AllowFiltering
				select _003C_003Eh__TransparentIdentifier3._003C_003Eh__TransparentIdentifier2._003C_003Eh__TransparentIdentifier1._003C_003Eh__TransparentIdentifier0.saof).Distinct());
		})).ToList();
	}

	private async Task<IList<SpecificationAttributeOption>> GetSpecificationAttributeOptionsByVendorIdInternalAsync(int vendorId, bool showHiddenProducts)
	{
		if (vendorId == 0)
		{
			return null;
		}
		DateTime nowUtc = DateTime.UtcNow;
		IStaticCacheManager cacheManager = CacheManager;
		CacheKey sPECIFICATIONATTRIBUTEOPTIONS_BY_VENDORID_KEY = SPECIFICATIONATTRIBUTEOPTIONS_BY_VENDORID_KEY;
		object obj = vendorId;
		object obj2 = showHiddenProducts;
		Store val = await _storeContext.GetCurrentStoreAsync();
		CacheKey val2 = ((ICacheKeyService)cacheManager).PrepareKeyForDefaultCache(sPECIFICATIONATTRIBUTEOPTIONS_BY_VENDORID_KEY, new object[3]
		{
			obj,
			obj2,
			((BaseEntity)val).Id
		});
		return (await CacheManager.GetAsync<IEnumerable<SpecificationAttributeOption>>(val2, (Func<Task<IEnumerable<SpecificationAttributeOption>>>)async delegate
		{
			IQueryable<Product> productsQuery = await _customAclHelper.GetAvailableProductsForCurrentErpCustomerAsync();
			productsQuery = await _storeHelper.GetProductsForCurrentStoreAsync(productsQuery);
			return await AsyncIQueryableExtensions.ToListAsync<SpecificationAttributeOption>((from _003C_003Eh__TransparentIdentifier1 in SpecificationAttributeOptionRepository.Table.Join((IEnumerable<ProductSpecificationAttribute>)ProductSpecificationAttributeRepository.Table, (Expression<Func<SpecificationAttributeOption, int>>)((SpecificationAttributeOption sao) => ((BaseEntity)sao).Id), (Expression<Func<ProductSpecificationAttribute, int>>)((ProductSpecificationAttribute psa) => psa.SpecificationAttributeOptionId), (SpecificationAttributeOption sao, ProductSpecificationAttribute psa) => new { sao, psa }).Join((IEnumerable<Product>)productsQuery, _003C_003Eh__TransparentIdentifier0 => _003C_003Eh__TransparentIdentifier0.psa.ProductId, (Expression<Func<Product, int>>)((Product p) => ((BaseEntity)p).Id), (_003C_003Eh__TransparentIdentifier0,  p) => new { _003C_003Eh__TransparentIdentifier0, p })
				where _003C_003Eh__TransparentIdentifier1.p.VendorId == vendorId && (_003C_003Eh__TransparentIdentifier1.p.ParentGroupedProductId == 0 || _003C_003Eh__TransparentIdentifier1.p.VisibleIndividually) && (showHiddenProducts || _003C_003Eh__TransparentIdentifier1.p.Published) && !_003C_003Eh__TransparentIdentifier1.p.Deleted && (!_003C_003Eh__TransparentIdentifier1.p.AvailableStartDateTimeUtc.HasValue || _003C_003Eh__TransparentIdentifier1.p.AvailableStartDateTimeUtc <= nowUtc) && (!_003C_003Eh__TransparentIdentifier1.p.AvailableEndDateTimeUtc.HasValue || _003C_003Eh__TransparentIdentifier1.p.AvailableEndDateTimeUtc >= nowUtc) && _003C_003Eh__TransparentIdentifier1._003C_003Eh__TransparentIdentifier0.psa.AllowFiltering
				select _003C_003Eh__TransparentIdentifier1._003C_003Eh__TransparentIdentifier0.sao).Distinct());
		})).ToList();
	}

	private async Task<IList<SpecificationAttributeOption>> GetSpecificationAttributeOptionsByIdsInternalAsync(IList<int> specificationAttributeOptionIds)
	{
		if (specificationAttributeOptionIds == null || specificationAttributeOptionIds.Count == 0)
		{
			return null;
		}
		IStaticCacheManager cacheManager = CacheManager;
		CacheKey sPECIFICATIONATTRIBUTEOPTIONS_BY_IDS_KEY = SPECIFICATIONATTRIBUTEOPTIONS_BY_IDS_KEY;
		object obj = specificationAttributeOptionIds;
		Store val = await _storeContext.GetCurrentStoreAsync();
		CacheKey val2 = ((ICacheKeyService)cacheManager).PrepareKeyForDefaultCache(sPECIFICATIONATTRIBUTEOPTIONS_BY_IDS_KEY, new object[2]
		{
			obj,
			((BaseEntity)val).Id
		});
		return new List<SpecificationAttributeOption>(await CacheManager.GetAsync<IEnumerable<SpecificationAttributeOption>>(val2, (Func<Task<IEnumerable<SpecificationAttributeOption>>>)async delegate
		{
			List<int> specificationAttributeOptionIdsArray = specificationAttributeOptionIds.ToList();
			return await AsyncIQueryableExtensions.ToListAsync<SpecificationAttributeOption>((from _003C_003Eh__TransparentIdentifier0 in SpecificationAttributeOptionRepository.Table.Join((IEnumerable<ProductSpecificationAttribute>)ProductSpecificationAttributeRepository.Table, (Expression<Func<SpecificationAttributeOption, int>>)((SpecificationAttributeOption sao) => ((BaseEntity)sao).Id), (Expression<Func<ProductSpecificationAttribute, int>>)((ProductSpecificationAttribute psa) => psa.SpecificationAttributeOptionId), (SpecificationAttributeOption sao, ProductSpecificationAttribute psa) => new { sao, psa })
				where _003C_003Eh__TransparentIdentifier0.psa.AllowFiltering && specificationAttributeOptionIdsArray.Contains(((BaseEntity)_003C_003Eh__TransparentIdentifier0.sao).Id)
				select _003C_003Eh__TransparentIdentifier0.sao).Distinct());
		}));
	}

	private async Task<IList<SpecificationAttributeOption>> GetAllSpecificationAttributeOptionsInternalAsync(bool includeNotAllowFilteringOptions)
	{
		IStaticCacheManager cacheManager = CacheManager;
		CacheKey sPECIFICATIONATTRIBUTEOPTIONS_ALL_KEY = SPECIFICATIONATTRIBUTEOPTIONS_ALL_KEY;
		Store val = await _storeContext.GetCurrentStoreAsync();
		CacheKey val2 = ((ICacheKeyService)cacheManager).PrepareKeyForDefaultCache(sPECIFICATIONATTRIBUTEOPTIONS_ALL_KEY, new object[1] { ((BaseEntity)val).Id });
		return new List<SpecificationAttributeOption>(await CacheManager.GetAsync<IEnumerable<SpecificationAttributeOption>>(val2, (Func<Task<IEnumerable<SpecificationAttributeOption>>>)(async () => await AsyncIQueryableExtensions.ToListAsync<SpecificationAttributeOption>((from _003C_003Eh__TransparentIdentifier0 in SpecificationAttributeOptionRepository.Table.Join((IEnumerable<ProductSpecificationAttribute>)ProductSpecificationAttributeRepository.Table, (Expression<Func<SpecificationAttributeOption, int>>)((SpecificationAttributeOption sao) => ((BaseEntity)sao).Id), (Expression<Func<ProductSpecificationAttribute, int>>)((ProductSpecificationAttribute psa) => psa.SpecificationAttributeOptionId), (SpecificationAttributeOption sao, ProductSpecificationAttribute psa) => new { sao, psa })
			where includeNotAllowFilteringOptions || _003C_003Eh__TransparentIdentifier0.psa.AllowFiltering
			select _003C_003Eh__TransparentIdentifier0.sao).Distinct()))));
	}

	private async Task<IList<SpecificationAttributeOption>> GetSpecificationAttributeOptionsByProductIdInternalAsync(int productId, bool includeNotAllowFilteringOptions)
	{
		if (productId == 0)
		{
			return null;
		}
		IStaticCacheManager cacheManager = CacheManager;
		CacheKey sPECIFICATIONATTRIBUTEOPTIONS_BY_PRODUCTID_KEY = SPECIFICATIONATTRIBUTEOPTIONS_BY_PRODUCTID_KEY;
		object obj = productId;
		Store val = await _storeContext.GetCurrentStoreAsync();
		CacheKey val2 = ((ICacheKeyService)cacheManager).PrepareKeyForDefaultCache(sPECIFICATIONATTRIBUTEOPTIONS_BY_PRODUCTID_KEY, new object[2]
		{
			obj,
			((BaseEntity)val).Id
		});
		return new List<SpecificationAttributeOption>(await CacheManager.GetAsync<IEnumerable<SpecificationAttributeOption>>(val2, (Func<Task<IEnumerable<SpecificationAttributeOption>>>)async delegate
		{
			IQueryable<Product> inner = await _customAclHelper.GetAvailableProductsForCurrentErpCustomerAsync();
			return await AsyncIQueryableExtensions.ToListAsync<SpecificationAttributeOption>(from _003C_003Eh__TransparentIdentifier1 in SpecificationAttributeOptionRepository.Table.Join((IEnumerable<ProductSpecificationAttribute>)ProductSpecificationAttributeRepository.Table, (Expression<Func<SpecificationAttributeOption, int>>)((SpecificationAttributeOption sao) => ((BaseEntity)sao).Id), (Expression<Func<ProductSpecificationAttribute, int>>)((ProductSpecificationAttribute psa) => psa.SpecificationAttributeOptionId), (SpecificationAttributeOption sao, ProductSpecificationAttribute psa) => new { sao, psa }).Join((IEnumerable<Product>)inner, _003C_003Eh__TransparentIdentifier0 => _003C_003Eh__TransparentIdentifier0.psa.ProductId, (Expression<Func<Product, int>>)((Product p) => ((BaseEntity)p).Id), (_003C_003Eh__TransparentIdentifier0,  p) => new { _003C_003Eh__TransparentIdentifier0, p })
				where (includeNotAllowFilteringOptions || _003C_003Eh__TransparentIdentifier1._003C_003Eh__TransparentIdentifier0.psa.AllowFiltering) && ((BaseEntity)_003C_003Eh__TransparentIdentifier1.p).Id == productId
				select _003C_003Eh__TransparentIdentifier1._003C_003Eh__TransparentIdentifier0.sao);
		}));
	}

	private async Task<IEnumerable<SpecificationAttributeOption>> GetSpecificationAttributeOptionsByProductIdsInternalAsync(IList<int> productIds, bool showHiddenProducts)
	{
		if (productIds == null || productIds.Count == 0)
		{
			return null;
		}
		IStaticCacheManager cacheManager = CacheManager;
		CacheKey sPECIFICATIONATTRIBUTEOPTIONS_BY_PRODUCTIDS_KEY = SPECIFICATIONATTRIBUTEOPTIONS_BY_PRODUCTIDS_KEY;
		object obj = productIds;
		Store val = await _storeContext.GetCurrentStoreAsync();
		CacheKey val2 = ((ICacheKeyService)cacheManager).PrepareKeyForDefaultCache(sPECIFICATIONATTRIBUTEOPTIONS_BY_PRODUCTIDS_KEY, new object[2] { obj, val });
		return await CacheManager.GetAsync<IEnumerable<SpecificationAttributeOption>>(val2, (Func<Task<IEnumerable<SpecificationAttributeOption>>>)async delegate
		{
			DateTime nowUtc = DateTime.UtcNow;
			List<int> productIdsArray = productIds.ToList();
			IQueryable<Product> inner = await _customAclHelper.GetAvailableProductsForCurrentErpCustomerAsync();
			return await AsyncIQueryableExtensions.ToListAsync<SpecificationAttributeOption>((IQueryable<SpecificationAttributeOption>)(from _003C_003Eh__TransparentIdentifier1 in SpecificationAttributeOptionRepository.Table.Join((IEnumerable<ProductSpecificationAttribute>)ProductSpecificationAttributeRepository.Table, (Expression<Func<SpecificationAttributeOption, int>>)((SpecificationAttributeOption saof) => ((BaseEntity)saof).Id), (Expression<Func<ProductSpecificationAttribute, int>>)((ProductSpecificationAttribute psa) => psa.SpecificationAttributeOptionId), (SpecificationAttributeOption saof, ProductSpecificationAttribute psa) => new { saof, psa }).Join((IEnumerable<Product>)inner, _003C_003Eh__TransparentIdentifier0 => _003C_003Eh__TransparentIdentifier0.psa.ProductId, (Expression<Func<Product, int>>)((Product p) => ((BaseEntity)p).Id), (_003C_003Eh__TransparentIdentifier0,  p) => new { _003C_003Eh__TransparentIdentifier0, p })
				where productIdsArray.Contains(((BaseEntity)_003C_003Eh__TransparentIdentifier1.p).Id) && (showHiddenProducts || _003C_003Eh__TransparentIdentifier1.p.Published) && !_003C_003Eh__TransparentIdentifier1.p.Deleted && (!_003C_003Eh__TransparentIdentifier1.p.AvailableStartDateTimeUtc.HasValue || _003C_003Eh__TransparentIdentifier1.p.AvailableStartDateTimeUtc <= nowUtc) && (!_003C_003Eh__TransparentIdentifier1.p.AvailableEndDateTimeUtc.HasValue || _003C_003Eh__TransparentIdentifier1.p.AvailableEndDateTimeUtc >= nowUtc) && _003C_003Eh__TransparentIdentifier1._003C_003Eh__TransparentIdentifier0.psa.AllowFiltering
				select _003C_003Eh__TransparentIdentifier1._003C_003Eh__TransparentIdentifier0.saof).Distinct().OrderBy((Expression<Func<SpecificationAttributeOption, int>>)((SpecificationAttributeOption x) => x.DisplayOrder)));
		});
	}

	private async Task<IDictionary<int, IList<int>>> GetSpecificationAttributeOptionsDictionaryForProductsInternalAsync(IQueryable<Product> products)
	{
		if (products == null || !products.Any())
		{
			return null;
		}
		List<int> productIds = products.Select((Expression<Func<Product, int>>)((Product x) => ((BaseEntity)x).Id)).ToList();
		IQueryable<Product> inner = await _customAclHelper.GetAvailableProductsForCurrentErpCustomerAsync();
		var obj = await AsyncIQueryableExtensions.ToListAsync(from _003C_003Eh__TransparentIdentifier1 in SpecificationAttributeOptionRepository.Table.Join((IEnumerable<ProductSpecificationAttribute>)ProductSpecificationAttributeRepository.Table, (Expression<Func<SpecificationAttributeOption, int>>)((SpecificationAttributeOption saof) => ((BaseEntity)saof).Id), (Expression<Func<ProductSpecificationAttribute, int>>)((ProductSpecificationAttribute psa) => psa.SpecificationAttributeOptionId), (SpecificationAttributeOption saof, ProductSpecificationAttribute psa) => new { saof, psa }).Join((IEnumerable<Product>)inner, _003C_003Eh__TransparentIdentifier0 => _003C_003Eh__TransparentIdentifier0.psa.ProductId, (Expression<Func<Product, int>>)((Product p) => ((BaseEntity)p).Id), (_003C_003Eh__TransparentIdentifier0,  p) => new { _003C_003Eh__TransparentIdentifier0, p })
			where productIds.Contains(((BaseEntity)_003C_003Eh__TransparentIdentifier1.p).Id) && _003C_003Eh__TransparentIdentifier1._003C_003Eh__TransparentIdentifier0.psa.AllowFiltering
			select new
			{
				ProductId = ((BaseEntity)_003C_003Eh__TransparentIdentifier1.p).Id,
				SpecificationAttributeOptionId = ((BaseEntity)_003C_003Eh__TransparentIdentifier1._003C_003Eh__TransparentIdentifier0.saof).Id
			} into x
			orderby x.ProductId
			select x);
		Dictionary<int, IList<int>> dictionary = new Dictionary<int, IList<int>>();
		int num = 0;
		List<int> list = null;
		foreach (var item in obj)
		{
			if (item.ProductId != num)
			{
				list = new List<int>();
				dictionary.Add(item.ProductId, list);
				list.Add(item.SpecificationAttributeOptionId);
				num = item.ProductId;
			}
			else
			{
				list.Add(item.SpecificationAttributeOptionId);
			}
		}
		return dictionary;
	}

	private async Task<IDictionary<int, IList<int>>> GetSpecificationAttributeOptionsDictionaryInternalAsync()
	{
		IStaticCacheManager cacheManager = CacheManager;
		IStaticCacheManager cacheManager2 = CacheManager;
		CacheKey sPECIFICATIONATTRIBUTEOPTIONS_DICTIONARY_KEY = SPECIFICATIONATTRIBUTEOPTIONS_DICTIONARY_KEY;
		//Store val = await _storeContext.GetCurrentStoreAsync();
        Store store = await _storeContext.GetCurrentStoreAsync();

        //var obj = await cacheManager.GetAsync(((ICacheKeyService)cacheManager2).PrepareKeyForDefaultCache(sPECIFICATIONATTRIBUTEOPTIONS_DICTIONARY_KEY, new object[1] { val }), async () => (Task<List<_003C_003Ef__AnonymousType44<int, int>>>)(object)(await AsyncIQueryableExtensions.ToListAsync(from x in (from _003C_003Eh__TransparentIdentifier1 in SpecificationAttributeOptionRepository.Table.Join((IEnumerable<SpecificationAttribute>)SpecificationAttributeRepository.Table, (Expression<Func<SpecificationAttributeOption, int>>)((SpecificationAttributeOption saof) => saof.SpecificationAttributeId), (Expression<Func<SpecificationAttribute, int>>)((SpecificationAttribute sa) => ((BaseEntity)sa).Id), (SpecificationAttributeOption saof, SpecificationAttribute sa) => new { saof, sa }).Join((IEnumerable<ProductSpecificationAttribute>)ProductSpecificationAttributeRepository.Table, _003C_003Eh__TransparentIdentifier0 => ((BaseEntity)_003C_003Eh__TransparentIdentifier0.saof).Id, (Expression<Func<ProductSpecificationAttribute, int>>)((ProductSpecificationAttribute psa) => psa.SpecificationAttributeOptionId), (_003C_003Eh__TransparentIdentifier0,  psa) => new { _003C_003Eh__TransparentIdentifier0, psa })
        //		where _003C_003Eh__TransparentIdentifier1.psa.AllowFiltering
        //		select new
        //		{
        //			SpecificationAttributeId = ((BaseEntity)_003C_003Eh__TransparentIdentifier1._003C_003Eh__TransparentIdentifier0.sa).Id,
        //			SpecificationAttributeOptionId = ((BaseEntity)_003C_003Eh__TransparentIdentifier1._003C_003Eh__TransparentIdentifier0.saof).Id
        //		}).Distinct()
        //	orderby x.SpecificationAttributeId
        //	select x)));
        
        var obj = await cacheManager.GetAsync(cacheManager2.PrepareKeyForDefaultCache(sPECIFICATIONATTRIBUTEOPTIONS_DICTIONARY_KEY, store), async () => await (from x in (from saof in SpecificationAttributeOptionRepository.Table
                                                                                                                                                                          join sa in SpecificationAttributeRepository.Table on saof.SpecificationAttributeId equals sa.Id
                                                                                                                                                                          join psa in ProductSpecificationAttributeRepository.Table on saof.Id equals psa.SpecificationAttributeOptionId
                                                                                                                                                                          where psa.AllowFiltering
                                                                                                                                                                          select new
                                                                                                                                                                          {
                                                                                                                                                                              SpecificationAttributeId = sa.Id,
                                                                                                                                                                              SpecificationAttributeOptionId = saof.Id
                                                                                                                                                                          }).Distinct()
                                                                                                                                                               orderby x.SpecificationAttributeId
                                                                                                                                                               select x).ToListAsync());
        Dictionary<int, IList<int>> dictionary = new Dictionary<int, IList<int>>();
		int num = 0;
		List<int> list = null;
		foreach (var item in obj)
		{
			if (item.SpecificationAttributeId != num)
			{
				if (list != null)
				{
					foreach (int item2 in list)
					{
						dictionary.Add(item2, list);
					}
				}
				num = item.SpecificationAttributeId;
				list = new List<int> { item.SpecificationAttributeOptionId };
			}
			else
			{
				list.Add(item.SpecificationAttributeOptionId);
			}
		}
		foreach (int item3 in list)
		{
			dictionary.Add(item3, list);
		}
		return dictionary;
	}

	private async Task<bool> GetIncludeFeaturedProductsInNormalListAsync()
	{
		return await SettingService.GetSettingByKeyAsync<bool>("catalogsettings.includefeaturedproductsinnormallists", false, 0, false);
	}
}
