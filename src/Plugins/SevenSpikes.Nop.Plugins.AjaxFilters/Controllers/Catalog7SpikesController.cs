using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Newtonsoft.Json;
using Nop.Core;
using Nop.Core.Caching;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Stores;
using Nop.Core.Domain.Tax;
using Nop.Core.Domain.Vendors;
using Nop.Core.Events;
using Nop.Services.Catalog;
using Nop.Services.Common;
using Nop.Services.Customers;
using Nop.Services.Directory;
using Nop.Services.Localization;
using Nop.Services.Security;
using Nop.Services.Stores;
using Nop.Services.Vendors;
using Nop.Web.Factories;
using Nop.Web.Framework.Events;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.UI.Paging;
using Nop.Web.Models.Catalog;
using SevenSpikes.Nop.Framework.Controllers;
using SevenSpikes.Nop.Plugins.AjaxFilters.Areas.Admin.Extensions;
using SevenSpikes.Nop.Plugins.AjaxFilters.Domain;
using SevenSpikes.Nop.Plugins.AjaxFilters.Domain.Enums;
using SevenSpikes.Nop.Plugins.AjaxFilters.Extensions;
using SevenSpikes.Nop.Plugins.AjaxFilters.Helpers;
using SevenSpikes.Nop.Plugins.AjaxFilters.Infrastructure.Cache;
using SevenSpikes.Nop.Plugins.AjaxFilters.Models;
using SevenSpikes.Nop.Plugins.AjaxFilters.Models.AttributeFilter;
using SevenSpikes.Nop.Plugins.AjaxFilters.Models.InStockFilter;
using SevenSpikes.Nop.Plugins.AjaxFilters.Models.ManufacturerFilter;
using SevenSpikes.Nop.Plugins.AjaxFilters.Models.OnSaleFilter;
using SevenSpikes.Nop.Plugins.AjaxFilters.Models.PriceRangeFilterSlider;
using SevenSpikes.Nop.Plugins.AjaxFilters.Models.SpecificationFilter;
using SevenSpikes.Nop.Plugins.AjaxFilters.Models.VendorFilter;
using SevenSpikes.Nop.Plugins.AjaxFilters.QueryStringManipulation;
using SevenSpikes.Nop.Plugins.AjaxFilters.Services;
using SevenSpikes.Nop.Services.Catalog;
using SevenSpikes.Nop.Services.Catalog.DTO;

namespace SevenSpikes.Nop.Plugins.AjaxFilters.Controllers;

public class Catalog7SpikesController : Base7SpikesPublicController
{
	private readonly VendorSettings _vendorSettings;

	private readonly IVendorService _vendorService;

	private readonly ICategoryService7Spikes _categoryService7Spikes;

	private readonly ICategoryService _categoryService;

	private readonly ISearchTermService _searchTermService;

	private readonly IManufacturerService _manufacturerService;

	private readonly IPriceCalculationServiceNopAjaxFilters _priceCalculationServiceNopAjaxFilters;

	private readonly NopAjaxFiltersSettings _nopAjaxFiltersSettings;

	private readonly CatalogSettings _catalogSettings;

	private readonly IStaticCacheManager _cacheManager;

	private readonly IWorkContext _workContext;

	private readonly IStoreContext _storeContext;

	private readonly ICurrencyService _currencyService;

	private readonly ILocalizationService _localizationService;

	private readonly IWebHelper _webHelper;

	private readonly IGenericAttributeService _genericAttributeService;

	private readonly IAclService _aclService;

	private readonly IStoreMappingService _storeMappingService;

	private readonly IEventPublisher _eventPublisher;

	private readonly IQueryStringToModelUpdater _queryStringToModelUpdater;

	private readonly IQueryStringBuilder _queryStringBuilder;

	private readonly IFiltersPageHelper _filtersPageHelper;

	private readonly ICatalogModelFactory _catalogModelFactory;

	private readonly IProductModelFactory _productModelFactory;

	private readonly ICustomerService _customerService;

	private int _categoryId;

	private int _manufacturerId;

	private int _vendorId;

	private bool _isOnSearchPage;

	private string _keyword;

	private bool _searchInProductDescriptions;

	private bool _includeProductsFromSubcategories;

	private bool _advancedSearch;

	private OnSaleFilterModel7Spikes _onSaleFilterModel;

	private PriceRangeFilterModel7Spikes _priceRangeFilterModel7Spikes;

	private SpecificationFilterModel7Spikes _specificationFilterModel7Spikes;

	private AttributeFilterModel7Spikes _attributeFilterModel7Spikes;

	private ManufacturerFilterModel7Spikes _manufacturerFilterModel7Spikes;

	private VendorFilterModel7Spikes _vendorFilterModel7Spikes;

	private InStockFilterModel7Spikes _inStockFilterModel7Spikes;

	private CatalogProductsCommand _pagingFilteringModel;

	private decimal? _minPriceConverted;

	private decimal? _maxPriceConverted;

	private SpecificationFilterModelDTO _specificationFilterModelDto;

	private AttributeFilterModelDTO _attributeFilterModelDto;

	private ManufacturerFilterModelDTO _manufacturerFilterModelDto;

	private VendorFilterModelDTO _vendorFilterModelDto;

	private readonly IStaticCacheManager _staticCacheManager;

	private IHttpContextAccessor _httpContextAccessor;

	public IProductServiceNopAjaxFilters ProductServiceNopAjaxFilters { get; set; }

	public Catalog7SpikesController(ICategoryService categoryService, IVendorService vendorService, ISearchTermService searchTermService, IManufacturerService manufacturerService, IWorkContext workContext, IStoreContext storeContext, ICurrencyService currencyService, ILocalizationService localizationService, IWebHelper webHelper, IGenericAttributeService genericAttributeService, IAclService aclService, IStoreMappingService storeMappingService, IEventPublisher eventPublisher, CatalogSettings catalogSettings, VendorSettings vendorSettings, IStaticCacheManager cacheManager, IProductServiceNopAjaxFilters productServiceNopAjaxFilters, IPriceCalculationServiceNopAjaxFilters priceCalculationServiceNopAjaxFilters, IFiltersPageHelper filtersPageHelper, NopAjaxFiltersSettings nopAjaxFiltersSettings, IQueryStringBuilder queryStringBuilder, IQueryStringToModelUpdater queryStringToModelUpdater, ICategoryService7Spikes categoryService7Spikes, ICatalogModelFactory catalogModelFactory, IProductModelFactory productModelFactory, IStaticCacheManager staticCacheManager, ICustomerService customerService, IHttpContextAccessor httpContextAccessor)
	{
		ProductServiceNopAjaxFilters = productServiceNopAjaxFilters;
		_categoryService = categoryService;
		_searchTermService = searchTermService;
		_manufacturerService = manufacturerService;
		_priceCalculationServiceNopAjaxFilters = priceCalculationServiceNopAjaxFilters;
		_filtersPageHelper = filtersPageHelper;
		_nopAjaxFiltersSettings = nopAjaxFiltersSettings;
		_catalogSettings = catalogSettings;
		_cacheManager = cacheManager;
		_workContext = workContext;
		_storeContext = storeContext;
		_currencyService = currencyService;
		_localizationService = localizationService;
		_webHelper = webHelper;
		_genericAttributeService = genericAttributeService;
		_aclService = aclService;
		_storeMappingService = storeMappingService;
		_eventPublisher = eventPublisher;
		_queryStringBuilder = queryStringBuilder;
		_queryStringToModelUpdater = queryStringToModelUpdater;
		_categoryService7Spikes = categoryService7Spikes;
		_catalogModelFactory = catalogModelFactory;
		_productModelFactory = productModelFactory;
		_staticCacheManager = staticCacheManager;
		_vendorService = vendorService;
		_vendorSettings = vendorSettings;
		_customerService = customerService;
		_httpContextAccessor = httpContextAccessor;
	}

