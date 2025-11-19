using System.Data;
using System.Text.RegularExpressions;
using ClosedXML.Excel;
using Microsoft.Data.SqlClient;
using Nop.Core;
using Nop.Core.Caching;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Localization;
using Nop.Core.Domain.Media;
using Nop.Data;
using Nop.Services.Catalog;
using Nop.Services.Common;
using Nop.Services.ExportImport.Help;
using Nop.Services.Logging;
using Nop.Services.Media;
using Nop.Services.Seo;

namespace Nop.Plugin.Comalytics.PictureAndSEOExportImport.Services;

public class PictureAndSEOExportImportService : IPictureAndSEOExportImportService
{
    private readonly IPictureService _pictureService;
    private readonly IUrlRecordService _urlRecordService;
    private readonly IGenericAttributeService _genericAttributeService;
    private readonly IWorkContext _workContext;
    private readonly CatalogSettings _catalogSettings;
    private readonly ProductEditorSettings _productEditorSettings;
    private readonly ISpecificationAttributeService _specificationAttributeService;
    private readonly ILogger _logger;
    private readonly IStaticCacheManager _cacheManager;

    public PictureAndSEOExportImportService(IPictureService pictureService,
                                            IUrlRecordService urlRecordService,
                                            IGenericAttributeService genericAttributeService,
                                            IWorkContext workContext,
                                            CatalogSettings catalogSettings,
                                            ProductEditorSettings productEditorSettings,
                                            ISpecificationAttributeService specificationAttributeService,
                                            ILogger logger,
                                            IStaticCacheManager cacheManager)
    {
        _pictureService = pictureService;
        _urlRecordService = urlRecordService;
        _genericAttributeService = genericAttributeService;
        _workContext = workContext;
        _catalogSettings = catalogSettings;
        _productEditorSettings = productEditorSettings;
        _specificationAttributeService = specificationAttributeService;
        _logger = logger;
        _cacheManager = cacheManager;
    }

    #region Utilities

    private async Task<string[]> GetPictureUrlsAsync(Product product)
    {
        //pictures (up to 3 pictures)
        string picture1 = null;
        string picture2 = null;
        string picture3 = null;
        var pictures = await _pictureService.GetPicturesByProductIdAsync(product.Id, 3);
        for (var i = 0; i < pictures.Count; i++)
        {
            var pictureLocalPath = await _pictureService.GetThumbLocalPathAsync(pictures[i]);
            switch (i)
            {
                case 0:
                    picture1 = pictureLocalPath;
                    break;

                case 1:
                    picture2 = pictureLocalPath;
                    break;

                case 2:
                    picture3 = pictureLocalPath;
                    break;
            }
        }

        return new[] { picture1, picture2, picture3 };
    }

    private async Task<Picture[]> GetPicturesAsync(Product product)
    {
        Picture picture1 = new Picture(), picture2 = new Picture(), picture3 = new Picture();
        var pictures = await _pictureService.GetPicturesByProductIdAsync(product.Id, 3);
        for (var i = 0; i < pictures.Count; i++)
        {
            switch (i)
            {
                case 0:
                    picture1 = pictures[i];
                    break;

                case 1:
                    picture2 = pictures[i];
                    break;

                case 2:
                    picture3 = pictures[i];
                    break;
            }
        }
        return new[] { picture1, picture2, picture3 };
    }

    private async Task<int> WriteStreamInDatabaseAsync(Stream stream, string destinationTableName, bool hasHeader = true)
    {
        var connectionString = DataSettingsManager.LoadSettings().ConnectionString;

        var dataTable = await GetDatatableFromXlsx(stream, hasHeader = true);

        using (var bulkCopy = new SqlBulkCopy(connectionString))
        {
            bulkCopy.DestinationTableName = destinationTableName; // Your SQL Table name

            // column mappings
            foreach (DataColumn column in dataTable.Columns)
            {
                bulkCopy.ColumnMappings.Add(column.ColumnName, column.ColumnName);
            }
            //Put your column X number here

            await bulkCopy.WriteToServerAsync(dataTable);
        }

        return dataTable?.Rows?.Count ?? 0;
    }

    private string GetProcessedNameWithOutSpace(string str)
    {
        var regex = new Regex(@"[\s]+([a-z0-9])", RegexOptions.IgnoreCase);
        str = regex.Replace(str, m => m.ToString().Trim().ToUpper());
        return str;
    }

    private async Task<DataTable> GetDatatableFromXlsx(Stream stream, bool hasHeader = true)
    {
        try
        {
            using (var workbook = new XLWorkbook(stream))
            {
                var worksheet = workbook.Worksheets.FirstOrDefault();
                if (worksheet == null)
                    throw new NopException("No worksheet found");

                var dataTable = new DataTable();

                // Get first row for columns
                var firstRow = worksheet.FirstRowUsed();
                if (firstRow == null)
                    throw new NopException("Worksheet has no data");

                // Add columns
                foreach (var cell in firstRow.Cells())
                {
                    dataTable.Columns.Add(hasHeader
                        ? GetProcessedNameWithOutSpace(cell.GetValue<string>())
                        : $"Column {cell.Address.ColumnNumber}");
                }

                // Add rows
                var startRow = hasHeader ? firstRow.RowNumber() + 1 : firstRow.RowNumber();
                foreach (var row in worksheet.Rows(startRow, worksheet.LastRowUsed().RowNumber()))
                {
                    var dataRow = dataTable.NewRow();
                    for (int col = 1; col <= dataTable.Columns.Count; col++)
                    {
                        dataRow[col - 1] = row.Cell(col).Value; // Fill data from the cell
                    }
                    dataTable.Rows.Add(dataRow);
                }

                return dataTable;
            }
        }
        catch (Exception exc)
        {
            await _logger.ErrorAsync($"Error Reading File in GetDatatableFromXlsx method due to - {exc.Message}", exc);
            return null;
        }
    }

