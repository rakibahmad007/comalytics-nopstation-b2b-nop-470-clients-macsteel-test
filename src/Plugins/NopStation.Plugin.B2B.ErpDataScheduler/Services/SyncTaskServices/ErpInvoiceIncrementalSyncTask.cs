using Quartz;

namespace NopStation.Plugin.B2B.ErpDataScheduler.Services.SyncTaskServices;

[DisallowConcurrentExecution]
public partial class ErpInvoiceIncrementalSyncTask : IJob
{
    #region Fields

    private readonly IErpInvoiceSyncService _erpInvoiceSyncService;

    #endregion

    #region Ctor

    public ErpInvoiceIncrementalSyncTask(IErpInvoiceSyncService erpInvoiceSyncService)
    {
        _erpInvoiceSyncService = erpInvoiceSyncService;
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

            await _erpInvoiceSyncService.IsErpInvoiceSyncSuccessfulAsync(
                erpAccountNumber: null,
                salesOrgCode: null,
                isManualTrigger,
                isIncrementalSync,
                context.CancellationToken
            );
        }
    }

    #endregion
}
