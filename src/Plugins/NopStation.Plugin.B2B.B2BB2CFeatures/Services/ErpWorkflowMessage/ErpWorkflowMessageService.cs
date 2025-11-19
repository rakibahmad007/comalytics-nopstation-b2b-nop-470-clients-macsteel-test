using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Messages;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Vendors;
using Nop.Core.Domain.Tax;
using Nop.Core.Events;
using Nop.Services.Affiliates;
using Nop.Services.Common;
using Nop.Core.Infrastructure;
using Nop.Services.Catalog;
using Nop.Services.Customers;
using Nop.Services.Directory;
using Nop.Services.Localization;
using Nop.Services.Messages;
using Nop.Services.Orders;
using Nop.Services.Stores;
using NopStation.Plugin.B2B.B2BB2CFeatures.Services.ErpCustomerFunctionality;
using NopStation.Plugin.B2B.ERPIntegrationCore.Domain;
using NopStation.Plugin.B2B.ERPIntegrationCore.Services;
using NopStation.Plugin.B2B.ERPIntegrationCore.Enums;
using Nop.Services.Tax;

namespace NopStation.Plugin.B2B.B2BB2CFeatures.Services.ErpWorkflowMessage;

public partial class ErpWorkflowMessageService : IErpWorkflowMessageService
{
    #region Fields

    private readonly EmailAccountSettings _emailAccountSettings;
    private readonly IEmailAccountService _emailAccountService;
    private readonly IEventPublisher _eventPublisher;
    private readonly ILanguageService _languageService;
    private readonly ILocalizationService _localizationService;
    private readonly IMessageTemplateService _messageTemplateService;
    private readonly IMessageTokenProvider _messageTokenProvider;
    private readonly IQueuedEmailService _queuedEmailService;
    private readonly IStoreContext _storeContext;
    private readonly IStoreService _storeService;
    private readonly ITokenizer _tokenizer;
    private readonly ICustomerService _customerService;
    private readonly IWorkContext _workContext;
    private readonly IOrderService _orderService;
    private readonly MessageTemplatesSettings _templatesSettings;
    private readonly IProductService _productService;
    private readonly ICurrencyService _currencyService;
    private readonly IPriceFormatter _priceFormatter;
    private readonly IShoppingCartService _shoppingCartService;
    private readonly ITaxService _taxService;
    private readonly MessagesSettings _messagesSettings;
    private readonly IAddressService _addressService;
    private readonly IAffiliateService _affiliateService;
    private readonly IErpCustomerFunctionalityService _erpCustomerFunctionalityService;
    private readonly IErpShipToAddressService _erpShipToAddressService;
    private readonly IGenericAttributeService _genericAttributeService;

    public static string JobTitleAttribute => "JobTitle";

    #endregion

    #region Ctor

    public ErpWorkflowMessageService(
        EmailAccountSettings emailAccountSettings,
        IEmailAccountService emailAccountService,
        IEventPublisher eventPublisher,
        ILanguageService languageService,
        ILocalizationService localizationService,
        IMessageTemplateService messageTemplateService,
        IMessageTokenProvider messageTokenProvider,
        IQueuedEmailService queuedEmailService,
        IStoreContext storeContext,
        IStoreService storeService,
        ITokenizer tokenizer,
        ICustomerService customerService,
        IWorkContext workContext,
        IOrderService orderService,
        MessageTemplatesSettings templatesSettings,
        IProductService productService,
        ICurrencyService currencyService,
        IPriceFormatter priceFormatter,
        IShoppingCartService shoppingCartService,
        ITaxService taxService,
        IAddressService addressService,
        IAffiliateService affiliateService,
        IErpCustomerFunctionalityService erpCustomerFunctionalityService,
        IErpShipToAddressService erpShipToAddressService,
        IGenericAttributeService genericAttributeService,
        MessagesSettings messagesSettings)
    {
        _emailAccountSettings = emailAccountSettings;
        _emailAccountService = emailAccountService;
        _eventPublisher = eventPublisher;
        _languageService = languageService;
        _localizationService = localizationService;
        _messageTemplateService = messageTemplateService;
        _messageTokenProvider = messageTokenProvider;
        _queuedEmailService = queuedEmailService;
        _storeContext = storeContext;
        _storeService = storeService;
        _tokenizer = tokenizer;
        _customerService = customerService;
        _workContext = workContext;
        _orderService = orderService;
        _templatesSettings = templatesSettings;
        _productService = productService;
        _currencyService = currencyService;
        _priceFormatter = priceFormatter;
        _shoppingCartService = shoppingCartService;
        _taxService = taxService;
        _addressService = addressService;
        _messagesSettings = messagesSettings;
        _affiliateService = affiliateService;
        _erpCustomerFunctionalityService = erpCustomerFunctionalityService;
        _erpShipToAddressService = erpShipToAddressService;
        _genericAttributeService = genericAttributeService;
    }

    #endregion

    #region Utilities

    protected virtual async Task<IList<MessageTemplate>> GetActiveMessageTemplatesAsync(string messageTemplateName, int storeId)
    {
        var messageTemplates = await _messageTemplateService.GetMessageTemplatesByNameAsync(messageTemplateName, storeId);

        if (!messageTemplates?.Any() ?? true)
            return new List<MessageTemplate>();

        messageTemplates = messageTemplates.Where(messageTemplate => messageTemplate.IsActive).ToList();

        return messageTemplates;
    }

    protected virtual async Task<EmailAccount> GetEmailAccountOfMessageTemplateAsync(MessageTemplate messageTemplate, int languageId)
    {
        var emailAccountId = await _localizationService.GetLocalizedAsync(messageTemplate, mt => mt.EmailAccountId, languageId);
        //some 0 validation (for localizable "Email account" dropdownlist which saves 0 if "Standard" value is chosen)
        if (emailAccountId == 0)
            emailAccountId = messageTemplate.EmailAccountId;

        var emailAccount = (await _emailAccountService.GetEmailAccountByIdAsync(emailAccountId)
            ?? await _emailAccountService.GetEmailAccountByIdAsync(_emailAccountSettings.DefaultEmailAccountId))
            ?? (await _emailAccountService.GetAllEmailAccountsAsync()).FirstOrDefault();

        return emailAccount;
    }

    protected virtual async Task<int> EnsureLanguageIsActiveAsync(int languageId, int storeId)
    {
        var language = await _languageService.GetLanguageByIdAsync(languageId);

        if (language == null || !language.Published)
        {
            language = (await _languageService.GetAllLanguagesAsync(storeId: storeId)).FirstOrDefault();
        }

        if (language == null || !language.Published)
        {
            language = (await _languageService.GetAllLanguagesAsync()).FirstOrDefault();
        }

        if (language == null)
            throw new Exception("No active language could be loaded");

        return language.Id;
    }

