namespace NopStation.Plugin.B2B.B2BB2CFeatures.Model;

public class B2BProductBoxStockInfoModel
{
    public int ProductId { get; set; }

    public string StockAvailability { get; set; }

    public bool DisplayBackInStockSubscription { get; set; }

    public string UOM { get; set; }

    public int CartQuantity { get; set; }

    public string CartQuantityInfoText { get; set; }
}
