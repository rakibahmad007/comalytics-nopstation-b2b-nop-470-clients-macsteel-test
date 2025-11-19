using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ClosedXML.Excel;
using Microsoft.Data.SqlClient;
using NNopStation.Plugin.B2B.B2BB2CFeatures.Services.SpecialIncludeExcludeService;
using Nop.Core;
using Nop.Core.Caching;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Localization;
using Nop.Data;
using Nop.Services.Catalog;
using Nop.Services.ExportImport.Help;
using Nop.Services.Logging;
using NopStation.Plugin.B2B.B2BB2CFeatures.Areas.Admin.Models.SpecialIncludeExcludes;
using NopStation.Plugin.B2B.ERPIntegrationCore.Domain;
using NopStation.Plugin.B2B.ERPIntegrationCore.Enums;

namespace NopStation.Plugin.B2B.B2BB2CFeatures.Services.SpecialIncludeExcludeService
{
    public class B2BSpecialIncludeExcludeService : IB2BSpecialIncludeExcludeService
    {
        private readonly CatalogSettings _catalogSettings;
        private readonly ILogger _logger;
        private readonly IB2BExportImportManager _b2BExportImportManager;
        private readonly INopDataProvider _nopDataProvider;
        private readonly IRepository<ErpAccount> _erpAccountRepository;
        private readonly IStaticCacheManager _staticCacheManager;

        public B2BSpecialIncludeExcludeService(
            CatalogSettings catalogSettings,
            ILogger logger,
            IB2BExportImportManager b2BExportImportManager,
            IRepository<ErpAccount> erpAccountRepository,
            IStaticCacheManager staticCacheManager,
            INopDataProvider nopDataProvider)
        {
            _catalogSettings = catalogSettings;
            _logger = logger;
            _b2BExportImportManager = b2BExportImportManager;
            _erpAccountRepository = erpAccountRepository;
            _staticCacheManager = staticCacheManager;
            _nopDataProvider = nopDataProvider;
        }

        public async Task DeleteSpecialIncludeExcludeByIdAsync(int id)
        {
            await DeleteSpecialIncludeExcludeByIdListAsync(new List<int> { id });
        }

