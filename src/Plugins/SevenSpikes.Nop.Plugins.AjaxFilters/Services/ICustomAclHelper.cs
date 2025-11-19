using System.Linq;
using System.Threading.Tasks;
using Nop.Core.Domain.Catalog;

namespace SevenSpikes.Nop.Plugins.AjaxFilters.Services;

public interface ICustomAclHelper
{
    Task<IQueryable<Product>> GetAvailableProductsForCurrentErpCustomerAsync();
}
