using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Core.Caching;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Stores;
using Nop.Services.Catalog;
using Nop.Services.Customers;
using Nop.Web.Framework.Components;
using NopStation.Plugin.B2B.B2BB2CFeatures;
using SevenSpikes.Nop.Framework.Components;
using SevenSpikes.Nop.Plugins.AjaxFilters.Domain;
using SevenSpikes.Nop.Plugins.AjaxFilters.Helpers;
using SevenSpikes.Nop.Plugins.AjaxFilters.Infrastructure.Cache;
using SevenSpikes.Nop.Plugins.AjaxFilters.Models.SpecificationFilter;
using SevenSpikes.Nop.Services.Catalog;
using SevenSpikes.Nop.Services.Helpers;

namespace SevenSpikes.Nop.Plugins.AjaxFilters.Components;

[ViewComponent(Name = "NopAjaxFiltersSpecificationFilters")]
public class SpecificationFilterComponent : Base7SpikesComponent
{
	private readonly IAclHelper _aclHelper;

	private readonly ISearchQueryStringHelper _searchQueryStringHelper;

	private readonly ISpecificationAttributeService7Spikes _specificationAttributeService7Spikes;

	private readonly IStaticCacheManager _staticCacheManager;

	private readonly IStoreContext _storeContext;

	private readonly IWorkContext _workContext;

	private readonly CatalogSettings _catalogSettings;

	private readonly NopAjaxFiltersSettings _nopAjaxFilterSettings;

	private readonly ISpecificationAttributeService _specificationAttributeService;
	private readonly B2BB2CFeaturesSettings _b2BB2CFeaturesSettings;

    public SpecificationFilterComponent(IAclHelper aclHelper, ISearchQueryStringHelper searchQueryStringHelper, ISpecificationAttributeService7Spikes specificationAttributeService7Spikes, IStaticCacheManager staticCacheManager, IStoreContext storeContext, IWorkContext workContext, CatalogSettings catalogSettings, NopAjaxFiltersSettings nopAjaxFilterSettings, ISpecificationAttributeService specificationAttributeService, B2BB2CFeaturesSettings b2BB2CFeaturesSettings)
    {
        _aclHelper = aclHelper;
        _searchQueryStringHelper = searchQueryStringHelper;
        _specificationAttributeService7Spikes = specificationAttributeService7Spikes;
        _staticCacheManager = staticCacheManager;
        _storeContext = storeContext;
        _workContext = workContext;
        _catalogSettings = catalogSettings;
        _nopAjaxFilterSettings = nopAjaxFilterSettings;
        _specificationAttributeService = specificationAttributeService;
        _b2BB2CFeaturesSettings = b2BB2CFeaturesSettings;
    }

    public async Task<IViewComponentResult> InvokeAsync(int categoryId, int manufacturerId, int vendorId)
	{
		SpecificationFilterModel7Spikes specificationFilterModel7Spikes = await GetSpecificationFilterInternalAsync(categoryId, manufacturerId, vendorId);
		if (specificationFilterModel7Spikes.SpecificationFilterGroups.Count == 0)
		{
			return ((ViewComponent)(object)this).Content(string.Empty);
		}
		return ((NopViewComponent)this).View<SpecificationFilterModel7Spikes>("SpecificationFilter", specificationFilterModel7Spikes);
	}

