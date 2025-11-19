using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LinqToDB.Common;
using Nop.Core;
using Nop.Core.Caching;
using Nop.Core.Domain.Customers;
using Nop.Data;
using Nop.Services.Customers;
using NopStation.Plugin.B2B.ERPIntegrationCore.Domain;
using NopStation.Plugin.B2B.ERPIntegrationCore.Enums;

namespace NopStation.Plugin.B2B.ERPIntegrationCore.Services;

public class ErpShipToAddressService : IErpShipToAddressService
{
    #region Fields

    private readonly IRepository<ErpShipToAddress> _erpShipToAddressRepository;
    private readonly IRepository<ErpShiptoAddressErpAccountMap> _erpShiptoAddressErpAccountMapRepository;
    private readonly IStaticCacheManager _staticCacheManager;
    private readonly IRepository<CustomerAddressMapping> _customerAddressMappingRepository;
    private readonly IRepository<ErpAccount> _erpAccountRepository;

    #endregion

    #region Ctor

    public ErpShipToAddressService(IRepository<ErpShipToAddress> erpShipToAddressRepository,
        IRepository<ErpShiptoAddressErpAccountMap> erpShiptoAddressErpAccountMap,
        IStaticCacheManager staticCacheManager,
        IRepository<CustomerAddressMapping> customerAddressMappingRepository,
        IRepository<ErpAccount> erpAccountRepository)
    {
        _erpShipToAddressRepository = erpShipToAddressRepository;
        _erpShiptoAddressErpAccountMapRepository = erpShiptoAddressErpAccountMap;
        _staticCacheManager = staticCacheManager;
        _customerAddressMappingRepository = customerAddressMappingRepository;
        _erpAccountRepository = erpAccountRepository;
    }

    #endregion

    #region Methods

    #region Insert/Update/Delete

    public async Task InsertErpShipToAddressAsync(ErpShipToAddress erpShipToAddress)
    {
        await _erpShipToAddressRepository.InsertAsync(erpShipToAddress);
    }

    public async Task InsertErpShipToAddressesAsync(IList<ErpShipToAddress> erpShipToAddresses)
    {
        await _erpShipToAddressRepository.InsertAsync(erpShipToAddresses);
    }

    public async Task UpdateErpShipToAddressAsync(ErpShipToAddress erpShipToAddress)
    {
        await _erpShipToAddressRepository.UpdateAsync(erpShipToAddress);
    }

    public async Task UpdateErpShipToAddressesAsync(IList<ErpShipToAddress> erpShipToAddresses)
    {
        if (erpShipToAddresses == null || !erpShipToAddresses.Any())
            return;

        // Ensure no duplicate target rows
        var distinctAddresses = erpShipToAddresses
            .GroupBy(a => a.Id)  // Group by the primary key to remove duplicates
            .Select(g => g.Last())  // Take the first occurrence
            .ToList();

        await _erpShipToAddressRepository.UpdateAsync(distinctAddresses);
    }

    public async Task DeleteErpShipToAddressAsync(ErpShipToAddress erpShipToAddress)
    {
        if (erpShipToAddress == null)
            return;

        //as ErpBaseEntity dosen't inherit ISoftDelete but has that feature
        erpShipToAddress.IsDeleted = true;
        await _erpShipToAddressRepository.UpdateAsync(erpShipToAddress);
    }

    public async Task DeleteErpShipToAddressByIdAsync(int id)
    {
        var erpShipToAddress = await GetErpShipToAddressByIdAsync(id);

        if (erpShipToAddress != null)
        {
            await DeleteErpShipToAddressAsync(erpShipToAddress);
        }
    }

    #endregion

    #region Read

    public async Task<ErpShipToAddress> GetErpShipToAddressByIdAsync(int id)
    {
        if (id == 0)
            return null;

        var erpShipToAddress = await _erpShipToAddressRepository.GetByIdAsync(id);

        if (erpShipToAddress == null || erpShipToAddress.IsDeleted)
            return null;

        return erpShipToAddress;
    }

    public async Task<ErpShipToAddress> GetErpShipToAddressByIdWithActiveAsync(int id)
    {
        if (id == 0)
            return null;

        var erpShipToAddress = await _erpShipToAddressRepository.GetByIdAsync(id);

        if (erpShipToAddress == null || !erpShipToAddress.IsActive || erpShipToAddress.IsDeleted)
            return null;

        return erpShipToAddress;
    }

