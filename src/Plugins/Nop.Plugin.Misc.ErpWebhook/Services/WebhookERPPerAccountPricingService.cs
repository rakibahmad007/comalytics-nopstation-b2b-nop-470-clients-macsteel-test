using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Data;
using Nop.Plugin.Misc.ErpWebhook.Domain.ParallelTables;
using Nop.Plugin.Misc.ErpWebhook.Models.ErpPricing;
using Nop.Plugin.Misc.ErpWebhook.Services.Interfaces;
using Nop.Services.Logging;
using NopStation.Plugin.B2B.ERPIntegrationCore.Domain;

namespace Nop.Plugin.Misc.ErpWebhook.Services
{
    public class WebhookERPPerAccountPricingService : IWebhookERPPerAccountPricingService
    {
        #region fields

        private ErpWebhookConfig _erpWebhookConfig = null;
        private readonly IRepository<ErpAccount> _erpAccountRepo;
        private readonly IErpWebhookService _erpWebhookService;
        private readonly IWorkContext _workContext;
        private readonly IRepository<Parallel_ErpAccountPricing> _erpAccountPricingRepo;
        private readonly IRepository<ErpSalesOrg> _erpSalesOrgRepo;
        private readonly IRepository<ErpSpecialPrice> _erpSpecialPriceRepo;
        private readonly ILogger _logger;

        #endregion

        #region ctor

        public WebhookERPPerAccountPricingService(IRepository<ErpAccount> erpAccountRepo,
            ILogger logger,
            IRepository<ErpSpecialPrice> erpSpecialPriceRepo,
            IErpWebhookService erpWebhookService,
            IWorkContext workContext,
            IRepository<Parallel_ErpAccountPricing> erpAccountPricingRepo,
            IRepository<ErpSalesOrg> erpSalesOrgRepo)
        {
            _erpAccountRepo = erpAccountRepo;
            _logger = logger;
            _erpSpecialPriceRepo = erpSpecialPriceRepo;
            _erpWebhookService = erpWebhookService;
            _workContext = workContext;
            _erpAccountPricingRepo = erpAccountPricingRepo;
            _erpSalesOrgRepo = erpSalesOrgRepo;
        }

        #endregion

        #region utils

        private async Task UpdateAccountLastPriceRefreshDateAsync(string accountNumber, string location)
        {
            if (string.IsNullOrEmpty(accountNumber) || string.IsNullOrEmpty(location))
            {
                return;
            }

            try
            {
                int salesOrgId = await _erpWebhookService.GetSalesOrganisationIdAsync(location);

                var b2BAccount = await _erpAccountRepo.Table
                    .Where(a => a.AccountNumber == accountNumber && a.ErpSalesOrgId == salesOrgId)
                    .FirstOrDefaultAsync();

                if (b2BAccount != null)
                {
                    b2BAccount.LastPriceRefresh = DateTime.UtcNow;
                    await _erpAccountRepo.UpdateAsync(b2BAccount);
                    _logger.Information($"UpdateAccountLastPriceRefreshDate - Done for account:{accountNumber} and location: {location}");
                }
                else
                {
                    _logger.Error($"UpdateAccountLastPriceRefreshDate - couldn't find b2b account by account:{accountNumber} and location: {location}");
                }
            }
            catch (Exception ex)
            {
                _logger.Error("UpdateAccountLastPriceRefreshDate - " + ex.TargetSite + "  -  " + ex.Message);
            }
        }

        private async Task<Dictionary<int, ErpSpecialPrice>> GetPricingsForAccountAsync(int accountId, List<int> productIds)
        {
            var pricings = await _erpSpecialPriceRepo.Table
                .Where(pap => pap.ErpAccountId == accountId && productIds.Contains(pap.NopProductId))
                .ToListAsync();

            return pricings.ToDictionary(pap => pap.NopProductId);
        }

