using System;
using System.Linq;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Core.Caching;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Directory;
using Nop.Core.Domain.Media;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Shipping;
using Nop.Core.Domain.Tax;
using Nop.Core.Domain.Vendors;
using Nop.Services.Catalog;
using Nop.Services.Common;
using Nop.Services.Customers;
using Nop.Services.Directory;
using Nop.Services.Helpers;
using Nop.Services.Localization;
using Nop.Services.Media;
using Nop.Services.Orders;
using Nop.Services.Payments;
using Nop.Services.Seo;
using Nop.Services.Shipping;
using Nop.Services.Vendors;
using Nop.Web.Factories;
using Nop.Web.Infrastructure.Cache;
using Nop.Web.Models.Common;
using Nop.Web.Models.Media;
using Nop.Web.Models.Order;
using NopStation.Plugin.B2B.B2BB2CFeatures.Services.ErpCustomerFunctionality;
using NopStation.Plugin.B2B.ERPIntegrationCore.Services;

namespace NopStation.Plugin.B2B.B2BB2CFeatures.Factories;

public class OverridenOrderModelFactory : OrderModelFactory
{
    #region Fields

    protected readonly AddressSettings _addressSettings;
    protected readonly CatalogSettings _catalogSettings;
    protected readonly IAddressModelFactory _addressModelFactory;
    protected readonly IAddressService _addressService;
    protected readonly ICountryService _countryService;
    protected readonly ICurrencyService _currencyService;
    protected readonly ICustomerService _customerService;
    protected readonly IDateTimeHelper _dateTimeHelper;
    protected readonly IGiftCardService _giftCardService;
    protected readonly ILocalizationService _localizationService;
    protected readonly IOrderProcessingService _orderProcessingService;
    protected readonly IOrderService _orderService;
    protected readonly IOrderTotalCalculationService _orderTotalCalculationService;
    protected readonly IPaymentPluginManager _paymentPluginManager;
    protected readonly IPaymentService _paymentService;
    protected readonly IPictureService _pictureService;
    protected readonly IPriceFormatter _priceFormatter;
    protected readonly IProductService _productService;
    protected readonly IRewardPointService _rewardPointService;
    protected readonly IShipmentService _shipmentService;
    protected readonly IShortTermCacheManager _shortTermCacheManager;
    protected readonly IStateProvinceService _stateProvinceService;
    protected readonly IStaticCacheManager _staticCacheManager;
    protected readonly IStoreContext _storeContext;
    protected readonly IUrlRecordService _urlRecordService;
    protected readonly IVendorService _vendorService;
    protected readonly IWebHelper _webHelper;
    protected readonly IWorkContext _workContext;
    protected readonly MediaSettings _mediaSettings;
    protected readonly OrderSettings _orderSettings;
    protected readonly PdfSettings _pdfSettings;
    protected readonly RewardPointsSettings _rewardPointsSettings;
    protected readonly ShippingSettings _shippingSettings;
    protected readonly TaxSettings _taxSettings;
    protected readonly VendorSettings _vendorSettings;
    protected readonly IErpCustomerFunctionalityService _erpCustomerFunctionalityService;
    private readonly IErpProductService _erpProductService;
    private readonly B2BB2CFeaturesSettings _b2BB2CFeaturesSettings;

    #endregion

    #region Ctor

