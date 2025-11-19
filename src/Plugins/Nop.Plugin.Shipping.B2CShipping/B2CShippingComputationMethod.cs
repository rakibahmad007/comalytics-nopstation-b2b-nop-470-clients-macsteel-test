using Nop.Core;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Shipping;
using Nop.Services.Catalog;
using Nop.Services.Configuration;
using Nop.Services.Customers;
using Nop.Services.Localization;
using Nop.Services.Orders;
using Nop.Services.Plugins;
using Nop.Services.Shipping;
using Nop.Services.Shipping.Tracking;
using NopStation.Plugin.B2B.B2BB2CFeatures;
using NopStation.Plugin.B2B.B2BB2CFeatures.Services.ErpCustomerFunctionality;
using NopStation.Plugin.B2B.ERPIntegrationCore.Domain;
using NopStation.Plugin.B2B.ERPIntegrationCore.Model;
using NopStation.Plugin.B2B.ERPIntegrationCore.Services;

namespace Nop.Plugin.Shipping.B2CShipping
{
    public class B2CShippingComputationMethod : BasePlugin, IShippingRateComputationMethod
    {
        #region Fields

        private readonly ILocalizationService _localizationService;
        private readonly IPriceCalculationService _priceCalculationService;
        private readonly ISettingService _settingService;
        private readonly IShippingService _shippingService;
        private readonly IStoreContext _storeContext;
        private readonly IWebHelper _webHelper;
        private readonly B2CShippingSettings _b2CShippingSettings;
        private readonly IWorkContext _workContext;
        private readonly IShoppingCartService _shoppingCartService;
        private readonly IErpIntegrationPluginManager _erpIntegrationPluginManager;
        private readonly ICustomerService _customerService;
        private readonly IErpShipToAddressService _erpShipToAddressService;
        private readonly IErpCustomerFunctionalityService _erpCustomerFunctionalityService;
        private readonly IErpSalesOrgService _erpSalesOrgService;

        #endregion

        #region Ctor

        public B2CShippingComputationMethod(ILocalizationService localizationService,
            IPriceCalculationService priceCalculationService,
            ISettingService settingService,
            IShippingService shippingService,
            IStoreContext storeContext,
            IWebHelper webHelper,
            B2CShippingSettings b2CShippingSettings,
            IWorkContext workContext,
            IShoppingCartService shoppingCartService,
            IErpIntegrationPluginManager erpIntegrationPluginManager,
            ICustomerService customerService,
            IErpShipToAddressService erpShipToAddressService,
            IErpCustomerFunctionalityService erpCustomerFunctionalityService,
            IErpSalesOrgService erpSalesOrgService)
        {
            _localizationService = localizationService;
            _priceCalculationService = priceCalculationService;
            _settingService = settingService;
            _shippingService = shippingService;
            _storeContext = storeContext;
            _webHelper = webHelper;
            _b2CShippingSettings = b2CShippingSettings;
            _workContext = workContext;
            _shoppingCartService = shoppingCartService;
            _erpIntegrationPluginManager = erpIntegrationPluginManager;
            _customerService = customerService;
            _erpShipToAddressService = erpShipToAddressService;
            _erpCustomerFunctionalityService = erpCustomerFunctionalityService;
            _erpSalesOrgService = erpSalesOrgService;
        }

        #endregion

        #region Utilities



        #endregion

        #region Methods

