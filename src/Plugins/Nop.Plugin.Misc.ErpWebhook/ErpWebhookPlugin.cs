using System.Threading.Tasks;
using Nop.Core;
using Nop.Core.Domain.Customers;
using Nop.Services.Common;
using Nop.Services.Configuration;
using Nop.Services.Customers;
using Nop.Services.Localization;
using Nop.Services.Plugins;
using Nop.Services.ScheduleTasks;
using Nop.Web.Framework.Menu;

namespace Nop.Plugin.Misc.ErpWebhook;

public class ErpWebhookPlugin : BasePlugin, IMiscPlugin, IAdminMenuPlugin
{
    #region Fields

    private readonly ICustomerService _customerService;
    private readonly ILocalizationService _localizationService;
    private readonly IWebHelper _webHelper;
    private readonly ISettingService _settingService;
    private readonly IWorkContext _workContext;
    private readonly IScheduleTaskService _scheduleTaskService;

    #endregion

    #region Ctor

    public ErpWebhookPlugin(
        ILocalizationService localizationService,
        IWebHelper webHelper,
        ISettingService settingService,
        ICustomerService customerService,
        IWorkContext workContext,
        IScheduleTaskService scheduleTaskService
    )
    {
        _localizationService = localizationService;
        _webHelper = webHelper;
        _settingService = settingService;
        _customerService = customerService;
        _workContext = workContext;
        _scheduleTaskService = scheduleTaskService;
    }

    #endregion

    #region Utilities

    protected async Task AddErpWebhookSettingsAsync()
    {
        var settings = new ErpWebhookSettings
        {
            WebhookSecretKey = string.Empty,
            AccounthookAlreadyRunning = false,
            ShiptoAddresshookAlreadyRunning = false,
            ProducthookAlreadyRunning = false,
            StockhookAlreadyRunning = false,
            AccountPricinghookAlreadyRunning = false,
            OrderhookAlreadyRunning = false,
            CredithookAlreadyRunning = false,
            DeliveryDateshookAlreadyRunning = false,
        };

        await _settingService.SaveSettingAsync(settings);
    }

    private async Task AddErpWebhookManagerRoleAsync()
    {
        var customerRole = await _customerService.GetCustomerRoleBySystemNameAsync(
            ErpWebhookDefaults.ErpWebhookManagerRoleSystemName
        );
        if (customerRole == null)
        {
            customerRole = new CustomerRole
            {
                Name = ErpWebhookDefaults.ErpWebhookManagerRoleName,
                FreeShipping = false,
                TaxExempt = false,
                Active = true,
                IsSystemRole = false,
                SystemName = ErpWebhookDefaults.ErpWebhookManagerRoleSystemName,
                EnablePasswordLifetime = false,
                OverrideTaxDisplayType = false,
            };

            await _customerService.InsertCustomerRoleAsync(customerRole);
        }
    }

