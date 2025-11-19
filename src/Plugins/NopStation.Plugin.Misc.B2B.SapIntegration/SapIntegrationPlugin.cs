using Nop.Core;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Shipping;
using Nop.Services.Common;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Plugins;
using Nop.Web.Framework.Menu;
using NopStation.Plugin.B2B.ERPIntegrationCore.Domain;
using NopStation.Plugin.B2B.ERPIntegrationCore.Model;
using NopStation.Plugin.B2B.ERPIntegrationCore.Model.Common;
using NopStation.Plugin.B2B.ERPIntegrationCore.Services;
using NopStation.Plugin.Misc.B2B.SapIntegration.Services;
using NopStation.Plugin.Misc.Core.Services;
using IB2BSalesOrgService = NopStation.Plugin.Misc.B2B.SapIntegration.Services.IB2BSalesOrgService;

namespace NopStation.Plugin.Misc.B2B.SapIntegration;

public class SapIntegrationPlugin : BasePlugin, IAdminMenuPlugin, IErpIntegrationPlugin, IMiscPlugin, INopStationPlugin
{
    #region Fields

    private readonly ILocalizationService _localizationService;
    private readonly ISettingService _settingService;
    private readonly IWebHelper _webHelper;
    private readonly IB2BAccountService _b2BAccountService;
    private readonly IB2BProductService _b2BProductService;
    private readonly IB2BPricingService _b2BPricingService;
    private readonly IB2BStockService _b2BStockService;
    private readonly IB2BShipToAddressService _shipToAddressService;
    private readonly IB2BOrderService _erpOrderService;
    private readonly IB2BInvoiceService _b2BInvoiceService;
    private readonly IB2BDocumentService _erpDocumentService;
    private readonly IB2BShippingService _erpShippingService;
    private readonly IB2BSalesOrgService _erpSalesOrgService;
    private const string THIRD_PARTY_PLUGINS = "Third party plugins";
    private const string PLUGIN_SYSTEM_NAME = "Misc.B2B.SapIntegration";
    private const string PLUGIN_TITLE = "Sap Integration";
    private const string PLUGIN_ICON_CLASS = "nav-icon fas fa-cube";
    private const bool PLUGIN_VISIBLE = true;
    private const string CHILD_NODE_CONFIG_SYSTEM_NAME = "Misc.B2B.SapIntegration.Configuration";
    private const string CHILD_NODE_CONFIG_TITLE = "Configuration";
    private const string CHILD_NODE_CONFIG_CONTROLLER_NAME = "SapIntegration";
    private const string CHILD_NODE_CONFIG_ACTION_NAME = "Configure";
    private const string CHILD_NODE_CONFIG_ICON_CLASS = "nav-icon fas fa-cogs";
    private const bool CHILD_NODE_CONFIG_VISIBLE = true;

    #endregion

    #region Ctor

    public SapIntegrationPlugin(ILocalizationService localizationService,
        ISettingService settingService,
        IWebHelper webHelper,
        IB2BAccountService b2BAccountService,
        IB2BProductService b2BProductService,
        IB2BPricingService b2BPricingService,
        IB2BStockService b2BStockService,
        IB2BShipToAddressService shipToAddressService,
        IB2BOrderService erpOrderService,
        IB2BInvoiceService b2BInvoiceService,
        IB2BDocumentService erpDocumentService,
        IB2BShippingService erpShippingService,
        IB2BSalesOrgService erpSalesOrgService)
    {
        _localizationService = localizationService;
        _settingService = settingService;
        _webHelper = webHelper;
        _b2BAccountService = b2BAccountService;
        _b2BProductService = b2BProductService;
        _b2BPricingService = b2BPricingService;
        _b2BStockService = b2BStockService;
        _shipToAddressService = shipToAddressService;
        _erpOrderService = erpOrderService;
        _b2BInvoiceService = b2BInvoiceService;
        _erpDocumentService = erpDocumentService;
        _erpShippingService = erpShippingService;
        _erpSalesOrgService = erpSalesOrgService;
    }

    #endregion

    #region Methods

    #region Common

    /// <summary>
    /// Gets a configuration page URL
    /// </summary>
    public override string GetConfigurationPageUrl()
    {
        return $"{_webHelper.GetStoreLocation()}Admin/SapIntegration/Configure";
    }

