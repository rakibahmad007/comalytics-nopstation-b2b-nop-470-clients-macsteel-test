using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Routing.Constraints;
using Nop.Web.Framework.Mvc.Routing;
using Nop.Web.Infrastructure;

namespace NopStation.Plugin.B2B.B2BB2CFeatures.Infrastructure;

public partial class RouteProvider : BaseRouteProvider, IRouteProvider
{
    public void RegisterRoutes(IEndpointRouteBuilder endpointRouteBuilder)
    {
        var lang = GetLanguageRoutePattern();

        endpointRouteBuilder.MapControllerRoute(
            name: "QuickOrder",
            pattern: $"{lang}/Favourites/",
            defaults: new { controller = "QuickOrder", action = "QuickOrderTemplateList" }
        );

        endpointRouteBuilder.MapControllerRoute(
            name: "QuickOrderDetails",
            pattern: $"{lang}/FavouritesDetails/{{id:min(0)}}",
            defaults: new { controller = "QuickOrder", action = "QuickOrderTemplateDetails" }
        );

        endpointRouteBuilder.MapControllerRoute(
            name: "Logout",
            pattern: $"{lang}/logout/",
            defaults: new { controller = "CustomerImpersonate", action = "Logout" }
        );

        endpointRouteBuilder.MapControllerRoute(
            name: "B2BRegister",
            pattern: $"{lang}/B2BRegister/",
            defaults: new { controller = "B2BB2CCustomer", action = "B2BRegister" }
        );

        endpointRouteBuilder.MapControllerRoute(
            name: "B2CRegister",
            pattern: $"{lang}/B2CRegister/",
            defaults: new { controller = "B2BB2CCustomer", action = "B2CRegister" }
        );

        endpointRouteBuilder.MapControllerRoute(
            name: "ErpAccountCustomerRegistrationForm",
            pattern: $"{lang}/ErpAccountCustomerRegistrationApplication/",
            defaults: new
            {
                controller = "B2BB2CCustomer",
                action = "ErpAccountCustomerRegistrationForm",
            }
        );

        endpointRouteBuilder.MapControllerRoute(
            name: "ErpAccountInvoices",
            pattern: $"{lang}/MyAccount/AccountTransactions",
            defaults: new { controller = "ErpAccountPublic", action = "ErpAccountInvoices" }
        );

        endpointRouteBuilder.MapControllerRoute(
            name: "ErpAccountInfo",
            pattern: $"{lang}/MyAccount/ErpAccountInfo",
            defaults: new { controller = "ErpAccountPublic", action = "ErpAccountInfo" }
        );

        endpointRouteBuilder.MapControllerRoute(
            name: "IsCartItemsLivePriceSyncProcessing",
            pattern: $"{lang}/IsCartItemsLivePriceSyncProcessing/",
            defaults: new
            {
                controller = "ErpCheckout",
                action = "IsCartItemsLivePriceSyncProcessing",
            }
        );

        endpointRouteBuilder.MapControllerRoute(
            name: "UpdateProductQuantityValueInMiniCart-Catalog",
            pattern: $"{lang}/updateproductqtyinminicart/catalog/{{productId:min(0)}}/{{shoppingCartTypeId:min(0)}}/{{quantity:min(0)}}/",
            defaults: new
            {
                controller = "OverridenShoppingCart",
                action = "UpdateProductQuantityValueInMiniCart_Catalog",
            }
        );

        endpointRouteBuilder.MapControllerRoute(
            name: "LoadB2BCartItemData",
            pattern: $"{lang}/LoadB2BCartItemData/",
            defaults: new
            {
                controller = "OverridenShoppingCart",
                action = "LoadB2BCartItemData",
            }
        );

        endpointRouteBuilder.MapControllerRoute(
            name: "LoadB2BOrderItemData",
            pattern: $"{lang}/LoadB2BOrderItemData/",
            defaults: new { controller = "ErpCheckout", action = "LoadB2BOrderItemData" }
        );

        endpointRouteBuilder.MapControllerRoute(
            name: "CheckCartItemQuotePriceChangeWarning",
            pattern: $"{lang}/CheckCartItemQuotePriceChangeWarning/",
            defaults: new
            {
                controller = "OverridenShoppingCart",
                action = "CheckCartItemQuotePriceChangeWarning",
            }
        );

        endpointRouteBuilder.MapControllerRoute(
            name: "CurrentCartItemsLivePriceCheck",
            pattern: $"{lang}/CurrentCartItemsLivePriceCheck/",
            defaults: new
            {
                controller = "HandleLiveErpCall",
                action = "CurrentCartItemsLivePriceCheck",
            }
        );

        endpointRouteBuilder.MapControllerRoute(
            name: "CurrentCartItemsLiveStockCheck",
            pattern: $"{lang}/CurrentCartItemsLiveStockCheck/",
            defaults: new
            {
                controller = "HandleLiveErpCall",
                action = "CurrentCartItemsLiveStockCheck",
            }
        );

        endpointRouteBuilder.MapControllerRoute(
            name: "QuoteOrder",
            pattern: $"{lang}/QuoteOrder/",
            defaults: new { controller = "QuoteOrder", action = "Index" }
        );

        endpointRouteBuilder.MapControllerRoute(
            name: "ErpQuoteOrderList",
            pattern: $"{lang}/MyAccount/Quotes",
            defaults: new { controller = "ErpAccountPublic", action = "ErpAccountQuoteOrders" }
        );

        endpointRouteBuilder.MapControllerRoute(
            name: "IsItemsInCart",
            pattern: $"{lang}/IsItemsInCart/",
            defaults: new { controller = "OverridenOrder", action = "IsItemsInCart" }
        );

        endpointRouteBuilder.MapControllerRoute(
            name: "B2BClearCart",
            pattern: $"{lang}/B2BClearCart/",
            defaults: new { controller = "OverridenShoppingCart", action = "ClearCart" }
        );

        endpointRouteBuilder.MapControllerRoute(
            name: "CustomerImpersonateList",
            pattern: $"{lang}/CustomerImpersonate/List",
            defaults: new { controller = "CustomerImpersonate", action = "List" }
        );

        endpointRouteBuilder.MapControllerRoute(
            name: "ErpAccountOrders",
            pattern: $"{lang}/MyAccount/Orders",
            defaults: new { controller = "ErpAccountPublic", action = "ErpAccountOrders" }
        );

        endpointRouteBuilder.MapControllerRoute(
            name: "LoadErpAccountInfoFromErp",
            pattern: $"{lang}/LoadErpAccountInfoFromErp",
            defaults: new
            {
                controller = "ErpAccountPublic",
                action = "LoadErpAccountInfoFromErp",
            }
        );

        endpointRouteBuilder.MapControllerRoute(
            name: "DownloadInvoice",
            pattern: $"{lang}/ErpAccountPublic/DownloadInvoice/{{invoiceId:min(0)}}",
            defaults: new { controller = "ErpAccountPublic", action = "DownloadInvoice" }
        );

        endpointRouteBuilder.MapControllerRoute(
            name: "GetDeliveryDatesBySuburbOrCity",
            pattern: $"{lang}/GetERPDeliveryDates/",
            defaults: new { controller = "ErpCheckout", action = "GetERPDeliveryDates" }
        );

        endpointRouteBuilder.MapControllerRoute(
            name: "GetCountryTwoLetterIsoCode",
            pattern: $"GetCountryTwoLetterIsoCode/{{code?}}",
            defaults: new
            {
                controller = "B2BB2CCustomer",
                action = "GetCountryTwoLetterIsoCode",
            },
            constraints: new { httpMethod = new HttpMethodRouteConstraint("GET") }
        );

        endpointRouteBuilder.MapControllerRoute(
            name: "IsHideAddToCart",
            pattern: $"{lang}/IsHideAddToCart/",
            defaults: new { controller = "ErpAccountPublic", action = "IsHideAddToCart" }
        );

        endpointRouteBuilder.MapControllerRoute(
            name: "IsShowAddToCart",
            pattern: $"{lang}/IsShowAddToCart/",
            defaults: new { controller = "ErpAccountPublic", action = "IsShowAddToCart" }
        );

        endpointRouteBuilder.MapControllerRoute(
            name: "ErpOrderInvoiceList",
            pattern: $"{lang}/erporderinvoicelist/",
            defaults: new { controller = "ErpAccountPublic", action = "ErpOrderInvoiceList" }
        );

        endpointRouteBuilder.MapControllerRoute(
            name: "GetPasswordRequirements",
            pattern: $"{lang}/GetPasswordRequirements/",
            defaults: new
            {
                controller = "ErpAccountPublic",
                action = "GetPasswordRequirements",
            }
        );

        endpointRouteBuilder.MapControllerRoute(
            name: "GetAccountStatementList",
            pattern: $"{lang}/GetAccountStatementList/",
            defaults: new
            {
                controller = "ErpAccountPublic",
                action = "GetAccountStatementList",
            }
        );

        endpointRouteBuilder.MapControllerRoute(
            name: "ExportLastNOrdersPerAccountExcel",
            pattern: $"{lang}/ExportLastNOrdersPerAccountExcel/",
            defaults: new { controller = "Export", action = "ExportLastNOrdersPerAccountExcel" }
        );

        endpointRouteBuilder.MapControllerRoute(
            name: "ExportLastNOrdersPerAccountPdf",
            pattern: $"{lang}/ExportLastNOrdersPerAccountPdf/",
            defaults: new { controller = "Export", action = "ExportLastNOrdersPerAccountPdf" }
        );

        endpointRouteBuilder.MapControllerRoute(
            name: "DownloadAccountStatement",
            pattern: $"{lang}/DownloadAccountStatement/{{accountNo}}/{{dateFrom}}/{{dateTo}}/",
            defaults: new
            {
                controller = "ErpAccountPublic",
                action = "DownloadAccountStatement",
            }
        );

        endpointRouteBuilder.MapControllerRoute(
            name: "GetAccountStatementList",
            pattern: $"{lang}/GetAccountStatementList/",
            defaults: new
            {
                controller = "ErpAccountPublic",
                action = "GetAccountStatementList",
            }
        );

        endpointRouteBuilder.MapControllerRoute(
            name: "DownloadAccountStatementByMonth",
            pattern: $"{lang}/DownloadAccountStatementByMonth/",
            defaults: new
            {
                controller = "ErpAccountPublic",
                action = "DownloadAccountStatementByMonth",
            }
        );

        endpointRouteBuilder.MapControllerRoute(
            name: "ErpCustomerConfiguration",
            pattern: $"{lang}/MyAccount/CustomerConfiguration",
            defaults: new
            {
                controller = "ErpAccountPublic",
                action = "ErpCustomerConfiguration",
            }
        );

        endpointRouteBuilder.MapControllerRoute(
            name: "LoadErpProductData",
            pattern: $"{lang}/LoadErpProductData/",
            defaults: new { controller = "ErpAccountPublic", action = "LoadErpProductData" }
        );

        endpointRouteBuilder.MapControllerRoute(
            name: "LoadErpNoItemsMsg",
            pattern: $"{lang}/LoadErpNoItemsMsg/",
            defaults: new { controller = "ErpAccountPublic", action = "LoadErpNoItemsMsg" }
        );

        endpointRouteBuilder.MapControllerRoute(
            name: "LoadProductInCartQuantity",
            pattern: $"{lang}/LoadProductInCartQuantity/",
            defaults: new
            {
                controller = "ErpAccountPublic",
                action = "LoadProductInCartQuantity",
            }
        );

        endpointRouteBuilder.MapControllerRoute(
            name: "CheckUserRegistered",
            pattern: $"{lang}/CheckUserRegistered/",
            defaults: new { controller = "ErpAccountPublic", action = "CheckUserRegistered" }
        );

        endpointRouteBuilder.MapControllerRoute(
            name: "UpdateProductQuantityValueInCart-Catalog",
            pattern: $"{lang}/updateproductqtyincart/catalog/{{productId}}/{{shoppingCartTypeId:min(0)}}/{{quantity:min(0)}}/",
            defaults: new
            {
                controller = "OverridenShoppingCart",
                action = "UpdateProductQuantityValueInCart_Catalog",
            }
        );

        endpointRouteBuilder.MapControllerRoute(
            name: "LoadErpProductLiveStock-ProductDetailsPage",
            pattern: $"{lang}/LoadErpProductLiveStock/",
            defaults: new { controller = "ErpAccountPublic", action = "LoadErpProductLiveStock" }
        );

        endpointRouteBuilder.MapControllerRoute(
            name: "ErpShippingAddresses",
            pattern: $"{lang}/MyAccount/ShippingAddresses/",
            defaults: new { controller = "ErpAccountPublic", action = "ErpShippingAddresses" }
        );

        endpointRouteBuilder.MapControllerRoute(
            name: "ErpBillingAddresses",
            pattern: $"{lang}/MyAccount/BillingAddresses/",
            defaults: new { controller = "ErpAccountPublic", action = "ErpBillingAddresses" }
        );      
        
        endpointRouteBuilder.MapControllerRoute(
            name: "LoadB2BAccountInfoWithCurrentOrderFromErp",
            pattern: $"{lang}/LoadB2BAccountInfoWithCurrentOrderFromErp/",
            defaults: new { controller = "ErpAccountPublic", action = "LoadB2BAccountInfoWithCurrentOrderFromErp" }
        );
    }

    #region Properties

    public int Priority => 1;

    #endregion
}
