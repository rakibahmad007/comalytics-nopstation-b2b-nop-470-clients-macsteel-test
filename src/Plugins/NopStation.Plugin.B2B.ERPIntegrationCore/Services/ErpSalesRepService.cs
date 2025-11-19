using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using LinqToDB;
using Nop.Core;
using Nop.Core.Domain.Customers;
using Nop.Data;
using Nop.Services.Catalog;
using Nop.Services.Customers;
using NopStation.Plugin.B2B.ERPIntegrationCore.Domain;
using NopStation.Plugin.B2B.ERPIntegrationCore.Enums;

namespace NopStation.Plugin.B2B.ERPIntegrationCore.Services;

public class ErpSalesRepService : IErpSalesRepService
{
    #region Fields

    private readonly ICustomerService _customerService;
    private readonly IRepository<Customer> _customerRepository;
    private readonly IRepository<CustomerRole> _customerRoleRepository;
    private readonly IRepository<ErpNopUser> _erpNopUserRepository;
    private readonly IRepository<ErpSalesOrg> _erpSalesOrgRepository;
    private readonly IRepository<ErpSalesRep> _erpSalesRepRepository; 
    private readonly IRepository<ErpAccount> _erpErpAccountRepository;
    private readonly IRepository<ErpUserFavourite> _erpUserFavouriteRepository;
    private readonly IRepository<ErpNopUserAccountMap> _erpNopUserAccountMapRepository;
    private readonly IRepository<ErpSalesRepSalesOrgMap> _erpErpSalesRepSalesOrgMapRepository;
    private readonly IRepository<ErpSalesRepErpAccountMap> _erpSalesRepErpAccountMapRepository;
    private readonly IRepository<CustomerCustomerRoleMapping> _customerCustomerRoleMappingRepository;

    #endregion

    #region Ctor

    public ErpSalesRepService(IRepository<ErpSalesRep> erpSalesRepRepository,
        IRepository<ErpNopUser> erpNopUserRepository,
        IRepository<ErpAccount> erpErpAccountRepository,
        IRepository<ErpSalesRepSalesOrgMap> erpErpSalesRepSalesOrgMapRepository,
        IRepository<ErpSalesOrg> erpSalesOrgRepository,
        IRepository<Customer> customerRepository,
        IRepository<CustomerRole> customerRoleRepository,
        IRepository<CustomerCustomerRoleMapping> customerCustomerRoleMappingRepository,
        IRepository<ErpNopUserAccountMap> erpNopUserAccountMapRepository,
        IRepository<ErpSalesRepErpAccountMap> erpSalesRepErpAccountMapRepository,
        IRepository<ErpUserFavourite> erpUserFavouriteRepository,
        ICustomerService customerService)
    {
        _erpSalesRepRepository = erpSalesRepRepository;
        _erpNopUserRepository = erpNopUserRepository;
        _erpErpAccountRepository = erpErpAccountRepository;
        _erpErpSalesRepSalesOrgMapRepository = erpErpSalesRepSalesOrgMapRepository;
        _erpSalesOrgRepository = erpSalesOrgRepository;
        _customerRepository = customerRepository;
        _customerRoleRepository = customerRoleRepository;
        _customerCustomerRoleMappingRepository = customerCustomerRoleMappingRepository;
        _erpNopUserAccountMapRepository = erpNopUserAccountMapRepository;
        _erpSalesRepErpAccountMapRepository = erpSalesRepErpAccountMapRepository;
        _erpUserFavouriteRepository = erpUserFavouriteRepository;
        _customerService = customerService;
    }

    #endregion

    #region Methods

    #region Insert/Update

    public async Task InsertErpSalesRepAsync(ErpSalesRep erpSalesRep)
    {
        await _erpSalesRepRepository.InsertAsync(erpSalesRep);
    }

    public async Task UpdateErpSalesRepAsync(ErpSalesRep erpSalesRep)
    {
        await _erpSalesRepRepository.UpdateAsync(erpSalesRep);
    }

