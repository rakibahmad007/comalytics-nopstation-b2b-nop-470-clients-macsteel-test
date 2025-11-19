using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ClosedXML.Excel;
using Nop.Data;
using Nop.Services.Catalog;
using Nop.Services.Helpers;
using Nop.Services.Logging;
using Nop.Web.Areas.Admin.Models.Catalog;
using Nop.Web.Framework.Models.Extensions;
using NopStation.Plugin.B2B.B2BB2CFeatures.Areas.Admin.Models;
using NopStation.Plugin.B2B.B2BB2CFeatures.Services.ErpPriceListDownloadTrack;
using NopStation.Plugin.B2B.ERPIntegrationCore.Domain;
using NopStation.Plugin.B2B.ERPIntegrationCore.Services;

namespace NopStation.Plugin.B2B.B2BB2CFeatures.Areas.Admin.Factories;

public class ErpSpecialPriceModelFactory : IErpSpecialPriceModelFactory
{
    #region Fields

    private readonly IErpSalesOrgService _erpSalesOrgService;
    private readonly IErpAccountService _erpAccountService;
    private readonly IErpSpecialPriceService _erpSpecialPriceService;
    private readonly IDateTimeHelper _dateTimeHelper;
    private readonly IERPExportImportManager _erpExportImportManager;
    private readonly ILogger _logger;
    private readonly ICategoryService _categoryService;
    private readonly INopDataProvider _nopDataProvider;

    #endregion

    #region Ctor

    public ErpSpecialPriceModelFactory(
        IErpAccountService erpAccountService,
        IErpSalesOrgService erpSalesOrgService,
        IErpSpecialPriceService erpSpecialPriceService,
        IDateTimeHelper dateTimeHelper,
        IERPExportImportManager erpExportImportManager,
        ILogger logger,
        ICategoryService categoryService,
        INopDataProvider nopDataProvider)
    {
        _erpSalesOrgService = erpSalesOrgService;
        _erpAccountService = erpAccountService;
        _erpSpecialPriceService = erpSpecialPriceService;
        _dateTimeHelper = dateTimeHelper;
        _erpExportImportManager = erpExportImportManager;
        _logger = logger;
        _categoryService = categoryService;
        _nopDataProvider = nopDataProvider;
    }

    #endregion

    #region Methods

    public async Task<ErpSpecialPriceSearchModel> PrepareErpProductSpecialPriceSearchModel(ErpSpecialPriceSearchModel searchModel, int productId)
    {
        ArgumentNullException.ThrowIfNull(searchModel);

        searchModel.ProductId = productId;
        searchModel.SetGridPageSize();
        return searchModel;
    }

    public async Task<ErpSpecialPriceListModel> PrepareErpProductSpecialPriceListModel(ErpSpecialPriceSearchModel searchModel)
    {
        ArgumentNullException.ThrowIfNull(searchModel);

        var erpProductPricings = await _erpSpecialPriceService.GetAllErpSpecialPricesAsync(
            pageIndex: searchModel.Page - 1,
            pageSize: searchModel.PageSize,
            getOnlyTotalCount: false,
            productId: searchModel.ProductId,
            accountId: searchModel.SearchErpAccountId,
            onlyIncludeActiveErpAccountsMappedPrices: true);

        var erpAccounts = await _erpAccountService.GetErpAccountListAsync();
        var erpSalesOrgs = await _erpSalesOrgService.GetErpSalesOrgsAsync(isActive: true, filterOutDeleted: true);
        var model = new ErpSpecialPriceListModel().PrepareToGrid(searchModel, erpProductPricings, () =>
        {
            return erpProductPricings.Select(productPricing =>
            {
                var pricingModel = new ErpSpecialPriceModel
                {
                    Id = productPricing.Id,
                    ProductId = productPricing.NopProductId,
                    Price = productPricing.Price,
                    PricingNote = productPricing.PricingNote,
                    DiscountPerc = productPricing.DiscountPerc,
                    PercentageOfAllocatedStock = productPricing.PercentageOfAllocatedStock
                };

                var erpAccount = erpAccounts.FirstOrDefault(w => w.Id == productPricing.ErpAccountId);
                if (erpAccount != null)
                {
                    var erpAccountSalesOrg = erpSalesOrgs.FirstOrDefault(w => w.Id == erpAccount.ErpSalesOrgId);

                    if (erpAccountSalesOrg != null)
                    {
                        pricingModel.ErpAccountId = productPricing.ErpAccountId;
                        pricingModel.ErpAccountNumber = $"{erpAccount.AccountName} - ({erpAccount.AccountNumber})";
                        pricingModel.ErpAccountSalesOrgId = erpAccount.ErpSalesOrgId;
                        pricingModel.ErpAccountSalesOrgName = $"{erpAccountSalesOrg.Name} - ({erpAccountSalesOrg.Code})";
                    }
                }

                return pricingModel;
            }).Where(x => x.ErpAccountId > 0 && x.ErpAccountSalesOrgId > 0);
        });
        return model;
    }

