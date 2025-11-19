using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Nop.Core;
using Nop.Core.Caching;
using Nop.Core.Domain.Logging;
using Nop.Data;
using Nop.Plugin.Misc.ErpWebhook.Filters;
using Nop.Plugin.Misc.ErpWebhook.Models.Common;
using Nop.Plugin.Misc.ErpWebhook.Models.Credit;
using Nop.Plugin.Misc.ErpWebhook.Models.ErpAccount;
using Nop.Plugin.Misc.ErpWebhook.Models.ErpDeliveryDates;
using Nop.Plugin.Misc.ErpWebhook.Models.ErpOrder;
using Nop.Plugin.Misc.ErpWebhook.Models.ErpPricing;
using Nop.Plugin.Misc.ErpWebhook.Models.ErpProduct;
using Nop.Plugin.Misc.ErpWebhook.Models.ErpProductsImage;
using Nop.Plugin.Misc.ErpWebhook.Models.ErpShipToAddress;
using Nop.Plugin.Misc.ErpWebhook.Models.ErpStock;
using Nop.Plugin.Misc.ErpWebhook.Services.Interfaces;
using Nop.Services.Configuration;
using Nop.Services.Logging;
using Nop.Web.Framework.Controllers;
using NopStation.Plugin.B2B.B2BB2CFeatures;
using NopStation.Plugin.B2B.ERPIntegrationCore.Domain;
using NopStation.Plugin.B2B.ERPIntegrationCore.Enums;
using NopStation.Plugin.B2B.ERPIntegrationCore.Services;

namespace Nop.Plugin.Misc.ErpWebhook.Controllers;

[ValidateWebhookManagerIpAddress]
[TokenAuthorize]
public class ErpWebhookController : BasePluginController
{
    #region Fields

    private ErpWebhookConfig _erpWebhookConfig = null;
    private readonly IStoreContext _storeContext;
    private readonly ILogger _logger;
    private readonly ErpWebhookSettings _erpWebhookSettings;
    private readonly B2BB2CFeaturesSettings _b2BB2CFeaturesSettings;
    private readonly IWebhookERPProductService _webhookProductService;
    private readonly IWebhookERPAccountService _webhookAccountService;
    private readonly IWebhookERPShipToAddressService _webhookShipToAddressService;
    private readonly IWebhookERPStockDataService _webhookERPStockDataService;
    private readonly IWebhookERPPriceGroupPricingService _webhookERPPriceGroupPricingService;
    private readonly IWebhookERPPerAccountPricingService _webhookERPPerAccountPricingService;
    private readonly IWebhookErpOrderService _webhookB2BOrderService;
    private readonly IWebhookDeliveryDatesService _webhookDeliveryDatesService;
    private readonly IErpWebhookService _erpWebhookService;
    private readonly ISettingService _settingService;
    private readonly IWebhookProductsImageService _webhookProductsImageService;
    string _prefilterFacets = null;
    private readonly INopDataProvider _nopDataProvider;
    private readonly IErpLogsService _erpLogsService;
    private readonly IStaticCacheManager _staticCacheManager;

    #endregion

    #region Ctor

    public ErpWebhookController(
        IStoreContext storeContext,
        ILogger logger,
        ErpWebhookSettings erpWebhookSettings,
        B2BB2CFeaturesSettings b2BB2CFeaturesSettings,
        IWebhookERPProductService webhookProductService,
        IWebhookERPAccountService webhookAccountService,
        IWebhookERPShipToAddressService webhookShipToAddressService,
        IWebhookERPStockDataService webhookERPStockDataService,
        IWebhookERPPriceGroupPricingService webhookERPPriceGroupPricingService,
        IWebhookERPPerAccountPricingService webhookERPPerAccountPricingService,
        IWebhookErpOrderService webhookB2BOrderService,
        IWebhookDeliveryDatesService webhookDeliveryDatesService,
        IErpWebhookService erpWebhookService,
        ISettingService settingService,
        IWebhookProductsImageService webhookProductsImageService,
        INopDataProvider nopDataProvider,
        IErpLogsService erpLogsService,
        IStaticCacheManager staticCacheManager
    )
    {
        _storeContext = storeContext;
        _logger = logger;
        _erpWebhookSettings = erpWebhookSettings;
        _b2BB2CFeaturesSettings = b2BB2CFeaturesSettings;
        _webhookProductService = webhookProductService;
        _webhookAccountService = webhookAccountService;
        _webhookShipToAddressService = webhookShipToAddressService;
        _webhookERPStockDataService = webhookERPStockDataService;
        _webhookERPPriceGroupPricingService = webhookERPPriceGroupPricingService;
        _webhookERPPerAccountPricingService = webhookERPPerAccountPricingService;
        _webhookB2BOrderService = webhookB2BOrderService;
        _webhookDeliveryDatesService = webhookDeliveryDatesService;
        _erpWebhookService = erpWebhookService;
        _settingService = settingService;
        _webhookProductsImageService = webhookProductsImageService;
        _nopDataProvider = nopDataProvider;
        _erpLogsService = erpLogsService;
        _staticCacheManager = staticCacheManager;
    }

    #endregion

    #region Methods

