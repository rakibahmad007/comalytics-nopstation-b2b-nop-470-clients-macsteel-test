using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Nop.Core;
using Nop.Core.Caching;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Media;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Security;
using Nop.Core.Domain.Shipping;
using Nop.Core.Domain.Tax;
using Nop.Core.Domain.Vendors;
using Nop.Services.Attributes;
using Nop.Services.Catalog;
using Nop.Services.Common;
using Nop.Services.Customers;
using Nop.Services.Directory;
using Nop.Services.Discounts;
using Nop.Services.Helpers;
using Nop.Services.Localization;
using Nop.Services.Media;
using Nop.Services.Orders;
using Nop.Services.Payments;
using Nop.Services.Security;
using Nop.Services.Seo;
using Nop.Services.Shipping;
using Nop.Services.Stores;
using Nop.Services.Tax;
using Nop.Services.Vendors;
using Nop.Web.Factories;
using Nop.Web.Models.ShoppingCart;
using Nop.Web.Models.Media;
using NopStation.Plugin.B2B.B2BB2CFeatures.Services.ErpCustomerFunctionality;
using NopStation.Plugin.B2B.ERPIntegrationCore.Enums;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Infrastructure.Cache;
using static LinqToDB.Reflection.Methods.LinqToDB.Insert;

namespace NopStation.Plugin.B2B.B2BB2CFeatures.Factories;

public class OverridenShoppingCartModelFactory : ShoppingCartModelFactory
{
    #region Fields

    private readonly IErpCustomerFunctionalityService _erpCustomerFunctionalityService;
    private readonly B2BB2CFeaturesSettings _b2BB2CFeaturesSettings;

    #endregion

    #region Ctor

    public OverridenShoppingCartModelFactory(
        AddressSettings addressSettings,
        CaptchaSettings captchaSettings,
        CatalogSettings catalogSettings,
        CommonSettings commonSettings,
        CustomerSettings customerSettings,
        IAddressModelFactory addressModelFactory,
        IAddressService addressService,
        IAttributeParser<CheckoutAttribute, CheckoutAttributeValue> checkoutAttributeParser,
        IAttributeService<CheckoutAttribute, CheckoutAttributeValue> checkoutAttributeService,
        ICheckoutAttributeFormatter checkoutAttributeFormatter,
        ICountryService countryService,
        ICurrencyService currencyService,
        ICustomerService customerService,
        IDateTimeHelper dateTimeHelper,
        IDiscountService discountService,
        IDownloadService downloadService,
        IGenericAttributeService genericAttributeService,
        IGiftCardService giftCardService,
        IHttpContextAccessor httpContextAccessor,
        ILocalizationService localizationService,
        IOrderProcessingService orderProcessingService,
        IOrderTotalCalculationService orderTotalCalculationService,
        IPaymentPluginManager paymentPluginManager,
        IPaymentService paymentService,
        IPermissionService permissionService,
        IPictureService pictureService,
        IPriceFormatter priceFormatter,
        IProductAttributeFormatter productAttributeFormatter,
        IProductService productService,
        IShippingService shippingService,
        IShoppingCartService shoppingCartService,
        IShortTermCacheManager shortTermCacheManager,
        IStateProvinceService stateProvinceService,
        IStaticCacheManager staticCacheManager,
        IStoreContext storeContext,
        IStoreMappingService storeMappingService,
        ITaxService taxService,
        IUrlRecordService urlRecordService,
        IVendorService vendorService,
        IWebHelper webHelper,
        IWorkContext workContext,
        MediaSettings mediaSettings,
        OrderSettings orderSettings,
        RewardPointsSettings rewardPointsSettings,
        ShippingSettings shippingSettings,
        ShoppingCartSettings shoppingCartSettings,
        TaxSettings taxSettings,
        VendorSettings vendorSettings,
        IErpCustomerFunctionalityService erpCustomerFunctionalityService,
        B2BB2CFeaturesSettings b2BB2CFeaturesSettings) : base(
            addressSettings,
            captchaSettings,
            catalogSettings,
            commonSettings,
            customerSettings,
            addressModelFactory,
            addressService,
            checkoutAttributeParser,
            checkoutAttributeService,
            checkoutAttributeFormatter,
            countryService,
            currencyService,
            customerService,
            dateTimeHelper,
            discountService,
            downloadService,
            genericAttributeService,
            giftCardService,
            httpContextAccessor,
            localizationService,
            orderProcessingService,
            orderTotalCalculationService,
            paymentPluginManager,
            paymentService,
            permissionService,
            pictureService,
            priceFormatter,
            productAttributeFormatter,
            productService,
            shippingService,
            shoppingCartService,
            shortTermCacheManager,
            stateProvinceService,
            staticCacheManager,
            storeContext,
            storeMappingService,
            taxService,
            urlRecordService,
            vendorService,
            webHelper,
            workContext,
            mediaSettings,
            orderSettings,
            rewardPointsSettings,
            shippingSettings,
            shoppingCartSettings,
            taxSettings,
            vendorSettings
        )
    {
        _erpCustomerFunctionalityService = erpCustomerFunctionalityService;
        _b2BB2CFeaturesSettings = b2BB2CFeaturesSettings;
    }


    #endregion

    #region Utilities

    private async Task<Dictionary<int, PictureModel>> PrepareCustomCartItemPictureModelAsync(
        IList<ShoppingCartItem> cart,
        IDictionary<int, Product> productsById,
        int pictureSize,
        bool showDefaultPicture,
        IDictionary<int, string> productNamesById)
    {
        var workingLanguage = await _workContext.GetWorkingLanguageAsync();
        var isConnectionSecured = _webHelper.IsCurrentConnectionSecured();
        var store = await _storeContext.GetCurrentStoreAsync();

        var tasks = cart.Select(async sci =>
        {
            if (!productsById.TryGetValue(sci.ProductId, out var product))
                return (sci.Id, (PictureModel)null);

            productNamesById.TryGetValue(sci.ProductId, out var productName);

            var model = await _shortTermCacheManager.GetAsync(async () =>
            {
                var sciPicture = await _pictureService.GetProductPictureAsync(product, sci.AttributesXml);
                var imageUrl = (await _pictureService.GetPictureUrlAsync(sciPicture, pictureSize, showDefaultPicture)).Url;
                var fullSizeImageUrl = (await _pictureService.GetPictureUrlAsync(sciPicture)).Url;
                var title = string.Format(await _localizationService.GetResourceAsync("Media.Product.ImageLinkTitleFormat"), productName ?? product.Name);
                var alt = string.Format(await _localizationService.GetResourceAsync("Media.Product.ImageAlternateTextFormat"), productName ?? product.Name);

                return new PictureModel
                {
                    ImageUrl = imageUrl,
                    FullSizeImageUrl = fullSizeImageUrl,
                    Title = title,
                    AlternateText = alt
                };
            }, 
            NopModelCacheDefaults.CartPictureModelKey, 
            sci, 
            pictureSize, 
            true,
            workingLanguage,
            isConnectionSecured,
            store);

            return (sci.Id, model);
        });

        var results = await Task.WhenAll(tasks);

        return results
            .Where(x => x.Item2 != null)
            .ToDictionary(x => x.Id, x => x.Item2);
    }