    public virtual async Task<IPagedList<ErpShipToAddress>> GetAllErpShipToAddressesAsync(string shipToCode = "",
        string shipToName = "", 
        int erpAccountId = 0, 
        string repNum = "", 
        string repFullName = "", 
        string repEmail = "", 
        int salesOrgId = 0,
        int pageIndex = 0, 
        int pageSize = int.MaxValue, 
        bool? showHidden = null, 
        string emailAddresses = "", 
        bool isForOrder = false,
        bool getOnlyTotalCount = false)
    {
        var erpShipToAddresses = await _erpShipToAddressRepository.GetAllPagedAsync(query =>
        {
            if (erpAccountId > 0)
            {
                if (salesOrgId > 0)
                {
                    query = from address in _erpShipToAddressRepository.Table
                            join cam in _erpShiptoAddressErpAccountMapRepository.Table on address.Id equals cam.ErpShiptoAddressId
                            join acc in _erpAccountRepository.Table on cam.ErpAccountId equals acc.Id
                            where cam.ErpShipToAddressCreatedByTypeId == (int)ErpShipToAddressCreatedByType.Admin && cam.ErpAccountId == erpAccountId &&
                            acc.ErpSalesOrgId == salesOrgId
                            select address;
                }
                else
                {
                    query = from address in _erpShipToAddressRepository.Table
                            join cam in _erpShiptoAddressErpAccountMapRepository.Table on address.Id equals cam.ErpShiptoAddressId
                            join acc in _erpAccountRepository.Table on cam.ErpAccountId equals acc.Id
                            where cam.ErpShipToAddressCreatedByTypeId == (int)ErpShipToAddressCreatedByType.Admin && cam.ErpAccountId == erpAccountId
                            && acc.ErpSalesOrgId > 0
                            select address;
                }

            }
            else
            {
                if (salesOrgId > 0)
                {
                    query = from address in _erpShipToAddressRepository.Table
                            join cam in _erpShiptoAddressErpAccountMapRepository.Table on address.Id equals cam.ErpShiptoAddressId
                            join acc in _erpAccountRepository.Table on cam.ErpAccountId equals acc.Id
                            where cam.ErpShipToAddressCreatedByTypeId == (int)ErpShipToAddressCreatedByType.Admin && cam.ErpAccountId > 0 &&
                             acc.ErpSalesOrgId == salesOrgId
                            select address;
                }
                else
                {
                    query = from address in _erpShipToAddressRepository.Table
                            join cam in _erpShiptoAddressErpAccountMapRepository.Table on address.Id equals cam.ErpShiptoAddressId
                            join acc in _erpAccountRepository.Table on cam.ErpAccountId equals acc.Id
                            where cam.ErpShipToAddressCreatedByTypeId == (int)ErpShipToAddressCreatedByType.Admin && cam.ErpAccountId > 0
                             && acc.ErpSalesOrgId > 0
                            select address;
                }
            }

            query = query.Where(egp => !egp.IsDeleted);

            if (showHidden.HasValue)
            {
                if (!showHidden.Value)
                    query = query.Where(v => v.IsActive);
                else
                    query = query.Where(v => !v.IsActive);
            }

            if (!string.IsNullOrEmpty(emailAddresses))
                query = query.Where(v => v.EmailAddresses.Contains(emailAddresses));

            if (!string.IsNullOrEmpty(shipToCode))
                query = query.Where(v => v.ShipToCode.Contains(shipToCode));

            if (!string.IsNullOrEmpty(shipToName))
                query = query.Where(v => v.ShipToName.Contains(shipToName));

            if (!string.IsNullOrEmpty(repNum))
                query = query.Where(v => v.RepNumber.Contains(repNum));

            if (!string.IsNullOrEmpty(repFullName))
                query = query.Where(v => v.RepFullName.Contains(repFullName));

            if (!string.IsNullOrEmpty(repEmail))
                query = query.Where(v => v.RepEmail.Contains(repEmail));

            query = query.OrderByDescending(c => c.CreatedOnUtc);

            return query;
        }, pageIndex, pageSize, getOnlyTotalCount);

        return erpShipToAddresses;
    }

