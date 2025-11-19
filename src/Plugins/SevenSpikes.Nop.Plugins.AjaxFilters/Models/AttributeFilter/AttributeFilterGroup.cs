using System.Collections.Generic;

namespace SevenSpikes.Nop.Plugins.AjaxFilters.Models.AttributeFilter;

public class AttributeFilterGroup
{
	public int Id { get; set; }

	public string Name { get; set; }

	public bool IsMain { get; set; }

	public IList<AttributeFilterItem> FilterItems { get; set; }

	public AttributeFilterGroup()
	{
		FilterItems = new List<AttributeFilterItem>();
	}
}
