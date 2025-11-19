using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Directory;
using Nop.Data;
using Nop.Plugin.Misc.ErpWebhook.Domain.ParallelTables;
using Nop.Plugin.Misc.ErpWebhook.Models.Credit;
using Nop.Plugin.Misc.ErpWebhook.Models.ErpAccount;
using Nop.Plugin.Misc.ErpWebhook.Services.Interfaces;
using Nop.Services.Common;
using Nop.Services.Directory;
using NopStation.Plugin.B2B.ERPIntegrationCore.Domain;
using NopStation.Plugin.B2B.ERPIntegrationCore.Enums;
using NopStation.Plugin.B2B.ERPIntegrationCore.Services;

namespace Nop.Plugin.Misc.ErpWebhook.Services;

public class WebhookERPAccountService : IWebhookERPAccountService
{
    #region Fields

    private ErpWebhookConfig _erpWebhookConfig = null;
    private readonly IRepository<Parallel_ErpAccount> _erpB2BAccountRepo;
    private readonly IErpLogsService _erpLogsService;
    private readonly IRepository<ErpAccount> _erpAccountRepo;
    private readonly IRepository<ErpSalesOrg> _erpSalesOrgRepo;
    private readonly IErpGroupPriceCodeService _erpGroupPriceCodeService;
    private readonly IRepository<StateProvince> _stateProvinceRepo;
    private readonly IAddressService _addressService;
    private readonly IRepository<Address> _addressRepo;
    private readonly ICountryService _countryService;
    private readonly IErpWebhookService _erpWebhookService;
    private readonly IWorkContext _workContext;
    private readonly IRepository<ErpSalesOrg> _b2bSalesOrgRepo;
    private readonly IErpSalesOrgService _erpSalesOrgService;
    private readonly IErpAccountService _erpAccountService;

    #endregion

    #region Ctor

    public WebhookERPAccountService(
        IRepository<Parallel_ErpAccount> erpB2BAccountRepo,
        IRepository<ErpAccount> erpAccountRepo,
        IRepository<ErpSalesOrg> erpSalesOrgRepo,
        IErpGroupPriceCodeService erpGroupPriceCodeService,
        IRepository<StateProvince> stateProvinceRepo,
        IAddressService addressService,
        IRepository<Address> addressRepo,
        ICountryService countryService,
        IErpWebhookService erpWebhookService,
        IRepository<ErpSalesOrg> b2bSalesOrgRepo,
        IWorkContext workContext,
        IErpLogsService erpLogsService,
        IErpSalesOrgService erpSalesOrgService,
        IErpAccountService erpAccountService)
    {
        _erpB2BAccountRepo = erpB2BAccountRepo;
        _erpAccountRepo = erpAccountRepo;
        _erpSalesOrgRepo = erpSalesOrgRepo;
        _erpGroupPriceCodeService = erpGroupPriceCodeService;
        _stateProvinceRepo = stateProvinceRepo;
        _addressService = addressService;
        _addressRepo = addressRepo;
        _countryService = countryService;
        _erpWebhookService = erpWebhookService;
        _b2bSalesOrgRepo = b2bSalesOrgRepo;
        _workContext = workContext;
        _erpLogsService = erpLogsService;
        _erpSalesOrgService = erpSalesOrgService;
        _erpAccountService = erpAccountService;
    }

    #endregion

    #region Utilities

