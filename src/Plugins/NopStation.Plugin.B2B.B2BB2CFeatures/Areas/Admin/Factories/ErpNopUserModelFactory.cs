using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using ClosedXML.Excel;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Data.SqlClient;
using NNopStation.Plugin.B2B.B2BB2CFeatures.Services.SpecialIncludeExcludeService;
using Nop.Core;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Localization;
using Nop.Data;
using Nop.Services;
using Nop.Services.Attributes;
using Nop.Services.Common;
using Nop.Services.Customers;
using Nop.Services.Directory;
using Nop.Services.ExportImport.Help;
using Nop.Services.Helpers;
using Nop.Services.Localization;
using Nop.Web.Areas.Admin.Factories;
using Nop.Web.Areas.Admin.Infrastructure.Mapper.Extensions;
using Nop.Web.Areas.Admin.Models.Common;
using Nop.Web.Areas.Admin.Models.Customers;
using Nop.Web.Framework.Factories;
using Nop.Web.Framework.Models.Extensions;
using NopStation.Plugin.B2B.B2BB2CFeatures.Areas.Admin.Models;
using NopStation.Plugin.B2B.ERPIntegrationCore;
using NopStation.Plugin.B2B.ERPIntegrationCore.Domain;
using NopStation.Plugin.B2B.ERPIntegrationCore.Enums;
using NopStation.Plugin.B2B.ERPIntegrationCore.Services;

namespace NopStation.Plugin.B2B.B2BB2CFeatures.Areas.Admin.Factories;

public class ErpNopUserModelFactory : IErpNopUserModelFactory
{
    #region Fields

    private readonly IDateTimeHelper _dateTimeHelper;
    private readonly IAddressService _addressService;
    private readonly ICustomerService _customerService;
    private readonly ICountryService _countryService;
    private readonly IStateProvinceService _stateProvinceService;
    private readonly IAddressModelFactory _addressModelFactory;
    private readonly AddressSettings _addressSettings;
    private readonly ILocalizationService _localizationService;
    private readonly IErpAccountService _erpAccountService;
    private readonly IErpSalesOrgService _erpSalesOrgService;
    private readonly IErpNopUserService _erpNopUserService;
    private readonly IAttributeFormatter<AddressAttribute, AddressAttributeValue> _addressAttributeFormatter;
    private readonly IErpNopUserAccountMapService _erpNopUserAccountMapService;
    private readonly IErpShipToAddressService _erpShipToAddressService;
    private readonly B2BB2CFeaturesSettings _b2BB2CFeaturesSettings;
    private readonly IGenericAttributeService _genericAttributeService;
    private readonly IAclSupportedModelFactory _aclSupportedModelFactory;
    private readonly IRepository<Customer> _customerRepository;
    private readonly IRepository<ErpNopUser> _erpNopUserRepository;
    private readonly IRepository<ErpSalesRep> _erpSalesRepRepository;
    private readonly IRepository<CustomerCustomerRoleMapping> _customerCustomerRoleMappingRepository;
    private readonly IOverridenCustomerModelFactory _overridenCustomerModelFactory;
    private readonly IB2BExportImportManager _b2BExportImportManager;
    private readonly IWorkContext _workContext;
    private readonly CatalogSettings _catalogSettings;

    #endregion

    #region Ctor

    public ErpNopUserModelFactory(ILocalizationService localizationService,
        IDateTimeHelper dateTimeHelper,
        IAddressService addressService,
        ICustomerService customerService,
        ICountryService countryService,
        IStateProvinceService stateProvinceService,
        IAddressModelFactory addressModelFactory,
        AddressSettings addressSettings,
        IErpAccountService erpAccountService,
        IErpSalesOrgService erpSalesOrgService,
        IErpNopUserService erpNopUserService,
        IAttributeFormatter<AddressAttribute, AddressAttributeValue> addressAttributeFormatter,
        IErpNopUserAccountMapService erpNopUserAccountMapService,
        IErpShipToAddressService erpShipToAddressService,
        B2BB2CFeaturesSettings b2BB2CFeaturesSettings,
        IGenericAttributeService genericAttributeService,
        IAclSupportedModelFactory aclSupportedModelFactory,
        IRepository<ErpNopUser> erpNopUserRepository,
        IRepository<ErpSalesRep> erpSalesRepRepository,
        IRepository<Customer> customerRepository,
        IRepository<CustomerCustomerRoleMapping> customerCustomerRoleMappingRepository,
        IOverridenCustomerModelFactory overridenCustomerModelFactory,
        IB2BExportImportManager b2BExportImportManager,
        IWorkContext workContext,
        CatalogSettings catalogSettings)
    {
        _localizationService = localizationService;
        _dateTimeHelper = dateTimeHelper;
        _addressService = addressService;
        _customerService = customerService;
        _countryService = countryService;
        _stateProvinceService = stateProvinceService;
        _addressModelFactory = addressModelFactory;
        _addressSettings = addressSettings;
        _erpAccountService = erpAccountService;
        _erpSalesOrgService = erpSalesOrgService;
        _erpNopUserService = erpNopUserService;
        _addressAttributeFormatter = addressAttributeFormatter;
        _erpNopUserAccountMapService = erpNopUserAccountMapService;
        _erpShipToAddressService = erpShipToAddressService;
        _b2BB2CFeaturesSettings = b2BB2CFeaturesSettings;
        _genericAttributeService = genericAttributeService;
        _aclSupportedModelFactory = aclSupportedModelFactory;
        _customerRepository = customerRepository;
        _erpNopUserRepository = erpNopUserRepository;
        _erpSalesRepRepository = erpSalesRepRepository;
        _customerCustomerRoleMappingRepository = customerCustomerRoleMappingRepository;
        _overridenCustomerModelFactory = overridenCustomerModelFactory;
        _b2BExportImportManager = b2BExportImportManager;
        _workContext = workContext;
        _catalogSettings = catalogSettings;
    }

    #endregion

    #region Utilities

