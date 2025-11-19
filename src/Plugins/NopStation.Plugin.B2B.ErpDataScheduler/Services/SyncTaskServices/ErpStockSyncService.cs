using Nop.Core.Caching;
using Nop.Core.Domain.Catalog;
using Nop.Data;
using Nop.Services.Catalog;
using NopStation.Plugin.B2B.B2BB2CFeatures;
using NopStation.Plugin.B2B.ErpDataScheduler.Domain;
using NopStation.Plugin.B2B.ErpDataScheduler.Services.SyncLogServices;
using NopStation.Plugin.B2B.ErpDataScheduler.Services.SyncWorkflowMessage;
using NopStation.Plugin.B2B.ERPIntegrationCore.Domain;
using NopStation.Plugin.B2B.ERPIntegrationCore.Enums;
using NopStation.Plugin.B2B.ERPIntegrationCore.Model;
using NopStation.Plugin.B2B.ERPIntegrationCore.Services;

namespace NopStation.Plugin.B2B.ErpDataScheduler.Services.SyncTaskServices;

public class ErpStockSyncService : IErpStockSyncService
{
    #region Fields

    private readonly ISyncLogService _erpSyncLogService;
    private readonly IErpProductService _erpProductService;
    private readonly IErpIntegrationPluginManager _erpIntegrationPluginService;
    private readonly IErpSalesOrgService _erpSalesOrgService;
    private readonly IErpWarehouseSalesOrgMapService _erpWarehouseSalesOrgMapService;
    private readonly IStaticCacheManager _staticCacheManager;
    private readonly ISyncWorkflowMessageService _syncWorkflowMessageService;
    private readonly B2BB2CFeaturesSettings _b2BB2CFeaturesSettings;
    private readonly ISpecificationAttributeService _specificationAttributeService;
    private readonly INopDataProvider _nopDataProvider;
    private const string SP_STOCK_RUN_FINISHED = "sp_erpstock_run_finished";

    #endregion Fields

    #region Ctor

    public ErpStockSyncService(ISyncLogService erpSyncLogService,
        IErpProductService erpProductService,
        IErpIntegrationPluginManager erpIntegrationPluginService,
        IErpSalesOrgService erpSalesOrgService,
        IErpWarehouseSalesOrgMapService erpWarehouseSalesOrgMapService,
        IStaticCacheManager staticCacheManager,
        ISyncWorkflowMessageService syncWorkflowMessageService,
        B2BB2CFeaturesSettings b2BB2CFeaturesSettings,
        ISpecificationAttributeService specificationAttributeService,
        INopDataProvider nopDataProvider)
    {
        _erpSyncLogService = erpSyncLogService;
        _erpProductService = erpProductService;
        _erpIntegrationPluginService = erpIntegrationPluginService;
        _erpSalesOrgService = erpSalesOrgService;
        _erpWarehouseSalesOrgMapService = erpWarehouseSalesOrgMapService;
        _staticCacheManager = staticCacheManager;
        _syncWorkflowMessageService = syncWorkflowMessageService;
        _b2BB2CFeaturesSettings = b2BB2CFeaturesSettings;
        _specificationAttributeService = specificationAttributeService;
        _nopDataProvider = nopDataProvider;
    }

    #endregion Ctor

    #region Utilities

