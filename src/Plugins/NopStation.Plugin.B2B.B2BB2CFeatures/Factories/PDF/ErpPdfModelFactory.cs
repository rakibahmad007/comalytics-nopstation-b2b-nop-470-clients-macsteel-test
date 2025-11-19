using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Directory;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Shipping;
using Nop.Core.Domain.Tax;
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
using NopStation.Plugin.B2B.B2BB2CFeatures.Contexts;
using NopStation.Plugin.B2B.B2BB2CFeatures.Model.PDF;
using NopStation.Plugin.B2B.B2BB2CFeatures.Services.ErpCustomerFunctionality;
using NopStation.Plugin.B2B.B2BB2CFeatures.Services.ErpSpecificationAttributeService;
using NopStation.Plugin.B2B.ERPIntegrationCore.Domain;
using NopStation.Plugin.B2B.ERPIntegrationCore.Enums;
using NopStation.Plugin.B2B.ERPIntegrationCore.Services;

namespace NopStation.Plugin.B2B.B2BB2CFeatures.Factories.PDF;

public class ErpPdfModelFactory : IErpPdfModelFactory
{
    #region Fields

    private readonly IOrderService _orderService;
    private readonly IErpOrderAdditionalDataService _erpOrderAdditionalDataService;
    private readonly IErpOrderItemAdditionalDataService _erpOrderItemAdditionalDataService;
    private readonly B2BB2CFeaturesSettings _b2BB2CFeaturesSettings;
    private readonly ILocalizationService _localizationService;
    private readonly IProductService _productService;
    private readonly IB2BB2CWorkContext _b2BB2CWorkContext;
    private readonly IErpAccountService _erpAccountService;
    private readonly IMeasureService _measureService;
    private readonly MeasureSettings _measureSettings;
    private readonly IPriceFormatter _priceFormatter;
    private readonly IVendorService _vendorService;
    private readonly IPictureService _pictureService;
    private readonly IAddressService _addressService;
    private readonly IStateProvinceService _stateProvinceService;
    private readonly IAddressModelFactory _addressModelFactory;
    private readonly AddressSettings _addressSettings;
    private readonly IErpShipToAddressService _erpShipToAddressService;
    private readonly IDateTimeHelper _dateTimeHelper;
    private readonly IShipmentService _shipmentService;
    private readonly ICustomerService _customerService;
    private readonly IRewardPointService _rewardPointService;
    private readonly IGiftCardService _giftCardService;
    private readonly ICurrencyService _currencyService;
    private readonly TaxSettings _taxSettings;
    private readonly IPaymentService _paymentService;
    private readonly IPaymentPluginManager _paymentPluginManager;
    private readonly CatalogSettings _catalogSettings;
    private readonly IErpSalesOrgService _erpSalesOrgService;
    private readonly ICountryService _countryService;
    private readonly IProductAttributeFormatter _productAttributeFormatter;
    private readonly IUrlRecordService _urlRecordService;
    private readonly IErpCustomerFunctionalityService _erpCustomerFunctionalityService;
    private readonly IErpProductService _erpProductService;
    private readonly IErpNopUserService _erpNopUserService;
    private readonly ILanguageService _languageService;
    private readonly IErpSpecificationAttributeService _erpSpecificationAttributeService;
    private readonly IErpLogsService _erpLogsService;

    #endregion Fields

    #region Ctor

