using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Newtonsoft.Json;
using Nop.Core;
using Nop.Core.Caching;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Stores;
using Nop.Web.Framework.Components;
using SevenSpikes.Nop.Framework.Components;
using SevenSpikes.Nop.Plugins.AjaxFilters.Domain;
using SevenSpikes.Nop.Plugins.AjaxFilters.Helpers;
using SevenSpikes.Nop.Plugins.AjaxFilters.Infrastructure.Cache;
using SevenSpikes.Nop.Plugins.AjaxFilters.Models;
using SevenSpikes.Nop.Services.Helpers;

namespace SevenSpikes.Nop.Plugins.AjaxFilters.Components;

[ViewComponent(Name = "NopAjaxFilters")]
public class NopAjaxFiltersComponent : Base7SpikesComponent
{
	private readonly IAclHelper _aclHelper;

	private readonly IStoreContext _storeContext;

	private readonly IWorkContext _workContext;

	private readonly IStaticCacheManager _staticCacheManager;

	private readonly CommonSettings _commonSettings;

	private readonly CatalogSettings _catalogSettings;

	private readonly NopAjaxFiltersSettings _nopAjaxFiltersSettings;

	private readonly IFiltersPageHelper _filtersPageHelper;

	public NopAjaxFiltersComponent(IAclHelper aclHelper, IStoreContext storeContext, IWorkContext workContext, IStaticCacheManager staticCacheManager, CommonSettings commonSettings, CatalogSettings catalogSettings, NopAjaxFiltersSettings nopAjaxFiltersSettings, IFiltersPageHelper filtersPageHelper)
	{
		_aclHelper = aclHelper;
		_storeContext = storeContext;
		_workContext = workContext;
		_staticCacheManager = staticCacheManager;
		_commonSettings = commonSettings;
		_catalogSettings = catalogSettings;
		_nopAjaxFiltersSettings = nopAjaxFiltersSettings;
		_filtersPageHelper = filtersPageHelper;
	}

	public async Task<IViewComponentResult> InvokeAsync(string widgetZone)
	{
		if (_nopAjaxFiltersSettings.WidgetZone == widgetZone && _nopAjaxFiltersSettings.EnableAjaxFilters)
		{
			FiltersPageParameters filtersPageParameters = (await GetFiltersPageHelperAsync()).GetFiltersPageParameters();
			if (!(await (await GetFiltersPageHelperAsync()).ValidateParametersAsync(filtersPageParameters)) || !ShouldShowFiltersInCategory(filtersPageParameters.CategoryId) || (filtersPageParameters.ManufacturerId > 0 && !_nopAjaxFiltersSettings.ShowFiltersOnManufacturerPage) || (filtersPageParameters.VendorId > 0 && !_nopAjaxFiltersSettings.ShowFiltersOnVendorPage) || (filtersPageParameters.SearchQueryStringParameters.IsOnSearchPage && !_nopAjaxFiltersSettings.ShowFiltersOnSearchPage))
			{
				return ((ViewComponent)(object)this).Content(string.Empty);
			}
			return ((NopViewComponent)this).View<NopAjaxFiltersModel>("NopFilters", await GetCahedNopAjaxFiltersSettingsModelAsync(filtersPageParameters));
		}
		return ((ViewComponent)(object)this).Content(string.Empty);
	}

	private bool ShouldShowFiltersInCategory(int categoryId)
	{
		bool result = _nopAjaxFiltersSettings.ShowFiltersOnCategoryPage;
		if (categoryId > 0 && !string.IsNullOrEmpty(_nopAjaxFiltersSettings.CategoriesWithoutFilters))
		{
			string[] array = _nopAjaxFiltersSettings.CategoriesWithoutFilters.Split(',');
			for (int i = 0; i < array.Length; i++)
			{
				if (int.TryParse(array[i], out var result2) && result2 == categoryId)
				{
					result = false;
					break;
				}
			}
		}
		return result;
	}

