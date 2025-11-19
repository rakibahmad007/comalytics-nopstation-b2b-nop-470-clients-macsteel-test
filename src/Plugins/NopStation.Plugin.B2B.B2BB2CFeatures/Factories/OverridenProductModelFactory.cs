using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DocumentFormat.OpenXml.EMMA;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Core;
using Nop.Core.Caching;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Media;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Security;
using Nop.Core.Domain.Seo;
using Nop.Core.Domain.Shipping;
using Nop.Core.Domain.Vendors;
using Nop.Services.Catalog;
using Nop.Services.Common;
using Nop.Services.Customers;
using Nop.Services.Directory;
using Nop.Services.Helpers;
using Nop.Services.Localization;
using Nop.Services.Media;
using Nop.Services.Orders;
using Nop.Services.Security;
using Nop.Services.Seo;
using Nop.Services.Shipping.Date;
using Nop.Services.Stores;
using Nop.Services.Tax;
using Nop.Services.Vendors;
using Nop.Web.Factories;
using Nop.Web.Models.Catalog;
using NopStation.Plugin.B2B.B2BB2CFeatures.Services.ErpCustomerFunctionality;
using NopStation.Plugin.B2B.ERPIntegrationCore.Infrastructure;
using NopStation.Plugin.B2B.ERPIntegrationCore.Services;

namespace NopStation.Plugin.B2B.B2BB2CFeatures.Factories;

public class OverridenProductModelFactory : ProductModelFactory
{
    #region Fields

    private readonly IErpSpecialPriceService _erpSpecialPriceService;
    private readonly IErpCustomerFunctionalityService _erpCustomerFunctionality;
    private readonly B2BB2CFeaturesSettings _b2BB2CFeaturesSettings;

    #endregion

    #region Ctor

    public OverridenProductModelFactory(CaptchaSettings captchaSettings,
        CatalogSettings catalogSettings,
        CustomerSettings customerSettings,
        ICategoryService categoryService,
        ICurrencyService currencyService,
        ICustomerService customerService,
        IDateRangeService dateRangeService,
        IDateTimeHelper dateTimeHelper,
        IDownloadService downloadService,
        IGenericAttributeService genericAttributeService,
        IJsonLdModelFactory jsonLdModelFactory,
        ILocalizationService localizationService,
        IManufacturerService manufacturerService,
        IPermissionService permissionService,
        IPictureService pictureService,
        IPriceCalculationService priceCalculationService,
        IPriceFormatter priceFormatter,
        IProductAttributeParser productAttributeParser,
        IProductAttributeService productAttributeService,
        IProductService productService,
        IProductTagService productTagService,
        IProductTemplateService productTemplateService,
        IReviewTypeService reviewTypeService,
        IShoppingCartService shoppingCartService,
        ISpecificationAttributeService specificationAttributeService,
        IStaticCacheManager staticCacheManager,
        IStoreContext storeContext,
        IStoreService storeService,
        IShoppingCartModelFactory shoppingCartModelFactory,
        ITaxService taxService,
        IUrlRecordService urlRecordService,
        IVendorService vendorService,
        IVideoService videoService,
        IWebHelper webHelper,
        IWorkContext workContext,
        MediaSettings mediaSettings,
        OrderSettings orderSettings,
        SeoSettings seoSettings,
        ShippingSettings shippingSettings,
        VendorSettings vendorSettings,
        IErpSpecialPriceService erpSpecialPriceService,
        IErpCustomerFunctionalityService erpCustomerFunctionality,
        B2BB2CFeaturesSettings b2BB2CFeaturesSettings) : base(captchaSettings,
            catalogSettings,
            customerSettings,
            categoryService,
            currencyService,
            customerService,
            dateRangeService,
            dateTimeHelper,
            downloadService,
            genericAttributeService,
            jsonLdModelFactory,
            localizationService,
            manufacturerService,
            permissionService,
            pictureService,
            priceCalculationService,
            priceFormatter,
            productAttributeParser,
            productAttributeService,
            productService,
            productTagService,
            productTemplateService,
            reviewTypeService,
            shoppingCartService,
            specificationAttributeService,
            staticCacheManager,
            storeContext,
            storeService,
            shoppingCartModelFactory,
            taxService,
            urlRecordService,
            vendorService,
            videoService,
            webHelper,
            workContext,
            mediaSettings,
            orderSettings,
            seoSettings,
            shippingSettings,
            vendorSettings)
    {
        _erpSpecialPriceService = erpSpecialPriceService;
        _erpCustomerFunctionality = erpCustomerFunctionality;
        _b2BB2CFeaturesSettings = b2BB2CFeaturesSettings;
    }

