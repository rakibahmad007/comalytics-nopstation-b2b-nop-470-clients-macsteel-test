using System;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Core.Caching;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Vendors;
using Nop.Services.Catalog;
using Nop.Services.Stores;
using Nop.Services.Vendors;
using Nop.Web.Factories;
using Nop.Web.Framework.UI.Paging;
using Nop.Web.Models.Catalog;

namespace SevenSpikes.Nop.Plugins.AjaxFilters.Helpers;

public class FiltersPageHelper : IFiltersPageHelper
{
	private readonly ICategoryService _categoryService;

	private readonly IManufacturerService _manufacturerService;

	private readonly IVendorService _vendorService;

	private readonly IStoreMappingService _storeMappingService;

	private readonly ICategoryTemplateService _categoryTemplateService;

	private readonly IManufacturerTemplateService _manufacturerTemplateService;

	private readonly CatalogSettings _catalogSettings;

	private readonly IStaticCacheManager _cacheManager;

	private readonly ISearchQueryStringHelper _searchQueryStringHelper;

	private ActionContext _requestContext;

	private FiltersPageParameters _filtersPageParameters;

	private ICatalogModelFactory _catalogModelFactory;

	private const int DefaultOrderBy = 0;

	private const int DefaultPageNumber = 1;

	private Category _category;

	private Manufacturer _manufacturer;

	private Vendor _vendor;

	public FiltersPageHelper(ICategoryService categoryService, IManufacturerService manufacturerService, IVendorService vendorService, IStoreMappingService storeMappingService, ICategoryTemplateService categoryTemplateService, IManufacturerTemplateService manufacturerTemplateService, CatalogSettings catalogSettings, IStaticCacheManager cacheManager, ISearchQueryStringHelper searchQueryStringHelper, ICatalogModelFactory catalogModelFactory)
	{
		_categoryService = categoryService;
		_manufacturerService = manufacturerService;
		_vendorService = vendorService;
		_storeMappingService = storeMappingService;
		_categoryTemplateService = categoryTemplateService;
		_manufacturerTemplateService = manufacturerTemplateService;
		_catalogSettings = catalogSettings;
		_cacheManager = cacheManager;
		_searchQueryStringHelper = searchQueryStringHelper;
		_catalogModelFactory = catalogModelFactory;
	}

	public async Task InitializeAsync(ActionContext requestContext, string query)
	{
		await InitializeAsync(requestContext);
		_filtersPageParameters.SearchQueryStringParameters = _searchQueryStringHelper.GetQueryStringParameters(query);
	}

	public async Task InitializeAsync(ActionContext requestContext)
	{
		_requestContext = requestContext;
		_filtersPageParameters = SetRequestParametersFromRouteData();
		if (_filtersPageParameters.CategoryId > 0)
		{
			_category = await _categoryService.GetCategoryByIdAsync(_filtersPageParameters.CategoryId);
		}
		if (_filtersPageParameters.ManufacturerId > 0)
		{
			_manufacturer = await _manufacturerService.GetManufacturerByIdAsync(_filtersPageParameters.ManufacturerId);
		}
		if (_filtersPageParameters.VendorId > 0)
		{
			_vendor = await _vendorService.GetVendorByIdAsync(_filtersPageParameters.VendorId);
		}
	}

	public async Task InitializeAsync(FiltersPageParameters filtersPageParameters)
	{
		_filtersPageParameters = filtersPageParameters;
		if (_filtersPageParameters.CategoryId > 0)
		{
			_category = await _categoryService.GetCategoryByIdAsync(_filtersPageParameters.CategoryId);
		}
		if (_filtersPageParameters.ManufacturerId > 0)
		{
			_manufacturer = await _manufacturerService.GetManufacturerByIdAsync(_filtersPageParameters.ManufacturerId);
		}
		if (_filtersPageParameters.VendorId > 0)
		{
			_vendor = await _vendorService.GetVendorByIdAsync(_filtersPageParameters.VendorId);
		}
	}

	public FiltersPageParameters GetFiltersPageParameters()
	{
		return _filtersPageParameters;
	}

