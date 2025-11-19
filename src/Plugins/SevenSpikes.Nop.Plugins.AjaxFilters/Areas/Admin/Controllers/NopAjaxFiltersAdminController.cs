using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.DependencyInjection;
using Nop.Core;
using Nop.Core.Caching;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Cms;
using Nop.Core.Infrastructure;
using Nop.Services;
using Nop.Services.Catalog;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Web.Framework.Kendoui;
using SevenSpikes.Nop.Core.Helpers;
using SevenSpikes.Nop.Framework;
using SevenSpikes.Nop.Framework.Areas.Admin.ControllerAttributes;
using SevenSpikes.Nop.Framework.Areas.Admin.Controllers;
using SevenSpikes.Nop.Framework.Areas.Admin.Helpers;
using SevenSpikes.Nop.Plugins.AjaxFilters.Areas.Admin.Extensions;
using SevenSpikes.Nop.Plugins.AjaxFilters.Areas.Admin.Models;
using SevenSpikes.Nop.Plugins.AjaxFilters.Domain;
using SevenSpikes.Nop.Plugins.AjaxFilters.Domain.Enums;
using SevenSpikes.Nop.Plugins.AjaxFilters.Helpers;

namespace SevenSpikes.Nop.Plugins.AjaxFilters.Areas.Admin.Controllers;

[ManagePluginsAdminAuthorize("SevenSpikes.Nop.Plugins.AjaxFilters", false)]
public class NopAjaxFiltersAdminController : Base7SpikesAdminController
{
	private readonly IConvertToDictionaryHelper _convertToDictionaryHelper;

	private readonly ICustomerActivityService _customerActivityService;

	private readonly ILocalizationService _localizationService;

	private readonly IProductAttributeService _productAttributeService;

	private readonly ISettingService _settingService;

	private readonly ISpecificationAttributeService _specificationAttributeService;

	private readonly IStaticCacheManager _staticCacheManager;

	private readonly WidgetSettings _widgetSettings;

	private readonly NopAjaxFiltersSettings _nopAjaxFiltersSettings;

	public NopAjaxFiltersAdminController(IConvertToDictionaryHelper convertToDictionaryHelper, ICustomerActivityService customerActivityService, ILocalizationService localizationService, IProductAttributeService productAttributeService, ISettingService settingService, ISpecificationAttributeService specificationAttributeService, IStaticCacheManager staticCacheManager, WidgetSettings widgetSettings, NopAjaxFiltersSettings nopAjaxFiltersSettings)
	{
		_convertToDictionaryHelper = convertToDictionaryHelper;
		_customerActivityService = customerActivityService;
		_localizationService = localizationService;
		_productAttributeService = productAttributeService;
		_settingService = settingService;
		_specificationAttributeService = specificationAttributeService;
		_staticCacheManager = staticCacheManager;
		_widgetSettings = widgetSettings;
		_nopAjaxFiltersSettings = nopAjaxFiltersSettings;
	}

