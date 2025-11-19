using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Plugin.Misc.ErpWebhook.Areas.Admin.Factories;
using Nop.Plugin.Misc.ErpWebhook.Areas.Admin.Models.AllowedWebhookManagerIpAddress;
using Nop.Plugin.Misc.ErpWebhook.Domain;
using Nop.Plugin.Misc.ErpWebhook.Services.Interfaces;
using Nop.Services.Localization;
using Nop.Services.Security;
using Nop.Web.Areas.Admin.Controllers;
using Nop.Web.Framework;
using Nop.Web.Framework.Mvc;
using Nop.Web.Framework.Mvc.Filters;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Plugin.Misc.ErpWebhook.Areas.Admin.Controllers;

[AuthorizeAdmin]
[Area(AreaNames.ADMIN)]
[AutoValidateAntiforgeryToken]
public class IpAddressController : BaseAdminController
{
    private readonly IAllowedWebhookManagerIpAddressModelFactory _allowedWebhookManagerIpAddressModelFactory;
    private readonly IAllowedWebhookManagerIpAddressesService _allowedWebhookManagerIpAddressesService;
    private readonly ILocalizationService _localizationService;
    private readonly IPermissionService _permissionService;

    public IpAddressController(
        IAllowedWebhookManagerIpAddressModelFactory allowedWebhookManagerIpAddressModelFactory,
        IAllowedWebhookManagerIpAddressesService allowedWebhookManagerIpAddressesService,
        ILocalizationService localizationService,
        IPermissionService permissionService
    )
    {
        _allowedWebhookManagerIpAddressModelFactory = allowedWebhookManagerIpAddressModelFactory;
        _allowedWebhookManagerIpAddressesService = allowedWebhookManagerIpAddressesService;
        _localizationService = localizationService;
        _permissionService = permissionService;
    }

    public virtual async Task<IActionResult> IpAddresses()
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManagePlugins))
            return AccessDeniedView();

        //prepare model
        var model =
            await _allowedWebhookManagerIpAddressModelFactory.PrepareAllowedWebhookManagerIpAddressSearchModelAsync(
                new AllowedWebhookManagerIpAddressSearchModel()
            );

        return View(model);
    }

    [HttpPost]
    public virtual async Task<IActionResult> IpAddressesList(
        AllowedWebhookManagerIpAddressSearchModel searchModel
    )
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManagePlugins))
            return AccessDeniedView();

        //prepare model
        var model =
            await _allowedWebhookManagerIpAddressModelFactory.PrepareAllowedWebhookManagerIpAddressListModelAsync(
                searchModel
            );

        return Json(model);
    }

    [HttpPost]
    public virtual async Task<IActionResult> IpAddressUpdate(
        AllowedWebhookManagerIpAddressModel model
    )
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManagePlugins))
            return AccessDeniedView();

        if (model.IpAddress != null)
            model.IpAddress = model.IpAddress.Trim();

        if (!ModelState.IsValid)
            return ErrorJson(ModelState.SerializeErrors());

        var resource = await _allowedWebhookManagerIpAddressesService.GetIpAddressByIdAsync(
            model.Id
        );

        if (resource != null)
        {
            resource.IpAddress = model.IpAddress;
            await _allowedWebhookManagerIpAddressesService.UpdateIpAddressAsync(resource);
        }

        return new NullJsonResult();
    }

    [HttpPost]
    public virtual async Task<IActionResult> IpAddressAdd(AllowedWebhookManagerIpAddressModel model)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManagePlugins))
            return AccessDeniedView();

        if (model.IpAddress != null)
            model.IpAddress = model.IpAddress.Trim();

        if (!ModelState.IsValid)
            return ErrorJson(ModelState.SerializeErrors());

        var res = await _allowedWebhookManagerIpAddressesService.GetIpAddressByIpAdressAsync(
            model.IpAddress
        );
        if (res == null)
        {
            var ipAddress = new AllowedWebhookManagerIpAddresses { IpAddress = model.IpAddress };

            await _allowedWebhookManagerIpAddressesService.AddIpAddressAsync(ipAddress);
        }
        else
        {
            return ErrorJson(
                await _localizationService.GetResourceAsync(
                    "Plugins.Misc.ErpWebhook.Fields.IpAddress.AlreadyExist"
                )
            );
        }

        return Json(new { Result = true });
    }

    [HttpPost]
    public virtual async Task<IActionResult> IpAddressDelete(int id)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManagePlugins))
            return AccessDeniedView();

        if (id > 0)
            await _allowedWebhookManagerIpAddressesService.DeleteIpAddressAsync(id);

        return new NullJsonResult();
    }
}