    private async Task<Dictionary<int, bool>> GetProductsRequiringProductInCartMapAsync(
        IList<ShoppingCartItem> cart,
        IDictionary<int, Product> productsById)
    {
        // Build a map: productId -> isRequiredByAnyOtherProduct
        var result = new Dictionary<int, bool>();
        var cartProductIds = cart.Select(ci => ci.ProductId).ToArray();

        // For each product in the cart, check if any other product in the cart requires it
        foreach (var productId in cartProductIds)
        {
            result[productId] = productsById.Values.Any(cartProduct =>
                cartProduct.RequireOtherProducts &&
                _productService.ParseRequiredProductIds(cartProduct).Contains(productId)
            );
        }
        return result;
    }

    private async Task<IList<ShoppingCartModel.ShoppingCartItemModel>> PrepareCustomShoppingCartItemModelsAsync(
        IList<ShoppingCartItem> cart,
        IDictionary<int, Product> productsById,
        IDictionary<int, Vendor> vendorsById,
        IDictionary<int, string> productNamesById,
        IDictionary<int, string> productSeNamesById,
        IDictionary<int, string> attributeInfosById,
        IDictionary<int, PictureModel> picturesById,
        IDictionary<int, bool> isProductOnSpecialById
    )
    {
        var result = new List<ShoppingCartModel.ShoppingCartItemModel>();
        var currentCurrency = await _workContext.GetWorkingCurrencyAsync();
        var currentCustomer = await _workContext.GetCurrentCustomerAsync();
        var isAnyProductRequiredForThisProduct = await GetProductsRequiringProductInCartMapAsync(cart, productsById);

        foreach (var sci in cart)
        {
            var product = productsById[sci.ProductId];
            var vendorName = product.VendorId != 0 && 
                vendorsById.ContainsKey(product.VendorId) ? vendorsById[product.VendorId].Name : string.Empty;
            var productName = productNamesById[product.Id];
            var productSeName = productSeNamesById[product.Id];
            var attributeInfo = attributeInfosById[sci.Id];
            var picture = picturesById[sci.Id];
            var isOnSpecial = isProductOnSpecialById.ContainsKey(product.Id) && isProductOnSpecialById[product.Id];

            var cartItemModel = new ShoppingCartModel.ShoppingCartItemModel
            {
                Id = sci.Id,
                Sku = await _productService.FormatSkuAsync(product, sci.AttributesXml),
                VendorName = _vendorSettings.ShowVendorOnOrderDetailsPage ? vendorName : string.Empty,
                ProductId = sci.ProductId,
                ProductName = productName,
                ProductSeName = productSeName,
                Quantity = sci.Quantity,
                AttributeInfo = attributeInfo,
                AllowItemEditing = _shoppingCartSettings.AllowCartItemEditing &&
                                   product.ProductType == ProductType.SimpleProduct &&
                                   (!string.IsNullOrEmpty(attributeInfo) || product.IsGiftCard) &&
                                   product.VisibleIndividually,
                DisableRemoval = isAnyProductRequiredForThisProduct[sci.ProductId],
                Picture = picture
            };

            // Allowed quantities
            var allowedQuantities = _productService.ParseAllowedQuantities(product);
            foreach (var qty in allowedQuantities)
            {
            cartItemModel.AllowedQuantities.Add(new SelectListItem
                {
                    Text = $"{qty}",
                    Value = $"{qty}",
                    Selected = sci.Quantity == qty
                });
            }

            // Recurring info
            if (product.IsRecurring)
                cartItemModel.RecurringInfo = string.Format(await _localizationService.GetResourceAsync("ShoppingCart.RecurringPeriod"),
                    product.RecurringCycleLength, await _localizationService.GetLocalizedEnumAsync(product.RecurringCyclePeriod));

            // Rental info
            if (product.IsRental)
            {
                var rentalStartDate = sci.RentalStartDateUtc.HasValue
                    ? _productService.FormatRentalDate(product, sci.RentalStartDateUtc.Value)
                    : string.Empty;
                var rentalEndDate = sci.RentalEndDateUtc.HasValue
                    ? _productService.FormatRentalDate(product, sci.RentalEndDateUtc.Value)
                    : string.Empty;
                cartItemModel.RentalInfo =
                    string.Format(await _localizationService.GetResourceAsync("ShoppingCart.Rental.FormattedDate"),
                        rentalStartDate, rentalEndDate);
            }

            // Unit prices
            if (product.CallForPrice &&
            //also check whether the current user is impersonated
                (!_orderSettings.AllowAdminsToBuyCallForPriceProducts || _workContext.OriginalCustomerIfImpersonated == null))
            {
                cartItemModel.UnitPrice = await _localizationService.GetResourceAsync("Products.CallForPrice");
                cartItemModel.UnitPriceValue = 0;
            }
            else
            {
                var (shoppingCartUnitPriceWithDiscountBase, _) = await _taxService.GetProductPriceAsync(product, (await _shoppingCartService.GetUnitPriceAsync(sci, true)).unitPrice);
                var shoppingCartUnitPriceWithDiscount = await _currencyService.ConvertFromPrimaryStoreCurrencyAsync(shoppingCartUnitPriceWithDiscountBase, currentCurrency);
                cartItemModel.UnitPrice = await _priceFormatter.FormatPriceAsync(shoppingCartUnitPriceWithDiscount);
                cartItemModel.UnitPriceValue = shoppingCartUnitPriceWithDiscount;
            }
            // Subtotal, discount
            if (product.CallForPrice &&
            //also check whether the current user is impersonated
                (!_orderSettings.AllowAdminsToBuyCallForPriceProducts || _workContext.OriginalCustomerIfImpersonated == null))
            {
                cartItemModel.SubTotal = await _localizationService.GetResourceAsync("Products.CallForPrice");
                cartItemModel.SubTotalValue = 0;
            }
            else
            {
                var (subTotal, shoppingCartItemDiscountBase, _, maximumDiscountQty) = await _shoppingCartService.GetSubTotalAsync(sci, true);
                var (shoppingCartItemSubTotalWithDiscountBase, _) = await _taxService.GetProductPriceAsync(product, subTotal);
                var shoppingCartItemSubTotalWithDiscount = await _currencyService.ConvertFromPrimaryStoreCurrencyAsync(shoppingCartItemSubTotalWithDiscountBase, currentCurrency);
                cartItemModel.SubTotal = await _priceFormatter.FormatPriceAsync(shoppingCartItemSubTotalWithDiscount);
                cartItemModel.SubTotalValue = shoppingCartItemSubTotalWithDiscount;
                cartItemModel.MaximumDiscountedQty = maximumDiscountQty;

                // Display an applied discount amount
                if (shoppingCartItemDiscountBase > decimal.Zero)
                {
                    (shoppingCartItemDiscountBase, _) = await _taxService.GetProductPriceAsync(product, shoppingCartItemDiscountBase);
                    if (shoppingCartItemDiscountBase > decimal.Zero)
                    {
                        var shoppingCartItemDiscount = await _currencyService.ConvertFromPrimaryStoreCurrencyAsync(shoppingCartItemDiscountBase, currentCurrency);
                        cartItemModel.Discount = await _priceFormatter.FormatPriceAsync(shoppingCartItemDiscount);
                        cartItemModel.DiscountValue = shoppingCartItemDiscount;
                    }
                }
            }

            // Item warnings
            var itemWarnings = await _shoppingCartService.GetShoppingCartItemWarningsAsync(
                currentCustomer,
                sci.ShoppingCartType,
                product,
                sci.StoreId,
                sci.AttributesXml,
                sci.CustomerEnteredPrice,
                sci.RentalStartDateUtc,
                sci.RentalEndDateUtc,
                sci.Quantity,
                false,
                sci.Id);
            foreach (var warning in itemWarnings)
                cartItemModel.Warnings.Add(warning);

            // Custom B2B property: Special category flag
            cartItemModel.CustomProperties.Add(
                B2BB2CFeaturesDefaults.ProductIsOnSpecial,
                $"{isOnSpecial}"
            );

            result.Add(cartItemModel);
        }
        return result;
    }

