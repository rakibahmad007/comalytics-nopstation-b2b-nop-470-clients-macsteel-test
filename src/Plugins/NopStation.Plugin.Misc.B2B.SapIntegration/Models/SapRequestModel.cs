using Newtonsoft.Json;

namespace NopStation.Plugin.Misc.B2B.SapIntegration.Models;

public class ErpRequestArgs
{
    public DateTime? LastChangeDate { get; set; }
    public string Customer { get; set; }
    public int PageNumber { get; set; }
    public int RowsPerPage { get; set; }
}

public class SapRequestModel
{
    public string Action { get; set; }
    public string Name { get; set; }

    [JsonProperty(PropertyName = "operator")]
    public string Operator { get; set; }
    public string OperatorPassword { get; set; }
    public string Company { get; set; }
    public string CompanyPassword { get; set; }
    public string BusinessObject { get; set; }
    public string Method { get; set; }
    public string XmlIn { get; set; }
    public string XmlParameters { get; set; }
}