using Microsoft.AspNetCore.Mvc.Filters;
using SevenSpikes.Nop.Framework.ActionFilters;

namespace SevenSpikes.Nop.Plugins.AjaxFilters.ActionFilters;

public class SearchActionFilterFactory : IControllerActionFilterFactory
{
	public string ControllerName => "Catalog";

	public string ActionName => "Search";

	public ActionFilterAttribute GetActionFilterAttribute()
	{
		return new SearchActionFilterAttribute();
	}
}
