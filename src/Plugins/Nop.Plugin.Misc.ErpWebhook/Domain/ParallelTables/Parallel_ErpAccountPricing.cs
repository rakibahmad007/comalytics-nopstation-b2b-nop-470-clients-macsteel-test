namespace Nop.Plugin.Misc.ErpWebhook.Domain.ParallelTables;

public class Parallel_ErpAccountPricing : BaseParallelEntity
{
    public string AccountNumber { get; set; }
    public string SalesOrganisationCode { get; set; }
    public string Sku { get; set; }
    public decimal? Price { get; set; }
    public decimal? ListPrice { get; set; }
    public decimal? DiscountPerc { get; set; }
    public string PricingNotes { get; set; }
}