    private async Task<IReadOnlyList<WebhookErpAccountModel>> RemoveDuplicatesAsync(IEnumerable<WebhookErpAccountModel> accounts)
    {
        var groups = accounts.ToLookup(a => new { a.AccountNumber, a.SalesOrganisationCode });
        var result = new List<WebhookErpAccountModel>(groups.Count);
        var logMessages = new List<string>();

        foreach (var g in groups)
        {
            if (string.IsNullOrEmpty(g.Key.AccountNumber) || string.IsNullOrEmpty(g.Key.SalesOrganisationCode))
            {
                logMessages.Add("Account without account number or sales org code");
            }

            if (g.Count() > 1)
            {
                logMessages.Add($"Account {g.Key.AccountNumber} - {g.Key.SalesOrganisationCode} appears {g.Count()} times in accounts");
            }

            result.Add(g.First());
        }

        // Consolidate log messages into a single log entry
        if (logMessages.Count != 0)
        {
            await _erpLogsService.InsertErpLogAsync(
                ErpLogLevel.Debug,
                ErpSyncLevel.Account,
                "Account Webhook Call: Validation issues found. Click view to see details.",
                $"Account validation issues: {string.Join(";\n", logMessages)}"
            );
        }

        return result;
    }


    private async Task MapAccountFieldsAsync(WebhookErpAccountModel erpAccount, Parallel_ErpAccount dbAccount)
    {
        var currentCustomer = await _workContext.GetCurrentCustomerAsync();
        if (dbAccount.Id <= 0)
        {
            dbAccount.CreatedOnUtc = DateTimeOffset.UtcNow.UtcDateTime;
            dbAccount.LastPriceRefresh = DateTimeOffset.UtcNow.UtcDateTime;
            dbAccount.CreatedById = currentCustomer.Id;
        }

        dbAccount.AccountNumber = erpAccount.AccountNumber;
        dbAccount.AccountName = erpAccount.AccountName ?? "";
        dbAccount.IsActive = _erpWebhookService.StringToBool(erpAccount.IsActive);
        dbAccount.IsDeleted = _erpWebhookService.StringToBool(erpAccount.IsDeleted);
        dbAccount.IsUpdated = false;
        dbAccount.VatNumber = erpAccount.VatNumber ?? "";

        dbAccount.PreFilterFacets = await GetPreFilterFacetBySalesOrgCodeAsync(erpAccount.SalesOrganisationCode, erpAccount.AccountNumber);

        if (dbAccount.PreFilterFacets == null)
        {
            await _erpLogsService.InsertErpLogAsync(
                ErpLogLevel.Debug,
                ErpSyncLevel.Account,
                "Account Webhook Call: Account mapping issue. Click view to see details.",
                $"Account map issue found: PreFilterFacets for account {erpAccount.AccountNumber} is null"
            );
        }

        dbAccount.LastAccountRefresh = DateTimeOffset.UtcNow.UtcDateTime;
        dbAccount.UpdatedById = currentCustomer.Id;
        dbAccount.UpdatedOnUtc = DateTime.UtcNow;

        if (erpAccount.AllowSwitchSalesOrg != null)
        {
            dbAccount.AllowSwitchSalesOrg = _erpWebhookService.StringToBool(erpAccount.AllowSwitchSalesOrg);
        }

        dbAccount.CreditLimit = erpAccount.CreditLimit;

        // Set current balance based on multiple conditions
        if (erpAccount.CreditLimitUsed != null)
        {
            dbAccount.CurrentBalance = erpAccount.CreditLimitUsed.Value;
        }
        else if (erpAccount.CurrentBalance != null)
        {
            dbAccount.CurrentBalance = erpAccount.CurrentBalance.Value;
        }
        else if (erpAccount.Balance != null)
        {
            dbAccount.CurrentBalance = erpAccount.Balance.Value;
        }

        if (erpAccount.CreditLimitAvailable != null)
        {
            dbAccount.CreditLimitAvailable = erpAccount.CreditLimitAvailable.Value;
        }

        if (erpAccount.AllowOverspend != null || dbAccount.Id < 0)
        {
            dbAccount.AllowOverspend = string.IsNullOrWhiteSpace(erpAccount.AllowOverspend)
                ? (_erpWebhookConfig.Accounts_Default_AllowOverspend ?? false)
                : _erpWebhookService.StringToBool(erpAccount.AllowOverspend);
        }

        if (erpAccount.PaymentTypeCode != null)
        {
            dbAccount.PaymentTypeCode = erpAccount.PaymentTypeCode;
        }

        if (erpAccount.OverrideBackOrderingConfigSetting != null)
        {
            dbAccount.OverrideBackOrderingConfigSetting = string.IsNullOrWhiteSpace(erpAccount.OverrideBackOrderingConfigSetting)
                ? (_erpWebhookConfig.OverrideBackOrderingConfigSetting ?? false)
                : _erpWebhookService.StringToBool(erpAccount.OverrideBackOrderingConfigSetting);
        }

        if (erpAccount.AllowAccountsBackOrdering != null)
        {
            dbAccount.AllowAccountsBackOrdering = string.IsNullOrWhiteSpace(erpAccount.AllowAccountsBackOrdering)
                ? (_erpWebhookConfig.AllowAccountsBackOrdering ?? false)
                : _erpWebhookService.StringToBool(erpAccount.AllowAccountsBackOrdering);
        }

        dbAccount.AllowAccountsAddressEditOnCheckout = _erpWebhookConfig.AllowAccountsAddressEditOnCheckout ?? false;

        if (Enum.IsDefined(typeof(StockDisplayFormat), erpAccount.StockDisplayFormatTypeId))
        {
            var stockDisplayFormat = (StockDisplayFormat)erpAccount.StockDisplayFormatTypeId;
            dbAccount.OverrideStockDisplayConfig = _erpWebhookService.StringToBool(erpAccount.OverrideStockDisplayConfig);
            dbAccount.StockDisplayFormatTypeId = (int)stockDisplayFormat;
        }

        dbAccount.B2BAccountStatusTypeId = erpAccount.B2BAccountStatusType;
        dbAccount.PercentageOfStockAllowed = erpAccount.PerscentageOfStockAllowed ?? 100;
        if (dbAccount.PercentageOfStockAllowed <= 0)
            dbAccount.PercentageOfStockAllowed = 100;

        if (!string.IsNullOrWhiteSpace(erpAccount.PaymentTermCode))
            dbAccount.PaymentTermsCode = erpAccount.PaymentTermCode;

        if (!string.IsNullOrWhiteSpace(erpAccount.PaymentTermDescription))
            dbAccount.PaymentTermsDescription = erpAccount.PaymentTermDescription;

        if (!string.IsNullOrWhiteSpace(erpAccount.B2BPriceGroupCode))
        {
            // Await the async method GetErpGroupPriceCodeByNameAsync
            var priceGroupCode = await _erpGroupPriceCodeService.GetErpGroupPriceCodeByNameAsync(erpAccount.B2BPriceGroupCode);
            dbAccount.B2BPriceGroupCodeId = priceGroupCode?.Id ?? (_erpWebhookConfig.B2BPriceGroupCodeId ?? 1);
        }
    }


