using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Core;
using Nop.Core.Caching;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Orders;
using Nop.Services.Catalog;
using Nop.Services.Common;
using Nop.Services.Customers;
using Nop.Services.Directory;
using Nop.Services.Helpers;
using Nop.Services.Localization;
using Nop.Services.Orders;
using Nop.Services.Security;
using Nop.Services.Shipping;
using Nop.Web.Areas.Admin.Factories;
using Nop.Web.Areas.Admin.Models.Common;
using Nop.Web.Framework.Models.Extensions;
using NopStation.Plugin.B2B.B2BB2CFeatures.Areas.Admin.Models;
using NopStation.Plugin.B2B.B2BB2CFeatures.Model;
using NopStation.Plugin.B2B.B2BB2CFeatures.Model.Checkout;
using NopStation.Plugin.B2B.B2BB2CFeatures.Model.ErpAccountPublic;
using NopStation.Plugin.B2B.B2BB2CFeatures.Services.ErpAccountCreditSyncFunctionality;
using NopStation.Plugin.B2B.B2BB2CFeatures.Services.ErpCustomerFunctionality;
using NopStation.Plugin.B2B.B2BB2CFeatures.Services.ExportManager;
using NopStation.Plugin.B2B.ERPIntegrationCore;
using NopStation.Plugin.B2B.ERPIntegrationCore.Domain;
using NopStation.Plugin.B2B.ERPIntegrationCore.Enums;
using NopStation.Plugin.B2B.ERPIntegrationCore.Infrastructure;
using NopStation.Plugin.B2B.ERPIntegrationCore.Model;
using NopStation.Plugin.B2B.ERPIntegrationCore.Services;

namespace NopStation.Plugin.B2B.B2BB2CFeatures.Factories;

public class ErpAccountPublicModelFactory : IErpAccountPublicModelFactory
{
    #region Fields

    private readonly IWorkContext _workContext;
    private readonly IDateTimeHelper _dateTimeHelper;
    private readonly IAddressService _addressService;
    private readonly IPriceFormatter _priceFormatter;
    private readonly ICustomerService _customerService;
    private readonly ICurrencyService _currencyService;
    private readonly IOrderService _orderService;
    private readonly IAddressModelFactory _addressModelFactory;
    private readonly AddressSettings _addressSettings;
    private readonly ILocalizationService _localizationService;
    private readonly IStoreContext _storeContext;
    private readonly IErpAccountService _erpAccountService;
    private readonly IErpSalesOrgService _erpSalesOrgService;
    private readonly IErpInvoiceService _erpInvoiceService;
    private readonly IErpOrderAdditionalDataService _erpOrderAdditionalDataService;
    private readonly IShippingService _shippingService; 
    private readonly IErpWarehouseSalesOrgMapService _erpWarehouseSalesOrgMapService;
    private readonly IErpCustomerFunctionalityService _erpCustomerFunctionalityService;
    private readonly IErpNopUserService _erpNopUserService;
    private readonly B2BB2CFeaturesSettings _b2BB2CFeaturesSettings;
    private readonly ICategoryProductsExportManager _categoryProductsExportManager;
    private readonly ICategoryService _categoryService;
    private readonly IProductService _productService;
    private readonly IErpShipToAddressService _erpShipToAddressService;
    private readonly ICountryService _countryService;
    private readonly IStateProvinceService _stateProvinceService;
    private readonly IPermissionService _permissionService;
    private readonly IStaticCacheManager _staticCacheManager;
    private readonly IErpIntegrationPluginManager _erpIntegrationPluginManager;
    private readonly IErpAccountCreditSyncFunctionality _erpAccountCreditSyncFunctionality;
    private readonly IShoppingCartService _shoppingCartService;
    private readonly IOrderTotalCalculationService _orderTotalCalculationService;
    private readonly IErpLogsService _erpLogsService;

    #endregion

    #region Ctor

    public ErpAccountPublicModelFactory(
        IWorkContext workContext,
        ILocalizationService localizationService,
        IDateTimeHelper dateTimeHelper,
        IAddressService addressService,
        IPriceFormatter priceFormatter,
        ICustomerService customerService,
        ICurrencyService currencyService,
        IOrderService orderService,
        IAddressModelFactory addressModelFactory,
        AddressSettings addressSettings,
        IStoreContext storeContext,
        IErpAccountService erpAccountService,
        IErpSalesOrgService erpSalesOrgService,
        IErpInvoiceService erpInvoiceService,
        IErpOrderAdditionalDataService erpOrderAdditionalDataService,
        IShippingService shippingService,
        IErpWarehouseSalesOrgMapService erpWarehouseSalesOrgMapService,
        IErpCustomerFunctionalityService erpCustomerFunctionalityService,
        IErpNopUserService erpNopUserService,
        B2BB2CFeaturesSettings b2BB2CFeaturesSettings,
        ICategoryProductsExportManager categoryProductsExportManager,
        ICategoryService categoryService,
        IProductService productService,
        IErpShipToAddressService erpShipToAddressService,
        ICountryService countryService,
        IStateProvinceService stateProvinceService,
        IPermissionService permissionService,
        IStaticCacheManager staticCacheManager,
        IErpIntegrationPluginManager erpIntegrationPluginManager,
        IErpAccountCreditSyncFunctionality erpAccountCreditSyncFunctionality,
        IShoppingCartService shoppingCartService,
        IOrderTotalCalculationService orderTotalCalculationService,
        IErpLogsService erpLogsService
        )
    {
        _workContext = workContext;
        _localizationService = localizationService;
        _dateTimeHelper = dateTimeHelper;
        _addressService = addressService;
        _priceFormatter = priceFormatter;
        _customerService = customerService;
        _currencyService = currencyService;
        _orderService = orderService;
        _addressModelFactory = addressModelFactory;
        _addressSettings = addressSettings;
        _storeContext = storeContext;
        _erpAccountService = erpAccountService;
        _erpSalesOrgService = erpSalesOrgService;
        _erpInvoiceService = erpInvoiceService;
        _erpOrderAdditionalDataService = erpOrderAdditionalDataService;
        _shippingService = shippingService; 
        _erpWarehouseSalesOrgMapService = erpWarehouseSalesOrgMapService;
        _erpCustomerFunctionalityService = erpCustomerFunctionalityService;
        _erpNopUserService = erpNopUserService;
        _b2BB2CFeaturesSettings = b2BB2CFeaturesSettings;
        _categoryProductsExportManager = categoryProductsExportManager;
        _categoryService = categoryService;
        _productService = productService;
        _erpShipToAddressService = erpShipToAddressService;
        _countryService = countryService;
        _stateProvinceService = stateProvinceService;
        _permissionService = permissionService;
        _staticCacheManager = staticCacheManager;
        _erpIntegrationPluginManager = erpIntegrationPluginManager;
        _erpAccountCreditSyncFunctionality = erpAccountCreditSyncFunctionality;
        _shoppingCartService = shoppingCartService;
        _orderTotalCalculationService = orderTotalCalculationService;
        _erpLogsService = erpLogsService;
    }

    #endregion

    #region Utilities

    protected virtual void SetAddressFieldsAsRequired(AddressModel model)
    {
        model.FirstNameRequired = true;
        model.LastNameRequired = true;
        model.EmailRequired = true;
        model.CompanyRequired = _addressSettings.CompanyRequired;
        model.CountyRequired = _addressSettings.CountyRequired;
        model.CityRequired = _addressSettings.CityRequired;
        model.StreetAddressRequired = _addressSettings.StreetAddressRequired;
        model.StreetAddress2Required = _addressSettings.StreetAddress2Required;
        model.ZipPostalCodeRequired = _addressSettings.ZipPostalCodeRequired;
        model.PhoneRequired = _addressSettings.PhoneRequired;
        model.FaxRequired = _addressSettings.FaxRequired;
    }

    protected async Task<decimal> GetErpCustomerAccountSavingsForCurrentYear(ErpAccount erpAccount)
    {
        if (
            erpAccount.TotalSavingsForthisYearUpdatedOnUtc.HasValue
            && erpAccount.TotalSavingsForthisYearUpdatedOnUtc.Value.Date == DateTime.UtcNow.Date
            && erpAccount.TotalSavingsForthisYear.HasValue
        )
            return erpAccount.TotalSavingsForthisYear.Value;

        var totalSavings = await GetB2BCurrentCustomerAccountSavingsForthisYearAsync(erpAccount);
        return totalSavings ?? decimal.Zero;
    }