    [HttpPost]
    public async Task<IActionResult> Account([FromBody] JToken body)
    {
        await _erpLogsService.InsertErpLogAsync(
            ErpLogLevel.Information,
            ErpSyncLevel.Account,
            "Account webhook: Call started. Click view to see details.",
            $"Account webhook: Call started, Body: {JsonConvert.SerializeObject(body)}"
        );

        var response = new WebhookResponseModel();

        #region Check if previous process running

        if (_erpWebhookSettings.AccounthookAlreadyRunning)
        {
            response.IsError = "False";
            response.Message = $"Previous process is not completed yet";

            await _erpLogsService.InsertErpLogAsync(
                ErpLogLevel.Information,
                ErpSyncLevel.Account,
                "Account webhook call skipped — previous process still running."
            );

            return Ok(response);
        }

        var storeScope = await _storeContext.GetActiveStoreScopeConfigurationAsync();
        var settingsKey = _settingService.GetSettingKey(
            _erpWebhookSettings,
            x => x.AccounthookAlreadyRunning
        );
        // updating value to true as new process running from here
        await _settingService.SetSettingAsync(settingsKey, true, storeScope, true);

        #endregion

        try
        {
            if (body != null)
            {
                #region Preparing JToken Data

                _erpWebhookConfig = await _erpWebhookService.LoadErpWebhookConfigsFromJsonAsync();
                var erpAccounts = new List<WebhookErpAccountModel>();
                if (body.Type == JTokenType.Array)
                {
                    try
                    {
                        var accounts = body
                            .ToObject<WebhookErpAccountModel[]>()
                            .Take(_erpWebhookConfig?.B2BAccountBatchSize ?? 200);
                        if (accounts != null)
                        {
                            // Filter out accounts with null or empty AccountNumber
                            var validAccounts = accounts
                                .Where(account => !string.IsNullOrWhiteSpace(account.AccountNumber))
                                .ToList();
                            erpAccounts.AddRange(validAccounts);
                        }
                    }
                    catch (Exception ex)
                    {
                        var errorMessage =
                            $"Account webhook: An error occurred while mapping data to array. Exception: {ex.Message}";
                        await _erpLogsService.InsertErpLogAsync(
                            ErpLogLevel.Error,
                            ErpSyncLevel.Account,
                            errorMessage,
                            ex.StackTrace
                        );

                        response.IsError = "True";
                        response.Message = errorMessage;

                        // updating value to false as this process terminating
                        await _settingService.SetSettingAsync(settingsKey, false, storeScope, true);

                        return BadRequest(response);
                    }
                }
                else if (body.Type == JTokenType.Object)
                {
                    try
                    {
                        var account = body.ToObject<WebhookErpAccountModel>();
                        if (account != null && !string.IsNullOrWhiteSpace(account.AccountNumber))
                        {
                            erpAccounts.Add(account);
                        }
                    }
                    catch (Exception ex)
                    {
                        var errorMessage =
                            $"Account webhook: An error occurred while mapping data to array. Exception: {ex.Message}";
                        await _erpLogsService.InsertErpLogAsync(
                            ErpLogLevel.Error,
                            ErpSyncLevel.Account,
                            errorMessage,
                            ex.StackTrace
                        );

                        response.IsError = "True";
                        response.Message = errorMessage;

                        // updating value to false as this process terminating
                        await _settingService.SetSettingAsync(settingsKey, false, storeScope, true);

                        return BadRequest(response);
                    }
                }
                else
                {
                    // Invalid JSON format
                    var errorMessage = $"Account webhook: invalid json format in body";
                    await _erpLogsService.InsertErpLogAsync(
                        ErpLogLevel.Error,
                        ErpSyncLevel.Account,
                        $"{errorMessage}. Click view to see details.",
                        $"Body: {JsonConvert.SerializeObject(body)}"
                    );

                    response.IsError = "True";
                    response.Message = errorMessage;

                    // updating value to false as this process terminating
                    await _settingService.SetSettingAsync(settingsKey, false, storeScope, true);

                    return BadRequest(response);
                }

                if (erpAccounts.Count == 0)
                {
                    var message = "Account webhook: no valid accounts came with call";
                    await _erpLogsService.InsertErpLogAsync(
                        ErpLogLevel.Warning,
                        ErpSyncLevel.Account,
                        message
                    );

                    response.IsError = "False";
                    response.Message = message;

                    // updating value to false as this process terminating
                    await _settingService.SetSettingAsync(settingsKey, false, storeScope, true);

                    return Ok(response);
                }

                #endregion

                #region Data upgrade process start

                var prefilterFacets = _erpWebhookConfig?.AccountPrefilterFacets;
                if (prefilterFacets != null && _erpWebhookConfig.ClientIsMacsteel == false)
                {
                    _prefilterFacets = prefilterFacets;
                    await _erpLogsService.InsertErpLogAsync(
                        ErpLogLevel.Debug,
                        ErpSyncLevel.Account,
                        $"Account webhook: Using the following prefilter for all accounts: {_prefilterFacets}"
                    );
                }

                await _webhookAccountService.ProcessErpAccountsAsync(erpAccounts);

                var sqlCommand = "EXEC UpdateB2BAccountFromParallelTableErpB2BAccount;";
                await _nopDataProvider.ExecuteNonQueryAsync(sqlCommand);
                await _nopDataProvider.ExecuteNonQueryAsync(
                "EXEC [B2BAccountNameToCustomerCompanySync] @OnlyActive = 1"
                );
                await _staticCacheManager.RemoveByPrefixAsync(NopEntityCacheDefaults<ErpAccount>.Prefix);

                #endregion

                await _erpLogsService.InsertErpLogAsync(
                ErpLogLevel.Information,
                ErpSyncLevel.Account,
                    "Account webhook: Call finished."
                );
            }
            else
            {
                await _erpLogsService.InsertErpLogAsync(
                    ErpLogLevel.Error,
                    ErpSyncLevel.Account,
                    "Account webhook: Error, the incoming data body is empty."
                );

                response.IsError = "True";
                response.Message = "Request body is null";

                // updating value to false as this process terminating
                await _settingService.SetSettingAsync(settingsKey, false, storeScope, true);

                return BadRequest(response);
            }
        }
        catch (Exception ex)
        {
            await _erpLogsService.InsertErpLogAsync(
                ErpLogLevel.Error,
                ErpSyncLevel.Account,
                $"Account webhook: Error - {ex.Message}",
                ex.StackTrace
            );
            response.IsError = "True";
            response.Message = $"Error occurred on account webhook: {ex.Message}";

            // updating value to false as this process terminating
            await _settingService.SetSettingAsync(settingsKey, false, storeScope, true);

            return StatusCode(StatusCodes.Status500InternalServerError, response);
        }

        // updating value to false as this process terminating
        await _settingService.SetSettingAsync(settingsKey, false, storeScope, true);

        return Ok(response);
    }

