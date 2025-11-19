using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Messages;
using Nop.Web.Framework;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Mvc.Filters;
using NopStation.Plugin.B2B.ERPIntegrationCore.Domain;
using NopStation.Plugin.B2B.ERPIntegrationCore.Model;
using NopStation.Plugin.Misc.B2B.SapIntegration.Filters;
using NopStation.Plugin.Misc.B2B.SapIntegration.Models;
using NopStation.Plugin.Misc.B2B.SapIntegration.Services;

namespace NopStation.Plugin.Misc.B2B.SapIntegration.Controllers;

public class SapIntegrationController : BasePluginController
{
    #region Fields

    private readonly IStoreContext _storeContext;
    private readonly ISettingService _settingService;
    private readonly INotificationService _notificationService;
    private readonly ILocalizationService _localizationService;
    private readonly IB2BAccountService _erpAccountService;
    private readonly IB2BInvoiceService _b2BInvoiceService;
    private readonly IB2BStockService _b2BStockService;
    private readonly IB2BPricingService _b2BPricingService;
    private readonly IB2BOrderService _erpOrderService;
    private readonly IB2BProductService _b2BProductService;
    private readonly IB2BShipToAddressService _shipToAddressService;
    private readonly IB2BShippingService _erpShippingService;
    private readonly IB2BDocumentService _erpDocumentService;
    private readonly IB2BAccountService _b2BAccountService;
    private readonly IB2BSalesOrgService _erpSalesOrgService;
    private readonly SapIntegrationSettings _sapIntegrationSettings;

    #endregion

    #region Ctor

    public SapIntegrationController(
        IStoreContext storeContext,
        ISettingService settingService,
        INotificationService notificationService,
        ILocalizationService localizationService,
        IB2BAccountService erpAccountService,
        IB2BInvoiceService b2BInvoiceService,
        IB2BStockService b2BStockService,
        IB2BPricingService b2BPricingService,
        IB2BOrderService erpOrderService,
        IB2BProductService b2BProductService,
        IB2BShipToAddressService shipToAddressService,
        IB2BShippingService erpShippingService,
        IB2BDocumentService erpDocumentService,
        IB2BAccountService b2BAccountService,
        SapIntegrationSettings sapIntegrationSettings,
        IB2BSalesOrgService erpSalesOrgService)
    {
        _storeContext = storeContext;
        _settingService = settingService;
        _notificationService = notificationService;
        _localizationService = localizationService;
        _erpAccountService = erpAccountService;
        _b2BInvoiceService = b2BInvoiceService;
        _b2BStockService = b2BStockService;
        _b2BPricingService = b2BPricingService;
        _erpOrderService = erpOrderService;
        _b2BProductService = b2BProductService;
        _shipToAddressService = shipToAddressService;
        _erpShippingService = erpShippingService;
        _erpDocumentService = erpDocumentService;
        _b2BAccountService = b2BAccountService;
        _sapIntegrationSettings = sapIntegrationSettings;
        _erpSalesOrgService = erpSalesOrgService;
    }

    #endregion

    #region Utilities

    /// <summary>
    /// Prepare ConfigurationModel
    /// </summary>
    /// <param name="model">Model</param>
    /// <returns>A task that represents the asynchronous operation</returns>
    protected async Task PrepareModelAsync(ConfigurationModel model)
    {
        model.HttpCallMaxRetries = _sapIntegrationSettings.HttpCallMaxRetries;
        model.HttpCallRestTimeInSeconds = _sapIntegrationSettings.HttpCallRestTimeInSeconds;

        model.Salesperson = _sapIntegrationSettings.Salesperson;
        model.TermsCode = _sapIntegrationSettings.TermsCode;
        model.CustomerClass = _sapIntegrationSettings.CustomerClass;
        model.ArStatementNo = _sapIntegrationSettings.ArStatementNo;

        model.OrderType = _sapIntegrationSettings.OrderType;
        model.NsProductClass_ShippingLine = _sapIntegrationSettings.NsProductClass_ShippingLine;
        model.NsProductClass_Promotion = _sapIntegrationSettings.NsProductClass_Promotion;

        model.BankCode_Peach = _sapIntegrationSettings.BankCode_Peach;
        model.BankCode_Loyalty = _sapIntegrationSettings.BankCode_Loyalty;

        model.DefaultEmailToSyncErpOrders = _sapIntegrationSettings.DefaultEmailToSyncErpOrders;
        model.DefaultFirstNameToSyncErpOrders =
            _sapIntegrationSettings.DefaultFirstNameToSyncErpOrders;
        model.DefaultLastNameToSyncErpOrders =
            _sapIntegrationSettings.DefaultLastNameToSyncErpOrders;
        model.DefaultPhoneNumberToSyncErpOrders =
            _sapIntegrationSettings.DefaultPhoneNumberToSyncErpOrders;
        model.DefaultMobileNumberToSyncErpOrders =
            _sapIntegrationSettings.DefaultMobileNumberToSyncErpOrders;
        model.SAPCompanyCode = _sapIntegrationSettings.SAPCompanyCode;
        model.VendorName = _sapIntegrationSettings.VendorName;
        model.ShippingCostSKU = _sapIntegrationSettings.ShippingCostSKU;
        model.B2BOrderTypeMappings = _sapIntegrationSettings.B2BOrderTypeMappings;
        model.B2COrderTypeMappings = _sapIntegrationSettings.B2COrderTypeMappings;

        model.AppServerHost = _sapIntegrationSettings.AppServerHost;
        model.SystemNumber = _sapIntegrationSettings.SystemNumber;
        model.SystemID = _sapIntegrationSettings.SystemID;
        model.User = _sapIntegrationSettings.User;
        model.Password = _sapIntegrationSettings.Password;
        model.RepositoryPassword = _sapIntegrationSettings.RepositoryPassword;
        model.Client = _sapIntegrationSettings.Client;
        model.Language = _sapIntegrationSettings.Language;
        model.PoolSize = _sapIntegrationSettings.PoolSize;
        model.AliasUser = _sapIntegrationSettings.AliasUser;
        model.IntegrationSecretKey = _sapIntegrationSettings.IntegrationSecretKey;
    }

