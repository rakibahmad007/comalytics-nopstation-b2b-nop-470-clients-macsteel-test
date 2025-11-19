using SevenSpikes.Nop.Plugins.AjaxFilters.Models.AttributeFilter;
using SevenSpikes.Nop.Plugins.AjaxFilters.Models.InStockFilter;
using SevenSpikes.Nop.Plugins.AjaxFilters.Models.ManufacturerFilter;
using SevenSpikes.Nop.Plugins.AjaxFilters.Models.OnSaleFilter;
using SevenSpikes.Nop.Plugins.AjaxFilters.Models.PriceRangeFilterSlider;
using SevenSpikes.Nop.Plugins.AjaxFilters.Models.SpecificationFilter;
using SevenSpikes.Nop.Plugins.AjaxFilters.Models.VendorFilter;

namespace SevenSpikes.Nop.Plugins.AjaxFilters.Models;

public class GetFilteredProductsModel
{
	public int CategoryId { get; set; }

	public int ManufacturerId { get; set; }

	public int VendorId { get; set; }

	public PriceRangeFilterModel7Spikes PriceRangeFilterModel7Spikes { get; set; }

	public SpecificationFilterModel7Spikes SpecificationFiltersModel7Spikes { get; set; }

	public AttributeFilterModel7Spikes AttributeFiltersModel7Spikes { get; set; }

	public ManufacturerFilterModel7Spikes ManufacturerFiltersModel7Spikes { get; set; }

	public VendorFilterModel7Spikes VendorFiltersModel7Spikes { get; set; }

	public OnSaleFilterModel7Spikes OnSaleFilterModel { get; set; }

	public InStockFilterModel7Spikes InStockFilterModel { get; set; }

	public string QueryString { get; set; }

	public bool ShouldNotStartFromFirstPage { get; set; }

	public string Keyword { get; set; }

	public int SearchCategoryId { get; set; }

	public int SearchManufacturerId { get; set; }

	public int SearchVendorId { get; set; }

	public decimal? PriceFrom { get; set; }

	public decimal? PriceTo { get; set; }

	public bool IncludeSubcategories { get; set; }

	public bool SearchInProductDescriptions { get; set; }

	public bool AdvancedSearch { get; set; }

	public bool IsOnSearchPage { get; set; }

	public int? Orderby { get; set; }

	public string Viewmode { get; set; }

	public int? PageNumber { get; set; }

	public int Pagesize { get; set; }
}
