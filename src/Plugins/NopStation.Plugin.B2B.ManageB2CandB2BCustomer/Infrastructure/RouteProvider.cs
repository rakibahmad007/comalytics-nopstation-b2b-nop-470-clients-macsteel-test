using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Nop.Web.Framework.Mvc.Routing;
using Nop.Web.Infrastructure;

namespace NopStation.Plugin.B2B.ManageB2CandB2BCustomer.Infrastructure;
public partial class RouteProvider : BaseRouteProvider, IRouteProvider
{
    public void RegisterRoutes(IEndpointRouteBuilder endpointRouteBuilder)
    {
        var lang = GetLanguageRoutePattern();

        endpointRouteBuilder.MapControllerRoute(name: "CreateShippingAddress",
            pattern: $"{lang}/CreateShippingAddress/",
            defaults: new { controller = "B2CShipToAddress", action = "Create" });

        endpointRouteBuilder.MapControllerRoute("CheckGeoFencingResponse", "CheckGeoFencingResponse/",
            new { controller = "B2CShipToAddress", action = "CheckGeoFencingResponse" });

        endpointRouteBuilder.MapControllerRoute("CheckShiptoAddressIfExist", "CheckShiptoAddressIfExist/",
            new { controller = "B2CShipToAddress", action = "CheckShiptoAddressIfExist" });

        endpointRouteBuilder.MapControllerRoute("B2CRegisterResult", "b2cregisterresult/{resultId:min(0)}",
            new { controller = "OverriddenB2BB2CCustomer", action = "B2CRegisterResult" });

        endpointRouteBuilder.MapControllerRoute("IsSalesOrgSameForOldAndNewB2CShipToAddress", "IsSalesOrgSameForOldAndNewB2CShipToAddress/",
            new { controller = "B2CShipToAddress", action = "IsSalesOrgSameForOldAndNewB2CShipToAddress" });

        endpointRouteBuilder.MapControllerRoute("UpdateDefaultShipToAddressOfB2CUser", "UpdateDefaultShipToAddressOfB2CUser/",
            new { controller = "B2CShipToAddress", action = "UpdateDefaultShipToAddressOfB2CUser" });

        endpointRouteBuilder.MapControllerRoute("DeleteB2CShipToAddress", "DeleteB2CShipToAddress/",
            new { controller = "B2CShipToAddress", action = "DeleteB2CShipToAddress" });

        endpointRouteBuilder.MapControllerRoute("IsShipToAddressOnlyOne", "IsShipToAddressOnlyOne/",
            new { controller = "B2CShipToAddress", action = "IsShipToAddressOnlyOne" });

        endpointRouteBuilder.MapControllerRoute("DeleteDefaultB2CShipToAddress", "DeleteDefaultB2CShipToAddress/",
            new { controller = "B2CShipToAddress", action = "DeleteDefaultB2CShipToAddress" });
    }

    #region Properties

    public int Priority => 2;

    #endregion
}
