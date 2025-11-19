using System;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Core.Caching;
using Nop.Core.Domain.Tax;
using Nop.Services.Catalog;
using Nop.Services.Security;
using Nop.Web.Framework.Components;
using Nop.Web.Models.Catalog;
using SevenSpikes.Nop.Framework.Components;
using SevenSpikes.Nop.Plugins.AjaxFilters.Domain;
using SevenSpikes.Nop.Plugins.AjaxFilters.Helpers;
using SevenSpikes.Nop.Plugins.AjaxFilters.Infrastructure.Cache;
using SevenSpikes.Nop.Plugins.AjaxFilters.Models.PriceRangeFilterSlider;
using SevenSpikes.Nop.Plugins.AjaxFilters.Services;

namespace SevenSpikes.Nop.Plugins.AjaxFilters.Components;

[ViewComponent(Name = "NopAjaxFiltersPriceRangeFilter")]
public class PriceRangeFilterSliderComponent : Base7SpikesComponent
{
	private readonly IPermissionService _permissionService;

	private readonly IPriceCalculationServiceNopAjaxFilters _priceCalculationServiceNopAjaxFilters;

	private readonly IPriceFormatter _priceFormatter;

	private readonly ISearchQueryStringHelper _searchQueryStringHelper;

	private readonly IStaticCacheManager _staticCacheManager;

	private readonly IStoreContext _storeContext;

	private readonly IWorkContext _workContext;

	private readonly NopAjaxFiltersSettings _nopAjaxFiltersSettings;

	public PriceRangeFilterSliderComponent(IPermissionService permissionService, IPriceCalculationServiceNopAjaxFilters priceCalculationServiceNopAjaxFilters, IPriceFormatter priceFormatter, ISearchQueryStringHelper searchQueryStringHelper, IStaticCacheManager staticCacheManager, IStoreContext storeContext, IWorkContext workContext, NopAjaxFiltersSettings nopAjaxFilterSettings)
	{
		_permissionService = permissionService;
		_priceCalculationServiceNopAjaxFilters = priceCalculationServiceNopAjaxFilters;
		_priceFormatter = priceFormatter;
		_searchQueryStringHelper = searchQueryStringHelper;
		_staticCacheManager = staticCacheManager;
		_storeContext = storeContext;
		_workContext = workContext;
		_nopAjaxFiltersSettings = nopAjaxFilterSettings;
	}

	public async Task<IViewComponentResult> InvokeAsync(int categoryId, int manufacturerId, int vendorId)
	{
		SearchQueryStringParameters searchPageParameters = _searchQueryStringHelper.GetQueryStringParameters(((ViewComponent)(object)this).Request.QueryString.Value);
		if (!(await _permissionService.AuthorizeAsync(StandardPermissionProvider.DisplayPrices)) || searchPageParameters.IsOnSearchPage)
		{
			return ((ViewComponent)(object)this).Content(string.Empty);
		}
		PriceRangeFilterModel7Spikes priceRangeFilterModel7Spikes = await GetPriceRangeFilterInternalAsync(categoryId, manufacturerId, vendorId);
		if ((priceRangeFilterModel7Spikes.MaxPrice == 0m && priceRangeFilterModel7Spikes.MinPrice == 0m) || priceRangeFilterModel7Spikes.MinPrice == priceRangeFilterModel7Spikes.MaxPrice)
		{
			return ((ViewComponent)(object)this).Content(string.Empty);
		}
		return ((NopViewComponent)this).View<PriceRangeFilterModel7Spikes>("PriceRangeFilterSlider", priceRangeFilterModel7Spikes);
	}