    /// <summary>
    /// Install the plugin
    /// </summary>
    /// <returns>A task that represents the asynchronous operation</returns>
    public override async Task InstallAsync()
    {
        var keyValuePairs = PluginResouces().ToDictionary(kv => kv.Key, kv => kv.Value);
        foreach (var keyValuePair in keyValuePairs)
        {
            await _localizationService.AddOrUpdateLocaleResourceAsync(
                keyValuePair.Key,
                keyValuePair.Value
            );
        }

        //settings
        await _settingService.SaveSettingAsync(
            new SapIntegrationSettings
            {
                HttpCallMaxRetries = 5,
                HttpCallRestTimeInSeconds = 1,
            }
        );

        await base.InstallAsync();
    }

    public async Task ManageSiteMapAsync(SiteMapNode rootNode)
    {
        var pluginNode = rootNode.ChildNodes.FirstOrDefault(x =>
            x.SystemName == THIRD_PARTY_PLUGINS
        );

        if (pluginNode is null)
        {
            return;
        }

        pluginNode.ChildNodes.Add(
            new()
            {
                SystemName = PLUGIN_SYSTEM_NAME,
                Title = PLUGIN_TITLE,
                IconClass = PLUGIN_ICON_CLASS,
                Visible = PLUGIN_VISIBLE,
                ChildNodes = new List<SiteMapNode>()
                {
                    new()
                    {
                        SystemName = CHILD_NODE_CONFIG_SYSTEM_NAME,
                        Title = CHILD_NODE_CONFIG_TITLE,
                        ControllerName = CHILD_NODE_CONFIG_CONTROLLER_NAME,
                        ActionName = CHILD_NODE_CONFIG_ACTION_NAME,
                        IconClass = CHILD_NODE_CONFIG_ICON_CLASS,
                        Visible = CHILD_NODE_CONFIG_VISIBLE,
                    },
                },
            }
        );
    }

