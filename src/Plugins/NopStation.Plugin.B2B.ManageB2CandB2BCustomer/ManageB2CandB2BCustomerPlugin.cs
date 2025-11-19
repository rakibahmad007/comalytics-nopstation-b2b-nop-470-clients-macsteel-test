using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Core.Domain.Customers;
using Nop.Services.Cms;
using Nop.Services.Customers;
using Nop.Services.Localization;
using Nop.Services.Plugins;
using Nop.Web.Framework.Infrastructure;
using Nop.Web.Framework.Menu;
using NopStation.Plugin.B2B.B2BB2CFeatures;
using NopStation.Plugin.B2B.ManageB2CandB2BCustomer.Areas.Admin.Components;
using NopStation.Plugin.B2B.ManageB2CandB2BCustomer.Components;
using NopStation.Plugin.Misc.Core.Helpers;
using NopStation.Plugin.Misc.Core.Services;


namespace NopStation.Plugin.B2B.ManageB2CandB2BCustomer;
public class ManageB2CCustomerPlugin : BasePlugin, IWidgetPlugin, IAdminMenuPlugin
{
    #region Fields

    private readonly ILocalizationService _localizationService;
    private readonly IWebHelper _webHelper;
    private readonly ICustomerService _customerService;
    private const string THIRD_PARTY_PLUGINS = "Third party plugins";
    private const string PLUGIN_SYSTEM_NAME = "NopStation.Plugin.B2B.ManageB2CandB2BCustomer";
    private const string PLUGIN_ICON_CLASS = "nav-icon fas fa-cube";
    private const bool PLUGIN_VISIBLE = true;
    private const string CHILD_NODE_CONFIG_SYSTEM_NAME = "NopStation.Plugin.B2B.ManageB2CandB2BCustomer.Configuration";
    private const string CHILD_NODE_CONFIG_TITLE = "Configuration";
    private const string CHILD_NODE_CONFIG_CONTROLLER_NAME = "B2CShipToAddress";
    private const string CHILD_NODE_CONFIG_ACTION_NAME = "Configure";
    private const string CHILD_NODE_CONFIG_ICON_CLASS = "nav-icon fas fa-cogs";
    private const bool CHILD_NODE_CONFIG_VISIBLE = true;

    #endregion

    #region Ctor

    public ManageB2CCustomerPlugin(IWebHelper webHelper,
        ILocalizationService localizationService,
        ICustomerService customerService)
    {
        _webHelper = webHelper;
        _localizationService = localizationService;
        _customerService = customerService;
    }

    #endregion

    #region Utilities