    [HttpPost]
    public async Task<IActionResult> ShiptoAddress([FromBody] JToken body)
    {
        await _erpLogsService.InsertErpLogAsync(
            ErpLogLevel.Information,
            ErpSyncLevel.ShipToAddress,
            "ShipToAddress webhook: Call started. Click view to see details.",
            $"ShipToAddress webhook: Call started, Body: {JsonConvert.SerializeObject(body)}"
        );

        var response = new WebhookResponseModel();

        #region check if previous process running

        if (_erpWebhookSettings.ShiptoAddresshookAlreadyRunning)
        {
            response.IsError = "False";
            response.Message = $"Previous process is not completed yet";

            await _erpLogsService.InsertErpLogAsync(
                ErpLogLevel.Information,
                ErpSyncLevel.ShipToAddress,
                "Ship-to Address webhook call skipped — previous process still running."
            );

            return Ok(response);
        }

        var storeScope = await _storeContext.GetActiveStoreScopeConfigurationAsync();
        var settingsKey = _settingService.GetSettingKey(
            _erpWebhookSettings,
            x => x.ShiptoAddresshookAlreadyRunning
        );
        // updating value to true as new process running from here
        await _settingService.SetSettingAsync(settingsKey, true, storeScope, true);

        #endregion

        try
        {
            if (body != null)
            {
                #region Preparing JToken Data

                _erpWebhookConfig = await _erpWebhookService.LoadErpWebhookConfigsFromJsonAsync();
                var erpShipToAddress = new List<ErpShipToAddressModel>();
                if (body.Type == JTokenType.Array)
                {
                    try
                    {
                        var shipToAddresses = body
                            .ToObject<ErpShipToAddressModel[]>()
                            .Take(_erpWebhookConfig?.B2BShipToAddressBatchSize ?? 50);
                        if (shipToAddresses != null)
                        {
                            // Filter out shipToAddresses with null or empty AccountNumber
                            var validShipToAddresses = shipToAddresses
                                .Where(sta =>
                                    !string.IsNullOrEmpty(sta.AccountNumber)
                                    && !string.IsNullOrEmpty(sta.ShipToCode)
                                    && !string.IsNullOrEmpty(sta.SalesOrganisationCode)
                                )
                                .ToList();
                            erpShipToAddress.AddRange(validShipToAddresses);
                        }
                    }
                    catch (Exception ex)
                    {
                        var errorMessage =
                            $"ShipToAddress webhook: An error occurred while mapping data to array. Exception: {ex.Message}";
                        await _erpLogsService.InsertErpLogAsync(
                            ErpLogLevel.Error,
                            ErpSyncLevel.ShipToAddress,
                            errorMessage,
                            ex.StackTrace
                        );

                        response.IsError = "True";
                        response.Message = errorMessage;

                        // updating value to false as this process terminating
                        await _settingService.SetSettingAsync(settingsKey, false, storeScope, true);

                        return BadRequest(response);
                    }
                }
                else if (body.Type == JTokenType.Object)
                {
                    try
                    {
                        var shipToAddress = body.ToObject<ErpShipToAddressModel>();
                        if (
                            shipToAddress != null
                            && !string.IsNullOrWhiteSpace(shipToAddress.AccountNumber)
                            && !string.IsNullOrWhiteSpace(shipToAddress.ShipToCode)
                            && !string.IsNullOrWhiteSpace(shipToAddress.SalesOrganisationCode)
                        )
                        {
                            erpShipToAddress.Add(shipToAddress);
                        }
                    }
                    catch (Exception ex)
                    {
                        var errorMessage =
                            $"ShipToAddress webhook: An error occurred while mapping data to array. Exception: {ex.Message}";
                        await _erpLogsService.InsertErpLogAsync(
                            ErpLogLevel.Error,
                            ErpSyncLevel.ShipToAddress,
                            errorMessage,
                            ex.StackTrace
                        );

                        response.IsError = "True";
                        response.Message = errorMessage;

                        // updating value to false as this process terminating
                        await _settingService.SetSettingAsync(settingsKey, false, storeScope, true);

                        return BadRequest(response);
                    }
                }
                else
                {
                    // Invalid JSON format
                    var errorMessage = "ShipToAddress webhook: invalid json format in body";
                    await _erpLogsService.InsertErpLogAsync(
                        ErpLogLevel.Error,
                        ErpSyncLevel.ShipToAddress,
                        $"{errorMessage}. Click view to see details.",
                        $"Body: {JsonConvert.SerializeObject(body)}"
                    );

                    response.IsError = "True";
                    response.Message = errorMessage;

                    // updating value to false as this process terminating
                    await _settingService.SetSettingAsync(settingsKey, false, storeScope, true);

                    return BadRequest(response);
                }

                if (erpShipToAddress.Count == 0)
                {
                    var message = "ShipToAddress webhook: no valid ship to address came with call";
                    await _erpLogsService.InsertErpLogAsync(
                        ErpLogLevel.Warning,
                        ErpSyncLevel.ShipToAddress,
                        message
                    );

                    response.IsError = "False";
                    response.Message = message;

                    // updating value to false as this process terminating
                    await _settingService.SetSettingAsync(settingsKey, false, storeScope, true);

                    return Ok(response);
                }

                #endregion

                #region Data upgradtion process start

                await _webhookShipToAddressService.ProcessErpShipToAddressAsync(erpShipToAddress);

                var sqlCommand = "EXEC UpdateB2BShipToAddressFromParallelTableErpB2BShipToAddress;";
                await _nopDataProvider.ExecuteNonQueryAsync(sqlCommand);

                #endregion
            }
            else
            {
                await _erpLogsService.InsertErpLogAsync(
                    ErpLogLevel.Error,
                    ErpSyncLevel.ShipToAddress,
                    "ShipToAddress webhook: Error, the incoming data body is empty."
                );
                response.IsError = "True";
                response.Message = "Request body is null";

                // updating value to false as this process terminating
                await _settingService.SetSettingAsync(settingsKey, false, storeScope, true);

                return BadRequest(response);
            }
        }
        catch (Exception ex)
        {
            await _erpLogsService.InsertErpLogAsync(
                ErpLogLevel.Error,
                ErpSyncLevel.ShipToAddress,
                $"ShipToAddress webhook: Error - {ex.Message}",
                ex.StackTrace
            );
            response.IsError = "True";
            response.Message = $"Error occurred on Ship to Address webhook: {ex.Message}";

            // updating value to false as this process terminating
            await _settingService.SetSettingAsync(settingsKey, false, storeScope, true);

            return StatusCode(StatusCodes.Status500InternalServerError, response);
        }

        // updating value to false as this process terminating
        await _settingService.SetSettingAsync(settingsKey, false, storeScope, true);

        return Ok(response);
    }

    [HttpPost]
    public async Task<IActionResult> Product([FromBody] JToken body)
    {
        await _logger.InsertLogAsync(
            logLevel: LogLevel.Information,
            shortMessage: "Product webhook call",
            fullMessage: $"Body: {JsonConvert.SerializeObject(body)}"
        );

        var response = new WebhookResponseModel()
        {
            IsError = "False",
            Message = "Request processed successfully",
        };

        #region check if previous process running

        bool processAlreadyRunning = _erpWebhookSettings.ProducthookAlreadyRunning;

        if (processAlreadyRunning)
        {
            response.IsError = "False";
            response.Message = $"Previous process is not completed yet";

            await _erpLogsService.InsertErpLogAsync(
                ErpLogLevel.Information,
                ErpSyncLevel.Product,
                "Product webhook call skipped — previous process still running."
            );

            return Ok(response);
        }

        var storeScope = await _storeContext.GetActiveStoreScopeConfigurationAsync();
        var settingsKey = _settingService.GetSettingKey(
            _erpWebhookSettings,
            x => x.ProducthookAlreadyRunning
        );
        // updating value to true as new process running from here
        await _settingService.SetSettingAsync(settingsKey, true, storeScope, true);

        #endregion

        try
        {
            if (body != null)
            {
                #region Preparing JToken Data

                _erpWebhookConfig = await _erpWebhookService.LoadErpWebhookConfigsFromJsonAsync();
                var erpProducts = new List<ErpProductModel>();
                var idsOfProductsToKeep = new HashSet<int>();

                if (body.Type == JTokenType.Array)
                {
                    try
                    {
                        var products = body.ToObject<ErpProductModel[]>()
                            .Take(_erpWebhookConfig?.B2BProductBatchSize ?? 30);
                        if (products != null)
                        {
                            products = products
                                .Where(x => !string.IsNullOrWhiteSpace(x.ShortDescription))
                                .ToArray();

                            erpProducts.AddRange(products);
                        }
                    }
                    catch (Exception ex)
                    {
                        var errorMessage =
                            $"Product webhook: An error occurred while mapping data to array. Exception: {ex.Message}";
                        await _logger.ErrorAsync(errorMessage, ex);

                        response.IsError = "True";
                        response.Message = errorMessage;

                        // updating value to false as this process terminating
                        await _settingService.SetSettingAsync(settingsKey, false, storeScope, true);

                        return BadRequest(response);
                    }
                }
                else if (body.Type == JTokenType.Object)
                {
                    try
                    {
                        var product = body.ToObject<ErpProductModel>();
                        if (product != null && !string.IsNullOrWhiteSpace(product.ShortDescription))
                        {
                            erpProducts.Add(product);
                        }
                    }
                    catch (Exception ex)
                    {
                        var errorMessage =
                            $"Product webhook: An error occurred while mapping data to array. Exception: {ex.Message}";
                        await _logger.ErrorAsync(errorMessage, ex);

                        response.IsError = "True";
                        response.Message = errorMessage;

                        // updating value to false as this process terminating
                        await _settingService.SetSettingAsync(settingsKey, false, storeScope, true);

                        return BadRequest(response);
                    }
                }
                else
                {
                    // Invalid JSON format
                    var errorMessage = $"Product webhook: invalid json format in body";
                    await _logger.InsertLogAsync(
                        LogLevel.Warning,
                        errorMessage,
                        $"body: {JsonConvert.SerializeObject(body)}"
                    );

                    response.IsError = "True";
                    response.Message = errorMessage;

                    // updating value to false as this process terminating
                    await _settingService.SetSettingAsync(settingsKey, false, storeScope, true);

                    return BadRequest(response);
                }

                if (!erpProducts.Any())
                {
                    var message = "Product webhook: no products came with call";
                    await _logger.InformationAsync(message);

                    response.IsError = "False";
                    response.Message = message;

                    // updating value to false as this process terminating
                    await _settingService.SetSettingAsync(settingsKey, false, storeScope, true);

                    return Ok(response);
                }

                #endregion

                #region Data upgradtion process start

                var productWithoutItemNo = erpProducts.Find(p => string.IsNullOrEmpty(p.Sku));
                if (productWithoutItemNo != null)
                {
                    int count = erpProducts.RemoveAll(p => string.IsNullOrEmpty(p.Sku));
                    _logger.Warning(
                        $"Removed from batch {count} materials which had to ItemNo."
                            + $" Example: ItemNo='{productWithoutItemNo.Sku}',"
                            + $" Description='{productWithoutItemNo.ShortDescription}',"
                            + $" BrandDesc='{productWithoutItemNo.ManufacturerDescription}', "
                            + $"VendorName='{productWithoutItemNo.VendorName}'"
                    );
                }
                if (!erpProducts.Any())
                {
                    var message =
                        "Product webhook: no products available after removing materials which had to ItemNo.";
                    await _logger.InformationAsync(message);

                    response.IsError = "False";
                    response.Message = message;

                    // updating value to false as this process terminating
                    await _settingService.SetSettingAsync(settingsKey, false, storeScope, true);

                    return Ok(response);
                }

                await _webhookProductService.ProcessErpProductsToParallelTableAsync(erpProducts);
                await _webhookProductService.ProcessErpProductsAsync();

                var sqlCommand = "Truncate Table [dbo].[Parallel_ErpProduct]";
                await _nopDataProvider.ExecuteNonQueryAsync(sqlCommand);

                #endregion
            }
            else
            {
                await _logger.InformationAsync($"Product webhook call - body null");
                response.Message = "Request body is null";

                // updating value to false as this process terminating
                await _settingService.SetSettingAsync(settingsKey, false, storeScope, true);

                return BadRequest(response);
            }
        }
        catch (Exception ex)
        {
            // updating value to false as this process terminating
            await _settingService.SetSettingAsync(settingsKey, false, storeScope, true);

            await _logger.ErrorAsync($"Product webhook: {ex.Message}", ex);
            response.IsError = "True";
            response.Message = $"Error occurred on product webhook: {ex.Message}";

            return StatusCode(StatusCodes.Status500InternalServerError, response);
        }

        // updating value to false as this process terminating
        await _settingService.SetSettingAsync(settingsKey, false, storeScope, true);

        return Ok(response);
    }

