using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Nop.Core.Domain.Cms;
using Nop.Core.Domain.ScheduleTasks;
using Nop.Plugin.Widgets.AdditionalCategoryInfo.Areas.Admin.Components;
using Nop.Services.Cms;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Plugins;
using Nop.Web.Framework.Infrastructure;

namespace Nop.Plugin.Widgets.AdditionalCategoryInfo;

public class AdditionalCategoryInfoPlugin : BasePlugin, IWidgetPlugin, IPlugin
{
    #region Fields

    private readonly ILocalizationService _localizationService;

    private readonly ISettingService _settingService;
    public bool HideInWidgetList => false;

    #endregion

    #region Ctor
    public AdditionalCategoryInfoPlugin(
        ILocalizationService localizationService,
        ISettingService settingService
    )
    {
        _localizationService = localizationService;
        _settingService = settingService;
    }

    #endregion

    #region Methods
    public override async Task InstallAsync()
    {
        AdditionalCategoryInfoSettings settings = new AdditionalCategoryInfoSettings();
        await _settingService.SaveSettingAsync<AdditionalCategoryInfoSettings>(settings, 0);
        await _localizationService.AddOrUpdateLocaleResourceAsync(
            "Plugins.Widgets.AdditionalCategoryInfo.Fields.Active",
            "Active",
            null
        );
        await _localizationService.AddOrUpdateLocaleResourceAsync(
            "Plugins.Widgets.AdditionalCategoryInfo.Fields.AdditionalInfoField",
            "Additional Info Field",
            null
        );
        await _localizationService.AddOrUpdateLocaleResourceAsync(
            "Plugins.Widgets.AdditionalCategoryInfo.Title",
            "Additional Category Info",
            null
        );
        await _localizationService.AddOrUpdateLocaleResourceAsync(
            "Plugins.Widgets.AdditionalCategoryInfo.UpdatedSuccess",
            "Successfully Updated",
            null
        );
        await base.InstallAsync();
    }

    public override async Task UninstallAsync()
    {
        await _settingService.DeleteSettingAsync<AdditionalCategoryInfoSettings>();
        await _localizationService.DeleteLocaleResourceAsync(
            "Plugins.Widgets.AdditionalCategoryInfo.Fields.Active"
        );
        await _localizationService.DeleteLocaleResourceAsync(
            "Plugins.Widgets.AdditionalCategoryInfo.Fields.AdditionalInfoField"
        );
        await _localizationService.DeleteLocaleResourceAsync(
            "Plugins.Widgets.AdditionalCategoryInfo.Title"
        );
        await _localizationService.DeleteLocaleResourceAsync(
            "Plugins.Widgets.AdditionalCategoryInfo.UpdatedSuccess"
        );
        await base.UninstallAsync();
    }

    public IList<string> GetWidgetZones()
    {
        return new List<string> { AdminWidgetZones.CategoryDetailsBlock };
    }

    public Task<IList<string>> GetWidgetZonesAsync()
    {
        return Task.FromResult<IList<string>>(
            new List<string> { AdminWidgetZones.CategoryDetailsBlock }
        );
    }

    public Type GetWidgetViewComponent(string widgetZone)
    {
        return typeof(AdditionalCategoryInfoViewComponent);
    }

    #endregion
}