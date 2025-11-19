namespace NopStation.Plugin.B2B.ErpDataScheduler.Services.SyncTaskServices;

public interface IErpInvoiceSyncService
{
    Task<bool> IsErpInvoiceSyncSuccessfulAsync(
        string? erpAccountNumber,
        string? salesOrgCode = null,
        bool isManualTrigger = false,
        bool isIncrementalSync = true,
        CancellationToken cancellationToken = default
    );
}