    [HttpPost]
    public async Task<IActionResult> Stock([FromBody] JToken body)
    {
        await _logger.InsertLogAsync(
            logLevel: LogLevel.Information,
            shortMessage: "Stock webhook call",
            fullMessage: $"Body: {JsonConvert.SerializeObject(body)}"
        );

        var response = new WebhookResponseModel()
        {
            IsError = "False",
            Message = "Request processed successfully",
        };

        #region check if previous process running

        bool processAlreadyRunning = _erpWebhookSettings.StockhookAlreadyRunning;

        if (processAlreadyRunning)
        {
            response.IsError = "False";
            response.Message = $"Previous process is not completed yet";

            await _erpLogsService.InsertErpLogAsync(
                ErpLogLevel.Information,
                ErpSyncLevel.Stock,
                "Stock webhook call skipped — previous process still running."
            );

            return Ok(response);
        }

        var storeScope = await _storeContext.GetActiveStoreScopeConfigurationAsync();
        var settingsKey = _settingService.GetSettingKey(
            _erpWebhookSettings,
            x => x.ProducthookAlreadyRunning
        );
        // updating value to true as new process running from here
        await _settingService.SetSettingAsync(settingsKey, true, storeScope, true);

        #endregion

        try
        {
            if (body != null)
            {
                #region Preparing JToken Data

                _erpWebhookConfig = await _erpWebhookService.LoadErpWebhookConfigsFromJsonAsync();
                var erpStockLevels = new List<ErpStockLevelModel>();
                if (body.Type == JTokenType.Array)
                {
                    try
                    {
                        var stockLevels = body.ToObject<ErpStockLevelModel[]>()
                            .Take(_erpWebhookConfig?.B2BStockBatchSize ?? 50);

                        if (stockLevels != null)
                        {
                            stockLevels = stockLevels
                                .Where(x => !string.IsNullOrWhiteSpace(x.Sku))
                                .ToArray();
                            erpStockLevels.AddRange(stockLevels);
                        }
                    }
                    catch (Exception ex)
                    {
                        var errorMessage =
                            $"StockLevel webhook: An error occurred while mapping data to array. Exception: {ex.Message}";
                        await _logger.ErrorAsync(errorMessage, ex);

                        response.IsError = "True";
                        response.Message = errorMessage;

                        await _settingService.SetSettingAsync(settingsKey, false, storeScope, true);

                        return BadRequest(response);
                    }
                }
                else if (body.Type == JTokenType.Object)
                {
                    try
                    {
                        var stockLevel = body.ToObject<ErpStockLevelModel>();
                        if (stockLevel != null && !string.IsNullOrWhiteSpace(stockLevel.Sku))
                        {
                            erpStockLevels.Add(stockLevel);
                        }
                    }
                    catch (Exception ex)
                    {
                        var errorMessage =
                            $"StockLevel webhook: An error occurred while mapping data to object. Exception: {ex.Message}";
                        await _logger.ErrorAsync(errorMessage, ex);

                        response.IsError = "True";
                        response.Message = errorMessage;

                        await _settingService.SetSettingAsync(settingsKey, false, storeScope, true);

                        return BadRequest(response);
                    }
                }
                else
                {
                    var errorMessage = $"StockLevel webhook: invalid json format in body";
                    await _logger.InsertLogAsync(
                        LogLevel.Warning,
                        errorMessage,
                        $"body: {JsonConvert.SerializeObject(body)}"
                    );

                    response.IsError = "True";
                    response.Message = errorMessage;

                    await _settingService.SetSettingAsync(settingsKey, false, storeScope, true);

                    return BadRequest(response);
                }

                if (!erpStockLevels.Any())
                {
                    var message = "StockLevel webhook: no valid stock came with call";
                    await _logger.InformationAsync(message);

                    response.IsError = "False";
                    response.Message = message;

                    await _settingService.SetSettingAsync(settingsKey, false, storeScope, true);

                    return Ok(response);
                }

                #endregion

                #region Data upgradtion process start

                await _webhookERPStockDataService.ProcessERPStockDataToParallelTableAsync(
                    erpStockLevels
                );

                var sqlCommand = "EXEC UpdateErpStockFromParallelTableParallel_ErpStock;";
                await _nopDataProvider.ExecuteNonQueryAsync(sqlCommand);

                #endregion
            }
            else
            {
                await _logger.InformationAsync($"Stock webhook call - body null");
                response.Message = "Request body is null";

                await _settingService.SetSettingAsync(settingsKey, false, storeScope, true);

                return BadRequest(response);
            }
        }
        catch (Exception ex)
        {
            await _logger.ErrorAsync($"Stock webhook: {ex.Message}", ex);
            response.IsError = "True";
            response.Message = $"Error occurred on Stock webhook: {ex.Message}";

            await _settingService.SetSettingAsync(settingsKey, false, storeScope, true);

            return StatusCode(StatusCodes.Status500InternalServerError, response);
        }

        await _settingService.SetSettingAsync(settingsKey, false, storeScope, true);

        return Ok(response);
    }

