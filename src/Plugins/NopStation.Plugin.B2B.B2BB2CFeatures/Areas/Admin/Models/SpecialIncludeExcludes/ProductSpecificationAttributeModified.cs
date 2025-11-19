namespace NopStation.Plugin.B2B.B2BB2CFeatures.Areas.Admin.Models.SpecialIncludeExcludes
{
    public class ProductSpecificationAttributeModified
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public int SpecificationAttributeOptionId { get; set; }
        public bool AllowFiltering { get; set; }
        public bool ShowOnProductPage { get; set; }
        public bool Operation { get; set; }
    }
}
