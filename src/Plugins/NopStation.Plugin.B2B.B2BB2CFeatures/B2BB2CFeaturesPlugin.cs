using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Nop.Core;
using Nop.Core.Domain.Localization;
using Nop.Core.Domain.Messages;
using Nop.Core.Infrastructure;
using Nop.Services.Cms;
using Nop.Services.Common;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Messages;
using Nop.Services.Plugins;
using Nop.Services.ScheduleTasks;
using Nop.Services.Security;
using Nop.Web.Framework.Infrastructure;
using Nop.Web.Framework.Menu;
using NopStation.Plugin.B2B.B2BB2CFeatures.Areas.Admin.Components;
using NopStation.Plugin.B2B.B2BB2CFeatures.Components;
using NopStation.Plugin.B2B.B2BB2CFeatures.Infrastructure;
using NopStation.Plugin.B2B.ERPIntegrationCore.Enums;
using NopStation.Plugin.Misc.Core.Helpers;
using NopStation.Plugin.Misc.Core.Services;

namespace NopStation.Plugin.B2B.B2BB2CFeatures;

public class B2BB2CFeaturesPlugin : BasePlugin, IMiscPlugin, IAdminMenuPlugin, INopStationPlugin, IWidgetPlugin
{
    #region Fields
     
    private readonly ILogger _logger;
    private readonly IWebHelper _webHelper;
    private readonly ISettingService _settingService;
    private readonly ILanguageService _languageService;
    private readonly IPermissionService _permissionService;
    private readonly IScheduleTaskService _scheduleTaskService;
    private readonly ILocalizationService _localizationService;
    private readonly IEmailAccountService _emailAccountService;
    private readonly IMessageTemplateService _messageTemplateService;

    public bool HideInWidgetList => false;
    public int Order => throw new NotImplementedException();

    #endregion

    #region Ctor

    public B2BB2CFeaturesPlugin(ILogger logger,
        IWebHelper webHelper,
        ILanguageService languageService,
        IPermissionService permissionService,
        IScheduleTaskService scheduleTaskService,
        IEmailAccountService emailAccountService,
        ILocalizationService localizationService,
        IMessageTemplateService messageTemplateService,
        ISettingService settingService)
    {
        _logger = logger;
        _webHelper = webHelper;
        _languageService = languageService;
        _permissionService = permissionService;
        _scheduleTaskService = scheduleTaskService;
        _emailAccountService = emailAccountService;
        _localizationService = localizationService;
        _messageTemplateService = messageTemplateService;
        _settingService = settingService;
    }

    #endregion

    #region Utilities

    private Language GetDefaultEnglishLanguage()
    {
        return _languageService
            .GetAllLanguages()
            .FirstOrDefault(x => x.UniqueSeoCode.Equals("en", StringComparison.InvariantCultureIgnoreCase));
    }

    public async Task InstalLocalResourseStringFromXmlFileAsync()
    {
        var language = GetDefaultEnglishLanguage();

        if (language == null)
        {
            _logger.Error("Can't Add Resource string. Couldn't Find The Requered Language!");
            return;
        }

        try
        {
            var fileProvider = EngineContext.Current.Resolve<INopFileProvider>();
            var path = fileProvider.MapPath(B2BB2CFeaturesDefaults.XmlResourceStringFilePath);
            using var sr = new StreamReader(path, Encoding.UTF8);
            await _localizationService.ImportResourcesFromXmlAsync(language, sr);
        }
        catch (Exception ex)
        {
            _logger.Error("B2B Features Plugin: Can't Add Resource string!", ex);
        }
    }