    protected async Task<decimal> GetCustomerAccountSavingsForAllTime(ErpAccount erpAccount)
    {
        if (
            erpAccount.TotalSavingsForAllTimeUpdatedOnUtc.HasValue
            && erpAccount.TotalSavingsForAllTimeUpdatedOnUtc.Value.Date == DateTime.UtcNow.Date
            && erpAccount.TotalSavingsForAllTime.HasValue
        )
            return erpAccount.TotalSavingsForAllTime.Value;

        var totalSavings = await GetB2BCurrentCustomerAccountSavingsForAllTimeAsync(erpAccount);
        return totalSavings ?? decimal.Zero;
    }

    private async Task<bool> HasB2BQuoteAssistantRole()
    {
        var customer = await _workContext.GetCurrentCustomerAsync();
        var customerRoles = await _customerService.GetCustomerRolesAsync(customer);
        return customerRoles.Any(x =>
            x.SystemName.Equals(ERPIntegrationCoreDefaults.B2BQuoteAssistantRoleSystemName)
        );
    }

    private async Task<bool> HasB2BOrderAssistantRole()
    {
        var customer = await _workContext.GetCurrentCustomerAsync();
        var customerRoles = await _customerService.GetCustomerRolesAsync(customer);
        return customerRoles.Any(x =>
            x.SystemName.Equals(ERPIntegrationCoreDefaults.B2BOrderAssistantRoleSystemName)
        );
    }

    #endregion

    #region Method

    public async Task<ErpAccountPublicSearchModel> PrepareErpAccountSearchModelAsync(
        ErpAccountPublicSearchModel searchModel
    )
    {
        ArgumentNullException.ThrowIfNull(searchModel);

        //prepare grid
        searchModel.SetGridPageSize();

        return searchModel;
    }

    public async Task<ErpAccountPublicListModel> PrepareErpAccountListModelAsync(
        ErpAccountPublicSearchModel searchModel
    )
    {
        ArgumentNullException.ThrowIfNull(searchModel);

        var erpAccounts = await _erpAccountService.GetAllErpAccountsAsync(
            pageIndex: searchModel.Page - 1,
            pageSize: searchModel.PageSize,
            showHidden: false
        );

        var model = await new ErpAccountPublicListModel().PrepareToGridAsync(
            searchModel,
            erpAccounts,
            () =>
            {
                return erpAccounts.SelectAwait(async erpAccount =>
                {
                    var erpAccountSalesOrgInfo = await _erpSalesOrgService.GetErpSalesOrgByIdAsync(
                        erpAccount.ErpSalesOrgId
                    );

                    var erpAccountModel = new ErpAccountPublicModel
                    {
                        Id = erpAccount.Id,
                        AccountNumber = erpAccount.AccountNumber,
                        AccountName = erpAccount.AccountName,
                        VatNumber = erpAccount.VatNumber,
                        CurrentBalance = erpAccount.CurrentBalance,
                    };

                    if (erpAccountSalesOrgInfo != null)
                    {
                        var address = await _addressService.GetAddressByIdAsync(
                            erpAccountSalesOrgInfo.AddressId
                        );
                        var addressModel = new AddressModel();
                        await _addressModelFactory.PrepareAddressModelAsync(addressModel, address);
                        var erpAccountSalesOrgModel = new ErpSalesOrgModel
                        {
                            Name = erpAccountSalesOrgInfo.Name,
                            Code = erpAccountSalesOrgInfo.Code,
                            Email = erpAccountSalesOrgInfo.Email,
                            Address = addressModel,
                            IntegrationClientId = erpAccountSalesOrgInfo.IntegrationClientId,
                            AuthenticationKey = erpAccountSalesOrgInfo.AuthenticationKey,
                        };

                        erpAccountModel.ErpSalesOrgModel = erpAccountSalesOrgModel;
                    }

                    return erpAccountModel;
                });
            }
        );

        return model;
    }
    public async Task<RecentTransactionListModel> PrepareRecentTransactionListAsync(
        RecentTransactionSearchModel transactionSearchModel
    )
    {
        var currCustomer = await _workContext.GetCurrentCustomerAsync();
        var currencyTmp = await _currencyService.GetCurrencyByIdAsync(currCustomer.CurrencyId ?? 0);
        var customerCurrency =
            currencyTmp != null && currencyTmp.Published
                ? currencyTmp
                : await _workContext.GetWorkingCurrencyAsync();
        var customerCurrencyCode = customerCurrency.CurrencyCode;

        IPagedList<ErpInvoice> erpInvoices = null;

        var erpNopUser = await _erpNopUserService.GetErpNopUserByCustomerIdAsync(currCustomer.Id, showHidden: false);
        var customerId = (erpNopUser != null && erpNopUser.ErpUserType == ErpUserType.B2CUser) ? currCustomer.Id : 0;

        if (transactionSearchModel.ErpAccountId > 0)
        {
            erpInvoices = await _erpInvoiceService.GetAllErpInvoiceAsync(
                postingFromDateUtc: transactionSearchModel.SearchTransactionDate,
                postingToDateUtc: transactionSearchModel.SearchTransactionDate,
                getOnlyTotalCount: false,
                erpAccountId: transactionSearchModel.ErpAccountId,
                customerId: customerId,
                erpDocumentNumber: transactionSearchModel.SearchDocumentNumberOrName,
                erpOrderNumber: transactionSearchModel.ErpOrderNumber,
                sortBy: (FinancialDocumentSortingEnum)transactionSearchModel.SearchSortOptionId,
                pageIndex: transactionSearchModel.Page - 1,
                pageSize: transactionSearchModel.PageSize
            );
        }
        async IAsyncEnumerable<RecentTransactionModel> GetTransactionModelsAsync()
        {
            var lang = await _workContext.GetWorkingLanguageAsync();

            foreach (var transaction in erpInvoices)
            {
                var isInvoice = !string.IsNullOrEmpty(transaction.DocumentDisplayName) &&
                                transaction.DocumentDisplayName == "Invoice";
                var isDownloadable = transaction.DocumentTypeId == (int)ErpDocumentType.Invoice;

                yield return new RecentTransactionModel
                {
                    Id = transaction.Id,
                    PostingDate = await _dateTimeHelper.ConvertToUserTimeAsync(transaction.PostingDateUtc, DateTimeKind.Utc),
                    DocumentDisplayName = transaction.DocumentDisplayName,
                    DocumentNo = transaction.ErpDocumentNumber,
                    Status = "",
                    Remaining = decimal.Zero,
                    AmountExVat = await _priceFormatter.FormatPriceAsync(
                        transaction.AmountExclVat,
                        true,
                        customerCurrencyCode,
                        lang.Id,
                        true
                    ),
                    IsDocumentTypeInvoice = isInvoice,
                    IsDocumentTypeDownloadable = isDownloadable,
                    ErpOrderNumber = transaction.ErpOrderNumber
                };
            }
        }

        var model = await new RecentTransactionListModel()
            .PrepareToGridAsync<RecentTransactionListModel, RecentTransactionModel, ErpInvoice>(
                transactionSearchModel,
                erpInvoices,
                () => GetTransactionModelsAsync()
            );

        return model;
    }
    public async Task<RecentTransactionListModel> PrepareRecentTransactionListAsync(
        ErpAccountInfoModel erpAccountInfoModel
    )
    {
        var currCustomer = await _workContext.GetCurrentCustomerAsync();
        var currencyTmp = await _currencyService.GetCurrencyByIdAsync(currCustomer.CurrencyId ?? 0);
        var customerCurrency =
            currencyTmp != null && currencyTmp.Published
                ? currencyTmp
                : await _workContext.GetWorkingCurrencyAsync();
        var customerCurrencyCode = customerCurrency.CurrencyCode;

        var transactionFromDate = erpAccountInfoModel.SearchTransactionFromDate;
        var transactionToDate = erpAccountInfoModel.SearchTransactionToDate;

        if (erpAccountInfoModel.SearchTransactionFromDate.HasValue)
        {
            transactionFromDate = erpAccountInfoModel.SearchTransactionFromDate.Value;
        }

        if (erpAccountInfoModel.SearchTransactionToDate.HasValue)
        {
            transactionToDate = erpAccountInfoModel
                .SearchTransactionToDate.Value.AddHours(23)
                .AddMinutes(59)
                .AddSeconds(59);
        }

        IPagedList<ErpInvoice> transactionPerAccounts = null;

        if (erpAccountInfoModel.ErpAccountId > 0)
        {
            var erpNopUser = await _erpNopUserService.GetErpNopUserByCustomerIdAsync(currCustomer.Id, showHidden: false);
            var customerId = (erpNopUser != null && erpNopUser.ErpUserType == ErpUserType.B2CUser) ? currCustomer.Id : 0;

            transactionPerAccounts = await _erpInvoiceService.GetAllErpInvoiceAsync(
                pageIndex: erpAccountInfoModel.Page - 1,
                pageSize: erpAccountInfoModel.PageSize,
                getOnlyTotalCount: false,
                erpAccountId: erpAccountInfoModel.ErpAccountId,
                customerId: customerId,
                erpDocumentNumber: erpAccountInfoModel.SearchDocumentNumberOrName,
                customOrderNumber: erpAccountInfoModel.SearchOrderNumberOrName,
                postingFromDateUtc: transactionFromDate,
                postingToDateUtc: transactionToDate,
                sortBy: (FinancialDocumentSortingEnum)erpAccountInfoModel.SearchSortOptionId
            );
        }
        var language = await _workContext.GetWorkingLanguageAsync();
        var model = await new RecentTransactionListModel().PrepareToGridAsync(
            erpAccountInfoModel,
            transactionPerAccounts,
            () =>
            {
                return transactionPerAccounts.SelectAwait(async transaction =>
                {
                    var isDocumentTypeInvoice =
                        !string.IsNullOrEmpty(transaction.DocumentDisplayName)
                        && transaction.DocumentDisplayName == "Invoice";
                    var isDocumentTypeDownloadable =
                        transaction.DocumentTypeId == (int)ErpDocumentType.Invoice;
                    var nopOrder =
                        await _erpOrderAdditionalDataService.GetNopOrderByErpOrderNumberAsync(
                            transaction.ErpOrderNumber
                        );

                    var erpOrderAdditionalData = await _erpOrderAdditionalDataService.GetErpOrderAdditionalDataByErpAccountIdAndErpOrderNumberAsync(transaction.ErpAccountId, transaction.ErpOrderNumber);

                    var accountModel = new RecentTransactionModel
                    {
                        Id = transaction.Id,
                        PostingDate = await _dateTimeHelper.ConvertToUserTimeAsync(
                            transaction.PostingDateUtc,
                            DateTimeKind.Utc
                        ),
                        DocumentDisplayName = transaction.DocumentDisplayName,
                        DocumentNo = transaction.ErpDocumentNumber,
                        Status = "",
                        Remaining = decimal.Zero,
                        AmountExVat = await _priceFormatter.FormatPriceAsync(
                            transaction.AmountExclVat,
                            true,
                            customerCurrencyCode,
                            language.Id,
                            true
                        ),
                        IsDocumentTypeInvoice = isDocumentTypeInvoice,
                        IsDocumentTypeDownloadable = isDocumentTypeDownloadable,
                        ErpOrderNumber = transaction.ErpOrderNumber,
                        CustomerOrder = erpOrderAdditionalData?.CustomerReference ?? string.Empty,
                        NopOrderId = nopOrder?.Id ?? 0,
                    };
                    return accountModel;
                });
            }
        );
        return model;
    }