    public virtual async Task<int> SendNotificationAsync(MessageTemplate messageTemplate,
        EmailAccount emailAccount, 
        int languageId, 
        IEnumerable<Token> tokens,
        string toEmailAddress, 
        string toName,
        string attachmentFilePath = null, 
        string attachmentFileName = null,
        string replyToEmailAddress = null, 
        string replyToName = null,
        string fromEmail = null, 
        string fromName = null, 
        string subject = null)
    {
        ArgumentNullException.ThrowIfNull(messageTemplate);

        ArgumentNullException.ThrowIfNull(emailAccount);

        var bcc = await _localizationService.GetLocalizedAsync(messageTemplate, mt => mt.BccEmailAddresses, languageId);

        if (string.IsNullOrEmpty(subject))
            subject = await _localizationService.GetLocalizedAsync(messageTemplate, mt => mt.Subject, languageId);

        var body = await _localizationService.GetLocalizedAsync(messageTemplate, mt => mt.Body, languageId);

        var subjectReplaced = _tokenizer.Replace(subject, tokens, false);
        var bodyReplaced = _tokenizer.Replace(body, tokens, true);

        toName = CommonHelper.EnsureMaximumLength(toName, 300);

        var email = new QueuedEmail
        {
            Priority = QueuedEmailPriority.High,
            From = !string.IsNullOrEmpty(fromEmail) ? fromEmail : emailAccount.Email,
            FromName = !string.IsNullOrEmpty(fromName) ? fromName : emailAccount.DisplayName,
            To = toEmailAddress,
            ToName = toName,
            ReplyTo = replyToEmailAddress,
            ReplyToName = replyToName,
            CC = string.Empty,
            Bcc = bcc,
            Subject = subjectReplaced,
            Body = bodyReplaced,
            AttachmentFilePath = attachmentFilePath,
            AttachmentFileName = attachmentFileName,
            AttachedDownloadId = messageTemplate.AttachedDownloadId,
            CreatedOnUtc = DateTime.UtcNow,
            EmailAccountId = emailAccount.Id,
            DontSendBeforeDateUtc = !messageTemplate.DelayBeforeSend.HasValue ? null
                : (DateTime.UtcNow + TimeSpan.FromHours(messageTemplate.DelayPeriod.ToHours(messageTemplate.DelayBeforeSend.Value)))
        };

        await _queuedEmailService.InsertQueuedEmailAsync(email);

        return email.Id;
    }

    protected virtual async Task<string> OrderItemListToHtmlTableAsync(Order order, int languageId, int vendorId = 0)
    {
        var productService = EngineContext.Current.Resolve<IProductService>();

        var sb = new StringBuilder();
        sb.AppendLine("<table border=\"0\" style=\"width:100%;\">");

        sb.AppendLine($"<tr style=\"background-color:{_templatesSettings.Color1};text-align:center;\">");
        sb.AppendLine($"<th>{await _localizationService.GetResourceAsync("Messages.Order.Product(s).Name", languageId)}</th>");
        sb.AppendLine($"<th>{await _localizationService.GetResourceAsync("Messages.Order.Product(s).Price", languageId)}</th>");
        sb.AppendLine($"<th>{await _localizationService.GetResourceAsync("Messages.Order.Product(s).Quantity", languageId)}</th>");
        sb.AppendLine($"<th>{await _localizationService.GetResourceAsync("Messages.Order.Product(s).Total", languageId)}</th>");
        sb.AppendLine("</tr>");

        var table = await _orderService.GetOrderItemsAsync(order.Id);
        for (var i = 0; i <= table.Count - 1; i++)
        {
            var orderItem = table[i];
            var product = await _productService.GetProductByIdAsync(orderItem.ProductId);
            if (product == null)
                continue;

            if (vendorId > 0 && product.VendorId != vendorId)
                continue;

            sb.AppendLine($"<tr style=\"background-color: {_templatesSettings.Color2};text-align: center;\">");
            //product name
            var productName = await _localizationService.GetLocalizedAsync(product, x => x.Name, languageId);

            sb.AppendLine("<td style=\"padding: 0.6em 0.4em;text-align: left;\">" + WebUtility.HtmlEncode(productName));

            //SKU
            var sku = await productService.FormatSkuAsync(product, orderItem.AttributesXml);
            if (!string.IsNullOrEmpty(sku))
            {
                sb.AppendLine("<br />");
                sb.AppendLine(string.Format(await _localizationService.GetResourceAsync("Messages.Order.Product(s).SKU", languageId), WebUtility.HtmlEncode(sku)));
            }

            sb.AppendLine("</td>");

            string unitPriceStr;
            if (order.CustomerTaxDisplayType == TaxDisplayType.IncludingTax)
            {
                //including tax
                var unitPriceInclTaxInCustomerCurrency = _currencyService.ConvertCurrency(orderItem.UnitPriceInclTax, order.CurrencyRate);
                unitPriceStr = await _priceFormatter.FormatPriceAsync(unitPriceInclTaxInCustomerCurrency, true, order.CustomerCurrencyCode, languageId, true);
            }
            else
            {
                //excluding tax
                var unitPriceExclTaxInCustomerCurrency = _currencyService.ConvertCurrency(orderItem.UnitPriceExclTax, order.CurrencyRate);
                unitPriceStr = await _priceFormatter.FormatPriceAsync(unitPriceExclTaxInCustomerCurrency, true, order.CustomerCurrencyCode, languageId, false);
            }

            sb.AppendLine($"<td style=\"padding: 0.6em 0.4em;text-align: right;\">{unitPriceStr}</td>");

            sb.AppendLine($"<td style=\"padding: 0.6em 0.4em;text-align: center;\">{orderItem.Quantity}</td>");

            string priceStr;
            if (order.CustomerTaxDisplayType == TaxDisplayType.IncludingTax)
            {
                //including tax
                var priceInclTaxInCustomerCurrency = _currencyService.ConvertCurrency(orderItem.PriceInclTax, order.CurrencyRate);
                priceStr = await _priceFormatter.FormatPriceAsync(priceInclTaxInCustomerCurrency, true, order.CustomerCurrencyCode, languageId, true);
            }
            else
            {
                //excluding tax
                var priceExclTaxInCustomerCurrency = _currencyService.ConvertCurrency(orderItem.PriceExclTax, order.CurrencyRate);
                priceStr = await _priceFormatter.FormatPriceAsync(priceExclTaxInCustomerCurrency, true, order.CustomerCurrencyCode, languageId, false);
            }

            sb.AppendLine($"<td style=\"padding: 0.6em 0.4em;text-align: right;\">{priceStr}</td>");

            sb.AppendLine("</tr>");
        }

        sb.AppendLine("</table>");
        var result = sb.ToString();
        return result;
    }

