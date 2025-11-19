using Quartz;

namespace NopStation.Plugin.B2B.ErpDataScheduler.Services.SyncTaskServices;

[DisallowConcurrentExecution]
public partial class ErpGroupPriceIncrementalSyncTask : IJob
{
    #region Fields

    private readonly IErpGroupPriceSyncService _erpGroupPriceSyncService;

    #endregion

    #region Ctor

    public ErpGroupPriceIncrementalSyncTask(IErpGroupPriceSyncService erpGroupPriceSyncService)
    {
        _erpGroupPriceSyncService = erpGroupPriceSyncService;
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

            await _erpGroupPriceSyncService.IsErpGroupPriceSyncSuccessfulAsync(
                priceCode: null,
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