    private async Task<string> CustomStockProcessAsync(Product product,
        ErpStockDataModel stockLevel,
        List<SpecificationAttribute> specifiactionAttributes,
        IList<Product> productsToUpdate,
        string syncTaskName)
    {
        if (product == null || stockLevel == null || specifiactionAttributes == null || productsToUpdate == null)
            return string.Empty;

        var enableStockRunFullLogInfo = _b2BB2CFeaturesSettings.EnableStockRunFullLogInfo;
        var fullLogInfo = string.Empty;

        try
        {
            // Set weight on product if not null
            if (stockLevel.Weight.HasValue)
            {
                if (product.Weight != stockLevel.Weight.Value)
                {
                    product.Weight = stockLevel.Weight.Value;
                    if (enableStockRunFullLogInfo)
                        fullLogInfo += $"SKU: {product.Sku}, Weight Updated to {stockLevel.Weight.Value}" + Environment.NewLine;

                    productsToUpdate.Add(product);
                }
                else
                {
                    if (enableStockRunFullLogInfo)
                        fullLogInfo += $"SKU: {product.Sku}, Weight update not required." + Environment.NewLine;
                }
            }
            else
            {
                if (enableStockRunFullLogInfo)
                    fullLogInfo += $"SKU: {product.Sku}, Stock data don't have Weight value." + Environment.NewLine;
            }

            // Get uom spec attribute and cache it if needed
            int? uomSpecId = specifiactionAttributes?.Find(x => x.Name == "UOM")?.Id ?? 0;
            if (uomSpecId == 0)
                return fullLogInfo; // No point in continuing if we dont have uom spec attribute

            // Check if the uom value already exists
            var options = await _specificationAttributeService.GetSpecificationAttributeOptionsBySpecificationAttributeAsync(uomSpecId.Value);

            var selectedOption = options.FirstOrDefault(op => op.Name == stockLevel.UnitOfMeasure);

            // Creates selected option if it doesnt exist
            if (selectedOption == null)
            {
                selectedOption = new SpecificationAttributeOption
                {
                    SpecificationAttributeId = uomSpecId.Value,
                    Name = stockLevel.UnitOfMeasure
                };
                await _specificationAttributeService.InsertSpecificationAttributeOptionAsync(selectedOption);
            }

            var mapping = (await _specificationAttributeService.GetProductSpecificationAttributesAsync(product.Id, selectedOption.Id))?.FirstOrDefault();

            // Insert spec attribute mapping
            if (mapping == null)
            {
                mapping = new ProductSpecificationAttribute
                {
                    ProductId = product.Id,
                    SpecificationAttributeOptionId = selectedOption.Id,
                    AttributeTypeId = (int)SpecificationAttributeType.Option
                };
                await _specificationAttributeService.InsertProductSpecificationAttributeAsync(mapping);
            }
        }
        catch (Exception e)
        {
            await _erpSyncLogService.SyncLogSaveOnFileAsync(
                syncTaskName,
                ErpSyncLevel.Stock, $"Error on CustomStockProcess: {e.Message} - {e.InnerException}");
        }

        return fullLogInfo;
    }

    private async Task ExecuteStoredProcedureAsync(string spName, string syncTaskName)
    {
        try
        {
            var sql = $"IF object_id('{spName}') IS NOT NULL EXEC {spName}";
            await _nopDataProvider.ExecuteNonQueryAsync(sql);
        }
        catch (Exception ex)
        {
            await _erpSyncLogService.SyncLogSaveOnFileAsync(
                syncTaskName,
                ErpSyncLevel.Stock,
                ex.Message,
                ex.StackTrace ?? string.Empty);
        }
    }

    private async Task<IList<CommonSalesOrgWarehouseDto>> GetDistinctSalesOrgWarehouseMapsBySalesOrgIdAsync(int salesOrgId)
    {
        var salesOrgWarehouseMaps = await _erpWarehouseSalesOrgMapService.GetSaleOrgWarehousebySalesOrgIdAsync(salesOrgId);

        var distinctWarehouseCodes = new List<CommonSalesOrgWarehouseDto>();

        foreach (var map in salesOrgWarehouseMaps)
        {
            if (map == null || string.IsNullOrWhiteSpace(map.WarehouseCode))
                continue;

            var existing = distinctWarehouseCodes.FirstOrDefault(x => x.WarehouseCode == map.WarehouseCode);

            if (existing == null)
            {
                existing = new CommonSalesOrgWarehouseDto
                {
                    ErpSalesOrgId = map.ErpSalesOrgId,
                    NopWarehouseId = map.NopWarehouseId,
                    WarehouseCode = map.WarehouseCode,
                    LastSyncedOnUtc = map.LastSyncedOnUtc
                };

                distinctWarehouseCodes.Add(existing);
            }

            if (map.IsB2CWarehouse)
                existing.B2CSalesOrgWarehouseId = map.Id;
            else
                existing.B2BSalesOrgWarehouseId = map.Id;
        }

        return distinctWarehouseCodes;
    }

    private async Task UpdateLastSyncedOnUtcOfWarehouse(int salesOrgWarehouseId)
    {
        var map = await _erpWarehouseSalesOrgMapService.GetErpWarehouseSalesOrgMapByIdAsync(salesOrgWarehouseId);
        if (map != null)
        {
            map.LastSyncedOnUtc = DateTime.UtcNow;
            await _erpWarehouseSalesOrgMapService.UpdateErpWarehouseSalesOrgMapAsync(map);
        }
    }

    #endregion Utilities

    #region Method

