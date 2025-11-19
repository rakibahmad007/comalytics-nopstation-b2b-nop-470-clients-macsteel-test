using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Core.Caching;
using Nop.Core.Domain.Customers;
using Nop.Data;
using NopStation.Plugin.B2B.ERPIntegrationCore.Domain;

namespace NopStation.Plugin.B2B.ERPIntegrationCore.Services;

public class ErpNopUserService : IErpNopUserService
{
    #region Fields

    private readonly IRepository<ErpNopUser> _erpNopUserRepository;
    private readonly IRepository<Customer> _customerRepository;
    private readonly IRepository<CustomerCustomerRoleMapping> _customerCustomerRoleMappingRepository;
    private readonly IStaticCacheManager _staticCacheManager;
    private readonly IRepository<ErpNopUserAccountMap> _erpNopUserAccountMapRepository;
    private readonly IRepository<ErpShipToAddress> _erpShipToAddressRepository;
    private readonly IRepository<ErpAccount> _erpAccountRepository;

    #endregion

    #region Ctor

    public ErpNopUserService(IRepository<ErpNopUser> erpNopUserRepository,
        IRepository<Customer> customerRepository,
        IRepository<CustomerCustomerRoleMapping> customerCustomerRoleMappingRepository,
        IStaticCacheManager staticCacheManager,
        IRepository<ErpNopUserAccountMap> erpNopUserAccountMapRepository,
        IRepository<ErpShipToAddress> erpShipToAddressRepository,
        IRepository<ErpAccount> erpAccountRepository)
    {
        _erpNopUserRepository = erpNopUserRepository;
        _customerRepository = customerRepository;
        _customerCustomerRoleMappingRepository = customerCustomerRoleMappingRepository;
        _staticCacheManager = staticCacheManager;
        _erpNopUserAccountMapRepository = erpNopUserAccountMapRepository;
        _erpShipToAddressRepository = erpShipToAddressRepository;
        _erpAccountRepository = erpAccountRepository;
    }

    #endregion

    #region Methods

    #region Insert/Update

    public async Task InsertErpNopUserAsync(ErpNopUser erpNopUser)
    {
        await _erpNopUserRepository.InsertAsync(erpNopUser);
    }

    public async Task UpdateErpNopUserAsync(ErpNopUser erpNopUser)
    {
        await _erpNopUserRepository.UpdateAsync(erpNopUser);
    }

    #endregion

    #region Delete

    private async Task DeleteErpNopUserAsync(ErpNopUser erpNopUser)
    {
        //as ErpBaseEntity dosen't inherit ISoftDelete but has that feature
        erpNopUser.IsDeleted = true;
        await _erpNopUserRepository.UpdateAsync(erpNopUser);
    }

    public async Task DeleteErpNopUserByIdAsync(int id)
    {
        var erpNopUser = await GetErpNopUserByIdAsync(id);
        if (erpNopUser != null)
        {
            await DeleteErpNopUserAsync(erpNopUser);
        }
    }

    #endregion

    #region Read

    public async Task<ErpNopUser> GetErpNopUserByIdAsync(int id)
    {
        if (id == 0)
            return null;

        var erpNopUser = await _erpNopUserRepository.GetByIdAsync(id, cache => default);

        if (erpNopUser == null || erpNopUser.IsDeleted)
            return null;

        return erpNopUser;
    }

    public async Task<ErpNopUser> GetErpNopUserByIdWithActiveAsync(int id)
    {
        if (id == 0)
            return null;

        var erpNopUser = await _erpNopUserRepository.GetByIdAsync(id, cache => default);

        if (erpNopUser == null || !erpNopUser.IsActive || erpNopUser.IsDeleted)
            return null;

        return erpNopUser;
    }

