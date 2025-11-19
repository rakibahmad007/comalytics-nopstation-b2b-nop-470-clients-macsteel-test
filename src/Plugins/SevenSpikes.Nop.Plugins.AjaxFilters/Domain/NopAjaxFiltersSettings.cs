using System.Collections.Generic;
using Nop.Core.Configuration;
using SevenSpikes.Nop.Plugins.AjaxFilters.Domain.Enums;

namespace SevenSpikes.Nop.Plugins.AjaxFilters.Domain;

public class NopAjaxFiltersSettings : ISettings
{
	public bool EnableAjaxFilters { get; set; }

	public FiltersUIMode FiltersUIMode { get; set; }

	public bool EnableInfiniteScroll { get; set; }

	public bool ScrollToElementOnThePageAfterFiltration { get; set; }

	public bool ScrollToElementOnThePageAfterFiltrationMobile { get; set; }

	public string ElementToScrollAfterFiltrationSelector { get; set; }

	public string WidgetZone { get; set; }

	public bool EnablePriceRangeFilter { get; set; }

	public bool ClosePriceRangeFilterBox { get; set; }

	public bool EnableSpecificationsFilter { get; set; }

	public bool CloseSpecificationsFilterBox { get; set; }

	public int NumberOfSpecificationFilters { get; set; }

	public bool EnableAttributesFilter { get; set; }

	public bool CloseAttributesFilterBox { get; set; }

	public int NumberOfAttributeFilters { get; set; }

	public bool EnableManufacturersFilter { get; set; }

	public bool CloseManufacturersFilterBox { get; set; }

	public bool EnableOnSaleFilter { get; set; }

	public bool CloseOnSaleFilterBox { get; set; }

	public bool EnableVendorsFilter { get; set; }

	public bool CloseVendorsFilterBox { get; set; }

	public bool EnableInStockFilter { get; set; }

	public bool CloseInStockFilterBox { get; set; }

	public string ProductsListPanelSelector { get; set; }

	public string CategoriesWithoutFilters { get; set; }

	public string ProductsGridPanelSelector { get; set; }

	public string PagerPanelSelector { get; set; }

	public string PagerPanelIntegrationSelector { get; set; }

	public string SortOptionsDropDownSelector { get; set; }

	public string ViewOptionsDropDownSelector { get; set; }

	public string ProductPageSizeDropDownSelector { get; set; }

	public bool DisableDropdownNavigationWithAjaxWhenThereAreNoFilters { get; set; }

	public bool ShowFiltersOnCategoryPage { get; set; }

	public bool ShowFiltersOnManufacturerPage { get; set; }

	public bool ShowFiltersOnVendorPage { get; set; }

	public bool ShowFiltersOnSearchPage { get; set; }

	public bool ShowSelectedFiltersPanel { get; set; }

	public bool ShowNumberOfReturnedProducts { get; set; }

	public string ShowNumberOfReturnedProductsSelector { get; set; }

	public string EncryptedDatabaseHashKey { get; set; }

	public bool DropSortFunctionDuringUpdate { get; set; }

	public string TrailingZeroesSeparator { get; set; }

	public bool PrepareSpecificationAttributes { get; set; }

	public bool SearchInProductTags { get; set; }

	public bool CloseFiltersPanelAfterFiltrationInMobile { get; set; }

	public int ScrolltoElementAfterFiltrationAdditionalOffset { get; set; }

	public List<string> ActiveProductAttributes { get; set; }

	public List<int> SpecificationAttributeSliders { get; set; }

	public NopAjaxFiltersSettings()
	{
		ActiveProductAttributes = new List<string>();
		SpecificationAttributeSliders = new List<int>();
	}
}