    public ErpPdfModelFactory(IOrderService orderService,
        IErpOrderItemAdditionalDataService erpOrderItemAdditionalDataService,
        IErpOrderAdditionalDataService erpOrderAdditionalDataService,
        B2BB2CFeaturesSettings b2BB2CFeaturesSettings,
        ILocalizationService localizationService,
        IB2BB2CWorkContext b2BB2CWorkContext,
        IErpAccountService erpAccountService,
        IMeasureService measureService,
        MeasureSettings measureSettings,
        IPriceFormatter priceFormatter,
        IProductService productService,
        IPictureService pictureService,
        IVendorService vendorService,
        IAddressService addressService,
        IStateProvinceService stateProvinceService,
        IAddressModelFactory addressModelFactory,
        AddressSettings addressSettings,
        IErpShipToAddressService erpShipToAddressService,
        IDateTimeHelper dateTimeHelper,
        IShipmentService shipmentService,
        ICustomerService customerService,
        IPaymentPluginManager paymentPluginManager,
        IPaymentService paymentService,
        TaxSettings taxSettings,
        ICurrencyService currencyService,
        IGiftCardService giftCardService,
        IRewardPointService rewardPointService,
        CatalogSettings catalogSettings,
        IErpSalesOrgService erpSalesOrgService,
        ICountryService countryService,
        IProductAttributeFormatter productAttributeFormatter,
        IUrlRecordService urlRecordService,
        IErpCustomerFunctionalityService erpCustomerFunctionalityService,
        IErpProductService erpProductService,
        IErpNopUserService erpNopUserService,
        ILanguageService languageService,
        IErpSpecificationAttributeService erpSpecificationAttributeService,
        IErpLogsService erpLogsService)
    {
        _orderService = orderService;
        _erpOrderItemAdditionalDataService = erpOrderItemAdditionalDataService;
        _erpOrderAdditionalDataService = erpOrderAdditionalDataService;
        _b2BB2CFeaturesSettings = b2BB2CFeaturesSettings;
        _localizationService = localizationService;
        _b2BB2CWorkContext = b2BB2CWorkContext;
        _erpAccountService = erpAccountService;
        _measureService = measureService;
        _measureSettings = measureSettings;
        _priceFormatter = priceFormatter;
        _productService = productService;
        _pictureService = pictureService;
        _vendorService = vendorService;
        _addressService = addressService;
        _stateProvinceService = stateProvinceService;
        _addressModelFactory = addressModelFactory;
        _addressSettings = addressSettings;
        _erpShipToAddressService = erpShipToAddressService;
        _dateTimeHelper = dateTimeHelper;
        _shipmentService = shipmentService;
        _customerService = customerService;
        _paymentPluginManager = paymentPluginManager;
        _paymentService = paymentService;
        _taxSettings = taxSettings;
        _currencyService = currencyService;
        _giftCardService = giftCardService;
        _rewardPointService = rewardPointService;
        _catalogSettings = catalogSettings;
        _erpSalesOrgService = erpSalesOrgService;
        _countryService = countryService;
        _productAttributeFormatter = productAttributeFormatter;
        _urlRecordService = urlRecordService;
        _erpCustomerFunctionalityService = erpCustomerFunctionalityService;
        _erpProductService = erpProductService;
        _erpNopUserService = erpNopUserService;
        _languageService = languageService;
        _erpSpecificationAttributeService = erpSpecificationAttributeService;
        _erpLogsService = erpLogsService;
    }

    #endregion Ctor

    //Email = customer.Email,
    //FirstName = customer.FirstName,
    //LastName = customer.LastName,
    //Company = model.Company,
    //Address1 = model.HouseNumber,
    //Address2 = model.Street,
    //City = model.CityName,
    //StateProvinceId = stateProvince.Id,
    //CountryId = country.Id,
    //ZipPostalCode = model.PostalCode,
    //PhoneNumber = model.Phone,
    //FaxNumber = model.Fax,

    #region Methods

    #region Utilities

    private async Task PrepareAddressPdfModelAsync(AddressPdfModel model, int addressId)
    {
        ArgumentNullException.ThrowIfNull(model);

        var address = await _addressService.GetAddressByIdAsync(addressId);
        if (address != null)
        {
            model.Name = address.FirstName + " " + address.LastName;
            model.Email = address.Email;
            model.Company = address.Company;
            model.Address1 = address.Address1;
            model.Address2 = address.Address2;
            model.City = address.City;
            model.StateProvince = (await _stateProvinceService.GetStateProvinceByIdAsync(address.StateProvinceId ?? 0))?.Name ?? string.Empty;
            model.Country = (await _countryService.GetCountryByIdAsync(address.CountryId ?? 0))?.Name ?? string.Empty;
            model.ZipPostalCode = address.ZipPostalCode;
            model.PhoneNumber = address.PhoneNumber;
        }
    }