        /// <summary>
        ///  Gets available shipping options
        /// </summary>
        /// <param name="getShippingOptionRequest">A request for getting shipping options</param>
        /// <returns>Represents a response of getting shipping rate options</returns>
        public async Task<GetShippingOptionResponse> GetShippingOptionsAsync(GetShippingOptionRequest getShippingOptionRequest)
        {
            if (getShippingOptionRequest == null)
                throw new ArgumentNullException(nameof(getShippingOptionRequest));

            var response = new GetShippingOptionResponse();

            // Check if customer role is allowed to get options
            if (string.IsNullOrWhiteSpace(_b2CShippingSettings.AllowedRoleIds))
            {
                return response;
            }

            var currentCustomer = await _workContext.GetCurrentCustomerAsync();
            var customerRoles = await _customerService.GetCustomerRolesAsync(currentCustomer);

            // Parse valid role IDs from settings
            var validRoleIds = _b2CShippingSettings.AllowedRoleIds.Contains(",")
                ? _b2CShippingSettings.AllowedRoleIds.Split(',').Select(int.Parse).ToList()
                : new List<int> { int.Parse(_b2CShippingSettings.AllowedRoleIds) };

            // Check current customer's roles
            var currentCustomerRoleIds = customerRoles.Select(x => x.Id).ToList();
            if (!currentCustomerRoleIds.Intersect(validRoleIds).Any())
                return response;

            // Get the shopping cart
            var cart = await _shoppingCartService.GetShoppingCartAsync(currentCustomer, ShoppingCartType.ShoppingCart, _storeContext.GetCurrentStore().Id);
            if (!cart.Any())
                return response;

            // Calculate total weight of items in the cart
            var cartItemsWeightInKG = (await Task.WhenAll(cart.Select(x => _shippingService.GetShoppingCartItemWeightAsync(x))))
                .Sum(weight => weight);
            var totalWeightInKgs = cartItemsWeightInKG != 0 ? cartItemsWeightInKG : 0.00M;

            // Load active ERP integration plugin
            var erpIntegrationPlugin = await _erpIntegrationPluginManager.LoadActiveERPIntegrationPlugin();
            if (erpIntegrationPlugin == null)
            {
                response.AddError(await _localizationService.GetResourceAsync("Plugins.Payments.B2CShipping.NoERPPlugin"));
                return response;
            }

            var erpShipToAddress = await _erpShipToAddressService.GetErpShipToAddressByNopAddressIdAsync(getShippingOptionRequest?.ShippingAddress?.Id ?? 0);
            if(erpShipToAddress == null)
            {
                response.AddError(await _localizationService.GetResourceAsync("Plugins.Payments.B2CShipping.OptionLoadFailed.NoShipToAddressFound"));
                return response;
            }

            var erpAccount = await _erpCustomerFunctionalityService.GetActiveErpAccountByCustomerAsync(currentCustomer);
            if (erpAccount == null)
            {
                response.AddError(await _localizationService.GetResourceAsync("Plugins.Payments.B2CShipping.OptionLoadFailed.NoErpAccountFound"));
                return response;
            }

            var erpSalesOrg = await _erpSalesOrgService.GetErpSalesOrgByIdAsync(erpAccount.ErpSalesOrgId);
            if(erpSalesOrg == null)
            {
                response.AddError(await _localizationService.GetResourceAsync("Plugins.Payments.B2CShipping.OptionLoadFailed.NoSalesOrgFound"));
                return response;
            }

            var b2CShippingCost = await erpIntegrationPlugin.GetShippingRateFromERPAsync(new ErpGetRequestModel
            {
                Distance =  erpShipToAddress.DistanceToNearestWareHouse.ToString(),
                Location = erpSalesOrg.Code,
                Weight = totalWeightInKgs.ToString()
            });

            // Add shipping option with ERP rate
            if (b2CShippingCost != null &&
                decimal.TryParse(b2CShippingCost.ShippingRate, out var shippingRate))
            {
                response.ShippingOptions.Add(new ShippingOption
                {
                    Name = await _localizationService.GetResourceAsync(B2BB2CFeaturesDefaults.B2CShippingOptionName),
                    Rate = shippingRate,
                    Description = await _localizationService.GetResourceAsync(B2BB2CFeaturesDefaults.B2CShippingOptionDescription),
                    ShippingRateComputationMethodSystemName = B2BB2CFeaturesDefaults.B2CShippingOptionSystemName
                });
            }
            else
            {
                response.AddError(await _localizationService.GetResourceAsync("Plugins.Payments.B2CShipping.OptionLoadFailed"));
            }

            return response;
        }


        /// <summary>
        /// Gets fixed shipping rate (if shipping rate computation method allows it and the rate can be calculated before checkout).
        /// </summary>
        /// <param name="getShippingOptionRequest">A request for getting shipping options</param>
        /// <returns>Fixed shipping rate; or null in case there's no fixed shipping rate</returns>
        public Task<decimal?> GetFixedRateAsync(GetShippingOptionRequest getShippingOptionRequest)
        {
            if (getShippingOptionRequest == null)
                throw new ArgumentNullException(nameof(getShippingOptionRequest));

            return null;
        }

        /// <summary>
        /// Gets a configuration page URL
        /// </summary>
        public override string GetConfigurationPageUrl()
        {
            return $"{_webHelper.GetStoreLocation()}Admin/B2CShipping/Configure";
        }

        /// <summary>
        /// Install plugin
        /// </summary>
        public override async Task InstallAsync()
        {
            //settings
            await _settingService.SaveSettingAsync(new B2CShippingSettings());
            await _localizationService.AddOrUpdateLocaleResourceAsync(new Dictionary<string, string>
            {
                ["Plugins.Payments.B2CShipping.Fields.AllowedRoleIds"] = "Allowed Roles",
                ["Plugins.Payments.B2CShipping.Fields.AllowedRoleIds.Hint"] = "Select roles",
                ["Plugins.Payments.B2CShipping.Options.OptionName"] = "Shipping Option Name",
                ["Plugins.Payments.B2CShipping.OptionLoadFailed"] = "Failed to load shipping options"
            });

            await base.InstallAsync();
        }

        /// <summary>
        /// Uninstall plugin
        /// </summary>
        public override async Task UninstallAsync()
        {
            //settings
            await _settingService.DeleteSettingAsync<B2CShippingSettings>();

            //locales
            await _localizationService.DeleteLocaleResourceAsync("Plugins.Payments.B2CShipping.Fields.AllowedRoleIds");
            await _localizationService.DeleteLocaleResourceAsync("Plugins.Payments.B2CShipping.Fields.AllowedRoleIds.Hint");
            await _localizationService.DeleteLocaleResourceAsync("Plugins.Payments.B2CShipping.Options.OptionName");
            await _localizationService.DeleteLocaleResourceAsync("Plugins.Payments.B2CShipping.OptionLoadFailed");

            await base.UninstallAsync();
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets a shipment tracker
        /// </summary>
        public async Task<IShipmentTracker> GetShipmentTrackerAsync()
        {
            await Task.Yield();

            // Uncomment the line below if you want to use a general shipment tracker
            // return await Task.FromResult(new GeneralShipmentTracker(EngineContext.Current.Resolve<ITypeFinder>()));

            return null; // Return null or the appropriate tracker implementation
        }


        #endregion
    }
}