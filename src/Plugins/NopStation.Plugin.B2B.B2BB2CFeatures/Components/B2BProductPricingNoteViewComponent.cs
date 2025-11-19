using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Core.Domain.Directory;
using Nop.Services.Catalog;
using Nop.Services.Directory;
using Nop.Services.Security;
using Nop.Web.Framework.Components;
using NopStation.Plugin.B2B.B2BB2CFeatures.Model;
using NopStation.Plugin.B2B.B2BB2CFeatures.Services.ErpCustomerFunctionality;
using NopStation.Plugin.B2B.ERPIntegrationCore.Infrastructure;
using NopStation.Plugin.B2B.ERPIntegrationCore.Services;

namespace NopStation.Plugin.B2B.B2BB2CFeatures.Components;

public class B2BProductPricingNoteViewComponent : NopViewComponent
{
    private readonly IErpCustomerFunctionalityService _erpCustomerFunctionalityService;
    private readonly B2BB2CFeaturesSettings _b2BB2CFeaturesSettings;
    private readonly IErpSpecialPriceService _erpSpecialPriceService;
    private readonly IProductService _productService;
    private readonly IMeasureService _measureService;
    private readonly MeasureSettings _measureSettings;
    private readonly IWorkContext _workContext;
    private readonly IPermissionService _permissionService;

    public B2BProductPricingNoteViewComponent(
        IErpCustomerFunctionalityService erpCustomerFunctionalityService, 
        B2BB2CFeaturesSettings b2BB2CFeaturesSettings, 
        IErpSpecialPriceService erpSpecialPriceService, 
        IProductService productService, 
        IMeasureService measureService, 
        MeasureSettings measureSettings, 
        IWorkContext workContext, 
        IPermissionService permissionService)
    {
        _erpCustomerFunctionalityService = erpCustomerFunctionalityService;
        _b2BB2CFeaturesSettings = b2BB2CFeaturesSettings;
        _erpSpecialPriceService = erpSpecialPriceService;
        _productService = productService;
        _measureService = measureService;
        _measureSettings = measureSettings;
        _workContext = workContext;
        _permissionService = permissionService;
    }

    public async Task<IViewComponentResult> InvokeAsync(string widgetZone, object additionalData)
    {
        if (!(additionalData is int productId))
        {
            return Content(string.Empty);
        }

        var erpAccount = await _erpCustomerFunctionalityService.GetActiveErpAccountByCustomerAsync(
            await _workContext.GetCurrentCustomerAsync()
        );
        if (erpAccount == null)
            return Content(string.Empty);
        var product = await _productService.GetProductByIdAsync(productId);

        if (product == null)
            return Content(string.Empty);

        var pricingNoteModel = new B2BProductDetailsPricingNoteModel();

        if (!await _permissionService.AuthorizeAsync(ErpPermissionProvider.DisplayB2BPrices))
        {
            pricingNoteModel.PricingNote = string.Empty;
        }
        else
        {
            var erpSpecialPrice = await _erpSpecialPriceService.GetErpSpecialPricesByErpAccountIdAndNopProductIdAsync(erpAccount.Id,product.Id);

            if (erpSpecialPrice != null)
            {
                pricingNoteModel.HavePricingNote = !string.IsNullOrEmpty(erpSpecialPrice.PricingNote);

                pricingNoteModel.PricingNote = await _erpSpecialPriceService.GetProductPricingNoteByErpSpecialPriceAsync(
                        erpSpecialPrice,
                        _b2BB2CFeaturesSettings.UseProductGroupPrice,
                        erpSpecialPrice.Price == _b2BB2CFeaturesSettings.ProductQuotePrice
                );

            }
        }

        var baseWeight = (await _measureService.GetMeasureWeightByIdAsync(_measureSettings.BaseWeightId))?.Name;

        if (_b2BB2CFeaturesSettings.DisplayWeightInformation)
        {
            pricingNoteModel.DisplayWeightInformation = _b2BB2CFeaturesSettings.DisplayWeightInformation;
            pricingNoteModel.Weight = product.Weight;
            pricingNoteModel.WeightValue = $"{product.Weight:F2} {baseWeight}";
        }

        return View(pricingNoteModel);
    }
}