    protected virtual async Task<string> ShoppingCartItemListToHtmlTableAsync(Customer customer, int languageId, int vendorId = 0)
    {
        var sb = new StringBuilder();
        sb.AppendLine("<table border=\"0\" style=\"width:100%;\">");

        sb.AppendLine($"<tr style=\"background-color:{_templatesSettings.Color1};text-align:center;\">");
        sb.AppendLine($"<th>{await _localizationService.GetResourceAsync("Messages.Order.Product(s).Name", languageId)}</th>");
        sb.AppendLine($"<th>{await _localizationService.GetResourceAsync("Messages.Order.Product(s).Price", languageId)}</th>");
        sb.AppendLine($"<th>{await _localizationService.GetResourceAsync("Messages.Order.Product(s).Quantity", languageId)}</th>");
        sb.AppendLine($"<th>{await _localizationService.GetResourceAsync("Messages.Order.Product(s).Total", languageId)}</th>");
        sb.AppendLine("</tr>");

        //var table = customer.ShoppingCartItems.Where(x=>x.ShoppingCartTypeId.Equals((int)ShoppingCartType.ShoppingCart)).ToList();

        var table = await _shoppingCartService.GetShoppingCartAsync(customer, ShoppingCartType.ShoppingCart);

        for (var i = 0; i <= table.Count - 1; i++)
        {
            var shoppingCartItem = table[i];
            var product = await _productService.GetProductByIdAsync(shoppingCartItem.ProductId);
            if (product == null)
                continue;

            if (vendorId > 0 && product.VendorId != vendorId)
                continue;

            sb.AppendLine($"<tr style=\"background-color: {_templatesSettings.Color2};text-align: center;\">");
            //product name
            var productName = await _localizationService.GetLocalizedAsync(product, x => x.Name, languageId);

            sb.AppendLine("<td style=\"padding: 0.6em 0.4em;text-align: left;\">" + WebUtility.HtmlEncode(productName));

            //SKU
            var sku = await _productService.FormatSkuAsync(product, shoppingCartItem.AttributesXml);
            if (!string.IsNullOrEmpty(sku))
            {
                sb.AppendLine("<br />");
                sb.AppendLine(string.Format(await _localizationService.GetResourceAsync("Messages.Order.Product(s).SKU", languageId), WebUtility.HtmlEncode(sku)));
            }

            sb.AppendLine("</td>");

            string unitPriceStr;
            var shoppingCartUnitPriceWithDiscountBase = await _taxService.GetProductPriceAsync(product, (await _shoppingCartService.GetUnitPriceAsync(shoppingCartItem, true)).unitPrice);
            var shoppingCartUnitPriceWithDiscount = await _currencyService.ConvertFromPrimaryStoreCurrencyAsync(shoppingCartUnitPriceWithDiscountBase.price, await _workContext.GetWorkingCurrencyAsync());
            unitPriceStr = await _priceFormatter.FormatPriceAsync(shoppingCartUnitPriceWithDiscount);

            sb.AppendLine($"<td style=\"padding: 0.6em 0.4em;text-align: right;\">{unitPriceStr}</td>");

            sb.AppendLine($"<td style=\"padding: 0.6em 0.4em;text-align: center;\">{shoppingCartItem.Quantity}</td>");

            string priceStr;
            //sub total
            var shoppingCartItemSubTotalWithDiscountBase = await _taxService.GetProductPriceAsync(product, (await _shoppingCartService.GetSubTotalAsync(shoppingCartItem, true)).subTotal);
            var shoppingCartItemSubTotalWithDiscount = await _currencyService.ConvertFromPrimaryStoreCurrencyAsync(shoppingCartItemSubTotalWithDiscountBase.price, await _workContext.GetWorkingCurrencyAsync());
            priceStr = await _priceFormatter.FormatPriceAsync(shoppingCartItemSubTotalWithDiscount);

            sb.AppendLine($"<td style=\"padding: 0.6em 0.4em;text-align: right;\">{priceStr}</td>");

            sb.AppendLine("</tr>");
        }

        sb.AppendLine("</table>");
        var result = sb.ToString();
        return result;
    }

    private async Task<string> CombineLineItemsToHtmlTableAsync(int orderId, int originalQuoteOrderId, int languageId)
    {
        var order = await _orderService.GetOrderByIdAsync(orderId);
        if (order == null)
            return string.Empty;

        var itemsOfOriginalQuoteOrder = await _orderService.GetOrderItemsAsync(originalQuoteOrderId);
        if (itemsOfOriginalQuoteOrder == null && itemsOfOriginalQuoteOrder?.Count < 1)
            return string.Empty;

        var originalQuoteOrderGroupedByProduct = itemsOfOriginalQuoteOrder.GroupBy(x => x.ProductId);
        var orderItemsWithMultipleLines = originalQuoteOrderGroupedByProduct.Where(x => x.Count() > 1).Select(x => x);

        var store = await _storeContext.GetCurrentStoreAsync();
        var combinedProdIds = new List<int>();

        var sb = new StringBuilder();
        sb.AppendLine("<table border=\"0\" style=\"width:100%;\">");

        sb.AppendLine($"<tr style=\"background-color:{_templatesSettings.Color2};text-align:center;\">");
        sb.AppendLine($"<td>{await _localizationService.GetResourceAsync("Messages.Order.Number", languageId)}</td>");
        sb.AppendLine($"<td>{order.CustomOrderNumber}</td>");
        sb.AppendLine("</tr>");

        sb.AppendLine($"<tr style=\"background-color:{_templatesSettings.Color2};text-align:center;\">");
        sb.AppendLine($"<td>{await _localizationService.GetResourceAsync("Messages.Order.Details", languageId)}</td>");
        sb.AppendLine($"<td><a>{store.Url}orderdetails/{order.Id}</a></td>");
        sb.AppendLine("</tr>");

        sb.AppendLine($"<tr style=\"background-color:{_templatesSettings.Color2};text-align:center;\">");
        sb.AppendLine($"<td>{await _localizationService.GetResourceAsync("Messages.Order.Date", languageId)}</td>");
        sb.AppendLine($"<td>{order.CreatedOnUtc.ToShortDateString()}</td>");
        sb.AppendLine("</tr>");

        sb.AppendLine($"<tr style=\"background-color:{_templatesSettings.Color2};text-align:center;\">");
        sb.AppendLine($"<td></td>");
        sb.AppendLine($"<td></td>");
        sb.AppendLine("</tr>");

        sb.AppendLine($"<tr style=\"background-color:{_templatesSettings.Color1};text-align:center;\">");
        sb.AppendLine($"<td>{await _localizationService.GetResourceAsync("Materials.Heading", languageId)}</td>");
        sb.AppendLine($"<td></td>");
        sb.AppendLine("</tr>");

        foreach (var item in orderItemsWithMultipleLines)
        {
            foreach (var prod in item)
            {
                sb.AppendLine($"<tr style=\"background-color:{_templatesSettings.Color2};text-align:center;\">");
                sb.AppendLine($"<td>{(await _productService.GetProductByIdAsync(prod.ProductId))?.Sku ?? string.Empty}</td>");
                sb.AppendLine($"<td>{prod.Quantity}</td>");
                sb.AppendLine("</tr>");
                combinedProdIds.Add(prod.ProductId);
            }
        }

        sb.AppendLine($"<tr style=\"background-color:{_templatesSettings.Color2};text-align:center;\">");
        sb.AppendLine($"<td></td>");
        sb.AppendLine($"<td></td>");
        sb.AppendLine("</tr>");

        sb.AppendLine($"<tr style=\"background-color:{_templatesSettings.Color1};text-align:center;\">");
        sb.AppendLine($"<td>{await _localizationService.GetResourceAsync("CombinedMaterials.Heading", languageId)}</td>");
        sb.AppendLine($"<td></td>");
        sb.AppendLine("</tr>");

        var combinedOrderItems = (await _orderService.GetOrderItemsAsync(orderId)).Where(x => combinedProdIds.Contains(x.ProductId)).Select(x => x).ToList();
        foreach (var item in combinedOrderItems)
        {
            sb.AppendLine($"<tr style=\"background-color:{_templatesSettings.Color2};text-align:center;\">");
            sb.AppendLine($"<td>{(await _productService.GetProductByIdAsync(item.ProductId))?.Sku ?? string.Empty}</td>");
            sb.AppendLine($"<td>{item.Quantity}</td>");
            sb.AppendLine("</tr>");
        }

        var result = sb.ToString();
        return result;
    }

