namespace NopStation.Plugin.Misc.B2B.SapIntegration.Models;
public class ErpPriceSpecialPricingSapResponseModel
{
    public string Company { get; set; }
    public string StockCode { get; set; }
    public string Customer { get; set; }
    public decimal? Price { get; set; }
    public string PriceCode { get; set; }
    public string Uom { get; set; }
    public decimal? BasePrice { get; set; }
    public decimal? DiscountPercentage { get; set; }
}
