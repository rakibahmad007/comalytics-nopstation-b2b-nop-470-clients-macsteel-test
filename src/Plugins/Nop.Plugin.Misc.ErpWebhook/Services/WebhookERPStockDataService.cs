using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Core.Domain.Catalog;
using Nop.Data;
using Nop.Plugin.Misc.ErpWebhook.Domain.ParallelTables;
using Nop.Plugin.Misc.ErpWebhook.Models.ErpStock;
using Nop.Plugin.Misc.ErpWebhook.Services.Interfaces;
using NopStation.Plugin.B2B.ERPIntegrationCore.Domain;

namespace Nop.Plugin.Misc.ErpWebhook.Services
{
    public class WebhookERPStockDataService : IWebhookERPStockDataService
    {
        #region fields

        private ErpWebhookConfig _erpWebhookConfig = null;
        private readonly ErpWebhookSettings _webhookSettings;
        private readonly IRepository<ErpWarehouseSalesOrgMap> _erpWarehouseSalesOrgMap;
        private readonly IRepository<ErpSalesOrg> _erpSalesOrgRepo;
        private readonly IRepository<ProductWarehouseInventory> _pwiRepo;
        private readonly IRepository<Product> _productRepo;
        private readonly IRepository<Parallel_ErpStock> _erpStockRepo;
        private readonly IErpWebhookService _erpWebhookService;
        private readonly IWorkContext _workContext;

        #endregion

        #region ctor

        public WebhookERPStockDataService(ErpWebhookSettings webhookSettings,
            IRepository<ErpWarehouseSalesOrgMap> erpWarehouseSalesOrgMap,
            IRepository<ErpSalesOrg> erpSalesOrgRepo,
            IRepository<ProductWarehouseInventory> pwiRepo,
            IRepository<Product> productRepo,
            IRepository<Parallel_ErpStock> erpStockRepo,
            IErpWebhookService erpWebhookService,
            IWorkContext workContext)
        {
            _webhookSettings = webhookSettings;
            _erpWarehouseSalesOrgMap = erpWarehouseSalesOrgMap;
            _erpSalesOrgRepo = erpSalesOrgRepo;
            _pwiRepo = pwiRepo;
            _productRepo = productRepo;
            _erpStockRepo = erpStockRepo;
            _erpWebhookService = erpWebhookService;
            _workContext = workContext;
        }

        #endregion

        #region utils

        public async Task<ILookup<string, ProductWarehouseInventory>> GetProductWarehouseInventoriesAsync(int warehouseId, List<string> skus)
        {
            var productIdToSkuDict = await _productRepo.Table
                .Where(p => skus.Contains(p.Sku) && !p.Deleted)
                .Select(p => new { p.Id, p.Sku })
                .ToDictionaryAsync(p => p.Id, p => p.Sku);

            List<int> prodIds = productIdToSkuDict.Keys.ToList();

            var existingPwis = await _pwiRepo.Table
                .Where(pwi => pwi.WarehouseId == warehouseId && prodIds.Contains(pwi.ProductId))
                .ToListAsync();

            var missingPwis = prodIds.Except(existingPwis.Select(pwi => pwi.ProductId))
                .Select(prodId => new ProductWarehouseInventory()
                {
                    ProductId = prodId,
                    WarehouseId = warehouseId,
                    StockQuantity = 0,
                    ReservedQuantity = 0,
                })
                .ToList();

            await _pwiRepo.InsertAsync(missingPwis);

            return existingPwis.Concat(missingPwis)
                .ToLookup(pwi => productIdToSkuDict[pwi.ProductId]);
        }

        public async Task<int> GetSalesOrganisationIdAsync(string location)
        {
            var b2bSalesOrgId = _erpWebhookConfig?.DefaultSalesOrgId ?? 1;

            if (!string.IsNullOrWhiteSpace(location))
                b2bSalesOrgId = await _erpSalesOrgRepo.Table.Where(x => x.Code.Equals(location)).Select(x => x.Id).FirstOrDefaultAsync();

            return b2bSalesOrgId;
        }

        public async Task<IEnumerable<ErpWarehouseSalesOrgMap>> GetWarehousesAsync(string location)
        {
            int salesOrgId = await GetSalesOrganisationIdAsync(location);
            return await _erpWarehouseSalesOrgMap.Table
                .Where(w => w.ErpSalesOrgId == salesOrgId)
                .OrderBy(w => w.WarehouseCode)
                .ToListAsync();
        }

        #endregion

        #region methods

