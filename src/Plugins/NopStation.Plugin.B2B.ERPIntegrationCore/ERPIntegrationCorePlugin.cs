using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Core.Domain.Customers;
using Nop.Services.Common;
using Nop.Services.Customers;
using Nop.Services.Localization;
using Nop.Services.Plugins;
using Nop.Services.Security;
using Nop.Web.Framework.Menu;
using NopStation.Plugin.B2B.ERPIntegrationCore.Infrastructure;
using NopStation.Plugin.Misc.Core.Services;

namespace NopStation.Plugin.B2B.ERPIntegrationCore;

public class ERPIntegrationCorePlugin : BasePlugin, IMiscPlugin, IAdminMenuPlugin, INopStationPlugin
{
    #region Fields

    private readonly ICustomerService _customerService;
    private readonly IWebHelper _webHelper;
    private readonly ILocalizationService _localizationService;
    private readonly IPermissionService _permissionService;

    #endregion

    #region Ctor

    public ERPIntegrationCorePlugin(ICustomerService customerService,
        IWebHelper webHelper,
        ILocalizationService localizationService,
        IPermissionService permissionService)
    {
        _customerService = customerService;
        _webHelper = webHelper;
        _localizationService = localizationService;
        _permissionService = permissionService;
    }

    #endregion

    #region Utilities

    private async Task InsertB2BB2CCustomerRolesAsync()
    {
        #region Insert B2B-B2C Customer Roles

        var b2BCustomerRole = await _customerService.GetCustomerRoleBySystemNameAsync(ERPIntegrationCoreDefaults.B2BCustomerRoleSystemName);
        if (b2BCustomerRole == null)
        {
            b2BCustomerRole = new CustomerRole
            {
                Name = ERPIntegrationCoreDefaults.B2BCustomerRole,
                Active = true,
                SystemName = ERPIntegrationCoreDefaults.B2BCustomerRoleSystemName
            };
            await _customerService.InsertCustomerRoleAsync(b2BCustomerRole);
        }

        var b2CCustomerRole = await _customerService.GetCustomerRoleBySystemNameAsync(ERPIntegrationCoreDefaults.B2CCustomerRoleSystemName);
        if (b2CCustomerRole == null)
        {
            b2CCustomerRole = new CustomerRole
            {
                Name = ERPIntegrationCoreDefaults.B2CCustomerRole,
                Active = true,
                SystemName = ERPIntegrationCoreDefaults.B2CCustomerRoleSystemName
            };
            await _customerService.InsertCustomerRoleAsync(b2CCustomerRole);
        }

        var quoteAssistantRole = await _customerService.GetCustomerRoleBySystemNameAsync(ERPIntegrationCoreDefaults.B2BQuoteAssistantRoleSystemName);
        if (quoteAssistantRole == null)
        {
            quoteAssistantRole = new CustomerRole
            {
                Name = ERPIntegrationCoreDefaults.B2BQuoteAssistantRole,
                Active = true,
                SystemName = ERPIntegrationCoreDefaults.B2BQuoteAssistantRoleSystemName
            };
            await _customerService.InsertCustomerRoleAsync(quoteAssistantRole);
        }

        var orderAssistantRole = await _customerService.GetCustomerRoleBySystemNameAsync(ERPIntegrationCoreDefaults.B2BOrderAssistantRoleSystemName);
        if (orderAssistantRole == null)
        {
            orderAssistantRole = new CustomerRole
            {
                Name = ERPIntegrationCoreDefaults.B2BOrderAssistantRole,
                Active = true,
                SystemName = ERPIntegrationCoreDefaults.B2BOrderAssistantRoleSystemName
            };
            await _customerService.InsertCustomerRoleAsync(orderAssistantRole);
        }

        var b2BSalesRepRole = await _customerService.GetCustomerRoleBySystemNameAsync(ERPIntegrationCoreDefaults.B2BSalesRepRoleSystemName);
        if (b2BSalesRepRole == null)
        {
            b2BSalesRepRole = new CustomerRole
            {
                Name = ERPIntegrationCoreDefaults.B2BSalesRepRole,
                Active = true,
                SystemName = ERPIntegrationCoreDefaults.B2BSalesRepRoleSystemName
            };
            await _customerService.InsertCustomerRoleAsync(b2BSalesRepRole);
        }

        var quickOrderUserRole = await _customerService.GetCustomerRoleBySystemNameAsync(ERPIntegrationCoreDefaults.QuickOrderUserRoleSystemName);
        if (quickOrderUserRole == null)
        {
            quickOrderUserRole = new CustomerRole
            {
                Name = ERPIntegrationCoreDefaults.QuickOrderUserRole,
                Active = true,
                SystemName = ERPIntegrationCoreDefaults.QuickOrderUserRoleSystemName
            };
            await _customerService.InsertCustomerRoleAsync(quickOrderUserRole);
        }

        var b2BB2CAdminRole = await _customerService.GetCustomerRoleBySystemNameAsync(ERPIntegrationCoreDefaults.B2BB2CAdminRoleSystemName);
        if (b2BB2CAdminRole == null)
        {
            b2BB2CAdminRole = new CustomerRole
            {
                Name = ERPIntegrationCoreDefaults.B2BB2CAdminRole,
                Active = true,
                SystemName = ERPIntegrationCoreDefaults.B2BB2CAdminRoleSystemName
            };
            await _customerService.InsertCustomerRoleAsync(b2BB2CAdminRole);
        }

        var b2bCustomerAccountingPersonnelRole = await _customerService.GetCustomerRoleBySystemNameAsync(ERPIntegrationCoreDefaults.B2BCustomerAccountingPersonnelRoleSystemName);
        if (b2bCustomerAccountingPersonnelRole == null)
        {
            b2bCustomerAccountingPersonnelRole = new CustomerRole
            {
                Name = ERPIntegrationCoreDefaults.B2BCustomerAccountingPersonnelRole,
                Active = true,
                SystemName = ERPIntegrationCoreDefaults.B2BCustomerAccountingPersonnelRoleSystemName
            };
            await _customerService.InsertCustomerRoleAsync(b2bCustomerAccountingPersonnelRole);
        }

        var b2BCustomerAccountManagerRole = await _customerService.GetCustomerRoleBySystemNameAsync(ERPIntegrationCoreDefaults.B2BCustomerAccountManagerRoleSystemName);
        if (b2BCustomerAccountManagerRole == null)
        {
            b2BCustomerAccountManagerRole = new CustomerRole
            {
                Name = ERPIntegrationCoreDefaults.B2BCustomerAccountManagerRole,
                FreeShipping = false,
                TaxExempt = false,
                Active = true,
                IsSystemRole = false,
                SystemName = ERPIntegrationCoreDefaults.B2BCustomerAccountManagerRoleSystemName,
                EnablePasswordLifetime = false,
                OverrideTaxDisplayType = false
            };
            await _customerService.InsertCustomerRoleAsync(b2BCustomerAccountManagerRole);
        }

        #endregion
    }

