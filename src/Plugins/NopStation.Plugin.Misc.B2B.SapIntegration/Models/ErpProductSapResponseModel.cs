namespace NopStation.Plugin.Misc.B2B.SapIntegration.Models;
public class ErpProductSapResponseModel
{
    public string Company { get; set; }
    public string StockCode { get; set; }
    public string ProductName { get; set; }
    public string ShortDescription { get; set; }
    public string LongDescription { get; set; }
    public string CategoryName1 { get; set; }
    public string CategoryName2 { get; set; }
    public string CategoryName3 { get; set; }
    public string ManufacturerCode { get; set; }
    public string ManufacturerName { get; set; }
    public string Published { get; set; }
    public string ProductTags { get; set; }
    public string GTIN { get; set; }
    public string ManufacturerPartNumber { get; set; }
    public string VendorCode { get; set; }
    public string VendorName { get; set; }
    public string TaxCategory { get; set; }
    public string Weight { get; set; }
    public string Length { get; set; }
    public string Width { get; set; }
    public string Height { get; set; }
    public string PrefilterFacet { get; set; }
    public string UnitOfMeasure { get; set; }
    public string Colour { get; set; }
    public string Size { get; set; }
    public string Thickness { get; set; }
    public DateTime? LastChangeDate { get; set; }
    public decimal? ConvFact { get; set; }
    public string ConvMulDiv { get; set; }
    public int VariableWeight { get; set; }
    public int? OnlinePromo { get; set; }
    public decimal? UnitCost { get; set; }
}
