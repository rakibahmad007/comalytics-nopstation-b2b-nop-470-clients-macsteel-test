using System.Collections.Generic;
using System.Threading.Tasks;
using Nop.Core.Domain.Catalog;
using SevenSpikes.Nop.Services.Catalog.DTO;

namespace SevenSpikes.Nop.Plugins.AjaxFilters.Services;

public interface IProductServiceNopAjaxFilters
{
	Task<ProductsResultDataDTO> SearchProductsAsync(IList<int> categoryIds, SpecificationFilterModelDTO specifiationFilterModelDTO, AttributeFilterModelDTO attributeFilterModelDTO, ManufacturerFilterModelDTO manufacturerFilterModelDTO, VendorFilterModelDTO vendorFilterModelDTO, int pageIndex = 0, int pageSize = int.MaxValue, int manufacturerId = 0, int vendorId = 0, int storeId = 0, bool? featuredProducts = null, decimal? priceMin = null, decimal? priceMax = null, int productTagId = 0, string keywords = null, bool searchDescriptions = false, bool searchSku = false, bool searchProductTags = false, int languageId = 0, ProductSortingEnum orderBy = 0, bool showHidden = false, bool onSale = false, bool inStock = false);

	Task<IList<int>> GetAllGroupProductIdsInCategoriesAsync(List<int> categoriesIds);

	Task<IList<int>> GetAllGroupProductIdsInCategoryAsync(int categoryId);

	Task<IList<int>> GetAllGroupProductIdsInManufacturerAsync(int manufacturerId);

	Task<IList<int>> GetAllGroupProductIdsInVendorAsync(int vendorId);

	Task<bool> HasProductsOnSaleAsync(int categoryId, int manufacturerId, int vendorId);

	Task<bool> HasProductsInStockAsync(int categoryId, int manufacturerId, int vendorId);
}