    private async Task AddOrUpdatePluginLocaleResourceAsync()
    {
        await _localizationService.AddOrUpdateLocaleResourceAsync(
            "Plugins.Misc.ErpWebhook.Fields.WebhookSecretKey",
            "Token"
        );
        await _localizationService.AddOrUpdateLocaleResourceAsync(
            "Plugins.Misc.ErpWebhook.Fields.WebhookSecretKey.Hint",
            "Generated Token"
        );
        await _localizationService.AddOrUpdateLocaleResourceAsync(
            "Plugins.Misc.ErpWebhook.Fields.AccountPrefilterFacets",
            "Account Prefilter Facets"
        );
        await _localizationService.AddOrUpdateLocaleResourceAsync(
            "Plugins.Misc.ErpWebhook.Fields.AccountPrefilterFacets.Hint",
            "Account Prefilter Facets"
        );
        await _localizationService.AddOrUpdateLocaleResourceAsync(
            "Plugins.Misc.ErpWebhook.Fields.AccountsDefaultAllowOverspend",
            "Accounts Default Allow Overspend"
        );
        await _localizationService.AddOrUpdateLocaleResourceAsync(
            "Plugins.Misc.ErpWebhook.Fields.AccountsDefaultAllowOverspend.Hint",
            "Accounts Default Allow Overspend"
        );
        await _localizationService.AddOrUpdateLocaleResourceAsync(
            "Plugins.Misc.ErpWebhook.Fields.OverrideLowStockActivityId",
            "Override Low Stock Activity"
        );
        await _localizationService.AddOrUpdateLocaleResourceAsync(
            "Plugins.Misc.ErpWebhook.Fields.OverrideLowStockActivityId.Hint",
            "Override Low Stock Activity"
        );
        await _localizationService.AddOrUpdateLocaleResourceAsync(
            "Plugins.Misc.ErpWebhook.Fields.LowStockActivityIdDefaultValue",
            "Default Value - Low Stock Activity Id"
        );
        await _localizationService.AddOrUpdateLocaleResourceAsync(
            "Plugins.Misc.ErpWebhook.Fields.LowStockActivityIdDefaultValue.Hint",
            "Default Value - Low Stock Activity Id"
        );
        await _localizationService.AddOrUpdateLocaleResourceAsync(
            "Plugins.Misc.ErpWebhook.Fields.OverrideBackorderModeId",
            "Override Backorder Mode"
        );
        await _localizationService.AddOrUpdateLocaleResourceAsync(
            "Plugins.Misc.ErpWebhook.Fields.OverrideBackorderModeId.Hint",
            "Override Backorder Mode"
        );
        await _localizationService.AddOrUpdateLocaleResourceAsync(
            "Plugins.Misc.ErpWebhook.Fields.BackorderModeIdDefaultValue",
            "Backorder Mode Id"
        );
        await _localizationService.AddOrUpdateLocaleResourceAsync(
            "Plugins.Misc.ErpWebhook.Fields.BackorderModeIdDefaultValue.Hint",
            "Default Value: Backorder Mode Id"
        );
        await _localizationService.AddOrUpdateLocaleResourceAsync(
            "Admin.ErpWebhookAdmin.Configuration.Settings",
            "Configure"
        );
        await _localizationService.AddOrUpdateLocaleResourceAsync(
            "ErpWebhook.Login.Permission.Denied",
            "User don't have Erp webhook manager role"
        );
        await _localizationService.AddOrUpdateLocaleResourceAsync(
            "ErpWebhook.Login.CustomerRole",
            "Please check if the user have Erp webhook manager role"
        );
        await _localizationService.AddOrUpdateLocaleResourceAsync(
            "Erpwebhook.Response.InvalidToken",
            "Invalid token"
        );
        await _localizationService.AddOrUpdateLocaleResourceAsync(
            "Erpwebhook.Response.TokenExpired",
            "Token expired"
        );
        await _localizationService.AddOrUpdateLocaleResourceAsync(
            "Plugins.Misc.ErpWebhook.Fields.IpAddress",
            "Ip Address"
        );
        await _localizationService.AddOrUpdateLocaleResourceAsync(
            "Plugins.Misc.ErpWebhook.Fields.IpAddress.AlreadyExist",
            "Ip Address already exist"
        );
        await _localizationService.AddOrUpdateLocaleResourceAsync(
            "Plugins.Misc.ErpWebhook.AdminMenu.IpAddress",
            "Allowed Ip Address"
        );
        await _localizationService.AddOrUpdateLocaleResourceAsync(
            "Plugins.Misc.ErpWebhook.AdminMenu.Configuration",
            "Configuration"
        );
        await _localizationService.AddOrUpdateLocaleResourceAsync(
            "Plugins.Misc.ErpWebhook.Allowed.IpAddress",
            "Allowed Ip Addresses To Access Erp Webhook"
        );
        await _localizationService.AddOrUpdateLocaleResourceAsync(
            "Plugins.Misc.ErpWebhook.Fields.DefaultCountryThreeLetterIsoCode",
            "Default Country"
        );
    }