    [HttpPost]
    public async Task<IActionResult> AccountPricing([FromBody] JToken body)
    {
        await _logger.InsertLogAsync(
            logLevel: LogLevel.Information,
            shortMessage: "Price webhook call",
            fullMessage: $"Body: {JsonConvert.SerializeObject(body)}"
        );

        var response = new WebhookResponseModel()
        {
            IsError = "False",
            Message = "Request processed successfully",
        };

        #region check if previous process running

        bool processAlreadyRunning = _erpWebhookSettings.AccountPricinghookAlreadyRunning;

        if (processAlreadyRunning)
        {
            response.IsError = "False";
            response.Message = $"Previous process is not completed yet";

            await _erpLogsService.InsertErpLogAsync(
                ErpLogLevel.Information,
                ErpSyncLevel.SpecialPrice,
                "Special Price webhook call skipped — previous process still running."
            );
            return Ok(response);
        }

        var storeScope = await _storeContext.GetActiveStoreScopeConfigurationAsync();
        var settingsKey = _settingService.GetSettingKey(
            _erpWebhookSettings,
            x => x.AccountPricinghookAlreadyRunning
        );
        // updating value to true as new process running from here
        await _settingService.SetSettingAsync(settingsKey, true, storeScope, true);

        #endregion

        try
        {
            if (body != null)
            {
                #region Price Group pricing

                if (_b2BB2CFeaturesSettings.UseProductGroupPrice)
                {
                    #region Preparing JToken Data

                    var erpPriceGroupPricings = new List<ErpPriceGroupPricingModel>();
                    if (body.Type == JTokenType.Array)
                    {
                        try
                        {
                            var priceGroupPricings = body.ToObject<ErpPriceGroupPricingModel[]>();
                            if (priceGroupPricings != null)
                            {
                                priceGroupPricings = priceGroupPricings
                                    .Where(x => !string.IsNullOrWhiteSpace(x.PriceGroupCode))
                                    .ToArray();
                                erpPriceGroupPricings.AddRange(priceGroupPricings);
                            }
                        }
                        catch (Exception ex)
                        {
                            var errorMessage =
                                $"AccountPricing webhook: An error occurred while mapping data to array. Exception: {ex.Message}";
                            await _logger.ErrorAsync(errorMessage, ex);

                            response.IsError = "True";
                            response.Message = errorMessage;

                            // updating value to false as this process terminating
                            await _settingService.SetSettingAsync(
                                settingsKey,
                                false,
                                storeScope,
                                true
                            );

                            return BadRequest(response);
                        }
                    }
                    else if (body.Type == JTokenType.Object)
                    {
                        try
                        {
                            var priceGroupPricing = body.ToObject<ErpPriceGroupPricingModel>();
                            if (
                                priceGroupPricing != null
                                && !string.IsNullOrWhiteSpace(priceGroupPricing.PriceGroupCode)
                            )
                            {
                                erpPriceGroupPricings.Add(priceGroupPricing);
                            }
                        }
                        catch (Exception ex)
                        {
                            var errorMessage =
                                $"AccountPricing webhook: An error occurred while mapping data to array. Exception: {ex.Message}";
                            await _logger.ErrorAsync(errorMessage, ex);

                            response.IsError = "True";
                            response.Message = errorMessage;

                            // updating value to false as this process terminating
                            await _settingService.SetSettingAsync(
                                settingsKey,
                                false,
                                storeScope,
                                true
                            );

                            return BadRequest(response);
                        }
                    }
                    else
                    {
                        // Invalid JSON format
                        var errorMessage =
                            $"AccountPricing webhook: invalid json format in body: {JsonConvert.SerializeObject(body)}";
                        await _logger.WarningAsync(errorMessage);

                        response.IsError = "True";
                        response.Message = errorMessage;

                        // updating value to false as this process terminating
                        await _settingService.SetSettingAsync(settingsKey, false, storeScope, true);

                        return BadRequest(response);
                    }

                    if (!erpPriceGroupPricings.Any())
                    {
                        var message =
                            "AccountPricing webhook: no valid account pricing came with call";
                        await _logger.InformationAsync(message);

                        response.IsError = "False";
                        response.Message = message;

                        // updating value to false as this process terminating
                        await _settingService.SetSettingAsync(settingsKey, false, storeScope, true);

                        return Ok(response);
                    }

                    #endregion

                    #region Data upgradtion process start

                    await _webhookERPPriceGroupPricingService.ProcessERPPriceGroupPricingAsync(
                        erpPriceGroupPricings
                    );

                    #endregion
                }

                #endregion

                #region Per Account Product Pricing

                if (!_b2BB2CFeaturesSettings.UseProductGroupPrice)
                {
                    #region Preparing JToken Data

                    _erpWebhookConfig =
                        await _erpWebhookService.LoadErpWebhookConfigsFromJsonAsync();
                    var erpAccountPricings = new List<ErpAccountPricingModel>();
                    if (body.Type == JTokenType.Array)
                    {
                        try
                        {
                            var accountPricings = body.ToObject<ErpAccountPricingModel[]>()
                                .Take(_erpWebhookConfig?.B2BAccountPricingBatchSize ?? 500);
                            if (accountPricings != null)
                            {
                                // Filter out accounts with null or empty AccountNumber
                                var validAccountPricings = accountPricings
                                    .Where(accp => !string.IsNullOrEmpty(accp.AccountNumber))
                                    .ToList();
                                erpAccountPricings.AddRange(validAccountPricings);
                            }
                        }
                        catch (Exception ex)
                        {
                            var errorMessage =
                                $"Group Pricing webhook: An error occurred while mapping data to array. Exception: {ex.Message}";
                            await _logger.ErrorAsync(errorMessage, ex);

                            response.IsError = "True";
                            response.Message = errorMessage;

                            // updating value to false as this process terminating
                            await _settingService.SetSettingAsync(
                                settingsKey,
                                false,
                                storeScope,
                                true
                            );

                            return BadRequest(response);
                        }
                    }
                    else if (body.Type == JTokenType.Object)
                    {
                        try
                        {
                            var accountPricing = body.ToObject<ErpAccountPricingModel>();
                            if (
                                accountPricing != null
                                && !string.IsNullOrWhiteSpace(accountPricing.Sku)
                            )
                            {
                                erpAccountPricings.Add(accountPricing);
                            }
                        }
                        catch (Exception ex)
                        {
                            var errorMessage =
                                $"Group Pricing webhook: An error occurred while mapping data to array. Exception: {ex.Message}";
                            await _logger.ErrorAsync(errorMessage, ex);

                            response.IsError = "True";
                            response.Message = errorMessage;

                            // updating value to false as this process terminating
                            await _settingService.SetSettingAsync(
                                settingsKey,
                                false,
                                storeScope,
                                true
                            );

                            return BadRequest(response);
                        }
                    }
                    else
                    {
                        // Invalid JSON format
                        var errorMessage = $"AccountPricing webhook: invalid json format in body";
                        await _logger.InsertLogAsync(
                            LogLevel.Warning,
                            errorMessage,
                            $"body: {JsonConvert.SerializeObject(body)}"
                        );

                        response.IsError = "True";
                        response.Message = errorMessage;

                        // updating value to false as this process terminating
                        await _settingService.SetSettingAsync(settingsKey, false, storeScope, true);

                        return BadRequest(response);
                    }

                    if (!erpAccountPricings.Any())
                    {
                        var message =
                            "AccountPricing webhook: no valid account pricing came with call";
                        await _logger.InformationAsync(message);

                        response.IsError = "False";
                        response.Message = message;

                        // updating value to false as this process terminating
                        await _settingService.SetSettingAsync(settingsKey, false, storeScope, true);

                        return Ok(response);
                    }

                    #endregion

                    #region Data upgradtion process start

                    await _webhookERPPerAccountPricingService.ProcessERPPerAccountPricingToParallelTableAsync(
                        erpAccountPricings
                    );

                    var sqlCommand =
                        "EXEC UpdateErp_Special_PriceFromParallelTableParallel_ErpAccountPricing;";
                    await _nopDataProvider.ExecuteNonQueryAsync(sqlCommand);

                    #endregion
                }

                #endregion
            }
            else
            {
                await _logger.WarningAsync($"Price webhook call - body null");
                response.Message = "Request body is null";

                // updating value to false as this process terminating
                await _settingService.SetSettingAsync(settingsKey, false, storeScope, true);

                return BadRequest(response);
            }
        }
        catch (Exception ex)
        {
            await _logger.ErrorAsync($"Price webhook: {ex.Message}", ex);
            response.IsError = "True";
            response.Message = $"Error occurred on Price webhook: {ex.Message}";

            // updating value to false as this process terminating
            await _settingService.SetSettingAsync(settingsKey, false, storeScope, true);

            return StatusCode(StatusCodes.Status500InternalServerError, response);
        }

        // updating value to false as this process terminating
        await _settingService.SetSettingAsync(settingsKey, false, storeScope, true);

        return Ok(response);
    }