    private async Task DeleteB2BB2CCustomerRolesAsync()
    {
        #region Delete B2B-B2C Customer Roles

        var b2BCustomerRole = await _customerService.GetCustomerRoleBySystemNameAsync(ERPIntegrationCoreDefaults.B2BCustomerRoleSystemName);
        if (b2BCustomerRole != null)
        {
            await _customerService.DeleteCustomerRoleAsync(b2BCustomerRole);
        }
        var b2CCustomerRole = await _customerService.GetCustomerRoleBySystemNameAsync(ERPIntegrationCoreDefaults.B2CCustomerRoleSystemName);
        if (b2CCustomerRole != null)
        {
            await _customerService.DeleteCustomerRoleAsync(b2CCustomerRole);
        }
        var quoteAssistantRole = await _customerService.GetCustomerRoleBySystemNameAsync(ERPIntegrationCoreDefaults.B2BQuoteAssistantRoleSystemName);
        if (quoteAssistantRole != null)
        {
            await _customerService.DeleteCustomerRoleAsync(quoteAssistantRole);
        }
        var orderAssistantRole = await _customerService.GetCustomerRoleBySystemNameAsync(ERPIntegrationCoreDefaults.B2BOrderAssistantRoleSystemName);
        if (orderAssistantRole != null)
        {
            await _customerService.DeleteCustomerRoleAsync(orderAssistantRole);
        }
        var b2BSalesRepRole = await _customerService.GetCustomerRoleBySystemNameAsync(ERPIntegrationCoreDefaults.B2BSalesRepRoleSystemName);
        if (b2BSalesRepRole != null)
        {
            await _customerService.DeleteCustomerRoleAsync(b2BSalesRepRole);
        }
        var quickOrderUserRole = await _customerService.GetCustomerRoleBySystemNameAsync(ERPIntegrationCoreDefaults.QuickOrderUserRoleSystemName);
        if (quickOrderUserRole != null)
        {
            await _customerService.DeleteCustomerRoleAsync(quickOrderUserRole);
        }
        var b2BB2CAdminRole = await _customerService.GetCustomerRoleBySystemNameAsync(ERPIntegrationCoreDefaults.B2BB2CAdminRoleSystemName);
        if (b2BB2CAdminRole != null)
        {
            await _customerService.DeleteCustomerRoleAsync(b2BB2CAdminRole);
        }
        var b2bCustomerAccountingPersonnelRole = await _customerService.GetCustomerRoleBySystemNameAsync(ERPIntegrationCoreDefaults.B2BCustomerAccountingPersonnelRoleSystemName);
        if (b2bCustomerAccountingPersonnelRole != null)
        {
            await _customerService.DeleteCustomerRoleAsync(b2bCustomerAccountingPersonnelRole);
        }
        var b2BCustomerAccountManagerRole = await _customerService.GetCustomerRoleBySystemNameAsync(ERPIntegrationCoreDefaults.B2BCustomerAccountManagerRoleSystemName);
        if (b2BCustomerAccountManagerRole != null)
        {
            await _customerService.DeleteCustomerRoleAsync(b2BCustomerAccountManagerRole);
        }

        #endregion
    }

