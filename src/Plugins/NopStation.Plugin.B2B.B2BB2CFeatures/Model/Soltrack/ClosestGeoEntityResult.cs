using System.Xml.Serialization;

namespace NopStation.Plugin.B2B.B2BB2CFeatures.Model.Soltrack;

[XmlRoot(ElementName = "ClosestGeoEntityResult", Namespace = "http://www.ituran.com/ituranWebService3")]
public class ClosestGeoEntityResult
{
    [XmlElement]
    public int ReturnErrorCode { get; set; }
    [XmlElement]
    public string ReturnErrorDescription { get; set; }
    [XmlElement]
    public int InsideDistributionAreaEntity { get; set; }
    [XmlElement]
    public int DistributionAreaEntityId { get; set; }
    [XmlElement]
    public string DistributionAreaEntityName { get; set; }            
    [XmlElement]
    public int DistributionAreaType { get; set; }         
    [XmlElement]
    public int InsideNoGoAreaEntity { get; set; }           
    [XmlElement]
    public int NoGoAreaEntityId { get; set; }        
    [XmlElement]
    public int NoGoAreaType { get; set; }            
    [XmlElement]
    public int InsideExpressStoreAreaEntity { get; set; }           
    [XmlElement]
    public int ExpressStoreAreaEntityId { get; set; }
    [XmlElement]
    public string ExpressStoreAreaEntityName { get; set; }
    [XmlElement]
    public int ExpressStoreAreaType { get; set; }            
    [XmlElement]
    public int InsideMainHub { get; set; }          
    [XmlElement]
    public int MainHubAreaEntityId { get; set; }            
    [XmlElement]
    public int MainHubAreaType { get; set; }           
    [XmlElement]
    public int GoRoutsEntityId { get; set; }           
    [XmlElement]
    public int GoRoutsEntityType { get; set; }            
    [XmlElement]
    public int BranchAreaEntityId { get; set; }            
    [XmlElement]
    public string BranchAreaEntityName { get; set; }            
    [XmlElement]
    public int BranchAreaEntityType { get; set; }
    [XmlElement] 
    public double DistanceFromBranchArea { get; set; }
}
