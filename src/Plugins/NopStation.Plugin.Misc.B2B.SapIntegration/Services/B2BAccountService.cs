using System.Text;
using NopStation.Plugin.B2B.ERPIntegrationCore.Domain;
using NopStation.Plugin.B2B.ERPIntegrationCore.Enums;
using NopStation.Plugin.B2B.ERPIntegrationCore.Model;
using NopStation.Plugin.B2B.ERPIntegrationCore.Services;
using SAP.Middleware.Connector;

namespace NopStation.Plugin.Misc.B2B.SapIntegration.Services;

public class B2BAccountService : IB2BAccountService
{
    #region Fields

    private readonly IErpLogsService _erpLogsService;
    private readonly SapIntegrationSettings _sapIntegrationSettings;

    #endregion

    #region Ctor

    public B2BAccountService(IErpLogsService erpLogsService,
        SapIntegrationSettings sapIntegrationSettings)
    {
        _erpLogsService = erpLogsService;
        _sapIntegrationSettings = sapIntegrationSettings;
    }

    #endregion

    #region Utils

    private string GetDateTimeInStringYYYYMMDD(DateTime date, string separator = null)
    {
        if (string.IsNullOrEmpty(separator))
            return $"{date.Year}{date.Month:00}{date.Day:00}";
        else
            return $"{date.Year}{separator}{date.Month:00}{separator}{date.Day:00}";
    }

    #endregion

    #region Methods

    public async Task<ErpResponseData<IList<ErpAccountDataModel>>> GetAccountsFromErpAsync(ErpGetRequestModel erpRequest)
    {
        //account synced from webhook
        throw new NotImplementedException();
    }

    public async Task<ErpResponseData<IList<ErpInvoiceDataModel>>> GetInvoiceByAccountNoFromErpAsync(ErpGetRequestModel erpRequest)
    {
        throw new NotImplementedException();
    }

    public async Task<ErpResponseData<string>> GetInvoicePdfByteCodeByDocumentNoFromErpAsync(ErpGetRequestModel erpRequest)
    {
        throw new NotImplementedException();
    }

    public async Task<ErpResponseData<IList<ErpShipToAddressDataModel>>> GetShipToAddressByAccountNumberFromErpAsync(ErpGetRequestModel erpRequest)
    {
        throw new NotImplementedException();
    }

    public async Task<ErpResponseData<IList<ErpAccountDataModel>>> GetAllAccountCreditFromErpAsync(ErpGetRequestModel request)
    {
        var resp = new ErpResponseData<IList<ErpAccountDataModel>>
        {
            Data = new List<ErpAccountDataModel>()
        };

        _ = int.TryParse(request.Start, out var start);
        if (request.Limit == 0)
            request.Limit = 1000;
        try
        {
            var dest = RfcDestinationManager.GetDestination("HUBCLIENT");
            var repo = dest.Repository;
            var func = repo.CreateFunction("ZEC_CREDIT_EXPOSURE");

            if (!string.IsNullOrEmpty(request.AccountNumber))
            {
                var eT_KUNNR_SELT = func.GetTable("ET_KUNNR_SELT");
                eT_KUNNR_SELT.Append();
                eT_KUNNR_SELT.SetValue("SIGN", "I");
                eT_KUNNR_SELT.SetValue("OPTION", "EQ");
                eT_KUNNR_SELT.SetValue("LOW", request.AccountNumber);
            }

            await _erpLogsService.InsertErpLogAsync(
                ErpLogLevel.Debug,
                ErpSyncLevel.Account,
                $"Account Credit check - before BAPI call (Account = {request.AccountNumber}). Click view to see details.",
                $"Account Credit check - before BAPI call: {func}"
            );

            func.Invoke(dest);

            await _erpLogsService.InsertErpLogAsync(
                ErpLogLevel.Debug,
                ErpSyncLevel.Account,
                $"Account Credit check - after BAPI call (Account = {request.AccountNumber}). Click view to see details.",
                $"Account Credit check - after BAPI call: {func}"
            );

            var erpAccountDataModel = func.GetTable("ES_CREDIT").Select((customer) => new ErpAccountDataModel()
            {
                AccountNumber = customer["KUNNR"].GetString(),
                CurrentBalance = customer["SKFOR"].GetDecimal(),
                CreditLimit = customer["CREDIT_LIMIT"].GetDecimal(),
                CreditLimitUsed = customer["CREDIT_LIMIT_USED"].GetDecimal(),
                CreditLimitAvailable = customer["DELTA_TO_LIMIT"].GetDecimal(),
                LastPaymentDate = DateTime.TryParse(customer["CASHD"].GetString(), out var lastPaymentDate) ? lastPaymentDate : DateTime.MinValue,
                LastPaymentAmount = customer["CASHA"].GetDecimal(),
            }).FirstOrDefault();

            resp.Data.Add(erpAccountDataModel);
        }
        catch (Exception e)
        {
            resp.ErpResponseModel.IsError = true;
            resp.ErpResponseModel.ErrorShortMessage = e.Message;
            resp.ErpResponseModel.ErrorFullMessage = e.StackTrace;
        }

        return resp;
    }

