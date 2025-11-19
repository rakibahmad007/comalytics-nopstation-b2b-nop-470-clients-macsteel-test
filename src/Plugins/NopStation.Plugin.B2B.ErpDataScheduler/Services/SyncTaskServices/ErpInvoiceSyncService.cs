using FluentValidation;
using Nop.Core.Domain.Directory;
using Nop.Services.Directory;
using NopStation.Plugin.B2B.B2BB2CFeatures;
using NopStation.Plugin.B2B.ErpDataScheduler.Services.SyncLogServices;
using NopStation.Plugin.B2B.ErpDataScheduler.Services.SyncWorkflowMessage;
using NopStation.Plugin.B2B.ERPIntegrationCore.Domain;
using NopStation.Plugin.B2B.ERPIntegrationCore.Enums;
using NopStation.Plugin.B2B.ERPIntegrationCore.Model;
using NopStation.Plugin.B2B.ERPIntegrationCore.Services;
using NopStation.Plugin.B2B.ERPIntegrationCore.Validators.Helpers;

namespace NopStation.Plugin.B2B.ErpDataScheduler.Services.SyncTaskServices;

public class ErpInvoiceSyncService : IErpInvoiceSyncService
{
    #region Fields

    private readonly ICurrencyService _currencyService;
    private readonly CurrencySettings _currencySettings;
    private readonly ISyncLogService _erpSyncLogService;
    private readonly IErpAccountService _erpAccountService;
    private readonly IErpInvoiceService _erpInvoiceService;
    private readonly IErpSalesOrgService _erpSalesOrgService;
    private readonly IErpDataClearCacheService _erpDataClearCacheService;
    private readonly IErpIntegrationPluginManager _erpIntegrationPluginService;
    private readonly IValidator<ErpInvoice> _erpInvoiceValidator;
    private readonly ISyncWorkflowMessageService _syncWorkflowMessageService;
    private readonly B2BB2CFeaturesSettings _b2BB2CFeaturesSettings;

    #endregion

    #region Ctor

    public ErpInvoiceSyncService(ICurrencyService currencyService,
        CurrencySettings currencySettings,
        ISyncLogService erpSyncLogService,
        IErpAccountService erpAccountService,
        IErpInvoiceService erpInvoiceService,
        IErpSalesOrgService erpSalesOrgService,
        IErpDataClearCacheService erpDataClearCacheService,
        IErpIntegrationPluginManager erpIntegrationPluginService,
        IValidator<ErpInvoice> erpInvoiceValidator,
        ISyncWorkflowMessageService syncWorkflowMessageService,
        B2BB2CFeaturesSettings b2BB2CFeaturesSettings)
    {
        _currencyService = currencyService;
        _currencySettings = currencySettings;
        _erpSyncLogService = erpSyncLogService;
        _erpAccountService = erpAccountService;
        _erpInvoiceService = erpInvoiceService;
        _erpSalesOrgService = erpSalesOrgService;
        _erpDataClearCacheService = erpDataClearCacheService;
        _erpIntegrationPluginService = erpIntegrationPluginService;
        _erpInvoiceValidator = erpInvoiceValidator;
        _syncWorkflowMessageService = syncWorkflowMessageService;
        _b2BB2CFeaturesSettings = b2BB2CFeaturesSettings;
    }

    #endregion

    #region Utilities

    private async Task<bool> IsvalidErpInvoiceAsync(ErpInvoice erpInvoice, string syncTaskName)
    {
        if (erpInvoice is null)
            return false;

        var validationResult = await _erpInvoiceValidator.ValidateAsync(erpInvoice);

        if (!validationResult.IsValid)
        {
            var errorMessages = ErpDataValidationHelper.PrepareValidationLog(validationResult);            
            await _erpSyncLogService.SyncLogSaveOnFileAsync(
                syncTaskName,
                ErpSyncLevel.Invoice,
                $"Data mapping skipped for {nameof(ErpInvoice)}, {nameof(ErpInvoice.ErpAccountId)}: {erpInvoice.Id}. \r\n {errorMessages}");
        }

        return validationResult.IsValid;
    }

    #endregion

    #region Method

