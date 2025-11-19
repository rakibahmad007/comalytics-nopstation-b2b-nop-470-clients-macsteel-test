using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using ClosedXML.Excel;
using Microsoft.Data.SqlClient;
using Nop.Core;
using Nop.Data;
using Nop.Plugin.Comalytics.DomainFilter.Domains;
using Nop.Services.Localization;

namespace Nop.Plugin.Comalytics.DomainFilter.Services
{
    public class DomainFilterExportImportService : IDomainFilterExportImportService
    {
        private readonly ILocalizationService _localizationService;

        public DomainFilterExportImportService(ILocalizationService localizationService)
        {
            _localizationService = localizationService;
        }

        #region Utilities

        // convert "Domain Or Email Name" to "DomainOrEmailName"
        private string GetProcessedNameWithOutSpace(string str)
        {
            var regex = new Regex(@"[\s]+([a-z0-9])", RegexOptions.IgnoreCase);

            str = regex.Replace(str, m => m.ToString().Trim().ToUpper());

            return str;
        }

        // convert "DomainOrEmailName" to "Domain Or Email Name"
        private string GetProcessedNameWithSpace(string str)
        {
            var r = new Regex(@"
                (?<=[A-Z])(?=[A-Z][a-z]) |
                 (?<=[^A-Z])(?=[A-Z]) |
                 (?<=[A-Za-z])(?=[^A-Za-z])", RegexOptions.IgnorePatternWhitespace);

            string value = r.Replace(str, " ");
            value = value.Substring(0, 1) + value.Substring(1).ToLower();

            return value;
        }

        private async Task<byte[]> ExportToXlsxAsync(DataTable dataTable, string worksheetName)
        {
            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add(worksheetName);

            // Add column headers
            int col = 1;
            var columns = new List<KeyValuePair<string, Type>>();
            foreach (DataColumn column in dataTable.Columns)
            {
                var columnName = column.ColumnName == "TypeId" ? "Type" : column.ColumnName;
                var dataType = column.ColumnName == "TypeId" ? typeof(string) : column.DataType;

                columns.Add(new KeyValuePair<string, Type>(columnName, dataType));

                var cell = worksheet.Cell(1, col);
                cell.Value = GetProcessedNameWithSpace(columnName);
                cell.Style.Font.Bold = true;
                cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                col++;
            }

            if (dataTable.Rows.Count > 0)
            {
                int row = 2; // Start from the second row for data
                foreach (DataRow currentDTRow in dataTable.Rows)
                {
                    int orderRowColumn = 1;
                    foreach (var column in columns)
                    {
                        var cell = worksheet.Cell(row, orderRowColumn);

                        if (column.Key == "Type")
                        {
                            cell.Value = await _localizationService.GetLocalizedEnumAsync((DomainType)currentDTRow["TypeId"]);
                        }
                        else
                        {
                            var value = currentDTRow[column.Key];
                            cell.Value = value is DBNull ? null : Convert.ToString(value); // Explicit conversion
                        }

                        orderRowColumn++;
                    }
                    row++;
                }

            }

            // Adjust columns to fit content
            worksheet.Columns().AdjustToContents();

            // Save the workbook to a memory stream
            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            return stream.ToArray();
        }

        private async Task<DataTable> GetDatatableFromXlsxAsync(Stream stream, bool hasHeader = true)
        {
            using (var workbook = new XLWorkbook(stream))
            {
                // Get the first worksheet
                var worksheet = workbook.Worksheets.FirstOrDefault();
                if (worksheet == null)
                    throw new NopException("No worksheet found");

                var dataTable = new DataTable();

                // Add columns
                var firstRow = worksheet.FirstRowUsed();
                if (firstRow == null)
                    throw new NopException("Worksheet has no data");

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
                    for (int i = 0; i < dataTable.Columns.Count; i++)
                    {
                        var cell = row.Cell(i + 1);
                        dataRow[i] = cell.IsEmpty() ? DBNull.Value : cell.Value;
                    }
                    dataTable.Rows.Add(dataRow);
                }

                return await Task.FromResult(dataTable);
            }
        }

        public async Task<byte[]> GetExcelPackageByQueryAsync(string query, string worksheetName)
        {
            var connectionString = DataSettingsManager.LoadSettings().ConnectionString;
            var dataTable = new DataTable();

            using (var connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync();
                var cmd = new SqlCommand(query, connection);
                using (var adapter = new SqlDataAdapter(cmd))
                {
                    await Task.Run(() => adapter.Fill(dataTable));
                }
            }

            return await ExportToXlsxAsync(dataTable, worksheetName);
        }

        public async Task<int> WriteStreamInDatabaseAsync(Stream stream, string destinationTableName, bool hasHeader = true)
        {
            var connectionString = DataSettingsManager.LoadSettings().ConnectionString;
            var dataTable = await GetDatatableFromXlsxAsync(stream, hasHeader);

            using (var bulkCopy = new SqlBulkCopy(connectionString))
            {
                bulkCopy.DestinationTableName = destinationTableName;

                // Column mappings
                foreach (DataColumn column in dataTable.Columns)
                {
                    var destColumn = column.ColumnName == "Type" ? "TypeId" : column.ColumnName;
                    bulkCopy.ColumnMappings.Add(column.ColumnName, destColumn);
                }

                await bulkCopy.WriteToServerAsync(dataTable);
            }

            return dataTable?.Rows?.Count ?? 0;
        }
        #endregion

    }
}
