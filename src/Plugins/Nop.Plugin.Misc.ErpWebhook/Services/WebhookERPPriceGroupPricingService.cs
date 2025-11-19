using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Nop.Core.Domain.Catalog;
using Nop.Data;
using Nop.Plugin.Misc.ErpWebhook.Models.ErpPricing;
using Nop.Plugin.Misc.ErpWebhook.Services.Interfaces;
using Nop.Services.Logging;
using NopStation.Plugin.B2B.ERPIntegrationCore.Domain;

namespace Nop.Plugin.Misc.ErpWebhook.Services
{
    public class WebhookERPPriceGroupPricingService : IWebhookERPPriceGroupPricingService
    {
        #region fields

        private readonly IRepository<ErpGroupPriceCode> _erpGroupPriceCode;
        private readonly IRepository<Product> _productRepo;
        private readonly IRepository<ErpGroupPrice> _erpGroupPriceRepo;
        private readonly ILogger _logger;
        private readonly INopDataProvider _nopDataProvider;

        #endregion

        #region ctor

        public WebhookERPPriceGroupPricingService(IRepository<ErpGroupPriceCode> erpGroupPriceCode,
            IRepository<Product> productRepo,
            IRepository<ErpGroupPrice> b2BPriceGroupProductPricingRepo,
            ILogger logger,
            INopDataProvider nopDataProvider)
        {
            _erpGroupPriceCode = erpGroupPriceCode;
            _productRepo = productRepo;
            _erpGroupPriceRepo = b2BPriceGroupProductPricingRepo;
            _logger = logger;
            _nopDataProvider = nopDataProvider;
        }

        #endregion

        #region utils
        public async Task<Dictionary<string, int>> GetPriceGroupIdsAsync(List<string> priceGroupCodes)
        {
            // Retrieve existing groups asynchronously
            var existing = await _erpGroupPriceCode.Table
                .Where(pg => priceGroupCodes.Contains(pg.Code))
                .ToDictionaryAsync(pg => pg.Code, pg => pg.Id);

            // Find new groups asynchronously
            var newGroups = new HashSet<string>(priceGroupCodes.Except(existing.Select(pg => pg.Key)))
                .Where(code => code != null)
                .Select(code => new ErpGroupPriceCode()
                {
                    Id = -1,
                    Code = code,
                }).ToList();

            if (newGroups.Any())
            {
                // Insert new groups asynchronously
                await _erpGroupPriceCode.InsertAsync(newGroups);
            }

            // Add new groups' Ids to the existing dictionary
            foreach (var newGroup in newGroups)
            {
                System.Diagnostics.Debug.Assert(newGroup.Id >= 0);
                existing.Add(newGroup.Code, newGroup.Id);
            }

            return existing;
        }

        public async Task<Dictionary<string, int>> GetProductIdsAsync(List<string> skus)
        {
            var products = await _productRepo.Table
                .Where(p => skus.Contains(p.Sku))
                .Select(p => new { p.Id, p.Sku, p.Published, p.Deleted })
                .ToListAsync();

            try
            {
                // Happy path
                return products.ToDictionary(p => p.Sku, p => p.Id);
            }
            catch (System.ArgumentException)
            {
                var result = new Dictionary<string, int>();
                // If more than one product per sku is loaded, the ToDictionary() call above will fail
                foreach (var grp in products.ToLookup(p => p.Sku))
                {
                    if (grp.Count() == 1)
                    {
                        var product = grp.First();
                        result.Add(product.Sku, product.Id);
                    }
                    else
                    {
                        var product = grp.OrderByDescending(p => p.Published)
                                         .ThenBy(p => p.Deleted)
                                         .ThenBy(p => p.Id)
                                         .First();
                        result.Add(product.Sku, product.Id);
                    }
                }
                return result;
            }
        }

        public async Task<Dictionary<int, ErpGroupPrice>> GetPricesAsync(int priceGroupId, List<int> productIds)
        {
            var prices = await _erpGroupPriceRepo.Table
                .Where(p => p.ErpNopGroupPriceCodeId == priceGroupId && productIds.Contains(p.NopProductId))
                .ToListAsync();

            return prices.ToDictionary(p => p.NopProductId);
        }

