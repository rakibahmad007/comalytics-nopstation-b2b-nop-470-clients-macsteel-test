using System.Xml.Serialization;

namespace NopStation.Plugin.Misc.B2B.SapIntegration.Models;
public class CustomerItem
{
    [XmlElement("key")]
    public CustomerKey Key { get; set; }
}
