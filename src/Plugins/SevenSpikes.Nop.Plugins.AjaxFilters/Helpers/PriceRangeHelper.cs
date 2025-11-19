using System.Globalization;
using Microsoft.Extensions.DependencyInjection;
using Nop.Core;
using Nop.Core.Infrastructure;
using Nop.Web.Models.Catalog;

namespace SevenSpikes.Nop.Plugins.AjaxFilters.Helpers;

public static class PriceRangeHelper
{
	public static PriceRangeModel GetSelectedPriceRange(decimal priceRangeTollerance = 0m)
	{
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		//IL_0098: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ac: Expected O, but got Unknown
		string text = EngineContext.Current.Resolve<IWebHelper>((IServiceScope)null).QueryString<string>("price");
		if (string.IsNullOrEmpty(text))
		{
			return null;
		}
		string[] array = text.Trim().Split('-');
		if (array.Length == 2)
		{
			CultureInfo provider = CultureInfo.CreateSpecificCulture("en-us");
			decimal.TryParse(array[0].Trim(), NumberStyles.Number, provider, out var result);
			decimal.TryParse(array[1].Trim(), NumberStyles.Number, provider, out var result2);
			if (result != 0m || result2 != 0m)
			{
				return new PriceRangeModel
				{
					From = result - priceRangeTollerance,
					To = result2 + priceRangeTollerance
				};
			}
		}
		return null;
	}
}
