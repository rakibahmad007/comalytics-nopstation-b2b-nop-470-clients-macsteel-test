using Quartz;

namespace NopStation.Plugin.B2B.ErpDataScheduler.Services.SyncTaskServices;

[DisallowConcurrentExecution]
public partial class ErpSpecialPriceIncrementalSyncTask : IJob
{
    #region Fields

    private readonly IErpSpecialPriceSyncService _erpSpecialPriceSyncService;

    #endregion

    #region Ctor

    public ErpSpecialPriceIncrementalSyncTask(
        IErpSpecialPriceSyncService erpSpecialPriceSyncService
    )
    {
        _erpSpecialPriceSyncService = erpSpecialPriceSyncService;
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

            await _erpSpecialPriceSyncService.IsErpSpecialPriceSyncSuccessfulAsync(
                erpAccountNumber: null,
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
