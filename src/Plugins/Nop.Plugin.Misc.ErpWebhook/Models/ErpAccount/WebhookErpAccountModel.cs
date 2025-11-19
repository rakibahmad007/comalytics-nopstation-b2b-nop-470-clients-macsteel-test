using System.Collections.Generic;
using Nop.Core;

namespace Nop.Plugin.Misc.ErpWebhook.Models.ErpAccount
{
    public class WebhookErpAccountModel : BaseEntity
    {
        public string AccountNumber { get; set; }
        public string AccountName { get; set; }
        public string SalesOrganisationCode { get; set; }
        public int B2BSalesOrganisationId { get; set; }
        public string IsActive { get; set; }
        public string IsDeleted { get; set; }
        public int B2BAccountStatusType { get; set; }
        public string PrefilterFacets { get; set; }
        public string VatNumber { get; set; }
        public string CurrentYearSavings { get; set; }
        public string AllTimeSavings { get; set; }
        public string B2BPriceGroupCode { get; set; }
        public string OverrideBackOrderingConfigSetting { get; set; }
        public string AllowAccountsBackOrdering { get; set; }
        public string AllowSwitchSalesOrg { get; set; }
        public string AllowOverspend { get; set; }
        public List<KeyValuePair<string, string>> Attributes { get; set; }
        public decimal? PerscentageOfStockAllowed { get; set; }
        public string OverrideStockDisplayConfig { get; set; }
        public int StockDisplayFormatTypeId { get; set; }

        //Credit limit
        public decimal CreditLimit { get; set; }
        public decimal? CreditLimitUsed { get; set; }
        public decimal? CreditLimitAvailable { get; set; }
        public decimal? Balance { get; set; }
        public decimal? CurrentBalance { get; set; }
        public string PaymentTypeCode { get; set; }
        public string PaymentTermCode { get; set; }
        public string PaymentTermDescription { get; set; }

        //Billing address
        public string BillingCountry { get; set; }
        public string Email { get; set; }
        public string BillingAddress1 { get; set; }
        public string BillingAddress2 { get; set; }
        public string BillingCity { get; set; }
        public string BillingProvince { get; set; }
        public string BillingPostalCode { get; set; }
        public string BillingPhonenum { get; set; }
    }
}