    #endregion

    #region Methods

    [AuthorizeAdmin]
    [Area(AreaNames.ADMIN)]
    public async Task<IActionResult> Configure()
    {
        var model = new ConfigurationModel();
        await PrepareModelAsync(model);

        return View("~/Plugins/Misc.B2B.SapIntegration/Views/Configure.cshtml", model);
    }

    [AuthorizeAdmin]
    [Area(AreaNames.ADMIN)]
    [HttpPost, ActionName("Configure")]
    [FormValueRequired("save")]
    public async Task<IActionResult> Configure(ConfigurationModel model)
    {
        if (!ModelState.IsValid)
            return await Configure();

        var storeId = await _storeContext.GetActiveStoreScopeConfigurationAsync();
        //var sapIntegrationSettings = await _settingService.LoadSettingAsync<SapIntegrationSettings>(storeId);

        _sapIntegrationSettings.HttpCallMaxRetries = model.HttpCallMaxRetries;
        _sapIntegrationSettings.HttpCallRestTimeInSeconds = model.HttpCallRestTimeInSeconds;

        _sapIntegrationSettings.Salesperson = model.Salesperson;
        _sapIntegrationSettings.TermsCode = model.TermsCode;
        _sapIntegrationSettings.CustomerClass = model.CustomerClass;
        _sapIntegrationSettings.ArStatementNo = model.ArStatementNo;

        _sapIntegrationSettings.OrderType = model.OrderType;
        _sapIntegrationSettings.NsProductClass_ShippingLine = model.NsProductClass_ShippingLine;
        _sapIntegrationSettings.NsProductClass_Promotion = model.NsProductClass_Promotion;

        _sapIntegrationSettings.BankCode_Peach = model.BankCode_Peach;
        _sapIntegrationSettings.BankCode_Loyalty = model.BankCode_Loyalty;

        // default for order sync
        _sapIntegrationSettings.DefaultEmailToSyncErpOrders = model.DefaultEmailToSyncErpOrders;
        _sapIntegrationSettings.DefaultFirstNameToSyncErpOrders = model.DefaultFirstNameToSyncErpOrders;
        _sapIntegrationSettings.DefaultLastNameToSyncErpOrders = model.DefaultLastNameToSyncErpOrders;
        _sapIntegrationSettings.DefaultPhoneNumberToSyncErpOrders = model.DefaultPhoneNumberToSyncErpOrders;
        _sapIntegrationSettings.DefaultMobileNumberToSyncErpOrders = model.DefaultMobileNumberToSyncErpOrders;

        _sapIntegrationSettings.SAPCompanyCode = model.SAPCompanyCode;
        _sapIntegrationSettings.VendorName = model.VendorName;
        _sapIntegrationSettings.ShippingCostSKU = model.ShippingCostSKU;
        _sapIntegrationSettings.B2BOrderTypeMappings = model.B2BOrderTypeMappings;
        _sapIntegrationSettings.B2COrderTypeMappings = model.B2COrderTypeMappings;

        _sapIntegrationSettings.AppServerHost = model.AppServerHost;
        _sapIntegrationSettings.SystemNumber = model.SystemNumber;
        _sapIntegrationSettings.SystemID = model.SystemID;
        _sapIntegrationSettings.User = model.User;
        _sapIntegrationSettings.Password = model.Password;
        _sapIntegrationSettings.RepositoryPassword = model.RepositoryPassword;
        _sapIntegrationSettings.Client = model.Client;
        _sapIntegrationSettings.Language = model.Language;
        _sapIntegrationSettings.PoolSize = model.PoolSize;
        _sapIntegrationSettings.AliasUser = model.AliasUser;
        _sapIntegrationSettings.IntegrationSecretKey = model.IntegrationSecretKey;

        await _settingService.SaveSettingAsync(_sapIntegrationSettings, settings => settings.HttpCallMaxRetries, storeId, clearCache: false);
        await _settingService.SaveSettingAsync(_sapIntegrationSettings, settings => settings.HttpCallRestTimeInSeconds, storeId, clearCache: false);

        await _settingService.SaveSettingAsync(_sapIntegrationSettings, settings => settings.Salesperson, storeId, clearCache: false);
        await _settingService.SaveSettingAsync(_sapIntegrationSettings, settings => settings.TermsCode, storeId, clearCache: false);
        await _settingService.SaveSettingAsync(_sapIntegrationSettings, settings => settings.CustomerClass, storeId, clearCache: false);
        await _settingService.SaveSettingAsync(_sapIntegrationSettings, settings => settings.ArStatementNo, storeId, clearCache: false);

        await _settingService.SaveSettingAsync(_sapIntegrationSettings, settings => settings.OrderType, storeId, clearCache: false);
        await _settingService.SaveSettingAsync(_sapIntegrationSettings, settings => settings.NsProductClass_ShippingLine, storeId, clearCache: false);
        await _settingService.SaveSettingAsync(_sapIntegrationSettings, settings => settings.NsProductClass_Promotion, storeId, clearCache: false);

        await _settingService.SaveSettingAsync(_sapIntegrationSettings, settings => settings.OrderType, storeId, clearCache: false);
        await _settingService.SaveSettingAsync(_sapIntegrationSettings, settings => settings.DefaultEmailToSyncErpOrders, storeId, clearCache: false);
        await _settingService.SaveSettingAsync(_sapIntegrationSettings, settings => settings.DefaultFirstNameToSyncErpOrders, storeId, clearCache: false);
        await _settingService.SaveSettingAsync(_sapIntegrationSettings, settings => settings.DefaultLastNameToSyncErpOrders, storeId, clearCache: false);
        await _settingService.SaveSettingAsync(_sapIntegrationSettings, settings => settings.DefaultPhoneNumberToSyncErpOrders, storeId, clearCache: false);
        await _settingService.SaveSettingAsync(_sapIntegrationSettings, settings => settings.DefaultMobileNumberToSyncErpOrders, storeId, clearCache: false);

        await _settingService.SaveSettingAsync(_sapIntegrationSettings, settings => settings.BankCode_Peach, storeId, clearCache: false);
        await _settingService.SaveSettingAsync(_sapIntegrationSettings, settings => settings.BankCode_Loyalty, storeId, clearCache: false);

        await _settingService.SaveSettingAsync(_sapIntegrationSettings, settings => settings.SAPCompanyCode, storeId, clearCache: false);
        await _settingService.SaveSettingAsync(_sapIntegrationSettings, settings => settings.VendorName, storeId, clearCache: false);
        await _settingService.SaveSettingAsync(_sapIntegrationSettings, settings => settings.ShippingCostSKU, storeId, clearCache: false);
        await _settingService.SaveSettingAsync(_sapIntegrationSettings, settings => settings.B2BOrderTypeMappings, storeId, clearCache: false);
        await _settingService.SaveSettingAsync(_sapIntegrationSettings, settings => settings.B2COrderTypeMappings, storeId, clearCache: false);

        await _settingService.SaveSettingAsync(_sapIntegrationSettings, settings => settings.AppServerHost, storeId, clearCache: false);
        await _settingService.SaveSettingAsync(_sapIntegrationSettings, settings => settings.SystemNumber, storeId, clearCache: false);
        await _settingService.SaveSettingAsync(_sapIntegrationSettings, settings => settings.SystemID, storeId, clearCache: false);
        await _settingService.SaveSettingAsync(_sapIntegrationSettings, settings => settings.User, storeId, clearCache: false);
        await _settingService.SaveSettingAsync(_sapIntegrationSettings, settings => settings.Password, storeId, clearCache: false);
        await _settingService.SaveSettingAsync(_sapIntegrationSettings, settings => settings.RepositoryPassword, storeId, clearCache: false);
        await _settingService.SaveSettingAsync(_sapIntegrationSettings, settings => settings.Client, storeId, clearCache: false);
        await _settingService.SaveSettingAsync(_sapIntegrationSettings, settings => settings.Language, storeId, clearCache: false);
        await _settingService.SaveSettingAsync(_sapIntegrationSettings, settings => settings.PoolSize, storeId, clearCache: false);
        await _settingService.SaveSettingAsync(_sapIntegrationSettings, settings => settings.AliasUser, storeId, clearCache: false);
        await _settingService.SaveSettingAsync(_sapIntegrationSettings, settings => settings.IntegrationSecretKey, storeId, clearCache: false);

        await _settingService.ClearCacheAsync();

        _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.Plugins.Saved"));

        return await Configure();
    }