	public async Task<ActionResult> Settings()
	{
		int storeScope = await base.StoreContext.GetActiveStoreScopeConfigurationAsync();
		NopAjaxFiltersSettings nopAjaxFiltersSettings = await _settingService.LoadSettingAsync<NopAjaxFiltersSettings>(storeScope);
		NopAjaxFiltersSettingsModel model = nopAjaxFiltersSettings.ToModel();
		await InitializeModelAsync(model, storeScope);
		model.ActiveStoreScopeConfiguration = storeScope;
		if (storeScope > 0)
		{
			StoreScopeSettingsHelper<NopAjaxFiltersSettings> storeScopeSettings = new StoreScopeSettingsHelper<NopAjaxFiltersSettings>(nopAjaxFiltersSettings, storeScope, _settingService);
			NopAjaxFiltersSettingsModel nopAjaxFiltersSettingsModel = model;
			nopAjaxFiltersSettingsModel.EnableAjaxFilters_OverrideForStore = await storeScopeSettings.SettingExistsAsync((NopAjaxFiltersSettings x) => x.EnableAjaxFilters);
			nopAjaxFiltersSettingsModel = model;
			nopAjaxFiltersSettingsModel.FiltersUIMode_OverrideForStore = await storeScopeSettings.SettingExistsAsync((NopAjaxFiltersSettings x) => x.FiltersUIMode);
			nopAjaxFiltersSettingsModel = model;
			nopAjaxFiltersSettingsModel.EnableInfiniteScroll_OverrideForStore = await storeScopeSettings.SettingExistsAsync((NopAjaxFiltersSettings x) => x.EnableInfiniteScroll);
			nopAjaxFiltersSettingsModel = model;
			nopAjaxFiltersSettingsModel.ScrollToElementOnThePageAfterFiltration_OverrideForStore = await storeScopeSettings.SettingExistsAsync((NopAjaxFiltersSettings x) => x.ScrollToElementOnThePageAfterFiltration);
			nopAjaxFiltersSettingsModel = model;
			nopAjaxFiltersSettingsModel.ScrollToElementOnThePageAfterFiltrationMobile_OverrideForStore = await storeScopeSettings.SettingExistsAsync((NopAjaxFiltersSettings x) => x.ScrollToElementOnThePageAfterFiltrationMobile);
			nopAjaxFiltersSettingsModel = model;
			nopAjaxFiltersSettingsModel.ElementToScrollAfterFiltrationSelector_OverrideForStore = await storeScopeSettings.SettingExistsAsync((NopAjaxFiltersSettings x) => x.ElementToScrollAfterFiltrationSelector);
			nopAjaxFiltersSettingsModel = model;
			nopAjaxFiltersSettingsModel.WidgetZone_OverrideForStore = await storeScopeSettings.SettingExistsAsync((NopAjaxFiltersSettings x) => x.WidgetZone);
			nopAjaxFiltersSettingsModel = model;
			nopAjaxFiltersSettingsModel.EnablePriceRangeFilter_OverrideForStore = await storeScopeSettings.SettingExistsAsync((NopAjaxFiltersSettings x) => x.EnablePriceRangeFilter);
			nopAjaxFiltersSettingsModel = model;
			nopAjaxFiltersSettingsModel.ClosePriceRangeFilterBox_OverrideForStore = await storeScopeSettings.SettingExistsAsync((NopAjaxFiltersSettings x) => x.ClosePriceRangeFilterBox);
			nopAjaxFiltersSettingsModel = model;
			nopAjaxFiltersSettingsModel.EnableSpecificationsFilter_OverrideForStore = await storeScopeSettings.SettingExistsAsync((NopAjaxFiltersSettings x) => x.EnableSpecificationsFilter);
			nopAjaxFiltersSettingsModel = model;
			nopAjaxFiltersSettingsModel.CloseSpecificationsFilterBox_OverrideForStore = await storeScopeSettings.SettingExistsAsync((NopAjaxFiltersSettings x) => x.CloseSpecificationsFilterBox);
			nopAjaxFiltersSettingsModel = model;
			nopAjaxFiltersSettingsModel.NumberOfSpecificationFilters_OverrideForStore = await storeScopeSettings.SettingExistsAsync((NopAjaxFiltersSettings x) => x.NumberOfSpecificationFilters);
			nopAjaxFiltersSettingsModel = model;
			nopAjaxFiltersSettingsModel.EnableAttributesFilter_OverrideForStore = await storeScopeSettings.SettingExistsAsync((NopAjaxFiltersSettings x) => x.EnableAttributesFilter);
			nopAjaxFiltersSettingsModel = model;
			nopAjaxFiltersSettingsModel.CloseAttributesFilterBox_OverrideForStore = await storeScopeSettings.SettingExistsAsync((NopAjaxFiltersSettings x) => x.CloseAttributesFilterBox);
			nopAjaxFiltersSettingsModel = model;
			nopAjaxFiltersSettingsModel.NumberOfAttributeFilters_OverrideForStore = await storeScopeSettings.SettingExistsAsync((NopAjaxFiltersSettings x) => x.NumberOfAttributeFilters);
			nopAjaxFiltersSettingsModel = model;
			nopAjaxFiltersSettingsModel.EnableManufacturersFilter_OverrideForStore = await storeScopeSettings.SettingExistsAsync((NopAjaxFiltersSettings x) => x.EnableManufacturersFilter);
			nopAjaxFiltersSettingsModel = model;
			nopAjaxFiltersSettingsModel.CloseManufacturersFilterBox_OverrideForStore = await storeScopeSettings.SettingExistsAsync((NopAjaxFiltersSettings x) => x.CloseManufacturersFilterBox);
			nopAjaxFiltersSettingsModel = model;
			nopAjaxFiltersSettingsModel.EnableOnSaleFilter_OverrideForStore = await storeScopeSettings.SettingExistsAsync((NopAjaxFiltersSettings x) => x.EnableOnSaleFilter);
			nopAjaxFiltersSettingsModel = model;
			nopAjaxFiltersSettingsModel.CloseOnSaleFilterBox_OverrideForStore = await storeScopeSettings.SettingExistsAsync((NopAjaxFiltersSettings x) => x.CloseOnSaleFilterBox);
			nopAjaxFiltersSettingsModel = model;
			nopAjaxFiltersSettingsModel.EnableVendorsFilter_OverrideForStore = await storeScopeSettings.SettingExistsAsync((NopAjaxFiltersSettings x) => x.EnableVendorsFilter);
			nopAjaxFiltersSettingsModel = model;
			nopAjaxFiltersSettingsModel.CloseVendorsFilterBox_OverrideForStore = await storeScopeSettings.SettingExistsAsync((NopAjaxFiltersSettings x) => x.CloseVendorsFilterBox);
			nopAjaxFiltersSettingsModel = model;
			nopAjaxFiltersSettingsModel.EnableInStockFilter_OverrideForStore = await storeScopeSettings.SettingExistsAsync((NopAjaxFiltersSettings x) => x.EnableInStockFilter);
			nopAjaxFiltersSettingsModel = model;
			nopAjaxFiltersSettingsModel.CloseInStockFilterBox_OverrideForStore = await storeScopeSettings.SettingExistsAsync((NopAjaxFiltersSettings x) => x.CloseInStockFilterBox);
			nopAjaxFiltersSettingsModel = model;
			nopAjaxFiltersSettingsModel.ProductsListPanelSelector_OverrideForStore = await storeScopeSettings.SettingExistsAsync((NopAjaxFiltersSettings x) => x.ProductsListPanelSelector);
			nopAjaxFiltersSettingsModel = model;
			nopAjaxFiltersSettingsModel.CategoriesWithoutFilters_OverrideForStore = await storeScopeSettings.SettingExistsAsync((NopAjaxFiltersSettings x) => x.CategoriesWithoutFilters);
			nopAjaxFiltersSettingsModel = model;
			nopAjaxFiltersSettingsModel.ProductsGridPanelSelector_OverrideForStore = await storeScopeSettings.SettingExistsAsync((NopAjaxFiltersSettings x) => x.ProductsGridPanelSelector);
			nopAjaxFiltersSettingsModel = model;
			nopAjaxFiltersSettingsModel.PagerPanelSelector_OverrideForStore = await storeScopeSettings.SettingExistsAsync((NopAjaxFiltersSettings x) => x.PagerPanelSelector);
			nopAjaxFiltersSettingsModel = model;
			nopAjaxFiltersSettingsModel.PagerPanelIntegrationSelector_OverrideForStore = await storeScopeSettings.SettingExistsAsync((NopAjaxFiltersSettings x) => x.PagerPanelIntegrationSelector);
			nopAjaxFiltersSettingsModel = model;
			nopAjaxFiltersSettingsModel.SortOptionsDropDownSelector_OverrideForStore = await storeScopeSettings.SettingExistsAsync((NopAjaxFiltersSettings x) => x.SortOptionsDropDownSelector);
			nopAjaxFiltersSettingsModel = model;
			nopAjaxFiltersSettingsModel.ViewOptionsDropDownSelector_OverrideForStore = await storeScopeSettings.SettingExistsAsync((NopAjaxFiltersSettings x) => x.ViewOptionsDropDownSelector);
			nopAjaxFiltersSettingsModel = model;
			nopAjaxFiltersSettingsModel.ProductPageSizeDropDownSelector_OverrideForStore = await storeScopeSettings.SettingExistsAsync((NopAjaxFiltersSettings x) => x.ProductPageSizeDropDownSelector);
			nopAjaxFiltersSettingsModel = model;
			nopAjaxFiltersSettingsModel.ShowFiltersOnCategoryPage_OverrideForStore = await storeScopeSettings.SettingExistsAsync((NopAjaxFiltersSettings x) => x.ShowFiltersOnCategoryPage);
			nopAjaxFiltersSettingsModel = model;
			nopAjaxFiltersSettingsModel.ShowFiltersOnManufacturerPage_OverrideForStore = await storeScopeSettings.SettingExistsAsync((NopAjaxFiltersSettings x) => x.ShowFiltersOnManufacturerPage);
			nopAjaxFiltersSettingsModel = model;
			nopAjaxFiltersSettingsModel.ShowFiltersOnVendorPage_OverrideForStore = await storeScopeSettings.SettingExistsAsync((NopAjaxFiltersSettings x) => x.ShowFiltersOnVendorPage);
			nopAjaxFiltersSettingsModel = model;
			nopAjaxFiltersSettingsModel.ShowFiltersOnSearchPage_OverrideForStore = await storeScopeSettings.SettingExistsAsync((NopAjaxFiltersSettings x) => x.ShowFiltersOnSearchPage);
			nopAjaxFiltersSettingsModel = model;
			nopAjaxFiltersSettingsModel.ShowSelectedFiltersPanel_OverrideForStore = await storeScopeSettings.SettingExistsAsync((NopAjaxFiltersSettings x) => x.ShowSelectedFiltersPanel);
			nopAjaxFiltersSettingsModel = model;
			nopAjaxFiltersSettingsModel.ShowNumberOfReturnedProducts_OverrideForStore = await storeScopeSettings.SettingExistsAsync((NopAjaxFiltersSettings x) => x.ShowNumberOfReturnedProducts);
			nopAjaxFiltersSettingsModel = model;
			nopAjaxFiltersSettingsModel.ShowNumberOfReturnedProductsSelector_OverrideForStore = await storeScopeSettings.SettingExistsAsync((NopAjaxFiltersSettings x) => x.ShowNumberOfReturnedProductsSelector);
			nopAjaxFiltersSettingsModel = model;
			nopAjaxFiltersSettingsModel.TrailingZeroesSeparator_OverrideForStore = await storeScopeSettings.SettingExistsAsync((NopAjaxFiltersSettings x) => x.TrailingZeroesSeparator);
			nopAjaxFiltersSettingsModel = model;
			nopAjaxFiltersSettingsModel.SearchInProductTags_OverrideForStore = await storeScopeSettings.SettingExistsAsync((NopAjaxFiltersSettings x) => x.SearchInProductTags);
			nopAjaxFiltersSettingsModel = model;
			nopAjaxFiltersSettingsModel.CloseFiltersPanelAfterFiltrationInMobile_OverrideForStore = await storeScopeSettings.SettingExistsAsync((NopAjaxFiltersSettings x) => x.CloseFiltersPanelAfterFiltrationInMobile);
		}
		return ((Controller)(object)this).View("Settings", (object?)model);
	}