    public async Task<ErpAccountInfoModel> PrepareErpAccountInfoModelAsync(
        ErpAccount b2BAccount,
        ErpAccountInfoModel model,
        bool enableErpAccountUpdate = false
    )
    {
        ArgumentNullException.ThrowIfNull(b2BAccount);
        ArgumentNullException.ThrowIfNull(model);

        var currCustomer = await _workContext.GetCurrentCustomerAsync();

        //customer currency
        var currencyTmp = await _currencyService.GetCurrencyByIdAsync(currCustomer.Id);
        var customerCurrency =
            currencyTmp != null && currencyTmp.Published
                ? currencyTmp
                : await _workContext.GetWorkingCurrencyAsync();
        var customerCurrencyCode = customerCurrency.CurrencyCode;

        // ERP Account Update
        if (_b2BB2CFeaturesSettings.EnableLiveCreditChecks)
            await _erpAccountCreditSyncFunctionality.LiveErpAccountCreditCheckAsync(b2BAccount);

        model.ErpAccountId = b2BAccount.Id;
        model.AccountNumber = b2BAccount.AccountNumber;
        model.AccountName = b2BAccount.AccountName;
        model.HasErpQuoteAssistantRole = await HasB2BQuoteAssistantRole();
        model.HasErpOrderAssistantRole = await HasB2BOrderAssistantRole();
        await PrepareTransactionSortingOptionsAsync(
            model.AvailableSortOptions
        );

        var availableBlanace = b2BAccount.CreditLimitAvailable;

        if (!model.HasErpQuoteAssistantRole && !model.HasErpOrderAssistantRole)
        {
            model.IsShowYearlySavings = _b2BB2CFeaturesSettings.IsShowYearlySavings;
            model.IsShowAllTimeSavings = _b2BB2CFeaturesSettings.IsShowAllTimeSavings;
            model.IsShowAccountStatementDownloadEnabled =
                _b2BB2CFeaturesSettings.EnableAccountStatementDownload;

            var currentYearSavings = string.Empty;
            var allTimeSavings = string.Empty;

            if (await _permissionService.AuthorizeAsync(ErpPermissionProvider.DisplayB2BAccountCreditInfo))
            {
                var language = await _workContext.GetWorkingLanguageAsync();
                if (model.IsShowYearlySavings)
                {
                    currentYearSavings = await _priceFormatter.FormatPriceAsync(
                        await GetErpCustomerAccountSavingsForCurrentYear(b2BAccount),
                        true,
                        customerCurrencyCode,
                        language.Id,
                        true
                    );
                }
                if (model.IsShowAllTimeSavings)
                {
                    allTimeSavings = await _priceFormatter.FormatPriceAsync(
                        await GetCustomerAccountSavingsForAllTime(b2BAccount),
                        true,
                        customerCurrencyCode,
                        language.Id,
                        true
                    );
                }

                model.CurrentYearOnlineSavings = currentYearSavings;
                model.AllTimeOnlineSavings = allTimeSavings;
                model.CreditLimit = await _priceFormatter.FormatPriceAsync(
                    b2BAccount.CreditLimit,
                    true,
                    customerCurrencyCode,
                    language.Id,
                    true
                );
                model.CurrentBalance = await _priceFormatter.FormatPriceAsync(
                    b2BAccount.CurrentBalance,
                    true,
                    customerCurrencyCode,
                    language.Id,
                    true
                );
                model.AvailableCredit = await _priceFormatter.FormatPriceAsync(
                    availableBlanace,
                    true,
                    customerCurrencyCode,
                    language.Id,
                    true
                );
                model.LastPaymentAmount = b2BAccount.LastPaymentAmount.HasValue
                    ? await _priceFormatter.FormatPriceAsync(
                        b2BAccount.LastPaymentAmount.Value,
                        true,
                        customerCurrencyCode,
                        language.Id,
                        true
                    )
                    : string.Empty;
                model.LastPaymentDate = b2BAccount.LastPaymentDate.HasValue
                    ? b2BAccount.LastPaymentDate.Value.ToShortDateString()
                    : string.Empty;
            }
        }

        //prepare page parameters
        if (!enableErpAccountUpdate)
        {
            model.SetGridPageSize();
        }

        return model;
    }

    public async Task<ErpAccountOrderSearchModel> PrepareErpAccountOrderSearchModelAsync(
        ErpAccount erpAccount,
        ErpNopUser erpNopUser,
        ErpAccountOrderSearchModel model
    )
    {
        ArgumentNullException.ThrowIfNull(erpNopUser);
        ArgumentNullException.ThrowIfNull(erpAccount);
        ArgumentNullException.ThrowIfNull(model);

        model.ErpAccountId = erpAccount.Id;
        model.ErpAccountNumber = erpAccount.AccountNumber;
        model.ErpNopUserId = erpNopUser.Id;

        //prepare page parameters
        model.SetGridPageSize();

        return model;
    }