    private async Task AddOrderAndQuoteOrderItemsTokensAsync(IList<Token> tokens, int orderId, int originalQuoteOrderId, int languageId)
    {
        tokens.Add(new Token("Warning.LineItemsCombined.Subject",
            await _localizationService.GetResourceAsync("Plugin.Misc.NopStation.B2BB2CFeatures.Warning.LineItemsCombined.Subject", languageId)));
        tokens.Add(new Token("Order.OrderDetails", await CombineLineItemsToHtmlTableAsync(orderId, originalQuoteOrderId, languageId), true));
    }

    protected async Task<(string email, string name)> GetCustomerReplyToNameAndEmailAsync(MessageTemplate messageTemplate, Customer customer)
    {
        if (!messageTemplate.AllowDirectReply)
            return (null, null);

        var replyToEmail = await _customerService.IsGuestAsync(customer)
            ? string.Empty
            : customer.Email;

        var replyToName = await _customerService.IsGuestAsync(customer)
            ? string.Empty
            : await _customerService.GetCustomerFullNameAsync(customer);

        return (replyToEmail, replyToName);
    }

    protected async Task<(string email, string name)> GetCustomerReplyToNameAndEmailAsync(MessageTemplate messageTemplate, Order order)
    {
        if (!messageTemplate.AllowDirectReply)
            return (null, null);

        var billingAddress = await _addressService.GetAddressByIdAsync(order.BillingAddressId);

        return (billingAddress.Email, $"{billingAddress.FirstName} {billingAddress.LastName}");
    }

    protected async Task<(string email, string name)> GetStoreOwnerNameAndEmailAsync(EmailAccount messageTemplateEmailAccount)
    {
        var storeOwnerEmailAccount = _messagesSettings.UseDefaultEmailAccountForSendStoreOwnerEmails ? await _emailAccountService.GetEmailAccountByIdAsync(_emailAccountSettings.DefaultEmailAccountId) : null;
        storeOwnerEmailAccount ??= messageTemplateEmailAccount;

        return (storeOwnerEmailAccount.Email, storeOwnerEmailAccount.DisplayName);
    }

    private async Task AddErpAccountTokensAsync(IList<Token> tokens, ErpAccount erpAccount)
    {
        tokens.Add(new Token("ErpAccount.AccountName", erpAccount.AccountName));
        tokens.Add(new Token("ErpAccount.AccountNumber", erpAccount.AccountNumber));

        await _eventPublisher.EntityTokensAddedAsync(erpAccount, tokens);
    }

    private async Task AddErpNopUserTokensAsync(IList<Token> tokens, ErpNopUser erpNopUser)
    {
        tokens.Add(new Token("ErpNopUser.NopCustomerId", erpNopUser.NopCustomerId));
        tokens.Add(new Token("ErpNopUser.ErpShipToAddressId", erpNopUser.ErpShipToAddressId));
        tokens.Add(new Token("ErpNopUser.ErpAccountId", erpNopUser.ErpAccountId));

        await _eventPublisher.EntityTokensAddedAsync(erpNopUser, tokens);
    }

    private async Task AddErpShipToAddressTokensAsync(IList<Token> tokens, ErpShipToAddress erpShipToAddress, ErpSalesOrg erpSalesOrg, ErpNopUser erpNopUser)
    {
        tokens.Add(new Token("ErpNopUser.Id", erpNopUser.Id));
        tokens.Add(new Token("ErpShipToAddress.NearestWarehouseId", erpShipToAddress.NearestWareHouseId));
        tokens.Add(new Token("ErpShipToAddress.DeliveryOption", erpShipToAddress.DeliveryOptionId));
        tokens.Add(new Token("ErpShipToAddress.DistanceToNearestWarehouse", erpShipToAddress.DistanceToNearestWareHouse));
        tokens.Add(new Token("ErpShipToAddress.Longitude", erpShipToAddress.Longitude));
        tokens.Add(new Token("ErpShipToAddress.Latitude", erpShipToAddress.Latitude));
        tokens.Add(new Token("ErpShipToAddress.SalesOrganizationName", erpSalesOrg.Name));

        await _eventPublisher.EntityTokensAddedAsync(erpShipToAddress, tokens);
    }

    private async Task AddCustomerTokensForB2BUserRegistrationAsync(IList<Token> tokens, Customer customer)
    {
        tokens.Add(new Token("Customer.JobTitle", await _genericAttributeService.GetAttributeAsync<string>(customer, JobTitleAttribute)));

        await _eventPublisher.EntityTokensAddedAsync(customer, tokens);
    }

    private async Task AddB2BUserRegistrationInfoTokensAsync(IList<Token> tokens, ErpUserRegistrationInfo erpUserRegistrationInfo, ErpSalesOrg erpSalesOrg)
    {
        tokens.Add(new Token("ErpUserRegistrationInfo.ErpAccountNumber", erpUserRegistrationInfo.ErpAccountNumber));
        tokens.Add(new Token("ErpUserRegistrationInfo.SalesOrganizationName", erpSalesOrg.Name));
        tokens.Add(new Token("ErpUserRegistrationInfo.SpecialInstructions", erpUserRegistrationInfo.SpecialInstructions));

        tokens.Add(new Token("ErpUserRegistrationInfo.AuthorizationFullName", erpUserRegistrationInfo.AuthorisationFullName));
        tokens.Add(new Token("ErpUserRegistrationInfo.PersonalAlternateContactNumber", erpUserRegistrationInfo.PersonalAlternateContactNumber));
        tokens.Add(new Token("ErpUserRegistrationInfo.ContactNumber", erpUserRegistrationInfo.AuthorisationContactNumber));
        tokens.Add(new Token("ErpUserRegistrationInfo.AuthorizationAlternateContactNumber", erpUserRegistrationInfo.AuthorisationAlternateContactNumber));
        tokens.Add(new Token("ErpUserRegistrationInfo.AuthorizationJobTitle", erpUserRegistrationInfo.AuthorisationJobTitle));
        tokens.Add(new Token("ErpUserRegistrationInfo.AuthorizationAdditionalComment", erpUserRegistrationInfo.AuthorisationAdditionalComment));

        await _eventPublisher.EntityTokensAddedAsync(erpUserRegistrationInfo, tokens);
    }

