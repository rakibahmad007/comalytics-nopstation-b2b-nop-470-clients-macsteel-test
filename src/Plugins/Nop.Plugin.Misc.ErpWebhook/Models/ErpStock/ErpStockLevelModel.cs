using System;

namespace Nop.Plugin.Misc.ErpWebhook.Models.ErpStock
{
    public class ErpStockLevelModel
    {
        public string ItemNo { get; set; }
        public string Sku { get; set; }
        public string SalesOrganisationCode { get; set; }
        public string WarehouseCode { get; set; }
        public decimal SellingPrice { get; set; }
        public decimal PromoPrice { get; set; }
        public decimal ContractPrice { get; set; }
        public decimal InStockforLocNo { get; set; }
        public int TotalOnHand { get; set; }
        public string UOM { get; set; } // Macsteel only
        public string StockNotes { get; set; }
        public decimal? Weight { get; set; }
        public DateTime? Timestamp { get; set; }
        public string Location { get; set; }
    }
}
