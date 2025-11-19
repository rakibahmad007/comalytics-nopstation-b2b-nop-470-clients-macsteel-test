using System.Xml.Serialization;

namespace NopStation.Plugin.Misc.B2B.SapIntegration.Models;
public class CustomerKey
{
    [XmlElement("customer")]
    public string Customer { get; set; }
}