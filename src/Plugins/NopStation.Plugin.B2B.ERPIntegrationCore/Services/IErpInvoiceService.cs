using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Nop.Core;
using NopStation.Plugin.B2B.ERPIntegrationCore.Domain;
using NopStation.Plugin.B2B.ERPIntegrationCore.Enums;

namespace NopStation.Plugin.B2B.ERPIntegrationCore.Services;

public interface IErpInvoiceService
{
    Task InsertErpInvoiceAsync(ErpInvoice erpInvoice);
    Task InsertErpInvoicesAsync(List<ErpInvoice> erpInvoices);

    Task UpdateErpInvoiceAsync(ErpInvoice erpInvoice);
    Task UpdateErpInvoicesAsync(List<ErpInvoice> erpInvoices);

    Task DeleteErpInvoiceByIdAsync(int id);

    Task<ErpInvoice> GetErpInvoiceByIdAsync(int id);

    Task<IPagedList<ErpInvoice>> GetAllErpInvoiceAsync(
        int pageIndex = 0,
        int pageSize = int.MaxValue,
        bool getOnlyTotalCount = false,
        string erpOrderNumber = null,
        string documentName = null,
        DateTime? documentDateUtc = null,
        int erpAccountId = 0,
        int customerId = 0,
        IList<int> documentTypeIds = null,
        string erpDocumentNumber = null,
        string customOrderNumber = null,
        DateTime? postingFromDateUtc = null,
        DateTime? postingToDateUtc = null,
        FinancialDocumentSortingEnum sortBy = FinancialDocumentSortingEnum.Position
    );

    Task<IList<ErpInvoice>> GetErpInvoicesByErpAccountIdAsync(int erpAccountId);

    Task<IList<ErpInvoice>> GetErpInvoicesByOrderNumberAsync(string orderNumber);

    (int, int) GetTotalNumberOfInvoicesAndNumOfShippedItemsByErpAccountIdERPOrderNumber(
        int erpAccountId,
        string erpOrderNumber
    );
    Task<int> CheckIsInvoiceOrPodAvailableByErpAccountIdERPOrderNumberAsync(
        int erpAccountId,
        string erpOrderNumber
    );
}
