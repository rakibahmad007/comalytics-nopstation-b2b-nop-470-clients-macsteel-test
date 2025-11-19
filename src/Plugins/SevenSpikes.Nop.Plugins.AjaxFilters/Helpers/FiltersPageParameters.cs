namespace SevenSpikes.Nop.Plugins.AjaxFilters.Helpers;

public class FiltersPageParameters
{
	public int CategoryId { get; set; }

	public int ManufacturerId { get; set; }

	public int VendorId { get; set; }

	public int? OrderBy { get; set; }

	public string ViewMode { get; set; }

	public int PageSize { get; set; }

	public int PageNumber { get; set; }

	public SearchQueryStringParameters SearchQueryStringParameters { get; set; }

	public FiltersPageParameters()
	{
		SearchQueryStringParameters = new SearchQueryStringParameters();
	}
}