        public async Task ProcessERPStockDataAsync(List<ErpStockLevelModel> erpStockLevels)
        {
            _erpWebhookConfig = await _erpWebhookService.LoadErpWebhookConfigsFromJsonAsync();

            if (erpStockLevels == null || !erpStockLevels.Any())
                return;
            var location = erpStockLevels.Select(x => x.Location).FirstOrDefault();
            foreach (var warehouse in await GetWarehousesAsync(location))
            {
                Dictionary<string, ErpStockLevelModel> updates = erpStockLevels
                                        .Where(x => !string.IsNullOrEmpty(x.ItemNo))
                                        .GroupBy(x => x.ItemNo, StringComparer.InvariantCultureIgnoreCase)
                                        .ToDictionary(x => x.Key, x => x.First(), StringComparer.InvariantCultureIgnoreCase);
                List<string> itemNos = updates.Keys.ToList();
                ILookup<string, ProductWarehouseInventory> pwis = await GetProductWarehouseInventoriesAsync(warehouse.NopWarehouseId, itemNos);
                foreach (var kvp in updates)
                {
                    string productSku = kvp.Key;
                    ErpStockLevelModel erpStockLevel = kvp.Value;
                    var pwisForThisSku = pwis[productSku].ToList();
                    if (!pwis.Any())
                    {
                        continue;
                    }

                    foreach (var pwi in pwisForThisSku)
                    {
                        int productId = pwi.ProductId;
                        pwi.StockQuantity = erpStockLevel.TotalOnHand;
                        pwi.ReservedQuantity = 0;
                        await _pwiRepo.UpdateAsync(pwi);
                    }
                }
            }
        }

        public async Task ProcessERPStockDataToParallelTableAsync(List<ErpStockLevelModel> erpStockLevels)
        {
            if (!erpStockLevels.Any())
                return;

            _erpWebhookConfig = await _erpWebhookService.LoadErpWebhookConfigsFromJsonAsync();
            var currentCustomerId = (await _workContext.GetCurrentCustomerAsync()).Id;
            var erpStockToAdd = new List<Parallel_ErpStock>();

            // Check if similar entities exist in the database
            var existingErpStock = await (from obj in erpStockLevels
                                   join dbEntity in _erpStockRepo.Table
                                   on new { obj.Sku, obj.SalesOrganisationCode, obj.WarehouseCode }
                                   equals new { dbEntity.Sku, dbEntity.SalesOrganisationCode, dbEntity.WarehouseCode }
                                   select dbEntity).ToListAsync();

            foreach (var dbErpStock in existingErpStock)
            {
                var updatedErpStock = erpStockLevels.Find(x => x.Sku.Equals(dbErpStock.Sku) &&
                                                               x.SalesOrganisationCode.Equals(dbErpStock.SalesOrganisationCode) &&
                                                               x.WarehouseCode.Equals(dbErpStock.WarehouseCode));
                if (updatedErpStock != null)
                {
                    dbErpStock.TotalOnHand = updatedErpStock.TotalOnHand;
                    dbErpStock.UOM = updatedErpStock.UOM ?? string.Empty;
                    dbErpStock.Weight = updatedErpStock.Weight ?? 0;
                    dbErpStock.UpdatedById = currentCustomerId;
                    dbErpStock.UpdatedOnUtc = DateTime.UtcNow;
                    dbErpStock.IsUpdated = false;
                }
            }

            foreach (var stock in existingErpStock)
            {
                await _erpStockRepo.UpdateAsync(stock);
            }

            var newErpStocks = await erpStockLevels.Where(model => !existingErpStock.Any(existing =>
                                                            model.Sku == existing.Sku &&
                                                            model.SalesOrganisationCode == existing.SalesOrganisationCode && model.WarehouseCode == existing.WarehouseCode))
                                                            .ToListAsync();

            foreach (var erpStock in newErpStocks)
            {
                var dbErpStock = new Parallel_ErpStock();

                dbErpStock.Sku = erpStock.Sku ?? string.Empty;
                dbErpStock.SalesOrganisationCode = erpStock.SalesOrganisationCode ?? string.Empty;
                dbErpStock.WarehouseCode = erpStock.WarehouseCode;
                dbErpStock.TotalOnHand = erpStock.TotalOnHand;
                dbErpStock.UOM = erpStock.UOM ?? string.Empty;
                dbErpStock.Weight = erpStock.Weight ?? 0;
                dbErpStock.UpdatedById = currentCustomerId;
                dbErpStock.UpdatedOnUtc = DateTime.UtcNow;

                // Common
                dbErpStock.CreatedById = currentCustomerId;
                dbErpStock.UpdatedById = currentCustomerId;
                dbErpStock.CreatedOnUtc = DateTime.UtcNow;
                dbErpStock.IsUpdated = false;

                erpStockToAdd.Add(dbErpStock);
            }

            await _erpStockRepo.InsertAsync(erpStockToAdd);
        }

        #endregion
    }
}