    #endregion

    public override async Task<ShoppingCartModel> PrepareShoppingCartModelAsync(
       ShoppingCartModel model,
       IList<ShoppingCartItem> cart,
       bool isEditable = true,
       bool validateCheckoutAttributes = false,
       bool prepareAndDisplayOrderReviewData = false
   )
    {
        /*
         * Efficient Batch Preparation Refactor:
         * - All product, vendor, localization, SeName, attribute info, picture, and special category data
         *   are fetched in bulk and stored in dictionaries for fast lookup.
         * - The cart item model preparation uses these dictionaries to avoid N+1 DB calls.
         * - This greatly improves performance for large carts.
         */

        ArgumentNullException.ThrowIfNull(cart);
        ArgumentNullException.ThrowIfNull(model);

        //simple properties
        model.OnePageCheckoutEnabled = _orderSettings.OnePageCheckoutEnabled;

        if (!cart.Any())
            return model;

        model.IsEditable = isEditable;
        model.ShowProductImages = _shoppingCartSettings.ShowProductImagesOnShoppingCart;
        model.ShowSku = _catalogSettings.ShowSkuOnProductDetailsPage;
        model.ShowVendorName = _vendorSettings.ShowVendorOnOrderDetailsPage;
        var customer = await _workContext.GetCurrentCustomerAsync();
        var store = await _storeContext.GetCurrentStoreAsync();
        var checkoutAttributesXml = await _genericAttributeService.GetAttributeAsync<string>(
            customer,
            NopCustomerDefaults.CheckoutAttributes,
            store.Id
        );
        var minOrderSubtotalAmountOk =
            await _orderProcessingService.ValidateMinOrderSubtotalAmountAsync(cart);
        if (!minOrderSubtotalAmountOk)
        {
            var minOrderSubtotalAmount =
                await _currencyService.ConvertFromPrimaryStoreCurrencyAsync(
                    _orderSettings.MinOrderSubtotalAmount,
                    await _workContext.GetWorkingCurrencyAsync()
                );
            model.MinOrderSubtotalWarning = string.Format(
                await _localizationService.GetResourceAsync("Checkout.MinOrderSubtotalAmount"),
                await _priceFormatter.FormatPriceAsync(minOrderSubtotalAmount, true, false)
            );
        }

        // Step 1: Bulk fetch all products for the cart and build productsById dictionary
        var productIds = cart.Select(x => x.ProductId).Distinct().ToArray();
        var products = await _productService.GetProductsByIdsAsync(productIds);
        var productsById = products.ToDictionary(p => p.Id);

        // Step 2: Bulk fetch vendors for only the products in the cart and build vendorsById dictionary
        var vendors = await _vendorService.GetVendorsByProductIdsAsync(productIds);
        var vendorsById = vendors.ToDictionary(v => v.Id);

        // Step 3: Bulk fetch localized product names for all products in the cart and build productNamesById dictionary
        var languageId = (await _workContext.GetWorkingLanguageAsync()).Id;
        var productNamesById = new Dictionary<int, string>();
        foreach (var product in products)
        {
            productNamesById[product.Id] = await _localizationService.GetLocalizedAsync(product, x => x.Name, languageId);
        }

        // Step 4: Bulk fetch product SeNames (URLs) for all products in the cart and build productSeNamesById dictionary
        var productSeNamesById = new Dictionary<int, string>();
        foreach (var product in products)
        {
            productSeNamesById[product.Id] = await _urlRecordService.GetSeNameAsync(product, languageId);
        }

        // Step 5: Bulk fetch attribute info for all cart items and build attributeInfosById dictionary
        var attributeInfosById = new Dictionary<int, string>();
        foreach (var sci in cart)
        {
            attributeInfosById[sci.Id] = await _productAttributeFormatter.FormatAttributesAsync(
                productsById[sci.ProductId], 
                sci.AttributesXml,
                customer,
                store);
        }

        // Step 6: Bulk fetch pictures for all cart items and build picturesById dictionary
        var picturesById = new Dictionary<int, PictureModel>();
        /*var picturesById = await PrepareCustomCartItemPictureModelAsync(
            cart, 
            productsById,
            _mediaSettings.CartThumbPictureSize,
            true,
            productNamesById);*/
        foreach (var sci in cart)
        {
            var product = productsById[sci.ProductId];
            picturesById[sci.Id] = await PrepareCartItemPictureModelAsync(
                sci, _mediaSettings.CartThumbPictureSize, true, productNamesById[product.Id]);
        }

        // Step 7: Bulk check special category flags for all products and build isProductOnSpecialById dictionary
        var isProductOnSpecialById = await _erpCustomerFunctionalityService.AreTheProductsFromSpecialCategoryAsync(products);

        // Step 8: Use all prepared dictionaries in the cart item model preparation
        var cartItemModels = await PrepareCustomShoppingCartItemModelsAsync(
            cart,
            productsById,
            vendorsById,
            productNamesById,
            productSeNamesById,
            attributeInfosById,
            picturesById,
            isProductOnSpecialById
        );

        bool prodcutforQuote = false;
        var productForQuoteResource = await _localizationService.GetResourceAsync("Products.ProductForQuote");

        foreach (var cartItemModel in cartItemModels)
        {
            if (cartItemModel.UnitPrice == await _priceFormatter.FormatPriceAsync(_b2BB2CFeaturesSettings.ProductQuotePrice))
            {
                cartItemModel.SubTotal = productForQuoteResource;
                cartItemModel.UnitPrice = productForQuoteResource;
                prodcutforQuote = true;
            }
            model.Items.Add(cartItemModel);
        }

        model.TermsOfServiceOnShoppingCartPage = _orderSettings.TermsOfServiceOnShoppingCartPage;
        model.TermsOfServiceOnOrderConfirmPage = _orderSettings.TermsOfServiceOnOrderConfirmPage;
        model.TermsOfServicePopup = _commonSettings.PopupForTermsOfServiceLinks;
        model.DisplayTaxShippingInfo = _catalogSettings.DisplayTaxShippingInfoShoppingCart;

        //discount and gift card boxes
        model.DiscountBox.Display = _shoppingCartSettings.ShowDiscountBox;
        var discountCouponCodes = await _customerService.ParseAppliedDiscountCouponCodesAsync(
            customer
        );

        foreach (var couponCode in discountCouponCodes)
        {
            var discount = await (
                await _discountService.GetAllDiscountsAsync(couponCode: couponCode)
            ).FirstOrDefaultAwaitAsync(async d =>
                d.RequiresCouponCode
                && (
                    await _discountService.ValidateDiscountAsync(d, customer, discountCouponCodes)
                ).IsValid
            );

            if (discount != null)
            {
                model.DiscountBox.AppliedDiscountsWithCodes.Add(
                    new ShoppingCartModel.DiscountBoxModel.DiscountInfoModel
                    {
                        Id = discount.Id,
                        CouponCode = discount.CouponCode,
                    }
                );
            }
        }

        model.GiftCardBox.Display = _shoppingCartSettings.ShowGiftCardBox;

        //cart warnings
        var cartWarnings = await _shoppingCartService.GetShoppingCartWarningsAsync(
            cart,
            checkoutAttributesXml,
            validateCheckoutAttributes
        );
        foreach (var warning in cartWarnings)
            model.Warnings.Add(warning);

        //checkout attributes
        model.CheckoutAttributes = await PrepareCheckoutAttributeModelsAsync(cart);
        // The products variable is now available globally for the cart items loop

        //payment methods
        //all payment methods (do not filter by country here as it could be not specified yet)
        var paymentMethods = await (
            await _paymentPluginManager.LoadActivePluginsAsync(customer, store.Id)
        )
            .WhereAwait(async pm => !await pm.HidePaymentMethodAsync(cart))
            .ToListAsync();
        //payment methods displayed during checkout (not with "Button" type)
        var nonButtonPaymentMethods = paymentMethods
            .Where(pm => pm.PaymentMethodType != PaymentMethodType.Button)
            .ToList();
        //"button" payment methods(*displayed on the shopping cart page)
        var buttonPaymentMethods = paymentMethods
            .Where(pm => pm.PaymentMethodType == PaymentMethodType.Button)
            .ToList();
        foreach (var pm in buttonPaymentMethods)
        {
            if (
                await _shoppingCartService.ShoppingCartIsRecurringAsync(cart)
                && pm.RecurringPaymentType == RecurringPaymentType.NotSupported
            )
                continue;

            var viewComponent = pm.GetPublicViewComponent();
            model.ButtonPaymentMethodViewComponents.Add(viewComponent);
        }
        //hide "Checkout" button if we have only "Button" payment methods
        model.HideCheckoutButton = nonButtonPaymentMethods.Count == 0 && model.ButtonPaymentMethodViewComponents.Any();

        //order review data
        if (prepareAndDisplayOrderReviewData)
        {
            model.OrderReviewData = await PrepareOrderReviewDataModelAsync(cart);
        }
        if (prodcutforQuote)
            model.CustomProperties.Add("ProductForQuote", prodcutforQuote.ToString());

        return model;
    }

