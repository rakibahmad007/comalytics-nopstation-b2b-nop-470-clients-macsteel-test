using System;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace NopStation.Plugin.B2B.B2BB2CFeatures.Areas.Admin.Models.SpecialIncludeExcludes
{
    public record SpecialIncludeExcludeModel : BaseNopEntityModel
    {
        [NopResourceDisplayName("Plugin.Misc.NopStation.Plugin.B2B.B2BB2CFeatures.SpecialIncludeExcludeModel.Fields.ErpAccountId")]
        public int ErpAccountId { get; set; }

        [NopResourceDisplayName("Plugin.Misc.NopStation.Plugin.B2B.B2BB2CFeatures.SpecialIncludeExcludeModel.Fields.Products")]
        public int ProductId { get; set; }
        public int ErpSalesOrgId { get; set; }
        public int SpecialTypeId { get; set; }

        [NopResourceDisplayName("Plugin.Misc.NopStation.Plugin.B2B.B2BB2CFeatures.SpecialIncludeExcludeModel.Fields.AccountName")]
        public string AccountName { get; set; }

        [NopResourceDisplayName("Plugin.Misc.NopStation.Plugin.B2B.B2BB2CFeatures.SpecialIncludeExcludeModel.Fields.AccountNumber")]
        public string AccountNumber { get; set; }

        [NopResourceDisplayName("Plugin.Misc.NopStation.Plugin.B2B.B2BB2CFeatures.SpecialIncludeExcludeModel.Fields.ErpAccountSalesOrganisation")]
        public string SalesOrgCode { get; set; }

        [NopResourceDisplayName("Plugin.Misc.NopStation.Plugin.B2B.B2BB2CFeatures.SpecialIncludeExcludeModel.Fields.ErpAccountSalesOrganisation.Name")]
        public string SalesOrgName { get; set; }

        [NopResourceDisplayName("Plugin.Misc.NopStation.Plugin.B2B.B2BB2CFeatures.SpecialIncludeExcludeModel.Fields.IsActive")]
        public bool IsActive { get; set; }

        [NopResourceDisplayName("Admin.Catalog.Products.Fields.Name")]
        public string ProductName { get; set; }

        [NopResourceDisplayName("Admin.Catalog.Products.Fields.Sku")]
        public string ProductSKU { get; set; }
        public DateTime LastUpdate { get; set; }
    }
}
