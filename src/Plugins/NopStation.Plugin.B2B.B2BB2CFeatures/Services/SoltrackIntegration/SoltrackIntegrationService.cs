using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Newtonsoft.Json;
using Nop.Core.Domain.Customers;
using Nop.Core.Http;
using NopStation.Plugin.B2B.B2BB2CFeatures.Model.Soltrack;
using NopStation.Plugin.B2B.ERPIntegrationCore.Enums;
using NopStation.Plugin.B2B.ERPIntegrationCore.Services;

namespace NopStation.Plugin.B2B.B2BB2CFeatures.Services.SoltrackIntegration;

public class SoltrackIntegrationService : ISoltrackIntegrationService
{
    #region Field

    private readonly B2BB2CFeaturesSettings _b2BB2CFeaturesSettings;
    private readonly IErpLogsService _erpLogsService;
    private readonly IHttpClientFactory _httpClientFactory;

    #endregion Field

    #region Ctor

    public SoltrackIntegrationService(
        B2BB2CFeaturesSettings b2BB2CFeaturesSettings,
        IErpLogsService erpLogsService,
        IHttpClientFactory httpClientFactory
    )
    {
        _b2BB2CFeaturesSettings = b2BB2CFeaturesSettings;
        _erpLogsService = erpLogsService;
        _httpClientFactory = httpClientFactory;
    }

    #endregion Ctor

    #region Methods

    public async Task<(
        bool isCustomerInExpressShopZone,
        bool isCustomerOnDeliveryRoute,
        ClosestGeoEntityResult
    )> GetSoltrackResponseAsync(Customer customer, string latitude, string longitude)
    {
        if (customer == null)
            return (false, false, null);

        var baseUrl = _b2BB2CFeaturesSettings.SoltrackBaseUrl.TrimEnd('/');
        var requestUrl =
            baseUrl
            + "/GetClosestGeoEntity?address="
            + "&username="
            + _b2BB2CFeaturesSettings.SoltrackUserName
            + "&password="
            + _b2BB2CFeaturesSettings.SoltrackPassword
            + "&lat="
            + latitude
            + "&lon="
            + longitude
            + "&DistributaionAreaType="
            + _b2BB2CFeaturesSettings.DistributionAreaType
            + "&nogoareatype="
            + _b2BB2CFeaturesSettings.NoGoAreaType
            + "&GoRoutsAreaType="
            + _b2BB2CFeaturesSettings.GoRoutsAreaType
            + "&BranchAreaType="
            + _b2BB2CFeaturesSettings.BranchAreaType
            + "&MainHubAreaType="
            + _b2BB2CFeaturesSettings.MainHubAreaType
            + "&ExpressStoreAreaType="
            + _b2BB2CFeaturesSettings.ExpressStoreAreaType;

        var httpClient = _httpClientFactory.CreateClient(NopHttpDefaults.DefaultHttpClient);
        httpClient.Timeout = TimeSpan.FromMilliseconds(
            _b2BB2CFeaturesSettings.SoltrackTimeOutMilliseconds
        );

        var task = Task.Run(() => httpClient.GetAsync(requestUrl));
        task.Wait();

        var isCustomerInExpressShopZone = false;
        var isCustomerOnDeliveryRoute = false;
        var response = task.Result;

        if (!response.IsSuccessStatusCode)
        {
            await _erpLogsService.InsertErpLogAsync(
                ErpLogLevel.Error,
                ErpSyncLevel.ShipToAddress,
                $"GetSoltrackResponse: Soltrack call was not successful for Customer: {customer.Email}. Click view to see details.",
                $"Soltrack request: {requestUrl}, Soltrack response {response}"
            );

            return (isCustomerInExpressShopZone, isCustomerOnDeliveryRoute, null);
        }

        var xml = response.Content.ReadAsStringAsync().Result;
        if (string.IsNullOrEmpty(xml))
        {
            await _erpLogsService.InsertErpLogAsync(
                ErpLogLevel.Error,
                ErpSyncLevel.ShipToAddress,
                "GetSoltrackResponse: The xml response from Soltrack is null or empty."
            );

            return (isCustomerInExpressShopZone, isCustomerOnDeliveryRoute, null);
        }

        var serializer = new XmlSerializer(typeof(ClosestGeoEntityResult));
        var result = new ClosestGeoEntityResult();
        var resultFormatted = string.Empty;

        using (var reader = new StringReader(xml))
        {
            result = (ClosestGeoEntityResult)serializer.Deserialize(reader);
            resultFormatted = JsonConvert.SerializeObject(result, Formatting.Indented);

            await _erpLogsService.InsertErpLogAsync(
                ErpLogLevel.Information,
                ErpSyncLevel.ShipToAddress,
                $"GetSoltrackResponse: Deserialized the response from Soltrack for Customer: {customer.Email}. Click view to see details.",
                $"Soltrack request: \n{requestUrl}\n\n" + 
                $"Soltrack response: \n{resultFormatted}"
            );
        }

        if (result == null)
        {
            await _erpLogsService.InsertErpLogAsync(
                ErpLogLevel.Error,
                ErpSyncLevel.ShipToAddress,
                "GetSoltrackResponse: The deserialized response from Soltrack is null."
            );

            return (isCustomerInExpressShopZone, isCustomerOnDeliveryRoute, null);
        }

        var isError = true;
        if (
            result.ReturnErrorCode == 20
            || result.ReturnErrorCode == 21
            || result.ReturnErrorCode == 22
            || result.ReturnErrorCode == 23
            || result.ReturnErrorCode == 200
        )
            isError = false;

        if (result.ReturnErrorCode == 23)
            isCustomerInExpressShopZone = true;
        else if (result.ReturnErrorCode == 200)
            isCustomerOnDeliveryRoute = true;

        if (!isError)
            return (isCustomerInExpressShopZone, isCustomerOnDeliveryRoute, result);

        return (isCustomerInExpressShopZone, isCustomerOnDeliveryRoute, null);
    }

    #endregion Methods
}
