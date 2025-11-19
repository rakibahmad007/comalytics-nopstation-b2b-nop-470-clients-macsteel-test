using System.Threading.Tasks;
using NopStation.Plugin.B2B.B2BB2CFeatures.Areas.Admin.Models.SpecialIncludeExcludes;

namespace NopStation.Plugin.B2B.B2BB2CFeatures.Areas.Admin.Factories
{
    public interface ISpecialIncludeExcludeModelFactory
    {
        Task<SpecialIncludeExcludeSearchModel> PrepareSpecialIncludeExcludeSearchModelAsync(
            SpecialIncludeExcludeSearchModel model
        );
        Task<SpecialIncludeExcludeListModel> PrepareSpecialIncludeExcludeListModelAsync(
            SpecialIncludeExcludeSearchModel model
        );
        Task<SpecialIncludeExcludeModel> PrepareSpecialIncludeExcludeModelAsync(
            SpecialIncludeExcludeModel model
        );
    }
}
