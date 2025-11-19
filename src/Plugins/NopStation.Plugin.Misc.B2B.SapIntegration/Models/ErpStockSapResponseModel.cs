namespace NopStation.Plugin.Misc.B2B.SapIntegration.Models;
public class ErpStockSapResponseModel
{
    public string Company { get; set; }
    public string StockCode { get; set; }
    public string Warehouse { get; set; }
    public decimal? QtyOnHand { get; set; }
    public DateTime? LastChangeDate { get; set; }
}
