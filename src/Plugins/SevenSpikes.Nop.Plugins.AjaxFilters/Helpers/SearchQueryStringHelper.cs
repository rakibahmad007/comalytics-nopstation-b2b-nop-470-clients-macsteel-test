using System.Collections.Generic;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Primitives;
using Nop.Web.Framework.UI.Paging;
using Nop.Web.Models.Catalog;

namespace SevenSpikes.Nop.Plugins.AjaxFilters.Helpers;

public class SearchQueryStringHelper : ISearchQueryStringHelper
{
	public SearchQueryStringParameters GetQueryStringParameters(string queryString)
	{
		if (string.IsNullOrEmpty(queryString))
		{
			return new SearchQueryStringParameters();
		}
		SearchQueryStringParameters searchQueryStringParameters = new SearchQueryStringParameters();
		Dictionary<string, StringValues> dictionary = QueryHelpers.ParseQuery(queryString);
		if (!dictionary.ContainsKey("q"))
		{
			return searchQueryStringParameters;
		}
		searchQueryStringParameters.IsOnSearchPage = true;
		string text = dictionary["q"];
		if (!string.IsNullOrEmpty(text))
		{
			searchQueryStringParameters.Keyword = text.Trim();
		}
		if (dictionary.ContainsKey("advs"))
		{
			string text2 = dictionary["advs"];
			if (!string.IsNullOrEmpty(text2) && bool.TryParse(text2.Split(',')[0], out var result))
			{
				searchQueryStringParameters.AdvancedSearch = result;
			}
		}
		if (searchQueryStringParameters.AdvancedSearch)
		{
			if (dictionary.ContainsKey("Sid"))
			{
				string text3 = dictionary["Sid"];
				if (!string.IsNullOrEmpty(text3) && bool.TryParse(text3.Split(',')[0], out var result2))
				{
					searchQueryStringParameters.SearchInProductDescriptions = result2;
				}
			}
			if (dictionary.ContainsKey("Isc"))
			{
				string text4 = dictionary["Isc"];
				if (!string.IsNullOrEmpty(text4) && bool.TryParse(text4.Split(',')[0], out var result3))
				{
					searchQueryStringParameters.IncludeSubcategories = result3;
				}
			}
			if (dictionary.ContainsKey("Cid"))
			{
				string text5 = dictionary["Cid"];
				if (!string.IsNullOrEmpty(text5) && int.TryParse(text5, out var result4))
				{
					searchQueryStringParameters.SearchCategoryId = result4;
				}
			}
			if (dictionary.ContainsKey("Mid"))
			{
				string text6 = dictionary["Mid"];
				if (!string.IsNullOrEmpty(text6) && int.TryParse(text6, out var result5))
				{
					searchQueryStringParameters.SearchManufacturerId = result5;
				}
			}
			if (dictionary.ContainsKey("Vid"))
			{
				string text7 = dictionary["Vid"];
				if (!string.IsNullOrEmpty(text7) && int.TryParse(text7, out var result6))
				{
					searchQueryStringParameters.SearchVendorId = result6;
				}
			}
			if (dictionary.ContainsKey("Pf"))
			{
				string text8 = dictionary["Pf"];
				if (!string.IsNullOrEmpty(text8))
				{
					searchQueryStringParameters.PriceFrom = (decimal.TryParse(text8, out var result7) ? new decimal?(result7) : null);
				}
			}
			if (dictionary.ContainsKey("Pt"))
			{
				string text9 = dictionary["Pt"];
				if (!string.IsNullOrEmpty(text9))
				{
					searchQueryStringParameters.PriceTo = (decimal.TryParse(text9, out var result8) ? new decimal?(result8) : null);
				}
			}
		}
		return searchQueryStringParameters;
	}

	public RouteValueDictionary PrepareSearchRouteValues(SearchModel model, CatalogProductsCommand command)
	{
		return new RouteValueDictionary(new
		{
			model.q,
			model.advs,
			model.cid,
			model.sid,
			model.isc,
			model.mid,
			model.CatalogProductsModel.PriceRangeFilter.SelectedPriceRange.From,
			model.CatalogProductsModel.PriceRangeFilter.SelectedPriceRange.To,
			((BasePageableModel)command).PageIndex,
			((BasePageableModel)command).PageNumber,
			((BasePageableModel)command).PageSize
		});
	}
}