    public List<KeyValuePair<string, string>> PluginResouces()
    {
        return new List<KeyValuePair<string, string>>
        {
            new (
                "NopStation.Plugin.Misc.B2B.SapIntegration.General",
                "Connection details"
            ),
            new (
                "NopStation.Plugin.Misc.B2B.SapIntegration.Fields.HttpCallMaxRetries",
                "Max retries"
            ),
            new (
                "NopStation.Plugin.Misc.B2B.SapIntegration.Fields.HttpCallMaxRetries.Hint",
                "Max retries to connect with sap"
            ),
            new (
                "NopStation.Plugin.Misc.B2B.SapIntegration.Fields.HttpCallRestTimeInSeconds",
                "Delay between each retry"
            ),
            new (
                "NopStation.Plugin.Misc.B2B.SapIntegration.Fields.HttpCallRestTimeInSeconds.Hint",
                "Delay between each retry in seconds"
            ),
            new (
                "NopStation.Plugin.Misc.B2B.SapIntegration.Fields.Salesperson",
                "Salesperson"
            ),
            new (
                "NopStation.Plugin.Misc.B2B.SapIntegration.Fields.Salesperson.Hint",
                "Enter the salesperson associated with the customer."
            ),
            new (
                "NopStation.Plugin.Misc.B2B.SapIntegration.Fields.TermsCode",
                "Terms Code"
            ),
            new (
                "NopStation.Plugin.Misc.B2B.SapIntegration.Fields.TermsCode.Hint",
                "Specify the terms code for the customer."
            ),
            new (
                "NopStation.Plugin.Misc.B2B.SapIntegration.Fields.CustomerClass",
                "Customer Class"
            ),
            new (
                "NopStation.Plugin.Misc.B2B.SapIntegration.Fields.CustomerClass.Hint",
                "Define the customer class for categorization."
            ),
            new (
                "NopStation.Plugin.Misc.B2B.SapIntegration.Fields.ArStatementNo",
                "AR Statement No"
            ),
            new (
                "NopStation.Plugin.Misc.B2B.SapIntegration.Fields.ArStatementNo.Hint",
                "Enter the AR statement number for the customer."
            ),
            new (
                "NopStation.Plugin.Misc.B2B.SapIntegration.Fields.OrderType",
                "Order Type"
            ),
            new (
                "NopStation.Plugin.Misc.B2B.SapIntegration.Fields.OrderType.Hint",
                "Specify the type of order being created."
            ),
            new (
                "NopStation.Plugin.Misc.B2B.SapIntegration.Fields.NsProductClass_ShippingLine",
                "NsProductClass(Shipping Line)"
            ),
            new (
                "NopStation.Plugin.Misc.B2B.SapIntegration.Fields.NsProductClass_ShippingLine.Hint",
                "Enter the product class for shipping line items."
            ),
            new (
                "NopStation.Plugin.Misc.B2B.SapIntegration.Fields.NsProductClass_Promotion",
                "NsProductClass(Promotion)"
            ),
            new (
                "NopStation.Plugin.Misc.B2B.SapIntegration.Fields.NsProductClass_Promotion.Hint",
                "Enter the product class for promotional items."
            ),
            new (
                "NopStation.Plugin.Misc.B2B.SapIntegration.Fields.BankCode_Peach",
                "Bank Code(Peach)"
            ),
            new (
                "NopStation.Plugin.Misc.B2B.SapIntegration.Fields.BankCode_Peach.Hint",
                "Specify the bank code for Peach payments."
            ),
            new (
                "NopStation.Plugin.Misc.B2B.SapIntegration.Fields.BankCode_Loyalty",
                "Bank Code(Loyalty)"
            ),
            new (
                "NopStation.Plugin.Misc.B2B.SapIntegration.Fields.BankCode_Loyalty.Hint",
                "Specify the bank code for Loyalty payments."
            ),
            new (
                "NopStation.Plugin.Misc.B2B.SapIntegration.Fields.DefaultEmailToSyncErpOrders",
                "Default Email"
            ),
            new (
                "NopStation.Plugin.Misc.B2B.SapIntegration.Fields.DefaultEmailToSyncErpOrders.Hint",
                "Specify the default customer email address to be used for syncing ERP orders."
            ),
            new (
                "NopStation.Plugin.Misc.B2B.SapIntegration.Fields.DefaultFirstNameToSyncErpOrders",
                "Default First Name"
            ),
            new (
                "NopStation.Plugin.Misc.B2B.SapIntegration.Fields.DefaultFirstNameToSyncErpOrders.Hint",
                "Specify the default first name to be used for syncing ERP orders."
            ),
            new (
                "NopStation.Plugin.Misc.B2B.SapIntegration.Fields.DefaultLastNameToSyncErpOrders",
                "Default Last Name"
            ),
            new (
                "NopStation.Plugin.Misc.B2B.SapIntegration.Fields.DefaultLastNameToSyncErpOrders.Hint",
                "Specify the default last name to be used for syncing ERP orders."
            ),
            new (
                "NopStation.Plugin.Misc.B2B.SapIntegration.Fields.DefaultPhoneNumberToSyncErpOrders",
                "Default Phone Number"
            ),
            new (
                "NopStation.Plugin.Misc.B2B.SapIntegration.Fields.DefaultPhoneNumberToSyncErpOrders.Hint",
                "Specify the default phone number to be used for syncing ERP orders."
            ),
            new (
                "NopStation.Plugin.Misc.B2B.SapIntegration.Fields.DefaultMobileNumberToSyncErpOrders",
                "Default Mobile Number"
            ),
            new (
                "NopStation.Plugin.Misc.B2B.SapIntegration.Fields.DefaultMobileNumberToSyncErpOrders.Hint",
                "Specify the default mobile number to be used for syncing ERP orders."
            ),
            new (
                "NopStation.Plugin.Misc.B2B.SapIntegration.OrderSync",
                "Defaults values for ERP order sync"
            ),
            new (
                "NopStation.Plugin.NopStation.Plugin.Misc.B2B.SapIntegration.CustomerCreation",
                "Customer CreationS"
            ),
            new (
                "NopStation.Plugin.Misc.NopStation.Plugin.Misc.B2B.SapIntegration.OrderCreation",
                "Order Creation"
            ),
            new (
                "NopStation.Plugin.Misc.NopStation.Plugin.Misc.B2B.SapIntegration.Payment",
                "Payment creation"
            ),
            new (
                "NopStation.Plugin.Misc.B2B.SapIntegration.Fields.SAPCompanyCode",
                "SAP Company Code"
            ),
            new (
                "NopStation.Plugin.Misc.B2B.SapIntegration.Fields.SAPCompanyCode.Hint",
                "Specify the SAP Company Code."
            ),

            new (
                "NopStation.Plugin.Misc.B2B.SapIntegration.Fields.VendorName",
                "Vendor Name"
            ),
            new (
                "NopStation.Plugin.Misc.B2B.SapIntegration.Fields.VendorName.Hint",
                "Specify the name of the vendor."
            ),

            new (
                "NopStation.Plugin.Misc.B2B.SapIntegration.Fields.ShippingCostSKU",
                "Shipping Cost SKU"
            ),
            new (
                "NopStation.Plugin.Misc.B2B.SapIntegration.Fields.ShippingCostSKU.Hint",
                "Specify the SKU for shipping costs."
            ),

            new (
                "NopStation.Plugin.Misc.B2B.SapIntegration.Fields.B2BOrderTypeMappings",
                "B2B Order Type Mappings"
            ),
            new (
                "NopStation.Plugin.Misc.B2B.SapIntegration.Fields.B2BOrderTypeMappings.Hint",
                "Specify the mappings for B2B order types."
            ),

            new (
                "NopStation.Plugin.Misc.B2B.SapIntegration.Fields.B2COrderTypeMappings",
                "B2C Order Type Mappings"
            ),
            new (
                "NopStation.Plugin.Misc.B2B.SapIntegration.Fields.B2COrderTypeMappings.Hint",
                "Specify the mappings for B2C order types."
            ),

            new (
                "NopStation.Plugin.Misc.B2B.SapIntegration.Fields.AppServerHost",
                "App Server Host"
            ),
            new (
                "NopStation.Plugin.Misc.B2B.SapIntegration.Fields.AppServerHost.Hint",
                "Specify the host for the application server."
            ),

            new (
                "NopStation.Plugin.Misc.B2B.SapIntegration.Fields.SystemNumber",
                "System Number"
            ),
            new (
                "NopStation.Plugin.Misc.B2B.SapIntegration.Fields.SystemNumber.Hint",
                "Specify the system number."
            ),

            new (
                "NopStation.Plugin.Misc.B2B.SapIntegration.Fields.SystemID",
                "System ID"
            ),
            new (
                "NopStation.Plugin.Misc.B2B.SapIntegration.Fields.SystemID.Hint",
                "Specify the system ID."
            ),

            new (
                "NopStation.Plugin.Misc.B2B.SapIntegration.Fields.User",
                "User"
            ),
            new (
                "NopStation.Plugin.Misc.B2B.SapIntegration.Fields.User.Hint",
                "Specify the username."
            ),

            new (
                "NopStation.Plugin.Misc.B2B.SapIntegration.Fields.Password",
                "Password"
            ),
            new (
                "NopStation.Plugin.Misc.B2B.SapIntegration.Fields.Password.Hint",
                "Specify the password for the user."
            ),

            new (
                "NopStation.Plugin.Misc.B2B.SapIntegration.Fields.RepositoryPassword",
                "Repository Password"
            ),
            new (
                "NopStation.Plugin.Misc.B2B.SapIntegration.Fields.RepositoryPassword.Hint",
                "Specify the password for the repository."
            ),

            new (
                "NopStation.Plugin.Misc.B2B.SapIntegration.Fields.Client",
                "Client"
            ),
            new (
                "NopStation.Plugin.Misc.B2B.SapIntegration.Fields.Client.Hint",
                "Specify the client."
            ),

            new (
                "NopStation.Plugin.Misc.B2B.SapIntegration.Fields.Language",
                "Language"
            ),
            new (
                "NopStation.Plugin.Misc.B2B.SapIntegration.Fields.Language.Hint",
                "Specify the language."
            ),

            new (
                "NopStation.Plugin.Misc.B2B.SapIntegration.Fields.PoolSize",
                "Pool Size"
            ),
            new (
                "NopStation.Plugin.Misc.B2B.SapIntegration.Fields.PoolSize.Hint",
                "Specify the pool size."
            ),

            new (
                "NopStation.Plugin.Misc.B2B.SapIntegration.Fields.AliasUser",
                "Alias User"
            ),
            new (
                "NopStation.Plugin.Misc.B2B.SapIntegration.Fields.AliasUser.Hint",
                "Specify the alias user."
            ),
            new (
                "NopStation.Plugin.Misc.B2B.SapIntegration.ErpActivityLogs.EditConfigurations",
                "Edit Configurations"
            ),

            new (
                "NopStation.Plugin.Misc.B2B.SapIntegration.Configuration.Updated",
                "Configuration Updated"
            ),

            new (
                "NopStation.Plugin.Misc.B2B.SapIntegration.Configuration",
                "Configuration"
            ),
            new (
                "NopStation.Plugin.Misc.B2B.SapIntegration.sapsettings",
                "Sap Configuration"
            ),
            new (
                "Integration.IntegrationSecretKey.Hint", 
                "Enter any 16-character key (letters and digits only) to represent your token generation."
            ),
            new (
                "Integration.IntegrationSecretKey", 
                "Integration Token"
            ),
            new (
                "Integration.Response.TokenExpired", 
                "Token Expired"
            ),
            new (
                "Integration.Response.InvalidToken", 
                "Invalid Token"
            ),
            new (
                "Integration.Login.CustomerRole", 
                "Please check if the user have Administration role"
            ),
            new (
                "Integration.Login.Permission.Denied", 
                "User don't have administration role"
            ),
            new (
                "Integration.Login.InvalidIntegrationSecretKey", 
                "Integration SecretKey Is Not Set."
            ),
        };
    }

