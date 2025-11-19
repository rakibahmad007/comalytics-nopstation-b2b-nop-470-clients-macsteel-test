using System;
using System.Linq;
using System.Threading.Tasks;
using Nop.Data;
using Nop.Plugin.Widgets.AdditionalCategoryInfo.Domain;
using Nop.Services.Cms;
using Nop.Services.Plugins;

namespace Nop.Plugin.Widgets.AdditionalCategoryInfo.Services;

public class AdditionalCategoryInfoDataService : IAdditionalCategoryInfoDataService
{
    private readonly IRepository<AdditionalCategoryInfoData> _repository;

    private readonly IPluginService _pluginService;

    private readonly IWidgetPluginManager _widgetPluginManager;

    public AdditionalCategoryInfoDataService(
        IRepository<AdditionalCategoryInfoData> repository,
        IPluginService pluginService,
        IWidgetPluginManager widgetPluginManager
    )
    {
        _repository = repository;
        _pluginService = pluginService;
        _widgetPluginManager = widgetPluginManager;
    }

    public async Task CreateAsync(AdditionalCategoryInfoData record)
    {
        ArgumentNullException.ThrowIfNull(record);
        await _repository.InsertAsync(record);
    }

    public async Task<AdditionalCategoryInfoData> GetAdditionalCategoryInfoByCategoryIdAsync(
        int categoryId
    )
    {
        return await _repository
            .Table.Where((x) => x.CategoryId == categoryId)
            .FirstOrDefaultAsync();
    }

    public async Task UpdateAsync(AdditionalCategoryInfoData record)
    {
        await _repository.UpdateAsync(record);
    }

    public async Task<bool> IsPluginActiveAsync()
    {
        IWidgetPlugin pluginInstance = (
            await _pluginService.GetPluginDescriptorBySystemNameAsync<IWidgetPlugin>(
                "Widgets.AdditionalCategoryInfo",
                LoadPluginsMode.All,
                null,
                0,
                null
            )
        ).Instance<IWidgetPlugin>();
        return _widgetPluginManager.IsPluginActive(pluginInstance);
    }
}
