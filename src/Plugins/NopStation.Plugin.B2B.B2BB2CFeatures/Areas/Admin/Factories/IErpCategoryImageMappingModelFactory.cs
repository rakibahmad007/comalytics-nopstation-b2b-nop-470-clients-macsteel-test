using System.Threading.Tasks;
using Nop.Web.Areas.Admin.Models.Catalog;
using NopStation.Plugin.B2B.B2BB2CFeatures.Areas.Admin.Models;

namespace NopStation.Plugin.B2B.B2BB2CFeatures.Areas.Admin.Factories;

public interface IErpCategoryImageMappingModelFactory
{
    Task<ErpCategoryImageMappingModel> PrepareB2BCategoryImageMappingModelAsync(CategoryModel categoryModel, int categoryId);
    Task SetCategoryImageVisibilityAsync(int categoryId, bool showImage);
    Task<bool> GetImageVisibilityStatusAsync(int categoryId);
}
