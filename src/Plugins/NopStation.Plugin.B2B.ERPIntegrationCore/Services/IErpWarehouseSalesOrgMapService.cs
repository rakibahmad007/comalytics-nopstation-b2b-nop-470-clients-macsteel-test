using System.Collections.Generic;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Core.Domain.Catalog;
using NopStation.Plugin.B2B.ERPIntegrationCore.Domain;

namespace NopStation.Plugin.B2B.ERPIntegrationCore.Services;

public interface IErpWarehouseSalesOrgMapService
{
    Task InsertErpWarehouseSalesOrgMapAsync(ErpWarehouseSalesOrgMap erpWarehouseSalesOrgMap);

    Task UpdateErpWarehouseSalesOrgMapAsync(ErpWarehouseSalesOrgMap erpWarehouseSalesOrgMap);

    Task DeleteErpWarehouseSalesOrgMapByIdAsync(int id);

    Task<ErpWarehouseSalesOrgMap> GetErpWarehouseSalesOrgMapByIdAsync(int id);

    Task<IList<ErpWarehouseSalesOrgMap>> GetErpWarehouseSalesOrgMapsBySalesOrgIdAsync(int salesOrgId, bool? isB2cWarehouse = null);

    Task<IPagedList<ErpWarehouseSalesOrgMap>> GetAllErpWarehouseSalesOrgMapsAsync(int pageIndex = 0, int pageSize = int.MaxValue, bool getOnlyTotalCount = false, int salesOrgId = 0, bool? isB2CWarehouse = null);

    Task<bool> CheckAnyErpSalesOrgWarehouseExistBySalesOrgIdAndNopWarehouseId(int salesOrgId, int nopWarehouseId, bool? isB2cWarehouse = null);

    Task<ErpWarehouseSalesOrgMap> GetErpWarehouseSalesOrgMapByWarehouseCodeAsync(string warehouseCode, bool isB2cWarehouse);

    Task<ErpWarehouseSalesOrgMap> GetB2CSalesOrgWarehouseMapForProduct(Product product, int salesOrganisationId, int quantity);

    Task<IList<ProductWarehouseInventory>> GetProductWarehouseInventoriesByProductIdSalesOrgIdAsync(int productId, int salesOrgId, bool isB2cWarehouse);

    Task<List<ErpWarehouseSalesOrgMap>> GetSaleOrgWarehousebySalesOrgIdAsync(int salesOrgId);
}
