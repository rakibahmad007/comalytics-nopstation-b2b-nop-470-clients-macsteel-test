using Nop.Web.Framework.Models;

namespace NopStation.Plugin.B2B.B2BB2CFeatures.Model.PDF;

public partial record OrderItemPdfModel : BaseNopEntityModel
{
    public string ProductName { get; set; }
    public string Sku { get; set; }
    public decimal Price { get; set; }
    public string PriceFormatted { get; set; }
    public int Quantity { get; set; }
    public decimal Total { get; set; }
    public string TotalFormatted { get; set; }
    public decimal Discount { get; set; }
    public string DiscountFormatted { get; set; }
    public decimal NetTotal { get; set; }
    public string NetTotalFormatted { get; set; }
    public string ProductDescription { get; set; }
    public string UnitOfMeasure { get; set; }
    public string Manufacturer { get; set; }
    public decimal TaxRate { get; set; }
    public decimal TaxAmount { get; set; }
    public string TaxAmountFormatted { get; set; }
    public string ErpOrderLineNotes { get; set; }
}
