using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Orders;
using Nop.Services.Catalog;
using Nop.Services.Localization;
using Nop.Services.Orders;
using NopStation.Plugin.B2B.B2BB2CFeatures.Model.Order;
using NopStation.Plugin.B2B.B2BB2CFeatures.Services.ErpCustomerFunctionality;
using NopStation.Plugin.B2B.ERPIntegrationCore.Domain;
using NopStation.Plugin.B2B.ERPIntegrationCore.Enums;
using NopStation.Plugin.B2B.ERPIntegrationCore.Services;

namespace NopStation.Plugin.B2B.B2BB2CFeatures.Services.ErpOrderTotalCalculationService;

public class ErpOrderTotalCalculationService : IErpOrderTotalCalculationService
{
    private readonly IWorkContext _workContext;
    private readonly IErpCustomerFunctionalityService _erpCustomerFunctionalityService;
    private readonly B2BB2CFeaturesSettings _b2BB2CFeaturesSettings;
    private readonly ILocalizationService _localizationService;
    private readonly IPriceFormatter _priceFormatter;
    private readonly IShoppingCartService _shoppingCartService;
    private readonly IProductService _productService;
    private readonly ICategoryService _categoryService;
    private readonly IErpWarehouseSalesOrgMapService _erpWarehouseSalesOrgMapService;
    private readonly IErpSpecialPriceService _erpSpecialPriceService;
    private readonly IErpProductService _erpProductService;
    private readonly IB2CUserStockRestrictionService _b2CUserStockRestrictionService;

    public ErpOrderTotalCalculationService(
        IWorkContext workContext,
        IErpCustomerFunctionalityService erpCustomerFunctionalityService,
        B2BB2CFeaturesSettings b2BB2CFeaturesSettings,
        ILocalizationService localizationService,
        IPriceFormatter priceFormatter,
        IShoppingCartService shoppingCartService,
        IProductService productService,
        ICategoryService categoryService,
        IErpWarehouseSalesOrgMapService erpWarehouseSalesOrgMapService,
        IErpSpecialPriceService erpSpecialPriceService,
        IErpProductService erpProductService,
        IB2CUserStockRestrictionService b2CUserStockRestrictionService
    )
    {
        _workContext = workContext;
        _erpCustomerFunctionalityService = erpCustomerFunctionalityService;
        _b2BB2CFeaturesSettings = b2BB2CFeaturesSettings;
        _localizationService = localizationService;
        _priceFormatter = priceFormatter;
        _shoppingCartService = shoppingCartService;
        _productService = productService;
        _categoryService = categoryService;
        _erpWarehouseSalesOrgMapService = erpWarehouseSalesOrgMapService;
        _erpSpecialPriceService = erpSpecialPriceService;
        _erpProductService = erpProductService;
        _b2CUserStockRestrictionService = b2CUserStockRestrictionService;
    }

    public async Task<B2BAdditionalOrderTotalsModel> LoadAdditionalOrderTotalsDataOfCartItemsAsync()
    {
        var additionalOrderTotalsModel = new B2BAdditionalOrderTotalsModel();
        bool prodcutForQuote = false;

        var customer = await _workContext.GetCurrentCustomerAsync();
        var erpAccount = await _erpCustomerFunctionalityService.GetActiveErpAccountByCustomerAsync(
            customer
        );

        if (
            erpAccount == null
            || !_b2BB2CFeaturesSettings.DisplayWeightInformation
            || !customer.HasShoppingCartItems
        )
            return additionalOrderTotalsModel;

        var baseWeight = await _localizationService.GetResourceAsync(
            "B2B.TotalWeight.Custom.BaseWeight"
        );
        var shoppingCarts = await _shoppingCartService.GetShoppingCartAsync(
            customer,
            ShoppingCartType.ShoppingCart
        );
        var cartItemProductsWeightWithQuantity = shoppingCarts
            .Select(async cartItem =>
            {
                var product = await _productService.GetProductByIdAsync(cartItem.ProductId);
                return new
                {
                    Weight = product?.Weight ?? decimal.Zero,
                    cartItem.Quantity,
                };
            })
            .Select(task => task.Result)
            .ToList();

        var totalPriceWithOutSavings = decimal.Zero;
        var b2BOnlineOrderDiscount = decimal.Zero;

        foreach (var cartItemProduct in shoppingCarts)
        {
            var product = await _productService.GetProductByIdAsync(cartItemProduct.ProductId);
            if (product == null)
                continue;

            (var productUnitPriceWithDiscount, var discountAmount, _) =
                await _shoppingCartService.GetUnitPriceAsync(cartItemProduct, true);

            if (productUnitPriceWithDiscount == _b2BB2CFeaturesSettings.ProductQuotePrice)
                prodcutForQuote = true;

            var productUnitPriceWithOutDiscount = productUnitPriceWithDiscount + discountAmount;
            totalPriceWithOutSavings +=
                productUnitPriceWithOutDiscount * cartItemProduct.Quantity
            ;
            b2BOnlineOrderDiscount += discountAmount * cartItemProduct.Quantity;
        }

        additionalOrderTotalsModel.IsShowPaymentTermsDescription = !string.IsNullOrEmpty(
            erpAccount.PaymentTermsDescription
        );
        additionalOrderTotalsModel.PaymentTermsDescription = erpAccount.PaymentTermsDescription;

        additionalOrderTotalsModel.TotalPriceWithOutSavings =
            await _priceFormatter.FormatPriceAsync(totalPriceWithOutSavings);
        additionalOrderTotalsModel.B2BOnlineOrderDiscount = await _priceFormatter.FormatPriceAsync(
            b2BOnlineOrderDiscount
        );

        var totalWeight = cartItemProductsWeightWithQuantity.Sum(x => x.Weight * x.Quantity);
        additionalOrderTotalsModel.TotalWeight = $"{totalWeight:F2} {baseWeight}";
        additionalOrderTotalsModel.IsVisibleWeight = true;

        var totalCustomerAccountSavings = decimal.Zero;
        additionalOrderTotalsModel.CustomerAccountSavings = totalCustomerAccountSavings;
        additionalOrderTotalsModel.CustomerAccountSavingsValue =
            await _priceFormatter.FormatPriceAsync(totalCustomerAccountSavings);

        if (prodcutForQuote)
        {
            additionalOrderTotalsModel.CustomerAccountSavingsValue = await _localizationService.GetResourceAsync("Products.ProductForQuote");
            additionalOrderTotalsModel.TotalPriceWithOutSavings = await _localizationService.GetResourceAsync("Products.ProductForQuote");
            additionalOrderTotalsModel.B2BOnlineOrderDiscount = await _localizationService.GetResourceAsync("Products.ProductForQuote");
            additionalOrderTotalsModel.ProductForQuote = true;
        }

        return additionalOrderTotalsModel;
    }