    public async Task<IList<ErpShipToAddress>> GetAllErpShipToAddressesAsync(bool showHidden = false)
    {
        var query = _erpShipToAddressRepository.Table.Where(egp => !egp.IsDeleted);

        if (!showHidden)
            query = query.Where(b => b.IsActive);

        query = query.OrderBy(b => b.ShipToCode);

        return await query.ToListAsync();
    }

    public async Task<IList<ErpShipToAddress>> GetErpShipToAddressesByErpAccountIdAsync(int erpAccountId, bool showHidden = false)
    {
        if (erpAccountId == 0)
            return null;

        var erpShipToAddresses = await _erpShipToAddressRepository.GetAllAsync(query =>
        {
            query = from address in _erpShipToAddressRepository.Table
                    join cam in _erpShiptoAddressErpAccountMapRepository.Table on address.Id equals cam.ErpShiptoAddressId
                    where cam.ErpShipToAddressCreatedByTypeId == (int)ErpShipToAddressCreatedByType.Admin
                    && cam.ErpAccountId == erpAccountId
                    && !address.IsDeleted
                    select address;

            if (!showHidden)
                query = query.Where(egp => egp.IsActive);

            query = query.OrderBy(ei => ei.Id);
            return query;
        });

        return erpShipToAddresses;
    }

    public async Task<IList<ErpShipToAddress>> GetAllErpShipToAddressesByErpAccountIdsAsync(int[] erpAccountIds, bool showHidden = false)
    {
        return await _erpShipToAddressRepository.GetAllAsync(query =>
        {
            query = from address in _erpShipToAddressRepository.Table
                    join cam in _erpShiptoAddressErpAccountMapRepository.Table on address.Id equals cam.ErpShiptoAddressId
                    where cam.ErpShipToAddressCreatedByTypeId == (int)ErpShipToAddressCreatedByType.Admin &&
                    !erpAccountIds.IsNullOrEmpty() &&
                    erpAccountIds.Contains(cam.ErpAccountId)
                    select address;

            query = query.Where(b => !b.IsDeleted);

            if (!showHidden)
                query = query.Where(b => b.IsActive);

            query = query.OrderBy(ei => ei.ShipToCode);
            return query;
        });
    }

    public async Task<Dictionary<string, List<ErpShipToAddress>>> GetErpAccountShipToAddressMappingAsync(int[] erpAccountIds,
        bool showHidden = false,
        bool isActiveOnly = false,
        int salesOrgId = 0)
    {
        var erpAccounts = _erpAccountRepository.Table;
        if (salesOrgId > 0)
        {
            erpAccounts = erpAccounts.Where(e => e.ErpSalesOrgId == salesOrgId);
        }

        var query = from address in _erpShipToAddressRepository.Table
                    join cam in _erpShiptoAddressErpAccountMapRepository.Table
                    on address.Id equals cam.ErpShiptoAddressId
                    where cam.ErpShipToAddressCreatedByTypeId == (int)ErpShipToAddressCreatedByType.Admin &&
                    erpAccountIds != null &&
                    erpAccountIds.Length > 0 && erpAccountIds.Contains(cam.ErpAccountId)
                    join erpAcc in erpAccounts on cam.ErpAccountId equals erpAcc.Id
                    select new
                    {
                        ErpAccountNumber = erpAcc.AccountNumber,
                        ShipToAddress = address
                    };

        if (!showHidden)
        {
            query = query.Where(x => !x.ShipToAddress.IsDeleted);
        }

        if (isActiveOnly)
        {
            query = query.Where(x => x.ShipToAddress.IsActive);
        }

        query = query.OrderBy(x => x.ShipToAddress.ShipToCode);

        // Fetch to memory, then group client-side to avoid server-side GroupBy guard
        var listByAccountNumber = await query.ToListAsync();
        var mappedResults = listByAccountNumber
            .GroupBy(x => x.ErpAccountNumber)
            .ToDictionary(
                group => group.Key,
                group => group
                    .Select(x => x.ShipToAddress)
                    .ToList()
            );

        return mappedResults;
    }