        private void MapErpAccountPricings(Parallel_ErpAccountPricing dbErpAccountPricings, ErpAccountPricingModel updatedErpAccountPricing)
        {
            dbErpAccountPricings.AccountNumber = updatedErpAccountPricing.AccountNumber;
            dbErpAccountPricings.SalesOrganisationCode = updatedErpAccountPricing.SalesOrganisationCode;
            dbErpAccountPricings.Sku = updatedErpAccountPricing.Sku;
            dbErpAccountPricings.Price = updatedErpAccountPricing.Price ?? 0;
            dbErpAccountPricings.ListPrice = updatedErpAccountPricing.ListPrice ?? 0;
            dbErpAccountPricings.DiscountPerc = updatedErpAccountPricing.DiscountPerc ?? 0;
            dbErpAccountPricings.PricingNotes = updatedErpAccountPricing.PricingNotes ?? string.Empty;
            dbErpAccountPricings.IsUpdated = false;
            dbErpAccountPricings.UpdatedOnUtc = DateTime.UtcNow;
        }

        #endregion

        #region Method

        public async Task ProcessERPPerAccountPricingToParallelTableAsync(List<ErpAccountPricingModel> erpAccountPricings)
        {
            if (!erpAccountPricings.Any())
                return;

            _erpWebhookConfig = await _erpWebhookService.LoadErpWebhookConfigsFromJsonAsync();
            var currentCustomerId = (await _workContext.GetCurrentCustomerAsync()).Id;
            var erpAccountPricingToAdd = new List<Parallel_ErpAccountPricing>();

            // Check if similar entities exist in the database
            var existingErpAccountPricing = await (from obj in erpAccountPricings
                                                   join dbEntity in _erpAccountPricingRepo.Table
                                                   on new { obj.AccountNumber, obj.SalesOrganisationCode, obj.Sku }
                                                   equals new { dbEntity.AccountNumber, dbEntity.SalesOrganisationCode, dbEntity.Sku }
                                                   select dbEntity).ToListAsync();

            foreach (var dbErpAccountPricings in existingErpAccountPricing)
            {
                var updatedErpAccountPricing = erpAccountPricings.Find(x => x.AccountNumber.Equals(dbErpAccountPricings.AccountNumber) &&
                                                                            x.SalesOrganisationCode.Equals(dbErpAccountPricings.SalesOrganisationCode) &&
                                                                            x.Sku.Equals(dbErpAccountPricings.Sku));
                if (updatedErpAccountPricing != null)
                {
                    dbErpAccountPricings.ListPrice = updatedErpAccountPricing.ListPrice ?? 0;
                    dbErpAccountPricings.Price = updatedErpAccountPricing.Price ?? 0;
                    dbErpAccountPricings.DiscountPerc = updatedErpAccountPricing.DiscountPerc ?? 0;
                    dbErpAccountPricings.PricingNotes = updatedErpAccountPricing.PricingNotes ?? string.Empty;
                    dbErpAccountPricings.UpdatedById = currentCustomerId;
                    dbErpAccountPricings.UpdatedOnUtc = DateTime.UtcNow;
                    dbErpAccountPricings.IsUpdated = false;
                }
            }

            if (existingErpAccountPricing.Any())
            {
                await _erpAccountPricingRepo.UpdateAsync(existingErpAccountPricing);
            }

            var newErpAccountPricings = erpAccountPricings.Where(model => !existingErpAccountPricing.Any(existing =>
                                                                        model.AccountNumber == existing.AccountNumber &&
                                                                        model.SalesOrganisationCode == existing.SalesOrganisationCode &&
                                                                        model.Sku == existing.Sku))
                                                            .ToList();

            foreach (var erpAccountPricingModel in newErpAccountPricings)
            {
                var dbErpAccountPricing = new Parallel_ErpAccountPricing();

                MapErpAccountPricings(dbErpAccountPricing, erpAccountPricingModel);

                //common
                dbErpAccountPricing.CreatedById = currentCustomerId;
                dbErpAccountPricing.UpdatedById = currentCustomerId;
                dbErpAccountPricing.CreatedOnUtc = DateTime.UtcNow;

                erpAccountPricingToAdd.Add(dbErpAccountPricing);
            }

            if (erpAccountPricingToAdd.Any())
            {
                await _erpAccountPricingRepo.InsertAsync(erpAccountPricingToAdd);
            }
        }

        #endregion
    }
}
