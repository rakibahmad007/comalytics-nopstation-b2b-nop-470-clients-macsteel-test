using SevenSpikes.Nop.Plugins.AjaxFilters.Domain.Enums;

namespace SevenSpikes.Nop.Plugins.AjaxFilters.Models.OnSaleFilter;

public class OnSaleFilterModel7Spikes
{
	public int Id { get; set; }

	public string Name { get; set; }

	public int CategoryId { get; set; }

	public int ManufacturerId { get; set; }

	public int VendorId { get; set; }

	public int Priority { get; set; }

	public FilterItemState FilterItemState { get; set; }
}
