using NopStation.Plugin.B2B.ERPIntegrationCore.Enums;
using NopStation.Plugin.B2B.ERPIntegrationCore.Model;
using NopStation.Plugin.B2B.ERPIntegrationCore.Model.Common;
using NopStation.Plugin.B2B.ERPIntegrationCore.Services;
using SAP.Middleware.Connector;

namespace NopStation.Plugin.Misc.B2B.SapIntegration.Services;

public class B2BDocumentService : IB2BDocumentService
{
    #region Fields

    private readonly IErpLogsService _erpLogsService;
    private readonly SapIntegrationSettings _sapIntegrationSettings;

    #endregion

    #region Ctor

    public B2BDocumentService(IErpLogsService erpLogsService, 
        SapIntegrationSettings sapIntegrationSettings)
    {
        _erpLogsService = erpLogsService;
        _sapIntegrationSettings = sapIntegrationSettings;
    }

    #endregion

    #region Methods

    public async Task<byte[]> GetDocumentAsync(string documentNumber)
    {
        try
        {
            if (!RfcDestinationManager.IsDestinationConfigurationRegistered())
                RfcDestinationManager.RegisterDestinationConfiguration(new SapConfig(_sapIntegrationSettings));

            var dest = RfcDestinationManager.GetDestination("HUBCLIENT");
            var repo = dest.Repository;
            var func = repo.CreateFunction("ZEC_GET_INVOICE");

            func.SetValue("BILLINGDOCUMENT", documentNumber);

            await _erpLogsService.InsertErpLogAsync(
                ErpLogLevel.Debug,
                ErpSyncLevel.Invoice,
                $"GetDocument before BAPI call (Document: {documentNumber}). Click view to see details.",
                $"GetDocument before BAPI call: {func}"
            );

            func.Invoke(dest);

            await _erpLogsService.InsertErpLogAsync(
                ErpLogLevel.Debug,
                ErpSyncLevel.Invoice,
                $"GetDocument after BAPI call (Document: {documentNumber}). Click view to see details.",
                $"GetDocument after BAPI call: {func}"
            );

            var pdf = func.GetTable("INV_PDF_TAB");
            var documentBytes = pdf.SelectMany(img => img["LINE"].GetByteArray()).ToArray();

            return documentBytes;
        }
        catch (Exception ex)
        {
            await _erpLogsService.InsertErpLogAsync(
                ErpLogLevel.Error,
                ErpSyncLevel.Invoice,
                $"GetDocument error (Document: {documentNumber}). Click view to see details.",
                $"GetDocument error: \nException message: {ex.Message}\nStack trace: {ex.StackTrace}"
            );
            return Array.Empty<byte>();
        }
    }

    public async Task<ErpResponseData<IList<ErpProductImageDataModel>>> GetSpecSheetAsync(ErpGetRequestModel erpRequest)
    {
        var erpResponseData = new ErpResponseData<IList<ErpProductImageDataModel>>
        {
            Data = new List<ErpProductImageDataModel>()
        };

        var responseContent = string.Empty;

        var eV_MAT_LAST = "";

        try
        {
            if (erpRequest == null)
            {
                erpResponseData.ErpResponseModel.IsError = false;
                erpResponseData.ErpResponseModel.ErrorShortMessage = "Request body content no data";
                return erpResponseData;
            }

            #region SAP

            try
            {
                if (!RfcDestinationManager.IsDestinationConfigurationRegistered())
                    RfcDestinationManager.RegisterDestinationConfiguration(new SapConfig(_sapIntegrationSettings));

                var dest = RfcDestinationManager.GetDestination("HUBCLIENT");
                var repo = dest.Repository;
                var func = repo.CreateFunction("ZEC_MAT_IMAGE_GETLIST");

                if (erpRequest.DateFrom != null && erpRequest.DateFrom != DateTime.MinValue)
                {
                    func.SetValue("IV_CHANGED", erpRequest.DateFrom.Value);
                }
                func.SetValue("IV_MAT_START", erpRequest.Start);
                func.SetValue("IV_ROWS", erpRequest.Limit);

                await _erpLogsService.InsertErpLogAsync(
                    ErpLogLevel.Debug,
                    ErpSyncLevel.SpecSheet,
                    $"GetProductImage before BAPI call (Start: {erpRequest.Start}, Limit = {erpRequest.Limit}, Date from = {erpRequest.DateFrom}). Click view to see details.",
                    $"GetProductImage before BAPI call: {func}"
                );

                func.Invoke(dest);

                await _erpLogsService.InsertErpLogAsync(
                    ErpLogLevel.Debug,
                    ErpSyncLevel.SpecSheet,
                    $"GetProductImage after BAPI call (Start: {erpRequest.Start}, Limit = {erpRequest.Limit}, Date from = {erpRequest.DateFrom}). Click view to see details.",
                    $"GetProductImage after BAPI call: {func}"
                );

                eV_MAT_LAST = func.GetString("EV_MAT_LAST");

                var products = func.GetTable("ES_MATERIALS");

                if (products != null && products.Count > 0)
                {
                    foreach (var product in products)
                    {
                        var specTab = product["MAT_SPEC_TAB"]?.GetTable();
                        var specData = specTab?.SelectMany(spec => spec["LINE"]?.GetByteArray()).ToArray();

                        erpResponseData.Data.Add(new ErpProductImageDataModel
                        {
                            Sku = product["MATNR"]?.GetString(),
                            SpecData = specData
                        });
                    }

                    erpResponseData.ErpResponseModel = new ErpResponseModel
                    {
                        Next = string.IsNullOrEmpty(eV_MAT_LAST) ? null : eV_MAT_LAST
                    };
                }
                else
                {
                    erpResponseData.Data = null;
                    erpResponseData.ErpResponseModel = new ErpResponseModel
                    {
                        Next = null,
                    };
                    erpResponseData.ErpResponseModel.IsError = true;
                    erpResponseData.ErpResponseModel.ErrorShortMessage = "No image data";
                }
            }
            catch (Exception ex)
            {
                await _erpLogsService.InsertErpLogAsync(
                    ErpLogLevel.Error,
                    ErpSyncLevel.SpecSheet,
                    $"GetProductImage error (Start: {erpRequest.Start}, Limit = {erpRequest.Limit}, Date from = {erpRequest.DateFrom}). Click view to see details.",
                    $"GetProductImage error: \nException message: {ex.Message}\nStack trace: {ex.StackTrace}"
                );
                erpResponseData.ErpResponseModel.IsError = true;
                erpResponseData.ErpResponseModel.ErrorShortMessage = ex.Message;
            }
            return erpResponseData;

            #endregion

        }
        catch (Exception ex)
        {
            erpResponseData.ErpResponseModel.IsError = true;
            erpResponseData.ErpResponseModel.ErrorShortMessage = ex.Message;
            erpResponseData.ErpResponseModel.ErrorFullMessage = !string.IsNullOrEmpty(responseContent) ? responseContent : ex.StackTrace;
        }

        return erpResponseData;
    }