	private async Task<SpecificationFilterModel7Spikes> GetSpecificationFilterInternalAsync(int categoryId, int manufacturerId, int vendorId)
	{
		SearchQueryStringParameters searchQueryStringParameters = _searchQueryStringHelper.GetQueryStringParameters(((ViewComponent)(object)this).Request.QueryString.Value);
		SpecificationFilterModel7Spikes specificationFilterModel7Spikes = new SpecificationFilterModel7Spikes();
		if (searchQueryStringParameters.IsOnSearchPage)
		{
			ICustomerService customerService = base.CustomerService;
			string customerRolesIds = string.Join(",", await customerService.GetCustomerRoleIdsAsync(await _workContext.GetCurrentCustomerAsync(), false));
			IStaticCacheManager staticCacheManager = _staticCacheManager;
			CacheKey nOP_AJAX_FILTERS_SPECIFICATION_OPTION_IDS_KEY = NopAjaxFiltersModelCacheEventConsumer.NOP_AJAX_FILTERS_SPECIFICATION_OPTION_IDS_KEY;
			object obj = searchQueryStringParameters.SearchCategoryId;
			object obj2 = searchQueryStringParameters.SearchManufacturerId;
			object obj3 = searchQueryStringParameters.SearchVendorId;
			object obj4 = ((BaseEntity)(await _workContext.GetWorkingLanguageAsync())).Id;
			object obj5 = customerRolesIds;
			Store val = await _storeContext.GetCurrentStoreAsync();
			CacheKey val2 = ((ICacheKeyService)staticCacheManager).PrepareKey(nOP_AJAX_FILTERS_SPECIFICATION_OPTION_IDS_KEY, new object[12]
			{
				obj,
				obj2,
				obj3,
				obj4,
				obj5,
				((BaseEntity)val).Id,
				searchQueryStringParameters.Keyword,
				searchQueryStringParameters.PriceFrom,
				searchQueryStringParameters.PriceTo,
				searchQueryStringParameters.IncludeSubcategories,
				searchQueryStringParameters.SearchInProductDescriptions,
				searchQueryStringParameters.AdvancedSearch
			});
			IList<int> list = await _staticCacheManager.GetAsync<IList<int>>(val2, (Func<Task<IList<int>>>)(async () => (IList<int>)null));
			IList<SpecificationAttributeOption> list2 = new List<SpecificationAttributeOption>();
			if (list != null && list.Any())
			{
				list2 = await _specificationAttributeService7Spikes.GetSpecificationAttributeOptionsByIdsAsync(list);
			}
			if (list2.Count > 0)
			{
                #region B2B
                // to hide facet filter
                list2 = list2.Where(x => x.SpecificationAttributeId != _b2BB2CFeaturesSettings.PreFilterFacetSpecificationAttributeId).ToList();
                #endregion
                specificationFilterModel7Spikes = await GetSpecificationFilterModel7SpikesAsync(categoryId, manufacturerId, vendorId, list2);
			}
		}
		else
		{
			IStaticCacheManager staticCacheManager = _staticCacheManager;
			CacheKey nOP_AJAX_FILTERS_SPECIFICATION_OPTION_IDS_KEY = NopAjaxFiltersModelCacheEventConsumer.NOP_AJAX_FILTERS_SPECIFICATION_FILTERS_MODEL_KEY;
			object obj5 = categoryId;
			object obj4 = manufacturerId;
			object obj3 = vendorId;
			object obj2 = ((BaseEntity)(await _workContext.GetWorkingLanguageAsync())).Id;
			object obj = await _aclHelper.GetAllowedCustomerRolesIdsAsync();
			Store val = await _storeContext.GetCurrentStoreAsync();
			CacheKey val3 = ((ICacheKeyService)staticCacheManager).PrepareKeyForDefaultCache(nOP_AJAX_FILTERS_SPECIFICATION_OPTION_IDS_KEY, new object[6]
			{
				obj5,
				obj4,
				obj3,
				obj2,
				obj,
				((BaseEntity)val).Id
			});
			specificationFilterModel7Spikes = await _staticCacheManager.GetAsync<SpecificationFilterModel7Spikes>(val3, (Func<Task<SpecificationFilterModel7Spikes>>)async delegate
			{
				IList<SpecificationAttributeOption> list3 = new List<SpecificationAttributeOption>();
				if (categoryId > 0)
				{
					list3 = await _specificationAttributeService7Spikes.GetSpecificationAttributeOptionsByCategoryIdAsync(categoryId, _catalogSettings.ShowProductsFromSubcategories);
				}
				else if (manufacturerId > 0)
				{
					list3 = await _specificationAttributeService7Spikes.GetSpecificationAttributeOptionsByManufacturerIdAsync(manufacturerId);
				}
				else if (vendorId > 0)
				{
					list3 = await _specificationAttributeService7Spikes.GetSpecificationAttributeOptionsByVendorIdAsync(vendorId);
				}
				SpecificationFilterModel7Spikes result = new SpecificationFilterModel7Spikes();
				if (list3.Count > 0)
				{
                    #region B2B
                    // to hide facet filter
                    list3 = list3.Where(x => x.SpecificationAttributeId != _b2BB2CFeaturesSettings.PreFilterFacetSpecificationAttributeId).ToList();
                    #endregion
                    result = await GetSpecificationFilterModel7SpikesAsync(categoryId, manufacturerId, vendorId, list3);
				}
				return result;
			});
		}
		return specificationFilterModel7Spikes;
	}