    public async Task<Dictionary<int, ErpShipToAddress>> GetErpAccountIdDefaultShipToAddressMappingAsync(int[] erpAccountIds, bool showHidden = false, bool isActiveOnly = false)
    {
        var query = from address in _erpShipToAddressRepository.Table
                    join cam in _erpShiptoAddressErpAccountMapRepository.Table
                    on address.Id equals cam.ErpShiptoAddressId
                    where cam.ErpShipToAddressCreatedByTypeId == (int)ErpShipToAddressCreatedByType.Admin &&
                    erpAccountIds != null &&
                    erpAccountIds.Length > 0 && erpAccountIds.Contains(cam.ErpAccountId)
                    join erpAcc in _erpAccountRepository.Table on cam.ErpAccountId equals erpAcc.Id
                    select new
                    {
                        ErpAccountId = erpAcc.Id,
                        ShipToAddress = address
                    };

        if (!showHidden)
        {
            query = query.Where(x => !x.ShipToAddress.IsDeleted);
        }

        if (isActiveOnly)
        {
            query = query.Where(x => x.ShipToAddress.IsActive);
        }

        query = query.OrderBy(x => x.ShipToAddress.ShipToCode);

        // Fetch to memory, then group client-side to avoid server-side GroupBy guard
        var listByAccountId = await query.ToListAsync();
        var mappedResults = listByAccountId
            .GroupBy(x => x.ErpAccountId)
            .ToDictionary(
                group => group.Key,
                group => group
                    .Select(x => x.ShipToAddress)
                    .FirstOrDefault()
            );

        return mappedResults;
    }

    public async Task<ErpShipToAddress> GetErpShipToAddressByShippingAddressIdAsync(int shippingAddressId)
    {
        if (shippingAddressId == 0)
            return null;

        return await _erpShipToAddressRepository.Table.FirstOrDefaultAsync(e => e.AddressId == shippingAddressId);
    }

    public async Task<IList<ErpShipToAddress>> GetAllErpShipToAddressByAddressIdsAsync(IList<int> addressIds)
    {
        if (addressIds == null || addressIds.Count == 0)
            return null;

        var erpShipToAddresses = await _erpShipToAddressRepository.Table
            .Where(erpAddress => addressIds.Contains(erpAddress.AddressId))
            .OrderByDescending(x => x.CreatedOnUtc).ToListAsync();

        return erpShipToAddresses;
    }

    public async Task<List<ErpShipToAddress>> GetErpShipToAddressesByCustomerAddressesAsync(int customerId, int erpAccountId = 0, int erpShipToAddressCreatedByTypeId = 0, bool showHidden = false)
    {
        var erpShipToAddresses = await _erpShipToAddressRepository.GetAllAsync(query =>
        {
            query = from shipToAddress in _erpShipToAddressRepository.Table
                    join customerAddressMapping in _customerAddressMappingRepository.Table
                    on shipToAddress.AddressId equals customerAddressMapping.AddressId
                    where customerAddressMapping.CustomerId == customerId
                    select shipToAddress;

            query = query.Distinct();

            query = query.Where(v => !v.IsDeleted);

            if (!showHidden)
            {
                query = query.Where(v => v.IsActive);
            }

            if (erpShipToAddressCreatedByTypeId > 0)
            {
                if (erpAccountId > 0)
                {
                    query = from sa in query
                            join sam in _erpShiptoAddressErpAccountMapRepository.Table on sa.Id equals sam.ErpShiptoAddressId
                            where sam.ErpShipToAddressCreatedByTypeId == erpShipToAddressCreatedByTypeId && sam.ErpAccountId == erpAccountId
                            select sa;
                }
                else
                {
                    query = from sa in query
                            join sam in _erpShiptoAddressErpAccountMapRepository.Table on sa.Id equals sam.ErpShiptoAddressId
                            where sam.ErpShipToAddressCreatedByTypeId == erpShipToAddressCreatedByTypeId
                            select sa;
                }
            }

            query = query.OrderByDescending(c => c.CreatedOnUtc);

            return query;
        });

        return erpShipToAddresses.ToList();
    }

    public async Task<int> CountErpShipToAddressOfSameShipToCodeAndErpAccountIdAsync(string shipToCode, int erpAccountId, ErpShipToAddressCreatedByType createdByType)
    {
        if (erpAccountId < 1)
            return 0;

        return await (from address in _erpShipToAddressRepository.Table
                    join cam in _erpShiptoAddressErpAccountMapRepository.Table on address.Id equals cam.ErpShiptoAddressId
                    where cam.ErpShipToAddressCreatedByTypeId == (int)createdByType &&
                    cam.ErpAccountId == erpAccountId &&
                    address.IsActive &&
                    !address.IsDeleted &&
                    address.ShipToCode.Trim() == shipToCode.Trim()
                    select address).CountAsync();
    }