    public async Task<ErpAccountOrderListModel> PrepareErpOrderListModelAsync(
        ErpAccountOrderSearchModel searchModel
    )
    {
        var orderPlacedOnDateFrom = searchModel.SearchOrderDateFrom;
        var orderPlacedOnDateTo = searchModel.SearchOrderDateTo;

        if (searchModel.SearchOrderDateFrom.HasValue)
        {
            orderPlacedOnDateFrom = searchModel.SearchOrderDateFrom.Value;
        }

        if (searchModel.SearchOrderDateTo.HasValue)
        {
            orderPlacedOnDateTo = searchModel
                .SearchOrderDateTo.Value
                .AddHours(23)
                .AddMinutes(59)
                .AddSeconds(59);
        }

        var currCustomer = await _workContext.GetCurrentCustomerAsync();
        var erpNopUser = await _erpNopUserService.GetErpNopUserByCustomerIdAsync(
            currCustomer.Id,
            showHidden: false
        );
        var erpOrderTypeId = 0;
        if (erpNopUser != null && erpNopUser.ErpUserType == ErpUserType.B2BUser)
        {
            erpOrderTypeId = (int)ErpOrderType.B2BSalesOrder;
        }
        if (erpNopUser != null && erpNopUser.ErpUserType == ErpUserType.B2CUser)
        {
            erpOrderTypeId = (int)ErpOrderType.B2CSalesOrder;
        }

        if (erpOrderTypeId == 0)
        {
            return new ErpAccountOrderListModel();
        }

        var currencyTmp = await _currencyService.GetCurrencyByIdAsync(currCustomer.CurrencyId ?? 0);
        var customerCurrency =
            currencyTmp != null && currencyTmp.Published
                ? currencyTmp
                : await _workContext.GetWorkingCurrencyAsync();
        var language = await _workContext.GetWorkingLanguageAsync();

        var erpOrderPerUsers =
            await _erpOrderAdditionalDataService.GetAllErpOrderAdditionalDataAsync(
                pageIndex: searchModel.Page - 1,
                pageSize: searchModel.PageSize,
                accountId: searchModel.ErpAccountId,
                nopCustomerId: erpNopUser.ErpUserType == ErpUserType.B2CUser ? searchModel.NopCustomerId : 0,
                erpOrderNumber: searchModel.SearchOrderNumberOrName,
                erpOrderTypeId: erpOrderTypeId,
                searchOrderDateFrom: orderPlacedOnDateFrom,
                searchOrderDateTo: orderPlacedOnDateTo
            );
        var model = await new ErpAccountOrderListModel().PrepareToGridAsync(
            searchModel,
            erpOrderPerUsers,
            () =>
            {
                return erpOrderPerUsers.SelectAwait(async erpOrderAdditionalData =>
                {
                    var nopOrder = await _orderService.GetOrderByIdAsync(
                        erpOrderAdditionalData.NopOrderId
                    );
                    var orderItems = await _orderService.GetOrderItemsAsync(nopOrder?.Id ?? 0);
                    var totalOrderItems = orderItems?.Sum(x => x.Quantity) ?? 0;

                    (var invoiceCount, var totalShipped) =
                        _erpInvoiceService.GetTotalNumberOfInvoicesAndNumOfShippedItemsByErpAccountIdERPOrderNumber(
                            searchModel.ErpAccountId,
                            erpOrderAdditionalData.ErpOrderNumber
                        );
                    var erpAccount = await _erpAccountService.GetErpAccountByIdAsync(
                        erpOrderAdditionalData.ErpAccountId
                    );
                    var salesOrg = await _erpSalesOrgService.GetErpSalesOrgByIdAsync(
                        erpAccount?.ErpSalesOrgId ?? 0
                    );

                    var isInvoiceOrPodAvailable =
                        await _erpInvoiceService.CheckIsInvoiceOrPodAvailableByErpAccountIdERPOrderNumberAsync(
                            searchModel.ErpAccountId,
                            erpOrderAdditionalData.ErpOrderNumber
                        );
                    var b2BAccountOrder = new ErpAccountOrderDetailsModel
                    {
                        ErpOrderOriginType = await _localizationService.GetLocalizedEnumAsync(
                            erpOrderAdditionalData.ErpOrderOriginType
                        ),
                        ErpOrderNumber = string.IsNullOrEmpty(erpOrderAdditionalData.ErpOrderNumber)
                            ? nopOrder?.CustomOrderNumber
                            : erpOrderAdditionalData.ErpOrderNumber,
                        CustomerOrder = erpOrderAdditionalData.CustomerReference,
                        PaygateReferenceNumber = string.Empty,
                        ErpAccountSalesOrgId = salesOrg?.Id ?? 0,
                        ERPOrderStatus = erpOrderAdditionalData.ERPOrderStatus,
                        ExpectedDelivery = erpOrderAdditionalData.DeliveryDate,
                        WarehouseName = (erpNopUser.ErpUserType == ErpUserType.B2CUser ?
                                (
                                    await _shippingService.GetWarehouseByIdAsync(
                                        salesOrg?.TradingWarehouseId ?? 0
                                    )
                                )?.Name : ""
                            ) ?? "",
                        Invoices = invoiceCount,
                        CustomerReference = erpOrderAdditionalData.CustomerReference,
                        SpecialInstructions = erpOrderAdditionalData.SpecialInstructions,
                        IsInvoiceOrPodAvailable = isInvoiceOrPodAvailable,
                    };

                    if (nopOrder != null)
                    {
                        b2BAccountOrder.NopOrderId = nopOrder.Id;
                        b2BAccountOrder.NopOrderNumber = nopOrder.CustomOrderNumber;

                        b2BAccountOrder.PlacedByCustomer = erpOrderAdditionalData.ErpOrderOriginType == ErpOrderOriginType.StandardOrder
                            ? erpAccount?.AccountName
                            : (await _customerService.GetCustomerByIdAsync(nopOrder.CustomerId))?.Email;

                        b2BAccountOrder.TotalOrderItems = totalOrderItems;
                        b2BAccountOrder.Unshipped = int.Max(totalOrderItems - totalShipped, 0);
                        b2BAccountOrder.OrderPlacedOn =
                            await _dateTimeHelper.ConvertToUserTimeAsync(
                                nopOrder.CreatedOnUtc,
                                DateTimeKind.Utc
                            );
                        b2BAccountOrder.OrderTotalAmount = await _priceFormatter.FormatPriceAsync(
                            nopOrder.OrderTotal,
                            true,
                            customerCurrency?.CurrencyCode ?? "",
                            language?.Id ?? 0,
                            true
                        );
                    }

                    return b2BAccountOrder;
                });
            }
        );

        return model;
    }

    public async Task<ErpAccountQuoteOrderSearchModel> PrepareErpAccountQuoteOrderSearchModelAsync(
        ErpAccount erpAccount,
        ErpNopUser erpNopUser,
        ErpAccountQuoteOrderSearchModel searchModel
    )
    {
        ArgumentNullException.ThrowIfNull(erpNopUser);
        ArgumentNullException.ThrowIfNull(erpAccount);
        ArgumentNullException.ThrowIfNull(searchModel);

        searchModel.ErpAccountId = erpAccount.Id;
        searchModel.ErpAccountNumber = erpAccount.AccountNumber;
        searchModel.ErpNopUserId = erpNopUser.Id;

        searchModel.SetGridPageSize();
        return searchModel;
    }