    public override async Task<WishlistModel> PrepareWishlistModelAsync(
        WishlistModel model,
        IList<ShoppingCartItem> cart,
        bool isEditable = true
    )
    {
        ArgumentNullException.ThrowIfNull(cart);

        ArgumentNullException.ThrowIfNull(model);

        model.EmailWishlistEnabled = _shoppingCartSettings.EmailWishlistEnabled;
        model.IsEditable = isEditable;
        model.DisplayAddToCart = await _permissionService.AuthorizeAsync(
            StandardPermissionProvider.EnableShoppingCart
        );
        model.DisplayTaxShippingInfo = _catalogSettings.DisplayTaxShippingInfoWishlist;

        if (!cart.Any())
            return model;

        //simple properties
        var customer = await _customerService.GetShoppingCartCustomerAsync(cart);

        model.CustomerGuid = customer.CustomerGuid;
        model.CustomerFullname = await _customerService.GetCustomerFullNameAsync(customer);
        model.ShowProductImages = _shoppingCartSettings.ShowProductImagesOnWishList;
        model.ShowSku = _catalogSettings.ShowSkuOnProductDetailsPage;

        //cart warnings
        var cartWarnings = await _shoppingCartService.GetShoppingCartWarningsAsync(
            cart,
            string.Empty,
            false
        );
        foreach (var warning in cartWarnings)
            model.Warnings.Add(warning);
        var products = await _productService.GetProductsByIdsAsync(
            cart.Select(x => x.ProductId).ToArray()
        );

        //cart items
        foreach (var sci in cart)
        {
            var cartItemModel = await PrepareWishlistItemModelAsync(sci);

            #region B2B

            var isProductOnSpecial =
                await _erpCustomerFunctionalityService.IsTheProductFromSpecialCategoryAsync(
                    products.FirstOrDefault(x => x.Id == sci.ProductId)
                );
            cartItemModel.CustomProperties.Add(
                B2BB2CFeaturesDefaults.ProductIsOnSpecial,
                isProductOnSpecial.ToString()
            );

            #endregion

            model.Items.Add(cartItemModel);
        }

        return model;
    }