    private async Task DeletePluginLocaleResourceAsync()
    {
        await _localizationService.DeleteLocaleResourceAsync(
            "Plugins.Misc.ErpWebhook.Fields.WebhookSecretKey"
        );
        await _localizationService.DeleteLocaleResourceAsync(
            "Plugins.Misc.ErpWebhook.Fields.WebhookSecretKey.Hint"
        );
        await _localizationService.DeleteLocaleResourceAsync(
            "Plugins.Misc.ErpWebhook.Fields.AccountPrefilterFacets"
        );
        await _localizationService.DeleteLocaleResourceAsync(
            "Plugins.Misc.ErpWebhook.Fields.AccountPrefilterFacets.Hint"
        );
        await _localizationService.DeleteLocaleResourceAsync(
            "Plugins.Misc.ErpWebhook.Fields.AccountsDefaultAllowOverspend"
        );
        await _localizationService.DeleteLocaleResourceAsync(
            "Plugins.Misc.ErpWebhook.Fields.AccountsDefaultAllowOverspend.Hint"
        );
        await _localizationService.DeleteLocaleResourceAsync(
            "Plugins.Misc.ErpWebhook.Fields.OverrideLowStockActivityId"
        );
        await _localizationService.DeleteLocaleResourceAsync(
            "Plugins.Misc.ErpWebhook.Fields.OverrideLowStockActivityId.Hint"
        );
        await _localizationService.DeleteLocaleResourceAsync(
            "Plugins.Misc.ErpWebhook.Fields.LowStockActivityIdDefaultValue"
        );
        await _localizationService.DeleteLocaleResourceAsync(
            "Plugins.Misc.ErpWebhook.Fields.LowStockActivityIdDefaultValue.Hint"
        );
        await _localizationService.DeleteLocaleResourceAsync(
            "Plugins.Misc.ErpWebhook.Fields.OverrideBackorderModeId"
        );
        await _localizationService.DeleteLocaleResourceAsync(
            "Plugins.Misc.ErpWebhook.Fields.OverrideBackorderModeId.Hint"
        );
        await _localizationService.DeleteLocaleResourceAsync(
            "Plugins.Misc.ErpWebhook.Fields.BackorderModeIdDefaultValue"
        );
        await _localizationService.DeleteLocaleResourceAsync(
            "Plugins.Misc.ErpWebhook.Fields.BackorderModeIdDefaultValue.Hint"
        );
        await _localizationService.DeleteLocaleResourceAsync(
            "Admin.ErpWebhookAdmin.Configuration.Settings"
        );
        await _localizationService.DeleteLocaleResourceAsync("ErpWebhook.Login.Permission.Denied");
        await _localizationService.DeleteLocaleResourceAsync("ErpWebhook.Login.CustomerRole");
        await _localizationService.DeleteLocaleResourceAsync("Erpwebhook.Response.InvalidToken");
        await _localizationService.DeleteLocaleResourceAsync("Erpwebhook.Response.TokenExpired");
        await _localizationService.DeleteLocaleResourceAsync(
            "Plugins.Misc.ErpWebhook.Fields.IpAddress"
        );
        await _localizationService.DeleteLocaleResourceAsync(
            "Plugins.Misc.ErpWebhook.Fields.IpAddress.AlreadyExist"
        );
        await _localizationService.DeleteLocaleResourceAsync(
            "Plugins.Misc.ErpWebhook.AdminMenu.IpAddress"
        );
        await _localizationService.DeleteLocaleResourceAsync(
            "Plugins.Misc.ErpWebhook.AdminMenu.Configuration"
        );
        await _localizationService.DeleteLocaleResourceAsync(
            "Plugins.Misc.ErpWebhook.Allowed.IpAddress"
        );
    }

    private async void AddScheduleTasks()
    {
        await _scheduleTaskService.InsertTaskAsync(
            new()
            {
                Enabled = true,
                Seconds = 216000,
                StopOnError = false,
                Name = ErpWebhookDefaults.ParallelErpAccountToB2BAccountTaskName,
                Type = ErpWebhookDefaults.ParallelErpAccountToB2BAccountTaskType,
            }
        );

        await _scheduleTaskService.InsertTaskAsync(
            new()
            {
                Enabled = true,
                Seconds = 216000,
                StopOnError = false,
                Name = ErpWebhookDefaults.ParallelErpShipToAddressToShipToAddressTaskName,
                Type = ErpWebhookDefaults.ParallelErpShipToAddressToShipToAddressTaskType,
            }
        );

        await _scheduleTaskService.InsertTaskAsync(
            new()
            {
                Enabled = true,
                Seconds = 216000,
                StopOnError = false,
                Name = ErpWebhookDefaults.ParallelErpProductToB2BProductTaskName,
                Type = ErpWebhookDefaults.ParallelErpProductToB2BProductTaskType,
            }
        );

        await _scheduleTaskService.InsertTaskAsync(
            new()
            {
                Enabled = true,
                Seconds = 216000,
                StopOnError = false,
                Name = ErpWebhookDefaults.ParallelErpOrderToOrderTaskName,
                Type = ErpWebhookDefaults.ParallelErpOrderToOrderTaskType,
            }
        );

        await _scheduleTaskService.InsertTaskAsync(
            new()
            {
                Enabled = true,
                Seconds = 216000,
                StopOnError = false,
                Name = ErpWebhookDefaults.ParallelErpAccountPricingToB2BPerAccountPricingTaskName,
                Type = ErpWebhookDefaults.ParallelErpAccountPricingToB2BPerAccountPricingTaskType,
            }
        );

        await _scheduleTaskService.InsertTaskAsync(
            new()
            {
                Enabled = true,
                Seconds = 216000,
                StopOnError = false,
                Name = ErpWebhookDefaults.ParallelErpStockToB2BStockTaskName,
                Type = ErpWebhookDefaults.ParallelErpStockToB2BStockTaskType,
            }
        );
    }

