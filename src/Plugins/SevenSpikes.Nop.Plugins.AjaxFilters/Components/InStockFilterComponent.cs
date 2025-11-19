using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Core.Caching;
using Nop.Core.Domain.Stores;
using Nop.Core.Infrastructure;
using Nop.Services.Localization;
using Nop.Web.Framework.Components;
using NopStation.Plugin.B2B.B2BB2CFeatures;
using SevenSpikes.Nop.Framework.Components;
using SevenSpikes.Nop.Plugins.AjaxFilters.Domain.Enums;
using SevenSpikes.Nop.Plugins.AjaxFilters.Infrastructure.Cache;
using SevenSpikes.Nop.Plugins.AjaxFilters.Models.InStockFilter;
using SevenSpikes.Nop.Plugins.AjaxFilters.Services;
using SevenSpikes.Nop.Services.Helpers;

namespace SevenSpikes.Nop.Plugins.AjaxFilters.Components;

[ViewComponent(Name = "NopAjaxFiltersInStockFilter")]
public class InStockFilterComponent : Base7SpikesComponent
{
    private readonly IAclHelper _aclHelper;

    private readonly ILocalizationService _localizationService;

    private readonly IProductServiceNopAjaxFilters _productService7Spikes;

    private readonly IStaticCacheManager _staticCacheManager;

    private readonly IStoreContext _storeContext;

    private readonly IWorkContext _workContext;
    private readonly B2BB2CFeaturesSettings _b2BB2CFeaturesSettings;


    public InStockFilterComponent(IAclHelper aclHelper, ILocalizationService localizationService, IProductServiceNopAjaxFilters productService7Spikes, IStaticCacheManager staticCacheManager, IStoreContext storeContext, IWorkContext workContext, B2BB2CFeaturesSettings b2BB2CFeaturesSettings)
    {
        _aclHelper = aclHelper;
        _localizationService = localizationService;
        _productService7Spikes = productService7Spikes;
        _staticCacheManager = staticCacheManager;
        _storeContext = storeContext;
        _workContext = workContext;
        _b2BB2CFeaturesSettings = b2BB2CFeaturesSettings;
    }

    public async Task<IViewComponentResult> InvokeAsync(int categoryId, int manufacturerId, int vendorId)
    {
        if (_b2BB2CFeaturesSettings.HideInStockFilterWhenCustomerIsLoggedOut && !EngineContext.Current.Resolve<IHttpContextAccessor>().HttpContext.User.Identity.IsAuthenticated)
            return Content(string.Empty);
        InStockFilterModel7Spikes inStockFilterModel7Spikes = await GetInStockFilterInternalAsync(categoryId, manufacturerId, vendorId);
        if (inStockFilterModel7Spikes == null)
        {
            return ((ViewComponent)(object)this).Content(string.Empty);
        }
        return ((NopViewComponent)this).View<InStockFilterModel7Spikes>("InStockFilter", inStockFilterModel7Spikes);
    }

    private async Task<InStockFilterModel7Spikes> GetInStockFilterInternalAsync(int categoryId, int manufacturerId, int vendorId)
    {
        IStaticCacheManager staticCacheManager = _staticCacheManager;
        CacheKey nOP_AJAX_FILTERS_INSTOCK_FILTERS_MODEL_KEY = NopAjaxFiltersModelCacheEventConsumer.NOP_AJAX_FILTERS_INSTOCK_FILTERS_MODEL_KEY;
        object obj = ((BaseEntity)(await _workContext.GetWorkingLanguageAsync())).Id;
        object obj2 = await _aclHelper.GetAllowedCustomerRolesIdsAsync();
        Store val = await _storeContext.GetCurrentStoreAsync();
        CacheKey val2 = ((ICacheKeyService)staticCacheManager).PrepareKeyForDefaultCache(nOP_AJAX_FILTERS_INSTOCK_FILTERS_MODEL_KEY, new object[6]
        {
            obj,
            obj2,
            ((BaseEntity)val).Id,
            categoryId,
            manufacturerId,
            vendorId
        });
        if (!(await _staticCacheManager.GetAsync<bool>(val2, (Func<Task<bool>>)(async () => await _productService7Spikes.HasProductsInStockAsync(categoryId, manufacturerId, vendorId)))))
        {
            return null;
        }
        InStockFilterModel7Spikes inStockFilterModel7Spikes = new InStockFilterModel7Spikes
        {
            Id = 1
        };
        InStockFilterModel7Spikes inStockFilterModel7Spikes2 = inStockFilterModel7Spikes;
        inStockFilterModel7Spikes2.Name = await _localizationService.GetResourceAsync("SevenSpikes.NopAjaxFilters.Public.InStock.Option");
        inStockFilterModel7Spikes.CategoryId = categoryId;
        inStockFilterModel7Spikes.VendorId = vendorId;
        inStockFilterModel7Spikes.ManufacturerId = manufacturerId;
        inStockFilterModel7Spikes.FilterItemState = FilterItemState.Unchecked;
        return inStockFilterModel7Spikes;
    }
}