    [HttpPost]
    public async Task<IActionResult> Order([FromBody] JToken body)
    {
        await _logger.InsertLogAsync(
            logLevel: LogLevel.Information,
            shortMessage: "Order webhook call",
            fullMessage: $"Body: {JsonConvert.SerializeObject(body)}"
        );

        var response = new WebhookResponseModel()
        {
            IsError = "False",
            Message = "Request processed successfully",
        };

        #region check if previous process running

        bool processAlreadyRunning = _erpWebhookSettings.OrderhookAlreadyRunning;

        if (processAlreadyRunning)
        {
            response.IsError = "False";
            response.Message = $"Previous process is not completed yet";

            await _erpLogsService.InsertErpLogAsync(
                ErpLogLevel.Information,
                ErpSyncLevel.Order,
                "Order webhook call skipped — previous process still running."
            );
            return Ok(response);
        }

        var storeScope = await _storeContext.GetActiveStoreScopeConfigurationAsync();
        var settingsKey = _settingService.GetSettingKey(
            _erpWebhookSettings,
            x => x.OrderhookAlreadyRunning
        );
        // updating value to true as new process running from here
        await _settingService.SetSettingAsync(settingsKey, true, storeScope, true);

        #endregion

        try
        {
            if (body != null)
            {
                #region Preparing JToken Data

                _erpWebhookConfig = await _erpWebhookService.LoadErpWebhookConfigsFromJsonAsync();
                var erpOrders = new List<ErpOrderModel>();
                if (body.Type == JTokenType.Array)
                {
                    try
                    {
                        var orders = body.ToObject<ErpOrderModel[]>()
                            .Take(_erpWebhookConfig?.B2BOrderBatchSize ?? 10);
                        if (orders != null)
                        {
                            orders = orders
                                .Where(x => !string.IsNullOrWhiteSpace(x.AccountNumber))
                                .ToArray();
                            erpOrders.AddRange(orders);
                        }
                    }
                    catch (Exception ex)
                    {
                        var errorMessage =
                            $"Orders webhook: An error occurred while mapping data to array. Exception: {ex.Message}";
                        await _logger.ErrorAsync(errorMessage, ex);

                        response.IsError = "True";
                        response.Message = errorMessage;

                        // updating value to false as this process terminating
                        await _settingService.SetSettingAsync(settingsKey, false, storeScope, true);

                        return BadRequest(response);
                    }
                }
                else if (body.Type == JTokenType.Object)
                {
                    try
                    {
                        var order = body.ToObject<ErpOrderModel>();
                        if (order != null && !string.IsNullOrWhiteSpace(order.AccountNumber))
                        {
                            erpOrders.Add(order);
                        }
                    }
                    catch (Exception ex)
                    {
                        var errorMessage =
                            $"Orders webhook: An error occurred while mapping data to array. Exception: {ex.Message}";
                        await _logger.ErrorAsync(errorMessage, ex);

                        response.IsError = "True";
                        response.Message = errorMessage;

                        // updating value to false as this process terminating
                        await _settingService.SetSettingAsync(settingsKey, false, storeScope, true);

                        return BadRequest(response);
                    }
                }
                else
                {
                    // Invalid JSON format
                    var errorMessage = $"Orders webhook: invalid json format in body";
                    await _logger.InsertLogAsync(
                        LogLevel.Warning,
                        errorMessage,
                        $"body: {JsonConvert.SerializeObject(body)}"
                    );

                    response.IsError = "True";
                    response.Message = errorMessage;

                    // updating value to false as this process terminating
                    await _settingService.SetSettingAsync(settingsKey, false, storeScope, true);

                    return BadRequest(response);
                }

                if (!erpOrders.Any())
                {
                    var message = "Orders webhook: no valid order came with call";
                    await _logger.InformationAsync(message);

                    response.IsError = "False";
                    response.Message = message;

                    // updating value to false as this process terminating
                    await _settingService.SetSettingAsync(settingsKey, false, storeScope, true);

                    return Ok(response);
                }

                #endregion

                #region Data upgradtion process start

                await _webhookB2BOrderService.ProcessErpOrdersToParallelTableAsync(erpOrders);
                await _webhookB2BOrderService.ProcessErpOrdersAsync();

                var sqlCommand = "Truncate Table [dbo].[Parallel_ErpOrder];";
                await _nopDataProvider.ExecuteNonQueryAsync(sqlCommand);

                #endregion
            }
            else
            {
                await _logger.InformationAsync($"Order webhook call - body null");
                response.Message = "Request body is null";

                // updating value to false as this process terminating
                await _settingService.SetSettingAsync(settingsKey, false, storeScope, true);

                return BadRequest(response);
            }
        }
        catch (Exception ex)
        {
            await _logger.ErrorAsync($"Order webhook: {ex.Message}", ex);
            response.IsError = "True";
            response.Message = $"Error occurred on Order webhook: {ex.Message}";

            // updating value to false as this process terminating
            await _settingService.SetSettingAsync(settingsKey, false, storeScope, true);

            return StatusCode(StatusCodes.Status500InternalServerError, response);
        }

        // updating value to false as this process terminating
        await _settingService.SetSettingAsync(settingsKey, false, storeScope, true);

        return Ok(response);
    }

