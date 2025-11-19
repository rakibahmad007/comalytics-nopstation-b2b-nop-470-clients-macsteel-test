using Newtonsoft.Json.Linq;

namespace HubClient;

public class ERPResponse
{
    public bool isError { get; set; } = false;
    public string errorMessage { get; set; } = "";
    public string messageId { get; set; }
}

public class ERPResponse<TData> : ERPResponse
{
    public TData data { get; set; }
}

public class ERPListResponse<TData> : ERPResponse
{
    public List<TData> data { get; set; }
    public string next { get; set; }
}

class ERPHealthCheckResponse : ERPResponse<HealthCheckData>
{
}

class HealthCheckData
{
    public string TennantID { get; set; }
    public string ComanyName { get; set; }
    public int HasProds { get; set; }
    public int ERPServiceConnectConfirmed { get; set; }
    public int ERPConnectConfirmed { get; set; }
    public int HasError { get; set; }
    public string ErrorMessage { get; set; }
    public string ERPType { get; set; }
}

public class ERPGetAccountsResponse<TAccount> : ERPResponse
{
    public List<TAccount> data;
    public string next;
}

public class ERPGetShipToAddressesResponse : ERPResponse
{
    public JArray data;
    public string next;
}
public class ERPGetInvoicesResponse : ERPResponse
{
    public JArray data;
}
public class ERPPlaceOrderResponse : ERPResponse
{
    public string OrderNo;
    public bool isTemporary = false;
    public DateTime? QuoteExpiryDate;
}
public class ERPJSONEnrichProductsResponse : ERPResponse
{
    public JArray data;
    public string next;
}
public class ERPJSONPagedResponse : ERPResponse
{
    public JArray data;
    public string next;
}
public class ERPJSONResponse : ERPResponse
{
    public JToken data;
}
public class ERPGetProductImageResponse : ERPResponse
{
    public string data;
}
public class ERPGetShippingRateResponse : ERPResponse
{
    public JToken data;
    public string next;
}

public class ShippingRates
{
    public ShippingRates()
    {
        this.ShippingRate = decimal.Zero;
    }
    public decimal? ShippingRate { get; set; }
    public string ShippingMethodeName { get; set; }
    public string Decription { get; set; }
}
