using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DocumentFormat.OpenXml;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Core;
using Nop.Core.Domain.Customers;
using Nop.Data;
using Nop.Services;
using Nop.Services.Customers;
using Nop.Services.Helpers;
using Nop.Services.Localization;
using Nop.Web.Areas.Admin.Infrastructure.Mapper.Extensions;
using Nop.Web.Areas.Admin.Models.Customers;
using Nop.Web.Framework.Factories;
using Nop.Web.Framework.Models.Extensions;
using NopStation.Plugin.B2B.B2BB2CFeatures.Areas.Admin.Models.ErpSalesRep;
using NopStation.Plugin.B2B.ERPIntegrationCore.Domain;
using NopStation.Plugin.B2B.ERPIntegrationCore.Enums;
using NopStation.Plugin.B2B.ERPIntegrationCore.Services;

namespace NopStation.Plugin.B2B.B2BB2CFeatures.Areas.Admin.Factories;

public class ErpSalesRepModelFactory : IErpSalesRepModelFactory
{
    #region Fields

    private readonly IErpSalesRepService _erpSalesRepService;
    private readonly IErpSalesOrgService _erpSalesOrgService;
    private readonly ICustomerService _customerService;
    private readonly ILocalizationService _localizationService;
    private readonly IErpSalesRepSalesOrgMapService _erpSalesRepSalesOrgMapService;
    private readonly IDateTimeHelper _dateTimeHelper;
    private readonly IErpNopUserService _erpNopUserService;
    private readonly IAclSupportedModelFactory _aclSupportedModelFactory;
    private readonly IRepository<Customer> _customerRepository;
    private readonly IRepository<ErpNopUser> _erpNopUserRepository;
    private readonly IRepository<ErpSalesRep> _erpSalesRepRepository;
    private readonly IRepository<CustomerCustomerRoleMapping> _customerCustomerRoleMappingRepository;
    private readonly IOverridenCustomerModelFactory _overridenCustomerModelFactory;

    #endregion

    #region ctor

    public ErpSalesRepModelFactory(
        IErpSalesRepService erpSalesRepService,
        IErpSalesOrgService erpSalesOrgService,
        ICustomerService customerService,
        ILocalizationService localizationService,
        IErpSalesRepSalesOrgMapService erpSalesRepSalesOrgMapService,
        IDateTimeHelper dateTimeHelper,
        IErpNopUserService erpNopUserService,
        IAclSupportedModelFactory aclSupportedModelFactory,
        IRepository<Customer> customerRepository,
        IRepository<ErpNopUser> erpNopUserRepository,
        IRepository<ErpSalesRep> erpSalesRepRepository,
        IRepository<CustomerCustomerRoleMapping> customerCustomerRoleMappingRepository,
        IOverridenCustomerModelFactory overridenCustomerModelFactory)
    {
        _erpSalesRepService = erpSalesRepService;
        _erpSalesOrgService = erpSalesOrgService;
        _customerService = customerService;
        _localizationService = localizationService;
        _erpSalesRepSalesOrgMapService = erpSalesRepSalesOrgMapService;
        _dateTimeHelper = dateTimeHelper;
        _erpNopUserService = erpNopUserService;
        _aclSupportedModelFactory = aclSupportedModelFactory;
        _customerRepository = customerRepository;
        _erpNopUserRepository = erpNopUserRepository;
        _erpSalesRepRepository = erpSalesRepRepository;
        _customerCustomerRoleMappingRepository = customerCustomerRoleMappingRepository;
        _overridenCustomerModelFactory = overridenCustomerModelFactory;
    }

    #endregion

    #region Utilities

    public async Task PrepareAvailableSalesRepsAsync(IList<SelectListItem> model)
    {
        var customers = await _erpSalesRepService.GetAllSalesRepCustomersAsync();

        model.Add(new SelectListItem
        {
            Text = await _localizationService.GetResourceAsync("Admin.Common.Select"),
            Value = "0"
        });

        foreach (var customer in customers)
        {
            model.Add(new SelectListItem
            {
                Text = customer.Email,
                Value = $"{customer.Id}"
            });
        }
    }