    public async Task<ErpQuoteOrderListModel> PrepareErpQuoteOrderListModelAsync(
        ErpAccountQuoteOrderSearchModel searchModel
    )
    {
        var orderPlacedOnDateFrom = searchModel.SearchOrderDateFrom;
        var orderPlacedOnDateTo = searchModel.SearchOrderDateTo;

        if (searchModel.SearchOrderDateFrom.HasValue)
        {
            orderPlacedOnDateFrom = searchModel.SearchOrderDateFrom.Value;
        }

        if (searchModel.SearchOrderDateTo.HasValue)
        {
            orderPlacedOnDateTo = searchModel
                .SearchOrderDateTo.Value.AddHours(23)
                .AddMinutes(59)
                .AddSeconds(59);
        }

        var currCustomer = await _workContext.GetCurrentCustomerAsync();
        var erpNopUser = await _erpNopUserService.GetErpNopUserByCustomerIdAsync(
            currCustomer.Id,
            showHidden: false
        );

        var erpOrderTypeId = 0;
        if (erpNopUser != null && erpNopUser.ErpUserType == ErpUserType.B2BUser)
        {
            erpOrderTypeId = (int)ErpOrderType.B2BQuote;
        }
        if (erpNopUser != null && erpNopUser.ErpUserType == ErpUserType.B2CUser)
        {
            erpOrderTypeId = (int)ErpOrderType.B2CQuote;
        }

        if (erpOrderTypeId == 0)
        {
            return new ErpQuoteOrderListModel();
        }

        var erpQuoteOrders = await _erpOrderAdditionalDataService.GetAllErpOrderAdditionalDataAsync(
            accountId: searchModel.ErpAccountId,
            erpOrderNumber: searchModel.SearchQuoteNumberOrName,
            erpOrderTypeId: erpOrderTypeId,
            nopCustomerId: searchModel.NopCustomerId,
            pageIndex: searchModel.Page - 1,
            pageSize: searchModel.PageSize,
            searchOrderDateTo: orderPlacedOnDateTo,
            searchOrderDateFrom: orderPlacedOnDateFrom
        );

        var currencyTmp = await _currencyService.GetCurrencyByIdAsync(currCustomer.CurrencyId ?? 0);
        var customerCurrency =
            currencyTmp != null && currencyTmp.Published
                ? currencyTmp
                : await _workContext.GetWorkingCurrencyAsync();
        var language = await _workContext.GetWorkingLanguageAsync();

        //prepare list model
        var model = await new ErpQuoteOrderListModel().PrepareToGridAsync(
            searchModel,
            erpQuoteOrders,
            () =>
            {
                return erpQuoteOrders.SelectAwait(async erpOrderAdditionalData =>
                {
                    var nopOrder = await _orderService.GetOrderByIdAsync(
                        erpOrderAdditionalData.NopOrderId
                    );
                    var erpAccount = await _erpAccountService.GetErpAccountByIdAsync(
                        erpOrderAdditionalData.ErpAccountId
                    );
                    var salesOrg = await _erpSalesOrgService.GetErpSalesOrgByIdAsync(
                        erpAccount?.ErpSalesOrgId ?? 0
                    );

                    var quoteOrderModel = new ErpQuoteOrderModel
                    {
                        ErpOrderOriginType = await _localizationService.GetLocalizedEnumAsync(
                            erpOrderAdditionalData.ErpOrderOriginType
                        ),
                        QuoteNumber = string.IsNullOrEmpty(erpOrderAdditionalData.ErpOrderNumber)
                            ? nopOrder?.CustomOrderNumber
                            : erpOrderAdditionalData.ErpOrderNumber,
                        ErpAccountSalesOrganisationId = salesOrg?.Id ?? 0,
                        ERPOrderStatus = erpOrderAdditionalData.ERPOrderStatus,
                        WarehouseName = (erpNopUser.ErpUserType == ErpUserType.B2CUser ?
                                (
                                    await _shippingService.GetWarehouseByIdAsync(
                                        salesOrg?.TradingWarehouseId ?? 0
                                    )
                                )?.Name : ""
                            ) ?? "",
                        CustomerOrder = erpOrderAdditionalData.CustomerReference,
                    };

                    if (erpOrderAdditionalData.QuoteExpiryDate.HasValue)
                    {
                        quoteOrderModel.ExpiryDate = await _dateTimeHelper.ConvertToUserTimeAsync(
                            erpOrderAdditionalData.QuoteExpiryDate.Value,
                            DateTimeKind.Utc
                        );
                        quoteOrderModel.IsQuoteExpired =
                            erpOrderAdditionalData.QuoteExpiryDate.Value.Date
                            < DateTime.UtcNow.Date;
                        quoteOrderModel.IsQuoteActive =
                            await _erpCustomerFunctionalityService.CheckQuoteOrderStatusAsync(
                                erpOrderAdditionalData
                            );
                        quoteOrderModel.IsQuoteConvertedToOrder =
                            erpOrderAdditionalData.QuoteSalesOrderId.HasValue
                            && erpOrderAdditionalData.QuoteSalesOrderId.Value > 0;
                    }

                    if (nopOrder != null)
                    {
                        quoteOrderModel.NopOrderId = nopOrder.Id;
                        quoteOrderModel.PlacedByCustomerEmail = erpOrderAdditionalData.ErpOrderOriginType == ErpOrderOriginType.StandardOrder
                            ? erpAccount?.AccountName
                            : (await _customerService.GetCustomerByIdAsync(nopOrder.CustomerId))?.Email;

                        quoteOrderModel.QuoteDate = await _dateTimeHelper.ConvertToUserTimeAsync(
                            nopOrder.CreatedOnUtc,
                            DateTimeKind.Utc
                        );
                        var orderItems = await _orderService.GetOrderItemsAsync(nopOrder.Id);
                        if (orderItems.Any(a => a.PriceExclTax == _b2BB2CFeaturesSettings.ProductQuotePrice))
                        {
                            quoteOrderModel.TotalAmount = await _localizationService.GetResourceAsync("Products.ProductForQuote");
                            quoteOrderModel.CustomProperties.Add("ProductForQuote", "true");
                        }
                        else
                            quoteOrderModel.TotalAmount = await _priceFormatter.FormatPriceAsync(
                                nopOrder.OrderTotal,
                                true,
                                customerCurrency?.CurrencyCode ?? "",
                                language?.Id ?? 0,
                                true
                            );
                    }

                    return quoteOrderModel;
                });
            }
        );


        return model;
    }

    public async Task PrepareTransactionSortingOptionsAsync(IList<SelectListItem> items)
    {
        var activeSortingOptionsIds = Enum.GetValues(typeof(FinancialDocumentSortingEnum))
            .Cast<int>()
            .ToList();
        var orderedActiveSortingOptions = activeSortingOptionsIds
            .Select(id => new { Id = id, Order = id })
            .OrderBy(option => option.Order)
            .ToList();

        foreach (var option in orderedActiveSortingOptions)
        {
            items.Add(
                new SelectListItem
                {
                    Text = await _localizationService.GetLocalizedEnumAsync(
                        (FinancialDocumentSortingEnum)option.Id
                    ),
                    Value = $"{option.Id}",
                }
            );
        }
    }

    public async Task<byte[]> ExportB2BAccountProductsToXlsxAsync(int categoryId = 0)
    {
        var customer = await _workContext.GetCurrentCustomerAsync();

        if (!await _customerService.IsRegisteredAsync(customer))
            return null;

        var erpAccount = await _erpCustomerFunctionalityService.GetActiveErpAccountByCustomerAsync(customer);
        if (erpAccount == null)
            return null;

        var categoryIdList = new List<int>();
        if (categoryId > 0 && _b2BB2CFeaturesSettings.IsCategoryPriceListAppliedToSubCategories)
        {
            categoryIdList.Add(categoryId);
            categoryIdList.AddRange(
                await _categoryService.GetChildCategoryIdsAsync(
                    categoryId,
                    (await _storeContext.GetCurrentStoreAsync()).Id,
                    showHidden: true
                )
            );
        }

        var products = await _productService.SearchProductsAsync(
            storeId: (await _storeContext.GetCurrentStoreAsync()).Id,
            categoryIds: categoryIdList
        );

        return await _categoryProductsExportManager.ExportProductsToXlsxAsync(products);
    }

    public async Task<byte[]> ExportB2BOrderPerAccountProductsToXlsxAsync()
    {
        var customer = await _workContext.GetCurrentCustomerAsync();

        if (!await _customerService.IsRegisteredAsync(customer))
            return null;

        var erpAccount = await _erpCustomerFunctionalityService.GetActiveErpAccountByCustomerAsync(customer);
        if (erpAccount == null)
            return null;
        var products = await _erpOrderAdditionalDataService.FindOrderProductsAsync(
            erpAccount,
            _b2BB2CFeaturesSettings.LastNOrdersPerAccount
        );

        if (products.Count < 1)
            return null;

        return await _categoryProductsExportManager.ExportProductsToXlsxAsync(products);
    }