    [HttpPost]
    public async Task<IActionResult> Credit([FromBody] JToken body)
    {
        await _logger.InsertLogAsync(
            logLevel: LogLevel.Information,
            shortMessage: "Credit webhook call",
            fullMessage: $"Body: {JsonConvert.SerializeObject(body)}"
        );

        var response = new WebhookResponseModel()
        {
            IsError = "False",
            Message = "Request processed successfully",
        };

        #region check if previous process running

        bool processAlreadyRunning = _erpWebhookSettings.CredithookAlreadyRunning;

        if (processAlreadyRunning)
        {
            response.IsError = "False";
            response.Message = $"Previous process is not completed yet";

            return Ok(response);
        }

        var storeScope = await _storeContext.GetActiveStoreScopeConfigurationAsync();
        var settingsKey = _settingService.GetSettingKey(
            _erpWebhookSettings,
            x => x.CredithookAlreadyRunning
        );
        // updating value to true as new process running from here
        await _settingService.SetSettingAsync(settingsKey, true, storeScope, true);

        #endregion

        try
        {
            if (body != null)
            {
                #region Preparing JToken Data

                _erpWebhookConfig = await _erpWebhookService.LoadErpWebhookConfigsFromJsonAsync();
                var credits = new List<Credit>();
                if (body.Type == JTokenType.Array)
                {
                    try
                    {
                        var erpCredits = body.ToObject<Credit[]>()
                            .Take(_erpWebhookConfig?.B2BCreditBatchSize ?? 200);

                        if (erpCredits != null)
                        {
                            erpCredits = erpCredits
                                .Where(x =>
                                    !string.IsNullOrWhiteSpace(x.AccountNumber)
                                    && !string.IsNullOrWhiteSpace(x.SalesOrganisationCode)
                                )
                                .ToArray();

                            credits.AddRange(erpCredits);
                        }
                    }
                    catch (Exception ex)
                    {
                        var errorMessage =
                            $"Credit webhook: An error occurred while mapping data to array. Exception: {ex.Message}";
                        await _logger.ErrorAsync(errorMessage, ex);

                        response.IsError = "True";
                        response.Message = errorMessage;

                        // updating value to false as this process terminating
                        await _settingService.SetSettingAsync(settingsKey, false, storeScope, true);

                        return BadRequest(response);
                    }
                }
                else if (body.Type == JTokenType.Object)
                {
                    try
                    {
                        var credit = body.ToObject<Credit>();
                        if (
                            credit != null
                            && !string.IsNullOrWhiteSpace(credit.AccountNumber)
                            && !string.IsNullOrWhiteSpace(credit.SalesOrganisationCode)
                        )
                        {
                            credits.Add(credit);
                        }
                    }
                    catch (Exception ex)
                    {
                        var errorMessage =
                            $"Credit webhook: An error occurred while mapping data to array. Exception: {ex.Message}";
                        await _logger.ErrorAsync(errorMessage, ex);

                        response.IsError = "True";
                        response.Message = errorMessage;

                        // updating value to false as this process terminating
                        await _settingService.SetSettingAsync(settingsKey, false, storeScope, true);

                        return BadRequest(response);
                    }
                }
                else
                {
                    // Invalid JSON format
                    var errorMessage = $"Credit webhook: invalid json format in body";
                    await _logger.InsertLogAsync(
                        LogLevel.Warning,
                        errorMessage,
                        $"body: {JsonConvert.SerializeObject(body)}"
                    );

                    response.IsError = "True";
                    response.Message = errorMessage;

                    // updating value to false as this process terminating
                    await _settingService.SetSettingAsync(settingsKey, false, storeScope, true);

                    return BadRequest(response);
                }

                if (!credits.Any())
                {
                    var message = "Credit webhook: no Credits came with call";
                    await _logger.InformationAsync(message);

                    response.IsError = "False";
                    response.Message = message;

                    // updating value to false as this process terminating
                    await _settingService.SetSettingAsync(settingsKey, false, storeScope, true);

                    return Ok(response);
                }

                #endregion

                #region Data upgradtion process start

                await _webhookAccountService.ProcessCreditsAsync(credits);

                #endregion

                await _logger.InformationAsync($"Credit webhook: finished");
            }
            else
            {
                await _logger.InformationAsync($"Credit webhook call - body null");
                response.Message = "Request body is null";

                // updating value to false as this process terminating
                await _settingService.SetSettingAsync(settingsKey, false, storeScope, true);

                return BadRequest(response);
            }
        }
        catch (Exception ex)
        {
            await _logger.ErrorAsync($"Credit webhook: {ex.Message}", ex);
            response.IsError = "True";
            response.Message = $"Error occurred on Credit webhook: {ex.Message}";

            // updating value to false as this process terminating
            await _settingService.SetSettingAsync(settingsKey, false, storeScope, true);

            return StatusCode(StatusCodes.Status500InternalServerError, response);
        }

        // updating value to false as this process terminating
        await _settingService.SetSettingAsync(settingsKey, false, storeScope, true);

        return Ok(response);
    }

    [HttpPost]
    public async Task<IActionResult> DeliveryDates([FromBody] JToken body)
    {
        await _erpLogsService.InsertErpLogAsync(
            ErpLogLevel.Information,
            ErpSyncLevel.DeliveryDates,
            "DeliveryDates webhook: Call started. Click view to see details.",
            $"DeliveryDates webhook: Call started, Body: {JsonConvert.SerializeObject(body)}"
        );

        var response = new WebhookResponseModel();

        #region check if previous process running

        if (_erpWebhookSettings.DeliveryDateshookAlreadyRunning)
        {
            response.IsError = "False";
            response.Message = $"Previous process is not completed yet";

            await _erpLogsService.InsertErpLogAsync(
                ErpLogLevel.Information,
                ErpSyncLevel.DeliveryDates,
                "Delivery Dates webhook call skipped — previous process still running."
            );

            return Ok(response);
        }

        var storeScope = await _storeContext.GetActiveStoreScopeConfigurationAsync();
        var settingsKey = _settingService.GetSettingKey(
            _erpWebhookSettings,
            x => x.DeliveryDateshookAlreadyRunning
        );
        // updating value to true as new process running from here
        await _settingService.SetSettingAsync(settingsKey, true, storeScope, true);

        #endregion

        try
        {
            if (body != null)
            {
                #region Preparing JToken Data

                _erpWebhookConfig = await _erpWebhookService.LoadErpWebhookConfigsFromJsonAsync();
                var erpDeliveryDates = new List<DeliveryDatesModel>();
                if (body.Type == JTokenType.Array)
                {
                    try
                    {
                        var deliveryDates = body.ToObject<DeliveryDatesModel[]>()
                            .Take(_erpWebhookConfig?.DeliveryDatesBatchSize ?? 200);
                        if (deliveryDates != null)
                        {
                            erpDeliveryDates.AddRange(
                                deliveryDates.Where(x =>
                                    !string.IsNullOrWhiteSpace(x.SalesOrgOrPlant)
                                )
                            );
                        }
                    }
                    catch (Exception ex)
                    {
                        var errorMessage =
                            $"DeliveryDates webhook: An error occurred while mapping data to array. Exception: {ex.Message}";
                        await _erpLogsService.InsertErpLogAsync(
                            ErpLogLevel.Error,
                            ErpSyncLevel.DeliveryDates,
                            errorMessage,
                            ex.StackTrace
                        );

                        response.IsError = "True";
                        response.Message = errorMessage;

                        // updating value to false as this process terminating
                        await _settingService.SetSettingAsync(settingsKey, false, storeScope, true);

                        return BadRequest(response);
                    }
                }
                else if (body.Type == JTokenType.Object)
                {
                    try
                    {
                        var deliveryDate = body.ToObject<DeliveryDatesModel>();
                        if (
                            deliveryDate != null
                            && !string.IsNullOrWhiteSpace(deliveryDate.SalesOrgOrPlant)
                        )
                        {
                            erpDeliveryDates.Add(deliveryDate);
                        }
                    }
                    catch (Exception ex)
                    {
                        var errorMessage =
                            $"DeliveryDates webhook: An error occurred while mapping data to array. Exception: {ex.Message}";
                        await _erpLogsService.InsertErpLogAsync(
                            ErpLogLevel.Error,
                            ErpSyncLevel.DeliveryDates,
                            errorMessage,
                            ex.StackTrace
                        );

                        response.IsError = "True";
                        response.Message = errorMessage;

                        // updating value to false as this process terminating
                        await _settingService.SetSettingAsync(settingsKey, false, storeScope, true);

                        return BadRequest(response);
                    }
                }
                else
                {
                    // Invalid JSON format
                    var errorMessage = $"DeliveryDates webhook: invalid json format in body";
                    await _erpLogsService.InsertErpLogAsync(
                        ErpLogLevel.Error,
                        ErpSyncLevel.DeliveryDates,
                        $"{errorMessage}. Click view to see details.",
                        $"Body: {JsonConvert.SerializeObject(body)}"
                    );

                    response.IsError = "True";
                    response.Message = errorMessage;

                    // updating value to false as this process terminating
                    await _settingService.SetSettingAsync(settingsKey, false, storeScope, true);

                    return BadRequest(response);
                }

                if (erpDeliveryDates.Count == 0)
                {
                    var message = "DeliveryDates webhook: no delivery dates came with call";
                    await _erpLogsService.InsertErpLogAsync(
                        ErpLogLevel.Warning,
                        ErpSyncLevel.DeliveryDates,
                        message
                    );

                    response.IsError = "False";
                    response.Message = message;

                    // updating value to false as this process terminating
                    await _settingService.SetSettingAsync(settingsKey, false, storeScope, true);

                    return Ok(response);
                }

                #endregion

                #region Data upgradtion process start

                await _webhookDeliveryDatesService.ProcessDeliveryDatesAsync(erpDeliveryDates);

                await _erpLogsService.InsertErpLogAsync(
                    ErpLogLevel.Information,
                    ErpSyncLevel.DeliveryDates,
                    "DeliveryDates webhook: Call finished."
                );

                #endregion
            }
            else
            {
                await _erpLogsService.InsertErpLogAsync(
                    ErpLogLevel.Error,
                    ErpSyncLevel.DeliveryDates,
                    "DeliveryDates webhook: Error, the incoming data body is empty."
                );

                response.IsError = "True";
                response.Message = "Request body is null";

                // updating value to false as this process terminating
                await _settingService.SetSettingAsync(settingsKey, false, storeScope, true);

                return BadRequest(response);
            }
        }
        catch (Exception ex)
        {
            await _erpLogsService.InsertErpLogAsync(
                ErpLogLevel.Error,
                ErpSyncLevel.DeliveryDates,
                $"DeliveryDates webhook: Error - {ex.Message}",
                ex.StackTrace
            );

            response.IsError = "True";
            response.Message = $"Error occurred on DeliveryDates webhook: {ex.Message}";

            // updating value to false as this process terminating
            await _settingService.SetSettingAsync(settingsKey, false, storeScope, true);

            return StatusCode(StatusCodes.Status500InternalServerError, response);
        }

        await _settingService.SetSettingAsync(settingsKey, false, storeScope, true);

        return Ok(response);
    }