    private async Task MapAddressFieldsAsync(WebhookErpAccountModel erpAccount, Address address, int defaultCountryId)
    {
        if (address.Id < 0)
        {
            address.CreatedOnUtc = DateTimeOffset.UtcNow.UtcDateTime;
        }

        address.Email = erpAccount.Email;
        address.Company = erpAccount.AccountName;
        address.CountryId = defaultCountryId;
        address.City = erpAccount.BillingCity ?? "";
        address.Address1 = erpAccount.BillingAddress1 ?? "";
        address.Address2 = erpAccount.BillingAddress2 ?? "";
        address.ZipPostalCode = erpAccount.BillingPostalCode ?? "";
        address.StateProvinceId = await GetStateProvinceIdAsync(erpAccount.BillingProvince);
        address.PhoneNumber = erpAccount.BillingPhonenum;
    }

    private async Task<int?> GetStateProvinceIdAsync(string proviencecodeorname)
    {
        int? provinceId = null;
        var stateProvince = await _stateProvinceRepo.Table
            .FirstOrDefaultAsync(p => p.Abbreviation == proviencecodeorname || p.Name == proviencecodeorname);
        if (stateProvince != null && stateProvince.Id > 0)
            provinceId = stateProvince.Id;
        return provinceId;
    }

    private async Task<int> GetDefaultCountryIdAsync(string code)
    {
        if (code == null)
            return 0;

        var defaultCountry = await _countryService.GetCountryByThreeLetterIsoCodeAsync(code);
        return defaultCountry?.Id ?? 0;
    }


