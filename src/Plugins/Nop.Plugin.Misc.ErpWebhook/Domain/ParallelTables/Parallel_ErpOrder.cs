using System;

namespace Nop.Plugin.Misc.ErpWebhook.Domain.ParallelTables;

public class Parallel_ErpOrder : BaseParallelEntity
{
    public string AccountNumber { get; set; }
    public string SalesOrganisationCode { get; set; }

    //[SalesOrder = 10, Quote = 20, B2CSalesOrder = 30, B2CQuote = 40]
    public string OrderType { get; set; }
    public DateTime? OrderDate { get; set; }
    public DateTime? QuoteExpiryDate { get; set; }
    public decimal TotalExcl { get; set; }
    public decimal TotalIncl { get; set; }
    public string CustomerReference { get; set; }

    //[It can be empty or nop order number with prefix]
    public string CustomNopOrderNumber { get; set; }
    public string OrderNumber { get; set; }

    //[Values: "Processing", "Released", "Pending Approval", "Confirmed", "Failed", "Approved", "Rejected"]
    public string ERPOrderStatus { get; set; }
    public decimal VAT { get; set; }
    public decimal ShippingFees { get; set; }
    public DateTime? DeliveryDate { get; set; }

    //[Previously it was only being create while order create from nop site.]
    public string SpecialInstructions { get; set; }

    //ShippingAddress
    public string ShippingName { get; set; }
    public string ShippingPhone { get; set; }
    public string ShippingEmail { get; set; }
    public string ShippingAddress1 { get; set; }
    public string ShippingAddress2 { get; set; }
    public string ShippingCity { get; set; }
    public string ShippingPostalCode { get; set; }
    public string ShippingCountryCode { get; set; }
    public string ShippingProvince { get; set; }

    //BillingAddress
    public string BillingName { get; set; }
    public string BillingPhone { get; set; }
    public string BillingEmail { get; set; }
    public string BillingAddress1 { get; set; }
    public string BillingAddress2 { get; set; }
    public string BillingCity { get; set; }
    public string BillingPostalCode { get; set; }
    public string BillingCountryCode { get; set; }
    public string BillingProvince { get; set; }
    public string DetailLinesJson { get; set; }
}
