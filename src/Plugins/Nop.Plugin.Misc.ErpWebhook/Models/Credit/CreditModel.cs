namespace Nop.Plugin.Misc.ErpWebhook.Models.Credit
{
    public class CreditModel
    {
        public string AccountNumber { get; set; }
        public string SalesOrganisationCode { get; set; }
        public int SalesOrgId { get; set; }
        public string CreditLimit { get; set; }
        public string CurrentBalance { get; set; }
        public string CreditLimitAvailable { get; set; }
        public string B2BAccountStatusTypeId { get; set; }
        public string LastPaymentAmount { get; set; }
        public string LastPaymentDate { get; set; }
    }

}
