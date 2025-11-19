using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Directory;
using Nop.Data;
using Nop.Plugin.Misc.ErpWebhook.Domain.ParallelTables;
using Nop.Plugin.Misc.ErpWebhook.Models.ErpShipToAddress;
using Nop.Plugin.Misc.ErpWebhook.Services.Interfaces;
using NopStation.Plugin.B2B.ERPIntegrationCore.Services;
using NopStation.Plugin.B2B.ERPIntegrationCore.Domain;
using NopStation.Plugin.B2B.ERPIntegrationCore.Enums;
using ErpShipToAddressModel = Nop.Plugin.Misc.ErpWebhook.Models.ErpShipToAddress.ErpShipToAddressModel;

namespace Nop.Plugin.Misc.ErpWebhook.Services;

public class WebhookERPShipToAddressService : IWebhookERPShipToAddressService
{
    #region Fields

    private ErpWebhookConfig _erpWebhookConfig = null;
    private readonly IRepository<ErpAccount> _erpAccountRepo;
    private readonly IRepository<Parallel_ErpShipToAddress> _erpShipToAddressRepo;
    private readonly IRepository<StateProvince> _stateProvinceRepo;
    private readonly IErpLogsService _erpLogsService;
    private readonly IRepository<ErpSalesOrg> _b2BSalesOrgRepo;
    private readonly IRepository<Address> _addressRepo;
    private readonly IErpWebhookService _erpWebhookService;
    private readonly IWorkContext _workContext;

    #endregion

    #region Ctor

    public WebhookERPShipToAddressService(IRepository<ErpAccount> erpAccountRepo,
        IRepository<Parallel_ErpShipToAddress> erpShipToAddressRepo,
        IRepository<StateProvince> stateProvinceRepo,
        IErpLogsService erpLogsService,
        IRepository<ErpSalesOrg> b2BSalesOrgRepo,
        IRepository<Address> addressRepo,
        IErpWebhookService erpWebhookService,
        IWorkContext workContext)
    {
        _erpAccountRepo = erpAccountRepo;
        _erpShipToAddressRepo = erpShipToAddressRepo;
        _stateProvinceRepo = stateProvinceRepo;
        _erpLogsService = erpLogsService;
        _b2BSalesOrgRepo = b2BSalesOrgRepo;
        _addressRepo = addressRepo;
        _erpWebhookService = erpWebhookService;
        _workContext = workContext;
    }

    #endregion

    #region Utils

    private async Task<Dictionary<string, int>> GetB2BAccountIdsAsync(List<AcountNoWithLocation> accountNosAndLocation)
    {
        if (accountNosAndLocation == null)
            return new Dictionary<string, int>();

        var accountNumbers = accountNosAndLocation.Select(x => x.AccNo).ToList();
        var b2BAccounts = await _erpAccountRepo.Table.Where(a => accountNumbers.Contains(a.AccountNumber)).ToListAsync();

        var accountIds = new Dictionary<string, int>();
        var b2bSalesOrgId = _erpWebhookConfig.DefaultSalesOrgId;
        var b2bSalesOrganizations = await _b2BSalesOrgRepo.Table.Where(x => x.IsActive && !x.IsDeleted).ToListAsync();

        foreach (var item in accountNosAndLocation)
        {
            if (!string.IsNullOrWhiteSpace(item.Location))
                b2bSalesOrgId = b2bSalesOrganizations.Where(x => x.Code.Equals(item.Location)).Select(x => x.Id).FirstOrDefault();

            var matchingAccount = b2BAccounts.FirstOrDefault(x => x.AccountNumber.Equals(item.AccNo) && x.ErpSalesOrgId == b2bSalesOrgId);

            if (matchingAccount != null && !accountIds.ContainsKey(matchingAccount.AccountNumber + "_" + b2bSalesOrgId))
            {
                accountIds.Add(matchingAccount.AccountNumber + "_" + b2bSalesOrgId, matchingAccount.Id);
            }
        }

        return accountIds;
    }