	[HttpPost]
	public async Task<ActionResult> SettingsAsync(NopAjaxFiltersSettingsModel nopAjaxFiltersSettingsModel)
	{
		if (!((ControllerBase)(object)this).ModelState.IsValid)
		{
			return ((ControllerBase)(object)this).RedirectToAction("Settings");
		}
		if (nopAjaxFiltersSettingsModel.EnableAjaxFilters && !_widgetSettings.ActiveWidgetSystemNames.Contains("SevenSpikes.Nop.Plugins.AjaxFilters"))
		{
			_widgetSettings.ActiveWidgetSystemNames.Add("SevenSpikes.Nop.Plugins.AjaxFilters");
			await _settingService.SaveSettingAsync<WidgetSettings>(_widgetSettings, 0);
		}
		int storeScope = await base.StoreContext.GetActiveStoreScopeConfigurationAsync();
		NopAjaxFiltersSettings settings = nopAjaxFiltersSettingsModel.ToEntity();
		StoreScopeSettingsHelper<NopAjaxFiltersSettings> storeScopeSettings = new StoreScopeSettingsHelper<NopAjaxFiltersSettings>(settings, storeScope, _settingService);
		await storeScopeSettings.SaveStoreSettingAsync(nopAjaxFiltersSettingsModel.EnableAjaxFilters_OverrideForStore, (NopAjaxFiltersSettings x) => x.EnableAjaxFilters);
		await storeScopeSettings.SaveStoreSettingAsync(nopAjaxFiltersSettingsModel.FiltersUIMode_OverrideForStore, (NopAjaxFiltersSettings x) => x.FiltersUIMode);
		await storeScopeSettings.SaveStoreSettingAsync(nopAjaxFiltersSettingsModel.EnableInfiniteScroll_OverrideForStore, (NopAjaxFiltersSettings x) => x.EnableInfiniteScroll);
		await storeScopeSettings.SaveStoreSettingAsync(nopAjaxFiltersSettingsModel.ScrollToElementOnThePageAfterFiltration_OverrideForStore, (NopAjaxFiltersSettings x) => x.ScrollToElementOnThePageAfterFiltration);
		await storeScopeSettings.SaveStoreSettingAsync(nopAjaxFiltersSettingsModel.ScrollToElementOnThePageAfterFiltrationMobile_OverrideForStore, (NopAjaxFiltersSettings x) => x.ScrollToElementOnThePageAfterFiltrationMobile);
		await storeScopeSettings.SaveStoreSettingAsync(nopAjaxFiltersSettingsModel.ElementToScrollAfterFiltrationSelector_OverrideForStore, (NopAjaxFiltersSettings x) => x.ElementToScrollAfterFiltrationSelector);
		await storeScopeSettings.SaveStoreSettingAsync(nopAjaxFiltersSettingsModel.WidgetZone_OverrideForStore, (NopAjaxFiltersSettings x) => x.WidgetZone);
		await storeScopeSettings.SaveStoreSettingAsync(nopAjaxFiltersSettingsModel.EnablePriceRangeFilter_OverrideForStore, (NopAjaxFiltersSettings x) => x.EnablePriceRangeFilter);
		await storeScopeSettings.SaveStoreSettingAsync(nopAjaxFiltersSettingsModel.ClosePriceRangeFilterBox_OverrideForStore, (NopAjaxFiltersSettings x) => x.ClosePriceRangeFilterBox);
		await storeScopeSettings.SaveStoreSettingAsync(nopAjaxFiltersSettingsModel.EnableSpecificationsFilter_OverrideForStore, (NopAjaxFiltersSettings x) => x.EnableSpecificationsFilter);
		await storeScopeSettings.SaveStoreSettingAsync(nopAjaxFiltersSettingsModel.CloseSpecificationsFilterBox_OverrideForStore, (NopAjaxFiltersSettings x) => x.CloseSpecificationsFilterBox);
		await storeScopeSettings.SaveStoreSettingAsync(nopAjaxFiltersSettingsModel.NumberOfSpecificationFilters_OverrideForStore, (NopAjaxFiltersSettings x) => x.NumberOfSpecificationFilters);
		await storeScopeSettings.SaveStoreSettingAsync(nopAjaxFiltersSettingsModel.EnableAttributesFilter_OverrideForStore, (NopAjaxFiltersSettings x) => x.EnableAttributesFilter);
		await storeScopeSettings.SaveStoreSettingAsync(nopAjaxFiltersSettingsModel.CloseAttributesFilterBox_OverrideForStore, (NopAjaxFiltersSettings x) => x.CloseAttributesFilterBox);
		await storeScopeSettings.SaveStoreSettingAsync(nopAjaxFiltersSettingsModel.NumberOfAttributeFilters_OverrideForStore, (NopAjaxFiltersSettings x) => x.NumberOfAttributeFilters);
		await storeScopeSettings.SaveStoreSettingAsync(nopAjaxFiltersSettingsModel.EnableManufacturersFilter_OverrideForStore, (NopAjaxFiltersSettings x) => x.EnableManufacturersFilter);
		await storeScopeSettings.SaveStoreSettingAsync(nopAjaxFiltersSettingsModel.CloseManufacturersFilterBox_OverrideForStore, (NopAjaxFiltersSettings x) => x.CloseManufacturersFilterBox);
		await storeScopeSettings.SaveStoreSettingAsync(nopAjaxFiltersSettingsModel.EnableOnSaleFilter_OverrideForStore, (NopAjaxFiltersSettings x) => x.EnableOnSaleFilter);
		await storeScopeSettings.SaveStoreSettingAsync(nopAjaxFiltersSettingsModel.CloseOnSaleFilterBox_OverrideForStore, (NopAjaxFiltersSettings x) => x.CloseOnSaleFilterBox);
		await storeScopeSettings.SaveStoreSettingAsync(nopAjaxFiltersSettingsModel.EnableVendorsFilter_OverrideForStore, (NopAjaxFiltersSettings x) => x.EnableVendorsFilter);
		await storeScopeSettings.SaveStoreSettingAsync(nopAjaxFiltersSettingsModel.CloseVendorsFilterBox_OverrideForStore, (NopAjaxFiltersSettings x) => x.CloseVendorsFilterBox);
		await storeScopeSettings.SaveStoreSettingAsync(nopAjaxFiltersSettingsModel.EnableInStockFilter_OverrideForStore, (NopAjaxFiltersSettings x) => x.EnableInStockFilter);
		await storeScopeSettings.SaveStoreSettingAsync(nopAjaxFiltersSettingsModel.CloseInStockFilterBox, (NopAjaxFiltersSettings x) => x.CloseInStockFilterBox);
		await storeScopeSettings.SaveStoreSettingAsync(nopAjaxFiltersSettingsModel.ProductsListPanelSelector_OverrideForStore, (NopAjaxFiltersSettings x) => x.ProductsListPanelSelector);
		await storeScopeSettings.SaveStoreSettingAsync(nopAjaxFiltersSettingsModel.CategoriesWithoutFilters_OverrideForStore, (NopAjaxFiltersSettings x) => x.CategoriesWithoutFilters);
		await storeScopeSettings.SaveStoreSettingAsync(nopAjaxFiltersSettingsModel.ProductsGridPanelSelector_OverrideForStore, (NopAjaxFiltersSettings x) => x.ProductsGridPanelSelector);
		await storeScopeSettings.SaveStoreSettingAsync(nopAjaxFiltersSettingsModel.PagerPanelSelector_OverrideForStore, (NopAjaxFiltersSettings x) => x.PagerPanelSelector);
		await storeScopeSettings.SaveStoreSettingAsync(nopAjaxFiltersSettingsModel.PagerPanelIntegrationSelector_OverrideForStore, (NopAjaxFiltersSettings x) => x.PagerPanelIntegrationSelector);
		await storeScopeSettings.SaveStoreSettingAsync(nopAjaxFiltersSettingsModel.SortOptionsDropDownSelector_OverrideForStore, (NopAjaxFiltersSettings x) => x.SortOptionsDropDownSelector);
		await storeScopeSettings.SaveStoreSettingAsync(nopAjaxFiltersSettingsModel.ViewOptionsDropDownSelector_OverrideForStore, (NopAjaxFiltersSettings x) => x.ViewOptionsDropDownSelector);
		await storeScopeSettings.SaveStoreSettingAsync(nopAjaxFiltersSettingsModel.ProductPageSizeDropDownSelector_OverrideForStore, (NopAjaxFiltersSettings x) => x.ProductPageSizeDropDownSelector);
		await storeScopeSettings.SaveStoreSettingAsync(nopAjaxFiltersSettingsModel.ShowFiltersOnManufacturerPage_OverrideForStore, (NopAjaxFiltersSettings x) => x.ShowFiltersOnManufacturerPage);
		await storeScopeSettings.SaveStoreSettingAsync(nopAjaxFiltersSettingsModel.ShowFiltersOnVendorPage_OverrideForStore, (NopAjaxFiltersSettings x) => x.ShowFiltersOnVendorPage);
		await storeScopeSettings.SaveStoreSettingAsync(nopAjaxFiltersSettingsModel.ShowFiltersOnCategoryPage_OverrideForStore, (NopAjaxFiltersSettings x) => x.ShowFiltersOnCategoryPage);
		await storeScopeSettings.SaveStoreSettingAsync(nopAjaxFiltersSettingsModel.ShowFiltersOnSearchPage_OverrideForStore, (NopAjaxFiltersSettings x) => x.ShowFiltersOnSearchPage);
		await storeScopeSettings.SaveStoreSettingAsync(nopAjaxFiltersSettingsModel.ShowSelectedFiltersPanel_OverrideForStore, (NopAjaxFiltersSettings x) => x.ShowSelectedFiltersPanel);
		await storeScopeSettings.SaveStoreSettingAsync(nopAjaxFiltersSettingsModel.ShowNumberOfReturnedProducts_OverrideForStore, (NopAjaxFiltersSettings x) => x.ShowNumberOfReturnedProducts);
		await storeScopeSettings.SaveStoreSettingAsync(nopAjaxFiltersSettingsModel.ShowNumberOfReturnedProductsSelector_OverrideForStore, (NopAjaxFiltersSettings x) => x.ShowNumberOfReturnedProductsSelector);
		await storeScopeSettings.SaveStoreSettingAsync(nopAjaxFiltersSettingsModel.TrailingZeroesSeparator_OverrideForStore, (NopAjaxFiltersSettings x) => x.TrailingZeroesSeparator);
		await storeScopeSettings.SaveStoreSettingAsync(nopAjaxFiltersSettingsModel.SearchInProductTags_OverrideForStore, (NopAjaxFiltersSettings x) => x.SearchInProductTags);
		await storeScopeSettings.SaveStoreSettingAsync(nopAjaxFiltersSettingsModel.CloseFiltersPanelAfterFiltrationInMobile_OverrideForStore, (NopAjaxFiltersSettings x) => x.CloseFiltersPanelAfterFiltrationInMobile);
		await _settingService.ClearCacheAsync();
		await _staticCacheManager.RemoveByPrefixAsync("nop.pres.nop.ajax.filters", Array.Empty<object>());
		await _customerActivityService.InsertActivityAsync("EditNopAjaxFiltersSettings", "Edit Nop Ajax Filters Settings", (BaseEntity)null);
		await FilterSettingHelper.UpdateNopCommerceFilterSettings();
		SuccessNotification(await _localizationService.GetResourceAsync("Admin.Configuration.Updated"));
		return ((ControllerBase)(object)this).RedirectToAction("Settings");
	}