    private async Task PrepareErpAccountPdfModelAsync(OrderPdfModel model, ErpOrderAdditionalData erpOrderAdditionalData)
    {
        ArgumentNullException.ThrowIfNull(model);
        ArgumentNullException.ThrowIfNull(erpOrderAdditionalData);

        var erpAccount = await _erpAccountService.GetErpAccountByIdAsync(erpOrderAdditionalData.ErpAccountId);
        if (erpAccount != null)
        {
            model.ErpAccountPdfModel.AccountNumber = erpAccount.AccountNumber;
            model.ErpAccountPdfModel.AccountName = erpAccount.AccountName;
            model.ErpAccountPdfModel.VatNumber = erpAccount.VatNumber;
            model.ErpAccountPdfModel.CreditLimit = erpAccount.CreditLimit;
            model.ErpAccountPdfModel.CurrentBalance = erpAccount.CurrentBalance;
            model.ErpAccountPdfModel.PaymentTypeCode = erpAccount.PaymentTypeCode;
            model.ErpAccountPdfModel.BillingSuburb = erpAccount.BillingSuburb;
            await PrepareAddressPdfModelAsync(model.ErpAccountPdfModel.BillingAddress, erpAccount.BillingAddressId ?? 0);
        }
    }

    private async Task PrepareBillingAddressPdfModelAsync(OrderPdfModel model, Order order)
    {
        ArgumentNullException.ThrowIfNull(model);
        ArgumentNullException.ThrowIfNull(order);

        await PrepareAddressPdfModelAsync(model.BillingAddressPdfModel, order.BillingAddressId);
    }

    private async Task PrepareSalesOrgPdfModelAsync(OrderPdfModel model, int salesOrgId)
    {
        ArgumentNullException.ThrowIfNull(model);

        var erpSalesOrg = await _erpSalesOrgService.GetErpSalesOrgByIdAsync(salesOrgId);
        if (erpSalesOrg != null)
        {
            model.SalesOrgPdfModel.SalesOrgCode = erpSalesOrg.Code;
            model.SalesOrgPdfModel.SalesOrgName = erpSalesOrg.Name;
            model.SalesOrgPdfModel.Suburb = erpSalesOrg.Suburb;
            model.SalesOrgPdfModel.Email = erpSalesOrg.Email;
            await PrepareAddressPdfModelAsync(model.SalesOrgPdfModel.Address, erpSalesOrg.AddressId);
        }
    }

    private async Task PrepareShipToAddressPdfModelAsync(OrderPdfModel model, Order order, ErpOrderAdditionalData erpOrderAdditionalData)
    {
        ArgumentNullException.ThrowIfNull(model);
        ArgumentNullException.ThrowIfNull(order);
        ArgumentNullException.ThrowIfNull(erpOrderAdditionalData);

        if (order.ShippingStatus != ShippingStatus.ShippingNotRequired)
        {
            if (!order.PickupInStore)
            {
                var erpShipToAddress = await _erpShipToAddressService.GetErpShipToAddressByIdAsync(erpOrderAdditionalData.ErpShipToAddressId ?? 0);
                if (erpShipToAddress != null)
                {
                    model.ShippingAddressPdfModel.ShipToCode = erpShipToAddress.ShipToCode;
                    model.ShippingAddressPdfModel.ShipToName = erpShipToAddress.ShipToName;
                    model.ShippingAddressPdfModel.DeliveryNotes = erpShipToAddress.DeliveryNotes;
                    model.ShippingAddressPdfModel.Suburb = erpShipToAddress.Suburb;

                    await PrepareAddressPdfModelAsync(model.ShippingAddressPdfModel.Address, order.ShippingAddressId ?? 0);
                }
            }
            else
            {
                await PrepareAddressPdfModelAsync(model.PickupAddressPdfModel, order.PickupAddressId ?? 0);
            }
        }
    }

