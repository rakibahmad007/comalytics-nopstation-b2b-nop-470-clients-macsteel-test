using Quartz;

namespace NopStation.Plugin.B2B.ErpDataScheduler.Services.SyncTaskServices;

[DisallowConcurrentExecution]
public partial class ErpSpecSheetIncrementalSyncTask : IJob
{
    #region Fields

    private readonly IErpSpecSheetSyncService _erpSpecSheetSyncService;

    #endregion

    #region Ctor

    public ErpSpecSheetIncrementalSyncTask(IErpSpecSheetSyncService erpSpecSheetSyncService)
    {
        _erpSpecSheetSyncService = erpSpecSheetSyncService;
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

            context.JobDetail.JobDataMap.TryGetBoolean(
                ErpDataSchedulerDefaults.IsIncrementalSync,
                out var isIncrementalSync
            );

            await _erpSpecSheetSyncService.IsErpSpecSheetSyncSuccessfulAsync(
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
