using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Orders;
using Nop.Services.Catalog;
using Nop.Services.Common;
using Nop.Services.Helpers;
using Nop.Services.Localization;
using Nop.Services.Orders;
using Nop.Web.Controllers;
using Nop.Web.Framework.Mvc;
using NopStation.Plugin.B2B.B2BB2CFeatures.Services.ErpCustomerFunctionality;
using NopStation.Plugin.B2B.B2BB2CFeatures.Services.ErpPriceSyncFunctionality;
using NopStation.Plugin.B2B.ERPIntegrationCore.Domain;
using NopStation.Plugin.B2B.ERPIntegrationCore.Enums;
using NopStation.Plugin.B2B.ERPIntegrationCore.Model;
using NopStation.Plugin.B2B.ERPIntegrationCore.Services;

namespace NopStation.Plugin.B2B.B2BB2CFeatures.Controllers;

public class HandleLiveErpCallController : BasePublicController
{
    #region Fields

    private readonly IGenericAttributeService _genericAttributeService;
    private readonly IStoreContext _storeContext;
    private readonly IWorkContext _workContext;
    private readonly IShoppingCartService _shoppingCartService;
    private readonly IProductService _productService;
    private readonly B2BB2CFeaturesSettings _b2BB2CFeaturesSettings;
    private readonly IDateTimeHelper _dateTimeHelper;
    private readonly IErpAccountService _erpAccountService;
    private readonly ILocalizationService _localizationService;
    private readonly IErpIntegrationPluginManager _erpIntegrationPluginService;
    private readonly IErpSalesOrgService _erpSalesOrgService;
    private readonly IErpIntegrationPluginManager _erpIntegrationPluginManager;
    private readonly IErpSpecialPriceService _erpSpecialPriceService;
    private readonly ICategoryService _categoryService;
    private readonly IErpLogsService _erpLogsService;
    private readonly IErpPriceSyncFunctionalityService _erpPriceSyncFunctionalityService;
    private readonly IErpCustomerFunctionalityService _erpCustomerFunctionalityService;

    #endregion

    #region Ctor

    public HandleLiveErpCallController(IGenericAttributeService genericAttributeService,
        IStoreContext storeContext,
        IWorkContext workContext,
        IShoppingCartService shoppingCartService,
        IProductService productService,
        B2BB2CFeaturesSettings b2BCustomerAccountSettings,
        IDateTimeHelper dateTimeHelper,
        IErpAccountService erpAccountService,
        ILocalizationService localizationService,
        IErpIntegrationPluginManager erpIntegrationPluginService,
        IErpSalesOrgService erpSalesOrgService,
        IErpIntegrationPluginManager erpIntegrationPluginManager,
        IErpSpecialPriceService erpSpecialPriceService,
        ICategoryService categoryService,
        IErpLogsService erpLogsService,
        IErpPriceSyncFunctionalityService erpPriceSyncFunctionalityService,
        IErpCustomerFunctionalityService erpCustomerFunctionalityService)
    {
        _genericAttributeService = genericAttributeService;
        _storeContext = storeContext;
        _workContext = workContext;
        _shoppingCartService = shoppingCartService;
        _productService = productService;
        _b2BB2CFeaturesSettings = b2BCustomerAccountSettings;
        _dateTimeHelper = dateTimeHelper;
        _erpAccountService = erpAccountService;
        _localizationService = localizationService;
        _erpIntegrationPluginService = erpIntegrationPluginService;
        _erpSalesOrgService = erpSalesOrgService;
        _erpIntegrationPluginManager = erpIntegrationPluginManager;
        _erpSpecialPriceService = erpSpecialPriceService;
        _categoryService = categoryService;
        _erpLogsService = erpLogsService;
        _erpPriceSyncFunctionalityService = erpPriceSyncFunctionalityService;
        _erpCustomerFunctionalityService = erpCustomerFunctionalityService;
    }

    #endregion

    #region Methods

