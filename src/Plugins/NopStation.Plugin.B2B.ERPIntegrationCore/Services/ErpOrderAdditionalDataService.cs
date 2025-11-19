using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Orders;
using Nop.Data;
using Nop.Services.Orders;
using NopStation.Plugin.B2B.ERPIntegrationCore.Domain;
using NopStation.Plugin.B2B.ERPIntegrationCore.Enums;

namespace NopStation.Plugin.B2B.ERPIntegrationCore.Services;

public class ErpOrderAdditionalDataService : IErpOrderAdditionalDataService
{
    #region Fields

    private readonly IRepository<ErpOrderAdditionalData> _erpOrderAdditionalDataRepository;
    private readonly IRepository<Order> _orderRepository;
    private readonly IRepository<ErpAccount> _erpAccountRepository;
    private readonly IRepository<ErpSalesOrg> _erpSalesOrgRepository;
    private readonly IOrderService _orderService;
    private readonly IRepository<OrderItem> _orderItemRepository;
    private readonly IRepository<Product> _productRepository;

    #endregion

    #region Ctor

    public ErpOrderAdditionalDataService(IRepository<ErpOrderAdditionalData> erpOrderAdditionalDataRepository,
        IRepository<Order> orderRepository,
        IOrderService orderService,
        IRepository<OrderItem> orderItemRepository,
        IRepository<Product> productRepository,
        IRepository<ErpAccount> erpAccountRepository,
        IRepository<ErpSalesOrg> erpSalesOrgRepository)
    {
        _erpOrderAdditionalDataRepository = erpOrderAdditionalDataRepository;
        _orderRepository = orderRepository;
        _erpAccountRepository = erpAccountRepository;
        _erpSalesOrgRepository = erpSalesOrgRepository;
        _orderService = orderService;
        _orderItemRepository = orderItemRepository;
        _productRepository = productRepository;
    }

    #endregion

    #region Methods

    #region Insert/Update

    public async Task InsertErpOrderAdditionalDataAsync(
        ErpOrderAdditionalData erpOrderAdditionalData
    )
    {
        await _erpOrderAdditionalDataRepository.InsertAsync(erpOrderAdditionalData);
    }

    public async Task UpdateErpOrderAdditionalDataAsync(
        ErpOrderAdditionalData erpOrderAdditionalData
    )
    {
        await _erpOrderAdditionalDataRepository.UpdateAsync(erpOrderAdditionalData);
    }

    #endregion

    #region Delete

    private async Task DeleteErpOrderAdditionalDataAsync(
        ErpOrderAdditionalData erpOrderAdditionalData
    )
    {
        await _erpOrderAdditionalDataRepository.DeleteAsync(erpOrderAdditionalData);
    }

    public async Task DeleteErpOrderAdditionalDataByIdAsync(int id)
    {
        var erpOrderAdditionalData = await GetErpOrderAdditionalDataByIdAsync(id);
        if (erpOrderAdditionalData != null)
        {
            await DeleteErpOrderAdditionalDataAsync(erpOrderAdditionalData);
        }
    }

    #endregion

    #region Read

    public async Task<Dictionary<string, bool>> CheckAccountHasOrders(string salesOrgCode, string[] erpAccountNumbers)
    {
        var orders = (
            from erpOrder in _erpOrderAdditionalDataRepository.Table
            join erpAccount in _erpAccountRepository.Table on erpOrder.ErpAccountId equals erpAccount.Id
            join salesOrg in _erpSalesOrgRepository.Table on erpAccount.ErpSalesOrgId equals salesOrg.Id
            where salesOrg.Code == salesOrgCode && erpAccountNumbers.Contains(erpAccount.AccountNumber)
            select erpAccount.AccountNumber
        ).Distinct().ToList();

        var accountNumberExistsDict = new Dictionary<string, bool>();

        foreach (var accountNumber in erpAccountNumbers)
        {
            accountNumberExistsDict.TryAdd(
                accountNumber,
                orders.Exists(o => o == accountNumber)
            );
        }

        return accountNumberExistsDict;
    }

    public async Task<bool> IfCustomerReferenceExistWithThisErpAccount(string customerReference, int erpAccountId)
    {
        if (string.IsNullOrEmpty(customerReference) || erpAccountId == 0)
        {
            return false;
        }

        return await _erpOrderAdditionalDataRepository.Table.AnyAsync(x =>
            x.CustomerReference == customerReference && x.ErpAccountId == erpAccountId
        );
    }

    public async Task<IList<ErpOrderAdditionalData>> GetAllFailedOrProcessingOrQueuedErpOrders(
        int maxIntegrationRetries = 0
    )
    {
        var erpOrderAdditionalData = await _erpOrderAdditionalDataRepository.GetAllPagedAsync(
            query =>
            {
                query = query.Where(x =>
                    x.IntegrationStatusTypeId == (int)IntegrationStatusType.Failed
                    || x.IntegrationStatusTypeId == (int)IntegrationStatusType.Processing
                    || x.IntegrationStatusTypeId == (int)IntegrationStatusType.Queued
                );

                query = query.Where(x => x.IntegrationRetries < maxIntegrationRetries);

                query = query.OrderByDescending(ei => ei.Id);

                return query;
            }
        );

        return erpOrderAdditionalData;
    }