    public virtual async Task<bool> IsErpStockSyncSuccessfulAsync(string? stockCode,
        string? salesOrgCode = null,
        bool isManualTrigger = false,
        bool isIncrementalSync = true,
        CancellationToken cancellationToken = default)
    {
        var syncTaskName = isIncrementalSync ?
           ErpDataSchedulerDefaults.ErpStockIncrementalSyncTaskName :
           ErpDataSchedulerDefaults.ErpStockSyncTaskName;

        var erpIntegrationPlugin = await _erpIntegrationPluginService.LoadActiveERPIntegrationPlugin();

        if (erpIntegrationPlugin is null)
        {
            await _erpSyncLogService.SyncLogSaveOnFileAsync(
                syncTaskName,
                ErpSyncLevel.Stock,
                $"No integration method found. Unable to run {syncTaskName}.");

            return false;
        }

        try
        {
            #region Data collection

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
                    ErpSyncLevel.Stock,
                    $"No Sales org found{(string.IsNullOrWhiteSpace(salesOrgCode) ?
                        "" :
                        $" with code '{salesOrgCode}'")}. Unable to run {syncTaskName}.");

                return false;
            }

            #endregion Data collection

            await _erpSyncLogService.SyncLogSaveOnFileAsync(
                syncTaskName,
                ErpSyncLevel.Stock,
                "Erp Stock Sync started.");

            var specifiactionAttributes = (await _specificationAttributeService.GetSpecificationAttributesAsync())?.ToList();

            foreach (var salesOrg in salesOrgs)
            {
                var erpSalesOrgWarehouseMaps = await GetDistinctSalesOrgWarehouseMapsBySalesOrgIdAsync(salesOrg.Id);
                if (erpSalesOrgWarehouseMaps == null || erpSalesOrgWarehouseMaps.Count == 0)
                    continue;

                foreach (var warehouse in erpSalesOrgWarehouseMaps)
                {
                    if (warehouse == null || string.IsNullOrEmpty(warehouse.WarehouseCode))
                        continue;

                    var start = "0";
                    var isError = false;
                    var lastErpProductStockSynced = string.Empty;
                    var totalSyncedSoFar = 0;
                    var totalNotSyncedSoFar = 0;

                    while (true)
                    {
                        var erpGetRequestModel = new ErpGetRequestModel
                        {
                            Start = start,
                            DateFrom = isIncrementalSync ? warehouse.LastSyncedOnUtc : null,
                            WarehouseCode = warehouse?.WarehouseCode,
                            ProductSku = stockCode,
                        };

                        var response = await erpIntegrationPlugin.GetStocksFromErpAsync(erpGetRequestModel);

                        if (response.ErpResponseModel.IsError)
                        {
                            isError = true;

                            await _erpSyncLogService.SyncLogSaveOnFileAsync(
                                syncTaskName,
                                ErpSyncLevel.Stock,
                                response.ErpResponseModel?.ErrorShortMessage ?? string.Empty,
                                response.ErpResponseModel?.ErrorFullMessage ?? string.Empty);

                            await _syncWorkflowMessageService.SendSyncFailNotificationAsync(
                                DateTime.UtcNow,
                                syncTaskName,
                                response.ErpResponseModel?.ErrorShortMessage + "\n\n" + response.ErpResponseModel?.ErrorFullMessage);

                            break;
                        }
                        else if (response.Data is null || !response.Data.Any())
                        {
                            isError = false;
                            break;
                        }

                        start = response.ErpResponseModel.Next;

                        var pwiToInsert = new List<ProductWarehouseInventory>();
                        var pwiToUpdate = new List<ProductWarehouseInventory>();
                        var stockQuantityHistoriesToInsert = new List<StockQuantityHistory>();
                        var productsToUpdate = new List<Product>();

                        var responseData = response.Data
                                .Where(x => !string.IsNullOrWhiteSpace(x.Sku.Trim().ToLower()) && !string.IsNullOrWhiteSpace(x.WarehouseNameOrCode))
                                .GroupBy(x => new { Sku = x.Sku.Trim().ToLower(), WarehouseCode = x.WarehouseNameOrCode })
                                .Select(g => g.Last());

                        totalNotSyncedSoFar += response.Data.Count - responseData.Count();

                        var products = await _erpProductService
                            .GetProductsBySkuAsync(
                                responseData
                                .Select(x => x.Sku.Trim().ToLower()).ToArray(),
                                filterOutDeleted: true
                            );

                        if (products is null || products.Count == 0)
                        {
                            totalNotSyncedSoFar += response.Data.Count;
                            isError = false;
                            break;
                        }

                        var inventories = await _erpProductService.GetProductWarehouseInventoryByProductIdsAndNopWarehouseIdsAsync(products.Select(x => x.Id).ToArray(), warehouse?.NopWarehouseId ?? 0);

                        var fullLogInfo = string.Empty;
                        var enableStockRunFullLogInfo = _b2BB2CFeaturesSettings.EnableStockRunFullLogInfo;

                        foreach (var erpStock in responseData)
                        {
                            if (string.IsNullOrWhiteSpace(erpStock.Sku) || !erpStock.QuantityOnHand.HasValue)
                            {
                                totalNotSyncedSoFar++;
                                continue;
                            }

                            var product = products.FirstOrDefault(x => x.Sku.Trim().ToLower().Equals(erpStock.Sku.Trim().ToLower()));
                            if (product == null || product.Id == 0)
                            {
                                totalNotSyncedSoFar++;
                                continue;
                            }

                            if (!string.IsNullOrEmpty(erpStock.WarehouseNameOrCode))
                            {
                                // no need to get warehouse again based on the response, we already have it in the loop
                                // will test, if okay, remove unused code
                                // var erpWarehouse = await _erpWarehouseSalesOrgMapService.GetErpWarehouseSalesOrgMapByWarehouseCodeAsync(erpStock.WarehouseNameOrCode.Trim());

                                var erpWarehouse = warehouse;

                                if (erpWarehouse != null && erpWarehouse.NopWarehouseId > 0)
                                {
                                    var inventory = inventories.Find(x => x.ProductId == product.Id && x.WarehouseId == erpWarehouse.NopWarehouseId);
                                    if (inventory == null)
                                    {
                                        inventory = new ProductWarehouseInventory
                                        {
                                            ProductId = product.Id,
                                            StockQuantity = (int)erpStock.QuantityOnHand.Value,
                                            WarehouseId = erpWarehouse.NopWarehouseId,
                                            ReservedQuantity = 0
                                        };

                                        pwiToInsert.Add(inventory);
                                    }
                                    else
                                    {
                                        if (inventory.StockQuantity != (int)erpStock.QuantityOnHand || inventory.ReservedQuantity > 0)
                                        {
                                            var quantityAdjustment = (int)erpStock.QuantityOnHand - inventory.StockQuantity;

                                            inventory.ReservedQuantity = 0;
                                            inventory.StockQuantity = (int)erpStock.QuantityOnHand;
                                            pwiToUpdate.Add(inventory);

                                            var stockQuantityHistory = new StockQuantityHistory();
                                            stockQuantityHistory.QuantityAdjustment = quantityAdjustment;
                                            stockQuantityHistory.StockQuantity = (int)erpStock.QuantityOnHand;
                                            stockQuantityHistory.CreatedOnUtc = DateTime.UtcNow;
                                            stockQuantityHistory.ProductId = inventory.ProductId;
                                            stockQuantityHistory.WarehouseId = erpWarehouse.NopWarehouseId;
                                            stockQuantityHistory.Message = $"Product Stock updated. The stock quantity has been updated by Erp Integration.";
                                            stockQuantityHistoriesToInsert.Add(stockQuantityHistory);
                                        }
                                        else
                                        {
                                            if (enableStockRunFullLogInfo)
                                                fullLogInfo += $"SKU: {erpStock.Sku}, Stock update not required for {erpStock.WarehouseNameOrCode}" + Environment.NewLine;
                                        }
                                    }

                                    var customStockProcessLog = await CustomStockProcessAsync(
                                        product,
                                        erpStock,
                                        specifiactionAttributes,
                                        productsToUpdate,
                                        syncTaskName);

                                    fullLogInfo += customStockProcessLog;

                                    product.StockQuantity = 0;
                                    product.ManageInventoryMethodId = (int)ManageInventoryMethod.ManageStock;
                                    product.UseMultipleWarehouses = true;
                                }
                            }
                            else
                            {
                                product.ManageInventoryMethodId = (int)ManageInventoryMethod.ManageStock;
                                product.StockQuantity = Convert.ToInt32(Math.Min(Math.Max(Math.Round(erpStock.QuantityOnHand ?? 0), int.MinValue), int.MaxValue));
                                product.UseMultipleWarehouses = false;
                            }

                            if (enableStockRunFullLogInfo)
                            {
                                await _erpSyncLogService.SyncLogSaveOnFileAsync(
                                    syncTaskName,
                                    ErpSyncLevel.Stock,
                                    $"A batch completed for updating stock levels for warehouse '{warehouse?.WarehouseCode}'{Environment.NewLine}{fullLogInfo}");
                                fullLogInfo = string.Empty;
                            }

                            lastErpProductStockSynced = product.Sku;
                            totalSyncedSoFar++;
                        }

                        await _erpProductService.UpdateBulkProductWarehouseInventoryAsync(pwiToUpdate);
                        await _erpProductService.InsertBulkProductWarehouseInventoryAsync(pwiToInsert);
                        await _erpProductService.InsertBulkStockQuantityHistoryAsync(stockQuantityHistoriesToInsert);
                        await _erpProductService.UpdateProductsAsync(products);

                        pwiToUpdate.Clear();
                        pwiToInsert.Clear();
                        stockQuantityHistoriesToInsert.Clear();
                        productsToUpdate.Clear();

                        if (cancellationToken.IsCancellationRequested)
                        {
                            await _erpSyncLogService.SyncLogSaveOnFileAsync(
                                syncTaskName,
                                ErpSyncLevel.Stock,
                                "The Erp Stock sync run is cancelled. " +
                                (!string.IsNullOrWhiteSpace(lastErpProductStockSynced) ?
                                $"The last synced Stock of Product : {lastErpProductStockSynced}. " : string.Empty) +
                                $"Total product stock synced so far: {totalSyncedSoFar}, " +
                                $"And total not synced due to invalid data or product not found: {totalNotSyncedSoFar}");

                            return false;
                        }

                        await _erpSyncLogService.SyncLogSaveOnFileAsync(
                            syncTaskName,
                            ErpSyncLevel.Stock,
                            (!string.IsNullOrWhiteSpace(lastErpProductStockSynced) ?
                            $"The last synced Stock of Product : {lastErpProductStockSynced} in this batch. " : string.Empty) +
                            $"Total product stock synced so far: {totalSyncedSoFar}, " +
                            $"And total not synced due to invalid data or product not found: {totalNotSyncedSoFar}" +
                            $"Here salesOrg code : {salesOrg.Code} and warehouse code: {warehouse.WarehouseCode}");

                        if (response.ErpResponseModel.Next == null)
                        {
                            isError = false;
                            break;
                        }
                    }

                    if (!isError)
                    {
                        await _erpSyncLogService.SyncLogSaveOnFileAsync(
                            syncTaskName,
                            ErpSyncLevel.Stock,
                            $"Erp Stock sync successful for Sales Org: ({salesOrg.Code}) {salesOrg.Name}.");
                    }
                    else
                    {
                        await _erpSyncLogService.SyncLogSaveOnFileAsync(
                            syncTaskName,
                            ErpSyncLevel.Stock,
                            $"Erp Stock sync is partially or not successful for Sales Org: ({salesOrg.Code}) {salesOrg.Name}.");
                    }

                    // using this, exec 2 SPs - sp_remove_stockless_products_from_specials and sp_remove_products_from_specials_of_wrong_warehouse
                    await ExecuteStoredProcedureAsync(SP_STOCK_RUN_FINISHED, syncTaskName);

                    await _erpSyncLogService.SyncLogSaveOnFileAsync(
                        syncTaskName,
                        ErpSyncLevel.Stock,
                        (!string.IsNullOrWhiteSpace(lastErpProductStockSynced) ?
                        $"The last synced Stock of Product: {lastErpProductStockSynced} for Sales Org: ({salesOrg.Code}) {salesOrg.Name}. " : string.Empty) +
                        $"Total product stock synced so far: {totalSyncedSoFar}, " +
                        $"And total not synced due to invalid data or product not found: {totalNotSyncedSoFar}");


                    await UpdateLastSyncedOnUtcOfWarehouse(warehouse.B2BSalesOrgWarehouseId);
                    await UpdateLastSyncedOnUtcOfWarehouse(warehouse.B2CSalesOrgWarehouseId);
                    salesOrg.LastErpStockSyncTimeOnUtc = DateTime.UtcNow;
                    await _erpSalesOrgService.UpdateErpSalesOrgAsync(salesOrg);
                }
            }

            await _staticCacheManager.RemoveByPrefixAsync("nop.pres.jcarousel.");
            await _erpSyncLogService.SyncLogSaveOnFileAsync(
                syncTaskName,
                ErpSyncLevel.Stock,
                "Erp Stock Sync ended.");

            return true;
        }
        catch (Exception ex)
        {
            await _staticCacheManager.RemoveByPrefixAsync("nop.pres.jcarousel.");
            await _erpSyncLogService.SyncLogSaveOnFileAsync(
                syncTaskName,
                ErpSyncLevel.Stock,
                ex.Message,
                ex.StackTrace ?? string.Empty);

            await _syncWorkflowMessageService.SendSyncFailNotificationAsync(
                DateTime.UtcNow,
                syncTaskName,
                ex.Message + "\n\n" + ex.StackTrace);

            await _erpSyncLogService.SyncLogSaveOnFileAsync(
                syncTaskName,
                ErpSyncLevel.Stock,
                "Erp Stock Sync ended.");

            return false;
        }
    }

    #endregion Method
}