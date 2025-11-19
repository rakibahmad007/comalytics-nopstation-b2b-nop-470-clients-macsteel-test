using System.Text;
using Newtonsoft.Json.Linq;
using Nop.Core.Infrastructure;
using NopStation.Plugin.B2B.ERPIntegrationCore.Domain;
using NopStation.Plugin.B2B.ERPIntegrationCore.Enums;
using NopStation.Plugin.B2B.ERPIntegrationCore.Model;
using NopStation.Plugin.B2B.ERPIntegrationCore.Services;
using NopStation.Plugin.Misc.B2B.SapIntegration.Models;
using SAP.Middleware.Connector;

namespace NopStation.Plugin.Misc.B2B.SapIntegration.Services;

public class B2BInvoiceService : IB2BInvoiceService
{
    private readonly IErpLogsService _erpLogsService;
    private readonly SapIntegrationSettings _sapIntegrationSettings;
    private readonly INopFileProvider _fileProvider;

    public B2BInvoiceService(IErpLogsService erpLogsService, 
        SapIntegrationSettings sapIntegrationSettings,
        INopFileProvider fileProvider)
    {
        _erpLogsService = erpLogsService;
        _sapIntegrationSettings = sapIntegrationSettings;
        _fileProvider = fileProvider;
    }

    private async Task<Dictionary<string, DocumentTypeMapping>> LoadDocumentTypeMappings()
    {
        var mappings = new Dictionary<string, DocumentTypeMapping>();

        var filePath = _fileProvider.MapPath("~/Plugins/Misc.B2B.SapIntegration/mappings.json");
        if (!_fileProvider.FileExists(filePath))
            return mappings;

        var text = _fileProvider.ReadAllText(filePath, Encoding.UTF8);
        if (string.IsNullOrEmpty(text))
            return mappings;

        var jsonObject = JObject.Parse(text);
        if (jsonObject.TryGetValue("documentTypes", out var documentTypesToken) && documentTypesToken is JObject documentTypes)
        {
            foreach (var property in documentTypes.Properties())
            {
                var docMapping = property.Value.ToObject<DocumentTypeMapping>();
                if (docMapping?.DocumentTypeId != null)
                {
                    mappings[property.Name] = new DocumentTypeMapping
                    {
                        DocumentTypeId = docMapping.DocumentTypeId.Value,
                        DisplayName = docMapping.DisplayName
                    };
                }
            }
        }        

        return mappings;
    }

