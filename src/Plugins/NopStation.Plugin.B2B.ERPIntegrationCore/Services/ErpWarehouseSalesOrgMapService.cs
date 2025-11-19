using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Core.Domain.Catalog;
using Nop.Data;
using NopStation.Plugin.B2B.ERPIntegrationCore.Domain;

namespace NopStation.Plugin.B2B.ERPIntegrationCore.Services;

public class ErpWarehouseSalesOrgMapService : IErpWarehouseSalesOrgMapService
{
    #region Fields

    private readonly IRepository<ErpWarehouseSalesOrgMap> _erpWarehouseSalesOrgMapRepository;
    private readonly IRepository<ErpSalesOrg> _erpSalesOrgRepository;
    private readonly IRepository<ProductWarehouseInventory> _productWarehouseInventoryRepository;

    #endregion

    #region Ctor

    public ErpWarehouseSalesOrgMapService(IRepository<ErpWarehouseSalesOrgMap> erpWarehouseSalesOrgMapRepository,
        IRepository<ErpSalesOrg> erpSalesOrgRepository,
        IRepository<ProductWarehouseInventory> productWarehouseInventoryRepository)
    {
        _erpWarehouseSalesOrgMapRepository = erpWarehouseSalesOrgMapRepository;
        _erpSalesOrgRepository = erpSalesOrgRepository;
        _productWarehouseInventoryRepository = productWarehouseInventoryRepository;
    }

    #endregion

    #region Methods

    #region Insert/Update

    public async Task InsertErpWarehouseSalesOrgMapAsync(ErpWarehouseSalesOrgMap erpWarehouseSalesOrgMap)
    {
        await _erpWarehouseSalesOrgMapRepository.InsertAsync(erpWarehouseSalesOrgMap);
    }

    public async Task UpdateErpWarehouseSalesOrgMapAsync(ErpWarehouseSalesOrgMap erpWarehouseSalesOrgMap)
    {
        await _erpWarehouseSalesOrgMapRepository.UpdateAsync(erpWarehouseSalesOrgMap);
    }

    #endregion

    #region Delete

    private async Task DeleteErpWarehouseSalesOrgMapAsync(ErpWarehouseSalesOrgMap erpWarehouseSalesOrgMap)
    {
        await _erpWarehouseSalesOrgMapRepository.DeleteAsync(erpWarehouseSalesOrgMap);
    }

    public async Task DeleteErpWarehouseSalesOrgMapByIdAsync(int id)
    {
        var erpWarehouseSalesOrgMap = await GetErpWarehouseSalesOrgMapByIdAsync(id);
        if (erpWarehouseSalesOrgMap != null)
        {
            await DeleteErpWarehouseSalesOrgMapAsync(erpWarehouseSalesOrgMap);
        }
    }

    #endregion

    #region Read

    public async Task<ErpWarehouseSalesOrgMap> GetErpWarehouseSalesOrgMapByIdAsync(int id)
    {
        if (id == 0)
            return null;

        return await _erpWarehouseSalesOrgMapRepository.GetByIdAsync(id, cache => default);
    }

    public async Task<IPagedList<ErpWarehouseSalesOrgMap>> GetAllErpWarehouseSalesOrgMapsAsync(
        int pageIndex = 0,
        int pageSize = int.MaxValue,
        bool getOnlyTotalCount = false,
        int salesOrgId = 0,
        bool? isB2CWarehouse = null)
    {
        var erpWarehouseSalesOrgMap = await _erpWarehouseSalesOrgMapRepository.GetAllPagedAsync(query =>
        {
            query = query.Where(x => x.ErpSalesOrgId == salesOrgId);

            if (isB2CWarehouse != null)
                query = query.Where(x => x.IsB2CWarehouse == isB2CWarehouse);

            query = query.OrderBy(ei => ei.Id);
            return query;

        }, pageIndex, pageSize, getOnlyTotalCount);

        return erpWarehouseSalesOrgMap;
    }

    public async Task<IList<ErpWarehouseSalesOrgMap>> GetErpWarehouseSalesOrgMapsBySalesOrgIdAsync(int salesOrgId, bool? isB2cWarehouse = null)
    {
        if (salesOrgId == 0)
            return null;
        var erpWarehouseOrgMap = await _erpWarehouseSalesOrgMapRepository.GetAllAsync(query =>
        {
            query = query.Where(v => v.ErpSalesOrgId == salesOrgId);

            if (isB2cWarehouse != null)
                query = query.Where(v => v.IsB2CWarehouse == isB2cWarehouse);

            query = query.OrderByDescending(ea => ea.Id);
            return query;
        });

        return erpWarehouseOrgMap;
    }

