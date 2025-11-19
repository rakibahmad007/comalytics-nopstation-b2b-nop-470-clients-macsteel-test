using System.Collections.Generic;
using NopStation.Plugin.B2B.B2BB2CFeatures.Model.Checkout;
using NopStation.Plugin.B2B.ERPIntegrationCore.Domain;

namespace NopStation.Plugin.B2B.B2BB2CFeatures.Model;

public class ErpShipToAddressListModel
{
    public ErpShipToAddressListModel()
    {
        ErpNopUser = new ErpNopUser();
        ErpShipToAddressList = new List<ErpShipToAddressModel>();
    }
    public IList<ErpShipToAddressModel> ErpShipToAddressList { get; set; }
    public ErpNopUser ErpNopUser { get; set; }
}