    #endregion

    #region Delete

    private async Task DeleteErpSalesRepAsync(ErpSalesRep erpSalesRep)
    {
        //as ErpBaseEntity dosen't inherit ISoftDelete but has that feature
        erpSalesRep.IsDeleted = true;
        await _erpSalesRepRepository.UpdateAsync(erpSalesRep);
    }

    public async Task DeleteErpSalesRepsAsync(IList<ErpSalesRep> erpSalesReps)
    {
        await _erpSalesRepRepository.DeleteAsync(erpSalesReps);
    }

    public async Task DeleteErpSalesRepByIdAsync(int id)
    {
        var erpSalesRep = await GetErpSalesRepByIdAsync(id);

        if (erpSalesRep != null)
        {
            await DeleteErpSalesRepAsync(erpSalesRep);
        }
    }

    #endregion

    #region Read

    public async Task<ErpSalesRep> GetErpSalesRepByIdAsync(int id)
    {
        if (id == 0)
            return null;

        var erpSalesRep = await _erpSalesRepRepository.GetByIdAsync(id, cache => default);

        if (erpSalesRep == null || erpSalesRep.IsDeleted)
            return null;

        return erpSalesRep;
    }

    public async Task<IList<ErpSalesRep>> GetErpSalesRepByIdsAsync(int[] erpSalesRepIds)
    {
        return await _erpSalesRepRepository.GetByIdsAsync(erpSalesRepIds, includeDeleted: false);
    }

    public async Task<ErpSalesRep> GetErpSalesRepByIdWithActiveAsync(int id)
    {
        if (id == 0)
            return null;

        var erpSalesRep = await _erpSalesRepRepository.GetByIdAsync(id, cache => default);

        if (erpSalesRep == null || !erpSalesRep.IsActive || erpSalesRep.IsDeleted)
            return null;

        return erpSalesRep;
    }

    public async Task<IPagedList<ErpSalesRep>> GetAllErpSalesRepAsync(string customerEmail = "", 
        int salesRepTypeId = 0,
        int pageIndex = 0, 
        int pageSize = int.MaxValue, 
        bool showHidden = false, 
        bool getOnlyTotalCount = false,
        int[] erpSalesOrgIds = null,
        bool? overrideActive = null)
    {
        var erpSalesReps = await _erpSalesRepRepository.GetAllPagedAsync(query =>
        {
            if (!string.IsNullOrEmpty(customerEmail))
            {
                query = from s in query
                        join customer in _customerRepository.Table on s.NopCustomerId equals customer.Id into customerJoined
                        from customer in customerJoined.DefaultIfEmpty()
                        where customer.Email == customerEmail
                        select s;
            }
            if (overrideActive.HasValue)
            {
                if (overrideActive.Value == false) // Inactive only
                    query = query.Where(x => !x.IsActive);
                else if (overrideActive.Value == true) // Active only
                    query = query.Where(x => x.IsActive);
            }

            if (salesRepTypeId > 0)
                query = query.Where(egp => egp.SalesRepTypeId == salesRepTypeId);

            if (!showHidden)
                query = query.Where(egp => egp.IsActive);

            query = query.Where(egp => !egp.IsDeleted);

            if (erpSalesOrgIds != null && erpSalesOrgIds.Length > 0)
            {
                query = query.Join(_erpErpSalesRepSalesOrgMapRepository.Table, x => x.Id, y => y.ErpSalesRepId,
                        (x, y) => new { SalesRep = x, Mapping = y })
                    .Where(z => erpSalesOrgIds.Contains(z.Mapping.ErpSalesOrgId))
                    .Select(z => z.SalesRep)
                    .Distinct();
            }

            query = query.OrderBy(egp => egp.Id);
            return query;

        }, pageIndex, pageSize, getOnlyTotalCount);

        return erpSalesReps;
    }