    #endregion

    #region TestActions

    [TokenAuthorize]
    [HttpPost]
    public async Task<IActionResult> GetAccounts([FromBody] ErpGetRequestModel erpRequest)
    {
        //var
        //    result = GetAccounts(new ERPRequest());
        //var result = await _erpAccountService.GetAccountsFromErpAsync(erpRequest);
        //if (result == null)
        //{
        //    // Return 404 Not Found if the result is null
        //    return NotFound(new { Message = "No accounts found or service returned no data." });
        //}

        //if (result.ErpResponseModel.IsError)
        //{
        //    // Return 500 Internal Server Error if there was an error in the response
        //    return StatusCode(StatusCodes.Status500InternalServerError,
        //                      new { Message = $"Short message: {result.ErpResponseModel.ErrorShortMessage}. Ful message: {result.ErpResponseModel.ErrorFullMessage}" });
        //}

        return Ok();
    }

    [TokenAuthorize]
    [HttpPost]
    public async Task<IActionResult> GetProductGroupPricesFromErp([FromBody] ErpGetRequestModel erpRequest)
    {
        var result = await _b2BPricingService.GetProductGroupPricingFromErpAsync(erpRequest);

        if (result == null)
        {
            // Return 404 Not Found if the result is null
            return NotFound(new { Message = "No Group Price found or service returned no data." });
        }

        if (result.ErpResponseModel.IsError)
        {
            // Return 500 Internal Server Error if there was an error in the response
            return StatusCode(
                StatusCodes.Status500InternalServerError,
                new
                {
                    Message = $"Short message: {result.ErpResponseModel.ErrorShortMessage}. Ful message: {result.ErpResponseModel.ErrorFullMessage}",
                }
            );
        }

        return Ok(result);
    }

