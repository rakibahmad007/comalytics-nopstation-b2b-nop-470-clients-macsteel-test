using NopStation.Plugin.B2B.ErpDataScheduler.Areas.Admin.Models.PartialSyncModels;
using Quartz;

namespace NopStation.Plugin.B2B.ErpDataScheduler.Services.SyncTaskServices;

[DisallowConcurrentExecution]
public partial class ErpInvoiceSyncTask : IJob
{
    #region Fields

    private readonly IErpInvoiceSyncService _erpInvoiceSyncService;

    #endregion

    #region Ctor

    public ErpInvoiceSyncTask(IErpInvoiceSyncService erpInvoiceSyncService)
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
            context.MergedJobDataMap.TryGetString(
                nameof(ErpInvoicePartialSyncModel.ErpAccountNumber),
                out var erpAccountNumber
            );
            context.MergedJobDataMap.TryGetString(
                nameof(ErpInvoicePartialSyncModel.SalesOrgCode),
                out var salesOrgCode
            );

            var isIncrementalSync = false;

            if (string.IsNullOrWhiteSpace(erpAccountNumber) && string.IsNullOrWhiteSpace(salesOrgCode))
            {
                context.JobDetail.JobDataMap.TryGetBooleanValue(
                    ErpDataSchedulerDefaults.IsIncrementalSync,
                    out isIncrementalSync
                );
            }

            await _erpInvoiceSyncService.IsErpInvoiceSyncSuccessfulAsync(
                erpAccountNumber,
                salesOrgCode,
                isManualTrigger,
                isIncrementalSync,
                context.CancellationToken
            );
        }
    }

    #endregion
}
