using System.Collections.Generic;

namespace SevenSpikes.Nop.Plugins.AjaxFilters.Models.AttributeFilter;

public class AttributeFilterModel7Spikes
{
	public int CategoryId { get; set; }

	public int ManufacturerId { get; set; }

	public int VendorId { get; set; }

	public int Priority { get; set; }

	public IList<AttributeFilterGroup> AttributeFilterGroups { get; set; }

	public AttributeFilterModel7Spikes()
	{
		AttributeFilterGroups = new List<AttributeFilterGroup>();
	}
}