    public override async Task<OrderTotalsModel> PrepareOrderTotalsModelAsync(
        IList<ShoppingCartItem> cart,
        bool isEditable
    )
    {
        var model = new OrderTotalsModel { IsEditable = isEditable };

        if (cart.Any())
        {
            var productForQuoteResource = await _localizationService.GetResourceAsync("Products.ProductForQuote");
            //subtotal
            var subTotalIncludingTax =
                await _workContext.GetTaxDisplayTypeAsync() == TaxDisplayType.IncludingTax
                && !_taxSettings.ForceTaxExclusionFromOrderSubtotal;
            var (orderSubTotalDiscountAmountBase, _, subTotalWithoutDiscountBase, _, _) =
                await _orderTotalCalculationService.GetShoppingCartSubTotalAsync(
                    cart,
                    subTotalIncludingTax
                );
            var subtotalBase = subTotalWithoutDiscountBase;
            var currentCurrency = await _workContext.GetWorkingCurrencyAsync();
            var subtotal = await _currencyService.ConvertFromPrimaryStoreCurrencyAsync(
                subtotalBase,
                currentCurrency
            );
            var currentLanguage = await _workContext.GetWorkingLanguageAsync();
            model.SubTotal = await _priceFormatter.FormatPriceAsync(
                subtotal,
                true,
                currentCurrency,
                currentLanguage.Id,
                subTotalIncludingTax
            );

            if (orderSubTotalDiscountAmountBase > decimal.Zero)
            {
                var orderSubTotalDiscountAmount =
                    await _currencyService.ConvertFromPrimaryStoreCurrencyAsync(
                        orderSubTotalDiscountAmountBase,
                        currentCurrency
                    );
                model.SubTotalDiscount = await _priceFormatter.FormatPriceAsync(
                    -orderSubTotalDiscountAmount,
                    true,
                    currentCurrency,
                    currentLanguage.Id,
                    subTotalIncludingTax
                );
            }

            //shipping info
            model.RequiresShipping = await _shoppingCartService.ShoppingCartRequiresShippingAsync(
                cart
            );
            var customer = await _workContext.GetCurrentCustomerAsync();
            var store = await _storeContext.GetCurrentStoreAsync();
            if (model.RequiresShipping)
            {
                var shoppingCartShippingBase =
                    await _orderTotalCalculationService.GetShoppingCartShippingTotalAsync(cart);
                if (shoppingCartShippingBase.HasValue)
                {
                    var shoppingCartShipping =
                        await _currencyService.ConvertFromPrimaryStoreCurrencyAsync(
                            shoppingCartShippingBase.Value,
                            currentCurrency
                        );
                    model.Shipping = await _priceFormatter.FormatShippingPriceAsync(
                        shoppingCartShipping,
                        true
                    );

                    //selected shipping method
                    var shippingOption =
                        await _genericAttributeService.GetAttributeAsync<ShippingOption>(
                            customer,
                            NopCustomerDefaults.SelectedShippingOptionAttribute,
                            store.Id
                        );
                    if (shippingOption != null)
                        model.SelectedShippingMethod = shippingOption.Name;
                }
            }
            else
            {
                model.HideShippingTotal = _shippingSettings.HideShippingTotal;
            }

            //payment method fee
            var paymentMethodSystemName = await _genericAttributeService.GetAttributeAsync<string>(
                customer,
                NopCustomerDefaults.SelectedPaymentMethodAttribute,
                store.Id
            );
            var paymentMethodAdditionalFee = await _paymentService.GetAdditionalHandlingFeeAsync(
                cart,
                paymentMethodSystemName
            );
            var (paymentMethodAdditionalFeeWithTaxBase, _) =
                await _taxService.GetPaymentMethodAdditionalFeeAsync(
                    paymentMethodAdditionalFee,
                    customer
                );
            if (paymentMethodAdditionalFeeWithTaxBase > decimal.Zero)
            {
                var paymentMethodAdditionalFeeWithTax =
                    await _currencyService.ConvertFromPrimaryStoreCurrencyAsync(
                        paymentMethodAdditionalFeeWithTaxBase,
                        currentCurrency
                    );
                model.PaymentMethodAdditionalFee =
                    await _priceFormatter.FormatPaymentMethodAdditionalFeeAsync(
                        paymentMethodAdditionalFeeWithTax,
                        true
                    );
            }

            //tax
            bool displayTax;
            bool displayTaxRates;
            if (
                _taxSettings.HideTaxInOrderSummary
                && await _workContext.GetTaxDisplayTypeAsync() == TaxDisplayType.IncludingTax
            )
            {
                displayTax = false;
                displayTaxRates = false;
            }
            else
            {
                var (shoppingCartTaxBase, taxRates) =
                    await _orderTotalCalculationService.GetTaxTotalAsync(cart);
                var shoppingCartTax = await _currencyService.ConvertFromPrimaryStoreCurrencyAsync(
                    shoppingCartTaxBase,
                    currentCurrency
                );

                if (shoppingCartTaxBase == 0 && _taxSettings.HideZeroTax)
                {
                    displayTax = false;
                    displayTaxRates = false;
                }
                else
                {
                    displayTaxRates = _taxSettings.DisplayTaxRates && taxRates.Any();
                    displayTax = !displayTaxRates;

                    model.Tax = await _priceFormatter.FormatPriceAsync(
                        shoppingCartTax,
                        true,
                        false
                    );
                    foreach (var tr in taxRates)
                    {
                        model.TaxRates.Add(
                            new OrderTotalsModel.TaxRate
                            {
                                Rate = _priceFormatter.FormatTaxRate(tr.Key),
                                Value = await _priceFormatter.FormatPriceAsync(
                                    await _currencyService.ConvertFromPrimaryStoreCurrencyAsync(
                                        tr.Value,
                                        currentCurrency
                                    ),
                                    true,
                                    false
                                ),
                            }
                        );
                    }
                }
            }

            model.DisplayTaxRates = displayTaxRates;
            model.DisplayTax = displayTax;

            var productforQuote = false;
            foreach (var item in cart)
            {
                var price = await _shoppingCartService.GetUnitPriceAsync(item, true);

                if (price.unitPrice == _b2BB2CFeaturesSettings.ProductQuotePrice)
                {
                    productforQuote = true;
                }
            }

            //total
            var (
                shoppingCartTotalBase,
                orderTotalDiscountAmountBase,
                _,
                appliedGiftCards,
                redeemedRewardPoints,
                redeemedRewardPointsAmount
            ) = await _orderTotalCalculationService.GetShoppingCartTotalAsync(cart);
            if (shoppingCartTotalBase.HasValue)
            {
                var shoppingCartTotal = await _currencyService.ConvertFromPrimaryStoreCurrencyAsync(
                    shoppingCartTotalBase.Value,
                    currentCurrency
                );
                var erpNopUser =
                    await _erpCustomerFunctionalityService.GetActiveErpNopUserByCustomerAsync(
                        customer
                    );
                if (erpNopUser != null && erpNopUser.ErpUserType == ErpUserType.B2CUser)
                {
                    var targetTotal = Math.Floor(shoppingCartTotal * 10) / 10;
                    var cashRounding = shoppingCartTotal - targetTotal;
                    model.CustomProperties.Add("CashRounding", cashRounding.ToString());
                    if (productforQuote)
                        model.CustomProperties.Add("ProductForQuote", $"{productforQuote}");
                    model.SubTotal = await _priceFormatter.FormatPriceAsync(subtotal, true, false);
                    shoppingCartTotal = targetTotal;
                    model.OrderTotal = await _priceFormatter.FormatPriceAsync(
                        shoppingCartTotal,
                        true,
                        false
                    );
                }
                else
                {
                    model.OrderTotal = await _priceFormatter.FormatPriceAsync(
                        shoppingCartTotal,
                        true,
                        false
                    );
                }
            }

            //discount
            if (orderTotalDiscountAmountBase > decimal.Zero)
            {
                var orderTotalDiscountAmount =
                    await _currencyService.ConvertFromPrimaryStoreCurrencyAsync(
                        orderTotalDiscountAmountBase,
                        currentCurrency
                    );
                model.OrderTotalDiscount = await _priceFormatter.FormatPriceAsync(
                    -orderTotalDiscountAmount,
                    true,
                    false
                );
            }

            //gift cards
            if (appliedGiftCards != null && appliedGiftCards.Any())
            {
                foreach (var appliedGiftCard in appliedGiftCards)
                {
                    var gcModel = new OrderTotalsModel.GiftCard
                    {
                        Id = appliedGiftCard.GiftCard.Id,
                        CouponCode = appliedGiftCard.GiftCard.GiftCardCouponCode,
                    };
                    var amountCanBeUsed =
                        await _currencyService.ConvertFromPrimaryStoreCurrencyAsync(
                            appliedGiftCard.AmountCanBeUsed,
                            currentCurrency
                        );
                    gcModel.Amount = await _priceFormatter.FormatPriceAsync(
                        -amountCanBeUsed,
                        true,
                        false
                    );

                    var remainingAmountBase =
                        await _giftCardService.GetGiftCardRemainingAmountAsync(
                            appliedGiftCard.GiftCard
                        ) - appliedGiftCard.AmountCanBeUsed;
                    var remainingAmount =
                        await _currencyService.ConvertFromPrimaryStoreCurrencyAsync(
                            remainingAmountBase,
                            currentCurrency
                        );
                    gcModel.Remaining = await _priceFormatter.FormatPriceAsync(
                        remainingAmount,
                        true,
                        false
                    );

                    model.GiftCards.Add(gcModel);
                }
            }

            //reward points to be spent (redeemed)
            if (redeemedRewardPointsAmount > decimal.Zero)
            {
                var redeemedRewardPointsAmountInCustomerCurrency =
                    await _currencyService.ConvertFromPrimaryStoreCurrencyAsync(
                        redeemedRewardPointsAmount,
                        currentCurrency
                    );
                model.RedeemedRewardPoints = redeemedRewardPoints;
                model.RedeemedRewardPointsAmount = await _priceFormatter.FormatPriceAsync(
                    -redeemedRewardPointsAmountInCustomerCurrency,
                    true,
                    false
                );
            }

            //reward points to be earned
            if (
                _rewardPointsSettings.Enabled
                && _rewardPointsSettings.DisplayHowMuchWillBeEarned
                && shoppingCartTotalBase.HasValue
            )
            {
                //get shipping total
                var shippingBaseInclTax = !model.RequiresShipping
                    ? 0
                    : (
                        await _orderTotalCalculationService.GetShoppingCartShippingTotalAsync(
                            cart,
                            true
                        )
                    ).shippingTotal ?? 0;

                //get total for reward points
                var totalForRewardPoints =
                    _orderTotalCalculationService.CalculateApplicableOrderTotalForRewardPoints(
                        shippingBaseInclTax,
                        shoppingCartTotalBase.Value
                    );
                if (totalForRewardPoints > decimal.Zero)
                    model.WillEarnRewardPoints =
                        await _orderTotalCalculationService.CalculateRewardPointsAsync(
                            customer,
                            totalForRewardPoints
                        );
            }
            if (productforQuote)
            {
                model.SubTotal = productForQuoteResource;
                model.Shipping = productForQuoteResource;
                model.SubTotalDiscount = productForQuoteResource;
                model.Tax = productForQuoteResource;
                model.OrderTotalDiscount = productForQuoteResource;
                model.OrderTotal = productForQuoteResource;
            }
        }

        return model;
    }

