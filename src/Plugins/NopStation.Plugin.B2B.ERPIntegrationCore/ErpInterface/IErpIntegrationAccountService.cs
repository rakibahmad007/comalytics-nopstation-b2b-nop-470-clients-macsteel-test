using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NopStation.Plugin.B2B.ERPIntegrationCore.Domain;
using NopStation.Plugin.B2B.ERPIntegrationCore.Model;

namespace NopStation.Plugin.B2B.ERPIntegrationCore.ErpInterface;

public interface IErpIntegrationAccountService
{
    Task<ErpResponseModel> CreateAccountNoErpAsync(ErpCreateAccountModel erpCreateAccountModel);

    Task<ErpResponseData<ErpAccountDataModel>> GetAccountFromErpAsync(
        ErpGetRequestModel erpRequest
    );

    Task<ErpResponseData<IList<ErpAccountDataModel>>> GetAccountsFromErpAsync(
        ErpGetRequestModel erpRequest
    );

    Task<ErpResponseData<IList<ErpInvoiceDataModel>>> GetInvoiceByAccountNoFromErpAsync(
        ErpGetRequestModel erpRequest
    );

    Task<ErpResponseData<string>> GetInvoicePdfByteCodeByDocumentNoFromErpAsync(
        ErpGetRequestModel erpRequest
    );

    Task<
        ErpResponseData<IList<ErpShipToAddressDataModel>>
    > GetShipToAddressByAccountNumberFromErpAsync(ErpGetRequestModel erpRequest);

    Task<ErpResponseData<IList<ErpAccountDataModel>>> GetAllAccountCreditFromErpAsync(
        ErpGetRequestModel erpRequest
    );

    Task<IList<string>> GetTheProofOfDeliveryPDFDocumentListAsync(string documentNo);

    Task<DownloadPodDocumentResponseModel> DownloadTheProofOfDeliveryPDFAsync(string documentNumber, string podDocumentNumber);

    Task<IList<string>> GetTheTestCertificatePDFDocumentListAsync(string documentNumber);
    Task<DownloadPodDocumentResponseModel> DownloadTheTestCertificatePDFAsync(string documentNumber, string testCertDocumentNumber);
    Task<byte[]> DownloadAccountStatementPDFAsync(ErpAccount erpAccount, DateTime dateFrom, DateTime dateTo);

    Task<ErpResponseData<string>> GetStatementPdfByteCodeFromErpAsync(ErpGetRequestModel erpRequest);

    Task<ErpResponseData<IList<GetAccountStatementPDFDocumentListModel>>> GetAccountStatementPDFDocumentListAsync(ErpAccount erpAccount, bool loadListOnly = true);

    Task<decimal?> GetAccountSavingsForTimePeriodAsync(ErpGetRequestModel erpRequest);
}
