using System.Reflection;
using System.Text.RegularExpressions;
using FluentValidation;
using Nop.Core;
using Nop.Core.Caching;
using Nop.Core.Domain.Catalog;
using Nop.Core.Infrastructure;
using Nop.Services.Catalog;
using NopStation.Plugin.B2B.ErpDataScheduler.Services.SyncLogServices;
using NopStation.Plugin.B2B.ErpDataScheduler.Services.SyncWorkflowMessage;
using NopStation.Plugin.B2B.ERPIntegrationCore.Domain;
using NopStation.Plugin.B2B.ERPIntegrationCore.Enums;
using NopStation.Plugin.B2B.ERPIntegrationCore.Model;
using NopStation.Plugin.B2B.ERPIntegrationCore.Services;
using NopStation.Plugin.B2B.ERPIntegrationCore.Validators.Helpers;

namespace NopStation.Plugin.B2B.ErpDataScheduler.Services.SyncTaskServices;

public class ErpSpecSheetSyncService : IErpSpecSheetSyncService
{
    #region Fields

    private readonly IProductService _productService;
    private readonly ISyncLogService _erpSyncLogService;
    private readonly IErpProductService _erpProductService;
    private readonly IErpIntegrationPluginManager _erpIntegrationPluginManager;
    private readonly IErpSalesOrgService _erpSalesOrgService;
    private readonly ISyncWorkflowMessageService _syncWorkflowMessageService;
    private readonly IValidator<Product> _productValidator;
    private readonly INopFileProvider _fileProvider;
    private readonly ErpDataSchedulerSettings _erpDataSchedulerSettings;
    private readonly IStaticCacheManager _staticCacheManager;
    private readonly IStoreContext _storeContext;

    #endregion

    #region Ctor

    public ErpSpecSheetSyncService(
        IProductService productService,
        ISyncLogService erpSyncLogService,
        IErpProductService erpProductService,
        IErpIntegrationPluginManager erpIntegrationPluginManager,
        IErpSalesOrgService erpSalesOrgService,
        ISyncWorkflowMessageService syncWorkflowMessageService,
        IValidator<Product> productValidator,
        INopFileProvider fileProvider,
        ErpDataSchedulerSettings erpDataSchedulerSettings,
        IStaticCacheManager staticCacheManager,
        IStoreContext storeContext)
    {
        _productService = productService;
        _erpSyncLogService = erpSyncLogService;
        _erpProductService = erpProductService;
        _erpIntegrationPluginManager = erpIntegrationPluginManager;
        _erpSalesOrgService = erpSalesOrgService;
        _syncWorkflowMessageService = syncWorkflowMessageService;
        _productValidator = productValidator;
        _fileProvider = fileProvider;
        _erpDataSchedulerSettings = erpDataSchedulerSettings;
        _staticCacheManager = staticCacheManager;
        _storeContext = storeContext;
    }

    #endregion

    #region Utilities

    private async Task<bool> IsThisProductIsValidAsync(Product product, string syncTaskName)
    {
        if (product is null)
            return false;

        var validationResult = await _productValidator.ValidateAsync(product);

        if (!validationResult.IsValid)
        {
            var errorMessages = ErpDataValidationHelper.PrepareValidationLog(validationResult);

            await _erpSyncLogService.SyncLogSaveOnFileAsync(
                syncTaskName,
                ErpSyncLevel.Product,
                $"Data mapping skipped for {nameof(Product)}, {nameof(Product.Sku)}: {product.Sku}. \r\n {errorMessages}");
        }

        return validationResult.IsValid;
    }

    private string GetSpecSheetDirectoryPath()
    {
        var path = _fileProvider.GetAbsolutePath(_erpDataSchedulerSettings.SpecSheetLocation);

        if (!_fileProvider.DirectoryExists(path))
            _fileProvider.CreateDirectory(path);

        return path;
    }

    #endregion

    #region Method

