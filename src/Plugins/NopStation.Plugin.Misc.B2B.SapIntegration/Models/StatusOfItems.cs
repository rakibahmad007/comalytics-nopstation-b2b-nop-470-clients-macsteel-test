using System.Xml.Serialization;

namespace NopStation.Plugin.Misc.B2B.SapIntegration.Models;
public class StatusOfItems
{
    [XmlElement("RecordsRead")]
    public int RecordsRead { get; set; }

    [XmlElement("RecordsInvalid")]
    public int RecordsInvalid { get; set; }
}
