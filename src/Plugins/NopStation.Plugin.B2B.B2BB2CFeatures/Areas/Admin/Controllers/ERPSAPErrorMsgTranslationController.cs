using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Services.Common;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Messages;
using Nop.Services.Security;
using Nop.Web.Framework;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Mvc;
using Nop.Web.Framework.Mvc.ModelBinding;
using Nop.Web.Framework.Validators;
using NopStation.Plugin.B2B.B2BB2CFeatures.Areas.Admin.Factories;
using NopStation.Plugin.B2B.B2BB2CFeatures.Areas.Admin.Models.ERPSAPErrorMsgTranslations;
using NopStation.Plugin.B2B.B2BB2CFeatures.Services.ErpCustomerFunctionality;
using NopStation.Plugin.B2B.B2BB2CFeatures.Services.ERPSAPErrorMsgTranslations;
using NopStation.Plugin.B2B.ERPIntegrationCore.Domain;

namespace NopStation.Plugin.B2B.B2BB2CFeatures.Areas.Admin.Controllers
{
    [Area(AreaNames.ADMIN)]
    public class ERPSAPErrorMsgTranslationController : BasePluginController
    {
        #region fields

        private readonly ICustomerActivityService _customerActivityService;
        private readonly IERPSAPErrorMsgTranslationService _erpSAPErrorMsgTranslationService;
        private readonly IERPSAPErrorMsgTranslationFactory _erpSAPErrorMsgTranslationFactory;
        private readonly INotificationService _notificationService;
        private readonly ILocalizationService _localizationService;
        private readonly IGenericAttributeService _genericAttributeService;
        private readonly IStoreContext _storeContext;
        private readonly IWorkContext _workContext;
        private readonly ILogger _logger;
        private readonly IPermissionService _permissionService;
        private readonly IErpCustomerFunctionalityService _erpCustomerFunctionality;

        #endregion

        #region ctor

        public ERPSAPErrorMsgTranslationController(
            ICustomerActivityService customerActivityService,
            IERPSAPErrorMsgTranslationService erpSAPErrorMsgTranslationService,
            IERPSAPErrorMsgTranslationFactory erpSAPErrorMsgTranslationFactory,
            INotificationService notificationService,
            ILocalizationService localizationService,
            IGenericAttributeService genericAttributeService,
            IStoreContext storeContext,
            IWorkContext workContext,
            ILogger logger,
            IPermissionService permissionService,
            IErpCustomerFunctionalityService b2BCustomerFunctionality
            )
        {
            _customerActivityService = customerActivityService;
            _erpSAPErrorMsgTranslationService = erpSAPErrorMsgTranslationService;
            _erpSAPErrorMsgTranslationFactory = erpSAPErrorMsgTranslationFactory;
            _notificationService = notificationService;
            _localizationService = localizationService;
            _genericAttributeService = genericAttributeService;
            _storeContext = storeContext;
            _workContext = workContext;
            _logger = logger;
            _permissionService = permissionService;
            _erpCustomerFunctionality = b2BCustomerFunctionality;
        }

        #endregion

        public virtual async Task<IActionResult> List()
        {
            if (!await _erpCustomerFunctionality.IsCurrentCustomerInAdministratorRoleAsync())
                return await AccessDeniedDataTablesJson();

            var model = await _erpSAPErrorMsgTranslationFactory.PrepareERPSAPErrorMsgSearchModelAsync(new ERPSAPErrorMsgSearchModel());
            return View("~/Plugins/NopStation.Plugin.B2B.B2BB2CFeatures/Areas/Admin/Views/ERPSAPErrorMsgTranslation/List.cshtml", model);
        }

        [HttpPost]
        public virtual async Task<IActionResult> ListData(ERPSAPErrorMsgSearchModel searchModel)
        {
            if (!await _erpCustomerFunctionality.IsCurrentCustomerInAdministratorRoleAsync())
                return await AccessDeniedDataTablesJson();

            var model = await _erpSAPErrorMsgTranslationFactory.PrepareERPSAPErrorMsgListModelAsync(searchModel);
            return Json(model);
        }

        [HttpPost]
        public virtual async Task<IActionResult> Create(ERPSAPErrorMsgTranslationModel model)
        {
            if (!await _erpCustomerFunctionality.IsCurrentCustomerInAdministratorRoleAsync())
                return await AccessDeniedDataTablesJson();

            if (model.ErrorMsg != null)
                model.ErrorMsg = model.ErrorMsg.Trim();

            if (model.UserTranslation != null)
                model.UserTranslation = model.UserTranslation.Trim();

            if (!ModelState.IsValid)
            {
                return ErrorJson(ModelState.SerializeErrors());
            }

            if (ModelState.IsValid)
            {
                var b2bErrorMsg = new ERPSAPErrorMsgTranslation
                {
                    ErrorMsg = model.ErrorMsg,
                    UserTranslation = model.UserTranslation
                };
                await _erpSAPErrorMsgTranslationService.InsertERPSAPErrorMsgTranslationAsync(b2bErrorMsg);
                await _customerActivityService.InsertActivityAsync("NopStation.Plugin.B2B.B2BB2CFeatures.SAPErrorMsgTranslation.Insert",
                   string.Format(await _localizationService.GetResourceAsync("NopStation.Plugin.B2B.B2BB2CFeatures.ActivityLog.SAPErrorMsgTranslation.Insert"), b2bErrorMsg.ErrorMsg, b2bErrorMsg.UserTranslation),
                   b2bErrorMsg);
            }
            return Json(new { Result = true });
        }

        [HttpPost]
        public virtual async Task<IActionResult> ErrorMsgUpdate([Validate] ERPSAPErrorMsgTranslationModel model)
        {
            if (!await _erpCustomerFunctionality.IsCurrentCustomerInAdministratorRoleAsync())
                return await AccessDeniedDataTablesJson();

            if (!ModelState.IsValid)
            {
                return ErrorJson(ModelState.SerializeErrors());
            }

            var errorMsg = await _erpSAPErrorMsgTranslationService.GetErrorMsgByIdAsync(model.Id);

            if (errorMsg == null)
                return RedirectToAction("Create");
            if (ModelState.IsValid)
            {
                errorMsg.ErrorMsg = model.ErrorMsg;
                errorMsg.UserTranslation = model.UserTranslation;
                await _erpSAPErrorMsgTranslationService.UpdateERPSAPErrorMsgTranslationAsync(errorMsg);
            }
            return new NullJsonResult();
        }

        [HttpPost]
        public virtual async Task<IActionResult> ErrorMsgDelete(int id)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageLanguages))
                return AccessDeniedView();

            var errorMsg = await _erpSAPErrorMsgTranslationService.GetErrorMsgByIdAsync(id)
                ?? throw new ArgumentException("No Error Message found with the specified id", nameof(id));
            await _erpSAPErrorMsgTranslationService.DeleteERPSAPErrorMsgTranslationAsync(errorMsg);
            return new NullJsonResult();
        }
    }
}
