using NopStation.Plugin.B2B.ErpDataScheduler.Areas.Admin.Models.PartialSyncModels;
using Quartz;

namespace NopStation.Plugin.B2B.ErpDataScheduler.Services.SyncTaskServices;

[DisallowConcurrentExecution]
public partial class ErpSpecSheetSyncTask : IJob
{
    #region Fields

    private readonly IErpSpecSheetSyncService _erpSpecSheetSyncService;

    #endregion

    #region Ctor

    public ErpSpecSheetSyncTask(IErpSpecSheetSyncService erpSpecSheetSyncService)
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
            context.MergedJobDataMap.TryGetString(
                nameof(ErpSpecSheetPartialSyncModel.SalesOrgCode),
                out var salesOrgCode
            );
            context.MergedJobDataMap.TryGetString(
                nameof(ErpSpecSheetPartialSyncModel.StockCode),
                out var stockCode
            );

            var isIncrementalSync = false;

            if (string.IsNullOrWhiteSpace(stockCode) && string.IsNullOrWhiteSpace(salesOrgCode))
            {
                context.JobDetail.JobDataMap.TryGetBooleanValue(
                    ErpDataSchedulerDefaults.IsIncrementalSync,
                    out isIncrementalSync
                );
            }

            await _erpSpecSheetSyncService.IsErpSpecSheetSyncSuccessfulAsync(
                stockCode,
                salesOrgCode,
                isManualTrigger,
                isIncrementalSync,
                context.CancellationToken
            );
        }
    }

    #endregion
}