	private async Task<NopAjaxFiltersModel> GetCahedNopAjaxFiltersSettingsModelAsync(FiltersPageParameters filtersPageParameters)
	{
		IStaticCacheManager staticCacheManager = _staticCacheManager;
		CacheKey nOP_AJAX_FILTERS_MODEL_KEY = NopAjaxFiltersModelCacheEventConsumer.NOP_AJAX_FILTERS_MODEL_KEY;
		object obj = filtersPageParameters.CategoryId;
		object obj2 = filtersPageParameters.ManufacturerId;
		object obj3 = filtersPageParameters.VendorId;
		object obj4 = ((BaseEntity)(await _workContext.GetWorkingLanguageAsync())).Id;
		object obj5 = await _aclHelper.GetAllowedCustomerRolesIdsAsync();
		Store val = await _storeContext.GetCurrentStoreAsync();
		CacheKey val2 = ((ICacheKeyService)staticCacheManager).PrepareKeyForDefaultCache(nOP_AJAX_FILTERS_MODEL_KEY, new object[15]
		{
			obj,
			obj2,
			obj3,
			obj4,
			obj5,
			((BaseEntity)val).Id,
			filtersPageParameters.SearchQueryStringParameters.Keyword,
			filtersPageParameters.SearchQueryStringParameters.SearchCategoryId,
			filtersPageParameters.SearchQueryStringParameters.SearchManufacturerId,
			filtersPageParameters.SearchQueryStringParameters.SearchVendorId,
			filtersPageParameters.SearchQueryStringParameters.PriceFrom,
			filtersPageParameters.SearchQueryStringParameters.PriceTo,
			filtersPageParameters.SearchQueryStringParameters.IncludeSubcategories,
			filtersPageParameters.SearchQueryStringParameters.SearchInProductDescriptions,
			filtersPageParameters.SearchQueryStringParameters.AdvancedSearch
		});
		return await _staticCacheManager.GetAsync<NopAjaxFiltersModel>(val2, (Func<Task<NopAjaxFiltersModel>>)async delegate
		{
			string availableSortOptionsJson = await GetAvailableSortOptionsJsonAsync(filtersPageParameters.OrderBy);
			string availableViewModesJson = await GetAvailableViewModesJsonAsync(filtersPageParameters.ViewMode);
			string availablePageSizesJson = GetAvailablePageSizesJson(filtersPageParameters.PageSize);
			return new NopAjaxFiltersModel
			{
				CategoryId = filtersPageParameters.CategoryId,
				ManufacturerId = filtersPageParameters.ManufacturerId,
				VendorId = filtersPageParameters.VendorId,
				SearchQueryStringParameters = filtersPageParameters.SearchQueryStringParameters,
				DefaultViewMode = _catalogSettings.DefaultViewMode,
				AvailableSortOptionsJson = availableSortOptionsJson,
				AvailableViewModesJson = availableViewModesJson,
				AvailablePageSizesJson = availablePageSizesJson
			};
		});
	}

	private async Task<string> GetAvailableSortOptionsJsonAsync(int? orderBy)
	{
		bool flag = _catalogSettings.ProductSortingEnumDisabled.Count == Enum.GetValues(typeof(ProductSortingEnum)).Length;
		int value;
		IOrderedEnumerable<KeyValuePair<int, int>> orderedEnumerable = from idOption in Enum.GetValues(typeof(ProductSortingEnum)).Cast<int>().Except(_catalogSettings.ProductSortingEnumDisabled)
			select new KeyValuePair<int, int>(idOption, _catalogSettings.ProductSortingEnumDisplayOrder.TryGetValue(idOption, out value) ? value : idOption) into x
			orderby x.Value
			select x;
		if (!orderBy.HasValue)
		{
			orderBy = ((!flag) ? orderedEnumerable.First().Key : 0);
		}
		IList<SelectListItem> availableSortOptions = null;
		if (_catalogSettings.AllowProductSorting && !flag)
		{
			availableSortOptions = new List<SelectListItem>();
			foreach (KeyValuePair<int, int> option in orderedEnumerable)
			{
				string text = await base.LocalizationService.GetLocalizedEnumAsync<ProductSortingEnum>((ProductSortingEnum)option.Key, (int?)null);
				availableSortOptions.Add(new SelectListItem
				{
					Text = text,
					Value = option.Key.ToString(),
					Selected = (option.Key == orderBy)
				});
			}
		}
		string result = string.Empty;
		if (availableSortOptions != null)
		{
			result = JsonConvert.SerializeObject((object)availableSortOptions);
		}
		return result;
	}