    protected async Task<string> PrepareModelAddressHtmlAsync(AddressModel model, Address address, bool singleLine = true)
    {
        ArgumentNullException.ThrowIfNull(model);

        var addressHtmlSb = new StringBuilder();

        if (singleLine)
        {
            if (_addressSettings.CompanyEnabled && !string.IsNullOrEmpty(model.Company))
                addressHtmlSb.Append(model.Company);

            if (_addressSettings.StreetAddressEnabled && !string.IsNullOrEmpty(model.Address1))
                addressHtmlSb.Append(", " + model.Address1);

            if (_addressSettings.StreetAddress2Enabled && !string.IsNullOrEmpty(model.Address2))
                addressHtmlSb.Append(" " + model.Address2);

            if (_addressSettings.CityEnabled && !string.IsNullOrEmpty(model.City))
                addressHtmlSb.Append(", " + model.City);

            if (_addressSettings.CountyEnabled && !string.IsNullOrEmpty(model.County))
                addressHtmlSb.Append(", " + model.County);

            if (
                _addressSettings.StateProvinceEnabled
                && !string.IsNullOrEmpty(model.StateProvinceName)
            )
                addressHtmlSb.Append(", " + model.StateProvinceName);

            if (_addressSettings.ZipPostalCodeEnabled && !string.IsNullOrEmpty(model.ZipPostalCode))
                addressHtmlSb.Append(", " + model.ZipPostalCode);

            if (_addressSettings.CountryEnabled && !string.IsNullOrEmpty(model.CountryName))
                addressHtmlSb.Append(", " + model.CountryName);
        }
        else
        {
            addressHtmlSb = new StringBuilder("<div>");

            if (_addressSettings.CompanyEnabled && !string.IsNullOrEmpty(model.Company))
                addressHtmlSb.AppendFormat("{0}<br />", WebUtility.HtmlEncode(model.Company));

            if (_addressSettings.StreetAddressEnabled && !string.IsNullOrEmpty(model.Address1))
                addressHtmlSb.AppendFormat("{0}<br />", WebUtility.HtmlEncode(model.Address1));

            if (_addressSettings.StreetAddress2Enabled && !string.IsNullOrEmpty(model.Address2))
                addressHtmlSb.AppendFormat("{0}<br />", WebUtility.HtmlEncode(model.Address2));

            if (_addressSettings.CityEnabled && !string.IsNullOrEmpty(model.City))
                addressHtmlSb.AppendFormat("{0},", WebUtility.HtmlEncode(model.City));

            if (_addressSettings.CountyEnabled && !string.IsNullOrEmpty(model.County))
                addressHtmlSb.AppendFormat("{0},", WebUtility.HtmlEncode(model.County));

            if (
                _addressSettings.StateProvinceEnabled
                && !string.IsNullOrEmpty(model.StateProvinceName)
            )
                addressHtmlSb.AppendFormat("{0},", WebUtility.HtmlEncode(model.StateProvinceName));

            if (_addressSettings.ZipPostalCodeEnabled && !string.IsNullOrEmpty(model.ZipPostalCode))
                addressHtmlSb.AppendFormat("{0}<br />", WebUtility.HtmlEncode(model.ZipPostalCode));

            if (_addressSettings.CountryEnabled && !string.IsNullOrEmpty(model.CountryName))
                addressHtmlSb.AppendFormat("{0}", WebUtility.HtmlEncode(model.CountryName));

            var customAttributesFormatted = await _addressAttributeFormatter.FormatAttributesAsync(
                address?.CustomAttributes
            );
            if (!string.IsNullOrEmpty(customAttributesFormatted))
            {
                //already encoded
                addressHtmlSb.AppendFormat("<br />{0}", customAttributesFormatted);
            }

            addressHtmlSb.Append("</div>");
        }

        return addressHtmlSb.ToString();
    }

    public async Task<List<SelectListItem>> PrepareShipToAddressDropdownAsync(
        int accountId,
        int customerId = 0
    )
    {
        var availableErpShipToAddresses = new List<SelectListItem>();
        if (accountId > 0)
        {
            var shipToAddresses = new List<ErpShipToAddress>();

            if (_b2BB2CFeaturesSettings.UseDefaultAccountForB2CUser && customerId > 0)
            {
                var erpNopUser = await _erpNopUserService.GetErpNopUserByCustomerIdAsync(
                    customerId, showHidden: false
                );

                if (erpNopUser != null && erpNopUser.ErpUserType == ErpUserType.B2CUser)
                {
                    shipToAddresses =
                        await _erpShipToAddressService.GetErpShipToAddressesByCustomerAddressesAsync(
                            customerId: customerId,
                            erpAccountId: accountId,
                            erpShipToAddressCreatedByTypeId: (int)ErpShipToAddressCreatedByType.User
                        );
                }
                else
                {
                    shipToAddresses =
                        (List<ErpShipToAddress>)
                            await _erpShipToAddressService.GetErpShipToAddressesByErpAccountIdAsync(
                                accountId
                            );
                }
            }
            else
            {
                shipToAddresses =
                    (List<ErpShipToAddress>)
                        await _erpShipToAddressService.GetErpShipToAddressesByErpAccountIdAsync(
                            accountId
                        );
            }

            foreach (var shipToAddress in shipToAddresses)
            {
                var address = await _addressService.GetAddressByIdAsync(shipToAddress.AddressId);
    
                var addressModel = address.ToModel<AddressModel>();

                addressModel.CountryName = (
                    await _countryService.GetCountryByAddressAsync(address)
                )?.Name;
                addressModel.StateProvinceName = (
                    await _stateProvinceService.GetStateProvinceByAddressAsync(address)
                )?.Name;

                var selectListItem = new SelectListItem
                {
                    Value = $"{shipToAddress.Id}",
                    Text = $"{shipToAddress.ShipToName} - {await PrepareModelAddressHtmlAsync(addressModel, address)}"
                };
                availableErpShipToAddresses.Add(selectListItem);
            }
        }

        availableErpShipToAddresses.Insert(
            0,
            new SelectListItem
            {
                Value = "0",
                Text = await _localizationService.GetResourceAsync(
                    "Plugin.Misc.NopStation.ERPIntegrationCore.ErpNopUser.Select"
                ),
            }
        );

        return availableErpShipToAddresses;
    }

    #endregion

