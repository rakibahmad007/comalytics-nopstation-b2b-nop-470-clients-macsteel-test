using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Nop.Core.Infrastructure;
using SevenSpikes.Nop.Framework.ActionFilters;
using SevenSpikes.Nop.Framework.Routing;
using SevenSpikes.Nop.Framework.ViewLocations;
using SevenSpikes.Nop.Plugins.AjaxFilters.ActionFilters;
using SevenSpikes.Nop.Plugins.AjaxFilters.Helpers;
using SevenSpikes.Nop.Plugins.AjaxFilters.Services;

namespace SevenSpikes.Nop.Plugins.AjaxFilters.Infrastructure;

public class RouteProvider : BaseRouteProvider
{
	protected override string PluginSystemName => "SevenSpikes.Nop.Plugins.AjaxFilters";

	protected override void RegisterDuplicateControllers(IViewLocationsManager viewLocationsManager)
	{
		List<DuplicateControllerInfo> list = new List<DuplicateControllerInfo>();
		DuplicateControllerInfo item = new DuplicateControllerInfo
		{
			DuplicateControllerName = "Catalog7Spikes",
			DuplicateOfControllerName = "Catalog"
		};
		list.Add(item);
		viewLocationsManager.AddDuplicateControllers(list);
	}

	protected override void RegisterPluginActionFilters(IList<IFilterProvider> providers)
	{
		GeneralActionFilterProvider generalActionFilterProvider = new GeneralActionFilterProvider();
		generalActionFilterProvider.Add(new SearchActionFilterFactory());
		providers.Add(generalActionFilterProvider);
	}

	protected override async Task RegisterRoutesAccessibleByNameAsync(IEndpointRouteBuilder routes)
	{
		await base.RegisterRoutesAccessibleByNameAsync(routes);
		string text = await GetRouteLanguagePatternAsync(routes);
		routes.MapControllerRoute("FilterProductSearch", text + "filterSearch/", new
		{
			controller = "Catalog7Spikes",
			action = "AjaxFiltersSearch"
		});
		routes.MapControllerRoute("GetFilteredProducts", text + "getFilteredProducts/", new
		{
			controller = "Catalog7Spikes",
			action = "GetFilteredProducts"
		});
	}

	protected override async Task UpdateDatabaseAsync()
	{
		await EngineContext.Current.Resolve<IAjaxFiltersDatabaseService>((IServiceScope)null).UpdateDatabaseScriptsAsync();
	}

	protected override async Task SetNopcommerceSettingsAsync()
	{
		await FilterSettingHelper.UpdateNopCommerceFilterSettings();
	}

	protected override bool ShouldAddPluginViewLocationsBeforeNopViewLocations()
	{
		return true;
	}
}