	private async Task InitializeModelAsync(NopAjaxFiltersSettingsModel model, int storeScope)
	{
		model.SupportedWidgetZones = new SelectList(await EngineContext.Current.Resolve<IInstallHelper>((IServiceScope)null).GetSupportedWidgetZonesAsync("SevenSpikes.Nop.Plugins.AjaxFilters", storeScope));
		model.AvailableFiltersUIModes = await model.FiltersUIMode.ToSelectListAsync();
        model.EnableAjaxFilters = model.EnableAjaxFilters && _widgetSettings.ActiveWidgetSystemNames.Contains("SevenSpikes.Nop.Plugins.AjaxFilters");
		model.IsTrialVersion = false;
	}

	[HttpPost]
	public ActionResult GridList(DataSourceRequest command)
	{
		IList<string> activeProductAttributes = _nopAjaxFiltersSettings.ActiveProductAttributes;
		IList<ProductAttribute> list = new List<ProductAttribute>();
		IDictionary<int, int> dictionary = new Dictionary<int, int>();
		if (activeProductAttributes.Count > 0)
		{
			dictionary = _convertToDictionaryHelper.CreateDictionaryFromSemicolonSeparatedPairs<int, int>(activeProductAttributes);
			list = _productAttributeService.GetProductAttributesByIds(dictionary.Select((KeyValuePair<int, int> x) => x.Key).ToArray());
		}
		DataSourceResult data = new DataSourceResult
		{
			Data = list.Select(delegate(ProductAttribute x)
			{
				int displayOrder = dictionary[((BaseEntity)x).Id];
				return new
				{
					Id = ((BaseEntity)x).Id,
					Name = x.Name,
					DisplayOrder = displayOrder
				};
			}),
			Total = list.Count
		};
		return ((Controller)(object)this).Json((object?)data);
	}