    #region Method
    public async Task<CustomerSearchModelForErpuser> PrepareCustomerSearchModelForErpUser(CustomerSearchModelForErpuser searchModel)
    {
        ArgumentNullException.ThrowIfNull(searchModel);

        //search registered customers by default
        var registeredRole = await _customerService.GetCustomerRoleBySystemNameAsync(NopCustomerDefaults.RegisteredRoleName);
        if (registeredRole != null)
            searchModel.SelectedCustomerRoleIds.Add(registeredRole.Id);

        //prepare available customer roles
        await _aclSupportedModelFactory.PrepareModelCustomerRolesAsync(searchModel);

        //set not to include deleted erp user and to include sales rep
        searchModel.IncludeDeletedErpUser = false;
        searchModel.IncludeDeletedSalesRep = true;

        //prepare page parameters
        searchModel.SetGridPageSize();

        return searchModel;
    }
    public async Task<CustomerListModel> PrepareCustomertListModelForErpUser(CustomerSearchModelForErpuser searchModel)
    {
        ArgumentNullException.ThrowIfNull(searchModel);

        //get parameters to filter customers
        _ = int.TryParse(searchModel.SearchDayOfBirth, out var dayOfBirth);
        _ = int.TryParse(searchModel.SearchMonthOfBirth, out var monthOfBirth);

        //get customers

        var query = _customerRepository.Table.Where(c => !c.Deleted);

        if (!string.IsNullOrEmpty(searchModel.SearchEmail))
            query = query.Where(c => c.Email.Contains(searchModel.SearchEmail));
        if (!string.IsNullOrEmpty(searchModel.SearchUsername))
            query = query.Where(c => c.Username.Contains(searchModel.SearchUsername));
        if (!string.IsNullOrEmpty(searchModel.SearchPhone))
            query = query.Where(c => c.Phone.Contains(searchModel.SearchPhone));
        if (!string.IsNullOrEmpty(searchModel.SearchIpAddress))
            query = query.Where(c => c.LastIpAddress.Contains(searchModel.SearchIpAddress));
        if (!string.IsNullOrEmpty(searchModel.SearchFirstName))
            query = query.Where(c => c.FirstName.Contains(searchModel.SearchFirstName));
        if (!string.IsNullOrEmpty(searchModel.SearchLastName))
            query = query.Where(c => c.LastName.Contains(searchModel.SearchLastName));
        if (!string.IsNullOrEmpty(searchModel.SearchCompany))
            query = query.Where(c => c.Company.Contains(searchModel.SearchCompany));
        if (!string.IsNullOrEmpty(searchModel.SearchZipPostalCode))
            query = query.Where(c => c.ZipPostalCode.Contains(searchModel.SearchZipPostalCode));
        if (dayOfBirth > 0 || monthOfBirth > 0)
        {
            query = query.Where(c =>
               c.DateOfBirth.HasValue &&
               (dayOfBirth == 0 || c.DateOfBirth.Value.Day == dayOfBirth) &&
               (monthOfBirth == 0 || c.DateOfBirth.Value.Month == monthOfBirth));
        }

        // Join with ErpNopUser
        var erpNopUserInfo = _erpNopUserRepository.Table;
        if (!searchModel.IncludeDeletedErpUser)
        {
            query = from c in query
                    join nopUser in erpNopUserInfo on c.Id equals nopUser.NopCustomerId into nopUserJoined
                    from nopUser in nopUserJoined.DefaultIfEmpty()
                    where nopUser == null
                    select c;
        }
        else
        {
            query = from c in query
                    join nopUser in erpNopUserInfo on c.Id equals nopUser.NopCustomerId into nopUserJoined
                    from nopUser in nopUserJoined.DefaultIfEmpty()
                    where nopUser == null || nopUser.IsDeleted
                    select c;
        }

        // Join with ErpSalesRep
        var salesRepRepo = _erpSalesRepRepository.Table;
        if (!searchModel.IncludeDeletedSalesRep)
        {
            query = from c in query
                    join s in salesRepRepo on c.Id equals s.NopCustomerId into sJoined
                    from s in sJoined.DefaultIfEmpty()
                    where s == null
                    select c;
        }
        else
        {
            query = from c in query
                    join s in salesRepRepo on c.Id equals s.NopCustomerId into sJoined
                    from s in sJoined.DefaultIfEmpty()
                    where s == null || s.IsDeleted
                    select c;
        }

        // Filter by roles
        if (searchModel.SelectedCustomerRoleIds?.Any() == true)
        {
            query = (from c in query
                     join crm in _customerCustomerRoleMappingRepository.Table
                         on c.Id equals crm.CustomerId
                     where searchModel.SelectedCustomerRoleIds.Contains(crm.CustomerRoleId)
                     select c).Distinct();
        }

        var customers = query.ToList();

        customers = customers.OrderByDescending(c => c.CreatedOnUtc).ToList();

        //prepare list model
        var pagedCustomers = new PagedList<Customer>(customers, searchModel.Page - 1, searchModel.PageSize);

        var model = await new CustomerListModel().PrepareToGridAsync<CustomerListModel, CustomerModel, Customer>(
            searchModel,
            pagedCustomers,
            () => _overridenCustomerModelFactory.PrepareCustomerModelsAsync(pagedCustomers)
        );


        return model;
    }

    public async Task<ErpNopUserSearchModel> PrepareErpNopUserSearchModelAsync(
        ErpNopUserSearchModel searchModel
    )
    {
        ArgumentNullException.ThrowIfNull(searchModel);

        var availableErpUserTypes = await ErpUserType.B2BUser.ToSelectListAsync(false);
        foreach (var types in availableErpUserTypes)
        {
            var enumValue = Enum.Parse(typeof(ErpUserType), types.Value);
            var resourceKey = $"Plugin.Misc.NopStation.ERPIntegrationCore.ErpNopUser.{enumValue}";
            types.Text = await _localizationService.GetResourceAsync(resourceKey);
            searchModel.AvailableErpUserTypes.Add(types);
        }
        searchModel.AvailableErpUserTypes.Insert(
            0,
            new SelectListItem
            {
                Value = "0",
                Text = await _localizationService.GetResourceAsync(
                    "Plugin.Misc.NopStation.ERPIntegrationCore.ErpNopUser.Select"
                ),
            }
        );

        searchModel.AvailableErpSalesOrgs = (await _erpSalesOrgService.GetAllErpSalesOrgAsync(showHidden: false))
            .Select(erpSalesOrg => new SelectListItem
            {
                Value = $"{erpSalesOrg.Id}",
                Text = $"{erpSalesOrg.Name}"
            }).ToList();

        searchModel.AvailableErpSalesOrgs.Insert(
            0,
            new SelectListItem
            {
                Value = "0",
                Text = await _localizationService.GetResourceAsync(
                    "Plugin.Misc.NopStation.ERPIntegrationCore.ErpNopUser.Select"
                ),
            }
        );

        var erpAccounts = await _erpAccountService.GetErpAccountListAsync(showHidden: false);        

        if (searchModel.NopUserId > 0)
        {
            var mappedAccounts =
                await _erpNopUserAccountMapService.GetAllErpNopUserAccountMapsByUserIdAsync(
                    searchModel.NopUserId
                );

            if (mappedAccounts.Any())
            {
                var mappedAccountIds = mappedAccounts.Select(x => x.ErpAccountId);
                foreach (var erpAccount in erpAccounts.Where(w => mappedAccountIds.Contains(w.Id)))
                {
                    var erpSalesOrg = await _erpSalesOrgService.GetErpSalesOrgByIdAsync(
                        erpAccount.ErpSalesOrgId
                    );
                    var selectListItem = new SelectListItem
                    {
                        Value = $"{erpAccount.Id}",
                        Text = $"{erpAccount.AccountNumber} ({erpSalesOrg?.Name})"
                    };
                    searchModel.AvailableErpAccountsForAUser.Add(selectListItem);
                }
            }

            //prepare available customer roles
            var availableRoles = await _customerService.GetAllCustomerRolesAsync(showHidden: true);
            var customerRoleIds = Array.Empty<int>();

            var erpNopUser = await _erpNopUserService.GetErpNopUserByIdAsync(searchModel.NopUserId);
            var customer = await _customerService.GetCustomerByIdAsync(erpNopUser.NopCustomerId);
            customerRoleIds = await _customerService.GetCustomerRoleIdsAsync(customer);

            searchModel.AvailableCustomerRoles = availableRoles.Select(role => new SelectListItem
            {
                Text = role.Name,
                Value = $"{role.Id}",
                Selected = customerRoleIds.Contains(role.Id)
            }).ToList();
        }

        searchModel.AvailableErpAccountsForAUser.Insert(
            0,
            new SelectListItem
            {
                Value = "0",
                Text = await _localizationService.GetResourceAsync(
                    "Plugin.Misc.NopStation.ERPIntegrationCore.ErpNopUser.Select"
                ),
            }
        );

        searchModel.AvailableErpShipToAddresses = await PrepareShipToAddressDropdownAsync(searchModel.AccountId);

        //prepare "active" filter (0 - all; 1 - active only; 2 - inactive only)
        searchModel.ShowInActiveOption.Add(
            new SelectListItem
            {
                Value = "0",
                Text = await _localizationService.GetResourceAsync(
                    "Plugin.Misc.NopStation.ERPIntegrationCore.ErpNopUserSearchModel.ShowAll"
                ),
            }
        );
        searchModel.ShowInActiveOption.Add(
            new SelectListItem
            {
                Value = "1",
                Text = await _localizationService.GetResourceAsync(
                    "Plugin.Misc.NopStation.ERPIntegrationCore.ErpNopUserSearchModel.ShowOnlyActive"
                ),
            }
        );
        searchModel.ShowInActiveOption.Add(
            new SelectListItem
            {
                Value = "2",
                Text = await _localizationService.GetResourceAsync(
                    "Plugin.Misc.NopStation.ERPIntegrationCore.ErpNopUserSearchModel.ShowOnlyInactive"
                ),
            }
        );
        searchModel.ShowInActive = 1;

        //prepare grid
        searchModel.SetGridPageSize();

        return searchModel;
    }

