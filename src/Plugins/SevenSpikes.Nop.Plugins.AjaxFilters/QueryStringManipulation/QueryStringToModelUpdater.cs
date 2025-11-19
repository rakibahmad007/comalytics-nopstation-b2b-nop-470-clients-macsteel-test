using System;
using System.Linq;
using System.Text.RegularExpressions;
using Nop.Web.Framework.UI.Paging;
using Nop.Web.Models.Catalog;
using SevenSpikes.Nop.Plugins.AjaxFilters.Domain.Enums;
using SevenSpikes.Nop.Plugins.AjaxFilters.Helpers;
using SevenSpikes.Nop.Plugins.AjaxFilters.Models.AttributeFilter;
using SevenSpikes.Nop.Plugins.AjaxFilters.Models.InStockFilter;
using SevenSpikes.Nop.Plugins.AjaxFilters.Models.ManufacturerFilter;
using SevenSpikes.Nop.Plugins.AjaxFilters.Models.OnSaleFilter;
using SevenSpikes.Nop.Plugins.AjaxFilters.Models.PriceRangeFilterSlider;
using SevenSpikes.Nop.Plugins.AjaxFilters.Models.SpecificationFilter;
using SevenSpikes.Nop.Plugins.AjaxFilters.Models.VendorFilter;

namespace SevenSpikes.Nop.Plugins.AjaxFilters.QueryStringManipulation;

public class QueryStringToModelUpdater : IQueryStringToModelUpdater
{
	private readonly IFiltersPageHelper _filtersPageHelper;

	public QueryStringToModelUpdater(IFiltersPageHelper filtersPageHelper)
	{
		_filtersPageHelper = filtersPageHelper;
	}

	public void UpdateModelsFromQueryString(string queryString, SpecificationFilterModel7Spikes specificationFiltersModel7Spikes, AttributeFilterModel7Spikes attributeFilterModel7Spikes, ManufacturerFilterModel7Spikes manufacturerFilterModel7Spikes, VendorFilterModel7Spikes vendorFilterModel7Spikes, PriceRangeFilterModel7Spikes priceRangeFilterModel7Spikes, CatalogProductsCommand pagingFilteringModel, OnSaleFilterModel7Spikes onSaleFilterModel, InStockFilterModel7Spikes inStockFilterModel7Spikes)
	{
		UpdateModelsFromQueryStringInternal(queryString, specificationFiltersModel7Spikes, attributeFilterModel7Spikes, manufacturerFilterModel7Spikes, vendorFilterModel7Spikes, priceRangeFilterModel7Spikes, pagingFilteringModel, onSaleFilterModel, inStockFilterModel7Spikes);
	}

	public void UpdateOnSaleModel(string queryStringParameter, OnSaleFilterModel7Spikes onSaleFilterModel7Spikes)
	{
		UpdateOnSaleModelInternal(queryStringParameter, onSaleFilterModel7Spikes);
	}

	public void UpdateInStockModel(string queryStringParameter, InStockFilterModel7Spikes inStockFilterModel7Spikes)
	{
		UpdateInStockModelInternal(queryStringParameter, inStockFilterModel7Spikes);
	}

	public void UpdateSpecificationModel(string queryStringParameter, SpecificationFilterModel7Spikes specificationFiltersModel7Spikes)
	{
		UpdateSpecificationModelInternal(queryStringParameter, specificationFiltersModel7Spikes);
	}

	public void UpdateAttributesFilterModel(string queryStringParameter, AttributeFilterModel7Spikes attributeFilterModel7Spikes)
	{
		UpdateAttributesFilterModelInternal(queryStringParameter, attributeFilterModel7Spikes);
	}

	public void UpdateManufacturerFilterModel(string queryStringParameter, ManufacturerFilterModel7Spikes manufacturerFilterModel7Spikes)
	{
		UpdateManufacturerFilterModelInternal(queryStringParameter, manufacturerFilterModel7Spikes);
	}

	public void UpdateVendorFilterModel(string queryStringParameter, VendorFilterModel7Spikes vendorFilterModel7Spikes)
	{
		UpdateVendorFilterModelInternal(queryStringParameter, vendorFilterModel7Spikes);
	}

	public void UpdatePriceRangeModel(string queryStringParameter, PriceRangeFilterModel7Spikes priceRangeFilterModel7Spikes)
	{
		UpdatePriceRangeModelInternal(queryStringParameter, priceRangeFilterModel7Spikes);
	}

