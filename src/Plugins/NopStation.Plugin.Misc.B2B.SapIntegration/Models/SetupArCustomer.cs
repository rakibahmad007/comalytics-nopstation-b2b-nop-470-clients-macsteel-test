using System.Xml.Serialization;

namespace NopStation.Plugin.Misc.B2B.SapIntegration.Models;
[XmlRoot("setuparcustomer")]
public class SetupArCustomer
{
    [XmlAttribute("Language")]
    public string Language { get; set; }

    [XmlElement("item")]
    public CustomerItem Item { get; set; }

    [XmlElement("StatusOfItems")]
    public StatusOfItems Status { get; set; }
}
