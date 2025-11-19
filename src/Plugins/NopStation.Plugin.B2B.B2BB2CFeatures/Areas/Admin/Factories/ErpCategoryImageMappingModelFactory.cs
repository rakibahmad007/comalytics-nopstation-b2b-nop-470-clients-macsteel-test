using System.Linq;
using System.Threading.Tasks;
using Nop.Data;
using Nop.Web.Areas.Admin.Models.Catalog;
using NopStation.Plugin.B2B.B2BB2CFeatures.Areas.Admin.Models;
using NopStation.Plugin.B2B.ERPIntegrationCore.Domain;

namespace NopStation.Plugin.B2B.B2BB2CFeatures.Areas.Admin.Factories;

public class ErpCategoryImageMappingModelFactory : IErpCategoryImageMappingModelFactory
{
    #region Fields

    private readonly IRepository<ErpCategoryImageShow> _categoryImageRepository;

    #endregion

    #region Ctor

    public ErpCategoryImageMappingModelFactory(IRepository<ErpCategoryImageShow> categoryImageRepository)
    {
        _categoryImageRepository = categoryImageRepository;
    }

    #endregion

    #region Methods

    public async Task<ErpCategoryImageMappingModel> PrepareB2BCategoryImageMappingModelAsync(CategoryModel categoryModel, int categoryId)
    {
        var showImage = await GetImageVisibilityStatusAsync(categoryId);

        var erpCategoryImageMappingModel = new ErpCategoryImageMappingModel();
        if (categoryModel != null)
        {
            erpCategoryImageMappingModel.IsShowImage = showImage;
            erpCategoryImageMappingModel.CategoryId = categoryId;
        }
        return erpCategoryImageMappingModel;
    }

    public async Task SetCategoryImageVisibilityAsync(int categoryId, bool showImage)
    {
        var categoryImage = await _categoryImageRepository.Table.FirstOrDefaultAsync(c => c.CategoryId == categoryId);
        if (categoryImage == null)
        {
            categoryImage = new ErpCategoryImageShow
            {
                CategoryId = categoryId,
                ShowImage = showImage
            };
            await _categoryImageRepository.InsertAsync(categoryImage);
        }
        else
        {
            categoryImage.ShowImage = showImage;
            await _categoryImageRepository.UpdateAsync(categoryImage);
        }
    }

    public async Task<bool> GetImageVisibilityStatusAsync(int categoryId)
    {
        var categoryImage = await _categoryImageRepository.Table.FirstOrDefaultAsync(c => c.CategoryId == categoryId);
        return categoryImage?.ShowImage ?? false;
    }

    #endregion
}