    public OverridenOrderModelFactory(
        AddressSettings addressSettings,
        CatalogSettings catalogSettings,
        IAddressModelFactory addressModelFactory,
        IAddressService addressService,
        ICountryService countryService,
        ICurrencyService currencyService,
        ICustomerService customerService,
        IDateTimeHelper dateTimeHelper,
        IGiftCardService giftCardService,
        ILocalizationService localizationService,
        IOrderProcessingService orderProcessingService,
        IOrderService orderService,
        IOrderTotalCalculationService orderTotalCalculationService,
        IPaymentPluginManager paymentPluginManager,
        IPaymentService paymentService,
        IPictureService pictureService,
        IPriceFormatter priceFormatter,
        IProductService productService,
        IRewardPointService rewardPointService,
        IShipmentService shipmentService,
        IShortTermCacheManager shortTermCacheManager,
        IStateProvinceService stateProvinceService,
        IStaticCacheManager staticCacheManager,
        IStoreContext storeContext,
        IUrlRecordService urlRecordService,
        IVendorService vendorService,
        IWebHelper webHelper,
        IWorkContext workContext,
        MediaSettings mediaSettings,
        OrderSettings orderSettings,
        PdfSettings pdfSettings,
        RewardPointsSettings rewardPointsSettings,
        ShippingSettings shippingSettings,
        TaxSettings taxSettings,
        VendorSettings vendorSettings,
        IErpCustomerFunctionalityService erpCustomerFunctionalityService,
        IErpProductService erpProductService
,
        B2BB2CFeaturesSettings b2BB2CFeaturesSettings)
        : base(
            addressSettings,
            catalogSettings,
            addressModelFactory,
            addressService,
            countryService,
            currencyService,
            customerService,
            dateTimeHelper,
            giftCardService,
            localizationService,
            orderProcessingService,
            orderService,
            orderTotalCalculationService,
            paymentPluginManager,
            paymentService,
            pictureService,
            priceFormatter,
            productService,
            rewardPointService,
            shipmentService,
            shortTermCacheManager,
            stateProvinceService,
            staticCacheManager,
            storeContext,
            urlRecordService,
            vendorService,
            webHelper,
            workContext,
            mediaSettings,
            orderSettings,
            pdfSettings,
            rewardPointsSettings,
            shippingSettings,
            taxSettings,
            vendorSettings
        )
    {
        _addressSettings = addressSettings;
        _catalogSettings = catalogSettings;
        _addressModelFactory = addressModelFactory;
        _addressService = addressService;
        _countryService = countryService;
        _currencyService = currencyService;
        _customerService = customerService;
        _dateTimeHelper = dateTimeHelper;
        _giftCardService = giftCardService;
        _localizationService = localizationService;
        _orderProcessingService = orderProcessingService;
        _orderService = orderService;
        _orderTotalCalculationService = orderTotalCalculationService;
        _paymentPluginManager = paymentPluginManager;
        _paymentService = paymentService;
        _pictureService = pictureService;
        _priceFormatter = priceFormatter;
        _productService = productService;
        _rewardPointService = rewardPointService;
        _shipmentService = shipmentService;
        _shortTermCacheManager = shortTermCacheManager;
        _stateProvinceService = stateProvinceService;
        _staticCacheManager = staticCacheManager;
        _storeContext = storeContext;
        _urlRecordService = urlRecordService;
        _vendorService = vendorService;
        _webHelper = webHelper;
        _workContext = workContext;
        _mediaSettings = mediaSettings;
        _orderSettings = orderSettings;
        _pdfSettings = pdfSettings;
        _rewardPointsSettings = rewardPointsSettings;
        _shippingSettings = shippingSettings;
        _taxSettings = taxSettings;
        _vendorSettings = vendorSettings;
        _erpCustomerFunctionalityService = erpCustomerFunctionalityService;
        _erpProductService = erpProductService;
        _b2BB2CFeaturesSettings = b2BB2CFeaturesSettings;
    }

    #endregion

    #region Utils

