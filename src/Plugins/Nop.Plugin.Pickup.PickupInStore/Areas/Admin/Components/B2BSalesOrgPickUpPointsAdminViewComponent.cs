using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Nop.Plugin.Pickup.PickupInStore.Areas.Admin.Factories;
using Nop.Plugin.Pickup.PickupInStore.Areas.Admin.Models.B2BSalesOrgPickupPoints;
using Nop.Web.Framework.Components;

namespace Nop.Plugin.Pickup.PickupInStore.Areas.Admin.Components;

[ViewComponent(Name = "B2BSalesOrgPickUpPointsAdmin")]
public class B2BSalesOrgPickUpPointsAdminViewComponent : NopViewComponent
{
    private readonly IB2BSalesOrgPickupPointFactory _b2BSalesOrgPickupPointFactory;

    public B2BSalesOrgPickUpPointsAdminViewComponent(IB2BSalesOrgPickupPointFactory b2BSalesOrgPickupPointFactory)
    {
        _b2BSalesOrgPickupPointFactory = b2BSalesOrgPickupPointFactory;
    }

    public async Task<IViewComponentResult> InvokeAsync(RouteValueDictionary data)
    {
        data.TryGetValue("additionalData", out var salesOrgId);

        if (salesOrgId is not int b2bSalesOrgId)
        {
            return Content(string.Empty);
        }

        if (b2bSalesOrgId <= 0)
            return Content(string.Empty);

        var searchModel = new B2BSalesOrgPickupPointSearchModel
        {
            B2BSalesOrgId = b2bSalesOrgId
        };

        //prepare page parameters
        searchModel.SetGridPageSize();

        await _b2BSalesOrgPickupPointFactory.PrepareB2BStorePickupPoints(searchModel.AddB2BSalesOrgPickupPointModel.AvailablePickupPoints);

        return View("~/Plugins/Pickup.PickupInStore/Areas/Admin/Views/Components/B2BSalesOrgPickUpPointsAdmin.cshtml", searchModel);
    }
}