    private async Task<string> ProductListLivePriceSync(ErpAccount b2BAccount, IList<Product> products)
    {
        if (b2BAccount == null || products == null || !products.Any())
            return string.Empty;

        var erpIntegrationPlugin = await _erpIntegrationPluginManager.LoadActiveERPIntegrationPlugin();
        if (erpIntegrationPlugin is null)
        {
            //put a log here
            return string.Empty;
        }

        var accountSalesOrg = await _erpSalesOrgService.GetErpSalesOrgByIdAsync(b2BAccount.ErpSalesOrgId);
        var priceChangedProductsSkus = string.Empty;

        if (accountSalesOrg != null && !string.IsNullOrEmpty(accountSalesOrg.Code ))
        {
            var mpn_skus = products
                .Where(x => !string.IsNullOrEmpty(x.ManufacturerPartNumber))
                .Select(x => $"{x.ManufacturerPartNumber}|{x.Sku}")
                .ToList();

            var skucommaSeparatedString = string.Join(',', mpn_skus);

            if (string.IsNullOrEmpty(skucommaSeparatedString))
                return string.Empty;

            try
            {
                var erpResponseData = await erpIntegrationPlugin.ProductListLivePriceSync(new ErpGetRequestModel
                {
                    Location = accountSalesOrg.Code,
                    AccountNumber = b2BAccount.AccountNumber,
                    ItemNos = skucommaSeparatedString,
                });

                //test log, must remove
                await _erpLogsService.InformationAsync(
                    $"LIVE PIRCE SYNC: Account Number: {b2BAccount.AccountNumber}, " +
                    $"Account Id: {b2BAccount.Id}, " +
                    $"Product skus: {skucommaSeparatedString}, " +
                    $"Response Data: {erpResponseData?.Data?.Count} items found.", 
                    ErpSyncLevel.SpecialPrice);

                if (erpResponseData != null && erpResponseData.Data != null)
                {
                    var priceChangedProductsList = new List<string>();

                    foreach (var product in products)
                    {
                        var productResponseModel = erpResponseData.Data
                            .FirstOrDefault(x => x.Sku == product.Sku);
                        if (productResponseModel != null)
                        {
                            var categoryIds = _b2BB2CFeaturesSettings.SkipLivePriceCheckCategoryIds.Split(',').Select(int.Parse).ToList();
                            var productCategories = await _categoryService.GetProductCategoriesByProductIdAsync(product.Id, true);
                            var skipProduct = false;
                            foreach (var cat in productCategories)
                            {
                                if (categoryIds.Any(a => a == cat.CategoryId))
                                    skipProduct = true;
                            }
                            if (skipProduct)
                                continue;

                            var accountPrice = productResponseModel.SpecialPrice;
                            if (accountPrice.HasValue)
                            {
                                var productPricing = await _erpSpecialPriceService.GetErpSpecialPricesByErpAccountIdAndNopProductIdAsync(b2BAccount.Id, product.Id);


                                if (productPricing != null)
                                {
                                    if (accountPrice.Value != productPricing.Price)
                                    {
                                        productPricing.Price = accountPrice.Value;
                                        productPricing.PricingNote = productResponseModel.PricingNotes;

                                        //cr7676
                                        productPricing.DiscountPerc = 
                                            _b2BB2CFeaturesSettings.EnableOnlineSavings && productResponseModel.DiscountPercentage.HasValue
                                                ? productResponseModel.DiscountPercentage.Value
                                                : 0;

                                        //productPricing.CustomerUoM = productResponseModel.UnitofMeasure;
                                        await _erpSpecialPriceService.UpdateErpSpecialPriceAsync(productPricing);

                                        priceChangedProductsList.Add(productResponseModel.Sku);
                                    }
                                }
                            }
                        }
                    }

                    if (priceChangedProductsList.Count > 0)
                    {
                        priceChangedProductsSkus = string.Join(',', priceChangedProductsList);
                    }
                }
            }
            catch (Exception ex)
            {

            }
        }

        return priceChangedProductsSkus;
    }


