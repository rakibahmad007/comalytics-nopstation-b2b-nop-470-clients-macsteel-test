using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Web.Framework.Components;
using NopStation.Plugin.B2B.B2BB2CFeatures.Services.ErpCustomerFunctionality;

namespace NopStation.Plugin.B2B.B2BB2CFeatures.Components;

public class QuickOrderHeaderLinkViewComponent : NopViewComponent
{
    private readonly IErpCustomerFunctionalityService _erpCustomerFunctionalityService;
    private readonly IWorkContext _workContext;

    public QuickOrderHeaderLinkViewComponent(IErpCustomerFunctionalityService erpCustomerFunctionalityService, IWorkContext workContext)
    {
        _erpCustomerFunctionalityService = erpCustomerFunctionalityService;
        _workContext = workContext;
    }

    public async Task<IViewComponentResult> InvokeAsync()
    {
        var nopUser = await _erpCustomerFunctionalityService.GetActiveErpNopUserByCustomerAsync(await _workContext.GetCurrentCustomerAsync());
        if (nopUser is null)
        {
            return Content(string.Empty);
        }

        return View();
    }
}
