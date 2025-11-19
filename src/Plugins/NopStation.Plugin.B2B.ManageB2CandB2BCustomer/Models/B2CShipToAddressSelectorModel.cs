using System.Collections.Generic;
using Nop.Web.Framework.Models;

namespace NopStation.Plugin.B2B.ManageB2CandB2BCustomer.Models;

public record B2CShipToAddressSelectorModel : BaseNopEntityModel
{
    public B2CShipToAddressSelectorModel()
    {
        AvailableShipToAddresses = new List<B2CShipToAddressModel>();
    }
    public IList<B2CShipToAddressModel> AvailableShipToAddresses { get; set; }
    public int ErpNopUserId { get; set; }
    public bool IsSalesOrgDifferent { get; set; }
    public int NextDefaultErpShipToAddressId { get; set; }
    public int SelectedErpShipToAddressId { get; set; }
    public int DefaultAddressChanged { get; internal set; }
}
