using System.Threading.Tasks;
using Nop.Data;
using Nop.Services.ScheduleTasks;

namespace Nop.Plugin.Misc.ErpWebhook.Services.ScheduleTasks
{
    public class ProcessParallelErpStockToB2BStockTask : IScheduleTask
    {
        private readonly INopDataProvider _nopDataProvider;

        public ProcessParallelErpStockToB2BStockTask(INopDataProvider nopDataProvider)
        {
            _nopDataProvider = nopDataProvider;
        }
        public async Task ExecuteAsync()
        {
            await _nopDataProvider.ExecuteNonQueryAsync("EXEC UpdateErpStockFromParallelTableParallel_ErpStock;");
        }
    }
}
