using Newtonsoft.Json;

namespace HubClient;

[JsonObject(MemberSerialization.OptIn)]
public class ERPRequest
{
    [JsonProperty]
    public string messageId { get; set;}
    [JsonProperty]
    public string AccNo { get; set; }
    [JsonProperty]
    public string OrderNumber { get; set; }
    [JsonProperty]
    public string start { get; set; }
    [JsonProperty]
    public int limit { get; set; }
    [JsonProperty]
    public DateTime? dateFrom { get; set; }
    [JsonProperty]
    public string location { get; set; }

    [JsonProperty]
    public bool SkipTransform { get; set; }

    public ERPRequest() { }

    public string ToJsonString()
    {
        return JsonConvert.SerializeObject(this);
    }
    public static ERPRequest Deserialize(string jsonString)
    {
        return (ERPRequest)JsonConvert.DeserializeObject<ERPRequest>(jsonString);
    }

}

public class ERPGetPricingRequest : ERPRequest
{
    [JsonProperty]
    public string itemNos { get; set; }
    [JsonProperty]
    public string priceGroupCode { get; set; }
    [JsonProperty]
    public string salesCode { get; set; }
}

public class ERPGetShippingRequest : ERPRequest
{
    [JsonProperty]
    public string salesOrg { get; set; }
    [JsonProperty]
    public string distance { get; set; }
    [JsonProperty]
    public string weight { get; set; }
}


public class ERPGetProductsRequest : ERPRequest
{
    [JsonProperty]
    public string itemNos { get; set; }
}
public class ERPGetDeliveryDatesRequest : ERPRequest
{
    [JsonProperty]
    public string AreaCode { get; set; }

    [JsonProperty]
    public string Plant { get; set; }
}
public class ERPGetProductImageRequest : ERPRequest
{
    [JsonProperty]
    public string ItemNo { get; set; }
}
public class ERPGetStockRequest : ERPRequest
{
    [JsonProperty]
    public string itemNos { get; set; }
}
public class ERPGetInvoicesRequest : ERPRequest
{
    [JsonProperty]
    public DateTime? dateTo { get; set; }
    [JsonProperty]
    public string documentNumber { get; set; }
}
public class ERPGetOrdersRequest : ERPRequest
{
    [JsonProperty]
    public DateTime dateTo { get; set; }
		[JsonProperty]
		public bool SkipTransform { get; set; }
}
public class ERPGetDocumentRequest : ERPRequest
{
    [JsonProperty]
    public string year { get; set; }
    [JsonProperty]
    public string period { get; set; }
    [JsonProperty]
    public string glitem { get; set; }
    [JsonProperty]
    public string document { get; set; }
}
public class ERPGetPODDocumentRequest : ERPRequest
{
    [JsonProperty]
    public string document { get; set; }
}
public class ERPPlaceOrderRequest : ERPRequest
{
   // [JsonProperty]
    //public FCAccSalesOrderHeader data { get; set; }

    public new string ToJsonString()
    {
        return JsonConvert.SerializeObject(this);
    }
    public static new ERPPlaceOrderRequest Deserialize(string jsonString)
    {
        return JsonConvert.DeserializeObject<ERPPlaceOrderRequest>(jsonString);
    }
}

public class ERPAccountSavingsRequest : ERPRequest
{
	[JsonProperty]
	public DateTime? dateto { get; set; }

	[JsonProperty]
	public string email { get; set; }
}

public class ERPAccountStatementRequest : ERPRequest
{
	[JsonProperty]
	public DateTime? dateTo { get; set; }

	[JsonProperty]
	public bool loadListOnly { get; set; }
}

public class ERPChangeConfigParameter : ERPRequest
{
    [JsonProperty]
    public string parameterName { get; set; }
    [JsonProperty]
    public string parameterValue { get; set; }

    public new string ToJsonString()
    {
        return JsonConvert.SerializeObject(this);
    }
    public static new ERPChangeConfigParameter Deserialize(string jsonString)
    {
        return JsonConvert.DeserializeObject<ERPChangeConfigParameter>(jsonString);
    }
}
