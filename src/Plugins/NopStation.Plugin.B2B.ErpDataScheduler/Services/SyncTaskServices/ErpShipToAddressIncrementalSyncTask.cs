using Quartz;

namespace NopStation.Plugin.B2B.ErpDataScheduler.Services.SyncTaskServices;

[DisallowConcurrentExecution]
public partial class ErpShipToAddressIncrementalSyncTask : IJob
{
    #region Fields

    private readonly IErpShipToAddressSyncService _erpShipToAddressSyncService;

    #endregion

    #region Ctor

    public ErpShipToAddressIncrementalSyncTask(
        IErpShipToAddressSyncService erpShipToAddressSyncService
    )
    {
        _erpShipToAddressSyncService = erpShipToAddressSyncService;
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

            await _erpShipToAddressSyncService.IsErpShipToAddressSyncSuccessfulAsync(
                erpAccountNumber: null,
                isManualTrigger,
                isIncrementalSync,
                context.CancellationToken
            );
        }
    }

    #endregion
}