    public async Task<IList<ErpSalesRep>> GetErpSalesRepsByNopCustomerIdAsync(int nopCustomerId, bool showHidden = false)
    {
        if (nopCustomerId == 0)
            return null;

        var erpSalesReps = await _erpSalesRepRepository.GetAllAsync(query =>
        {
            if (!showHidden)
                query = query.Where(egp => egp.IsActive);

            query = query.Where(egp => !egp.IsDeleted);
            query = query.Where(ei => ei.NopCustomerId == nopCustomerId);
            query = query.OrderBy(ei => ei.Id);
            return query;
        });

        return erpSalesReps;
    }

    #endregion

    #region Sales rep users

    public async Task<IPagedList<ErpNopUser>> GetAllSalesRepUsersAsync(int salesRepId = 0, 
        int salesRepTypeId = 0,
        int salesRepCustomerId = 0,
        string erpAccontNo = null,
        string accountName = null, 
        string email = null, 
        string fullName = null,
        int pageIndex = 0, 
        int pageSize = int.MaxValue, 
        bool showHidden = false,
        bool getOnlyTotalCount = false,
        int salesOrgId = 0,
        bool? isActive = null)
    {
        var erpNopUsers = await _erpNopUserRepository.GetAllPagedAsync(async query =>
        {
            if (!showHidden)
                query = query.Where(v => v.IsActive);

            if (isActive.HasValue)
            {
                query = query.Where(v => v.IsActive == isActive.Value);
            }

            query = query.Where(v => !v.IsDeleted);

            var erpAccounts = _erpErpAccountRepository.Table.Where(a => !a.IsDeleted);
            if (!showHidden)
                erpAccounts = erpAccounts.Where(a => a.IsActive);

            if (!string.IsNullOrWhiteSpace(erpAccontNo))
                erpAccounts = erpAccounts.Where(c => c.AccountNumber.Contains(erpAccontNo));
            if (!string.IsNullOrWhiteSpace(accountName))
                erpAccounts = erpAccounts.Where(c => c.AccountName.Contains(accountName));

            if (salesOrgId != 0)
            {
                erpAccounts = erpAccounts.Where(a => a.ErpSalesOrgId == salesOrgId);
            }

            var customer = _customerRepository.Table.Where(c => c.Active && !c.Deleted);
            if (!string.IsNullOrWhiteSpace(email))
                customer = customer.Where(c => c.Email.Contains(email));
            if (!string.IsNullOrWhiteSpace(fullName))
                customer = customer.Where(c => (c.FirstName + " " + c.LastName).Contains(fullName));

            query = from u in query
                    join c in customer
                    on u.NopCustomerId equals c.Id
                    select u;

            if (salesRepTypeId == (int)SalesRepType.AllUsers)
            {
                query = (from nopUser in query
                         join account in erpAccounts on nopUser.ErpAccountId equals account.Id
                         join fav in _erpUserFavouriteRepository.Table.Where(f => f.NopCustomerId == salesRepCustomerId)
                             on nopUser.Id equals fav.ErpNopUserId into favGroup
                         from fav in favGroup.DefaultIfEmpty()
                         select nopUser).Distinct();
            }
            else if (salesRepTypeId == (int)SalesRepType.BySalesOrganisation)
            {
                query = (from nopUser in query
                         join account in erpAccounts on nopUser.ErpAccountId equals account.Id
                         join som in _erpErpSalesRepSalesOrgMapRepository.Table on account.ErpSalesOrgId equals som.ErpSalesOrgId
                         join fav in _erpUserFavouriteRepository.Table.Where(f => f.NopCustomerId == salesRepCustomerId)
                             on nopUser.Id equals fav.ErpNopUserId into favGroup
                         from fav in favGroup.DefaultIfEmpty()
                         where som.ErpSalesRepId == salesRepId
                         select nopUser).Distinct();
            }
            else if (salesRepTypeId == (int)SalesRepType.MultiBuyers)
            {
                query = (from nopUser in query
                         join account in erpAccounts on nopUser.ErpAccountId equals account.Id
                         join srm in _erpSalesRepErpAccountMapRepository.Table on account.Id equals srm.ErpAccountId
                         join fav in _erpUserFavouriteRepository.Table.Where(f => f.NopCustomerId == salesRepCustomerId)
                             on nopUser.Id equals fav.ErpNopUserId into favGroup
                         from fav in favGroup.DefaultIfEmpty()
                         where srm.ErpSalesRepId == salesRepId
                         select nopUser).Distinct();
            }

            var administratorsRoleId = (await _customerService.GetCustomerRoleBySystemNameAsync(NopCustomerDefaults.AdministratorsRoleName))?.Id ?? 0;

            var customerAccountManagerRoleId = (await _customerService.GetCustomerRoleBySystemNameAsync
                (ERPIntegrationCoreDefaults.B2BCustomerAccountManagerRoleSystemName))?.Id ?? 0;

            var salesRepRoleId = (await _customerService.GetCustomerRoleBySystemNameAsync(ERPIntegrationCoreDefaults.B2BSalesRepRoleSystemName))?.Id ?? 0;

            var registeredRoleId = (await _customerService.GetCustomerRoleBySystemNameAsync(NopCustomerDefaults.RegisteredRoleName))?.Id ?? 0;

            var b2BCustomerRoleId = (await _customerService.GetCustomerRoleBySystemNameAsync(ERPIntegrationCoreDefaults.B2BCustomerRoleSystemName))?.Id;

            var b2CCustomerRoleId = (await _customerService.GetCustomerRoleBySystemNameAsync(ERPIntegrationCoreDefaults.B2CCustomerRoleSystemName))?.Id;

            var customerRoleMappings = (from roleMap in _customerCustomerRoleMappingRepository.Table 
                join role in _customerRoleRepository.Table on roleMap.CustomerRoleId equals role.Id
                where role.Active
                select roleMap).Distinct();

            var excludedCustomerIds = customerRoleMappings
                .Where(w => w.CustomerRoleId == administratorsRoleId || 
                    w.CustomerRoleId == customerAccountManagerRoleId || 
                    w.CustomerRoleId == salesRepRoleId)
                .Select(s => s.CustomerId)
                .Distinct();

            query = query.Where(u => !excludedCustomerIds.Contains(u.NopCustomerId));

            query = query.Where(u => u.NopCustomerId != salesRepCustomerId);

            query = (from n in query
                    join m in _erpNopUserAccountMapRepository.Table on n.Id equals m.ErpUserId
                    join crm in customerRoleMappings on n.NopCustomerId equals crm.CustomerId
                    where 
                    customerRoleMappings
                        .Where(x => x.CustomerId == n.NopCustomerId &&
                                (x.CustomerRoleId == registeredRoleId)).Any() && 
                    customerRoleMappings
                        .Where(x => x.CustomerId == n.NopCustomerId && 
                            (x.CustomerRoleId == b2CCustomerRoleId || x.CustomerRoleId == b2BCustomerRoleId)).Any()
                    select n).Distinct();

            query = query.OrderByDescending(ea => ea.Id);

            return query;
        }, pageIndex, pageSize, getOnlyTotalCount);

        return erpNopUsers;
    }

