namespace Nop.Plugin.Misc.ErpWebhook.Domain.ParallelTables;

public class Parallel_ErpStock : BaseParallelEntity
{
    public string Sku { get; set; }
    public string SalesOrganisationCode { get; set; }
    public string WarehouseCode { get; set; }
    public int TotalOnHand { get; set; }
    public string UOM { get; set; }
    public decimal? Weight { get; set; }
}
