using System;
using System.Linq;
using System.Text;
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

public class QueryStringBuilder : IQueryStringBuilder
{
	private readonly IFiltersPageHelper _filtersPageHelper;

	private SpecificationFilterModel7Spikes _specificationFilterModel7Spikes;

	private AttributeFilterModel7Spikes _attributeFilterModel7Spikes;

	private ManufacturerFilterModel7Spikes _manufacturerFilterModel7Spikes;

	private VendorFilterModel7Spikes _vendorFilterModel7Spikes;

	private OnSaleFilterModel7Spikes _onSaleFilterModel7Spikes;

	private InStockFilterModel7Spikes _inStockFilterModel7Spikes;

	private PriceRangeFilterModel7Spikes _priceRangeFilterModel7Spikes;

	private CatalogProductsCommand _catalogPagingFilteringModel;

	public string SpecificationsQueryString { get; set; }

	public string PagingQueryString { get; set; }

	public string PriceRangeQueryString { get; set; }

	public string ManufacturersQueryString { get; set; }

	public string VendorsQueryString { get; set; }

	public string OnSaleQueryString { get; set; }

	public string InStockQueryString { get; set; }

	public string AttributesQueryString { get; set; }

	public QueryStringBuilder(IFiltersPageHelper filtersPageHelper)
	{
		_filtersPageHelper = filtersPageHelper;
	}

	public string GetQueryString(bool shouldRebuildQueryString)
	{
		if (shouldRebuildQueryString)
		{
			BuildQueryString();
		}
		return GetQueryStringInternal();
	}

	public void SetDataForQueryString(SpecificationFilterModel7Spikes specificationFilterModel7Spikes, AttributeFilterModel7Spikes attributeFilterModel7Spikes, ManufacturerFilterModel7Spikes manufacturerFilterModel7Spikes, VendorFilterModel7Spikes vendorFilterModel7Spikes, PriceRangeFilterModel7Spikes priceRangeFilterModel7Spikes, CatalogProductsCommand catalogPagingFilteringModel, OnSaleFilterModel7Spikes onSaleFilterModel, InStockFilterModel7Spikes inStockFilterModel7Spikes)
	{
		_catalogPagingFilteringModel = catalogPagingFilteringModel;
		_priceRangeFilterModel7Spikes = priceRangeFilterModel7Spikes;
		_manufacturerFilterModel7Spikes = manufacturerFilterModel7Spikes;
		_vendorFilterModel7Spikes = vendorFilterModel7Spikes;
		_onSaleFilterModel7Spikes = onSaleFilterModel;
		_inStockFilterModel7Spikes = inStockFilterModel7Spikes;
		_attributeFilterModel7Spikes = attributeFilterModel7Spikes;
		_specificationFilterModel7Spikes = specificationFilterModel7Spikes;
	}

	private void BuildQueryString()
	{
		BuildSpecificationsQueryString(_specificationFilterModel7Spikes);
		BuildAttributesQueryString(_attributeFilterModel7Spikes);
		BuildManufacturerQueryString(_manufacturerFilterModel7Spikes);
		BuildVendorQueryString(_vendorFilterModel7Spikes);
		BuildOnSaleQueryString(_onSaleFilterModel7Spikes);
		BuildInStockQueryString(_inStockFilterModel7Spikes);
		BuildPriceRangeQueryString(_priceRangeFilterModel7Spikes);
		BuildPagingFilterQueryString(_catalogPagingFilteringModel);
	}