	private FiltersPageParameters SetRequestParametersFromRouteData()
	{
		FiltersPageParameters filtersPageParameters = new FiltersPageParameters();
		if (_requestContext.RouteData.Values.Keys.Contains("categoryid"))
		{
			filtersPageParameters.CategoryId = int.Parse(_requestContext.RouteData.Values["categoryid"].ToString());
		}
		if (_requestContext.RouteData.Values.Keys.Contains("manufacturerid"))
		{
			filtersPageParameters.ManufacturerId = int.Parse(_requestContext.RouteData.Values["manufacturerid"].ToString());
		}
		if (_requestContext.RouteData.Values.Keys.Contains("vendorid"))
		{
			filtersPageParameters.VendorId = int.Parse(_requestContext.RouteData.Values["vendorid"].ToString());
		}
		if (_requestContext.RouteData.Values.Keys.Contains("orderBy"))
		{
			filtersPageParameters.OrderBy = int.Parse(_requestContext.RouteData.Values["orderBy"].ToString());
		}
		if (_requestContext.RouteData.Values.Keys.Contains("viewMode"))
		{
			filtersPageParameters.ViewMode = _requestContext.RouteData.Values["viewMode"].ToString();
		}
		if (_requestContext.RouteData.Values.Keys.Contains("pageSize"))
		{
			filtersPageParameters.PageSize = int.Parse(_requestContext.RouteData.Values["pageSize"].ToString());
		}
		return filtersPageParameters;
	}

	public async Task<bool> ValidateParametersAsync(FiltersPageParameters filtersPageParameters)
	{
		bool valid = true;
		if (filtersPageParameters.CategoryId == 0 && filtersPageParameters.ManufacturerId == 0 && filtersPageParameters.VendorId == 0 && !filtersPageParameters.SearchQueryStringParameters.IsOnSearchPage)
		{
			valid = false;
		}
		else if (filtersPageParameters.CategoryId > 0)
		{
			Category val = await _categoryService.GetCategoryByIdAsync(filtersPageParameters.CategoryId);
			bool flag = val == null;
			if (!flag)
			{
				flag = !(await _storeMappingService.AuthorizeAsync<Category>(val));
			}
			if (flag)
			{
				valid = false;
			}
		}
		else if (filtersPageParameters.ManufacturerId > 0)
		{
			Manufacturer val2 = await _manufacturerService.GetManufacturerByIdAsync(filtersPageParameters.ManufacturerId);
			bool flag = val2 == null;
			if (!flag)
			{
				flag = !(await _storeMappingService.AuthorizeAsync<Manufacturer>(val2));
			}
			if (flag)
			{
				valid = false;
			}
		}
		else if (filtersPageParameters.VendorId > 0 && await _vendorService.GetVendorByIdAsync(filtersPageParameters.VendorId) == null)
		{
			valid = false;
		}
		return valid;
	}

	public string[] GetPageSizes()
	{
		string text = string.Empty;
		if (_category != null && _category.AllowCustomersToSelectPageSize && _category.PageSizeOptions != null)
		{
			text = _category.PageSizeOptions;
		}
		if (_manufacturer != null && _manufacturer.AllowCustomersToSelectPageSize && _manufacturer.PageSizeOptions != null)
		{
			text = _manufacturer.PageSizeOptions;
		}
		if (_vendor != null && _vendor.AllowCustomersToSelectPageSize && _vendor.PageSizeOptions != null)
		{
			text = _vendor.PageSizeOptions;
		}
		if (_filtersPageParameters.SearchQueryStringParameters.IsOnSearchPage && _catalogSettings.SearchPageAllowCustomersToSelectPageSize)
		{
			text = _catalogSettings.SearchPagePageSizeOptions;
		}
		return text.Split(new char[2] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries);
	}

