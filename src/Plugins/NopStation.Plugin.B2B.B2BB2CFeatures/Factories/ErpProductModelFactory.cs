using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Core.Caching;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Directory;
using Nop.Core.Domain.Orders;
using Nop.Services.Catalog;
using Nop.Services.Directory;
using Nop.Services.Localization;
using Nop.Services.Orders;
using Nop.Services.Security;
using Nop.Services.Seo;
using Nop.Web.Areas.Admin.Factories;
using Nop.Web.Framework.Models.Extensions;
using NopStation.Plugin.B2B.B2BB2CFeatures.Model.ErpProductList;
using NopStation.Plugin.B2B.B2BB2CFeatures.Model.OrderSummary;
using NopStation.Plugin.B2B.B2BB2CFeatures.Services.ErpCustomerFunctionality;
using NopStation.Plugin.B2B.B2BB2CFeatures.Services.ErpPriceSyncFunctionality;
using NopStation.Plugin.B2B.B2BB2CFeatures.Services.ErpSpecificationAttributeService;
using NopStation.Plugin.B2B.ERPIntegrationCore.Domain;
using NopStation.Plugin.B2B.ERPIntegrationCore.Enums;
using NopStation.Plugin.B2B.ERPIntegrationCore.Infrastructure;
using NopStation.Plugin.B2B.ERPIntegrationCore.Services;

namespace NopStation.Plugin.B2B.B2BB2CFeatures.Factories;

public class ErpProductModelFactory : IErpProductModelFactory
{
    #region Fields

    private readonly IBaseAdminModelFactory _baseAdminModelFactory;
    private readonly IPriceFormatter _priceFormatter;
    private readonly IProductService _productService;
    private readonly IStoreContext _storeContext;
    private readonly IStaticCacheManager _staticCacheManager;
    private readonly IErpAccountService _erpAccountService;
    private readonly IPermissionService _permissionService;
    private readonly ILocalizationService _localizationService;
    private readonly IWorkContext _workContext;
    private readonly IUrlRecordService _urlRecordService;
    private readonly IErpSpecialPriceService _erpSpecialPriceService;
    private readonly IShoppingCartService _shoppingCartService;
    private readonly IPriceCalculationService _priceCalculationService;
    private readonly IErpSpecificationAttributeService _erpSpecificationAttributeService;
    private readonly B2BB2CFeaturesSettings _b2BB2CFeaturesSettings;
    private readonly IOrderService _orderService;
    private readonly IErpIntegrationPluginManager _erpIntegrationPluginManager;
    private readonly ICategoryService _categoryService;
    private readonly IErpCustomerFunctionalityService _erpCustomerFunctionalityService;
    private readonly IMeasureService _measureService;
    private readonly MeasureSettings _measureSettings;
    private readonly IErpPriceSyncFunctionalityService _erpPriceSyncFunctionalityService;

    #endregion Fields

    #region Ctor

    public ErpProductModelFactory(
        IBaseAdminModelFactory baseAdminModelFactory,
        IPriceFormatter priceFormatter,
        IProductService productService,
        IStoreContext storeContext,
        IStaticCacheManager staticCacheManager,
        IErpAccountService erpAccountService,
        IPermissionService permissionService,
        ILocalizationService localizationService,
        IWorkContext workContext,
        IUrlRecordService urlRecordService,
        IErpSpecialPriceService erpSpecialPriceService,
        IShoppingCartService shoppingCartService,
        IPriceCalculationService priceCalculationService,
        IErpSpecificationAttributeService erpSpecificationAttributeService,
        B2BB2CFeaturesSettings b2BB2CFeaturesSettings,
        IOrderService orderService,
        IErpIntegrationPluginManager erpIntegrationPluginManager,
        ICategoryService categoryService,
        IErpCustomerFunctionalityService erpCustomerFunctionalityService,
        IMeasureService measureService,
        MeasureSettings measureSettings,
        IErpPriceSyncFunctionalityService erpPriceSyncFunctionalityService)
    {
        _baseAdminModelFactory = baseAdminModelFactory;
        _priceFormatter = priceFormatter;
        _productService = productService;
        _storeContext = storeContext;
        _staticCacheManager = staticCacheManager;
        _erpAccountService = erpAccountService;
        _permissionService = permissionService;
        _localizationService = localizationService;
        _workContext = workContext;
        _urlRecordService = urlRecordService;
        _erpSpecialPriceService = erpSpecialPriceService;
        _shoppingCartService = shoppingCartService;
        _priceCalculationService = priceCalculationService;
        _erpSpecificationAttributeService = erpSpecificationAttributeService;
        _b2BB2CFeaturesSettings = b2BB2CFeaturesSettings;
        _orderService = orderService;
        _erpIntegrationPluginManager = erpIntegrationPluginManager;
        _categoryService = categoryService;
        _erpCustomerFunctionalityService = erpCustomerFunctionalityService;
        _measureService = measureService;
        _measureSettings = measureSettings;
        _erpPriceSyncFunctionalityService = erpPriceSyncFunctionalityService;
    }

