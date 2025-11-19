using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Nop.Core;
using Nop.Core.Domain.Cms;
using Nop.Core.Infrastructure;
using Nop.Data;
using Nop.Services.Configuration;
using SevenSpikes.Nop.Framework.Plugin;
using SevenSpikes.Nop.Plugins.AjaxFilters.Components;
using SevenSpikes.Nop.Plugins.AjaxFilters.Domain;
using SevenSpikes.Nop.Plugins.AjaxFilters.Infrastructure.Constants;
using SevenSpikes.Nop.Plugins.AjaxFilters.Services;

namespace SevenSpikes.Nop.Plugins.AjaxFilters.Infrastructure;

public class AjaxFiltersPlugin : BaseAdminWidgetPlugin7Spikes
{
	private readonly WidgetSettings _widgetSettings;

	private readonly ISettingService _settingService;

	private readonly IAjaxFiltersDatabaseService _ajaxFiltersDatabaseService;

	private readonly INopDataProvider _nopDataProvider;

	public AjaxFiltersPlugin(WidgetSettings widgetSettings, ISettingService settingService, IAjaxFiltersDatabaseService ajaxFiltersDatabaseService, INopDataProvider nopDataProvider)
		: base(Plugin.MenuItems, "SevenSpikes.Plugins.AjaxFilters.Admin.Menu.MenuName", "SevenSpikes.Nop.Plugins.AjaxFilters", Plugin.IsTrialVersion, "http://www.nop-templates.com/ajax-filters-plugin-for-nopcommerce")
	{
		_widgetSettings = widgetSettings;
		_settingService = settingService;
		_ajaxFiltersDatabaseService = ajaxFiltersDatabaseService;
		_nopDataProvider = nopDataProvider;
	}

	public override string GetConfigurationPageUrl()
	{
		return base.StoreLocation + "Admin/NopAjaxFiltersAdmin/Settings";
	}

	protected override async Task InstallAdditionalSettingsAsync()
	{
		if (!_widgetSettings.ActiveWidgetSystemNames.Contains("SevenSpikes.Nop.Plugins.AjaxFilters"))
		{
			_widgetSettings.ActiveWidgetSystemNames.Add("SevenSpikes.Nop.Plugins.AjaxFilters");
			await _settingService.SaveSettingAsync<WidgetSettings>(_widgetSettings, 0);
		}
		await _ajaxFiltersDatabaseService.RemoveDatabaseScriptsAsync();
		await _ajaxFiltersDatabaseService.CreateDatabaseScriptsAsync();
	}

	protected override async Task UninstallAdditionalSettingsAsync()
	{
		NopAjaxFiltersSettings nopAjaxFiltersSettings = EngineContext.Current.Resolve<NopAjaxFiltersSettings>((IServiceScope)null);
		nopAjaxFiltersSettings.WidgetZone = string.Empty;
		await _settingService.SaveSettingAsync<NopAjaxFiltersSettings>(nopAjaxFiltersSettings, 0);
		await _ajaxFiltersDatabaseService.RemoveDatabaseScriptsAsync();
	}

	public override async Task InstallAsync()
	{
		if ((int)DataSettingsManager.LoadSettings((INopFileProvider)null, false).DataProvider == 3)
		{
			throw new NopException("There is no PostgreSQL support in the Ajax Filters plugin");
		}
		await base.InstallAsync();
	}

	public override Type GetWidgetViewComponent(string widgetZone)
	{
		return typeof(NopAjaxFiltersComponent);
	}
}