    public async Task<ErpOrderAdditionalData> GetErpOrderAdditionalDataByIdAsync(int id)
    {
        if (id <= 0)
            return null;

        return await _erpOrderAdditionalDataRepository.GetByIdAsync(id, cache => default);
    }

    public async Task<IPagedList<ErpOrderAdditionalData>> GetAllErpOrderAdditionalDataAsync(
        int pageIndex = 0,
        int pageSize = int.MaxValue,
        bool getOnlyTotalCount = false,
        int accountId = 0,
        int nopCustomerId = 0,
        string email = null,
        string erpOrderNumber = null,
        string nopOrderNumber = null,
        int erpOrderOriginTypeId = 0,
        int erpOrderTypeId = 0,
        int integrationStatusTypeId = 0,
        DateTime? searchOrderDateFrom = null,
        DateTime? searchOrderDateTo = null
    )
    {
        var erpOrderAdditionalData = await _erpOrderAdditionalDataRepository.GetAllPagedAsync(
            query =>
            {
                if (accountId > 0)
                    query = query.Where(x => x.ErpAccountId == accountId);
                if (erpOrderTypeId > 0)
                    query = query.Where(x => x.ErpOrderTypeId == erpOrderTypeId);
                if (integrationStatusTypeId > 0)
                    query = query.Where(x => x.IntegrationStatusTypeId == integrationStatusTypeId);
                if (erpOrderOriginTypeId > 0)
                    query = query.Where(x => x.ErpOrderOriginTypeId == erpOrderOriginTypeId);
                if (!string.IsNullOrEmpty(erpOrderNumber))
                {
                    query = query.Where(x =>
                        x.ErpOrderNumber.Contains(erpOrderNumber.ToLower()) ||
                        x.CustomerReference.Contains(erpOrderNumber.ToLower())
                    );
                }
                if ((searchOrderDateFrom != null && searchOrderDateFrom.HasValue) ||
                    (searchOrderDateTo != null && searchOrderDateTo.HasValue))
                {
                    query =
                        from or in _orderRepository.Table
                        join q in query on or.Id equals q.NopOrderId
                        where
                            (!searchOrderDateFrom.HasValue || or.CreatedOnUtc >= searchOrderDateFrom.Value) &&
                            (!searchOrderDateTo.HasValue || or.CreatedOnUtc <= searchOrderDateTo.Value)
                        select q;
                }
                if (nopCustomerId > 0)
                {
                    query =
                        from or in _orderRepository.Table
                        join q in query on or.Id equals q.NopOrderId
                        where or.CustomerId == nopCustomerId
                        select q;
                }
                query = query.OrderByDescending(ei => ei.Id);

                return query;
            },
            pageIndex,
            pageSize,
            getOnlyTotalCount
        );

        return erpOrderAdditionalData;
    }

    public async Task<ErpOrderAdditionalData> GetErpOrderAdditionalDataByNopOrderIdAsync(
        int nopOrderId
    )
    {
        if (nopOrderId <= 0)
            return null;

        var query =
            from c in _erpOrderAdditionalDataRepository.Table
            where c.NopOrderId == nopOrderId
            orderby c.Id
            select c;

        return await query.FirstOrDefaultAsync();
    }

    public async Task<IList<ErpOrderAdditionalData>> GetErpOrderAdditionalDatasByAccountIdAsync(
        int accountId
    )
    {
        var erpOrderAdditionalData = await _erpOrderAdditionalDataRepository.GetAllAsync(query =>
        {
            if (accountId > 0)
                query = query.Where(x => x.ErpAccountId == accountId);
            query = query.OrderBy(ei => ei.Id);
            return query;
        });

        return erpOrderAdditionalData;
    }

    public async Task<Order> GetNopOrderByErpOrderNumberAsync(string erpOrderNumber)
    {
        if (string.IsNullOrEmpty(erpOrderNumber))
            return null;

        var erpOrderAdditionalData = _erpOrderAdditionalDataRepository.Table.FirstOrDefault(od =>
            od.ErpOrderNumber == erpOrderNumber
        );
        if (erpOrderAdditionalData != null && erpOrderAdditionalData.NopOrderId > 0)
            return await _orderService.GetOrderByIdAsync(erpOrderAdditionalData.NopOrderId);
        return null;
    }

