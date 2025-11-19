using NopStation.Plugin.B2B.ERPIntegrationCore.Model;
using NopStation.Plugin.B2B.ERPIntegrationCore.Model.Common;

namespace NopStation.Plugin.Misc.B2B.SapIntegration.Services;
public interface IB2BDocumentService
{
    Task<byte[]> GetDocumentAsync(string documentNumber);
    Task<byte[]> GetDocumentForQuoteAsync(string documentNumber);
    Task<byte[]> GetDocumentForOrderAsync(string documentNumber);
    Task<IList<DownloadPodDocumentResponseModel>> GetPODDocumentListAsync(string documentNumber);
    Task<IList<DownloadPodDocumentResponseModel>> GetTheTestCertificateListAsync(string documentNumber);
    Task<ErpResponseData<IList<ErpProductImageDataModel>>> GetSpecSheetAsync(ErpGetRequestModel erpRequest);
}