    private async Task PrepareOrderSummaryPdfModelAsync(OrderPdfModel model, Order order, ErpOrderAdditionalData erpOrderAdditionalData)
    {
        ArgumentNullException.ThrowIfNull(model);
        ArgumentNullException.ThrowIfNull(order);
        ArgumentNullException.ThrowIfNull(erpOrderAdditionalData);

       var salesRep = await _customerService.GetCustomerByIdAsync(erpOrderAdditionalData.OrderPlacedByNopCustomerId);

        model.OrderSummaryPdfModel.OrderNumber = string.IsNullOrEmpty(erpOrderAdditionalData.ErpOrderNumber)
            ? order.Id.ToString()
            : erpOrderAdditionalData.ErpOrderNumber;
        model.OrderSummaryPdfModel.CustomOrderNumber = order.CustomOrderNumber;
        model.OrderSummaryPdfModel.OrderDate = order.CreatedOnUtc;
        model.OrderSummaryPdfModel.OrderType = erpOrderAdditionalData.ErpOrderType;
        model.OrderSummaryPdfModel.DeliveryDate = erpOrderAdditionalData.DeliveryDate;
        model.OrderSummaryPdfModel.OrderStatus = await _localizationService.GetLocalizedEnumAsync(order.OrderStatus);
        model.OrderSummaryPdfModel.PaymentStatus = await _localizationService.GetLocalizedEnumAsync(order.PaymentStatus);
        model.OrderSummaryPdfModel.ShippingStatus = await _localizationService.GetLocalizedEnumAsync(order.ShippingStatus);
        model.OrderSummaryPdfModel.CustomerReference = erpOrderAdditionalData.CustomerReference;
        model.OrderSummaryPdfModel.PickupInStore = order.PickupInStore;

        if (salesRep is not null)
        model.OrderSummaryPdfModel.SalesRepName = await _customerService.GetCustomerFullNameAsync(salesRep);

        // Basic order totals (existing)
        model.OrderSummaryPdfModel.OrderSubtotal = order.OrderSubtotalExclTax;
        model.OrderSummaryPdfModel.OrderSubtotalFormatted = await _priceFormatter.FormatPriceAsync(order.OrderSubtotalExclTax, true, order.CustomerCurrencyCode, false, order.CustomerLanguageId);

        model.OrderSummaryPdfModel.TotalDiscount = order.OrderDiscount;
        model.OrderSummaryPdfModel.TotalDiscountFormatted = await _priceFormatter.FormatPriceAsync(order.OrderDiscount, true, order.CustomerCurrencyCode, false, order.CustomerLanguageId);

        model.OrderSummaryPdfModel.ShippingCost = order.OrderShippingExclTax;
        model.OrderSummaryPdfModel.ShippingCostFormatted = await _priceFormatter.FormatPriceAsync(order.OrderShippingExclTax, true, order.CustomerCurrencyCode, false, order.CustomerLanguageId);

        model.OrderSummaryPdfModel.TaxAmount = order.OrderTax;
        model.OrderSummaryPdfModel.TaxAmountFormatted = await _priceFormatter.FormatPriceAsync(order.OrderTax, true, order.CustomerCurrencyCode, false, order.CustomerLanguageId);

        if (!model.IsB2b)
        {
            model.OrderSummaryPdfModel.CashRounding = await _priceFormatter.FormatPriceAsync(
                erpOrderAdditionalData.CashRounding.HasValue ? erpOrderAdditionalData.CashRounding.Value : decimal.Zero,
                true,
                order.CustomerCurrencyCode,
                false,
                order.CustomerLanguageId) + "-";
        }

        model.OrderSummaryPdfModel.OrderTotal = order.OrderTotal;
        model.OrderSummaryPdfModel.OrderTotalFormatted = await _priceFormatter.FormatPriceAsync(order.OrderTotal, true, order.CustomerCurrencyCode, false, order.CustomerLanguageId);

        model.OrderSummaryPdfModel.CurrencyCode = order.CustomerCurrencyCode;
        //model.OrderSummaryPdfModel.OrderNotes = order.OrderNotes;

        model.OrderSummaryPdfModel.ErpOrderType = _erpOrderAdditionalDataService.GetErpOrderTypeByOrderTypeEnum(erpOrderAdditionalData.ErpOrderType);

        // Get payment method name
        var paymentMethod = await _paymentPluginManager.LoadPluginBySystemNameAsync(order.PaymentMethodSystemName);
        model.OrderSummaryPdfModel.PaymentMethod = paymentMethod?.PluginDescriptor?.FriendlyName ?? order.PaymentMethodSystemName;

        // Get shipping method name
        model.OrderSummaryPdfModel.ShippingMethod = order.ShippingMethod;

        // Additional comprehensive totals (new fields) with conditional tax display logic

        // Special Instructions from ERP order additional data
        model.OrderSummaryPdfModel.SpecialInstructions = erpOrderAdditionalData.SpecialInstructions ?? string.Empty;



        // TODO : need to cross check from here: taken from 4.2

        // Order Subtotal - conditional based on tax display type
        if (order.CustomerTaxDisplayType == TaxDisplayType.IncludingTax &&
            !_taxSettings.ForceTaxExclusionFromOrderSubtotal)
        {
            // Including tax
            model.OrderSummaryPdfModel.OrderSubtotalInclTax = order.OrderSubtotalInclTax;
            model.OrderSummaryPdfModel.OrderSubtotalInclTaxFormatted = await _priceFormatter.FormatPriceAsync(
                _currencyService.ConvertCurrency(order.OrderSubtotalInclTax, order.CurrencyRate),
                true, order.CustomerCurrencyCode, true, order.CustomerLanguageId);
        }
        else
        {
            // Excluding tax (already handled in basic totals, but set IncludingTax versions for completeness)
            model.OrderSummaryPdfModel.OrderSubtotalInclTax = order.OrderSubtotalInclTax;
            model.OrderSummaryPdfModel.OrderSubtotalInclTaxFormatted = await _priceFormatter.FormatPriceAsync(
                _currencyService.ConvertCurrency(order.OrderSubtotalInclTax, order.CurrencyRate),
                true, order.CustomerCurrencyCode, true, order.CustomerLanguageId);
        }

        // Order Subtotal Discount - conditional based on tax display type
        if (order.OrderSubTotalDiscountExclTax > decimal.Zero)
        {
            if (order.CustomerTaxDisplayType == TaxDisplayType.IncludingTax &&
                !_taxSettings.ForceTaxExclusionFromOrderSubtotal)
            {
                // Including tax
                model.OrderSummaryPdfModel.OrderSubtotalDiscountInclTax = order.OrderSubTotalDiscountInclTax;
                model.OrderSummaryPdfModel.OrderSubtotalDiscountInclTaxFormatted = await _priceFormatter.FormatPriceAsync(
                    _currencyService.ConvertCurrency(order.OrderSubTotalDiscountInclTax, order.CurrencyRate),
                    true, order.CustomerCurrencyCode, true, order.CustomerLanguageId);
            }
            else
            {
                // Excluding tax
                model.OrderSummaryPdfModel.OrderSubtotalDiscountExclTax = order.OrderSubTotalDiscountExclTax;
                model.OrderSummaryPdfModel.OrderSubtotalDiscountExclTaxFormatted = await _priceFormatter.FormatPriceAsync(
                    _currencyService.ConvertCurrency(order.OrderSubTotalDiscountExclTax, order.CurrencyRate),
                    true, order.CustomerCurrencyCode, false, order.CustomerLanguageId);
            }
        }

        // Shipping - conditional based on tax display type and shipping status
        if (order.ShippingStatus != ShippingStatus.ShippingNotRequired)
        {
            if (order.CustomerTaxDisplayType == TaxDisplayType.IncludingTax)
            {
                // Including tax
                model.OrderSummaryPdfModel.ShippingCostInclTax = order.OrderShippingInclTax;
                model.OrderSummaryPdfModel.ShippingCostInclTaxFormatted = await _priceFormatter.FormatShippingPriceAsync(
                    _currencyService.ConvertCurrency(order.OrderShippingInclTax, order.CurrencyRate),
                    true, order.CustomerCurrencyCode, order.CustomerLanguageId, true);
            }
            else
            {
                // Excluding tax (already handled in basic totals, but set IncludingTax versions for completeness)
                model.OrderSummaryPdfModel.ShippingCostInclTax = order.OrderShippingInclTax;
                model.OrderSummaryPdfModel.ShippingCostInclTaxFormatted = await _priceFormatter.FormatShippingPriceAsync(
                    _currencyService.ConvertCurrency(order.OrderShippingInclTax, order.CurrencyRate),
                    true, order.CustomerCurrencyCode, order.CustomerLanguageId, true);
            }
        }

        // Payment Method Additional Fee - conditional based on tax display type
        if (order.PaymentMethodAdditionalFeeExclTax > decimal.Zero)
        {
            if (order.CustomerTaxDisplayType == TaxDisplayType.IncludingTax)
            {
                // Including tax
                model.OrderSummaryPdfModel.PaymentMethodAdditionalFeeInclTax = order.PaymentMethodAdditionalFeeInclTax;
                model.OrderSummaryPdfModel.PaymentMethodAdditionalFeeInclTaxFormatted = await _priceFormatter.FormatPaymentMethodAdditionalFeeAsync(
                    _currencyService.ConvertCurrency(order.PaymentMethodAdditionalFeeInclTax, order.CurrencyRate),
                    true, order.CustomerCurrencyCode, order.CustomerLanguageId, true);
            }
            else
            {
                // Excluding tax
                model.OrderSummaryPdfModel.PaymentMethodAdditionalFeeExclTax = order.PaymentMethodAdditionalFeeExclTax;
                model.OrderSummaryPdfModel.PaymentMethodAdditionalFeeExclTaxFormatted = await _priceFormatter.FormatPaymentMethodAdditionalFeeAsync(
                    _currencyService.ConvertCurrency(order.PaymentMethodAdditionalFeeExclTax, order.CurrencyRate),
                    true, order.CustomerCurrencyCode, order.CustomerLanguageId, false);
            }
        }

        // Tax display and rates breakdown - conditional logic from PrintTotalsForB2CUser
        var taxRates = new SortedDictionary<decimal, decimal>();
        bool displayTax;
        var displayTaxRates = true;

        if (_taxSettings.HideTaxInOrderSummary && order.CustomerTaxDisplayType == TaxDisplayType.IncludingTax)
        {
            displayTax = false;
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
                taxRates = _orderService.ParseTaxRates(order, order.TaxRates);
                displayTaxRates = _taxSettings.DisplayTaxRates && taxRates.Any();
                displayTax = !displayTaxRates;
            }
        }