    public async Task PrepareAvailableSalesRepTypeAsync(IList<SelectListItem> model)
    {
        // Prepare SalesRepTypes dropdown options
        var availableSalesRepTypes = await SalesRepType.AllUsers.ToSelectListAsync(false);
        foreach (var types in availableSalesRepTypes)
        {
            model.Add(types);
        }
        model.Insert(0, new SelectListItem
        {
            Value = "0",
            Text = await _localizationService.GetResourceAsync("Admin.Common.Select"),
        });
    }

    public async Task PrepareAvailableSalesOrgsAsync(IList<SelectListItem> model)
    {

        var salesOrgs = await _erpSalesOrgService.GetErpSalesOrgsAsync(isActive: true, filterOutDeleted: true);

        foreach (var salesOrg in salesOrgs)
        {
            model.Add(new SelectListItem
            {
                Text = salesOrg.Name,
                Value = $"{salesOrg.Id}"
            });
        }
    }

    #endregion

    #region Method

    public async Task<ErpSalesRepModel> PrepareErpSalesRepModelAsync(ErpSalesRepModel model, ErpSalesRep erpSalesRep)
    {
        if (erpSalesRep != null)
        {
            model = erpSalesRep.ToModel<ErpSalesRepModel>();
            model.NopCustomerId = erpSalesRep.NopCustomerId;
            model.SalesRepTypeId = erpSalesRep.SalesRepTypeId;
            model.CreatedOnUtc = await _dateTimeHelper.ConvertToUserTimeAsync(erpSalesRep.CreatedOnUtc, DateTimeKind.Utc);
            model.UpdatedOnUtc = await _dateTimeHelper.ConvertToUserTimeAsync(erpSalesRep.UpdatedOnUtc, DateTimeKind.Utc);

            var salesOrgMaps = await _erpSalesRepSalesOrgMapService.GetErpSalesRepSalesOrgMapsByErpSalesRepIdAsync(erpSalesRep.Id);
            if (salesOrgMaps.Any())
            {
                model.SalesOrgIds = salesOrgMaps.Select(x => x.ErpSalesOrgId).ToList();
            }
        }
        model.NopCustomer = model.NopCustomerId > 0 ? (await _customerService.GetCustomerByIdAsync(model.NopCustomerId))?.Email ?? "" : "";
        model.IsActive = erpSalesRep == null || erpSalesRep.IsActive;

        #region Prepare NopCustomers dropdown options

        var customerRoleId = (await _customerService.GetCustomerRoleBySystemNameAsync(NopCustomerDefaults.RegisteredRoleName)).Id;
        var customerIdsWithOnlyRegisteredRole = await _erpNopUserService.GetAllCustomersByOnlyTheseRoleIdsAsync(customerRoleId);

        var existingNopCustomer = (await _erpNopUserService.GetAllErpNopUsersAsync()).Select(s => s.NopCustomerId)?.ToList();
        if (customerIdsWithOnlyRegisteredRole.Count > 0)
        {
            model.AvailableCustomers = (await _customerService.GetCustomersByIdsAsync(customerIdsWithOnlyRegisteredRole.ToArray())).Where(w => !existingNopCustomer.Contains(w.Id))
            .Select(nopCustomer => new SelectListItem
            {
                Value = nopCustomer.Id.ToString(),
                Text = nopCustomer.FirstName + " " + nopCustomer.LastName + " (" + nopCustomer.Email + ")",
            }).ToList();
        }

        model.AvailableCustomers.Insert(0, new SelectListItem
        {
            Text = await _localizationService.GetResourceAsync("Admin.Common.Select"),
            Value = "0"
        });

        #endregion

        await PrepareAvailableSalesRepTypeAsync(model.AvailableSalesRepType);
        await PrepareAvailableSalesOrgsAsync(model.AvailableSalesOrgs);

        model.ErpAccountSearchModel.SetGridPageSize();

        return model;
    }

