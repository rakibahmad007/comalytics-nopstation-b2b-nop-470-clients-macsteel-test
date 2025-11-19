-- ERP Account Incremental Sync
IF NOT EXISTS (SELECT 1 FROM [Erp_Data_Sync_Task] WHERE [Type] = 'NopStation.Plugin.B2B.ErpDataScheduler.Services.SyncTaskServices.ErpAccountIncrementalSyncTask')
BEGIN
    INSERT INTO [Erp_Data_Sync_Task] ([Name], [Type], [Seconds], [Enabled], [StopOnError], [LastEnabledUtc], [QuartzJobName], [IsIncremental])
    VALUES (N'Erp Account Incremental Synchronization', 'NopStation.Plugin.B2B.ErpDataScheduler.Services.SyncTaskServices.ErpAccountIncrementalSyncTask', 3600, 1, 1, GETUTCDATE(), 'ErpAccountIncrementalSync', 1);
END;

-- ERP Invoice Incremental Sync
IF NOT EXISTS (SELECT 1 FROM [Erp_Data_Sync_Task] WHERE [Type] = 'NopStation.Plugin.B2B.ErpDataScheduler.Services.SyncTaskServices.ErpInvoiceIncrementalSyncTask')
BEGIN
    INSERT INTO [Erp_Data_Sync_Task] ([Name], [Type], [Seconds], [Enabled], [StopOnError], [LastEnabledUtc], [QuartzJobName], [IsIncremental])
    VALUES (N'Erp Invoice Incremental Synchronization', 'NopStation.Plugin.B2B.ErpDataScheduler.Services.SyncTaskServices.ErpInvoiceIncrementalSyncTask', 3600, 1, 1, GETUTCDATE(), 'ErpInvoiceIncrementalSync', 1);
END;

-- ERP Product Incremental Sync
IF NOT EXISTS (SELECT 1 FROM [Erp_Data_Sync_Task] WHERE [Type] = 'NopStation.Plugin.B2B.ErpDataScheduler.Services.SyncTaskServices.ErpProductIncrementalSyncTask')
BEGIN
    INSERT INTO [Erp_Data_Sync_Task] ([Name], [Type], [Seconds], [Enabled], [StopOnError], [LastEnabledUtc], [QuartzJobName], [IsIncremental])
    VALUES (N'Erp Product Incremental Synchronization', 'NopStation.Plugin.B2B.ErpDataScheduler.Services.SyncTaskServices.ErpProductIncrementalSyncTask', 3600, 1, 1, GETUTCDATE(), 'ErpProductIncrementalSync', 1);
END;

-- ERP Stock Incremental Sync
IF NOT EXISTS (SELECT 1 FROM [Erp_Data_Sync_Task] WHERE [Type] = 'NopStation.Plugin.B2B.ErpDataScheduler.Services.SyncTaskServices.ErpStockIncrementalSyncTask')
BEGIN
    INSERT INTO [Erp_Data_Sync_Task] ([Name], [Type], [Seconds], [Enabled], [StopOnError], [LastEnabledUtc], [QuartzJobName], [IsIncremental])
    VALUES (N'Erp Stock Incremental Synchronization', 'NopStation.Plugin.B2B.ErpDataScheduler.Services.SyncTaskServices.ErpStockIncrementalSyncTask', 3600, 1, 1, GETUTCDATE(), 'ErpStockIncrementalSync', 1);
END;

-- ERP Special Price Incremental Sync
IF NOT EXISTS (SELECT 1 FROM [Erp_Data_Sync_Task] WHERE [Type] = 'NopStation.Plugin.B2B.ErpDataScheduler.Services.SyncTaskServices.ErpSpecialPriceIncrementalSyncTask')
BEGIN
    INSERT INTO [Erp_Data_Sync_Task] ([Name], [Type], [Seconds], [Enabled], [StopOnError], [LastEnabledUtc], [QuartzJobName], [IsIncremental])
    VALUES (N'Erp Special Price Incremental Synchronization', 'NopStation.Plugin.B2B.ErpDataScheduler.Services.SyncTaskServices.ErpSpecialPriceIncrementalSyncTask', 3600, 1, 1, GETUTCDATE(), 'ErpSpecialPriceIncrementalSync', 1);
END;