    public async Task<ErpNopUserListModel> PrepareErpNopUserListModelAsync(
        ErpNopUserSearchModel searchModel
    )
    {
        ArgumentNullException.ThrowIfNull(searchModel);

        var erpNopUsers = await _erpNopUserService.GetAllErpNopUsersAsync(pageIndex: searchModel.Page - 1,
            pageSize: searchModel.PageSize,
            showHidden: searchModel.ShowInActive == 0 ? null : (searchModel.ShowInActive == 2),
            email: searchModel.Email,
            accountId: searchModel.AccountId,
            firstName: searchModel.FirstName,
            lastName: searchModel.LastName,
            erpShipToAddressId: searchModel.ErpShipToAddressId,
            shipToCode: searchModel.ShipToCode,
            userType: searchModel.ErpUserTypeId,
            salesOrgId: searchModel.SalesOrgId
        );

        var model = await new ErpNopUserListModel().PrepareToGridAsync(searchModel, erpNopUsers, () =>
        {
            return erpNopUsers.SelectAwait(async erpNopUser =>
            {
                var erpNopUserModel = new ErpNopUserModel();

                if (erpNopUser != null)
                {                    
                    var nopCustomer = await _customerService.GetCustomerByIdAsync(erpNopUser.NopCustomerId);

                    var erpAccount = await _erpAccountService.GetErpAccountByIdAsync(erpNopUser.ErpAccountId);
                    if (erpAccount == null)
                        return null;

                    var erpSalesOrg = await _erpSalesOrgService.GetErpSalesOrgByIdAsync(erpAccount.ErpSalesOrgId);
                    if (erpSalesOrg == null)
                        return null;

                    var erpShipToAddress = await _erpShipToAddressService.GetErpShipToAddressByIdAsync(erpNopUser.ErpShipToAddressId);
                    if (erpShipToAddress == null)
                        return null;

                    var addressOfShipToAddress = await _addressService.GetAddressByIdAsync(erpShipToAddress.AddressId);

                    var addressModelOfShipToAddress = new AddressModel();
                    if (addressOfShipToAddress != null)
                        addressModelOfShipToAddress = addressOfShipToAddress.ToModel(addressModelOfShipToAddress);

                    await _addressModelFactory.PrepareAddressModelAsync(addressModelOfShipToAddress, addressOfShipToAddress);
                    addressModelOfShipToAddress.AddressHtml = await PrepareModelAddressHtmlAsync(addressModelOfShipToAddress, addressOfShipToAddress, false);

                    var billingErpShipToAddress = await _addressService.GetAddressByIdAsync(erpNopUser.BillingErpShipToAddressId);
                    var billingErpShipToAddressModel = new AddressModel();

                        if (billingErpShipToAddress != null)
                            billingErpShipToAddressModel = billingErpShipToAddress.ToModel(
                                billingErpShipToAddressModel
                            );
                        await _addressModelFactory.PrepareAddressModelAsync(
                            billingErpShipToAddressModel,
                            billingErpShipToAddress
                        );

                    var shippingErpShipToAddress = await _addressService.GetAddressByIdAsync(erpNopUser.ShippingErpShipToAddressId);
                    var shippingErpShipToAddressModel = new AddressModel();

                        if (shippingErpShipToAddress != null)
                            shippingErpShipToAddressModel = shippingErpShipToAddress.ToModel(
                                shippingErpShipToAddressModel
                            );
                        await _addressModelFactory.PrepareAddressModelAsync(
                            shippingErpShipToAddressModel,
                            shippingErpShipToAddress
                        );

                    var selectedCustomerRoles = await _customerService.GetCustomerRolesAsync(nopCustomer);

                    erpNopUserModel = new ErpNopUserModel
                    {
                        Id = erpNopUser.Id,
                        NopCustomerId = erpNopUser.NopCustomerId,
                        NopCustomer = $"{nopCustomer.FirstName} {nopCustomer.LastName}",
                        NopCustomerEmail = nopCustomer.Email,
                        ErpAccountId = erpNopUser.ErpAccountId,
                        ErpAccountInfo = $"{erpAccount.AccountName} ({erpAccount.AccountNumber})",
                        ErpSalesOrgId = erpSalesOrg.Id,
                        ErpSalesOrg = $"{erpSalesOrg.Name} - ({erpSalesOrg.Code})",
                        ErpShipToAddressId = erpNopUser.ErpShipToAddressId,
                        ErpShipToAddress = addressModelOfShipToAddress,
                        ShipToCode = erpShipToAddress.ShipToCode,
                        BillingErpShipToAddressId = erpNopUser.BillingErpShipToAddressId,
                        BillingErpShipToAddress = billingErpShipToAddressModel,
                        ShippingErpShipToAddressId = erpNopUser.ShippingErpShipToAddressId,
                        ShippingErpShipToAddress = shippingErpShipToAddressModel,
                        ErpUserTypeId = erpNopUser.ErpUserTypeId,
                        ErpUserType = $"{((ErpUserType)erpNopUser.ErpUserTypeId)}",
                        CreatedBy = $"{erpNopUser.CreatedById}",
                        UpdatedBy = $"{erpNopUser.UpdatedById}",
                        IsActive = erpNopUser.IsActive,
                        CreatedOn = await _dateTimeHelper.ConvertToUserTimeAsync(erpNopUser.CreatedOnUtc, DateTimeKind.Utc),
                        UpdatedOn = await _dateTimeHelper.ConvertToUserTimeAsync(erpNopUser.UpdatedOnUtc, DateTimeKind.Utc),
                        SelectedCustomerRoleIds = selectedCustomerRoles.Select(x => x.Id).ToList(),
                        SelectedCustomerRoles = string.Join(", ", selectedCustomerRoles.Select(x => x.Name))
                    };
                }

                return erpNopUserModel;
            }).Where(x => x != null);
        });

        return model;
    }
    
