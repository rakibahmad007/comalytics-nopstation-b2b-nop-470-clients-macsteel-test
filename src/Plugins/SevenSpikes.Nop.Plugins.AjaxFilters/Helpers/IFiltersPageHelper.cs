using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Web.Models.Catalog;

namespace SevenSpikes.Nop.Plugins.AjaxFilters.Helpers;

public interface IFiltersPageHelper
{
	FiltersPageParameters GetFiltersPageParameters();

	Task<bool> ValidateParametersAsync(FiltersPageParameters filtersPageParameters);

	string[] GetPageSizes();

	Task AdjustPagingFilteringModelPageSizeAndPageNumberAsync(CatalogProductsCommand catalogPagingFilteringModel);

	Task<string> GetTemplateViewPathAsync();

	Task InitializeAsync(ActionContext requestContext, string query);

	Task InitializeAsync(ActionContext requestContext);

	Task InitializeAsync(FiltersPageParameters filtersPageParameters);

	int GetDefaultPageSize();

	int GetDefaultOrderBy();

	int GetDefaultPageNumber();

	string GetDefaultViewMode();
}
