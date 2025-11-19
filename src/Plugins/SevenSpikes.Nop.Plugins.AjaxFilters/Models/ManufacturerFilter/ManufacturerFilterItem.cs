using SevenSpikes.Nop.Plugins.AjaxFilters.Domain.Enums;

namespace SevenSpikes.Nop.Plugins.AjaxFilters.Models.ManufacturerFilter;

public class ManufacturerFilterItem
{
	public int Id { get; set; }

	public string Name { get; set; }

	public FilterItemState FilterItemState { get; set; }
}
