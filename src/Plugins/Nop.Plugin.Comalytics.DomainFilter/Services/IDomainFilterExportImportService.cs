using System.IO;
using System.Threading.Tasks;

namespace Nop.Plugin.Comalytics.DomainFilter
{
    public interface IDomainFilterExportImportService
    {
        Task<byte[]> GetExcelPackageByQueryAsync(string query, string worksheetName);

        Task<int> WriteStreamInDatabaseAsync(Stream stream, string destinationTableName, bool hasHeader = true);
    }
}