    #endregion Ctor

    #region Method

    public async Task<ErpProductListSearchModel> PrepareErpProductListSearchModelAsync(
        ErpProductListSearchModel searchModel
    )
    {
        ArgumentNullException.ThrowIfNull(searchModel);

        await _baseAdminModelFactory.PrepareCategoriesAsync(searchModel.AvailableCategories);
        await _baseAdminModelFactory.PrepareManufacturersAsync(searchModel.AvailableManufacturers);
        searchModel.SetGridPageSize();
        return searchModel;
    }

    public async Task<ErpProductListModel> PrepareErpProductListModelAsync(
        ErpProductListSearchModel searchModel
    )
    {
        ArgumentNullException.ThrowIfNull(searchModel);

        var products = await _productService.SearchProductsAsync(
            showHidden: true,
            categoryIds: new List<int> { searchModel.SearchCategoryId },
            manufacturerIds: new List<int> { searchModel.SearchManufacturerId },
            keywords: searchModel.SearchProductName,
            pageIndex: searchModel.Page - 1,
            pageSize: searchModel.PageSize
        );

        var model = await new ErpProductListModel().PrepareToGridAsync(
            searchModel,
            products,
            () =>
            {
                //fill in model values from the entity
                return products.SelectAwait(async product =>
                {
                    var b2bProductModel = new ErpProductDataModel
                    {
                        Name = product.Name,
                        Sku = product.Sku,
                        ManufacturerPartNumber = product.ManufacturerPartNumber,
                        SeName = await _urlRecordService.GetSeNameAsync(product),
                    };
                    return b2bProductModel;
                });
            }
        );
        return model;
    }

