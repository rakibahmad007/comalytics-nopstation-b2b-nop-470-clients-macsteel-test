using Newtonsoft.Json;

namespace NopStation.Plugin.B2B.B2BB2CFeatures.Model;

public class ErpShippingCostResponseModel
{

    [JsonProperty("data")]
    public ShippingCost Data { get; set; }

    public class ShippingCost
    {
        public decimal? ShippingRate { get; set; }
    }
}