    private void UpdateInvoiceProperties(ErpInvoice existingInvoice, 
        ErpInvoiceDataModel erpInvoice, 
        int erpAccountId, 
        string currencyCode)
    {
        if (existingInvoice == null || erpInvoice == null)
            return;

        existingInvoice.ShipmentDateUtc = erpInvoice.ShipmentDateUtc;
        existingInvoice.PostingDateUtc = erpInvoice.PostingDateUtc ?? DateTime.UtcNow;
        existingInvoice.DocumentDateUtc = erpInvoice.DocumentDateUtc;
        existingInvoice.ErpDocumentNumber = erpInvoice.ErpDocumentNumber?.Trim();
        existingInvoice.ErpOrderNumber = erpInvoice.ErpOrderNumber;
        existingInvoice.Description = erpInvoice.Description;
        existingInvoice.ErpAccountId = erpAccountId;
        existingInvoice.CurrencyCode = currencyCode;
        existingInvoice.PODSignedById = erpInvoice.PODSignedById;
        existingInvoice.PODSignedOnUtc = erpInvoice.PODSignedOnUtc;
        existingInvoice.RelatedDocumentNo = erpInvoice.RelatedDocumentNo;
        existingInvoice.ItemCount = erpInvoice.Items?.Count ?? 0;
        existingInvoice.DueDateUtc = erpInvoice.DueDateUtc ?? DateTime.UtcNow;
        existingInvoice.AmountExclVat = erpInvoice.AmountExclVat ?? decimal.Zero;
        existingInvoice.AmountInclVat = erpInvoice.AmountInclVat ?? decimal.Zero;
        existingInvoice.DocumentTypeId = erpInvoice.DocumentTypeId;
        existingInvoice.DocumentDisplayName = erpInvoice.DocumentDisplayName ?? string.Empty;
    }

