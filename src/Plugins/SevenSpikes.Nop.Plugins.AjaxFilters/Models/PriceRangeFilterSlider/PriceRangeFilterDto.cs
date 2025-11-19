namespace SevenSpikes.Nop.Plugins.AjaxFilters.Models.PriceRangeFilterSlider;

public class PriceRangeFilterDto
{
	public decimal MinPrice { get; set; }

	public decimal MaxPrice { get; set; }

	public string Formatting { get; set; }

	public bool TaxDisplayTypeIncludingTax { get; set; }

	public bool TaxPriceIncludeTax { get; set; }

	public decimal TaxRatePercentage { get; set; }

	public decimal MaxDiscountAmount { get; set; }

	public decimal MaxDiscountPercentage { get; set; }
}
