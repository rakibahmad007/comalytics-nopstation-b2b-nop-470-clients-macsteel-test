using System.Runtime.CompilerServices;
using NopStation.Plugin.B2B.ERPIntegrationCore.Enums;
using NopStation.Plugin.B2B.ERPIntegrationCore.Model;
using NopStation.Plugin.B2B.ERPIntegrationCore.Services;
using SAP.Middleware.Connector;

namespace NopStation.Plugin.Misc.B2B.SapIntegration.Services;

public class B2BShippingService : IB2BShippingService
{
    #region Fields

    private readonly IErpLogsService _erpLogsService;
    private readonly SapIntegrationSettings _sapIntegrationSettings;

    #endregion

    #region Ctor

    public B2BShippingService(IErpLogsService erpLogsService, 
        SapIntegrationSettings sapIntegrationSettings)
    {
        _erpLogsService = erpLogsService;
        _sapIntegrationSettings = sapIntegrationSettings;
    }

    #endregion

    #region Utils

    private async Task<double> ConvertIntoDecimalAsync(string distance, [CallerMemberName] string caller = null)
    {
        try
        {
            if (string.IsNullOrEmpty(distance))
                return 0;
            distance = distance.Replace(",", "");
            _ = double.TryParse(distance, out var val);
            return val;
        }
        catch (Exception ex)
        {
            await _erpLogsService.InsertErpLogAsync(
                ErpLogLevel.Error,
                ErpSyncLevel.Order,
                $"Error during executing request from {caller}: {ex.Message}",
                ex.StackTrace);
            return 0;
        }
    }

    #endregion

    #region Methods

    public async Task<ErpResponseModel> GetShippingRateAsync(ErpGetRequestModel erpRequest)
    {
        var erpResponseData = new ErpResponseModel();
        var responseContent = string.Empty;

        try
        {
            if (erpRequest == null)
            {
                erpResponseData.IsError = false;
                erpResponseData.ErrorShortMessage = "Request body content no data";
                return erpResponseData;
            }

            #region SAP

            try
            {
                if (!RfcDestinationManager.IsDestinationConfigurationRegistered())
                    RfcDestinationManager.RegisterDestinationConfiguration(new SapConfig(_sapIntegrationSettings));

                var dest = RfcDestinationManager.GetDestination("HUBCLIENT");
                var repo = dest.Repository;
                var func = repo.CreateFunction("ZEC_TRP_CHARGES");
                func.SetValue("VKORG", erpRequest.Location);
                func.SetValue("DISTANCE", (await ConvertIntoDecimalAsync(erpRequest.Distance)).ToString("#.##"));
                func.SetValue("WEIGHT", (await ConvertIntoDecimalAsync(erpRequest.Weight)).ToString("#.##"));

                await _erpLogsService.InsertErpLogAsync(
                    ErpLogLevel.Debug,
                    ErpSyncLevel.Order,
                    $"GetShippingRate before BAPI call (Location: {erpRequest.Location}). Click view to see details.",
                    $"GetShippingRate before BAPI call (Location = {erpRequest.Location}, Distance = {erpRequest.Distance}, Weight = {erpRequest.Weight}):\n\n{func}"
                );

                func.Invoke(dest);

                erpResponseData.ShippingRate = func.GetValue("CURRENT").ToString();
                erpResponseData.ShippingRatePerTo = func.GetValue("CURRENT_RATE_PER_TO").ToString();
                erpResponseData.Next = " ";

                await _erpLogsService.InsertErpLogAsync(
                    ErpLogLevel.Debug,
                    ErpSyncLevel.Order,
                    $"GetShippingRate after BAPI call (Location: {erpRequest.Location}). Click view to see details.",
                    $"GetShippingRate after BAPI call - Response: Shipping Rate = {erpResponseData.ShippingRate}, Shipping Rate PerTo = {erpResponseData.ShippingRatePerTo}\n\n Payload: {func}"
                );
            }
            catch (Exception ex)
            {
                await _erpLogsService.InsertErpLogAsync(
                    ErpLogLevel.Error,
                    ErpSyncLevel.Order,
                    $"GetShippingRate error (Location: {erpRequest.Location}). Click view to see details.",
                    $"GetShippingRate error: \nException message: {ex.Message}\nStack trace: {ex.StackTrace}"
                );
            }

            return erpResponseData;

            #endregion

        }
        catch (Exception ex)
        {
            erpResponseData.IsError = true;
            erpResponseData.ErrorShortMessage = ex.Message;
            erpResponseData.ErrorFullMessage = !string.IsNullOrEmpty(responseContent) ? responseContent : ex.StackTrace;
        }

        return erpResponseData;
    }

    #endregion
}