    public async Task UninstalLocalResourseStringFromXmlFileAsync()
    {
        try
        {
            var fileProvider = EngineContext.Current.Resolve<INopFileProvider>();
            var path = fileProvider.MapPath(B2BB2CFeaturesDefaults.XmlResourceStringFilePath);

            var doc = new XmlDocument();
            doc.Load(path);

            foreach (XmlNode node in doc.DocumentElement.ChildNodes)
            {
                if (node.Attributes != null)
                {
                    var resource = node.Attributes["Name"]?.InnerText;
                    if (!string.IsNullOrEmpty(resource))
                    {
                        await _localizationService.DeleteLocaleResourceAsync(resource);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            _logger.Error("B2B Features Plugin: Can't Remove Resource string!", ex);
        }
    }

    private async Task InsertScheduleTasksAsync()
    {
        if (await _scheduleTaskService.GetTaskByTypeAsync(B2BB2CFeaturesDefaults.ProcessFailedErpOrdersTask) is null)
        {
            await _scheduleTaskService.InsertTaskAsync(
                new()
                {
                    Enabled = false,
                    StopOnError = false,
                    LastEnabledUtc = DateTime.UtcNow,
                    Name = B2BB2CFeaturesDefaults.ProcessFailedErpOrdersTaskName,
                    Type = B2BB2CFeaturesDefaults.ProcessFailedErpOrdersTask,
                    Seconds = B2BB2CFeaturesDefaults.DefaultTaskTimeOutPeriod,
                }
            );
        }

        if (await _scheduleTaskService.GetTaskByTypeAsync(B2BB2CFeaturesDefaults.ProcessBackInStockSubscriptionsTask) is null)
        {
            await _scheduleTaskService.InsertTaskAsync(
                new()
                {
                    Enabled = true,
                    StopOnError = false,
                    LastEnabledUtc = DateTime.UtcNow,
                    Name = B2BB2CFeaturesDefaults.ProcessBackInStockSubscriptionsTaskName,
                    Type = B2BB2CFeaturesDefaults.ProcessBackInStockSubscriptionsTask,
                    Seconds = B2BB2CFeaturesDefaults.DefaultSubscriptionsTaskTimeOutPeriod,
                }
            );
        }
    }

    private async Task InsertMessageTemplatesAsync()
    {
        var emailAccount = (await _emailAccountService.GetAllEmailAccountsAsync()).FirstOrDefault();
        if (emailAccount is not null)
        {
            var messageTemplates = await _messageTemplateService.GetMessageTemplatesByNameAsync(
                    B2BB2CFeaturesDefaults.MessageTemplateSystemNames_ERPOrderPlaceFailedSalesRepNotification);

            if (messageTemplates.Count == 0)
            {
                await _messageTemplateService.InsertMessageTemplateAsync(
                    new MessageTemplate
                    {
                        Name = B2BB2CFeaturesDefaults.MessageTemplateSystemNames_ERPOrderPlaceFailedSalesRepNotification,
                        Subject = "%Store.Name%. Order place at ERP failed",
                        Body =
                            $"<p>{Environment.NewLine}<a href=\"%Store.URL%\">%Store.Name%</a>{Environment.NewLine}<br />{Environment.NewLine}<br />{Environment.NewLine}Hello %Order.CustomerFullName%,{Environment.NewLine}<br />{Environment.NewLine}Your order is not placed at ERP. Plese contact with Account Rep. Below is the summary of the order.{Environment.NewLine}<br />{Environment.NewLine}<br />{Environment.NewLine}Order Number: %Order.OrderNumber%{Environment.NewLine}<br />{Environment.NewLine}Order Details: <a target=\"_blank\" href=\"%Order.OrderURLForCustomer%\">%Order.OrderURLForCustomer%</a>{Environment.NewLine}<br />{Environment.NewLine}Date Ordered: %Order.CreatedOn%{Environment.NewLine}<br />{Environment.NewLine}<br />{Environment.NewLine}<br />{Environment.NewLine}<br />{Environment.NewLine}Billing Address{Environment.NewLine}<br />{Environment.NewLine}%Order.BillingFirstName% %Order.BillingLastName%{Environment.NewLine}<br />{Environment.NewLine}%Order.BillingAddress1%{Environment.NewLine}<br />{Environment.NewLine}%Order.BillingCity% %Order.BillingZipPostalCode%{Environment.NewLine}<br />{Environment.NewLine}%Order.BillingStateProvince% %Order.BillingCountry%{Environment.NewLine}<br />{Environment.NewLine}<br />{Environment.NewLine}<br />{Environment.NewLine}<br />{Environment.NewLine}%if (%Order.Shippable%) Shipping Address{Environment.NewLine}<br />{Environment.NewLine}%Order.ShippingFirstName% %Order.ShippingLastName%{Environment.NewLine}<br />{Environment.NewLine}%Order.ShippingAddress1%{Environment.NewLine}<br />{Environment.NewLine}%Order.ShippingCity% %Order.ShippingZipPostalCode%{Environment.NewLine}<br />{Environment.NewLine}%Order.ShippingStateProvince% %Order.ShippingCountry%{Environment.NewLine}<br />{Environment.NewLine}<br />{Environment.NewLine}Shipping Method: %Order.ShippingMethod%{Environment.NewLine}<br />{Environment.NewLine}<br />{Environment.NewLine} endif% %Order.Product(s)%{Environment.NewLine}</p>{Environment.NewLine}",
                        IsActive = true,
                        EmailAccountId = emailAccount.Id,
                    }
                );
            }

            messageTemplates = await _messageTemplateService.GetMessageTemplatesByNameAsync(
                    B2BB2CFeaturesDefaults.MessageTemplateSystemNames_ERPAccountCustomerRegistrationCreatedNotificationToAdmin);

            if (messageTemplates.Count == 0)
            {
                await _messageTemplateService.InsertMessageTemplateAsync(
                    new MessageTemplate
                    {
                        Name =
                            B2BB2CFeaturesDefaults.MessageTemplateSystemNames_ERPAccountCustomerRegistrationCreatedNotificationToAdmin,
                        Subject = "%Store.Name%. ERP Customer Registration Application Created",
                        Body =
                            $"<p>{Environment.NewLine}<a href=\"%Store.URL%\">%Store.Name%</a>{Environment.NewLine}<br />{Environment.NewLine}<br />{Environment.NewLine}Hello %Application.AdminName%,{Environment.NewLine}<br />{Environment.NewLine}An application is created to register a new customer in ERP.{Environment.NewLine}<br />{Environment.NewLine}<br />{Environment.NewLine}Please review the application. Application Id: %Application.Id%. And Registration Number: %Application.RegistrationNumber%{Environment.NewLine}<br />{Environment.NewLine}Thanks</p>{Environment.NewLine}",
                        IsActive = true,
                        EmailAccountId = emailAccount.Id,
                    }
                );
            }

            messageTemplates = await _messageTemplateService.GetMessageTemplatesByNameAsync(
                    B2BB2CFeaturesDefaults.MessageTemplateSystemNames_ERPAccountCustomerRegistrationCreatedNotificationToCustomer);

            if (messageTemplates.Count == 0)
            {
                await _messageTemplateService.InsertMessageTemplateAsync(
                    new MessageTemplate
                    {
                        Name =
                        B2BB2CFeaturesDefaults.MessageTemplateSystemNames_ERPAccountCustomerRegistrationCreatedNotificationToCustomer,
                        Subject = "%Store.Name%. ERP Customer Registration Application Created",
                        Body =
                            $"<p>{Environment.NewLine}<a href=\"%Store.URL%\">%Store.Name%</a>{Environment.NewLine}<br />{Environment.NewLine}<br />{Environment.NewLine}Hello %Application.CustomerFullName%,{Environment.NewLine}<br />{Environment.NewLine}An application is created to register a new customer in ERP.{Environment.NewLine}<br />{Environment.NewLine}<br />{Environment.NewLine}Admin will review your application. You will get another mail if it gets approved.{Environment.NewLine}<br />{Environment.NewLine}Thank you</p>{Environment.NewLine}",
                        IsActive = true,
                        EmailAccountId = emailAccount.Id,
                    }
                );
            }

            messageTemplates = await _messageTemplateService.GetMessageTemplatesByNameAsync(
                    B2BB2CFeaturesDefaults.MessageTemplateSystemNames_ERPAccountCustomerRegistrationApprovedNotification);

            if (messageTemplates.Count == 0)
            {
                await _messageTemplateService.InsertMessageTemplateAsync(
                    new MessageTemplate
                    {
                        Name =
                        B2BB2CFeaturesDefaults.MessageTemplateSystemNames_ERPAccountCustomerRegistrationApprovedNotification,
                        Subject = "%Store.Name%. ERP Customer Registration Application Approved",
                        Body =
                            $"<p>{Environment.NewLine}<a href=\"%Store.URL%\">%Store.Name%</a>{Environment.NewLine}<br />{Environment.NewLine}<br />{Environment.NewLine}Hello %Application.CustomerFullName%,{Environment.NewLine}<br />{Environment.NewLine}Your application is Approved to register a new customer in ERP.{Environment.NewLine}<br />{Environment.NewLine}<br />{Environment.NewLine}Admin will create an ERP account for your user according to your given information.{Environment.NewLine}<br />{Environment.NewLine}Thank you</p>{Environment.NewLine}",
                        IsActive = true,
                        EmailAccountId = emailAccount.Id,
                    }
                );
            }

            messageTemplates = await _messageTemplateService.GetMessageTemplatesByNameAsync(
                    B2BB2CFeaturesDefaults.MessageTemplateSystemNames_B2CCustomerWelcomeMessage);

            if (messageTemplates.Count == 0)
            {
                await _messageTemplateService.InsertMessageTemplateAsync(
                    new MessageTemplate
                    {
                        Name =
                            B2BB2CFeaturesDefaults.MessageTemplateSystemNames_B2CCustomerWelcomeMessage,
                        Subject = "B2C Customer: Welcome to %Store.Name%.",
                        Body =
                            $"<p>{Environment.NewLine}<a href=\"%Store.URL%\">%Store.Name%</a></p>{Environment.NewLine}",
                        IsActive = true,
                        EmailAccountId = emailAccount.Id,
                    }
                );
            }

            messageTemplates = await _messageTemplateService.GetMessageTemplatesByNameAsync(
                B2BB2CFeaturesDefaults.MessageTemplateSystemNames_B2CCustomerEmailVerificationMessage);

            if (messageTemplates.Count == 0)
            {
                await _messageTemplateService.InsertMessageTemplateAsync(
                    new MessageTemplate
                    {
                        Name =
                            B2BB2CFeaturesDefaults.MessageTemplateSystemNames_B2CCustomerEmailVerificationMessage,
                        Subject = "%Store.Name%. B2C Customer: Email address verification",
                        Body =
                            $"<p>{Environment.NewLine}<a href=\"%Store.URL%\">%Store.Name%</a></p>{Environment.NewLine}",
                        IsActive = true,
                        EmailAccountId = emailAccount.Id,
                    }
                );
            }

            messageTemplates = await _messageTemplateService.GetMessageTemplatesByNameAsync(
                B2BB2CFeaturesDefaults.MessageTemplateSystemNames_B2BCustomerRegisteredNotification);

            if (messageTemplates.Count == 0)
            {
                await _messageTemplateService.InsertMessageTemplateAsync(
                    new MessageTemplate
                    {
                        Name =
                            B2BB2CFeaturesDefaults.MessageTemplateSystemNames_B2BCustomerRegisteredNotification,
                        Subject = "%Store.Name%. New B2B customer registration",
                        Body =
                            $"<p>{Environment.NewLine}<a href=\"%Store.URL%\">%Store.Name%</a>{Environment.NewLine}<br />{Environment.NewLine}<br />{Environment.NewLine}A new b2b customer registered with your store. Below are the customer's details:{Environment.NewLine}<br />{Environment.NewLine}Full name: %Customer.FullName%{Environment.NewLine}<br />{Environment.NewLine}Email: %Customer.Email%{Environment.NewLine}<br />{Environment.NewLine}Account Number: %B2BUserRegistrationInfo.AccountNumber%{Environment.NewLine}<br />{Environment.NewLine}Sales Organization Name: %B2BUserRegistrationInfo.SalesOrganizationName%{Environment.NewLine}<br />{Environment.NewLine}Special Instructions: %B2BUserRegistrationInfo.SpecialInstructions%{Environment.NewLine}</p>{Environment.NewLine}",
                        IsActive = true,
                        EmailAccountId = emailAccount.Id,
                    }
                );
            }

            messageTemplates = await _messageTemplateService.GetMessageTemplatesByNameAsync(
                B2BB2CFeaturesDefaults.MessageTemplateSystemNames_B2CCustomerRegisteredNotification);

            if (messageTemplates.Count == 0)
            {
                await _messageTemplateService.InsertMessageTemplateAsync(
                    new MessageTemplate
                    {
                        Name =
                            B2BB2CFeaturesDefaults.MessageTemplateSystemNames_B2CCustomerRegisteredNotification,
                        Subject = "%Store.Name%. New B2C customer registration",
                        Body =
                            $"<p>{Environment.NewLine}<a href=\"%Store.URL%\">%Store.Name%</a>{Environment.NewLine}<br />{Environment.NewLine}<br />{Environment.NewLine}A new b2c customer registered with your store. Below are the customer's details:{Environment.NewLine}<br />{Environment.NewLine}Full name: %Customer.FullName%{Environment.NewLine}<br />{Environment.NewLine}Email: %Customer.Email%{Environment.NewLine}<br />{Environment.NewLine}Account Number: %B2BUserRegistrationInfo.AccountNumber%{Environment.NewLine}<br />{Environment.NewLine}Sales Organization Name: %B2BUserRegistrationInfo.SalesOrganizationName%{Environment.NewLine}<br />{Environment.NewLine}Special Instructions: %B2BUserRegistrationInfo.SpecialInstructions%{Environment.NewLine}</p>{Environment.NewLine}",
                        IsActive = true,
                        EmailAccountId = emailAccount.Id,
                    }
                );
            }
        }
    }

    private async Task InsertB2BFeaturesSettingsAsync()
    {
        var settings = new B2BB2CFeaturesSettings
        {
            DisableLiveStockCheckProductGreaterThanAmount = 500,
            PaymentPopupMessageDelayTimeInSec = 20,
            EnableLivePriceChecks = false,
            IsShowYearlySavings = false,
            DisplayQuantityColumnInExcel = true,
            DisplayUOMColumnInExcelAndPdf = true,
            DisplayPricingNoteColumnInExcelAndPdf = true,
            DisplayWeightColumnInExcelAndPdf = true,
            DisplayAllCategoriesPriceListInCategoryPage = true,
            DisplayCategoryPriceListInCategoryPage = true,
            IsCategoryPriceListAppliedToSubCategories = true,
            LastNOrdersPerAccount = 10,
            StockDisplayFormat = StockDisplayFormat.DoNotShowAnyStockAtAll,
        };
        await _settingService.SaveSettingAsync(settings);
    }

    #endregion

    #region Method

    public Type GetWidgetViewComponent(string widgetZone)
    {
        if (widgetZone.Equals(PublicWidgetZones.HeaderLinksBefore))
            return typeof(PublicHeaderViewComponent);
        else if (widgetZone.Equals(PublicWidgetZones.HeadHtmlTag))
            return typeof(B2BRootHeadViewComponent);
        else if (widgetZone.Equals(PublicWidgetZones.OrderSummaryContentDeals))
            return typeof(OrderSummaryContentDealsViewComponent);
        else if (widgetZone.Equals(B2BB2CFeaturesDefaults.ZoneAfterTirePriceCard))
            return typeof(ErpSpecialPriceViewComponent);
        else if (widgetZone.Equals(B2BB2CFeaturesDefaults.ZoneAfterSpecialPriceCard))
            return typeof(ErpPriceGroupViewComponent);
        else if (widgetZone.Equals(AdminWidgetZones.OrderDetailsBlock))
            return typeof(ErpOrderItemInOrderDetailsAdminViewComponent);
        else if (widgetZone.Equals(B2BB2CFeaturesDefaults.ErpAdminWidgetZonesOrderDetailsBlock))
            return typeof(ErpOrderInOrderDetailsAdminViewComponent);
        else if (widgetZone.Equals(AdminWidgetZones.CustomerDetailsBlock))
            return typeof(NopCustomerErpAccountInfoComponent);
        else if (widgetZone.Equals(PublicWidgetZones.CategoryDetailsAfterBreadcrumb))
            return typeof(ErpPriceListInCategoryPageViewComponent);
        else if (widgetZone.Equals(B2BB2CFeaturesDefaults.ProductPriceB2BPricingNote))
            return typeof(B2BProductPricingNoteViewComponent);
        else if (widgetZone.Equals(PublicWidgetZones.ProductBoxAddinfoMiddle))
            return typeof(B2BProductBoxStockInfoViewComponent);
        else if (widgetZone.Equals(B2BB2CFeaturesDefaults.CategoryVisibilityBlock))
            return typeof(ErpCategoryImageMappingAdminViewComponent);
        else if (widgetZone.Equals(PublicWidgetZones.HeaderLinksAfter))
            return typeof(B2BUserListHeaderLinkViewComponent);
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

        return Task.FromResult<IList<string>>(
            new List<string>
            {
                PublicWidgetZones.HeaderLinksBefore,
                PublicWidgetZones.HeadHtmlTag,
                PublicWidgetZones.OrderSummaryContentDeals,
                B2BB2CFeaturesDefaults.ZoneAfterTirePriceCard,
                B2BB2CFeaturesDefaults.ZoneAfterSpecialPriceCard,
                B2BB2CFeaturesDefaults.ErpAdminWidgetZonesOrderDetailsBlock,
                AdminWidgetZones.OrderDetailsBlock,
                AdminWidgetZones.CustomerDetailsBlock,
                PublicWidgetZones.CategoryDetailsAfterBreadcrumb,
                B2BB2CFeaturesDefaults.ProductPriceB2BPricingNote,
                PublicWidgetZones.ProductBoxAddinfoMiddle,
                B2BB2CFeaturesDefaults.CategoryVisibilityBlock,
                PublicWidgetZones.HeaderLinksAfter
            }
        );
    }

    public override async Task InstallAsync()
    {
        await InstalLocalResourseStringFromXmlFileAsync();

        await _permissionService.InstallPermissionsAsync(new B2BB2CPermissionProvider());

        await InsertScheduleTasksAsync();

        await InsertMessageTemplatesAsync();

        await InsertB2BFeaturesSettingsAsync();

        await this.InstallPluginAsync();

        await base.InstallAsync();
    }

    public override async Task UpdateAsync(string currentVersion, string targetVersion)
    {
        if (targetVersion == currentVersion)
            return;

        await _permissionService.InstallPermissionsAsync(new B2BB2CPermissionProvider());

        await InstalLocalResourseStringFromXmlFileAsync();

        await InsertScheduleTasksAsync();

        await InsertMessageTemplatesAsync();
        
        await base.UpdateAsync(currentVersion, targetVersion);
    }

    public override async Task UninstallAsync()
    {
        await UninstalLocalResourseStringFromXmlFileAsync();

        await _permissionService.UninstallPermissionsAsync(new B2BB2CPermissionProvider());

        await base.UninstallAsync();
    }

    public override string GetConfigurationPageUrl()
    {
        return $"{_webHelper.GetStoreLocation()}Admin/B2BB2CFeatures/Configure";
    }

    public async Task ManageSiteMapAsync(SiteMapNode rootNode)
    {
        var childNode = new SiteMapNode()
        {
            SystemName = B2BB2CFeaturesDefaults.B2BB2C_Features_Root_SiteMapNode_SystemName,
            Title = await _localizationService.GetResourceAsync(
                        "NopStation.Plugin.B2B.B2BB2CFeatures.SiteMapNode.Title"),
            IconClass = "nav-icon fas fa-cube",
            Visible = true,
            ChildNodes = new List<SiteMapNode>()
            {
                new ()
                {
                    SystemName = B2BB2CFeaturesDefaults.B2BB2C_Features_Configuration_SiteMapNode_SystemName,
                    Title = await _localizationService.GetResourceAsync(
                        "NopStation.Plugin.B2B.B2BB2CFeatures.SiteMapNode.Title.Configuration"),
                    ControllerName = "B2BB2CFeatures",
                    ActionName = "Configure",
                    IconClass = "nav-icon fas fa-cogs",
                    Visible = true,
                    ChildNodes = new List<SiteMapNode>() { },
                },
                new ()
                {
                    SystemName = B2BB2CFeaturesDefaults.ErpAccounts_SiteMapNode_SystemName,
                    Title = await _localizationService.GetResourceAsync(
                        "NopStation.Plugin.B2B.B2BB2CFeatures.SiteMapNode.Title.ErpAccounts"),
                    ControllerName = "ErpAccount",
                    ActionName = "List",
                    IconClass = "nav-icon fas fa-users",
                    Visible = true,
                    ChildNodes = new List<SiteMapNode>() { },
                },
                new ()
                {
                    SystemName = B2BB2CFeaturesDefaults.ErpRegistrationApplication_SiteMapNode_SystemName,
                    Title = await _localizationService.GetResourceAsync(
                        "NopStation.Plugin.B2B.B2BB2CFeatures.SiteMapNode.Title.ErpRegistrationApplication"),
                    ControllerName = "ErpRegistrationApplication",
                    ActionName = "List",
                    IconClass = "nav-icon fas fa-users",
                    Visible = true,
                    ChildNodes = new List<SiteMapNode>() { },
                },
                new ()
                {
                    SystemName = B2BB2CFeaturesDefaults.ErpSalesOrgs_SiteMapNode_SystemName,
                    Title = await _localizationService.GetResourceAsync(
                        "NopStation.Plugin.B2B.B2BB2CFeatures.SiteMapNode.Title.ErpSalesOrgs"),
                    ControllerName = "ErpSalesOrg",
                    ActionName = "List",
                    IconClass = "nav-icon fas fa-building",
                    Visible = true,
                    ChildNodes = new List<SiteMapNode>() { },
                },
                new ()
                {
                    SystemName = B2BB2CFeaturesDefaults.ErpShipToAddress_SiteMapNode_SystemName,
                    Title = await _localizationService.GetResourceAsync(
                        "NopStation.Plugin.B2B.B2BB2CFeatures.SiteMapNode.Title.ErpShipToAddress"),
                    ControllerName = "ErpShipToAddress",
                    ActionName = "List",
                    IconClass = "nav-icon fas fa-circle",
                    Visible = true,
                    ChildNodes = new List<SiteMapNode>() { },
                },
                new ()
                {
                    SystemName = B2BB2CFeaturesDefaults.ErpNopUsers_SiteMapNode_SystemName,
                    Title = await _localizationService.GetResourceAsync(
                        "NopStation.Plugin.B2B.B2BB2CFeatures.SiteMapNode.Title.ErpNopUsers"),
                    ControllerName = "ErpNopUser",
                    ActionName = "List",
                    IconClass = "nav-icon fas fa-user",
                    Visible = true,
                    ChildNodes = new List<SiteMapNode>() { },
                },
                new ()
                {
                    SystemName = B2BB2CFeaturesDefaults.ErpGroupPriceCode_SiteMapNode_SystemName,
                    Title = await _localizationService.GetResourceAsync(
                        "NopStation.Plugin.B2B.B2BB2CFeatures.SiteMapNode.Title.ErpGroupPriceCode"),
                    ControllerName = "ErpGroupPriceCode",
                    ActionName = "List",
                    IconClass = "nav-icon fas fa-dollar-sign",
                    Visible = true,
                    ChildNodes = new List<SiteMapNode>() { },
                },
                new ()
                {
                    SystemName = B2BB2CFeaturesDefaults.ErpInvoices_SiteMapNode_SystemName,
                    Title = await _localizationService.GetResourceAsync(
                        "NopStation.Plugin.B2B.B2BB2CFeatures.SiteMapNode.Title.ErpInvoices"),
                    ControllerName = "ErpInvoice",
                    ActionName = "List",
                    IconClass = "nav-icon fas fa-file-invoice-dollar",
                    Visible = true,
                    ChildNodes = new List<SiteMapNode>() { },
                },
                new ()
                {
                    SystemName = B2BB2CFeaturesDefaults.ErpDeliveryDates_SiteMapNode_SystemName,
                    Title = await _localizationService.GetResourceAsync(
                        "NopStation.Plugin.B2B.B2BB2CFeatures.SiteMapNode.Title.ErpDeliveryDates"),
                    ControllerName = "ErpDeliveryDates",
                    ActionName = "List",
                    IconClass = "nav-icon fas fa-list",
                    Visible = true,
                    ChildNodes = new List<SiteMapNode>() { },
                },
                new ()
                {
                    SystemName = B2BB2CFeaturesDefaults.SalesRepresentatives_SiteMapNode_SystemName,
                    Title = await _localizationService.GetResourceAsync(
                        "NopStation.Plugin.B2B.B2BB2CFeatures.SiteMapNode.Title.SalesRepresentatives"),
                    ControllerName = "SalesRepresentative",
                    ActionName = "List",
                    IconClass = "nav-icon fas fa-user-tie",
                    Visible = true,
                    ChildNodes = new List<SiteMapNode>() { },
                },
                new ()
                {
                    SystemName = B2BB2CFeaturesDefaults.B2CMacsteelExpressShop_SiteMapNode_SystemName,
                    Title = await _localizationService.GetResourceAsync(
                        "NopStation.Plugin.B2B.B2BB2CFeatures.SiteMapNode.Title.B2CMacsteelExpressShop"),
                    ControllerName = "B2CMacsteelExpressShop",
                    ActionName = "List",
                    IconClass = "nav-icon fas fa-genderless",
                    Visible = true,
                    ChildNodes = new List<SiteMapNode>() { },
                },
                new ()
                {
                    SystemName = B2BB2CFeaturesDefaults.ErpAllProducts_SiteMapNode_SystemName,
                    Title = await _localizationService.GetResourceAsync(
                        "NopStation.Plugin.B2B.B2BB2CFeatures.SiteMapNode.Title.ErpAllProducts"),
                    ControllerName = "ErpProductPricing",
                    ActionName = "AllProductList",
                    IconClass = "nav-icon fas fa-list",
                    Visible = true,
                    ChildNodes = new List<SiteMapNode>() { },
                },
                new ()
                {
                    SystemName = B2BB2CFeaturesDefaults.ErpOrder_SiteMapNode_SystemName,
                    Title = await _localizationService.GetResourceAsync(
                        "NopStation.Plugin.B2B.B2BB2CFeatures.SiteMapNode.Title.ErpOrder"),
                    ControllerName = "ErpOrder",
                    ActionName = "List",
                    IconClass = "nav-icon fas fa-list",
                    Visible = true,
                    ChildNodes = new List<SiteMapNode>() { },
                },
                new ()
                {
                    SystemName = B2BB2CFeaturesDefaults.CustomerRestrictionShown_SiteMapNode_SystemName,
                    Title = await _localizationService.GetResourceAsync(
                        "NopStation.Plugin.B2B.B2BB2CFeatures.SiteMapNode.Title.CustomerRestrictionShown"),
                    ControllerName = "B2BCustomerRestriction",
                    ActionName = "ShownList",
                    IconClass = "nav-icon fas fa-list",
                    Visible = true,
                    ChildNodes = new List<SiteMapNode>() { },
                },
                new ()
                {
                    SystemName = B2BB2CFeaturesDefaults.CustomerRestrictionHide_SiteMapNode_SystemName,
                    Title = await _localizationService.GetResourceAsync(
                        "NopStation.Plugin.B2B.B2BB2CFeatures.SiteMapNode.Title.CustomerRestrictionHide"),
                    ControllerName = "B2BCustomerRestriction",
                    ActionName = "HideList",
                    IconClass = "nav-icon fas fa-list",
                    Visible = true,
                    ChildNodes = new List<SiteMapNode>() { },
                },
                new ()
                {
                    SystemName = B2BB2CFeaturesDefaults.ERPPriceListDownloadTrack_SiteMapNode_SystemName,
                    Title = await _localizationService.GetResourceAsync(
                        "NopStation.Plugin.B2B.B2BB2CFeatures.SiteMapNode.Title.ERPPriceListDownloadTrack"),
                    ControllerName = "ERPPriceListDownloadTrack",
                    ActionName = "List",
                    IconClass = "nav-icon fas fa-list",
                    Visible = true,
                    ChildNodes = new List<SiteMapNode>() { },
                },
                new ()
                {
                    SystemName = B2BB2CFeaturesDefaults.ERPSAPErrorMsgTranslations_SiteMapNode_SystemName,
                    Title = await _localizationService.GetResourceAsync(
                        "NopStation.Plugin.B2B.B2BB2CFeatures.SiteMapNode.Title.ERPSAPErrorMsgTranslations"),
                    ControllerName = "ERPSAPErrorMsgTranslation",
                    ActionName = "List",
                    IconClass = "nav-icon fas fa-list",
                    Visible = true,
                    ChildNodes = new List<SiteMapNode>() { },
                },
            },
        };
        var pluginNode = rootNode.ChildNodes.FirstOrDefault(x =>
            x.SystemName == "Third party plugins"
        );
        pluginNode?.ChildNodes.Add(childNode);

        childNode = new SiteMapNode()
        {
            SystemName = B2BB2CFeaturesDefaults.ErpLogs_SiteMapNode_SystemName,
            Title = await _localizationService.GetResourceAsync(
                "NopStation.Plugin.B2B.B2BB2CFeatures.SiteMapNode.Title.ErpLogs"),
            ControllerName = "ErpLogs",
            ActionName = "List",
            IconClass = "nav-icon fas fa-list",
            Visible = true,
            ChildNodes = new List<SiteMapNode>() { },
        };
        pluginNode?.ChildNodes.Add(childNode);

        childNode = new SiteMapNode()
        {
            SystemName = B2BB2CFeaturesDefaults.ErpActivityLogs_SiteMapNode_SystemName,
            Title = await _localizationService.GetResourceAsync(
                "NopStation.Plugin.B2B.B2BB2CFeatures.SiteMapNode.Title.ErpActivityLogs"),
            IconClass = "nav-icon fas fa-cube",
            Visible = true,
            ChildNodes = new List<SiteMapNode>()
            {
                new ()
                {
                    SystemName = B2BB2CFeaturesDefaults.ErpActivityLogsList_SiteMapNode_SystemName,
                    Title = await _localizationService.GetResourceAsync(
                        "NopStation.Plugin.B2B.B2BB2CFeatures.SiteMapNode.Title.ErpActivityLogsList"),
                    ControllerName = "ErpActivityLogs",
                    ActionName = "ERPActivityLogs",
                    IconClass = "far fa-dot-circle",
                    Visible = true,
                    ChildNodes = new List<SiteMapNode>() { },
                },
            }
        };

        pluginNode?.ChildNodes.Add(childNode);
    }

    public List<KeyValuePair<string, string>> PluginResouces()
    {
        var fileProvider = EngineContext.Current.Resolve<INopFileProvider>();
        var path = fileProvider.MapPath(B2BB2CFeaturesDefaults.XmlResourceStringFilePath);
        using var sr = new StreamReader(path, Encoding.UTF8);
        var result = new HashSet<(string name, string value)>();

        using (var xmlReader = XmlReader.Create(sr))
            while (xmlReader.ReadToFollowing("Language"))
            {
                if (xmlReader.NodeType != XmlNodeType.Element)
                    continue;

                using var languageReader = xmlReader.ReadSubtree();
                while (languageReader.ReadToFollowing("LocaleResource"))
                    if (
                        xmlReader.NodeType == XmlNodeType.Element
                        && xmlReader.GetAttribute("Name") is string name
                    )
                    {
                        using var lrReader = languageReader.ReadSubtree();
                        if (
                            lrReader.ReadToFollowing("Value")
                            && lrReader.NodeType == XmlNodeType.Element
                        )
                            result.Add((name.ToLowerInvariant(), lrReader.ReadString()));
                    }

                break;
            }

        return result
            .Select(item => new KeyValuePair<string, string>(item.name, item.value))
            .ToList();
    }

    #endregion
}
