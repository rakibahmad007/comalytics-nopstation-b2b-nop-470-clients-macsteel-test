using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Core.Caching;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Discounts;
using Nop.Data;
using Nop.Services.Catalog;
using Nop.Services.Customers;
using Nop.Services.Localization;
using Nop.Services.Security;
using Nop.Services.Stores;

namespace NopStation.Plugin.B2B.B2BB2CFeatures.Services.Overriden;

/// <summary>
/// Category service
/// </summary>
public partial class OverridenCategoryService : CategoryService
{
    private readonly IShortTermCacheManager _shortTermCacheManager;

    #region Ctor

    public OverridenCategoryService(
        IAclService aclService,
        ICustomerService customerService,
        ILocalizationService localizationService,
        IRepository<Category> categoryRepository,
        IRepository<DiscountCategoryMapping> discountCategoryMappingRepository,
        IRepository<Product> productRepository,
        IRepository<ProductCategory> productCategoryRepository,
        IStaticCacheManager staticCacheManager,
        IStoreContext storeContext,
        IStoreMappingService storeMappingService,
        IWorkContext workContext,
        IShortTermCacheManager shortTermCacheManager
    )
        : base(
            aclService,
            customerService,
            localizationService,
            categoryRepository,
            discountCategoryMappingRepository,
            productRepository,
            productCategoryRepository,
            staticCacheManager,
            storeContext,
            storeMappingService,
            workContext
        )
    {
        _shortTermCacheManager = shortTermCacheManager;
    }

    #endregion

    protected override async Task<IList<ProductCategory>> GetProductCategoriesByProductIdAsync(
        int productId,
        int storeId,
        bool showHidden = false
    )
    {
        if (productId == 0)
            return new List<ProductCategory>();

        var customer = await _workContext.GetCurrentCustomerAsync();
        var customerRoleIds = await _customerService.GetCustomerRoleIdsAsync(customer);

        return await _shortTermCacheManager.GetAsync(
            async () =>
            {
                var query = _productCategoryRepository.Table;
                if (!showHidden)
                {
                    var categoriesQuery = _categoryRepository.Table.Where(c => c.Published);

                    //apply store mapping constraints
                    categoriesQuery = await _storeMappingService.ApplyStoreMapping(
                        categoriesQuery,
                        storeId
                    );

                    //apply ACL constraints
                    categoriesQuery = await _aclService.ApplyAcl(categoriesQuery, customerRoleIds);

                    query = query.Where(pc =>
                        categoriesQuery.Any(c => !c.Deleted && c.Id == pc.CategoryId)
                    );
                }

                return await query
                    .Where(pc => pc.ProductId == productId)
                    .OrderBy(pc => pc.DisplayOrder)
                    .ThenBy(pc => pc.Id)
                    .ToListAsync();
            },
            NopCatalogDefaults.ProductCategoriesByProductCacheKey,
            productId,
            showHidden,
            customerRoleIds,
            storeId
        );
    }
}