    protected override async Task<WishlistModel.ShoppingCartItemModel> PrepareWishlistItemModelAsync(ShoppingCartItem sci)
    {
        ArgumentNullException.ThrowIfNull(sci);

        var product = await _productService.GetProductByIdAsync(sci.ProductId);

        var cartItemModel = new WishlistModel.ShoppingCartItemModel
        {
            Id = sci.Id,
            Sku = await _productService.FormatSkuAsync(product, sci.AttributesXml),
            ProductId = product.Id,
            ProductName = await _localizationService.GetLocalizedAsync(product, x => x.Name),
            ProductSeName = await _urlRecordService.GetSeNameAsync(product),
            Quantity = sci.Quantity,
            AttributeInfo = await _productAttributeFormatter.FormatAttributesAsync(product, sci.AttributesXml),
        };

        //allow editing?
        //1. setting enabled?
        //2. simple product?
        //3. has attribute or gift card?
        //4. visible individually?
        cartItemModel.AllowItemEditing = _shoppingCartSettings.AllowCartItemEditing &&
                                         product.ProductType == ProductType.SimpleProduct &&
                                         (!string.IsNullOrEmpty(cartItemModel.AttributeInfo) ||
                                          product.IsGiftCard) &&
                                         product.VisibleIndividually;

        //allowed quantities
        var allowedQuantities = _productService.ParseAllowedQuantities(product);
        foreach (var qty in allowedQuantities)
        {
            cartItemModel.AllowedQuantities.Add(new SelectListItem
            {
                Text = qty.ToString(),
                Value = qty.ToString(),
                Selected = sci.Quantity == qty
            });
        }

        //recurring info
        if (product.IsRecurring)
            cartItemModel.RecurringInfo = string.Format(await _localizationService.GetResourceAsync("ShoppingCart.RecurringPeriod"),
                product.RecurringCycleLength, await _localizationService.GetLocalizedEnumAsync(product.RecurringCyclePeriod));

        //rental info
        if (product.IsRental)
        {
            var rentalStartDate = sci.RentalStartDateUtc.HasValue
                ? _productService.FormatRentalDate(product, sci.RentalStartDateUtc.Value)
                : string.Empty;
            var rentalEndDate = sci.RentalEndDateUtc.HasValue
                ? _productService.FormatRentalDate(product, sci.RentalEndDateUtc.Value)
                : string.Empty;
            cartItemModel.RentalInfo =
                string.Format(await _localizationService.GetResourceAsync("ShoppingCart.Rental.FormattedDate"),
                    rentalStartDate, rentalEndDate);
        }

        //unit prices
        var currentCurrency = await _workContext.GetWorkingCurrencyAsync();
        if (product.CallForPrice &&
            //also check whether the current user is impersonated
            (!_orderSettings.AllowAdminsToBuyCallForPriceProducts || _workContext.OriginalCustomerIfImpersonated == null))
        {
            cartItemModel.UnitPrice = await _localizationService.GetResourceAsync("Products.CallForPrice");
            cartItemModel.UnitPriceValue = 0;
        }
        else
        {
            var (shoppingCartUnitPriceWithDiscountBase, _) = await _taxService.GetProductPriceAsync(product, (await _shoppingCartService.GetUnitPriceAsync(sci, true)).unitPrice);
            var shoppingCartUnitPriceWithDiscount = await _currencyService.ConvertFromPrimaryStoreCurrencyAsync(shoppingCartUnitPriceWithDiscountBase, currentCurrency);

            if (shoppingCartUnitPriceWithDiscount == _b2BB2CFeaturesSettings.ProductQuotePrice)
            {
                cartItemModel.UnitPrice = await _localizationService.GetResourceAsync("Products.ProductForQuote");
            }
            else
            {
                cartItemModel.UnitPrice = await _priceFormatter.FormatPriceAsync(shoppingCartUnitPriceWithDiscount);
            }
            cartItemModel.UnitPriceValue = shoppingCartUnitPriceWithDiscount;
        }
        //subtotal, discount
        if (product.CallForPrice &&
            //also check whether the current user is impersonated
            (!_orderSettings.AllowAdminsToBuyCallForPriceProducts || _workContext.OriginalCustomerIfImpersonated == null))
        {
            cartItemModel.SubTotal = await _localizationService.GetResourceAsync("Products.CallForPrice");
            cartItemModel.SubTotalValue = 0;
        }
        else
        {
            //sub total
            var (subTotal, shoppingCartItemDiscountBase, _, maximumDiscountQty) = await _shoppingCartService.GetSubTotalAsync(sci, true);
            var (shoppingCartItemSubTotalWithDiscountBase, _) = await _taxService.GetProductPriceAsync(product, subTotal);
            var shoppingCartItemSubTotalWithDiscount = await _currencyService.ConvertFromPrimaryStoreCurrencyAsync(shoppingCartItemSubTotalWithDiscountBase, currentCurrency);
            if (shoppingCartItemSubTotalWithDiscount == _b2BB2CFeaturesSettings.ProductQuotePrice)
            {
                cartItemModel.UnitPrice = await _localizationService.GetResourceAsync("Products.ProductForQuote");
                cartItemModel.SubTotal = await _localizationService.GetResourceAsync("Products.ProductForQuote");
            }
            else
                cartItemModel.SubTotal = await _priceFormatter.FormatPriceAsync(shoppingCartItemSubTotalWithDiscount);
            cartItemModel.SubTotalValue = shoppingCartItemSubTotalWithDiscount;
            cartItemModel.MaximumDiscountedQty = maximumDiscountQty;

            //display an applied discount amount
            if (shoppingCartItemDiscountBase > decimal.Zero)
            {
                (shoppingCartItemDiscountBase, _) = await _taxService.GetProductPriceAsync(product, shoppingCartItemDiscountBase);
                if (shoppingCartItemDiscountBase > decimal.Zero)
                {
                    var shoppingCartItemDiscount = await _currencyService.ConvertFromPrimaryStoreCurrencyAsync(shoppingCartItemDiscountBase, currentCurrency);
                    cartItemModel.Discount = await _priceFormatter.FormatPriceAsync(shoppingCartItemDiscount);
                    cartItemModel.DiscountValue = shoppingCartItemDiscount;
                }
            }
        }

        //picture
        if (_shoppingCartSettings.ShowProductImagesOnWishList)
        {
            cartItemModel.Picture = await PrepareCartItemPictureModelAsync(sci,
                _mediaSettings.CartThumbPictureSize, true, cartItemModel.ProductName);
        }

        //item warnings
        var itemWarnings = await _shoppingCartService.GetShoppingCartItemWarningsAsync(
            await _workContext.GetCurrentCustomerAsync(),
            sci.ShoppingCartType,
            product,
            sci.StoreId,
            sci.AttributesXml,
            sci.CustomerEnteredPrice,
            sci.RentalStartDateUtc,
            sci.RentalEndDateUtc,
            sci.Quantity,
            false,
            sci.Id);
        foreach (var warning in itemWarnings)
            cartItemModel.Warnings.Add(warning);

        return cartItemModel;
    }