	private async Task<string> GetAvailableViewModesJsonAsync(string viewMode)
	{
		IList<SelectListItem> list = null;
		string selectedViewMode2 = viewMode;
		if (string.IsNullOrWhiteSpace(selectedViewMode2))
		{
			selectedViewMode2 = _catalogSettings.DefaultViewMode;
		}
		selectedViewMode2 = selectedViewMode2.ToLowerInvariant();
		if (selectedViewMode2 != "grid" && selectedViewMode2 != "list")
		{
			selectedViewMode2 = "grid";
		}
		if (_catalogSettings.AllowProductViewModeChanging)
		{
			List<SelectListItem> list2 = new List<SelectListItem>();
			List<SelectListItem> list3 = list2;
			SelectListItem selectListItem = new SelectListItem();
			SelectListItem selectListItem2 = selectListItem;
			selectListItem2.Text = await base.LocalizationService.GetResourceAsync("Catalog.ViewMode.Grid");
			selectListItem.Value = "grid";
			selectListItem.Selected = selectedViewMode2 == "grid";
			list3.Add(selectListItem);
			List<SelectListItem> list4 = list2;
			SelectListItem selectListItem3 = new SelectListItem();
			SelectListItem selectListItem4 = selectListItem3;
			selectListItem4.Text = await base.LocalizationService.GetResourceAsync("Catalog.ViewMode.List");
			selectListItem3.Value = "list";
			selectListItem3.Selected = selectedViewMode2 == "list";
			list4.Add(selectListItem3);
			list = list2;
		}
		string result = string.Empty;
		if (list != null)
		{
			result = JsonConvert.SerializeObject((object)list);
		}
		return result;
	}

	private string GetAvailablePageSizesJson(int pageSize)
	{
		IList<SelectListItem> list = new List<SelectListItem>();
		string[] pageSizes = _filtersPageHelper.GetPageSizes();
		if (pageSizes.Any())
		{
			if (pageSize <= 0 || !pageSizes.Contains(pageSize.ToString()))
			{
				pageSize = _filtersPageHelper.GetDefaultPageSize();
			}
			string[] array = pageSizes;
			foreach (string text in array)
			{
				if (int.TryParse(text, out var result) && result > 0)
				{
					list.Add(new SelectListItem
					{
						Text = text,
						Value = text,
						Selected = text.Equals(pageSize.ToString(), StringComparison.InvariantCultureIgnoreCase)
					});
				}
			}
			if (list.Any())
			{
				list = list.OrderBy((SelectListItem x) => int.Parse(x.Text)).ToList();
			}
		}
		string result2 = string.Empty;
		if (list.Any())
		{
			result2 = JsonConvert.SerializeObject((object)list);
		}
		return result2;
	}

	private async Task<IFiltersPageHelper> GetFiltersPageHelperAsync()
	{
		string value = ((ViewComponent)(object)this).Request.QueryString.Value;
		if (string.IsNullOrEmpty(value))
		{
			await _filtersPageHelper.InitializeAsync(((ViewComponent)(object)this).Url.ActionContext);
		}
		else
		{
			await _filtersPageHelper.InitializeAsync(((ViewComponent)(object)this).Url.ActionContext, value);
		}
		return _filtersPageHelper;
	}
}
