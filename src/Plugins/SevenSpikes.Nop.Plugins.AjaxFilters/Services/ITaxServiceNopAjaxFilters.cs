using System.Threading.Tasks;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Customers;

namespace SevenSpikes.Nop.Plugins.AjaxFilters.Services;

public interface ITaxServiceNopAjaxFilters
{
	Task<decimal> GetTaxRateForProductAsync(Product product, int taxCategoryId, Customer customer);
}
