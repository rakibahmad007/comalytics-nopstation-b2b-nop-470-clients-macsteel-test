using System.Collections.Generic;
using System.Threading.Tasks;
using SevenSpikes.Nop.Plugins.AjaxFilters.Models.AttributeFilter;

namespace SevenSpikes.Nop.Plugins.AjaxFilters.Services;

public interface IProductAttributeServiceAjaxFilters
{
	Task<IList<AttributeFilterItem>> GetSortedAttributeValuesBasedOnTheirPredefinedDisplayOrderAsync(IEnumerable<AttributeFilterItem> attributeValues);
}