    #endregion Utilities

    #region Methods

    public async Task<byte[]> ExportToExcelAsync(IEnumerable<Product> products)
    {
        var properties = new[]
        {
            new PropertyByName<Product, Language>("ProductId", (p, l) => p.Id),

            new PropertyByName<Product, Language>("Name", (p, l) => p.Name),
            new PropertyByName<Product, Language>("Sku", (p, l) => p.Sku),

            new PropertyByName<Product, Language>("MetaKeywords", (p, l) => p.MetaKeywords),
            new PropertyByName<Product, Language>("MetaDescription", (p, l) => p.MetaDescription),
            new PropertyByName<Product, Language>("MetaTitle", (p, l) => p.MetaTitle),
            new PropertyByName<Product, Language>("SeName", async (p, l) => await _urlRecordService.GetSeNameAsync(p, 0)),

            new PropertyByName<Product, Language>("Picture1Id", async (p, l) => (await GetPicturesAsync(p))[0].Id),
            new PropertyByName<Product, Language>("Picture1Title", async (p, l) => (await GetPicturesAsync(p))[0].TitleAttribute??""),
            new PropertyByName<Product, Language>("Picture1Alt", async (p, l) => (await GetPicturesAsync(p))[0].AltAttribute),
            new PropertyByName<Product, Language>("Picture1Url", async (p, l) => (await GetPictureUrlsAsync (p))[0]),

            new PropertyByName<Product, Language>("Picture2Id", async (p, l) => (await GetPicturesAsync(p))[1].Id),
            new PropertyByName<Product, Language>("Picture2Title",async (p, l) => (await GetPicturesAsync(p))[1].TitleAttribute),
            new PropertyByName<Product, Language>("Picture2Alt",async (p, l) => (await GetPicturesAsync(p))[1].AltAttribute),
            new PropertyByName<Product, Language>("Picture2Url", async (p, l) => (await GetPictureUrlsAsync(p))[1]),

            new PropertyByName<Product, Language>("Picture3Id", async(p, l) => (await GetPicturesAsync(p))[2].Id),
            new PropertyByName<Product, Language>("Picture3Title",async (p, l) => (await GetPicturesAsync(p))[2].TitleAttribute),
            new PropertyByName<Product, Language>("Picture3Alt", async(p, l) => (await GetPicturesAsync(p))[2].AltAttribute),
            new PropertyByName<Product, Language>("Picture3Url", async(p, l) => (await GetPictureUrlsAsync(p))[2]),
        };

        var productList = products.ToList();

        var productAdvancedMode = true;
        try
        {
            productAdvancedMode = await _genericAttributeService.GetAttributeAsync<bool>(await _workContext.GetCurrentCustomerAsync(), "product-advanced-mode");
        }
        catch (ArgumentNullException)
        {
        }

        return await new PropertyManager<Product, Language>(properties, _catalogSettings).ExportToXlsxAsync(productList);
    }

    public async Task<int> ImportExcelAsync(Stream stream)
    {
        var connString = DataSettingsManager.LoadSettings().ConnectionString;
        using (var connection = new SqlConnection(connString))
        {
            connection.Open();
            try
            {
                var query = @"Truncate TABLE [dbo].[ProductPictureAndSEOUpdateExcelImport];";
                var cmd = new SqlCommand(query, connection);
                var rowChanged = await cmd.ExecuteNonQueryAsync();
                connection.Close();
            }
            catch (Exception ex)
            {
                await _logger.ErrorAsync(ex.Message);
                connection.Close();
            }
        }
        var totalRow = await WriteStreamInDatabaseAsync(stream, "ProductPictureAndSEOUpdateExcelImport");

        if (totalRow > 0)
        {
            using (var connection = new SqlConnection(connString))
            {
                connection.Open();
                try
                {
                    var query = "[dbo].[ProductPictureAndSEOUpdateExcelImportProcedure]";
                    var cmd = new SqlCommand(query, connection)
                    {
                        CommandType = CommandType.StoredProcedure
                    };

                    cmd.CommandTimeout = 3600;

                    var returnParameter = cmd.Parameters.Add("@ReturnVal", SqlDbType.Int);
                    returnParameter.Direction = ParameterDirection.ReturnValue;

                    var rowChanged = await cmd.ExecuteNonQueryAsync();
                    var result = returnParameter.Value;
                    connection.Close();

                    await _cacheManager.RemoveByPrefixAsync("Nop.urlrecord."); // in 4.2 - NopSeoDefaults.UrlRecordPrefixCacheKey

                    return int.Parse(result.ToString());
                }
                catch (Exception ex)
                {
                    await _logger.ErrorAsync(ex.Message);
                    connection.Close();
                }
            }
        }

        return -1;
    }

    #endregion
}