using System.Threading.Tasks;
using Nop.Data;
using Nop.Services.ScheduleTasks;

namespace Nop.Plugin.Misc.ErpWebhook.Services.ScheduleTasks
{
    public class ProcessParallelErpAccountPricingToB2BPerAccountPricingTask : IScheduleTask
    {
        private readonly INopDataProvider _nopDataProvider;

        public ProcessParallelErpAccountPricingToB2BPerAccountPricingTask(INopDataProvider nopDataProvider)
        {
            _nopDataProvider = nopDataProvider;
        }
        public async Task ExecuteAsync()
        {
            await _nopDataProvider.ExecuteNonQueryAsync("EXEC UpdateB2BPerAccountPricingFromParallelTableErpAccountPricing;");
        }
    }
}
