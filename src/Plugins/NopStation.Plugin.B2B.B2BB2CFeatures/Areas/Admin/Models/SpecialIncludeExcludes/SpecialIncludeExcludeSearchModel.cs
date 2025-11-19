using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace NopStation.Plugin.B2B.B2BB2CFeatures.Areas.Admin.Models.SpecialIncludeExcludes
{
    public record SpecialIncludeExcludeSearchModel : BaseSearchModel
    {
        public SpecialIncludeExcludeSearchModel()
        {
            AvailableSalesOrgs = new List<SelectListItem>();
            AvailablePublishedOptions = new List<SelectListItem>();
            AvailableStatuses = new List<SelectListItem>();
        }

        [NopResourceDisplayName(
            "Plugin.Misc.NopStation.Plugin.B2B.B2BB2CFeatures.Fields.AccountName"
        )]
        public string AccountName { get; set; }

        [NopResourceDisplayName(
            "Plugin.Misc.NopStation.Plugin.B2B.B2BB2CFeatures.Fields.AccountNumber"
        )]
        public string AccountNumber { get; set; }

        [NopResourceDisplayName(
            "Plugin.Misc.NopStation.Plugin.B2B.B2BB2CFeatures.Fields.ErpAccountSalesOrganisation"
        )]
        public int ErpSalesOrgId { get; set; }

        [NopResourceDisplayName("Plugin.Misc.NopStation.Plugin.B2B.B2BB2CFeatures.Fields.IsActive")]
        public string IsActive { get; set; }

        [NopResourceDisplayName("Admin.Catalog.Products.Fields.Published")]
        public string Published { get; set; }

        [NopResourceDisplayName("Admin.Catalog.Products.Fields.Sku")]
        public string ProductSKU { get; set; }
        public int Type { get; set; }
        public IList<SelectListItem> AvailableSalesOrgs { get; set; }
        public IList<SelectListItem> AvailablePublishedOptions { get; set; }
        public IList<SelectListItem> AvailableStatuses { get; set; }
    }
}