    [TokenAuthorize]
    [HttpPost]
    public async Task<IActionResult> GetOrders([FromBody] ErpGetRequestModel erpRequest)
    {
        var result = await _erpOrderService.GetOrderByAccountFromErpAsync(erpRequest);

        if (result == null)
        {
            // Return 404 Not Found if the result is null
            return NotFound(new { Message = "No orders found or service returned no data." });
        }

        if (result.ErpResponseModel.IsError)
        {
            return StatusCode(
                StatusCodes.Status500InternalServerError,
                new
                {
                    Message = $"Short message: {result.ErpResponseModel.ErrorShortMessage}. Ful message: {result.ErpResponseModel.ErrorFullMessage}",
                }
            );
        }

        return Ok(result);
    }

    [TokenAuthorize]
    [HttpPost]
    public async Task<IActionResult> GetStocks([FromBody] ErpGetRequestModel erpRequest)
    {
        var result = await _b2BStockService.GetStockFromErpAsync(erpRequest);

        if (result == null)
        {
            // Return 404 Not Found if the result is null
            return NotFound(new { Message = "No stock found or service returned no data." });
        }

        if (result.ErpResponseModel.IsError)
        {
            // Return 500 Internal Server Error if there was an error in the response
            return StatusCode(
                StatusCodes.Status500InternalServerError,
                new
                {
                    Message = $"Short message: {result.ErpResponseModel.ErrorShortMessage}. Ful message: {result.ErpResponseModel.ErrorFullMessage}",
                }
            );
        }

        return Ok(result);
    }

    [TokenAuthorize]
    [HttpPost]
    public async Task<IActionResult> GetProducts([FromBody] ErpGetRequestModel erpRequest)
    {
        var result = await _b2BProductService.GetProductsFromErpAsync(erpRequest);

        if (result == null)
        {
            // Return 404 Not Found if the result is null
            return NotFound(new { Message = "No product found or service returned no data." });
        }

        if (result.ErpResponseModel.IsError)
        {
            // Return 500 Internal Server Error if there was an error in the response
            return StatusCode(
                StatusCodes.Status500InternalServerError,
                new
                {
                    Message = $"Short message: {result.ErpResponseModel.ErrorShortMessage}. Ful message: {result.ErpResponseModel.ErrorFullMessage}",
                }
            );
        }

        return Ok(result);
    }