    public async Task<ErpSpecialPriceModel> PrepareErpProductSpecialPriceModel(ErpSpecialPriceModel model, ErpSpecialPrice erpProductPricing)
    {
        if (erpProductPricing != null)
        {
            var erpAccount = await _erpAccountService.GetErpAccountByIdAsync(erpProductPricing.ErpAccountId);

            model = model ?? new ErpSpecialPriceModel();
            model.Id = erpProductPricing.Id;
            model.ProductId = erpProductPricing.NopProductId;
            model.Price = erpProductPricing.Price;
            model.DiscountPerc = erpProductPricing.DiscountPerc;
            model.PricingNote = erpProductPricing.PricingNote;
            model.PercentageOfAllocatedStock = erpProductPricing.PercentageOfAllocatedStock;
            if (erpProductPricing.PercentageOfAllocatedStockResetTimeUtc.HasValue)
                model.PercentageOfAllocatedStockResetTimeUtc = await _dateTimeHelper.ConvertToUserTimeAsync(erpProductPricing.PercentageOfAllocatedStockResetTimeUtc.Value, DateTimeKind.Utc);

            if (erpAccount != null)
            {
                var erpAccountSalesOrg = await _erpSalesOrgService.GetErpSalesOrgByIdAsync(erpAccount.ErpSalesOrgId);

                if (erpAccountSalesOrg != null)
                {
                    model.ErpAccountId = erpProductPricing.ErpAccountId;
                    model.ErpAccountNumber = $"{erpAccount.AccountName} - ({erpAccount.AccountNumber})";
                    model.ErpAccountSalesOrgId = erpAccount.ErpSalesOrgId;
                    model.ErpAccountSalesOrgName = $"{erpAccountSalesOrg.Name} - ({erpAccountSalesOrg.Code})";
                }
            }
        }

        return model;
    }

    public async Task<byte[]> ExportSpecialPriceToXlsx(List<int> list)
    {
        var query = @"
            SELECT 
                account.AccountNumber,
                account.AccountName,
                ESO.Name as [Sales organisation name],
                p.Sku,
                ESP.Price,
                ESP.PricingNote,
                ESP.CustomerUoM,
                ESP.PercentageOfAllocatedStock,
                ESP.DiscountPerc
            FROM 
                Erp_Special_Price ESP WITH (NOLOCK)
            INNER JOIN 
                Product p ON 
                    p.Id = ESP.NopProductId AND 
                    p.Deleted = 0 AND 
                    p.Published = 1 AND 
                    p.Id IN (" + string.Join(", ", list) + ") " +
            "INNER JOIN " +
                "Erp_Account account ON " +
                    "account.Id = ESP.ErpAccountId " +
            "INNER JOIN " +
                "Erp_Sales_Org ESO ON " +
                    "ESO.Id = account.ErpSalesOrgId " +
            "ORDER BY p.Sku;";
        try
        {
            var currentDate = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            var dataTable = await _erpExportImportManager.GetXLWorkbookByQuery(query, new object { });
            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add($"SpecialPricing_{currentDate}");

            // Copy data from DataTable to Excel worksheet
            worksheet.Cell(1, 1).InsertTable(dataTable);
            // Save the workbook to a MemoryStream
            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            stream.Position = 0;

            // Read the contents of the MemoryStream into a byte array
            var bytes = new byte[stream.Length];
            await stream.ReadAsync(bytes);

            return bytes;
        }
        catch (Exception ex)
        {
            await _logger.ErrorAsync("SpecialPricing_ Export excel File Generate Fail", ex);
        }
        return null;
    }