        // Store raw tax rates and formatted tax rates breakdown
        model.OrderSummaryPdfModel.TaxRates = order.TaxRates;

        // Format tax rates breakdown if needed
        if (displayTaxRates && taxRates.Any())
        {
            var taxRatesList = new List<string>();
            foreach (var item in taxRates)
            {
                var taxRate = string.Format(await _localizationService.GetResourceAsync("PDFInvoice.TaxRate", order.CustomerLanguageId),
                    _priceFormatter.FormatTaxRate(item.Key));
                var taxValue = await _priceFormatter.FormatPriceAsync(
                    _currencyService.ConvertCurrency(item.Value, order.CurrencyRate), true, order.CustomerCurrencyCode,
                    false, order.CustomerLanguageId);
                taxRatesList.Add($"{taxRate} {taxValue}");
            }
            // Store formatted tax rates breakdown (could be used by PDF generator)
            model.OrderSummaryPdfModel.TaxRates += $" | Breakdown: {string.Join(", ", taxRatesList)}";
        }

        //// Gift Card Usage History
        //if (order.GiftCardUsageHistory?.Any() == true)
        //{
        //    var giftCardUsages = new List<string>();
        //    foreach (var gcuh in order.GiftCardUsageHistory)
        //    {
        //        var gcTitle = string.Format(await _localizationService.GetResourceAsync("PDFInvoice.GiftCardInfo", order.CustomerLanguageId),
        //            gcuh.GiftCard.GiftCardCouponCode);
        //        var gcAmountStr = await _priceFormatter.FormatPriceAsync(
        //            -_currencyService.ConvertCurrency(gcuh.UsedValue, order.CurrencyRate), true,
        //            order.CustomerCurrencyCode, false, order.CustomerLanguageId);
        //        giftCardUsages.Add($"{gcTitle} {gcAmountStr}");
        //    }
        //    model.OrderSummaryPdfModel.GiftCardUsageHistory = string.Join("; ", giftCardUsages);
        //}
        //else
        //{
        //    model.OrderSummaryPdfModel.GiftCardUsageHistory = string.Empty;
        //}