    [TokenAuthorize]
    [HttpPost]
    public async Task<IActionResult> GetProductImage([FromBody] ErpGetRequestModel erpRequest)
    {
        var result = await _b2BProductService.GetProductImageFromErpAsync(erpRequest);

        if (result == null)
        {
            // Return 404 Not Found if the result is null
            return NotFound(new { Message = "No product found or service returned no data." });
        }

        if (result.ErpResponseModel.IsError)
        {
            // Return 500 Internal Server Error if there was an error in the response
            return StatusCode(
                StatusCodes.Status500InternalServerError,
                new
                {
                    Message = $"Short message: {result.ErpResponseModel.ErrorShortMessage}. Ful message: {result.ErpResponseModel.ErrorFullMessage}",
                }
            );
        }

        return Ok(result);
    }

    [TokenAuthorize]
    [HttpPost]
    public async Task<IActionResult> GetProductImages([FromBody] ErpGetRequestModel erpRequest)
    {
        var result = await _b2BProductService.GetProductImagesFromErpAsync(erpRequest);

        if (result == null)
        {
            // Return 404 Not Found if the result is null
            return NotFound(new { Message = "No product found or service returned no data." });
        }

        if (result.ErpResponseModel.IsError)
        {
            // Return 500 Internal Server Error if there was an error in the response
            return StatusCode(
                StatusCodes.Status500InternalServerError,
                new
                {
                    Message = $"Short message: {result.ErpResponseModel.ErrorShortMessage}. Ful message: {result.ErpResponseModel.ErrorFullMessage}",
                }
            );
        }

        return Ok(result);
    }

    [TokenAuthorize]
    [HttpPost]
    public async Task<IActionResult> GetPrices([FromBody] ErpGetRequestModel erpRequest)
    {
        var result = await _b2BPricingService.GetPerAccountProductPricingFromErpAsync(erpRequest);

        if (result == null)
        {
            // Return 404 Not Found if the result is null
            return NotFound(new { Message = "No pricing found or service returned no data." });
        }

        if (result.ErpResponseModel.IsError)
        {
            // Return 500 Internal Server Error if there was an error in the response
            return StatusCode(
                StatusCodes.Status500InternalServerError,
                new
                {
                    Message = $"Short message: {result.ErpResponseModel.ErrorShortMessage}. Ful message: {result.ErpResponseModel.ErrorFullMessage}",
                }
            );
        }

        return Ok(result);
    }

    [TokenAuthorize]
    [HttpPost]
    public async Task<IActionResult> GetGroupPrices([FromBody] ErpGetRequestModel erpRequest)
    {
        var result = await _b2BPricingService.GetProductGroupPricingFromErpAsync(erpRequest);

        if (result == null)
        {
            // Return 404 Not Found if the result is null
            return NotFound(new { Message = "No pricing found or service returned no data." });
        }

        if (result.ErpResponseModel.IsError)
        {
            // Return 500 Internal Server Error if there was an error in the response
            return StatusCode(
                StatusCodes.Status500InternalServerError,
                new
                {
                    Message = $"Short message: {result.ErpResponseModel.ErrorShortMessage}. Ful message: {result.ErpResponseModel.ErrorFullMessage}",
                }
            );
        }

        return Ok(result);
    }

    [TokenAuthorize]
    [HttpPost]
    public async Task<IActionResult> GetInvoicePdfHex([FromBody] ErpGetRequestModel erpRequest)
    {
        var result = await _b2BInvoiceService.GetInvoicePdfByteCodeByDocumentNoFromErpAsync(
            erpRequest
        );

        if (result == null)
        {
            // Return 404 Not Found if the result is null
            return NotFound(new { Message = "No invoice found or service returned no data." });
        }

        if (result.ErpResponseModel.IsError)
        {
            // Return 500 Internal Server Error if there was an error in the response
            return StatusCode(
                StatusCodes.Status500InternalServerError,
                new
                {
                    Message = $"Short message: {result.ErpResponseModel.ErrorShortMessage}. Ful message: {result.ErpResponseModel.ErrorFullMessage}",
                }
            );
        }

        return Ok(result);
    }