    public override async Task<MiniShoppingCartModel> PrepareMiniShoppingCartModelAsync()
    {
        var customer = await _workContext.GetCurrentCustomerAsync();
        var model = new MiniShoppingCartModel
        {
            ShowProductImages = _shoppingCartSettings.ShowProductImagesInMiniShoppingCart,
            //let's always display it
            DisplayShoppingCartButton = true,
            CurrentCustomerIsGuest = await _customerService.IsGuestAsync(customer),
            AnonymousCheckoutAllowed = _orderSettings.AnonymousCheckoutAllowed,
        };

        //performance optimization (use "HasShoppingCartItems" property)
        if (customer.HasShoppingCartItems)
        {
            var store = await _storeContext.GetCurrentStoreAsync();
            var cart = await _shoppingCartService.GetShoppingCartAsync(customer, ShoppingCartType.ShoppingCart, store.Id);

            if (cart.Any())
            {
                model.TotalProducts = cart.Sum(item => item.Quantity);

                //subtotal
                var subTotalIncludingTax = await _workContext.GetTaxDisplayTypeAsync() == TaxDisplayType.IncludingTax && !_taxSettings.ForceTaxExclusionFromOrderSubtotal;




                var (_, _, subTotalWithoutDiscountBase, _, _) = await _orderTotalCalculationService.GetShoppingCartSubTotalAsync(cart, subTotalIncludingTax);
                var subtotalBase = subTotalWithoutDiscountBase;
                var currentCurrency = await _workContext.GetWorkingCurrencyAsync();
                var subtotal = await _currencyService.ConvertFromPrimaryStoreCurrencyAsync(subtotalBase, currentCurrency);
                model.SubTotal = await _priceFormatter.FormatPriceAsync(subtotal, false, currentCurrency, (await _workContext.GetWorkingLanguageAsync()).Id, subTotalIncludingTax);
                model.SubTotalValue = subtotal;

                var requiresShipping = await _shoppingCartService.ShoppingCartRequiresShippingAsync(cart);
                //a customer should visit the shopping cart page (hide checkout button) before going to checkout if:
                //1. "terms of service" are enabled
                //2. min order sub-total is OK
                //3. we have at least one checkout attribute
                var checkoutAttributesExist = (await _checkoutAttributeService
                        .GetAllAttributesAsync(_staticCacheManager, _storeMappingService, store.Id, !requiresShipping))
                    .Any();

                var minOrderSubtotalAmountOk = await _orderProcessingService.ValidateMinOrderSubtotalAmountAsync(cart);

                var cartProductIds = cart.Select(ci => ci.ProductId).ToArray();

                var downloadableProductsRequireRegistration =
                    _customerSettings.RequireRegistrationForDownloadableProducts && await _productService.HasAnyDownloadableProductAsync(cartProductIds);

                model.DisplayCheckoutButton = !_orderSettings.TermsOfServiceOnShoppingCartPage &&
                                              minOrderSubtotalAmountOk &&
                                              !checkoutAttributesExist &&
                                              !(downloadableProductsRequireRegistration
                                                && await _customerService.IsGuestAsync(customer));

                //products. sort descending (recently added products)
                foreach (var sci in cart
                             .OrderByDescending(x => x.Id)
                             .Take(_shoppingCartSettings.MiniShoppingCartProductNumber)
                             .ToList())
                {
                    var product = await _productService.GetProductByIdAsync(sci.ProductId);

                    var cartItemModel = new MiniShoppingCartModel.ShoppingCartItemModel
                    {
                        Id = sci.Id,
                        ProductId = sci.ProductId,
                        ProductName = await _localizationService.GetLocalizedAsync(product, x => x.Name),
                        ProductSeName = await _urlRecordService.GetSeNameAsync(product),
                        Quantity = sci.Quantity,
                        AttributeInfo = await _productAttributeFormatter.FormatAttributesAsync(product, sci.AttributesXml)
                    };

                    //unit prices
                    if (product.CallForPrice &&
                        //also check whether the current user is impersonated
                        (!_orderSettings.AllowAdminsToBuyCallForPriceProducts || _workContext.OriginalCustomerIfImpersonated == null))
                    {
                        cartItemModel.UnitPrice = await _localizationService.GetResourceAsync("Products.CallForPrice");
                        cartItemModel.UnitPriceValue = 0;
                    }
                    else
                    {
                        var (shoppingCartUnitPriceWithDiscountBase, _) = await _taxService.GetProductPriceAsync(product, (await _shoppingCartService.GetUnitPriceAsync(sci, true)).unitPrice);
                        var shoppingCartUnitPriceWithDiscount = await _currencyService.ConvertFromPrimaryStoreCurrencyAsync(shoppingCartUnitPriceWithDiscountBase, currentCurrency);
                        if (shoppingCartUnitPriceWithDiscountBase == _b2BB2CFeaturesSettings.ProductQuotePrice)
                        {
                            cartItemModel.UnitPrice = await _localizationService.GetResourceAsync("Products.ProductForQuote");
                            model.SubTotal = await _localizationService.GetResourceAsync("Products.ProductForQuote");
                        }
                        else
                            cartItemModel.UnitPrice = await _priceFormatter.FormatPriceAsync(shoppingCartUnitPriceWithDiscount);
                        cartItemModel.UnitPriceValue = shoppingCartUnitPriceWithDiscount;
                    }

                    //picture
                    if (_shoppingCartSettings.ShowProductImagesInMiniShoppingCart)
                    {
                        cartItemModel.Picture = await PrepareCartItemPictureModelAsync(sci,
                            _mediaSettings.MiniCartThumbPictureSize, true, cartItemModel.ProductName);
                    }

                    model.Items.Add(cartItemModel);
                }
            }
        }

        return model;
    }

}
