using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Plugin.Comalytics.DomainFilter.Domains;
using Nop.Plugin.Comalytics.DomainFilter.Factories;
using Nop.Plugin.Comalytics.DomainFilter.Models;
using Nop.Plugin.Comalytics.DomainFilter.Services;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Messages;
using Nop.Services.Security;
using Nop.Web.Areas.Admin.Infrastructure.Mapper.Extensions;
using Nop.Web.Framework;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Mvc.Filters;

namespace Nop.Plugin.Comalytics.DomainFilter.Controllers
{
    [Area(AreaNames.ADMIN)]
    public class DomainFilterController : BasePluginController
    {
        #region Fields

        private readonly IDomainFilterService _domainFilterService;
        private readonly IDomainModelFactory _domainModelFactory;
        private readonly ILocalizationService _localizationService;
        private readonly INotificationService _notificationService;
        private readonly IPermissionService _permissionService;
        private readonly ISettingService _settingService;
        private readonly IStoreContext _storeContext;
        private readonly IWorkContext _workContext;

        #endregion

        #region Ctor

        public DomainFilterController(
            IDomainModelFactory domainModelFactory,
            IDomainFilterService domainFilterService,
            ILocalizationService localizationService,
            INotificationService notificationService,
            IPermissionService permissionService,
            ISettingService settingService,
            IStoreContext storeContext,
            IWorkContext workContext)
        {
            _domainFilterService = domainFilterService;
            _domainModelFactory = domainModelFactory;
            _localizationService = localizationService;
            _notificationService = notificationService;
            _permissionService = permissionService;
            _settingService = settingService;
            _storeContext = storeContext;
            _workContext = workContext;
        }

        #endregion

