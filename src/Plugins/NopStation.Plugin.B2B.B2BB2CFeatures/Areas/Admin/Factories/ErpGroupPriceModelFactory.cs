using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ClosedXML.Excel;
using Nop.Services.Catalog;
using Nop.Services.Logging;
using Nop.Web.Areas.Admin.Models.Catalog;
using Nop.Web.Framework.Models.Extensions;
using NopStation.Plugin.B2B.B2BB2CFeatures.Areas.Admin.Models;
using NopStation.Plugin.B2B.B2BB2CFeatures.Services.ErpPriceListDownloadTrack;
using NopStation.Plugin.B2B.ERPIntegrationCore.Domain;
using NopStation.Plugin.B2B.ERPIntegrationCore.Services;

namespace NopStation.Plugin.B2B.B2BB2CFeatures.Areas.Admin.Factories;

public class ErpGroupPriceModelFactory : IErpGroupPriceModelFactory
{
    #region Fields

    private readonly IErpGroupPriceService _erpGroupPriceService;
    private readonly IErpGroupPriceCodeService _erpPriceGroupCodeService;
    private readonly IErpGroupPriceCodeModelFactory _erpPriceGroupCodeModelFactory;
    private readonly IERPExportImportManager _eRPExportImportManager;
    private readonly ILogger _logger;
    private readonly ICategoryService _categoryService;

    #endregion

    #region Ctor

    public ErpGroupPriceModelFactory(
        IErpGroupPriceService erpGroupPriceService,
        IErpGroupPriceCodeService erpPriceGroupCodeService,
        IErpGroupPriceCodeModelFactory erpPriceGroupCodeModelFactory,
        IERPExportImportManager eRPExportImportManager,
        ILogger logger,
        ICategoryService categoryService)
    {
        _erpGroupPriceService = erpGroupPriceService;
        _erpPriceGroupCodeService = erpPriceGroupCodeService;
        _erpPriceGroupCodeModelFactory = erpPriceGroupCodeModelFactory;
        _eRPExportImportManager = eRPExportImportManager;
        _logger = logger;
        _categoryService = categoryService;
    }

    #endregion

    #region Methods

    public async Task<ErpPriceGroupProductPricingSearchModel> PrepareErpProductPricingSearchModel(ErpPriceGroupProductPricingSearchModel searchModel, int productId)
    {
        ArgumentNullException.ThrowIfNull(searchModel);

        searchModel.ProductId = productId;
        searchModel.SetGridPageSize();
        await PrepareErpProductPricingModel(searchModel.AddErpPriceGroupProductPricing, null);
        return searchModel;
    }

    public async Task<ErpPriceGroupProductPricingListModel> PrepareErpProductPricingListModel(ErpPriceGroupProductPricingSearchModel searchModel)
    {
        ArgumentNullException.ThrowIfNull(searchModel);

        var erpProductPricings = await _erpGroupPriceService.GetAllErpGroupPricesAsync(
            pageIndex: searchModel.Page - 1,
            pageSize: searchModel.PageSize,
            showHidden: false,
            getOnlyTotalCount: false,
            overridePublished: false,
            productId: searchModel.ProductId,
            groupCode: searchModel.SearchErpPriceGroupCode);

        var erpGroupPriceCodes = await _erpPriceGroupCodeService.GetAllErpGroupPriceCodesAsync();

        var model = new ErpPriceGroupProductPricingListModel().PrepareToGrid(searchModel, erpProductPricings, () =>
        {
            return erpProductPricings.Select(productPricing =>
            {
                var erpGroupPriceCodeCheck = erpGroupPriceCodes.FirstOrDefault(f => f.Id == productPricing.ErpNopGroupPriceCodeId);

                if (erpGroupPriceCodeCheck is null)
                {
                    return null;
                }
                var pricingModel = new ErpPriceGroupProductPricingModel
                {
                    Id = productPricing.Id,
                    ProductId = productPricing.NopProductId,
                    ErpGroupPriceCodeId = productPricing.ErpNopGroupPriceCodeId,
                    ErpGroupPriceCode = erpGroupPriceCodeCheck.Code,
                    Price = productPricing.Price
                };

                return pricingModel;
            }).Where(x => x != null);
        });
        return model;
    }

    public async Task<ErpPriceGroupProductPricingModel> PrepareErpProductPricingModel(ErpPriceGroupProductPricingModel model, ErpGroupPrice erpProductPricing)
    {
        if (erpProductPricing != null)
        {
            var erpGroupPriceCode = await _erpPriceGroupCodeService.GetErpGroupPriceCodeByIdAsync(erpProductPricing.ErpNopGroupPriceCodeId);

            model ??= new ErpPriceGroupProductPricingModel();
            model.Id = erpProductPricing.Id;
            model.ProductId = erpProductPricing.NopProductId;
            model.ErpGroupPriceCodeId = erpProductPricing.ErpNopGroupPriceCodeId;
            model.ErpGroupPriceCode = erpGroupPriceCode.Code;
            model.Price = erpProductPricing.Price;
        }

        await _erpPriceGroupCodeModelFactory.PrepareErpGroupPriceCodes(model.AvailableErpPriceGroupCodes, false);
        return model;
    }

    public async Task<byte[]> ExportPriceGroupProductPricingToXlsx(List<int> list)
    {
        var query = @"SELECT 
                              PGC.Code, 
                              p.Sku, 
                              PGPP.Price 
                            from 
                              Erp_Group_Price PGPP with(nolock) 
                              inner join Product p on p.Id = PGPP.NopProductId 
                              and p.Deleted = 0 
                              and p.Published = 1 
                              and p.Id IN (" + string.Join(", ", list) + ") " +
                              "inner join Erp_Group_Price_Code PGC on PGPP.ErpNopGroupPriceCodeId = PGC.Id order by p.sku;";  
        
        try
        {
            var currentDate = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            var dataTable = await _eRPExportImportManager.GetXLWorkbookByQuery(query, new object { });
            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add($"GroupPricing_{currentDate}");

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
            await _logger.ErrorAsync("PriceGroupProductPricing Export excel File Generate Fail", ex);
        }
        return null;
    }

    public async Task<byte[]> ExportPriceGroupProductPricingToXlsxAll(ProductSearchModel searchModel)
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

        var query = @" SELECT
                              PGC.Code, 
                              p.Sku, 
                              PGPP.Price 
                            from 
                              Erp_Group_Price PGPP with(nolock) 
                              inner join Product p on p.Id = PGPP.NopProductId 
                              and p.Deleted = 0 
                              and p.Published = 1 
                              inner join Erp_Group_Price_Code PGC on PGPP.ErpNopGroupPriceCodeId = PGC.Id";

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
            var dataTable = await _eRPExportImportManager.GetXLWorkbookByQuery(query, new object { });
            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add($"GroupPricing_{currentDate}");

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
            await _logger.ErrorAsync("PriceGroupProductPricing Export excel File Generate Fail", ex);
        }
        return null;
    }

    public void ImportPriceGroupProductPricingFromXlsx(Stream stream)
    {
        //_objectContext.ExecuteSqlCommand("Truncate TABLE [dbo].[B2BPriceGroupProductPricingImport];");

        //var totalRow = _eRPExportImportManager.WriteStreamInDatabase(stream, "B2BPriceGroupProductPricingImport");

        //if (totalRow > 0)
        //{
        //    _objectContext.ExecuteSqlCommand("[dbo].[B2BPriceGroupProductPricingImportProcedure]",
        //    false, 3600);
        //}
    }

    #endregion
}