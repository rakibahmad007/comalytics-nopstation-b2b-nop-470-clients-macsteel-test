using System.Collections.Generic;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Core.Domain.Shipping;
using NopStation.Plugin.B2B.ERPIntegrationCore.Domain;

namespace NopStation.Plugin.B2B.ERPIntegrationCore.Services;

public interface IErpSalesOrgService
{
    Task InsertErpSalesOrgAsync(ErpSalesOrg erpSalesOrg);

    Task UpdateErpSalesOrgAsync(ErpSalesOrg erpSalesOrg);

    Task UpdateErpSalesOrgsAsync(IList<ErpSalesOrg> erpSalesOrgs);

    Task DeleteErpSalesOrgByIdAsync(int id);

    Task<ErpSalesOrg> GetErpSalesOrgByIdAsync(int id);

    Task<ErpSalesOrg> GetErpSalesOrgByCodeAsync(string code);

    Task<ErpSalesOrg> GetErpSalesOrgByIdWithActiveAsync(int id);

    Task<IPagedList<ErpSalesOrg>> GetAllErpSalesOrgAsync(int pageIndex = 0,
        int pageSize = int.MaxValue,
        string name = null,
        string email = null,
        string code = null,
        bool? showHidden = null,
        bool getOnlyTotalCount = false);

    Task<IList<ErpSalesOrg>> GetErpSalesOrgsAsync(bool isActive = true, bool filterOutDeleted = true);

    Task<bool> IsMappedWithAnyERPAccountAsync(int erpSalesOrgId);

    Task<IList<ErpSalesOrg>> GetSalesOrganisationsByCodesAsync(IList<string> salesOrgCodes);

    Task<ErpSalesOrg> GetErpSalesOrgByTradingWarehouseIdAsync(int tradingWarehouseId);

    Task<IList<ErpSalesOrg>> GetAllErpSalesOrgByTradingWarehouseId(int tradingWarehouseId);

    Task<Warehouse> GetNopWarehousebyB2CWarehouseCodeAsync(string warehouseCode);

    Task<IList<ErpSalesOrg>> GetErpSalesOrganisationsByIdsAsync(int[] salesOrgIds);

    Task<ErpSalesOrg> GetErpSalesOrgByErpAccountIdAsync(int erpAccountId);

    Task<bool> CheckAnyB2BSalesOrgProductsExistBySalesOrgIdAndProductsIdAsync(
        int salesOrgId,
        int productId
    );
}
