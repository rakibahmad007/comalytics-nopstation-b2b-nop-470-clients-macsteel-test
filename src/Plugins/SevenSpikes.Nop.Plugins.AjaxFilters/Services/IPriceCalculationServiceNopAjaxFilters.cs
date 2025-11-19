using System.Threading.Tasks;
using SevenSpikes.Nop.Plugins.AjaxFilters.Models.PriceRangeFilterSlider;

namespace SevenSpikes.Nop.Plugins.AjaxFilters.Services;

public interface IPriceCalculationServiceNopAjaxFilters
{
	Task<PriceRangeFilterDto> GetPriceRangeFilterDtoAsync(int categoryId, int manufacturerId, int vendorId);

	Task<decimal> CalculateBasePriceAsync(decimal price, PriceRangeFilterDto priceRangeModel, bool isFromPrice);
}