	private void BuildInStockQueryString(InStockFilterModel7Spikes inStockFilterModel7Spikes)
	{
		InStockQueryString = string.Empty;
		if (inStockFilterModel7Spikes != null && (inStockFilterModel7Spikes.FilterItemState == FilterItemState.Checked || inStockFilterModel7Spikes.FilterItemState == FilterItemState.CheckedDisabled))
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append("isFilters=");
			stringBuilder.Append(inStockFilterModel7Spikes.Id);
			stringBuilder.Append("!##!");
			TrimEndDelimiter(stringBuilder, "!##!");
			InStockQueryString = stringBuilder.ToString();
		}
	}

	private void BuildAttributesQueryString(AttributeFilterModel7Spikes attributeFilterModel7Spikes)
	{
		AttributesQueryString = string.Empty;
		if (attributeFilterModel7Spikes == null || !attributeFilterModel7Spikes.AttributeFilterGroups.SelectMany((AttributeFilterGroup fi) => fi.FilterItems).Any((AttributeFilterItem fis) => fis.FilterItemState == FilterItemState.Checked || fis.FilterItemState == FilterItemState.CheckedDisabled))
		{
			return;
		}
		StringBuilder stringBuilder = new StringBuilder();
		if (attributeFilterModel7Spikes.AttributeFilterGroups.Count > 0)
		{
			stringBuilder.Append("attrFilters=");
		}
		foreach (AttributeFilterGroup attributeFilterGroup in attributeFilterModel7Spikes.AttributeFilterGroups)
		{
			if (stringBuilder.ToString() != "attrFilters=")
			{
				stringBuilder.Append("!-#!");
			}
			StringBuilder stringBuilder2 = new StringBuilder();
			foreach (AttributeFilterItem item in attributeFilterGroup.FilterItems.Where((AttributeFilterItem attrbuteItem) => attrbuteItem.FilterItemState == FilterItemState.Checked || attrbuteItem.FilterItemState == FilterItemState.CheckedDisabled))
			{
				stringBuilder2.Append(item.ValueId);
				stringBuilder2.Append("!##!");
			}
			TrimEndDelimiter(stringBuilder2, "!##!");
			if (attributeFilterGroup.IsMain && stringBuilder2.Length == 0)
			{
				if (stringBuilder.ToString() != "attrFilters=")
				{
					stringBuilder.Append("!-#!");
				}
				stringBuilder.Append(attributeFilterGroup.Id);
				stringBuilder.Append("m");
			}
			if (stringBuilder2.Length > 0)
			{
				stringBuilder.Append(attributeFilterGroup.Id);
				if (attributeFilterGroup.IsMain)
				{
					stringBuilder.Append("m");
				}
				stringBuilder.Append("!#-!");
				stringBuilder.Append(stringBuilder2);
			}
		}
		if (stringBuilder.ToString() != "attrFilters=")
		{
			AttributesQueryString = stringBuilder.ToString();
		}
	}

	private void BuildSpecificationsQueryString(SpecificationFilterModel7Spikes specificationFilterModel7Spikes)
	{
		SpecificationsQueryString = string.Empty;
		if (specificationFilterModel7Spikes == null || !specificationFilterModel7Spikes.SpecificationFilterGroups.SelectMany((SpecificationFilterGroup fi) => fi.FilterItems).Any((SpecificationFilterItem fis) => fis.FilterItemState == FilterItemState.Checked || fis.FilterItemState == FilterItemState.CheckedDisabled))
		{
			return;
		}
		StringBuilder stringBuilder = new StringBuilder();
		if (specificationFilterModel7Spikes.SpecificationFilterGroups.Count > 0)
		{
			stringBuilder.Append("specFilters=");
		}
		foreach (SpecificationFilterGroup specificationFilterGroup in specificationFilterModel7Spikes.SpecificationFilterGroups)
		{
			StringBuilder stringBuilder2 = new StringBuilder();
			foreach (SpecificationFilterItem item in specificationFilterGroup.FilterItems.Where((SpecificationFilterItem specificationItem) => specificationItem.FilterItemState == FilterItemState.Checked || specificationItem.FilterItemState == FilterItemState.CheckedDisabled))
			{
				stringBuilder2.Append(item.Id);
				stringBuilder2.Append("!##!");
			}
			TrimEndDelimiter(stringBuilder2, "!##!");
			if (specificationFilterGroup.IsMain && stringBuilder2.Length == 0)
			{
				if (stringBuilder.ToString() != "specFilters=")
				{
					stringBuilder.Append("!-#!");
				}
				stringBuilder.Append(specificationFilterGroup.Id);
				stringBuilder.Append("m");
			}
			if (stringBuilder2.Length > 0)
			{
				if (stringBuilder.ToString() != "specFilters=")
				{
					stringBuilder.Append("!-#!");
				}
				stringBuilder.Append(specificationFilterGroup.Id);
				if (specificationFilterGroup.IsMain)
				{
					stringBuilder.Append("m");
				}
				stringBuilder.Append("!#-!");
				stringBuilder.Append(stringBuilder2);
			}
		}
		if (stringBuilder.ToString() != "specFilters=")
		{
			SpecificationsQueryString = stringBuilder.ToString();
		}
	}

	private void BuildPagingFilterQueryString(CatalogProductsCommand pagingFilteringModel)
	{
		PagingQueryString = string.Empty;
		if (pagingFilteringModel == (CatalogProductsCommand)null)
		{
			return;
		}
		StringBuilder stringBuilder = new StringBuilder();
		if (((BasePageableModel)pagingFilteringModel).PageSize != _filtersPageHelper.GetDefaultPageSize() || pagingFilteringModel.ViewMode != _filtersPageHelper.GetDefaultViewMode() || pagingFilteringModel.OrderBy != _filtersPageHelper.GetDefaultOrderBy() || ((BasePageableModel)pagingFilteringModel).PageNumber != _filtersPageHelper.GetDefaultPageNumber())
		{
			stringBuilder.Append("pageSize=");
			stringBuilder.Append(((BasePageableModel)pagingFilteringModel).PageSize);
			if (!string.IsNullOrEmpty(pagingFilteringModel.ViewMode))
			{
				AppendSeparator(stringBuilder);
				stringBuilder.Append("viewMode=");
				stringBuilder.Append(pagingFilteringModel.ViewMode);
			}
			AppendSeparator(stringBuilder);
			stringBuilder.Append("orderBy=");
			stringBuilder.Append(pagingFilteringModel.OrderBy);
			AppendSeparator(stringBuilder);
			stringBuilder.Append("pageNumber=");
			stringBuilder.Append(((BasePageableModel)pagingFilteringModel).PageNumber);
		}
		PagingQueryString = stringBuilder.ToString();
	}

	private void BuildPriceRangeQueryString(PriceRangeFilterModel7Spikes priceRangeFilterModel7Spikes)
	{
		PriceRangeQueryString = string.Empty;
		if (priceRangeFilterModel7Spikes == null || priceRangeFilterModel7Spikes.SelectedPriceRange == (PriceRangeModel)null)
		{
			return;
		}
		StringBuilder stringBuilder = new StringBuilder();
		bool hasValue = priceRangeFilterModel7Spikes.SelectedPriceRange.From.HasValue;
		bool hasValue2 = priceRangeFilterModel7Spikes.SelectedPriceRange.To.HasValue;
		if (!hasValue && !hasValue2)
		{
			return;
		}
		stringBuilder.Append("prFilter=");
		if (hasValue)
		{
			stringBuilder.Append("From-");
			stringBuilder.Append(Convert.ToInt32(priceRangeFilterModel7Spikes.SelectedPriceRange.From));
		}
		if (hasValue2)
		{
			if (hasValue)
			{
				stringBuilder.Append("!-#!");
			}
			stringBuilder.Append("To-");
			stringBuilder.Append(Convert.ToInt32(priceRangeFilterModel7Spikes.SelectedPriceRange.To));
		}
		if (stringBuilder.ToString() != "prFilter=")
		{
			PriceRangeQueryString = stringBuilder.ToString();
		}
	}

	private void BuildManufacturerQueryString(ManufacturerFilterModel7Spikes manufacturerFilterModel7Spikes)
	{
		ManufacturersQueryString = string.Empty;
		if (manufacturerFilterModel7Spikes == null || manufacturerFilterModel7Spikes.ManufacturerFilterItems.Count <= 0 || !manufacturerFilterModel7Spikes.ManufacturerFilterItems.Any((ManufacturerFilterItem m) => m.FilterItemState == FilterItemState.Checked || m.FilterItemState == FilterItemState.CheckedDisabled))
		{
			return;
		}
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append("manFilters=");
		foreach (ManufacturerFilterItem manufacturerFilterItem in manufacturerFilterModel7Spikes.ManufacturerFilterItems)
		{
			if (manufacturerFilterItem.FilterItemState == FilterItemState.Checked || manufacturerFilterItem.FilterItemState == FilterItemState.CheckedDisabled)
			{
				stringBuilder.Append(manufacturerFilterItem.Id);
				stringBuilder.Append("!##!");
			}
		}
		TrimEndDelimiter(stringBuilder, "!##!");
		ManufacturersQueryString = stringBuilder.ToString();
	}

	private void BuildOnSaleQueryString(OnSaleFilterModel7Spikes onSaleFilterModel7Spikes)
	{
		OnSaleQueryString = string.Empty;
		if (onSaleFilterModel7Spikes != null && (onSaleFilterModel7Spikes.FilterItemState == FilterItemState.Checked || onSaleFilterModel7Spikes.FilterItemState == FilterItemState.CheckedDisabled))
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append("osFilters=");
			if (onSaleFilterModel7Spikes.FilterItemState == FilterItemState.Checked || onSaleFilterModel7Spikes.FilterItemState == FilterItemState.CheckedDisabled)
			{
				stringBuilder.Append(onSaleFilterModel7Spikes.Id);
				stringBuilder.Append("!##!");
			}
			TrimEndDelimiter(stringBuilder, "!##!");
			OnSaleQueryString = stringBuilder.ToString();
		}
	}

	private void BuildVendorQueryString(VendorFilterModel7Spikes vendorFilterModel7Spikes)
	{
		VendorsQueryString = string.Empty;
		if (vendorFilterModel7Spikes == null || vendorFilterModel7Spikes.VendorFilterItems.Count <= 0 || !vendorFilterModel7Spikes.VendorFilterItems.Any((VendorFilterItem v) => v.FilterItemState == FilterItemState.Checked || v.FilterItemState == FilterItemState.CheckedDisabled))
		{
			return;
		}
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append("venFilters=");
		foreach (VendorFilterItem vendorFilterItem in vendorFilterModel7Spikes.VendorFilterItems)
		{
			if (vendorFilterItem.FilterItemState == FilterItemState.Checked || vendorFilterItem.FilterItemState == FilterItemState.CheckedDisabled)
			{
				stringBuilder.Append(vendorFilterItem.Id);
				stringBuilder.Append("!##!");
			}
		}
		TrimEndDelimiter(stringBuilder, "!##!");
		VendorsQueryString = stringBuilder.ToString();
	}

	private static void AppendSeparator(StringBuilder sb)
	{
		if (sb.Length > 0)
		{
			sb.Append("&");
		}
	}

	private static void AppendQueryString(string queryString, StringBuilder sb)
	{
		if (!string.IsNullOrEmpty(queryString))
		{
			AppendSeparator(sb);
			sb.Append(queryString);
		}
	}

	private static void TrimEndDelimiter(StringBuilder itemSb, string delimeterToBeTrimmed)
	{
		if (itemSb.Length > 0)
		{
			int length = delimeterToBeTrimmed.Length;
			itemSb.Remove(itemSb.Length - length, length);
		}
	}

	private string GetQueryStringInternal()
	{
		StringBuilder stringBuilder = new StringBuilder();
		AppendQueryString(SpecificationsQueryString, stringBuilder);
		AppendQueryString(AttributesQueryString, stringBuilder);
		AppendQueryString(ManufacturersQueryString, stringBuilder);
		AppendQueryString(VendorsQueryString, stringBuilder);
		AppendQueryString(OnSaleQueryString, stringBuilder);
		AppendQueryString(InStockQueryString, stringBuilder);
		AppendQueryString(PriceRangeQueryString, stringBuilder);
		AppendQueryString(PagingQueryString, stringBuilder);
		return stringBuilder.ToString();
	}
}
