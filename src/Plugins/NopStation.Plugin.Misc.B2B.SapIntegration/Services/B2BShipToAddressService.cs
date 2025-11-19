using NopStation.Plugin.B2B.ERPIntegrationCore.Enums;
using NopStation.Plugin.B2B.ERPIntegrationCore.Model;
using NopStation.Plugin.B2B.ERPIntegrationCore.Services;
using SAP.Middleware.Connector;

namespace NopStation.Plugin.Misc.B2B.SapIntegration.Services;

public class B2BShipToAddressService : IB2BShipToAddressService
{
    private readonly IErpLogsService _erpLogsService;
    private readonly SapIntegrationSettings _sapIntegrationSettings;

    public B2BShipToAddressService(IErpLogsService erpLogsService, SapIntegrationSettings sapIntegrationSettings)
    {
        _erpLogsService = erpLogsService;
        _sapIntegrationSettings = sapIntegrationSettings;
    }

    public async Task<ErpResponseData<IList<ErpShipToAddressDataModel>>> GetShipToAddressFromErpAsync(ErpGetRequestModel erpRequest)
    {
        var erpResponseData = new ErpResponseData<IList<ErpShipToAddressDataModel>>();
        var responseContent = string.Empty;

        try
        {
            if (erpRequest == null)
            {
                erpResponseData.ErpResponseModel.IsError = false;
                erpResponseData.ErpResponseModel.ErrorShortMessage = "Request body content no data";
                return erpResponseData;
            }

            #region SAP

            if (erpRequest.Limit == 0)
                erpRequest.Limit = 1000;
            try
            {
                if (!RfcDestinationManager.IsDestinationConfigurationRegistered())
                    RfcDestinationManager.RegisterDestinationConfiguration(new SapConfig(_sapIntegrationSettings));

                var dest = RfcDestinationManager.GetDestination("HUBCLIENT");
                var repo = dest.Repository;
                var func = repo.CreateFunction("ZEC_CUSTOMER_GETLIST");

                func.SetValue("IV_CUST_START", erpRequest.Start);
                func.SetValue("IV_ROWS", erpRequest.Limit);
                if (!string.IsNullOrWhiteSpace(erpRequest.AccountNumber))
                    func.SetValue("KUNNR", erpRequest.AccountNumber);

                await _erpLogsService.InsertErpLogAsync(
                    ErpLogLevel.Debug,
                    ErpSyncLevel.ShipToAddress,
                    $"GetShipToAddresses before BAPI call (Account: {erpRequest.AccountNumber}). Click view to see details.",
                    $"GetShipToAddresses before BAPI call: {func}"
                );

                func.Invoke(dest);

                await _erpLogsService.InsertErpLogAsync(
                    ErpLogLevel.Debug,
                    ErpSyncLevel.ShipToAddress,
                    $"GetShipToAddresses after BAPI call (Account: {erpRequest.AccountNumber}). Click view to see details.",
                    $"GetShipToAddresses after BAPI call: {func}"
                );

                var shipTos = func.GetTable("ET_SHIP_TO")
                    .Where(shipto => shipto["VKORG1"].GetString() == erpRequest.Location);
                var shipToAddresses = new List<ErpShipToAddressDataModel>();

                foreach (var shipTo in shipTos)
                {
                    var shipToAddress = new ErpShipToAddressDataModel
                    {
                        AccountNumber = shipTo["KUNNR"].GetString(),
                        ShipToCode = shipTo["KUNN2"].GetString(),
                        ShipToName = shipTo["NAME1"].GetString(),
                        Address1 = shipTo["HOUSE_NUM1"].GetString(),
                        Address2 = shipTo["STREET"].GetString(),
                        City = shipTo["CITY1"].GetString(),
                        StateProvince = shipTo["REGION"].GetString(),
                        Suburb = shipTo["CITY2"].GetString(),
                        Country = shipTo["LAND1"].GetString(),
                        ZipPostalCode = shipTo["POST_CODE1"].GetString(),
                        PhoneNumber = shipTo["TELF1"].GetString(),
                        EmailAddress = shipTo["SMTP_ADDR"].GetString(),
                        SalesOrgCode = shipTo["VKORG1"].GetString()
                    };

                    shipToAddresses.Add(shipToAddress);
                }

                if (shipToAddresses.Count != 0)
                {
                    erpResponseData.Data = shipToAddresses;
                    erpResponseData.ErpResponseModel.Next = func.GetString("EV_CUST_LAST");

                }
                else
                {
                    erpResponseData.Data = null;
                    erpResponseData.ErpResponseModel.Next = null;
                }
            }
            catch (Exception e)
            {
                await _erpLogsService.InsertErpLogAsync(
                    ErpLogLevel.Error,
                    ErpSyncLevel.ShipToAddress,
                    $"GetShipToAddresses error (Account: {erpRequest.AccountNumber}). Click view to see details.",
                    $"GetShipToAddresses error: \nException message: {e.Message}\nStack trace: {e.StackTrace}"
                );
                erpResponseData.ErpResponseModel.IsError = true;
                erpResponseData.ErpResponseModel.ErrorShortMessage = e.Message;
                return erpResponseData;
            }

            return erpResponseData;

            #endregion
        }
        catch (Exception ex)
        {
            erpResponseData.ErpResponseModel.IsError = true;
            erpResponseData.ErpResponseModel.ErrorShortMessage = ex.Message;
            erpResponseData.ErpResponseModel.ErrorFullMessage = !string.IsNullOrEmpty(responseContent)
                ? responseContent
                : ex.StackTrace;
        }

        return erpResponseData;
    }
}
