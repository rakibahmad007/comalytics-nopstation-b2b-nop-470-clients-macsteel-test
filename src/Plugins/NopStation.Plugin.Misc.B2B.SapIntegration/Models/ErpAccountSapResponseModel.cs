namespace NopStation.Plugin.Misc.B2B.SapIntegration.Models;
public class ErpAccountSapResponseModel
{
    public string Company { get; set; }
    public string Customer { get; set; }
    public string Name { get; set; }
    public string VatNumber { get; set; }
    public decimal? CreditLimit { get; set; }
    public decimal? CurrentBalance { get; set; }
    public decimal? AvailableCredit { get; set; }
    public decimal? LastPaymentAmount { get; set; }
    public DateTime? LastPaymentDate { get; set; }
    public bool AllowOverspend { get; set; }
    public string PriceGroupCode { get; set; }
    public string PrefilterFacet { get; set; }
    public string PaymentTypeCode { get; set; }
    public bool AllowbackOrdering { get; set; }
    public bool AllowAddressChangeOnCheckout { get; set; }
    public bool AccountStatus { get; set; }
    public bool IsActive { get; set; }
    public bool IsDeleted { get; set; }
    public string PaymentTermsCode { get; set; }
    public string PaymentTermsDescription { get; set; }
    public string BillingName { get; set; }
    public string BillingSurname { get; set; }
    public string BillingEmail { get; set; }
    public string BillingCompanyName { get; set; }
    public string BillingCountry { get; set; }
    public string BillingProvince { get; set; }
    public string BillingCity { get; set; }
    public string BillingAddress1 { get; set; }
    public string BillingAddress2 { get; set; }
    public string BillingPostalCode { get; set; }
    public string BillingPhoneNumber { get; set; }
    public bool WebItem { get; set; }
    public DateTime? LastChanged { get; set; }
}
