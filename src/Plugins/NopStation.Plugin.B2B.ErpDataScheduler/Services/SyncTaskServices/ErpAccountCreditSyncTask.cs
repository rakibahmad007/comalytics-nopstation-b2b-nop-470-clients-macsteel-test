using NopStation.Plugin.B2B.ErpDataScheduler.Areas.Admin.Models.PartialSyncModels;
using Quartz;

namespace NopStation.Plugin.B2B.ErpDataScheduler.Services.SyncTaskServices;

[DisallowConcurrentExecution]
public partial class ErpAccountCreditSyncTask : IJob
{
    #region Fields

    private readonly IErpAccountCreditSyncService _erpAccountCreditSyncService;

    #endregion

    #region Ctor

    public ErpAccountCreditSyncTask(IErpAccountCreditSyncService erpAccountCreditSyncService)
    {
        _erpAccountCreditSyncService = erpAccountCreditSyncService;
    }

    #endregion

    #region Methods

    public async Task Execute(IJobExecutionContext context)
    {
        if (context.JobDetail.JobDataMap.TryGetBooleanValue(ErpDataSchedulerDefaults.JobShouldExecute, out var shouldExecute) && shouldExecute)
        {
            context.MergedJobDataMap.TryGetBoolean(ErpDataSchedulerDefaults.IsManualTrigger, out var isManualTrigger);

            context.MergedJobDataMap.TryGetBoolean(ErpDataSchedulerDefaults.IsIncrementalSync, out var isIncrementalSync);

            context.MergedJobDataMap.TryGetString(nameof(ErpAccountCreditPartialSyncModel.ErpAccountNumber), out var erpAccountNumber);

            await _erpAccountCreditSyncService.IsErpAccountCreditSyncSuccessfulAsync(erpAccountNumber, isManualTrigger, isIncrementalSync, context.CancellationToken);
        }
    }

    #endregion
}