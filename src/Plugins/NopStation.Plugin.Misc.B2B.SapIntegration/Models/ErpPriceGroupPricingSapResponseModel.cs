namespace NopStation.Plugin.Misc.B2B.SapIntegration.Models;
public class ErpPriceGroupPricingSapResponseModel
{
    public string StockCode { get; set; }
    public decimal? Price { get; set; }
    public string PriceCode { get; set; }
    public string Uom { get; set; }
    public decimal? BasePrice { get; set; }
    public decimal? DiscountPercentage { get; set; }
}