    #endregion

    #region Methods

    #region Order workflow

    public async Task<int> SendERPOrderPlaceFailedSalesRepNotificationAsync(Order order, int languageId, ErpShipToAddress erpShipToAddress)
    {
        ArgumentNullException.ThrowIfNull(order);

        var store = await _storeService.GetStoreByIdAsync(order.StoreId) ?? await _storeContext.GetCurrentStoreAsync();
        languageId = await EnsureLanguageIsActiveAsync(languageId, store.Id);

        var messageTemplate = (await GetActiveMessageTemplatesAsync(B2BB2CFeaturesDefaults.MessageTemplateSystemNames_ERPOrderPlaceFailedSalesRepNotification, store.Id)).FirstOrDefault();

        if (messageTemplate is null)
            return 0;

        var commonTokens = new List<Token>();
        var customer = await _customerService.GetCustomerByIdAsync(order.CustomerId);
        await _messageTokenProvider.AddOrderTokensAsync(commonTokens, order, languageId);
        await _messageTokenProvider.AddCustomerTokensAsync(commonTokens, customer);

        var emailAccount = await GetEmailAccountOfMessageTemplateAsync(messageTemplate, languageId);

        var tokens = new List<Token>(commonTokens);
        await _messageTokenProvider.AddStoreTokensAsync(tokens, store, emailAccount);

        await _eventPublisher.MessageTokensAddedAsync(messageTemplate, tokens);

        var toEmail = erpShipToAddress.RepEmail;
        var toName = erpShipToAddress.RepFullName;

        return await SendNotificationAsync(messageTemplate, emailAccount, languageId, tokens, toEmail, toName);
    }

    public async Task<IList<int>> SendOrderPlacedStoreOwnerNotificationAsync(Order order, int languageId)
    {
        ArgumentNullException.ThrowIfNull(order);

        var store = await _storeService.GetStoreByIdAsync(order.StoreId) ?? await _storeContext.GetCurrentStoreAsync();
        languageId = await EnsureLanguageIsActiveAsync(languageId, store.Id);

        var messageTemplates = await GetActiveMessageTemplatesAsync(MessageTemplateSystemNames.ORDER_PLACED_STORE_OWNER_NOTIFICATION, store.Id);
        if (!messageTemplates.Any())
            return new List<int>();

        var commonTokens = new List<Token>();
        await _messageTokenProvider.AddOrderTokensAsync(commonTokens, order, languageId);
        await _messageTokenProvider.AddCustomerTokensAsync(commonTokens, order.CustomerId);

        return await messageTemplates.SelectAwait(async messageTemplate =>
        {
            var emailAccount = await GetEmailAccountOfMessageTemplateAsync(messageTemplate, languageId);

            var tokens = new List<Token>(commonTokens);
            await _messageTokenProvider.AddStoreTokensAsync(tokens, store, emailAccount);

            await _eventPublisher.MessageTokensAddedAsync(messageTemplate, tokens);

            var (toEmail, toName) = await GetStoreOwnerNameAndEmailAsync(emailAccount);
            var (replyToEmail, replyToName) = await GetCustomerReplyToNameAndEmailAsync(messageTemplate, order);

            return await SendNotificationAsync(messageTemplate, emailAccount, languageId, tokens, toEmail, toName,
                replyToEmailAddress: replyToEmail, replyToName: replyToName);
        }).ToListAsync();
    }

    public async Task<IList<int>> SendOrderPlacedVendorNotificationAsync(Order order, Vendor vendor, int languageId)
    {
        ArgumentNullException.ThrowIfNull(order);

        ArgumentNullException.ThrowIfNull(vendor);

        var store = await _storeService.GetStoreByIdAsync(order.StoreId) ?? await _storeContext.GetCurrentStoreAsync();
        languageId = await EnsureLanguageIsActiveAsync(languageId, store.Id);

        var messageTemplates = await GetActiveMessageTemplatesAsync(MessageTemplateSystemNames.ORDER_PLACED_VENDOR_NOTIFICATION, store.Id);
        if (!messageTemplates.Any())
            return new List<int>();

        var commonTokens = new List<Token>();
        await _messageTokenProvider.AddOrderTokensAsync(commonTokens, order, languageId, vendor.Id);
        await _messageTokenProvider.AddCustomerTokensAsync(commonTokens, order.CustomerId);

        return await messageTemplates.SelectAwait(async messageTemplate =>
        {
            var emailAccount = await GetEmailAccountOfMessageTemplateAsync(messageTemplate, languageId);

            var tokens = new List<Token>(commonTokens);
            await _messageTokenProvider.AddStoreTokensAsync(tokens, store, emailAccount);

            await _eventPublisher.MessageTokensAddedAsync(messageTemplate, tokens);

            var toEmail = vendor.Email;
            var toName = vendor.Name;

            return await SendNotificationAsync(messageTemplate, emailAccount, languageId, tokens, toEmail, toName);
        }).ToListAsync();
    }

    public async Task<IList<int>> SendOrderPlacedAffiliateNotificationAsync(Order order, int languageId)
    {
        ArgumentNullException.ThrowIfNull(order);

        var affiliate = await _affiliateService.GetAffiliateByIdAsync(order.AffiliateId);

        ArgumentNullException.ThrowIfNull(affiliate);

        var store = await _storeService.GetStoreByIdAsync(order.StoreId) ?? await _storeContext.GetCurrentStoreAsync();
        languageId = await EnsureLanguageIsActiveAsync(languageId, store.Id);

        var messageTemplates = await GetActiveMessageTemplatesAsync(MessageTemplateSystemNames.ORDER_PLACED_AFFILIATE_NOTIFICATION, store.Id);
        if (!messageTemplates.Any())
            return new List<int>();

        var commonTokens = new List<Token>();
        await _messageTokenProvider.AddOrderTokensAsync(commonTokens, order, languageId);
        await _messageTokenProvider.AddCustomerTokensAsync(commonTokens, order.CustomerId);

        return await messageTemplates.SelectAwait(async messageTemplate =>
        {
            var emailAccount = await GetEmailAccountOfMessageTemplateAsync(messageTemplate, languageId);

            var tokens = new List<Token>(commonTokens);
            await _messageTokenProvider.AddStoreTokensAsync(tokens, store, emailAccount);

            await _eventPublisher.MessageTokensAddedAsync(messageTemplate, tokens);

            var affiliateAddress = await _addressService.GetAddressByIdAsync(affiliate.AddressId);
            var toEmail = affiliateAddress.Email;
            var toName = $"{affiliateAddress.FirstName} {affiliateAddress.LastName}";

            return await SendNotificationAsync(messageTemplate, emailAccount, languageId, tokens, toEmail, toName);
        }).ToListAsync();
    }

