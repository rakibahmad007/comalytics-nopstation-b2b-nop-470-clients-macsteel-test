using System.Threading.Tasks;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Customers;
using Nop.Services.Tax;

namespace SevenSpikes.Nop.Plugins.AjaxFilters.Services;

public class TaxServiceNopAjaxFilters : ITaxServiceNopAjaxFilters
{
	private ITaxService _taxService;

	public TaxServiceNopAjaxFilters(ITaxService taxService)
	{
		_taxService = taxService;
	}

	public async Task<decimal> GetTaxRateForProductAsync(Product product, int taxCategoryId, Customer customer)
	{
		return (await _taxService.GetProductPriceAsync(product, taxCategoryId, product.Price, false, customer, false)).Item2;
	}
}