    public async Task<byte[]> GetDocumentForQuoteAsync(string documentNumber)
    {
        try
        {
            if (!RfcDestinationManager.IsDestinationConfigurationRegistered())
                RfcDestinationManager.RegisterDestinationConfiguration(new SapConfig(_sapIntegrationSettings));

            var dest = RfcDestinationManager.GetDestination("HUBCLIENT");
            var repo = dest.Repository;
            var func = repo.CreateFunction("ZEC_QUOTE_CONFIRMATION_PDF");

            func.SetValue("QUOTATIONNUMBER", documentNumber);

            await _erpLogsService.InsertErpLogAsync(
                ErpLogLevel.Debug,
                ErpSyncLevel.Invoice,
                $"GetDocumentForQuote before BAPI call (Document = {documentNumber}). Click view to see details.",
                $"GetDocumentForQuote before BAPI call: {func}"
            );

            func.Invoke(dest);

            await _erpLogsService.InsertErpLogAsync(
                ErpLogLevel.Debug,
                ErpSyncLevel.Invoice,
                $"GetDocumentForQuote after BAPI call (Document = {documentNumber}). Click view to see details.",
                $"GetDocumentForQuote after BAPI call: {func}"
            );

            var pdf = func.GetTable("VT_PDF");
            var documentBytes = pdf.SelectMany((img) => img["LINE"].GetByteArray()).ToArray();
            return documentBytes;
        }
        catch (Exception ex)
        {
            await _erpLogsService.InsertErpLogAsync(
                ErpLogLevel.Error,
                ErpSyncLevel.Invoice,
                $"GetDocumentForQuote error (Document = {documentNumber}). Click view to see details.",
                $"GetDocumentForQuote error: \nException message: {ex.Message}\nStack trace: {ex.StackTrace}"
            );
            return Array.Empty<byte>();
        }
    }

    public async Task<byte[]> GetDocumentForOrderAsync(string documentNumber)
    {
        try
        {
            if (!RfcDestinationManager.IsDestinationConfigurationRegistered())
                RfcDestinationManager.RegisterDestinationConfiguration(new SapConfig(_sapIntegrationSettings));

            var dest = RfcDestinationManager.GetDestination("HUBCLIENT");
            var repo = dest.Repository;
            var func = repo.CreateFunction("ZEC_ORDER_CONFIRMATION_PDF");
            func.SetValue("ORDERNUMBER", documentNumber);

            await _erpLogsService.InsertErpLogAsync(
                ErpLogLevel.Debug,
                ErpSyncLevel.Invoice,
                $"GetDocumentForOrder before BAPI call (Document = {documentNumber}). Click view to see details.",
                $"GetDocumentForOrder before BAPI call: {func}"
            );

            func.Invoke(dest);

            await _erpLogsService.InsertErpLogAsync(
                ErpLogLevel.Debug,
                ErpSyncLevel.Invoice,
                $"GetDocumentForOrder after BAPI call (Document = {documentNumber}). Click view to see details.",
                $"GetDocumentForOrder after BAPI call: {func}"
            );

            var pdf = func.GetTable("VT_PDF");

            var documentBytes = pdf.SelectMany((img) => img["LINE"].GetByteArray()).ToArray();

            return documentBytes;
        }
        catch (Exception ex)
        {
            await _erpLogsService.InsertErpLogAsync(
                ErpLogLevel.Error,
                ErpSyncLevel.Invoice,
                $"GetDocumentForOrder error (Document = {documentNumber}). Click view to see details.",
                $"GetDocumentForOrder error: \nException message: {ex.Message}\nStack trace: {ex.StackTrace}"
            );
            return Array.Empty<byte>();
        }
    }

