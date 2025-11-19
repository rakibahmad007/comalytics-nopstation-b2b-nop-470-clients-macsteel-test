using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Plugin.Pickup.PickupInStore.Areas.Admin.Models.B2BSalesOrgPickupPoints;

namespace Nop.Plugin.Pickup.PickupInStore.Areas.Admin.Factories
{
    public interface IB2BSalesOrgPickupPointFactory
    {
        #region B2BSalesOrgPickupPoint

        Task<B2BSalesOrgPickupPointListModel> PrepareB2BSalesOrgPickupPointListModel(B2BSalesOrgPickupPointSearchModel searchModel);

        Task PrepareB2BStorePickupPoints(IList<SelectListItem> items, bool withSpecialDefaultItem = false);
        #endregion
    }
}