	[HttpPost]
	public async Task<ActionResult> Update(DataSourceRequest command, int id, int displayOrder)
	{
		if (id == 0)
		{
			return base.EmptyJson;
		}
		IList<string> activeProductAttributes = _nopAjaxFiltersSettings.ActiveProductAttributes;
		IDictionary<int, int> dictionary = _convertToDictionaryHelper.CreateDictionaryFromSemicolonSeparatedPairs<int, int>(activeProductAttributes);
		if (dictionary.ContainsKey(id))
		{
			_nopAjaxFiltersSettings.ActiveProductAttributes.Remove($"{id}:{dictionary[id]}");
			_nopAjaxFiltersSettings.ActiveProductAttributes.Add($"{id}:{displayOrder}");
			await _settingService.SaveSettingAsync<NopAjaxFiltersSettings>(_nopAjaxFiltersSettings, 0);
		}
		return GridList(command);
	}

	[HttpPost]
	public async Task<ActionResult> Delete(DataSourceRequest command, int id)
	{
		if (id == 0)
		{
			return base.EmptyJson;
		}
		IList<string> activeProductAttributes = _nopAjaxFiltersSettings.ActiveProductAttributes;
		IDictionary<int, int> dictionary = _convertToDictionaryHelper.CreateDictionaryFromSemicolonSeparatedPairs<int, int>(activeProductAttributes);
		if (dictionary.ContainsKey(id))
		{
			_nopAjaxFiltersSettings.ActiveProductAttributes.Remove($"{id}:{dictionary[id]}");
			await _settingService.SaveSettingAsync<NopAjaxFiltersSettings>(_nopAjaxFiltersSettings, 0);
		}
		return GridList(command);
	}