    public async Task<IPagedList<ErpNopUser>> GetAllErpNopUsersAsync(int pageIndex = 0, 
        int pageSize = int.MaxValue,
        bool? showHidden = null,
        bool getOnlyTotalCount = false,
        string email = null,
        string name = null,
        string firstName = null,
        string lastName = null,
        int accountId = 0,
        int userType = 0,
        int salesOrgId = 0,
        string shipToCode = "",
        int erpShipToAddressId = 0)
    {
        var erpNopUsers = await _erpNopUserRepository.GetAllPagedAsync(query =>
        {
            // showHidden is null for getting all, true for only actives and false for only inactives
            if (showHidden.HasValue)
            {
                if (!showHidden.Value)
                    query = query.Where(v => v.IsActive);
                else
                    query = query.Where(v => !v.IsActive);
            }

            query = query.Where(enu => !enu.IsDeleted);

            var hasCustomerFilters = !string.IsNullOrWhiteSpace(name) || 
                                   !string.IsNullOrWhiteSpace(firstName) || 
                                   !string.IsNullOrWhiteSpace(lastName) || 
                                   !string.IsNullOrWhiteSpace(email);

            if (hasCustomerFilters)
            {
                query = query.Join(_customerRepository.Table, x => x.NopCustomerId, y => y.Id,
                            (x, y) => new { ErpNopUser = x, Customer = y })
                        .Where(z => z.Customer.Active && !z.Customer.Deleted &&
                                  (string.IsNullOrWhiteSpace(name) || 
                                   z.Customer.FirstName.Contains(name) || z.Customer.LastName.Contains(name)) &&
                                  (string.IsNullOrWhiteSpace(firstName) || 
                                   z.Customer.FirstName.Contains(firstName)) &&
                                  (string.IsNullOrWhiteSpace(lastName) || 
                                   z.Customer.LastName.Contains(lastName)) &&
                                  (string.IsNullOrWhiteSpace(email) || 
                                   z.Customer.Email.Contains(email)))
                        .Select(z => z.ErpNopUser)
                        .Distinct();
            }
            else
            {
                query = query.Join(_customerRepository.Table, x => x.NopCustomerId, y => y.Id,
                            (x, y) => new { ErpNopUser = x, Customer = y })
                        .Where(z => z.Customer.Active && !z.Customer.Deleted)
                        .Select(z => z.ErpNopUser)
                        .Distinct();
            }
            if (!string.IsNullOrWhiteSpace(shipToCode))
            {
                query = query.Join(_erpShipToAddressRepository.Table, x => x.ErpShipToAddressId, y => y.Id,
                        (x, y) => new { ErpNopUser = x, ErpShipToAddress = y })
                    .Where(z => z.ErpShipToAddress.ShipToCode.Contains(shipToCode))
                    .Select(z => z.ErpNopUser)
                    .Distinct();
            }

            if (salesOrgId > 0)
            {
                query = (from enu in query
                         join map in _erpNopUserAccountMapRepository.Table
                             on enu.Id equals map.ErpUserId into maps
                         from map in maps.DefaultIfEmpty()
                         join ea in _erpAccountRepository.Table
                             on (map != null ? map.ErpAccountId : enu.ErpAccountId) equals ea.Id
                         where ea.ErpSalesOrgId == salesOrgId
                         select enu).Distinct();
            }

            if (accountId > 0)
            {
                query = query.Join(_erpNopUserAccountMapRepository.Table,
                    x => x.Id,
                    y => y.ErpUserId,
                    (x, y) => new { ErpNopUser = x, AccountMap = y })
                    .Where(z => z.ErpNopUser.ErpAccountId == accountId || z.AccountMap.ErpAccountId == accountId)
                    .Select(z => z.ErpNopUser);
            }

            if (erpShipToAddressId > 0)
                query = query.Where(enu => enu.ErpShipToAddressId.Equals(erpShipToAddressId));

            if (userType > 0)
                query = query.Where(enu => enu.ErpUserTypeId.Equals(userType));

            query = query.OrderByDescending(enu => enu.CreatedOnUtc);

            return query;

        }, pageIndex, pageSize, getOnlyTotalCount);

        return erpNopUsers;
    }

    public async Task<ErpNopUser> GetErpNopUserByCustomerIdAsync(int customerId, bool showHidden = false)
    {
        if (customerId == 0)
            return null;

        var key = _staticCacheManager.PrepareKeyForDefaultCache(ERPIntegrationCoreDefaults.ErpNopUserByCustomerCacheKey, customerId);

        var query = _erpNopUserRepository.Table.Where(enu=> enu.NopCustomerId == customerId && !enu.IsDeleted);

        if (!showHidden)
        {
            query = query.Where(enu => enu.IsActive);
        }

        return await _staticCacheManager.GetAsync(key, async () => await query.FirstOrDefaultAsync());
    }

