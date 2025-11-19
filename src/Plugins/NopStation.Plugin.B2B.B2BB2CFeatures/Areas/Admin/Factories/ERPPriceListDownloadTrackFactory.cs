using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ClosedXML.Excel;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Services;
using Nop.Services.Customers;
using Nop.Services.Helpers;
using Nop.Services.Localization;
using Nop.Web.Framework.Models.Extensions;
using NopStation.Plugin.B2B.B2BB2CFeatures.Areas.Admin.Models.ERPPriceListDownloadTracks;
using NopStation.Plugin.B2B.B2BB2CFeatures.Services.ErpPriceListDownloadTrack;
using NopStation.Plugin.B2B.ERPIntegrationCore.Enums;
using NopStation.Plugin.B2B.ERPIntegrationCore.Services;

namespace NopStation.Plugin.B2B.B2BB2CFeatures.Areas.Admin.Factories;

public class ERPPriceListDownloadTrackFactory : IERPPriceListDownloadTrackFactory
{
    private readonly IERPPriceListDownloadTrackService _erpPriceListDownloadTrackService;
    private readonly IDateTimeHelper _dateTimeHelper;
    private readonly ILocalizationService _localizationService;
    private readonly IErpAccountService _erpAccountService;
    private readonly IErpSalesOrgService _erpSalesOrgService;
    private readonly ICustomerService _customerService;
    private readonly IERPExportImportManager _erpExportImportManager;

    public ERPPriceListDownloadTrackFactory(IERPPriceListDownloadTrackService erpPriceListDownloadTrackService,
        IDateTimeHelper dateTimeHelper,
        ILocalizationService localizationService,
        IErpAccountService erpAccountService,
        IErpSalesOrgService erpSalesOrgService,
        ICustomerService customerService,
        IERPExportImportManager erpExportImportManager)
    {
        _erpPriceListDownloadTrackService = erpPriceListDownloadTrackService;
        _dateTimeHelper = dateTimeHelper;
        _localizationService = localizationService;
        _erpAccountService = erpAccountService;
        _erpSalesOrgService = erpSalesOrgService;
        _customerService = customerService;
        _erpExportImportManager = erpExportImportManager;
    }

    public async Task<ErpPriceListListModel> PrepareERPPriceListListModelAsync(ErpPriceListSearchModel searchModel)
    {
        ArgumentNullException.ThrowIfNull(searchModel);

        DateTime? downloadFromUserTime = !searchModel.SearchDownloadedFrom.HasValue
            ? null
            : _dateTimeHelper.ConvertToUtcTime(searchModel.SearchDownloadedFrom.Value, await _dateTimeHelper.GetCurrentTimeZoneAsync());

        DateTime? downloadToUserTime = !searchModel.SearchDownloadedTo.HasValue
            ? null
            : _dateTimeHelper.ConvertToUtcTime(searchModel.SearchDownloadedTo.Value, await _dateTimeHelper.GetCurrentTimeZoneAsync()).Add(TimeSpan.FromDays(1));

        var priceListDownloadTractList = await _erpPriceListDownloadTrackService.GetAllERPPriceListDownloadTrackAsync(
            b2bAccountId: searchModel.SearchB2BAccountId,
            b2bSalesOrganisationId: searchModel.SearchB2BSalesOrganizationId,
            downloadedFromUtc: downloadFromUserTime,
            downloadedToUtc: downloadToUserTime,
            priceListDownloadTypeId: searchModel.SearchPriceListDownloadTypeId,
            pageIndex: searchModel.Page - 1,
            pageSize: searchModel.PageSize);

        var model = await new ErpPriceListListModel().PrepareToGridAsync(searchModel, priceListDownloadTractList, () =>
        {
            return priceListDownloadTractList.SelectAwait(async priceList =>
            {
                var customer = await _customerService.GetCustomerByIdAsync(priceList.NopCustomerId);
                var b2BAccount = await _erpAccountService.GetErpAccountByIdAsync(priceList.B2BAccountId);
                var salesOrg = await _erpSalesOrgService.GetErpSalesOrgByIdAsync(b2BAccount.ErpSalesOrgId);

                return new ErpPriceListModel
                {
                    Id = priceList.Id,
                    CustomerName = await _customerService.GetCustomerFullNameAsync(customer),
                    CustomerEmail = customer.Email,
                    B2BAccountName = b2BAccount.AccountName,
                    B2BAccountNumber = b2BAccount.AccountNumber,
                    B2BSalesOrganisationName = salesOrg?.Name,
                    DownloadedOn = await _dateTimeHelper.ConvertToUserTimeAsync(priceList.DownloadedOnUtc, DateTimeKind.Utc),
                    PriceListDownloadType = await _localizationService.GetLocalizedEnumAsync(priceList.PriceListDownloadType)
                };
            });
        });

        return model;
    }

