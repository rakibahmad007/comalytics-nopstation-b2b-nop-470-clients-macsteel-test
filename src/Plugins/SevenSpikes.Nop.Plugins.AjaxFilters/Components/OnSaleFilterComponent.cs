using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Core.Caching;
using Nop.Core.Domain.Stores;
using Nop.Services.Localization;
using Nop.Web.Framework.Components;
using SevenSpikes.Nop.Framework.Components;
using SevenSpikes.Nop.Plugins.AjaxFilters.Domain.Enums;
using SevenSpikes.Nop.Plugins.AjaxFilters.Infrastructure.Cache;
using SevenSpikes.Nop.Plugins.AjaxFilters.Models.OnSaleFilter;
using SevenSpikes.Nop.Plugins.AjaxFilters.Services;
using SevenSpikes.Nop.Services.Helpers;

namespace SevenSpikes.Nop.Plugins.AjaxFilters.Components;

[ViewComponent(Name = "NopAjaxFilterOnSaleFilter")]
public class OnSaleFilterComponent : Base7SpikesComponent
{
	private readonly IAclHelper _aclHelper;

	private readonly ILocalizationService _localizationService;

	private readonly IProductServiceNopAjaxFilters _productService7Spikes;

	private readonly IStaticCacheManager _staticCacheManager;

	private readonly IStoreContext _storeContext;

	private readonly IWorkContext _workContext;

	public OnSaleFilterComponent(IAclHelper aclHelper, ILocalizationService localizationService, IProductServiceNopAjaxFilters productService7Spikes, IStaticCacheManager staticCacheManager, IStoreContext storeContext, IWorkContext workContext)
	{
		_aclHelper = aclHelper;
		_localizationService = localizationService;
		_productService7Spikes = productService7Spikes;
		_staticCacheManager = staticCacheManager;
		_storeContext = storeContext;
		_workContext = workContext;
	}

	public async Task<IViewComponentResult> InvokeAsync(int categoryId, int manufacturerId, int vendorId)
	{
		OnSaleFilterModel7Spikes onSaleFilterModel7Spikes = await GetOnSaleFilterInternalAsync(categoryId, manufacturerId, vendorId);
		if (onSaleFilterModel7Spikes == null)
		{
			return ((ViewComponent)(object)this).Content(string.Empty);
		}
		return ((NopViewComponent)this).View<OnSaleFilterModel7Spikes>("OnSaleFilter", onSaleFilterModel7Spikes);
	}

	private async Task<OnSaleFilterModel7Spikes> GetOnSaleFilterInternalAsync(int categoryId, int manufacturerId, int vendorId)
	{
		IStaticCacheManager staticCacheManager = _staticCacheManager;
		CacheKey nOP_AJAX_FILTERS_ONSALE_FILTERS_MODEL_KEY = NopAjaxFiltersModelCacheEventConsumer.NOP_AJAX_FILTERS_ONSALE_FILTERS_MODEL_KEY;
		object obj = ((BaseEntity)(await _workContext.GetWorkingLanguageAsync())).Id;
		object obj2 = await _aclHelper.GetAllowedCustomerRolesIdsAsync();
		Store val = await _storeContext.GetCurrentStoreAsync();
		CacheKey val2 = ((ICacheKeyService)staticCacheManager).PrepareKeyForDefaultCache(nOP_AJAX_FILTERS_ONSALE_FILTERS_MODEL_KEY, new object[6]
		{
			obj,
			obj2,
			((BaseEntity)val).Id,
			categoryId,
			manufacturerId,
			vendorId
		});
		if (!(await _staticCacheManager.GetAsync<bool>(val2, (Func<Task<bool>>)(async () => await _productService7Spikes.HasProductsOnSaleAsync(categoryId, manufacturerId, vendorId)))))
		{
			return null;
		}
		OnSaleFilterModel7Spikes salesModel = new OnSaleFilterModel7Spikes
		{
			Id = 1
		};
		OnSaleFilterModel7Spikes onSaleFilterModel7Spikes = salesModel;
		onSaleFilterModel7Spikes.Name = await _localizationService.GetResourceAsync("SevenSpikes.NopAjaxFilters.Public.OnSale.Option");
		salesModel.CategoryId = categoryId;
		salesModel.VendorId = vendorId;
		salesModel.ManufacturerId = manufacturerId;
		salesModel.FilterItemState = FilterItemState.Unchecked;
		return salesModel;
	}
}
