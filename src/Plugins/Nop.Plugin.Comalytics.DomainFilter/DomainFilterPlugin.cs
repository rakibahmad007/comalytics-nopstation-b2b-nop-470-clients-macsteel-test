using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Routing;
using Nop.Core;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Plugins;
using Nop.Services.Security;
using Nop.Web.Framework.Menu;

namespace Nop.Plugin.Comalytics.DomainFilter
{
    public class DomainFilterPlugin : BasePlugin, IAdminMenuPlugin
    {
        #region Fields

        public bool HideInWidgetList => false;

        private readonly ILocalizationService _localizationService;
        private readonly IPermissionService _permissionService;
        private readonly ISettingService _settingService;
        private readonly IWebHelper _webHelper;

        #endregion

        #region Ctor

        public DomainFilterPlugin(
            ILocalizationService localizationService,
            IPermissionService permissionService,
            ISettingService settingService,
            IWebHelper webHelper)
        {
            _localizationService = localizationService;
            _permissionService = permissionService;
            _settingService = settingService;
            _webHelper = webHelper;
        }

        #endregion

        #region Utility

        #endregion

        #region Methods

        public override string GetConfigurationPageUrl()
        {
            return $"{_webHelper.GetStoreLocation()}Admin/DomainFilter/Configure";
        }

        public override async Task InstallAsync()
        {
            var filterSetting = new DomainFilterSettings()
            {
                EnableFilter = false
            };
            await _settingService.SaveSettingAsync(filterSetting);
            // Locales
            await _localizationService.AddOrUpdateLocaleResourceAsync(new Dictionary<string, string>
            {
                ["Plugins.Comalytics.DomainFilter.Configuration.Fields.EnableFilter"] = "Enable Filter",
                ["Plugins.Comalytics.DomainFilter.Configuration.Fields.EnableFilter.Hint"] = "Enable Filter",
                ["Plugins.Comalytics.DomainFilter.Domain.DomainOrEmailName.AlreadyExists"] = "Domain Name Already Exists",
                ["Plugins.Comalytics.DomainFilter.Domain.Insert"] = "Domain inserted successfully",
                ["Plugins.Comalytics.DomainFilter.Domain.Update"] = "Domain updated successfully",
                ["Plugins.Comalytics.DomainFilter.DomainModel.Type.Email"] = "Email",
                ["Plugins.Comalytics.DomainFilter.DomainModel.Type.Domain"] = "Domain",
                ["Plugins.Comalytics.DomainFilter.DomainSearchModel.SearchDomainOrEmailName"] = "Domain Name",
                ["Plugins.Comalytics.DomainFilter.DomainSearchModel.SearchType"] = "Type",
                ["Plugins.Comalytics.DomainFilter.DomainSearchModel.SearchType.All"] = "All",
                ["Plugins.Comalytics.DomainFilter.DomainSearchModel.SearchType.Email"] = "Email",
                ["Plugins.Comalytics.DomainFilter.DomainSearchModel.SearchType.Domain"] = "Domain",
                ["Plugins.Comalytics.DomainFilter.DomainSearchModel.SearchActive"] = "Active",
                ["Plugins.Comalytics.DomainFilter.DomainSearchModel.SearchActive.All"] = "All",
                ["Plugins.Comalytics.DomainFilter.DomainSearchModel.SearchActive.ActiveOnly"] = "Active Only",
                ["Plugins.Comalytics.DomainFilter.DomainSearchModel.SearchActive.InactiveOnly"] = "Inactive Only",
                ["Plugins.Comalytics.DomainFilter.Domain.DomainOrEmailName"] = "Domain Or Email Name",
                ["Plugins.Comalytics.DomainFilter.Domain.Type"] = "Type",
                ["Plugins.Comalytics.DomainFilter.Domain.IsActive"] = "Is Active",
                ["Plugins.Comalytics.DomainFilter.Domain.Info"] = "Info",
                ["Plugins.Comalytics.DomainFilter.Configuration"] = "Configuration",
                ["Plugins.Comalytics.DomainFilter.AddNew"] = "Add New",
                ["Plugins.Comalytics.DomainFilter.BackToList"] = "Back To List",
                ["Plugins.Comalytics.DomainFilter.EditDomainDetails"] = "Edit Domain Details",
                ["Plugins.Comalytics.DomainFilter.List"] = "List",
                ["Plugins.Comalytics.DomainFilter.Domains"] = "List of Blacklisted Domains",
                ["Plugins.Comalytics.DomainFilter.List.Fields.DomainOrEmailName"] = "Domain Name",
                ["Plugins.Comalytics.DomainFilter.List.Fields.Type"] = "Type",
                ["Plugins.Comalytics.DomainFilter.List.Fields.IsActive"] = "Is Active",
                ["Plugins.Comalytics.DomainFilter.Menu.Title"] = "Domain Filter",
                ["Plugins.Comalytics.DomainFilter.Menu.Configuration"] = "Configuration",
                ["Plugins.Comalytics.DomainFilter.Menu.List"] = "List",
                ["Plugin.Comalytics.DomainFilter.Imported"] = "Domains imported successfully",
                ["Enums.Plugin.Comalytics.DomainFilter.DomainType.Email"] = "Email",
                ["Enums.Plugin.Comalytics.DomainFilter.DomainType.Domain"] = "Domain",
                ["Plugins.Comalytics.DomainFilter.Domain.DomainOrEmailName.Required"] = "Domain Or Email name is required.",
                ["Plugins.Comalytics.DomainFilter.Domain.Type.Required.DomainOrEmail"] = "Type must be Domain or Email",
                ["Plugins.Comalytics.DomainFilter.Domain.Blacklisted"] = "Domain or Email is blacklisted."
            });

            await base.InstallAsync();
        }