    public async Task<IList<ErpProductDataModel>> PrepareErpProductDataListModelAsync(
        List<string> productIds,
        ErpAccount erpAccount
    )
    {
        if (erpAccount == null || productIds == null)
        {
            return null;
        }

        var productDataModels = new List<ErpProductDataModel>();

        var currentCustomer = await _workContext.GetCurrentCustomerAsync();
        var erpCustomerConfiguration = await _erpCustomerFunctionalityService
                .GetErpCustomerConfigurationByNopCustomerIdAsync(currentCustomer.Id);
        var baseWeight = string.Empty;

        if (erpCustomerConfiguration != null &&
            !erpCustomerConfiguration.IsHideWeightInfo)
        {
            baseWeight = (await _measureService.GetMeasureWeightByIdAsync(_measureSettings.BaseWeightId))?.Name;
        }

        foreach (var id in productIds)
        {
            if (!int.TryParse(id, out var productId))
            {
                continue;
            }

            var product = await _productService.GetProductByIdAsync(productId);
            if (product == null)
            {
                continue;
            }

            ErpSpecialPrice productPricing = null;
            if (!_b2BB2CFeaturesSettings.UseProductGroupPrice)
            {
                productPricing =
                    await _erpSpecialPriceService.GetErpSpecialPricesByErpAccountIdAndNopProductIdAsync(
                        erpAccount.Id,
                        productId
                    );
            }

            var shoppingCartItemQuantity = 0;

            if (currentCustomer.HasShoppingCartItems)
            {
                shoppingCartItemQuantity =
                    (await _shoppingCartService.GetShoppingCartAsync(
                        currentCustomer,
                        ShoppingCartType.ShoppingCart,
                        (await _storeContext.GetCurrentStoreAsync()).Id,
                        productId
                    ))?.FirstOrDefault()?.Quantity ?? 0;
            }


            var displayBackInStockSubscription = false;
            if (await _permissionService.AuthorizeAsync(ErpPermissionProvider.DisplayB2BStock)
                && product.ManageInventoryMethod == ManageInventoryMethod.ManageStock
                && product.BackorderMode == BackorderMode.NoBackorders
                && product.AllowBackInStockSubscriptions
                && await _productService.GetTotalStockQuantityAsync(product) <= 0)
            {
                displayBackInStockSubscription = true;
            }

            string stockAvailability = string.Empty;
            bool isOutOfStock = false;
            stockAvailability = await _productService.FormatStockMessageAsync(product, "");
            var outOfStockText = await _localizationService.GetResourceAsync("Products.Availability.OutOfStock");
            isOutOfStock = stockAvailability == outOfStockText;


            var b2bProductDataModel = new ErpProductDataModel()
            {
                Id = productId,
                ManufacturerPartNumber = product.ManufacturerPartNumber ?? string.Empty,
                IsOutOfStock = isOutOfStock,
                StockAvailability = stockAvailability,
                UOM = await _erpSpecificationAttributeService
                    .GetProductUOMByProductIdAndSpecificationAttributeId(
                        productId,
                        _b2BB2CFeaturesSettings.UnitOfMeasureSpecificationAttributeId
                    ) ?? string.Empty,
                Quantity = shoppingCartItemQuantity,
                DisplayBackInStockSubscription = displayBackInStockSubscription
            };

            if (await _permissionService.AuthorizeAsync(ErpPermissionProvider.DisplayB2BPrices) &&
                productPricing != null)
            {
                b2bProductDataModel.PricingNotes = await _erpSpecialPriceService.GetProductPricingNoteByErpSpecialPriceAsync(
                    productPricing,
                    _b2BB2CFeaturesSettings.UseProductGroupPrice,
                    productPricing.Price == _b2BB2CFeaturesSettings.ProductQuotePrice
                );
            }
            else
            {
                b2bProductDataModel.PricingNotes = string.Empty;
            }

            if (_b2BB2CFeaturesSettings.DisplayWeightInformation &&
                erpCustomerConfiguration != null &&
                !erpCustomerConfiguration.IsHideWeightInfo)
            {
                b2bProductDataModel.Weight = product.Weight;
                b2bProductDataModel.WeightValue = $"{product.Weight:F2} {baseWeight}";
            }
            productDataModels.Add(b2bProductDataModel);
        }
        return productDataModels;
    }