        //// Reward Points
        //if (order.RedeemedRewardPointsEntry != null)
        //{
        //    model.OrderSummaryPdfModel.RewardPointsUsed = -order.RedeemedRewardPointsEntry.Points;
        //    model.OrderSummaryPdfModel.RewardPointsAmount = order.RedeemedRewardPointsEntry.UsedAmount;
        //    model.OrderSummaryPdfModel.RewardPointsAmountFormatted = await _priceFormatter.FormatPriceAsync(
        //        -_currencyService.ConvertCurrency(order.RedeemedRewardPointsEntry.UsedAmount, order.CurrencyRate),
        //        true, order.CustomerCurrencyCode, false, order.CustomerLanguageId);
        //}
        //else
        //{
        //    model.OrderSummaryPdfModel.RewardPointsUsed = null;
        //    model.OrderSummaryPdfModel.RewardPointsAmount = null;
        //    model.OrderSummaryPdfModel.RewardPointsAmountFormatted = string.Empty;
        //}
    }

    private async Task PrepareOrderItemPdfModelListAsync(OrderPdfModel model, Order order)
    {
        ArgumentNullException.ThrowIfNull(model);
        ArgumentNullException.ThrowIfNull(order);

        var orderItems = await _orderService.GetOrderItemsAsync(order.Id);

        if(orderItems.Any(a => a.UnitPriceExclTax == _b2BB2CFeaturesSettings.ProductQuotePrice))
            model.OrderSummaryPdfModel.CashRounding = await _localizationService.GetResourceAsync("Products.ProductForQuote");

        var additionalDataDict = (await _erpOrderItemAdditionalDataService.GetAllErpOrderItemAdditionalDataByErpOrderIdAsync(order.Id)).ToDictionary(x => x.NopOrderItemId);

        foreach (var orderItem in orderItems)
        {
            var product = await _erpProductService.GetProductByIdAsync(orderItem.ProductId);
            if (product == null)
                continue;

            var notes = additionalDataDict.GetValueOrDefault(orderItem.Id)?.ErpOrderLineNotes;

            var orderItemModel = new OrderItemPdfModel
            {
                ProductName = product.Name,
                Sku = product.Sku,
                Price = orderItem.UnitPriceExclTax,
                PriceFormatted = await _priceFormatter.FormatPriceAsync(orderItem.UnitPriceExclTax, true, order.CustomerCurrencyCode, false, order.CustomerLanguageId),
                Quantity = orderItem.Quantity,

                Total = orderItem.PriceExclTax,
                TotalFormatted = await _priceFormatter.FormatPriceAsync(orderItem.PriceExclTax, true, order.CustomerCurrencyCode, false, order.CustomerLanguageId),

                Discount = orderItem.DiscountAmountExclTax,
                DiscountFormatted = await _priceFormatter.FormatPriceAsync(orderItem.DiscountAmountExclTax, true, order.CustomerCurrencyCode, false, order.CustomerLanguageId),

                NetTotal = orderItem.PriceExclTax - orderItem.DiscountAmountExclTax,
                NetTotalFormatted = await _priceFormatter.FormatPriceAsync(orderItem.PriceExclTax - orderItem.DiscountAmountExclTax, true, order.CustomerCurrencyCode, false, order.CustomerLanguageId),

                ProductDescription = await _localizationService.GetLocalizedAsync(product, x => x.ShortDescription),
                TaxRate = decimal.Zero, // Will be calculated if tax settings require it

                TaxAmount = decimal.Zero,
                TaxAmountFormatted = await _priceFormatter.FormatPriceAsync(decimal.Zero, true, order.CustomerCurrencyCode, false, order.CustomerLanguageId),

                UnitOfMeasure = await _erpSpecificationAttributeService.GetProductUOMByProductIdAndSpecificationAttributeId(orderItem.ProductId, _b2BB2CFeaturesSettings.UnitOfMeasureSpecificationAttributeId) ?? string.Empty,

                ErpOrderLineNotes = notes
            };

            if (!string.IsNullOrEmpty(orderItem.AttributesXml))
            {
                var attributeDescription = await _productAttributeFormatter.FormatAttributesAsync(product, orderItem.AttributesXml);
                if (!string.IsNullOrEmpty(attributeDescription))
                {
                    orderItemModel.ProductDescription += $" - {attributeDescription}";
                }
            }

            model.OrderItemPdfModelList.Add(orderItemModel);
        }
    }

    #endregion Utilities

    public async Task<OrderPdfModel> PrepareErpOrderPdfModelAsync(Order order)
    {
        ArgumentNullException.ThrowIfNull(order);

        var erpOrderAdditionalData = await _erpOrderAdditionalDataService.GetErpOrderAdditionalDataByNopOrderIdAsync(order.Id);

        ArgumentNullException.ThrowIfNull(erpOrderAdditionalData);

        try
        {
            var model = new OrderPdfModel();

            var erpNopUser = await _erpNopUserService.GetErpNopUserByCustomerIdAsync(order.CustomerId);
            if (erpNopUser == null)
                return model;

            var erpAccount = await _erpAccountService.GetErpAccountByIdAsync(erpOrderAdditionalData.ErpAccountId);
            if (erpAccount == null)
                return model;

            model.IsB2b = erpNopUser.ErpUserType == ErpUserType.B2BUser;

            await PrepareSalesOrgPdfModelAsync(model, erpAccount.ErpSalesOrgId);

            await PrepareErpAccountPdfModelAsync(model, erpOrderAdditionalData);

            await PrepareBillingAddressPdfModelAsync(model, order);

            await PrepareShipToAddressPdfModelAsync(model, order, erpOrderAdditionalData);

            await PrepareOrderSummaryPdfModelAsync(model, order, erpOrderAdditionalData);

            await PrepareOrderItemPdfModelListAsync(model, order);

            return model;
        }
        catch (Exception ex)
        {
            await _erpLogsService.InsertErpLogAsync(ErpLogLevel.Error,
                ErpSyncLevel.Order,
                $"Something went wrong while creating PDF for order id -{order.Id}, customer id = {order.CustomerId} " +
                $"due to {ex.Message}",
                ex.StackTrace);

            return null;
        }
    }

    #endregion Methods
}