using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NopStation.Plugin.B2B.B2BB2CFeatures.Areas.Admin.Models;
public class ErpAccountExportImportModel
{
    public string Id { get; set; }
    public string AccountNumber { get; set; }
    public string AccountName { get; set; }
    public string SalesOrganisationCode { get; set; }
    public string BillingFirstName { get; set; }
    public string BillingLastName { get; set; }
    public string BillingEmail { get; set; }
    public string BillingCompany { get; set; }
    public string BillingCountry { get; set; }
    public string BillingStateProvince { get; set; }
    public string BillingCity { get; set; }
    public string BillingAddress1 { get; set; }
    public string BillingAddress2 { get; set; }
    public string BillingSuburb { get; set; }
    public string BillingZipPostalCode { get; set; }
    public string BillingPhoneNumber { get; set; }
    public string VatNumber { get; set; }
    public string CreditLimit { get; set; }
    public string CurrentBalance { get; set; }
    public string AllowOverspend { get; set; }
    public string PriceGroupCode { get; set; }
    public string PreFilterFacets { get; set; }
    public string PaymentTypeCode { get; set; }
    public string OverrideBackOrderingConfigSetting { get; set; }
    public string AllowAccountsBackOrdering { get; set; }
    public string OverrideAddressEditOnCheckoutConfigSetting { get; set; }
    public string AllowAccountsAddressEditOnCheckout { get; set; }
    public string StockDisplayFormatTypeId { get; set; }
    public string ErpAccountStatusTypeId { get; set; }
    public string PercentageOfStockAllowed { get; set; }
    public string LastAccountRefresh { get; set; }
    public string LastPriceRefresh { get; set; }
    public string IsActive { get; set; }
    public string IsDefaultPaymentAccount { get; set; }
}
