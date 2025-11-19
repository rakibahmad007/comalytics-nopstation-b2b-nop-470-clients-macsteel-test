using System.Xml.Serialization;

namespace NopStation.Plugin.Misc.B2B.SapIntegration.Models;
public class ErpOrderPlaceResponesModel
{
    public string Data { get; set; }
    public bool Success { get; set; }

}
[XmlRoot(ElementName = "SalesOrders")]
public class SalesOrders
{
    [XmlAttribute(AttributeName = "Language")]
    public string Language { get; set; }

    [XmlAttribute(AttributeName = "Language2")]
    public string Language2 { get; set; }

    [XmlAttribute(AttributeName = "CssStyle")]
    public string CssStyle { get; set; }

    [XmlAttribute(AttributeName = "DecFormat")]
    public string DecFormat { get; set; }

    [XmlAttribute(AttributeName = "DateFormat")]
    public string DateFormat { get; set; }

    [XmlAttribute(AttributeName = "Role")]
    public string Role { get; set; }

    [XmlAttribute(AttributeName = "Version")]
    public string Version { get; set; }

    [XmlAttribute(AttributeName = "OperatorPrimaryRole")]
    public string OperatorPrimaryRole { get; set; }

    [XmlElement(ElementName = "Order")]
    public Order Order { get; set; }

}

public class Order
{
    [XmlElement(ElementName = "CustomerPoNumber")]
    public string CustomerPoNumber { get; set; }

    [XmlElement(ElementName = "SalesOrder")]
    public string SalesOrder { get; set; }

    [XmlElement(ElementName = "OrderActionType")]
    public string OrderActionType { get; set; }
}
