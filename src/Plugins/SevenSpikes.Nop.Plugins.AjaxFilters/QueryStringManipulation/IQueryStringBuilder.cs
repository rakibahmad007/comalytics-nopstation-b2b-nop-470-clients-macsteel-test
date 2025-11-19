using Nop.Web.Models.Catalog;
using SevenSpikes.Nop.Plugins.AjaxFilters.Models.AttributeFilter;
using SevenSpikes.Nop.Plugins.AjaxFilters.Models.InStockFilter;
using SevenSpikes.Nop.Plugins.AjaxFilters.Models.ManufacturerFilter;
using SevenSpikes.Nop.Plugins.AjaxFilters.Models.OnSaleFilter;
using SevenSpikes.Nop.Plugins.AjaxFilters.Models.PriceRangeFilterSlider;
using SevenSpikes.Nop.Plugins.AjaxFilters.Models.SpecificationFilter;
using SevenSpikes.Nop.Plugins.AjaxFilters.Models.VendorFilter;

namespace SevenSpikes.Nop.Plugins.AjaxFilters.QueryStringManipulation;

public interface IQueryStringBuilder
{
	void SetDataForQueryString(SpecificationFilterModel7Spikes specificationFilterModel7Spikes, AttributeFilterModel7Spikes attributeFilterModel7Spikes, ManufacturerFilterModel7Spikes manufacturerFilterModel7Spikes, VendorFilterModel7Spikes vendorFilterModel7Spikes, PriceRangeFilterModel7Spikes priceRangeFilterModel7Spikes, CatalogProductsCommand catalogPagingFilteringModel, OnSaleFilterModel7Spikes onSaleFilterModel, InStockFilterModel7Spikes inStockFilterModel);

	string GetQueryString(bool shouldRebuildQueryString);
}