    public static List<KeyValuePair<string, string>> GetPluginResources()
    {
        var list = new Dictionary<string, string>()
        {
            ["NopStation.Plugin.B2B.ManageB2CandB2BCustomer.Fields.ServiceUrl"] = "Service URL",
            ["NopStation.Plugin.B2B.ManageB2CandB2BCustomer.Fields.ServiceUrl.Hint"] = "Service URL of ManageB2CandB2BCustomer",
            ["NopStation.Plugin.B2B.ManageB2CandB2BCustomer.Fields.AuthTocken"] = "Auth Token",
            ["NopStation.Plugin.B2B.ManageB2CandB2BCustomer.Fields.AuthTocken.Hint"] = "Auth Token of ManageB2CandB2BCustomer",
            ["NopStation.Plugin.B2B.OverriddenB2BB2CCustomer.General"] = "Connection Details",
            ["NopStation.Plugin.B2B.ManageB2CandB2BCustomer.Address"] = "Enter your delivery address",
            ["NopStation.Plugin.B2B.ManageB2CandB2BCustomer.Fields.Latitude"] = "Latitutde",
            ["NopStation.Plugin.B2B.ManageB2CandB2BCustomer.Fields.Latitude.Required"] = "Latitude is required",
            ["NopStation.Plugin.B2B.ManageB2CandB2BCustomer.Fields.Longitude"] = "Longitude",
            ["NopStation.Plugin.B2B.ManageB2CandB2BCustomer.Fields.Longitude.Required"] = "Longitude is required",
            ["NopStation.Plugin.B2B.ManageB2CandB2BCustomer.Fields.HouseNumber"] = "Street Number",
            ["NopStation.Plugin.B2B.ManageB2CandB2BCustomer.Fields.HouseNumber.Required"] = "Street Number is required",
            ["NopStation.Plugin.B2B.ManageB2CandB2BCustomer.Fields.Street.Required"] = "Street is required",
            ["NopStation.Plugin.B2B.ManageB2CandB2BCustomer.Fields.Suburb"] = "Suburb",
            ["NopStation.Plugin.B2B.ManageB2CandB2BCustomer.Fields.Suburb.Required"] = "Suburb is required. If not applicable, please use the name of the town/ city.",
            ["NopStation.Plugin.B2B.ManageB2CandB2BCustomer.Fields.City.Required"] = "City is required",
            ["NopStation.Plugin.B2B.ManageB2CandB2BCustomer.Fields.StateProvince.Required"] = "State Province is required",
            ["NopStation.Plugin.B2B.ManageB2CandB2BCustomer.Fields.PostalCode.Required"] = "Postal Code is required",
            ["NopStation.Plugin.B2B.ManageB2CandB2BCustomer.Fields.Country.Required"] = "Country is required",
            ["NopStation.Plugin.B2B.ManageB2CandB2BCustomer.Fields.Country.MustBe.SouthAfrica"] = "Country must be South Africa",
            ["NopStation.Plugin.B2B.ManageB2CandB2BCustomer.Account.B2CRegister"] = "B2C Registration Failed",

            ["NopStation.Plugin.B2B.ManageB2CandB2BCustomer.B2CShipToAddressSelector.NoShop.Warning.Details"] = "Your default address is no longer an area Macsteel delivers to, please choose or add a new default address",
            ["NopStation.Plugin.B2B.ManageB2CandB2BCustomer.B2CShipToAddressSelector"] = "Change Delivery Address",
            ["NopStation.Plugin.B2B.ManageB2CandB2BCustomer.B2CShipToAddressSelector.SelectShippingAddress"] = "Select Your Default Delivery Address",
            ["NopStation.Plugin.B2B.ManageB2CandB2BCustomer.B2CShipToAddressSelector.Default"] = "(Default)",
            ["NopStation.Plugin.B2B.ManageB2CandB2BCustomer.B2CShipToAddressSelector.NextDefault"] = "Your new default address will be:",
            ["NopStation.Plugin.B2B.ManageB2CandB2BCustomer.B2CShipToAddressSelector.NextDefault.DifferentSalesOrg"] = "This address falls in a different region. With this change, products, prices and stock levels will be recalculated, and your cart will be cleared",
            ["NopStation.Plugin.B2B.ManageB2CandB2BCustomer.B2CShipToAddressSelector.NextDefault.ContinueOrCancel"] = "If this is correct, click Continue to proceed.",
            ["NopStation.Plugin.B2B.ManageB2CandB2BCustomer.B2CShipToAddressSelector.CollectionMessage"] = "*For collection - Please select collection method on checkout",
            ["NopStation.Plugin.B2B.ManageB2CandB2BCustomer.B2CShipToAddressSelector.Add"] = "Add New",
            ["NopStation.Plugin.B2B.ManageB2CandB2BCustomer.B2CShipToAddressSelector.Continue"] = "Continue",
            ["NopStation.Plugin.B2B.ManageB2CandB2BCustomer.B2CShipToAddressSelector.Update.Failed"] = "Failed to Update shipping address",
            ["NopStation.Plugin.B2B.ManageB2CandB2BCustomer.B2CShipToAddressSelector.Update.Success"] = "Shipping Address updated successfully",
            ["NopStation.Plugin.B2B.ManageB2CandB2BCustomer.B2CShipToAddressSelector.Update.Warning.Text"] = "The selected address falls into a different region. With this change, products, prices and stock levels will be recalculated, and your cart will be cleared",
            ["NopStation.Plugin.B2B.ManageB2CandB2BCustomer.B2CShipToAddressSelector.Update.Same.Text"] = "You've made no changes to your delivery address",
            ["NopStation.Plugin.B2B.ManageB2CandB2BCustomer.B2CShipToAddressSelector.Only.One.Address.Text"] = "A default delivery address is required. Please add a new delivery address before deleting the default address.",
            ["NopStation.Plugin.B2B.ManageB2CandB2BCustomer.B2CShipToAddressSelector.Create"] = "Create New Delivery Address",
            ["NopStation.Plugin.B2B.ManageB2CandB2BCustomer.B2CShipToAddressSelector.Save"] = "Save",
            ["NopStation.Plugin.B2B.ManageB2CandB2BCustomer.B2CShipToAddressSelector.Warning"] = "Warning",
            ["NopStation.Plugin.B2B.ManageB2CandB2BCustomer.B2CShipToAddressSelector.SalesOrgDifferentwarningDialog.Text"] = "The selected address falls into a different region. With this change, products, prices and stock levels will be recalculated, and your cart will be cleared.",
            ["NopStation.Plugin.B2B.ManageB2CandB2BCustomer.B2CShipToAddressSelector.Cancel"] = "Cancel",
            ["NopStation.Plugin.B2B.ManageB2CandB2BCustomer.B2CShipToAddressSelector.Create.Error.Title"] = "Error Creating New Address",
            ["NopStation.Plugin.B2B.ManageB2CandB2BCustomer.B2CShipToAddressSelector.Create.Error.Text"] = "There was an error creating the address. Please try again, or contact support",
            ["NopStation.Plugin.B2B.ManageB2CandB2BCustomer.B2CShipToAddressSelector.IsShipToAddressAlreadyExist.True"] = "This delivery address already exists on your profile, please enter a new delivery address to continue",
            ["NopStation.Plugin.B2B.ManageB2CandB2BCustomer.B2CShipToAddressSelector.Already.Exists.Cancel"] = "Close",
            ["NopStation.Plugin.B2B.ManageB2CandB2BCustomer.B2CShipToAddressSelector.Notification"] = "Notification",
            ["NopStation.Plugin.B2B.ManageB2CandB2BCustomer.B2CShipToAddressSelector.IsShipToAddressIsInDeliveryRoute.False.Notification"] = "Please note your address falls outside Macsteel's delivery routes, and therefore, you will only be able to choose to collect on checkout.",
            ["NopStation.Plugin.B2B.ManageB2CandB2BCustomer.B2CShipToAddressSelector.ShipToAddress.Created.Successfully"] = "The delivery address has been created successfully, and set as your default delivery address.",
            ["NopStation.Plugin.B2B.ManageB2CandB2BCustomer.B2CShipToAddressSelector.Delete.Successful"] = "Shipping address was deleted successfully",
            ["NopStation.Plugin.B2B.ManageB2CandB2BCustomer.B2CShipToAddressSelector.Delete.Failed"] = "Failed to delete shipping address",

            ["NopStation.Plugin.B2B.ManageB2CandB2BCustomer.B2CShipToAddress.Create.CountryOrStateProvinceNotFound"] = "Country or State/Province not found",
            ["NopStation.Plugin.B2B.ManageB2CandB2BCustomer.B2CShipToAddress.Create.Successful"] = "Address created successfully",
            ["Plugins.Payments.B2BCustomerAccount.B2CShipToAddress.Create.Failed"] = "Address creation failed",
            ["Plugins.Payments.B2BCustomerAccount.B2CShipToAddress.Delete.Failed"] = "Address deletion failed",
            ["Plugins.Payments.B2BCustomerAccount.B2CShipToAddressSelector.NoShopZone.NotUpdatedPreviousOne"] = "Selected address was in no shop zone. Address was not updated.",
            ["NopStation.Plugin.B2B.ManageB2CandB2BCustomer.B2CShipToAddressSelector.Delete.Successful"] = "Address deleted successfully",
            ["Plugins.Payments.B2BCustomerAccount.B2CShipToAddress.Delete.Failed"] = "Address deletion failed",
            ["NopStation.Plugin.B2B.ManageB2CandB2BCustomer.B2CShipToAddressSelector.Update.Failed"] = "Address update failed",
            ["NopStation.Plugin.B2B.ManageB2CandB2BCustomer.B2BCheckout.B2CShipToAddress.DeliveryOption.NoShop"] = "Present shipping address is in no-shop zone.",
            ["NopStation.Plugin.B2B.ManageB2CandB2BCustomer.B2BCheckout.B2CShipToAddress.DeliveryOption.NotFound"] = "Delivery option not found",
            ["plugins.payments.b2bcustomeraccount.b2cshiptoaddressselector"] = "Change delivery address",
            ["NopStation.Plugin.B2B.ManageB2CandB2BCustomer.B2CRegister.GivenAddressInNoShopZone.Title"] = "Area in no-shop zone",
            ["NopStation.Plugin.B2B.ManageB2CandB2BCustomer.B2CRegister.GivenAddressInNoShopZone.Message"] = "We unfortunately do not sell in your area at this time. Please try again later.",
            ["B2BB2C.Account.Registration.AccountNotCreated.SoltrackResponse.Error"] = "We have a problem validating your address. Please contact us to assist with the registration process.",
            ["B2BB2C.Account.Registration.AccountNotCreated.SoltrackResponse.Warehouse.NotFound"] = "Warehouse not found for this location. Please try with another location.",
            ["B2BB2C.Account.Registration.AccountNotCreated.LatitudeOrLongitudeNotFound"] = "Latitude Or Longitude was not found. Please try with another location.",
            ["Plugin.B2B.ManageB2CandB2BCustomer.Account.Fields.AccountNumber.Required"] = "Account Number Required",
            ["Plugin.B2B.ManageB2CandB2BCustomer.Account.Fields.AccountNumber.Invalid"] = "Account Number Invalid. If you don’t have an account with Macsteel, please register for cash sales. Please click the “Register” icon top right and then select “No”.",
            ["Plugin.B2B.ManageB2CandB2BCustomer.Account.Fields.JobTitle.Required"] = "Job Title Required",
            ["Plugin.B2B.ManageB2CandB2BCustomer.Account.Fields.AuthorisationFullName.Required"] = "Name is Required",
            ["Plugin.B2B.ManageB2CandB2BCustomer.Account.Fields.AuthorisationContactNumber.Required"] = "Contact Number Required",
            ["Plugin.B2B.ManageB2CandB2BCustomer.Account.Fields.AuthorisationJobTitle.Required"] = "Job Title Required",
            ["Plugin.B2B.ManageB2CandB2BCustomer.Account.Fields.Password.Required"] = "Password is Required",
            ["Plugins.B2B.ManageB2CAndB2BCustomer.Areas.Admin.B2CRegistrationInfo.Fields.NearestWarehouse"] = "Nearest Warehouse",
            ["Plugins.B2B.ManageB2BCustomer.Admin.Customers.ErpRegisterInfo"] = "Erp Register Info",
        };

        return list.ToList();
    }

