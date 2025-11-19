using FluentValidation;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NNopStation.Plugin.B2B.B2BB2CFeatures.Services.SpecialIncludeExcludeService;
using Nop.Core.Infrastructure;
using Nop.Data;
using Nop.Services.Catalog;
using Nop.Services.Common;
using Nop.Services.Customers;
using Nop.Services.Messages;
using Nop.Services.Orders;
using Nop.Web.Controllers;
using Nop.Web.Factories;
using Nop.Web.Models.Customer;
using NopStation.Plugin.B2B.B2BB2CFeatures.ActionFilters;
using NopStation.Plugin.B2B.B2BB2CFeatures.Areas.Admin.Controllers;
using NopStation.Plugin.B2B.B2BB2CFeatures.Areas.Admin.Factories;
using NopStation.Plugin.B2B.B2BB2CFeatures.Areas.Admin.Factories.ErpDeliveryDates;
using NopStation.Plugin.B2B.B2BB2CFeatures.Contexts;
using NopStation.Plugin.B2B.B2BB2CFeatures.Controllers;
using NopStation.Plugin.B2B.B2BB2CFeatures.Factories;
using NopStation.Plugin.B2B.B2BB2CFeatures.Factories.PDF;
using NopStation.Plugin.B2B.B2BB2CFeatures.Factories.QuickOrder;
using NopStation.Plugin.B2B.B2BB2CFeatures.Helpers;
using NopStation.Plugin.B2B.B2BB2CFeatures.Services.Customers;
using NopStation.Plugin.B2B.B2BB2CFeatures.Services.ErpAccountCreditSyncFunctionality;
using NopStation.Plugin.B2B.B2BB2CFeatures.Services.ErpCustomerFunctionality;
using NopStation.Plugin.B2B.B2BB2CFeatures.Services.ErpDeliveyDates;
using NopStation.Plugin.B2B.B2BB2CFeatures.Services.ErpOrderTotalCalculationService;
using NopStation.Plugin.B2B.B2BB2CFeatures.Services.ErpPriceListDownloadTrack;
using NopStation.Plugin.B2B.B2BB2CFeatures.Services.ErpPriceSyncFunctionality;
using NopStation.Plugin.B2B.B2BB2CFeatures.Services.ERPSAPErrorMsgTranslations;
using NopStation.Plugin.B2B.B2BB2CFeatures.Services.ErpShippingService;
using NopStation.Plugin.B2B.B2BB2CFeatures.Services.ErpShoppingCartItemService;
using NopStation.Plugin.B2B.B2BB2CFeatures.Services.ErpSpecificationAttributeService;
using NopStation.Plugin.B2B.B2BB2CFeatures.Services.ErpWorkflowMessage;
using NopStation.Plugin.B2B.B2BB2CFeatures.Services.ExportManager;
using NopStation.Plugin.B2B.B2BB2CFeatures.Services.Overriden;
using NopStation.Plugin.B2B.B2BB2CFeatures.Services.PDF;
using NopStation.Plugin.B2B.B2BB2CFeatures.Services.SoltrackIntegration;
using NopStation.Plugin.B2B.B2BB2CFeatures.Services.SpecialIncludeExcludeService;
using NopStation.Plugin.B2B.B2BB2CFeatures.Validators;
using NopStation.Plugin.B2B.ERPIntegrationCore.Data.Erp;
using NopStation.Plugin.B2B.ERPIntegrationCore.Domain;
using NopStation.Plugin.B2B.ERPIntegrationCore.Services.QuickOrderServices;
using NopStation.Plugin.Misc.Core.Infrastructure;
namespace NopStation.Plugin.B2B.B2BB2CFeatures.Infrastructure;

