using System.Threading.Tasks;
using Nop.Data;
using Nop.Services.ScheduleTasks;

namespace Nop.Plugin.Misc.ErpWebhook.Services.ScheduleTasks;

public class ProcessParallelErpAccountToB2BAccountTask : IScheduleTask
{
    private readonly INopDataProvider _nopDataProvider;

    public ProcessParallelErpAccountToB2BAccountTask(INopDataProvider nopDataProvider)
    {
        _nopDataProvider = nopDataProvider;
    }
    public async Task ExecuteAsync()
    {
        await _nopDataProvider.ExecuteNonQueryAsync("EXEC UpdateB2BAccountFromParallelTableErpB2BAccount;");
    }
}