    public async Task<ErpNopUserAccountListModel> PrepareErpNopUserAccountListModelAsync(int nopUserId, ErpNopUserSearchModel searchModel)
    {
        if (nopUserId <= 0)
            throw new ArgumentNullException();

        var mappedAccounts = await _erpNopUserAccountMapService.GetAllErpNopUserAccountMapsByUserIdAsync(nopUserId);
        var mappedAccountIds = mappedAccounts.Select(x => x.ErpAccountId).ToList();
        var defaultErpAccountId = (await _erpNopUserService.GetErpNopUserByIdAsync(nopUserId)).ErpAccountId;

        var erpSalesOrgs = await _erpSalesOrgService.GetErpSalesOrgsAsync(isActive: false, filterOutDeleted: true);

        var erpAccounts = await _erpAccountService.GetAllErpAccountsByIdsAsync(
            pageIndex: searchModel.Page - 1, 
            pageSize: searchModel.PageSize, 
            showHidden: true, 
            getOnlyTotalCount: false,
            mappedAccountIds.Contains(searchModel.AccountId) ? [searchModel.AccountId] : mappedAccountIds, 
            email: searchModel.Email);

        var model = await new ErpNopUserAccountListModel().PrepareToGridAsync(searchModel, erpAccounts, () =>
        {
            return erpAccounts.SelectAwait(async erpAccount =>
            {
                var address = await _addressService.GetAddressByIdAsync(erpAccount.BillingAddressId ?? 0);
                var addressModel = new AddressModel();
                if (address != null)
                    addressModel = address.ToModel(addressModel);
                await _addressModelFactory.PrepareAddressModelAsync(addressModel, address);

                var erpSalesOrg = erpSalesOrgs.FirstOrDefault(x => x.Id == erpAccount.ErpSalesOrgId);

                var erpNopUserAccountModel = new ErpNopUserAccountModel
                {
                    Id = erpAccount.Id,
                    AccountNumber = erpAccount.AccountNumber,
                    AccountName = erpAccount.AccountName,
                    VatNumber = erpAccount.VatNumber,
                    CurrentBalance = erpAccount.CurrentBalance,
                    ErpSalesOrgId = erpAccount.ErpSalesOrgId,
                    ErpSalesOrg = $"{erpSalesOrg?.Name} ({erpSalesOrg?.Code})",
                    BillingAddressId = erpAccount.BillingAddressId,
                    BillingAddress = addressModel,
                    BillingSuburb = erpAccount.BillingSuburb,
                    CreditLimit = erpAccount.CreditLimit,
                    CreditLimitAvailable = erpAccount.CreditLimitAvailable,
                    LastPaymentAmount = erpAccount.LastPaymentAmount,
                    LastPaymentDate = erpAccount.LastPaymentDate,
                    AllowOverspend = erpAccount.AllowOverspend,
                    PreFilterFacets = erpAccount.PreFilterFacets,
                    PaymentTypeCode = erpAccount.PaymentTypeCode,
                    OverrideAddressEditOnCheckoutConfigSetting = erpAccount.OverrideAddressEditOnCheckoutConfigSetting,
                    OverrideBackOrderingConfigSetting = erpAccount.OverrideBackOrderingConfigSetting,
                    AllowAccountsAddressEditOnCheckout = erpAccount.AllowAccountsAddressEditOnCheckout,
                    AllowAccountsBackOrdering = erpAccount.AllowAccountsBackOrdering,
                    OverrideStockDisplayFormatConfigSetting = erpAccount.OverrideStockDisplayFormatConfigSetting,
                    ErpAccountStatusTypeId = erpAccount.ErpAccountStatusTypeId,
                    ErpAccountStatusType = ((ErpAccountStatusType)erpAccount.ErpAccountStatusTypeId).ToString(),
                    LastErpAccountSyncDate = erpAccount.LastErpAccountSyncDate,
                    B2BPriceGroupCodeId = erpAccount.B2BPriceGroupCodeId,
                    TotalSavingsForthisYear = erpAccount.TotalSavingsForthisYear ?? 0,
                    TotalSavingsForAllTime = erpAccount.TotalSavingsForAllTime ?? 0,
                    TotalSavingsForAllTimeUpdatedOnUtc = erpAccount.TotalSavingsForAllTimeUpdatedOnUtc,
                    TotalSavingsForthisYearUpdatedOnUtc = erpAccount.TotalSavingsForthisYearUpdatedOnUtc,
                    LastTimeOrderSyncOnUtc = erpAccount.LastTimeOrderSyncOnUtc,
                    CreatedOn = await _dateTimeHelper.ConvertToUserTimeAsync(erpAccount.CreatedOnUtc, DateTimeKind.Utc),
                    UpdatedOn = await _dateTimeHelper.ConvertToUserTimeAsync(erpAccount.UpdatedOnUtc, DateTimeKind.Utc),
                    IsActive = erpAccount.IsActive,
                    IsDefault = erpAccount.Id == defaultErpAccountId
                };

                return erpNopUserAccountModel;
            });
        });

        return model;
    }

