using Quartz;

namespace NopStation.Plugin.B2B.ErpDataScheduler.Services.SyncTaskServices;

[DisallowConcurrentExecution]
public partial class ErpOrderIncrementalSyncTask : IJob
{
    #region Fields

    private readonly IErpOrderSyncService _erpOrderSyncService;

    #endregion

    #region Ctor

    public ErpOrderIncrementalSyncTask(IErpOrderSyncService erpOrderSyncService)
    {
        _erpOrderSyncService = erpOrderSyncService;
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

            await _erpOrderSyncService.IsErpOrderSyncSuccessfulAsync(
                erpAccountNumber: null,
                orderNumber: null,
                salesOrgCode: null,
                isManualTrigger,
                isIncrementalSync,
                context.CancellationToken
            );
        }
    }

    #endregion
}
