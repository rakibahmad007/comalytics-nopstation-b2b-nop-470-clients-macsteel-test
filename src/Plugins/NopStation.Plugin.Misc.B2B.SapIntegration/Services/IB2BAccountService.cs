using NopStation.Plugin.B2B.ERPIntegrationCore.Domain;
using NopStation.Plugin.B2B.ERPIntegrationCore.Model;

namespace NopStation.Plugin.Misc.B2B.SapIntegration.Services;

public interface IB2BAccountService
{
    Task<ErpResponseData<IList<ErpAccountDataModel>>> GetAccountsFromErpAsync(
        ErpGetRequestModel erpRequest
    );
    Task<ErpResponseData<IList<ErpAccountDataModel>>> GetAllAccountCreditFromErpAsync(
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
    Task<byte[]> GetAccountStatementPDFAsync(
        ErpAccount erpAccount,
        DateTime? dateFrom,
        DateTime? dateTo
    );

    Task<decimal?> GetAccountSavingsAsync(ErpGetRequestModel erpRequest);
}