	[HttpGet]
	public ActionResult GetFilteredProducts()
	{
		return new EmptyResult();
	}

	[HttpPost]
	public async Task<ActionResult> GetFilteredProducts([FromBody] GetFilteredProductsModel model)
	{
		if (model == null)
		{
			return new EmptyResult();
		}
		CatalogProductsCommand pagingFilteringModel = CreateAndPopulatePagingModel(model);
		(ProductsModel, string) tuple = await GetFilteredProductsInternalAsync(model.CategoryId, model.ManufacturerId, model.VendorId, model.PriceRangeFilterModel7Spikes, model.SpecificationFiltersModel7Spikes, model.AttributeFiltersModel7Spikes, model.ManufacturerFiltersModel7Spikes, model.VendorFiltersModel7Spikes, pagingFilteringModel, model.OnSaleFilterModel, model.InStockFilterModel, model.QueryString, model.ShouldNotStartFromFirstPage, model.Keyword, model.SearchCategoryId, model.SearchManufacturerId, model.SearchVendorId, model.PriceFrom, model.PriceTo, model.IncludeSubcategories, model.SearchInProductDescriptions, model.AdvancedSearch, model.IsOnSearchPage);
		if (model.CategoryId > 0 && !((ControllerBase)(object)this).RouteData.Values.ContainsKey("categoryid"))
		{
			((ControllerBase)(object)this).RouteData.Values.Add("categoryid", model.CategoryId.ToString());
		}
		if (model.ManufacturerId > 0 && !((ControllerBase)(object)this).RouteData.Values.ContainsKey("manufacturerid"))
		{
			((ControllerBase)(object)this).RouteData.Values.Add("manufacturerid", model.ManufacturerId.ToString());
		}
		if (model.VendorId > 0 && !((ControllerBase)(object)this).RouteData.Values.ContainsKey("vendorid"))
		{
			((ControllerBase)(object)this).RouteData.Values.Add("vendorid", model.VendorId.ToString());
		}
		return ((Controller)(object)this).PartialView(tuple.Item2, (object?)tuple.Item1);
	}