    public async Task<IList<int>> SendOrderPlacedCustomerNotificationAsync(Order order, int languageId,
        string attachmentFilePath = null, string attachmentFileName = null)
    {
        ArgumentNullException.ThrowIfNull(order);

        var store = await _storeService.GetStoreByIdAsync(order.StoreId) ?? await _storeContext.GetCurrentStoreAsync();
        languageId = await EnsureLanguageIsActiveAsync(languageId, store.Id);

        #region Prepare multiple email addresses

        var customer = await _customerService.GetCustomerByIdAsync(order.CustomerId);
        var erpNopUser = await _erpCustomerFunctionalityService.GetActiveErpNopUserByCustomerAsync(customer);

        var emailList = new List<(string toEmailAddress, string toName)>
        {
            (customer?.Email, $"{customer?.FirstName} {customer?.LastName}")
        };

        var erpShipToAddress = await _erpShipToAddressService.GetErpShipToAddressByIdAsync(erpNopUser?.ErpShipToAddressId ?? 0);
        if (erpShipToAddress is not null && erpShipToAddress.EmailAddresses is not null)
        {
            var emails = erpShipToAddress.EmailAddresses.Split(';');
            foreach (var email in emails)
            {
                if (!emailList.Exists(x => x.toEmailAddress == email.Trim()))
                    emailList.Add((email.Trim(), $"{erpShipToAddress.ShipToName}"));
            }
        }

        if (!string.IsNullOrWhiteSpace(erpShipToAddress.RepEmail) &&
            !emailList.Exists(x => x.toEmailAddress == erpShipToAddress.RepEmail.Trim()))
        {
            emailList.Add((erpShipToAddress.RepEmail.Trim(), $"{erpShipToAddress.ShipToName}"));
        }

        var billingAddress = await _addressService.GetAddressByIdAsync(order.BillingAddressId);

        if (billingAddress != null && !emailList.Exists(x => x.toEmailAddress == billingAddress.Email.Trim()))
            emailList.Add((billingAddress.Email.Trim(), $"{billingAddress.FirstName} {billingAddress.LastName}"));

        #endregion Prepare multiple email addresses

        var messageTemplates = await GetActiveMessageTemplatesAsync(MessageTemplateSystemNames.ORDER_PLACED_CUSTOMER_NOTIFICATION, store.Id);
        if (!messageTemplates.Any())
            return new List<int>();

        var commonTokens = new List<Token>();
        await _messageTokenProvider.AddOrderTokensAsync(commonTokens, order, languageId);
        await _messageTokenProvider.AddCustomerTokensAsync(commonTokens, order.CustomerId);

        var results = new List<int>();

        foreach (var email in emailList)
        {
            foreach (var messageTemplate in messageTemplates)
            {
                var emailAccount = await GetEmailAccountOfMessageTemplateAsync(messageTemplate, languageId);

                var tokens = new List<Token>(commonTokens);
                await _messageTokenProvider.AddStoreTokensAsync(tokens, store, emailAccount);

                await _eventPublisher.MessageTokensAddedAsync(messageTemplate, tokens);

                var result = await SendNotificationAsync(messageTemplate, emailAccount, languageId, tokens,
                    email.toEmailAddress, email.toName,
                    attachmentFilePath, attachmentFileName);

                results.Add(result);
            }
        }

        return results;
    }

    #endregion Order workflow

    #region ERP Customer Registration Application

    public async Task<int> SendERPCustomerRegistrationApplicationCreatedNotificationAsync(ErpAccountCustomerRegistrationForm applicationForm, int languageId)
    {
        ArgumentNullException.ThrowIfNull(applicationForm);

        var store = await _storeContext.GetCurrentStoreAsync();
        languageId = await EnsureLanguageIsActiveAsync(languageId, store.Id);
        var messageTemplateToAdmin = (await GetActiveMessageTemplatesAsync(B2BB2CFeaturesDefaults.MessageTemplateSystemNames_ERPAccountCustomerRegistrationCreatedNotificationToAdmin, store.Id)).FirstOrDefault();
        var messageTemplateToCustomer = (await GetActiveMessageTemplatesAsync(B2BB2CFeaturesDefaults.MessageTemplateSystemNames_ERPAccountCustomerRegistrationCreatedNotificationToCustomer, store.Id)).FirstOrDefault();

        var commonTokens = new List<Token>();
        await AddApplicationFormTokensAsync(commonTokens, applicationForm);

        var emailAccount = await GetEmailAccountOfMessageTemplateAsync(messageTemplateToAdmin, languageId);
        var tokens = new List<Token>(commonTokens);
        await _messageTokenProvider.AddStoreTokensAsync(tokens, store, emailAccount);
        if (messageTemplateToAdmin is not null)
        {
            await _eventPublisher.MessageTokensAddedAsync(messageTemplateToAdmin, tokens);
            var toEmail = emailAccount.Email;
            var toName = !string.IsNullOrEmpty(emailAccount.DisplayName) ? emailAccount.DisplayName : "Admin";
            await SendNotificationAsync(messageTemplateToAdmin, emailAccount, languageId, tokens, toEmail, toName);
        }
        if (messageTemplateToCustomer is not null)
        {
            await _eventPublisher.MessageTokensAddedAsync(messageTemplateToCustomer, tokens);
            var toEmail = applicationForm.AccountsEmail;
            var toName = applicationForm.FullRegisteredName;
            await SendNotificationAsync(messageTemplateToCustomer, emailAccount, languageId, tokens, toEmail, toName);
        }
        return 0;
    }

    public async Task<int> SendERPCustomerRegistrationApplicationApprovedNotificationAsync(ErpAccountCustomerRegistrationForm applicationForm, int languageId)
    {
        ArgumentNullException.ThrowIfNull(applicationForm);

        var store = await _storeContext.GetCurrentStoreAsync();
        languageId = await EnsureLanguageIsActiveAsync(languageId, store.Id);
        var messageTemplate = (await GetActiveMessageTemplatesAsync(B2BB2CFeaturesDefaults.MessageTemplateSystemNames_ERPAccountCustomerRegistrationApprovedNotification, store.Id)).FirstOrDefault();

        var commonTokens = new List<Token>();
        await AddApplicationFormTokensAsync(commonTokens, applicationForm);

        var emailAccount = await GetEmailAccountOfMessageTemplateAsync(messageTemplate, languageId);
        var tokens = new List<Token>(commonTokens);
        await _messageTokenProvider.AddStoreTokensAsync(tokens, store, emailAccount);
        if (messageTemplate is null)
        {
            return 0;
        }

        await _eventPublisher.MessageTokensAddedAsync(messageTemplate, tokens);
        var toEmail = applicationForm.AccountsEmail;
        var toName = applicationForm.FullRegisteredName;
        return await SendNotificationAsync(messageTemplate, emailAccount, languageId, tokens, toEmail, toName);
    }

