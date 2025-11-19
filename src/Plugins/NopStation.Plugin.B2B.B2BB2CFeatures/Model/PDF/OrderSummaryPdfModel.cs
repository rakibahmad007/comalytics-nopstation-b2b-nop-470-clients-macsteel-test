using System;
using Nop.Web.Framework.Models;
using NopStation.Plugin.B2B.ERPIntegrationCore.Enums;

namespace NopStation.Plugin.B2B.B2BB2CFeatures.Model.PDF;

public partial record OrderSummaryPdfModel : BaseNopEntityModel
{
    public string OrderNumber { get; set; }
    public string CustomOrderNumber { get; set; }
    public DateTime OrderDate { get; set; }
    public string OrderStatus { get; set; }
    public string PaymentMethod { get; set; }
    public string PaymentStatus { get; set; }
    public string ShippingMethod { get; set; }
    public string ShippingStatus { get; set; }
    public decimal OrderSubtotal { get; set; }
    public string OrderSubtotalFormatted { get; set; }
    public decimal TotalDiscount { get; set; }
    public string TotalDiscountFormatted { get; set; }
    public decimal ShippingCost { get; set; }
    public string ShippingCostFormatted { get; set; }
    public decimal TaxAmount { get; set; }
    public string CashRounding { get; set; }
    public string TaxAmountFormatted { get; set; }
    public decimal OrderTotal { get; set; }
    public string OrderTotalFormatted { get; set; }
    public string CurrencyCode { get; set; }
    public string OrderNotes { get; set; }
    public string ErpOrderType { get; set; }

    // Additional fields for comprehensive order totals
    public string SpecialInstructions { get; set; }
    public decimal OrderSubtotalInclTax { get; set; }
    public string OrderSubtotalInclTaxFormatted { get; set; }
    public decimal OrderSubtotalDiscountExclTax { get; set; }
    public string OrderSubtotalDiscountExclTaxFormatted { get; set; }
    public decimal OrderSubtotalDiscountInclTax { get; set; }
    public string OrderSubtotalDiscountInclTaxFormatted { get; set; }
    public decimal ShippingCostInclTax { get; set; }
    public string ShippingCostInclTaxFormatted { get; set; }
    public decimal PaymentMethodAdditionalFeeExclTax { get; set; }
    public string PaymentMethodAdditionalFeeExclTaxFormatted { get; set; }
    public decimal PaymentMethodAdditionalFeeInclTax { get; set; }
    public string PaymentMethodAdditionalFeeInclTaxFormatted { get; set; }
    public string TaxRates { get; set; }
    public string GiftCardUsageHistory { get; set; }
    public int? RewardPointsUsed { get; set; }
    public decimal? RewardPointsAmount { get; set; }
    public string RewardPointsAmountFormatted { get; set; }

    public DateTime? DeliveryDate { get; set; }
    public ErpOrderType OrderType { get; set; }
    public string CustomerReference { get; set; }
    public bool PickupInStore { get; set; }
    public string SalesRepName { get; set; }
}
