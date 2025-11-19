using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Core.Domain.Directory;
using Nop.Services.Catalog;
using Nop.Services.Common;
using Nop.Services.Directory;
using Nop.Services.Localization;
using Nop.Services.Orders;
using Nop.Services.Shipping;
using NopStation.Plugin.B2B.B2BB2CFeatures.Model.Checkout;
using NopStation.Plugin.B2B.B2BB2CFeatures.Model.OrderSummary;
using NopStation.Plugin.B2B.B2BB2CFeatures.Services.ErpCustomerFunctionality;
using NopStation.Plugin.B2B.B2BB2CFeatures.Services.ERPSAPErrorMsgTranslations;
using NopStation.Plugin.B2B.ERPIntegrationCore.Enums;
using NopStation.Plugin.B2B.ERPIntegrationCore.Services;

namespace NopStation.Plugin.B2B.B2BB2CFeatures.Factories;

public class ErpOrderItemModelFactory : IErpOrderItemModelFactory
{
    private readonly ILocalizationService _localizationService;
    private readonly B2BB2CFeaturesSettings _b2BB2CFeaturesSettings;
    private readonly IPriceFormatter _priceFormatter;
    private readonly IErpAccountService _erpAccountService;
    private readonly IOrderService _orderService;
    private readonly IErpOrderItemAdditionalDataService _erpOrderItemAdditionalDataService;
    private readonly IErpOrderAdditionalDataService _erpOrderAdditionalDataService;
    private readonly IMeasureService _measureService;
    private readonly MeasureSettings _measureSettings;
    private readonly IErpCustomerFunctionalityService _erpCustomerFunctionalityService;
    private readonly IProductService _productService;
    private readonly IB2BSalesOrgPickupPointService _b2BSalesOrgPickupPointService;
    private readonly IShippingService _shippingService;
    private readonly IStoreContext _storeContext;
    private readonly ICountryService _countryService;
    private readonly IStateProvinceService _stateProvinceService;
    private readonly IWorkContext _workContext;
    private readonly IAddressService _addressService;
    private readonly IERPSAPErrorMsgTranslationService _eRPSAPErrorMsgTranslationService;

    public ErpOrderItemModelFactory(
        IOrderService orderService,
        IErpOrderItemAdditionalDataService erpOrderItemAdditionalDataService,
        IErpOrderAdditionalDataService erpOrderAdditionalDataService,
        IMeasureService measureService,
        MeasureSettings measureSettings,
        ILocalizationService localizationService,
        B2BB2CFeaturesSettings b2BB2CFeaturesSettings,
        IPriceFormatter priceFormatter,
        IErpAccountService erpAccountService,
        IErpCustomerFunctionalityService erpCustomerFunctionalityService,
        IProductService productService,
        IB2BSalesOrgPickupPointService b2BSalesOrgPickupPointService,
        IShippingService shippingService,
        IStoreContext storeContext,
        ICountryService countryService,
        IStateProvinceService stateProvinceService,
        IWorkContext workContext,
        IAddressService addressService,
        IERPSAPErrorMsgTranslationService eRPSAPErrorMsgTranslationService
    )
    {
        _orderService = orderService;
        _erpOrderItemAdditionalDataService = erpOrderItemAdditionalDataService;
        _erpOrderAdditionalDataService = erpOrderAdditionalDataService;
        _measureService = measureService;
        _measureSettings = measureSettings;
        _localizationService = localizationService;
        _b2BB2CFeaturesSettings = b2BB2CFeaturesSettings;
        _priceFormatter = priceFormatter;
        _erpAccountService = erpAccountService;
        _erpCustomerFunctionalityService = erpCustomerFunctionalityService;
        _productService = productService;
        _b2BSalesOrgPickupPointService = b2BSalesOrgPickupPointService;
        _shippingService = shippingService;
        _storeContext = storeContext;
        _countryService = countryService;
        _stateProvinceService = stateProvinceService;
        _workContext = workContext;
        _addressService = addressService;
        _eRPSAPErrorMsgTranslationService = eRPSAPErrorMsgTranslationService;
    }

