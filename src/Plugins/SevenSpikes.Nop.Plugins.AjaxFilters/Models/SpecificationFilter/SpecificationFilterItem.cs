using SevenSpikes.Nop.Plugins.AjaxFilters.Domain.Enums;

namespace SevenSpikes.Nop.Plugins.AjaxFilters.Models.SpecificationFilter;

public class SpecificationFilterItem
{
	public int Id { get; set; }

	public string Name { get; set; }

	public int DisplayOrder { get; set; }

	public FilterItemState FilterItemState { get; set; }

	public string ColorSquaresRgb { get; set; }
}