    [TokenAuthorize]
    [HttpPost]
    public async Task<IActionResult> GetInvoices([FromBody] ErpGetRequestModel erpRequest)
    {
        var result = await _b2BInvoiceService.GetInvoiceByAccountNoFromErpAsync(erpRequest);

        if (result == null)
        {
            // Return 404 Not Found if the result is null
            return NotFound(new { Message = "No invoice found or service returned no data." });
        }

        if (result.ErpResponseModel.IsError)
        {
            // Return 500 Internal Server Error if there was an error in the response
            return StatusCode(
                StatusCodes.Status500InternalServerError,
                new
                {
                    Message = $"Short message: {result.ErpResponseModel.ErrorShortMessage}. Ful message: {result.ErpResponseModel.ErrorFullMessage}",
                }
            );
        }

        return Ok(result);
    }

    [TokenAuthorize]
    [HttpPost]
    public async Task<IActionResult> GetStatementPdfHex([FromBody] ErpGetRequestModel erpRequest)
    {
        var result = await _b2BInvoiceService.GetStatementPdfByteCodeFromErpAsync(erpRequest);
        if (result == null)
        {
            // Return 404 Not Found if the result is null
            return NotFound(new { Message = "No invoice found or service returned no data." });
        }

        if (result.ErpResponseModel.IsError)
        {
            // Return 500 Internal Server Error if there was an error in the response
            return StatusCode(
                StatusCodes.Status500InternalServerError,
                new
                {
                    Message = $"Short message: {result.ErpResponseModel.ErrorShortMessage}. Ful message: {result.ErpResponseModel.ErrorFullMessage}",
                }
            );
        }

        return Ok(result);
    }

    [TokenAuthorize]
    [HttpPost]
    public async Task<IActionResult> GetShipToAddress([FromBody] ErpGetRequestModel erpRequest)
    {
        var result = await _shipToAddressService.GetShipToAddressFromErpAsync(erpRequest);

        if (result == null)
        {
            // Return 404 Not Found if the result is null
            return NotFound(
                new { Message = "No ship to address found or service returned no data." }
            );
        }

        if (result.ErpResponseModel.IsError)
        {
            // Return 500 Internal Server Error if there was an error in the response
            return StatusCode(
                StatusCodes.Status500InternalServerError,
                new
                {
                    Message = $"Short message: {result.ErpResponseModel.ErrorShortMessage}. Ful message: {result.ErpResponseModel.ErrorFullMessage}",
                }
            );
        }

        return Ok(result);
    }

    [TokenAuthorize]
    [HttpPost]
    public async Task<IActionResult> GetShippingRate([FromBody] ErpGetRequestModel erpRequest)
    {
        var result = await _erpShippingService.GetShippingRateAsync(erpRequest);

        if (result == null)
        {
            // Return 404 Not Found if the result is null
            return NotFound(
                new { Message = "No Shipping Rate found or service returned no data." }
            );
        }

        if (result.IsError)
        {
            // Return 500 Internal Server Error if there was an error in the response
            return StatusCode(
                StatusCodes.Status500InternalServerError,
                new
                {
                    Message = $"Short message: {result.ErrorShortMessage}. Ful message: {result.ErrorFullMessage}",
                }
            );
        }

        return Ok(result);
    }

    [TokenAuthorize]
    [HttpPost]
    public async Task<IActionResult> CreateOrder([FromBody] ErpPlaceOrderDataModel erpRequest)
    {
        var result = await _erpOrderService.CreateOrderOnErpAsync(erpRequest);

        if (result == null)
        {
            // Return 404 Not Found if the result is null
            return NotFound(new { Message = "Can not create order at ERP." });
        }

        if (result.IsError)
        {
            // Return 500 Internal Server Error if there was an error in the response
            return StatusCode(
                StatusCodes.Status500InternalServerError,
                new
                {
                    Message = $"Short message: {result.ErrorShortMessage}. Ful message: {result.ErrorFullMessage}",
                }
            );
        }

        return Ok(result);
    }

    [TokenAuthorize]
    [HttpPost]
    public async Task<IActionResult> GetOrderByAccount([FromBody] ErpGetRequestModel erpRequest)
    {
        var result = await _erpOrderService.GetOrderByAccountFromErpAsync(erpRequest);

        if (result == null)
        {
            // Return 404 Not Found if the result is null
            return NotFound(new { Message = "No orders found or service returned no data." });
        }

        if (result.ErpResponseModel.IsError)
        {
            // Return 500 Internal Server Error if there was an error in the response
            return StatusCode(
                StatusCodes.Status500InternalServerError,
                new
                {
                    Message = $"Short message: {result.ErpResponseModel.ErrorShortMessage}. Ful message: {result.ErpResponseModel.ErrorFullMessage}",
                }
            );
        }

        return Ok(result);
    }