    private async Task<Dictionary<string, StateProvince>> LoadStateProvincesAsync(IEnumerable<string> stateProvinceNames)
    {
        stateProvinceNames = stateProvinceNames.Distinct().Select(x => x.ToLower()).ToArray();
        var defaultCountry = await _erpWebhookService.GetCountryIdByTwoOrThreeLetterIsoCodeAsync(_erpWebhookConfig.DefaultCountryThreeLetterIsoCode);

        var existing = await _stateProvinceRepo.Table
            .Where(sp => stateProvinceNames.Contains(sp.Name.ToLower()) && sp.CountryId == (defaultCountry ?? 0))
            .ToDictionaryAsync(sp => sp.Name);

        return existing;
    }

    public async Task MapShipToAddressFieldsAsync(ErpShipToAddressModel erpShipTo, Parallel_ErpShipToAddress shipToAddress, Dictionary<string, StateProvince> stateProvinces)
    {
        if (shipToAddress.Id <= 0)
        {
            var currentCustomer = await _workContext.GetCurrentCustomerAsync();
            shipToAddress.CreatedById = currentCustomer.Id;
            shipToAddress.CreatedOnUtc = DateTimeOffset.UtcNow.UtcDateTime;
            shipToAddress.ShipToAddressCreatedByTypeId = (int)ErpShipToAddressCreatedByType.Admin;
        }

        shipToAddress.IsActive = _erpWebhookService.StringToBool(erpShipTo.IsActive);
        var updatedCustomer = await _workContext.GetCurrentCustomerAsync();
        shipToAddress.UpdatedById = updatedCustomer.Id;
        shipToAddress.UpdatedOnUtc = DateTimeOffset.UtcNow.UtcDateTime;
        shipToAddress.IsDeleted = _erpWebhookService.StringToBool(erpShipTo.IsDeleted);
        shipToAddress.IsUpdated = false;
        shipToAddress.ShipToCode = erpShipTo.ShipToCode ?? "";
        shipToAddress.ShipToName = erpShipTo.ShipToName ?? "";
        shipToAddress.Suburb = erpShipTo.Suburb ?? "";
        if (stateProvinces.TryGetValue(erpShipTo.StateProvince ?? "", out StateProvince stateProvince))
        {
            shipToAddress.ProvinceCode = stateProvince.Abbreviation ?? "";
        }
        shipToAddress.DeliveryNotes = erpShipTo.DeliveryNotes ?? "";
        shipToAddress.EmailAddresses = erpShipTo.EmailAddress ?? "";
        if (!string.IsNullOrEmpty(erpShipTo.RepNumber))
        {
            shipToAddress.RepNumber = erpShipTo.RepNumber;
            shipToAddress.RepFullName = erpShipTo.RepFullName;
            shipToAddress.RepPhoneNumber = erpShipTo.RepPhoneNumber;
            shipToAddress.RepEmail = erpShipTo.RepEmail;
        }
        shipToAddress.OrderId = 0;
    }

    public async Task MapAddressFieldsAsync(ErpShipToAddressModel erpShipTo, Address address, int? defaultCountryId, Dictionary<string, StateProvince> stateProvinces)
    {
        if (address.Id <= 0)
        {
            address.CreatedOnUtc = DateTime.UtcNow;
        }

        address.Email = erpShipTo.EmailAddress ?? "";
        address.Company = erpShipTo.Company ?? "";
        address.CountryId = defaultCountryId;
        if (stateProvinces.TryGetValue(erpShipTo.StateProvince ?? "", out StateProvince stateProvince))
        {
            address.StateProvinceId = stateProvince?.Id;
        }
        address.County = erpShipTo.County ?? "";
        address.Address1 = erpShipTo.Address1 ?? "";
        address.Address2 = erpShipTo.Address2 ?? "";
        address.City = erpShipTo.City ?? "";
        address.ZipPostalCode = erpShipTo.ZipPostalCode ?? "";
        address.PhoneNumber = erpShipTo.PhoneNumber ?? "";
        address.FaxNumber = erpShipTo.FaxNumber ?? "";
    }