public class NopStartup : INopStartup
{
    public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
    {
        services.AddNopStationServices("NopStation.Plugin.B2B.B2BB2CFeatures");

        services.Configure<RazorViewEngineOptions>(options =>
        {
            options.ViewLocationExpanders.Add(new ViewLocationExpander());
        });
        services.AddScoped<IRepository<ErpGroupPriceCode>, ErpRepository<ErpGroupPriceCode>>();
        services.AddScoped<IRepository<B2CMacsteelExpressShop>, ErpRepository<B2CMacsteelExpressShop>>();
        services.AddScoped<IRepository<ErpOrderItemAdditionalData>, ErpRepository<ErpOrderItemAdditionalData>>();
        services.AddScoped<IRepository<ErpOrderAdditionalData>, ErpRepository<ErpOrderAdditionalData>>();
        services.AddScoped<IRepository<ErpWarehouseSalesOrgMap>, ErpRepository<ErpWarehouseSalesOrgMap>>();
        services.AddScoped<IRepository<ErpShipToAddress>, ErpRepository<ErpShipToAddress>>();
        services.AddScoped<IRepository<B2CShoppingCartItem>, ErpRepository<B2CShoppingCartItem>>();
        services.AddScoped<IRepository<ErpNopUser>, ErpRepository<ErpNopUser>>();
        services.AddScoped<IRepository<ErpUserRegistrationInfo>, ErpRepository<ErpUserRegistrationInfo>>();
        services.AddScoped<IRepository<ErpUserFavourite>, ErpRepository<ErpUserFavourite>>();
        services.AddScoped<IRepository<ErpAccount>, ErpRepository<ErpAccount>>();
        services.AddScoped<IRepository<ErpCustomerConfiguration>, ErpRepository<ErpCustomerConfiguration>>();
        services.AddScoped<IRepository<ErpInvoice>, ErpRepository<ErpInvoice>>();
        services.AddScoped<IRepository<ErpSpecialPrice>, ErpRepository<ErpSpecialPrice>>();
        services.AddScoped<IRepository<ErpGroupPrice>, ErpRepository<ErpGroupPrice>>();
        services.AddScoped<IRepository<ERPPriceListDownloadTrack>, ErpRepository<ERPPriceListDownloadTrack>>();
        services.AddScoped<IRepository<ErpProductNotePerSalesOrg>, ErpRepository<ErpProductNotePerSalesOrg>>();
        services.AddScoped<IRepository<ErpSalesOrg>, ErpRepository<ErpSalesOrg>>();
        services.AddScoped<IRepository<B2BSalesOrgPickupPoint>, ErpRepository<B2BSalesOrgPickupPoint>>();
        services.AddScoped<IRepository<ErpSalesRep>, ErpRepository<ErpSalesRep>>();
        services.AddScoped<IRepository<ErpShiptoAddressErpAccountMap>, ErpRepository<ErpShiptoAddressErpAccountMap>>();
        services.AddScoped<IRepository<ErpSalesRepSalesOrgMap>, ErpRepository<ErpSalesRepSalesOrgMap>>();
        services.AddScoped<IRepository<ErpActivityLogs>, ErpRepository<ErpActivityLogs>>();
        services.AddScoped<IRepository<ErpDeliveryDates>, ErpRepository<ErpDeliveryDates>>();

        //register services
        services.AddScoped<ICommonHelper, CommonHelper>();
        services.AddScoped<IB2BB2CWorkContext, B2BB2CWebWorkContext>();
        services.AddScoped<ICustomerRegistrationService, B2BB2CCustomerRegistrationService>();
        services.AddScoped<ICommonHelperService, CommonHelperService>();
        services.AddScoped<IErpCustomerFunctionalityService, ErpCustomerFunctionalityService>();
        services.AddScoped<IErpPriceSyncFunctionalityService, ErpPriceSyncFunctionalityService>();
        services.AddScoped<IPriceCalculationService, OverridenPriceCalculationService>();
        services.AddScoped<IOrderTotalCalculationService, OverriddenOrderTotalCalculationService>();
        services.AddScoped<IProductService, OverridenProductService>();
        services.AddScoped<ICategoryService, OverridenCategoryService>();
        services.AddScoped<IErpSpecificationAttributeService, ErpSpecificationAttributeService>();
        services.AddScoped<IShoppingCartService, OverridenShoppingCartService>();
        services.AddScoped<IErpDeliveryDatesService, ErpDeliveryDatesService>();
        services.AddScoped<IOverriddenOrderProcessingService, OverriddenOrderProcessingService>();
        services.AddScoped<IOrderProcessingService, OverriddenOrderProcessingService>();
        services.AddScoped<IAddressService, OverridenAddressService>();
        services.AddScoped<IB2BSpecialIncludeExcludeService, B2BSpecialIncludeExcludeService>();
        services.AddScoped<IB2BExportImportManager, B2BExportImportManager>();
        services.AddScoped<IERPPriceListDownloadTrackService, ERPPriceListDownloadTrackService>();
        services.AddScoped<IERPExportImportManager, ERPExportImportManager>();
        services.AddScoped<IERPSAPErrorMsgTranslationService, ERPSAPErrorMsgTranslationService>();
        services.AddScoped<IErpShoppingCartItemService, ErpShoppingCartItemService>();

        services.AddScoped<IB2BRegisterModelFactory, B2BRegisterModelFactory>();
        services.AddScoped<IErpShipToAddressModelFactory, ErpShipToAddressModelFactory>();
        services.AddScoped<IErpAccountModelFactory, ErpAccountModelFactory>();
        services.AddScoped<IErpSalesOrgModelFactory, ErpSalesOrgModelFactory>();
        services.AddScoped<IErpNopUserModelFactory, ErpNopUserModelFactory>();
        services.AddScoped<IErpInvoiceModelFactory, ErpInvoiceModelFactory>();
        services.AddScoped<ISalesRepUserModelFactory, SalesRepUserModelFactory>();
        services.AddScoped<IErpDeliveyDatesModelFactory, ErpDeliveyDatesModelFactory>();

        services.AddScoped<CustomerController, B2BB2CCustomerController>();
        services.AddScoped<IErpGroupPriceCodeModelFactory, ErpGroupPriceCodeModelFactory>();
        services.AddScoped<IErpGroupPriceModelFactory, ErpGroupPriceModelFactory>();
        services.AddScoped<IErpSpecialPriceModelFactory, ErpSpecialPriceModelFactory>();
        services.AddScoped<IErpOrderModelFactory, ErpOrderModelFactory>();
        services.AddScoped<IErpSalesRepModelFactory, ErpSalesRepModelFactory>();

        services.AddScoped<IErpAccountPublicModelFactory, ErpAccountPublicModelFactory>();
        services.AddScoped<ICustomerModelFactory, Factories.OverridenCustomerModelFactory>();
        services.AddScoped<Nop.Web.Areas.Admin.Factories.ICustomerModelFactory, Areas.Admin.Factories.OverridenCustomerModelFactory>();
        services.AddScoped<IErpLogsModelFactory, ErpLogsModelFactory>();
        services.AddScoped<IErpCheckoutModelFactory, ErpCheckoutModelFactory>();
        services.AddScoped<IErpOrderItemModelFactory, ErpOrderItemModelFactory>();
        services.AddScoped<IErpProductModelFactory, ErpProductModelFactory>();

        services.AddScoped<IErpRegistrationApplicationModelFactory, ErpRegistrationApplicationModelFactory>();
        services.AddScoped<ISpecialIncludeExcludeModelFactory, SpecialIncludeExcludeModelFactory>();
        services.AddScoped<IERPPriceListDownloadTrackFactory, ERPPriceListDownloadTrackFactory>();
        services.AddScoped<IERPSAPErrorMsgTranslationFactory, ERPSAPErrorMsgTranslationFactory>();
        services.AddScoped<IShoppingCartModelFactory, OverridenShoppingCartModelFactory>();
        services.AddScoped<IOrderModelFactory, OverridenOrderModelFactory>();
        services.AddScoped<IProductModelFactory, OverridenProductModelFactory>();

        services.AddScoped<OrderController, Controllers.OverridenOrderController>();
        services.AddScoped<ShoppingCartController, OverridenShoppingCartController>();

        services.AddScoped<CheckoutController, ErpCheckoutController>();
        services.AddScoped<Nop.Web.Areas.Admin.Controllers.ProductController, OverridenProductController>();
        services.AddScoped<Nop.Web.Areas.Admin.Controllers.CustomerController, OverridenCustomerController>();
        services.AddScoped<Nop.Web.Areas.Admin.Controllers.OrderController, Areas.Admin.Controllers.OverridenOrderController>();

        services.AddScoped<IQuickOrderTemplateService, QuickOrderTemplateService>();
        services.AddScoped<IQuickOrderItemService, QuickOrderItemService>();
        services.AddScoped<IQuickOrderTemplateModelFactory, QuickOrderTemplateModelFactory>();
        services.AddScoped<IQuickOrderItemModelFactory, QuickOrderItemModelFactory>();

        services.AddScoped<ICategoryProductsExportManager, CategoryProductsExportManager>();
        services.Configure<MvcOptions>(options =>
        {
            options.Filters.Add<ErpSalesRepActionFilterAttribute>();
            options.Filters.Add<ErpNopUserActionFilterAttribute>();
        });
        
        services.AddScoped<IErpWorkflowMessageService, ErpWorkflowMessageService>();
        services.AddScoped<IErpActivityLogsModelFactory, ErpActivityLogsModelFactory>();
        services.AddScoped<IErpShippingService, ErpShippingService>();
        services.AddScoped<IErpOrderTotalCalculationService, ErpOrderTotalCalculationService>();
        services.AddScoped<Nop.Web.Areas.Admin.Controllers.CategoryController, OverridenCategoryController>();
        services.AddScoped<ICatalogModelFactory, OverridenCatalogModelFactory>();
        services.AddScoped<IErpCategoryImageMappingModelFactory, ErpCategoryImageMappingModelFactory>();
        services.AddScoped<IB2CMacsteelExpressShopFactory, B2CMacsteelExpressShopFactory>();
        services.AddScoped<ISoltrackIntegrationService, SoltrackIntegrationService>();
        services.AddScoped<IQueuedEmailService, OverriddenQueuedEmailService>();
        services.AddScoped<IErpAccountCreditSyncFunctionality, ErpAccountCreditSyncFunctionality>();
        services.AddTransient<IValidator<ChangePasswordModel>, OverridenChangePasswordValidator>();
        services.AddTransient<IValidator<PasswordRecoveryConfirmModel>, OverridenPasswordRecoveryConfirmValidator>();
        services.AddScoped<IOverridenCustomerModelFactory, Areas.Admin.Factories.OverridenCustomerModelFactory>();

        services.AddScoped<IErpPdfModelFactory, ErpPdfModelFactory>();

        services.AddScoped<IErpPdfService, ErpPdfService>();

        services.Configure<FormOptions>(options =>
        {
            options.ValueCountLimit = int.MaxValue;
        });

        // Convert URLs to lowercase
        services.AddRouting(options => options.LowercaseUrls = true);
    }

    public void Configure(IApplicationBuilder application) { }

    public int Order => 30000;
}
