using System.Collections.Generic;

namespace Nop.Plugin.Misc.ErpWebhook.Models.ErpProduct
{
    public class ErpProductModel
    {
        public string Sku { get; set; }
        public string ManufacturerPartNumber { get; set; }
        public string ShortDescription { get; set; }
        public bool? IsSpecial { get; set; }
        public bool? IsActive { get; set; }
        public bool? IsDeleted { get; set; }
        public string FullDescription { get; set; }
        public decimal? Height { get; set; }
        public decimal? Width { get; set; }
        public decimal? Length { get; set; }
        public decimal? Weight { get; set; }
        public decimal? SellingPriceA { get; set; }
        public decimal? InStockforLocNo { get; set; }
        public IEnumerable<ErpProductCategoryModel> Categories { get; set; } = new List<ErpProductCategoryModel>();
        public List<KeyValuePair<string, string>> SpecificastionAttributes { get; set; } = new List<KeyValuePair<string, string>>();
        public string VendorName { get; set; }
        public string ManufacturerName { get; set; }
        public string ManufacturerDescription { get; set; }
    }
}