    /// <summary>
    /// Uninstall the plugin
    /// </summary>
    /// <returns>A task that represents the asynchronous operation</returns>
    public override async Task UninstallAsync()
    {
        //settings
        await _settingService.DeleteSettingAsync<SapIntegrationSettings>();

        //locales
        await _localizationService.DeleteLocaleResourcesAsync(
            "NopStation.Plugin.Misc.B2B.SapIntegration"
        );

        await _localizationService.DeleteLocaleResourceAsync(
            "Integration.IntegrationSecretKey.Hint"
        );

        await _localizationService.DeleteLocaleResourceAsync(
            "Integration.IntegrationSecretKey"
        );

        await _localizationService.DeleteLocaleResourceAsync(
            "Integration.Response.TokenExpired"
        );

        await _localizationService.DeleteLocaleResourceAsync(
            "Integration.Response.InvalidToken"
        );

        await _localizationService.DeleteLocaleResourceAsync(
            "Integration.Login.CustomerRole"
        );

        await _localizationService.DeleteLocaleResourceAsync(
            "Integration.Login.Permission.Denied"
        );
        await _localizationService.DeleteLocaleResourceAsync(
            "Integration.Login.InvalidIntegrationSecretKey"
        );
        await base.UninstallAsync();
    }

    #endregion