    [TokenAuthorize]
    [HttpPost]
    public async Task<IActionResult> GetDocument([FromBody] ErpGetRequestModel erpRequest)
    {
        var documentNumber = erpRequest.DocumentNumber;

        if (string.IsNullOrEmpty(documentNumber))
        {
            return BadRequest(
                new { Message = "Missing required parameters: ClientId, Key, or DocumentNumber" }
            );
        }

        var documentBytes = await _erpDocumentService.GetDocumentAsync(documentNumber);

        if (documentBytes.Length == 0)
        {
            return NotFound(
                new { Message = "Document not found or error occurred while retrieving." }
            );
        }

        // Return 200 OK with the document as byte array
        return File(documentBytes, "application/pdf", "Invoice.pdf");
    }

    [TokenAuthorize]
    [HttpPost]
    public async Task<IActionResult> GetDocumentForQuote([FromBody] ErpGetRequestModel erpRequest)
    {
        var documentNumber = erpRequest.DocumentNumber;

        if (string.IsNullOrEmpty(documentNumber))
        {
            return BadRequest(
                new { Message = "Missing required parameters: ClientId, Key, or DocumentNumber" }
            );
        }

        var documentBytes = await _erpDocumentService.GetDocumentForQuoteAsync(documentNumber);

        if (documentBytes.Length == 0)
        {
            return NotFound(
                new { Message = "Document not found or error occurred while retrieving." }
            );
        }

        return File(documentBytes, "application/pdf", "Quote.pdf");
    }

    [TokenAuthorize]
    [HttpPost]
    public async Task<IActionResult> GetDocumentForOrder([FromBody] ErpGetRequestModel erpRequest)
    {
        var documentNumber = erpRequest.DocumentNumber;

        if (string.IsNullOrEmpty(documentNumber))
        {
            return BadRequest(
                new { Message = "Missing required parameters: ClientId, Key, or DocumentNumber" }
            );
        }

        var documentBytes = await _erpDocumentService.GetDocumentForOrderAsync(documentNumber);

        if (documentBytes.Length == 0)
        {
            return NotFound(
                new { Message = "Document not found or error occurred while retrieving." }
            );
        }

        return File(documentBytes, "application/pdf", "Order.pdf");
    }

    [TokenAuthorize]
    [HttpPost]
    public async Task<IActionResult> GetPODDocuments([FromBody] ErpGetRequestModel erpRequest)
    {
        var documentNumber = erpRequest.DocumentNumber;

        if (string.IsNullOrEmpty(documentNumber))
        {
            return BadRequest(new { Message = "Missing required parameters" });
        }
        var podDocumentList = await _erpDocumentService.GetPODDocumentListAsync(documentNumber);

        if (podDocumentList == null && podDocumentList.Count() == 0)
        {
            return NotFound(
                new { Message = "Document not found or error occurred while retrieving." }
            );
        }

        return Ok(podDocumentList);
    }

    [TokenAuthorize]
    [HttpPost]
    public async Task<IActionResult> GetAccountStatementPDF([FromBody] ErpGetRequestModel erpRequest)
    {
        if (erpRequest == null)
        {
            return BadRequest(new { Message = "Missing required parameters" });
        }

        var erpAccount = new ErpAccount { AccountNumber = erpRequest.AccountNumber };

        DateTime? dateFrom = erpRequest.DateFrom;
        DateTime? dateTo = erpRequest.DateTo;

        var documentBytes = await _b2BAccountService.GetAccountStatementPDFAsync(
            erpAccount,
            dateFrom,
            dateTo
        );

        if (documentBytes == null || documentBytes.Length == 0)
        {
            return NotFound(
                new { Message = "Document not found or an error occurred while retrieving." }
            );
        }

        return File(documentBytes, "application/pdf", "AccountStatement.pdf");
    }

    [TokenAuthorize]
    [HttpPost]
    public async Task<IActionResult> GetTheTestCertificates([FromBody] ErpGetRequestModel erpRequest)
    {
        var documentNumber = erpRequest.DocumentNumber;

        if (string.IsNullOrEmpty(documentNumber))
        {
            return BadRequest(new { Message = "Missing required parameters" });
        }

        var testCertList = await _erpDocumentService.GetTheTestCertificateListAsync(documentNumber);

        if (testCertList == null && testCertList.Count() == 0)
        {
            return NotFound(
                new { Message = "Document not found or error occurred while retrieving." }
            );
        }

        return Ok(testCertList);
    }