    public async Task<byte[]> PrintB2BOrderPerAccountProductsToPdfAsync(ErpAccount erpAccount)
    {
        var customer = await _workContext.GetCurrentCustomerAsync();

        if (!await _customerService.IsRegisteredAsync(customer))
            return null;

        if (erpAccount == null)
            return null;

        var products = await _erpOrderAdditionalDataService.FindOrderProductsAsync(
            erpAccount,
            _b2BB2CFeaturesSettings.LastNOrdersPerAccount
        );

        if (products.Count < 1)
            return null;

        byte[] bytes;
        await using (var stream = new MemoryStream())
        {
            await _categoryProductsExportManager.ExportProductsToPdfAsync(stream, products);
            bytes = stream.ToArray();
        }
        return bytes;
    }

    public async Task<byte[]> PrintB2BAccountProductsToPdfAsync(
        ErpAccount erpAccount,
        int categoryId = 0
    )
    {
        var currentStore = await _storeContext.GetCurrentStoreAsync();
        var categoryIdList = new List<int>();
        if (categoryId > 0 && _b2BB2CFeaturesSettings.IsCategoryPriceListAppliedToSubCategories)
        {
            categoryIdList.Add(categoryId);
            categoryIdList.AddRange(
                await _categoryService.GetChildCategoryIdsAsync(
                    categoryId,
                    currentStore.Id,
                    showHidden: true
                )
            );
        }

        var products = await _productService.SearchProductsAsync(
            storeId: currentStore.Id,
            categoryIds: categoryIdList
        );

        using var memoryStream = new MemoryStream();
        await _categoryProductsExportManager.ExportProductsToPdfAsync(memoryStream, products);

        return memoryStream.ToArray();
    }

    public async Task<ErpCustomerConfigurationModel> PrepareB2BCustomerConfigurationModelAsync(ErpAccount erpAccount, ErpCustomerConfigurationModel model)
    {
        ArgumentNullException.ThrowIfNull(erpAccount);
        ArgumentNullException.ThrowIfNull(model);

        var currentCustomer = await _workContext.GetCurrentCustomerAsync();

        var erpCustomerConfiguration = await _erpCustomerFunctionalityService.GetErpCustomerConfigurationByNopCustomerIdAsync(currentCustomer.Id);

        if (erpCustomerConfiguration == null)
        {
            erpCustomerConfiguration = new ErpCustomerConfiguration()
            {
                NopCustomerId = currentCustomer.Id
            };

            await _erpCustomerFunctionalityService.InsertErpCustomerConfigurationAsync(erpCustomerConfiguration);
        }

        model.IsHidePricingNote = erpCustomerConfiguration.IsHidePricingNote;
        model.IsHideWeightInfo = erpCustomerConfiguration.IsHideWeightInfo;

        return model;
    }

    public async Task SetB2BCustomerConfigurationModelAsync(ErpAccount erpAccount, ErpCustomerConfigurationModel model)
    {
        ArgumentNullException.ThrowIfNull(erpAccount);
        ArgumentNullException.ThrowIfNull(model);

        var currentCustomer = await _workContext.GetCurrentCustomerAsync();

        var erpCustomerConfiguration = await _erpCustomerFunctionalityService.GetErpCustomerConfigurationByNopCustomerIdAsync(currentCustomer.Id);

        if (erpCustomerConfiguration == null)
        {
            erpCustomerConfiguration = new ErpCustomerConfiguration()
            {
                NopCustomerId = currentCustomer.Id,
                IsHidePricingNote = model.IsHidePricingNote,
                IsHideWeightInfo = model.IsHideWeightInfo,
            };

            await _erpCustomerFunctionalityService.InsertErpCustomerConfigurationAsync(erpCustomerConfiguration);
            return;
        }

        erpCustomerConfiguration.IsHideWeightInfo = model.IsHideWeightInfo;
        erpCustomerConfiguration.IsHidePricingNote = model.IsHidePricingNote;

        await _erpCustomerFunctionalityService.UpdateErpCustomerConfigurationAsync(erpCustomerConfiguration);
    }

    public async Task<ErpShipToAddressListModel> PrepareB2BShippingAddressListModel(ErpAccount erpAccount, ErpNopUser erpNopUser, ErpShipToAddressListModel model)
    {
        ArgumentNullException.ThrowIfNull(erpAccount);
        ArgumentNullException.ThrowIfNull(model);

        if (erpNopUser is null)
            return model;

        if (erpNopUser.ErpUserType == ErpUserType.B2BUser)
        {
            var shipToAddresses = await _erpShipToAddressService.GetAllErpShipToAddressesAsync(erpAccountId: erpNopUser.ErpAccountId, showHidden: false);

            foreach (var shipto in shipToAddresses)
            {
                var shipToAddressModel = new ErpShipToAddressModel
                {
                    Id = shipto.Id,
                    ShipToCode = shipto.ShipToCode,
                    ShipToName = shipto.ShipToName,
                    AddressId = shipto.AddressId,
                    Suburb = shipto.Suburb,
                    DeliveryNotes = shipto.DeliveryNotes,
                    EmailAddresses = shipto.EmailAddresses,
                    IsActive = shipto.IsActive,
                    ErpAccountId = erpAccount.Id,
                    ErpSalesOrganisationId = erpAccount.ErpSalesOrgId,
                    ErpSalesOrganisationCode = (await _erpSalesOrgService.GetErpSalesOrgByIdAsync(erpAccount.ErpSalesOrgId))?.Code
                };

                if (shipto.AddressId > 0)
                {
                    var address = await _addressService.GetAddressByIdAsync(shipto.AddressId);

                    if (address != null)
                    {
                        var country = await _countryService.GetCountryByIdAsync(address.CountryId ?? 0);

                        if (country != null)
                        {
                            shipToAddressModel.CountryId = country.Id;
                            shipToAddressModel.CountryName = country.Name;
                            shipToAddressModel.CountryCode = country.ThreeLetterIsoCode;
                        }

                        shipToAddressModel.Company = address.Company;
                        shipToAddressModel.StateProvinceId = address.StateProvinceId;
                        shipToAddressModel.StateProvinceName = (await _stateProvinceService.GetStateProvinceByIdAsync(address.StateProvinceId ?? 0))?.Name ?? string.Empty;
                        shipToAddressModel.City = address.City;
                        shipToAddressModel.Address1 = address.Address1;
                        shipToAddressModel.Address2 = address.Address2;
                        shipToAddressModel.PostalCode = address.ZipPostalCode;
                        shipToAddressModel.Phone = address.PhoneNumber;
                    }
                }

                model.ErpShipToAddressList.Add(shipToAddressModel);
            }
        }

        else if (erpNopUser.ErpUserType == ErpUserType.B2CUser)
        {
            List<ErpShipToAddress> shipToAddresses;
            if (_b2BB2CFeaturesSettings.UseDefaultAccountForB2CUser)
            {
                shipToAddresses = await _erpShipToAddressService.GetErpShipToAddressesByCustomerAddressesAsync(
                         customerId: erpNopUser.NopCustomerId,
                         erpShipToAddressCreatedByTypeId: (int)ErpShipToAddressCreatedByType.User);
            }
            else
            {
                shipToAddresses = (List<ErpShipToAddress>)await _erpShipToAddressService.GetErpShipToAddressesByAccountIdAsync(accountId: erpAccount.Id, isActiveOnly: true);
            }

            foreach (var shipto in shipToAddresses)
            {
                var shipToAddressModel = new ErpShipToAddressModel
                {
                    Id = shipto.Id,
                    ShipToCode = shipto.ShipToCode,
                    ShipToName = shipto.ShipToName,
                    AddressId = shipto.AddressId,
                    Suburb = shipto.Suburb,
                    DeliveryNotes = shipto.DeliveryNotes,
                    EmailAddresses = shipto.EmailAddresses,
                    IsActive = shipto.IsActive,
                    ErpAccountId = erpAccount.Id,
                    ErpSalesOrganisationId = erpAccount.ErpSalesOrgId,
                    ErpSalesOrganisationCode = (await _erpSalesOrgService.GetErpSalesOrgByIdAsync(erpAccount.ErpSalesOrgId))?.Code
                };

                if (shipto.AddressId > 0)
                {
                    var address = await _addressService.GetAddressByIdAsync(shipto.AddressId);

                    if (address != null)
                    {
                        var country = await _countryService.GetCountryByIdAsync(address.CountryId ?? 0);

                        if (country != null)
                        {
                            shipToAddressModel.CountryId = country.Id;
                            shipToAddressModel.CountryName = country.Name;
                            shipToAddressModel.CountryCode = country.ThreeLetterIsoCode;
                        }

                        shipToAddressModel.Company = address.Company;
                        shipToAddressModel.StateProvinceId = address.StateProvinceId;
                        shipToAddressModel.StateProvinceName = (await _stateProvinceService.GetStateProvinceByIdAsync(address.StateProvinceId ?? 0))?.Name ?? string.Empty;
                        shipToAddressModel.City = address.City;
                        shipToAddressModel.HouseNumber = address.Address1;
                        shipToAddressModel.Street = address.Address2;
                        shipToAddressModel.PostalCode = address.ZipPostalCode;
                        shipToAddressModel.Phone = address.PhoneNumber;
                    }
                }
                model.ErpShipToAddressList.Add(shipToAddressModel);
            }
        }

        model.ErpShipToAddressList = model.ErpShipToAddressList
                   .OrderByDescending(x => x.Id == erpNopUser.ErpShipToAddressId)
                   .ThenBy(x => x.CreatedOnUtc)
                   .ToList();

        return model;
    }

