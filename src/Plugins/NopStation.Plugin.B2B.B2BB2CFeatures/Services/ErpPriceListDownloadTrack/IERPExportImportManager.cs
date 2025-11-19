using System.Data;
using System.IO;
using System.Threading.Tasks;

namespace NopStation.Plugin.B2B.B2BB2CFeatures.Services.ErpPriceListDownloadTrack
{
    public interface IERPExportImportManager
    {
        Task<byte[]> GetExcelFileByQueryAsync(string query, object parameters);
        Task<DataTable> GetXLWorkbookByQuery(string query, object parameters);
        int WriteStreamInDatabase(Stream stream, string destinationTableName, bool hasHeader = true);
    }
}
