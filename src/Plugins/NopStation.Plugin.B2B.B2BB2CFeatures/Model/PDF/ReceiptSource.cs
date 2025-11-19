using System.Collections.Generic;
using Nop.Services.Common.Pdf;
using NopStation.Plugin.B2B.ERPIntegrationCore.Enums;

namespace NopStation.Plugin.B2B.B2BB2CFeatures.Model.PDF;

public sealed class ReceiptSource : DocumentSource
{
    public byte[] LogoData { get; set; }
    public required ErpOrderType OrderType { get; set; }
    public string OrderNumber { get; set; }
    public string CustomOrderNumber { get; set; }
    public string SalesOrgCode { get; set; }
    public string SalesOrgName { get; set; }
    public string SalesOrgAddress { get; set; }
    public string SalesOrgSuburb { get; set; }
    public string SalesOrgPostalCode { get; set; }
    public string SalesOrgCity { get; set; }
    public string SalesOrgCountry { get; set; }
    public string CustomerName { get; set; }
    public AddressModel BillingAddress { get; set; }
    public AddressModel ShippingAddress { get; set; }
    public AddressModel PickupAddress { get; set; }

    public string OrderDate { get; set; }
    public string DeliveryDate { get; set; }
    public string CustomerReference { get; set; }
    public string SalesRepName { get; set; }
    public bool PickupInStore { get; set; }
    public string DeliverOrCollect { get; set; }
    public string SpecialInstruction { get; set; }
    public string OrderSubtotal { get; set; }
    public string ShippingCost { get; set; }
    public string CashRounding { get; set; }
    public string TaxAmount { get; set; }
    public string OrderTotal { get; set; }
    public bool IsB2b { get; set; }

    public IList<OrderItemPdfModel> OrderItems { get; set; } = [];
}

public record AddressModel : AddressPdfModel
{
    public string AccountNumber { get; set; }
    public string Suburb { get; set; }
    public string ShipToCode { get; set; }
}