	[HttpPost]
	public async Task<ActionResult> Create(DataSourceRequest command, string name, int displayOrder)
	{
		if (!string.IsNullOrEmpty(name))
		{
			ProductAttribute productAttributeByName = _productAttributeService.GetProductAttributeByName(name);
			if (productAttributeByName != null)
			{
				IList<string> activeProductAttributes = _nopAjaxFiltersSettings.ActiveProductAttributes;
				if (!_convertToDictionaryHelper.CreateDictionaryFromSemicolonSeparatedPairs<int, int>(activeProductAttributes).ContainsKey(((BaseEntity)productAttributeByName).Id))
				{
					_nopAjaxFiltersSettings.ActiveProductAttributes.Add($"{((BaseEntity)productAttributeByName).Id}:{displayOrder}");
					await _settingService.SaveSettingAsync<NopAjaxFiltersSettings>(_nopAjaxFiltersSettings, 0);
				}
			}
		}
		return GridList(command);
	}

	[HttpGet]
	public async Task<ActionResult> GetProductAttributes()
	{
		IList<string> activeProductAttributes = _nopAjaxFiltersSettings.ActiveProductAttributes;
		IDictionary<int, int> source = _convertToDictionaryHelper.CreateDictionaryFromSemicolonSeparatedPairs<int, int>(activeProductAttributes);
		IEnumerable<int> currentProductAttributeIds = source.Select((KeyValuePair<int, int> x) => x.Key);
		IList<SelectListItem> data = (from x in (IEnumerable<ProductAttribute>)(await _productAttributeService.GetAllProductAttributesAsync(0, int.MaxValue))
			where !currentProductAttributeIds.Contains(((BaseEntity)x).Id)
			select new SelectListItem
			{
				Value = ((BaseEntity)x).Id.ToString(),
				Text = x.Name
			}).ToList();
		return ((Controller)(object)this).Json((object?)data);
	}