    /// <summary>
    /// Prepare the order item picture model
    /// </summary>
    /// <param name="orderItem">Order item</param>
    /// <param name="pictureSize">Picture size</param>
    /// <param name="showDefaultPicture">Whether to show the default picture</param>
    /// <param name="productName">Product name</param>
    /// <returns>
    /// A task that represents the asynchronous operation
    /// The task result contains the picture model
    /// </returns>
    protected override async Task<PictureModel> PrepareOrderItemPictureModelAsync(OrderItem orderItem, int pictureSize, bool showDefaultPicture, string productName)
    {
        var language = await _workContext.GetWorkingLanguageAsync();
        var store = await _storeContext.GetCurrentStoreAsync();

        var model = await _shortTermCacheManager.GetAsync(async () =>
        {
            var product = await _erpProductService.GetProductByIdAsync(orderItem.ProductId, false);

            //order item picture
            var orderItemPicture = await _pictureService.GetProductPictureAsync(product, orderItem.AttributesXml);

            return new PictureModel
            {
                ImageUrl = (await _pictureService.GetPictureUrlAsync(orderItemPicture, pictureSize, showDefaultPicture)).Url,
                Title = string.Format(await _localizationService.GetResourceAsync("Media.Product.ImageLinkTitleFormat"), productName),
                AlternateText = string.Format(await _localizationService.GetResourceAsync("Media.Product.ImageAlternateTextFormat"), productName),
            };
        }, NopModelCacheDefaults.OrderPictureModelKey, orderItem, pictureSize, showDefaultPicture, language, _webHelper.IsCurrentConnectionSecured(), store);

        return model;
    }

    #endregion

    #region Methods

