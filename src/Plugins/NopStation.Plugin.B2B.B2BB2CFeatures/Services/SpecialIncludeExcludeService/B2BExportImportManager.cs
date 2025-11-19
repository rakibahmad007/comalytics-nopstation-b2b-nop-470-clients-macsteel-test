using System;
using System.Data;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using ClosedXML.Excel;
using Microsoft.Data.SqlClient;
using NNopStation.Plugin.B2B.B2BB2CFeatures.Services.SpecialIncludeExcludeService;
using Nop.Core;
using Nop.Data;

namespace NopStation.Plugin.B2B.B2BB2CFeatures.Services.SpecialIncludeExcludeService
{
    public class B2BExportImportManager : IB2BExportImportManager
    {
        // this will convert "SalesOrganisationCode" to "Sales organisation code"
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

        // this will convert "Sales organisation code" to "SalesOrganisationCode"
        private string GetProcessedNameWithOutSpace(string str)
        {
            var regex = new Regex(@"[\s]+([a-z0-9])", RegexOptions.IgnoreCase);

            str = regex.Replace(str, m => m.ToString().Trim().ToUpper());

            return str;
        }

        public async Task<DataTable> GetXLWorkbookByQuery(string query, object parameters = null)
        {
            var connectionString = DataSettingsManager.LoadSettings().ConnectionString;
            var dataTable = new DataTable();

            using (var connection = new SqlConnection(connectionString))
            using (var command = new SqlCommand(query, connection))
            {
                command.CommandTimeout = 300;
                if (parameters != null)
                {
                    foreach (var property in parameters.GetType().GetProperties())
                    {
                        command.Parameters.AddWithValue($"@{property.Name}", property.GetValue(parameters) ?? DBNull.Value);
                    }
                }

                using (var adapter = new SqlDataAdapter(command))
                {
                    await connection.OpenAsync();
                    adapter.Fill(dataTable);
                }
            }

            return dataTable;
        }

        private XLWorkbook ExportToXlsx(DataTable dataTable, string worksheetName)
        {
            var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add(worksheetName);

            // Add header row
            int col = 1;
            foreach (DataColumn column in dataTable.Columns)
            {
                worksheet.Cell(1, col).Value = GetProcessedNameWithSpace(column.ColumnName);
                worksheet.Cell(1, col).Style.Font.Bold = true;
                worksheet.Cell(1, col).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                col++;
            }

            // Add data rows
            for (int rowIndex = 0; rowIndex < dataTable.Rows.Count; rowIndex++)
            {
                var row = dataTable.Rows[rowIndex];
                for (int columnIndex = 0; columnIndex < dataTable.Columns.Count; columnIndex++)
                {
                    worksheet.Cell(rowIndex + 2, columnIndex + 1).Value = row[columnIndex]?.ToString();
                }
            }

            return workbook;
        }

        public int WriteStreamInDatabase(Stream stream, string destinationTableName, bool hasHeader = true)
        {
            var connectionString = DataSettingsManager.LoadSettings().ConnectionString;

            var dataTable = GetDatatableFromXlsx(stream, hasHeader);

            using (SqlBulkCopy bulkCopy = new SqlBulkCopy(connectionString))
            {
                bulkCopy.DestinationTableName = destinationTableName;

                // Column mappings
                foreach (DataColumn column in dataTable.Columns)
                {
                    bulkCopy.ColumnMappings.Add(column.ColumnName, column.ColumnName);
                }

                bulkCopy.WriteToServer(dataTable);
            }

            return dataTable?.Rows?.Count ?? 0;
        }

        private DataTable GetDatatableFromXlsx(Stream stream, bool hasHeader = true)
        {
            using (var workbook = new XLWorkbook(stream))
            {
                var worksheet = workbook.Worksheets.FirstOrDefault();
                if (worksheet == null)
                    throw new NopException("No worksheet found");

                var dataTable = new DataTable();

                // Add columns
                var headerRow = worksheet.Row(1);
                foreach (var cell in headerRow.CellsUsed())
                {
                    dataTable.Columns.Add(hasHeader ? GetProcessedNameWithOutSpace(cell.Value.ToString()) : $"Column {cell.Address.ColumnNumber}");
                }

                // Add rows
                int startRow = hasHeader ? 2 : 1;
                for (int rowNum = startRow; rowNum <= worksheet.LastRowUsed().RowNumber(); rowNum++)
                {
                    var row = worksheet.Row(rowNum);
                    var dataRow = dataTable.NewRow();

                    for (int colNum = 1; colNum <= dataTable.Columns.Count; colNum++)
                    {
                        dataRow[colNum - 1] = row.Cell(colNum).Value;
                    }

                    dataTable.Rows.Add(dataRow);
                }

                return dataTable;
            }
        }

    }
}