	private async Task<SpecificationFilterModel7Spikes> GetSpecificationFilterModel7SpikesAsync(int categoryId, int manufacturerId, int vendorId, IEnumerable<SpecificationAttributeOption> specificationAttributeOptions)
	{
		SpecificationFilterModel7Spikes specificationFilterModel7Spikes = new SpecificationFilterModel7Spikes
		{
			CategoryId = categoryId,
			ManufacturerId = manufacturerId,
			VendorId = vendorId
		};
		Dictionary<int, SpecificationFilterGroup> specificationFilterGroupsDictionary = new Dictionary<int, SpecificationFilterGroup>();
		foreach (SpecificationAttributeOption specificationAttributeOption in specificationAttributeOptions)
		{
			specificationFilterGroupsDictionary.TryGetValue(specificationAttributeOption.SpecificationAttributeId, out var value);
			if (value == null)
			{
				SpecificationAttribute val = await _specificationAttributeService.GetSpecificationAttributeByIdAsync(specificationAttributeOption.SpecificationAttributeId);
				SpecificationFilterGroup specificationFilterGroup = new SpecificationFilterGroup
				{
					Id = specificationAttributeOption.SpecificationAttributeId,
					DisplayOrder = val.DisplayOrder
				};
				SpecificationFilterGroup specificationFilterGroup2 = specificationFilterGroup;
				specificationFilterGroup2.Name = await base.LocalizationService.GetLocalizedAsync<SpecificationAttribute, string>(val, (Expression<Func<SpecificationAttribute, string>>)((SpecificationAttribute x) => x.Name), (int?)null, true, true);
				value = specificationFilterGroup;
				specificationFilterModel7Spikes.SpecificationFilterGroups.Add(value);
				specificationFilterGroupsDictionary.Add(value.Id, value);
			}
			string colorSquaresRgb = ((!string.IsNullOrEmpty(specificationAttributeOption.ColorSquaresRgb)) ? specificationAttributeOption.ColorSquaresRgb.ToLower().Trim() : null);
			IList<SpecificationFilterItem> filterItems = value.FilterItems;
			SpecificationFilterItem specificationFilterItem = new SpecificationFilterItem
			{
				Id = ((BaseEntity)specificationAttributeOption).Id
			};
			SpecificationFilterItem specificationFilterItem2 = specificationFilterItem;
			specificationFilterItem2.Name = await base.LocalizationService.GetLocalizedAsync<SpecificationAttributeOption, string>(specificationAttributeOption, (Expression<Func<SpecificationAttributeOption, string>>)((SpecificationAttributeOption x) => x.Name), (int?)null, true, true);
			specificationFilterItem.DisplayOrder = specificationAttributeOption.DisplayOrder;
			specificationFilterItem.ColorSquaresRgb = colorSquaresRgb;
			filterItems.Add(specificationFilterItem);
		}
		foreach (SpecificationFilterGroup specificationFilterGroup3 in specificationFilterModel7Spikes.SpecificationFilterGroups)
		{
			specificationFilterGroup3.FilterItems = (from x in specificationFilterGroup3.FilterItems
				orderby x.DisplayOrder, x.Name
				select x).ToList();
		}
		specificationFilterModel7Spikes.SpecificationFilterGroups = specificationFilterModel7Spikes.SpecificationFilterGroups.OrderBy((SpecificationFilterGroup x) => x.DisplayOrder).Take(_nopAjaxFilterSettings.NumberOfSpecificationFilters).ToList();
		return specificationFilterModel7Spikes;
	}
}
