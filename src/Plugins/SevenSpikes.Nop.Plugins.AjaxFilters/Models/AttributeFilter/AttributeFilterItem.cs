using System.Collections.Generic;
using SevenSpikes.Nop.Plugins.AjaxFilters.Domain.Enums;

namespace SevenSpikes.Nop.Plugins.AjaxFilters.Models.AttributeFilter;

public class AttributeFilterItem
{
	public int ValueId { get; set; }

	public string Name { get; set; }

	public int AttributeId { get; set; }

	public IList<int> ProductVariantAttributeIds { get; set; }

	public FilterItemState FilterItemState { get; set; }

	public string ColorSquaresRgb { get; set; }

	public string ImageSquaresUrl { get; set; }

	public AttributeFilterItem()
	{
		ProductVariantAttributeIds = new List<int>();
	}
}