    #endregion

    #region Methods

    public async Task ProcessErpShipToAddressAsync(List<ErpShipToAddressModel> erpShipToAddress)
    {
        _erpWebhookConfig = await _erpWebhookService.LoadErpWebhookConfigsFromJsonAsync();

        erpShipToAddress = (await RemoveDuplicatesAsync(erpShipToAddress)).ToList();
        var b2bSalesOrganisation = await _b2BSalesOrgRepo.Table
                            .Where(x => x.IsActive && !x.IsDeleted)
                            .ToListAsync();

        #region duplicating data for 1032

        // Check if any account has SalesOrganisationCode equal to "1030"
        var has1030 = erpShipToAddress.Any(ship => ship.SalesOrganisationCode == "1030");
        var hasSalesOrg1032 = b2bSalesOrganisation.Any(so => so.Code == "1032");

        // If there are accounts with SalesOrganisationCode equal to "1030", create a duplicate with SalesOrganisationCode "1032"
        if (has1030 && hasSalesOrg1032)
        {
            // Create a new list containing all original accounts plus the duplicated accounts
            erpShipToAddress = erpShipToAddress
                .Concat(erpShipToAddress
                .Where(acc => acc.SalesOrganisationCode == "1030")
                .Select(ship =>
                {
                    return new ErpShipToAddressModel
                    {
                        AccountNumber = ship.AccountNumber,
                        SalesOrganisationCode = "1032", // Change SalesOrganisationCode to "1032" 
                        IsActive = ship.IsActive,
                        IsDeleted = ship.IsDeleted,
                        Address1 = ship.Address1,
                        Address2 = ship.Address2,
                        City = ship.City,
                        Company = ship.Company,
                        Country = ship.Country,
                        County = ship.County,
                        CustomAttributes = ship.CustomAttributes,
                        DeliveryNotes = ship.DeliveryNotes,
                        EmailAddress = ship.EmailAddress,
                        FaxNumber = ship.FaxNumber,
                        PhoneNumber = ship.PhoneNumber,
                        RepPhoneNumber = ship.RepPhoneNumber,
                        RepEmail = ship.RepEmail,
                        RepFullName = ship.RepFullName,
                        RepNumber = ship.RepNumber,
                        ShipToCode = ship.ShipToCode,
                        ShipToName = ship.ShipToName,
                        StateProvince = ship.StateProvince,
                        Suburb = ship.Suburb,
                        ZipPostalCode = ship.ZipPostalCode
                    };
                })).ToList();
        }

        #endregion

        var erpShipTos = erpShipToAddress
            .ToDictionary(sa => sa.ShipToCode + '_' + b2bSalesOrganisation
            .Where(x => x.Code
            .Equals(sa.SalesOrganisationCode))
            .Select(x => x.Id)
            .FirstOrDefault());

        await _erpLogsService.InsertErpLogAsync(
            ErpLogLevel.Debug,
            ErpSyncLevel.ShipToAddress,
            "ShipToAddress Webhook Call: Initiating ship-to address processing. Click view to see details.",
            $"Processing ship-to addresses: {string.Join("; ", erpShipTos.Keys)}");

        var accountNumbersAndLocation = erpShipToAddress
            .Select(sa => new AcountNoWithLocation { AccNo = sa.AccountNumber, Location = sa.SalesOrganisationCode })
            .Distinct()
            .ToList();

        var accountNumbersToIds = await GetB2BAccountIdsAsync(accountNumbersAndLocation);

        var accountIds = accountNumbersToIds.Values
            .Distinct()
            .ToList();

        await _erpLogsService.InsertErpLogAsync(
            ErpLogLevel.Debug,
            ErpSyncLevel.ShipToAddress,
            "ShipToAddress Webhook Call: Loaded account IDs. Click view to see details.",
            $"Loaded account IDs: {string.Join("; ", accountNumbersToIds.Select(kvp => $"{kvp.Key} => {kvp.Value}"))}");

        var existing = (await _erpShipToAddressRepo.Table
            .Where(sa => accountIds.Contains(sa.B2BAccountId))
            .ToListAsync())
            .AsEnumerable()
            .GroupBy(sa => new { sa.ShipToCode, sa.B2BSalesOrganisationId })
            .ToDictionary(group => group.Key.ShipToCode + "_" + group.Key.B2BSalesOrganisationId, group => group.FirstOrDefault());

        var stateProvinces = await LoadStateProvincesAsync(erpShipTos.Select(st => st.Value.StateProvince));
        var defaultCountryId = await _erpWebhookService.GetCountryIdByTwoOrThreeLetterIsoCodeAsync(_erpWebhookConfig.DefaultCountryThreeLetterIsoCode);

        var addressesToUpdate = new Dictionary<string, int>();
        var logMessages = new List<string>();

        foreach (var erpShipTo in erpShipTos)
        {
            if (existing.TryGetValue(erpShipTo.Key, out var existingShipTo))
            {
                await MapShipToAddressFieldsAsync(erpShipTo.Value, existingShipTo, stateProvinces);
                addressesToUpdate.Add(erpShipTo.Key, existingShipTo.AddressId);

                // Collect log messages for consolidation
                logMessages.Add($"Ship-to {existingShipTo.Id} for account {erpShipTo.Value.AccountNumber} will be updated.");
            }
            else
            {
                var b2bSalesOrgId = 0;
                if (!string.IsNullOrWhiteSpace(erpShipTo.Value.SalesOrganisationCode))
                    b2bSalesOrgId = b2bSalesOrganisation
                        .Where(x => x.Code
                        .Equals(erpShipTo.Value.SalesOrganisationCode))
                        .Select(x => x.Id)
                        .FirstOrDefault();

                if (!accountNumbersToIds.TryGetValue(erpShipTo.Value.AccountNumber + "_" + b2bSalesOrgId, out int accountId))
                {
                    // Collect log messages for consolidation
                    logMessages.Add($"ProcessShipToAddress: could not find account with number '{erpShipTo.Value.AccountNumber}'. Ship-To-Address '{erpShipTo.Value.ShipToCode}' skipped.");
                    continue;
                }

                var address = new Address();
                await MapAddressFieldsAsync(erpShipTo.Value, address, defaultCountryId, stateProvinces);
                await _addressRepo.InsertAsync(address);
                // Collect log messages for consolidation
                logMessages.Add($"Created new address {address.Id} for account {erpShipTo.Value.AccountNumber}.");

                var newShipTo = new Parallel_ErpShipToAddress
                {
                    B2BAccountId = accountId,
                    B2BSalesOrganisationId = b2bSalesOrgId,
                    AddressId = address.Id,
                };
                await MapShipToAddressFieldsAsync(erpShipTo.Value, newShipTo, stateProvinces);
                await _erpShipToAddressRepo.InsertAsync(newShipTo);
                // Collect log messages for consolidation
                logMessages.Add($"Will create new ship-to for account {erpShipTo.Value.AccountNumber} with address id {address.Id}.");
            }
        }

        var addressIds = addressesToUpdate.Values.ToList();
        await _erpLogsService.InsertErpLogAsync(
            ErpLogLevel.Debug,
            ErpSyncLevel.ShipToAddress,
            "ShipToAddress Webhook Call: Addresses to update. Click view to see details.",
            $"Addresses to update: {string.Join(";\n", addressesToUpdate.Select(kvp => $"add. {kvp.Value} (shipto {kvp.Key})"))}");

        var addresses = _addressRepo.Table
            .Where(a => addressIds.Contains(a.Id))
            .ToDictionary(a => a.Id);

        await _erpLogsService.InsertErpLogAsync(
            ErpLogLevel.Debug,
            ErpSyncLevel.ShipToAddress,
            "ShipToAddress Webhook Call: Addresses loaded. Click view to see details.",
            $"Addresses loaded: {string.Join("; ", addresses.Keys)}.");
        foreach (var shipToCodeToAddressId in addressesToUpdate)
        {
            var shipToAddress = erpShipTos[shipToCodeToAddressId.Key];
            if (addresses.TryGetValue(shipToCodeToAddressId.Value, out var address))
            {
                // Collect log messages for consolidation
                logMessages.Add($"Address {address.Id} of ship-to {shipToAddress.ShipToCode} will be updated.");
                await MapAddressFieldsAsync(shipToAddress, address, defaultCountryId, stateProvinces);
            }
            else
            {
                var existingShipTo = existing[shipToCodeToAddressId.Key];
                // Collect log messages for consolidation
                logMessages.Add($"Address {existingShipTo.AddressId} of ship-to {shipToAddress.ShipToCode} did not exist and a new one will be created.");

                address = new Address() { Id = shipToCodeToAddressId.Value };
                await MapAddressFieldsAsync(shipToAddress, address, defaultCountryId, stateProvinces);
                await _addressRepo.InsertAsync(address);

                existingShipTo.AddressId = address.Id;
                await _erpShipToAddressRepo.UpdateAsync(existingShipTo);
            }
        }

        // Consolidate logs after the loop
        if (logMessages.Count != 0)
        {
            await _erpLogsService.InsertErpLogAsync(
                ErpLogLevel.Debug,
                ErpSyncLevel.ShipToAddress,
                "ShipToAddress Webhook Call: Ship-to address processing details. Click view to see details.",
                string.Join(";\n", logMessages));        
        }

        if (existing.Count != 0)
        {
            await _erpShipToAddressRepo.UpdateAsync(existing.Values.ToList());            
        }
    }

