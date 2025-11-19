namespace NopStation.Plugin.B2B.ErpDataScheduler.Services.SyncTaskServices;

public interface IErpProductSyncService
{
    Task<bool> IsErpProductSyncSuccessfulAsync(
        string? stockCode,
        string? salesOrgCode = null,
        bool isManualTrigger = false,
        bool isIncrementalSync = true,
        CancellationToken cancellationToken = default
    );
}
