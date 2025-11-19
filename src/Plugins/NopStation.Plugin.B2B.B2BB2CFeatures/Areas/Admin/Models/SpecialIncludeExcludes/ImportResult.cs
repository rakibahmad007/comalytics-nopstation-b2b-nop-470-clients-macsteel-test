using System.Collections.Generic;

namespace NopStation.Plugin.B2B.B2BB2CFeatures.Areas.Admin.Models.SpecialIncludeExcludes
{
    public class ImportResult
    {
        public ImportResult()
        {
            FailedImports = new List<ExportImportModel>();
            ProductSpecificationAttributeModifiedList = new List<ProductSpecificationAttributeModified>();
        }
        public IList<ExportImportModel> FailedImports { get; set; }
        public IList<ProductSpecificationAttributeModified> ProductSpecificationAttributeModifiedList { get; set; }
        public int GivenTotal { get; set; }
        public string Message { get; set; }
    }
}
