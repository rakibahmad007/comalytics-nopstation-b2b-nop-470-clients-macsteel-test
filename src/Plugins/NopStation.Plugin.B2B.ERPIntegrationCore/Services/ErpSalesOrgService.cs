using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Core.Domain.Shipping;
using Nop.Data;
using NopStation.Plugin.B2B.ERPIntegrationCore.Domain;

namespace NopStation.Plugin.B2B.ERPIntegrationCore.Services;

public class ErpSalesOrgService : IErpSalesOrgService
{
    #region Fields

    private readonly IRepository<ErpSalesOrg> _erpErpSalesOrgRepository;
    private readonly IRepository<ErpAccount> _erpAccountRepository;
    private readonly IRepository<ErpWarehouseSalesOrgMap> _erpWarehouseSalesOrgMapRepository;
    private readonly IRepository<ErpProductNotePerSalesOrg> _erpProductNotePerSalesOrgrepository;
    private readonly IRepository<Warehouse> _warehouseRepository;

    #endregion

    #region Ctor

    public ErpSalesOrgService(IRepository<ErpSalesOrg> erpSalesOrgRepository,
        IRepository<ErpAccount> erpAccountRepository,
        IRepository<ErpWarehouseSalesOrgMap> erpWarehouseSalesOrgMapRepository,
        IRepository<Warehouse> warehouseRepository,
        IRepository<ErpProductNotePerSalesOrg> erpProductNotePerSalesOrgrepository)
    {
        _erpErpSalesOrgRepository = erpSalesOrgRepository;
        _erpAccountRepository = erpAccountRepository;
        _erpWarehouseSalesOrgMapRepository = erpWarehouseSalesOrgMapRepository;
        _warehouseRepository = warehouseRepository;
        _erpProductNotePerSalesOrgrepository = erpProductNotePerSalesOrgrepository;
    }

    #endregion

    #region Methods

    #region Insert/Update

    public async Task InsertErpSalesOrgAsync(ErpSalesOrg erpSalesOrg)
    {
        await _erpErpSalesOrgRepository.InsertAsync(erpSalesOrg);
    }

    public async Task UpdateErpSalesOrgAsync(ErpSalesOrg erpSalesOrg)
    {
        await _erpErpSalesOrgRepository.UpdateAsync(erpSalesOrg);
    }

    public async Task UpdateErpSalesOrgsAsync(IList<ErpSalesOrg> erpSalesOrgs)
    {
        await _erpErpSalesOrgRepository.UpdateAsync(erpSalesOrgs);
    }

    #endregion

    #region Delete

    private async Task DeleteErpSalesOrgAsync(ErpSalesOrg erpSalesOrg)
    {
        //as ErpBaseEntity dosen't inherit ISoftDelete but has that feature
        erpSalesOrg.IsDeleted = true;
        await _erpErpSalesOrgRepository.UpdateAsync(erpSalesOrg);
    }

    public async Task DeleteErpSalesOrgByIdAsync(int id)
    {
        var erpSalesOrg = await GetErpSalesOrgByIdAsync(id);
        if (erpSalesOrg != null)
        {
            await DeleteErpSalesOrgAsync(erpSalesOrg);
        }
    }

    #endregion

    #region Read

    public async Task<ErpSalesOrg> GetErpSalesOrgByIdAsync(int id)
    {
        if (id == 0)
            return null;

        var erpSalesOrg = await _erpErpSalesOrgRepository.GetByIdAsync(id, cache => default);

        if (erpSalesOrg == null || erpSalesOrg.IsDeleted)
            return null;

        return erpSalesOrg;
    }

    public async Task<ErpSalesOrg> GetErpSalesOrgByCodeAsync(string code)
    {
        if (string.IsNullOrWhiteSpace(code))
            return null;

        var erpSalesOrg = await _erpErpSalesOrgRepository.Table.FirstOrDefaultAsync(x => x.Code.Trim().ToLower() == code.Trim().ToLower());

        if (erpSalesOrg == null || erpSalesOrg.IsDeleted)
            return null;

        return erpSalesOrg;
    }

    public async Task<ErpSalesOrg> GetErpSalesOrgByIdWithActiveAsync(int id)
    {
        if (id == 0)
            return null;

        var erpSalesOrg = await _erpErpSalesOrgRepository.GetByIdAsync(id, cache => default);

        if (erpSalesOrg == null || !erpSalesOrg.IsActive || erpSalesOrg.IsDeleted)
            return null;

        return erpSalesOrg;
    }

