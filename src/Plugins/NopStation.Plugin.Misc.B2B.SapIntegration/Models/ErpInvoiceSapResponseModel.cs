namespace NopStation.Plugin.Misc.B2B.SapIntegration.Models;
public class ErpInvoiceSapResponseModel
{
    public string Company { get; set; }
    public string Customer { get; set; }
    public DateTime? TransactionDate { get; set; }
    public string DocumentNo { get; set; }
    public string DocType { get; set; }
    public string Reference { get; set; }
    public string SalesOrder { get; set; }
    public decimal? Amount { get; set; }
    public decimal? Balance { get; set; }
}