        public async Task ManageSiteMapAsync(SiteMapNode rootNode)
        {
            var menuItem = new SiteMapNode()
            {
                IconClass = "fas fa-filter",
                SystemName = "Comalytics B2C",
                Title = await _localizationService.GetResourceAsync("Plugins.Comalytics.DomainFilter.Menu.Title"),
                Visible = true
            };

            var configItem = new SiteMapNode()
            {
                ActionName = "Configure",
                ControllerName = "DomainFilter",
                IconClass = "far fa-dot-circle",
                RouteValues = new RouteValueDictionary() { { "area", "Admin" } },
                SystemName = "Domain Filter Configuration",
                Title = await _localizationService.GetResourceAsync("Plugins.Comalytics.DomainFilter.Menu.Configuration"),
                Visible = true
            };

            menuItem.ChildNodes.Add(configItem);

            var listItem = new SiteMapNode()
            {
                ActionName = "List",
                ControllerName = "DomainFilter",
                IconClass = "far fa-dot-circle",
                RouteValues = new RouteValueDictionary() { { "area", "Admin" } },
                SystemName = "Domain Filter List",
                Title = await _localizationService.GetResourceAsync("Plugins.Comalytics.DomainFilter.Menu.List"),
                Visible = true
            };

            menuItem.ChildNodes.Add(listItem);
            rootNode.ChildNodes.Add(menuItem);
        }

