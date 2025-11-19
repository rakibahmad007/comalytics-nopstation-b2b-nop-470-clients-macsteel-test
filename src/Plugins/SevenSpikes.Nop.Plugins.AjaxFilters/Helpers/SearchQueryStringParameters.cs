namespace SevenSpikes.Nop.Plugins.AjaxFilters.Helpers;

public class SearchQueryStringParameters
{
	public bool IsOnSearchPage { get; set; }

	public int SearchCategoryId { get; set; }

	public int SearchManufacturerId { get; set; }

	public int SearchVendorId { get; set; }

	public string Keyword { get; set; }

	public decimal? PriceFrom { get; set; }

	public decimal? PriceTo { get; set; }

	public bool IncludeSubcategories { get; set; }

	public bool SearchInProductDescriptions { get; set; }

	public bool AdvancedSearch { get; set; }
}
