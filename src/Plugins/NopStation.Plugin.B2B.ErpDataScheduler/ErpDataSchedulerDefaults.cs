namespace NopStation.Plugin.B2B.ErpDataScheduler;

public static class ErpDataSchedulerDefaults
{
    public static string ErpAccountSyncTask =>
        "NopStation.Plugin.B2B.ErpDataScheduler.Services.SyncTaskServices.ErpAccountSyncTask";

    public static string ErpAccountSyncTaskName => "Erp Account Synchronization";

    public static string ErpAccountIncrementalSyncTask =>
        "NopStation.Plugin.B2B.ErpDataScheduler.Services.SyncTaskServices.ErpAccountIncrementalSyncTask";

    public static string ErpAccountIncrementalSyncTaskName =>
        "Erp Account Incremental Synchronization";

    public static string ErpInvoiceSyncTask =>
        "NopStation.Plugin.B2B.ErpDataScheduler.Services.SyncTaskServices.ErpInvoiceSyncTask";

    public static string ErpInvoiceSyncTaskName => "Erp Invoice Synchronization";

    public static string ErpInvoiceIncrementalSyncTask =>
        "NopStation.Plugin.B2B.ErpDataScheduler.Services.SyncTaskServices.ErpInvoiceIncrementalSyncTask";

    public static string ErpInvoiceIncrementalSyncTaskName =>
        "Erp Invoice Incremental Synchronization";

    public static string ErpProductSyncTask =>
        "NopStation.Plugin.B2B.ErpDataScheduler.Services.SyncTaskServices.ErpProductSyncTask";

    public static string ErpProductSyncTaskName => "Erp Product Synchronization";

    public static string ErpProductIncrementalSyncTask =>
        "NopStation.Plugin.B2B.ErpDataScheduler.Services.SyncTaskServices.ErpProductIncrementalSyncTask";

    public static string ErpProductIncrementalSyncTaskName =>
        "Erp Product Incremental Synchronization";

    public static string ErpStockSyncTask =>
        "NopStation.Plugin.B2B.ErpDataScheduler.Services.SyncTaskServices.ErpStockSyncTask";

    public static string ErpStockSyncTaskName => "Erp Stock Synchronization";

    public static string ErpStockIncrementalSyncTask =>
        "NopStation.Plugin.B2B.ErpDataScheduler.Services.SyncTaskServices.ErpStockIncrementalSyncTask";

    public static string ErpStockIncrementalSyncTaskName => "Erp Stock Incremental Synchronization";

    public static string ErpOrderSyncTask =>
        "NopStation.Plugin.B2B.ErpDataScheduler.Services.SyncTaskServices.ErpOrderSyncTask";

    public static string ErpOrderSyncTaskName => "Erp Order Synchronization";

    public static string ErpOrderIncrementalSyncTask =>
        "NopStation.Plugin.B2B.ErpDataScheduler.Services.SyncTaskServices.ErpOrderIncrementalSyncTask";

    public static string ErpOrderIncrementalSyncTaskName => "Erp Order Incremental Synchronization";

    public static string ErpSpecialPriceSyncTask =>
        "NopStation.Plugin.B2B.ErpDataScheduler.Services.SyncTaskServices.ErpSpecialPriceSyncTask";

    public static string ErpSpecialPriceSyncTaskName => "Erp Special Price Synchronization";

    public static string ErpSpecialPriceIncrementalSyncTask =>
        "NopStation.Plugin.B2B.ErpDataScheduler.Services.SyncTaskServices.ErpSpecialPriceIncrementalSyncTask";

    public static string ErpSpecialPriceIncrementalSyncTaskName =>
        "Erp Special Price Incremental Synchronization";

    public static string ErpGroupPriceSyncTask =>
        "NopStation.Plugin.B2B.ErpDataScheduler.Services.SyncTaskServices.ErpGroupPriceSyncTask";

    public static string ErpGroupPriceSyncTaskName => "Erp Group Price Synchronization";

    public static string ErpGroupPriceIncrementalSyncTask =>
        "NopStation.Plugin.B2B.ErpDataScheduler.Services.SyncTaskServices.ErpGroupPriceIncrementalSyncTask";