    public async Task<ErpOrderDetailsModel> PrepareB2BOrderItemDataModelListModelAsync(
        int nopOrderId,
        List<string> itemIds
    )
    {
        var currentCustomer = await _workContext.GetCurrentCustomerAsync();
        var b2BOrderDetailsModel = new ErpOrderDetailsModel();
        var b2BAccount = await _erpAccountService.GetActiveErpAccountByCustomerIdAsync(
            currentCustomer.Id
        );

        var nopOrder = await _orderService.GetOrderByIdAsync(nopOrderId);

        var erpOrder =
            await _erpOrderAdditionalDataService.GetErpOrderAdditionalDataByNopOrderIdAsync(
                nopOrderId
            );
        var b2bOrder =
            erpOrder.ErpOrderType == ErpOrderType.B2BSalesOrder
            || erpOrder.ErpOrderType == ErpOrderType.B2BQuote
                ? erpOrder
                : null;
        var b2COrder =
            erpOrder.ErpOrderType == ErpOrderType.B2CSalesOrder
            || erpOrder.ErpOrderType == ErpOrderType.B2CQuote
                ? erpOrder
                : null;

        var baseWeight = "";
        var totalPriceWithOutSavingsExcTax = decimal.Zero;
        var b2BOnlineOrderDiscountExcTax = decimal.Zero;
        var pickupPointResponse = await _shippingService.GetPickupPointsAsync(
                null,
                address: null,
                customer: currentCustomer,
                storeId: (await _storeContext.GetCurrentStoreAsync()).Id
            );

        var pickupPointId = (await _b2BSalesOrgPickupPointService.GetAllB2BSalesOrgPickupPointsBySalesOrganisationIdAsync(b2BAccount.ErpSalesOrgId))?
            .Select(x => x.NopPickupPointId)?
            .FirstOrDefault() ?? 0;
        var pickUpPoint = pickupPointResponse?.PickupPoints?.FirstOrDefault(x => x.Id == $"{pickupPointId}");
        var country = await _countryService.GetCountryByTwoLetterIsoCodeAsync(pickUpPoint.CountryCode);
        var state = await _stateProvinceService.GetStateProvinceByAbbreviationAsync(pickUpPoint.StateAbbreviation, country?.Id);
        var lang = await _workContext.GetWorkingLanguageAsync();
        var stateName = state != null ? await _localizationService.GetLocalizedAsync(state, x => x.Name, lang?.Id) : string.Empty;
        var countryName = country != null ? await _localizationService.GetLocalizedAsync(country, x => x.Name, lang?.Id) : string.Empty;
        bool productForQuote = false;

        if (_b2BB2CFeaturesSettings.DisplayWeightInformation)
            baseWeight = (
                await _measureService.GetMeasureWeightByIdAsync(_measureSettings.BaseWeightId)
            )?.Name;

        if (b2BAccount != null && (b2bOrder != null || b2COrder != null) && itemIds != null)
        {
            foreach (var id in itemIds)
            {
                if (int.TryParse(id, out var itemId))
                {
                    var nopOrderItem = await _orderService.GetOrderItemByIdAsync(itemId);
                    var erpOrderItem =
                        await _erpOrderItemAdditionalDataService.GetErpOrderItemAdditionalDataByNopOrderItemIdAsync(itemId);

                    if (erpOrderItem == null)
                        continue;

                    var adjustmentValue = 0;
                    var discountPerUnitExcTax = decimal.Zero;
                    if (nopOrderItem.DiscountAmountExclTax > 0 && nopOrderItem.Quantity > 0)
                    {
                        discountPerUnitExcTax = nopOrderItem.DiscountAmountExclTax / nopOrderItem.Quantity;
                    }
                    var unitPriceWithOutDiscountExcTax = discountPerUnitExcTax + nopOrderItem.UnitPriceExclTax;
                    totalPriceWithOutSavingsExcTax += nopOrderItem.PriceExclTax + nopOrderItem.DiscountAmountExclTax - adjustmentValue;
                    b2BOnlineOrderDiscountExcTax += nopOrderItem.DiscountAmountExclTax;

                    var orderItemDataModel = new ErpOrderDetailsModel.ErpOrderItemDataModel
                    {
                        NopOrderItemId = itemId,
                        Quantity = nopOrderItem.Quantity,
                        UnitPriceWithOutDiscountExcTax = await _priceFormatter.FormatPriceAsync(unitPriceWithOutDiscountExcTax),
                        DiscountForPerUnitProductExcTax = await _priceFormatter.FormatPriceAsync(discountPerUnitExcTax),
                        ItemWeight = nopOrderItem.ItemWeight ?? decimal.Zero,
                        ItemWeightValue = _b2BB2CFeaturesSettings.DisplayWeightInformation && nopOrderItem.ItemWeight.HasValue
                                ? $"{nopOrderItem.ItemWeight:F2} {baseWeight}" : "",
                    };

                    if (erpOrderItem != null && b2bOrder != null)
                    {
                        orderItemDataModel.Id = erpOrderItem.Id;
                        orderItemDataModel.ERPSalesUoM =
                            erpOrderItem.ErpSalesUoM ?? string.Empty;
                        orderItemDataModel.ERPOrderLineStatus =
                            erpOrderItem.ErpOrderLineStatus ?? string.Empty;
                        orderItemDataModel.ERPDateRequired = erpOrderItem
                            .ErpDateRequired
                            .HasValue
                            ? erpOrderItem.ErpDateRequired.Value.ToShortDateString()
                            : string.Empty;
                        orderItemDataModel.ERPDateExpected = erpOrderItem
                            .ErpDateExpected
                            .HasValue
                            ? erpOrderItem.ErpDateExpected.Value.ToShortDateString()
                            : string.Empty;
                        orderItemDataModel.ERPDeliveryMethod =
                            erpOrderItem.ErpDeliveryMethod ?? string.Empty;
                        orderItemDataModel.ERPInvoiceNumber =
                            erpOrderItem.ErpInvoiceNumber ?? string.Empty;
                    }
                    else if (erpOrderItem != null && b2COrder != null)
                    {
                        orderItemDataModel.Id = erpOrderItem.Id;
                        orderItemDataModel.ERPSalesUoM =
                            erpOrderItem.ErpSalesUoM ?? string.Empty;
                        orderItemDataModel.ERPOrderLineStatus =
                            erpOrderItem.ErpOrderLineStatus ?? string.Empty;
                        orderItemDataModel.ERPOrderLineNumber =
                            erpOrderItem.ErpOrderLineNumber ?? string.Empty;
                        orderItemDataModel.ERPDateRequired = erpOrderItem.ErpDateRequired.HasValue
                            ? erpOrderItem.ErpDateRequired.Value.ToShortDateString() : string.Empty;
                        orderItemDataModel.ERPDateExpected = erpOrderItem.ErpDateExpected.HasValue
                            ? erpOrderItem.ErpDateExpected.Value.ToShortDateString() : string.Empty;
                        orderItemDataModel.DeliveryDate = erpOrderItem.DeliveryDate.HasValue ? erpOrderItem.DeliveryDate.Value.ToShortDateString() : string.Empty;
                        orderItemDataModel.DeliveryDate = erpOrder.DeliveryDate.HasValue
                            ? erpOrder.DeliveryDate.Value.ToShortDateString()
                            : string.Empty;
                        orderItemDataModel.ERPDeliveryMethod =
                            erpOrderItem.ErpDeliveryMethod ?? string.Empty;
                        orderItemDataModel.ERPInvoiceNumber =
                            erpOrderItem.ErpInvoiceNumber ?? string.Empty;
                        orderItemDataModel.SpecialInstructions = erpOrderItem.SpecialInstruction ?? string.Empty;
                        orderItemDataModel.SpecialInstructions =
                            erpOrder.SpecialInstructions ?? string.Empty;

                        var nopB2CWarehouse = await _shippingService.GetWarehouseByIdAsync(erpOrderItem.NopWarehouseId ?? 0);
                        orderItemDataModel.WarehouseName = nopB2CWarehouse?.Name ?? erpOrderItem.WareHouse ?? string.Empty;

                        if (nopOrder.PickupInStore)
                        {
                            orderItemDataModel.WarehouseDetails = string.Join(", ", new[]
                            {
                                pickUpPoint?.Name,
                                pickUpPoint?.Address,
                                pickUpPoint?.County,
                                pickUpPoint?.City,
                                stateName,
                                countryName
                            }.Where(part => !string.IsNullOrWhiteSpace(part)));
                        }
                        else
                        {
                            var warehouseAddress = await _addressService.GetAddressByIdAsync(nopB2CWarehouse?.AddressId ?? 0);
                            var stateProvince = await _stateProvinceService.GetStateProvinceByIdAsync(warehouseAddress?.StateProvinceId ?? 0);
                            var wCountry = await _countryService.GetCountryByIdAsync(warehouseAddress?.CountryId ?? 0);

                            orderItemDataModel.WarehouseDetails = string.Join(", ", new[]
                            {
                                nopB2CWarehouse?.Name ?? erpOrderItem.WareHouse,
                                warehouseAddress?.Address1 ?? warehouseAddress?.Address2,
                                warehouseAddress?.County,
                                warehouseAddress?.City,
                                stateProvince?.Name,
                                wCountry?.Name
                            }.Where(part => !string.IsNullOrWhiteSpace(part)));
                        }
                    }
                    var product = await _productService.GetProductByIdAsync(
                        nopOrderItem.ProductId
                    );
                    var isProductOnSpecial =
                        await _erpCustomerFunctionalityService.IsTheProductFromSpecialCategoryAsync(
                            product
                        );
                    orderItemDataModel.CustomProperties.Add(
                        B2BB2CFeaturesDefaults.ProductIsOnSpecial,
                        $"{isProductOnSpecial}"
                    );

                    if (nopOrderItem.UnitPriceExclTax == _b2BB2CFeaturesSettings.ProductQuotePrice)
                    {
                        orderItemDataModel.UnitPriceWithOutDiscountExcTax = await _localizationService.GetResourceAsync("Products.ProductForQuote");
                        productForQuote = true;
                    }
                    b2BOrderDetailsModel.Items.Add(orderItemDataModel);
                }
            }

            if (b2bOrder != null)
            {
                b2BOrderDetailsModel.ErpOrderNumber = string.IsNullOrWhiteSpace(b2bOrder.ErpOrderNumber) ? nopOrder.CustomOrderNumber : b2bOrder.ErpOrderNumber;
                b2BOrderDetailsModel.ERPOrderStatus = b2bOrder.ERPOrderStatus ?? string.Empty;
                b2BOrderDetailsModel.IsQuoteOrder = b2bOrder.ErpOrderType != ErpOrderType.B2BSalesOrder;
            }
            else if (b2COrder != null)
            {
                b2BOrderDetailsModel.ErpOrderNumber = string.IsNullOrWhiteSpace(b2COrder.ErpOrderNumber) ? nopOrder.CustomOrderNumber : b2COrder.ErpOrderNumber;
                b2BOrderDetailsModel.ERPOrderStatus = b2COrder.ERPOrderStatus ?? string.Empty;
                b2BOrderDetailsModel.IsQuoteOrder = b2COrder.ErpOrderType != ErpOrderType.B2CSalesOrder;
            }
            if (productForQuote)
            {

                b2BOrderDetailsModel.TotalPriceWithOutSavingsExcTax = await _localizationService.GetResourceAsync("Products.ProductForQuote");
                b2BOrderDetailsModel.ErpOnlineOrderDiscountExcTax = await _localizationService.GetResourceAsync("Products.ProductForQuote");
            }
            else
            {
                b2BOrderDetailsModel.TotalPriceWithOutSavingsExcTax = await _priceFormatter.FormatPriceAsync(totalPriceWithOutSavingsExcTax);
                b2BOrderDetailsModel.ErpOnlineOrderDiscountExcTax = await _priceFormatter.FormatPriceAsync(b2BOnlineOrderDiscountExcTax);
            }

            b2BOrderDetailsModel.TotalPriceWithOutSavingsExcTax =
                await _priceFormatter.FormatPriceAsync(totalPriceWithOutSavingsExcTax);
            b2BOrderDetailsModel.ErpOnlineOrderDiscountExcTax =
                await _priceFormatter.FormatPriceAsync(b2BOnlineOrderDiscountExcTax);

            if (_b2BB2CFeaturesSettings.DisplayWeightInformation)
            {
                baseWeight = await _localizationService.GetResourceAsync(
                    "B2B.TotalWeight.Custom.BaseWeight"
                );
                b2BOrderDetailsModel.TotalWeight =
                    b2BOrderDetailsModel.Items.Sum(x => x.Quantity * x.ItemWeight)
                    ?? decimal.Zero;
                b2BOrderDetailsModel.TotalWeightValue =
                    $"{b2BOrderDetailsModel.TotalWeight:F2} {baseWeight}";
            }
        }

        return b2BOrderDetailsModel;
    }