    public async Task<ErpNopUserModel> PrepareErpNopUserModelAsync(
        ErpNopUserModel model,
        ErpNopUser erpNopUser
    )
    {
        if (erpNopUser != null)
        {
            var nopCustomer = await _customerService.GetCustomerByIdAsync(erpNopUser.NopCustomerId);

            var erpAccount = await _erpAccountService.GetErpAccountByIdAsync(erpNopUser.ErpAccountId);
            var erpSalesOrg = await _erpSalesOrgService.GetErpSalesOrgByIdAsync(erpAccount.ErpSalesOrgId);

            model ??= new ErpNopUserModel();

            model.Id = erpNopUser.Id;
            model.NopCustomerId = erpNopUser.NopCustomerId;
            model.NopCustomer = $"{nopCustomer?.FirstName} {nopCustomer?.LastName}";
            model.NopCustomerEmail = nopCustomer?.Email;
            model.ErpAccountId = erpNopUser.ErpAccountId;
            model.ErpAccountInfo = $"{erpAccount?.AccountName} ({erpAccount?.AccountNumber})";
            model.ErpSalesOrgId = erpSalesOrg.Id;
            model.ErpSalesOrg = $"{erpSalesOrg.Name} - ({erpSalesOrg.Code})";
            model.ErpShipToAddressId = erpNopUser.ErpShipToAddressId;
            model.BillingErpShipToAddressId = erpNopUser.BillingErpShipToAddressId;
            model.ShippingErpShipToAddressId = erpNopUser.ShippingErpShipToAddressId;
            model.ErpUserTypeId = erpNopUser.ErpUserTypeId;
            model.ErpUserType = ((ErpUserType)erpNopUser.ErpUserTypeId).ToString();
            model.CreatedBy = $"{erpNopUser.CreatedById}";
            model.UpdatedBy = $"{erpNopUser.UpdatedById}";
            model.IsActive = erpNopUser.IsActive;
            model.CreatedOn = await _dateTimeHelper.ConvertToUserTimeAsync(
                erpNopUser.CreatedOnUtc,
                DateTimeKind.Utc
            );
            model.UpdatedOn = await _dateTimeHelper.ConvertToUserTimeAsync(
                erpNopUser.UpdatedOnUtc,
                DateTimeKind.Utc
            );

            var dateOfTermsAndConditionChecked =
                await _genericAttributeService.GetAttributeAsync<DateTime?>(
                    nopCustomer,
                    ERPIntegrationCoreDefaults.CustomerDateOfTermsAndConditionCheckedAttributeName,
                    defaultValue: null
                );

            model.DateOfTermsAndConditionChecked = !dateOfTermsAndConditionChecked.HasValue
                ? dateOfTermsAndConditionChecked
                : await _dateTimeHelper.ConvertToUserTimeAsync(
                    dateOfTermsAndConditionChecked.Value,
                    DateTimeKind.Utc
                );

            if (model.DateOfTermsAndConditionChecked.HasValue)
            {
                model.IsDateOfTermsAndConditionChecked = true;
            }

            var selectedCustomerRoles = await _customerService.GetCustomerRolesAsync(nopCustomer);

            model.SelectedCustomerRoleIds = selectedCustomerRoles.Select(x => x.Id).ToList();
            model.SelectedCustomerRoles = string.Join(", ", selectedCustomerRoles.Select(x => x.Name));

            //prepare nested search model
            model.ErpNopUserSearchModel.NopUserId = erpNopUser.Id;
            await PrepareErpNopUserSearchModelAsync(model.ErpNopUserSearchModel);
        }

        #region Dropdowns

        // Prepare AvailableErpUserTypes dropdown options
        var availableErpUserTypes = await ErpUserType.B2BUser.ToSelectListAsync(false);
        foreach (var types in availableErpUserTypes)
        {
            var enumValue = Enum.Parse(typeof(ErpUserType), types.Value);
            var resourceKey = $"Plugin.Misc.NopStation.ERPIntegrationCore.ErpNopUser.{enumValue}";
            types.Text = await _localizationService.GetResourceAsync(resourceKey);
            model.AvailableErpUserTypes.Add(types);
        }
        model.AvailableErpUserTypes.Insert(
            0,
            new SelectListItem
            {
                Value = "0",
                Text = await _localizationService.GetResourceAsync(
                    "Plugin.Misc.NopStation.ERPIntegrationCore.ErpNopUser.Select"
                ),
            }
        );

        // Prepare NopCustomers dropdown options
        var customerRoleId = (
            await _customerService.GetCustomerRoleBySystemNameAsync(
                NopCustomerDefaults.RegisteredRoleName
            )
        ).Id;
        var customerIdsWithOnlyRegisteredRole =
            await _erpNopUserService.GetAllCustomersByOnlyTheseRoleIdsAsync(customerRoleId);

        var existingErpNopUsersNopCustomerIds = await _erpNopUserService.GetAllErpNopUsersCustomerIds();
        if (customerIdsWithOnlyRegisteredRole.Count > 0)
        {
            model.AvailableNopCustomers = (await _customerService.GetCustomersByIdsAsync(customerIdsWithOnlyRegisteredRole.ToArray()))
            .Where(w => !existingErpNopUsersNopCustomerIds.Contains(w.Id))
            .Select(nopCustomer => new SelectListItem
            {
                Value = $"{nopCustomer.Id}",
                Text = $"{nopCustomer.FirstName} {nopCustomer.LastName} ({nopCustomer.Email})"
            }).ToList();
        }
        model.AvailableNopCustomers.Insert(0, new SelectListItem
        {
            Value = "0",
            Text = await _localizationService.GetResourceAsync("Plugin.Misc.NopStation.ERPIntegrationCore.ErpNopUser.Select")
        });

        var customerRoleIds = Array.Empty<int>();
        if (erpNopUser != null && erpNopUser.NopCustomerId > 0)
        {
            var currentErpUsersNopCustomer = await _customerService.GetCustomerByIdAsync(erpNopUser.NopCustomerId);
            model.AvailableNopCustomers.Insert(1, new SelectListItem
            {
                Value = $"{currentErpUsersNopCustomer?.Id}",
                Text = $"{currentErpUsersNopCustomer?.FirstName} {currentErpUsersNopCustomer?.LastName}"
            });
            customerRoleIds = await _customerService.GetCustomerRoleIdsAsync(currentErpUsersNopCustomer);
        }

        var availableRoles = await _customerService.GetAllCustomerRolesAsync();
        model.AvailableCustomerRoles = availableRoles
        .Where(role =>
            role.SystemName != NopCustomerDefaults.AdministratorsRoleName && 
            role.SystemName != NopCustomerDefaults.ForumModeratorsRoleName &&
            role.SystemName != NopCustomerDefaults.GuestsRoleName &&
            role.SystemName != NopCustomerDefaults.VendorsRoleName
        )
        .Select(role => new SelectListItem
        {
            Text = role.Name,
            Value = $"{role.Id}",
            Selected = customerRoleIds.Contains(role.Id)
        }).ToList();

        #endregion

        #region Addresses

        //prepare ErpShipToAddress model
        var erpShipToAddress = await _addressService.GetAddressByIdAsync(
            erpNopUser?.ErpShipToAddressId ?? 0
        );
        var erpShipToAddressModel = new AddressModel();
        if (erpShipToAddress != null)
            erpShipToAddressModel = erpShipToAddress.ToModel(erpShipToAddressModel);
        await _addressModelFactory.PrepareAddressModelAsync(
            erpShipToAddressModel,
            erpShipToAddress
        );

        //prepare BillingErpShipToAddress model
        var billingErpShipToAddress = await _addressService.GetAddressByIdAsync(
            erpNopUser?.BillingErpShipToAddressId ?? 0
        );
        var billingErpShipToAddressModel = new AddressModel();
        if (billingErpShipToAddress != null)
            billingErpShipToAddressModel = billingErpShipToAddress.ToModel(
                billingErpShipToAddressModel
            );
        await _addressModelFactory.PrepareAddressModelAsync(
            billingErpShipToAddressModel,
            billingErpShipToAddress
        );

        //prepare ShippingErpShipToAddress model
        var shippingErpShipToAddress = await _addressService.GetAddressByIdAsync(
            erpNopUser?.ShippingErpShipToAddressId ?? 0
        );
        var shippingErpShipToAddressModel = new AddressModel();
        if (shippingErpShipToAddress != null)
            shippingErpShipToAddressModel = shippingErpShipToAddress.ToModel(
                shippingErpShipToAddressModel
            );
        await _addressModelFactory.PrepareAddressModelAsync(
            shippingErpShipToAddressModel,
            shippingErpShipToAddress
        );

        model.ErpShipToAddress = erpShipToAddressModel;
        model.BillingErpShipToAddress = billingErpShipToAddressModel;
        model.ShippingErpShipToAddress = shippingErpShipToAddressModel;
        model.IsActive = erpNopUser is null || erpNopUser.IsActive;

        #endregion

        return model;
    }

