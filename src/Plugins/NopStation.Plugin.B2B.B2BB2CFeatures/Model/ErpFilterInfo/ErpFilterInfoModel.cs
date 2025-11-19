using System.Collections.Generic;

namespace NopStation.Plugin.B2B.B2BB2CFeatures.Model.ErpfilterInfo.ErpFilterInfoModel
{
    public class ErpFilterInfoModel
    {
        public ErpFilterInfoModel()
        {
            PreFilterFacetSpecIds = new List<int>();
        }

        public bool IsErpAccount { get; set; }

        public int ErpAccountId { get; set; }

        public int ErpSalesOrgId { get; set; }

        public bool UsePriceGroupPricing { get; set; }

        public int PriceGroupCodeId { get; set; }

        public IList<int> PreFilterFacetSpecIds { get; set; }

        public bool ErpFilterFacetReturnNoProduct { get; set; }
    }
}