    public async Task<ErpResponseData<IList<ErpInvoiceDataModel>>> GetInvoiceByAccountNoFromErpAsync(ErpGetRequestModel erpRequest)
    {
        var erpResponseData = new ErpResponseData<IList<ErpInvoiceDataModel>>
        {
            Data = new List<ErpInvoiceDataModel>(),
        };
        var responseContent = string.Empty;

        try
        {
            if (erpRequest == null)
            {
                erpResponseData.ErpResponseModel.IsError = true;
                erpResponseData.ErpResponseModel.ErrorShortMessage =
                    "Request body content is empty";
                return erpResponseData;
            }

            #region SAP

            if (erpRequest.Limit == 0)
                erpRequest.Limit = 1000;

            if (erpRequest.Start == "0")
                erpRequest.Start = string.Empty;

            var dateFrom =
                erpRequest.DateFrom == null ? DateTime.MinValue : erpRequest.DateFrom.Value;
            var dateTo =
                erpRequest.DateTo == null || erpRequest.DateTo == DateTime.MinValue
                    ? DateTime.Now
                    : erpRequest.DateTo.Value;

            try
            {
                if (!RfcDestinationManager.IsDestinationConfigurationRegistered())
                    RfcDestinationManager.RegisterDestinationConfiguration(new SapConfig(_sapIntegrationSettings));

                var dest = RfcDestinationManager.GetDestination("HUBCLIENT");
                var repo = dest.Repository;
                var func = repo.CreateFunction("BAPI_AR_ACC_GETSTATEMENT");

                func.SetValue("COMPANYCODE", _sapIntegrationSettings.SAPCompanyCode ?? "");
                func.SetValue("CUSTOMER", erpRequest.AccountNumber);
                func.SetValue("DATE_FROM", dateFrom.ToString("yyyy-MM-dd"));
                func.SetValue("DATE_TO", dateTo.ToString("yyyy-MM-dd"));

                await _erpLogsService.InsertErpLogAsync(
                    ErpLogLevel.Debug,
                    ErpSyncLevel.Invoice,
                    $"GetInvoice before BAPI call (Account: {erpRequest.AccountNumber}). Click view to see details.",
                    $"GetInvoice before BAPI call : {func}"
                );

                func.Invoke(dest);

                var transactions = func.GetTable("LINEITEMS");

                await _erpLogsService.InsertErpLogAsync(
                    ErpLogLevel.Debug,
                    ErpSyncLevel.Invoice,
                    $"GetInvoice after BAPI call (Account = {erpRequest.AccountNumber}, " +
                    $"Location = {erpRequest.Location}, " +
                    $"Start = {erpRequest.Start}, " +
                    $"Limit = {erpRequest.Limit}, " +
                    $"DateFrom = {erpRequest.DateFrom?.ToString("yyyy-MM-dd HH:mm:ss")}" +
                    (transactions != null && transactions.Any() ? 
                        $", Next = {(transactions.Count < erpRequest.Limit ? "" : 
                        transactions.Last()["DOC_NO"].GetString())}" : "") +
                    $"). Click view to see details.",
                    $"GetInvoice after BAPI call : {func}"
                );

                var documentTypeMappings = await LoadDocumentTypeMappings();

                foreach (var transaction in transactions)
                {
                    documentTypeMappings.TryGetValue(transaction["DOC_TYPE"].GetString(), out var docTypes);

                    var invoice = new ErpInvoiceDataModel
                    {
                        PostingDateUtc = DateTime.Parse(transaction["PSTNG_DATE"].GetString()),
                        ErpDocumentNumber = transaction["DOC_NO"].GetString(),
                        Description = transaction["REF_DOC_NO"].GetString(),
                        AmountInclVat = transaction["LC_AMOUNT"].GetDecimal() * (transaction["DB_CR_IND"].GetString() == "H" ? -1 : 1),
                        AmountExclVat = transaction["LC_AMOUNT"].GetDecimal(),
                        ErpAccountNumber = transaction["CUSTOMER"].GetString(),
                        CurrencyCode = transaction["CURRENCY"].GetString(),
                        DocumentTypeId = docTypes?.DocumentTypeId ?? 0,
                        DocumentDisplayName = docTypes?.DisplayName ?? string.Empty,
                        ErpOrderNumber = transaction["ALLOC_NMBR"].GetString(),
                        Items = new List<ErpOrderItemAdditionalData>
                        {
                            new ()
                            {
                                ErpOrderLineNumber = transaction["ALLOC_NMBR"].GetString(),
                            },
                        },
                        DueDateUtc = DateTime.Parse(transaction["BLINE_DATE"].GetString()),
                        DocumentDateUtc = DateTime.Parse(transaction["DOC_DATE"].GetString()),
                        RelatedDocumentNo = transaction["REF_KEY_2"].GetString(),
                    };

                    erpResponseData.Data.Add(invoice);
                }

                if (!erpResponseData.Data.Any())
                    erpResponseData.Data = null;

                erpResponseData.ErpResponseModel.Next =
                    transactions.Count < erpRequest.Limit
                        ? null
                        : transactions.Last()["DOC_NO"].GetString();
            }
            catch (Exception ex)
            {
                await _erpLogsService.InsertErpLogAsync(
                    ErpLogLevel.Error,
                    ErpSyncLevel.Invoice,
                    $"GetInvoice error (Account: {erpRequest.AccountNumber}). Click view to see details.",
                    $"GetInvoice error: \nException message: {ex.Message}\nStack trace: {ex.StackTrace}"
                );
                erpResponseData.ErpResponseModel.IsError = true;
                erpResponseData.ErpResponseModel.ErrorShortMessage = ex.Message;
                return erpResponseData;
            }

            #endregion
        }
        catch (Exception ex)
        {
            await _erpLogsService.InsertErpLogAsync(
                ErpLogLevel.Error,
                ErpSyncLevel.Invoice,
                $"GetInvoice error (Account: {erpRequest.AccountNumber}). Click view to see details.",
                $"GetInvoice error: \nException message: {ex.Message}\nStack trace: {ex.StackTrace}"
            );
            erpResponseData.ErpResponseModel.IsError = true;
            erpResponseData.ErpResponseModel.ErrorShortMessage = ex.Message;
            erpResponseData.ErpResponseModel.ErrorFullMessage = !string.IsNullOrEmpty(
                responseContent
            )
                ? responseContent
                : ex.StackTrace;
        }

        return erpResponseData;
    }

    public async Task<ErpResponseData<string>> GetInvoicePdfByteCodeByDocumentNoFromErpAsync(ErpGetRequestModel erpRequest)
    {
        throw new NotImplementedException();
    }

    public async Task<ErpResponseData<string>> GetStatementPdfByteCodeFromErpAsync(ErpGetRequestModel erpRequest)
    {
        throw new NotImplementedException();
    }
}