	private async Task<PriceRangeFilterModel7Spikes> GetPriceRangeFilterInternalAsync(int categoryId, int manufacturerId, int vendorId)
	{
		IStaticCacheManager staticCacheManager = _staticCacheManager;
		CacheKey nOP_AJAX_FILTERS_PRICE_RANGE_FILTER_DTO_KEY = NopAjaxFiltersModelCacheEventConsumer.NOP_AJAX_FILTERS_PRICE_RANGE_FILTER_DTO_KEY;
		object obj = categoryId;
		object obj2 = manufacturerId;
		object obj3 = vendorId;
		object obj4 = ((BaseEntity)(await _workContext.GetCurrentCustomerAsync())).Id;
		object obj5 = ((BaseEntity)(await _workContext.GetWorkingCurrencyAsync())).Id;
		object obj6 = ((BaseEntity)(await _workContext.GetWorkingLanguageAsync())).Id;
		object obj7 = ((BaseEntity)(await _storeContext.GetCurrentStoreAsync())).Id;
		TaxDisplayType val = await _workContext.GetTaxDisplayTypeAsync();
		CacheKey val2 = ((ICacheKeyService)staticCacheManager).PrepareKeyForDefaultCache(nOP_AJAX_FILTERS_PRICE_RANGE_FILTER_DTO_KEY, new object[8] { obj, obj2, obj3, obj4, obj5, obj6, obj7, val });
		PriceRangeFilterDto priceRangeFilterDto = await _staticCacheManager.GetAsync<PriceRangeFilterDto>(val2, (Func<Task<PriceRangeFilterDto>>)(async () => await _priceCalculationServiceNopAjaxFilters.GetPriceRangeFilterDtoAsync(categoryId, manufacturerId, vendorId)));
		staticCacheManager = _staticCacheManager;
		nOP_AJAX_FILTERS_PRICE_RANGE_FILTER_DTO_KEY = NopAjaxFiltersModelCacheEventConsumer.NOP_AJAX_FILTERS_PRICE_RANGE_FILTER_MODEL_KEY;
		obj7 = categoryId;
		obj6 = manufacturerId;
		obj5 = vendorId;
		obj4 = ((BaseEntity)(await _workContext.GetCurrentCustomerAsync())).Id;
		obj3 = ((BaseEntity)(await _workContext.GetWorkingCurrencyAsync())).Id;
		obj2 = ((BaseEntity)(await _workContext.GetWorkingLanguageAsync())).Id;
		obj = ((BaseEntity)(await _storeContext.GetCurrentStoreAsync())).Id;
		val = await _workContext.GetTaxDisplayTypeAsync();
		CacheKey val3 = ((ICacheKeyService)staticCacheManager).PrepareKeyForDefaultCache(nOP_AJAX_FILTERS_PRICE_RANGE_FILTER_DTO_KEY, new object[8] { obj7, obj6, obj5, obj4, obj3, obj2, obj, val });
		PriceRangeFilterModel7Spikes priceRangeFilterModel7Spikes = await _staticCacheManager.GetAsync<PriceRangeFilterModel7Spikes>(val3, (Func<Task<PriceRangeFilterModel7Spikes>>)async delegate
		{
			decimal minPrice2 = priceRangeFilterDto.MinPrice;
			decimal maxPrice = priceRangeFilterDto.MaxPrice;
			string formatting = (await _workContext.GetWorkingCurrencyAsync()).CustomFormatting;
			PriceRangeFilterModel7Spikes result = null;
			if (minPrice2 != 0m || maxPrice != 0m)
			{
				if (maxPrice - minPrice2 < 1m)
				{
					minPrice2 = maxPrice;
				}
				else
				{
					minPrice2 = Math.Floor(minPrice2);
					maxPrice = Math.Ceiling(maxPrice);
				}
				string currencySymbol = string.Empty;
				if (!string.IsNullOrEmpty((await _workContext.GetWorkingCurrencyAsync()).DisplayLocale))
				{
					currencySymbol = CultureInfo.GetCultureInfo((await _workContext.GetWorkingCurrencyAsync()).DisplayLocale).NumberFormat.CurrencySymbol;
				}
				string minPriceFormatted = await GetFormattedPriceAsync(minPrice2);
				string maxPriceFormatted = await GetFormattedPriceAsync(maxPrice);
				result = new PriceRangeFilterModel7Spikes
				{
					CategoryId = categoryId,
					ManufacturerId = manufacturerId,
					VendorId = vendorId,
					MinPrice = minPrice2,
					MaxPrice = maxPrice,
					MinPriceFormatted = minPriceFormatted,
					MaxPriceFormatted = maxPriceFormatted,
					Formatting = formatting,
					CurrencySymbol = currencySymbol
				};
			}
			return result;
		});
		if (priceRangeFilterModel7Spikes == null || priceRangeFilterModel7Spikes.MinPrice == priceRangeFilterModel7Spikes.MaxPrice)
		{
			return new PriceRangeFilterModel7Spikes();
		}
		PriceRangeModel selectedPriceRange = (PriceRangeModel)(((object)PriceRangeHelper.GetSelectedPriceRange()) ?? ((object)new PriceRangeModel
		{
			From = Math.Floor(priceRangeFilterModel7Spikes.MinPrice),
			To = Math.Ceiling(priceRangeFilterModel7Spikes.MaxPrice)
		}));
		priceRangeFilterModel7Spikes.SelectedPriceRange = selectedPriceRange;
		return priceRangeFilterModel7Spikes;
	}

	private async Task<string> GetFormattedPriceAsync(decimal price)
	{
		string text = await _priceFormatter.FormatPriceAsync(price, true, false);
		string trailingZeroesSeparator = _nopAjaxFiltersSettings.TrailingZeroesSeparator;
		if (string.IsNullOrEmpty(trailingZeroesSeparator))
		{
			return text;
		}
		return new Regex("[" + trailingZeroesSeparator + "][0]{2,}", RegexOptions.RightToLeft).Replace(text, "");
	}
}
