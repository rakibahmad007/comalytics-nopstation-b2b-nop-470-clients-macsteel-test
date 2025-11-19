using System;
using System.Collections.Generic;
using System.Text;

namespace Nop.Plugin.Misc.ErpWebhook.Models.ErpPricing
{
    public class ErpPriceGroupPricingModel
    {
        public string PriceGroupCode { get; set; }
        public string ItemNo { get; set; }
        public decimal? SellingPrice { get; set; }
        // Other fields TBD
        public decimal? PromoPrice { get; set; }
        public decimal? ListPrice { get; set; }
        public decimal? RetailPrice { get; set; }
        public decimal? DiscountPerc { get; set; }
        public string PricingNotes { get; set; }
        public Dictionary<string, decimal?> Prices { get; set; }
    }
}
