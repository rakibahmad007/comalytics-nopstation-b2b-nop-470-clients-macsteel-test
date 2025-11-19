using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;
using Nop.Core;
using Nop.Core.Domain.Customers;
using Nop.Services.Catalog;
using Nop.Services.Common;
using Nop.Services.Configuration;
using Nop.Services.Customers;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Messages;
using Nop.Services.Orders;
using Nop.Services.Security;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Mvc;
using NopStation.Plugin.B2B.B2BB2CFeatures.Factories;
using NopStation.Plugin.B2B.B2BB2CFeatures.Factories.PDF;
using NopStation.Plugin.B2B.B2BB2CFeatures.Model;
using NopStation.Plugin.B2B.B2BB2CFeatures.Model.Checkout;
using NopStation.Plugin.B2B.B2BB2CFeatures.Model.ErpAccountPublic;
using NopStation.Plugin.B2B.B2BB2CFeatures.Services.ErpCustomerFunctionality;
using NopStation.Plugin.B2B.B2BB2CFeatures.Services.ErpPriceSyncFunctionality;
using NopStation.Plugin.B2B.ERPIntegrationCore;
using NopStation.Plugin.B2B.ERPIntegrationCore.Domain;
using NopStation.Plugin.B2B.ERPIntegrationCore.Enums;
using NopStation.Plugin.B2B.ERPIntegrationCore.Infrastructure;
using NopStation.Plugin.B2B.ERPIntegrationCore.Services;

namespace NopStation.Plugin.B2B.B2BB2CFeatures.Controllers;

public class ErpAccountPublicController : BasePluginController
{
    #region Fields

    private readonly ICustomerService _customerService;
    private readonly IWorkContext _workContext;
    private readonly IErpAccountService _erpAccountService;
    private readonly IErpNopUserService _erpNopUserService;
    private readonly IPermissionService _permissionService;
    private readonly IErpAccountPublicModelFactory _erpAccountPublicModelFactory;
    private readonly IErpCustomerFunctionalityService _erpCustomerFunctionalityService;
    private readonly ISettingService _settingService;
    private readonly IStoreContext _storeContext;
    private readonly IErpLogsService _erpLogsService;
    private readonly INotificationService _notificationService;
    private readonly ILocalizationService _localizationService;
    private readonly IErpActivityLogsService _erpActivityLogsService;
    private readonly IErpSalesOrgService _erpSalesOrgService;
    private readonly IErpInvoiceService _erpInvoiceService;
    private readonly IErpIntegrationPluginManager _erpIntegrationPluginManager;
    private readonly IErpPriceSyncFunctionalityService _erpPriceSyncFunctionalityService;
    private readonly B2BB2CFeaturesSettings _b2BB2CFeaturesSettings;
    private readonly ILogger _logger;
    private readonly IOrderService _orderService;
    private readonly IErpOrderAdditionalDataService _erpOrderAdditionalDataService;
    private readonly CustomerSettings _customerSettings;
    private readonly IGenericAttributeService _genericAttributeService;
    private readonly IErpProductModelFactory _erpProductModelFactory;
    protected readonly IPriceFormatter _priceFormatter;
    private readonly IErpPdfModelFactory _erpPdfModelFactory;

    #endregion Fields

    #region Ctor

    public ErpAccountPublicController(
        ICustomerService customerService,
        IWorkContext workContext,
        IErpAccountService erpAccountService,
        IErpNopUserService erpNopUserService,
        IPermissionService permissionService,
        IErpAccountPublicModelFactory erpAccountPublicModelFactory,
        IErpCustomerFunctionalityService erpCustomerFunctionalityService,
        ISettingService settingService,
        IStoreContext storeContext,
        IErpLogsService erpLogsService,
        INotificationService notificationService,
        ILocalizationService localizationService,
        IErpActivityLogsService erpActivityLogsService,
        IErpSalesOrgService erpSalesOrgService,
        B2BB2CFeaturesSettings b2BB2CFeaturesSettings,
        IErpIntegrationPluginManager erpIntegrationPluginManager,
        IErpPriceSyncFunctionalityService erpPriceSyncFunctionalityService,
        IErpInvoiceService erpInvoiceService,
        ILogger logger,
        IOrderService orderService,
        IErpOrderAdditionalDataService erpOrderAdditionalDataService,
        CustomerSettings customerSettings,
        IGenericAttributeService genericAttributeService,
        IErpProductModelFactory erpProductModelFactory,
        IPriceFormatter priceFormatter,
        IErpPdfModelFactory erpPdfModelFactory)
    {
        _customerService = customerService;
        _workContext = workContext;
        _erpAccountService = erpAccountService;
        _erpNopUserService = erpNopUserService;
        _permissionService = permissionService;
        _erpAccountPublicModelFactory = erpAccountPublicModelFactory;
        _erpCustomerFunctionalityService = erpCustomerFunctionalityService;
        _settingService = settingService;
        _storeContext = storeContext;
        _erpLogsService = erpLogsService;
        _notificationService = notificationService;
        _localizationService = localizationService;
        _erpActivityLogsService = erpActivityLogsService;
        _erpSalesOrgService = erpSalesOrgService;
        _erpInvoiceService = erpInvoiceService;
        _erpIntegrationPluginManager = erpIntegrationPluginManager;
        _erpPriceSyncFunctionalityService = erpPriceSyncFunctionalityService;
        _b2BB2CFeaturesSettings = b2BB2CFeaturesSettings;
        _logger = logger;
        _orderService = orderService;
        _erpOrderAdditionalDataService = erpOrderAdditionalDataService;
        _customerSettings = customerSettings;
        _erpProductModelFactory = erpProductModelFactory;
        _genericAttributeService = genericAttributeService;
        _priceFormatter = priceFormatter;
        _erpPdfModelFactory = erpPdfModelFactory;
    }

    #endregion Ctor

    #region Utilities

    private async Task<(ErpAccount erpAccount, ErpNopUser erpNopUser)> GetErpAccountAndUserOfCurrentCustomerAsync(int customerId)
    {
        var erpNopUser = await _erpNopUserService.GetErpNopUserByCustomerIdAsync(customerId, showHidden: false);
        var erpAccount = await _erpAccountService.GetErpAccountByIdAsync(erpNopUser?.ErpAccountId ?? 0);

        return (erpAccount, erpNopUser);
    }

    [HttpGet]
    public async Task<IActionResult> IsHideAddToCart()
    {
        var isHideAddToCart = await _erpCustomerFunctionalityService.IsHideAddToCartAsync();
        return Json(new { success = isHideAddToCart });
    }

    [HttpGet]
    public async Task<IActionResult> IsShowAddToCart()
    {
        var isHideAddToCart = await _erpCustomerFunctionalityService.IsHideAddToCartAsync();
        return Json(new { success = !isHideAddToCart });
    }