        #region Configuration
        [HttpGet]
        public virtual async Task<IActionResult> Configure()
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageCustomers))
                return AccessDeniedView();

            // Load settings for a chosen store scope
            var storeScope = await _storeContext.GetActiveStoreScopeConfigurationAsync();

            // Load plugin settings and set model data
            var domainFilterSettings = await _settingService.LoadSettingAsync<DomainFilterSettings>(storeScope);

            var model = new ConfigurationModel
            {
                EnableFilter = domainFilterSettings.EnableFilter
            };

            if (storeScope > 0)
            {
                model.EnableFilter_OverrideForStore = await _settingService.SettingExistsAsync(domainFilterSettings, x => x.EnableFilter, storeScope);
            }

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Configure(ConfigurationModel model)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageCustomers))
                return AccessDeniedView();

            if (!ModelState.IsValid)
                return await Configure();

            // Load settings for a chosen store scope
            var storeScope = await _storeContext.GetActiveStoreScopeConfigurationAsync();

            // Set plugin settings from model data
            var domainFilterSettings = await _settingService.LoadSettingAsync<DomainFilterSettings>(storeScope);
            domainFilterSettings.EnableFilter = model.EnableFilter;

            await _settingService.SaveSettingOverridablePerStoreAsync(
                domainFilterSettings,
                x => x.EnableFilter,
                model.EnableFilter_OverrideForStore,
                storeScope,
                false
            );

            // Clear the cache
            await _settingService.ClearCacheAsync();
            _notificationService.SuccessNotification(
                await _localizationService.GetResourceAsync("Admin.Plugins.Saved")
            );

            return await Configure();
        }


        #endregion

        #region Create/Update/Edit/Delete

        public virtual async Task<IActionResult> Create()
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageCustomers))
                return AccessDeniedView();

            var model = await _domainModelFactory.PrepareDomainModelAsync(new DomainModel(), null);
            return View(model);
        }

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        [FormValueRequired("save", "save-continue")]
        public virtual async Task<IActionResult> Create(DomainModel model, bool continueEditing, IFormCollection form)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageCustomers))
                return AccessDeniedView();

            // Check if domain or email name already exists
            if (_domainFilterService.DomainOrEmailExists(model.DomainOrEmailName, model.TypeId))
            {
                ModelState.AddModelError("DomainOrEmailName",
                    await _localizationService.GetResourceAsync("Plugins.Comalytics.DomainFilter.Domain.DomainOrEmailName.AlreadyExists"));
            }

            if (ModelState.IsValid)
            {
                var domain = model.ToEntity<Domain>();
                _domainFilterService.InsertDomain(domain);

                _notificationService.SuccessNotification(
                    await _localizationService.GetResourceAsync("Plugins.Comalytics.DomainFilter.Domain.Insert"));

                if (!continueEditing)
                    return RedirectToAction("List");

                return RedirectToAction("Edit", new { id = domain.Id });
            }

            return View(model);
        }

        [HttpPost]
        public virtual async Task<IActionResult> DomainList(DomainSearchModel searchModel)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageCustomers))
                return AccessDeniedView();

            var model = await _domainModelFactory.PrepareDomainListModelAsync(searchModel);
            return Json(model);
        }

        [HttpPost]
        public virtual async Task<IActionResult> Delete(int id)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageCustomers))
                return AccessDeniedView();

            var domain = _domainFilterService.GetDomainById(id);
            if (domain == null)
                return RedirectToAction("List");

            _domainFilterService.DeleteDomain(domain);

            _notificationService.SuccessNotification(
                await _localizationService.GetResourceAsync("Plugins.Comalytics.DomainFilter.Domain.Delete"));

            return RedirectToAction("List");
        }

        public virtual async Task<IActionResult> Edit(int id)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageCustomers))
                return AccessDeniedView();

            var domain = _domainFilterService.GetDomainById(id);
            if (domain == null)
                return RedirectToAction("List");

            var model = await _domainModelFactory.PrepareDomainModelAsync(null, domain);

            return View(model);
        }

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        [FormValueRequired("save", "save-continue")]
        public virtual async Task<IActionResult> Edit(DomainModel model, bool continueEditing, IFormCollection form)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageCustomers))
                return AccessDeniedView();

            var domain = _domainFilterService.GetDomainById(model.Id);
            if (domain == null)
                return RedirectToAction("List");

            // Check if domain or email name has changed and if the new name already exists
            if (domain.DomainOrEmailName != model.DomainOrEmailName &&
                _domainFilterService.DomainOrEmailExists(model.DomainOrEmailName, model.TypeId))
            {
                ModelState.AddModelError("DomainOrEmailName",
                    await _localizationService.GetResourceAsync("Plugins.Comalytics.DomainFilter.Domain.DomainOrEmailName.AlreadyExists"));
            }

            if (ModelState.IsValid)
            {
                domain = model.ToEntity(domain);
                _domainFilterService.UpdateDomain(domain);

                _notificationService.SuccessNotification(
                    await _localizationService.GetResourceAsync("Plugins.Comalytics.DomainFilter.Domain.Update"));

                if (!continueEditing)
                    return RedirectToAction("List");

                return RedirectToAction("Edit", new { id = domain.Id });
            }

            model = await _domainModelFactory.PrepareDomainModelAsync(model, domain);

            return View(model);
        }

        public virtual async Task<IActionResult> List()
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageCustomers))
                return AccessDeniedView();

            var model = await _domainModelFactory.PrepareDomainSearchModelAsync(new DomainSearchModel());
            return View(model);
        }


        #endregion

        #region Export/Import

        [HttpPost]
        public virtual async Task<IActionResult> DomainImportExcel(IFormFile importexcelfile)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageCustomers))
                return AccessDeniedView();

            try
            {
                // Check if the uploaded file is valid
                if (importexcelfile != null && importexcelfile.Length > 0)
                {
                    await _domainModelFactory.ImportDomainListFromXlsx(importexcelfile.OpenReadStream());
                }
                else
                {
                    _notificationService.ErrorNotification(await _localizationService.GetResourceAsync("Admin.Common.UploadFile"));
                    return RedirectToAction("List");
                }

                _notificationService.SuccessNotification(
                    await _localizationService.GetResourceAsync("Plugin.Comalytics.DomainFilter.Imported"));
                return RedirectToAction("List");
            }
            catch (Exception exc)
            {
                await _notificationService.ErrorNotificationAsync(exc);
                return RedirectToAction("List");
            }
        }

        [HttpPost, ActionName("List")]
        [FormValueRequired("exportexcel-all")]
        public virtual async Task<IActionResult> ExportExcelAll(DomainSearchModel searchModel)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageCustomers))
                return AccessDeniedView();

            try
            {
                var bytes = await _domainModelFactory.ExportDomainsToXlsx(searchModel);
                return File(bytes, MimeTypes.TextXlsx, "DomainList.xlsx");
            }
            catch (Exception exc)
            {
                await _notificationService.ErrorNotificationAsync(exc);
                return RedirectToAction("List");
            }
        }

        [HttpPost]
        public virtual async Task<IActionResult> ExportExcelSelected(string selectedIds)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageCustomers))
                return AccessDeniedView();

            // Check if any items have been selected for export
            if (!string.IsNullOrEmpty(selectedIds))
            {
                var ids = selectedIds
                    .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(x => Convert.ToInt32(x))
                    .ToList();

                // Await the result of ExportDomainsToXlsx since it returns Task<byte[]>
                var bytes = await _domainModelFactory.ExportDomainsToXlsx(ids);

                return File(bytes, MimeTypes.TextXlsx, "DomainList.xlsx");
            }

            return RedirectToAction("List");
        }

        #endregion
    }
}
