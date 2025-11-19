using Nop.Web.Framework.Models;

namespace Nop.Plugin.Pickup.PickupInStore.Areas.Admin.Models.B2BSalesOrgPickupPoints
{
    public record B2BSalesOrgPickupPointSearchModel : BaseSearchModel
    {
        public B2BSalesOrgPickupPointSearchModel()
        {
            AddB2BSalesOrgPickupPointModel = new B2BSalesOrgPickupPointModel();
        }

        public int B2BSalesOrgId { get; set; }

        public B2BSalesOrgPickupPointModel AddB2BSalesOrgPickupPointModel { get; set; }
    }
}