    #region Online savings

    public async Task<decimal?> GetB2BCurrentCustomerAccountSavingsForthisYearAsync(ErpAccount erpAccount)
    {
        if (erpAccount == null)
            return decimal.Zero;

        try
        {
            var cacheKey = new CacheKey(
                string.Format(B2BB2CFeaturesDefaults.B2BUserCurrentYearSavingsByCustomerCacheKey,
                (await _workContext.GetCurrentCustomerAsync()).Id))
            {
                CacheTime = _b2BB2CFeaturesSettings.OnlineSavingsCacheTime
            };

            return await _staticCacheManager.GetAsync(cacheKey, async () =>
            {
                var erpIntegrationPlugin = await _erpIntegrationPluginManager.LoadActiveERPIntegrationPlugin();
                if (erpIntegrationPlugin is null)
                {
                    return decimal.Zero;
                }

                var erpRequest = new ErpGetRequestModel
                {
                    AccountNumber = erpAccount.AccountNumber,
                    DateFrom = new DateTime(DateTime.UtcNow.Year, 1, 1),
                    DateTo = new DateTime(DateTime.UtcNow.Year, 12, 31)
                };

                var totalSavings = await erpIntegrationPlugin.GetAccountSavingsForTimePeriodAsync(erpRequest);

                if (totalSavings == null)
                {
                    return erpAccount.TotalSavingsForthisYear == null ? decimal.Zero : erpAccount.TotalSavingsForthisYear;
                }
                else
                {
                    erpAccount.TotalSavingsForthisYear = totalSavings.Value;
                    erpAccount.TotalSavingsForthisYearUpdatedOnUtc = DateTime.UtcNow;
                    await _erpAccountService.UpdateErpAccountAsync(erpAccount);
                    return totalSavings;
                }
            });
        }
        catch (Exception ex)
        {
            await _erpLogsService.ErrorAsync($"Error while getting B2B current customer account savings for this year for " +
                $"({erpAccount.AccountNumber} - {erpAccount.AccountName}). \n{ex.Message}", ErpSyncLevel.Account);
            return decimal.Zero;
        }
    }

    public async Task<decimal?> GetB2BCurrentCustomerAccountSavingsForAllTimeAsync(ErpAccount erpAccount)
    {
        if (erpAccount == null)
            return decimal.Zero;

        try
        {
            var cacheKey = new CacheKey(
                string.Format(B2BB2CFeaturesDefaults.B2BUserAllTimeSavingsByCustomerCacheKey, (await _workContext.GetCurrentCustomerAsync()).Id))
            {
                CacheTime = _b2BB2CFeaturesSettings.OnlineSavingsCacheTime
            };

            return await _staticCacheManager.GetAsync(cacheKey, async () =>
            {
                var erpIntegrationPlugin = await _erpIntegrationPluginManager.LoadActiveERPIntegrationPlugin();
                if (erpIntegrationPlugin is null)
                {
                    return decimal.Zero;
                }

                var erpRequest = new ErpGetRequestModel
                {
                    AccountNumber = erpAccount.AccountNumber,
                    DateFrom = new DateTime(1970, 1, 1),
                    DateTo = DateTime.UtcNow
                };

                var totalSavings = await erpIntegrationPlugin.GetAccountSavingsForTimePeriodAsync(erpRequest);

                if (totalSavings == null)
                {
                    return erpAccount.TotalSavingsForAllTime == null ? decimal.Zero : erpAccount.TotalSavingsForAllTime;
                }
                else
                {
                    erpAccount.TotalSavingsForAllTime = totalSavings.Value;
                    erpAccount.TotalSavingsForAllTimeUpdatedOnUtc = DateTime.UtcNow;

                    await _erpAccountService.UpdateErpAccountAsync(erpAccount);

                    return totalSavings;
                }
            });
        }
        catch (Exception ex)
        {
            await _erpLogsService.ErrorAsync($"Error while getting B2B current customer account savings for all time for " +
                $"({erpAccount.AccountNumber} - {erpAccount.AccountName}). \n{ex.Message}", ErpSyncLevel.Account);
            return decimal.Zero;
        }
    }

    public async Task<decimal?> GetB2CCurrentCustomerAccountSavingsForthisYearAsync(ErpNopUser erpNopUser)
    {
        if (erpNopUser == null || erpNopUser.ErpUserType != ErpUserType.B2CUser)
            return decimal.Zero;

        var currCustomer = await _workContext.GetCurrentCustomerAsync();
        try
        {
            var cacheKey = new CacheKey(string.Format(B2BB2CFeaturesDefaults.B2CUserCurrentYearSavingsByCustomerCacheKey, currCustomer.Id))
            {
                CacheTime = _b2BB2CFeaturesSettings.OnlineSavingsCacheTime
            };

            return await _staticCacheManager.GetAsync(cacheKey, async () =>
            {
                var erpIntegrationPlugin = await _erpIntegrationPluginManager.LoadActiveERPIntegrationPlugin();
                if (erpIntegrationPlugin is null)
                {
                    return decimal.Zero;
                }

                var erpRequest = new ErpGetRequestModel
                {
                    CustomerEmail = currCustomer.Email,
                    DateFrom = new DateTime(DateTime.UtcNow.Year, 1, 1),
                    DateTo = new DateTime(DateTime.UtcNow.Year, 12, 31)
                };
                var totalSavings = await erpIntegrationPlugin.GetAccountSavingsForTimePeriodAsync(erpRequest);

                if (totalSavings == null)
                {
                    return erpNopUser.TotalSavingsForthisYear == null ? decimal.Zero : erpNopUser.TotalSavingsForthisYear;
                }
                else
                {
                    erpNopUser.TotalSavingsForthisYear = totalSavings.Value;
                    erpNopUser.TotalSavingsForthisYearUpdatedOnUtc = DateTime.UtcNow;
                    await _erpNopUserService.UpdateErpNopUserAsync(erpNopUser);
                    return totalSavings;
                }
            });
        }
        catch (Exception ex)
        {
            await _erpLogsService.ErrorAsync($"Error while getting B2C current customer account savings for this year for " +
                $"({currCustomer.Email} - {currCustomer.FirstName}). \n{ex.Message}", ErpSyncLevel.Account);
            return decimal.Zero;
        }
    }