    [TokenAuthorize]
    [HttpPost]
    public async Task<IActionResult> GetSpecSheets([FromBody] ErpGetRequestModel erpRequest)
    {
        var result = await _erpDocumentService.GetSpecSheetAsync(erpRequest);

        if (result == null)
        {
            // Return 404 Not Found if the result is null
            return NotFound(new { Message = "No Spec Sheet found or service returned no data." });
        }

        if (result.ErpResponseModel.IsError)
        {
            // Return 500 Internal Server Error if there was an error in the response
            return StatusCode(
                StatusCodes.Status500InternalServerError,
                new
                {
                    Message = $"Short message: {result.ErpResponseModel.ErrorShortMessage}. Full message: {result.ErpResponseModel.ErrorFullMessage}",
                }
            );
        }

        return Ok(result);
    }

    [TokenAuthorize]
    [HttpPost]
    public async Task<IActionResult> GetAreaCodes([FromBody] ErpGetRequestModel erpRequest)
    {
        var areaCodes = await _erpSalesOrgService.GetAreaCodesFromErpAsync( new ErpGetRequestModel());

        if (areaCodes != null && areaCodes.Data != null && areaCodes.Data.Count() == 0)
        {
            return NotFound(
                new { Message = "Area codes not found or error occurred while retrieving." }
            );
        }

        return Ok(areaCodes.Data);
    }

    [TokenAuthorize]
    [HttpPost]
    public async Task<IActionResult> GetAllAccountCreditFromErp([FromBody] ErpGetRequestModel erpRequest)
    {
        if (erpRequest == null)
        {
            return BadRequest(new { Message = "Payload is null!" });
        }

        var erpResponse = await _b2BAccountService.GetAllAccountCreditFromErpAsync(erpRequest);

        if (erpResponse == null || erpResponse.Data == null)
        {
            return NotFound(
                new { Message = "Error occurred" }
            );
        }

        return Ok(erpResponse);
    }

    [TokenAuthorize]
    [HttpPost]
    public async Task<IActionResult> GetAllTimeSavings([FromBody] ErpGetRequestModel erpRequest)
    {
        if (erpRequest == null)
            return BadRequest(new { Message = "Request body cannot be null." });

        erpRequest.DateFrom = new DateTime(1970, 1, 1);
        erpRequest.DateTo = DateTime.UtcNow;

        string identifier;
        if (!string.IsNullOrWhiteSpace(erpRequest.AccountNumber))
        {
            erpRequest.CustomerEmail = string.Empty;
            identifier = $"B2B Account Number: {erpRequest.AccountNumber}";
        }
        else if (!string.IsNullOrWhiteSpace(erpRequest.CustomerEmail))
        {
            identifier = $"B2C Customer Email: {erpRequest.CustomerEmail}";
        }
        else
        {
            return BadRequest(new { Message = "Either AccountNumber or CustomerEmail must be provided." });
        }

        try
        {
            var savings = await _b2BAccountService.GetAccountSavingsAsync(erpRequest);

            if (savings == null)
            {
                return NotFound(new
                {
                    Message = $"No savings data found for {identifier}."
                });
            }

            return Ok(new
            {
                Message = $"Account savings retrieved successfully for {identifier}.",
                TotalSavings = savings
            });
        }
        catch (Exception ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, new
            {
                Message = "An internal error occurred while processing your request.",
                Details = ex.Message
            });
        }
    }

    [TokenAuthorize]
    [HttpPost]
    public async Task<IActionResult> GetForThisYearSavings([FromBody] ErpGetRequestModel erpRequest)
    {
        if (erpRequest == null)
            return BadRequest(new { Message = "Request body cannot be null." });

        erpRequest.DateFrom = new DateTime(DateTime.UtcNow.Year, 1, 1);
        erpRequest.DateTo = new DateTime(DateTime.UtcNow.Year, 12, 31);

        string identifier;
        if (!string.IsNullOrWhiteSpace(erpRequest.AccountNumber))
        {
            erpRequest.CustomerEmail = string.Empty;
            identifier = $"B2B Account Number: {erpRequest.AccountNumber}";
        }
        else if (!string.IsNullOrWhiteSpace(erpRequest.CustomerEmail))
        {
            identifier = $"B2C Customer Email: {erpRequest.CustomerEmail}";
        }
        else
        {
            return BadRequest(new { Message = "Either AccountNumber or CustomerEmail must be provided." });
        }

        try
        {
            var savings = await _b2BAccountService.GetAccountSavingsAsync(erpRequest);

            if (savings == null)
            {
                return NotFound(new
                {
                    Message = $"No savings data found for {identifier}."
                });
            }

            return Ok(new
            {
                Message = $"Account savings retrieved successfully for {identifier}.",
                TotalSavings = savings
            });
        }
        catch (Exception ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, new
            {
                Message = "An internal error occurred while processing your request.",
                Details = ex.Message
            });
        }
    }

    #endregion
}