    public async Task<IPagedList<ErpSalesOrg>> GetSalesRepOrgsPagedAsync(int salesRepId,
        int pageIndex = 0, 
        int pageSize = int.MaxValue, 
        bool showHidden = false,
        bool getOnlyTotalCount = false)
    {
        var salesOrgs = await _erpSalesOrgRepository.GetAllPagedAsync(query =>
        {
            var qry = from o in query
                      join som in _erpErpSalesRepSalesOrgMapRepository.Table on o.Id equals som.ErpSalesOrgId
                      where som.ErpSalesRepId == salesRepId && !o.IsDeleted &&
                            (showHidden || o.IsActive)
                      select o;

            query = qry.Where(v => !v.IsDeleted);

            query = query.OrderByDescending(ea => ea.Id);

            return query;

        }, pageIndex, pageSize, getOnlyTotalCount);

        return salesOrgs;
    }

    public async Task<IList<ErpSalesOrg>> GetSalesRepOrgsAsync(int salesRepId, 
        bool showHidden = false)
    {
        return await _erpSalesOrgRepository.GetAllAsync(query =>
        {
            return from o in query
                   join som in _erpErpSalesRepSalesOrgMapRepository.Table on o.Id equals som.ErpSalesOrgId
                   where som.ErpSalesRepId == salesRepId && !o.IsDeleted &&
                         (showHidden || o.IsActive)
                   select o;

        }, cache => cache.PrepareKeyForDefaultCache(ERPIntegrationCoreDefaults.SalesRepOrgCacheKey, salesRepId, showHidden));
    }