    protected async Task<string> UpdateCartItemProductLivePriceAsync(ErpAccount erpAccount)
    {
        if (erpAccount is null)
            return string.Empty;

        var cart = await _shoppingCartService
            .GetShoppingCartAsync(await _workContext.GetCurrentCustomerAsync(), 
            ShoppingCartType.ShoppingCart, 
            (await _storeContext.GetCurrentStoreAsync()).Id);

        if (!cart.Any())
            return string.Empty;

        var productIds = cart.Select(x => x.ProductId).ToList();
        var products = await _productService.GetProductsByIdsAsync(productIds.ToArray());

        var erpIntegrationPlugin = await _erpIntegrationPluginService.LoadActiveERPIntegrationPlugin();
        if (erpIntegrationPlugin is null)
        {
            return string.Empty;
        }


        return await ProductListLivePriceSync(erpAccount, products);
    }

    public async Task<IActionResult> CurrentCartItemsLiveStockCheck()
    {
        var currCustomer = await _workContext.GetCurrentCustomerAsync();
        var erpAccount = await _erpCustomerFunctionalityService.GetActiveErpAccountByCustomerAsync(currCustomer);

        if (erpAccount == null || !_b2BB2CFeaturesSettings.EnableLiveStockChecks)
        {
            return new NullJsonResult();
        }

        var cart = await _shoppingCartService.GetShoppingCartAsync(
            currCustomer,
            ShoppingCartType.ShoppingCart,
            (await _storeContext.GetCurrentStoreAsync()).Id
        );

        if (!cart.Any())
            return new NullJsonResult();

        if (cart.Count > _b2BB2CFeaturesSettings.DisableLiveStockCheckProductGreaterThanAmount)
        {
            return Json(new
            {
                success = false,
                message = $"Sync not possible because cart has over {_b2BB2CFeaturesSettings.DisableLiveStockCheckProductGreaterThanAmount} items."
            });
        }

        var products = await _productService.GetProductsByIdsAsync(cart.Select(x => x.ProductId).ToArray());
        var result = await _erpPriceSyncFunctionalityService.ProductListLiveStockSyncAsync(erpAccount, products);

        return Json(new
        {
            success = result.success,
            message = result.message
        });
    }

    public async Task<IActionResult> CurrentCartItemsLivePriceCheck()
    {
        if (!_b2BB2CFeaturesSettings.EnableLivePriceChecks)
        {
            return new NullJsonResult();
        }

        var currCustomer = await _workContext.GetCurrentCustomerAsync();
        var currStore = await _storeContext.GetCurrentStoreAsync();
        var erpAccount = await _erpAccountService.GetActiveErpAccountByCustomerIdAsync(currCustomer.Id);

        if (erpAccount == null)
        {
            return new NullJsonResult();
        }

        var updatedPriceProductSkus = string.Empty;

        if (!erpAccount.LastPriceRefresh.HasValue)
        {
            updatedPriceProductSkus = await UpdateCartItemProductLivePriceAsync(erpAccount);
        }
        else
        {
            var priceUpdateOnLocalTime = _dateTimeHelper.ConvertToUtcTime(erpAccount.LastPriceRefresh.Value, DateTimeKind.Utc);

            if (priceUpdateOnLocalTime < DateTime.UtcNow)
            {
                updatedPriceProductSkus = await UpdateCartItemProductLivePriceAsync(erpAccount);
            }
        }

        await _genericAttributeService.SaveAttributeAsync(currCustomer, B2BB2CFeaturesDefaults.CartItemsLivePriceSyncProcessing, false, currStore.Id);

        if (!string.IsNullOrEmpty(updatedPriceProductSkus))
        {
            var msg = string.Format(
                await _localizationService
                .GetResourceAsync("NopStation.Plugin.B2B.B2BB2CFeatures.LivePriceSync.CartItemPriceUpdated"), 
                updatedPriceProductSkus);

            return Json(new
            {
                success = true,
                data = updatedPriceProductSkus,
                message = msg
            });
        }

        return new NullJsonResult();
    }

    #endregion
}