    public async Task<IPagedList<ErpSalesOrg>> GetAllErpSalesOrgAsync(int pageIndex = 0, int pageSize = int.MaxValue, string name = null, string email = null, string code = null, bool? showHidden = null, bool getOnlyTotalCount = false)
    {
        var erpSalesOrgs = await _erpErpSalesOrgRepository.GetAllPagedAsync(query =>
        {
            // showHidden is null for getting all, true for only actives and false for only inactives
            if (showHidden.HasValue)
            {
                if (!showHidden.Value)
                    query = query.Where(v => v.IsActive);
                else
                    query = query.Where(v => !v.IsActive);
            }

            if (!string.IsNullOrWhiteSpace(name))
                query = query.Where(c => c.Name.Contains(name));

            if (!string.IsNullOrWhiteSpace(email))
                query = query.Where(c => c.Email.Contains(email));

            if (!string.IsNullOrWhiteSpace(code))
                query = query.Where(c => c.Code.Contains(code));

            query = query.Where(egp => !egp.IsDeleted);
            query = query.OrderBy(egp => egp.Id);
            return query;

        }, pageIndex, pageSize, getOnlyTotalCount);

        return erpSalesOrgs;
    }

    public async Task<IList<ErpSalesOrg>> GetErpSalesOrgsAsync(bool isActive = true, bool filterOutDeleted = true)
    {
        var erpSalesOrgs = await _erpErpSalesOrgRepository.GetAllAsync(query =>
        {
            if (isActive)
                query = query.Where(v => v.IsActive);
            if (filterOutDeleted)
                query = query.Where(v => !v.IsDeleted);
            query = query.OrderBy(ea => ea.Name);
            return query;
        });

        return erpSalesOrgs;
    }

    public async Task<bool> IsMappedWithAnyERPAccountAsync(int erpSalesOrgId)
    {
        var isMapped = await _erpAccountRepository.Table.AnyAsync(ea => !ea.IsDeleted && ea.ErpSalesOrgId == erpSalesOrgId);
        return isMapped;
    }

    public async Task<IList<ErpSalesOrg>> GetErpSalesOrganisationsByIdsAsync(int[] salesOrgIds)
    {
        if (salesOrgIds == null || salesOrgIds.Length == 0)
            return new List<ErpSalesOrg>();

        var salesOrgs = await _erpErpSalesOrgRepository.Table
            .Where(p => salesOrgIds.Contains(p.Id) && !p.IsDeleted)
            .ToListAsync();

        //sort by passed identifiers
        var sortedSaleOrgs = new List<ErpSalesOrg>();
        foreach (var id in salesOrgIds)
        {
            var salesOrg = salesOrgs.Find(x => x.Id == id);
            if (salesOrg != null)
                sortedSaleOrgs.Add(salesOrg);
        }

        return sortedSaleOrgs;
    }

    #endregion

    public async Task<IList<ErpSalesOrg>> GetSalesOrganisationsByCodesAsync(IList<string> salesOrgCodes)
    {
        if (salesOrgCodes is null || !salesOrgCodes.Any())
            return new List<ErpSalesOrg>();

        return await _erpErpSalesOrgRepository.Table.Where(x => salesOrgCodes.Contains(x.Code)).ToListAsync();
    }

    public async Task<ErpSalesOrg> GetErpSalesOrgByTradingWarehouseIdAsync(int tradingWarehouseId)
    {
        if (tradingWarehouseId == 0)
            return null;

        return await (from b in _erpErpSalesOrgRepository.Table
                      where !b.IsDeleted &&
                            b.IsActive &&
                            b.TradingWarehouseId == tradingWarehouseId
                      select b).FirstOrDefaultAsync();
    }

    public async Task<IList<ErpSalesOrg>> GetAllErpSalesOrgByTradingWarehouseId(int tradingWarehouseId)
    {
        if (tradingWarehouseId == 0)
            return null;

        return await (from b in _erpErpSalesOrgRepository.Table
                      where !b.IsDeleted &&
                            b.IsActive &&
                            b.TradingWarehouseId == tradingWarehouseId
                      select b).ToListAsync();
    }

    public async Task<ErpSalesOrg> GetErpSalesOrgByErpAccountIdAsync(int erpAccountId)
    {
        if (erpAccountId == 0)
            return null;

        return await (from ea in _erpAccountRepository.Table
                      join so in _erpErpSalesOrgRepository.Table on ea.ErpSalesOrgId equals so.Id
                      where ea.Id == erpAccountId && !so.IsDeleted && so.IsActive
                      select so).FirstOrDefaultAsync();
    }

    public async Task<bool> CheckAnyB2BSalesOrgProductsExistBySalesOrgIdAndProductsIdAsync(
        int salesOrgId,
        int productId
    )
    {
        if (salesOrgId == 0 || productId == 0)
            return false;

        var query = _erpProductNotePerSalesOrgrepository.Table;
        return await query.AnyAsync(a => a.SalesOrgId == salesOrgId && a.ProductId == productId);
    }

    public async Task<Warehouse> GetNopWarehousebyB2CWarehouseCodeAsync(string warehouseCode)
    {
        if (string.IsNullOrEmpty(warehouseCode))
            return null;

        return await(from w in _warehouseRepository.Table
                     join map in _erpWarehouseSalesOrgMapRepository.Table
                         on w.Id equals map.NopWarehouseId
                     where map.WarehouseCode == warehouseCode && map.IsB2CWarehouse
                     select w).FirstOrDefaultAsync();
    }

    #endregion
}
