using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Plugin.Pickup.PickupInStore.Areas.Admin.Models.B2BSalesOrgPickupPoints
{
    public record B2BSalesOrgPickupPointModel : BaseNopEntityModel
    {
        public B2BSalesOrgPickupPointModel()
        {
            AvailablePickupPoints = new List<SelectListItem>();
        }

        [NopResourceDisplayName("Plugins.Payments.B2BCustomerAccount.B2BSalesOrgPickupPoint.Fields.PickupPoint")]
        public int PickupPointId { get; set; }

        [NopResourceDisplayName("Plugins.Payments.B2BCustomerAccount.B2BSalesOrgPickupPoint.Fields.PickupPointName")]
        public string PickupPointName { get; set; }

        public int B2BSalesOrgId { get; set; }

        [NopResourceDisplayName("Plugins.Payments.B2BCustomerAccount.Fields.SalesOrganisationCode")]
        public string SalesOrganisationCode { get; set; }

        [NopResourceDisplayName("Plugins.Payments.B2BCustomerAccount.Fields.SalesOrganisationName")]
        public string SalesOrganisationName { get; set; }

        public IList<SelectListItem> AvailablePickupPoints { get; set; }
    }
}
