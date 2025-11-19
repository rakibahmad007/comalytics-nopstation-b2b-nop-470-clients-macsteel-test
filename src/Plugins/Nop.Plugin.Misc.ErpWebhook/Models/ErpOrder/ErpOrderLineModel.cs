using System;

namespace Nop.Plugin.Misc.ErpWebhook.Models.ErpOrder
{
    public class ErpOrderLineModel
    {
        public string ERPLineNumber { get; set; }
        public string Sku { get; set; }
        public int Quantity { get; set; }
        public decimal? UnitPriceExcl { get; set; }
        public decimal? UnitPriceIncl { get; set; }
        public decimal? DiscountIncl { get; set; }
        public decimal? DiscountExcl { get; set; }
        public decimal? LineTotalExcl { get; set; }
        public decimal? LineTotalIncl { get; set; }
        public string Description { get; set; }
        public string ERPOrderLineStatus { get; set; } //[Values: "Ok", "Rejected"]
        public string UOM { get; set; }
        public decimal? Weight { get; set; }
        public DateTime? DateRequired { get; set; }
        public DateTime? DateExpected { get; set; }
        public string DeliveryMethod { get; set; }
        public string WarehouseCode { get; set; }
        public DateTime? DeliveryDate { get; set; }
        public string SpecInstruct { get; set; }
    }
}
