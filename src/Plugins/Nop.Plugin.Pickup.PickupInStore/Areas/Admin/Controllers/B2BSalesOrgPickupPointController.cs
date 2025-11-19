using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Plugin.Pickup.PickupInStore.Areas.Admin.Factories;
using Nop.Plugin.Pickup.PickupInStore.Areas.Admin.Models.B2BSalesOrgPickupPoints;
using Nop.Services.Customers;
using Nop.Services.Localization;
using Nop.Services.Messages;
using Nop.Services.Security;
using Nop.Web.Framework;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Mvc;
using Nop.Web.Framework.Mvc.ModelBinding;
using Nop.Web.Framework.Validators;
using NopStation.Plugin.B2B.ERPIntegrationCore.Domain;
using NopStation.Plugin.B2B.ERPIntegrationCore.Services;

namespace Nop.Plugin.Pickup.PickupInStore.Areas.Admin.Controllers
{
    [Area(AreaNames.ADMIN)]
    public class B2BSalesOrgPickupPointController : BasePluginController
    {
        private readonly IPermissionService _permissionService;
        private readonly IB2BSalesOrgPickupPointFactory _b2BSalesOrgPickupPointFactory;
        private readonly INotificationService _notificationService;
        private readonly ILocalizationService _localizationService;
        private readonly IWorkContext _workContext;
        private readonly IB2BSalesOrgPickupPointService _b2BSalesOrgPickupPointService;
        private readonly ICustomerService _customerService;

        public B2BSalesOrgPickupPointController(
            IPermissionService permissionService,
            IB2BSalesOrgPickupPointFactory b2BSalesOrgPickupPointFactory,
            INotificationService notificationService,
            ILocalizationService localizationService,
            IWorkContext workContext,
            IB2BSalesOrgPickupPointService b2BSalesOrgPickupPointService,
            ICustomerService customerService
            )
        {
            _permissionService = permissionService;
            _b2BSalesOrgPickupPointFactory = b2BSalesOrgPickupPointFactory;
            _notificationService = notificationService;
            _localizationService = localizationService;
            _workContext = workContext;
            _b2BSalesOrgPickupPointService = b2BSalesOrgPickupPointService;
            _customerService = customerService;
        }

        #region Utilities
        public async Task<bool> HasAdministratorsRole()
        {
            // check if the current user is an administrator
            return await _customerService.IsAdminAsync(await _workContext.GetCurrentCustomerAsync());
        }
        #endregion

        #region B2BSalesOrgPickupPoint
        [HttpPost]
        public virtual async Task<IActionResult> B2BSalesOrgPickupPointList(B2BSalesOrgPickupPointSearchModel searchModel)
        {
            //prepare model
            var model = await _b2BSalesOrgPickupPointFactory.PrepareB2BSalesOrgPickupPointListModel(searchModel);

            return Json(model);
        }

        [HttpPost]
        public virtual async Task<IActionResult> CreateB2BSalesOrgPickupPoint(int salesOrgId, [Validate] B2BSalesOrgPickupPointModel model)
        {
            if (salesOrgId == 0)
            {
                return RedirectToAction("List", "B2BSalesOrganisation");
            }

            // check if any Sales Organisation PickupPoint Exist
            if (await _b2BSalesOrgPickupPointService.CheckAnyB2BSalesOrgPickupPointExistBySalesOrgIdAndPickupPointIdAsync(salesOrgId, model.PickupPointId))
            {
                ModelState.AddModelError("PickupPointId", await _localizationService.GetResourceAsync("Plugins.Payments.B2BCustomerAccount.B2BSalesOrgPickupPoint.AlreadyExist"));
            }

            if (ModelState.IsValid)
            {
                var salesOrgPickupPoint = new B2BSalesOrgPickupPoint
                {
                    NopPickupPointId = model.PickupPointId,
                    B2BSalesOrgId = salesOrgId
                };

                await _b2BSalesOrgPickupPointService.InsertB2BSalesOrgPickupPointAsync(salesOrgPickupPoint);
            }
            else
            {
                return ErrorJson(ModelState.SerializeErrors());
            }

            return Json(new { Result = true });
        }

        [HttpPost]
        public virtual async Task<IActionResult> EditB2BSalesOrgPickupPoint(int salesOrgId, B2BSalesOrgPickupPointModel model)
        {
            if (!(await HasAdministratorsRole()))
                return AccessDeniedView();

            //try to get a B2BSalesOrgPickupPoint with the specified id
            var b2BSalesOrgPickupPoint = await _b2BSalesOrgPickupPointService.GetB2BSalesOrgPickupPointByIdAsync(model.Id);
            if (b2BSalesOrgPickupPoint == null)
                return RedirectToAction("Edit", "B2BSalesOrganisation", new { id = model.B2BSalesOrgId });

            if (!ModelState.IsValid)
            {
                return ErrorJson(ModelState.SerializeErrors());
            }

            await _b2BSalesOrgPickupPointService.UpdateB2BSalesOrgPickupPointAsync(b2BSalesOrgPickupPoint);

            return new NullJsonResult();
        }

        [HttpPost]
        public virtual async Task<IActionResult> DeleteB2BSalesOrgPickupPoint(int id)
        {
            if (!await HasAdministratorsRole())
                return AccessDeniedView();

            //try to get a B2BSalesOrgPickupPoint with the specified id
            var b2BSalesOrgPickupPoint = await _b2BSalesOrgPickupPointService.GetB2BSalesOrgPickupPointByIdAsync(id)
                ?? throw new ArgumentException("No B2B Sales Org PickupPoint found with the specified id", nameof(id));

            await _b2BSalesOrgPickupPointService.DeleteB2BSalesOrgPickupPointAsync(b2BSalesOrgPickupPoint);

            return new NullJsonResult();
        }

        #endregion
    }
}