        public async Task DeleteSpecialIncludeExcludeByIdListAsync(ICollection<int> ids)
        {
            if (ids.Count < 1)
                return;

            var idDt = new DataTable();
            idDt.Columns.Add(new DataColumn("Id", typeof(int)));
            foreach (var id in ids)
            {
                var row = idDt.NewRow();
                row[idDt.Columns.IndexOf("Id")] = id;
                idDt.Rows.Add(row);
            }

            try
            {
                var connectionString = DataSettingsManager.LoadSettings().ConnectionString;
                using (var connection = new SqlConnection(connectionString))
                {
                    await connection.OpenAsync();

                    using (var cmd = new SqlCommand("dbo.SP_B2BCustomerAccount_UpdateOrDeleteSpecialIncludeExcludes", connection))
                    {
                        cmd.CommandTimeout = 900;
                        cmd.CommandType = CommandType.StoredProcedure;
                        var dataParam = cmd.Parameters.AddWithValue("@ids", idDt);
                        // Map the C# DataTable to the SQL User Defined Type
                        dataParam.SqlDbType = SqlDbType.Structured;
                        dataParam.TypeName = "dbo.B2BCustomerAccount_SpecialIncludeExcludeIdType";
                        cmd.Parameters.AddWithValue("@mode", 1);
                        cmd.Parameters.AddWithValue("@active", false);

                        var dataSets = new DataSet();
                        var dataAdapter = new SqlDataAdapter(cmd);
                        await Task.Run(() => dataAdapter.Fill(dataSets)); // Execute Fill asynchronously
                        dataAdapter.Dispose();
                        _staticCacheManager.RemoveByPrefix(NopCatalogDefaults.ProductSpecificationAttributeAllByProductPrefix);
                        await _staticCacheManager.ClearAsync();
                    }

                    // Restore previous prefilter facets
                    using (var cmd = new SqlCommand("dbo.ErpPrefilterSpecificationAttributeRestoreSP", connection))
                    {
                        cmd.CommandTimeout = 300;
                        cmd.CommandType = CommandType.StoredProcedure;
                        var dataParam = cmd.Parameters.AddWithValue("@ids", idDt);
                        dataParam.SqlDbType = SqlDbType.Structured;
                        dataParam.TypeName = "dbo.B2BCustomerAccount_SpecialIncludeExcludeIdType";
                        await cmd.ExecuteNonQueryAsync();
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Error($"Error deleting special include-exclude records. {ex.Message}. {ex.StackTrace}");
            }
        }


        public async Task<IPagedList<SpecialIncludeExcludeModel>> GetAllSpecialIncludeExcludesAsync(
        SpecialType? type = null,
        string accountName = "",
        string accountNumber = "",
        int erpSalesOrg_Id = -1,
        bool? isActive = null,
        bool? published = null,
        int pageIndex = 0,
        int pageSize = int.MaxValue,
        bool showHidden = false)
        {
            var sql = "SELECT [SpInEx].*, [Acc].[AccountNumber], [Acc].[AccountName], [So].[Code], [Prod].[Name]," +
                      " [Prod].[Sku], count(*) OVER() AS [Total] FROM" +
                      " [dbo].[Erp_Special_Includes_And_Excludes] [SpInEx] LEFT JOIN" +
                      " [dbo].[Erp_Account] [Acc] ON [SpInEx].[ErpAccountId] = [Acc].[Id]" +
                      " LEFT JOIN [dbo].[Erp_Sales_Org] [So] ON [SpInEx].[ErpSalesOrgId] = [So].[Id]" +
                      " LEFT JOIN [dbo].[Product] [Prod] ON [SpInEx].[ProductId] = [Prod].[Id]";

            var whereFlag = false;

            if (type != null)
            {
                whereFlag = true;
                sql += $" WHERE [SpInEx].[SpecialTypeId] = {(int)type} ";
            }

            if (!string.IsNullOrWhiteSpace(accountNumber))
            {
                sql += whereFlag
                    ? $" AND [Acc].[AccountNumber] LIKE '{accountNumber}' "
                    : $" WHERE [Acc].[AccountNumber] LIKE '{accountNumber}' ";
                whereFlag = true;
            }

            if (!string.IsNullOrWhiteSpace(accountName))
            {
                sql += whereFlag
                    ? $" AND [Acc].[AccountName] LIKE '{accountName}%' "
                    : $" WHERE [Acc].[AccountName] LIKE '{accountName}%' ";
                whereFlag = true;
            }

            if (erpSalesOrg_Id > 0)
            {
                sql += whereFlag
                    ? $" AND [So].[Id] = {erpSalesOrg_Id} "
                    : $" WHERE [So].[Id] = {erpSalesOrg_Id} ";
                whereFlag = true;
            }

            if (isActive != null)
            {
                sql += whereFlag
                    ? $" AND [SpInEx].[IsActive] = " + ((bool)isActive ? "1 " : "0 ")
                    : $" WHERE [SpInEx].[IsActive] = " + ((bool)isActive ? "1 " : "0 ");
                whereFlag = true;
            }

            if (published != null)
            {
                sql += whereFlag
                    ? $" AND [Prod].[Published] = " + ((bool)published ? "1 " : "0 ")
                    : $" WHERE [Prod].[Published] = " + ((bool)published ? "1 " : "0 ");
                whereFlag = true;
            }

            sql += " ORDER BY [SpInEx].[Id] DESC " +
                   "OFFSET @skip ROWS FETCH NEXT @take ROWS ONLY";

            var listModel = new List<SpecialIncludeExcludeModel>();
            var total = 0;

            try
            {
                var connectionString = DataSettingsManager.LoadSettings().ConnectionString;
                using (var connection = new SqlConnection(connectionString))
                {
                    await connection.OpenAsync();

                    using (var cmd = new SqlCommand(sql, connection))
                    {
                        cmd.CommandTimeout = 300;
                        cmd.CommandType = CommandType.Text;
                        cmd.Parameters.AddWithValue("@skip", (pageIndex - 1) * pageSize);
                        cmd.Parameters.AddWithValue("@take", pageSize);

                        using (var reader = await cmd.ExecuteReaderAsync())
                        {
                            if (reader.HasRows)
                            {
                                while (await reader.ReadAsync())
                                {
                                    var model = new SpecialIncludeExcludeModel
                                    {
                                        Id = (int)reader["Id"],
                                        AccountNumber = reader["AccountNumber"].ToString(),
                                        AccountName = reader["AccountName"].ToString(),
                                        ErpAccountId = (int)reader["ErpAccountId"],
                                        ErpSalesOrgId = (int)reader["ErpSalesOrgId"],
                                        ProductId = (int)reader["ProductId"],
                                        SpecialTypeId = (int)reader["SpecialTypeId"],
                                        ProductName = reader["Name"].ToString(),
                                        ProductSKU = reader["Sku"].ToString(),
                                        SalesOrgCode = reader["Code"].ToString(),
                                        IsActive = (bool)reader["IsActive"],
                                        LastUpdate = Convert.ToDateTime(reader["LastUpdate"])
                                    };
                                    listModel.Add(model);
                                    if (total == 0)
                                    {
                                        total = (int)reader["Total"];
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Error($"Error reading special include-exclude records. {ex.Message}. {ex.StackTrace}");
            }

            return new PagedList<SpecialIncludeExcludeModel>(listModel, pageIndex, pageSize, total);
        }

        public async Task<SpecialIncludeExcludeModel> GetSpecialIncludeExcludeByIdAsync(int id)
        {
            var sql = "SELECT [SpInEx].*, [Acc].[AccountNumber], [Acc].[AccountName], [So].[Code], [Prod].[Name]," +
                      " [Prod].[Sku] FROM" +
                      " [dbo].[Erp_Special_Includes_And_Excludes] [SpInEx] LEFT JOIN" +
                      " [dbo].[Erp_Account] [Acc] ON [SpInEx].[ErpAccountId] = [Acc].[Id]" +
                      " LEFT JOIN [dbo].[Erp_Sales_Org] [So] ON [SpInEx].[ErpSalesOrgId] = [So].[Id]" +
                      " LEFT JOIN [dbo].[Product] [Prod] ON [SpInEx].[ProductId] = [Prod].[Id] WHERE [SpInEx].[Id] = @id";

            try
            {
                var connectionString = DataSettingsManager.LoadSettings().ConnectionString;
                using (var connection = new SqlConnection(connectionString))
                {
                    await connection.OpenAsync();
                    using (var cmd = new SqlCommand(sql, connection))
                    {
                        cmd.CommandType = CommandType.Text;
                        cmd.Parameters.AddWithValue("@id", id);
                        using (var reader = await cmd.ExecuteReaderAsync())
                        {
                            if (reader.HasRows)
                            {
                                while (await reader.ReadAsync())
                                {
                                    var model = new SpecialIncludeExcludeModel
                                    {
                                        Id = (int)reader["Id"],
                                        AccountNumber = reader["AccountNumber"].ToString(),
                                        AccountName = reader["AccountName"].ToString(),
                                        ErpAccountId = (int)reader["ErpAccountId"],
                                        ErpSalesOrgId = (int)reader["ErpSalesOrgId"],
                                        ProductId = (int)reader["ProductId"],
                                        SpecialTypeId = (int)reader["SpecialTypeId"],
                                        ProductName = reader["Name"].ToString(),
                                        ProductSKU = reader["Sku"].ToString(),
                                        SalesOrgCode = reader["Code"].ToString(),
                                        IsActive = (bool)reader["IsActive"],
                                        LastUpdate = Convert.ToDateTime(reader["LastUpdate"])
                                    };
                                    return model;
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Error($"Error reading special include-exclude records by Id: {id}. {ex.Message}. {ex.StackTrace}");
            }
            return null;
        }

        public async Task<ImportResult> ImportSpecialIncludeExcludesFromXlsxAsync(Stream stream, SpecialType type)
        {
            using (var workbook = new XLWorkbook(stream))
            {
                var worksheet = workbook.Worksheets.FirstOrDefault();
                if (worksheet == null)
                    throw new NopException("No workbook found");

                // Await the asynchronous method to resolve the task and get the result
                var properties = await GetPropertiesByExcelCellsAsync<ExportImportModel>(worksheet);

                // Pass the resolved list to the PropertyManager
                var manager = new PropertyManager<ExportImportModel, Language>(properties, _catalogSettings);

                var iRow = 2;

                var dataToImport = new DataTable();
                dataToImport.Columns.Add(new DataColumn("AccountNumber", typeof(string)));
                dataToImport.Columns.Add(new DataColumn("SalesOrgCode", typeof(string)));
                dataToImport.Columns.Add(new DataColumn("SKU", typeof(string)));
                dataToImport.Columns.Add(new DataColumn("IsActive", typeof(bool)));

                while (true)
                {
                    var allColumnsAreEmpty = manager.GetDefaultProperties
                    .Select(property => worksheet.Cell(iRow, property.PropertyOrderPosition))
                    .All(cell => cell == null || string.IsNullOrEmpty(cell.GetValue<string>()));

                    if (allColumnsAreEmpty)
                        break;

                    var model = await GetModelFromXlsxAsync(manager, worksheet, iRow);
                    if (!string.IsNullOrWhiteSpace(model.AccountNumber) &&
                        !string.IsNullOrWhiteSpace(model.SalesOrgCode) &&
                        !string.IsNullOrWhiteSpace(model.SKU))
                    {
                        var row = dataToImport.NewRow();
                        row[dataToImport.Columns.IndexOf("AccountNumber")] = model.AccountNumber;
                        row[dataToImport.Columns.IndexOf("SalesOrgCode")] = model.SalesOrgCode;
                        row[dataToImport.Columns.IndexOf("SKU")] = model.SKU;
                        row[dataToImport.Columns.IndexOf("IsActive")] = model.IsActive;
                        dataToImport.Rows.Add(row);
                    }
                    iRow++;
                }

                var result = await InvokeDbStoredProcedureForImportAsync(dataToImport, type);

                _staticCacheManager.RemoveByPrefix(NopCatalogDefaults.ProductSpecificationAttributeAllByProductPrefix);
                await _staticCacheManager.ClearAsync();

                return result;
            }
        }

        public async Task<byte[]> ExportSpecialIncludeExcludeToXlsxAsync(int type, int mode)
        {
            var sql = "SELECT [SpInEx].[Id] [ID], [Acc].[AccountNumber] [AccountNumber], [So].[Code] [SalesOrgCode]," +
                      " [Prod].[Sku] [SKU], [SpInEx].[IsActive] [IsActive] FROM [dbo].[Erp_Special_Includes_And_Excludes] [SpInEx] LEFT JOIN [dbo].[Erp_Account] [Acc]" +
                      " ON [SpInEx].[ErpAccountId] = [Acc].[Id] LEFT JOIN [dbo].[Erp_Sales_Org] [So] ON [SpInEx].[ErpSalesOrgId] = [So].[Id]" +
                      " LEFT JOIN [dbo].[Product] [Prod] ON [SpInEx].[ProductId] = [Prod].[Id] WHERE [SpInEx].[SpecialTypeId] = @type";

            if (mode != 0)
            {
                mode = (mode == 1) ? 1 : 0;
                sql += " AND [SpInEx].[IsActive] = @mode;";
            }

            // Pass parameters to the query
            var dataTable = await _b2BExportImportManager.GetXLWorkbookByQuery(sql, new { type, mode });

            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("SpecialIncludeExcludes");

                // Load data into the worksheet
                worksheet.Cell(1, 1).InsertTable(dataTable);

                // Set horizontal alignment for the "IsActive" column
                var isActiveColumnIndex = 5; // Assuming "IsActive" is in the 5th column
                var rows = worksheet.RowsUsed().Skip(1);
                foreach (var row in rows)
                {
                    var cell = row.Cell(isActiveColumnIndex);
                    cell.Value = cell.Value.ToString() == "True" ? 1 : 0;
                    cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                }

                // Return the workbook as a byte array
                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    return stream.ToArray();
                }
            }
        }

        public async Task<byte[]> ExportSpecialIncludeExcludeToXlsxAsync(ICollection<int> ids)
        {
            if (ids == null || !ids.Any())
                throw new ArgumentException("The provided IDs collection is null or empty.", nameof(ids));

            var idsCommaSeparated = string.Join(",", ids).TrimEnd(',');
            var sql = $"SELECT [SpInEx].[Id] [ID], [Acc].[AccountNumber] [AccountNumber], [So].[Code] [SalesOrgCode]," +
                      " [Prod].[Sku] [SKU], [SpInEx].[IsActive] [IsActive] FROM [dbo].[Erp_Special_Includes_And_Excludes] [SpInEx] LEFT JOIN [dbo].[Erp_Account] [Acc]" +
                      " ON [SpInEx].[ErpAccountId] = [Acc].[Id] LEFT JOIN [dbo].[Erp_Sales_Org] [So] ON [SpInEx].[ErpSalesOrgId] = [So].[Id]" +
                      $" LEFT JOIN [dbo].[Product] [Prod] ON [SpInEx].[ProductId] = [Prod].[Id] WHERE [SpInEx].[Id] IN ({idsCommaSeparated});";

            // Prepare a DataTable to hold query results
            var dataTable = new DataTable();

            // Use ExecuteReaderAsync to fetch data
            using (var connection = new SqlConnection(DataSettingsManager.LoadSettings().ConnectionString))
            using (var command = new SqlCommand(sql, connection))
            {
                await connection.OpenAsync();
                using (var reader = await command.ExecuteReaderAsync())
                {
                    dataTable.Load(reader);
                }
            }

            // Create the workbook and worksheet
            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("SpecialIncludeExcludes");

            // Load the data into the worksheet
            worksheet.Cell(1, 1).InsertTable(dataTable);

            // Set horizontal alignment for the "IsActive" column (5th column)
            var column = worksheet.Column(5); // Column 5 corresponds to the `IsActive` column
            foreach (var cell in column.CellsUsed())
            {
                cell.Value = cell.GetValue<string>() == "True" ? 1 : 0;
                cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            }

            // Return the workbook as a byte array
            using var memoryStream = new MemoryStream();
            workbook.SaveAs(memoryStream);
            return memoryStream.ToArray();
        }

        public async Task UpdateSpecialIncludeExcludeAsync(int id, bool isActive, byte mode = 0)
        {
            if (id < 1)
                return;

            var idDt = new DataTable();
            idDt.Columns.Add(new DataColumn("Id", typeof(int)));
            var dtRow = idDt.NewRow();
            dtRow[idDt.Columns.IndexOf("Id")] = id;
            idDt.Rows.Add(dtRow);

            try
            {
                var connectionString = DataSettingsManager.LoadSettings().ConnectionString;
                using (var connection = new SqlConnection(connectionString))
                {
                    await connection.OpenAsync();

                    using (var cmd = new SqlCommand("dbo.SP_B2BCustomerAccount_UpdateOrDeleteSpecialIncludeExcludes", connection))
                    {
                        cmd.CommandTimeout = 900;
                        cmd.CommandType = CommandType.StoredProcedure;
                        var dataParam = cmd.Parameters.AddWithValue("@ids", idDt);
                        dataParam.SqlDbType = SqlDbType.Structured;
                        dataParam.TypeName = "dbo.B2BCustomerAccount_SpecialIncludeExcludeIdType";
                        cmd.Parameters.AddWithValue("@mode", mode);
                        cmd.Parameters.AddWithValue("@active", isActive);

                        using (var adapter = new SqlDataAdapter(cmd))
                        {
                            var dataSet = new DataSet();
                            adapter.Fill(dataSet);
                        }

                        _staticCacheManager.RemoveByPrefix(NopCatalogDefaults.ProductSpecificationAttributeAllByProductPrefix);
                        await _staticCacheManager.ClearAsync();
                    }

                    if (!isActive)
                    {
                        // Restore previous prefilter facets
                        using (var cmd = new SqlCommand("dbo.ErpPrefilterSpecificationAttributeRestoreSP", connection))
                        {
                            cmd.CommandTimeout = 300;
                            cmd.CommandType = CommandType.StoredProcedure;
                            var dataParam = cmd.Parameters.AddWithValue("@ids", idDt);
                            dataParam.SqlDbType = SqlDbType.Structured;
                            dataParam.TypeName = "dbo.B2BCustomerAccount_SpecialIncludeExcludeIdType";
                            await cmd.ExecuteNonQueryAsync();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Error($"Error creating or updating special include-exclude records. {ex.Message}. {ex.StackTrace}");
            }
        }

        public static async Task<IList<PropertyByName<T, Language>>> GetPropertiesByExcelCellsAsync<T>(IXLWorksheet workbook)
        {
            var properties = new List<PropertyByName<T, Language>>();
            var poz = 1;
            while (true)
            {
                try
                {
                    var x = workbook;
                    var y = x.Cell(1, poz).Value;



                    //var cellValue = await workbook.Cell [1, poz].GetValueAsync();

                    if (string.IsNullOrEmpty(y.ToString()))
                        break;

                    poz += 1;
                    //properties.Add(new PropertyByName<T>(cellValue.ToString()));
                    properties.Add(new PropertyByName<T, Language>(y.ToString()));

                }
                catch
                {
                    break;
                }
            }

            return properties;
        }
        protected virtual async Task<ExportImportModel> GetModelFromXlsxAsync(
    PropertyManager<ExportImportModel, Language> manager, IXLWorksheet worksheet, int iRow)
        {
            manager.ReadDefaultFromXlsx(worksheet, iRow);
            var model = new ExportImportModel();

            foreach (var property in manager.GetDefaultProperties)
            {
                switch (property.PropertyName)
                {
                    case "AccountNumber":
                        model.AccountNumber = property.StringValue;
                        break;
                    case "SalesOrgCode":
                        model.SalesOrgCode = property.StringValue;
                        break;
                    case "SKU":
                        model.SKU = property.StringValue;
                        break;
                    case "IsActive":
                        model.IsActive = property.IntValue != 0;
                        break;
                }
            }
            return model;
        }

        private async Task<ImportResult> InvokeDbStoredProcedureForImportAsync(DataTable data, SpecialType type)
        {
            var importResult = new ImportResult
            {
                GivenTotal = data.Rows.Count
            };
            try
            {
                var connectionString = DataSettingsManager.LoadSettings().ConnectionString;
                using (var connection = new SqlConnection(connectionString))
                {
                    await connection.OpenAsync();

                    using (var cmd = new SqlCommand("dbo.SP_B2BCustomerAccount_ImportSpecialIncludeExcludes", connection))
                    {
                        cmd.CommandTimeout = 900;
                        cmd.CommandType = CommandType.StoredProcedure;
                        var dataParam = cmd.Parameters.AddWithValue("@data", data);
                        dataParam.SqlDbType = SqlDbType.Structured;
                        dataParam.TypeName = "dbo.B2BCustomerAccount_SpecialIncludeExcludeImportType";
                        cmd.Parameters.AddWithValue("@type", (int)type);

                        using (var adapter = new SqlDataAdapter(cmd))
                        {
                            var dataSet = new DataSet();
                            adapter.Fill(dataSet);

                            foreach (DataRow row in dataSet.Tables[0].Rows)
                            {
                                var exportImportModel = new ExportImportModel
                                {
                                    AccountNumber = row["AccountNumber"].ToString(),
                                    SalesOrgCode = row["SalesOrgCode"].ToString(),
                                    IsActive = (bool)row["IsActive"],
                                    SKU = row["SKU"].ToString()
                                };
                                importResult.FailedImports.Add(exportImportModel);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Error($"Error executing dbo.SP_B2BCustomerAccount_ImportSpecialIncludeExcludes sp. {ex.Message}. {ex.StackTrace}");
            }
            return importResult;
        }

        public async Task<int> GetB2BSalesOrgIdByB2BAccountIdAsync(int erpAccountId)
        {
            if (erpAccountId > 0)
                return await _erpAccountRepository.GetByIdAsync(erpAccountId).ContinueWith(task =>
                    task.Result.ErpSalesOrgId);

            return 0;
        }

        public async Task AddSpecialIncludesAndExcludesAsync(SpecialIncludesAndExcludes entity)
        {
            var sql = "INSERT INTO [dbo].[Erp_Special_Includes_And_Excludes](ErpAccountId,ErpSalesOrgId,ProductId,SpecialTypeId,IsActive,LastUpdate) OUTPUT inserted.Id VALUES (@ErpAccountId,@ErpSalesOrgId,@ProductId,@SpecialTypeId,@IsActive,@LastUpdate)";
            try
            {
                var connectionString = DataSettingsManager.LoadSettings().ConnectionString;
                using (var connection = new SqlConnection(connectionString))
                {
                    await connection.OpenAsync();

                    using (var cmd = new SqlCommand(sql, connection))
                    {
                        cmd.CommandType = CommandType.Text;
                        cmd.Parameters.AddWithValue("@ErpAccountId", entity.ErpAccountId);
                        cmd.Parameters.AddWithValue("@ErpSalesOrgId", entity.ErpSalesOrgId);
                        cmd.Parameters.AddWithValue("@ProductId", entity.ProductId);
                        cmd.Parameters.AddWithValue("@SpecialTypeId", entity.SpecialTypeId);
                        cmd.Parameters.AddWithValue("@LastUpdate", entity.LastUpdate);
                        cmd.Parameters.AddWithValue("@isActive", entity.IsActive);

                        var id = await cmd.ExecuteScalarAsync();
                        await UpdateSpecialIncludeExcludeAsync((int)id, entity.IsActive, 2);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Error($"Error Adding a special include-exclude record. {ex.Message}. {ex.StackTrace}");
            }
        }

        public async Task<SpecialIncludesAndExcludes> GetUniqueB2BSpecialIncludesAndExcludesAsync(SpecialIncludeExcludeModel model)
        {
            var sql = "SELECT * FROM [dbo].[Erp_Special_Includes_And_Excludes] WHERE [ErpAccountId] = @ErpAccountId AND [ErpSalesOrgId] = @ErpSalesOrgId AND [ProductId] = @ProductId";
            try
            {
                var connectionString = DataSettingsManager.LoadSettings().ConnectionString;
                using (var connection = new SqlConnection(connectionString))
                {
                    await connection.OpenAsync();

                    using (var cmd = new SqlCommand(sql, connection))
                    {
                        cmd.CommandType = CommandType.Text;
                        cmd.Parameters.AddWithValue("@ErpAccountId", model.ErpAccountId);
                        cmd.Parameters.AddWithValue("@ErpSalesOrgId", model.ErpSalesOrgId);
                        cmd.Parameters.AddWithValue("@ProductId", model.ProductId);

                        using (var reader = await cmd.ExecuteReaderAsync())
                        {
                            if (await reader.ReadAsync())
                            {
                                return new SpecialIncludesAndExcludes
                                {
                                    Id = (int)reader["Id"],
                                    ErpAccountId = (int)reader["ErpAccountId"],
                                    ErpSalesOrgId = (int)reader["ErpSalesOrgId"],
                                    ProductId = (int)reader["ProductId"],
                                    SpecialTypeId = (int)reader["SpecialTypeId"],
                                    IsActive = (bool)reader["IsActive"],
                                    LastUpdate = Convert.ToDateTime(reader["LastUpdate"])
                                };
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Error($"Error reading special include-exclude records by SpecialIncludeExcludeModel: {ex.Message}. {ex.StackTrace}");
            }
            return null;
        }

    }
}
