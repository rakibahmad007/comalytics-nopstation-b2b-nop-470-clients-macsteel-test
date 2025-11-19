using System.Collections.Generic;
using System.Threading.Tasks;
using Nop.Plugin.Misc.ErpWebhook.Domain.ParallelTables;
using Nop.Plugin.Misc.ErpWebhook.Models.ErpProduct;

namespace Nop.Plugin.Misc.ErpWebhook.Services.Interfaces
{
    public interface IWebhookERPProductService
    {
        Task ProcessErpProductsAsync();
        Task ProcessErpProductsToParallelTableAsync(List<ErpProductModel> batch);
        Task<List<Parallel_ErpProduct>> GetErpProductsAsync(int skipCount, int batchSize);
        Task UpdateErpProductsAsync(List<Parallel_ErpProduct> erpProducts);
    }
}