    public async Task<ErpPriceListSearchModel> PrepareERPPriceListSearchModelAsync(ErpPriceListSearchModel searchModel)
    {
        ArgumentNullException.ThrowIfNull(searchModel);

        var availablePriceListDownloadTypes = await PriceListDownloadType.Excel.ToSelectListAsync(false);
        foreach (var item in availablePriceListDownloadTypes)
        {
            searchModel.AvailablPriceListTypeOptions.Add(item);
        }
        await PrepareDefaultItemAsync(searchModel.AvailablPriceListTypeOptions);
        searchModel.SetGridPageSize();
        return searchModel;
    }

    protected virtual async Task PrepareDefaultItemAsync(IList<SelectListItem> items)
    {
        ArgumentNullException.ThrowIfNull(items);

        const string value = "0";
        var defaultItemText = await _localizationService.GetResourceAsync("Admin.Common.All");
        items.Insert(0, new SelectListItem { Text = defaultItemText, Value = value });
    }

    public async Task<byte[]> ExportERPPriceListDownloadToXlsxAsync(ErpPriceListSearchModel searchModel)
    {
        var query = @"SELECT priceList.[Id]
            ,customer.[Email]
            ,account.[AccountNumber]
            ,account.[AccountName]
            ,saleOrg.[Code] AS AccountSalesOrganisationCode
            ,DATEADD(HOUR,2,priceList.[DownloadedOnUtc]) AS 'DownloadedOn(mm/dd/yy)'
            ,CASE 
            WHEN priceList.[PriceListDownloadTypeId] = 5 THEN 'Excel'
            ELSE 'PDF'
            END AS DownloadType
            FROM [dbo].[Erp_Price_List_Download_Track] priceList
            LEFT JOIN [dbo].[Customer] customer ON priceList.[NopCustomerId] = customer.Id
            LEFT JOIN [dbo].[Erp_Account] account ON priceList.B2BAccountId = account.Id
            LEFT JOIN [dbo].[Erp_Sales_Org] saleOrg ON account.ErpSalesOrgId = saleOrg.Id 
            WHERE priceList.[Id] > 0";

        if (searchModel.SearchNopCustomerId > 0)
            query += " AND priceList.[NopCustomerId] = " + searchModel.SearchNopCustomerId;
        if (searchModel.SearchB2BAccountId > 0)
            query += " AND priceList.[B2BAccountId] = " + searchModel.SearchB2BAccountId;
        if (searchModel.SearchB2BSalesOrganizationId > 0)
            query += " AND priceList.[ErpSalesOrgId] = " + searchModel.SearchB2BSalesOrganizationId;
        if (searchModel.SearchPriceListDownloadTypeId > 0)
            query += " AND priceList.[PriceListDownloadTypeId] = " + searchModel.SearchPriceListDownloadTypeId;

        query += " Order by priceList.[Id] Desc";

        // Assuming _erpExportImportManager.GetXLWorkbookByQuery returns a DataTable
        var dataTable = await _erpExportImportManager.GetXLWorkbookByQuery(query, new object { });

        using var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add("PriceListDownloadTrack");

        // Copy data from DataTable to Excel worksheet
        worksheet.Cell(1, 1).InsertTable(dataTable.AsEnumerable());

        // Save the workbook to a MemoryStream
        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        stream.Position = 0;

        // Read the contents of the MemoryStream into a byte array
        var bytes = new byte[stream.Length];
        await stream.ReadAsync(bytes);

        return bytes;
    }

    public async Task<byte[]> ExportERPPriceListDownloadToXlsxAsync(List<int> ids)
    {
        var query = @"SELECT priceList.[Id]
                ,customer.[Email]
                ,account.[AccountNumber]
                ,account.[AccountName]
                ,saleOrg.[Code] AS AccountSalesOrganisationCode
                ,DATEADD(HOUR,2,priceList.[DownloadedOnUtc]) AS 'DownloadedOn(mm/dd/yy)'
                ,CASE 
                WHEN priceList.[PriceListDownloadTypeId] = 5 THEN 'Excel'
                ELSE 'PDF'
                END AS DownloadType
                FROM [dbo].[Erp_Price_List_Download_Track] priceList
                LEFT JOIN [dbo].[Customer] customer ON priceList.[NopCustomerId] = customer.Id
                LEFT JOIN [dbo].[Erp_Account] account ON priceList.B2BAccountId = account.Id
                LEFT JOIN [dbo].[Erp_Sales_Org] saleOrg ON account.ErpSalesOrgId = saleOrg.Id 
                WHERE priceList.[Id] IN(" + string.Join(", ", ids) + ")";

        query += " Order by priceList.[Id] Desc";

        // Assuming _erpExportImportManager.GetXLWorkbookByQuery returns a DataTable
        var dataTable = await _erpExportImportManager.GetXLWorkbookByQuery(query, new object { });

        using var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add("PriceListDownloadTrack");

        // Copy data from DataTable to Excel worksheet
        worksheet.Cell(1, 1).InsertTable(dataTable.AsEnumerable());

        // Save the workbook to a MemoryStream
        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        stream.Position = 0;

        // Read the contents of the MemoryStream into a byte array
        var bytes = new byte[stream.Length];
        await stream.ReadAsync(bytes);

        return bytes;
    }
}