	[HttpPost]
	public ActionResult SpecificationAttributeSlidersGridList(DataSourceRequest command)
	{
		IList<int> specificationAttributeSliders = _nopAjaxFiltersSettings.SpecificationAttributeSliders;
		IList<SpecificationAttribute> list = new List<SpecificationAttribute>();
		if (specificationAttributeSliders.Count > 0)
		{
			list = _specificationAttributeService.GetSpecificationAttributeByIds(specificationAttributeSliders.ToArray());
		}
		DataSourceResult data = new DataSourceResult
		{
			Data = list.Select((SpecificationAttribute x) => new
			{
				((BaseEntity)x).Id,
				x.Name
			}),
			Total = list.Count
		};
		return ((Controller)(object)this).Json((object?)data);
	}

	[HttpPost]
	public async Task<ActionResult> SpecificationAttributeSlidersDeleteAsync(DataSourceRequest command, int id)
	{
		if (id == 0)
		{
			return base.EmptyJson;
		}
		if (((ICollection<int>)_nopAjaxFiltersSettings.SpecificationAttributeSliders).Contains(id))
		{
			_nopAjaxFiltersSettings.SpecificationAttributeSliders.Remove(id);
			await _settingService.SaveSettingAsync<NopAjaxFiltersSettings>(_nopAjaxFiltersSettings, 0);
		}
		return GridList(command);
	}