    private async Task<IList<ErpSalesOrg>> GetERPSalesOrganisationsAsync()
    {
        return await _erpSalesOrgRepo.Table
            .Where(b => !b.IsDeleted && b.IsActive)
            .ToListAsync();
    }

    private async Task<string> GetPreFilterFacetBySalesOrgCodeAsync(string salesOrgCode, string accountNumber)
    {
        if (string.IsNullOrWhiteSpace(salesOrgCode) || string.IsNullOrWhiteSpace(accountNumber))
            return string.Empty;

        if (!(_erpWebhookConfig.ClientIsMacsteel ?? false))
            return _erpWebhookConfig.AccountPrefilterFacets ?? string.Empty;

        var salesOrg = await _erpSalesOrgService.GetErpSalesOrgByCodeAsync(salesOrgCode);
        if (salesOrg == null)
            return string.Empty;

        var erpAccount = await _erpAccountService.GetErpAccountByIdAsync(salesOrg.ErpAccountIdForB2C);
        bool isB2C = erpAccount?.AccountNumber == accountNumber;

        var warehouseCodes = await _erpWebhookService.GetWareHouseCodesBySalesOrgCodeAsync(salesOrgCode, isB2C);

        return warehouseCodes?.Any() == true
            ? string.Join(",", warehouseCodes)
            : string.Empty;
    }

    #endregion

    #region Methods

    #region Account