    #region Erp Integration Methods

    #region Erp Account

    //These are not implemented here as macsteel use webhook instead.
    public async Task<ErpResponseData<ErpAccountDataModel>> GetAccountFromErpAsync(
        ErpGetRequestModel erpRequest
    )
    {
        var erpAccounts = await _b2BAccountService.GetAccountsFromErpAsync(erpRequest);
        var response = new ErpResponseData<ErpAccountDataModel>
        {
            ErpResponseModel = new ErpResponseModel
            {
                IsError = erpAccounts.ErpResponseModel.IsError,
                StatusCode = erpAccounts.ErpResponseModel.StatusCode,
                ErrorShortMessage = erpAccounts.ErpResponseModel.ErrorShortMessage,
                ErrorFullMessage = erpAccounts.ErpResponseModel.ErrorFullMessage,
            },
        };

        if (erpAccounts.Data != null && erpAccounts.Data.Any())
        {
            response.Data = erpAccounts.Data.FirstOrDefault();
        }
        else
        {
            response.Data = new ErpAccountDataModel();
        }

        return response;
    }

    public async Task<ErpResponseData<IList<ErpAccountDataModel>>> GetAccountsFromErpAsync(ErpGetRequestModel erpRequest)
    {
        return await _b2BAccountService.GetAccountsFromErpAsync(erpRequest);
    }

