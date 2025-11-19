using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Core.Domain.Orders;
using Nop.Data;
using NopStation.Plugin.B2B.ERPIntegrationCore.Domain;

namespace NopStation.Plugin.B2B.ERPIntegrationCore.Services;

public class ErpOrderItemAdditionalDataService : IErpOrderItemAdditionalDataService
{
    #region Fields

    private readonly IRepository<ErpOrderItemAdditionalData> _erpOrderItemAdditionalDataRepository;
    private readonly IRepository<OrderItem> _orderItemRepository;

    #endregion

    #region Ctor

    public ErpOrderItemAdditionalDataService(IRepository<ErpOrderItemAdditionalData> erpOrderItemAdditionalDataRepository,
        IRepository<OrderItem> orderItemRepository)
    {
        _erpOrderItemAdditionalDataRepository= erpOrderItemAdditionalDataRepository;
        _orderItemRepository = orderItemRepository;
    }

    #endregion

    #region Methods

    #region Insert/Update

    public async Task InsertErpOrderItemAdditionalDataAsync(ErpOrderItemAdditionalData erpOrderItemAdditionalData)
    {
        await _erpOrderItemAdditionalDataRepository.InsertAsync(erpOrderItemAdditionalData);
    }

    public async Task UpdateErpOrderItemAdditionalDataAsync(ErpOrderItemAdditionalData erpOrderItemAdditionalData)
    {
        await _erpOrderItemAdditionalDataRepository.UpdateAsync(erpOrderItemAdditionalData);
    }

    #endregion

    #region Delete

    private async Task DeleteErpOrderItemAdditionalDataAsync(ErpOrderItemAdditionalData erpOrderItemAdditionalData)
    {
        await _erpOrderItemAdditionalDataRepository.DeleteAsync(erpOrderItemAdditionalData);
    }

    public async Task DeleteErpOrderItemAdditionalDataByIdAsync(int id)
    {
        var erpOrderItemAdditionalData = await GetErpOrderItemAdditionalDataByIdAsync(id);
        if (erpOrderItemAdditionalData != null)
        {
            await DeleteErpOrderItemAdditionalDataAsync(erpOrderItemAdditionalData);
        }
    }

    #endregion

    #region Read

    public async Task<ErpOrderItemAdditionalData> GetErpOrderItemAdditionalDataByIdAsync(int id)
    {
        if (id == 0)
            return null;

        return await _erpOrderItemAdditionalDataRepository.GetByIdAsync(id, cache => default);
    }
    
    public async Task<ErpOrderItemAdditionalData> GetErpOrderItemAdditionalDataByNopOrderItemIdAsync(int orderItemId)
    {
        if (orderItemId == 0)
            return null;

        return await (from eoiad in _erpOrderItemAdditionalDataRepository.Table
                      where eoiad.NopOrderItemId == orderItemId
                      select eoiad).FirstOrDefaultAsync();
    }

    public async Task<IPagedList<ErpOrderItemAdditionalData>> GetAllErpOrderItemAdditionalDataAsync(int pageIndex = 0, int pageSize = int.MaxValue, bool getOnlyTotalCount = false)
    {
        var erpOrderItemAdditionalData = await _erpOrderItemAdditionalDataRepository.GetAllPagedAsync(query =>
        {
            query = query.OrderBy(ei => ei.Id);
            return query;

        }, pageIndex, pageSize, getOnlyTotalCount);

        return erpOrderItemAdditionalData;
    }

    public async Task<IList<ErpOrderItemAdditionalData>> GetAllErpOrderItemAdditionalDataByErpOrderIdAsync(int orderId)
    {
        if (orderId == 0)
            return null;

        var erpOrderItemAdditionalData = await _erpOrderItemAdditionalDataRepository.GetAllAsync(query =>
        {
            query = query.Where(ei => ei.ErpOrderId == orderId);
            query = query.OrderBy(ei => ei.Id);
            return query;
        });

        return erpOrderItemAdditionalData;
    }

    public async Task<ErpOrderItemAdditionalData> GetErpOrderAdditionalItemByErpOrderLineNumberAndNopOrderIdAndProductId(string erpOrderLineNumber, 
        int nopOrderId, int productId)
    {
        return (from ei in _erpOrderItemAdditionalDataRepository.Table
            join oi in _orderItemRepository.Table on ei.NopOrderItemId equals oi.Id
            where ei.ErpOrderLineNumber.Equals(erpOrderLineNumber) && oi.OrderId == nopOrderId && oi.ProductId == productId
            select ei).FirstOrDefault();
    }

    public async Task<IList<(ErpOrderItemAdditionalData erpOrderItem, int productId)>> GetErpOrderAdditionalItemsByNopOrderIdAsync(int nopOrderId)
    {
        var result = await _erpOrderItemAdditionalDataRepository.Table
        .Join(_orderItemRepository.Table,
              ei => ei.NopOrderItemId,
              oi => oi.Id,
              (ei, oi) => new { ei, oi })
        .Where(x => x.oi.OrderId == nopOrderId)
        .ToListAsync();

        return result.Select(x => (x.ei, x.oi.ProductId)).ToList();
    }

    public async Task<OrderItem> GetNopOrderItemByOrderIdAndProductId(int orderId, int productId)
    {
        return await (from oi in _orderItemRepository.Table
                      where oi.OrderId == orderId && oi.ProductId == productId
                      select oi).FirstOrDefaultAsync();
    }

    #endregion

    #endregion
}
