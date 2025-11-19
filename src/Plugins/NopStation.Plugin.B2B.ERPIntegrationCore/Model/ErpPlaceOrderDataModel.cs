using System;
using System.Collections.Generic;

namespace NopStation.Plugin.B2B.ERPIntegrationCore.Model;

public class ErpPlaceOrderDataModel
{
    public ErpPlaceOrderDataModel()
    {
        ShippingAddress = new ErpAddressModel();
        BillingAddress = new ErpAddressModel();
        ErpPlaceOrderItemDatas = new List<ErpPlaceOrderItemDataModel>();
    }

    public string AccountNumber { get; set; }
    public string Location { get; set; }
    public string CustomOrderNumber { get; set; }
    public string ErpOrderNumber { get; set; }
    public string RepCode { get; set; }
    public string AddressCode { get; set; }
    public ErpAddressModel ShippingAddress { get; set; }
    public ErpAddressModel BillingAddress { get; set; }
    public string CustomerName { get; set; }
    public string Notes { get; set; }
    public string CustomerReference { get; set; }
    public string DeliveryInstruction { get; set; }
    public string OrderCategory { get; set; }
    public string DeliveryMethod { get; set; }
    public string CustomerFirstName { get; set; }
    public string CustomerLastName { get; set; }
    public string CustomerPhoneNumber { get; set; }
    public string CustomerMobileNumber { get; set; }
    public string CustomerEmail { get; set; }
    public string VatNumber { get; set; }
    public string TaxNumber { get; set; }
    public string OrderType { get; set; }
    public string QuoteNumber { get; set; }
    public decimal? OrderTax { get; set; }
    public decimal? OrderSubtotalExclTax { get; set; }
    public decimal? OrderSubtotalInclTax { get; set; }
    public string CustomerCurrencyCode { get; set; }
    public DateTime? OrderDate { get; set; }
    public DateTime? DeliveryDate { get; set; }
    public DateTime? DateRequired { get; set; }
    public IList<ErpPlaceOrderItemDataModel> ErpPlaceOrderItemDatas { get; set; }
    public string AccountName { get; set; }
    public decimal ShippingAmount { get; set; }
    public string User { get; set; }
    public string CustomerNumber { get; set; }
    public string Reference { get; set; }
    public decimal? CashRounding { get; set; }
    public string Status { get; set; }
    public DateTime? QuoteExpiryDate { get; set; }
    public string Address1 { get; set; }
    public string Address2 { get; set; }
    public string Address3 { get; set; }
    public string AddressCountryCode { get; set; }
    public string AddressProvince { get; set; }
    public string PostalCode { get; set; }
    public string DelName { get; set; }
    public string DelPhone { get; set; }
    public string DelEmail { get; set; }
    public string DelAddress1 { get; set; }
    public string DelAddress2 { get; set; }
    public string DelAddress3 { get; set; }
    public string DelPostalCode { get; set; }
    public string DelCountryCode { get; set; }
    public string DelProvince { get; set; }
    public decimal ShippingFees { get; set; }
}