    public async Task<ErpShipToAddress> GetErpShipToAddressByNopAddressIdAsync(int nopAddressId)
    {
        if (nopAddressId <= 0)
            return null;

        return await _erpShipToAddressRepository.Table.FirstOrDefaultAsync(x => x.AddressId == nopAddressId && !x.IsDeleted);
    }

    public async Task<IList<ErpShipToAddress>> GetAllErpShipToAddressByAddressIdAsync(IList<int> addressIds)
    {
        if (addressIds == null || addressIds.Count == 0)
            return null;

        return await _erpShipToAddressRepository.Table
            .Where(erpAddress => addressIds.Contains(erpAddress.AddressId))
            .OrderByDescending(x => x.CreatedOnUtc)
            .ToListAsync();
    }

    #endregion

    #endregion

    #region ErpShipToAddressErpAccountMap

    public async Task<IList<ErpShiptoAddressErpAccountMap>> GetErpShipToAddressErpAccountMapsByErpAccountIdsAsync(int[] erpAccountIds)
    {
        if (erpAccountIds.Length == 0)
            return null;

        return await _erpShiptoAddressErpAccountMapRepository.Table
            .Where(m => erpAccountIds
                .Contains(m.ErpAccountId) && m.ErpShipToAddressCreatedByTypeId == (int)ErpShipToAddressCreatedByType.Admin)
            .ToListAsync();
    }

    public virtual async Task<ErpShiptoAddressErpAccountMap> GetErpShipToAddressErpAccountMapByErpShipToAddressIdAsync(int erpShipToAddressId)
    {
        if (erpShipToAddressId == 0)
            return null;

        return await _erpShiptoAddressErpAccountMapRepository.Table
            .FirstOrDefaultAsync(m => m.ErpShiptoAddressId == erpShipToAddressId);
    }

    public virtual async Task RemoveErpShipToAddressErpAccountMapAsync(ErpAccount erpAccount, ErpShipToAddress erpShipToAddress)
    {
        ArgumentNullException.ThrowIfNull(erpAccount);

        if (await _erpShiptoAddressErpAccountMapRepository.Table
            .FirstOrDefaultAsync(m => m.ErpShiptoAddressId == erpShipToAddress.Id && m.ErpAccountId == erpAccount.Id)
            is ErpShiptoAddressErpAccountMap mapping)
        {
            if (erpAccount.BillingAddressId == erpShipToAddress.Id)
                erpAccount.BillingAddressId = null;

            await _erpShiptoAddressErpAccountMapRepository.DeleteAsync(mapping);
        }
    }

    public virtual async Task InsertErpShipToAddressErpAccountMapAsync(ErpAccount erpAccount,
        ErpShipToAddress erpShipToAddress,
        ErpShipToAddressCreatedByType createdByType)
    {
        ArgumentNullException.ThrowIfNull(erpAccount);

        if (await _erpShiptoAddressErpAccountMapRepository.Table
            .FirstOrDefaultAsync(m => m.ErpShiptoAddressId == erpShipToAddress.Id && m.ErpAccountId == erpAccount.Id) is null)
        {
            var mapping = new ErpShiptoAddressErpAccountMap
            {
                ErpShiptoAddressId = erpShipToAddress.Id,
                ErpAccountId = erpAccount.Id,
                ErpShipToAddressCreatedByTypeId = (int)createdByType
            };

            await _erpShiptoAddressErpAccountMapRepository.InsertAsync(mapping);
        }
    }

    public async Task InsertErpShipToAddressErpAccountMapsAsync(IList<ErpShiptoAddressErpAccountMap> erpShiptoAddressErpAccountMaps)
    {
        await _erpShiptoAddressErpAccountMapRepository.InsertAsync(erpShiptoAddressErpAccountMaps);
    }