        public override async Task UninstallAsync()
        {
            await _settingService.DeleteSettingAsync<DomainFilterSettings>();

            await _localizationService.DeleteLocaleResourceAsync("Plugins.Comalytics.DomainFilter.Configuration.Fields.EnableFilter");
            await _localizationService.DeleteLocaleResourceAsync("Plugins.Comalytics.DomainFilter.Domain.DomainOrEmailName.AlreadyExists");
            await _localizationService.DeleteLocaleResourceAsync("Plugins.Comalytics.DomainFilter.Domain.Insert");
            await _localizationService.DeleteLocaleResourceAsync("Plugins.Comalytics.DomainFilter.Domain.Update");
            await _localizationService.DeleteLocaleResourceAsync("Plugins.Comalytics.DomainFilter.DomainModel.Type.Email");
            await _localizationService.DeleteLocaleResourceAsync("Plugins.Comalytics.DomainFilter.DomainModel.Type.Domain");
            await _localizationService.DeleteLocaleResourceAsync("Plugins.Comalytics.DomainFilter.DomainSearchModel.SearchDomainOrEmailName");
            await _localizationService.DeleteLocaleResourceAsync("Plugins.Comalytics.DomainFilter.DomainSearchModel.SearchType");
            await _localizationService.DeleteLocaleResourceAsync("Plugins.Comalytics.DomainFilter.DomainSearchModel.SearchType.All");
            await _localizationService.DeleteLocaleResourceAsync("Plugins.Comalytics.DomainFilter.DomainSearchModel.SearchType.Email");
            await _localizationService.DeleteLocaleResourceAsync("Plugins.Comalytics.DomainFilter.DomainSearchModel.SearchType.Domain");
            await _localizationService.DeleteLocaleResourceAsync("Plugins.Comalytics.DomainFilter.DomainSearchModel.SearchActive");
            await _localizationService.DeleteLocaleResourceAsync("Plugins.Comalytics.DomainFilter.DomainSearchModel.SearchActive.All");
            await _localizationService.DeleteLocaleResourceAsync("Plugins.Comalytics.DomainFilter.DomainSearchModel.SearchActive.ActiveOnly");
            await _localizationService.DeleteLocaleResourceAsync("Plugins.Comalytics.DomainFilter.DomainSearchModel.SearchActive.InactiveOnly");
            await _localizationService.DeleteLocaleResourceAsync("Plugins.Comalytics.DomainFilter.Domain.DomainOrEmailName");
            await _localizationService.DeleteLocaleResourceAsync("Plugins.Comalytics.DomainFilter.Domain.Type");
            await _localizationService.DeleteLocaleResourceAsync("Plugins.Comalytics.DomainFilter.Domain.IsActive");
            await _localizationService.DeleteLocaleResourceAsync("Plugins.Comalytics.DomainFilter.Domain.Info");
            await _localizationService.DeleteLocaleResourceAsync("Plugins.Comalytics.DomainFilter.Configuration");
            await _localizationService.DeleteLocaleResourceAsync("Plugins.Comalytics.DomainFilter.AddNew");
            await _localizationService.DeleteLocaleResourceAsync("Plugins.Comalytics.DomainFilter.BackToList");
            await _localizationService.DeleteLocaleResourceAsync("Plugins.Comalytics.DomainFilter.EditDomainDetails");
            await _localizationService.DeleteLocaleResourceAsync("Plugins.Comalytics.DomainFilter.List");
            await _localizationService.DeleteLocaleResourceAsync("Plugins.Comalytics.DomainFilter.Domains");
            await _localizationService.DeleteLocaleResourceAsync("Plugins.Comalytics.DomainFilter.List.Fields.DomainOrEmailName");
            await _localizationService.DeleteLocaleResourceAsync("Plugins.Comalytics.DomainFilter.List.Fields.Type");
            await _localizationService.DeleteLocaleResourceAsync("Plugins.Comalytics.DomainFilter.List.Fields.IsActive");
            await _localizationService.DeleteLocaleResourceAsync("Plugins.Comalytics.DomainFilter.Menu.Title");
            await _localizationService.DeleteLocaleResourceAsync("Plugins.Comalytics.DomainFilter.Menu.Configuration");
            await _localizationService.DeleteLocaleResourceAsync("Plugins.Comalytics.DomainFilter.Menu.List");
            await _localizationService.DeleteLocaleResourceAsync("Plugin.Comalytics.DomainFilter.Imported");
            await _localizationService.DeleteLocaleResourceAsync("Enums.Plugin.Comalytics.DomainFilter.DomainType.Email");
            await _localizationService.DeleteLocaleResourceAsync("Enums.Plugin.Comalytics.DomainFilter.DomainType.Domain");
            await _localizationService.DeleteLocaleResourceAsync("Plugins.Comalytics.DomainFilter.Domain.DomainOrEmailName.Required");
            await _localizationService.DeleteLocaleResourceAsync("Plugins.Comalytics.DomainFilter.Domain.Type.Required.DomainOrEmail");
            await _localizationService.DeleteLocaleResourceAsync("Plugins.Comalytics.DomainFilter.Domain.Blacklisted");

            await base.UninstallAsync();
        }

        #endregion
    }
}
