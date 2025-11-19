using System.Collections.Generic;
using System.Threading.Tasks;
using Nop.Plugin.Misc.ErpWebhook.Domain.ParallelTables;
using Nop.Plugin.Misc.ErpWebhook.Models.ErpOrder;

namespace Nop.Plugin.Misc.ErpWebhook.Services.Interfaces
{
    public interface IWebhookErpOrderService
    {
        Task ProcessErpOrdersAsync();
        Task ProcessErpOrdersToParallelTableAsync(List<ErpOrderModel> erpOrders);
        Task<List<Parallel_ErpOrder>> GetErpOrdersAsync(int skipCount, int batchSize);
        Task UpdateErpOrdersAsync(List<Parallel_ErpOrder> erpOrders);
    }
}