-- ERP Group Price Incremental Sync
IF NOT EXISTS (SELECT 1 FROM [Erp_Data_Sync_Task] WHERE [Type] = 'NopStation.Plugin.B2B.ErpDataScheduler.Services.SyncTaskServices.ErpGroupPriceIncrementalSyncTask')
BEGIN
    INSERT INTO [Erp_Data_Sync_Task] ([Name], [Type], [Seconds], [Enabled], [StopOnError], [LastEnabledUtc], [QuartzJobName], [IsIncremental])
    VALUES (N'Erp Group Price Incremental Synchronization', 'NopStation.Plugin.B2B.ErpDataScheduler.Services.SyncTaskServices.ErpGroupPriceIncrementalSyncTask', 3600, 1, 1, GETUTCDATE(), 'ErpGroupPriceIncrementalSync', 1);
END;

-- ERP Ship To Address Incremental Sync
IF NOT EXISTS (SELECT 1 FROM [Erp_Data_Sync_Task] WHERE [Type] = 'NopStation.Plugin.B2B.ErpDataScheduler.Services.SyncTaskServices.ErpShipToAddressIncrementalSyncTask')
BEGIN
    INSERT INTO [Erp_Data_Sync_Task] ([Name], [Type], [Seconds], [Enabled], [StopOnError], [LastEnabledUtc], [QuartzJobName], [IsIncremental])
    VALUES (N'Erp Ship To Address Incremental Synchronization', 'NopStation.Plugin.B2B.ErpDataScheduler.Services.SyncTaskServices.ErpShipToAddressIncrementalSyncTask', 3600, 1, 1, GETUTCDATE(), 'ErpShipToAddressIncrementalSync', 1);
END;

-- ERP Order Incremental Sync
IF NOT EXISTS (SELECT 1 FROM [Erp_Data_Sync_Task] WHERE [Type] = 'NopStation.Plugin.B2B.ErpDataScheduler.Services.SyncTaskServices.ErpOrderIncrementalSyncTask')
BEGIN
    INSERT INTO [Erp_Data_Sync_Task] ([Name], [Type], [Seconds], [Enabled], [StopOnError], [LastEnabledUtc], [QuartzJobName], [IsIncremental])
    VALUES (N'Erp Order Incremental Synchronization', 'NopStation.Plugin.B2B.ErpDataScheduler.Services.SyncTaskServices.ErpOrderIncrementalSyncTask', 3600, 1, 1, GETUTCDATE(), 'ErpOrderIncrementalSync', 1);
END;

-- ERP Spec Sheet Incremental Sync
IF NOT EXISTS (SELECT 1 FROM [Erp_Data_Sync_Task] WHERE [Type] = 'NopStation.Plugin.B2B.ErpDataScheduler.Services.SyncTaskServices.ErpSpecSheetIncrementalSyncTask')
BEGIN
    INSERT INTO [Erp_Data_Sync_Task] ([Name], [Type], [Seconds], [Enabled], [StopOnError], [LastEnabledUtc], [QuartzJobName], [IsIncremental])
    VALUES (N'Erp SpecSheet Incremental Synchronization', 'NopStation.Plugin.B2B.ErpDataScheduler.Services.SyncTaskServices.ErpSpecSheetIncrementalSyncTask', 3600, 1, 1, GETUTCDATE(), 'ErpSpecSheetIncrementalSync', 1);
END;



-- Update IsIncremental for Existing Sync

UPDATE [Erp_Data_Sync_Task]
SET [IsIncremental] = 0
WHERE [Type] IN (
    'NopStation.Plugin.B2B.ErpDataScheduler.Services.SyncTaskServices.ErpAccountSyncTask',
    'NopStation.Plugin.B2B.ErpDataScheduler.Services.SyncTaskServices.ErpInvoiceSyncTask',
    'NopStation.Plugin.B2B.ErpDataScheduler.Services.SyncTaskServices.ErpProductSyncTask',
    'NopStation.Plugin.B2B.ErpDataScheduler.Services.SyncTaskServices.ErpStockSyncTask',
    'NopStation.Plugin.B2B.ErpDataScheduler.Services.SyncTaskServices.ErpSpecialPriceSyncTask',
    'NopStation.Plugin.B2B.ErpDataScheduler.Services.SyncTaskServices.ErpGroupPriceSyncTask',
    'NopStation.Plugin.B2B.ErpDataScheduler.Services.SyncTaskServices.ErpShipToAddressSyncTask',
    'NopStation.Plugin.B2B.ErpDataScheduler.Services.SyncTaskServices.ErpOrderSyncTask',
    'NopStation.Plugin.B2B.ErpDataScheduler.Services.SyncTaskServices.ErpSpecSheetSyncTask'
);
