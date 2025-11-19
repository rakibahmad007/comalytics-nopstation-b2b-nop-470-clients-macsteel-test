using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Nop.Core;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Stores;
using Nop.Core.Infrastructure;
using Nop.Services.Configuration;
using Nop.Services.Stores;
using SevenSpikes.Nop.Plugins.AjaxFilters.Domain;

namespace SevenSpikes.Nop.Plugins.AjaxFilters.Helpers;

public static class FilterSettingHelper
{
	private static ISettingService _settingService;

	private static ISettingService SettingService
	{
		get
		{
			if (_settingService == null)
			{
				_settingService = EngineContext.Current.Resolve<ISettingService>((IServiceScope)null);
			}
			return _settingService;
		}
	}

	public static async Task UpdateNopCommerceFilterSettings()
	{
		IList<Store> stores = await EngineContext.Current.Resolve<IStoreService>((IServiceScope)null).GetAllStoresAsync();
		await UpdateFilterSettingForStore(0);
		if (stores.Count <= 1 || !(await SettingService.GetSettingByKeyAsync<bool>("SevenSpikesCommonSettings.LoadStoreSettingsOnLoad", true, 0, false)))
		{
			return;
		}
		foreach (Store item in stores)
		{
			await UpdateFilterSettingForStore(((BaseEntity)item).Id);
		}
	}

	private static async Task UpdateFilterSettingForStore(int storeId)
	{
		CatalogSettings catalogSettings = await SettingService.LoadSettingAsync<CatalogSettings>(storeId);
		if (!(await SettingService.LoadSettingAsync<NopAjaxFiltersSettings>(storeId)).EnableAjaxFilters)
		{
			return;
		}
		bool flag = catalogSettings.EnableManufacturerFiltering;
		if (flag)
		{
			flag = await SettingService.SettingExistsAsync<CatalogSettings, bool>(catalogSettings, (Expression<Func<CatalogSettings, bool>>)((CatalogSettings x) => x.EnableManufacturerFiltering), storeId);
		}
		if (flag)
		{
			catalogSettings.EnableManufacturerFiltering = false;
			await SettingService.SaveSettingAsync<CatalogSettings, bool>(catalogSettings, (Expression<Func<CatalogSettings, bool>>)((CatalogSettings x) => x.EnableManufacturerFiltering), storeId, true);
		}
		flag = catalogSettings.EnableSpecificationAttributeFiltering;
		if (flag)
		{
			flag = await SettingService.SettingExistsAsync<CatalogSettings, bool>(catalogSettings, (Expression<Func<CatalogSettings, bool>>)((CatalogSettings x) => x.EnableSpecificationAttributeFiltering), storeId);
		}
		if (flag)
		{
			catalogSettings.EnableSpecificationAttributeFiltering = false;
			await SettingService.SaveSettingAsync<CatalogSettings, bool>(catalogSettings, (Expression<Func<CatalogSettings, bool>>)((CatalogSettings x) => x.EnableSpecificationAttributeFiltering), storeId, true);
		}
		flag = catalogSettings.EnablePriceRangeFiltering;
		if (flag)
		{
			flag = await SettingService.SettingExistsAsync<CatalogSettings, bool>(catalogSettings, (Expression<Func<CatalogSettings, bool>>)((CatalogSettings x) => x.EnablePriceRangeFiltering), storeId);
		}
		if (flag)
		{
			catalogSettings.EnablePriceRangeFiltering = false;
			await SettingService.SaveSettingAsync<CatalogSettings, bool>(catalogSettings, (Expression<Func<CatalogSettings, bool>>)((CatalogSettings x) => x.EnablePriceRangeFiltering), storeId, true);
		}
	}
}