    public async Task<IList<DownloadPodDocumentResponseModel>> GetPODDocumentListAsync(string documentNumber)
    {
        try
        {
            if (!RfcDestinationManager.IsDestinationConfigurationRegistered())
                RfcDestinationManager.RegisterDestinationConfiguration(new SapConfig(_sapIntegrationSettings));

            var dest = RfcDestinationManager.GetDestination("HUBCLIENT");
            var repo = dest.Repository;
            var func = repo.CreateFunction("ZEC_GET_DELIVERY_PDF");

            func.SetValue("IV_VBELN", documentNumber);

            await _erpLogsService.InsertErpLogAsync(
                ErpLogLevel.Debug,
                ErpSyncLevel.Invoice,
                $"GetPODDocument before BAPI call (Document = {documentNumber}). Click view to see details.",
                $"GetPODDocument before BAPI call: {func}"
            );

            func.Invoke(dest);

            await _erpLogsService.InsertErpLogAsync(
                ErpLogLevel.Debug,
                ErpSyncLevel.Invoice,
                $"GetPODDocument after BAPI call (Document = {documentNumber}). Click view to see details.",
                $"GetPODDocument after BAPI call: {func}"
            );

            var pdfs = func.GetTable("VT_PDF_LIST");

            if (pdfs.Count > 0)
            {
                var result = pdfs.Select(pdf => new DownloadPodDocumentResponseModel
                {
                    DocumentNumber = pdf["VBELN"].GetString(),
                    ImageBase64 = Convert.ToBase64String(
                        pdf["DELIVERY_PDF"]
                            .GetTable()
                            .SelectMany(img => img["LINE"].GetByteArray())
                            .ToArray()
                    )
                }).ToList();

                return result;
            }

            return new List<DownloadPodDocumentResponseModel>();
        }
        catch (Exception ex)
        {
            await _erpLogsService.InsertErpLogAsync(
                ErpLogLevel.Error,
                ErpSyncLevel.Invoice,
                $"GetPODDocument error (Document = {documentNumber}). Click view to see details.",
                $"GetPODDocument error: \nException message: {ex.Message}\nStack trace: {ex.StackTrace}"
            );
            return new List<DownloadPodDocumentResponseModel>();
        }
    }

    public async Task<IList<DownloadPodDocumentResponseModel>> GetTheTestCertificateListAsync(string documentNumber)
    {
        try
        {
            if (!RfcDestinationManager.IsDestinationConfigurationRegistered())
                RfcDestinationManager.RegisterDestinationConfiguration(new SapConfig(_sapIntegrationSettings));

            var dest = RfcDestinationManager.GetDestination("HUBCLIENT");
            var repo = dest.Repository;
            var func = repo.CreateFunction("ZEC_TEST_CERT");
            
            func.SetValue("IV_VBELN", documentNumber);

            await _erpLogsService.InsertErpLogAsync(
                ErpLogLevel.Debug,
                ErpSyncLevel.Invoice,
                $"GetTestCertificate before BAPI call (Document = {documentNumber})",
                $"GetTestCertificate before BAPI call : {func}"
            );

            func.Invoke(dest);

            await _erpLogsService.InsertErpLogAsync(
                ErpLogLevel.Debug,
                ErpSyncLevel.Invoice,
                $"GetTestCertificate after BAPI call (Document = {documentNumber})",
                $"GetTestCertificate after BAPI call : {func}"
            );

            var pdfs = func.GetTable("VT_PDF_LIST");

            if (pdfs.Count > 0)
            {
                var result = pdfs.Select(pdf => new DownloadPodDocumentResponseModel
                {
                    DocumentNumber = pdf["VBELN"].GetString(),
                    ImageBase64 = Convert.ToBase64String(
                        pdf["TEST_CERT_PDF"]
                            .GetTable()
                            .SelectMany(pf => pf["LINE"].GetByteArray())
                            .ToArray()
                    )
                }).ToList();

                return result;
            }

            return new List<DownloadPodDocumentResponseModel>();
        }
        catch (Exception ex)
        {
            await _erpLogsService.InsertErpLogAsync(
                ErpLogLevel.Error,
                ErpSyncLevel.Invoice,
                $"GetTestCertificate error (Document = {documentNumber})",
                $"GetTestCertificate error: \nException message: {ex.Message}\nStack trace: {ex.StackTrace}"
            );
            return new List<DownloadPodDocumentResponseModel>();
        }
    }

    #endregion
}