	public void UpdatePagingFilterModelWithPageSize(string queryStringParameter, CatalogProductsCommand pagingFilteringModel)
	{
		UpdatePagingFilterModelWithPageSizeInternal(queryStringParameter, pagingFilteringModel);
	}

	public void UpdatePagingFilterModelWithViewMode(string queryStringParameter, CatalogProductsCommand pagingFilteringModel)
	{
		UpdatePagingFilterModelWithViewModeInternal(queryStringParameter, pagingFilteringModel);
	}

	public void UpdatePagingFilterModelWithOrderBy(string queryStringParameter, CatalogProductsCommand pagingFilteringModel)
	{
		UpdatePagingFilterModelWithOrderByInternal(queryStringParameter, pagingFilteringModel);
	}

	public void UpdatePagingFilterModelWithPageNumber(string queryStringParameter, CatalogProductsCommand pagingFilteringModel)
	{
		UpdatePagingFilterModelWithPageNumberInternal(queryStringParameter, pagingFilteringModel);
	}

	private static int SetSelectedPriceRange(string priceRangeWithPrefix, string prefix)
	{
		if (!string.IsNullOrEmpty(priceRangeWithPrefix))
		{
			string[] array = Regex.Split(priceRangeWithPrefix, prefix);
			if (!string.IsNullOrEmpty(array[1]) && int.TryParse(array[1], out var result) && result > 0)
			{
				return result;
			}
		}
		return 0;
	}

	private QueryParameterTypes GetParameterType(string queryParameter)
	{
		if (queryParameter.IndexOf("specFilters=", StringComparison.Ordinal) >= 0)
		{
			return QueryParameterTypes.SpecificationFilter;
		}
		if (queryParameter.IndexOf("attrFilters=", StringComparison.Ordinal) >= 0)
		{
			return QueryParameterTypes.AttributeFilter;
		}
		if (queryParameter.IndexOf("manFilters=", StringComparison.Ordinal) >= 0)
		{
			return QueryParameterTypes.ManufacturerFilter;
		}
		if (queryParameter.IndexOf("venFilters=", StringComparison.Ordinal) >= 0)
		{
			return QueryParameterTypes.VendorFilter;
		}
		if (queryParameter.IndexOf("prFilter=", StringComparison.Ordinal) >= 0)
		{
			return QueryParameterTypes.PriceRangeFilter;
		}
		if (queryParameter.IndexOf("orderBy=", StringComparison.Ordinal) >= 0)
		{
			return QueryParameterTypes.OrderBy;
		}
		if (queryParameter.IndexOf("pageSize=", StringComparison.Ordinal) >= 0)
		{
			return QueryParameterTypes.PageSize;
		}
		if (queryParameter.IndexOf("pageNumber=", StringComparison.Ordinal) >= 0)
		{
			return QueryParameterTypes.PageNumber;
		}
		if (queryParameter.IndexOf("viewMode=", StringComparison.Ordinal) >= 0)
		{
			return QueryParameterTypes.ViewMode;
		}
		if (queryParameter.IndexOf("osFilters=", StringComparison.Ordinal) >= 0)
		{
			return QueryParameterTypes.OnSaleFilter;
		}
		if (queryParameter.IndexOf("isFilters=", StringComparison.Ordinal) >= 0)
		{
			return QueryParameterTypes.InStockFilter;
		}
		return QueryParameterTypes.UnknownType;
	}

	private static void ClearOnSaleFilterModel(OnSaleFilterModel7Spikes onSaleFilterModel)
	{
		if (onSaleFilterModel != null)
		{
			onSaleFilterModel.Priority = 0;
			onSaleFilterModel.FilterItemState = FilterItemState.Unchecked;
		}
	}

	private static void ClearSpecificationFilterModel(SpecificationFilterModel7Spikes specificationFilterModel7Spikes)
	{
		if (specificationFilterModel7Spikes == null)
		{
			return;
		}
		specificationFilterModel7Spikes.Priority = 0;
		foreach (SpecificationFilterGroup specificationFilterGroup in specificationFilterModel7Spikes.SpecificationFilterGroups)
		{
			specificationFilterGroup.IsMain = false;
			foreach (SpecificationFilterItem filterItem in specificationFilterGroup.FilterItems)
			{
				filterItem.FilterItemState = FilterItemState.Unchecked;
			}
		}
	}

