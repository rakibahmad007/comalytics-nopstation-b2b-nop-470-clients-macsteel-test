namespace NopStation.Plugin.Misc.B2B.SapIntegration.Models;
public class ErpDeliveryRoutesSapResponseModel
{
    public string Company { get; set; }
    public string RouteCode { get; set; }
    public string RouteDescription { get; set; }
    public string Warehouse { get; set; }
    public string CutOffTime { get; set; }
    public DateTime? DeliveryDate1 { get; set; }
    public DateTime? DeliveryDate2 { get; set; }
    public DateTime? DeliveryDate3 { get; set; }
    public DateTime? DeliveryDate4 { get; set; }
    public DateTime? DeliveryDate5 { get; set; }
    public DateTime? DeliveryDate6 { get; set; }
    public DateTime? DeliveryDate7 { get; set; }
    public decimal? DeliveryCost { get; set; }
}
