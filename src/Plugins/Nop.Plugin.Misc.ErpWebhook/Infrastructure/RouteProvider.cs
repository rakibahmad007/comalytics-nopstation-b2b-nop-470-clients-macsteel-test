using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Nop.Web.Framework.Mvc.Routing;

namespace Nop.Plugin.Misc.ErpWebhook.Infrastructure;

public class RouteProvider : IRouteProvider
{
    public int Priority => 1000;

    public void RegisterRoutes(IEndpointRouteBuilder endpointRouteBuilder)
    {
        //webhook routes
        endpointRouteBuilder.MapControllerRoute(ErpWebhookDefaults.ProductWebhookRoute,
            "webhook/Product", new { controller = "ErpWebhook", action = "Product" });
        endpointRouteBuilder.MapControllerRoute(ErpWebhookDefaults.OrderWebhookRoute,
            "webhook/Order", new { controller = "ErpWebhook", action = "Order" });
        endpointRouteBuilder.MapControllerRoute(ErpWebhookDefaults.PriceWebhookRoute,
            "webhook/Price", new { controller = "ErpWebhook", action = "AccountPricing" });
        endpointRouteBuilder.MapControllerRoute(ErpWebhookDefaults.StockWebhookRoute,
            "webhook/Stock", new { controller = "ErpWebhook", action = "Stock" });
        endpointRouteBuilder.MapControllerRoute(ErpWebhookDefaults.AccountWebhookRoute,
            "webhook/Account", new { controller = "ErpWebhook", action = "Account" });
        endpointRouteBuilder.MapControllerRoute(ErpWebhookDefaults.ShiptoAddressWebhookRoute,
            "webhook/ShiptoAddress", new { controller = "ErpWebhook", action = "ShiptoAddress" });
        endpointRouteBuilder.MapControllerRoute(ErpWebhookDefaults.CreditWebhookRoute,
            "webhook/Credit", new { controller = "ErpWebhook", action = "Credit" });
        endpointRouteBuilder.MapControllerRoute(ErpWebhookDefaults.DeliveryDates,
            "Webhook/DeliveryDates", new { controller = "ErpWebhook", action = "DeliveryDates" });
        endpointRouteBuilder.MapControllerRoute(ErpWebhookDefaults.ProductsImage,
            "Webhook/ProductsImage", new { controller = "ErpWebhook", action = "ProductsImage" });
    }
}