    private async Task InsertManageB2CandB2BCustomerRolesAsync()
    {
        if (await _customerService.GetCustomerRoleBySystemNameAsync(ManageB2CandB2BCustomerDefaults.B2CNoShopRoleSystemName) is null)
        {
            await _customerService.InsertCustomerRoleAsync(new CustomerRole
            {
                Name = ManageB2CandB2BCustomerDefaults.B2CNoShopRoleSystemName,
                Active = true,
                IsSystemRole = false,
                SystemName = ManageB2CandB2BCustomerDefaults.B2CNoShopRoleSystemName
            });
        }

        if (await _customerService.GetCustomerRoleBySystemNameAsync(ManageB2CandB2BCustomerDefaults.B2CRegistrationUnsuccessfulRoleSystemName) is null)
        {
            await _customerService.InsertCustomerRoleAsync(new CustomerRole
            {
                Name = ManageB2CandB2BCustomerDefaults.B2CRegistrationUnsuccessfulRoleSystemName,
                Active = true,
                IsSystemRole = false,
                SystemName = ManageB2CandB2BCustomerDefaults.B2CRegistrationUnsuccessfulRoleSystemName
            });
        }
    }

    #endregion

    #region Method

    /// <summary>
    /// Gets a configuration page URL
    /// </summary>
    public override string GetConfigurationPageUrl()
    {
        return $"{_webHelper.GetStoreLocation()}Admin/B2CShipToAddress/Configure";
    }