	private static void ClearAttributeFilterModel(AttributeFilterModel7Spikes attributeFilterModel7Spikes)
	{
		if (attributeFilterModel7Spikes == null)
		{
			return;
		}
		attributeFilterModel7Spikes.Priority = 0;
		foreach (AttributeFilterGroup attributeFilterGroup in attributeFilterModel7Spikes.AttributeFilterGroups)
		{
			attributeFilterGroup.IsMain = false;
			foreach (AttributeFilterItem filterItem in attributeFilterGroup.FilterItems)
			{
				filterItem.FilterItemState = FilterItemState.Unchecked;
			}
		}
	}

	private static void ClearManufacturerFilterModel(ManufacturerFilterModel7Spikes manufacturerFilterModel7Spikes)
	{
		if (manufacturerFilterModel7Spikes == null)
		{
			return;
		}
		manufacturerFilterModel7Spikes.Priority = 0;
		foreach (ManufacturerFilterItem manufacturerFilterItem in manufacturerFilterModel7Spikes.ManufacturerFilterItems)
		{
			manufacturerFilterItem.FilterItemState = FilterItemState.Unchecked;
		}
	}

	private static void ClearVendorFilterModel(VendorFilterModel7Spikes vendorFilterModel7Spikes)
	{
		if (vendorFilterModel7Spikes == null)
		{
			return;
		}
		vendorFilterModel7Spikes.Priority = 0;
		foreach (VendorFilterItem vendorFilterItem in vendorFilterModel7Spikes.VendorFilterItems)
		{
			vendorFilterItem.FilterItemState = FilterItemState.Unchecked;
		}
	}

	private static void ClearPriceRangeFilterModel(PriceRangeFilterModel7Spikes priceRangeFilterModel7Spikes)
	{
		if (priceRangeFilterModel7Spikes != null && !(priceRangeFilterModel7Spikes.SelectedPriceRange == (PriceRangeModel)null))
		{
			priceRangeFilterModel7Spikes.Priority = 0;
			priceRangeFilterModel7Spikes.SelectedPriceRange.From = null;
			priceRangeFilterModel7Spikes.SelectedPriceRange.To = null;
		}
	}

	private void SetPagingFilteringModelToDefaultValues(CatalogProductsCommand pagingFilteringModel)
	{
		((BasePageableModel)pagingFilteringModel).PageSize = _filtersPageHelper.GetDefaultPageSize();
        pagingFilteringModel.ViewMode = pagingFilteringModel.CustomProperties.ContainsKey("CategoryViewMode")
               ? (string)pagingFilteringModel.CustomProperties["CategoryViewMode"]
               : _filtersPageHelper.GetDefaultViewMode();
        pagingFilteringModel.OrderBy = _filtersPageHelper.GetDefaultOrderBy();
	}