    public async Task<ErpNopUserAccountMapModel> PrepareErpNopUserModelAsync(ErpNopUserAccountMapModel model, ErpNopUserAccountMap erpNopUserAccountMap)
    {
        if (erpNopUserAccountMap != null)
        {
            //get Account
            var erpAccount = await _erpAccountService.GetErpAccountByIdAsync(
                erpNopUserAccountMap.ErpAccountId
            );

            //fill in model values from the entity
            model ??= new ErpNopUserAccountMapModel();

            model.Id = erpNopUserAccountMap.Id;
            model.ErpUserId = erpNopUserAccountMap.ErpUserId;
            model.ErpAccountId = erpNopUserAccountMap.ErpAccountId;
            if (erpAccount != null)
                model.ErpAccountNumber = erpAccount.AccountNumber;
        }

        return model;
    }

    #endregion

    #region Export/Excel

    public async Task<byte[]> ExportSelectedErpNopUsersToXlsxAsync(string ids =  null)
    {

        var sql = @"SELECT erpuser.[Id],
                           customer.[Email],
                           account.[AccountNumber],
                           account.[AccountName],
                           saleOrg.[Code] AS AccountSalesOrganisationCode,
                           shipto.[ShipToCode],
                           shipto.[ShipToName],
                           erpuser.[IsActive]
                    FROM [dbo].[Erp_Nop_User] erpuser
                    LEFT JOIN [dbo].[Customer] customer 
                        ON erpuser.[NopCustomerId] = customer.[Id]
                    LEFT JOIN [dbo].[Erp_Account] account 
                        ON erpuser.[ErpAccountId] = account.[Id]
                    LEFT JOIN [dbo].[Erp_Sales_Org] saleOrg 
                        ON account.[ErpSalesOrgId] = saleOrg.[Id]
                    LEFT JOIN [dbo].[Erp_ShipToAddress] shipto 
                        ON erpuser.[ErpShipToAddressId] = shipto.[Id]
                    WHERE erpuser.[Id] > 0
                      AND erpuser.[IsDeleted] = 0
                      AND customer.[Active] = 1
                      AND customer.[Deleted] = 0
                      AND erpuser.ErpUserTypeId = 5
                      AND erpuser.Id IN @Ids
                      ORDER BY account.Id";

        if (!string.IsNullOrEmpty(ids))
        {
            sql = sql.Replace("@Ids", $"({ids})");
        }

        var dataTable = await _b2BExportImportManager.GetXLWorkbookByQuery(sql, null);

        using (var workbook = new XLWorkbook())
        {
            var worksheet = workbook.Worksheets.Add("ErpNopUsers");

            // Load data into the worksheet
            worksheet.Cell(1, 1).InsertTable(dataTable);

            using (var stream = new MemoryStream())
            {
                workbook.SaveAs(stream);
                return stream.ToArray();
            }
        }
    }
    public async Task<byte[]> ExportAllErpNopUsersToXlsxAsync(ErpNopUserSearchModel searchModel)
    {

        var sql = new StringBuilder(@"SELECT erpuser.[Id],
                           customer.[Email],
                           account.[AccountNumber],
                           account.[AccountName],
                           saleOrg.[Code] AS AccountSalesOrganisationCode,
                           shipto.[ShipToCode],
                           shipto.[ShipToName],
                           erpuser.[IsActive]
                    FROM [dbo].[Erp_Nop_User] erpuser
                    LEFT JOIN [dbo].[Customer] customer 
                        ON erpuser.[NopCustomerId] = customer.[Id]
                    LEFT JOIN [dbo].[Erp_Account] account 
                        ON erpuser.[ErpAccountId] = account.[Id]
                    LEFT JOIN [dbo].[Erp_Sales_Org] saleOrg 
                        ON account.[ErpSalesOrgId] = saleOrg.[Id]
                    LEFT JOIN [dbo].[Erp_ShipToAddress] shipto 
                        ON erpuser.[ErpShipToAddressId] = shipto.[Id]
                    WHERE erpuser.[Id] > 0
                      AND erpuser.[IsDeleted] = 0
                      AND customer.[Active] = 1
                      AND customer.[Deleted] = 0
                      AND erpuser.ErpUserTypeId = 5");
        // Append filters
        bool? showHidden = searchModel.ShowInActive == 0 ? null : (searchModel.ShowInActive == 2);
        if (showHidden.HasValue)
            sql.Append(showHidden.Value ? " AND erpuser.IsActive = 0" : " AND erpuser.IsActive = 1");

        if (searchModel.AccountId > 0)
            sql.Append(" AND erpuser.ErpAccountId = @erpAccountId");

        if (!string.IsNullOrWhiteSpace(searchModel.FirstName))
            sql.Append(" AND customer.FirstName LIKE '%' + @firstName + '%'");

        if (!string.IsNullOrWhiteSpace(searchModel.LastName))
            sql.Append(" AND customer.LastName LIKE '%' + @lastName + '%'");

        if (!string.IsNullOrWhiteSpace(searchModel.Email))
            sql.Append(" AND customer.Email LIKE '%' + @email + '%'");

        if (!string.IsNullOrWhiteSpace(searchModel.ShipToCode))
            sql.Append(" AND shipto.ShipToCode LIKE '%' + @shipToCode + '%'");

        if (searchModel.ErpShipToAddressId > 0)
            sql.Append(" AND erpuser.ErpShipToAddressId = @erpShipToAddressId");

        sql.Append(" ORDER BY account.Id");


        var parameters = new
        {
            erpAccountId = searchModel.AccountId,
            firstName = searchModel.FirstName,
            lastName = searchModel.LastName,
            email = searchModel.Email,
            shipToCode = searchModel.ShipToCode,
            erpShipToAddressId = searchModel.ErpShipToAddressId
        };

        var dataTable = await _b2BExportImportManager.GetXLWorkbookByQuery(sql.ToString(), parameters);

        using (var workbook = new XLWorkbook())
        {
            var worksheet = workbook.Worksheets.Add("ErpNopUsers");

            // Load data into the worksheet
            worksheet.Cell(1, 1).InsertTable(dataTable);

            using (var stream = new MemoryStream())
            {
                workbook.SaveAs(stream);
                return stream.ToArray();
            }
        }

    }

    #endregion

    #region Import/Excel
    public static async Task<IList<PropertyByName<T, Language>>> GetPropertiesByExcelCellsAsync<T>(IXLWorksheet workbook)
    {
        var properties = new List<PropertyByName<T, Language>>();
        var poz = 1;
        while (true)
        {
            try
            {
                var x = workbook;
                var y = x.Cell(1, poz).Value;

                if (string.IsNullOrEmpty(y.ToString()))
                    break;

                poz += 1;
                properties.Add(new PropertyByName<T, Language>(y.ToString()));

            }
            catch
            {
                break;
            }
        }

        return properties;
    }
    protected virtual async Task<ErpNopUserExportImportModel> GetModelFromXlsxAsync(
   PropertyManager<ErpNopUserExportImportModel, Language> manager, IXLWorksheet worksheet, int iRow)
    {
        manager.ReadDefaultFromXlsx(worksheet, iRow);
        var model = new ErpNopUserExportImportModel();

        foreach (var property in manager.GetDefaultProperties)
        {
            switch (property.PropertyName)
            {
                case "Email":
                    model.Email = property.StringValue;
                    break;
                case "AccountNumber":
                    model.AccountNumber = property.StringValue;
                    break;
                case "AccountName":
                    model.AccountName = property.StringValue;
                    break;
                case "AccountSalesOrganisationCode":
                    model.AccountSalesOrganisationCode = property.StringValue;
                    break;
                case "ShipToCode":
                    model.ShipToCode = property.StringValue;
                    break;
                case "ShipToName":
                    model.ShipToName = property.StringValue;
                    break;
                case "IsActive":
                    model.IsActive = property.StringValue;
                    break;

            }
        }
        return model;
    }
    public async Task ImportErpNopUsersFromXlsxAsync(Stream stream)
    {

        var dataTable = new DataTable();

        using (var workbook = new XLWorkbook(stream))
        {
            var worksheet = workbook.Worksheets.FirstOrDefault();
            if (worksheet == null)
                throw new NopException("No workbook found");
            var properties = await GetPropertiesByExcelCellsAsync<ErpNopUserExportImportModel>(worksheet);

            // Pass the resolved list to the PropertyManager
            var manager = new PropertyManager<ErpNopUserExportImportModel, Language>(properties, _catalogSettings);

            var iRow = 2;
            dataTable.Columns.Add(new DataColumn("Email", typeof(string)));
            dataTable.Columns.Add(new DataColumn("AccountNumber", typeof(string)));
            dataTable.Columns.Add(new DataColumn("AccountName", typeof(string)));
            dataTable.Columns.Add(new DataColumn("AccountSalesOrganisationCode", typeof(string)));
            dataTable.Columns.Add(new DataColumn("ShipToCode", typeof(string)));
            dataTable.Columns.Add(new DataColumn("ShipToName", typeof(string)));
            dataTable.Columns.Add(new DataColumn("IsActive", typeof(string)));

            while (true)
            {
                var allColumnsAreEmpty = manager.GetDefaultProperties
                .Select(property => worksheet.Cell(iRow, property.PropertyOrderPosition))
                .All(cell => cell == null || string.IsNullOrEmpty(cell.GetValue<string>()));

                if (allColumnsAreEmpty)
                    break;

                var model = await GetModelFromXlsxAsync(manager, worksheet, iRow);
                var row = dataTable.NewRow();
                row[dataTable.Columns.IndexOf("Email")] = model.Email;
                row[dataTable.Columns.IndexOf("AccountNumber")] = model.AccountNumber;
                row[dataTable.Columns.IndexOf("AccountName")] = model.AccountName;
                row[dataTable.Columns.IndexOf("AccountSalesOrganisationCode")] = model.AccountSalesOrganisationCode;
                row[dataTable.Columns.IndexOf("ShipToCode")] = model.ShipToCode;
                row[dataTable.Columns.IndexOf("ShipToName")] = model.ShipToName;
                row[dataTable.Columns.IndexOf("IsActive")] = model.IsActive;
                dataTable.Rows.Add(row);
                iRow++;
            }
        }

        string connectionString = DataSettingsManager.LoadSettings().ConnectionString;

        using (var connection = new SqlConnection(connectionString))
        {
            await connection.OpenAsync();

            // Truncate staging table
            using (var truncateCmd = new SqlCommand("TRUNCATE TABLE [dbo].[ErpNopUserImport]", connection))
            {
                truncateCmd.CommandTimeout = 300;
                await truncateCmd.ExecuteNonQueryAsync();
            }

            // Insert each row
            foreach (DataRow row in dataTable.Rows)
            {
                using (var insertCmd = new SqlCommand(@"
                INSERT INTO [dbo].[ErpNopUserImport]
                (Email, AccountNumber, AccountName, AccountSalesOrganisationCode, ShipToCode, ShipToName, IsActive, ErpUserType)
                VALUES (@Email, @AccountNumber, @AccountName, @AccountSalesOrganisationCode, @ShipToCode, @ShipToName, @IsActive, @ErpUserType)",
                    connection))
                {
                    insertCmd.Parameters.AddWithValue("@Email",row["Email"]);
                    insertCmd.Parameters.AddWithValue("@AccountNumber",row["AccountNumber"]);
                    insertCmd.Parameters.AddWithValue("@AccountName", row["AccountName"]);
                    insertCmd.Parameters.AddWithValue("@AccountSalesOrganisationCode", row["AccountSalesOrganisationCode"]);
                    insertCmd.Parameters.AddWithValue("@ShipToCode", row["ShipToCode"]);
                    insertCmd.Parameters.AddWithValue("@ShipToName", row["ShipToName"]);
                    insertCmd.Parameters.AddWithValue("@IsActive", row["IsActive"]);
                    insertCmd.Parameters.AddWithValue("@ErpUserType", "B2BUser");

                    await insertCmd.ExecuteNonQueryAsync();
                }
            }

            // Call the stored procedure
            using (var spCmd = new SqlCommand("[dbo].[ErpNopUserImportProcedure]", connection))
            {
                spCmd.CommandType = CommandType.StoredProcedure;
                spCmd.CommandTimeout = 300;
                spCmd.Parameters.AddWithValue("@CurrentUserId", ((await _workContext.GetCurrentCustomerAsync()).Id));

                await spCmd.ExecuteNonQueryAsync();
            }
        }
    }

    #endregion
}