    public async Task<IActionResult> GetPasswordRequirements()
    {
        var passwordLengthValidation = string.Format(
            await _localizationService.GetResourceAsync("Validation.Password.LengthValidation"),
            _customerSettings.PasswordMinLength
        );
        var requireUppercase = _customerSettings.PasswordRequireUppercase
            ? await _localizationService.GetResourceAsync("Validation.Password.RequireUppercase")
            : string.Empty;
        var requireLowercase = _customerSettings.PasswordRequireLowercase
            ? await _localizationService.GetResourceAsync("Validation.Password.RequireLowercase")
            : string.Empty;
        var requireDigit = _customerSettings.PasswordRequireDigit
            ? await _localizationService.GetResourceAsync("Validation.Password.RequireDigit")
            : string.Empty;
        var requireNonAlphanumeric = _customerSettings.PasswordRequireNonAlphanumeric
            ? await _localizationService.GetResourceAsync(
                "Validation.Password.RequireNonAlphanumeric"
            )
            : string.Empty;

        var message = string.Format(
            await _localizationService.GetResourceAsync("Validation.Password.Rule"),
            passwordLengthValidation,
            requireUppercase,
            requireLowercase,
            requireDigit,
            requireNonAlphanumeric
        );

        return Json(new { success = true, message = message });
    }

    public async Task<IActionResult> CheckUserRegistered()
    {
        return Json(
            new
            {
                success = await _customerService.IsRegisteredAsync(
                    await _workContext.GetCurrentCustomerAsync()
                ),
            }
        );
    }

    public async Task<IActionResult> ErpShippingAddresses()
    {
        var currCustomer = await _workContext.GetCurrentCustomerAsync();
        if (!await _customerService.IsRegisteredAsync(currCustomer))
            return Challenge();

        var (erpAccount, erpNopUser) = await GetErpAccountAndUserOfCurrentCustomerAsync(currCustomer.Id);
        if (erpAccount == null || erpNopUser == null)
            return RedirectToRoute("CustomerInfo");

        var model = await _erpAccountPublicModelFactory.PrepareB2BShippingAddressListModel(erpAccount, erpNopUser, new ErpShipToAddressListModel());
        model.ErpNopUser = erpNopUser;
        return View(model);
    }

    public async Task<IActionResult> ErpBillingAddresses()
    {
        var currCustomer = await _workContext.GetCurrentCustomerAsync();
        if (!await _customerService.IsRegisteredAsync(currCustomer))
            return Challenge();

        var (erpAccount, erpNopUser) = await GetErpAccountAndUserOfCurrentCustomerAsync(currCustomer.Id);
        if (erpAccount == null || erpNopUser == null)
            return RedirectToRoute("CustomerInfo");

        var model = await _erpAccountPublicModelFactory.PrepareB2BBillingAddressModel(erpAccount, erpNopUser, new ErpBillingAddressModel());
        return View(model);
    }

    #endregion

    #region Methods

    #region Customer Configuration

