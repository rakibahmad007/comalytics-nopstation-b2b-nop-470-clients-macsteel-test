using System;

namespace NopStation.Plugin.B2B.ERPIntegrationCore.Model;

public class ErpPlaceOrderItemDataModel
{
    public string Sku { get; set; }
    public string BatchCode { get; set; }
    public string Description { get; set; }
    public decimal? Quantity { get; set; }
    public string UnitOfMeasure { get; set; }
    public string SpecialInstruction { get; set; }
    public decimal? UnitPriceExclTax { get; set; }
    public decimal? UnitPriceInclTax { get; set; }
    public decimal? UnitQuantityDiscount { get; set; }
    public decimal Discount { get; set; }
    public decimal TotalQuantityDiscount { get; set; }
    public decimal? DiscountPercentage { get; set; }
    public decimal? DiscountAmountInclTax { get; set; }
    public decimal? DiscountAmountExclTax { get; set; }
    public decimal? PriceExclTax { get; set; }
    public decimal? PriceInclTax { get; set; }
    public decimal? GrossPrice { get; set; }
    public decimal? GrossPrice_INCVAT { get; set; }
    public string SerialNumber { get; set; }
    public DateTime? DeliveryDate { get; set; }
    public DateTime? DateRequired { get; set; }
    public DateTime? DateExpected { get; set; }
    public string WarehouseCode { get; set; }
    public string ErpOrderLineNumber { get; set; }
    public string ErpSalesUoM { get; set; }
    public string ErpOrderLineStatus { get; set; }
    public string ERPLineNumber { get; set; }
    public int? LineNo { get; set; }
    public decimal? Weight { get; set; }
    public string DeliveryMethod { get; set; }
    public string InvoiceNumber { get; set; }
}