    public static string ErpGroupPriceIncrementalSyncTaskName =>
        "Erp Group Price Incremental Synchronization";

    public static string ErpShipToAddressSyncTask =>
        "NopStation.Plugin.B2B.ErpDataScheduler.Services.SyncTaskServices.ErpShipToAddressSyncTask";

    public static string ErpShipToAddressSyncTaskName => "Erp Ship To Address Synchronization";

    public static string ErpShipToAddressIncrementalSyncTask =>
        "NopStation.Plugin.B2B.ErpDataScheduler.Services.SyncTaskServices.ErpShipToAddressIncrementalSyncTask";

    public static string ErpShipToAddressIncrementalSyncTaskName =>
        "Erp Ship To Address Incremental Synchronization";

    public static string ErpSpecSheetSyncTask =>
        "NopStation.Plugin.B2B.ErpDataScheduler.Services.SyncTaskServices.ErpSpecSheetSyncTask";

    public static string ErpSpecSheetSyncTaskName => "Erp SpecSheet Synchronization";

    public static string ErpSpecSheetIncrementalSyncTask =>
        "NopStation.Plugin.B2B.ErpDataScheduler.Services.SyncTaskServices.ErpSpecSheetIncrementalSyncTask";

    public static string ErpSpecSheetIncrementalSyncTaskName =>
        "Erp SpecSheet Incremental Synchronization";

    public static string SyncLogFileSaveDefaultPath => "SyncLogs\\";

    public static string SyncLogFileExtension => "txt";

    public static string SyncLogFileDeleteTask =>
        "NopStation.Plugin.B2B.ErpDataScheduler.Services.SyncLogServices.SyncLogFileDeleteTask";

    public static string SyncLogFileDeleteTaskName => "Sync Log Files Delete";

    public static int DefaultSyncLogFileDeleteTaskInverval => 3600;

    public static string ErpAccountSyncTaskIdentity => "ErpAccountSync";

    public static string ErpAccountIncrementalSyncTaskIdentity => "ErpAccountIncrementalSync";

    public static string ErpGroupPriceSyncTaskIdentity => "ErpGroupPriceSync";

    public static string ErpGroupPriceIncrementalSyncTaskIdentity => "ErpGroupPriceIncrementalSync";

    public static string ErpInvoiceSyncTaskIdentity => "ErpInvoiceSync";

    public static string ErpInvoiceIncrementalSyncTaskIdentity => "ErpInvoiceIncrementalSync";

    public static string ErpOrderSyncTaskIdentity => "ErpOrderSync";

    public static string ErpOrderIncrementalSyncTaskIdentity => "ErpOrderIncrementalSync";

    public static string ErpProductSyncTaskIdentity => "ErpProductSync";

    public static string ErpProductIncrementalSyncTaskIdentity => "ErpProductIncrementalSync";

    public static string ErpShipToAddressSyncTaskIdentity => "ErpShipToAddressSync";

    public static string ErpShipToAddressIncrementalSyncTaskIdentity =>
        "ErpShipToAddressIncrementalSync";

    public static string ErpSpecialPriceSyncTaskIdentity => "ErpSpecialPriceSync";

    public static string ErpSpecialPriceIncrementalSyncTaskIdentity =>
        "ErpSpecialPriceIncrementalSync";

    public static string ErpStockSyncTaskIdentity => "ErpStockSync";

    public static string ErpStockIncrementalSyncTaskIdentity => "ErpStockIncrementalSync";

    public static string ErpSpecSheetSyncTaskIdentity => "ErpSpecSheetSync";

    public static string ErpSpecSheetIncrementalSyncTaskIdentity => "ErpSpecSheetIncrementalSync";

    public static string JobShouldExecute => "JobShouldExecute";

    public static string IsManualTrigger => "ManualTrigger";

    public static string IsIncrementalSync => "IncrementalSync";

    public static string SyncFailedNotificationMessageTemplate =>
        "SyncFailedNotificationMessageTemplate";
}
