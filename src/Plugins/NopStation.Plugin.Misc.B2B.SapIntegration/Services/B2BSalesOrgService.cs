using NopStation.Plugin.B2B.ERPIntegrationCore.Enums;
using NopStation.Plugin.B2B.ERPIntegrationCore.Model;
using NopStation.Plugin.B2B.ERPIntegrationCore.Services;
using SAP.Middleware.Connector;

namespace NopStation.Plugin.Misc.B2B.SapIntegration.Services;

public class B2BSalesOrgService : IB2BSalesOrgService
{
    #region Fields

    private readonly IErpLogsService _erpLogsService;
    private readonly SapIntegrationSettings _sapIntegrationSettings;

    #endregion

    #region Ctor

    public B2BSalesOrgService(IErpLogsService erpLogsService, 
        SapIntegrationSettings sapIntegrationSettings)
    {
        _erpLogsService = erpLogsService;
        _sapIntegrationSettings = sapIntegrationSettings;
    }

    #endregion

    #region Methods

    public async Task<ErpResponseData<IList<ErpAreaCodeResponseModel>>> GetAreaCodesFromErpAsync(ErpGetRequestModel erpRequest)
    {
        var erpResponseData = new ErpResponseData<IList<ErpAreaCodeResponseModel>>();

        if (erpRequest == null)
        {
            erpResponseData.ErpResponseModel.IsError = false;
            erpResponseData.ErpResponseModel.ErrorShortMessage = "Request body content has no data.";
            return erpResponseData;
        }

        try
        {
            if (erpRequest.Limit == 0)
                erpRequest.Limit = 1000;

            if (!RfcDestinationManager.IsDestinationConfigurationRegistered())
                RfcDestinationManager.RegisterDestinationConfiguration(new SapConfig(_sapIntegrationSettings));

            var dest = RfcDestinationManager.GetDestination("HUBCLIENT");
            var repo = dest.Repository;
            var func = repo.CreateFunction("ZEC_AREAS");

            await _erpLogsService.InsertErpLogAsync(
                ErpLogLevel.Debug,
                ErpSyncLevel.SalesOrg,
                $"GetAreaCodes before call to BAPI details (Start = {erpRequest.Start}, Limit = {erpRequest.Limit}). Click view to see details.",
                $"GetAreaCodes before call to BAPI: {func}"
            );

            func.Invoke(dest);

            await _erpLogsService.InsertErpLogAsync(
                ErpLogLevel.Debug,
                ErpSyncLevel.SalesOrg,
                $"GetAreaCodes after call to BAPI details (Start = {erpRequest.Start}, Limit = {erpRequest.Limit}). Click view to see details.",
                $"GetAreaCodes after call to BAPI: {func}"
            );

            var areas = func.GetTable("EV_AREAS");

            erpResponseData.Data = areas
                .Select(area => new ErpAreaCodeResponseModel
                {
                    Area = area["AREA"].GetString()
                })
                .ToList();
        }
        catch (Exception ex)
        {
            await _erpLogsService.InsertErpLogAsync(
                ErpLogLevel.Error,
                ErpSyncLevel.SalesOrg,
                $"GetAreaCodes error (Start = {erpRequest.Start}, Limit = {erpRequest.Limit}). Click view to see details.",
                $"GetAreaCodes error: \nException message: {ex.Message}\nStack trace: {ex.StackTrace}"
            );
            erpResponseData.ErpResponseModel.IsError = true;
            erpResponseData.ErpResponseModel.ErrorShortMessage = ex.Message;
            erpResponseData.ErpResponseModel.ErrorFullMessage = ex.StackTrace;
        }

        return erpResponseData;
    }

    #endregion
}