    public async Task<byte[]> GetAccountStatementPDFAsync(
        ErpAccount erpAccount,
        DateTime? dateFrom,
        DateTime? dateTo
    )
    {
        try
        {
            if (!RfcDestinationManager.IsDestinationConfigurationRegistered())
                RfcDestinationManager.RegisterDestinationConfiguration(new SapConfig(_sapIntegrationSettings));

            var dest = RfcDestinationManager.GetDestination("HUBCLIENT");
            var repo = dest.Repository;
            var func = repo.CreateFunction("ZEC_CUSTOMER_STATEMENTS");

            var eT_BUKRS = func.GetTable("ET_BUKRS");
            eT_BUKRS.Append();
            eT_BUKRS.SetValue("SIGN", "I");
            eT_BUKRS.SetValue("OPTION", "EQ");
            eT_BUKRS.SetValue("LOW", "1000");

            var message = new StringBuilder();
            message.AppendLine($"GetAccountStatementPDF call: ET_BUKRS: {eT_BUKRS}; Count: {eT_BUKRS.Count}");

            if (!string.IsNullOrEmpty(erpAccount.AccountNumber))
            {
                message.AppendLine($"Setting Acc No.: {erpAccount.AccountNumber}");

                var eT_KONTO = func.GetTable("ET_KONTO");
                eT_KONTO.Append();
                eT_KONTO.SetValue("SIGN", "I");
                eT_KONTO.SetValue("OPTION", "EQ");
                eT_KONTO.SetValue("LOW", erpAccount.AccountNumber);

                message.AppendLine($"ET_KONTO: {eT_KONTO}; Count: {eT_KONTO.Count}");
            }

            var currentMonthFirstDate = new DateTime(
                DateTime.UtcNow.Year,
                DateTime.UtcNow.Month,
                1
            );
            var statementStartDate = dateFrom ?? currentMonthFirstDate.AddMonths(-1).Date;
            var statementEndDate = dateTo ?? currentMonthFirstDate.AddDays(-1).Date;

            var eT_BUDAT = func.GetTable("ET_BUDAT");
            eT_BUDAT.Append();
            eT_BUDAT.SetValue("SIGN", "I");
            eT_BUDAT.SetValue("OPTION", "EQ");
            eT_BUDAT.SetValue("LOW", GetDateTimeInStringYYYYMMDD(statementStartDate));
            eT_BUDAT.SetValue("HIGH", GetDateTimeInStringYYYYMMDD(statementEndDate));

            message.AppendLine(
                $"GetAccountStatement call to BAPI: {func}; " +
                $"ET_BUDAT: {eT_BUDAT}; " +
                $"Count: {eT_BUDAT.Count}; " +
                $"DateFrom: {statementStartDate:yyyy-MM-dd}; " +
                $"DateTo: {statementEndDate:yyyy-MM-dd}");

            message.AppendLine($"ET_BUDAT: {eT_BUDAT}; Count: {eT_BUDAT.Count}");

            await _erpLogsService.InsertErpLogAsync(
                ErpLogLevel.Debug,
                ErpSyncLevel.Invoice,
                $"GetAccountStatement before BAPI call (Account: {erpAccount.AccountNumber} (Id: {erpAccount.Id})). Click view to see details.",
                $"{message}\n\n GetAccountStatement before BAPI call: {func}"
            );
            
            func.Invoke(dest);

            await _erpLogsService.InsertErpLogAsync(
                ErpLogLevel.Debug,
                ErpSyncLevel.Invoice,
                $"GetAccountStatement after BAPI call (Account: {erpAccount.AccountNumber} (Id: {erpAccount.Id})). Click view to see details.",
                $"GetAccountStatement after BAPI call: {func}"
            );

            var statementTab = func.GetTable("ES_STATEMENT");

            var pdfRow = statementTab.FirstOrDefault();
            if (pdfRow != null)
            {
                var pdfBytes = pdfRow["STATEMENT_PDF"]
                    .GetTable()
                    .SelectMany(img => img["LINE"].GetByteArray())
                    .ToArray();

                return pdfBytes;
            }

            return Array.Empty<byte>();
        }
        catch (Exception ex)
        {
            await _erpLogsService.InsertErpLogAsync(
                ErpLogLevel.Error,
                ErpSyncLevel.Invoice,
                $"GetAccountStatementPDF error (Account: {erpAccount.AccountNumber} (Id: {erpAccount.Id})). Click view to see details.",
                $"GetAccountStatementPDF error: \nException message: {ex.Message}\nStack trace: {ex.StackTrace}"
            );
            return Array.Empty<byte>();
        }
    }

