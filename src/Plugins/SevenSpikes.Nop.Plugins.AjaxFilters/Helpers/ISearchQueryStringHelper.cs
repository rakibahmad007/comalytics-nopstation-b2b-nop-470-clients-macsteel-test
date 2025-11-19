using Microsoft.AspNetCore.Routing;
using Nop.Web.Models.Catalog;

namespace SevenSpikes.Nop.Plugins.AjaxFilters.Helpers;

public interface ISearchQueryStringHelper
{
	SearchQueryStringParameters GetQueryStringParameters(string queryString);

	RouteValueDictionary PrepareSearchRouteValues(SearchModel model, CatalogProductsCommand command);
}
