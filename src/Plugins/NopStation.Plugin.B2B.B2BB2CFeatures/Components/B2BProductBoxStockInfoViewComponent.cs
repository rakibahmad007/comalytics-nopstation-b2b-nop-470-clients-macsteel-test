using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Core.Domain.Catalog;
using Nop.Services.Catalog;
using Nop.Services.Security;
using Nop.Web.Framework.Components;
using Nop.Web.Models.Catalog;
using NopStation.Plugin.B2B.B2BB2CFeatures.Model;
using NopStation.Plugin.B2B.B2BB2CFeatures.Services.ErpCustomerFunctionality;
using NopStation.Plugin.B2B.B2BB2CFeatures.Services.ErpSpecificationAttributeService;
using NopStation.Plugin.B2B.ERPIntegrationCore.Infrastructure;

namespace NopStation.Plugin.B2B.B2BB2CFeatures.Components;
public class B2BProductBoxStockInfoViewComponent : NopViewComponent
{
    private readonly IWorkContext _workContext;
    private readonly IProductService _productService;
    private readonly IPermissionService _permissionService;
    private readonly B2BB2CFeaturesSettings _b2BB2CFeaturesSettings;
    private readonly IErpCustomerFunctionalityService _erpCustomerFunctionalityService;
    private readonly IErpSpecificationAttributeService _erpSpecificationAttributeService;

    public B2BProductBoxStockInfoViewComponent(
        IWorkContext workContext, 
        IProductService productService, 
        IPermissionService permissionService, 
        B2BB2CFeaturesSettings b2BB2CFeaturesSettings, 
        IErpCustomerFunctionalityService erpCustomerFunctionalityService, 
        IErpSpecificationAttributeService erpSpecificationAttributeService)
    {
        _workContext = workContext;
        _productService = productService;
        _permissionService = permissionService;
        _b2BB2CFeaturesSettings = b2BB2CFeaturesSettings;
        _erpCustomerFunctionalityService = erpCustomerFunctionalityService;
        _erpSpecificationAttributeService = erpSpecificationAttributeService;
    }

    public async Task<IViewComponentResult> InvokeAsync(string widgetZone, object additionalData)
    {
        var productOverviewModel = additionalData as ProductOverviewModel;
        if (productOverviewModel == null)
            return Content(string.Empty);

        var erpAccount = await _erpCustomerFunctionalityService.GetActiveErpAccountByCustomerAsync(
            await _workContext.GetCurrentCustomerAsync()
        );
        if (erpAccount == null)
            return Content(string.Empty);

        var product = await _productService.GetProductByIdAsync(productOverviewModel.Id);
        if (product == null)
            return Content(string.Empty);

        productOverviewModel.CustomProperties.TryGetValue(B2BB2CFeaturesDefaults.ProductOverviewModelStockAvailabilityKey, out var stockAvailability);

        var model = new B2BProductBoxStockInfoModel
        {
            ProductId = productOverviewModel.Id,
            StockAvailability = stockAvailability?.ToString() ?? await _productService.FormatStockMessageAsync(product, ""),
            UOM = await _erpSpecificationAttributeService
                        .GetProductUOMByProductIdAndSpecificationAttributeId(productOverviewModel.Id, _b2BB2CFeaturesSettings.UnitOfMeasureSpecificationAttributeId) ?? string.Empty,
        };

        if (await _permissionService.AuthorizeAsync(ErpPermissionProvider.DisplayB2BStock) &&
            product.ManageInventoryMethod == ManageInventoryMethod.ManageStock &&
            product.BackorderMode == BackorderMode.NoBackorders &&
            product.AllowBackInStockSubscriptions &&
            await _productService.GetTotalStockQuantityAsync(product) <= 0)
        {
            model.DisplayBackInStockSubscription = true;
        }

        return View(model);
    }
}