    public async Task<ErpOrderAdditionalData> GetErpOrderAdditionalDataByErpAccountIdAndErpOrderNumberAsync(int accountId, string erpOrderNumber)
    {
        if (accountId <= 0 || string.IsNullOrEmpty(erpOrderNumber))
            return null;

        return await _erpOrderAdditionalDataRepository
            .Table.Where(x =>
                x.ErpAccountId == accountId && x.ErpOrderNumber.Trim() == erpOrderNumber.Trim()
            )
            .FirstOrDefaultAsync();
    }

    public async Task<ErpOrderAdditionalData> GetErpOrderAdditionalDataByByQuoteSalesOrderId(
        int quoteSalesOrderId
    )
    {
        if (quoteSalesOrderId <= 0)
            return null;

        return await _erpOrderAdditionalDataRepository.Table.FirstOrDefaultAsync(x =>
            x.QuoteSalesOrderId == quoteSalesOrderId
        );
    }

    #endregion

    #region Customer functionality

    public async Task<bool> CheckQuoteOrderStatusAsync(
        ErpOrderAdditionalData erpOrderAdditionalData
    )
    {
        if (
            erpOrderAdditionalData == null
            || erpOrderAdditionalData.ErpOrderType == ErpOrderType.B2BSalesOrder
            || erpOrderAdditionalData.ErpOrderType == ErpOrderType.B2CSalesOrder
        )
            return false;

        if (
            erpOrderAdditionalData.QuoteExpiryDate == null
            || string.IsNullOrEmpty(erpOrderAdditionalData.ERPOrderStatus)
        )
            return false;

        if (erpOrderAdditionalData.QuoteExpiryDate.Value.Date < DateTime.UtcNow.Date)
            return false;

        // a quote can be placed only once (so if any order placed already with this quote order then it is false)
        if (
            erpOrderAdditionalData.QuoteSalesOrderId.HasValue
            && erpOrderAdditionalData.QuoteSalesOrderId.Value > 0
        )
            return false;

        return
            erpOrderAdditionalData.ERPOrderStatus
                == ERPIntegrationCoreDefaults.ERPOrderStatusApproved
            || erpOrderAdditionalData.ERPOrderStatus
                == ERPIntegrationCoreDefaults.ERPOrderStatusPendingApproval
            || erpOrderAdditionalData.ERPOrderStatus == nameof(OrderStatus.Complete)
        ;
    }

    public async Task<IDictionary<string, string>> GetAllCustomerReferencesByERPOrderNumbersAsync(
        IList<string> erpOrderNumbers
    )
    {
        if (erpOrderNumbers == null || !erpOrderNumbers.Any())
            return null;

        var query = _erpOrderAdditionalDataRepository.Table.Where(opa =>
            erpOrderNumbers.Contains(opa.ErpOrderNumber)
        );

        return await query.ToDictionaryAsync(t => t.ErpOrderNumber, t => t.CustomerReference);
    }

    public async Task<IPagedList<Product>> FindOrderProductsAsync(
        ErpAccount erpAccount,
        int lastNOrders,
        int pageIndex = 0,
        int pageSize = int.MaxValue
    )
    {
        ArgumentNullException.ThrowIfNull(erpAccount);

        if (pageSize == int.MaxValue)
            pageSize = int.MaxValue - 1;

        var orderIds = (
            from oad in _erpOrderAdditionalDataRepository.Table
            join o in _orderRepository.Table on oad.NopOrderId equals o.Id
            where oad.ErpAccountId == erpAccount.Id
            orderby o.CreatedOnUtc descending
            select o.Id
        ).Take(lastNOrders);

        var productIdsQuery = (
            from oi in _orderItemRepository.Table
            join o in _orderRepository.Table on oi.OrderId equals o.Id
            where orderIds.Contains(oi.OrderId)
            select oi.ProductId
        ).Distinct();

        int totalRecords = await productIdsQuery.CountAsync();

        var pagedProductIds = await productIdsQuery
            .OrderBy(id => id)
            .Skip(pageIndex * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var products = await (
            from p in _productRepository.Table
            where pagedProductIds.Contains(p.Id)
            select p
        ).ToListAsync();

        return new PagedList<Product>(products, pageIndex, pageSize, totalRecords);
    }

    public async Task<bool> IsShipToAddressUsedToOrderAsync(int shipToAddressId)
    {
        return (await _erpOrderAdditionalDataRepository.Table
            .FirstOrDefaultAsync(erpOrder => erpOrder.ErpShipToAddressId == shipToAddressId)) != null;
    }

    #endregion

    // later, we can fix the enum names(i.e. B2bQuote) to avoid the space in localized enum value
    public string GetErpOrderTypeByOrderTypeEnum(ErpOrderType erpOrderType)
    {
        return erpOrderType switch
        {
            ErpOrderType.B2BQuote => "B2B Quote",
            ErpOrderType.B2BSalesOrder => "B2B Sales Order",
            ErpOrderType.B2CQuote => "B2C Quote",
            ErpOrderType.B2CSalesOrder => "B2C Sales Order",
            _ => string.Empty
        };
    }

    #endregion
}