	private void UpdateModelsFromQueryStringInternal(string queryString, SpecificationFilterModel7Spikes specificationFiltersModel7Spikes, AttributeFilterModel7Spikes attributeFilterModel7Spikes, ManufacturerFilterModel7Spikes manufacturerFilterModel7Spikes, VendorFilterModel7Spikes vendorFilterModel7Spikes, PriceRangeFilterModel7Spikes priceRangeFilterModel7Spikes, CatalogProductsCommand pagingFilteringModel, OnSaleFilterModel7Spikes onSaleFilterModel, InStockFilterModel7Spikes inStockFilterModel7Spikes)
	{
		if (string.IsNullOrEmpty(queryString))
		{
			return;
		}
		ClearSpecificationFilterModel(specificationFiltersModel7Spikes);
		ClearAttributeFilterModel(attributeFilterModel7Spikes);
		ClearManufacturerFilterModel(manufacturerFilterModel7Spikes);
		ClearVendorFilterModel(vendorFilterModel7Spikes);
		ClearPriceRangeFilterModel(priceRangeFilterModel7Spikes);
		ClearOnSaleFilterModel(onSaleFilterModel);
		ClearInStockFilterModel(inStockFilterModel7Spikes);
		SetPagingFilteringModelToDefaultValues(pagingFilteringModel);
		string[] array = Regex.Split(queryString, "&");
		foreach (string text in array)
		{
			switch (GetParameterType(text))
			{
			case QueryParameterTypes.SpecificationFilter:
				UpdateSpecificationModel(text, specificationFiltersModel7Spikes);
				break;
			case QueryParameterTypes.OnSaleFilter:
				UpdateOnSaleModel(text, onSaleFilterModel);
				break;
			case QueryParameterTypes.InStockFilter:
				UpdateInStockModel(text, inStockFilterModel7Spikes);
				break;
			case QueryParameterTypes.AttributeFilter:
				UpdateAttributesFilterModel(text, attributeFilterModel7Spikes);
				break;
			case QueryParameterTypes.ManufacturerFilter:
				UpdateManufacturerFilterModel(text, manufacturerFilterModel7Spikes);
				break;
			case QueryParameterTypes.VendorFilter:
				UpdateVendorFilterModel(text, vendorFilterModel7Spikes);
				break;
			case QueryParameterTypes.PriceRangeFilter:
				UpdatePriceRangeModel(text, priceRangeFilterModel7Spikes);
				break;
			case QueryParameterTypes.PageSize:
				UpdatePagingFilterModelWithPageSize(text, pagingFilteringModel);
				break;
			case QueryParameterTypes.ViewMode:
				UpdatePagingFilterModelWithViewMode(text, pagingFilteringModel);
				break;
			case QueryParameterTypes.OrderBy:
				UpdatePagingFilterModelWithOrderBy(text, pagingFilteringModel);
				break;
			case QueryParameterTypes.PageNumber:
				UpdatePagingFilterModelWithPageNumber(text, pagingFilteringModel);
				break;
			}
		}
	}

	private void ClearInStockFilterModel(InStockFilterModel7Spikes inStockFilterModel7Spikes)
	{
		if (inStockFilterModel7Spikes != null)
		{
			inStockFilterModel7Spikes.Priority = 0;
			inStockFilterModel7Spikes.FilterItemState = FilterItemState.Unchecked;
		}
	}

	private void UpdateOnSaleModelInternal(string queryStringParameter, OnSaleFilterModel7Spikes onSaleFilterModel)
	{
		if (!string.IsNullOrEmpty(queryStringParameter) && onSaleFilterModel != null && Regex.Split(queryStringParameter, "osFilters=").Length >= 2)
		{
			onSaleFilterModel.Priority = 1;
			onSaleFilterModel.FilterItemState = FilterItemState.Checked;
		}
	}

	private void UpdateSpecificationModelInternal(string queryStringParameter, SpecificationFilterModel7Spikes specificationFiltersModel7Spikes)
	{
		if (string.IsNullOrEmpty(queryStringParameter) || specificationFiltersModel7Spikes == null)
		{
			return;
		}
		string[] array = Regex.Split(queryStringParameter, "specFilters=");
		if (array.Length < 2)
		{
			return;
		}
		string[] array2 = Regex.Split(array[1], "!-#!");
		specificationFiltersModel7Spikes.Priority = 1;
		string[] array3 = array2;
		foreach (string input in array3)
		{
			string[] array4 = Regex.Split(input, "!#-!");
			if (array4.Length > 2)
			{
				continue;
			}
			string text = array4[0];
			string s = text;
			bool isMain = false;
			if (text.IndexOf("m", StringComparison.Ordinal) >= 0)
			{
				s = Regex.Split(text, "m")[0];
				isMain = true;
			}
			if (!int.TryParse(s, out var groupId) || groupId <= 0)
			{
				continue;
			}
			SpecificationFilterGroup specificationFilterGroup = specificationFiltersModel7Spikes.SpecificationFilterGroups.FirstOrDefault((SpecificationFilterGroup x) => x.Id == groupId);
			if (specificationFilterGroup == null)
			{
				continue;
			}
			specificationFilterGroup.IsMain = isMain;
			if (array4.Length < 2)
			{
				continue;
			}
			string[] array5 = Regex.Split(array4[1], "!##!");
			foreach (string s2 in array5)
			{
				if (int.TryParse(s2, out var itemId) && itemId > 0)
				{
					SpecificationFilterItem specificationFilterItem = specificationFilterGroup.FilterItems.FirstOrDefault((SpecificationFilterItem x) => x.Id == itemId);
					if (specificationFilterItem != null)
					{
						specificationFilterItem.FilterItemState = FilterItemState.Checked;
					}
				}
			}
		}
	}