    public async Task AddApplicationFormTokensAsync(List<Token> tokens, ErpAccountCustomerRegistrationForm applicationForm)
    {
        tokens.Add(new Token("Application.CustomerFullName", applicationForm.FullRegisteredName));
        tokens.Add(new Token("Application.AdminName", "Admin"));
        tokens.Add(new Token("Application.Id", applicationForm.Id));
        tokens.Add(new Token("Application.RegistrationNumber", applicationForm.RegistrationNumber));
    }

    #endregion

    public virtual async Task<IList<int>> SendOrderOrDeliveryDatesOrShippingCostBAPIFailedMessageAsync(Customer customer, int failedType = 0, int nopOrderId = 0)
    {
        ArgumentNullException.ThrowIfNull(customer);

        if (failedType == 0)
            throw new ArgumentNullException(nameof(failedType));

        var store = await _storeContext.GetCurrentStoreAsync();
        var languageId = await EnsureLanguageIsActiveAsync((await _workContext.GetWorkingLanguageAsync()).Id, store.Id);
        var order = await _orderService.GetOrderByIdAsync(nopOrderId);

        var messageTemplates = await GetActiveMessageTemplatesAsync(B2BB2CFeaturesDefaults.MessageTemplateSystemNames_OrderOrDeliveryDatesOrShippingCostBAPIFailedMessage, store.Id);
        if (!messageTemplates.Any())
            return new List<int>();

        //tokens
        var commonTokens = new List<Token>();
        await AddCustomerTokensAsync(commonTokens, customer);

        if (failedType == (int)ERPFailedTypes.CreateOrderBAPIFails && order != null)
            await AddOrderItemOrShoppingCartItemTokensAsync(commonTokens, failedType, languageId, order);

        if (failedType == (int)ERPFailedTypes.ShippingCostFails || failedType == (int)ERPFailedTypes.DeliveryDateFails)
            await AddOrderItemOrShoppingCartItemTokensAsync(commonTokens, failedType, languageId, customer: customer);

        return await messageTemplates.SelectAwait(async messageTemplate =>
        {
            //email account
            var emailAccount = await GetEmailAccountOfMessageTemplateAsync(messageTemplate, languageId);

            var tokens = new List<Token>(commonTokens);

            var toEmail = emailAccount.Email;
            var toName = emailAccount.DisplayName;

            return await SendNotificationAsync(messageTemplate, emailAccount, languageId, tokens, toEmail, toName);
        }).ToListAsync();
    }

    public virtual async Task AddCustomerTokensAsync(IList<Token> tokens, Customer customer)
    {
        tokens.Add(new Token("Customer.Email", customer.Email));
        tokens.Add(new Token("Customer.Username", customer.Username));
        tokens.Add(new Token("Customer.FullName", await _customerService.GetCustomerFullNameAsync(customer)));
        tokens.Add(new Token("Customer.FirstName", customer.FirstName));
        tokens.Add(new Token("Customer.LastName", customer.LastName));
        tokens.Add(new Token("Customer.Contact", customer.Phone));

        //event notification
        await _eventPublisher.EntityTokensAddedAsync(customer, tokens);
    }

    public virtual async Task AddOrderItemOrShoppingCartItemTokensAsync(IList<Token> tokens, int failedType, int languageId, Order order = null, Customer customer = null)
    {
        if (failedType == (int)ERPFailedTypes.CreateOrderBAPIFails && order != null)
        {
            tokens.Add(new Token("ERPFailed.OrderNumber", order.CustomOrderNumber));
            tokens.Add(new Token("ERPFailed.StoreOwnerNotification.Subject", await _localizationService.GetResourceAsync("Plugins.Payments.B2BCustomerAccount.ERPFailed.StoreOwnerNotification.Subject.CreateOrderBapiFailed", languageId)));
            tokens.Add(new Token("ERPFailed.CurrentProduct(s)", await OrderItemListToHtmlTableAsync(order, languageId), true));
        }

        if ((failedType == (int)ERPFailedTypes.DeliveryDateFails || failedType == (int)ERPFailedTypes.ShippingCostFails) && customer != null)
        {
            if (failedType == (int)ERPFailedTypes.DeliveryDateFails)
                tokens.Add(new Token("ERPFailed.StoreOwnerNotification.Subject", await _localizationService.GetResourceAsync("Plugins.Payments.B2BCustomerAccount.ERPFailed.StoreOwnerNotification.Subject.DeliveryDatesFailed", languageId)));
            else
                tokens.Add(new Token("ERPFailed.StoreOwnerNotification.Subject", await _localizationService.GetResourceAsync("Plugins.Payments.B2BCustomerAccount.ERPFailed.StoreOwnerNotification.Subject.ShippingFailed", languageId)));

            tokens.Add(new Token("ERPFailed.CurrentProduct(s)", ShoppingCartItemListToHtmlTableAsync(customer, languageId), true));
        }
    }

    public async Task SendCombinedLineItemWarningMessageAsync(int orderId, int originalQuoteOrderId, int languageId)
    {
        if (orderId < 0 || originalQuoteOrderId < 0)
            return;

        var quoteOrderItems = (await _orderService.GetOrderItemsAsync(orderId)).GroupBy(x => x.ProductId);
        if (quoteOrderItems != null && quoteOrderItems.Any())
        {
            var hasMultipleLineItem = quoteOrderItems.Any(x => x.Count() > 1);
            if (hasMultipleLineItem)
            {
                var messageTemplates = await GetActiveMessageTemplatesAsync(B2BB2CFeaturesDefaults.MessageTemplateSystemNames_SendCombinedLineItemWarningMessage, (await _storeContext.GetCurrentStoreAsync()).Id);
                if (!messageTemplates.Any())
                    return;

                //tokens
                var commonTokens = new List<Token>();
                await AddOrderAndQuoteOrderItemsTokensAsync(commonTokens, orderId, originalQuoteOrderId, languageId);

                var count = await messageTemplates.SelectAwait(async messageTemplate =>
                {
                    //email account
                    var emailAccount = await GetEmailAccountOfMessageTemplateAsync(messageTemplate, languageId);

                    var tokens = new List<Token>(commonTokens);

                    var toEmail = emailAccount.Email;
                    var toName = emailAccount.DisplayName;

                    return await SendNotificationAsync(messageTemplate, emailAccount, languageId, tokens, toEmail, toName);
                }).ToListAsync();
            }
        }
    }