    public async Task<bool> CheckAnyErpSalesOrgWarehouseExistBySalesOrgIdAndNopWarehouseId(int salesOrgId, int nopWarehouseId, bool? isB2cWarehouse = null)
    {
        if (salesOrgId == 0 || nopWarehouseId == 0)
            return true;

        var existing = await _erpWarehouseSalesOrgMapRepository.GetAllAsync(query =>
        {
            query = query.Where(v => v.ErpSalesOrgId == salesOrgId && v.NopWarehouseId == nopWarehouseId);

            if (isB2cWarehouse != null)
                query = query.Where(v => v.IsB2CWarehouse == isB2cWarehouse);

            query = query.OrderByDescending(ea => ea.Id);
            return query;
        });

        return existing.Any();
    }

    public async Task<ErpWarehouseSalesOrgMap> GetErpWarehouseSalesOrgMapByWarehouseCodeAsync(string warehouseCode, bool isB2cWarehouse)
    {
        if (string.IsNullOrEmpty(warehouseCode))
            return null;

        return await _erpWarehouseSalesOrgMapRepository.Table
            .Where(map => map.WarehouseCode == warehouseCode && map.IsB2CWarehouse == isB2cWarehouse)
            .FirstOrDefaultAsync();
    }

    public async Task<ErpWarehouseSalesOrgMap> GetB2CSalesOrgWarehouseMapForProduct(Product product, int salesOrganisationId, int quantity)
    {
        var salesOrg = await _erpSalesOrgRepository.GetByIdAsync(salesOrganisationId);
        if (salesOrg == null)
            return null;

        var pwiList = _productWarehouseInventoryRepository.Table.Where(x => x.ProductId == product.Id);

        var selectedPwi = pwiList
            .Where(x => (x.WarehouseId == salesOrg.TradingWarehouseId) && (quantity <= x.StockQuantity))
            .FirstOrDefault();

        if (selectedPwi == null)
        {
            var erpWarehouseSalesOrgMap = _erpWarehouseSalesOrgMapRepository.Table
                .Where(sowh => sowh.ErpSalesOrgId == salesOrg.Id && sowh.IsB2CWarehouse);

            var otherSalesOrgWarehouseIds = erpWarehouseSalesOrgMap
                .Where(x => x.NopWarehouseId != salesOrg.TradingWarehouseId)
                .Select(x => x.NopWarehouseId);

            selectedPwi = pwiList
                .Where(x => otherSalesOrgWarehouseIds.Contains(x.WarehouseId) && quantity <= x.StockQuantity)
                .OrderByDescending(x => x.StockQuantity)
                .FirstOrDefault();
        }

        if (selectedPwi == null)
            return null;

        return _erpWarehouseSalesOrgMapRepository.Table
            .FirstOrDefault(sowh => sowh.NopWarehouseId == selectedPwi.WarehouseId && sowh.IsB2CWarehouse);
    }

    public async Task<IList<ProductWarehouseInventory>> GetProductWarehouseInventoriesByProductIdSalesOrgIdAsync(int productId, int salesOrgId, bool isB2cWarehouse)
    {
        return await (
            from pwi in _productWarehouseInventoryRepository.Table
            join sow in _erpWarehouseSalesOrgMapRepository.Table
                on pwi.WarehouseId equals sow.NopWarehouseId
            where pwi.ProductId == productId
               && sow.ErpSalesOrgId == salesOrgId
               && sow.IsB2CWarehouse == isB2cWarehouse
            select pwi
        ).Distinct().ToListAsync();
    }

    public async Task<List<ErpWarehouseSalesOrgMap>> GetSaleOrgWarehousebySalesOrgIdAsync(
        int salesOrgId
    )
    {
        if (salesOrgId < 1)
            return null;

        return await _erpWarehouseSalesOrgMapRepository
            .Table.Where(x => x.ErpSalesOrgId == salesOrgId)
            .ToListAsync();
    }

    #endregion

    #endregion
}