    public virtual async Task<bool> IsErpInvoiceSyncSuccessfulAsync(string? erpAccountNumber,
        string? salesOrgCode = null,
        bool isManualTrigger = false, 
        bool isIncrementalSync = true, 
        CancellationToken cancellationToken = default)
    {
        var syncTaskName = isIncrementalSync ?
            ErpDataSchedulerDefaults.ErpInvoiceIncrementalSyncTaskName :
            ErpDataSchedulerDefaults.ErpInvoiceSyncTaskName;

        var erpIntegrationPlugin = await _erpIntegrationPluginService.LoadActiveERPIntegrationPlugin();

        if (erpIntegrationPlugin is null)
        {
            await _erpSyncLogService.SyncLogSaveOnFileAsync(
                syncTaskName,
                ErpSyncLevel.Invoice,
                $"No integration method found. Unable to run {syncTaskName}.");

            return false;
        }

        try
        {
            #region Data collections

            var salesOrgs = new List<ErpSalesOrg>();

            if (string.IsNullOrWhiteSpace(salesOrgCode))
            {
                salesOrgs = (await _erpSalesOrgService.GetErpSalesOrgsAsync(isActive: true, filterOutDeleted: true)).ToList();
            }
            else
            {
                var salesOrg = await _erpSalesOrgService.GetErpSalesOrgByCodeAsync(salesOrgCode);
                if (salesOrg != null)
                {
                    salesOrgs.Add(salesOrg);
                }
            }

            if (salesOrgs.Count == 0)
            {
                await _erpSyncLogService.SyncLogSaveOnFileAsync(
                    syncTaskName,
                    ErpSyncLevel.Invoice,
                    $"No Sales org found. Unable to run {syncTaskName}.");

                return false;
            }

            IList<ErpAccount> specificErpAccounts = null;
            if (!string.IsNullOrWhiteSpace(salesOrgCode))
            {
                if (!string.IsNullOrWhiteSpace(erpAccountNumber))
                {
                    specificErpAccounts = (await _erpAccountService.GetErpAccountsOfOnlyActiveErpNopUsersAsync
                            (salesOrgId: salesOrgs.FirstOrDefault()?.Id ?? 0, accountNumber: erpAccountNumber)).ToList();

                    if (specificErpAccounts == null || specificErpAccounts != null && specificErpAccounts.Count == 0)
                    {
                        await _erpSyncLogService.SyncLogSaveOnFileAsync(
                            syncTaskName,
                            ErpSyncLevel.Invoice,
                            $"No Active Erp Account found with Active Erp Nop User with Account Number: {erpAccountNumber} " +
                            $"and Sales Org: {salesOrgs.FirstOrDefault()?.Code}. " +
                            $"Unable to run {syncTaskName}.");

                        return false;
                    }
                    else
                    {
                        var accList = string.Join("|", specificErpAccounts.Select(x => x.AccountNumber));
                        await _erpSyncLogService.SyncLogSaveOnFileAsync(
                            syncTaskName,
                            ErpSyncLevel.Invoice,
                            $"Before Invoice Sync run: Accounts discovered, Sales Org: {salesOrgCode}, Account Numbers: [{accList}]");
                    }
                }
                else
                {
                    await _erpSyncLogService.SyncLogSaveOnFileAsync(
                        syncTaskName,
                        ErpSyncLevel.Invoice,
                        $"Invoice Sync will run for Sales Org: {salesOrgCode} related Accounts.");
                }
            }

            var currency = await _currencyService.GetCurrencyByIdAsync(_currencySettings.PrimaryStoreCurrencyId);

            #endregion

            await _erpSyncLogService.SyncLogSaveOnFileAsync(
                syncTaskName,
                ErpSyncLevel.Invoice,
                "Erp Invoice Sync started.");

            foreach (var salesOrg in salesOrgs)
            {
                IList<ErpAccount> oldErpAccounts;

                if (specificErpAccounts != null)
                {
                    if (specificErpAccounts.FirstOrDefault(x => x.ErpSalesOrgId == salesOrg.Id) != null)
                    {
                        oldErpAccounts = specificErpAccounts;
                    }
                    else
                    {
                        continue;
                    }
                }
                else
                {
                    oldErpAccounts = await _erpAccountService.GetErpAccountsOfOnlyActiveErpNopUsersAsync(salesOrgId: salesOrg.Id);
                }

                if (oldErpAccounts.Count == 0)
                {
                    await _erpSyncLogService.SyncLogSaveOnFileAsync(
                        syncTaskName,
                        ErpSyncLevel.Invoice,
                        $"No Erp Accounts found with Active Nop Users for Sales org : {salesOrg.Name}");

                    if (specificErpAccounts != null)
                        return false;

                    continue;
                }

                var lastErpInvoiceSynced = string.Empty;
                var lastErpInvoiceSyncedOfErpAccount = string.Empty;
                var totalSyncedSoFar = 0;
                var totalNotSyncedSoFar = 0;
                var isError = false;
                var lastErrorMessage = "";

                foreach (var erpAccount in oldErpAccounts)
                {
                    var erpInvoiceUpdateList = new List<ErpInvoice>();
                    var erpInvoiceInsertList = new List<ErpInvoice>();

                    var start = "0";
                    var erpInvoices = await _erpInvoiceService.GetErpInvoicesByErpAccountIdAsync(erpAccount.Id);
                    lastErpInvoiceSyncedOfErpAccount = erpAccount.AccountNumber;

                    var existingInvoicesDict = erpInvoices?
                        .ToDictionary(inv => inv.ErpDocumentNumber?.Trim(), inv => inv) ?? 
                        new Dictionary<string, ErpInvoice>();

                    var totalInvoiceForErpAcc = 0;

                    var response = await erpIntegrationPlugin.GetInvoiceByAccountNoFromErpAsync(new ErpGetRequestModel
                    {
                        Start = start,
                        AccountNumber = erpAccount.AccountNumber,
                        Location = salesOrg.Code,
                        DateFrom = isIncrementalSync
                            ? erpAccount.LastTimeOrderSyncOnUtc
                            : DateTime.Today.AddMonths(- _b2BB2CFeaturesSettings.SyncInvoicesForLastXMonths),
                        DateTo = DateTime.Today.AddDays(1)
                    });

                    if (response.ErpResponseModel.IsError)
                    {
                        isError = true;
                        lastErrorMessage = $"The last error: {response.ErpResponseModel.ErrorShortMessage}";

                        await _syncWorkflowMessageService.SendSyncFailNotificationAsync(
                            DateTime.UtcNow,
                            syncTaskName,
                            response.ErpResponseModel.ErrorShortMessage + "\n\n" + response.ErpResponseModel.ErrorFullMessage);

                        continue;
                    }
                    else if (response.Data is null)
                    {
                        isError = false;
                        continue;
                    }

                    start = response.ErpResponseModel.Next;

                    var responseData = response.Data
                        .Where(x => !string.IsNullOrWhiteSpace(x.ErpDocumentNumber?.Trim()))
                        .GroupBy(x => x.ErpDocumentNumber.Trim())
                        .Select(g => g.Last());

                    totalInvoiceForErpAcc += responseData.Count();

                    await _erpSyncLogService.SyncLogSaveOnFileAsync(
                    syncTaskName,
                    ErpSyncLevel.Invoice,
                    $"Total invoice came from SAP : {totalInvoiceForErpAcc}, for Sales org: {salesOrg.Code} and for erpAcc number:{erpAccount.AccountNumber} erpAcc id:{erpAccount.Id}");

                    totalNotSyncedSoFar += response.Data.Count - responseData.Count();

                    foreach (var erpInvoice in responseData)
                    {
                        var documentNumber = erpInvoice.ErpDocumentNumber?.Trim();

                        if (existingInvoicesDict.TryGetValue(documentNumber, out var existingInvoice))
                        {
                            UpdateInvoiceProperties(existingInvoice, erpInvoice, erpAccount.Id, currency.CurrencyCode);
                            if (await IsvalidErpInvoiceAsync(existingInvoice, syncTaskName))
                            {
                                erpInvoiceUpdateList.Add(existingInvoice);
                            }
                            else
                            {
                                totalNotSyncedSoFar++;
                            }
                        }
                        else
                        {
                            var newErpInvoice = new ErpInvoice();
                            UpdateInvoiceProperties(newErpInvoice, erpInvoice, erpAccount.Id, currency.CurrencyCode);

                            if (await IsvalidErpInvoiceAsync(newErpInvoice, syncTaskName))
                            {
                                erpInvoiceInsertList.Add(newErpInvoice);
                                existingInvoicesDict[documentNumber] = newErpInvoice;
                            }
                            else
                            {
                                totalNotSyncedSoFar++;
                            }
                        }
                    }

                    if (erpInvoiceInsertList.Count != 0)
                    {
                        await _erpInvoiceService.InsertErpInvoicesAsync(erpInvoiceInsertList);
                        lastErpInvoiceSynced = erpInvoiceInsertList.LastOrDefault()?.ErpDocumentNumber;
                        totalSyncedSoFar += erpInvoiceInsertList.Count;
                        erpInvoiceInsertList.Clear();
                    }

                    if (erpInvoiceUpdateList.Count != 0)
                    {
                        await _erpInvoiceService.UpdateErpInvoicesAsync(erpInvoiceUpdateList);
                        lastErpInvoiceSynced = erpInvoiceUpdateList.LastOrDefault()?.ErpDocumentNumber;
                        totalSyncedSoFar += erpInvoiceUpdateList.Count;
                        await _erpDataClearCacheService.ClearCacheOfEntities(erpInvoiceUpdateList);
                        erpInvoiceUpdateList.Clear();
                    }

                    if (cancellationToken.IsCancellationRequested)
                    {
                        await _erpSyncLogService.SyncLogSaveOnFileAsync(
                            syncTaskName,
                            ErpSyncLevel.Invoice,
                            "The Erp Invoice Sync run is cancelled. " +
                            (!string.IsNullOrWhiteSpace(lastErpInvoiceSynced) ?
                            $"The last synced Erp Invoice: {lastErpInvoiceSynced}, of Erp Account: {lastErpInvoiceSyncedOfErpAccount} for Sales Org: ({salesOrg.Code}) {salesOrg.Name}. " : string.Empty) +
                            $"Total invoices synced in this session: {totalSyncedSoFar} " +
                            $"And total not synced due to invalid data: {totalNotSyncedSoFar}");

                        return false;
                    }

                    if (response.ErpResponseModel.Next == null)
                    {
                        isError = false;
                        //break;
                    }

                    erpAccount.LastTimeOrderSyncOnUtc = DateTime.UtcNow;
                    await _erpAccountService.UpdateErpAccountAsync(erpAccount);
                }

                if (!isError)
                {
                    await _erpSyncLogService.SyncLogSaveOnFileAsync(
                        syncTaskName,
                        ErpSyncLevel.Invoice,
                        $"Erp Invoice sync successful for Sales Org: ({salesOrg.Code}) {salesOrg.Name}");
                }
                else
                {
                    await _erpSyncLogService.SyncLogSaveOnFileAsync(
                        syncTaskName,
                        ErpSyncLevel.Invoice,
                        $"Erp Invoice sync is partially or not successful for Sales Org: ({salesOrg.Code}) {salesOrg.Name}",
                        lastErrorMessage);
                }

                await _erpSyncLogService.SyncLogSaveOnFileAsync(
                    syncTaskName,
                    ErpSyncLevel.Invoice,
                    (!string.IsNullOrWhiteSpace(lastErpInvoiceSynced) ?
                    $"The last synced Erp Invoice: {lastErpInvoiceSynced}, of Erp Account: {lastErpInvoiceSyncedOfErpAccount} for Sales Org: ({salesOrg.Code}) {salesOrg.Name}. " : string.Empty) +
                    $"Total synced in this session: {totalSyncedSoFar} " +
                    $"And total not synced due to invalid data: {totalNotSyncedSoFar}");
            }

            await _erpSyncLogService.SyncLogSaveOnFileAsync(
                syncTaskName,
                ErpSyncLevel.Invoice,
                "Erp Invoice Sync ended.");

            return true;
        }
        catch (Exception ex)
        {
            await _erpSyncLogService.SyncLogSaveOnFileAsync(
                syncTaskName,
                ErpSyncLevel.Invoice,
                ex.Message,
                ex.StackTrace ?? string.Empty);

            await _syncWorkflowMessageService.SendSyncFailNotificationAsync(
                DateTime.UtcNow,
                syncTaskName,
                ex.Message + "\n\n" + ex.StackTrace);

            await _erpSyncLogService.SyncLogSaveOnFileAsync(
                syncTaskName,
                ErpSyncLevel.Invoice,
                "Erp Invoice Sync ended.");

            return false;
        }
    }

    #endregion
}