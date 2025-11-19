using Quartz;

namespace NopStation.Plugin.B2B.ErpDataScheduler.Services.SyncTaskServices;

[DisallowConcurrentExecution]
public partial class ErpStockIncrementalSyncTask : IJob
{
    #region Fields

    private readonly IErpStockSyncService _erpStockSyncService;

    #endregion

    #region Ctor

    public ErpStockIncrementalSyncTask(IErpStockSyncService erpStockSyncService)
    {
        _erpStockSyncService = erpStockSyncService;
    }

    #endregion

    #region Methods

    public async Task Execute(IJobExecutionContext context)
    {
        if (
            context.JobDetail.JobDataMap.TryGetBooleanValue(
                ErpDataSchedulerDefaults.JobShouldExecute,
                out var shouldExecute
            ) && shouldExecute
        )
        {
            context.MergedJobDataMap.TryGetBoolean(
                ErpDataSchedulerDefaults.IsManualTrigger,
                out var isManualTrigger
            );

            context.JobDetail.JobDataMap.TryGetBooleanValue(
                ErpDataSchedulerDefaults.IsIncrementalSync,
                out var isIncrementalSync
            );

            await _erpStockSyncService.IsErpStockSyncSuccessfulAsync(
                stockCode: null,
                salesOrgCode: null,
                isManualTrigger,
                isIncrementalSync,
                context.CancellationToken
            );
        }
    }

    #endregion
}