    public virtual async Task<IList<ErpShipToAddress>> GetErpShipToAddressesByAccountIdAsync(bool showHidden = false, bool isActiveOnly = false, int accountId = 0)
    {
        var query = from address in _erpShipToAddressRepository.Table
                    join cam in _erpShiptoAddressErpAccountMapRepository.Table on address.Id equals cam.ErpShiptoAddressId
                    where cam.ErpShipToAddressCreatedByTypeId == (int)ErpShipToAddressCreatedByType.Admin && cam.ErpAccountId == accountId
                    select address;

        if (!showHidden)
            query = query.Where(b => !b.IsDeleted);

        if (isActiveOnly)
            query = query.Where(b => b.IsActive);

        var key = _staticCacheManager.PrepareKeyForDefaultCache(NopCustomerServicesDefaults.CustomerAddressesCacheKey, accountId);

        return await _staticCacheManager.GetAsync(key, async () => await query.ToListAsync());
    }

    public virtual async Task<ErpShipToAddress> GetErpShipToAddressAsync(int accountId, int erpShiptoAddressId)
    {
        if (accountId == 0 || erpShiptoAddressId == 0)
            return null;

        var query = from address in _erpShipToAddressRepository.Table
                    join cam in _erpShiptoAddressErpAccountMapRepository.Table on address.Id equals cam.ErpShiptoAddressId
                    where cam.ErpShipToAddressCreatedByTypeId == (int)ErpShipToAddressCreatedByType.Admin &&
                    cam.ErpAccountId == accountId && address.Id == erpShiptoAddressId
                    select address;

        var key = _staticCacheManager.PrepareKeyForDefaultCache(NopCustomerServicesDefaults.CustomerAddressCacheKey, accountId, erpShiptoAddressId);

        return await _staticCacheManager.GetAsync(key, async () => await query.FirstOrDefaultAsync());
    }

    public virtual async Task<ErpShipToAddress> GetCustomerBillingAddressAsync(ErpAccount erpAccount)
    {
        return erpAccount is null
            ? throw new ArgumentNullException(nameof(erpAccount))
            : await GetErpShipToAddressAsync(erpAccount.Id, erpAccount.BillingAddressId ?? 0);
    }

    public async Task<ErpShipToAddress> GetErpShipToAddressByNopOrderIdAsync(int nopOrderId)
    {
        if (nopOrderId == 0)
            return null;

        return await (from s in _erpShipToAddressRepository.Table
                      join m in _erpShiptoAddressErpAccountMapRepository.Table
                      on s.Id equals m.ErpShiptoAddressId
                      where s.OrderId == nopOrderId &&
                            m.ErpShipToAddressCreatedByTypeId == (int)ErpShipToAddressCreatedByType.User
                      select s).FirstOrDefaultAsync();
    }

    public async Task<(ErpShipToAddress ShipToAddress, string ErrorMessage)> CreateErpShipToAddressWithMappingAsync(
        ErpShipToAddress erpShipToAddress,
        ErpAccount erpAccount,
        ErpShipToAddressCreatedByType createdByType)
    {
        try
        {
            ArgumentNullException.ThrowIfNull(erpShipToAddress);
            ArgumentNullException.ThrowIfNull(erpAccount);

            if (string.IsNullOrWhiteSpace(erpShipToAddress.ShipToCode))
                throw new ArgumentException("ShipToCode cannot be null or empty", nameof(erpShipToAddress));

            if (erpAccount.Id < 1)
                throw new ArgumentException("ErpAccount Id cannot be zero", nameof(erpShipToAddress));

            var countDuplicates = await CountErpShipToAddressOfSameShipToCodeAndErpAccountIdAsync(
                erpShipToAddress.ShipToCode.Trim(),
                erpAccount.Id,
                createdByType);

            if (countDuplicates > 0)
            {
                throw new InvalidOperationException(
                    $"A ShipToAddress with ShipToCode '{erpShipToAddress.ShipToCode.Trim()}' already exists for ErpAccount '{erpAccount.AccountNumber}' (ID: {erpAccount.Id})");
            }

            await _erpShipToAddressRepository.InsertAsync(erpShipToAddress);

            await _erpShiptoAddressErpAccountMapRepository.InsertAsync(new ErpShiptoAddressErpAccountMap
            {
                ErpShiptoAddressId = erpShipToAddress.Id,
                ErpAccountId = erpAccount.Id,
                ErpShipToAddressCreatedByTypeId = (int)createdByType
            });

            return (erpShipToAddress, string.Empty);
        }
        catch (Exception ex)
        {
            return (null, $"Error: {ex.Message}");
        }
    }

    public string GenerateUniqueShipToCode()
    {
        return $"M-{DateTime.UtcNow:yyMMddHHmmssfff}";
    }

    #endregion
}
