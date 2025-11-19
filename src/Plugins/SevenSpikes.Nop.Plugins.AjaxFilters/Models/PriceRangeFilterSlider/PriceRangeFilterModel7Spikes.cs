using Nop.Web.Models.Catalog;

namespace SevenSpikes.Nop.Plugins.AjaxFilters.Models.PriceRangeFilterSlider;

public class PriceRangeFilterModel7Spikes
{
	public int CategoryId { get; set; }

	public int ManufacturerId { get; set; }

	public int VendorId { get; set; }

	public int Priority { get; set; }

	public PriceRangeModel SelectedPriceRange { get; set; }

	public decimal MinPrice { get; set; }

	public decimal MaxPrice { get; set; }

	public string CurrencySymbol { get; set; }

	public string Formatting { get; set; }

	public string MinPriceFormatted { get; set; }

	public string MaxPriceFormatted { get; set; }
}
