using System.Collections.Generic;
using Nop.Web.Framework.Models;

namespace NopStation.Plugin.B2B.B2BB2CFeatures.Model.PDF;

public partial record OrderPdfModel : BaseNopEntityModel
{
    public OrderPdfModel()
    {
        ErpAccountPdfModel = new ErpAccountPdfModel();
        ShippingAddressPdfModel = new ShippingAddressPdfModel();
        BillingAddressPdfModel = new AddressPdfModel();
        PickupAddressPdfModel = new AddressPdfModel();
        SalesOrgPdfModel = new SalesOrgPdfModel();
        OrderSummaryPdfModel = new OrderSummaryPdfModel();
        OrderItemPdfModelList = new List<OrderItemPdfModel>();
    }

    public bool IsB2b { get; set; }

    public ErpAccountPdfModel ErpAccountPdfModel { get; set; }
    public ShippingAddressPdfModel ShippingAddressPdfModel { get; set; }
    public AddressPdfModel PickupAddressPdfModel { get; set; }
    public AddressPdfModel BillingAddressPdfModel { get; set; }
    public SalesOrgPdfModel SalesOrgPdfModel { get; set; }
    public OrderSummaryPdfModel OrderSummaryPdfModel { get; set; }
    public IList<OrderItemPdfModel> OrderItemPdfModelList { get; set; }
}

public partial record ErpAccountPdfModel : BaseNopEntityModel
{
    public ErpAccountPdfModel()
    {
        BillingAddress = new AddressPdfModel();
    }

    public string AccountNumber { get; set; }
    public string AccountName { get; set; }
    public string Email { get; set; }
    public string PhoneNumber { get; set; }
    public string VatNumber { get; set; }
    public decimal? CreditLimit { get; set; }
    public decimal? CurrentBalance { get; set; }
    public string PaymentTypeCode { get; set; }
    public string PriceGroupCode { get; set; }
    public string BillingSuburb { get; set; }
    public AddressPdfModel BillingAddress { get; set; }
}

public partial record ShippingAddressPdfModel : BaseNopEntityModel
{
    public ShippingAddressPdfModel()
    {
        Address = new AddressPdfModel();
    }

    public string ShipToCode { get; set; }
    public string ShipToName { get; set; }
    public string DeliveryNotes { get; set; }
    public string Suburb { get; set; }
    public AddressPdfModel Address { get; set; }
}

public partial record SalesOrgPdfModel : BaseNopEntityModel
{
    public SalesOrgPdfModel()
    {
        Address = new AddressPdfModel();
    }

    public string SalesOrgCode { get; set; }
    public string SalesOrgName { get; set; }
    public string Suburb { get; set; }
    public string PhoneNumber { get; set; }
    public string Email { get; set; }
    public AddressPdfModel Address { get; set; }
}