    public async Task<IActionResult> ErpCustomerConfiguration()
    {
        var currentCustomer = await _workContext.GetCurrentCustomerAsync();

        if (!await _customerService.IsRegisteredAsync(currentCustomer))
            return Challenge();

        var erpAccount = await _erpCustomerFunctionalityService.GetActiveErpAccountByCustomerAsync(
            currentCustomer
        );
        if (erpAccount == null)
            return RedirectToRoute("CustomerInfo");

        var model = await _erpAccountPublicModelFactory.PrepareB2BCustomerConfigurationModelAsync(
            erpAccount,
            new ErpCustomerConfigurationModel()
        );
        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> ErpCustomerConfiguration(ErpCustomerConfigurationModel model)
    {
        if (model == null)
        {
            _notificationService.Notification(NotifyType.Error, "No data found to save");
        }

        var currentCustomer = await _workContext.GetCurrentCustomerAsync();

        if (!await _customerService.IsRegisteredAsync(currentCustomer))
            return Challenge();

        var erpAccount = await _erpCustomerFunctionalityService.GetActiveErpAccountByCustomerAsync(
            currentCustomer
        );
        if (erpAccount == null)
            return RedirectToRoute("CustomerInfo");

        await _erpAccountPublicModelFactory.SetB2BCustomerConfigurationModelAsync(
            erpAccount,
            model
        );
        _notificationService.Notification(NotifyType.Success, "Configuration Updated");
        return RedirectToAction("ErpCustomerConfiguration");
    }

    #endregion

    #region Product

    public async Task<IActionResult> LoadErpProductData(List<string> productIds)
    {
        var currentCustomer = await _workContext.GetCurrentCustomerAsync();

        var shortMsg = string.Empty;
        var fullMessage = string.Empty;

        if (productIds.Count < 1)
        {
            shortMsg = $"Failed to load Erp Product Data Model for " +
                $"Customer Email: {currentCustomer.Email} (Id: {currentCustomer.Id})";
            fullMessage = $"{shortMsg}. Product List is empty";

            await _erpLogsService.InsertErpLogAsync(
                ErpLogLevel.Error,
                ErpSyncLevel.Product,
                shortMsg,
                fullMessage: fullMessage
            );

            return Json(new { success = false });
        }

        var erpAccount = await _erpCustomerFunctionalityService.GetActiveErpAccountByCustomerAsync(
            currentCustomer
        );

        if (erpAccount == null)
        {
            shortMsg = $"Failed to load Erp Product Data Model for " +
                $"Customer Email: {currentCustomer.Email} (Id: {currentCustomer.Id})";
            fullMessage = $"{shortMsg}. Erp Account not found for the customer";

            await _erpLogsService.InsertErpLogAsync(
                ErpLogLevel.Error,
                ErpSyncLevel.Product,
                shortMsg,
                fullMessage: fullMessage
            );
            return Json(new { success = false });
        }

        var model = await _erpProductModelFactory.PrepareErpProductDataListModelAsync(
            productIds,
            erpAccount
        );

        if (model == null)
        {
            shortMsg = $"Failed to load Erp Product Data Model for " +
            $"Customer Email: {currentCustomer.Email} (Id: {currentCustomer.Id}), " +
            $"Erp Account: {erpAccount.AccountNumber} (Id: {erpAccount.Id})";

            await _erpLogsService.InsertErpLogAsync(
                ErpLogLevel.Error,
                ErpSyncLevel.Product,
                shortMsg,
                fullMessage: fullMessage
            );

            return Json(new { success = false });
        }

        var erpCustomerConfiguration =
        await _erpCustomerFunctionalityService.GetErpCustomerConfigurationByNopCustomerIdAsync(
            currentCustomer.Id
        );

        var isHideWeightinfo = !_b2BB2CFeaturesSettings.DisplayWeightInformation || erpCustomerConfiguration.IsHideWeightInfo;
        return Json(
            new
            {
                Data = model,
                IsHidePricingnote = erpCustomerConfiguration.IsHidePricingNote,
                IsHideWeightinfo = isHideWeightinfo,
            }
        );
    }

    public async Task<IActionResult> LoadErpProductLiveStock(int productId)
    {
        var erpAccount = await _erpCustomerFunctionalityService.GetActiveErpAccountByCustomerAsync(await _workContext.GetCurrentCustomerAsync());

        if (erpAccount == null)
        {
            return Json(new
            {
                Msg = "No b2b account found"
            });
        }

        //get availability msg
        var availability = await _erpProductModelFactory.UpdateLiveStockAndGetProductAvailabilityAsync(productId, erpAccount);

        if (string.IsNullOrEmpty(availability))
        {
            return Json(new
            {
                Msg = "No availability"
            });
        }

        return Json(new
        {
            Data = availability
        });
    }

    public async Task<IActionResult> LoadErpNoItemsMsg()
    {
        var erpAccount = await _erpCustomerFunctionalityService.GetActiveErpAccountByCustomerAsync(
            await _workContext.GetCurrentCustomerAsync()
        );

        if (erpAccount == null)
        {
            return Json(new { hasMessage = false });
        }

        var accountSalesOrg = await _erpSalesOrgService.GetErpSalesOrgByIdAsync(
            erpAccount.ErpSalesOrgId
        );

        if (accountSalesOrg == null || string.IsNullOrEmpty(accountSalesOrg.NoItemsMessage))
        {
            return Json(new { hasMessage = false });
        }

        return Json(
            new { hasMessage = true, Message = accountSalesOrg.NoItemsMessage ?? string.Empty }
        );
    }

    public async Task<IActionResult> LoadProductInCartQuantity(int productId)
    {
        var model = await _erpProductModelFactory.PrepareProductInCartQuantityModelAsync(productId);
        return Json(new { Data = model });
    }

    #endregion

    #region Financial Transation / Download POD - Invoice - Account Statements - Test Cert

    public async Task<IActionResult> LoadErpAccountInfoFromErp()
    {
        var currCustomer = await _workContext.GetCurrentCustomerAsync();
        var erpAccount = await _erpAccountService.GetActiveErpAccountByCustomerIdAsync(
            currCustomer.Id
        );
        if (erpAccount == null)
            return new NullJsonResult();

        var model = await _erpAccountPublicModelFactory.PrepareErpAccountInfoModelAsync(
            erpAccount,
            new ErpAccountInfoModel(),
            enableErpAccountUpdate: true
        );
        return Json(
            new
            {
                CreditLimit = model.CreditLimit,
                CurrentBalance = model.CurrentBalance,
                AvailableCredit = model.AvailableCredit,
                LastPaymentAmount = model.LastPaymentAmount,
                LastPaymentDate = model.LastPaymentDate,
            }
        );
    }

    public async Task<IActionResult> ErpAccountInvoices()
    {
        //return RedirectToAction("ErpAccountInfo");
        var currCustomer = await _workContext.GetCurrentCustomerAsync();
        if (!await _customerService.IsRegisteredAsync(currCustomer))
            return Challenge();

        var erpAccount = await _erpAccountService.GetActiveErpAccountByCustomerIdAsync(
            currCustomer.Id
        );
        if (erpAccount == null)
            return RedirectToRoute("CustomerInfo");

        var model = await _erpAccountPublicModelFactory.PrepareErpAccountInfoModelAsync(
            erpAccount,
            new ErpAccountInfoModel()
        );
        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> LoadErpFinancialTransactionList(
        ErpAccountInfoModel searchModel
    )
    {
        var currCustomer = await _workContext.GetCurrentCustomerAsync();
        (var erpAccount, var erpNopUser) = await GetErpAccountAndUserOfCurrentCustomerAsync(
            currCustomer.Id
        );

        if (erpAccount == null ||
            await _erpCustomerFunctionalityService.IsCustomerInB2BQuoteAssistantRoleAsync(currCustomer) ||
            !await _permissionService.AuthorizeAsync(ErpPermissionProvider.DisplayB2BFinancialTransactions))
            return await AccessDeniedDataTablesJson();

        if (erpAccount.Id > 0)
            searchModel.ErpAccountId = erpAccount.Id;

        if (erpNopUser != null)
        {
            searchModel.ErpNopUserId = erpNopUser.Id;
        }

        var model = await _erpAccountPublicModelFactory.PrepareRecentTransactionListAsync(
            searchModel
        );
        return Json(model);
    }

    public async Task<IActionResult> DownloadInvoice(int invoiceId)
    {
        var erpIntegrationPlugin =
            await _erpIntegrationPluginManager.LoadActiveERPIntegrationPlugin();

        if (erpIntegrationPlugin is null)
        {
            await _erpLogsService.InsertErpLogAsync(
                ErpLogLevel.Error,
                ErpSyncLevel.Invoice,
                "No integration method found."
            );
            _notificationService.ErrorNotification("No integration method found.");
            return null;
        }

        if (invoiceId <= 0)
        {
            _notificationService.ErrorNotification(
                await _localizationService.GetResourceAsync(
                    "B2BB2CFeatures.DownloadInvoice.ErrorMessage.InvoiceData.IdIsNotValid"
                )
            );
            return RedirectToAction("ErpAccountInvoices");
        }

        var erpInvoice = await _erpInvoiceService.GetErpInvoiceByIdAsync(invoiceId);
        if (erpInvoice == null || string.IsNullOrEmpty(erpInvoice.ErpDocumentNumber))
        {
            _notificationService.ErrorNotification(
                await _localizationService.GetResourceAsync(
                    "B2BB2CFeatures.DownloadInvoice.ErrorMessage.InvoiceData.NotFound"
                )
            );
            return RedirectToAction("ErpAccountInvoices");
        }

        var erpAccount = await _erpAccountService.GetErpAccountByIdAsync(erpInvoice.ErpAccountId);
        if (erpAccount == null)
        {
            _notificationService.ErrorNotification(
                await _localizationService.GetResourceAsync(
                    "B2BB2CFeatures.DownloadInvoice.ErrorMessage.InvoiceData.B2BAccountNotFound"
                )
            );
            return RedirectToAction("ErpAccountInvoices");
        }
        var salesOrg = await _erpSalesOrgService.GetErpSalesOrgByIdAsync(erpAccount.ErpSalesOrgId);
        if (salesOrg == null)
        {
            _notificationService.ErrorNotification(
                await _localizationService.GetResourceAsync(
                    "B2BB2CFeatures.DownloadInvoice.ErrorMessage.InvoiceData.ErpSalesOrgNotFound"
                )
            );
            return RedirectToAction("ErpAccountInvoices");
        }

        try
        {
            var bytes = await erpIntegrationPlugin.GetDocumentAsync(
                erpInvoice.ErpDocumentNumber
            );

            if (bytes != null && bytes.Length > 0)
            {
                var fileName = "Invoice_" + erpInvoice.ErpDocumentNumber + ".pdf";
                var filePath = Path.Combine(Path.GetTempPath(), fileName);

                System.IO.File.WriteAllBytes(filePath, bytes);

                return PhysicalFile(filePath, "application/pdf", fileName);
            }

            _notificationService.ErrorNotification(
                await _localizationService.GetResourceAsync(
                    "B2BB2CFeatures.DownloadInvoice.ErrorMessage.InvoiceDataNotFound"
                )
            );

            return RedirectToAction("ErpAccountInvoices");
        }
        catch (Exception ex)
        {
            await _erpLogsService.InsertErpLogAsync(
                ErpLogLevel.Information,
                ErpSyncLevel.Account,
                ex.Message,
                ex.StackTrace
            );
            _notificationService.ErrorNotification(
                await _localizationService.GetResourceAsync(
                    "B2BB2CFeatures.DownloadInvoice.ErrorMessage.InvoiceDataNotFound"
                )
            );
            return RedirectToAction("ErpAccountInvoices");
        }
    }

    public async Task<IActionResult> DownloadInvoiceFromFtp(string id)
    {
        var store = await _storeContext.GetCurrentStoreAsync();
        var b2BB2CFeaturesSettings = await _settingService.LoadSettingAsync<B2BB2CFeaturesSettings>(
            store.Id
        );

        var baseUrl = b2BB2CFeaturesSettings.DownloadInvoicesPath + "/";
        //var baseUrl = "ftp://89.116.28.135/RenamedInvoiceTest/";

        try
        {
            var listRequest = (FtpWebRequest)WebRequest.Create(baseUrl);
            listRequest.UsePassive = true;
            listRequest.Method = WebRequestMethods.Ftp.ListDirectoryDetails;
            listRequest.Credentials = new NetworkCredential(
                b2BB2CFeaturesSettings.FtpUserName,
                b2BB2CFeaturesSettings.FtpPassword
            );

            var lines = new List<string>();
            using (var listResponse = listRequest.GetResponse())
            using (var listStream = listResponse.GetResponseStream())
            using (var listReader = new StreamReader(listStream))
            {
                while (!listReader.EndOfStream)
                {
                    lines.Add(listReader.ReadLine());
                }
            }

            var fileDictionary = new Dictionary<string, List<string>>();

            foreach (var line in lines)
            {
                var tokens = line.Split(
                    new[] { ' ' },
                    9,
                    StringSplitOptions.RemoveEmptyEntries
                );
                var name = tokens[8];

                var fileNameWithoutExt = Path.GetFileNameWithoutExtension(name);
                var invoicePath = Path.Combine(baseUrl, name);

                if (string.IsNullOrEmpty(invoicePath))
                    continue;

                var mimeType = GetMimeTypeFromFilePath(invoicePath);
                if (string.IsNullOrEmpty(mimeType))
                    continue;

                var nameTokens = fileNameWithoutExt.Split(new[] { '_' }, 2);
                var invoiceNo = nameTokens[0];

                if (string.Equals(invoiceNo, id, StringComparison.OrdinalIgnoreCase))
                {
                    if (fileDictionary.ContainsKey(invoiceNo))
                    {
                        fileDictionary[invoiceNo].Add(invoicePath);
                    }
                    else
                    {
                        fileDictionary[invoiceNo] = new List<string> { invoicePath };
                    }
                }
            }

            foreach (var filesGroup in fileDictionary.Values)
            {
                if (filesGroup.Count > 0)
                {
                    var zipFileName = $"{id}.zip";
                    var zipFilePath = Path.Combine(Path.GetTempPath(), zipFileName);

                    var counter = 1;
                    while (System.IO.File.Exists(zipFilePath))
                    {
                        zipFileName = $"{id}_{counter}.zip";
                        zipFilePath = Path.Combine(Path.GetTempPath(), zipFileName);
                        counter++;
                    }

                    using (var zipArchive = ZipFile.Open(zipFilePath, ZipArchiveMode.Create))
                    {
                        foreach (var ftpFilePath in filesGroup)
                        {
                            var fileName = Path.GetFileName(ftpFilePath);
                            var entry = zipArchive.CreateEntry(fileName);

                            using var entryStream = entry.Open();
                            var ftpRequest = (FtpWebRequest)WebRequest.Create(ftpFilePath);
                            ftpRequest.Method = WebRequestMethods.Ftp.DownloadFile;
                            ftpRequest.Credentials = new NetworkCredential(
                                b2BB2CFeaturesSettings.FtpUserName,
                                b2BB2CFeaturesSettings.FtpPassword
                            );

                            using var ftpResponse = (FtpWebResponse)ftpRequest.GetResponse();
                            using var ftpStream = ftpResponse.GetResponseStream();
                            ftpStream.CopyTo(entryStream);
                        }
                    }

                    return File(
                        System.IO.File.ReadAllBytes(zipFilePath),
                        "application/zip",
                        zipFileName
                    );
                }
            }
        }
        catch (Exception ex)
        {
            await _erpLogsService.InsertErpLogAsync(
                ErpLogLevel.Error,
                ErpSyncLevel.Account,
                ex.Message,
                ex.StackTrace
            );
            _notificationService.ErrorNotification(
                await _localizationService.GetResourceAsync(
                    "B2BB2CFeatures.DownloadInvoice.ErrorMessage.InvoiceDataNotFound"
                )
            );
            return RedirectToAction("ErpAccountInvoices");
        }

        _notificationService.ErrorNotification(
            await _localizationService.GetResourceAsync(
                "B2BB2CFeatures.DownloadInvoice.ErrorMessage.InvoiceDataNotFound"
            )
        );

        return RedirectToAction("ErpAccountInvoices");
    }

    protected virtual string GetMimeTypeFromFilePath(string filePath)
    {
        new FileExtensionContentTypeProvider().TryGetContentType(filePath, out var mimeType);
        return mimeType ?? null;
    }

    public async Task<IActionResult> GetPodDocumentList(string documentNo)
    {
        var erpAccount = await _erpCustomerFunctionalityService.GetActiveErpAccountByCustomerAsync(
            await _workContext.GetCurrentCustomerAsync()
        );
        var erpIntegrationPlugin =
            await _erpIntegrationPluginManager.LoadActiveERPIntegrationPlugin();

        if (erpAccount == null)
            return RedirectToAction("ErpAccountInvoices");
        try
        {
            var podDocumentNumbers =
                await erpIntegrationPlugin.GetTheProofOfDeliveryPDFDocumentListAsync(documentNo);
            if (podDocumentNumbers == null || !podDocumentNumbers.Any())
            {
                return Json(new { HavePodDocument = false });
            }
            return Json(new { HavePodDocument = true, podDocuments = podDocumentNumbers });
        }
        catch (Exception ex)
        {
            _notificationService.ErrorNotification(
                await _localizationService.GetResourceAsync(
                    "Plugin.Misc.NopStation.B2BB2CFeatures.PODListDownLoad.NotFound"
                )
            );
            await _logger.ErrorAsync(
                $"B2B Download POD, Error occured for Account Number: {erpAccount.AccountNumber}"
                    + ex.Message,
                ex
            );
            return RedirectToAction("ErpAccountInvoices");
        }
    }

    public async Task<IActionResult> DownloadPod(string documentNo, string pod)
    {
        var erpAccount = await _erpCustomerFunctionalityService.GetActiveErpAccountByCustomerAsync(
            await _workContext.GetCurrentCustomerAsync()
        );
        var erpIntegrationPlugin =
            await _erpIntegrationPluginManager.LoadActiveERPIntegrationPlugin();

        if (erpAccount == null)
        {
            _notificationService.Notification(
                NotifyType.Error,
                await _localizationService.GetResourceAsync(
                    "Plugin.Misc.NopStation.B2BB2CFeatures.PODDownLoad.AccessDenied"
                )
            );
            return RedirectToAction("ErpAccountInvoices");
        }

        try
        {
            var model = await erpIntegrationPlugin.DownloadTheProofOfDeliveryPDFAsync(documentNo, pod);
            if (model == null || string.IsNullOrWhiteSpace(model.ImageBase64))
            {
                _notificationService.ErrorNotification(
                    await _localizationService.GetResourceAsync(
                        "Plugin.Misc.NopStation.B2BB2CFeatures.PODDownLoad.NotFound"
                    )
                );
                await _logger.ErrorAsync(
                    $"B2B Download POD, Return null for Account Number: {erpAccount.AccountNumber}"
                );

                return RedirectToAction("ErpAccountInvoices");
            }
            var file = File(Convert.FromBase64String(model.ImageBase64), MimeTypes.ApplicationPdf, "POD_" + model.DocumentNumber + ".pdf");
            return Json(new { file = file, fileName = file.FileDownloadName });
        }
        catch (Exception ex)
        {
            _notificationService.ErrorNotification(
                await _localizationService.GetResourceAsync(
                    "Plugin.Misc.NopStation.B2BB2CFeatures.PODDownLoad.NotFound"
                )
            );
            await _logger.ErrorAsync(
                $"B2B Download POD, Error occured for Account Number: {erpAccount.AccountNumber}"
                    + ex.Message,
                ex
            );

            return RedirectToAction("B2BAccountInfo");
        }
    }

    public async Task<IActionResult> GetAccountStatementList()
    {
        var erpAccount = await _erpCustomerFunctionalityService.GetActiveErpAccountByCustomerAsync(
            await _workContext.GetCurrentCustomerAsync()
        );
        if (erpAccount == null)
            return RedirectToAction("ErpAccountInvoices");

        var hasB2BCustomerAccountingPersonnelRole =
            await _erpCustomerFunctionalityService.IsCurrentCustomerInB2BCustomerAccountingPersonnelRoleAsync();

        if (!hasB2BCustomerAccountingPersonnelRole)
        {
            return Json(new { HaveAccountStatements = false });
        }

        try
        {
            var erpIntegrationPlugin =
                await _erpIntegrationPluginManager.LoadActiveERPIntegrationPlugin();
            var accountStatements =
                await erpIntegrationPlugin.GetAccountStatementPDFDocumentListAsync(erpAccount);

            if (accountStatements == null || !accountStatements.Data.Any())
            {
                return Json(new { HaveAccountStatements = false });
            }

            return Json(
                new { HaveAccountStatements = true, AccountStatements = accountStatements.Data }
            );
        }
        catch (Exception ex)
        {
            _notificationService.ErrorNotification(
                await _localizationService.GetResourceAsync(
                    "Plugin.Misc.NopStation.B2BB2CFeatures.AccountStatementListDownLoad.NotFound"
                )
            );
            _logger.Error(
                $"B2B Download Account Statement List, Error occured for Account Number: {erpAccount.AccountNumber}"
                    + ex.Message,
                ex
            );
            return RedirectToAction("ErpAccountInvoices");
        }
    }

    public async Task<IActionResult> DownloadAccountStatement(
        string accountNo,
        string dateFrom,
        string dateTo
    )
    {
        var erpAccount = await _erpCustomerFunctionalityService.GetActiveErpAccountByCustomerAsync(
            await _workContext.GetCurrentCustomerAsync()
        );

        if (erpAccount == null || erpAccount.AccountNumber != accountNo)
            return RedirectToAction("ErpAccountInvoices");

        try
        {
            //var erpIntegrationPlugin = await _erpIntegrationPluginManager.LoadActiveERPIntegrationPlugin();
            //var bytes = await erpIntegrationPlugin.DownloadAccountStatementPDFAsync(erpAccount, dateFrom, dateTo);

            var bytes = new byte[1024];
            if (bytes == null)
            {
                _notificationService.ErrorNotification(
                    await _localizationService.GetResourceAsync(
                        "Plugin.Misc.NopStation.B2BB2CFeatures.AccountStatementDownLoad.NotFound"
                    )
                );
                _logger.Error(
                    $"B2B Download Account Statement, Return null for Account Number: {erpAccount.AccountNumber}"
                );
                return RedirectToAction("ErpAccountInvoices");
            }

            return File(
                bytes,
                MimeTypes.ApplicationPdf,
                "AccountNo_"
                    + erpAccount.AccountNumber
                    + "_AccountStatement_"
                    + dateFrom
                    + "_to_"
                    + dateTo
                    + ".pdf"
            );
        }
        catch (Exception ex)
        {
            _notificationService.ErrorNotification("No Account Statement found");
            await _logger.ErrorAsync(
                $"B2B Download Account Statement, Error occured for Account Number: {erpAccount.AccountNumber}"
                    + ex.Message,
                ex
            );
            return RedirectToAction("ErpAccountInvoices");
        }
    }

    public async Task<IActionResult> ErpOrderInvoiceList(int orderId)
    {
        var erpOrderNumber = string.Empty;
        var order = await _orderService.GetOrderByIdAsync(orderId);
        var currentCustomer = await _workContext.GetCurrentCustomerAsync();
        (var erpAccount, var erpNopUser) = await GetErpAccountAndUserOfCurrentCustomerAsync(
            currentCustomer.Id
        );

        if (erpAccount != null)
        {
            var erpAdditionalData =
                await _erpOrderAdditionalDataService.GetErpOrderAdditionalDataByNopOrderIdAsync(
                    orderId
                );

            var erpAccountForOrder = await _erpAccountService.GetErpAccountByIdAsync(
                erpAdditionalData?.ErpAccountId ?? 0
            );

            if (
                order == null
                || order.Deleted
                || erpAdditionalData == null
                || erpAccountForOrder.AccountNumber != erpAccount.AccountNumber
            )
                return Challenge();

            erpOrderNumber = erpAdditionalData.ErpOrderNumber;
        }

        if (string.IsNullOrEmpty(erpOrderNumber))
        {
            _notificationService.WarningNotification("No ERP Order Number found");
            return RedirectToRoute("OrderDetails", new { orderId = orderId });
        }

        var model = new RecentTransactionSearchModel { ErpOrderNumber = erpOrderNumber };

        if (erpAccount != null && erpNopUser.ErpUserType == ErpUserType.B2BUser)
            model.ErpAccountId = erpAccount.Id;

        if (erpAccount != null && erpNopUser.ErpUserType == ErpUserType.B2CUser)
        {
            model.ErpAccountId = erpAccount.Id;
            model.CustomerId = currentCustomer.Id;
        }
        //if (erpNopUser != null && (erpNopUser.ErpUserType == ErpUserType.B2BUser || erpNopUser.ErpUserType == ErpUserType.B2CUser))
        //    model.ErpAccountId = erpAccount?.Id ?? 0;

        model.SetGridPageSize();
        await _erpAccountPublicModelFactory.PrepareTransactionSortingOptionsAsync(
            model.AvailableSortOptions
        );

        return View(model);
    }

    public async Task<IActionResult> DownloadAccountStatementByMonth(
        string accountNo,
        int monthCount
    )
    {
        var erpAccount = await _erpCustomerFunctionalityService.GetActiveErpAccountByCustomerAsync(
            await _workContext.GetCurrentCustomerAsync()
        );

        if (erpAccount == null)
        {
            _notificationService.ErrorNotification(
                await _localizationService.GetResourceAsync(
                    "Plugin.Misc.NopStation.B2BB2CFeatures.AccountStatementDownLoad.AccessDenied"
                )
            );
            return AccessDeniedView();
        }

        if (
            !await _permissionService.AuthorizeAsync(
                ErpPermissionProvider.DisplayB2BAccountStatements
            )
        )
        {
            _notificationService.ErrorNotification(
                await _localizationService.GetResourceAsync(
                    "Plugin.Misc.NopStation.B2BB2CFeatures.AccountStatementDownLoad.AccessDenied"
                )
            );
            return AccessDeniedView();
        }

        if (erpAccount.AccountNumber != accountNo || monthCount < 1 || monthCount > 3)
        {
            _notificationService.ErrorNotification(
                await _localizationService.GetResourceAsync(
                    "Plugin.Misc.NopStation.B2BB2CFeatures.AccountStatementDownLoad.NotFound"
                )
            );
            return RedirectToAction("ErpAccountInvoices");
        }

        try
        {
            var currentMonthFirstDate = new DateTime(
                DateTime.UtcNow.Year,
                DateTime.UtcNow.Month,
                1
            );
            var dateFrom = currentMonthFirstDate.AddMonths(-monthCount);
            var dateTo = currentMonthFirstDate.AddMonths(-monthCount + 1).AddDays(-1);

            var erpIntegrationPlugin =
                await _erpIntegrationPluginManager.LoadActiveERPIntegrationPlugin();
            var bytes = await erpIntegrationPlugin.DownloadAccountStatementPDFAsync(
                erpAccount,
                dateFrom,
                dateTo
            );

            if (bytes == null)
            {
                _notificationService.ErrorNotification(
                    await _localizationService.GetResourceAsync(
                        "Plugin.Misc.NopStation.B2BB2CFeatures.AccountStatementDownLoad.NotFound"
                    )
                );
                await _logger.ErrorAsync(
                    $"B2B Download Account Statement, Return null for Account Number: {erpAccount.AccountNumber}"
                );

                return RedirectToAction("ErpAccountInvoices");
            }

            var file = File(
                bytes,
                MimeTypes.ApplicationPdf,
                "AccountNo_"
                    + erpAccount.AccountNumber
                    + "_AccountStatement_"
                    + dateFrom.ToString("yyyy-MM-dd")
                    + "_to_"
                    + dateTo.ToString("yyyy-MM-dd")
                    + ".pdf"
            );

            return Json(new { file = file, fileName = file.FileDownloadName });
        }
        catch (Exception ex)
        {
            _notificationService.ErrorNotification("No Account Statement found");
            await _logger.ErrorAsync(
                $"B2B Download Account Statement, Error occured for Account Number: {erpAccount.AccountNumber}"
                    + ex.Message,
                ex
            );

            return RedirectToAction("ErpAccountInvoices");
        }
    }

    public async Task<IActionResult> DownloadTestCert(string documentNo, string testCert)
    {
        var erpAccount = await _erpCustomerFunctionalityService.GetActiveErpAccountByCustomerAsync(
            await _workContext.GetCurrentCustomerAsync()
        );
        if (erpAccount == null)
        {
            _notificationService.Notification(
                NotifyType.Error,
                await _localizationService.GetResourceAsync(
                    "Plugin.Misc.NopStation.B2BB2CFeatures.DownloadTestCert.AccessDenied"
                )
            );
            return RedirectToAction("ErpAccountInvoices");
        }

        try
        {
            var erpIntegrationPlugin = await _erpIntegrationPluginManager.LoadActiveERPIntegrationPlugin();
            var model = await erpIntegrationPlugin.DownloadTheTestCertificatePDFAsync(documentNo, testCert);

            if (model == null || string.IsNullOrWhiteSpace(model.ImageBase64))
            {
                _notificationService.ErrorNotification("No Test Cert found");
                await _logger.ErrorAsync($"B2B Download Test Cert, Return null for Account Number: {erpAccount.AccountNumber}");

                return RedirectToAction("ErpAccountInvoices");
            }

            var file = File(Convert.FromBase64String(model.ImageBase64), MimeTypes.ApplicationPdf, "TestCert_" + model.DocumentNumber + ".pdf");

            return Json(new { file = file, fileName = file.FileDownloadName });
        }
        catch (Exception ex)
        {
            _notificationService.ErrorNotification("No Test Cert found");
            await _logger.ErrorAsync(
                $"B2B Download Test Cert, Error occured for Account Number: {erpAccount.AccountNumber}"
                    + ex.Message,
                ex
            );

            return RedirectToAction("ErpAccountInvoices");
        }
    }


    public async Task<IActionResult> GetTestCertDocumentList(string documentNo)
    {
        var erpAccount = await _erpCustomerFunctionalityService.GetActiveErpAccountByCustomerAsync(
            await _workContext.GetCurrentCustomerAsync()
        );
        if (erpAccount == null)
            return RedirectToAction("ErpAccountInvoices");
        try
        {
            var erpIntegrationPlugin =
                await _erpIntegrationPluginManager.LoadActiveERPIntegrationPlugin();

            var testCertDocumentNumbers = await erpIntegrationPlugin.GetTheTestCertificatePDFDocumentListAsync(documentNo);

            if (testCertDocumentNumbers == null || !testCertDocumentNumbers.Any())
            {
                return Json(new { HaveTestCertDocument = false });
            }

            return Json(
                new { HaveTestCertDocument = true, testCertDocuments = testCertDocumentNumbers }
            );
        }
        catch (Exception ex)
        {
            _notificationService.ErrorNotification("No Test Cert found");
            await _logger.ErrorAsync(
                $"B2B Download Test Cert, Error occured for Account Number: {erpAccount.AccountNumber}"
                    + ex.Message,
                ex
            );
            return RedirectToAction("ErpAccountInvoices");
        }
    }

    #endregion

    #region Orders / Quotes

    public async Task<IActionResult> ErpAccountOrders()
    {
        var currCustomer = await _workContext.GetCurrentCustomerAsync();
        if (!await _customerService.IsRegisteredAsync(currCustomer))
            return Challenge();

        (var erpAccount, var erpNopUser) = await GetErpAccountAndUserOfCurrentCustomerAsync(
            currCustomer.Id
        );

        if (erpAccount == null)
            return RedirectToRoute("CustomerInfo");

        if (!await _permissionService.AuthorizeAsync(ErpPermissionProvider.DisplayB2BOrders))
            return AccessDeniedView();

        var model = await _erpAccountPublicModelFactory.PrepareErpAccountOrderSearchModelAsync(
            erpAccount,
            erpNopUser,
            new ErpAccountOrderSearchModel()
        );
        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> LoadB2BAccountOrderList(ErpAccountOrderSearchModel searchModel)
    {
        var currCustomer = await _workContext.GetCurrentCustomerAsync();
        (var erpAccount, var erpNopUser) = await GetErpAccountAndUserOfCurrentCustomerAsync(
            currCustomer.Id
        );

        if (erpAccount == null)
            return await AccessDeniedDataTablesJson();

        if (!await _permissionService.AuthorizeAsync(ErpPermissionProvider.DisplayB2BOrders))
            return await AccessDeniedDataTablesJson();

        searchModel.ErpAccountId = erpAccount.Id;

        if (erpNopUser.ErpUserType == ErpUserType.B2CUser)
        {
            searchModel.ErpNopUserId = erpNopUser.Id;
            if (_b2BB2CFeaturesSettings.UseDefaultAccountForB2CUser)
            {
                searchModel.NopCustomerId = currCustomer.Id;
            }
        }

        var model = await _erpAccountPublicModelFactory.PrepareErpOrderListModelAsync(searchModel);
        return Json(model);
    }

    public async Task<IActionResult> ErpAccountQuoteOrders()
    {
        var customer = await _workContext.GetCurrentCustomerAsync();
        if (!await _customerService.IsRegisteredAsync(customer))
            return Challenge();

        (var erpAccount, var erpNopUser) = await GetErpAccountAndUserOfCurrentCustomerAsync(
            customer.Id
        );
        if (erpAccount == null)
            return RedirectToRoute("CustomerInfo");

        if (!await _permissionService.AuthorizeAsync(ErpPermissionProvider.DisplayB2BQuotes))
            return AccessDeniedView();

        var model = await _erpAccountPublicModelFactory.PrepareErpAccountQuoteOrderSearchModelAsync(
            erpAccount,
            erpNopUser,
            new ErpAccountQuoteOrderSearchModel()
        );

        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> LoadErpQuoteOrderList(
        ErpAccountQuoteOrderSearchModel searchModel
    )
    {
        var currCustomer = await _workContext.GetCurrentCustomerAsync();
        (var erpAccount, var erpNopUser) = await GetErpAccountAndUserOfCurrentCustomerAsync(currCustomer.Id);

        if (erpAccount == null)
            return await AccessDeniedDataTablesJson();

        if (!await _permissionService.AuthorizeAsync(ErpPermissionProvider.DisplayB2BQuotes))
            return await AccessDeniedDataTablesJson();

        searchModel.ErpAccountId = erpAccount.Id;

        if (erpNopUser.ErpUserType == ErpUserType.B2CUser)
        {
            searchModel.ErpNopUserId = erpNopUser.Id;
            if (_b2BB2CFeaturesSettings.UseDefaultAccountForB2CUser)
            {
                searchModel.NopCustomerId = currCustomer.Id;
            }
        }

        var model = await _erpAccountPublicModelFactory.PrepareErpQuoteOrderListModelAsync(searchModel);
        return Json(model);
    }
    [HttpPost]
    public virtual async Task<IActionResult> LoadErpInvoiceList(RecentTransactionSearchModel searchModel)
    {
        var currentCustomer = await _workContext.GetCurrentCustomerAsync();
        (var erpAccount, var erpNopUser) = await GetErpAccountAndUserOfCurrentCustomerAsync(
            currentCustomer.Id
        );
        var customerId = (erpNopUser != null && erpNopUser.ErpUserType == ErpUserType.B2CUser) ? currentCustomer.Id : 0;
        //if (b2BAccount == null || HasB2BQuoteAssistantRole())

        if (erpAccount == null || await _erpCustomerFunctionalityService.IsCurrentCustomerInB2BQuoteAssistantRoleAsync())
            return await AccessDeniedDataTablesJson();

        if (erpNopUser != null && erpNopUser.ErpUserType == ErpUserType.B2BUser && searchModel.ErpAccountId != erpAccount.Id)
            return Challenge();

        if (erpNopUser != null && erpNopUser.ErpUserType == ErpUserType.B2CUser && erpNopUser.NopCustomerId != customerId)
            return Challenge();

        //prepare model
        var model = await _erpAccountPublicModelFactory.PrepareRecentTransactionListAsync(searchModel);

        return Json(model);
    }

    #endregion

    #region Price List Download / Live Price Sync

    [Route("/B2BAccountPublic/B2BPriceList")]
    public async Task<IActionResult> ErpPriceList()
    {
        var erpAccount = await _erpCustomerFunctionalityService.GetActiveErpAccountByCustomerAsync(
            await _workContext.GetCurrentCustomerAsync()
        );
        if (erpAccount == null)
        {
            _notificationService.Notification(
                NotifyType.Error,
                await _localizationService.GetResourceAsync(
                    "Plugin.Misc.NopStation.B2BB2CFeatures.PriceListDownLoad.LoginRequired"
                )
            );
            return Challenge();
        }
        return View(
            "~/Plugins/NopStation.Plugin.B2B.B2BB2CFeatures/Views/ErpAccountPublic/PriceList.cshtml"
        );
    }

    public async Task<IActionResult> AllProductsLivePriceSync()
    {
        await _erpPriceSyncFunctionalityService.ExecuteAllProductsLivePriceSync();
        return Json(new { success = true });
    }

    #endregion

    #region T & C

    [HttpPost, ActionName("TermsAndConditionResponse")]
    [FormValueRequired("accept")]
    public async Task<IActionResult> TermsAndConditionResponseAccept()
    {
        await _genericAttributeService.SaveAttributeAsync(
            await _workContext.GetCurrentCustomerAsync(),
            ERPIntegrationCoreDefaults.CustomerDateOfTermsAndConditionCheckedAttributeName,
            DateTime.UtcNow
        );
        return RedirectToRoute("Homepage");
    }

    [HttpPost, ActionName("TermsAndConditionResponse")]
    [FormValueRequired("cancel")]
    public Task<IActionResult> TermsAndConditionResponseCancel()
    {
        return Task.FromResult<IActionResult>(RedirectToRoute("Homepage"));
    }

    #endregion

    #region Online Savings

    public async Task<IActionResult> GetCustomerAccountSavingsForthisYear()
    {
        if (!_b2BB2CFeaturesSettings.IsShowYearlySavings)
            return Json(new { success = false });

        (var erpAccount, var erpNopUser) = await GetErpAccountAndUserOfCurrentCustomerAsync((await _workContext.GetCurrentCustomerAsync()).Id);

        if (erpAccount == null || erpNopUser == null)
            return Json(new { success = false });

        if (erpNopUser.ErpUserType == ErpUserType.B2BUser)
        {
            if (erpAccount.TotalSavingsForthisYearUpdatedOnUtc.HasValue &&
            erpAccount.TotalSavingsForthisYearUpdatedOnUtc.Value.AddMinutes(_b2BB2CFeaturesSettings.OnlineSavingsCacheTime) > DateTime.UtcNow)
            {
                if (erpAccount.TotalSavingsForthisYear.HasValue)
                {
                    return Json(new
                    {
                        success = true,
                        totalSavings = await _priceFormatter.FormatPriceAsync(erpAccount.TotalSavingsForthisYear.Value)
                    });
                }
            }

            var totalSavings = await _erpAccountPublicModelFactory.GetB2BCurrentCustomerAccountSavingsForthisYearAsync(erpAccount);
            if (!totalSavings.HasValue)
            {
                return Json(new { success = false });
            }
            else
            {
                return Json(new
                {
                    success = true,
                    totalSavings = await _priceFormatter.FormatPriceAsync(totalSavings.Value)
                });
            }
        }
        else if (erpNopUser.ErpUserType == ErpUserType.B2CUser)
        {
            if (erpNopUser.TotalSavingsForthisYearUpdatedOnUtc.HasValue &&
           erpNopUser.TotalSavingsForthisYearUpdatedOnUtc.Value.AddMinutes(_b2BB2CFeaturesSettings.OnlineSavingsCacheTime) > DateTime.UtcNow)
            {
                if (erpNopUser.TotalSavingsForthisYear.HasValue)
                {
                    return Json(new
                    {
                        success = true,
                        totalSavings = _priceFormatter.FormatPriceAsync(erpNopUser.TotalSavingsForthisYear.Value)
                    });
                }
            }

            var totalSavings = await _erpAccountPublicModelFactory.GetB2CCurrentCustomerAccountSavingsForthisYearAsync(erpNopUser);
            if (!totalSavings.HasValue)
            {
                return Json(new { success = false });
            }
            else
            {
                return Json(new
                {
                    success = true,
                    totalSavings = _priceFormatter.FormatPriceAsync(totalSavings.Value)
                });
            }
        }
        else
        {
            return Json(new { success = false });
        }
    }

    public async Task<IActionResult> GetCustomerAccountSavingsForAllTime()
    {
        if (!_b2BB2CFeaturesSettings.IsShowAllTimeSavings)
            return Json(new { success = false });

        (var erpAccount, var erpNopUser) = await GetErpAccountAndUserOfCurrentCustomerAsync((await _workContext.GetCurrentCustomerAsync()).Id);

        if (erpAccount == null || erpNopUser == null)
            return Json(new { success = false });

        if (erpNopUser.ErpUserType == ErpUserType.B2BUser)
        {
            if (erpAccount.TotalSavingsForAllTimeUpdatedOnUtc.HasValue &&
            erpAccount.TotalSavingsForAllTimeUpdatedOnUtc.Value.AddMinutes(_b2BB2CFeaturesSettings.OnlineSavingsCacheTime) > DateTime.UtcNow)
            {
                if (erpAccount.TotalSavingsForAllTime.HasValue)
                {
                    return Json(new
                    {
                        success = true,
                        totalSavings = await _priceFormatter.FormatPriceAsync(erpAccount.TotalSavingsForAllTime.Value)
                    });
                }
            }

            var totalSavings = await _erpAccountPublicModelFactory.GetB2BCurrentCustomerAccountSavingsForAllTimeAsync(erpAccount);
            if (!totalSavings.HasValue)
            {
                return Json(new { success = false });
            }
            else
            {
                return Json(new
                {
                    success = true,
                    totalSavings = await _priceFormatter.FormatPriceAsync(totalSavings.Value)
                });
            }
        }
        else if (erpNopUser.ErpUserType == ErpUserType.B2CUser)
        {
            if (erpNopUser.TotalSavingsForAllTimeUpdatedOnUtc.HasValue &&
            erpNopUser.TotalSavingsForAllTimeUpdatedOnUtc.Value.AddMinutes(_b2BB2CFeaturesSettings.OnlineSavingsCacheTime) > DateTime.UtcNow)
            {
                if (erpNopUser.TotalSavingsForAllTime.HasValue)
                {
                    return Json(new
                    {
                        success = true,
                        totalSavings = await _priceFormatter.FormatPriceAsync(erpNopUser.TotalSavingsForAllTime.Value)
                    });
                }
            }

            var totalSavings = await _erpAccountPublicModelFactory.GetB2CCurrentCustomerAccountSavingsForAllTimeAsync(erpNopUser);
            if (!totalSavings.HasValue)
            {
                return Json(new { success = false });
            }
            else
            {
                return Json(new
                {
                    success = true,
                    totalSavings = await _priceFormatter.FormatPriceAsync(totalSavings.Value)
                });
            }
        }
        else
        {
            return Json(new { success = false });
        }
    }

    #endregion

    public async Task<IActionResult> LoadB2BAccountInfoWithCurrentOrderFromErp()
    {
        var currentCustomer = _workContext.GetCurrentCustomerAsync().GetAwaiter().GetResult();
        var erpAccount = await _erpCustomerFunctionalityService.GetActiveErpAccountByCustomerAsync(
            currentCustomer
        );
        if (erpAccount == null)
            return new NullJsonResult();

        var model = await _erpAccountPublicModelFactory.PrepareB2BAccountInfoAjaxLoadModelAsync(erpAccount, currentCustomer, enableErpAccountUpdate: true);
        return Json(new
        {
            data = model
        });
    }

    // test endpoint, must remove later
    public async Task<IActionResult> TestOrderPdfDataByOrderId(int orderId)
    {
        var order = await _orderService.GetOrderByIdAsync(orderId);
        if (order == null)
            return Challenge();
        var model = await _erpPdfModelFactory.PrepareErpOrderPdfModelAsync(order);
        return Ok(model);
    }

    #endregion
}