    public async Task<byte[]> ExportSpecialPriceToXlsxAll(ProductSearchModel searchModel)
    {
        var categoryIds = new List<int> { searchModel.SearchCategoryId };
        if (searchModel.SearchIncludeSubCategories && searchModel.SearchCategoryId > 0)
        {
            var childCategoryIds = await _categoryService.GetChildCategoryIdsAsync(parentCategoryId: searchModel.SearchCategoryId, showHidden: true);
            categoryIds.AddRange(childCategoryIds);
        }

        if (categoryIds != null && categoryIds.Contains(0))
            categoryIds.Remove(0);

        var commaSeparatedCategoryIds = categoryIds == null ? string.Empty : string.Join(",", categoryIds);

        var query = @"SELECT 
                              account.AccountNumber, 
                              account.AccountName, 
                              SO.Name as [Sales organisation name], 
                              SO.Code, 
                              p.Sku, 
                              PAPP.Price, 
                              PAPP.PricingNote, 
                              PAPP.CustomerUoM, 
                              PAPP.PercentageOfAllocatedStock, 
                              PAPP.DiscountPerc 
                            FROM 
                              Erp_Special_Price PAPP with(nolock) 
                              inner join Product p on p.Id = PAPP.NopProductId 
                              and p.Deleted = 0 
                              and p.Published = 1 
                              inner join Erp_Account account on account.Id = PAPP.ErpAccountId 
                              inner join Erp_Sales_Org SO on SO.Id = account.ErpSalesOrgId
                            ";

        if (categoryIds.Count > 0)
            query += " INNER JOIN Product_Category_Mapping pcm with (NOLOCK) ON p.Id = pcm.ProductId ";

        query += " where p.Deleted=0 ";

        if (!string.IsNullOrEmpty(searchModel.SearchProductName))
            query += " and p.[Name] like '%" + searchModel.SearchProductName + "%' ";

        if (searchModel.SearchPublishedId > 0)
            query += " and p.Published = " + searchModel.SearchPublishedId % 2;

        if (searchModel.SearchWarehouseId > 0)
        {
            query += " AND  ( (p.UseMultipleWarehouses = 0 AND p.WarehouseId = " + searchModel.SearchWarehouseId + " )OR (p.UseMultipleWarehouses > 0 AND EXISTS(SELECT 1 FROM ProductWarehouseInventory[pwi] WHERE[pwi].WarehouseId = " + searchModel.SearchWarehouseId + " AND[pwi].ProductId = p.Id)) )";
        }

        if (searchModel.SearchProductTypeId > 0)
            query += " and p.ProductTypeId = " + searchModel.SearchProductTypeId;

        if (categoryIds.Count > 0)
            query += " AND pcm.CategoryId IN ( " + commaSeparatedCategoryIds + ")";

        query += " order by p.sku;";

        try
        {
            var currentDate = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            var dataTable = await _erpExportImportManager.GetXLWorkbookByQuery(query, new object { });
            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add($"SpecialPricing_{currentDate}");

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
        catch (Exception ex)
        {
            await _logger.ErrorAsync("SpecialPricing_ Export excel File Generate Fail", ex);
        }
        return null;
    }

    public async void ImportSpecialPriceFromXlsx(Stream stream)
    {
        await _nopDataProvider.ExecuteNonQueryAsync("Truncate TABLE [dbo].[SpecialPricingImport];");

        var totalRow = _erpExportImportManager.WriteStreamInDatabase(stream, "SpecialPricingImport");

        if (totalRow > 0)
        {
            await _nopDataProvider.ExecuteNonQueryAsync("[dbo].[SpecialPricingImportProcedure]");
        }
    }

    #endregion
}