    public virtual async Task<bool> IsErpSpecSheetSyncSuccessfulAsync(
        string? stockCode, 
        string? salesOrgCode = null, 
        bool isManualTrigger = false, 
        bool isIncrementalSync = true, 
        CancellationToken cancellationToken = default
    )
    {
        var syncTaskName = isIncrementalSync ? 
            ErpDataSchedulerDefaults.ErpSpecSheetIncrementalSyncTaskName : 
            ErpDataSchedulerDefaults.ErpSpecSheetSyncTaskName;

        var erpIntegrationPlugin = await _erpIntegrationPluginManager.LoadActiveERPIntegrationPlugin();

        if (erpIntegrationPlugin is null)
        {
            await _erpSyncLogService.SyncLogSaveOnFileAsync(
                syncTaskName,
                ErpSyncLevel.Product,
                $"No integration method found. Unable to run {syncTaskName}.");

            return false;
        }

        var previousStart = "0";
        var lastSyncedErpProduct = string.Empty;

        try
        {
            #region Data Collections

            var salesOrgs = new List<ErpSalesOrg>();
            if (!string.IsNullOrWhiteSpace(salesOrgCode))
            {
                var salesOrg = await _erpSalesOrgService.GetErpSalesOrgByCodeAsync(salesOrgCode);
                if (salesOrg != null)
                {
                    salesOrgs.Add(salesOrg);
                }
            }
            else
            {
                salesOrgs = (await _erpSalesOrgService.GetErpSalesOrgsAsync(isActive: true, filterOutDeleted: true)).ToList();
            }

            if (salesOrgs.Count == 0)
            {
                await _erpSyncLogService.SyncLogSaveOnFileAsync(
                    syncTaskName,
                    ErpSyncLevel.SpecSheet,
                    $"No Sales org found. Unable to run {syncTaskName}.");

                return false;
            }

            var lineBreakReplacer = new Regex(@"\r?\n");
            var programName = Assembly.GetExecutingAssembly().GetName().Name;

            #endregion

            await _erpSyncLogService.SyncLogSaveOnFileAsync(
                syncTaskName,
                ErpSyncLevel.SpecSheet,
                "Erp SpecSheet Sync started.");

            foreach (var salesOrg in salesOrgs)
            {
                var start = "0";
                previousStart = "0";
                var isError = false;
                var totalSyncedSoFar = 0;
                var totalNotSyncedSoFar = 0;
                var limit = 50;
                List<Product> products;

                while (true)
                {
                    var erpGetRequestModel = new ErpGetRequestModel
                    {
                        Start = start,
                        Location = salesOrg.Code,
                        ProductSku = stockCode,
                        DateFrom = isIncrementalSync ? salesOrg.LastErpProductSyncTimeOnUtc : null,
                        Limit = limit
                    };

                    var response = await erpIntegrationPlugin.GetSpecSheetAsync(erpGetRequestModel);

                    if (response.ErpResponseModel.IsError)
                    {
                        isError = true;

                        await _erpSyncLogService.SyncLogSaveOnFileAsync(
                            syncTaskName,
                            ErpSyncLevel.SpecSheet,
                            response.ErpResponseModel.ErrorShortMessage,
                            response.ErpResponseModel.ErrorFullMessage);

                        await _syncWorkflowMessageService.SendSyncFailNotificationAsync(
                            DateTime.UtcNow,
                            syncTaskName,
                            response.ErpResponseModel.ErrorShortMessage + "\n\n" + response.ErpResponseModel.ErrorFullMessage);

                        break;
                    }
                    else if (response.Data is null)
                    {
                        isError = false;
                        break;
                    }

                    previousStart = start;
                    start = response.ErpResponseModel.Next;

                    var responseData = response.Data
                        .Where(x => !string.IsNullOrWhiteSpace(x.Sku))
                        .GroupBy(x => x.Sku)
                        .Select(g => g.Last());

                    if (responseData == null)
                    {
                        isError = false;
                        break;
                    }

                    totalNotSyncedSoFar += response.Data.Count - responseData.Count();

                    products = (List<Product>?)await _erpProductService
                            .GetProductsBySkuAsync(
                                responseData.Select(x => x.Sku.Trim().ToLower()).ToArray(),
                                filterOutDeleted: true,
                                filterOutUnpublished: false);

                    var path = GetSpecSheetDirectoryPath();
                    foreach (var erpProduct in responseData)
                    {
                        var oldErpProduct = products.FirstOrDefault(x => x.Sku.Trim().ToLower() == erpProduct.Sku.Trim().ToLower());

                        var fileName = $"SpecData_{erpProduct.Sku}.pdf";
                        var filePath = _fileProvider.Combine(path, fileName);
                        File.WriteAllBytes(filePath, erpProduct.SpecData);

                        var store = await _storeContext.GetCurrentStoreAsync();

                        if (oldErpProduct != null)
                        {
                            var fileUrl = $"{store.Url.TrimEnd('/')}/{_erpDataSchedulerSettings.SpecSheetLocation.Trim('/')}/{fileName}";
                            oldErpProduct.ShortDescription = $"<a target='_blank' href='{fileUrl}'>Click Here To Open Spec Sheet</a>";

                            if (await IsThisProductIsValidAsync(oldErpProduct, syncTaskName))
                            {
                                await _productService.UpdateProductAsync(oldErpProduct);
                                lastSyncedErpProduct = oldErpProduct.Sku;
                                totalSyncedSoFar++;
                                continue;
                            }
                        }

                        totalNotSyncedSoFar++;
                    }


                    if (cancellationToken.IsCancellationRequested)
                    {
                        await _erpSyncLogService.SyncLogSaveOnFileAsync(
                            syncTaskName,
                            ErpSyncLevel.SpecSheet,
                            $"The Erp SpecSheet Sync run is cancelled for Sales Org: ({salesOrg.Code}) {salesOrg.Name}." +
                            (!string.IsNullOrWhiteSpace(lastSyncedErpProduct) ?
                            $"The last synced Erp SpecSheet: {lastSyncedErpProduct} in this batch. " : string.Empty) +
                            $"Total SpecSheets synced so far: {totalSyncedSoFar} " +
                            $"And total SpecSheets not sync due to invalid data: {totalNotSyncedSoFar}");

                        return false;
                    }

                    await _erpSyncLogService.SyncLogSaveOnFileAsync(
                        syncTaskName,
                        ErpSyncLevel.SpecSheet,
                        (!string.IsNullOrWhiteSpace(lastSyncedErpProduct) ?
                        $"The last synced Erp SpecSheet: {lastSyncedErpProduct} in this batch. " : string.Empty) +
                        $"Total SpecSheet synced so far: {totalSyncedSoFar}");

                    if (response.ErpResponseModel.Next == null)
                    {
                        isError = false;
                        break;
                    }
                }

                if (!isError)
                {
                    //await _erpProductService.UnpublishAllOldProduct(syncStartTime);
                    await _erpSyncLogService.SyncLogSaveOnFileAsync(
                        syncTaskName,
                        ErpSyncLevel.SpecSheet,
                        $"Erp SpecSheet sync successful for Sales Org: ({salesOrg.Code}) {salesOrg.Name}");
                }
                else
                {
                    await _erpSyncLogService.SyncLogSaveOnFileAsync(
                        syncTaskName,
                        ErpSyncLevel.SpecSheet,
                        $"Erp SpecSheet sync is partially or not successful for Sales Org: ({salesOrg.Code}) {salesOrg.Name}");
                }

                await _erpSyncLogService.SyncLogSaveOnFileAsync(
                    syncTaskName,
                    ErpSyncLevel.SpecSheet,
                    (!string.IsNullOrWhiteSpace(lastSyncedErpProduct) ?
                    $"The last synced Erp SpecSheet: {lastSyncedErpProduct}. " : string.Empty) +
                    $"Total SpecSheet synced so far: {totalSyncedSoFar} " +
                    $"And total SpecSheets not sync due to invalid data: {totalNotSyncedSoFar}");

                salesOrg.LastErpProductSyncTimeOnUtc = DateTime.UtcNow;
                await _erpSalesOrgService.UpdateErpSalesOrgAsync(salesOrg);
            }

            await _staticCacheManager.RemoveByPrefixAsync("nop.pres.jcarousel.");
            await _erpSyncLogService.SyncLogSaveOnFileAsync(
                syncTaskName,
                ErpSyncLevel.SpecSheet,
                "Erp SpecSheet Sync ended.");

            return true;
        }
        catch (Exception ex)
        {
            await _staticCacheManager.RemoveByPrefixAsync("nop.pres.jcarousel.");

            await _erpSyncLogService.SyncLogSaveOnFileAsync(
                syncTaskName,
                ErpSyncLevel.SpecSheet,
                ex.Message,
                ex.StackTrace ?? string.Empty);

            await _syncWorkflowMessageService.SendSyncFailNotificationAsync(
                DateTime.UtcNow,
                syncTaskName,
                ex.Message + "\n\n" + ex.StackTrace);

            await _erpSyncLogService.SyncLogSaveOnFileAsync(
                syncTaskName,
                ErpSyncLevel.SpecSheet,
                "Erp SpecSheet Sync ended.");

            return false;
        }
    }

    #endregion
}