    public async Task<decimal?> GetB2CCurrentCustomerAccountSavingsForAllTimeAsync(ErpNopUser erpNopUser)
    {
        if (erpNopUser == null)
            return decimal.Zero;

        var currCustomer = await _workContext.GetCurrentCustomerAsync();
        try
        {
            var cacheKey = new CacheKey(string.Format(B2BB2CFeaturesDefaults.B2CUserCurrentYearSavingsByCustomerCacheKey, currCustomer.Id))
            {
                CacheTime = _b2BB2CFeaturesSettings.OnlineSavingsCacheTime
            };
            return await _staticCacheManager.GetAsync(cacheKey, async () =>
            {
                var erpIntegrationPlugin = await _erpIntegrationPluginManager.LoadActiveERPIntegrationPlugin();
                if (erpIntegrationPlugin is null)
                {
                    return decimal.Zero;
                }

                var erpRequest = new ErpGetRequestModel
                {
                    CustomerEmail = currCustomer.Email,
                    DateFrom = new DateTime(1970, 1, 1),
                    DateTo = DateTime.UtcNow
                };
                var totalSavings = await erpIntegrationPlugin.GetAccountSavingsForTimePeriodAsync(erpRequest);

                if (totalSavings == null)
                {
                    return erpNopUser.TotalSavingsForAllTime == null ? decimal.Zero : erpNopUser.TotalSavingsForAllTime;
                }
                else
                {
                    erpNopUser.TotalSavingsForAllTime = totalSavings.Value;
                    erpNopUser.TotalSavingsForAllTimeUpdatedOnUtc = DateTime.UtcNow;

                    await _erpNopUserService.UpdateErpNopUserAsync(erpNopUser);

                    return totalSavings;
                }
            });
        }
        catch (Exception ex)
        {
            await _erpLogsService.ErrorAsync($"Error while getting B2C current customer account savings for all time for " +
                $"({currCustomer.Email} - {currCustomer.FirstName}). \n{ex.Message}", ErpSyncLevel.Account);
            return decimal.Zero;
        }
    }

    public async Task<ErpBillingAddressModel> PrepareB2BBillingAddressModel(ErpAccount erpAccount, ErpNopUser erpNopUser, ErpBillingAddressModel erpBillingAddressModel)
    {
        if (erpAccount == null)
            throw new ArgumentNullException(nameof(erpAccount));

        if (erpBillingAddressModel == null)
            throw new ArgumentNullException(nameof(erpBillingAddressModel));

        erpBillingAddressModel.ErpAccountId = erpAccount.Id;

        // prepare billing address
        if (erpAccount.BillingAddressId.HasValue && erpAccount.BillingAddressId.Value > 0)
        {
            var address = await _addressService.GetAddressByIdAsync(erpAccount.BillingAddressId.Value);
            var country = await _countryService.GetCountryByAddressAsync(address);
            var stateProvince = await _stateProvinceService.GetStateProvinceByIdAsync(address?.StateProvinceId ?? 0);

            erpBillingAddressModel.Id = address.Id;
            erpBillingAddressModel.FirstName = address.FirstName;
            erpBillingAddressModel.LastName = address.LastName;
            erpBillingAddressModel.Email = address.Email;
            erpBillingAddressModel.Company = address.Company;
            erpBillingAddressModel.CountryId = address.CountryId;
            erpBillingAddressModel.CountryName = country?.Name ?? string.Empty;
            erpBillingAddressModel.StateProvinceId = address.StateProvinceId;
            erpBillingAddressModel.StateProvinceName = stateProvince?.Name ?? string.Empty;
            erpBillingAddressModel.Address1 = address.Address1;
            erpBillingAddressModel.City = address.City;
            erpBillingAddressModel.Address2 = address.Address2;
            erpBillingAddressModel.ZipPostalCode = address.ZipPostalCode;
            erpBillingAddressModel.PhoneNumber = address.PhoneNumber;
            erpBillingAddressModel.Suburb = erpAccount.BillingSuburb;
        }

        return erpBillingAddressModel;
    }

    #endregion

    public async Task<ErpAccountInfoAjaxLoadModel> PrepareB2BAccountInfoAjaxLoadModelAsync(ErpAccount b2BAccount, Customer customer, bool enableErpAccountUpdate = false)
    {
        if (b2BAccount == null || customer == null)
            throw new ArgumentNullException(nameof(b2BAccount));

        //customer currency
        var currCurrency = await _workContext.GetWorkingCurrencyAsync();
        var customerCurrency = currCurrency;
        if (customer.CurrencyId.HasValue)
        {
            var customerCurrencyByCurrencyId = await _currencyService.GetCurrencyByIdAsync(customer.CurrencyId.Value);
            if (customerCurrencyByCurrencyId != null && customerCurrencyByCurrencyId.Published)
            {
                customerCurrency = customerCurrencyByCurrencyId;
            }
        }

        var customerCurrencyCode = customerCurrency.CurrencyCode;
        var lang = await _workContext.GetWorkingLanguageAsync();

        // ERP Account Update
        if (_b2BB2CFeaturesSettings.EnableLiveCreditChecks)
            await _erpAccountCreditSyncFunctionality.LiveErpAccountCreditCheckAsync(b2BAccount);

        var model = new ErpAccountInfoAjaxLoadModel();
        model.ErpAccountId = b2BAccount.Id;
        model.HasErpQuoteAssistantRole = await _erpCustomerFunctionalityService.IsCurrentCustomerInB2BQuoteAssistantRoleAsync();
        model.HasErpOrderAssistantRole = await _erpCustomerFunctionalityService.IsCurrentCustomerInB2BOrderAssistantRoleAsync();
        var availableBlanace = b2BAccount.CreditLimitAvailable;

        if (!model.HasErpQuoteAssistantRole && !model.HasErpOrderAssistantRole)
        {
            model.CreditLimit = await _priceFormatter.FormatPriceAsync(b2BAccount.CreditLimit, true, customerCurrencyCode, lang.Id, true);
            model.CurrentBalance = await _priceFormatter.FormatPriceAsync(b2BAccount.CurrentBalance, true, customerCurrencyCode, lang.Id, true);
            model.AvailableCredit = await _priceFormatter.FormatPriceAsync(availableBlanace, true, customerCurrencyCode, lang.Id, true);
            model.LastPaymentAmount = b2BAccount.LastPaymentAmount.HasValue ?
                await _priceFormatter.FormatPriceAsync(b2BAccount.LastPaymentAmount.Value, true, customerCurrencyCode, lang.Id, true) : string.Empty;
            model.LastPaymentDate = b2BAccount.LastPaymentDate.HasValue ? b2BAccount.LastPaymentDate.Value.ToShortDateString() : string.Empty;
        }

        var allowOverSpend = b2BAccount.AllowOverspend;
        model.AllowOverSpend = allowOverSpend;
        if (!allowOverSpend)
        {
            var shoppingCartItems = await _shoppingCartService.GetShoppingCartAsync(customer, ShoppingCartType.ShoppingCart, (await _storeContext.GetCurrentStoreAsync()).Id);
            var shoppingCartTotalBase = (await _orderTotalCalculationService.GetShoppingCartTotalAsync(shoppingCartItems)).shoppingCartTotal;
            var orderTotal = decimal.Zero;

            if (shoppingCartTotalBase.HasValue)
            {
                orderTotal = await _currencyService.ConvertFromPrimaryStoreCurrencyAsync(shoppingCartTotalBase.Value, currCurrency);
            }

            model.CurrentBalanceWithCurrentOrderTotal = await _priceFormatter.FormatPriceAsync((b2BAccount.CurrentBalance + orderTotal), true, customerCurrencyCode, lang.Id, true);
            model.CurrentOrderTotal = await _priceFormatter.FormatPriceAsync(orderTotal, true, customerCurrencyCode, lang.Id, true);
            model.IsOverSpend = orderTotal > availableBlanace;
            if (model.HasErpOrderAssistantRole || model.HasErpQuoteAssistantRole)
            {
                model.CreditWarningMessage = string.Format(await _localizationService.GetResourceAsync("Plugins.Payments.B2BCustomerAccount.B2BQouteOrder.CreditLimitExceed"));
            }
            else
            {
                model.CreditWarningMessage = string.Format(await _localizationService.GetResourceAsync("Plugins.Payments.B2BCustomerAccount.B2BQouteOrder.CreditLimitExceedWithValue"), model.AvailableCredit, model.CurrentOrderTotal);
            }
        }
        return model;
    }

    #endregion
}
