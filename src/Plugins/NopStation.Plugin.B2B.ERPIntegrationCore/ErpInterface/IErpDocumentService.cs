using System.Threading.Tasks;

namespace NopStation.Plugin.B2B.ERPIntegrationCore.ErpInterface;

public interface IErpDocumentService
{
    Task<byte[]> GetDocumentAsync(string documentNumber);
    Task<byte[]> GetDocumentForQuoteAsync(string documentNumber);
    Task<byte[]> GetDocumentForOrderAsync(string documentNumber);
}