    public async Task<ErpNopUser> GetErpNopUserByCustomerIdAndErpAccountIdAsync(int customerId, int erpAccountId = 0)
    {
        if (customerId == 0)
            return null;

        var key = _staticCacheManager.PrepareKeyForDefaultCache(ERPIntegrationCoreDefaults.ErpNopUserByCustomerAndErpAccountCacheKey, customerId, erpAccountId);

        var query = from enu in _erpNopUserRepository.Table
                    where enu.NopCustomerId == customerId && !enu.IsDeleted && enu.IsActive
                    select enu;

        if (erpAccountId > 0)
        {
            query = query.Join(_erpNopUserAccountMapRepository.Table,
                x => x.Id,
                y => y.ErpUserId,
                (x, y) => new { ErpNopUser = x, AccountMap = y })
                .Where(z => z.ErpNopUser.ErpAccountId == erpAccountId || z.AccountMap.ErpAccountId == erpAccountId)
                .Select(z => z.ErpNopUser)
                .Distinct();
        }

        return await _staticCacheManager.GetAsync(key, async () => await query.FirstOrDefaultAsync());
    }

    public async Task<IList<ErpNopUser>> GetAllErpNopUserByAccountIdAsync(int accountId, bool showHidden = false)
    {
        if (accountId == 0)
            return new List<ErpNopUser>();

        var erpNopUsers = await _erpNopUserRepository.GetAllAsync(query =>
        {
            query = from user in query
                    join map in _erpNopUserAccountMapRepository.Table
                        on user.Id equals map.ErpUserId
                    where map.ErpAccountId == accountId
                          && !user.IsDeleted
                          && (showHidden || user.IsActive)
                    orderby user.Id
                    select user;

            return query;
        });

        return erpNopUsers;
    }

    public async Task<IList<int>> GetAllCustomersByOnlyTheseRoleIdsAsync(int id)
    {
        if (id <= 0)
            return null;

        var customerIds = _customerCustomerRoleMappingRepository.Table
            .GroupBy(mapping => mapping.CustomerId)
            .Where(group => group.Count() == 1 && group.Max(mapping => mapping.CustomerRoleId) == id)
            .Select(group => group.Key);

        return await customerIds.ToListAsync();
    }

    public async Task<IList<int>> GetAllErpNopUsersCustomerIds()
    {
        return await _erpNopUserRepository.Table.Where(enu => !enu.IsDeleted).Select(enu => enu.NopCustomerId).ToListAsync();
    }

    public async Task<IList<ErpNopUser>> GetAllErpNopUsersByCustomerIdAsync(int customerId, bool showHidden = false)
    {
        if (customerId == 0)
            return null;

        var erpNopUsers = await _erpNopUserRepository.GetAllAsync(query =>
        {
            if (!showHidden)
                query = query.Where(enu => enu.IsActive);

            query = query.Where(enu => !enu.IsDeleted);
            query = query.Where(enu => enu.NopCustomerId == customerId);
            query = query.OrderBy(enu => enu.Id);
            return query;

        });

        return erpNopUsers;
    }

    public async Task<IList<int>> GetAllErpNopUsersErpAccountIdsByCustomerId(int customerId = 0)
    {
        if (customerId == 0)
            return null;

        return await _erpNopUserRepository.Table
            .Where(enu => !enu.IsDeleted && enu.NopCustomerId == customerId)
            .Select(enu => enu.ErpAccountId)
            .ToListAsync();
    }

    public async Task<bool> IsTheCustomerAlreadyAUserOfThisErpAccount(int customerId = 0, int erpAccountId = 0)
    {
        if (customerId == 0 || erpAccountId == 0)
            return false;

        var directAssignment = await _erpNopUserRepository.Table
            .AnyAsync(enu => !enu.IsDeleted && enu.NopCustomerId == customerId && enu.ErpAccountId == erpAccountId);
        
        if (directAssignment)
            return true;

        var mappedAssignment = await (from user in _erpNopUserRepository.Table
                                     join map in _erpNopUserAccountMapRepository.Table
                                         on user.Id equals map.ErpUserId
                                     where user.NopCustomerId == customerId 
                                           && !user.IsDeleted 
                                           && map.ErpAccountId == erpAccountId
                                     select user).AnyAsync();

        return mappedAssignment;
    }

    #endregion

    #endregion
}