    [HttpPost]
    public async Task<IActionResult> ProductsImage([FromBody] JToken body)
    {
        await _logger.InsertLogAsync(
            logLevel: LogLevel.Information,
            shortMessage: "ProductsImage webhook call",
            fullMessage: $"Body: {JsonConvert.SerializeObject(body)}"
        );

        var response = new WebhookResponseModel()
        {
            IsError = "False",
            Message = "Request processed successfully",
        };

        #region check if previous process running

        bool processAlreadyRunning = _erpWebhookSettings.ProductsImagehookAlreadyRunning;

        if (processAlreadyRunning)
        {
            response.IsError = "False";
            response.Message = $"Previous process is not completed yet";

            await _erpLogsService.InsertErpLogAsync(
                ErpLogLevel.Information,
                ErpSyncLevel.Product,
                "Products Image call skipped — previous process still running."
            );

            return Ok(response);
        }

        var storeScope = await _storeContext.GetActiveStoreScopeConfigurationAsync();
        var settingsKey = _settingService.GetSettingKey(
            _erpWebhookSettings,
            x => x.ProductsImagehookAlreadyRunning
        );
        // updating value to true as new process running from here
        await _settingService.SetSettingAsync(settingsKey, true, storeScope, true);

        #endregion

        try
        {
            if (body != null)
            {
                #region Preparing JToken Data

                _erpWebhookConfig = await _erpWebhookService.LoadErpWebhookConfigsFromJsonAsync();
                var erpProductsImage = new List<ErpProductsImageModel>();
                if (body.Type == JTokenType.Array)
                {
                    try
                    {
                        var productsImage = body.ToObject<ErpProductsImageModel[]>()
                            .Take(_erpWebhookConfig?.ProductsImageBatchSize ?? 200);
                        if (productsImage != null)
                        {
                            erpProductsImage.AddRange(
                                productsImage.Where(x =>
                                    !string.IsNullOrWhiteSpace(x.Sku)
                                    && !string.IsNullOrWhiteSpace(x.ImageBase64)
                                )
                            );
                        }
                    }
                    catch (Exception ex)
                    {
                        var errorMessage =
                            $"ProductsImage webhook: An error occurred while mapping data to array. Exception: {ex.Message}";
                        await _logger.ErrorAsync(errorMessage, ex);

                        response.IsError = "True";
                        response.Message = errorMessage;

                        // updating value to false as this process terminating
                        await _settingService.SetSettingAsync(settingsKey, false, storeScope, true);

                        return BadRequest(response);
                    }
                }
                else if (body.Type == JTokenType.Object)
                {
                    try
                    {
                        var productsImage = body.ToObject<ErpProductsImageModel>();
                        if (
                            productsImage != null
                            && !string.IsNullOrWhiteSpace(productsImage.Sku)
                            && !string.IsNullOrWhiteSpace(productsImage.ImageBase64)
                        )
                        {
                            erpProductsImage.Add(productsImage);
                        }
                    }
                    catch (Exception ex)
                    {
                        var errorMessage =
                            $"ProductsImage webhook: An error occurred while mapping data to array. Exception: {ex.Message}";
                        await _logger.ErrorAsync(errorMessage, ex);

                        response.IsError = "True";
                        response.Message = errorMessage;

                        // updating value to false as this process terminating
                        await _settingService.SetSettingAsync(settingsKey, false, storeScope, true);

                        return BadRequest(response);
                    }
                }
                else
                {
                    // Invalid JSON format
                    await _logger.InsertLogAsync(
                        LogLevel.Warning,
                        $"ProductsImages webhook: invalid json format in body",
                        $"Body:{JsonConvert.SerializeObject(body)}"
                    );

                    response.IsError = "True";
                    response.Message = "Invalid json format in body";

                    // updating value to false as this process terminating
                    await _settingService.SetSettingAsync(settingsKey, false, storeScope, true);

                    return BadRequest(response);
                }

                if (!erpProductsImage.Any())
                {
                    var message = "ProductsImages webhook: no image came with call";
                    await _logger.InformationAsync(message);

                    response.IsError = "False";
                    response.Message = message;

                    // updating value to false as this process terminating
                    await _settingService.SetSettingAsync(settingsKey, false, storeScope, true);

                    return Ok(response);
                }

                #endregion

                #region Data upgradtion process start

                await _webhookProductsImageService.ProcessProductsImageAsync(erpProductsImage);

                #endregion
            }
            else
            {
                await _logger.InformationAsync($"ProductsImage webhook call - body null");
                response.Message = "Request body is null";

                // updating value to false as this process terminating
                await _settingService.SetSettingAsync(settingsKey, false, storeScope, true);

                return BadRequest(response);
            }
        }
        catch (Exception ex)
        {
            await _logger.ErrorAsync($"ProductsImage webhook: {ex.Message}", ex);
            response.IsError = "True";
            response.Message = $"Error occurred on ProductsImage webhook: {ex.Message}";

            // updating value to false as this process terminating
            await _settingService.SetSettingAsync(settingsKey, false, storeScope, true);

            return StatusCode(StatusCodes.Status500InternalServerError, response);
        }

        await _settingService.SetSettingAsync(settingsKey, false, storeScope, true);

        return Ok();
    }

    #endregion
}
