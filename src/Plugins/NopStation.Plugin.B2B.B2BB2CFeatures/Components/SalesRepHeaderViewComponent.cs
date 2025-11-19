using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Web.Framework.Components;
using NopStation.Plugin.B2B.B2BB2CFeatures.Services.Customers;

namespace NopStation.Plugin.B2B.B2BB2CFeatures.Components;

public class SalesRepHeaderViewComponent : NopViewComponent
{
    private readonly ICommonHelperService _commonHelperService;
    private readonly IWorkContext _workContext;

    public SalesRepHeaderViewComponent(ICommonHelperService commonHelperService,
        IWorkContext workContext)
    {
        _commonHelperService = commonHelperService;
        _workContext = workContext;
    }

    public async Task<IViewComponentResult> InvokeAsync(string widgetZone, object additionalData)
    {
        if (await _commonHelperService.HasB2BSalesRepRoleAsync())
        {
            var salesRepCustomer = await _workContext.GetCurrentCustomerAsync();
            ViewBag.CustomerEmail = salesRepCustomer.Email;
            return View("~/Plugins/NopStation.Plugin.B2B.B2BB2CFeatures/Views/Shared/Components/SalesRepHeader/Default.cshtml");
        }

        return Content(string.Empty);
    }
}