    public async Task ProcessErpAccountsAsync(IEnumerable<WebhookErpAccountModel> accounts)
    {
        accounts = await RemoveDuplicatesAsync(accounts);

        if (!accounts.Any())
        {
            return;
        }

        #region Duplicating data for 1032

        _erpWebhookConfig = await _erpWebhookService.LoadErpWebhookConfigsFromJsonAsync();
        var salesOrg = await GetERPSalesOrganisationsAsync();
        var salesOrgDict = salesOrg.ToDictionary(so => so.Code, so => so.Id);

        // Check if any account has SalesOrganisationCode equal to "1030"
        var has1030 = accounts.Any(acc => acc.SalesOrganisationCode == "1030");
        var hasSalesOrg1032 = salesOrg.Any(so => so.Code == "1032");

        // If there are accounts with SalesOrganisationCode equal to "1030", create a duplicate with SalesOrganisationCode "1032"
        if (has1030 && hasSalesOrg1032)
        {
            // Create a new list containing all original accounts plus the duplicated accounts
            accounts = accounts
                .Concat(accounts
                .Where(acc => acc.SalesOrganisationCode == "1030")
                .Select((Func<WebhookErpAccountModel, WebhookErpAccountModel>)(acc =>
                {
                    var duplicatedAccount = new WebhookErpAccountModel
                    {
                        AccountNumber = acc.AccountNumber,
                        AccountName = acc.AccountName,
                        SalesOrganisationCode = "1032", // Change SalesOrganisationCode to "1032"
                        B2BSalesOrganisationId = acc.B2BSalesOrganisationId,
                        IsActive = acc.IsActive,
                        IsDeleted = acc.IsDeleted,
                        B2BAccountStatusType = acc.B2BAccountStatusType,
                        PrefilterFacets = acc.PrefilterFacets,
                        VatNumber = acc.VatNumber,
                        CurrentYearSavings = acc.CurrentYearSavings,
                        AllTimeSavings = acc.AllTimeSavings,
                        B2BPriceGroupCode = acc.B2BPriceGroupCode,
                        OverrideBackOrderingConfigSetting = acc.OverrideBackOrderingConfigSetting,
                        AllowAccountsBackOrdering = acc.AllowAccountsBackOrdering,
                        AllowSwitchSalesOrg = acc.AllowSwitchSalesOrg,
                        AllowOverspend = acc.AllowOverspend,
                        Attributes = acc.Attributes,
                        PerscentageOfStockAllowed = acc.PerscentageOfStockAllowed,
                        CreditLimit = acc.CreditLimit,
                        CreditLimitUsed = acc.CreditLimitUsed,
                        CreditLimitAvailable = acc.CreditLimitAvailable,
                        Balance = acc.Balance,
                        CurrentBalance = acc.CurrentBalance,
                        PaymentTypeCode = acc.PaymentTypeCode,
                        PaymentTermCode = acc.PaymentTermCode,
                        PaymentTermDescription = acc.PaymentTermDescription,
                        BillingCountry = acc.BillingCountry,
                        Email = acc.Email,
                        BillingAddress1 = acc.BillingAddress1,
                        BillingAddress2 = acc.BillingAddress2,
                        BillingCity = acc.BillingCity,
                        BillingProvince = acc.BillingProvince,
                        BillingPostalCode = acc.BillingPostalCode,
                        BillingPhonenum = acc.BillingPhonenum
                    };
                    return duplicatedAccount;
                })));
        }

        #endregion

        // Filter accounts and create the dictionary
        var erpAccounts = accounts
            .Where(sa => salesOrgDict.ContainsKey(sa.SalesOrganisationCode))
            .ToDictionary(sa => sa.AccountNumber + '_' + salesOrgDict[sa.SalesOrganisationCode]);

        var accountNos = erpAccounts.Keys.ToList();

        await _erpLogsService.InsertErpLogAsync(
            ErpLogLevel.Debug,
            ErpSyncLevel.Account,
            "Account Webhook Call: Initiating account processing. Click view to see details.",
            $"Processing {accountNos.Count} account{(accountNos.Count == 1 ? "" : "s")}.\n Account list: {string.Join(", ", accountNos)}");

        #region Create or update B2BAccounts

        var commonAccounts = 
            from erpAccount in accounts
            join b2bSales in salesOrg
            on erpAccount.SalesOrganisationCode equals b2bSales.Code
            join erpB2BAccount in _erpB2BAccountRepo.Table
            on b2bSales.Id equals erpB2BAccount.B2BSalesOrganisationId
            where erpB2BAccount.AccountNumber == erpAccount.AccountNumber
            select erpB2BAccount;

        var existingAccounts = commonAccounts.ToDictionary(x => x.AccountNumber + '_' + x.B2BSalesOrganisationId, x => x);

        var failedToLoadAccounts = new List<string>();

        foreach (var dbAccount in existingAccounts)
        {
            if (!erpAccounts.TryGetValue(dbAccount.Key, out var erpAccount))
            {
                failedToLoadAccounts.Add(dbAccount.Key);
                continue;
            }

            erpAccount.B2BSalesOrganisationId = salesOrg.FirstOrDefault(x => x.Code.Equals(erpAccount.SalesOrganisationCode))?.Id ?? 1;

            if (!erpAccount.B2BSalesOrganisationId.Equals(dbAccount.Value.B2BSalesOrganisationId))
                continue;

            await MapAccountFieldsAsync(erpAccount, dbAccount.Value);
        }

        if (failedToLoadAccounts.Count > 0)
        { 
            await _erpLogsService.InsertErpLogAsync(
                ErpLogLevel.Debug,
                ErpSyncLevel.Account,
                $"Account Webhook Call: Account load failure of total {failedToLoadAccounts.Count} accounts. Click view to see details.",
                $"Failed to load total {failedToLoadAccounts.Count} accounts during account processing. \nThose accounts: {string.Join(", ", failedToLoadAccounts)}");
        }

        await _erpB2BAccountRepo.UpdateAsync(existingAccounts.Values.ToList());

        await _erpLogsService.InsertErpLogAsync(
            ErpLogLevel.Debug,
            ErpSyncLevel.Account,
            "Account Webhook Call: Account updates: Processing existing accounts. Click view to see details.",
            $"Updating {existingAccounts.Count} account{(existingAccounts.Count == 1 ? "" : "s")}. Accounts: {string.Join(", ", existingAccounts.Keys)}");


        var missing = accountNos.Except(existingAccounts.Keys).ToList();
        if (missing.Count > 0)
        {
            await _erpLogsService.InsertErpLogAsync(
            ErpLogLevel.Debug,
            ErpSyncLevel.Account,
            "Account Webhook Call: Account creation: Processing new accounts. Click view to see details.",
            $"Creating {missing.Count} account{(missing.Count == 1 ? "" : "s")}. Accounts: {string.Join(", ", missing)}");

            var newAccounts = erpAccounts
                .Where(erpAccount => missing.Contains(erpAccount.Key)).Select(x => x.Value).ToList();

            var newAccountsTobeInserted = new List<Parallel_ErpAccount>();
            foreach (var erpAccount in newAccounts)
            {
                var dbAccount = new Parallel_ErpAccount()
                {
                    AccountNumber = erpAccount.AccountNumber,
                    B2BSalesOrganisationId = salesOrg.FirstOrDefault(x => x.Code.Equals(erpAccount.SalesOrganisationCode))?.Id ?? 1,
                };

                await MapAccountFieldsAsync(erpAccount, dbAccount);
                newAccountsTobeInserted.Add(dbAccount);
                existingAccounts.Add(dbAccount.AccountNumber + '_' + dbAccount.B2BSalesOrganisationId, dbAccount);
            }

            if (newAccountsTobeInserted.Count != 0)
            {
                await _erpB2BAccountRepo.InsertAsync(newAccountsTobeInserted);
            }
        }

        #endregion

        #region Create or update (billing) Address

        var billingAddressIds = existingAccounts.Values
            .Where(a => a.BillingAddressId.HasValue && a.BillingAddressId.Value >= 0)
            .ToDictionary(a => a.Id, a => a.BillingAddressId.Value);

        var billingAddresses = await _addressRepo.Table
            .Where(x => billingAddressIds.Values.Contains(x.Id))
            .ToDictionaryAsync(x => x.Id, x => x);

        var createdBillingAddresses = new List<string>();
        var updatedBillingAddresses = new List<string>();

        foreach (var erpAccount in erpAccounts)
        {
            if (!existingAccounts.TryGetValue(erpAccount.Key, out var b2bAccount))
            {
                throw new Exception($"Could not find account '{erpAccount.Key}' after it should have been updated/created");
            }

            if (!billingAddressIds.TryGetValue(b2bAccount.Id, out var billingAddressId) ||
                (billingAddressId < 0) ||
                !billingAddresses.TryGetValue(billingAddressId, out var address))
            {
                createdBillingAddresses.Add(erpAccount.Key);
                address = new Address();
                await MapAddressFieldsAsync(erpAccount.Value, address, await GetDefaultCountryIdAsync(_erpWebhookConfig.DefaultCountryThreeLetterIsoCode));

                await _addressService.InsertAddressAsync(address);
                b2bAccount.BillingAddressId = address.Id;
                await _erpB2BAccountRepo.UpdateAsync(b2bAccount);
            }
            else
            {
                updatedBillingAddresses.Add(erpAccount.Key);
                await MapAddressFieldsAsync(erpAccount.Value, address, await GetDefaultCountryIdAsync(_erpWebhookConfig.DefaultCountryThreeLetterIsoCode));
                await _addressService.UpdateAddressAsync(address);
            }
        }

        if (createdBillingAddresses.Count > 0)
        {
            await _erpLogsService.InsertErpLogAsync(
                ErpLogLevel.Debug,
                ErpSyncLevel.Account,
                "Account Webhook Call: Billing address creation: New addresses added. Click view to see details.",
                $"{createdBillingAddresses.Count} billing address{(createdBillingAddresses.Count > 1 ? "es" : "")} created. Accounts: {string.Join(", ", createdBillingAddresses)}");
        }

        if (updatedBillingAddresses.Count > 0)
        {
            await _erpLogsService.InsertErpLogAsync(
                ErpLogLevel.Debug,
                ErpSyncLevel.Account,
                "Account Webhook Call: Billing address updates: Existing addresses modified. Click view to see details.",
                $"{updatedBillingAddresses.Count} billing address{(updatedBillingAddresses.Count > 1 ? "es" : "")} updated. Accounts: {string.Join(", ", updatedBillingAddresses)}");
        }

        #endregion
    }


