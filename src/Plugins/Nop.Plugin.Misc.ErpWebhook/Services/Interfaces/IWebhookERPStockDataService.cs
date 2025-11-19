using System.Collections.Generic;
using System.Threading.Tasks;
using Nop.Plugin.Misc.ErpWebhook.Models.ErpStock;

namespace Nop.Plugin.Misc.ErpWebhook.Services.Interfaces
{
    public interface IWebhookERPStockDataService
    {
        Task ProcessERPStockDataAsync(List<ErpStockLevelModel> erpStockLevels);
        Task ProcessERPStockDataToParallelTableAsync(List<ErpStockLevelModel> erpStockLevels);
    }
}
