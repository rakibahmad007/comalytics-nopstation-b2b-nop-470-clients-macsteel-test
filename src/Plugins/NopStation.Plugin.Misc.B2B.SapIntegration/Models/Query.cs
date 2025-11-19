using System.Xml.Serialization;

namespace NopStation.Plugin.Misc.B2B.SapIntegration.Models;
public class Query
{
    [XmlElement(ElementName = "Document")]
    public Document Document { get; set; }
}