    #region 2581

    public async Task<int> GetTotalStockQuantityForAdminOPEventAsync(Product product, ErpAccount b2BAccount, ErpNopUser erpNopUser)
    {
        if (product == null || b2BAccount == null || erpNopUser == null)
            throw new ArgumentNullException(nameof(product));

        if (product.ManageInventoryMethod != ManageInventoryMethod.ManageStock)
        {
            var categoryIds = new List<int>();
            if (!string.IsNullOrWhiteSpace(_b2BB2CFeaturesSettings.SkipLiveStockCheckCategoryIds))
            {
                categoryIds = _b2BB2CFeaturesSettings.SkipLiveStockCheckCategoryIds.Split(',').Select(int.Parse).ToList();
            }
            var productCategories = await _categoryService.GetProductCategoriesByProductIdAsync(product.Id);
            foreach (var cat in productCategories)
            {
                if (categoryIds.Exists(a => a == cat.CategoryId))
                    return 1000;
            }
            //We can calculate total stock quantity when 'Manage inventory' property is set to 'Track inventory'
            return 0;
        }

        var productWarehouseInventory = await _erpWarehouseSalesOrgMapService.GetProductWarehouseInventoriesByProductIdSalesOrgIdAsync(product.Id, b2BAccount.ErpSalesOrgId, true);
        var totalStock = (decimal)productWarehouseInventory.Sum(x => x.StockQuantity);
        totalStock = totalStock - productWarehouseInventory.Sum(x => x.ReservedQuantity);

        if (erpNopUser.ErpUserType == ErpUserType.B2CUser)
        {
            // For B2C - first try to get % of stock from B2CUserStockRestriction
            var existingB2CUserStockRestriction =
                await _b2CUserStockRestrictionService.GetB2CUserStockRestrictionByUserIdProductIdAsync(erpNopUser.Id, product.Id);
            if (existingB2CUserStockRestriction != null &&
                existingB2CUserStockRestriction.Id > 0 &&
                existingB2CUserStockRestriction.PercentageOfAllocatedStockResetTimeUtc.HasValue &&
                !(existingB2CUserStockRestriction.PercentageOfAllocatedStockResetTimeUtc.Value < DateTime.UtcNow))
            {
                return _erpProductService.GetStockByPercentage(totalStock, existingB2CUserStockRestriction.NewPercentageOfAllocatedStock);
            }
        }

        var productPricing = await _erpSpecialPriceService.GetErpSpecialPricesByErpAccountIdAndNopProductIdAsync(b2BAccount.Id, product.Id);

        if (productPricing == null || productPricing.Id == 0
            || !productPricing.PercentageOfAllocatedStockResetTimeUtc.HasValue
            || productPricing.PercentageOfAllocatedStockResetTimeUtc.Value < DateTime.UtcNow)
        {
            // if PercentageOfAllocatedStockResetTimeUtc doesn't have any value
            // or PercentageOfAllocatedStockResetTimeUtc has passed already
            // then take value from account
            return _erpProductService.GetStockByPercentage(totalStock, b2BAccount.PercentageOfStockAllowed ?? 0);
        }

        return _erpProductService.GetStockByPercentage(totalStock, productPricing.PercentageOfAllocatedStock);
    }

    #endregion 2581
}