    public async Task<ErpCheckoutCompletedModel> PrepareB2BCheckoutCompletedModelAsync(
        int nopOrderId
    )
    {
        var order = await _orderService.GetOrderByIdAsync(nopOrderId);
        var b2BCheckoutCompleted = new ErpCheckoutCompletedModel();
        if (order is null)
            return b2BCheckoutCompleted;
        var erpOrder =
            await _erpOrderAdditionalDataService.GetErpOrderAdditionalDataByNopOrderIdAsync(
                nopOrderId
            );
        if (erpOrder is null)
            return b2BCheckoutCompleted;
        var b2BAccount = await _erpAccountService.GetActiveErpAccountByCustomerIdAsync(
            (await _workContext.GetCurrentCustomerAsync()).Id
        );
        if (b2BAccount != null && erpOrder != null)
        {
            b2BCheckoutCompleted.ErpOrderNumber = !string.IsNullOrWhiteSpace(
                erpOrder?.ErpOrderNumber
            )
                ? erpOrder?.ErpOrderNumber
                : order.CustomOrderNumber;
            b2BCheckoutCompleted.IsErpIntegrationSuccess =
                erpOrder?.IntegrationStatusType == IntegrationStatusType.Confirmed;
            if (erpOrder?.ErpOrderType == ErpOrderType.B2BSalesOrder)
            {
                if (b2BAccount.AllowOverspend && b2BAccount.CreditLimit < b2BAccount.CurrentBalance)
                {
                    if (_b2BB2CFeaturesSettings.IsShowOverSpendWarningText)
                    {
                        b2BCheckoutCompleted.DisplayOverSpendWarningText = true;
                        b2BCheckoutCompleted.OverSpendWarningText =
                            _b2BB2CFeaturesSettings.OverSpendWarningText;
                    }
                }
            }
            else if (
                erpOrder?.ErpOrderType == ErpOrderType.B2BQuote
                || erpOrder?.ErpOrderType == ErpOrderType.B2CQuote
            )
                b2BCheckoutCompleted.IsQuoteOrder = true;
            if (!b2BCheckoutCompleted.IsErpIntegrationSuccess)
            {
                b2BCheckoutCompleted.IntegrationError =
                    await _eRPSAPErrorMsgTranslationService.GetTranslatedAndCompleteIntegrationErrorMsgAsync(
                        (ErpOrderType)(erpOrder?.ErpOrderType ?? erpOrder?.ErpOrderType),
                        erpOrder?.IntegrationError ?? erpOrder?.IntegrationError
                    );
            }
        }
        return b2BCheckoutCompleted;
    }
}