    public async Task<decimal> GetErpProductPriceAsync(int productId)
{
    var cacheKey = _staticCacheManager.PrepareKeyForDefaultCache(B2BB2CFeaturesDefaults.ErpProductModelProductPriceCacheKey, productId);
    return await _staticCacheManager.GetAsync(cacheKey, async () =>
    {
        var product = await _productService.GetProductByIdAsync(productId);
        if (product == null)
            return decimal.Zero;

        (_, var finalPrice, _, _) = await _priceCalculationService.GetFinalPriceAsync
        (product, await _workContext.GetCurrentCustomerAsync(), await _storeContext.GetCurrentStoreAsync(), includeDiscounts: true);
        return finalPrice;
    });
}

public async Task<ProductInCartQuantityModel> PrepareProductInCartQuantityModelAsync(int productId)
{
    var product = await _productService.GetProductByIdAsync(productId);

    if (product == null)
        return new ProductInCartQuantityModel();

    var currCustomer = await _workContext.GetCurrentCustomerAsync();

    var shoppingCartItemQuantity = currCustomer.HasShoppingCartItems ?
                    (await _shoppingCartService.GetShoppingCartAsync(currCustomer, ShoppingCartType.ShoppingCart, (await _storeContext.GetCurrentStoreAsync()).Id, product.Id))?.FirstOrDefault()?.Quantity ?? 0 : 0;

    return new ProductInCartQuantityModel
    {
        Id = product.Id,
        Quantity = shoppingCartItemQuantity,
    };
}

public async Task<IList<ProductInCartQuantityModel>> PrepareProductInCartQuantityModelAsync(List<string> productIds)
{
    var productInCartQuantityModels = new List<ProductInCartQuantityModel>();

    if (productIds != null)
    {
        foreach (var id in productIds)
        {
            if (int.TryParse(id, out var productId))
            {
                var product = await _productService.GetProductByIdAsync(productId);

                if (product == null)
                {
                    continue;
                }
                var currentCustomer = await _workContext.GetCurrentCustomerAsync();
                var shoppingCartItemQuantity = currentCustomer.HasShoppingCartItems ?
                    (await _shoppingCartService.GetShoppingCartAsync(currentCustomer, ShoppingCartType.ShoppingCart, (await _storeContext.GetCurrentStoreAsync()).Id, productId))?.FirstOrDefault()?.Quantity ?? 0 : 0;
                productInCartQuantityModels.Add(new ProductInCartQuantityModel
                {
                    Id = productId,
                    Quantity = shoppingCartItemQuantity
                });
            }
        }
    }
    return productInCartQuantityModels;
}

public async Task<string> UpdateLiveStockAndGetProductAvailabilityAsync(
    int productId,
    ErpAccount erpAccount
)
{
    var product = await _productService.GetProductByIdAsync(productId);

    if (product == null)
        return null;
    var categoryIds = new List<int>();
    if (!string.IsNullOrWhiteSpace(_b2BB2CFeaturesSettings.SkipLiveStockCheckCategoryIds))
    {
        categoryIds = _b2BB2CFeaturesSettings
            .SkipLiveStockCheckCategoryIds.Split(',')
            .Select(int.Parse)
            .ToList();
    }
    var productCategories = await _categoryService.GetProductCategoriesByProductIdAsync(
        productId
    );
    foreach (var cat in productCategories)
    {
        if (categoryIds.Exists(a => a == cat.CategoryId))
            return await _productService.FormatStockMessageAsync(product, "");
    }

    if (_b2BB2CFeaturesSettings.EnableLiveStockChecks)
    {
        await _erpPriceSyncFunctionalityService.ProductListLiveStockSyncAsync(erpAccount, new List<Product> { product });
    }

    var stockAvailability = await _productService.FormatStockMessageAsync(product, "");
    return stockAvailability;
}

public async Task<ErpOrderSummaryModel> PrepareErpOrderSummaryModelAsync()
{
    var orderSummeryModel = new ErpOrderSummaryModel();
    var totalPriceWithOutSavings = decimal.Zero;
    var b2bOnlineOrderDiscount = decimal.Zero;
    var productForQuote = false;
    var currentCustomer = await _workContext.GetCurrentCustomerAsync();
    if (currentCustomer.HasShoppingCartItems)
    {
        var baseWeight = "";
        if (_b2BB2CFeaturesSettings.DisplayWeightInformation)
        {
            var measureWeight = await _measureService.GetMeasureWeightByIdAsync(_measureSettings.BaseWeightId);
            baseWeight = measureWeight?.Name;
        }

        var shoppingCartItems = (
            await _shoppingCartService.GetShoppingCartAsync(currentCustomer)
        )
            ?.Where(x => x.ShoppingCartType == ShoppingCartType.ShoppingCart)
            .ToList();

        var b2bAccount = await _erpAccountService.GetActiveErpAccountByCustomerIdAsync(
            currentCustomer.Id
        );

        var erpUser = await _erpCustomerFunctionalityService.GetActiveErpNopUserByCustomerAsync(
            currentCustomer
        );

        if (b2bAccount == null || erpUser == null)
            return new ErpOrderSummaryModel();

        var isB2CUser = erpUser.ErpUserType == ErpUserType.B2CUser;

        foreach (var cartItemProduct in shoppingCartItems)
        {
            var product = await _productService.GetProductByIdAsync(cartItemProduct.ProductId);
            if (product == null)
            {
                continue;
            }

            var productQuantity = cartItemProduct.Quantity;
            if (!isB2CUser)
                productQuantity = await _productService.GetTotalStockQuantityAsync(product);

            (var unitPrice, var discountAmount, _) = await _shoppingCartService.GetUnitPriceAsync(cartItemProduct, false);
            var b2bProductPrice = unitPrice + discountAmount;
            totalPriceWithOutSavings += b2bProductPrice * cartItemProduct.Quantity;
            b2bOnlineOrderDiscount += discountAmount * cartItemProduct.Quantity;

            var discountForPerUnitProduct = "";
            var unitPriceWithOutDiscount = "";
            if (unitPrice == _b2BB2CFeaturesSettings.ProductQuotePrice)
            {
                productForQuote = true;
                unitPriceWithOutDiscount = await _localizationService.GetResourceAsync("Products.ProductForQuote");
                discountForPerUnitProduct = await _localizationService.GetResourceAsync("Products.ProductForQuote");
            }
            else
            {
                unitPriceWithOutDiscount = await _priceFormatter.FormatPriceAsync(b2bProductPrice);
                discountForPerUnitProduct = await _priceFormatter.FormatPriceAsync(discountAmount);
            }

            orderSummeryModel.Items.Add(
                new ErpOrderSummaryModel.ErpOrderSummaryItemModel
                {
                    Id = cartItemProduct.ProductId,
                    Weight = product.Weight,
                    WeightValue = _b2BB2CFeaturesSettings.DisplayWeightInformation ? $"{product.Weight:F2} {baseWeight}" : "",
                    StockAvailability = await _productService.FormatStockMessageAsync(
                        product,
                        ""
                    ),
                    Uom =
                        await _erpSpecificationAttributeService.GetProductUOMByProductIdAndSpecificationAttributeId(
                            cartItemProduct.ProductId,
                            _b2BB2CFeaturesSettings.UnitOfMeasureSpecificationAttributeId
                        ) ?? string.Empty,
                    IsBackOrder = productQuantity < cartItemProduct.Quantity,
                    UnitPriceWithOutDiscount = unitPriceWithOutDiscount,
                    DiscountForPerUnitProduct = discountForPerUnitProduct,
                }
            );
        }
        if (productForQuote)
        {
            orderSummeryModel.TotalPriceWithOutSavings = await _localizationService.GetResourceAsync("Products.ProductForQuote");
            orderSummeryModel.ErpOnlineOrderDiscount = await _localizationService.GetResourceAsync("Products.ProductForQuote");
        }
        else
        {
            orderSummeryModel.TotalPriceWithOutSavings = await _priceFormatter.FormatPriceAsync(
                totalPriceWithOutSavings
            );
            orderSummeryModel.ErpOnlineOrderDiscount = await _priceFormatter.FormatPriceAsync(
                b2bOnlineOrderDiscount
            );
        }
    }
    return orderSummeryModel;
}

public async Task<IList<(string, int)>> GetOrderItemsUOMData(int orderId)
{
    var orderItems = await _orderService.GetOrderItemsAsync(orderId);
    var uomData = new List<(string, int)>();
    foreach (var orderItem in orderItems)
    {
        uomData.Add(
            (
                await _erpSpecificationAttributeService.GetProductUOMByProductIdAndSpecificationAttributeId(
                    orderItem.ProductId,
                    _b2BB2CFeaturesSettings.UnitOfMeasureSpecificationAttributeId
                ) ?? string.Empty,
                orderItem.ProductId
            )
        );
    }

    return uomData;
}

    #endregion Method
}