    public async Task<ErpResponseData<IList<ErpAccountDataModel>>> GetAllAccountCreditFromErpAsync(ErpGetRequestModel erpRequest)
    {
        return await _b2BAccountService.GetAllAccountCreditFromErpAsync(erpRequest);
    }

    #endregion

    #region Order and Quote

    public async Task<ErpResponseModel> CreateOrderOnErpAsync(ErpPlaceOrderDataModel erpRequest)
    {
        return await _erpOrderService.CreateOrderOnErpAsync(erpRequest);
    }

    public async Task<ErpResponseData<IList<ErpPlaceOrderDataModel>>> GetOrderByAccountFromErpAsync(ErpGetRequestModel erpRequest)
    {
        return await _erpOrderService.GetOrderByAccountFromErpAsync(erpRequest);
    }

    public async Task<ErpResponseData<IList<ErpPlaceOrderDataModel>>> GetOrderByOrderNumberFromErpAsync(ErpGetRequestModel erpRequest)
    {
        throw new NotImplementedException();
    }

    public async Task<ErpResponseData<IList<ErpPlaceOrderDataModel>>> GetQuoteByAccountFromErpAsync(ErpGetRequestModel erpRequest)
    {
        throw new NotImplementedException();
    }

    public async Task<ErpResponseData<IList<ErpPlaceOrderDataModel>>> GetQuoteByQuoteNumberFromErpAsync(ErpGetRequestModel erpRequest)
    {
        throw new NotImplementedException();
    }

    #endregion

    #region Invoice

    public async Task<ErpResponseData<IList<ErpInvoiceDataModel>>> GetInvoiceByAccountNoFromErpAsync(ErpGetRequestModel erpRequest)
    {
        return await _b2BInvoiceService.GetInvoiceByAccountNoFromErpAsync(erpRequest);
    }

    public async Task<ErpResponseData<string>> GetInvoicePdfByteCodeByDocumentNoFromErpAsync(ErpGetRequestModel erpRequest)
    {
        return await _b2BInvoiceService.GetInvoicePdfByteCodeByDocumentNoFromErpAsync(erpRequest);
    }

    public async Task<ErpResponseData<string>> GetStatementPdfByteCodeFromErpAsync(ErpGetRequestModel erpRequest)
    {
        return await _b2BInvoiceService.GetStatementPdfByteCodeFromErpAsync(erpRequest);
    }

    #endregion

    #region Product

    public async Task<ErpResponseData<ErpProductDataModel>> GetProductByItemNoFromErpAsync(
        ErpGetRequestModel erpRequest
    )
    {
        var erpProducts = await _b2BProductService.GetProductsFromErpAsync(erpRequest);
        var response = new ErpResponseData<ErpProductDataModel>
        {
            ErpResponseModel = new ErpResponseModel
            {
                IsError = erpProducts.ErpResponseModel.IsError,
                StatusCode = erpProducts.ErpResponseModel.StatusCode,
                ErrorShortMessage = erpProducts.ErpResponseModel.ErrorShortMessage,
                ErrorFullMessage = erpProducts.ErpResponseModel.ErrorFullMessage,
            },
        };

        if (erpProducts.Data != null && erpProducts.Data.Any())
        {
            response.Data = erpProducts.Data.FirstOrDefault();
        }
        else
        {
            response.Data = new ErpProductDataModel();
        }

        return response;
    }

    public async Task<ErpResponseData<IList<ErpProductDataModel>>> GetProductsFromErpAsync(
        ErpGetRequestModel erpRequest
    )
    {
        return await _b2BProductService.GetProductsFromErpAsync(erpRequest);
    }

    public async Task<ErpResponseData<ErpProductImageDataModel>> GetProductImageFromErpAsync(
        ErpGetRequestModel erpRequest
    )
    {
        return await _b2BProductService.GetProductImageFromErpAsync(erpRequest);
    }

    public async Task<ErpResponseData<IList<ErpProductImageDataModel>>> GetProductImagesFromErpAsync(
        ErpGetRequestModel erpRequest
    )
    {
        return await _b2BProductService.GetProductImagesFromErpAsync(erpRequest);
    }

    #endregion

    #region Pricing