    #endregion

    #region Utilities

    protected override async Task<ProductDetailsModel.AddToCartModel> PrepareProductAddToCartModelAsync(Product product, ShoppingCartItem updatecartitem)
    {
        ArgumentNullException.ThrowIfNull(product);

        var model = new ProductDetailsModel.AddToCartModel
        {
            ProductId = product.Id
        };

        if (updatecartitem != null)
        {
            model.UpdatedShoppingCartItemId = updatecartitem.Id;
            model.UpdateShoppingCartItemType = updatecartitem.ShoppingCartType;
        }

        //quantity
        model.EnteredQuantity = updatecartitem != null ? updatecartitem.Quantity : product.OrderMinimumQuantity;
        //allowed quantities
        var allowedQuantities = _productService.ParseAllowedQuantities(product);
        foreach (var qty in allowedQuantities)
        {
            model.AllowedQuantities.Add(new SelectListItem
            {
                Text = qty.ToString(),
                Value = qty.ToString(),
                Selected = updatecartitem != null && updatecartitem.Quantity == qty
            });
        }
        //minimum quantity notification
        if (product.OrderMinimumQuantity > 1)
        {
            model.MinimumQuantityNotification = string.Format(await _localizationService.GetResourceAsync("Products.MinimumQuantityNotification"), product.OrderMinimumQuantity);
        }

        //'add to cart', 'add to wishlist' buttons
        model.DisableBuyButton = product.DisableBuyButton || !await _permissionService.AuthorizeAsync(StandardPermissionProvider.EnableShoppingCart);
        model.DisableWishlistButton = product.DisableWishlistButton || !await _permissionService.AuthorizeAsync(StandardPermissionProvider.EnableWishlist);
        
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.DisplayPrices) ||
            !await _permissionService.AuthorizeAsync(ErpPermissionProvider.DisplayB2BPrices))
        {
            model.DisableBuyButton = true;
            model.DisableWishlistButton = true;
        }

        //pre-order
        if (product.AvailableForPreOrder)
        {
            model.AvailableForPreOrder = !product.PreOrderAvailabilityStartDateTimeUtc.HasValue ||
                                         product.PreOrderAvailabilityStartDateTimeUtc.Value >= DateTime.UtcNow;
            model.PreOrderAvailabilityStartDateTimeUtc = product.PreOrderAvailabilityStartDateTimeUtc;

            if (model.AvailableForPreOrder &&
                model.PreOrderAvailabilityStartDateTimeUtc.HasValue &&
                _catalogSettings.DisplayDatePreOrderAvailability)
            {
                model.PreOrderAvailabilityStartDateTimeUserTime =
                    (await _dateTimeHelper.ConvertToUserTimeAsync(model.PreOrderAvailabilityStartDateTimeUtc.Value)).ToString("D");
            }
        }
        //rental
        model.IsRental = product.IsRental;

        //customer entered price
        model.CustomerEntersPrice = product.CustomerEntersPrice;
        if (!model.CustomerEntersPrice)
            return model;

        var currentCurrency = await _workContext.GetWorkingCurrencyAsync();
        var minimumCustomerEnteredPrice = await _currencyService.ConvertFromPrimaryStoreCurrencyAsync(product.MinimumCustomerEnteredPrice, currentCurrency);
        var maximumCustomerEnteredPrice = await _currencyService.ConvertFromPrimaryStoreCurrencyAsync(product.MaximumCustomerEnteredPrice, currentCurrency);

        model.CustomerEnteredPrice = updatecartitem != null ? updatecartitem.CustomerEnteredPrice : minimumCustomerEnteredPrice;
        model.CustomerEnteredPriceRange = string.Format(await _localizationService.GetResourceAsync("Products.EnterProductPrice.Range"),
            await _priceFormatter.FormatPriceAsync(minimumCustomerEnteredPrice, false, false),
            await _priceFormatter.FormatPriceAsync(maximumCustomerEnteredPrice, false, false));

        return model;
    }

    #endregion

    #region Methods

    protected override async Task PrepareSimpleProductOverviewPriceModelAsync(
        Product product,
        ProductOverviewModel.ProductPriceModel priceModel
    )
    {
        priceModel.DisableBuyButton =
            product.DisableBuyButton
            || !await _permissionService.AuthorizeAsync(StandardPermissionProvider.EnableShoppingCart)
            || !await _permissionService.AuthorizeAsync(StandardPermissionProvider.DisplayPrices);

        #region Custom for B2B

        var totalStockQuantity = await _productService.GetTotalStockQuantityAsync(product);

        priceModel.DisableBuyButton = await _erpCustomerFunctionality
            .B2BDisableBuyButtonAsync(product, currentValue: priceModel.DisableBuyButton, totalStockQuantity: totalStockQuantity);

        #endregion

        priceModel.DisableWishlistButton =
            product.DisableWishlistButton
            || !await _permissionService.AuthorizeAsync(StandardPermissionProvider.EnableWishlist)
            || !await _permissionService.AuthorizeAsync(StandardPermissionProvider.DisplayPrices) 
            || !await _permissionService.AuthorizeAsync(ErpPermissionProvider.DisplayB2BPrices);
        priceModel.DisableAddToCompareListButton = !_catalogSettings.CompareProductsEnabled;

        priceModel.IsRental = product.IsRental;

        if (product.AvailableForPreOrder)
        {
            priceModel.AvailableForPreOrder =
                !product.PreOrderAvailabilityStartDateTimeUtc.HasValue
                || product.PreOrderAvailabilityStartDateTimeUtc.Value >= DateTime.UtcNow;
            priceModel.PreOrderAvailabilityStartDateTimeUtc =
                product.PreOrderAvailabilityStartDateTimeUtc;
        }

        if (!await _customerService.IsRegisteredAsync(await _workContext.GetCurrentCustomerAsync())
            && _b2BB2CFeaturesSettings.IsShowLoginForPrice)
        {
            priceModel.OldPrice = null;
            priceModel.Price = await _localizationService.GetResourceAsync(
                "NopStation.Plugin.B2B.B2BB2CFeatures.Products.Price.LoginForPrice"
            );
        }
        else if (await _permissionService.AuthorizeAsync(StandardPermissionProvider.DisplayPrices)
            || await _permissionService.AuthorizeAsync(ErpPermissionProvider.DisplayB2BPrices))
        {
            if (product.CustomerEntersPrice)
                return;

            if (product.CallForPrice
                && (
                    !_orderSettings.AllowAdminsToBuyCallForPriceProducts
                    || _workContext.OriginalCustomerIfImpersonated == null
                )
            )
            {
                priceModel.OldPrice = null;
                priceModel.OldPriceValue = null;
                priceModel.Price = await _localizationService.GetResourceAsync(
                    "Products.CallForPrice"
                );
                priceModel.PriceValue = null;
            }
            else
            {
                var store = await _storeContext.GetCurrentStoreAsync();
                var customer = await _workContext.GetCurrentCustomerAsync();

                var (minPossiblePriceWithoutDiscount, minPossiblePriceWithDiscount) = (
                    decimal.Zero,
                    decimal.Zero
                );
                var hasMultiplePrices = false;
                if (_catalogSettings.DisplayFromPrices)
                {
                    var customerRoleIds = await _customerService.GetCustomerRoleIdsAsync(customer);
                    var cacheKey = _staticCacheManager.PrepareKeyForDefaultCache(
                        NopCatalogDefaults.ProductMultiplePriceCacheKey,
                        product,
                        customerRoleIds,
                        store
                    );
                    if (!_catalogSettings.CacheProductPrices || product.IsRental)
                        cacheKey.CacheTime = 0;

                    var cachedPrice = await _staticCacheManager.GetAsync(
                        cacheKey,
                        async () =>
                        {
                            var prices =
                                new List<(
                                    decimal PriceWithoutDiscount,
                                    decimal PriceWithDiscount
                                )>();

                            var attributesMappings =
                                await _productAttributeService.GetProductAttributeMappingsByProductIdAsync(
                                    product.Id
                                );
                            if (
                                !attributesMappings.Any(am =>
                                    !am.IsNonCombinable() && am.IsRequired
                                )
                            )
                            {
                                (var priceWithoutDiscount, var priceWithDiscount, _, _) =
                                    await _priceCalculationService.GetFinalPriceAsync(
                                        product,
                                        customer,
                                        store
                                    );
                                prices.Add((priceWithoutDiscount, priceWithDiscount));
                            }

                            var allAttributesXml =
                                await _productAttributeParser.GenerateAllCombinationsAsync(
                                    product,
                                    true
                                );
                            foreach (var attributesXml in allAttributesXml)
                            {
                                var warnings = new List<string>();
                                warnings.AddRange(
                                    await _shoppingCartService.GetShoppingCartItemAttributeWarningsAsync(
                                        customer,
                                        ShoppingCartType.ShoppingCart,
                                        product,
                                        1,
                                        attributesXml,
                                        true,
                                        true,
                                        true
                                    )
                                );
                                if (warnings.Count != 0)
                                    continue;

                                var combination =
                                    await _productAttributeParser.FindProductAttributeCombinationAsync(
                                        product,
                                        attributesXml
                                    );
                                if (combination?.OverriddenPrice.HasValue ?? false)
                                {
                                    (var priceWithoutDiscount, var priceWithDiscount, _, _) =
                                        await _priceCalculationService.GetFinalPriceAsync(
                                            product,
                                            customer,
                                            store,
                                            combination.OverriddenPrice.Value,
                                            decimal.Zero,
                                            true,
                                            1,
                                            null,
                                            null
                                        );
                                    prices.Add((priceWithoutDiscount, priceWithDiscount));
                                }
                                else
                                {
                                    var additionalCharge = decimal.Zero;
                                    var attributeValues =
                                        await _productAttributeParser.ParseProductAttributeValuesAsync(
                                            attributesXml
                                        );
                                    foreach (var attributeValue in attributeValues)
                                    {
                                        additionalCharge +=
                                            await _priceCalculationService.GetProductAttributeValuePriceAdjustmentAsync(
                                                product,
                                                attributeValue,
                                                customer,
                                                store
                                            );
                                    }
                                    if (additionalCharge != decimal.Zero)
                                    {
                                        (var priceWithoutDiscount, var priceWithDiscount, _, _) =
                                            await _priceCalculationService.GetFinalPriceAsync(
                                                product,
                                                customer,
                                                store,
                                                additionalCharge
                                            );
                                        prices.Add((priceWithoutDiscount, priceWithDiscount));
                                    }
                                }
                            }

                            if (prices.Distinct().Count() > 1)
                            {
                                (minPossiblePriceWithoutDiscount, minPossiblePriceWithDiscount) =
                                    prices.OrderBy(p => p.PriceWithDiscount).First();
                                return new
                                {
                                    PriceWithoutDiscount = minPossiblePriceWithoutDiscount,
                                    PriceWithDiscount = minPossiblePriceWithDiscount,
                                };
                            }

                            (minPossiblePriceWithoutDiscount, minPossiblePriceWithDiscount, _, _) =
                                await _priceCalculationService.GetFinalPriceAsync(
                                    product,
                                    customer,
                                    store
                                );

                            return null;
                        }
                    );

                    if (cachedPrice is not null)
                    {
                        hasMultiplePrices = true;
                        (minPossiblePriceWithoutDiscount, minPossiblePriceWithDiscount) = (
                            cachedPrice.PriceWithoutDiscount,
                            cachedPrice.PriceWithDiscount
                        );
                    }
                }
                else
                    (minPossiblePriceWithoutDiscount, minPossiblePriceWithDiscount, _, _) =
                        await _priceCalculationService.GetFinalPriceAsync(product, customer, store);

                if (product.HasTierPrices)
                {
                    var (
                        tierPriceMinPossiblePriceWithoutDiscount,
                        tierPriceMinPossiblePriceWithDiscount,
                        _,
                        _
                    ) = await _priceCalculationService.GetFinalPriceAsync(
                        product,
                        customer,
                        store,
                        quantity: int.MaxValue
                    );

                    minPossiblePriceWithoutDiscount = Math.Min(
                        minPossiblePriceWithoutDiscount,
                        tierPriceMinPossiblePriceWithoutDiscount
                    );
                    minPossiblePriceWithDiscount = Math.Min(
                        minPossiblePriceWithDiscount,
                        tierPriceMinPossiblePriceWithDiscount
                    );
                }

                #region B2B specific change for list price strike through as old price

                var strikeThroughPrice = decimal.Zero;
                var oldPriceBase = decimal.Zero;
                var erpAccount = await _erpCustomerFunctionality.GetActiveErpAccountByCustomerAsync(customer);
                if (erpAccount != null)
                {
                    var specialPrice =
                        await _erpSpecialPriceService.GetErpSpecialPricesByErpAccountIdAndNopProductIdAsync(
                            erpAccount.Id,
                            product.Id
                        );
                    if (specialPrice != null && specialPrice.ListPrice > 0)
                    {
                        oldPriceBase = specialPrice.ListPrice;
                    }
                }

                #endregion

                var (finalPriceWithoutDiscountBase, _) = await _taxService.GetProductPriceAsync(product, minPossiblePriceWithoutDiscount);
                var (finalPriceWithDiscountBase, _) = await _taxService.GetProductPriceAsync(product, minPossiblePriceWithDiscount);

                (oldPriceBase, _) = await _taxService.GetProductPriceAsync(product, oldPriceBase);
                var currentCurrency = await _workContext.GetWorkingCurrencyAsync();
                var oldPrice = await _currencyService.ConvertFromPrimaryStoreCurrencyAsync(
                    oldPriceBase,
                    currentCurrency
                );
                var finalPriceWithoutDiscount =
                    await _currencyService.ConvertFromPrimaryStoreCurrencyAsync(
                        finalPriceWithoutDiscountBase,
                        currentCurrency
                    );
                var finalPriceWithDiscount =
                    await _currencyService.ConvertFromPrimaryStoreCurrencyAsync(
                        finalPriceWithDiscountBase,
                        currentCurrency
                    );
                if (finalPriceWithoutDiscountBase != oldPriceBase && oldPriceBase > decimal.Zero)
                    strikeThroughPrice = oldPrice;

                if (finalPriceWithoutDiscountBase != finalPriceWithDiscountBase)
                    strikeThroughPrice = finalPriceWithoutDiscount;

                if (strikeThroughPrice > decimal.Zero)
                {
                    priceModel.OldPrice = await _priceFormatter.FormatPriceAsync(
                        strikeThroughPrice
                    );
                    priceModel.OldPriceValue = strikeThroughPrice;
                }
                else
                {
                    priceModel.OldPrice = null;
                    priceModel.OldPriceValue = null;
                }

                var tierPrices = product.HasTierPrices
                    ? await _productService.GetTierPricesAsync(product, customer, store)
                    : new List<TierPrice>();

                var hasTierPrices =
                    tierPrices.Any() && !(tierPrices.Count == 1 && tierPrices[0].Quantity <= 1);

                var price = await _priceFormatter.FormatPriceAsync(finalPriceWithDiscount);
                priceModel.Price =
                    hasTierPrices || hasMultiplePrices
                        ? string.Format(
                            await _localizationService.GetResourceAsync("Products.PriceRangeFrom"),
                            price
                        )
                        : price;
                priceModel.PriceValue = finalPriceWithDiscount;

                if (product.IsRental)
                {
                    priceModel.OldPrice = await _priceFormatter.FormatRentalProductPeriodAsync(
                        product,
                        priceModel.OldPrice
                    );
                    priceModel.Price = await _priceFormatter.FormatRentalProductPeriodAsync(
                        product,
                        priceModel.Price
                    );
                }

                priceModel.DisplayTaxShippingInfo =
                    _catalogSettings.DisplayTaxShippingInfoProductBoxes
                    && product.IsShipEnabled
                    && !product.IsFreeShipping;

                priceModel.BasePricePAngV = await _priceFormatter.FormatBasePriceAsync(
                    product,
                    finalPriceWithDiscount
                );
                priceModel.BasePricePAngVValue = finalPriceWithDiscount;

                #region Custom (B2B)
                if (priceModel.PriceValue == _b2BB2CFeaturesSettings.ProductQuotePrice)
                {
                    priceModel.Price = await _localizationService.GetResourceAsync("Products.ProductForQuote");
                }
                else if (minPossiblePriceWithoutDiscount == decimal.Zero)
                {
                    priceModel.OldPrice = null;
                    priceModel.Price = await _localizationService.GetResourceAsync(
                        "Products.CallForPrice"
                    );
                    priceModel.BasePricePAngV = null;
                }

                #endregion
            }
        }
        else
        {
            priceModel.OldPrice = null;
            priceModel.OldPriceValue = null;
            priceModel.Price = null;
            priceModel.PriceValue = null;
        }
    }

    protected override async Task<ProductDetailsModel.ProductPriceModel> PrepareProductPriceModelAsync(
        Product product
    )
    {
        ArgumentNullException.ThrowIfNull(product);

        var model = new ProductDetailsModel.ProductPriceModel { ProductId = product.Id };

        var isProductOnSpecial =
            await _erpCustomerFunctionality.IsTheProductFromSpecialCategoryAsync(product);
        model.CustomProperties.Add(
            B2BB2CFeaturesDefaults.ProductIsOnSpecial,
            $"{isProductOnSpecial}"
        );

        if (!await _customerService.IsRegisteredAsync(await _workContext.GetCurrentCustomerAsync())
            && _b2BB2CFeaturesSettings.IsShowLoginForPrice)
        {
            model.OldPrice = null;
            model.Price = await _localizationService.GetResourceAsync("NopStation.Plugin.B2B.B2BB2CFeatures.Products.Price.LoginForPrice");
        }
        else if (await _permissionService.AuthorizeAsync(StandardPermissionProvider.DisplayPrices)
            || await _permissionService.AuthorizeAsync(ErpPermissionProvider.DisplayB2BPrices))
        {
            model.HidePrices = false;
            if (product.CustomerEntersPrice)
            {
                model.CustomerEntersPrice = true;
            }
            else
            {
                if (
                    product.CallForPrice
                    && (
                        !_orderSettings.AllowAdminsToBuyCallForPriceProducts
                        || _workContext.OriginalCustomerIfImpersonated == null
                    )
                )
                {
                    model.CallForPrice = true;
                }
                else
                {
                    var customer = await _workContext.GetCurrentCustomerAsync();
                    var store = await _storeContext.GetCurrentStoreAsync();
                    var currentCurrency = await _workContext.GetWorkingCurrencyAsync();

                    #region B2B specific change for list price strike through as old price

                    var oldPriceBase = decimal.Zero;
                    var erpAccount =
                        await _erpCustomerFunctionality.GetActiveErpAccountByCustomerAsync(
                            customer
                        );
                    if (erpAccount != null)
                    {
                        var specialPrice =
                            await _erpSpecialPriceService.GetErpSpecialPricesByErpAccountIdAndNopProductIdAsync(
                                erpAccount.Id,
                                product.Id
                            );
                        if (specialPrice != null && specialPrice.ListPrice > 0)
                        {
                            oldPriceBase = specialPrice.ListPrice;
                        }
                    }

                    #endregion

                    var (finalPriceWithoutDiscountBase, _) = await _taxService.GetProductPriceAsync(product, (await _priceCalculationService.GetFinalPriceAsync(product, customer, store, includeDiscounts: false)).finalPrice);
                    var (finalPriceWithDiscountBase, _) = await _taxService.GetProductPriceAsync(product, (await _priceCalculationService.GetFinalPriceAsync(product, customer, store)).finalPrice);
                    
                    (oldPriceBase, _) = await _taxService.GetProductPriceAsync(product, oldPriceBase);
                    var oldPrice = await _currencyService.ConvertFromPrimaryStoreCurrencyAsync(oldPriceBase, currentCurrency);
                    var finalPriceWithoutDiscount = await _currencyService.ConvertFromPrimaryStoreCurrencyAsync(finalPriceWithoutDiscountBase, currentCurrency);
                    var finalPriceWithDiscount = await _currencyService.ConvertFromPrimaryStoreCurrencyAsync(finalPriceWithDiscountBase, currentCurrency);

                    if (
                        finalPriceWithoutDiscountBase != oldPriceBase
                        && oldPriceBase > decimal.Zero
                    )
                    {
                        model.OldPrice = await _priceFormatter.FormatPriceAsync(oldPrice);
                        model.OldPriceValue = oldPrice;
                    }

                    model.Price =
                        finalPriceWithoutDiscount > decimal.Zero
                            ? await _priceFormatter.FormatPriceAsync(finalPriceWithoutDiscount)
                            : await _localizationService.GetResourceAsync("Products.CallForPrice");

                    if (finalPriceWithoutDiscountBase != finalPriceWithDiscountBase)
                    {
                        model.PriceWithDiscount = await _priceFormatter.FormatPriceAsync(
                            finalPriceWithDiscount
                        );
                        model.PriceWithDiscountValue = finalPriceWithDiscount;
                    }

                    model.PriceValue = finalPriceWithDiscount;

                    model.DisplayTaxShippingInfo =
                        _catalogSettings.DisplayTaxShippingInfoProductDetailsPage
                        && product.IsShipEnabled
                        && !product.IsFreeShipping;

                    model.BasePricePAngV = await _priceFormatter.FormatBasePriceAsync(
                        product,
                        finalPriceWithDiscountBase
                    );
                    model.BasePricePAngVValue = finalPriceWithDiscountBase;
                    model.CurrencyCode = currentCurrency.CurrencyCode;

                    if (product.IsRental)
                    {
                        model.IsRental = true;
                        var priceStr = await _priceFormatter.FormatPriceAsync(
                            finalPriceWithDiscount
                        );
                        model.RentalPrice = await _priceFormatter.FormatRentalProductPeriodAsync(
                            product,
                            priceStr
                        );
                        model.RentalPriceValue = finalPriceWithDiscount;
                    }

                    #region Custom (B2B)

                    if (finalPriceWithoutDiscount == decimal.Zero)
                    {
                        model.CallForPrice = true;
                        model.Price = await _localizationService.GetResourceAsync(
                            "Products.CallForPrice"
                        );
                    }
                    if (model.PriceValue == _b2BB2CFeaturesSettings.ProductQuotePrice)
                    {
                        model.Price = await _localizationService.GetResourceAsync("Products.ProductForQuote");
                        model.CallForPrice = false;
                    }

                    #endregion
                }
            }
        }
        else
        {
            model.HidePrices = true;
            model.OldPrice = null;
            model.OldPriceValue = null;
            model.Price = null;
        }

        return model;
    }

    protected override async Task PrepareGroupedProductOverviewPriceModelAsync(
        Product product,
        ProductOverviewModel.ProductPriceModel priceModel
    )
    {
        var store = await _storeContext.GetCurrentStoreAsync();
        var associatedProducts = await _productService.GetAssociatedProductsAsync(
            product.Id,
            store.Id
        );

        priceModel.DisableBuyButton =
            !await _permissionService.AuthorizeAsync(StandardPermissionProvider.EnableShoppingCart)
            || !await _permissionService.AuthorizeAsync(StandardPermissionProvider.DisplayPrices);

        #region Custom for B2B

        var totalStockQuantity = await _productService.GetTotalStockQuantityAsync(product);

        priceModel.DisableBuyButton = await _erpCustomerFunctionality
            .B2BDisableBuyButtonAsync(product, currentValue: priceModel.DisableBuyButton, totalStockQuantity: totalStockQuantity);

        #endregion

        priceModel.DisableWishlistButton =
            !await _permissionService.AuthorizeAsync(StandardPermissionProvider.EnableWishlist)
            || !await _permissionService.AuthorizeAsync(StandardPermissionProvider.DisplayPrices) 
            || !await _permissionService.AuthorizeAsync(ErpPermissionProvider.DisplayB2BPrices);

        priceModel.DisableAddToCompareListButton = !_catalogSettings.CompareProductsEnabled;
        if (!associatedProducts.Any())
            return;

        if (!await _customerService.IsRegisteredAsync(await _workContext.GetCurrentCustomerAsync()) && _b2BB2CFeaturesSettings.IsShowLoginForPrice)
        {
            priceModel.OldPrice = null;
            priceModel.Price = await _localizationService.GetResourceAsync(
                "NopStation.Plugin.B2B.B2BB2CFeatures.Products.Price.LoginForPrice"
            );
        }
        else if (await _permissionService.AuthorizeAsync(StandardPermissionProvider.DisplayPrices)
            || await _permissionService.AuthorizeAsync(ErpPermissionProvider.DisplayB2BPrices))
        {
            decimal? minPossiblePrice = null;
            Product minPriceProduct = null;
            var customer = await _workContext.GetCurrentCustomerAsync();
            foreach (var associatedProduct in associatedProducts)
            {
                var (_, tmpMinPossiblePrice, _, _) =
                    await _priceCalculationService.GetFinalPriceAsync(
                        associatedProduct,
                        customer,
                        store
                    );

                if (associatedProduct.HasTierPrices)
                {
                    tmpMinPossiblePrice = Math.Min(
                        tmpMinPossiblePrice,
                        (
                            await _priceCalculationService.GetFinalPriceAsync(
                                associatedProduct,
                                customer,
                                store,
                                quantity: int.MaxValue
                            )
                        ).finalPrice
                    );
                }

                if (minPossiblePrice.HasValue && tmpMinPossiblePrice >= minPossiblePrice.Value)
                    continue;
                minPriceProduct = associatedProduct;
                minPossiblePrice = tmpMinPossiblePrice;
            }

            if (minPriceProduct == null || minPriceProduct.CustomerEntersPrice)
                return;

            if (
                minPriceProduct.CallForPrice
                && (
                    !_orderSettings.AllowAdminsToBuyCallForPriceProducts
                    || _workContext.OriginalCustomerIfImpersonated == null
                )
            )
            {
                priceModel.OldPrice = null;
                priceModel.OldPriceValue = null;
                priceModel.Price = await _localizationService.GetResourceAsync(
                    "Products.CallForPrice"
                );
                priceModel.PriceValue = null;
            }
            else
            {
                var (finalPriceBase, _) = await _taxService.GetProductPriceAsync(
                    minPriceProduct,
                    minPossiblePrice.Value
                );
                var finalPrice = await _currencyService.ConvertFromPrimaryStoreCurrencyAsync(
                    finalPriceBase,
                    await _workContext.GetWorkingCurrencyAsync()
                );

                priceModel.OldPrice = null;
                priceModel.OldPriceValue = null;

                priceModel.PriceValue = finalPrice;

                if (priceModel.PriceValue == _b2BB2CFeaturesSettings.ProductQuotePrice)
                {
                    priceModel.Price = await _localizationService.GetResourceAsync("Products.ProductForQuote");
                }

                else
                {
                    priceModel.Price =
                        finalPrice > decimal.Zero
                            ? string.Format(
                                await _localizationService.GetResourceAsync("Products.PriceRangeFrom"),
                                await _priceFormatter.FormatPriceAsync(finalPrice)
                            )
                            : await _localizationService.GetResourceAsync("Products.CallForPrice");
                }
                priceModel.BasePricePAngV = await _priceFormatter.FormatBasePriceAsync(
                    product,
                    finalPriceBase
                );
                priceModel.BasePricePAngVValue = finalPriceBase;
            }
        }
        else
        {
            priceModel.OldPrice = null;
            priceModel.OldPriceValue = null;
            priceModel.Price = null;
            priceModel.PriceValue = null;
        }
    }

    public override async Task<IEnumerable<ProductOverviewModel>> PrepareProductOverviewModelsAsync(
        IEnumerable<Product> products,
        bool preparePriceModel = true,
        bool preparePictureModel = true,
        int? productThumbPictureSize = null,
        bool prepareSpecificationAttributes = false,
        bool forceRedirectionAfterAddingToCart = false
    )
    {
        ArgumentNullException.ThrowIfNull(products);

        var models = new List<ProductOverviewModel>();
        foreach (var product in products)
        {
            var model = new ProductOverviewModel
            {
                Id = product.Id,
                Name = await _localizationService.GetLocalizedAsync(product, x => x.Name),
                ShortDescription = await _localizationService.GetLocalizedAsync(
                    product,
                    x => x.ShortDescription
                ),
                FullDescription = await _localizationService.GetLocalizedAsync(
                    product,
                    x => x.FullDescription
                ),
                SeName = await _urlRecordService.GetSeNameAsync(product),
                Sku = product.Sku,
                ProductType = product.ProductType,
                MarkAsNew =
                    product.MarkAsNew
                    && (
                        !product.MarkAsNewStartDateTimeUtc.HasValue
                        || product.MarkAsNewStartDateTimeUtc.Value < DateTime.UtcNow
                    )
                    && (
                        !product.MarkAsNewEndDateTimeUtc.HasValue
                        || product.MarkAsNewEndDateTimeUtc.Value > DateTime.UtcNow
                    ),
            };

            //price
            if (preparePriceModel)
            {
                model.ProductPrice = await PrepareProductOverviewPriceModelAsync(
                    product,
                    forceRedirectionAfterAddingToCart
                );
            }

            //picture
            if (preparePictureModel)
            {
                model.PictureModels = await PrepareProductOverviewPicturesModelAsync(
                    product,
                    productThumbPictureSize
                );
            }

            //specs
            if (prepareSpecificationAttributes)
            {
                model.ProductSpecificationModel = await PrepareProductSpecificationModelAsync(
                    product
                );
            }

            //reviews
            model.ReviewOverviewModel = await PrepareProductReviewOverviewModelAsync(product);

            #region B2B

            var stockAvailability = await _productService.FormatStockMessageAsync(product, "");
            model.CustomProperties.Add(B2BB2CFeaturesDefaults.ProductOverviewModelStockAvailabilityKey, stockAvailability);

            var isProductOnSpecial = await _erpCustomerFunctionality.IsTheProductFromSpecialCategoryAsync(product);
            model.CustomProperties.Add(B2BB2CFeaturesDefaults.ProductIsOnSpecial, $"{isProductOnSpecial}");

            #endregion

            models.Add(model);
        }

        return models;
    }

    #endregion
}