    public List<KeyValuePair<string, string>> PluginResouces()
    {
        return new List<KeyValuePair<string, string>>
        {
            new ("Plugin.Misc.NopStation.ERPIntegrationCore.ErpAccount.Select",
            "Select"),
            new ("Plugin.Misc.NopStation.ERPIntegrationCore.Admin.Configuration.Fields.SelectedErpIntegrationPlugin",
            "ERP Integration Plugin"),
            new ("Plugin.Misc.NopStation.ERPIntegrationCore.Admin.Configuration.Fields.SelectedErpIntegrationPlugin.Hint",
            "Select an ERP Integration Plugin from these System Names."),
            new ("NopStation.Plugin.B2B.ERPIntegrationCore.Admin.Configuration.Fields.ShowDebugLog",
            "Show Debug Log"),
            new ("NopStation.Plugin.B2B.ERPIntegrationCore.Admin.Configuration.Fields.ShowDebugLog.Hint",
            "Check to enable showing debug logs."),
            new ("Plugin.Misc.NopStation.ERPIntegrationCore.Admin.Configuration.Title",
            "ERP Integration Core Settings"),
            new ("Plugin.Misc.NopStation.ERPIntegrationCore.Admin.Configuration.BlockTitle.Settings",
            "Settings"),
            new ("Plugins.Misc.NopStation.ERPIntegrationCore.Configuration.Updated",
            "ERPIntegrationCore settings has been updated successfully."),
        };
    }

    #endregion

    #region Methods

    public override string GetConfigurationPageUrl()
    {
        return $"{_webHelper.GetStoreLocation()}Admin/ERPIntegrationCore/Configure";
    }

    public override async Task InstallAsync()
    {
        await InsertB2BB2CCustomerRolesAsync();

        await _permissionService.InstallPermissionsAsync(new ErpPermissionProvider());

        await this.InstallPluginAsync();

        await base.InstallAsync();
    }

    public override async Task UpdateAsync(string currentVersion, string targetVersion)
    {
        if (targetVersion == currentVersion)
            return;

        await InsertB2BB2CCustomerRolesAsync();

        await _permissionService.InstallPermissionsAsync(new ErpPermissionProvider());

        var keyValuePairs = PluginResouces().ToDictionary(kv => kv.Key, kv => kv.Value);
        foreach (var keyValuePair in keyValuePairs)
        {
            await _localizationService.AddOrUpdateLocaleResourceAsync(keyValuePair.Key, keyValuePair.Value);
        }
        await base.UpdateAsync(currentVersion, targetVersion);
    }

    public override async Task UninstallAsync()
    {
        await DeleteB2BB2CCustomerRolesAsync();

        await _permissionService.UninstallPermissionsAsync(new ErpPermissionProvider());

        await this.UninstallPluginAsync();

        await base.UninstallAsync();
    }

    public async Task ManageSiteMapAsync(SiteMapNode rootNode)
    {
        var childNode = new SiteMapNode()
        {
            SystemName = "NopStation.ERPIntegrationCore.Configuration",
            Title = "Core Configuration",
            IconClass = "nav-icon fas fa-cogs",
            Visible = true,
            ActionName = "Configure",
            ControllerName = "ERPIntegrationCore"
        };

        var pluginNode = rootNode.ChildNodes.FirstOrDefault(x => x.SystemName == "Third party plugins");
        if (pluginNode != null)
        {
            pluginNode.Title = "B2B/B2C Plugins";
            pluginNode.ChildNodes.Add(childNode);
        }
    }

    #endregion
}
