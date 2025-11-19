using Nop.Core.Domain.Catalog;

namespace Nop.Plugin.Comalytics.PictureAndSEOExportImport.Services
{
    public interface IPictureAndSEOExportImportService
    {
        Task<byte[]> ExportToExcelAsync(IEnumerable<Product> products);
        Task<int> ImportExcelAsync(Stream stream);
    }
}