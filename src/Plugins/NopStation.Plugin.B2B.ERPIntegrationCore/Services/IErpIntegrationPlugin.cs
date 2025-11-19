using Nop.Services.Plugins;
using NopStation.Plugin.B2B.ERPIntegrationCore.ErpInterface;

namespace NopStation.Plugin.B2B.ERPIntegrationCore.Services
{
    public interface IErpIntegrationPlugin
        : IPlugin,
            IErpIntegrationAccountService,
            IErpIntegrationProductService,
            IErpIntegrationOrderService,
            IErpIntegrationSalesOrgService,
            IErpIntegrationShippingService,
            IErpDocumentService { }
}