    public async Task<
        ErpResponseData<ErpPriceGroupPricingDataModel>
    > GetProductGroupPriceFromErpAsync(ErpGetRequestModel erpRequest)
    {
        var erpResponse = await GetProductGroupPricesFromErpAsync(erpRequest);
        var firstPricingData = erpResponse?.Data?.FirstOrDefault() ?? null;
        return new ErpResponseData<ErpPriceGroupPricingDataModel>() { Data = firstPricingData };
    }

    public async Task<
        ErpResponseData<IList<ErpPriceGroupPricingDataModel>>
    > GetProductGroupPricesFromErpAsync(ErpGetRequestModel erpRequest)
    {
        //macsteel only have per account pricing
        throw new NotImplementedException();
    }

    public async Task<
        ErpResponseData<ErpPriceSpecialPricingDataModel>
    > GetProductSpecialPriceFromErpAsync(ErpGetRequestModel erpRequest)
    {
        var perAccountProductPricings =
            await _b2BPricingService.GetPerAccountProductPricingFromErpAsync(erpRequest);
        var response = new ErpResponseData<ErpPriceSpecialPricingDataModel>
        {
            ErpResponseModel = new ErpResponseModel
            {
                IsError = perAccountProductPricings.ErpResponseModel.IsError,
                StatusCode = perAccountProductPricings.ErpResponseModel.StatusCode,
                ErrorShortMessage = perAccountProductPricings.ErpResponseModel.ErrorShortMessage,
                ErrorFullMessage = perAccountProductPricings.ErpResponseModel.ErrorFullMessage,
            },
        };

        if (perAccountProductPricings.Data != null && perAccountProductPricings.Data.Any())
        {
            var pricing = perAccountProductPricings.Data.FirstOrDefault();
            response.Data = pricing;
        }
        else
        {
            response.Data = new ErpPriceSpecialPricingDataModel();
        }

        return response;
    }

    public async Task<
        ErpResponseData<IList<ErpPriceSpecialPricingDataModel>>
    > GetProductSpecialPricesFromErpAsync(ErpGetRequestModel erpRequest)
    {
        return await _b2BPricingService.GetPerAccountProductPricingFromErpAsync(erpRequest);
    }

    #endregion

    #region Salesorg and warehouse

    public async Task<ErpResponseModel> GetSalesOrgsFromErpAsync(ErpGetRequestModel erpRequest)
    {
        throw new NotImplementedException();
    }

    public async Task<ErpResponseModel> GetSalesWarehouseFromErpAsync(ErpGetRequestModel erpRequest)
    {
        throw new NotImplementedException();
    }

    public async Task<ErpResponseData<IList<ErpAreaCodeResponseModel>>> GetAreaCodesForSalesOrgAsLocationAsync(ErpGetRequestModel erpRequest)
    {
        return await _erpSalesOrgService.GetAreaCodesFromErpAsync(erpRequest);
    }
    #endregion

    #region Stock

    public async Task<ErpResponseData<ErpStockDataModel>> GetStockByItemNoFromErpAsync(
        ErpGetRequestModel erpRequest
    )
    {
        var erpStock = await _b2BStockService.GetStockFromErpAsync(erpRequest);
        var response = new ErpResponseData<ErpStockDataModel>
        {
            ErpResponseModel = new ErpResponseModel
            {
                IsError = erpStock.ErpResponseModel.IsError,
                StatusCode = erpStock.ErpResponseModel.StatusCode,
                ErrorShortMessage = erpStock.ErpResponseModel.ErrorShortMessage,
                ErrorFullMessage = erpStock.ErpResponseModel.ErrorFullMessage,
            },
        };

        if (erpStock.Data != null && erpStock.Data.Any())
        {
            response.Data = erpStock.Data.FirstOrDefault();
        }
        else
        {
            response.Data = new ErpStockDataModel();
        }

        return response;
    }

    public async Task<ErpResponseData<IList<ErpStockDataModel>>> GetStocksFromErpAsync(
        ErpGetRequestModel erpRequest
    )
    {
        return await _b2BStockService.GetStockFromErpAsync(erpRequest);
    }

    #endregion

    #region Ship to address

    public async Task<
        ErpResponseData<IList<ErpShipToAddressDataModel>>
    > GetShipToAddressByAccountNumberFromErpAsync(ErpGetRequestModel erpRequest)
    {
        return await _shipToAddressService.GetShipToAddressFromErpAsync(erpRequest);
    }