	private void UpdateAttributesFilterModelInternal(string queryStringParameter, AttributeFilterModel7Spikes attributeFilterModel7Spikes)
	{
		if (string.IsNullOrEmpty(queryStringParameter) || attributeFilterModel7Spikes == null)
		{
			return;
		}
		string[] array = Regex.Split(queryStringParameter, "attrFilters=");
		if (array.Length < 2)
		{
			return;
		}
		string[] array2 = Regex.Split(array[1], "!-#!");
		attributeFilterModel7Spikes.Priority = 1;
		string[] array3 = array2;
		foreach (string input in array3)
		{
			string[] array4 = Regex.Split(input, "!#-!");
			if (array4.Length > 2)
			{
				continue;
			}
			string text = array4[0];
			string s = text;
			bool isMain = false;
			if (text.IndexOf("m", StringComparison.Ordinal) >= 0)
			{
				s = Regex.Split(text, "m")[0];
				isMain = true;
			}
			if (!int.TryParse(s, out var groupId) || groupId <= 0)
			{
				continue;
			}
			AttributeFilterGroup attributeFilterGroup = attributeFilterModel7Spikes.AttributeFilterGroups.FirstOrDefault((AttributeFilterGroup a) => a.Id == groupId);
			if (attributeFilterGroup == null)
			{
				continue;
			}
			attributeFilterGroup.IsMain = isMain;
			if (array4.Length < 2)
			{
				continue;
			}
			string[] array5 = Regex.Split(array4[1], "!##!");
			foreach (string attributeFilterItem in array5)
			{
				if (!string.IsNullOrEmpty(attributeFilterItem))
				{
					AttributeFilterItem attributeFilterItem2 = attributeFilterGroup.FilterItems.FirstOrDefault((AttributeFilterItem a) => a.ValueId.ToString() == attributeFilterItem);
					if (attributeFilterItem2 != null)
					{
						attributeFilterItem2.FilterItemState = FilterItemState.Checked;
					}
				}
			}
		}
	}

	private void UpdateManufacturerFilterModelInternal(string queryStringParameter, ManufacturerFilterModel7Spikes manufacturerFilterModel7Spikes)
	{
		if (string.IsNullOrEmpty(queryStringParameter) || manufacturerFilterModel7Spikes == null)
		{
			return;
		}
		string[] array = Regex.Split(queryStringParameter, "manFilters=");
		if (array.Length < 2)
		{
			return;
		}
		string[] array2 = Regex.Split(array[1], "!##!");
		manufacturerFilterModel7Spikes.Priority = 1;
		string[] array3 = array2;
		foreach (string s in array3)
		{
			if (int.TryParse(s, out var manufacturer) && manufacturer > 0)
			{
				ManufacturerFilterItem manufacturerFilterItem = manufacturerFilterModel7Spikes.ManufacturerFilterItems.FirstOrDefault((ManufacturerFilterItem m) => m.Id == manufacturer);
				if (manufacturerFilterItem != null)
				{
					manufacturerFilterItem.FilterItemState = FilterItemState.Checked;
				}
			}
		}
	}

	private void UpdateVendorFilterModelInternal(string queryStringParameter, VendorFilterModel7Spikes vendorFilterModel7Spikes)
	{
		if (string.IsNullOrEmpty(queryStringParameter) || vendorFilterModel7Spikes == null)
		{
			return;
		}
		string[] array = Regex.Split(queryStringParameter, "venFilters=");
		if (array.Length < 2)
		{
			return;
		}
		string[] array2 = Regex.Split(array[1], "!##!");
		vendorFilterModel7Spikes.Priority = 1;
		string[] array3 = array2;
		foreach (string s in array3)
		{
			if (int.TryParse(s, out var vendor) && vendor > 0)
			{
				VendorFilterItem vendorFilterItem = vendorFilterModel7Spikes.VendorFilterItems.FirstOrDefault((VendorFilterItem m) => m.Id == vendor);
				if (vendorFilterItem != null)
				{
					vendorFilterItem.FilterItemState = FilterItemState.Checked;
				}
			}
		}
	}

