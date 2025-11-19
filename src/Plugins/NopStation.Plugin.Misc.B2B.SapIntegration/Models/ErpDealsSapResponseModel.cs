namespace NopStation.Plugin.Misc.B2B.SapIntegration.Models;
public class ErpDealsSapResponseModel
{
    public string Company { get; set; }
    public int DealId { get; set; }
    public int Type { get; set; }
    public string TypeDesc { get; set; }
    public string Name { get; set; }
    public string HdrStockcode { get; set; }
    public int HdrQuantity { get; set; }
    public decimal? HdrAmount { get; set; }
    public string Stockcode { get; set; }
    public decimal? Amount { get; set; }
    public string Applicability { get; set; }
    public bool Published { get; set; }
}
