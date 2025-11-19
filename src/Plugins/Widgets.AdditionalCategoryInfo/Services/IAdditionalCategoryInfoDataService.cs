using System.Threading.Tasks;
using Nop.Plugin.Widgets.AdditionalCategoryInfo.Domain;

namespace Nop.Plugin.Widgets.AdditionalCategoryInfo.Services;

public interface IAdditionalCategoryInfoDataService
{
    Task CreateAsync(AdditionalCategoryInfoData record);

    Task<AdditionalCategoryInfoData> GetAdditionalCategoryInfoByCategoryIdAsync(int id);

    Task UpdateAsync(AdditionalCategoryInfoData record);

    Task<bool> IsPluginActiveAsync();
}
