using System.Data;
using System.IO;
using System.Threading.Tasks;

namespace NNopStation.Plugin.B2B.B2BB2CFeatures.Services.SpecialIncludeExcludeService
{
    public interface IB2BExportImportManager
    {
        Task<DataTable> GetXLWorkbookByQuery(string query, object parameters = null);
        int WriteStreamInDatabase(Stream stream, string destinationTableName, bool hasHeader = true);
    }
}