    public async Task<decimal?> GetAccountSavingsAsync(ErpGetRequestModel erpRequest)
    {
        try
        {
            if (!RfcDestinationManager.IsDestinationConfigurationRegistered())
                RfcDestinationManager.RegisterDestinationConfiguration(new SapConfig(_sapIntegrationSettings));

            var dest = RfcDestinationManager.GetDestination("HUBCLIENT");
            var repo = dest.Repository;
            var func = repo.CreateFunction("ZEC_SAVINGS");

            if (erpRequest.DateFrom.HasValue)
            {
                var date = GetDateTimeInStringYYYYMMDD(erpRequest.DateFrom.Value);
                func.SetValue("IV_PERIOD_START", date);
            }
            if (erpRequest.DateTo.HasValue)
            {
                var date = GetDateTimeInStringYYYYMMDD(erpRequest.DateTo.Value);
                func.SetValue("IV_PERIOD_END", date);
            }

            if (!string.IsNullOrEmpty(erpRequest.AccountNumber))
            {
                func.SetValue("IV_SPLIT_PER_EMAIL", "N");

                var eT_KUNNR_SELT = func.GetTable("ET_KUNNR_SELT");
                eT_KUNNR_SELT.Append();
                eT_KUNNR_SELT.SetValue("SIGN", "I");
                eT_KUNNR_SELT.SetValue("OPTION", "EQ");
                eT_KUNNR_SELT.SetValue("LOW", erpRequest.AccountNumber);
            }
            else if (!string.IsNullOrEmpty(erpRequest.CustomerEmail))
            {
                func.SetValue("IV_SPLIT_PER_EMAIL", "Y");

                var eT_EMAIL_SELT = func.GetTable("ET_EMAIL_SELT");
                eT_EMAIL_SELT.Append();
                eT_EMAIL_SELT.SetValue("SIGN", "I");
                eT_EMAIL_SELT.SetValue("OPTION", "EQ");
                eT_EMAIL_SELT.SetValue("LOW", erpRequest.CustomerEmail);
            }

            await _erpLogsService.InsertErpLogAsync(
                ErpLogLevel.Debug,
                ErpSyncLevel.Account,
                $"GetAccountSavings - before BAPI call (Account: {erpRequest.AccountNumber ?? erpRequest.CustomerEmail}). Click view to see details.",
                $"GetAccountSavings - before BAPI call (Account: {erpRequest.AccountNumber ?? erpRequest.CustomerEmail}, " +
                $"DateFrom: {erpRequest.DateFrom:yyyy-MM-dd}, DateTo: {erpRequest.DateTo:yyyy-MM-dd}):\n {func}"
            );

            func.Invoke(dest);

            var row = func.GetTable("ES_SAVINGS").FirstOrDefault();

            if (row != null)
            {
                await _erpLogsService.InsertErpLogAsync(
                    ErpLogLevel.Debug,
                    ErpSyncLevel.Account,
                    $"GetAccountSavings - after BAPI call (Account: {erpRequest.AccountNumber ?? erpRequest.CustomerEmail}). Click view to see details.",
                    $"GetAccountSavings - after BAPI call: Discount = {row["DISCOUNT"].GetDecimal()}"
                );
                return row["DISCOUNT"].GetDecimal();
            }

            await _erpLogsService.InsertErpLogAsync(
                ErpLogLevel.Debug,
                ErpSyncLevel.Account,
                $"GetAccountSavings - Discount not found (Account: {erpRequest.AccountNumber ?? erpRequest.CustomerEmail}). Click view to see details.",
                $"GetAccountSavings - after BAPI call: Discount value not found. Using 0 as default."
            );
            return 0;
        }
        catch (Exception ex)
        {
            await _erpLogsService.InsertErpLogAsync(
                ErpLogLevel.Error,
                ErpSyncLevel.Account,
                $"GetAccountSavings error (Account: {erpRequest.AccountNumber ?? erpRequest.CustomerEmail}). Click view to see details.",
                $"GetAccountSavings error: \nException message: {ex.Message}\nStack trace: {ex.StackTrace}"
            );
            return null;
        }
    }

    #endregion
}