    #endregion

    #region Credit

    public async Task ProcessCreditsAsync(IEnumerable<Credit> credits)
    {
        // Asynchronously join credits with sales organizations
        var modifiedCredits = 
            await (from credit in credits
            join salesOrg in _b2bSalesOrgRepo.Table on credit.SalesOrganisationCode equals salesOrg.Code
            select new CreditModel
            {
                AccountNumber = credit.AccountNumber,
                SalesOrganisationCode = credit.SalesOrganisationCode,
                SalesOrgId = salesOrg.Id,
                CreditLimit = credit.CreditLimit,
                CurrentBalance = credit.CurrentBalance,
                CreditLimitAvailable = credit.CreditLimitAvailable,
                B2BAccountStatusTypeId = credit.B2BAccountStatusTypeId,
                LastPaymentAmount = credit.LastPaymentAmount,
                LastPaymentDate = credit.LastPaymentDate
            }).ToListAsync(); // Asynchronous execution of the query

        if (modifiedCredits != null && modifiedCredits.Count == 0)
        {
            throw new Exception("No data matched for existing sales organisations");
        }

        // Asynchronously join the modifiedCredits with the B2BAccountRepo.Table
        var b2bAccounts = 
            await (from credit in modifiedCredits
            join b2bAccount in _erpAccountRepo.Table
            on new { AccountNumber = credit.AccountNumber, SalesOrgId = credit.SalesOrgId }
            equals new { AccountNumber = b2bAccount.AccountNumber, SalesOrgId = b2bAccount.ErpSalesOrgId }
            select b2bAccount).ToListAsync();  // Asynchronous execution of the query

        foreach (var b2bAccount in b2bAccounts)
        {
            var matchingCredit = modifiedCredits.FirstOrDefault(credit => credit.AccountNumber.Equals(b2bAccount.AccountNumber)
                && credit.SalesOrgId.Equals(b2bAccount.ErpSalesOrgId));

            if (matchingCredit == null)
                continue;

            b2bAccount.AccountNumber = matchingCredit.AccountNumber;
            b2bAccount.CreditLimit = string.IsNullOrWhiteSpace(matchingCredit.CreditLimit) ? b2bAccount.CreditLimit : decimal.Parse(matchingCredit.CreditLimit);
            b2bAccount.CurrentBalance = string.IsNullOrWhiteSpace(matchingCredit.CurrentBalance) ? b2bAccount.CurrentBalance : decimal.Parse(matchingCredit.CurrentBalance);
            b2bAccount.CreditLimitAvailable = string.IsNullOrWhiteSpace(matchingCredit.CreditLimitAvailable) ? b2bAccount.CreditLimitAvailable : decimal.Parse(matchingCredit.CreditLimitAvailable);
            b2bAccount.ErpAccountStatusTypeId = string.IsNullOrWhiteSpace(matchingCredit.B2BAccountStatusTypeId) ? b2bAccount.ErpAccountStatusTypeId : int.Parse(matchingCredit.B2BAccountStatusTypeId);
            b2bAccount.LastPaymentAmount = string.IsNullOrWhiteSpace(matchingCredit.LastPaymentAmount) ? b2bAccount.LastPaymentAmount : decimal.Parse(matchingCredit.LastPaymentAmount);
            if (DateTime.TryParse(matchingCredit.LastPaymentDate, out var parsedDate))
            {
                b2bAccount.LastPaymentDate = parsedDate;
            }
        }

        await _erpAccountRepo.UpdateAsync(b2bAccounts);
    }

    #endregion

    #endregion
}
