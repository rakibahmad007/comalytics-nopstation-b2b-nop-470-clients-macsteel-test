using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Plugin.Shipping.B2CShipping.Areas.Admin.Models
{
    public record ConfigurationModel : BaseSearchModel
    {

        public ConfigurationModel()
        {
            AvailableCustomerRoles = new List<SelectListItem>();
        }

        public int ActiveStoreScopeConfiguration { get; set; }

        [NopResourceDisplayName("Plugins.Payments.B2CShipping.Fields.AllowedRoleIds")]
        public IList<int> AllowedRoleIds { get; set; }
        public bool AllowedRoleIds_OverrideForStore { get; set; }

        public IList<SelectListItem> AvailableCustomerRoles { get; set; }

    }
}