	private CatalogProductsCommand CreateAndPopulatePagingModel(GetFilteredProductsModel model)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Expected O, but got Unknown
		return new CatalogProductsCommand
		{
			OrderBy = model.Orderby,
			PageNumber = model.PageNumber.GetValueOrDefault(),
			PageSize = model.Pagesize,
			ViewMode = model.Viewmode
		};
	}

	public async Task<ActionResult> AjaxFiltersSearch(SearchModel model, CatalogProductsCommand command)
	{
		if (!_nopAjaxFiltersSettings.ShowFiltersOnSearchPage)
		{
			return new RedirectResult(((ControllerBase)(object)this).Url.RouteUrl("ProductSearch") + ((ControllerBase)(object)this).Request.QueryString);
		}
		if (model == (SearchModel)null)
		{
			throw new ArgumentNullException("model");
		}
		string searchTerms = model.q ?? string.Empty;
		searchTerms = searchTerms.Trim();
		await _catalogModelFactory.PrepareSortingOptionsAsync(model.CatalogProductsModel, command);
		await _catalogModelFactory.PrepareViewModesAsync(model.CatalogProductsModel, command);
		await _catalogModelFactory.PreparePageSizeOptionsAsync(model.CatalogProductsModel, command, _catalogSettings.SearchPageAllowCustomersToSelectPageSize, _catalogSettings.SearchPagePageSizeOptions, _catalogSettings.SearchPageProductsPerPage);
		var categoriesModels = new List<SearchModel.CategoryModel>();
		ICategoryService categoryService = _categoryService;
		IList<Category> allCategories = await categoryService.GetAllCategoriesAsync(((BaseEntity)(await _storeContext.GetCurrentStoreAsync())).Id, false);
		foreach (Category c in allCategories)
		{
			string categoryBreadcrumb2 = string.Empty;
			IList<Category> breadcrumb = await _categoryService.GetCategoryBreadCrumbAsync(c, allCategories, false);
			for (int j = 0; j <= breadcrumb.Count - 1; j++)
			{
				string text = categoryBreadcrumb2;
				categoryBreadcrumb2 = text + await _localizationService.GetLocalizedAsync<Category, string>(breadcrumb[j], (Expression<Func<Category, string>>)((Category x) => x.Name), (int?)null, true, true);
				if (j != breadcrumb.Count - 1)
				{
					categoryBreadcrumb2 += " >> ";
				}
			}
			categoriesModels.Add(new SearchModel.CategoryModel
			{
				Id = ((BaseEntity)c).Id,
				Breadcrumb = categoryBreadcrumb2
			});
		}
		if (categoriesModels.Any())
		{
			IList<SelectListItem> availableCategories = model.AvailableCategories;
			SelectListItem selectListItem = new SelectListItem
			{
				Value = "0"
			};
			SelectListItem selectListItem2 = selectListItem;
			selectListItem2.Text = await _localizationService.GetResourceAsync("Common.All");
			availableCategories.Add(selectListItem);
			foreach (var item2 in categoriesModels)
			{
				model.AvailableCategories.Add(new SelectListItem
				{
					Value = ((BaseNopEntityModel)item2).Id.ToString(),
					Text = item2.Breadcrumb,
					Selected = (model.cid == ((BaseNopEntityModel)item2).Id)
				});
			}
		}
		IManufacturerService manufacturerService = _manufacturerService;
		IPagedList<Manufacturer> manufacturers = await manufacturerService.GetAllManufacturersAsync("", ((BaseEntity)(await _storeContext.GetCurrentStoreAsync())).Id, 0, int.MaxValue, false, (bool?)null);
		if (((IEnumerable<Manufacturer>)manufacturers).Any())
		{
			IList<SelectListItem> availableCategories = model.AvailableManufacturers;
			SelectListItem selectListItem2 = new SelectListItem
			{
				Value = "0"
			};
			SelectListItem selectListItem = selectListItem2;
			selectListItem.Text = await _localizationService.GetResourceAsync("Common.All");
			availableCategories.Add(selectListItem2);
			foreach (Manufacturer k in (IEnumerable<Manufacturer>)manufacturers)
			{
				availableCategories = model.AvailableManufacturers;
				selectListItem = new SelectListItem
				{
					Value = ((BaseEntity)k).Id.ToString()
				};
				selectListItem2 = selectListItem;
				selectListItem2.Text = await _localizationService.GetLocalizedAsync<Manufacturer, string>(k, (Expression<Func<Manufacturer, string>>)((Manufacturer x) => x.Name), (int?)null, true, true);
				selectListItem.Selected = model.mid == ((BaseEntity)k).Id;
				availableCategories.Add(selectListItem);
			}
		}
		model.asv = _vendorSettings.AllowSearchByVendor;
		if (model.asv)
		{
			IPagedList<Vendor> vendors = await _vendorService.GetAllVendorsAsync("", "", 0, int.MaxValue, false);
			if (((IEnumerable<Vendor>)vendors).Any())
			{
				IList<SelectListItem> availableCategories = model.AvailableVendors;
				SelectListItem selectListItem2 = new SelectListItem
				{
					Value = "0"
				};
				SelectListItem selectListItem = selectListItem2;
				selectListItem.Text = await _localizationService.GetResourceAsync("Common.All");
				availableCategories.Add(selectListItem2);
				foreach (Vendor vendor in (IEnumerable<Vendor>)vendors)
				{
					availableCategories = model.AvailableVendors;
					selectListItem = new SelectListItem
					{
						Value = ((BaseEntity)vendor).Id.ToString()
					};
					selectListItem2 = selectListItem;
					selectListItem2.Text = await _localizationService.GetLocalizedAsync<Vendor, string>(vendor, (Expression<Func<Vendor, string>>)((Vendor x) => x.Name), (int?)null, true, true);
					selectListItem.Selected = model.vid == ((BaseEntity)vendor).Id;
					availableCategories.Add(selectListItem);
				}
			}
		}
		IPagedList<ProductOverviewModel> productOverviews = (IPagedList<ProductOverviewModel>)(object)new PagedList<ProductOverviewModel>((IList<ProductOverviewModel>)new List<ProductOverviewModel>(), 0, 1, (int?)null);
		if (_httpContextAccessor.HttpContext.Request.Query.ContainsKey("q"))
		{
			if (searchTerms.Length < _catalogSettings.ProductSearchTermMinimumLength)
			{
				CatalogProductsModel catalogProductsModel = model.CatalogProductsModel;
				catalogProductsModel.WarningMessage = string.Format(await _localizationService.GetResourceAsync("Search.SearchTermMinimumLengthIsNCharacters"), _catalogSettings.ProductSearchTermMinimumLength);
			}
			else
			{
				List<int> categoryIds = new List<int>();
				int j = 0;
				decimal? priceFrom = null;
				decimal? priceTo = null;
				bool searchInDescriptions = false;
				int vendorId = 0;
				if (model.advs)
				{
					int cid = model.cid;
					if (cid > 0)
					{
						categoryIds.Add(cid);
						if (model.isc)
						{
							List<int> list = categoryIds;
							categoryService = _categoryService;
							int num = cid;
							list.AddRange(await categoryService.GetChildCategoryIdsAsync(num, ((BaseEntity)(await _storeContext.GetCurrentStoreAsync())).Id, false));
						}
					}
					j = model.mid;
					priceFrom = model.CatalogProductsModel.PriceRangeFilter.SelectedPriceRange.From;
					priceTo = model.CatalogProductsModel.PriceRangeFilter.SelectedPriceRange.To;
					if (model.asv)
					{
						vendorId = model.vid;
					}
					searchInDescriptions = model.sid;
				}
				ProductsModel item = (await GetFilteredProductsInternalAsync(0, 0, 0, new PriceRangeFilterModel7Spikes(), new SpecificationFilterModel7Spikes(), new AttributeFilterModel7Spikes(), new ManufacturerFilterModel7Spikes(), new VendorFilterModel7Spikes(), command, new OnSaleFilterModel7Spikes(), new InStockFilterModel7Spikes(), string.Empty, shouldNotStartFromFirstPage: false, model.q, model.cid, model.mid, model.vid, priceFrom, priceTo, model.isc, model.sid, model.advs, isOnSearchPage: true)).Item1;
				int num2 = ((BasePageableModel)command).PageSize;
				if (num2 <= 0)
				{
					num2 = 1;
				}
				productOverviews = (IPagedList<ProductOverviewModel>)(object)new PagedList<ProductOverviewModel>((IList<ProductOverviewModel>)item.Products.ToList(), ((BasePageableModel)command).PageIndex, num2, (int?)item.TotalCount);
				model.CatalogProductsModel.Products = item.Products.ToList();
				CatalogProductsModel catalogProductsModel = model.CatalogProductsModel;
				catalogProductsModel.NoResultMessage = await _localizationService.GetResourceAsync("Catalog.Products.NoResult");
				if (!string.IsNullOrEmpty(searchTerms))
				{
					ISearchTermService searchTermService = _searchTermService;
					string categoryBreadcrumb2 = searchTerms;
					SearchTerm val = await searchTermService.GetSearchTermByKeywordAsync(categoryBreadcrumb2, ((BaseEntity)(await _storeContext.GetCurrentStoreAsync())).Id);
					if (val != null)
					{
						SearchTerm obj = val;
						int count = obj.Count;
						obj.Count = count + 1;
						await _searchTermService.UpdateSearchTermAsync(val);
					}
					else
					{
						SearchTerm val2 = new SearchTerm();
						val2.Keyword = searchTerms;
						SearchTerm val3 = val2;
						val3.StoreId = ((BaseEntity)(await _storeContext.GetCurrentStoreAsync())).Id;
						val2.Count = 1;
						await _searchTermService.InsertSearchTermAsync(val2);
					}
				}
				IEventPublisher eventPublisher = _eventPublisher;
				ProductSearchEvent val4 = new ProductSearchEvent();
				val4.SearchTerm = searchTerms;
				val4.SearchInDescriptions = searchInDescriptions;
				val4.CategoryIds = categoryIds;
				val4.ManufacturerId = j;
				ProductSearchEvent val5 = val4;
				val5.WorkingLanguageId = ((BaseEntity)(await _workContext.GetWorkingLanguageAsync())).Id;
				val4.VendorId = vendorId;
				await eventPublisher.PublishAsync<ProductSearchEvent>(val4);
			}
		}
		((BasePageableModel)model.CatalogProductsModel).LoadPagedList<ProductOverviewModel>(productOverviews);
		return ((Controller)(object)this).View("Search", (object?)model);
	}

	private async Task<(ProductsModel productsModel, string templateViewPath)> GetFilteredProductsInternalAsync(int categoryId, int manufacturerId, int vendorId, PriceRangeFilterModel7Spikes priceRangeFilterModel7Spikes, SpecificationFilterModel7Spikes specificationFiltersModel7Spikes, AttributeFilterModel7Spikes attributeFiltersModel7Spikes, ManufacturerFilterModel7Spikes manufacturerFiltersModel7Spikes, VendorFilterModel7Spikes vendorFiltersModel7Spikes, CatalogProductsCommand pagingFilteringModel, OnSaleFilterModel7Spikes onSaleFilterModel, InStockFilterModel7Spikes inStockFilterModel, string queryString, bool shouldNotStartFromFirstPage, string keyword, int searchCategoryId, int searchManufacturerId, int searchVendorId, decimal? priceFrom, decimal? priceTo, bool includeSubcategories, bool searchInProductDescriptions, bool advancedSearch, bool isOnSearchPage = false)
	{
		_categoryId = categoryId;
		_manufacturerId = manufacturerId;
		_vendorId = vendorId;
		_keyword = keyword;
		_advancedSearch = advancedSearch;
		_includeProductsFromSubcategories = _catalogSettings.ShowProductsFromSubcategories;
		_isOnSearchPage = isOnSearchPage;
		if (_isOnSearchPage)
		{
			_includeProductsFromSubcategories = false;
			if (advancedSearch)
			{
				_categoryId = searchCategoryId;
				_manufacturerId = searchManufacturerId;
				_vendorId = searchVendorId;
				_includeProductsFromSubcategories = includeSubcategories;
				_searchInProductDescriptions = searchInProductDescriptions;
				if (priceFrom.HasValue)
				{
					ICurrencyService currencyService = _currencyService;
					decimal value = priceFrom.Value;
					_minPriceConverted = await currencyService.ConvertToPrimaryStoreCurrencyAsync(value, await _workContext.GetWorkingCurrencyAsync());
				}
				if (priceTo.HasValue)
				{
					ICurrencyService currencyService = _currencyService;
					decimal value = priceTo.Value;
					_maxPriceConverted = await currencyService.ConvertToPrimaryStoreCurrencyAsync(value, await _workContext.GetWorkingCurrencyAsync());
				}
			}
		}
		if (!shouldNotStartFromFirstPage)
		{
			((BasePageableModel)pagingFilteringModel).PageNumber = 1;
		}
		FiltersPageParameters filtersPageParameters = new FiltersPageParameters
		{
			CategoryId = categoryId,
			ManufacturerId = manufacturerId,
			VendorId = vendorId,
			SearchQueryStringParameters = new SearchQueryStringParameters
			{
				IsOnSearchPage = isOnSearchPage,
				Keyword = keyword,
				AdvancedSearch = advancedSearch,
				IncludeSubcategories = includeSubcategories,
				PriceFrom = priceFrom,
				PriceTo = priceTo,
				SearchCategoryId = searchCategoryId,
				SearchManufacturerId = searchManufacturerId,
				SearchInProductDescriptions = searchInProductDescriptions,
				SearchVendorId = searchVendorId
			},
			OrderBy = pagingFilteringModel.OrderBy,
			PageSize = ((BasePageableModel)pagingFilteringModel).PageSize,
			PageNumber = ((BasePageableModel)pagingFilteringModel).PageNumber,
			ViewMode = pagingFilteringModel.ViewMode
		};
		await _filtersPageHelper.InitializeAsync(filtersPageParameters);
		ICustomerService customerService = _customerService;
		string customerRolesIds = string.Join(",", await customerService.GetCustomerRoleIdsAsync(await _workContext.GetCurrentCustomerAsync(), false));
		_priceRangeFilterModel7Spikes = priceRangeFilterModel7Spikes;
		_specificationFilterModel7Spikes = specificationFiltersModel7Spikes;
		_attributeFilterModel7Spikes = attributeFiltersModel7Spikes;
		_manufacturerFilterModel7Spikes = manufacturerFiltersModel7Spikes;
		_vendorFilterModel7Spikes = vendorFiltersModel7Spikes;
		_pagingFilteringModel = pagingFilteringModel;
		_onSaleFilterModel = onSaleFilterModel;
		_inStockFilterModel7Spikes = inStockFilterModel;
		await _filtersPageHelper.AdjustPagingFilteringModelPageSizeAndPageNumberAsync(_pagingFilteringModel);
		_queryStringBuilder.SetDataForQueryString(_specificationFilterModel7Spikes, _attributeFilterModel7Spikes, _manufacturerFilterModel7Spikes, _vendorFilterModel7Spikes, _priceRangeFilterModel7Spikes, _pagingFilteringModel, _onSaleFilterModel, _inStockFilterModel7Spikes);
		queryString = queryString.TrimStart('#', '/');
		string value2 = WebUtility.UrlEncode("#");
		if (!queryString.Contains("!#-!") && queryString.Contains(value2))
		{
			queryString = WebUtility.UrlDecode(queryString);
		}
		ProductsModel productsModel;
		if (!string.IsNullOrEmpty(queryString))
		{
			IStaticCacheManager cacheManager = _cacheManager;
			CacheKey nOP_AJAX_FILTERS_FILTERED_PRODUCTS_KEY = NopAjaxFiltersModelCacheEventConsumer.NOP_AJAX_FILTERS_FILTERED_PRODUCTS_KEY;
			object obj = _categoryId;
			object obj2 = _manufacturerId;
			object obj3 = _vendorId;
			object obj4 = customerRolesIds;
			object obj5 = queryString;
			object obj6 = ((BaseEntity)(await _storeContext.GetCurrentStoreAsync())).Id;
			object obj7 = ((BaseEntity)(await _workContext.GetWorkingLanguageAsync())).Id;
			object obj8 = ((BaseEntity)(await _workContext.GetWorkingCurrencyAsync())).Id;
			TaxDisplayType val = await _workContext.GetTaxDisplayTypeAsync();
			CacheKey val2 = ((ICacheKeyService)cacheManager).PrepareKey(nOP_AJAX_FILTERS_FILTERED_PRODUCTS_KEY, new object[15]
			{
				obj, obj2, obj3, obj4, obj5, obj6, obj7, obj8, val, _keyword,
				_minPriceConverted, _maxPriceConverted, _includeProductsFromSubcategories, _searchInProductDescriptions, _advancedSearch
			});
			productsModel = await _staticCacheManager.GetAsync<ProductsModel>(val2, (Func<Task<ProductsModel>>)(async () => (ProductsModel)null));
		}
		else
		{
			string queryString2 = _queryStringBuilder.GetQueryString(shouldRebuildQueryString: true);
			IStaticCacheManager cacheManager = _cacheManager;
			CacheKey nOP_AJAX_FILTERS_FILTERED_PRODUCTS_KEY = NopAjaxFiltersModelCacheEventConsumer.NOP_AJAX_FILTERS_FILTERED_PRODUCTS_KEY;
			object obj8 = _categoryId;
			object obj7 = _manufacturerId;
			object obj6 = _vendorId;
			object obj5 = customerRolesIds;
			object obj4 = queryString2;
			object obj3 = ((BaseEntity)(await _storeContext.GetCurrentStoreAsync())).Id;
			object obj2 = ((BaseEntity)(await _workContext.GetWorkingLanguageAsync())).Id;
			object obj = ((BaseEntity)(await _workContext.GetWorkingCurrencyAsync())).Id;
			TaxDisplayType val = await _workContext.GetTaxDisplayTypeAsync();
			CacheKey val3 = ((ICacheKeyService)cacheManager).PrepareKey(nOP_AJAX_FILTERS_FILTERED_PRODUCTS_KEY, new object[15]
			{
				obj8, obj7, obj6, obj5, obj4, obj3, obj2, obj, val, _keyword,
				_minPriceConverted, _maxPriceConverted, _includeProductsFromSubcategories, _searchInProductDescriptions, _advancedSearch
			});
			productsModel = await _staticCacheManager.GetAsync<ProductsModel>(val3, (Func<Task<ProductsModel>>)(async () => (ProductsModel)null));
		}
		if (productsModel == null)
		{
            // storing category wise view mode, we'll need this to SetPagingFilteringModelToDefaultValues

            _pagingFilteringModel.CustomProperties.Add("CategoryViewMode", pagingFilteringModel.ViewMode);
			_queryStringToModelUpdater.UpdateModelsFromQueryString(queryString, _specificationFilterModel7Spikes, _attributeFilterModel7Spikes, _manufacturerFilterModel7Spikes, _vendorFilterModel7Spikes, _priceRangeFilterModel7Spikes, _pagingFilteringModel, _onSaleFilterModel, _inStockFilterModel7Spikes);
			await ValidatePageSizeAsync(_pagingFilteringModel, categoryId, manufacturerId, vendorId);
			ValidateOrderBy(_pagingFilteringModel);
			await AdjustMinAndMaxPriceRangeWithDiscountsAndTaxAsync();
			ProductsResultDataDTO productsResultData = await SearchProductsAsync();
			string viewMode = ((!string.IsNullOrWhiteSpace(_pagingFilteringModel.ViewMode)) ? _pagingFilteringModel.ViewMode : _catalogSettings.DefaultViewMode);
			ProductsModel productsModel2 = new ProductsModel
			{
				NopAjaxFiltersSettingsModel = _nopAjaxFiltersSettings.ToModel()
			};
			ProductsModel productsModel3 = productsModel2;
			IProductModelFactory productModelFactory = _productModelFactory;
			IPagedList<Product> productsPagedList = productsResultData.ProductsPagedList;
			bool prepareSpecificationAttributes = _nopAjaxFiltersSettings.PrepareSpecificationAttributes;
			productsModel3.Products = await productModelFactory.PrepareProductOverviewModelsAsync((IEnumerable<Product>)productsPagedList, true, true, (int?)null, prepareSpecificationAttributes, false);
			productsModel2.ViewMode = viewMode;
			productsModel2.HashQuery = _queryStringBuilder.GetQueryString(shouldRebuildQueryString: true);
			productsModel2.TotalCount = productsResultData.ProductsPagedList.TotalCount;
			productsModel = productsModel2;
			((BasePageableModel)productsModel.PagingFilteringContext).LoadPagedList<Product>(productsResultData.ProductsPagedList);
			AdjustModelsFilterItemsStateWithSelectedOptionIds(productsResultData);
			await SetSelectedOptionIdsCacheAsync(productsResultData);
			AddJavaScriptRequiredInfoToProductModel(productsModel);
			IStaticCacheManager cacheManager = _cacheManager;
			CacheKey nOP_AJAX_FILTERS_FILTERED_PRODUCTS_KEY = NopAjaxFiltersModelCacheEventConsumer.NOP_AJAX_FILTERS_FILTERED_PRODUCTS_KEY;
			object obj = _categoryId;
			object obj2 = _manufacturerId;
			object obj3 = _vendorId;
			object obj4 = customerRolesIds;
			object obj5 = productsModel.HashQuery;
			object obj6 = ((BaseEntity)(await _storeContext.GetCurrentStoreAsync())).Id;
			object obj7 = ((BaseEntity)(await _workContext.GetWorkingLanguageAsync())).Id;
			object obj8 = ((BaseEntity)(await _workContext.GetWorkingCurrencyAsync())).Id;
			TaxDisplayType val = await _workContext.GetTaxDisplayTypeAsync();
			CacheKey val4 = ((ICacheKeyService)cacheManager).PrepareKey(nOP_AJAX_FILTERS_FILTERED_PRODUCTS_KEY, new object[15]
			{
				obj, obj2, obj3, obj4, obj5, obj6, obj7, obj8, val, _keyword,
				_minPriceConverted, _maxPriceConverted, _includeProductsFromSubcategories, _searchInProductDescriptions, _advancedSearch
			});
			await _staticCacheManager.SetAsync<ProductsModel>(val4, productsModel);
		}
		return (productsModel: productsModel, templateViewPath: await _filtersPageHelper.GetTemplateViewPathAsync());
	}

	private async Task ValidatePageSizeAsync(CatalogProductsCommand pagingFilteringModel, int categoryId, int manufacturerId, int vendorId)
	{
		int num = int.MinValue;
		if (categoryId > 0)
		{
			Category val = await _categoryService.GetCategoryByIdAsync(categoryId);
			if (val.AllowCustomersToSelectPageSize)
			{
				string text = val.PageSizeOptions;
				if (string.IsNullOrEmpty(text))
				{
					text = _catalogSettings.DefaultCategoryPageSizeOptions;
				}
				num = GetMaxPageSizeOption(text);
			}
			else
			{
				num = val.PageSize;
			}
		}
		if (manufacturerId > 0)
		{
			Manufacturer val2 = await _manufacturerService.GetManufacturerByIdAsync(manufacturerId);
			if (val2.AllowCustomersToSelectPageSize)
			{
				string text2 = val2.PageSizeOptions;
				if (string.IsNullOrEmpty(text2))
				{
					text2 = _catalogSettings.DefaultManufacturerPageSizeOptions;
				}
				num = GetMaxPageSizeOption(text2);
			}
			else
			{
				num = val2.PageSize;
			}
		}
		if (vendorId > 0)
		{
			Vendor val3 = await _vendorService.GetVendorByIdAsync(vendorId);
			if (val3.AllowCustomersToSelectPageSize)
			{
				string text3 = val3.PageSizeOptions;
				if (string.IsNullOrEmpty(text3))
				{
					text3 = _vendorSettings.DefaultVendorPageSizeOptions;
				}
				num = GetMaxPageSizeOption(text3);
			}
			else
			{
				num = val3.PageSize;
			}
		}
		if (_isOnSearchPage)
		{
			num = ((!_catalogSettings.SearchPageAllowCustomersToSelectPageSize) ? _catalogSettings.SearchPageProductsPerPage : GetMaxPageSizeOption(_catalogSettings.SearchPagePageSizeOptions));
		}
		if (num != int.MinValue && ((BasePageableModel)pagingFilteringModel).PageSize > num)
		{
			((BasePageableModel)pagingFilteringModel).PageSize = num;
			((BasePageableModel)pagingFilteringModel).PageNumber = 1;
		}
	}

	public void ValidateOrderBy(CatalogProductsCommand pagingFilteringModel)
	{
		if (_pagingFilteringModel.OrderBy.HasValue && !_catalogSettings.ProductSortingEnumDisabled.Contains(_pagingFilteringModel.OrderBy.Value))
		{
			return;
		}
		int value = 0;
		if (_catalogSettings.ProductSortingEnumDisabled.Count != Enum.GetValues(typeof(ProductSortingEnum)).Length)
		{
			value = (from idOption in Enum.GetValues(typeof(ProductSortingEnum)).Cast<int>().Except(_catalogSettings.ProductSortingEnumDisabled)
				select new KeyValuePair<int, int>(idOption, _catalogSettings.ProductSortingEnumDisplayOrder.TryGetValue(idOption, out var value2) ? value2 : idOption) into x
				orderby x.Value
				select x).FirstOrDefault().Value;
		}
		_pagingFilteringModel.OrderBy = value;
	}

	private int GetMaxPageSizeOption(string pageSizeOptions)
	{
		string[] array = pageSizeOptions.Split(new char[2] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries);
		int num = 0;
		string[] array2 = array;
		for (int i = 0; i < array2.Length; i++)
		{
			if (int.TryParse(array2[i], out var result))
			{
				num = Math.Max(result, num);
			}
		}
		return num;
	}

	private async Task<ProductsResultDataDTO> SearchProductsAsync()
	{
		SetFilterDTOs();
		List<int> categoryIds = new List<int>();
		if (_categoryId > 0)
		{
			Category val = await _categoryService.GetCategoryByIdAsync(_categoryId);
			categoryIds.Add(((BaseEntity)val).Id);
			if (_includeProductsFromSubcategories)
			{
				categoryIds.AddRange(await _categoryService7Spikes.GetCategoryIdsByParentCategoryAsync(((BaseEntity)val).Id));
			}
		}
		bool onSale = false;
		if (_onSaleFilterModel != null)
		{
			onSale = _onSaleFilterModel.FilterItemState == FilterItemState.Checked;
		}
		bool inStock = false;
		if (_inStockFilterModel7Spikes != null)
		{
			inStock = _inStockFilterModel7Spikes.FilterItemState == FilterItemState.Checked;
		}
		IProductServiceNopAjaxFilters productServiceNopAjaxFilters = ProductServiceNopAjaxFilters;
		IList<int> categoryIds2 = categoryIds;
		int vendorId = _vendorId;
		int manufacturerId = _manufacturerId;
		string keyword = _keyword;
		SpecificationFilterModelDTO specificationFilterModelDto = _specificationFilterModelDto;
		AttributeFilterModelDTO attributeFilterModelDto = _attributeFilterModelDto;
		ManufacturerFilterModelDTO manufacturerFilterModelDto = _manufacturerFilterModelDto;
		VendorFilterModelDTO vendorFilterModelDto = _vendorFilterModelDto;
		int pageIndex = ((BasePageableModel)_pagingFilteringModel).PageNumber - 1;
		int pageSize = ((BasePageableModel)_pagingFilteringModel).PageSize;
		int manufacturerId2 = manufacturerId;
		int vendorId2 = vendorId;
		ProductSortingEnum orderBy = (ProductSortingEnum)_pagingFilteringModel.OrderBy.Value;
		int id = ((BaseEntity)(await _storeContext.GetCurrentStoreAsync())).Id;
		bool? featuredProducts = (_catalogSettings.IncludeFeaturedProductsInNormalLists ? null : new bool?(false));
		decimal? minPriceConverted = _minPriceConverted;
		decimal? maxPriceConverted = _maxPriceConverted;
		string keywords = keyword;
		bool onSale2 = onSale;
		bool inStock2 = inStock;
		int id2 = ((BaseEntity)(await _workContext.GetWorkingLanguageAsync())).Id;
		return await productServiceNopAjaxFilters.SearchProductsAsync(categoryIds2, specificationFilterModelDto, attributeFilterModelDto, manufacturerFilterModelDto, vendorFilterModelDto, pageIndex, pageSize, manufacturerId2, vendorId2, id, featuredProducts, minPriceConverted, maxPriceConverted, 0, keywords, _searchInProductDescriptions, searchSku: true, _nopAjaxFiltersSettings.SearchInProductTags || _searchInProductDescriptions, id2, orderBy, showHidden: false, onSale2, inStock2);
	}

	private void AddJavaScriptRequiredInfoToProductModel(ProductsModel productsModel)
	{
		SerializeAndPopulateFilterModelsToProductModel(productsModel);
		SetCurrentFiltersSelectionToProductModel(productsModel);
	}

	private void SetCurrentFiltersSelectionToProductModel(ProductsModel productsModel)
	{
		string priceRangeFromJson = string.Empty;
		string priceRangeToJson = string.Empty;
		if (_priceRangeFilterModel7Spikes != null && _priceRangeFilterModel7Spikes.SelectedPriceRange != (PriceRangeModel)null)
		{
			if (_priceRangeFilterModel7Spikes.SelectedPriceRange.From.HasValue)
			{
				priceRangeFromJson = JsonConvert.SerializeObject((object)Convert.ToInt32(_priceRangeFilterModel7Spikes.SelectedPriceRange.From));
			}
			if (_priceRangeFilterModel7Spikes.SelectedPriceRange.To.HasValue)
			{
				priceRangeToJson = JsonConvert.SerializeObject((object)Convert.ToInt32(_priceRangeFilterModel7Spikes.SelectedPriceRange.To));
			}
		}
		string currentPageSizeJson = JsonConvert.SerializeObject((object)((BasePageableModel)_pagingFilteringModel).PageSize);
		string currentViewModeJson = JsonConvert.SerializeObject((object)_pagingFilteringModel.ViewMode);
		string currentOrderByJson = JsonConvert.SerializeObject((object)_pagingFilteringModel.OrderBy);
		string currentPageNumberJson = JsonConvert.SerializeObject((object)((BasePageableModel)_pagingFilteringModel).PageNumber);
		productsModel.PriceRangeFromJson = priceRangeFromJson;
		productsModel.PriceRangeToJson = priceRangeToJson;
		productsModel.CurrentPageSizeJson = currentPageSizeJson;
		productsModel.CurrentViewModeJson = currentViewModeJson;
		productsModel.CurrentOrderByJson = currentOrderByJson;
		productsModel.CurrentPageNumberJson = currentPageNumberJson;
	}

	private void SerializeAndPopulateFilterModelsToProductModel(ProductsModel productsModel)
	{
		string specificationFilterModel7SpikesJson = string.Empty;
		if (_specificationFilterModel7Spikes != null)
		{
			specificationFilterModel7SpikesJson = JsonConvert.SerializeObject((object)_specificationFilterModel7Spikes);
		}
		string attributeFilterModel7SpikesJson = string.Empty;
		if (_attributeFilterModel7Spikes != null)
		{
			attributeFilterModel7SpikesJson = JsonConvert.SerializeObject((object)_attributeFilterModel7Spikes);
		}
		string manufacturerFilterModel7SpikesJson = string.Empty;
		if (_manufacturerFilterModel7Spikes != null)
		{
			manufacturerFilterModel7SpikesJson = JsonConvert.SerializeObject((object)_manufacturerFilterModel7Spikes);
		}
		string vendorFilterModel7SpikesJson = string.Empty;
		if (_vendorFilterModel7Spikes != null)
		{
			vendorFilterModel7SpikesJson = JsonConvert.SerializeObject((object)_vendorFilterModel7Spikes);
		}
		string onSaleFilterModel7SpikesJson = string.Empty;
		if (_onSaleFilterModel != null)
		{
			onSaleFilterModel7SpikesJson = JsonConvert.SerializeObject((object)_onSaleFilterModel);
		}
		string inStockFilterModel7SpikesJson = string.Empty;
		if (_inStockFilterModel7Spikes != null)
		{
			inStockFilterModel7SpikesJson = JsonConvert.SerializeObject((object)_inStockFilterModel7Spikes);
		}
		productsModel.SpecificationFilterModel7SpikesJson = specificationFilterModel7SpikesJson;
		productsModel.AttributeFilterModel7SpikesJson = attributeFilterModel7SpikesJson;
		productsModel.ManufacturerFilterModel7SpikesJson = manufacturerFilterModel7SpikesJson;
		productsModel.VendorFilterModel7SpikesJson = vendorFilterModel7SpikesJson;
		productsModel.OnSaleFilterModel7SpikesJson = onSaleFilterModel7SpikesJson;
		productsModel.InStockFilterModel7SpikesJson = inStockFilterModel7SpikesJson;
	}

	private async Task SetSelectedOptionIdsCacheAsync(ProductsResultDataDTO productsResultData)
	{
		if (_isOnSearchPage)
		{
			ICustomerService customerService = _customerService;
			string customerRolesIds = string.Join(",", await customerService.GetCustomerRoleIdsAsync(await _workContext.GetCurrentCustomerAsync(), false));
			await SetSpecificationOptionIdsCacheAsync(productsResultData.SpecificationOptionIds, customerRolesIds);
			await SetAttributeOptionIdsCacheAsync(productsResultData.ProductVariantIds, customerRolesIds);
		}
	}

	private async Task SetSpecificationOptionIdsCacheAsync(IList<int> specificationOptionIds, string customerRolesIds)
	{
		IStaticCacheManager cacheManager = _cacheManager;
		CacheKey nOP_AJAX_FILTERS_SPECIFICATION_OPTION_IDS_KEY = NopAjaxFiltersModelCacheEventConsumer.NOP_AJAX_FILTERS_SPECIFICATION_OPTION_IDS_KEY;
		object obj = _categoryId;
		object obj2 = _manufacturerId;
		object obj3 = _vendorId;
		object obj4 = ((BaseEntity)(await _workContext.GetWorkingLanguageAsync())).Id;
		Store val = await _storeContext.GetCurrentStoreAsync();
		CacheKey specCacheKey = ((ICacheKeyService)cacheManager).PrepareKey(nOP_AJAX_FILTERS_SPECIFICATION_OPTION_IDS_KEY, new object[12]
		{
			obj,
			obj2,
			obj3,
			obj4,
			customerRolesIds,
			((BaseEntity)val).Id,
			_keyword,
			_minPriceConverted,
			_maxPriceConverted,
			_includeProductsFromSubcategories,
			_searchInProductDescriptions,
			_advancedSearch
		});
		if (await _staticCacheManager.GetAsync<IList<int>>(specCacheKey, (Func<Task<IList<int>>>)(async () => (IList<int>)null)) == null)
		{
			await _staticCacheManager.SetAsync<IList<int>>(specCacheKey, specificationOptionIds);
		}
	}

	private async Task SetAttributeOptionIdsCacheAsync(IList<int> productVariantIds, string customerRolesIds)
	{
		IStaticCacheManager cacheManager = _cacheManager;
		CacheKey nOP_AJAX_FILTERS_ATTRIBUTE_OPTION_IDS_KEY = NopAjaxFiltersModelCacheEventConsumer.NOP_AJAX_FILTERS_ATTRIBUTE_OPTION_IDS_KEY;
		object obj = _categoryId;
		object obj2 = _manufacturerId;
		object obj3 = _vendorId;
		object obj4 = ((BaseEntity)(await _workContext.GetWorkingLanguageAsync())).Id;
		Store val = await _storeContext.GetCurrentStoreAsync();
		CacheKey val2 = ((ICacheKeyService)cacheManager).PrepareKey(nOP_AJAX_FILTERS_ATTRIBUTE_OPTION_IDS_KEY, new object[12]
		{
			obj,
			obj2,
			obj3,
			obj4,
			customerRolesIds,
			((BaseEntity)val).Id,
			_keyword,
			_minPriceConverted,
			_maxPriceConverted,
			_includeProductsFromSubcategories,
			_searchInProductDescriptions,
			_advancedSearch
		});
		if (_staticCacheManager.GetAsync<IList<int>>(val2, (Func<Task<IList<int>>>)(async () => (IList<int>)null)) == null)
		{
			await _staticCacheManager.SetAsync<IList<int>>(val2, productVariantIds);
		}
	}

	private void AdjustModelsFilterItemsStateWithSelectedOptionIds(ProductsResultDataDTO productsResultData)
	{
		if ((_priceRangeFilterModel7Spikes != null && _priceRangeFilterModel7Spikes.Priority != 0) || (_specificationFilterModel7Spikes != null && _specificationFilterModelDto.Priority != 0) || (_attributeFilterModel7Spikes != null && _attributeFilterModelDto.Priority != 0) || (_manufacturerFilterModel7Spikes != null && _manufacturerFilterModelDto.Priority != 0) || (_vendorFilterModel7Spikes != null && _vendorFilterModelDto.Priority != 0) || _onSaleFilterModel != null || _inStockFilterModel7Spikes != null)
		{
			if (_specificationFilterModel7Spikes != null)
			{
				AdjustSpecificationFilterModel7SpikesWithSelectedOptionIds(_specificationFilterModel7Spikes, productsResultData.SpecificationOptionIds);
			}
			if (_attributeFilterModel7Spikes != null)
			{
				AdjustAttributeFilterModel7SpikesWithSelectedProductVariantIds(_attributeFilterModel7Spikes, productsResultData.ProductVariantIds);
			}
			if (_manufacturerFilterModel7Spikes != null)
			{
				AdjustManufacturerFilterModel7SpikesWithSelectedOptionIds(_manufacturerFilterModel7Spikes, productsResultData.ManufacturerIds);
			}
			if (_vendorFilterModel7Spikes != null)
			{
				AdjustVendorFilterModel7SpikesWithSelectedOptionIds(_vendorFilterModel7Spikes, productsResultData.VendorIds);
			}
			if (_onSaleFilterModel != null)
			{
				AdjustOnSaleFilterModel7SpikesWithSelectedOptionIds(_onSaleFilterModel, productsResultData.HasProductsOnSale);
			}
			if (_inStockFilterModel7Spikes != null)
			{
				AdjustInStockFilterModel7SpikesWithSelectedOptionIds(_inStockFilterModel7Spikes, productsResultData.HasProductsInStock);
			}
		}
	}

	private void SetFilterDTOs()
	{
		if (_specificationFilterModel7Spikes != null)
		{
			_specificationFilterModelDto = _specificationFilterModel7Spikes.ToDTO();
			_specificationFilterModelDto.SpecificationFilterDTOs = _specificationFilterModelDto.SpecificationFilterDTOs.Where((SpecificationFilterDTO x) => x.SelectedFilterIds.Count > 0).ToList();
		}
		if (_attributeFilterModel7Spikes != null)
		{
			_attributeFilterModelDto = _attributeFilterModel7Spikes.ToDTO();
			_attributeFilterModelDto.AttributeFilterDTOs = _attributeFilterModelDto.AttributeFilterDTOs.Where((AttributeFilterDTO x) => x.SelectedProductVariantIds.Count > 0).ToList();
		}
		if (_manufacturerFilterModel7Spikes != null)
		{
			_manufacturerFilterModelDto = _manufacturerFilterModel7Spikes.ToDTO();
		}
		if (_vendorFilterModel7Spikes != null)
		{
			_vendorFilterModelDto = _vendorFilterModel7Spikes.ToDTO();
		}
	}

	private void AdjustOnSaleFilterModel7SpikesWithSelectedOptionIds(OnSaleFilterModel7Spikes onSaleFilterModel, bool hasProductsOnSale)
	{
		onSaleFilterModel.FilterItemState = FilterItemStateManager.GetNewStateBaseOnOptionAvailability(onSaleFilterModel.FilterItemState, hasProductsOnSale);
	}

	private async Task AdjustMinAndMaxPriceRangeWithDiscountsAndTaxAsync()
	{
		if (_priceRangeFilterModel7Spikes != null && !(_priceRangeFilterModel7Spikes.SelectedPriceRange == (PriceRangeModel)null))
		{
			PriceRangeModel selectedPriceRange = _priceRangeFilterModel7Spikes.SelectedPriceRange;
			IStaticCacheManager cacheManager = _cacheManager;
			CacheKey nOP_AJAX_FILTERS_PRICE_RANGE_FILTER_DTO_KEY = NopAjaxFiltersModelCacheEventConsumer.NOP_AJAX_FILTERS_PRICE_RANGE_FILTER_DTO_KEY;
			object obj = _categoryId;
			object obj2 = _manufacturerId;
			object obj3 = _vendorId;
			object obj4 = ((BaseEntity)(await _workContext.GetCurrentCustomerAsync())).Id;
			object obj5 = ((BaseEntity)(await _workContext.GetWorkingCurrencyAsync())).Id;
			object obj6 = ((BaseEntity)(await _workContext.GetWorkingLanguageAsync())).Id;
			object obj7 = ((BaseEntity)(await _storeContext.GetCurrentStoreAsync())).Id;
			TaxDisplayType val = await _workContext.GetTaxDisplayTypeAsync();
			CacheKey val2 = ((ICacheKeyService)cacheManager).PrepareKeyForDefaultCache(nOP_AJAX_FILTERS_PRICE_RANGE_FILTER_DTO_KEY, new object[8] { obj, obj2, obj3, obj4, obj5, obj6, obj7, val });
			PriceRangeFilterDto priceRangeFilterDto = await _staticCacheManager.GetAsync<PriceRangeFilterDto>(val2, (Func<Task<PriceRangeFilterDto>>)(async () => (PriceRangeFilterDto)null));
			if (priceRangeFilterDto == null)
			{
				priceRangeFilterDto = await _priceCalculationServiceNopAjaxFilters.GetPriceRangeFilterDtoAsync(_categoryId, _manufacturerId, _vendorId);
			}
			if (selectedPriceRange.From.HasValue)
			{
				_minPriceConverted = await _priceCalculationServiceNopAjaxFilters.CalculateBasePriceAsync(selectedPriceRange.From.Value, priceRangeFilterDto, isFromPrice: true);
			}
			if (selectedPriceRange.To.HasValue)
			{
				_maxPriceConverted = await _priceCalculationServiceNopAjaxFilters.CalculateBasePriceAsync(selectedPriceRange.To.Value, priceRangeFilterDto, isFromPrice: false);
			}
		}
	}

	private void AdjustSpecificationFilterModel7SpikesWithSelectedOptionIds(SpecificationFilterModel7Spikes specificationFilterModel7Spikes, IList<int> specificationOptionIds)
	{
		foreach (SpecificationFilterGroup specificationFilterGroup in specificationFilterModel7Spikes.SpecificationFilterGroups)
		{
			foreach (SpecificationFilterItem filterItem in specificationFilterGroup.FilterItems)
			{
				List<int> list = specificationOptionIds.ToList();
				if (specificationFilterGroup.IsMain && filterItem.FilterItemState != FilterItemState.Disabled && filterItem.FilterItemState != FilterItemState.CheckedDisabled)
				{
					list.Add(filterItem.Id);
				}
				bool optionAvailable = list.Contains(filterItem.Id);
				filterItem.FilterItemState = FilterItemStateManager.GetNewStateBaseOnOptionAvailability(filterItem.FilterItemState, optionAvailable);
			}
		}
	}

	private void AdjustAttributeFilterModel7SpikesWithSelectedProductVariantIds(AttributeFilterModel7Spikes attributeFilterModel7Spikes, IList<int> productVariantIds)
	{
		foreach (AttributeFilterGroup attributeFilterGroup in attributeFilterModel7Spikes.AttributeFilterGroups)
		{
			foreach (AttributeFilterItem filterItem in attributeFilterGroup.FilterItems)
			{
				List<int> list = productVariantIds.ToList();
				if (attributeFilterGroup.IsMain && filterItem.FilterItemState != FilterItemState.Disabled && filterItem.FilterItemState != FilterItemState.CheckedDisabled)
				{
					list.AddRange(filterItem.ProductVariantAttributeIds);
				}
				bool optionAvailable = filterItem.ProductVariantAttributeIds.Intersect(list).Any();
				filterItem.FilterItemState = FilterItemStateManager.GetNewStateBaseOnOptionAvailability(filterItem.FilterItemState, optionAvailable);
			}
		}
	}

	private void AdjustManufacturerFilterModel7SpikesWithSelectedOptionIds(ManufacturerFilterModel7Spikes manufacturerFilterModel7Spikes, IList<int> specificationOptionIds)
	{
		foreach (ManufacturerFilterItem manufacturerFilterItem in manufacturerFilterModel7Spikes.ManufacturerFilterItems)
		{
			List<int> list = specificationOptionIds.ToList();
			if (manufacturerFilterModel7Spikes.Priority == 1 && manufacturerFilterItem.FilterItemState != FilterItemState.Disabled && manufacturerFilterItem.FilterItemState != FilterItemState.CheckedDisabled)
			{
				list.Add(manufacturerFilterItem.Id);
			}
			bool optionAvailable = list.Contains(manufacturerFilterItem.Id);
			manufacturerFilterItem.FilterItemState = FilterItemStateManager.GetNewStateBaseOnOptionAvailability(manufacturerFilterItem.FilterItemState, optionAvailable);
		}
	}

	private void AdjustVendorFilterModel7SpikesWithSelectedOptionIds(VendorFilterModel7Spikes vendorFilterModel7Spikes, IList<int> specificationOptionIds)
	{
		foreach (VendorFilterItem vendorFilterItem in vendorFilterModel7Spikes.VendorFilterItems)
		{
			List<int> list = specificationOptionIds.ToList();
			if (vendorFilterModel7Spikes.Priority == 1 && vendorFilterItem.FilterItemState != FilterItemState.Disabled && vendorFilterItem.FilterItemState != FilterItemState.CheckedDisabled)
			{
				list.Add(vendorFilterItem.Id);
			}
			bool optionAvailable = list.Contains(vendorFilterItem.Id);
			vendorFilterItem.FilterItemState = FilterItemStateManager.GetNewStateBaseOnOptionAvailability(vendorFilterItem.FilterItemState, optionAvailable);
		}
	}

	private void AdjustInStockFilterModel7SpikesWithSelectedOptionIds(InStockFilterModel7Spikes inStockFilterModel, bool hasProductsInStock)
	{
		inStockFilterModel.FilterItemState = FilterItemStateManager.GetNewStateBaseOnOptionAvailability(inStockFilterModel.FilterItemState, hasProductsInStock);
	}
}