        public async Task Update_LastPriceUpdatedOnUtc_OfAllPriceGroupsAsync(DateTimeOffset newValue)
        {
            try
            {
                var sql = $"UPDATE ErpGroupPriceCode SET LastPriceUpdatedOnUtc = '{newValue:yyyy-MM-dd HH:mm:ss}'"; // Ensure proper format for DateTimeOffset
                await _nopDataProvider.ExecuteNonQueryAsync(sql); // Await the asynchronous database operation

            }
            catch (Exception ex)
            {
                _logger.Error($"Error setting last price refresh date of all groups to {newValue}: {ex.Message}", ex);
            }
        }

        #endregion

        #region Methods

        public async Task ProcessERPPriceGroupPricingAsync(List<ErpPriceGroupPricingModel> erpAccountPricings)
        {
            ILookup<string, ErpPriceGroupPricingModel> pricingsPerPriceGroup = erpAccountPricings.ToLookup(p => p.PriceGroupCode);
            List<string> priceGroupCodes = new HashSet<string>(pricingsPerPriceGroup.Select(g => g.Key)).ToList();
            Dictionary<string, int> priceGroupIds = await GetPriceGroupIdsAsync(priceGroupCodes);

            List<string> itemNos = new HashSet<string>(erpAccountPricings.Select(p => p.ItemNo)).ToList();
            Dictionary<string, int> itemIds = await GetProductIdsAsync(itemNos);

            foreach (IGrouping<string, ErpPriceGroupPricingModel> erpPricings in pricingsPerPriceGroup)
            {
                string priceGroupCode = erpPricings.Key;
                if (priceGroupCode == null)
                {
                    _logger.Warning("Skipping null price group code");
                    continue;
                }

                var prices = erpPricings.GroupBy(p => p.ItemNo).ToDictionary(x => x.Key, x => x.First());

                if (!priceGroupIds.TryGetValue(priceGroupCode, out int priceGroupId))
                {
                    _logger.Warning($"Id for price group '{priceGroupCode}' could not be determined. Pricings will be skipped");
                    continue;
                }
                _logger.Warning($"Updating prices for price group '{priceGroupCode}' (id={priceGroupId}), products {string.Join("; ", prices.Values.Select(p => p.ItemNo))}");

                List<int> productIdsInThisGroup = new List<int>();
                foreach (var pricing in prices.Values)
                {
                    if (string.IsNullOrEmpty(pricing.ItemNo))
                    {
                        _logger.Warning($"Pricing without ItemNo. It will be skipped");
                        continue;
                    }
                    decimal? price = pricing.SellingPrice;
                    if (!price.HasValue || price.Value < 0)
                    {
                        _logger.Warning($"Missing or negative pricing for item {pricing.ItemNo}: '{price}'. It will be skipped");
                        continue;
                    }
                    if (!itemIds.TryGetValue(pricing.ItemNo, out int productId))
                    {
                        continue;
                    }
                    productIdsInThisGroup.Add(productId);
                }

                Dictionary<int, ErpGroupPrice> existingPricings = await GetPricesAsync(priceGroupId, productIdsInThisGroup);

                foreach (var pricing in prices.Values)
                {
                    decimal? price = pricing.SellingPrice;
                    if (string.IsNullOrEmpty(pricing.ItemNo) || !price.HasValue || price.Value < 0)
                    {
                        continue;
                    }
                    if (!itemIds.TryGetValue(pricing.ItemNo, out int productId))
                    {
                        continue;
                    }
                    if (existingPricings.TryGetValue(productId, out ErpGroupPrice existingPricing))
                    {
                        existingPricing.Price = price ?? 0;
                        await _erpGroupPriceRepo.UpdateAsync(existingPricing);
                    }
                    else
                    {
                        var newPricing = new ErpGroupPrice
                        {
                            ErpNopGroupPriceCodeId = priceGroupId,
                            NopProductId = productId,
                            Price = price ?? 0,
                        };
                        await _erpGroupPriceRepo.InsertAsync(newPricing);
                    }
                }
            }

            await Update_LastPriceUpdatedOnUtc_OfAllPriceGroupsAsync(DateTimeOffset.UtcNow);
        }

        #endregion
    }
}