    public override async Task InstallAsync()
    {
        await InsertManageB2CandB2BCustomerRolesAsync();

        var keyValuePairs = GetPluginResources().ToDictionary(kv => kv.Key, kv => kv.Value);

        foreach (var keyValuePair in keyValuePairs)
        {
            await _localizationService.AddOrUpdateLocaleResourceAsync(keyValuePair.Key, keyValuePair.Value);
        }

        await base.InstallAsync();
    }

    public override async Task UninstallAsync()
    {
        await base.UninstallAsync();
    }

    public override async Task UpdateAsync(string currentVersion, string targetVersion)
    {
        if (targetVersion == currentVersion)
            return;

        await InsertManageB2CandB2BCustomerRolesAsync();

        var keyValuePairs = GetPluginResources().ToDictionary(kv => kv.Key, kv => kv.Value);
        foreach (var keyValuePair in keyValuePairs)
        {
            await _localizationService.AddOrUpdateLocaleResourceAsync(keyValuePair.Key, keyValuePair.Value);
        }

        await base.UpdateAsync(currentVersion, targetVersion);
    }

    public async Task ManageSiteMapAsync(SiteMapNode rootNode)
    {
        var pluginNode = rootNode.ChildNodes.FirstOrDefault(x => x.SystemName == THIRD_PARTY_PLUGINS);

        if (pluginNode is null)
        {
            return;
        }

        pluginNode.ChildNodes.Add(new()
        {
            SystemName = PLUGIN_SYSTEM_NAME,
            Title = await _localizationService.GetResourceAsync("Plugin.ManageB2bAndB2cCustomer.SiteMapName.Title"),
            IconClass = PLUGIN_ICON_CLASS,
            Visible = PLUGIN_VISIBLE,
            ChildNodes = new List<SiteMapNode>() {
                new()
                {
                    SystemName = CHILD_NODE_CONFIG_SYSTEM_NAME,
                    Title = CHILD_NODE_CONFIG_TITLE,
                    ControllerName = CHILD_NODE_CONFIG_CONTROLLER_NAME,
                    ActionName = CHILD_NODE_CONFIG_ACTION_NAME,
                    IconClass = CHILD_NODE_CONFIG_ICON_CLASS,
                    Visible = CHILD_NODE_CONFIG_VISIBLE
                }
            }
        });
    }

    #region Widget Plugin

    public Type GetWidgetViewComponent(string widgetZone)
    {
        if (widgetZone.Equals(B2BB2CFeaturesDefaults.CustomZoneForB2CShipToAddressSelector))
            return typeof(B2CShipToAddressSelectorViewComponent);
        else if (widgetZone.Equals(AdminWidgetZones.CustomerDetailsBlock))
            return typeof(NopCustomerRegisterInfoViewComponent);
        else
            return null;
    }

    public Task<IList<string>> GetWidgetZonesAsync()
    {
        var pluginAssembly = Assembly.GetExecutingAssembly();

        if (!NopInstance.Load<ILicenseService>().IsLicensedAsync(pluginAssembly).Result)
        {
            return Task.FromResult<IList<string>>(new List<string> { string.Empty });
        }

        return Task.FromResult<IList<string>>(new List<string> { B2BB2CFeaturesDefaults.CustomZoneForB2CShipToAddressSelector, AdminWidgetZones.CustomerDetailsBlock });
    }
    public bool HideInWidgetList => false;

    #endregion

    #endregion
}
