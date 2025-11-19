using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using Nop.Web.Framework.Models;
using Nop.Web.Models.Catalog;
using SevenSpikes.Nop.Framework.Models;
using SevenSpikes.Nop.Plugins.AjaxFilters.Areas.Admin.Models;

namespace SevenSpikes.Nop.Plugins.AjaxFilters.Models;

public record ProductsModel : Base7SpikesProductsModel
{
	public IEnumerable<ProductOverviewModel> Products { get; set; }

	public IList<int> ProductIdsToDetermineFilters { get; set; }

	public string SpecificationFilterModel7SpikesJson { get; set; }

	public string AttributeFilterModel7SpikesJson { get; set; }

	public string ManufacturerFilterModel7SpikesJson { get; set; }

	public string VendorFilterModel7SpikesJson { get; set; }

	public string OnSaleFilterModel7SpikesJson { get; set; }

	public string InStockFilterModel7SpikesJson { get; set; }

	public string ViewMode { get; set; }

	public CatalogProductsCommand PagingFilteringContext { get; set; }

	public NopAjaxFiltersSettingsModel NopAjaxFiltersSettingsModel { get; set; }

	public string HashQuery { get; set; }

	public string PriceRangeFromJson { get; set; }

	public string PriceRangeToJson { get; set; }

	public string CurrentPageSizeJson { get; set; }

	public string CurrentViewModeJson { get; set; }

	public string CurrentOrderByJson { get; set; }

	public string CurrentPageNumberJson { get; set; }

	public int TotalCount { get; set; }

	public ProductsModel()
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Expected O, but got Unknown
		Products = new List<ProductOverviewModel>();
		PagingFilteringContext = new CatalogProductsCommand();
	}

	public override IList<ProductOverviewModel> GetProductOverviewModels(string consumerName)
	{
		return Products.ToList();
	}

	//[CompilerGenerated]
	//public override string ToString()
	//{
	//	StringBuilder stringBuilder = new StringBuilder();
	//	stringBuilder.Append("ProductsModel");
	//	stringBuilder.Append(" { ");
	//	if (((BaseNopModel)this).PrintMembers(stringBuilder))
	//	{
	//		stringBuilder.Append(' ');
	//	}
	//	stringBuilder.Append('}');
	//	return stringBuilder.ToString();
	//}
}