    public override async Task<OrderDetailsModel> PrepareOrderDetailsModelAsync(Order order)
    {
        ArgumentNullException.ThrowIfNull(order);
        var model = new OrderDetailsModel
        {
            Id = order.Id,
            CreatedOn = await _dateTimeHelper.ConvertToUserTimeAsync(
                order.CreatedOnUtc,
                DateTimeKind.Utc
            ),
            OrderStatus = await _localizationService.GetLocalizedEnumAsync(order.OrderStatus),
            IsReOrderAllowed = _orderSettings.IsReOrderAllowed,
            IsReturnRequestAllowed = await _orderProcessingService.IsReturnRequestAllowedAsync(
                order
            ),
            PdfInvoiceDisabled =
                _pdfSettings.DisablePdfInvoicesForPendingOrders
                && order.OrderStatus == OrderStatus.Pending,
            CustomOrderNumber = order.CustomOrderNumber,

            //shipping info
            ShippingStatus = await _localizationService.GetLocalizedEnumAsync(order.ShippingStatus),
        };

        var languageId = (await _workContext.GetWorkingLanguageAsync()).Id;
        var productForQuoteResource = await _localizationService.GetResourceAsync("Products.ProductForQuote");

        if (order.ShippingStatus != ShippingStatus.ShippingNotRequired)
        {
            model.IsShippable = true;
            model.PickupInStore = order.PickupInStore;
            if (!order.PickupInStore)
            {
                var shippingAddress = await _addressService.GetAddressByIdAsync(
                    order.ShippingAddressId ?? 0
                );

                await _addressModelFactory.PrepareAddressModelAsync(
                    model.ShippingAddress,
                    address: shippingAddress,
                    excludeProperties: false,
                    addressSettings: _addressSettings
                );
            }
            else if (
                order.PickupAddressId.HasValue
                && await _addressService.GetAddressByIdAsync(order.PickupAddressId.Value)
                    is Address pickupAddress
            )
            {
                var (addressLine, addressFields) = await _addressService.FormatAddressAsync(
                    pickupAddress,
                    languageId
                );
                model.PickupAddress = new AddressModel
                {
                    Address1 = pickupAddress.Address1,
                    City = pickupAddress.City,
                    County = pickupAddress.County,
                    StateProvinceName = await _stateProvinceService.GetStateProvinceByAddressAsync(
                        pickupAddress
                    )
                        is StateProvince stateProvince
                        ? await _localizationService.GetLocalizedAsync(
                            stateProvince,
                            entity => entity.Name
                        )
                        : string.Empty,
                    CountryName = await _countryService.GetCountryByAddressAsync(pickupAddress)
                        is Country country
                        ? await _localizationService.GetLocalizedAsync(
                            country,
                            entity => entity.Name
                        )
                        : string.Empty,
                    ZipPostalCode = pickupAddress.ZipPostalCode,
                    AddressFields = addressFields,
                    AddressLine = addressLine,
                };
            }

            model.ShippingMethod = order.ShippingMethod;

            //shipments (only already shipped or ready for pickup)
            var shipments = (
                await _shipmentService.GetShipmentsByOrderIdAsync(
                    order.Id,
                    !order.PickupInStore,
                    order.PickupInStore
                )
            )
                .OrderBy(x => x.CreatedOnUtc)
                .ToList();
            foreach (var shipment in shipments)
            {
                var shipmentModel = new OrderDetailsModel.ShipmentBriefModel
                {
                    Id = shipment.Id,
                    TrackingNumber = shipment.TrackingNumber,
                };
                if (shipment.ShippedDateUtc.HasValue)
                    shipmentModel.ShippedDate = await _dateTimeHelper.ConvertToUserTimeAsync(
                        shipment.ShippedDateUtc.Value,
                        DateTimeKind.Utc
                    );
                if (shipment.ReadyForPickupDateUtc.HasValue)
                    shipmentModel.ReadyForPickupDate = await _dateTimeHelper.ConvertToUserTimeAsync(
                        shipment.ReadyForPickupDateUtc.Value,
                        DateTimeKind.Utc
                    );
                if (shipment.DeliveryDateUtc.HasValue)
                    shipmentModel.DeliveryDate = await _dateTimeHelper.ConvertToUserTimeAsync(
                        shipment.DeliveryDateUtc.Value,
                        DateTimeKind.Utc
                    );
                model.Shipments.Add(shipmentModel);
            }
        }

        var billingAddress = await _addressService.GetAddressByIdAsync(order.BillingAddressId);

        //billing info
        await _addressModelFactory.PrepareAddressModelAsync(
            model.BillingAddress,
            address: billingAddress,
            excludeProperties: false,
            addressSettings: _addressSettings
        );

        //VAT number
        model.VatNumber = order.VatNumber;

        //payment method
        var customer = await _customerService.GetCustomerByIdAsync(order.CustomerId);
        var paymentMethod = await _paymentPluginManager.LoadPluginBySystemNameAsync(
            order.PaymentMethodSystemName,
            customer,
            order.StoreId
        );
        model.PaymentMethod =
            paymentMethod != null
                ? await _localizationService.GetLocalizedFriendlyNameAsync(
                    paymentMethod,
                    languageId
                )
                : order.PaymentMethodSystemName;
        model.PaymentMethodStatus = await _localizationService.GetLocalizedEnumAsync(
            order.PaymentStatus
        );
        model.CanRePostProcessPayment = await _paymentService.CanRePostProcessPaymentAsync(order);
        //custom values
        model.CustomValues = _paymentService.DeserializeCustomValues(order);

        //order subtotal
        if (
            order.CustomerTaxDisplayType == TaxDisplayType.IncludingTax
            && !_taxSettings.ForceTaxExclusionFromOrderSubtotal
        )
        {
            //including tax

            //order subtotal
            var orderSubtotalInclTaxInCustomerCurrency = _currencyService.ConvertCurrency(
                order.OrderSubtotalInclTax,
                order.CurrencyRate
            );
            if (orderSubtotalInclTaxInCustomerCurrency == _b2BB2CFeaturesSettings.ProductQuotePrice)
            {
                model.OrderTotal = productForQuoteResource;
                model.OrderSubTotalDiscount = productForQuoteResource;
            }
            else
            {
                model.OrderSubtotal = await _priceFormatter.FormatPriceAsync(
                    orderSubtotalInclTaxInCustomerCurrency,
                    true,
                    order.CustomerCurrencyCode,
                    languageId,
                    true
                );
            }
            model.OrderSubtotalValue = orderSubtotalInclTaxInCustomerCurrency;
            //discount (applied to order subtotal)
            var orderSubTotalDiscountInclTaxInCustomerCurrency = _currencyService.ConvertCurrency(
                order.OrderSubTotalDiscountInclTax,
                order.CurrencyRate
            );
            if (orderSubTotalDiscountInclTaxInCustomerCurrency > decimal.Zero)
            {
                model.OrderSubTotalDiscount = await _priceFormatter.FormatPriceAsync(
                    -orderSubTotalDiscountInclTaxInCustomerCurrency,
                    true,
                    order.CustomerCurrencyCode,
                    languageId,
                    true
                );
                model.OrderSubTotalDiscountValue = orderSubTotalDiscountInclTaxInCustomerCurrency;
            }
        }
        else
        {
            //excluding tax

            //order subtotal
            var orderSubtotalExclTaxInCustomerCurrency = _currencyService.ConvertCurrency(
                order.OrderSubtotalExclTax,
                order.CurrencyRate
            );
            if (orderSubtotalExclTaxInCustomerCurrency == _b2BB2CFeaturesSettings.ProductQuotePrice)
            {
                model.OrderTotal = productForQuoteResource;
                model.OrderSubTotalDiscount = productForQuoteResource;
            }
            else
            {
                model.OrderSubtotal = await _priceFormatter.FormatPriceAsync(
                    orderSubtotalExclTaxInCustomerCurrency,
                    true,
                    order.CustomerCurrencyCode,
                    languageId,
                    false
                );
            }
            model.OrderSubtotalValue = orderSubtotalExclTaxInCustomerCurrency;
            //discount (applied to order subtotal)
            var orderSubTotalDiscountExclTaxInCustomerCurrency = _currencyService.ConvertCurrency(
                order.OrderSubTotalDiscountExclTax,
                order.CurrencyRate
            );
            if (orderSubTotalDiscountExclTaxInCustomerCurrency > decimal.Zero)
            {
                model.OrderSubTotalDiscount = await _priceFormatter.FormatPriceAsync(
                    -orderSubTotalDiscountExclTaxInCustomerCurrency,
                    true,
                    order.CustomerCurrencyCode,
                    languageId,
                    false
                );
                model.OrderSubTotalDiscountValue = orderSubTotalDiscountExclTaxInCustomerCurrency;
            }
        }

        if (order.CustomerTaxDisplayType == TaxDisplayType.IncludingTax)
        {
            //including tax

            //order shipping
            var orderShippingInclTaxInCustomerCurrency = _currencyService.ConvertCurrency(
                order.OrderShippingInclTax,
                order.CurrencyRate
            );
            model.OrderShipping = await _priceFormatter.FormatShippingPriceAsync(
                orderShippingInclTaxInCustomerCurrency,
                true,
                order.CustomerCurrencyCode,
                languageId,
                true
            );
            model.OrderShippingValue = orderShippingInclTaxInCustomerCurrency;
            //payment method additional fee
            var paymentMethodAdditionalFeeInclTaxInCustomerCurrency =
                _currencyService.ConvertCurrency(
                    order.PaymentMethodAdditionalFeeInclTax,
                    order.CurrencyRate
                );
            if (paymentMethodAdditionalFeeInclTaxInCustomerCurrency > decimal.Zero)
            {
                model.PaymentMethodAdditionalFee =
                    await _priceFormatter.FormatPaymentMethodAdditionalFeeAsync(
                        paymentMethodAdditionalFeeInclTaxInCustomerCurrency,
                        true,
                        order.CustomerCurrencyCode,
                        languageId,
                        true
                    );
                model.PaymentMethodAdditionalFeeValue =
                    paymentMethodAdditionalFeeInclTaxInCustomerCurrency;
            }
        }
        else
        {
            //excluding tax

            //order shipping
            var orderShippingExclTaxInCustomerCurrency = _currencyService.ConvertCurrency(
                order.OrderShippingExclTax,
                order.CurrencyRate
            );
            model.OrderShipping = await _priceFormatter.FormatShippingPriceAsync(
                orderShippingExclTaxInCustomerCurrency,
                true,
                order.CustomerCurrencyCode,
                languageId,
                false
            );
            model.OrderShippingValue = orderShippingExclTaxInCustomerCurrency;
            //payment method additional fee
            var paymentMethodAdditionalFeeExclTaxInCustomerCurrency =
                _currencyService.ConvertCurrency(
                    order.PaymentMethodAdditionalFeeExclTax,
                    order.CurrencyRate
                );
            if (paymentMethodAdditionalFeeExclTaxInCustomerCurrency > decimal.Zero)
            {
                model.PaymentMethodAdditionalFee =
                    await _priceFormatter.FormatPaymentMethodAdditionalFeeAsync(
                        paymentMethodAdditionalFeeExclTaxInCustomerCurrency,
                        true,
                        order.CustomerCurrencyCode,
                        languageId,
                        false
                    );
                model.PaymentMethodAdditionalFeeValue =
                    paymentMethodAdditionalFeeExclTaxInCustomerCurrency;
            }
        }

        //tax
        var displayTax = true;
        var displayTaxRates = true;
        if (
            _taxSettings.HideTaxInOrderSummary
            && order.CustomerTaxDisplayType == TaxDisplayType.IncludingTax
        )
        {
            displayTax = false;
            displayTaxRates = false;
        }
        else
        {
            if (order.OrderTax == 0 && _taxSettings.HideZeroTax)
            {
                displayTax = false;
                displayTaxRates = false;
            }
            else
            {
                var taxRates = _orderService.ParseTaxRates(order, order.TaxRates);
                displayTaxRates = _taxSettings.DisplayTaxRates && taxRates.Any();
                displayTax = !displayTaxRates;

                var orderTaxInCustomerCurrency = _currencyService.ConvertCurrency(
                    order.OrderTax,
                    order.CurrencyRate
                );
                model.Tax = await _priceFormatter.FormatPriceAsync(
                    orderTaxInCustomerCurrency,
                    true,
                    order.CustomerCurrencyCode,
                    false,
                    languageId
                );

                foreach (var tr in taxRates)
                {
                    model.TaxRates.Add(
                        new OrderDetailsModel.TaxRate
                        {
                            Rate = _priceFormatter.FormatTaxRate(tr.Key),
                            Value = await _priceFormatter.FormatPriceAsync(
                                _currencyService.ConvertCurrency(tr.Value, order.CurrencyRate),
                                true,
                                order.CustomerCurrencyCode,
                                false,
                                languageId
                            ),
                        }
                    );
                }
            }
        }
        model.DisplayTaxRates = displayTaxRates;
        model.DisplayTax = displayTax;
        model.DisplayTaxShippingInfo = _catalogSettings.DisplayTaxShippingInfoOrderDetailsPage;
        model.PricesIncludeTax = order.CustomerTaxDisplayType == TaxDisplayType.IncludingTax;

        //discount (applied to order total)
        var orderDiscountInCustomerCurrency = _currencyService.ConvertCurrency(
            order.OrderDiscount,
            order.CurrencyRate
        );
        if (orderDiscountInCustomerCurrency > decimal.Zero)
        {
            model.OrderTotalDiscount = await _priceFormatter.FormatPriceAsync(
                -orderDiscountInCustomerCurrency,
                true,
                order.CustomerCurrencyCode,
                false,
                languageId
            );
            model.OrderTotalDiscountValue = orderDiscountInCustomerCurrency;
        }

        //gift cards
        foreach (var gcuh in await _giftCardService.GetGiftCardUsageHistoryAsync(order))
        {
            model.GiftCards.Add(
                new OrderDetailsModel.GiftCard
                {
                    CouponCode = (
                        await _giftCardService.GetGiftCardByIdAsync(gcuh.GiftCardId)
                    ).GiftCardCouponCode,
                    Amount = await _priceFormatter.FormatPriceAsync(
                        -(_currencyService.ConvertCurrency(gcuh.UsedValue, order.CurrencyRate)),
                        true,
                        order.CustomerCurrencyCode,
                        false,
                        languageId
                    ),
                }
            );
        }

        //reward points
        if (
            order.RedeemedRewardPointsEntryId.HasValue
            && await _rewardPointService.GetRewardPointsHistoryEntryByIdAsync(
                order.RedeemedRewardPointsEntryId.Value
            )
                is RewardPointsHistory redeemedRewardPointsEntry
        )
        {
            model.RedeemedRewardPoints = -redeemedRewardPointsEntry.Points;
            model.RedeemedRewardPointsAmount = await _priceFormatter.FormatPriceAsync(
                -(
                    _currencyService.ConvertCurrency(
                        redeemedRewardPointsEntry.UsedAmount,
                        order.CurrencyRate
                    )
                ),
                true,
                order.CustomerCurrencyCode,
                false,
                languageId
            );
        }

        //total
        var orderTotalInCustomerCurrency = _currencyService.ConvertCurrency(
            order.OrderTotal,
            order.CurrencyRate
        );
        model.OrderTotal = await _priceFormatter.FormatPriceAsync(
            orderTotalInCustomerCurrency,
            true,
            order.CustomerCurrencyCode,
            false,
            languageId
        );
        model.OrderTotalValue = orderTotalInCustomerCurrency;

        //checkout attributes
        model.CheckoutAttributeInfo = order.CheckoutAttributeDescription;
        
        var orderNotes = (await _orderService.GetOrderNotesByOrderIdAsync(order.Id, true)).OrderByDescending(on => on.CreatedOnUtc).ToList();
        //order notes
        foreach (var orderNote in orderNotes)
        {
            model.OrderNotes.Add(
                new OrderDetailsModel.OrderNote
                {
                    Id = orderNote.Id,
                    HasDownload = orderNote.DownloadId > 0,
                    Note = _orderService.FormatOrderNoteText(orderNote),
                    CreatedOn = await _dateTimeHelper.ConvertToUserTimeAsync(
                        orderNote.CreatedOnUtc,
                        DateTimeKind.Utc
                    ),
                }
            );
        }

        //purchased products
        model.ShowSku = _catalogSettings.ShowSkuOnProductDetailsPage;
        model.ShowVendorName = _vendorSettings.ShowVendorOnOrderDetailsPage;
        model.ShowProductThumbnail = _orderSettings.ShowProductThumbnailInOrderDetailsPage;

        var orderItems = await _orderService.GetOrderItemsAsync(order.Id);

        bool productForQuote = false;
        foreach (var orderItem in orderItems)
        {
            var product = await _erpProductService.GetProductByIdAsync(orderItem.ProductId, false);

            var orderItemModel = new OrderDetailsModel.OrderItemModel
            {
                Id = orderItem.Id,
                OrderItemGuid = orderItem.OrderItemGuid,
                Sku = await _productService.FormatSkuAsync(product, orderItem.AttributesXml),
                VendorName =
                    (await _vendorService.GetVendorByIdAsync(product.VendorId))?.Name
                    ?? string.Empty,
                ProductId =
                    (product.ParentGroupedProductId > 0 && !product.VisibleIndividually)
                        ? product.ParentGroupedProductId
                        : product.Id,
                ProductName = await _localizationService.GetLocalizedAsync(product, x => x.Name),
                ProductSeName = await _urlRecordService.GetSeNameAsync(product),
                Quantity = orderItem.Quantity,
                AttributeInfo = orderItem.AttributeDescription,
            };
            //rental info
            if (product.IsRental)
            {
                var rentalStartDate = orderItem.RentalStartDateUtc.HasValue
                    ? _productService.FormatRentalDate(product, orderItem.RentalStartDateUtc.Value)
                    : "";
                var rentalEndDate = orderItem.RentalEndDateUtc.HasValue
                    ? _productService.FormatRentalDate(product, orderItem.RentalEndDateUtc.Value)
                    : "";
                orderItemModel.RentalInfo = string.Format(
                    await _localizationService.GetResourceAsync("Order.Rental.FormattedDate"),
                    rentalStartDate,
                    rentalEndDate
                );
            }

            #region B2B

            var isProductOnSpecial =
                await _erpCustomerFunctionalityService.IsTheProductFromSpecialCategoryAsync(
                    product
                );
            orderItemModel.CustomProperties.Add(
                B2BB2CFeaturesDefaults.ProductIsOnSpecial,
                isProductOnSpecial.ToString()
            );

            #endregion

            model.Items.Add(orderItemModel);

            //unit price, subtotal
            if (order.CustomerTaxDisplayType == TaxDisplayType.IncludingTax)
            {
                //including tax
                var unitPriceInclTaxInCustomerCurrency = _currencyService.ConvertCurrency(
                    orderItem.UnitPriceInclTax,
                    order.CurrencyRate
                );
                orderItemModel.UnitPrice = await _priceFormatter.FormatPriceAsync(
                    unitPriceInclTaxInCustomerCurrency,
                    true,
                    order.CustomerCurrencyCode,
                    languageId,
                    true
                );
                orderItemModel.UnitPriceValue = unitPriceInclTaxInCustomerCurrency;

                var priceInclTaxInCustomerCurrency = _currencyService.ConvertCurrency(
                    orderItem.PriceInclTax,
                    order.CurrencyRate
                );
                orderItemModel.SubTotal = await _priceFormatter.FormatPriceAsync(
                    priceInclTaxInCustomerCurrency,
                    true,
                    order.CustomerCurrencyCode,
                    languageId,
                    true
                );
                orderItemModel.SubTotalValue = priceInclTaxInCustomerCurrency;
            }
            else
            {
                //excluding tax
                var unitPriceExclTaxInCustomerCurrency = _currencyService.ConvertCurrency(
                    orderItem.UnitPriceExclTax,
                    order.CurrencyRate
                );
                orderItemModel.UnitPrice = await _priceFormatter.FormatPriceAsync(
                    unitPriceExclTaxInCustomerCurrency,
                    true,
                    order.CustomerCurrencyCode,
                    languageId,
                    false
                );
                orderItemModel.UnitPriceValue = unitPriceExclTaxInCustomerCurrency;

                var priceExclTaxInCustomerCurrency = _currencyService.ConvertCurrency(
                    orderItem.PriceExclTax,
                    order.CurrencyRate
                );
                orderItemModel.SubTotal = await _priceFormatter.FormatPriceAsync(
                    priceExclTaxInCustomerCurrency,
                    true,
                    order.CustomerCurrencyCode,
                    languageId,
                    false
                );
                orderItemModel.SubTotalValue = priceExclTaxInCustomerCurrency;
            }

            //downloadable products
            if (await _orderService.IsDownloadAllowedAsync(orderItem))
                orderItemModel.DownloadId = product.DownloadId;
            if (await _orderService.IsLicenseDownloadAllowedAsync(orderItem))
                orderItemModel.LicenseId = orderItem.LicenseDownloadId ?? 0;

            if (_orderSettings.ShowProductThumbnailInOrderDetailsPage)
            {
                orderItemModel.Picture = await PrepareOrderItemPictureModelAsync(
                    orderItem,
                    _mediaSettings.OrderThumbPictureSize,
                    true,
                    orderItemModel.ProductName
                );
            }
            if (orderItem.UnitPriceExclTax == _b2BB2CFeaturesSettings.ProductQuotePrice)
            {
                productForQuote = true;
                orderItemModel.SubTotal = productForQuoteResource;
                orderItemModel.UnitPrice = productForQuoteResource;
            }
        }
        if (productForQuote)
        {
            model.OrderShipping = productForQuoteResource;
            model.OrderSubtotal = productForQuoteResource;
            model.OrderSubTotalDiscount = productForQuoteResource;
            model.OrderTotal = productForQuoteResource;
            model.OrderTotalDiscount = productForQuoteResource;
            model.PaymentMethodAdditionalFee = productForQuoteResource;
            model.Tax = productForQuoteResource;

            model.CustomProperties.Add("ProductForQuote", "true");
        }

        return model;
    }

    #endregion
}
