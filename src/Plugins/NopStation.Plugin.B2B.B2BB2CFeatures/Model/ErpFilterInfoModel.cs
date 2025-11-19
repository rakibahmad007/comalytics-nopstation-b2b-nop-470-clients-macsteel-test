using System.Collections.Generic;

namespace NopStation.Plugin.B2B.B2BB2CFeatures.Model;

public class ErpFilterInfoModel
{
    public ErpFilterInfoModel()
    {
        PreFilterFacetSpecIds = new List<int>();
        SpecialExcludeSpecIds = new List<int>();
    }

    public bool IsErpAccount { get; set; }

    public int ErpAccountId { get; set; }

    public int ErpSalesOrganisationId { get; set; }

    public bool UsePriceGroupPricing { get; set; }

    public int PriceGroupCodeId { get; set; }

    public IList<int> PreFilterFacetSpecIds { get; set; }
    public IList<int> SpecialExcludeSpecIds { get; set; }
    public IList<int> ExcludedProductIds { get; set; }

    public bool ErpFilterFacetReturnNoProduct { get; set; }
}