    public async Task<string> GetSalesOrgCodeFromIntegrationSettings()
    {
        return string.Empty;
    }

    public Task<ErpResponseModel> CreateAccountNoErpAsync(
        ErpCreateAccountModel erpCreateAccountModel
    )
    {
        throw new NotImplementedException();
    }

    public async Task<IList<string>> GetTheProofOfDeliveryPDFDocumentListAsync(string documentNo)
    {
        var podDocumentList = await _erpDocumentService.GetPODDocumentListAsync(documentNo);

        return podDocumentList.Select(x => x.DocumentNumber).ToList();
    }

    public Task<ShippingOption> GetShippingCostFromERPAsync(
        decimal totalWeightInKgs,
        Customer currentCustomer,
        Address shippingAddress
    )
    {
        throw new NotImplementedException();
    }

    #endregion

    #region Shipping

    public async Task<ErpResponseModel> GetShippingRateFromERPAsync(ErpGetRequestModel erpRequest)
    {
        return await _erpShippingService.GetShippingRateAsync(erpRequest);
    }

    #endregion

    #region Document

    public async Task<byte[]> GetDocumentAsync(string documentNumber)
    {
        //ZEC_GET_INVOICE
        return await _erpDocumentService.GetDocumentAsync(documentNumber);
    }
    public async Task<ErpResponseData<IList<ErpProductImageDataModel>>> GetSpecSheetAsync(ErpGetRequestModel erpRequest)
    {
        return await _erpDocumentService.GetSpecSheetAsync(erpRequest);
    }

    public async Task<byte[]> GetDocumentForQuoteAsync(string documentNumber)
    {
        return await _erpDocumentService.GetDocumentForQuoteAsync(documentNumber);
    }

    public async Task<byte[]> GetDocumentForOrderAsync(string documentNumber)
    {
        return await _erpDocumentService.GetDocumentForOrderAsync(documentNumber);
    }

    public async Task<DownloadPodDocumentResponseModel> DownloadTheProofOfDeliveryPDFAsync(string documentNumber, string podDocumentNumber)
    {
        var podDocumentList = await _erpDocumentService.GetPODDocumentListAsync(documentNumber);

        var pod = podDocumentList.Where(x => x.DocumentNumber == podDocumentNumber).FirstOrDefault();
        if (pod == null)
            return null;

        return pod;
    }

    public async Task<byte[]> DownloadAccountStatementPDFAsync(
        ErpAccount erpAccount,
        DateTime dateFrom,
        DateTime dateTo
    )
    {
        return await _b2BAccountService.GetAccountStatementPDFAsync(erpAccount, dateFrom, dateTo);
    }

    public async Task<DownloadPodDocumentResponseModel> DownloadTheTestCertificatePDFAsync(string documentNumber, string testCertDocumentNumber)
    {
        var testCertificateList = await _erpDocumentService.GetTheTestCertificateListAsync(documentNumber);
        var testCertificate = testCertificateList.Where(tc => tc.DocumentNumber == documentNumber).FirstOrDefault();

        if (testCertificate == null)
            return null;
        return testCertificate;
    }

    public async Task<IList<string>> GetTheTestCertificatePDFDocumentListAsync(string documentNumber)
    {
        var testCertificateDocumentList = await _erpDocumentService.GetTheTestCertificateListAsync(documentNumber);

        return testCertificateDocumentList.Select(x => x.DocumentNumber).ToList();
    }

    public Task<
        ErpResponseData<IList<GetAccountStatementPDFDocumentListModel>>
    > GetAccountStatementPDFDocumentListAsync(ErpAccount erpAccount, bool loadListOnly = true)
    {
        throw new NotImplementedException();
    }

    public async Task<decimal?> GetAccountSavingsForTimePeriodAsync(ErpGetRequestModel erpRequest)
    {
        return await _b2BAccountService.GetAccountSavingsAsync(erpRequest);
    }

    public async Task<ErpResponseData<IList<ErpPriceSpecialPricingDataModel>>> ProductListLivePriceSync(ErpGetRequestModel erpRequest)
    {
        return await _b2BPricingService.GetPerAccountProductPricingFromErpAsync(erpRequest);
    }

    #endregion

    #endregion

    #endregion
}