	private void UpdatePriceRangeModelInternal(string queryStringParameter, PriceRangeFilterModel7Spikes priceRangeFilterModel7Spikes)
	{
		//IL_00b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bc: Expected O, but got Unknown
		if (string.IsNullOrEmpty(queryStringParameter) || priceRangeFilterModel7Spikes == null)
		{
			return;
		}
		string[] array = Regex.Split(queryStringParameter, "prFilter=");
		if (string.IsNullOrEmpty(array[1]))
		{
			return;
		}
		string[] array2 = Regex.Split(array[1], "!-#!");
		string priceRangeWithPrefix = string.Empty;
		string priceRangeWithPrefix2 = string.Empty;
		if (array2.Length < 2)
		{
			string text = array2[0];
			if (text.IndexOf("From-", StringComparison.Ordinal) > -1)
			{
				priceRangeWithPrefix = text;
			}
			if (text.IndexOf("To-", StringComparison.Ordinal) > -1)
			{
				priceRangeWithPrefix2 = text;
			}
		}
		else
		{
			priceRangeWithPrefix = array2[0];
			priceRangeWithPrefix2 = array2[1];
		}
		int num = SetSelectedPriceRange(priceRangeWithPrefix, "From-");
		int num2 = SetSelectedPriceRange(priceRangeWithPrefix2, "To-");
		priceRangeFilterModel7Spikes.Priority = 1;
		if (priceRangeFilterModel7Spikes.SelectedPriceRange == (PriceRangeModel)null && (num > 0 || num2 > 0))
		{
			priceRangeFilterModel7Spikes.SelectedPriceRange = new PriceRangeModel();
		}
		if (num > 0 && priceRangeFilterModel7Spikes.SelectedPriceRange != (PriceRangeModel)null)
		{
			priceRangeFilterModel7Spikes.SelectedPriceRange.From = num;
		}
		if (num2 > 0 && priceRangeFilterModel7Spikes.SelectedPriceRange != (PriceRangeModel)null)
		{
			priceRangeFilterModel7Spikes.SelectedPriceRange.To = num2;
		}
	}

	private void UpdatePagingFilterModelWithPageSizeInternal(string queryStringParameter, CatalogProductsCommand pagingFilteringModel)
	{
		if (!string.IsNullOrEmpty(queryStringParameter) && !(pagingFilteringModel == (CatalogProductsCommand)null) && int.TryParse(Regex.Split(queryStringParameter, "pageSize=")[1], out var result) && result > 0)
		{
			((BasePageableModel)pagingFilteringModel).PageSize = result;
		}
	}

	private void UpdatePagingFilterModelWithViewModeInternal(string queryStringParameter, CatalogProductsCommand pagingFilteringModel)
	{
		if (!string.IsNullOrEmpty(queryStringParameter) && !(pagingFilteringModel == (CatalogProductsCommand)null))
		{
			string[] array = Regex.Split(queryStringParameter, "viewMode=");
			if (array.Length > 1)
			{
				pagingFilteringModel.ViewMode = array[1];
			}
		}
	}

	private void UpdatePagingFilterModelWithOrderByInternal(string queryStringParameter, CatalogProductsCommand pagingFilteringModel)
	{
		if (!string.IsNullOrEmpty(queryStringParameter) && !(pagingFilteringModel == (CatalogProductsCommand)null) && int.TryParse(Regex.Split(queryStringParameter, "orderBy=")[1], out var result) && result > -1)
		{
			pagingFilteringModel.OrderBy = result;
		}
	}

	private void UpdatePagingFilterModelWithPageNumberInternal(string queryStringParameter, CatalogProductsCommand pagingFilteringModel)
	{
		if (!string.IsNullOrEmpty(queryStringParameter) && !(pagingFilteringModel == (CatalogProductsCommand)null) && int.TryParse(Regex.Split(queryStringParameter, "pageNumber=")[1], out var result) && result > -1)
		{
			((BasePageableModel)pagingFilteringModel).PageNumber = result;
		}
	}

	private void UpdateInStockModelInternal(string queryStringParameter, InStockFilterModel7Spikes inStockFilterModel7Spikes)
	{
		if (!string.IsNullOrEmpty(queryStringParameter) && inStockFilterModel7Spikes != null && Regex.Split(queryStringParameter, "isFilters=").Length >= 2)
		{
			inStockFilterModel7Spikes.Priority = 1;
			inStockFilterModel7Spikes.FilterItemState = FilterItemState.Checked;
		}
	}
}
