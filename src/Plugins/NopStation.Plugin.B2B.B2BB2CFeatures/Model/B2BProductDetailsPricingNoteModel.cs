namespace NopStation.Plugin.B2B.B2BB2CFeatures.Model;
public class B2BProductDetailsPricingNoteModel
{
    public bool IsHidePricingNote { get; set; }
    public bool IsHideWeightInfo { get; set; }

    public bool HavePricingNote { get; set; }
    public string PricingNote { get; set; }

    public bool DisplayWeightInformation { get; set; }
    public decimal Weight { get; set; }
    public string WeightValue { get; set; }
}