	public int GetDefaultPageSize()
	{
		int result = 0;
		string[] array = null;
		if (_category != null && _category.AllowCustomersToSelectPageSize && _category.PageSizeOptions != null)
		{
			array = _category.PageSizeOptions.Split(new char[2] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries);
		}
		else if (_category != null)
		{
			result = _category.PageSize;
		}
		if (_manufacturer != null && _manufacturer.AllowCustomersToSelectPageSize && _manufacturer.PageSizeOptions != null)
		{
			array = _manufacturer.PageSizeOptions.Split(new char[2] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries);
		}
		else if (_manufacturer != null)
		{
			result = _manufacturer.PageSize;
		}
		if (_vendor != null && _vendor.AllowCustomersToSelectPageSize && _vendor.PageSizeOptions != null)
		{
			array = _vendor.PageSizeOptions.Split(new char[2] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries);
		}
		else if (_vendor != null)
		{
			result = _vendor.PageSize;
		}
		if (_filtersPageParameters.SearchQueryStringParameters.IsOnSearchPage && _catalogSettings.SearchPageAllowCustomersToSelectPageSize && _catalogSettings.SearchPagePageSizeOptions != null)
		{
			array = _catalogSettings.SearchPagePageSizeOptions.Split(new char[2] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries);
		}
		else
		{
			result = _catalogSettings.SearchPageProductsPerPage;
		}
		if (array != null && array.Any() && int.TryParse(array.FirstOrDefault(), out var result2) && result2 > 0)
		{
			result = result2;
		}
		return result;
	}

	public int GetDefaultOrderBy()
	{
		return 0;
	}

	public int GetDefaultPageNumber()
	{
		return 1;
	}

	public string GetDefaultViewMode()
	{
		return _catalogSettings.DefaultViewMode;
	}

	public async Task AdjustPagingFilteringModelPageSizeAndPageNumberAsync(CatalogProductsCommand catalogPagingFilteringModel)
	{
		string[] pageSizes = GetPageSizes();
		if (pageSizes.Any() && (((BasePageableModel)catalogPagingFilteringModel).PageSize <= 0 || !pageSizes.Contains(((BasePageableModel)catalogPagingFilteringModel).PageSize.ToString(CultureInfo.InvariantCulture))) && int.TryParse(pageSizes.FirstOrDefault(), out var result) && result > 0)
		{
			((BasePageableModel)catalogPagingFilteringModel).PageSize = result;
		}
		Category category = null;
		Manufacturer manufacturer = null;
		Vendor vendor = null;
		if (_filtersPageParameters.CategoryId > 0)
		{
			category = await _categoryService.GetCategoryByIdAsync(_filtersPageParameters.CategoryId);
		}
		if (_filtersPageParameters.ManufacturerId > 0)
		{
			manufacturer = await _manufacturerService.GetManufacturerByIdAsync(_filtersPageParameters.ManufacturerId);
		}
		if (_filtersPageParameters.VendorId > 0)
		{
			vendor = await _vendorService.GetVendorByIdAsync(_filtersPageParameters.VendorId);
		}
		if (((BasePageableModel)catalogPagingFilteringModel).PageSize <= 0 && category != null)
		{
			((BasePageableModel)catalogPagingFilteringModel).PageSize = category.PageSize;
		}
		if (((BasePageableModel)catalogPagingFilteringModel).PageSize <= 0 && manufacturer != null)
		{
			((BasePageableModel)catalogPagingFilteringModel).PageSize = manufacturer.PageSize;
		}
		if (((BasePageableModel)catalogPagingFilteringModel).PageSize <= 0 && vendor != null)
		{
			((BasePageableModel)catalogPagingFilteringModel).PageSize = _vendor.PageSize;
		}
		if (((BasePageableModel)catalogPagingFilteringModel).PageSize <= 0 && _filtersPageParameters.SearchQueryStringParameters.IsOnSearchPage)
		{
			((BasePageableModel)catalogPagingFilteringModel).PageSize = _catalogSettings.SearchPageProductsPerPage;
		}
		if (((BasePageableModel)catalogPagingFilteringModel).PageNumber <= 0)
		{
			((BasePageableModel)catalogPagingFilteringModel).PageNumber = 1;
		}
	}

	public async Task<string> GetTemplateViewPathAsync()
	{
		string empty = string.Empty;
		if (_category != null)
		{
			return await _catalogModelFactory.PrepareCategoryTemplateViewPathAsync(_category.CategoryTemplateId);
		}
		if (_manufacturer != null)
		{
			return await _catalogModelFactory.PrepareManufacturerTemplateViewPathAsync(_manufacturer.ManufacturerTemplateId);
		}
		if (_vendor != null)
		{
			return "VendorTemplate.ProductsInGridOrLines";
		}
		if (_filtersPageParameters.SearchQueryStringParameters.IsOnSearchPage)
		{
			return "SearchTemplate.ProductsInGridOrLines";
		}
		return empty;
	}
}