    public virtual async Task<IList<int>> SendB2CCustomerWelcomeMessageAsync(Customer customer, 
        ErpAccount erpAccount, 
        ErpNopUser erpNopUser, 
        ErpSalesOrg erpSalesOrg, 
        ErpShipToAddress erpShipToAddress, 
        int languageId)
    {
        ArgumentNullException.ThrowIfNull(customer);
        ArgumentNullException.ThrowIfNull(erpAccount);
        ArgumentNullException.ThrowIfNull(erpNopUser);
        ArgumentNullException.ThrowIfNull(erpShipToAddress);
        ArgumentNullException.ThrowIfNull(erpSalesOrg);

        var store = await _storeContext.GetCurrentStoreAsync();
        languageId = await EnsureLanguageIsActiveAsync((await _workContext.GetWorkingLanguageAsync()).Id, store.Id);

        var messageTemplates = await GetActiveMessageTemplatesAsync(B2BB2CFeaturesDefaults.MessageTemplateSystemNames_B2CCustomerWelcomeMessage, store.Id);
        if (!messageTemplates.Any())
            return new List<int>();

        var commonTokens = new List<Token>();
        await _messageTokenProvider.AddCustomerTokensAsync(commonTokens, customer);
        await AddErpAccountTokensAsync(commonTokens, erpAccount);
        await AddErpNopUserTokensAsync(commonTokens, erpNopUser);
        await AddErpShipToAddressTokensAsync(commonTokens, erpShipToAddress, erpSalesOrg, erpNopUser);

        return await messageTemplates.SelectAwait(async messageTemplate =>
        {
            var emailAccount = await GetEmailAccountOfMessageTemplateAsync(messageTemplate, languageId);

            var tokens = new List<Token>(commonTokens);
            await _messageTokenProvider.AddStoreTokensAsync(tokens, store, emailAccount);

            await _eventPublisher.MessageTokensAddedAsync(messageTemplate, tokens);

            var toEmail = customer.Email;
            var toName = await _customerService.GetCustomerFullNameAsync(customer);

            return await SendNotificationAsync(messageTemplate, emailAccount, languageId, tokens, toEmail, toName);
        }).ToListAsync();
    }

    public virtual async Task<IList<int>> SendB2CCustomerEmailVerificationMessageAsync(Customer customer, ErpAccount erpAccount, ErpNopUser erpNopUser, ErpSalesOrg erpSalesOrg, ErpShipToAddress erpShipToAddress, int languageId)
    {
        ArgumentNullException.ThrowIfNull(customer);
        ArgumentNullException.ThrowIfNull(erpAccount);
        ArgumentNullException.ThrowIfNull(erpNopUser);
        ArgumentNullException.ThrowIfNull(erpShipToAddress);
        ArgumentNullException.ThrowIfNull(erpSalesOrg);

        var store = await _storeContext.GetCurrentStoreAsync();
        languageId = await EnsureLanguageIsActiveAsync((await _workContext.GetWorkingLanguageAsync()).Id, store.Id);

        var messageTemplates = await GetActiveMessageTemplatesAsync(B2BB2CFeaturesDefaults.MessageTemplateSystemNames_B2CCustomerEmailVerificationMessage, store.Id);

        if (!messageTemplates.Any())
            return new List<int>();

        var commonTokens = new List<Token>();

        await _messageTokenProvider.AddCustomerTokensAsync(commonTokens, customer);
        await AddErpAccountTokensAsync(commonTokens, erpAccount);
        await AddErpNopUserTokensAsync(commonTokens, erpNopUser);
        await AddErpShipToAddressTokensAsync(commonTokens, erpShipToAddress, erpSalesOrg, erpNopUser);

        return await messageTemplates.SelectAwait(async messageTemplate =>
        {
            var emailAccount = await GetEmailAccountOfMessageTemplateAsync(messageTemplate, languageId);

            var tokens = new List<Token>(commonTokens);
            await _messageTokenProvider.AddStoreTokensAsync(tokens, store, emailAccount);

            await _eventPublisher.MessageTokensAddedAsync(messageTemplate, tokens);

            var toEmail = customer.Email;
            var toName = await _customerService.GetCustomerFullNameAsync(customer);

            return await SendNotificationAsync(messageTemplate, emailAccount, languageId, tokens, toEmail, toName);
        }).ToListAsync();
    }

    public virtual async Task<List<int>> SendErpCustomerRegisteredNotificationMessageAsync(Customer customer, 
        ErpAccount erpAccount, 
        ErpNopUser erpNopUser, 
        ErpSalesOrg erpSalesOrg, 
        ErpShipToAddress erpShipToAddress, 
        int languageId, 
        string email,
        ErpUserRegistrationInfo erpUserRegistrationInfo, 
        bool isB2bUser)
    {
        ArgumentNullException.ThrowIfNull(customer);
        ArgumentNullException.ThrowIfNull(erpSalesOrg);

        var store = await _storeContext.GetCurrentStoreAsync();
        languageId = await EnsureLanguageIsActiveAsync((await _workContext.GetWorkingLanguageAsync()).Id, store.Id);

        IList<MessageTemplate> messageTemplates;

        var commonTokens = new List<Token>();

        if (!isB2bUser)
        {
            ArgumentNullException.ThrowIfNull(erpNopUser);
            ArgumentNullException.ThrowIfNull(erpAccount);
            ArgumentNullException.ThrowIfNull(erpShipToAddress);

            messageTemplates = await GetActiveMessageTemplatesAsync(B2BB2CFeaturesDefaults.MessageTemplateSystemNames_B2CCustomerRegisteredNotification, store.Id);

            if (!messageTemplates.Any())
                return new List<int>();

            await _messageTokenProvider.AddCustomerTokensAsync(commonTokens, customer);
            await AddErpAccountTokensAsync(commonTokens, erpAccount);
            await AddErpNopUserTokensAsync(commonTokens, erpNopUser);
            await AddErpShipToAddressTokensAsync(commonTokens, erpShipToAddress, erpSalesOrg, erpNopUser);
        }
        else
        {
            ArgumentNullException.ThrowIfNull(erpUserRegistrationInfo);

            messageTemplates = await GetActiveMessageTemplatesAsync(B2BB2CFeaturesDefaults.MessageTemplateSystemNames_B2BCustomerRegisteredNotification, store.Id);

            if (!messageTemplates.Any())
                return new List<int>();

            await _messageTokenProvider.AddCustomerTokensAsync(commonTokens, customer);
            await AddCustomerTokensForB2BUserRegistrationAsync(commonTokens, customer);
            await AddB2BUserRegistrationInfoTokensAsync(commonTokens, erpUserRegistrationInfo, erpSalesOrg);
        }

        if (!messageTemplates.Any())
            return new List<int>();

        return await messageTemplates.SelectAwait(async messageTemplate =>
        {
            var emailAccount = await GetEmailAccountOfMessageTemplateAsync(messageTemplate, languageId);

            var tokens = new List<Token>(commonTokens);
            await _messageTokenProvider.AddStoreTokensAsync(tokens, store, emailAccount);

            await _eventPublisher.MessageTokensAddedAsync(messageTemplate, tokens);

            return await SendNotificationAsync(messageTemplate, emailAccount, languageId, tokens, email, erpSalesOrg.Name);
        }).ToListAsync();
    }

    #endregion
}
