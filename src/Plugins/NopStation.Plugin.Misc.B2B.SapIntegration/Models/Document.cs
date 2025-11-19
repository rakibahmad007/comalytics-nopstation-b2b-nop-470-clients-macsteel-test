using System.Xml.Serialization;

namespace NopStation.Plugin.Misc.B2B.SapIntegration.Models;
public class Document
{
    [XmlElement(ElementName = "Status")]
    public object Status { get; set; }

    [XmlElement(ElementName = "DocumentGuid")]
    public object DocumentGuid { get; set; }

    [XmlElement(ElementName = "ErrorMessage")]
    public object ErrorMessage { get; set; }

    [XmlElement(ElementName = "StatusPreview")]
    public object StatusPreview { get; set; }

    [XmlElement(ElementName = "DocumentHex")]
    public string DocumentHex { get; set; }
}