	[HttpPost]
	public async Task<ActionResult> SpecificationAttributeSlidersCreateAsync(DataSourceRequest command, string name, int displayOrder)
	{
		if (!string.IsNullOrEmpty(name))
		{
			SpecificationAttribute specificationAttributeByName = _specificationAttributeService.GetSpecificationAttributeByName(name);
			if (specificationAttributeByName != null && !((ICollection<int>)_nopAjaxFiltersSettings.SpecificationAttributeSliders).Contains(((BaseEntity)specificationAttributeByName).Id))
			{
				_nopAjaxFiltersSettings.SpecificationAttributeSliders.Add(((BaseEntity)specificationAttributeByName).Id);
				await _settingService.SaveSettingAsync<NopAjaxFiltersSettings>(_nopAjaxFiltersSettings, 0);
			}
		}
		return GridList(command);
	}

	[HttpGet]
	public async Task<ActionResult> GetSpecifications()
	{
		IList<int> currentSpecifications = _nopAjaxFiltersSettings.SpecificationAttributeSliders;
		IList<SelectListItem> data = (from x in (IEnumerable<SpecificationAttribute>)(await _specificationAttributeService.GetSpecificationAttributesAsync(0, int.MaxValue))
			where !currentSpecifications.Contains(((BaseEntity)x).Id)
			select new SelectListItem
			{
				Value = ((BaseEntity)x).Id.ToString(),
				Text = x.Name
			}).ToList();
		return ((Controller)(object)this).Json((object?)data);
	}
}