    private async Task<IEnumerable<ErpShipToAddressModel>> RemoveDuplicatesAsync(IEnumerable<ErpShipToAddressModel> erpShipToAddress)
    {
        ArgumentNullException.ThrowIfNull(erpShipToAddress);

        var logMessages = new List<string>();

        var emptyAccCount = erpShipToAddress.Count(sa => string.IsNullOrEmpty(sa.AccountNumber));
        if (emptyAccCount > 0)
        {
            logMessages.Add($"There are {emptyAccCount} ship-tos in this erpShipToAddress without AccNo.");
            erpShipToAddress = erpShipToAddress
                .Where(sa => !string.IsNullOrEmpty(sa.AccountNumber));
        }
        var groupings = erpShipToAddress
            .ToLookup(a => new { a.ShipToCode, a.SalesOrganisationCode });

        var list = new List<ErpShipToAddressModel>();

        foreach (var grouping in groupings)
        {
            if (grouping.Count() > 1)
                logMessages.Add($"ShipToAddress {grouping.Key.ShipToCode} with salesOrg {grouping.Key.SalesOrganisationCode} appears {grouping.Count()} times in erpShipToAddress.");

            if (string.IsNullOrEmpty(grouping.Key.ShipToCode))
            {
                logMessages.Add($"There are {grouping.Count()} ship-tos in this erpShipToAddress without ShipToCode.");
                continue;
            }

            list.Add(grouping.First());
        }

        if (logMessages.Count != 0)
        {
            await _erpLogsService.InsertErpLogAsync(
                ErpLogLevel.Debug,
                ErpSyncLevel.ShipToAddress,
                "ShipToAddress Webhook Call: Ship-to address validation issues. Click view to see details.",
                string.Join(";\n", logMessages));
        }

        return list;
    }


    #endregion
}
