namespace Nop.Plugin.Misc.ErpWebhook.Domain.ParallelTables;

public class Parallel_ErpProduct : BaseParallelEntity
{
    public string Sku { get; set; }
    public string ManufacturerPartNumber { get; set; }
    public string ShortDescription { get; set; }
    public bool? IsSpecial { get; set; }
    public string FullDescription { get; set; }
    public decimal? Height { get; set; }
    public decimal? Width { get; set; }
    public decimal? Length { get; set; }
    public decimal? Weight { get; set; }
    public decimal? SellingPriceA { get; set; }
    public decimal? InStockforLocNo { get; set; }
    public string CategoriesJson { get; set; }
    public string SpecificationAttributesJson { get; set; }
    public string VendorName { get; set; }
    public string ManufacturerName { get; set; }
    public string ManufacturerDescription { get; set; }
}
