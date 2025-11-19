using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Core.Caching;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Stores;
using Nop.Web.Framework.Components;
using SevenSpikes.Nop.Framework.Components;
using SevenSpikes.Nop.Plugins.AjaxFilters.Infrastructure.Cache;
using SevenSpikes.Nop.Plugins.AjaxFilters.Models.ManufacturerFilter;
using SevenSpikes.Nop.Services.Catalog;
using SevenSpikes.Nop.Services.Helpers;

namespace SevenSpikes.Nop.Plugins.AjaxFilters.Components;

[ViewComponent(Name = "NopAjaxFiltersManufacturerFilters")]
public class ManufacturerFilterComponent : Base7SpikesComponent
{
	private readonly IAclHelper _aclHelper;

	private readonly IManufacturerService7Spikes _manufacturerService7Spikes;

	private readonly IStaticCacheManager _staticCacheManager;

	private readonly IStoreContext _storeContext;

	private readonly IWorkContext _workContext;

	private readonly CatalogSettings _catalogSettings;

	public ManufacturerFilterComponent(IAclHelper aclHelper, IManufacturerService7Spikes manufacturerService7Spikes, IStaticCacheManager staticCacheManager, IStoreContext storeContext, IWorkContext workContext, CatalogSettings catalogSettings)
	{
		_aclHelper = aclHelper;
		_manufacturerService7Spikes = manufacturerService7Spikes;
		_staticCacheManager = staticCacheManager;
		_storeContext = storeContext;
		_workContext = workContext;
		_catalogSettings = catalogSettings;
	}

	public async Task<IViewComponentResult> InvokeAsync(int categoryId, int vendorId)
	{
		ManufacturerFilterModel7Spikes manufacturerFilterModel7Spikes = await GetManufacturerFilterInternalAsync(categoryId, vendorId);
		if (manufacturerFilterModel7Spikes.ManufacturerFilterItems.Count == 0)
		{
			return ((ViewComponent)(object)this).Content(string.Empty);
		}
		return ((NopViewComponent)this).View<ManufacturerFilterModel7Spikes>("ManufacturerFilter", manufacturerFilterModel7Spikes);
	}

	private async Task<ManufacturerFilterModel7Spikes> GetManufacturerFilterInternalAsync(int categoryId, int vendorId)
	{
		IStaticCacheManager staticCacheManager = _staticCacheManager;
		CacheKey nOP_AJAX_FILTERS_MANUFACTURER_FILTERS_MODEL_KEY = NopAjaxFiltersModelCacheEventConsumer.NOP_AJAX_FILTERS_MANUFACTURER_FILTERS_MODEL_KEY;
		object obj = categoryId;
		object obj2 = vendorId;
		object obj3 = ((BaseEntity)(await _workContext.GetWorkingLanguageAsync())).Id;
		object obj4 = await _aclHelper.GetAllowedCustomerRolesIdsAsync();
		Store val = await _storeContext.GetCurrentStoreAsync();
		CacheKey val2 = ((ICacheKeyService)staticCacheManager).PrepareKeyForDefaultCache(nOP_AJAX_FILTERS_MANUFACTURER_FILTERS_MODEL_KEY, new object[5]
		{
			obj,
			obj2,
			obj3,
			obj4,
			((BaseEntity)val).Id
		});
		ManufacturerFilterModel7Spikes manufacturerFilterModel7Spikes = await _staticCacheManager.GetAsync<ManufacturerFilterModel7Spikes>(val2, (Func<Task<ManufacturerFilterModel7Spikes>>)async delegate
		{
			IList<Manufacturer> list = new List<Manufacturer>();
			if (categoryId > 0)
			{
				list = await _manufacturerService7Spikes.GetManufacturersByCategoryIdAsync(categoryId, _catalogSettings.ShowProductsFromSubcategories);
			}
			else if (vendorId > 0)
			{
				list = await _manufacturerService7Spikes.GetManufacturersByVendorIdAsync(vendorId);
			}
			ManufacturerFilterModel7Spikes manufacturerFilterModel7SpikesToReturn = new ManufacturerFilterModel7Spikes();
			if (list.Count > 0)
			{
				manufacturerFilterModel7SpikesToReturn = new ManufacturerFilterModel7Spikes
				{
					CategoryId = categoryId,
					VendorId = vendorId
				};
				foreach (Manufacturer item in list)
				{
					ManufacturerFilterItem manufacturerFilterItem = new ManufacturerFilterItem
					{
						Id = ((BaseEntity)item).Id
					};
					ManufacturerFilterItem manufacturerFilterItem2 = manufacturerFilterItem;
					manufacturerFilterItem2.Name = await base.LocalizationService.GetLocalizedAsync<Manufacturer, string>(item, (Expression<Func<Manufacturer, string>>)((Manufacturer x) => x.Name), (int?)null, true, true);
					manufacturerFilterModel7SpikesToReturn.ManufacturerFilterItems.Add(manufacturerFilterItem);
				}
			}
			return manufacturerFilterModel7SpikesToReturn;
		});
		if (manufacturerFilterModel7Spikes.CategoryId == 0 && manufacturerFilterModel7Spikes.VendorId == 0)
		{
			return new ManufacturerFilterModel7Spikes();
		}
		return manufacturerFilterModel7Spikes;
	}
}