    public async Task<ErpSalesRepSearchModel> PrepareErpSalesRepSearchModelAsync(ErpSalesRepSearchModel searchModel)
    {
        ArgumentNullException.ThrowIfNull(searchModel);

        await PrepareAvailableSalesRepsAsync(searchModel.AvailableSalesReps);
        await PrepareAvailableSalesRepTypeAsync(searchModel.AvailableSalesRepType);
        await PrepareAvailableSalesOrgsAsync(searchModel.AvailableSalesOrgs);
        searchModel.AvailableActiveOptions.Add(new SelectListItem
        {
            Value = "0",
            Text = await _localizationService.GetResourceAsync("B2BB2CFeatures.ErpSalesRep.Fields.SearchActive.All"),
        });
        searchModel.AvailableActiveOptions.Add(new SelectListItem
        {
            Value = "1",
            Text = await _localizationService.GetResourceAsync("B2BB2CFeatures.ErpSalesRep.Fields.SearchActive.ActiveOnly"),
        });
        searchModel.AvailableActiveOptions.Add(new SelectListItem
        {
            Value = "2",
            Text = await _localizationService.GetResourceAsync("B2BB2CFeatures.ErpSalesRep.Fields.SearchActive.InactiveOnly"),
        });
        searchModel.SearchActiveId = 1;

        searchModel.SetGridPageSize();

        return searchModel;
    }

    public async Task<ErpSalesRepListModel> PrepareErpSalesRepListModelAsync(ErpSalesRepSearchModel searchModel)
    {
        ArgumentNullException.ThrowIfNull(searchModel);

        var overrideActive = searchModel.SearchActiveId == 0 ? null : (bool?)(searchModel.SearchActiveId == 1);

        var erpSalesReps = await _erpSalesRepService.GetAllErpSalesRepAsync(
            customerEmail: searchModel.SearchCustomerEmail,
            salesRepTypeId: searchModel.SalesRepTypeId,
            erpSalesOrgIds: searchModel.SelectedSalesOrgIds.ToArray(),
            overrideActive: overrideActive,
            showHidden: true,
            pageIndex: searchModel.Page - 1,
            pageSize: searchModel.PageSize);

        var model = await new ErpSalesRepListModel().PrepareToGridAsync(searchModel, erpSalesReps, () =>
        {
            return erpSalesReps.SelectAwait(async erpSalesRep =>
            {
                var erpSalesRepModel = erpSalesRep.ToModel<ErpSalesRepModel>();
                var customer = await _customerService.GetCustomerByIdAsync(erpSalesRep.NopCustomerId);

                erpSalesRepModel.NopCustomer = customer?.Email;

                erpSalesRepModel.SalesRepType = CommonHelper.ConvertEnum(((SalesRepType)erpSalesRep.SalesRepTypeId).ToString());

                erpSalesRepModel.CustomerRoleNames = string.Join(", ",
                    (await _customerService.GetCustomerRolesAsync(customer)).Select(role => role.Name));

                erpSalesRepModel.CommaSeparatedOrgNames = string.Join(", ",
                    (await _erpSalesRepService.GetSalesRepOrgsAsync(erpSalesRep.Id)).Select(org => org.Name));

                return erpSalesRepModel;
            });
        });

        return model;
    }

    public async Task<CustomerSearchModelForErpSalesRep> PrepareCustomerSearchModelForErpSalesRepAsync(CustomerSearchModelForErpSalesRep searchModel)
    {
        ArgumentNullException.ThrowIfNull(searchModel);

        var registeredRole = await _customerService.GetCustomerRoleBySystemNameAsync(NopCustomerDefaults.RegisteredRoleName);
        if (registeredRole != null)
            searchModel.SelectedCustomerRoleIds.Add(registeredRole.Id);

        await _aclSupportedModelFactory.PrepareModelCustomerRolesAsync(searchModel);
        searchModel.IncludeDeletedErpUser = true;
        searchModel.IncludeDeletedSalesRep = false;
        searchModel.SetGridPageSize();
        return searchModel;
    }

    public async Task<CustomerListModel> PrepareCustomertListModelForErpSalesRep(CustomerSearchModelForErpSalesRep searchModel)
    {
        ArgumentNullException.ThrowIfNull(searchModel);

        _ = int.TryParse(searchModel.SearchDayOfBirth, out var dayOfBirth);
        _ = int.TryParse(searchModel.SearchMonthOfBirth, out var monthOfBirth);

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

        var pagedCustomers = new PagedList<Customer>(customers, searchModel.Page - 1, searchModel.PageSize);

        var model = await new CustomerListModel().PrepareToGridAsync(
            searchModel,
            pagedCustomers,
            () => _overridenCustomerModelFactory.PrepareCustomerModelsAsync(pagedCustomers)
        );

        return model;
    }
    #endregion
}