    public async Task<IPagedList<Customer>> GetAllSalesRepCustomersAsync(int pageIndex = 0, 
        int pageSize = int.MaxValue, 
        bool getOnlyTotalCount = false)
    {
        var registeredCustomerRole = await _customerService.GetCustomerRoleBySystemNameAsync(NopCustomerDefaults.RegisteredRoleName);
        var customers = await _customerRepository.GetAllPagedAsync(query =>
        {
            if (registeredCustomerRole != null)
            {
                query = query.Join(_customerCustomerRoleMappingRepository.Table, x => x.Id, y => y.CustomerId,
                            (x, y) => new { Customer = x, Mapping = y })
                        .Where(z => z.Mapping.CustomerRoleId == registeredCustomerRole.Id)
                        .Select(z => z.Customer)
                        .Distinct();
            }

            query = query
                .GroupJoin(
                    _erpSalesRepRepository.Table,
                    customer => customer.Id,
                    salesRep => salesRep.NopCustomerId,
                    (customer, salesRepGroup) => new { Customer = customer, SalesReps = salesRepGroup }
                )
                .Where(joinedData => joinedData.SalesReps.Any())
                .Select(joinedData => joinedData.Customer);

            query = query.OrderByDescending(c => c.CreatedOnUtc);

            return query;
        }, pageIndex, pageSize, getOnlyTotalCount);

        return customers;
    }

    public async Task<IPagedList<Customer>> GetAllCustomersNotYetSalesRepAsync(int includeSalesRepCustomerId = 0, 
        int pageIndex = 0, 
        int pageSize = int.MaxValue, 
        bool getOnlyTotalCount = false)
    {
        var registeredCustomerRole = await _customerService.GetCustomerRoleBySystemNameAsync(NopCustomerDefaults.RegisteredRoleName);
        var customers = await _customerRepository.GetAllPagedAsync(query =>
        {
            if (registeredCustomerRole != null)
            {
                query = query.Join(_customerCustomerRoleMappingRepository.Table, x => x.Id, y => y.CustomerId,
                            (x, y) => new { Customer = x, Mapping = y })
                        .Where(z => z.Mapping.CustomerRoleId == registeredCustomerRole.Id)
                        .Select(z => z.Customer)
                        .Distinct();
            }

            query = query
                .GroupJoin(
                    _erpSalesRepRepository.Table.Where(s => !s.IsDeleted),
                    customer => customer.Id,
                    salesRep => salesRep.NopCustomerId,
                    (customer, salesRepGroup) => new { Customer = customer, SalesReps = salesRepGroup }
                )
                .Where(joinedData => !joinedData.SalesReps.Any()
                    || (includeSalesRepCustomerId > 0 && joinedData.Customer.Id == includeSalesRepCustomerId))
                .Select(joinedData => joinedData.Customer);

            query = query.OrderByDescending(c => c.CreatedOnUtc);

            return query;
        }, pageIndex, pageSize, getOnlyTotalCount);

        return customers;
    }

    #endregion

    #endregion
}