    private async void RemoveScheduleTasks()
    {
        var b2bAcctask = await _scheduleTaskService.GetTaskByTypeAsync(
            ErpWebhookDefaults.ParallelErpAccountToB2BAccountTaskType
        );
        if (b2bAcctask != null)
            await _scheduleTaskService.DeleteTaskAsync(b2bAcctask);

        var shipToTask = await _scheduleTaskService.GetTaskByTypeAsync(
            ErpWebhookDefaults.ParallelErpShipToAddressToShipToAddressTaskType
        );
        if (shipToTask != null)
            await _scheduleTaskService.DeleteTaskAsync(shipToTask);

        var productTask = await _scheduleTaskService.GetTaskByTypeAsync(
            ErpWebhookDefaults.ParallelErpProductToB2BProductTaskType
        );
        if (productTask != null)
            await _scheduleTaskService.DeleteTaskAsync(productTask);

        var orderTask = await _scheduleTaskService.GetTaskByTypeAsync(
            ErpWebhookDefaults.ParallelErpOrderToOrderTaskType
        );
        if (orderTask != null)
            await _scheduleTaskService.DeleteTaskAsync(orderTask);

        var pricingTask = await _scheduleTaskService.GetTaskByTypeAsync(
            ErpWebhookDefaults.ParallelErpAccountPricingToB2BPerAccountPricingTaskType
        );
        if (pricingTask != null)
            await _scheduleTaskService.DeleteTaskAsync(pricingTask);

        var stockTask = await _scheduleTaskService.GetTaskByTypeAsync(
            ErpWebhookDefaults.ParallelErpStockToB2BStockTaskType
        );
        if (stockTask != null)
            await _scheduleTaskService.DeleteTaskAsync(stockTask);
    }

    #endregion

    /// <summary>
    /// Gets a configuration page URL
    /// </summary>
    public override string GetConfigurationPageUrl()
    {
        return _webHelper.GetStoreLocation() + "Admin/ErpWebhookAdmin/Configure";
    }

    /// <summary>
    /// Install plugin
    /// </summary>
    public override async Task InstallAsync()
    {
        await AddErpWebhookSettingsAsync();
        await AddErpWebhookManagerRoleAsync();
        await AddOrUpdatePluginLocaleResourceAsync();
        //AddScheduleTasks();

        await base.InstallAsync();
    }

    public override async Task UpdateAsync(string currentVersion, string targetVersion)
    {
        if (targetVersion == currentVersion)
            return;

        await AddErpWebhookManagerRoleAsync();
        await AddOrUpdatePluginLocaleResourceAsync();
    }

    /// <summary>
    /// Uninstall plugin
    /// </summary>
    public override async Task UninstallAsync()
    {
        await _settingService.DeleteSettingAsync<ErpWebhookSettings>();

        var customerRole = await _customerService.GetCustomerRoleBySystemNameAsync(
            ErpWebhookDefaults.ErpWebhookManagerRoleSystemName
        );
        if (customerRole != null)
            await _customerService.DeleteCustomerRoleAsync(customerRole);

        await DeletePluginLocaleResourceAsync();
        //RemoveScheduleTasks();

        await base.UninstallAsync();
    }

    public async Task ManageSiteMapAsync(SiteMapNode rootNode)
    {
        var customer = await _workContext.GetCurrentCustomerAsync();
        if (!await _customerService.IsAdminAsync(customer))
            return;

        var node = new SiteMapNode
        {
            IconClass = "fa fa-tags",
            Visible = true,
            Title = "Erp Webhook",
        };

        var configurationNode = new SiteMapNode
        {
            IconClass = "fa-dot-circle-o",
            Visible = true,
            Title = await _localizationService.GetResourceAsync(
                "Plugins.Misc.ErpWebhook.AdminMenu.Configuration"
            ),
            Url = "/Admin/ErpWebhookAdmin/Configure",
            SystemName = "ErpWebhookConfigure",
        };

        var ipAddress = new SiteMapNode
        {
            IconClass = "fa-dot-circle-o",
            Visible = true,
            Title = await _localizationService.GetResourceAsync(
                "Plugins.Misc.ErpWebhook.AdminMenu.IpAddress"
            ),
            Url = "/Admin/IpAddress/IpAddresses",
            SystemName = "IpAddressesConfigure",
        };

        node.ChildNodes.Add(configurationNode);
        node.ChildNodes.Add(ipAddress);
        rootNode.ChildNodes.Add(node);
    }
}
